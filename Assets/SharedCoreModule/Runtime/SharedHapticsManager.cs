using System.Runtime.InteropServices;
using UnityEngine;

namespace SharedCoreModule
{
    public enum SharedHapticType
    {
        Selection,
        Light,
        Medium,
        Heavy,
        Success,
        Warning,
        Error
    }

    public sealed class SharedHapticsManager : MonoBehaviour
    {
        private const string VibrationEnabledKey = "shared_haptics_enabled";

        [SerializeField] private bool vibrationEnabled = true;

        public bool IsEnabled => vibrationEnabled;

        private void Awake()
        {
            vibrationEnabled = PlayerPrefs.GetInt(VibrationEnabledKey, vibrationEnabled ? 1 : 0) == 1;
        }

        public void SetEnabled(bool enabled, bool save = true)
        {
            vibrationEnabled = enabled;
            PlayerPrefs.SetInt(VibrationEnabledKey, enabled ? 1 : 0);

            if (save)
            {
                PlayerPrefs.Save();
            }
        }

        public void Vibrate()
        {
            Play(SharedHapticType.Heavy);
        }

        public void Play(SharedHapticType type)
        {
            if (!vibrationEnabled)
            {
                return;
            }

#if UNITY_IOS && !UNITY_EDITOR
            SharedHaptics_Play((int)type);
#elif UNITY_ANDROID && !UNITY_EDITOR
            PlayAndroid(type);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void PlayAndroid(SharedHapticType type)
        {
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                    {
                        using (AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow"))
                        using (AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView"))
                        {
                            decorView.Call<bool>("performHapticFeedback", GetAndroidHapticConstant(type));
                        }

                        activity.Dispose();
                    }));
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning($"Android haptic feedback failed: {exception.Message}");
            }
        }

        private static int GetAndroidHapticConstant(SharedHapticType type)
        {
            int sdkVersion;
            using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                sdkVersion = version.GetStatic<int>("SDK_INT");
            }

            switch (type)
            {
                case SharedHapticType.Selection:
                    return 4; // CLOCK_TICK
                case SharedHapticType.Light:
                    return 3; // KEYBOARD_TAP
                case SharedHapticType.Medium:
                    return 1; // VIRTUAL_KEY
                case SharedHapticType.Heavy:
                    return 0; // LONG_PRESS
                case SharedHapticType.Success:
                    return sdkVersion >= 30 ? 16 : 1; // CONFIRM / VIRTUAL_KEY
                case SharedHapticType.Warning:
                    return sdkVersion >= 23 ? 6 : 1; // CONTEXT_CLICK / VIRTUAL_KEY
                case SharedHapticType.Error:
                    return sdkVersion >= 30 ? 17 : 0; // REJECT / LONG_PRESS
                default:
                    return 1;
            }
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SharedHaptics_Play(int type);
#endif
    }
}
