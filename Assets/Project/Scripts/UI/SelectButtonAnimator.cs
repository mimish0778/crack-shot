using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrackShot
{
    public class SelectButtonAnimator : ButtonAnimator
    {
        [Header("Split Effect")]
        [SerializeField] private float flyDistance = 4f;
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private float squareSize = 18f;
        [SerializeField] private Image iconImage;

        [Header("Stage Icon")]
        [Tooltip("Icons corresponding to the selected stage number (1-based). Index 0 = Stage 1.")]
        [SerializeField] private Sprite[] stageIcons;

        private Canvas _canvas;

        private static Color StageColor => CyberFx.StageOrCurrentColor;

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponentInParent<Canvas>();
            ApplyStageIcon();
        }

        private void ApplyStageIcon()
        {
            if (iconImage == null || stageIcons == null || stageIcons.Length == 0)
            {
                return;
            }

            int index = GameManager.Instance != null ? GameManager.Instance.SelectedStageIndex : 0;
            if (index < 0 || index >= stageIcons.Length || stageIcons[index] == null)
            {
                return;
            }

            iconImage.sprite = stageIcons[index];
        }

        public override void OnPointerClick(PointerEventData e)
        {
            if (_button == null || !_button.interactable)
            {
                return;
            }
            if (e.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            base.OnPointerClick(e);
            StartCoroutine(SplitEffect());
        }

        private Vector2 SelfLocalPos(RectTransform canvasRt)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRt,
                RectTransformUtility.WorldToScreenPoint(_canvas.worldCamera, transform.position),
                _canvas.worldCamera, out Vector2 localPos);
            return localPos;
        }

        private IEnumerator SplitEffect()
        {
            if (iconImage)
            {
                iconImage.enabled = false;
            }

            var dirs = new Vector2[] { new(-1, 1), new(1, 1), new(-1, -1), new(1, -1) };
            var squares = new RectTransform[4];
            var images = new Image[4];
            var canvasRt = _canvas.GetComponent<RectTransform>();

            for (int i = 0; i < 4; i++)
            {
                var go = new GameObject($"SqPart_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_canvas.transform, false);

                var img = go.GetComponent<Image>();
                img.color = StageColor;
                img.raycastTarget = false;

                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(squareSize, squareSize);

                Vector2 localPos = SelfLocalPos(canvasRt);
                rt.anchoredPosition = localPos + dirs[i] * (squareSize * 0.6f);
                squares[i] = rt;
                images[i] = img;
            }

            yield return CyberFx.Tween(duration, t =>
            {
                float ease = Ease.OutCubic(t);
                float fade = 1f - t;

                Vector2 localPos = SelfLocalPos(canvasRt);

                for (int i = 0; i < 4; i++)
                {
                    if (squares[i] == null)
                    {
                        continue;
                    }
                    squares[i].anchoredPosition = localPos + dirs[i] * (squareSize * 0.6f + flyDistance * ease);
                    images[i].color = StageColor.WithAlpha(fade);
                }
            });

            foreach (var sq in squares)
                if (sq != null)
                {
                    Destroy(sq.gameObject);
                }

            if (iconImage)
            {
                iconImage.enabled = true;
            }
        }
    }
}
