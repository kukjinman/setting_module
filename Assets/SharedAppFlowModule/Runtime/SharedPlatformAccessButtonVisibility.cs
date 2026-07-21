using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SharedAppFlowModule
{
    public sealed class SharedPlatformAccessButtonVisibility : MonoBehaviour
    {
        [SerializeField] private GameObject platformButton;
        [SerializeField, Min(0.1f)] private float visibleDuration = 5f;

        private float hideAtUnscaledTime;

        public void Configure(Button button, float duration = 5f)
        {
            platformButton = button != null ? button.gameObject : null;
            visibleDuration = Mathf.Max(0.1f, duration);
        }

        private void OnEnable()
        {
            ApplyPlatformLabel();
            ShowTemporarily();
        }

        private void Update()
        {
            if (WasPointerPressedThisFrame())
            {
                ShowTemporarily();
            }

            if (platformButton != null &&
                platformButton.activeSelf &&
                Time.unscaledTime >= hideAtUnscaledTime)
            {
                platformButton.SetActive(false);
            }
        }

        public void ShowTemporarily()
        {
            if (platformButton == null)
            {
                return;
            }

            platformButton.SetActive(true);
            hideAtUnscaledTime = Time.unscaledTime + visibleDuration;
        }

        private void ApplyPlatformLabel()
        {
            if (platformButton == null)
            {
                return;
            }

            Text label = platformButton.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                return;
            }

#if UNITY_IOS
            label.text = "GC";
#elif UNITY_ANDROID
            label.text = "PG";
#else
            label.text = "GC";
#endif
        }

        private static bool WasPointerPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                return true;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began)
            {
                return true;
            }

            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }
    }
}
