using UnityEngine;

namespace SharedCoreModule
{
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
            if (!vibrationEnabled)
            {
                return;
            }

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
