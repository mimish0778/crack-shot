using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CrackShot
{
    public class SceneLoader : PersistentSingleton<SceneLoader>
    {
        [Header("Transition")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private float transitionDuration = 0.45f;

        [Header("Scene Names")]
        [SerializeField] private string titleSceneName = "TitleScene";
        [SerializeField] private string selectSceneName = "SelectScene";
        [SerializeField] private string playSceneName = "PlayScene";
        [SerializeField] private string resultSceneName = "ResultScene";

        private Image _scanLineImage;

        private static Color MutedCyan => CyberFx.MutedCyan;
        private static Color MutedPink => CyberFx.MutedPink;

        private void Start()
        {
            SetupScanLine();
            if (fadeImage != null)
            {
                fadeImage.color = Color.black;
                StartCoroutine(BootSequence());
            }
        }

        private IEnumerator BootSequence()
        {
            fadeImage.color = Color.black;
            HideScanLine();

            yield return new WaitForSeconds(0.4f);

            yield return StartCoroutine(Flicker(0.6f, flickerCount: 7));

            for (int i = 0; i < 3; i++)
            {
                float speed = 0.08f + i * 0.04f;
                Color lineCol = i % 2 == 0 ? MutedCyan : MutedPink;
                lineCol.a = 0.6f + i * 0.15f;
                yield return StartCoroutine(SingleScan(speed, lineCol));
                yield return new WaitForSeconds(0.03f);
            }

            yield return CyberFx.Tween(0.12f, t =>
                fadeImage.color = MutedCyan.WithAlpha(Mathf.Sin(t * Mathf.PI) * 0.4f));

            yield return CyberFx.Tween(0.4f, t =>
            {
                fadeImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.3f, 0f, t));
            });

            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            HideScanLine();
        }

        private IEnumerator Flicker(float totalTime, int flickerCount)
        {
            float interval = totalTime / flickerCount;
            for (int i = 0; i < flickerCount; i++)
            {
                fadeImage.color = Color.Lerp(Color.black, (i % 2 == 0) ? MutedCyan : MutedPink,
                                     Random.Range(0.08f, 0.35f)).WithAlpha(1f);
                yield return new WaitForSeconds(interval * Random.Range(0.1f, 0.4f));

                fadeImage.color = Color.black;
                yield return new WaitForSeconds(interval * Random.Range(0.05f, 0.3f));
            }
            fadeImage.color = Color.black;
        }

        private IEnumerator SingleScan(float duration, Color lineColor)
        {
            if (_scanLineImage == null)
            {
                yield break;
            }
            _scanLineImage.gameObject.SetActive(true);
            fadeImage.color = Color.black;
            var rt = _scanLineImage.rectTransform;
            rt.sizeDelta = new Vector2(0f, 3f);

            yield return CyberFx.Tween(duration, t =>
            {
                float y = Mathf.Lerp(1f, 0f, t);
                rt.anchorMin = new Vector2(0f, y);
                rt.anchorMax = new Vector2(1f, y);
                rt.anchoredPosition = Vector2.zero;
                _scanLineImage.color = lineColor;
            });

            HideScanLine();
        }

        public void LoadTitleScene() => LoadScene(titleSceneName);
        public void LoadSelectScene() => LoadScene(selectSceneName);
        public void LoadPlayScene() => LoadScene(playSceneName);
        public void LoadResultScene() => LoadScene(resultSceneName);

        public void LoadScene(string sceneName) => StartCoroutine(LoadCoroutine(sceneName));

        public void ReloadCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);

        private IEnumerator LoadCoroutine(string sceneName)
        {
            yield return StartCoroutine(CyberOut());
            SceneManager.LoadScene(sceneName);
            yield return null;
            yield return StartCoroutine(CyberIn());
        }

        private IEnumerator CyberOut()
        {
            if (fadeImage == null)
            {
                yield break;
            }
            float half = transitionDuration * 0.5f;

            yield return CyberFx.Tween(half * 0.4f, t =>
                fadeImage.color = MutedCyan.WithAlpha(Mathf.Lerp(0f, 0.35f, t)));

            yield return ScanSweep(half * 0.4f, topToBottom: true);

            yield return CyberFx.Tween(half * 0.2f, t =>
                fadeImage.color = Color.Lerp(MutedCyan.WithAlpha(0.35f), Color.black, t));

            fadeImage.color = Color.black;
            HideScanLine();
        }

        private IEnumerator CyberIn()
        {
            if (fadeImage == null)
            {
                yield break;
            }
            fadeImage.color = Color.black;
            float half = transitionDuration * 0.5f;

            yield return ScanSweep(half * 0.4f, topToBottom: true);

            yield return CyberFx.Tween(half * 0.3f, t =>
                fadeImage.color = MutedCyan.WithAlpha(Mathf.Lerp(0.3f, 0f, t)));

            fadeImage.color = new Color(0, 0, 0, 0);
            HideScanLine();
        }

        private IEnumerator ScanSweep(float duration, bool topToBottom)
        {
            if (_scanLineImage == null)
            {
                yield break;
            }
            _scanLineImage.gameObject.SetActive(true);

            var rect = _scanLineImage.rectTransform;
            float from = topToBottom ? 1f : 0f;
            float to = topToBottom ? 0f : 1f;

            yield return CyberFx.Tween(duration, t =>
            {
                float anchorY = Mathf.Lerp(from, to, t);
                rect.anchorMin = new Vector2(0f, anchorY);
                rect.anchorMax = new Vector2(1f, anchorY);
                rect.anchoredPosition = Vector2.zero;

                _scanLineImage.color = ((Time.time % 2f < 1f) ? MutedCyan : MutedPink).WithAlpha(1f);
            });
        }

        private void HideScanLine()
        {
            if (_scanLineImage != null)
            {
                _scanLineImage.gameObject.SetActive(false);
            }
        }

        private void SetupScanLine()
        {
            if (_scanLineImage != null)
            {
                HideScanLine();
                return;
            }
            if (fadeImage == null)
            {
                return;
            }

            var go = new GameObject("ScanLine");
            go.transform.SetParent(fadeImage.transform.parent, false);

            var img = go.AddComponent<Image>();
            img.color = MutedCyan;
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(1f, 0.5f);
            rt.sizeDelta = new Vector2(0f, 4f);
            rt.anchoredPosition = Vector2.zero;

            go.transform.SetAsLastSibling();
            _scanLineImage = img;
            HideScanLine();
        }
    }
}
