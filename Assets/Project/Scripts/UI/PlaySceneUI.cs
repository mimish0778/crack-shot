using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrackShot
{
    public class PlaySceneUI : Singleton<PlaySceneUI>
    {
        [Header("References")]
        [SerializeField] private TextMeshProUGUI parText;
        [SerializeField] private TextMeshProUGUI shotCountText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI failedMessageText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button retryButton;

        [Header("Cyber Failed Message")]
        [SerializeField] private float decodeSpeed = 0.03f;
        [SerializeField] private int noiseIterations = 1;

        private const float PulseSpeed = 6.3f;

        private void Start()
        {
            retryButton?.onClick.AddListener(OnRetryClicked);
            selectButton?.onClick.AddListener(OnSelectClicked);
            HideMessage();
            UpdateShotCount(0);
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnShotCountChanged.AddListener(UpdateShotCount);
                ScoreManager.Instance.OnParChanged.AddListener(UpdatePar);
                ScoreManager.Instance.OnTimeChanged.AddListener(UpdateTimer);
                UpdatePar(ScoreManager.Instance.Par);
            }
        }

        public void UpdateShotCount(int count)
        {
            if (shotCountText == null)
            {
                return;
            }
            shotCountText.StopAllCoroutines();
            shotCountText.StartCoroutine(
                CyberText.Scramble(shotCountText, $"Shots: {count}", CyberFx.Pink, Color.white));
        }

        public void UpdatePar(int par) { if (parText != null) { parText.text = $"Par: {par}"; } }
        public void UpdateTimer(float time) { if (timerText != null) { timerText.text = ScoreManager.GetTimeString(time); } }

        public void ShowFailedMessage(FailReason reason, float visibleDuration = 2f)
        {
            if (failedMessageText == null)
            {
                return;
            }

            failedMessageText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(DecodeAndAutoHide(CyberMessage(reason), visibleDuration));
        }

        public void HideMessage()
        {
            StopAllCoroutines();
            if (failedMessageText != null)
            {
                failedMessageText.gameObject.SetActive(false);
            }
        }

        private IEnumerator PinkPulse()
        {
            float t = 0f;
            while (failedMessageText != null && failedMessageText.gameObject.activeSelf)
            {
                t += Time.deltaTime;
                failedMessageText.color = CyberFx.Pink.WithAlpha(0.5f + 0.5f * Mathf.Abs(Mathf.Sin(t * PulseSpeed)));
                yield return null;
            }
        }

        private IEnumerator DecodeAndAutoHide(string target, float duration)
        {
            yield return StartCoroutine(DecodeMessage(target));
            yield return new WaitForSeconds(duration);
            HideMessage();
        }

        private static string CyberMessage(FailReason reason) => reason switch
        {
            FailReason.NotThroughGate => "[ GATE BYPASS FAILED ]",
            FailReason.OutOfBounds => "[ BOUNDARY VIOLATION ]",
            FailReason.BallCollision => "[ COLLISION DETECTED ]",
            FailReason.DidNotPassThrough => "[ GATE BYPASS FAILED ]",
            _ => "[ ERROR ]",
        };

        private IEnumerator DecodeMessage(string target)
        {
            yield return StartCoroutine(CyberText.Decode(
                failedMessageText, target,
                decodeSpeed * 0.4f, decodeSpeed * 0.3f,
                CyberFx.PinkHex, CyberFx.CyanHex,
                "333333", noiseIterations));

            StartCoroutine(PinkPulse());
        }

        private void OnRetryClicked()
        {
            AudioManager.Instance?.PlayPlayRetry();
            GameManager.Instance?.ResetAndReload();
        }

        private void OnSelectClicked()
        {
            AudioManager.Instance?.PlayPlaySelect();
            GameManager.Instance?.ResetAndGoToSelect();
        }
    }
}
