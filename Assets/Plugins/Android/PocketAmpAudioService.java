package com.softaware.pocketamp;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.media.AudioManager;
import android.os.Build;
import android.os.IBinder;
import android.media.session.MediaSession;
import android.media.MediaMetadata;
import android.media.session.PlaybackState;
import android.os.PowerManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

public class PocketAmpAudioService extends Service implements AudioManager.OnAudioFocusChangeListener {
    private static final String TAG = "PocketAmpAudioService";
    private static final String CHANNEL_ID = "PocketAmpAudioChannel";
    private static final int NOTIFICATION_ID = 1337;
    
    private MediaSession mediaSession;
    private NotificationManager notificationManager;
    private PowerManager.WakeLock wakeLock;
    private AudioManager audioManager;

    private boolean isPausedByFocusLoss = false;

    public interface RemoteControlListener {
        void onPlay();
        void onPause();
        void onNext();
        void onPrev();
        void onSeekTo(long pos);
    }

    private static RemoteControlListener listener;

    public static void setListener(RemoteControlListener listener) {
        PocketAmpAudioService.listener = listener;
    }

    @Override
    public void onCreate() {
        super.onCreate();
        Log.d(TAG, "Service onCreate");
        
        audioManager = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
        
        createNotificationChannel();
        setupMediaSession();
        setupWakeLock();
    }

    private void setupWakeLock() {
        PowerManager powerManager = (PowerManager) getSystemService(Context.POWER_SERVICE);
        if (powerManager != null) {
            wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "PocketAmp:PlaybackWakeLock");
            wakeLock.acquire(60 * 60 * 1000L); // 1 hour max - safety timeout
            Log.d(TAG, "WakeLock acquired (with 1h timeout)");
        }
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "PocketAmp Playback",
                    NotificationManager.IMPORTANCE_LOW
            );
            channel.setDescription("Shows controls for PocketAmp music playback");
            notificationManager = getSystemService(NotificationManager.class);
            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }
    }

    private void setupMediaSession() {
        mediaSession = new MediaSession(this, "PocketAmpSession");
        mediaSession.setCallback(new MediaSession.Callback() {
            @Override
            public void onPlay() {
                Log.d(TAG, "MediaSession: onPlay");
                if (requestAudioFocus()) {
                    if (listener != null) {
                        listener.onPlay();
                    } else {
                        UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePlay", "");
                    }
                }
            }

            @Override
            public void onPause() {
                Log.d(TAG, "MediaSession: onPause");
                abandonAudioFocus();
                if (listener != null) {
                    listener.onPause();
                } else {
                    UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
                }
            }

            @Override
            public void onStop() {
                Log.d(TAG, "MediaSession: onStop");
                abandonAudioFocus();
                super.onStop();
            }

            @Override
            public void onSkipToNext() {
                Log.d(TAG, "MediaSession: onSkipToNext");
                if (listener != null) {
                    listener.onNext();
                } else {
                    UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativeNext", "");
                }
            }

            @Override
            public void onSkipToPrevious() {
                Log.d(TAG, "MediaSession: onSkipToPrevious");
                if (listener != null) {
                    listener.onPrev();
                } else {
                    UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePrev", "");
                }
            }
            @Override
            public void onSeekTo(long pos) {
                Log.d(TAG, "MediaSession: onSeekTo " + pos);
                if (listener != null) {
                    listener.onSeekTo(pos);
                } else {
                    UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativeSeek", String.valueOf(pos));
                }
            }
        });
        
        mediaSession.setFlags(MediaSession.FLAG_HANDLES_MEDIA_BUTTONS | 
                             MediaSession.FLAG_HANDLES_TRANSPORT_CONTROLS);
        mediaSession.setActive(true);
    }

    public static final String ACTION_UPDATE_METADATA = "UPDATE_METADATA";
    public static final String ACTION_STOP_SERVICE = "STOP_SERVICE";
    public static final String ACTION_PLAY = "PLAY";
    public static final String ACTION_PAUSE = "PAUSE";
    public static final String ACTION_PREV = "PREV";
    public static final String ACTION_NEXT = "NEXT";

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent == null) return START_NOT_STICKY;

        String action = intent.getAction();
        Log.d(TAG, "onStartCommand action: " + action);
        
        if (ACTION_UPDATE_METADATA.equals(action)) {
            String title = intent.getStringExtra("title");
            String artist = intent.getStringExtra("artist");
            boolean isPlaying = intent.getBooleanExtra("isPlaying", false);
            long duration = intent.getLongExtra("duration", -1);
            long position = intent.getLongExtra("position", -1);
            
            // If we are playing, request focus to ensure we have it (e.g. initial start)
            if (isPlaying) {
                requestAudioFocus();
            }
            
            updateNotification(title, artist, duration, position, isPlaying);
        } else if (ACTION_STOP_SERVICE.equals(action)) {
            Log.d(TAG, "Stopping service");
            abandonAudioFocus();
            stopForeground(true);
            stopSelf();
        } else if (ACTION_PLAY.equals(action)) {
             if (requestAudioFocus()) {
                if (listener != null) listener.onPlay();
                else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePlay", "");
             }
        } else if (ACTION_PAUSE.equals(action)) {
            abandonAudioFocus();
            if (listener != null) listener.onPause();
            else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
        } else if (ACTION_PREV.equals(action)) {
            if (listener != null) listener.onPrev();
            else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePrev", "");
        } else if (ACTION_NEXT.equals(action)) {
            if (listener != null) listener.onNext();
            else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativeNext", "");
        }

        return START_NOT_STICKY;
    }

    private void updateNotification(String title, String artist, long duration, long position, boolean isPlaying) {
        // Update Media Metadata
        MediaMetadata.Builder metadataBuilder = new MediaMetadata.Builder()
                .putString(MediaMetadata.METADATA_KEY_TITLE, title)
                .putString(MediaMetadata.METADATA_KEY_ARTIST, artist);
        
        if (duration > 0) {
            metadataBuilder.putLong(MediaMetadata.METADATA_KEY_DURATION, duration);
        }
        
        mediaSession.setMetadata(metadataBuilder.build());

        // Update Playback State
        PlaybackState.Builder stateBuilder = new PlaybackState.Builder()
                .setActions(PlaybackState.ACTION_PLAY | PlaybackState.ACTION_PAUSE | 
                           PlaybackState.ACTION_PLAY_PAUSE |
                           PlaybackState.ACTION_SKIP_TO_NEXT | PlaybackState.ACTION_SKIP_TO_PREVIOUS |
                           PlaybackState.ACTION_SEEK_TO);
        
        long validPosition = position >= 0 ? position : PlaybackState.PLAYBACK_POSITION_UNKNOWN;
        
        stateBuilder.setState(isPlaying ? PlaybackState.STATE_PLAYING : PlaybackState.STATE_PAUSED, 
                             validPosition, 1.0f);
        mediaSession.setPlaybackState(stateBuilder.build());

        // Build Notification
        Intent notificationIntent = new Intent(this, UnityPlayer.currentActivity.getClass());
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, notificationIntent, 
                PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);

        Notification.Builder builder;
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            builder = new Notification.Builder(this, CHANNEL_ID);
        } else {
            builder = new Notification.Builder(this);
        }

        builder.setContentTitle(title)
                .setContentText(artist)
                .setSmallIcon(android.R.drawable.ic_media_play)
                .setContentIntent(pendingIntent)
                .setVisibility(Notification.VISIBILITY_PUBLIC)
                .setOngoing(isPlaying);

        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP) {
            builder.setStyle(new Notification.MediaStyle()
                    .setMediaSession(mediaSession.getSessionToken())
                    .setShowActionsInCompactView(0, 1, 2));

            // Add actions
            builder.addAction(new Notification.Action.Builder(
                    android.R.drawable.ic_media_previous, "Previous", 
                    createActionPendingIntent(ACTION_PREV)).build());
            
            int playPauseIcon = isPlaying ? android.R.drawable.ic_media_pause : android.R.drawable.ic_media_play;
            builder.addAction(new Notification.Action.Builder(
                    playPauseIcon, isPlaying ? "Pause" : "Play", 
                    createActionPendingIntent(isPlaying ? ACTION_PAUSE : ACTION_PLAY)).build());
            
            builder.addAction(new Notification.Action.Builder(
                    android.R.drawable.ic_media_next, "Next", 
                    createActionPendingIntent(ACTION_NEXT)).build());
        }

        Notification notification = builder.build();
        startForeground(NOTIFICATION_ID, notification);
    }

    private PendingIntent createActionPendingIntent(String action) {
        Intent intent = new Intent(this, PocketAmpAudioService.class);
        intent.setAction(action);
        return PendingIntent.getService(this, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
    }
    
    // Audio Focus Handling
    
    private boolean requestAudioFocus() {
        if (audioManager == null) return false;
        
        int result = audioManager.requestAudioFocus(this, AudioManager.STREAM_MUSIC, AudioManager.AUDIOFOCUS_GAIN);
        return result == AudioManager.AUDIOFOCUS_REQUEST_GRANTED;
    }
    
    private void abandonAudioFocus() {
        if (audioManager == null) return;
        audioManager.abandonAudioFocus(this);
    }

    @Override
    public void onAudioFocusChange(int focusChange) {
        Log.d(TAG, "onAudioFocusChange: " + focusChange);
        switch (focusChange) {
            case AudioManager.AUDIOFOCUS_GAIN:
                if (isPausedByFocusLoss) {
                    if (listener != null) listener.onPlay();
                    else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePlay", "");
                    isPausedByFocusLoss = false;
                }
                // Also restore volume if ducked (not implemented here but common pattern)
                break;
                
            case AudioManager.AUDIOFOCUS_LOSS:
                // Permanent loss (other app started playing)
                if (listener != null) listener.onPause();
                else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
                break;
                
            case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT:
                // Temporary loss (e.g. phone call)
                if (listener != null) listener.onPause();
                else UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
                isPausedByFocusLoss = true;
                break;
                
            case AudioManager.AUDIOFOCUS_LOSS_TRANSIENT_CAN_DUCK:
                // We could lower volume here, but for PocketAmp pausing might be better or just let it mix?
                // Standard behavior is to duck (lower volume). 
                // For simplicity and "PocketAmp" feel, let's keep playing but maybe implemented later.
                // For now, do nothing (keep playing) or pause if preferred.
                break;
        }
    }

    @Override
    public void onTaskRemoved(Intent rootIntent) {
        Log.d(TAG, "onTaskRemoved - cleaning up service");
        abandonAudioFocus();
        stopForeground(true);
        stopSelf();
        super.onTaskRemoved(rootIntent);
        
        // Kill process to prevent it from lingering in Unity's Idle Loop
        android.os.Process.killProcess(android.os.Process.myPid());
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onDestroy() {
        Log.d(TAG, "Service onDestroy");
        abandonAudioFocus();
        if (mediaSession != null) {
            mediaSession.setActive(false);
            mediaSession.release();
        }
        if (wakeLock != null && wakeLock.isHeld()) {
            wakeLock.release();
            Log.d(TAG, "WakeLock released");
        }
        super.onDestroy();
    }
}
