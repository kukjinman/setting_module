using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SharedAppFlowModule
{
    public sealed class SharedIntroLogoBreakEffect : MonoBehaviour
    {
        [SerializeField] private Image logoImage;
        [SerializeField] private RectTransform piecesRoot;
        [SerializeField] private int columns = 16;
        [SerializeField] private int rows = 16;
        [SerializeField] private float breakDuration = 1.25f;
        [SerializeField] private float scatterDistance = 120f;
        [SerializeField] private float fallDistance = 28f;
        [SerializeField, Range(0f, 1f)] private float alphaThreshold = 0.08f;

        private readonly List<Piece> pieces = new List<Piece>();
        private Color originalLogoColor = Color.white;

        public bool WasSkipped { get; private set; }

        public void Configure(Image logo)
        {
            logoImage = logo;
        }

        private void Awake()
        {
            CacheOriginalColor();
        }

        private void OnEnable()
        {
            ResetEffect();
        }

        public void ResetEffect()
        {
            CacheOriginalColor();
            ClearPieces();

            if (logoImage != null)
            {
                logoImage.enabled = true;
                logoImage.color = originalLogoColor;
            }
        }

        public IEnumerator Play(System.Func<bool> skipRequested = null)
        {
            WasSkipped = false;

            if (logoImage == null)
            {
                yield break;
            }

            CacheOriginalColor();
            BuildPieces();
            logoImage.enabled = false;

            float elapsed = 0f;
            float duration = Mathf.Max(0.01f, breakDuration);

            while (elapsed < duration)
            {
                if (skipRequested != null && skipRequested())
                {
                    WasSkipped = true;
                    ClearPieces();
                    yield break;
                }

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                ApplyPieces(eased);
                yield return null;
            }

            ClearPieces();
        }

        private void CacheOriginalColor()
        {
            if (logoImage != null)
            {
                originalLogoColor = logoImage.color;
            }
        }

        private void BuildPieces()
        {
            ClearPieces();
            EnsurePiecesRoot();

            RectTransform logoRect = logoImage.rectTransform;
            Rect rect = logoRect.rect;
            int safeColumns = Mathf.Max(1, columns);
            int safeRows = Mathf.Max(1, rows);
            Vector2 cellSize = new Vector2(rect.width / safeColumns, rect.height / safeRows);

            for (int y = 0; y < safeRows; y++)
            {
                for (int x = 0; x < safeColumns; x++)
                {
                    float u = (x + 0.5f) / safeColumns;
                    float v = (y + 0.5f) / safeRows;
                    Color color = SampleLogoColor(u, v);

                    if (color.a <= alphaThreshold)
                    {
                        continue;
                    }

                    Vector2 start = new Vector2(
                        rect.xMin + cellSize.x * (x + 0.5f),
                        rect.yMin + cellSize.y * (y + 0.5f));

                    CreatePiece(start, cellSize, color);
                }
            }
        }

        private void EnsurePiecesRoot()
        {
            if (piecesRoot == null)
            {
                GameObject rootObject = new GameObject("Logo Break Pieces", typeof(RectTransform));
                rootObject.transform.SetParent(logoImage.transform.parent, false);
                piecesRoot = rootObject.GetComponent<RectTransform>();
            }

            RectTransform logoRect = logoImage.rectTransform;
            piecesRoot.anchorMin = logoRect.anchorMin;
            piecesRoot.anchorMax = logoRect.anchorMax;
            piecesRoot.pivot = logoRect.pivot;
            piecesRoot.sizeDelta = logoRect.sizeDelta;
            piecesRoot.anchoredPosition = logoRect.anchoredPosition;
            piecesRoot.localScale = logoRect.localScale;
            piecesRoot.localRotation = logoRect.localRotation;
            piecesRoot.SetAsLastSibling();
        }

        private void CreatePiece(Vector2 start, Vector2 size, Color color)
        {
            GameObject pieceObject = new GameObject("Logo Piece", typeof(RectTransform));
            pieceObject.transform.SetParent(piecesRoot, false);

            RectTransform rect = pieceObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Mathf.Max(2f, size.x * 0.92f), Mathf.Max(2f, size.y * 0.92f));
            rect.anchoredPosition = start;

            Image image = pieceObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;

            Vector2 random = UnityEngine.Random.insideUnitCircle;
            Vector2 radial = start.sqrMagnitude > 0.001f ? start.normalized : random.normalized;
            Vector2 direction = (radial * 0.75f + random * 0.55f).normalized;
            Vector2 end = start
                + direction * UnityEngine.Random.Range(scatterDistance * 0.35f, scatterDistance)
                + Vector2.down * UnityEngine.Random.Range(0f, fallDistance);

            pieces.Add(new Piece
            {
                Rect = rect,
                Image = image,
                Start = start,
                End = end,
                Rotation = UnityEngine.Random.Range(-90f, 90f),
                Color = color
            });
        }

        private Color SampleLogoColor(float u, float v)
        {
            Sprite sprite = logoImage.sprite;

            if (sprite == null || sprite.texture == null)
            {
                return logoImage.color;
            }

            try
            {
                Texture2D texture = sprite.texture;
                Rect rect = sprite.rect;
                float textureU = (rect.x + rect.width * u) / texture.width;
                float textureV = (rect.y + rect.height * v) / texture.height;
                return texture.GetPixelBilinear(textureU, textureV);
            }
            catch (UnityException)
            {
                return logoImage.color;
            }
        }

        private void ApplyPieces(float t)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                Piece piece = pieces[i];

                if (piece.Rect == null || piece.Image == null)
                {
                    continue;
                }

                piece.Rect.anchoredPosition = Vector2.LerpUnclamped(piece.Start, piece.End, t);
                piece.Rect.localRotation = Quaternion.Euler(0f, 0f, piece.Rotation * t);
                piece.Rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.72f, t);

                Color color = piece.Color;
                color.a *= 1f - t;
                piece.Image.color = color;
            }
        }

        private void ClearPieces()
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i].Rect != null)
                {
                    Destroy(pieces[i].Rect.gameObject);
                }
            }

            pieces.Clear();
        }

        private struct Piece
        {
            public RectTransform Rect;
            public Image Image;
            public Vector2 Start;
            public Vector2 End;
            public float Rotation;
            public Color Color;
        }
    }
}
