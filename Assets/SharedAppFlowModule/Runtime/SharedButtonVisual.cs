using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    public enum SharedButtonVariant
    {
        Primary,
        Secondary,
        Navigation,
        Back,
        Destructive
    }

    [Serializable]
    public struct SharedButtonStyle
    {
        public Color BaseColor;
        public Color AccentColor;
        public Color TextColor;

        public SharedButtonStyle(Color baseColor, Color accentColor, Color textColor)
        {
            BaseColor = baseColor;
            AccentColor = accentColor;
            TextColor = textColor;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform), typeof(Image), typeof(Button))]
    public sealed class SharedButtonVisual : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISelectHandler,
        IDeselectHandler,
        ISubmitHandler
    {
        private const string HighlightName = "Shared Button Surface Highlight";
        private const string DepthRimName = "Shared Button Depth Rim";

        [SerializeField] private SharedButtonVariant variant = SharedButtonVariant.Secondary;
        [SerializeField] private SharedButtonStyle style;
        [SerializeField] private bool hasConfiguredStyle;
        [SerializeField, Range(1.02f, 1.035f)] private float hoverScale = 1.028f;
        [SerializeField, Range(0.96f, 0.975f)] private float pressedScale = 0.968f;
        [SerializeField, Range(3f, 5f)] private float pressedOffset = 4f;
        [SerializeField, Min(1f)] private float animationSpeed = 18f;
        [SerializeField] private Image surfaceHighlight;
        [SerializeField] private Image depthRim;

        private RectTransform rectTransform;
        private Image background;
        private Button button;
        private Vector2 baseAnchoredPosition;
        private Vector2 appliedOffset;
        private Vector3 baseScale;
        private bool pointerOver;
        private bool selected;
        private bool pressed;
        private Coroutine submitRoutine;

        public SharedButtonVariant Variant => variant;
        public SharedButtonStyle Style => style;

        private void Awake()
        {
            CacheComponents();
            CaptureBaseTransform();
            Configure(variant, hasConfiguredStyle ? style : GetDefaultStyle(variant));
        }

        private void OnEnable()
        {
            CacheComponents();
            CaptureBaseTransform();
        }

        private void OnDisable()
        {
            if (submitRoutine != null)
            {
                StopCoroutine(submitRoutine);
                submitRoutine = null;
            }

            pointerOver = false;
            selected = false;
            pressed = false;
            RestoreImmediately();
        }

        private void Update()
        {
            if (rectTransform == null)
            {
                return;
            }

            // Adopt position changes made by a layout or caller without feeding our own press offset back in.
            Vector2 expectedPosition = baseAnchoredPosition + appliedOffset;
            if ((rectTransform.anchoredPosition - expectedPosition).sqrMagnitude > 0.0001f)
            {
                baseAnchoredPosition += rectTransform.anchoredPosition - expectedPosition;
            }

            bool canInteract = button == null || button.IsInteractable();
            float targetScaleFactor = pressed && canInteract
                ? pressedScale
                : (pointerOver || selected) && canInteract ? hoverScale : 1f;
            Vector2 targetOffset = pressed && canInteract ? Vector2.down * pressedOffset : Vector2.zero;
            float t = 1f - Mathf.Exp(-animationSpeed * Time.unscaledDeltaTime);

            rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, baseScale * targetScaleFactor, t);
            appliedOffset = Vector2.Lerp(appliedOffset, targetOffset, t);
            rectTransform.anchoredPosition = baseAnchoredPosition + appliedOffset;
        }

        public void Configure(SharedButtonVariant buttonVariant)
        {
            Configure(buttonVariant, GetDefaultStyle(buttonVariant));
        }

        public void Configure(SharedButtonVariant buttonVariant, Color baseColor, Color accentColor)
        {
            SharedButtonStyle defaultStyle = GetDefaultStyle(buttonVariant);
            Configure(buttonVariant, new SharedButtonStyle(baseColor, accentColor, defaultStyle.TextColor));
        }

        public void Configure(
            SharedButtonVariant buttonVariant,
            Color baseColor,
            Color accentColor,
            Color textColor)
        {
            Configure(buttonVariant, new SharedButtonStyle(baseColor, accentColor, textColor));
        }

        public void Configure(SharedButtonVariant buttonVariant, SharedButtonStyle buttonStyle)
        {
            CacheComponents();
            variant = buttonVariant;
            style = buttonStyle;
            hasConfiguredStyle = true;

            background.color = style.BaseColor;
            background.raycastTarget = true;
            ConfigureButtonColors();
            ConfigureEffects();
            ApplyContentContrast();
        }

        public static SharedButtonStyle GetDefaultStyle(SharedButtonVariant buttonVariant)
        {
            Color text = new Color(0.97f, 0.96f, 0.9f, 1f);
            switch (buttonVariant)
            {
                case SharedButtonVariant.Primary:
                    return new SharedButtonStyle(
                        new Color(0.16f, 0.32f, 0.42f, 1f),
                        new Color(0.25f, 0.86f, 0.95f, 1f), text);
                case SharedButtonVariant.Navigation:
                    return new SharedButtonStyle(
                        new Color(0.12f, 0.18f, 0.24f, 1f),
                        new Color(0.4f, 0.76f, 0.9f, 1f), text);
                case SharedButtonVariant.Back:
                    return new SharedButtonStyle(
                        new Color(0.38f, 0.22f, 0.08f, 1f),
                        new Color(1f, 0.58f, 0.16f, 1f), text);
                case SharedButtonVariant.Destructive:
                    return new SharedButtonStyle(
                        new Color(0.42f, 0.12f, 0.14f, 1f),
                        new Color(1f, 0.34f, 0.3f, 1f), Color.white);
                default:
                    return new SharedButtonStyle(
                        new Color(0.12f, 0.16f, 0.2f, 1f),
                        new Color(0.42f, 0.65f, 0.76f, 1f), text);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => pointerOver = true;

        public void OnPointerExit(PointerEventData eventData)
        {
            pointerOver = false;
            pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                pressed = true;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                pressed = false;
                RestoreImmediately();
            }
        }

        public void OnSelect(BaseEventData eventData) => selected = true;

        public void OnDeselect(BaseEventData eventData)
        {
            selected = false;
            pressed = false;
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (button != null && !button.IsInteractable())
            {
                return;
            }

            if (submitRoutine != null)
            {
                StopCoroutine(submitRoutine);
            }

            submitRoutine = StartCoroutine(PlaySubmitFeedback());
        }

        private IEnumerator PlaySubmitFeedback()
        {
            pressed = true;
            yield return new WaitForSecondsRealtime(0.1f);
            pressed = false;
            submitRoutine = null;
        }

        private void CacheComponents()
        {
            if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
            if (background == null) background = GetComponent<Image>();
            if (button == null) button = GetComponent<Button>();
        }

        private void CaptureBaseTransform()
        {
            if (rectTransform == null)
            {
                return;
            }

            baseAnchoredPosition = rectTransform.anchoredPosition - appliedOffset;
            appliedOffset = Vector2.zero;
            baseScale = rectTransform.localScale;
            rectTransform.anchoredPosition = baseAnchoredPosition;
        }

        private void RestoreImmediately()
        {
            if (rectTransform == null)
            {
                return;
            }

            rectTransform.anchoredPosition = baseAnchoredPosition;
            rectTransform.localScale = baseScale;
            appliedOffset = Vector2.zero;
        }

        private void ConfigureButtonColors()
        {
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.Lerp(Color.white, style.AccentColor, 0.12f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            button.targetGraphic = background;
        }

        private void ConfigureEffects()
        {
            Outline outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = style.AccentColor;
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            Shadow shadow = null;
            Shadow[] shadows = GetComponents<Shadow>();
            for (int i = 0; i < shadows.Length; i++)
            {
                // Outline derives from Shadow; only reuse an actual Shadow component.
                if (shadows[i].GetType() == typeof(Shadow))
                {
                    shadow = shadows[i];
                    break;
                }
            }

            if (shadow == null) shadow = gameObject.AddComponent<Shadow>();
            shadow.effectColor = Darken(style.BaseColor, 0.58f, 0.9f);
            shadow.effectDistance = new Vector2(0f, -5f);
            shadow.useGraphicAlpha = true;

            surfaceHighlight = GetOrCreateLayer(surfaceHighlight, HighlightName);
            SetLayerAnchors(surfaceHighlight.rectTransform, 0.84f, 1f, 2f, -2f, -2f, -1f);
            surfaceHighlight.color = Lighten(style.BaseColor, 0.35f, 0.72f);

            depthRim = GetOrCreateLayer(depthRim, DepthRimName);
            SetLayerAnchors(depthRim.rectTransform, 0f, 0.13f, 2f, -2f, 1f, -1f);
            depthRim.color = Darken(style.BaseColor, 0.5f, 0.86f);
        }

        private Image GetOrCreateLayer(Image current, string layerName)
        {
            if (current == null)
            {
                Transform existing = transform.Find(layerName);
                if (existing != null)
                {
                    current = existing.GetComponent<Image>();
                    if (current == null) current = existing.gameObject.AddComponent<Image>();
                }
            }

            if (current == null)
            {
                GameObject layer = new GameObject(layerName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                layer.transform.SetParent(transform, false);
                current = layer.GetComponent<Image>();
            }

            current.raycastTarget = false;
            current.transform.SetAsFirstSibling();
            return current;
        }

        private void ApplyContentContrast()
        {
            Graphic[] graphics = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == background || graphic == surfaceHighlight || graphic == depthRim)
                {
                    continue;
                }

                graphic.color = style.TextColor;
                graphic.raycastTarget = false;
            }
        }

        private static void SetLayerAnchors(
            RectTransform rect,
            float minY,
            float maxY,
            float left,
            float right,
            float bottom,
            float top)
        {
            rect.anchorMin = new Vector2(0f, minY);
            rect.anchorMax = new Vector2(1f, maxY);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(right, top);
            rect.localScale = Vector3.one;
        }

        private static Color Lighten(Color color, float amount, float alpha)
        {
            Color result = Color.Lerp(color, Color.white, amount);
            result.a = alpha;
            return result;
        }

        private static Color Darken(Color color, float multiplier, float alpha)
        {
            return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, alpha);
        }
    }
}
