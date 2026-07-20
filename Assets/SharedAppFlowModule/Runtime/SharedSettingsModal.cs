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

        private CanvasGroup canvasGroup;
        private Vector2 shownPosition;
        private Coroutine transitionRoutine;
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
        }

        public void Open()
        {
            ShowOptionsMenu();
            SetOpen(true, false);
        }

        public void Close()
        {
            SetOpen(false, false);
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
            SetPage(settingsPage);
            RefreshSettingsControls();
        }

        public void ShowStats()
        {
            SetPage(statsPage);
        }

        public void ShowCredits()
        {
            SetPage(creditsPage);
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
                return;
            }

            transitionRoutine = StartCoroutine(Animate(open));
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
                shownPosition = panelRoot != null ? panelRoot.anchoredPosition : Vector2.zero;
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
            SetPageActive(optionsPage, activePage);
            SetPageActive(settingsPage, activePage);
            SetPageActive(statsPage, activePage);
            SetPageActive(creditsPage, activePage);
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
