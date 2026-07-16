using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SharedSettingsModal : MonoBehaviour
    {
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private float transitionDuration = 0.22f;
        [SerializeField] private Vector2 hiddenOffset = new Vector2(0f, -420f);

        private CanvasGroup canvasGroup;
        private Vector2 shownPosition;
        private Coroutine transitionRoutine;
        private bool isOpen;
        private bool initialized;

        public bool IsOpen => isOpen;

        public void Configure(RectTransform panel, Button close)
        {
            panelRoot = panel;
            closeButton = close;
            initialized = false;
        }

        private void Awake()
        {
            Initialize();

            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
                closeButton.onClick.AddListener(Close);
            }
        }

        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }

        public void Open()
        {
            SetOpen(true, false);
        }

        public void Close()
        {
            SetOpen(false, false);
        }

        public void Toggle()
        {
            SetOpen(!isOpen, false);
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
