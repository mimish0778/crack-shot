using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrackShot
{
    public class ResultSceneUI : MonoBehaviour
    {
        private const float SemitoneRatio = 1.05946f;

        [Header("Score")]
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI parText;
        [SerializeField] private TextMeshProUGUI shotCountText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI labelText;

        [Header("Animation")]
        [SerializeField] private float itemInterval = 0.3f;
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float scoreDelay = 0.3f;
        [Tooltip("表示開始までの最初の溜め。")]
        [SerializeField] private float initialRevealDelay = 0.3f;
        [Tooltip("ボタンを順番にフェードインさせる間隔。")]
        [SerializeField] private float buttonRevealInterval = 0.1f;
        [Tooltip("スコア各行デコードのノイズ／確定インターバル。")]
        [SerializeField] private float decodeNoiseInterval = 0.012f;
        [SerializeField] private float decodeConfirmInterval = 0.015f;

        [Header("Label Punch")]
        [SerializeField] private float labelPunchScale = 1.6f;
        [SerializeField] private float labelPunchTime = 0.18f;
        [SerializeField] private float labelSettleTime = 0.28f;

        [Header("Buttons")]
        [SerializeField] private Button selectButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button nextStageButton;

        private void Start()
        {
            retryButton?.onClick.AddListener(OnRetryClicked);
            selectButton?.onClick.AddListener(OnSelectClicked);
            nextStageButton?.onClick.AddListener(OnNextStageClicked);

            if (stageNameText && StageManager.Instance?.CurrentStageData != null)
            {
                stageNameText.text = StageManager.Instance.CurrentStageData.StageName;
            }

            InitializeButton(retryButton);
            InitializeButton(selectButton);
            InitializeButton(nextStageButton);
            HideAll();

            StartCoroutine(RevealResults());
        }

        private void InitializeButton(Button button)
        {
            if (button == null)
            {
                return;
            }
            var cg = button.gameObject.GetOrAdd<CanvasGroup>();
            cg.alpha = 0f;
            button.gameObject.SetActive(false);
        }

        private void HideAll()
        {
            PrepareForReveal(parText);
            PrepareForReveal(shotCountText);
            PrepareForReveal(timerText);
            PrepareForReveal(labelText);
        }

        private static void PrepareForReveal(TextMeshProUGUI text)
        {
            if (text == null)
            {
                return;
            }
            text.gameObject.SetActive(true);
            CyberText.SetAlpha(text, 0f);
        }

        private IEnumerator RevealResults()
        {
            if (ScoreManager.Instance == null)
            {
                yield break;
            }

            var (shot, par, label) = ScoreManager.Instance.GetResult();
            string timeStr = ScoreManager.GetTimeString(ScoreManager.Instance.ElapsedTime);

            yield return new WaitForSeconds(initialRevealDelay);

            if (parText)
            {
                AudioManager.Instance?.PlayResultReveal();
                yield return StartCoroutine(Decode(parText, $"Par: {par}"));
            }
            yield return new WaitForSeconds(itemInterval);

            if (shotCountText)
            {
                AudioManager.Instance?.PlayResultReveal(SemitoneRatio);
                yield return StartCoroutine(Decode(shotCountText, $"Shots: {shot}"));
            }
            yield return new WaitForSeconds(itemInterval);

            if (timerText)
            {
                AudioManager.Instance?.PlayResultReveal(SemitoneRatio * SemitoneRatio);
                yield return StartCoroutine(Decode(timerText, timeStr));
            }
            yield return new WaitForSeconds(itemInterval + scoreDelay);

            if (labelText && !string.IsNullOrEmpty(label))
            {
                bool isUnderPar = ScoreManager.Instance.ScoreToPar < 0;
                AudioManager.Instance?.PlayResultLabel();
                yield return StartCoroutine(Decode(labelText, label));
                if (isUnderPar)
                {
                    yield return StartCoroutine(UnderParReveal(labelText));
                }
                else
                {
                    yield return StartCoroutine(PunchScale(labelText.transform));
                }
            }
            yield return new WaitForSeconds(itemInterval);

            bool hasNext = GameManager.Instance != null && GameManager.Instance.HasNextStage;

            yield return StartCoroutine(FadeInButton(selectButton));
            yield return new WaitForSeconds(buttonRevealInterval);
            yield return StartCoroutine(FadeInButton(retryButton));
            yield return new WaitForSeconds(buttonRevealInterval);
            if (hasNext)
            {
                yield return StartCoroutine(FadeInButton(nextStageButton));
            }
        }

        private IEnumerator UnderParReveal(TextMeshProUGUI text)
        {
            var shimmer = text.gameObject.GetOrAdd<MetallicShimmer>();
            shimmer.Play(MetallicShimmer.TierForScore(ScoreManager.Instance.ScoreToPar));

            yield return StartCoroutine(PunchScale(text.transform));
        }

        private IEnumerator PunchScale(Transform target)
        {
            Vector3 normal = target.localScale;
            Vector3 big = normal * labelPunchScale;

            yield return CyberFx.Tween(labelPunchTime, t => target.localScale = Vector3.Lerp(normal, big, t));

            yield return CyberFx.Tween(labelSettleTime, t =>
            {
                float overshoot = 1f + 0.25f * Mathf.Sin(t * Mathf.PI * 2.5f) * (1f - t);
                target.localScale = normal * overshoot;
            });

            target.localScale = normal;
        }

        private IEnumerator Decode(TextMeshProUGUI text, string target)
        {
            text.gameObject.SetActive(true);
            CyberText.SetAlpha(text, 1f);
            yield return StartCoroutine(CyberText.Decode(
                text, target, decodeNoiseInterval, decodeConfirmInterval,
                CyberFx.CyanHex, CyberFx.PinkHex));
            text.color = CyberFx.Cyan;
        }

        private IEnumerator FadeInButton(Button button)
        {
            if (button == null)
            {
                yield break;
            }
            var cg = button.gameObject.GetOrAdd<CanvasGroup>();
            button.gameObject.SetActive(true);
            button.interactable = false;
            cg.alpha = 0f;

            yield return CyberFx.Tween(fadeInTime, t => cg.alpha = t);
            cg.alpha = 1f;
            button.interactable = true;
        }

        private void OnRetryClicked()
        {
            AudioManager.Instance?.PlayResultRetry();
            GameManager.Instance?.ResetAndGoToPlay();
        }

        private void OnSelectClicked()
        {
            AudioManager.Instance?.PlayResultSelect();
            GameManager.Instance?.ResetAndGoToSelect();
        }

        private void OnNextStageClicked()
        {
            if (GameManager.Instance == null)
            {
                return;
            }
            if (!GameManager.Instance.HasNextStage)
            {
                AudioManager.Instance?.PlayResultSelect();
                GameManager.Instance.ResetAndGoToSelect();
                return;
            }
            AudioManager.Instance?.PlayResultNextStage();
            GameManager.Instance.SelectedStageIndex = GameManager.Instance.NextStageIndex;
            GameManager.Instance.ResetAndGoToPlay();
        }
    }
}
