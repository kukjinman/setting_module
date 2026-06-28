using UnityEngine;
using UnityEngine.UI;

namespace SharedSettingsModule
{
    public sealed class SharedSettingsLauncher : MonoBehaviour
    {
        private static readonly Vector2 LegacyDefaultPanelSize = new(360f, 460f);

        [SerializeField] private Sprite panelSprite = null;
        [SerializeField] private bool openOnStart = true;
        [SerializeField] private bool createBlackBackground = true;
        [SerializeField] private Canvas targetCanvas = null;
        [SerializeField] private KeyCode toggleKey = KeyCode.Escape;
        [SerializeField] private Vector2 panelSize = Vector2.zero;

        private SharedSettingsModal modal;

        private void Start()
        {
            if (createBlackBackground)
            {
                EnsureBlackBackground(targetCanvas);
            }

            EnsureModal();

            if (openOnStart)
            {
                Open();
            }
        }

        private void Update()
        {
            if (ToggleKeyPressedThisFrame())
            {
                Toggle();
            }
        }

        public void Open()
        {
            EnsureModal().Open();
        }

        public void Close()
        {
            EnsureModal().Close();
        }

        public void Toggle()
        {
            EnsureModal().Toggle();
        }

        private SharedSettingsModal EnsureModal()
        {
            if (modal != null)
            {
                return modal;
            }

            modal = SharedSettingsModal.Create(panelSprite, targetCanvas != null ? targetCanvas.transform : null);
            modal.PanelSize = ResolvePanelSize();
            return modal;
        }

        private Vector2 ResolvePanelSize()
        {
            return panelSize == LegacyDefaultPanelSize ? Vector2.zero : panelSize;
        }

        private static void EnsureBlackBackground(Canvas targetCanvas)
        {
            if (targetCanvas != null)
            {
                EnsureBlackBackgroundInCanvas(targetCanvas);
                return;
            }

            if (GameObject.Find("Shared Settings Black Background") != null)
            {
                return;
            }

            var root = new GameObject("Shared Settings Black Background", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            var rootRect = (RectTransform)root.transform;
            Stretch(rootRect);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1000;
            canvas.pixelPerfect = true;

            var scaler = root.GetComponent<CanvasScaler>();
            SharedSettingsModal.ApplyAssetCanvasScale(scaler);
            root.AddComponent<SharedSettingsPixelCanvasScaler>();

            var background = new GameObject("Black Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(root.transform, false);

            var backgroundRect = (RectTransform)background.transform;
            Stretch(backgroundRect);

            var image = background.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }

        private static void EnsureBlackBackgroundInCanvas(Canvas targetCanvas)
        {
            if (targetCanvas.transform.Find("Black Background") != null)
            {
                return;
            }

            var background = new GameObject("Black Background", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(targetCanvas.transform, false);

            var backgroundRect = (RectTransform)background.transform;
            Stretch(backgroundRect);
            background.SetActive(true);
            background.transform.SetAsFirstSibling();

            var image = background.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private bool ToggleKeyPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (toggleKey == KeyCode.Escape)
            {
                var keyboard = UnityEngine.InputSystem.Keyboard.current;
                if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
                {
                    return true;
                }
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(toggleKey);
#else
            return false;
#endif
        }
    }

    public sealed class SharedSettingsPixelCanvasScaler : MonoBehaviour
    {
        private CanvasScaler scaler;
        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            ApplyIfNeeded(force: true);
        }

        private void Update()
        {
            ApplyIfNeeded(force: false);
        }

        private void ApplyIfNeeded(bool force)
        {
            if (!force && lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            {
                return;
            }

            SharedSettingsModal.ApplyAssetCanvasScale(scaler);
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
        }
    }
}
