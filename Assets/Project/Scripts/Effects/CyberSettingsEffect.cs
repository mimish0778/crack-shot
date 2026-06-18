using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CrackShot
{
    public class CyberSettingsEffect : Singleton<CyberSettingsEffect>
    {
        [Header("References")]
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private Slider[] sliders;

        [Header("Title")]
        [SerializeField] private string titleLabel = "[ SYS.CONFIG_v1.0 ]";
        [SerializeField] private float titleFontSize = 10f;

        [Header("Corner Brackets")]
        [SerializeField] private float borderWidth = 2f;
        [SerializeField] private float cornerLength = 22f;

        [Header("Scanlines")]
        [SerializeField] [Range(0f, 0.2f)] private float scanlineAlpha = 0.06f;
        [SerializeField] private float scanSpeed = 28f;

        [Header("Data Strip")]
        [SerializeField] private float dataScrollSpeed = 35f;

        [Header("Tab Transition")]
        [SerializeField] private float transitionDuration = 0.55f;
        [SerializeField] [Range(0.1f, 1f)] private float transitionExponent = 0.65f;

        [Header("Open Animation")]
        [SerializeField] private float sweepDuration = 0.22f;

        private readonly List<Image> _corners = new();
        private readonly List<Image> _sliderFills = new();

        private RawImage _scanOverlay;
        private Texture2D _scanTex;
        private float _scanOffset;

        private TMP_Text _dataText;
        private TMP_Text _titleText;
        private string _dataSource;
        private float _dataCharOffset;

        private Color _activeColor = CyberFx.Pink;
        private Color _fromColor = CyberFx.Pink;
        private Color _toColor = CyberFx.Pink;
        private float _transitionT = 1f;

        private bool _built;

        private void Start() => Build();

        private void Update()
        {
            if (!_built)
            {
                return;
            }
            AdvanceTransition();
            ApplyActiveColor();
            ScrollScanlines();
            ScrollData();
        }

        public Color ActiveColor => _activeColor;

        public bool IsInsideWindow(Transform t) =>
            windowRect != null && t.IsChildOf(windowRect);

        public void SetTab(bool isAudio, bool immediate = false)
        {
            Color target = isAudio ? CyberFx.Pink : CyberFx.Cyan;
            if (immediate)
            {
                _activeColor = _fromColor = _toColor = target;
                _transitionT = 1f;
            }
            else
            {
                _fromColor = _activeColor;
                _toColor = target;
                _transitionT = 0f;
            }
        }

        private void AdvanceTransition()
        {
            if (_transitionT >= 1f)
            {
                return;
            }
            _transitionT = Mathf.Min(1f, _transitionT + Time.deltaTime / transitionDuration);

            float c = CyberFx.SigmoidEase01(_transitionT, transitionExponent);
            _activeColor = CyberFx.LerpViaWhite(_fromColor, _toColor, c);
        }

        private void ApplyActiveColor()
        {
            Color col = _activeColor;

            SetCornerColor(col);

            foreach (var fill in _sliderFills)
            {
                if (fill)
                {
                    fill.color = col;
                }
            }

            if (_titleText)
            {
                _titleText.color = col;
            }

            if (_dataText)
            {
                _dataText.color = col.WithAlpha(0.35f);
            }

            if (_scanOverlay)
            {
                _scanOverlay.color = col.WithAlpha(1f);
            }
        }

        private void Build()
        {
            if (_built || windowRect == null)
            {
                return;
            }
            _built = true;

            BuildScanlines();
            BuildCornerBrackets();
            BuildTitleBar();
            BuildDataStrip();
            CacheSliderFills();
        }

        private void BuildScanlines()
        {
            const int h = 8;
            _scanTex = new Texture2D(1, h, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
            };
            for (int i = 0; i < h; i++)
            {
                _scanTex.SetPixel(0, i, new Color(1f, 1f, 1f, i == 0 ? scanlineAlpha : 0f));
            }
            _scanTex.Apply();

            var go = new GameObject("FX_Scanlines", typeof(RawImage));
            go.transform.SetParent(windowRect, false);
            _scanOverlay = go.GetComponent<RawImage>();
            _scanOverlay.texture = _scanTex;
            _scanOverlay.color = CyberFx.Pink;
            _scanOverlay.raycastTarget = false;

            var rt = _scanOverlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.transform.SetSiblingIndex(1);
        }

        private void ScrollScanlines()
        {
            if (_scanOverlay == null || windowRect == null)
            {
                return;
            }
            float h = Mathf.Max(1f, windowRect.rect.height);
            _scanOffset += scanSpeed * Time.deltaTime / h;
            _scanOverlay.uvRect = new Rect(0, _scanOffset, 1, 1);
        }

        private void BuildCornerBrackets()
        {
            float[,] c = { { 0, 1 }, { 1, 1 }, { 0, 0 }, { 1, 0 } };
            for (int i = 0; i < 4; i++)
            {
                float ax = c[i, 0], ay = c[i, 1];
                AddBracketArm($"BracketH_{i}", ax, ay, new Vector2(cornerLength, borderWidth));
                AddBracketArm($"BracketV_{i}", ax, ay, new Vector2(borderWidth, cornerLength));
            }
            foreach (var img in _corners)
            {
                img.transform.SetAsLastSibling();
            }
        }

        private void AddBracketArm(string name, float ax, float ay, Vector2 size)
        {
            var go = new GameObject(name, typeof(Image));
            go.transform.SetParent(windowRect, false);
            var img = go.GetComponent<Image>();
            img.color = CyberFx.Pink;
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(ax, ay);
            rt.sizeDelta = size;
            rt.anchoredPosition = Vector2.zero;
            _corners.Add(img);
        }

        private void BuildTitleBar()
        {
            var go = new GameObject("FX_Title", typeof(RectTransform));
            go.transform.SetParent(windowRect, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-8f, 26f);
            rt.anchoredPosition = new Vector2(0f, -2f);

            _titleText = go.AddComponent<TextMeshProUGUI>();
            _titleText.text = titleLabel;
            _titleText.fontSize = titleFontSize;
            _titleText.color = CyberFx.Pink;
            _titleText.alignment = TextAlignmentOptions.Center;
            _titleText.fontStyle = FontStyles.Bold;
            _titleText.raycastTarget = false;

            go.AddComponent<CyberGlitchEffect>();
            go.transform.SetAsLastSibling();
        }

        private void BuildDataStrip()
        {
            var rng = new System.Random(7);
            var sb = new System.Text.StringBuilder(160);
            for (int i = 0; i < 160; i++)
            {
                if (i % 5 == 4)
                {
                    sb.Append(' ');
                    continue;
                }
                sb.Append("0123456789ABCDEF"[rng.Next(16)]);
            }
            _dataSource = sb.ToString();

            var container = new GameObject("FX_DataStripContainer", typeof(RectTransform), typeof(RectMask2D));
            container.transform.SetParent(windowRect, false);

            var crt = container.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 0f);
            crt.anchorMax = new Vector2(1f, 0f);
            crt.pivot = new Vector2(0.5f, 0f);
            crt.sizeDelta = new Vector2(-16f, 18f);
            crt.anchoredPosition = new Vector2(0f, 2f);

            var go = new GameObject("FX_DataText", typeof(RectTransform));
            go.transform.SetParent(container.transform, false);

            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            _dataText = go.AddComponent<TextMeshProUGUI>();
            _dataText.text = _dataSource;
            _dataText.fontSize = 9f;
            _dataText.color = CyberFx.Pink.WithAlpha(0.35f);
            _dataText.alignment = TextAlignmentOptions.Left;
            _dataText.raycastTarget = false;
            _dataText.overflowMode = TextOverflowModes.Overflow;
            _dataText.enableWordWrapping = false;
            container.transform.SetAsLastSibling();
        }

        private void ScrollData()
        {
            if (_dataText == null || string.IsNullOrEmpty(_dataSource))
            {
                return;
            }
            _dataCharOffset += dataScrollSpeed * Time.deltaTime;
            int index = Mathf.FloorToInt(_dataCharOffset) % _dataSource.Length;
            _dataText.text = _dataSource[index..] + _dataSource[..index];
        }

        private void CacheSliderFills()
        {
            if (sliders == null)
            {
                return;
            }
            foreach (var s in sliders)
            {
                if (s == null)
                {
                    continue;
                }
                var fill = s.fillRect?.GetComponent<Image>();
                if (fill != null)
                {
                    _sliderFills.Add(fill);
                }
            }
        }

        private GameObject _sweepBar;

        public void PlayOpenAnimation()
        {
            if (!_built)
            {
                Build();
            }
            StopAllCoroutines();
            if (_sweepBar != null)
            {
                Destroy(_sweepBar);
                _sweepBar = null;
            }
            StartCoroutine(GlitchIn());
        }

        private IEnumerator GlitchIn()
        {
            for (int i = 0; i < 2; i++)
            {
                SetCornerColor(Color.white);
                yield return new WaitForSeconds(0.04f);
                SetCornerColor(_activeColor);
                yield return new WaitForSeconds(0.05f);
            }

            yield return StartCoroutine(SweepScanBar());

            SetCornerColor(Color.white);
            yield return new WaitForSeconds(0.04f);
            SetCornerColor(_activeColor);
        }

        private IEnumerator SweepScanBar()
        {
            var go = new GameObject("FX_SweepBar", typeof(Image));
            _sweepBar = go;
            go.transform.SetParent(windowRect, false);
            go.transform.SetAsLastSibling();

            var img = go.GetComponent<Image>();
            img.raycastTarget = false;

            var rt = img.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, 5f);
            rt.anchoredPosition = Vector2.zero;

            float height = Mathf.Max(1f, windowRect.rect.height);

            yield return CyberFx.Tween(sweepDuration, progress =>
            {
                rt.anchoredPosition = new Vector2(0f, -height * progress);
                img.color = _activeColor.WithAlpha(Mathf.Lerp(0.55f, 0f, progress * progress));
                rt.sizeDelta = new Vector2(0f, Mathf.Lerp(5f, 2f, progress));
            });

            Destroy(go);
            _sweepBar = null;
        }

        private void SetCornerColor(Color col)
        {
            foreach (var img in _corners)
            {
                if (img)
                {
                    img.color = col;
                }
            }
        }
    }
}
