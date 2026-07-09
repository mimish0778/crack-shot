using UnityEngine;
using TMPro;

namespace CrackShot
{
    public class MetallicShimmer : MonoBehaviour
    {
        public enum Tier { Birdie, Eagle, Albatross, Condor }

        public static Tier TierForScore(int scoreToPar) => scoreToPar switch
        {
            <= ScoreManager.CondorThreshold => Tier.Condor,
            ScoreManager.AlbatrossThreshold => Tier.Albatross,
            ScoreManager.EagleThreshold => Tier.Eagle,
            _ => Tier.Birdie,
        };

        private static readonly Color[] Highlights =
        {
            new Color(1f, 0.88f, 0.55f),
            new Color(1f, 1f, 1f),
            new Color(1f, 1f, 0.72f),
            new Color(0.85f, 0.55f, 1f),
        };
        private static readonly Color[] Mids =
        {
            new Color(0.72f, 0.45f, 0.2f),
            new Color(0.72f, 0.72f, 0.76f),
            new Color(1f, 0.82f, 0.1f),
            new Color(0.48f, 0.05f, 0.72f),
        };
        private static readonly Color[] Darks =
        {
            new Color(0.28f, 0.16f, 0.06f),
            new Color(0.28f, 0.28f, 0.32f),
            new Color(0.45f, 0.32f, 0f),
            new Color(0.14f, 0f, 0.22f),
        };

        private TMP_Text _text;
        private Color _highlight;
        private Color _mid;
        private Color _dark;

        private Color _topBase, _bottomBaseStart, _bottomBaseEnd;

        private int _cachedCharCount = -1;
        private int _visibleCount;
        private float[] _charCenterX;
        private int[] _charVertexIndex;
        private int[] _charMaterialIndex;

        private float _pos = SweepStartPos;
        private float _speed = InitialSweepSpeed;
        private float _waitTimer;
        private bool _sweeping = true;

        private const float ShimmerWidth = 0.28f;
        private const float SlowSpeed = 0.75f;
        private const float SweepInterval = 3.0f;
        private const float SweepStartPos = -0.3f;
        private const float SweepEndPos = 1.3f;
        private const float InitialSweepSpeed = 1.1f;

        public void Play(Tier tier)
        {
            _text = GetComponent<TMP_Text>();
            _highlight = Highlights[(int)tier];
            _mid = Mids[(int)tier];
            _dark = Darks[(int)tier];

            _topBase = Color.Lerp(_mid, _highlight, 0.5f);
            _bottomBaseStart = Color.Lerp(_dark, _mid, 0.45f);
            _bottomBaseEnd = Color.Lerp(_mid, _highlight, 0.35f);

            _text.color = Color.white;
            _text.enableVertexGradient = false;

            _pos = SweepStartPos;
            _speed = InitialSweepSpeed;
            _sweeping = true;
            _waitTimer = 0f;
            _cachedCharCount = -1;
            enabled = true;
        }

        private void Update()
        {
            if (_text == null)
            {
                return;
            }

            if (_sweeping)
            {
                _pos += Time.deltaTime * _speed;
                if (_pos > SweepEndPos)
                {
                    _pos = SweepEndPos;
                    _sweeping = false;
                    _waitTimer = 0f;
                    _speed = SlowSpeed;
                }
            }
            else
            {
                _waitTimer += Time.deltaTime;
                if (_waitTimer >= SweepInterval)
                {
                    _pos = SweepStartPos;
                    _sweeping = true;
                }
            }

            ApplyVertexColors();
        }

        private void ApplyVertexColors()
        {
            var info = _text.textInfo;

            if (info == null || info.characterCount != _cachedCharCount)
            {
                BuildLayoutCache();
                info = _text.textInfo;
                if (info == null || _visibleCount == 0)
                {
                    return;
                }
            }

            for (int k = 0; k < _visibleCount; k++)
            {
                float dist = Mathf.Abs(_charCenterX[k] - _pos);
                float shimmer = Mathf.Pow(Mathf.Max(0f, 1f - dist / ShimmerWidth), 3f);

                Color top = Color.Lerp(_topBase, _highlight, shimmer);
                Color bot = Color.Lerp(_bottomBaseStart, _bottomBaseEnd, shimmer);

                var colors = info.meshInfo[_charMaterialIndex[k]].colors32;
                int vi = _charVertexIndex[k];
                colors[vi + 0] = bot;
                colors[vi + 1] = top;
                colors[vi + 2] = top;
                colors[vi + 3] = bot;
            }

            _text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private void BuildLayoutCache()
        {
            _text.ForceMeshUpdate();
            var info = _text.textInfo;
            _cachedCharCount = info != null ? info.characterCount : -1;
            _visibleCount = 0;
            if (info == null || info.characterCount == 0)
            {
                return;
            }

            float minX = float.MaxValue, maxX = float.MinValue;
            for (int c = 0; c < info.characterCount; c++)
            {
                if (!info.characterInfo[c].isVisible)
                {
                    continue;
                }
                float bl = info.characterInfo[c].bottomLeft.x;
                float br = info.characterInfo[c].bottomRight.x;
                if (bl < minX)
                {
                    minX = bl;
                }
                if (br > maxX)
                {
                    maxX = br;
                }
            }
            float rangeX = Mathf.Max(maxX - minX, 0.01f);

            if (_charCenterX == null || _charCenterX.Length < info.characterCount)
            {
                _charCenterX = new float[info.characterCount];
                _charVertexIndex = new int[info.characterCount];
                _charMaterialIndex = new int[info.characterCount];
            }

            for (int c = 0; c < info.characterCount; c++)
            {
                var ci = info.characterInfo[c];
                if (!ci.isVisible)
                {
                    continue;
                }
                _charCenterX[_visibleCount] = ((ci.bottomLeft.x + ci.bottomRight.x) * 0.5f - minX) / rangeX;
                _charVertexIndex[_visibleCount] = ci.vertexIndex;
                _charMaterialIndex[_visibleCount] = ci.materialReferenceIndex;
                _visibleCount++;
            }
        }
    }
}
