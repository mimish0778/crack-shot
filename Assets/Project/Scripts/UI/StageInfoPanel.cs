using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace CrackShot
{
    public class StageInfoPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Stage Info")]
        [SerializeField] private int stageIndex;
        [SerializeField] private StageData stageData;

        [Header("Panel Settings")]
        [SerializeField] private RectTransform infoPanel;
        [SerializeField] private float panelHeight = 100f;
        [SerializeField] private float animDuration = 0.15f;

        [Header("Text (Order: Par -> BestScore -> BestTime -> Label)")]
        [SerializeField] private TextMeshProUGUI parText;
        [SerializeField] private TextMeshProUGUI bestScoreText;
        [SerializeField] private TextMeshProUGUI bestTimeText;
        [SerializeField] private TextMeshProUGUI bestLabelText;

        private CanvasGroup _group;
        private Coroutine _anim;

        private void Awake()
        {
            if (infoPanel == null)
            {
                return;
            }
            _group = infoPanel.gameObject.GetOrAdd<CanvasGroup>();
            infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, 0f);
            _group.alpha = 0f;
            infoPanel.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData e)
        {
            if (infoPanel == null)
            {
                return;
            }
            UpdatePanel();
            if (_anim != null)
            {
                StopCoroutine(_anim);
            }
            _anim = StartCoroutine(Expand());
        }

        public void OnPointerExit(PointerEventData e)
        {
            if (infoPanel == null)
            {
                return;
            }
            if (_anim != null)
            {
                StopCoroutine(_anim);
            }
            _anim = StartCoroutine(Collapse());
        }

        private IEnumerator Expand()
        {
            infoPanel.gameObject.SetActive(true);
            float startH = infoPanel.sizeDelta.y;
            yield return CyberFx.Tween(animDuration, raw =>
            {
                float t = Ease.OutQuad(raw);
                infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, Mathf.Lerp(startH, panelHeight, t));
                _group.alpha = t;
            });
            infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, panelHeight);
            _group.alpha = 1f;
        }

        private IEnumerator Collapse()
        {
            float startH = infoPanel.sizeDelta.y;
            yield return CyberFx.Tween(animDuration, t =>
            {
                infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, Mathf.Lerp(startH, 0f, t));
                _group.alpha = 1f - t;
            });
            infoPanel.sizeDelta = new Vector2(infoPanel.sizeDelta.x, 0f);
            _group.alpha = 0f;
            infoPanel.gameObject.SetActive(false);
        }

        private void UpdatePanel()
        {
            if (parText != null && stageData != null)
            {
                parText.text = $"Par: {stageData.Par}";
            }

            if (ScoreManager.Instance == null)
            {
                return;
            }
            int best = ScoreManager.Instance.GetBestScore(stageIndex);

            if (best < 0)
            {
                if (bestScoreText != null)
                {
                    bestScoreText.text = "Best: --";
                }
                if (bestTimeText != null)
                {
                    bestTimeText.text = "--:--.--";
                }
                if (bestLabelText != null)
                {
                    bestLabelText.text = "";
                }
            }
            else
            {
                float bestTime = ScoreManager.Instance.GetBestTime(stageIndex);

                if (bestScoreText != null)
                {
                    bestScoreText.text = $"Best: {best}";
                }
                if (bestTimeText != null)
                {
                    bestTimeText.text = bestTime >= 0
                        ? ScoreManager.GetTimeString(bestTime)
                        : "--:--.--";
                }

                if (bestLabelText != null && stageData != null)
                {
                    bestLabelText.text = GetLabel(best - stageData.Par);
                }
            }
        }

        private static string GetLabel(int diff)
        {
            if (diff > ScoreManager.MaxNamedOverParDiff)
            {
                return "";
            }
            return ScoreManager.GetLabel(diff);
        }
    }
}
