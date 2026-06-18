using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CrackShot
{
    [RequireComponent(typeof(TMP_Text))]
    public class CyberGlitchEffect : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float intervalMin = 0.6f;
        [SerializeField] private float intervalMax = 3.5f;
        [SerializeField] private int burstCount = 5;

        [Header("Character Glitch")]
        [SerializeField] private bool enableCharGlitch = true;
        [SerializeField] private float glitchDuration = 0.04f;
        [SerializeField] private bool excludeHyphen = true;

        [Header("RGB Shift Ghost")]
        [SerializeField] private bool enableRgbShift = true;
        [SerializeField] private float rgbShiftDuration = 0.08f;
        [SerializeField] private float rgbShiftOffset = 4f;
        [SerializeField] [Range(0f, 1f)] private float rgbShiftAlpha = 0.5f;

        private TMP_Text _text;
        private string _original;

        private TextMeshProUGUI _ghostCyan;
        private TextMeshProUGUI _ghostPink;
        private Vector2 _ghostBasePos;
        private bool _ghostsVisible;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            CreateGhosts();
        }

        private void Start() => StartGlitch();

        private void OnEnable()
        {
            if (_text != null)
            {
                StartGlitch();
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            RestoreText();
            HideGhosts();
            _ghostsVisible = false;
        }

        private void Update()
        {
            if (!_ghostsVisible)
            {
                return;
            }
            if (_ghostCyan != null)
            {
                _ghostCyan.text = _text.text;
            }
            if (_ghostPink != null)
            {
                _ghostPink.text = _text.text;
            }
        }

        private void OnDestroy()
        {
            if (_ghostCyan != null)
            {
                Destroy(_ghostCyan.gameObject);
            }
            if (_ghostPink != null)
            {
                Destroy(_ghostPink.gameObject);
            }
        }

        private void StartGlitch()
        {
            StopAllCoroutines();
            _original = _text.text;
            StartCoroutine(GlitchLoop());
        }

        private IEnumerator GlitchLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(intervalMin, intervalMax));

                int count = Random.Range(1, burstCount + 1);
                for (int i = 0; i < count; i++)
                {
                    yield return StartCoroutine(PlayRandomEffect());
                    if (i < count - 1)
                    {
                        yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
                    }
                }
            }
        }

        private IEnumerator PlayRandomEffect()
        {
            if (!enableCharGlitch)
            {
                yield break;
            }

            bool withRgbShift = enableRgbShift && _ghostCyan != null && _ghostPink != null;

            if (withRgbShift)
            {
                var (cyanOffset, pinkOffset) = RandomShiftOffsets();
                ShowGhost(_ghostCyan, cyanOffset);
                ShowGhost(_ghostPink, pinkOffset);
                _ghostsVisible = true;
                yield return new WaitForSeconds(rgbShiftDuration * Random.Range(0.3f, 0.7f));
            }

            yield return StartCoroutine(CharGlitchOnce());

            if (withRgbShift)
            {
                yield return new WaitForSeconds(rgbShiftDuration * Random.Range(0.5f, 1.2f));
                HideGhosts();
                _ghostsVisible = false;
            }
        }

        private IEnumerator CharGlitchOnce()
        {
            string snapshot = _text.text;
            if (string.IsNullOrEmpty(snapshot))
            {
                yield break;
            }

            int index = PickIndex(snapshot);
            if (index < 0)
            {
                yield break;
            }

            char glitchChar = CyberText.NoiseChars[Random.Range(0, CyberText.NoiseChars.Length)];
            string hex = Random.value > 0.5f ? CyberFx.CyanHex : CyberFx.PinkHex;

            string glitched = snapshot.Substring(0, index)
                            + $"<color=#{hex}>{glitchChar}</color>"
                            + snapshot.Substring(index + 1);
            _text.text = glitched;

            yield return new WaitForSeconds(glitchDuration * Random.Range(0.8f, 1.4f));

            if (_text != null && _text.text == glitched)
            {
                _text.text = snapshot;
            }
        }

        private int PickIndex(string text)
        {
            var candidates = new List<int>();
            bool inTag = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (c == '<')
                {
                    inTag = true;
                    continue;
                }
                if (c == '>')
                {
                    inTag = false;
                    continue;
                }
                if (inTag)
                {
                    continue;
                }

                if (c == ' ' || c == '\n')
                {
                    continue;
                }
                if (excludeHyphen && c == '-')
                {
                    continue;
                }
                candidates.Add(i);
            }
            if (candidates.Count == 0)
            {
                return -1;
            }
            return candidates[Random.Range(0, candidates.Count)];
        }

        private (Vector2 cyan, Vector2 pink) RandomShiftOffsets()
        {
            float angle = CyberFx.RandomPhase();
            float distance = rgbShiftOffset * Random.Range(0.6f, 1.3f);
            var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            return (offset, -offset);
        }

        private void ShowGhost(TextMeshProUGUI ghost, Vector2 offset)
        {
            if (ghost == null)
            {
                return;
            }
            ghost.text = _text.text;
            ghost.rectTransform.anchoredPosition = _ghostBasePos + offset;
            ghost.color = ghost.color.WithAlpha(rgbShiftAlpha);
        }

        private void HideGhosts()
        {
            CyberText.SetAlpha(_ghostCyan, 0f);
            CyberText.SetAlpha(_ghostPink, 0f);
        }

        private void CreateGhosts()
        {
            if (_text is not TextMeshProUGUI uiText)
            {
                return;
            }

            _ghostBasePos = uiText.rectTransform.anchoredPosition;
            _ghostCyan = CreateGhost("FX_GhostCyan", uiText, CyberFx.Cyan);
            _ghostPink = CreateGhost("FX_GhostPink", uiText, CyberFx.Pink);
        }

        private static TextMeshProUGUI CreateGhost(string name, TextMeshProUGUI src, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(src.transform.parent, false);
            go.transform.SetSiblingIndex(src.transform.GetSiblingIndex());

            var srt = src.rectTransform;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = srt.anchorMin;
            rt.anchorMax = srt.anchorMax;
            rt.pivot = srt.pivot;
            rt.sizeDelta = srt.sizeDelta;
            rt.anchoredPosition = srt.anchoredPosition;

            var ghost = go.AddComponent<TextMeshProUGUI>();
            ghost.font = src.font;
            ghost.fontSharedMaterial = src.fontSharedMaterial;
            ghost.fontSize = src.fontSize;
            ghost.fontStyle = src.fontStyle;
            ghost.alignment = src.alignment;
            ghost.enableWordWrapping = src.enableWordWrapping;
            ghost.overflowMode = src.overflowMode;
            ghost.raycastTarget = false;
            ghost.overrideColorTags = true;
            ghost.text = src.text;
            ghost.color = color.WithAlpha(0f);
            return ghost;
        }

        private void RestoreText() { if (_text != null) { _text.text = _original; } }
    }
}
