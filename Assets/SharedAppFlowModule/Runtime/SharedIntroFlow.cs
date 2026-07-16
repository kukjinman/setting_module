using System.Collections;
using SharedCoreModule;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace SharedAppFlowModule
{
    public sealed class SharedIntroFlow : MonoBehaviour
    {
        [SerializeField] private SharedAppFlowController controller;
        [SerializeField] private SharedAuthManager authManager;
        [SerializeField] private SharedIntroLogoBreakEffect logoBreakEffect;
        [SerializeField] private SharedAppScreenPanel introPanel;
        [SerializeField] private float slideUpSeconds = 1.2f;
        [SerializeField] private float holdAfterSlideSeconds = 0.4f;
        [SerializeField] private float postBreakHoldSeconds = 0.45f;
        [SerializeField] private bool skipOnPointerPress = true;
        [SerializeField] private bool routeAutomatically = true;

        private Coroutine routeRoutine;

        public void Configure(SharedAppFlowController flowController, SharedAuthManager auth)
        {
            controller = flowController;
            authManager = auth;
        }

        public void ConfigureLogoBreak(SharedIntroLogoBreakEffect breakEffect)
        {
            logoBreakEffect = breakEffect;
        }

        public void ConfigurePanel(SharedAppScreenPanel panel)
        {
            introPanel = panel;
        }

        public void ConfigureTiming(float slideSeconds, float holdSeconds)
        {
            slideUpSeconds = Mathf.Max(0f, slideSeconds);
            holdAfterSlideSeconds = Mathf.Max(0f, holdSeconds);
        }

        private void OnEnable()
        {
            if (!routeAutomatically)
            {
                return;
            }

            if (routeRoutine != null)
            {
                StopCoroutine(routeRoutine);
            }

            routeRoutine = StartCoroutine(RouteAfterIntro());
        }

        private void OnDisable()
        {
            if (routeRoutine != null)
            {
                StopCoroutine(routeRoutine);
                routeRoutine = null;
            }
        }

        private IEnumerator RouteAfterIntro()
        {
            if (introPanel == null)
            {
                introPanel = GetComponent<SharedAppScreenPanel>();
            }

            yield return WaitForSlideUp();

            if (routeRoutine == null)
            {
                yield break;
            }

            yield return WaitWithSkip(holdAfterSlideSeconds);

            if (routeRoutine == null)
            {
                yield break;
            }

            ResolveControllerAndAuth();

            if (logoBreakEffect != null)
            {
                yield return logoBreakEffect.Play(() => skipOnPointerPress && IsSkipInputPressed());

                if (logoBreakEffect.WasSkipped)
                {
                    RouteNow();
                    yield break;
                }
            }

            yield return WaitWithSkip(postBreakHoldSeconds);

            if (routeRoutine == null)
            {
                yield break;
            }

            if (controller != null)
            {
                RouteToSavedDestination();
            }

            routeRoutine = null;
        }

        private IEnumerator WaitForSlideUp()
        {
            if (introPanel == null)
            {
                yield return WaitWithSkip(slideUpSeconds);
                yield break;
            }

            while (!introPanel.HasCompletedShow)
            {
                if (skipOnPointerPress && IsSkipInputPressed())
                {
                    RouteNow();
                    yield break;
                }

                yield return null;
            }
        }

        private IEnumerator WaitWithSkip(float seconds)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(0f, seconds);

            while (elapsed < duration)
            {
                if (skipOnPointerPress && IsSkipInputPressed())
                {
                    RouteNow();
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void ResolveControllerAndAuth()
        {
            if (controller == null)
            {
                controller = GetComponentInParent<SharedAppFlowController>();
            }

            if (authManager == null && SharedCoreRoot.Instance != null)
            {
                authManager = SharedCoreRoot.Instance.Auth;
            }

            if (authManager == null)
            {
                authManager = UnityEngine.Object.FindFirstObjectByType<SharedAuthManager>();
            }
        }

        private void RouteNow()
        {
            ResolveControllerAndAuth();

            if (controller == null)
            {
                routeRoutine = null;
                return;
            }

            RouteToSavedDestination();

            routeRoutine = null;
        }

        private void RouteToSavedDestination()
        {
            bool hasSavedLogin = authManager != null
                ? authManager.HasSavedLogin()
                : SharedAuthManager.HasSavedLoginState();

            controller.ShowScreen(hasSavedLogin ? SharedAppScreenId.Home : SharedAppScreenId.Login);
        }

        private static bool IsSkipInputPressed()
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

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                return touch.phase == TouchPhase.Began;
            }

            return Input.GetMouseButtonDown(0) || Input.anyKeyDown;
#else
            return false;
#endif
        }
    }
}
