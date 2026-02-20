using UnityEngine;

namespace ChristopherCreates.AndroidNativeAudio
{
	public class OnErrorListener : AndroidJavaProxy
	{
		public OnErrorListener() : base("android.media.MediaPlayer$OnErrorListener")
		{
		}

		bool onError(AndroidJavaObject mediaPlayer, int what, int extra)
		{
			Debug.LogError("ANAMusic MediaPlayer Error: what=" + what + " extra=" + extra);
			// Returning true means we handled the error and it shouldn't trigger onCompletion.
			return true;
		}
	}
}
