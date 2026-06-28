using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SharedSettingsModule
{
    public sealed class SharedSettingsModal : MonoBehaviour
    {
        public static readonly Vector2 AssetReferenceResolution = new(960f, 540f);

        private static readonly Color OverlayColor = new(0.5f, 0.5f, 0.5f, 0.5f);
        private static readonly Color FallbackPanelColor = new(0.09f, 0.1f, 0.12f, 0.96f);
        private static readonly Color TextColor = new(0.92f, 0.9f, 0.8f, 1f);
        private const float DefaultSpriteScale = 2f;
        private const float HiddenBottomOffset = 720f;
        private const float MaxPanelWidthRatio = 0.84f;
        private const float MaxPanelHeightRatio = 0.86f;

        [SerializeField] private Sprite panelSprite;
        [SerializeField] private Vector2 panelSize = Vector2.zero;
        [SerializeField] private float slideDuration = 0.28f;
        [SerializeField] private bool createOwnCanvas = true;

        private CanvasGroup canvasGroup;
        private Image overlayImage;
        private RectTransform virtualRoot;
        private RectTransform panelRect;
        private RectTransform titleRect;
        private RectTransform closeButtonRect;
        private Coroutine animationRoutine;
        private bool isOpen;
        private Vector2 resolvedPanelSize;
        private Vector2 shownPosition;
        private Vector2 hiddenPosition;

        public bool IsOpen => isOpen;

        public Vector2 PanelSize
        {
            get => panelSize;
            set
            {
                panelSize = value;
                ApplyPanelSize();
            }
        }

        public static SharedSettingsModal Show(Sprite panelSprite = null)
        {
            return Show(panelSprite, null);
        }

        public static SharedSettingsModal Show(Sprite panelSprite, Transform parent)
        {
            var modal = Create(panelSprite, parent);
            modal.Open();
            return modal;
        }

        public static SharedSettingsModal Create(Sprite panelSprite = null)
        {
            return Create(panelSprite, null);
        }

        public static SharedSettingsModal Create(Sprite panelSprite, Transform parent)
        {
            EnsureEventSystem();

            var root = parent == null
                ? new GameObject("Shared Settings Modal", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster))
                : new GameObject("Shared Settings Modal", typeof(RectTransform));
            root.SetActive(false);

            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var modal = root.AddComponent<SharedSettingsModal>();
            modal.panelSprite = panelSprite;
            modal.createOwnCanvas = parent == null;
            modal.Build();
            return modal;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }

        private void Awake()
        {
            if (panelRect != null)
            {
                return;
            }

            Build();
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (isOpen && EscapePressedThisFrame())
            {
                Close();
            }
        }

        public void Open()
        {
            if (isOpen && animationRoutine == null)
            {
                return;
            }

            gameObject.SetActive(true);
            PlayAnimation(true);
        }

        public void Close()
        {
            if (!isOpen && animationRoutine == null)
            {
                return;
            }

            PlayAnimation(false);
        }

        public void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        private void Build()
        {
            ClearChildren((RectTransform)transform);

            if (createOwnCanvas)
            {
                var canvas = GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = gameObject.AddComponent<Canvas>();
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 30000;
                canvas.pixelPerfect = true;

                var scaler = GetComponent<CanvasScaler>();
                if (scaler == null)
                {
                    scaler = gameObject.AddComponent<CanvasScaler>();
                }

                ApplyAssetCanvasScale(scaler);

                if (GetComponent<GraphicRaycaster>() == null)
                {
                    gameObject.AddComponent<GraphicRaycaster>();
                }

                if (GetComponent<SharedSettingsPixelCanvasScaler>() == null)
                {
                    gameObject.AddComponent<SharedSettingsPixelCanvasScaler>();
                }
            }

            canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            var rootRect = (RectTransform)transform;
            Stretch(rootRect);

            overlayImage = CreateImage("Dim Overlay", rootRect, WithAlpha(OverlayColor, 0f));
            Stretch(overlayImage.rectTransform);

            var overlayButton = overlayImage.gameObject.AddComponent<Button>();
            overlayButton.transition = Selectable.Transition.None;
            overlayButton.onClick.AddListener(Close);

            virtualRoot = CreateVirtualRoot(rootRect);
            panelRect = CreatePanel(virtualRoot);
            CreateTitle(panelRect);
            CreateCloseButton(panelRect);
            ApplyPanelSize();

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            overlayImage.raycastTarget = false;
        }

        private RectTransform CreatePanel(RectTransform parent)
        {
            var panelImage = CreateImage("Settings Panel", parent, panelSprite == null ? FallbackPanelColor : Color.white);
            panelImage.sprite = panelSprite;
            panelImage.preserveAspect = true;
            panelImage.type = HasBorder(panelSprite) ? Image.Type.Sliced : Image.Type.Simple;

            var rect = panelImage.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            return rect;
        }

        private void CreateTitle(RectTransform parent)
        {
            var title = CreateText("Title", parent, "Settings", 7, FontStyle.Bold, TextAnchor.MiddleCenter);
            titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -7f);
            titleRect.sizeDelta = new Vector2(70f, 10f);
        }

        private void CreateCloseButton(RectTransform parent)
        {
            var buttonImage = CreateImage("Close Button", parent, new Color(0.06f, 0.07f, 0.09f, 0.78f));
            closeButtonRect = buttonImage.rectTransform;
            closeButtonRect.anchorMin = new Vector2(1f, 1f);
            closeButtonRect.anchorMax = new Vector2(1f, 1f);
            closeButtonRect.pivot = new Vector2(1f, 1f);
            closeButtonRect.anchoredPosition = new Vector2(-5f, -5f);
            closeButtonRect.sizeDelta = new Vector2(10f, 10f);

            var button = buttonImage.gameObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.transition = Selectable.Transition.None;
            button.onClick.AddListener(Close);

            var label = CreateText("Text", buttonImage.rectTransform, "X", 6, FontStyle.Bold, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
        }

        private void ApplyPanelSize()
        {
            if (panelRect == null)
            {
                return;
            }

            resolvedPanelSize = ResolvePanelSize();
            panelRect.sizeDelta = resolvedPanelSize;
            shownPosition = Vector2.zero;
            hiddenPosition = new Vector2(0f, -resolvedPanelSize.y - HiddenBottomOffset);

            if (isOpen)
            {
                panelRect.anchoredPosition = shownPosition;
            }
            else
            {
                panelRect.anchoredPosition = hiddenPosition;
            }
        }

        private Vector2 ResolvePanelSize()
        {
            if (panelSize.x > 0f && panelSize.y > 0f)
            {
                return panelSize;
            }

            if (panelSprite != null)
            {
                return FitInsideReferenceResolution(panelSprite.rect.size * DefaultSpriteScale);
            }

            return FitInsideReferenceResolution(new Vector2(360f, 460f));
        }

        private static Vector2 FitInsideReferenceResolution(Vector2 size)
        {
            var maxSize = new Vector2(
                AssetReferenceResolution.x * MaxPanelWidthRatio,
                AssetReferenceResolution.y * MaxPanelHeightRatio
            );
            var scale = Mathf.Min(1f, maxSize.x / size.x, maxSize.y / size.y);
            return new Vector2(
                Mathf.Round(size.x * scale),
                Mathf.Round(size.y * scale)
            );
        }

        public static int CalculateAssetIntegerScale()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return 1;
            }

            var widthScale = Screen.width / AssetReferenceResolution.x;
            var heightScale = Screen.height / AssetReferenceResolution.y;
            return Mathf.Max(1, Mathf.FloorToInt(Mathf.Min(widthScale, heightScale)));
        }

        public static void ApplyAssetCanvasScale(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.scaleFactor = CalculateAssetIntegerScale();
            scaler.referencePixelsPerUnit = 100f;
        }

        private void PlayAnimation(bool open)
        {
            if (animationRoutine != null)
            {
                StopCoroutine(animationRoutine);
            }

            animationRoutine = StartCoroutine(Animate(open));
        }

        private IEnumerator Animate(bool open)
        {
            isOpen = open;
            overlayImage.raycastTarget = open;
            canvasGroup.blocksRaycasts = true;

            var elapsed = 0f;
            var duration = Mathf.Max(0.01f, slideDuration);
            var startPanel = panelRect.anchoredPosition;
            var endPanel = open ? shownPosition : hiddenPosition;
            var startAlpha = canvasGroup.alpha;
            var endAlpha = open ? 1f : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = EaseOutCubic(Mathf.Clamp01(elapsed / duration));
                panelRect.anchoredPosition = Vector2.LerpUnclamped(startPanel, endPanel, t);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                overlayImage.color = WithAlpha(OverlayColor, OverlayColor.a * canvasGroup.alpha);
                yield return null;
            }

            panelRect.anchoredPosition = endPanel;
            canvasGroup.alpha = endAlpha;
            overlayImage.color = WithAlpha(OverlayColor, OverlayColor.a * endAlpha);
            canvasGroup.blocksRaycasts = open;
            animationRoutine = null;

            if (!open)
            {
                gameObject.SetActive(false);
            }
        }

        private Text CreateText(string name, RectTransform parent, string value, int size, FontStyle style, TextAnchor alignment)
        {
            var text = new GameObject(name, typeof(RectTransform)).AddComponent<Text>();
            text.transform.SetParent(parent, false);
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = TextColor;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static Image CreateImage(string name, RectTransform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform)).AddComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            return image;
        }

        private static RectTransform CreateVirtualRoot(RectTransform parent)
        {
            var rect = new GameObject("Asset Reference Root", typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = AssetReferenceResolution;
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ClearChildren(RectTransform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyUiObject(parent.GetChild(i).gameObject);
            }
        }

        private static void DestroyUiObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                target.SetActive(false);
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private static bool HasBorder(Sprite sprite)
        {
            return sprite != null && sprite.border != Vector4.zero;
        }

        private static float EaseOutCubic(float t)
        {
            var inverse = 1f - t;
            return 1f - inverse * inverse * inverse;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static bool EscapePressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                return true;
            }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }
    }
}
