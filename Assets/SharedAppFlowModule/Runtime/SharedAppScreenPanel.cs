using System.Collections;
using UnityEngine;

namespace SharedAppFlowModule
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class SharedAppScreenPanel : MonoBehaviour
    {
        [SerializeField] private SharedAppScreenId screenId;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private float transitionDuration = 0.2f;
        [SerializeField] private Vector2 showHiddenOffset = new Vector2(0f, -24f);
        [SerializeField] private Vector2 hideHiddenOffset = new Vector2(0f, -24f);

        private CanvasGroup canvasGroup;
        private RectTransform animatedRoot;
        private Vector2 shownPosition;
        private Coroutine transitionRoutine;
        private bool initialized;

        public SharedAppScreenId ScreenId => screenId;
        public bool IsTransitioning { get; private set; }
        public bool IsShown { get; private set; }
        public bool HasCompletedShow { get; private set; }

        public void Configure(SharedAppScreenId id, RectTransform root)
        {
            screenId = id;
            contentRoot = root;
            initialized = false;
        }

        public void ConfigureTransition(float duration, Vector2 offset)
        {
            transitionDuration = Mathf.Max(0.01f, duration);
            showHiddenOffset = offset;
            hideHiddenOffset = offset;
        }

        public void ConfigureTransition(float duration, Vector2 showOffset, Vector2 hideOffset)
        {
            transitionDuration = Mathf.Max(0.01f, duration);
            showHiddenOffset = showOffset;
            hideHiddenOffset = hideOffset;
        }

        private void Awake()
        {
            Initialize();
        }

        public void Show(bool instant = false)
        {
            Initialize();
            gameObject.SetActive(true);

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            if (instant || !gameObject.activeInHierarchy)
            {
                ApplyVisibleState(1f, true);
                IsTransitioning = false;
                IsShown = true;
                HasCompletedShow = true;
                return;
            }

            IsTransitioning = true;
            IsShown = false;
            HasCompletedShow = false;
            transitionRoutine = StartCoroutine(Animate(true));
        }

        public void Hide(bool instant = false)
        {
            Initialize();

            if (transitionRoutine != null)
            {
                StopCoroutine(transitionRoutine);
            }

            if (instant || !gameObject.activeInHierarchy)
            {
                ApplyVisibleState(0f, false);
                IsTransitioning = false;
                IsShown = false;
                HasCompletedShow = false;
                gameObject.SetActive(false);
                return;
            }

            IsTransitioning = true;
            IsShown = false;
            HasCompletedShow = false;
            transitionRoutine = StartCoroutine(Animate(false));
        }

        private void Initialize()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (contentRoot == null)
            {
                contentRoot = transform as RectTransform;
            }

            animatedRoot = contentRoot != null ? contentRoot : transform as RectTransform;

            if (!initialized)
            {
                shownPosition = animatedRoot != null ? animatedRoot.anchoredPosition : Vector2.zero;
                initialized = true;
            }
        }

        private IEnumerator Animate(bool visible)
        {
            float from = visible ? 0f : 1f;
            float to = visible ? 1f : 0f;
            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, transitionDuration);

            ApplyVisibleState(from, visible);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = 1f - Mathf.Pow(1f - t, 3f);
                ApplyVisibleState(Mathf.Lerp(from, to, t), visible);
                yield return null;
            }

            ApplyVisibleState(to, visible);

            if (!visible)
            {
                gameObject.SetActive(false);
            }

            IsTransitioning = false;
            IsShown = visible;
            HasCompletedShow = visible;
            transitionRoutine = null;
        }

        private void ApplyVisibleState(float amount, bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = amount;
                canvasGroup.interactable = amount >= 0.99f;
                canvasGroup.blocksRaycasts = amount >= 0.99f;
            }

            if (animatedRoot != null)
            {
                Vector2 offset = visible ? showHiddenOffset : hideHiddenOffset;
                animatedRoot.anchoredPosition = Vector2.Lerp(shownPosition + offset, shownPosition, amount);
            }
        }
    }
}
