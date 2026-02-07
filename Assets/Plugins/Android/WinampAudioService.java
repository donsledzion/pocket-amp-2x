package com.softaware.winamp;

import android.app.Notification;
import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.Build;
import android.os.IBinder;
import android.media.session.MediaSession;
import android.media.MediaMetadata;
import android.media.session.PlaybackState;
import android.os.PowerManager;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

public class WinampAudioService extends Service {
    private static final String TAG = "WinampAudioService";
    private static final String CHANNEL_ID = "WinampAudioChannel";
    private static final int NOTIFICATION_ID = 1337;
    
    private MediaSession mediaSession;
    private NotificationManager notificationManager;
    private PowerManager.WakeLock wakeLock;

    @Override
    public void onCreate() {
        super.onCreate();
        Log.d(TAG, "Service onCreate");
        createNotificationChannel();
        setupMediaSession();
        setupWakeLock();
    }

    private void setupWakeLock() {
        PowerManager powerManager = (PowerManager) getSystemService(Context.POWER_SERVICE);
        if (powerManager != null) {
            wakeLock = powerManager.newWakeLock(PowerManager.PARTIAL_WAKE_LOCK, "Winamp:PlaybackWakeLock");
            wakeLock.acquire();
            Log.d(TAG, "WakeLock acquired");
        }
    }

    private void createNotificationChannel() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            NotificationChannel channel = new NotificationChannel(
                    CHANNEL_ID,
                    "Winamp Playback",
                    NotificationManager.IMPORTANCE_LOW
            );
            channel.setDescription("Shows controls for Winamp music playback");
            notificationManager = getSystemService(NotificationManager.class);
            if (notificationManager != null) {
                notificationManager.createNotificationChannel(channel);
            }
        }
    }

    private void setupMediaSession() {
        mediaSession = new MediaSession(this, "WinampSession");
        mediaSession.setCallback(new MediaSession.Callback() {
            @Override
            public void onPlay() {
                Log.d(TAG, "MediaSession: onPlay");
                UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePlay", "");
            }

            @Override
            public void onPause() {
                Log.d(TAG, "MediaSession: onPause");
                UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
            }

            @Override
            public void onSkipToNext() {
                Log.d(TAG, "MediaSession: onSkipToNext");
                UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativeNext", "");
            }

            @Override
            public void onSkipToPrevious() {
                Log.d(TAG, "MediaSession: onSkipToPrevious");
                UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePrev", "");
            }
        });
        
        mediaSession.setFlags(MediaSession.FLAG_HANDLES_MEDIA_BUTTONS | MediaSession.FLAG_HANDLES_TRANSPORT_CONTROLS);
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
            updateNotification(title, artist, isPlaying);
        } else if (ACTION_STOP_SERVICE.equals(action)) {
            Log.d(TAG, "Stopping service");
            stopForeground(true);
            stopSelf();
        } else if (ACTION_PLAY.equals(action)) {
            UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePlay", "");
        } else if (ACTION_PAUSE.equals(action)) {
            UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePause", "");
        } else if (ACTION_PREV.equals(action)) {
            UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativePrev", "");
        } else if (ACTION_NEXT.equals(action)) {
            UnityPlayer.UnitySendMessage("AudioPlayer", "OnNativeNext", "");
        }

        return START_NOT_STICKY;
    }

    private void updateNotification(String title, String artist, boolean isPlaying) {
        // Update Media Metadata
        MediaMetadata.Builder metadataBuilder = new MediaMetadata.Builder()
                .putString(MediaMetadata.METADATA_KEY_TITLE, title)
                .putString(MediaMetadata.METADATA_KEY_ARTIST, artist);
        mediaSession.setMetadata(metadataBuilder.build());

        // Update Playback State
        PlaybackState.Builder stateBuilder = new PlaybackState.Builder()
                .setActions(PlaybackState.ACTION_PLAY | PlaybackState.ACTION_PAUSE | 
                           PlaybackState.ACTION_PLAY_PAUSE |
                           PlaybackState.ACTION_SKIP_TO_NEXT | PlaybackState.ACTION_SKIP_TO_PREVIOUS);
        
        stateBuilder.setState(isPlaying ? PlaybackState.STATE_PLAYING : PlaybackState.STATE_PAUSED, 
                             PlaybackState.PLAYBACK_POSITION_UNKNOWN, 1.0f);
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
        Intent intent = new Intent(this, WinampAudioService.class);
        intent.setAction(action);
        return PendingIntent.getService(this, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT | PendingIntent.FLAG_IMMUTABLE);
    }

    @Override
    public IBinder onBind(Intent intent) {
        return null;
    }

    @Override
    public void onDestroy() {
        Log.d(TAG, "Service onDestroy");
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
