using System.Collections;
using SharedCoreModule;
using UnityEngine;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SharedSettingsModal : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private GameObject optionsPage;
        [SerializeField] private GameObject settingsPage;
        [SerializeField] private GameObject statsPage;
        [SerializeField] private GameObject creditsPage;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button statsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button settingsBackButton;
        [SerializeField] private Button statsBackButton;
        [SerializeField] private Button creditsBackButton;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private float transitionDuration = 0.22f;
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -420f);
        [SerializeField] private Vector2 shownPosition = Vector2.zero;
        [Header("Backdrop")]
        [SerializeField] private bool blurBackdrop = true;
        [SerializeField, Range(2, 16)] private int blurDownsample = 8;

        private CanvasGroup canvasGroup;
        private RawImage blurredBackdrop;
        private Texture2D blurredBackdropTexture;
        private Coroutine transitionRoutine;
        private Coroutine pageTransitionRoutine;
        private bool isOpen;
        private bool initialized;

        public bool IsOpen => isOpen;

        public void Configure(
            RectTransform panel,
            GameObject menu,
            GameObject settings,
            GameObject stats,
            GameObject credits,
            Button openSettings,
            Button openStats,
            Button openCredits,
            Button close,
            Button settingsBack,
            Button statsBack,
            Button creditsBack,
            Slider masterSlider,
            Slider bgmSlider,
            Slider sfxSlider,
            Toggle hapticsToggle)
        {
            panelRoot = panel;
            optionsPage = menu;
            settingsPage = settings;
            statsPage = stats;
            creditsPage = credits;
            settingsButton = openSettings;
            statsButton = openStats;
            creditsButton = openCredits;
            closeButton = close;
            settingsBackButton = settingsBack;
            statsBackButton = statsBack;
            creditsBackButton = creditsBack;
            masterVolumeSlider = masterSlider;
            bgmVolumeSlider = bgmSlider;
            sfxVolumeSlider = sfxSlider;
            vibrationToggle = hapticsToggle;
            shownPosition = panel != null ? panel.anchoredPosition : Vector2.zero;
            initialized = false;
        }

        private void Awake()
        {
            Initialize();
            BindListeners();
        }

        private void OnDestroy()
        {
            UnbindListeners();
            ReleaseBackdropTexture();
        }

        public void Open()
        {
            ShowOptionsMenu();
            SetOpen(true, false);
        }

        public void Close()
        {
            SetOpen(false, true);
        }

        public void Toggle()
        {
            if (isOpen)
            {
                Close();
                return;
            }

            Open();
        }

        public void ShowOptionsMenu()
        {
            SetPage(optionsPage);
        }

        public void ShowSettings()
        {
            ShowPageWithSlide(settingsPage);
            RefreshSettingsControls();
        }

        public void ShowStats()
        {
            ShowPageWithSlide(statsPage);
        }

        public void ShowCredits()
        {
            ShowPageWithSlide(creditsPage);
        }

        public void SetOpen(bool open, bool instant)
        {
            Initialize();
            isOpen = open;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
                transitionRoutine = null;
            }

            if (instant || !gameObject.activeInHierarchy)
            {
                ApplyOpenState(open ? 1f : 0f);
                gameObject.SetActive(open);

                if (!open && panelRoot != null)
                {
                    panelRoot.anchoredPosition = shownPosition;
                }

                return;
            }

            if (open && blurBackdrop)
            {
                // Keep the modal invisible while the underlying screen is captured.
                ApplyOpenState(0f);
                transitionRoutine = StartCoroutine(CaptureBackdropAndAnimate());
            }
            else
            {
                transitionRoutine = StartCoroutine(Animate(open));
            }
        }

        private IEnumerator CaptureBackdropAndAnimate()
        {
            yield return new WaitForEndOfFrame();

            Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            if (screenshot != null)
            {
                int divisor = Mathf.Max(2, blurDownsample);
                int width = Mathf.Max(1, screenshot.width / divisor);
                int height = Mathf.Max(1, screenshot.height / divisor);
                RenderTexture temporary = RenderTexture.GetTemporary(
                    width,
                    height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Default);
                temporary.filterMode = FilterMode.Bilinear;
                Graphics.Blit(screenshot, temporary);

                RenderTexture previous = RenderTexture.active;
                RenderTexture.active = temporary;
                ReleaseBackdropTexture();
                blurredBackdropTexture = new Texture2D(width, height, TextureFormat.RGB24, false)
                {
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                blurredBackdropTexture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                blurredBackdropTexture.Apply(false, false);
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(temporary);
                Destroy(screenshot);

                EnsureBackdropImage();
                blurredBackdrop.texture = blurredBackdropTexture;
                blurredBackdrop.enabled = true;
            }

            yield return Animate(true);
        }

        private void EnsureBackdropImage()
        {
            if (blurredBackdrop != null)
            {
                return;
            }

            GameObject backdropObject = new GameObject(
                "Blurred Backdrop",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            RectTransform backdropTransform = backdropObject.GetComponent<RectTransform>();
            backdropTransform.SetParent(transform, false);
            backdropTransform.SetAsFirstSibling();
            backdropTransform.anchorMin = Vector2.zero;
            backdropTransform.anchorMax = Vector2.one;
            backdropTransform.anchoredPosition = Vector2.zero;
            backdropTransform.sizeDelta = Vector2.zero;

            blurredBackdrop = backdropObject.GetComponent<RawImage>();
            blurredBackdrop.raycastTarget = false;
        }

        private void ReleaseBackdropTexture()
        {
            if (blurredBackdropTexture == null)
            {
                return;
            }

            Destroy(blurredBackdropTexture);
            blurredBackdropTexture = null;
        }

        private void Initialize()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (panelRoot == null)
            {
                panelRoot = transform as RectTransform;
            }

            if (!initialized)
            {
                initialized = true;
            }
        }

        private void BindListeners()
        {
            AddButtonListener(settingsButton, ShowSettings);
            AddButtonListener(statsButton, ShowStats);
            AddButtonListener(creditsButton, ShowCredits);
            AddButtonListener(closeButton, Close);
            AddButtonListener(settingsBackButton, ShowOptionsMenu);
            AddButtonListener(statsBackButton, ShowOptionsMenu);
            AddButtonListener(creditsBackButton, ShowOptionsMenu);

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.AddListener(SetBgmVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
            }

            if (vibrationToggle != null)
            {
                vibrationToggle.onValueChanged.AddListener(SetVibrationEnabled);
            }
        }

        private void UnbindListeners()
        {
            RemoveButtonListener(settingsButton, ShowSettings);
            RemoveButtonListener(statsButton, ShowStats);
            RemoveButtonListener(creditsButton, ShowCredits);
            RemoveButtonListener(closeButton, Close);
            RemoveButtonListener(settingsBackButton, ShowOptionsMenu);
            RemoveButtonListener(statsBackButton, ShowOptionsMenu);
            RemoveButtonListener(creditsBackButton, ShowOptionsMenu);

            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            }

            if (bgmVolumeSlider != null)
            {
                bgmVolumeSlider.onValueChanged.RemoveListener(SetBgmVolume);
            }

            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
            }

            if (vibrationToggle != null)
            {
                vibrationToggle.onValueChanged.RemoveListener(SetVibrationEnabled);
            }
        }

        private void SetPage(GameObject activePage)
        {
            if (pageTransitionRoutine != null)
            {
                StopCoroutine(pageTransitionRoutine);
                pageTransitionRoutine = null;
            }

            SetPageActive(optionsPage, activePage);
            SetPageActive(settingsPage, activePage);
            SetPageActive(statsPage, activePage);
            SetPageActive(creditsPage, activePage);

            if (activePage != null)
            {
                RectTransform rect = activePage.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
                }

                CanvasGroup group = activePage.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 1f;
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }
            }
        }

        private void ShowPageWithSlide(GameObject activePage)
        {
            if (activePage == null)
            {
                return;
            }

            if (pageTransitionRoutine != null)
            {
                StopCoroutine(pageTransitionRoutine);
            }

            SetPageActive(optionsPage, activePage);
            SetPageActive(settingsPage, activePage);
            SetPageActive(statsPage, activePage);
            SetPageActive(creditsPage, activePage);
            pageTransitionRoutine = StartCoroutine(AnimatePageUp(activePage));
        }

        private IEnumerator AnimatePageUp(GameObject page)
        {
            RectTransform pageTransform = page.transform as RectTransform;
            CanvasGroup pageCanvasGroup = page.GetComponent<CanvasGroup>();
            if (pageCanvasGroup == null)
            {
                pageCanvasGroup = page.AddComponent<CanvasGroup>();
            }

            Vector2 targetPosition = Vector2.zero;
            Vector2 startPosition = targetPosition + hiddenOffset;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, transitionDuration);

            if (pageTransform != null)
            {
                pageTransform.anchoredPosition = startPosition;
            }

            pageCanvasGroup.alpha = 0f;
            pageCanvasGroup.interactable = false;
            pageCanvasGroup.blocksRaycasts = false;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);

                if (pageTransform != null)
                {
                    pageTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
                }

                pageCanvasGroup.alpha = t;
                yield return null;
            }

            if (pageTransform != null)
            {
                pageTransform.anchoredPosition = targetPosition;
            }

            pageCanvasGroup.alpha = 1f;
            pageCanvasGroup.interactable = true;
            pageCanvasGroup.blocksRaycasts = true;
            pageTransitionRoutine = null;
        }

        private static void SetPageActive(GameObject page, GameObject activePage)
        {
            if (page != null)
            {
                page.SetActive(page == activePage);
            }
        }

        private void RefreshSettingsControls()
        {
            SharedAudioManager audio = ResolveAudioManager();
            if (audio != null)
            {
                masterVolumeSlider?.SetValueWithoutNotify(audio.MasterVolume);
                bgmVolumeSlider?.SetValueWithoutNotify(audio.BgmVolume);
                sfxVolumeSlider?.SetValueWithoutNotify(audio.SfxVolume);
            }

            SharedHapticsManager haptics = ResolveHapticsManager();
            if (haptics != null)
            {
                vibrationToggle?.SetIsOnWithoutNotify(haptics.IsEnabled);
            }
        }

        private static void SetMasterVolume(float value)
        {
            ResolveAudioManager()?.SetMasterVolume(value);
        }

        private static void SetBgmVolume(float value)
        {
            ResolveAudioManager()?.SetBgmVolume(value);
        }

        private static void SetSfxVolume(float value)
        {
            ResolveAudioManager()?.SetSfxVolume(value);
        }

        private static void SetVibrationEnabled(bool enabled)
        {
            SharedHapticsManager haptics = ResolveHapticsManager();
            if (haptics == null)
            {
                return;
            }

            haptics.SetEnabled(enabled);
            if (enabled)
            {
                haptics.Play(SharedHapticType.Selection);
            }
        }

        private static SharedAudioManager ResolveAudioManager()
        {
            if (SharedCoreRoot.Instance != null)
            {
                return SharedCoreRoot.Instance.Audio;
            }

            return Object.FindFirstObjectByType<SharedAudioManager>();
        }

        private static SharedHapticsManager ResolveHapticsManager()
        {
            if (SharedCoreRoot.Instance != null)
            {
                return SharedCoreRoot.Instance.Haptics;
            }

            return Object.FindFirstObjectByType<SharedHapticsManager>();
        }

        private static void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void RemoveButtonListener(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(action);
            }
        }

        private IEnumerator Animate(bool open)
        {
            float from = canvasGroup != null ? canvasGroup.alpha : (open ? 0f : 1f);
            float to = open ? 1f : 0f;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, transitionDuration);

            ApplyOpenState(from);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                ApplyOpenState(Mathf.Lerp(from, to, t));
                yield return null;
            }

            ApplyOpenState(to);

            if (!open)
            {
                gameObject.SetActive(false);

                if (panelRoot != null)
                {
                    panelRoot.anchoredPosition = shownPosition;
                }
            }

            transitionRoutine = null;
        }

        private void ApplyOpenState(float amount)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = amount;
                canvasGroup.interactable = amount >= 0.99f;
                canvasGroup.blocksRaycasts = amount >= 0.99f;
            }

            if (panelRoot != null)
            {
                panelRoot.anchoredPosition = Vector2.Lerp(shownPosition + hiddenOffset, shownPosition, amount);
            }
        }
    }
}
