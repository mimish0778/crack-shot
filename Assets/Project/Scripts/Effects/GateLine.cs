using UnityEngine;

namespace CrackShot
{
    [RequireComponent(typeof(LineRenderer))]
    public class GateLine : MonoBehaviour
    {
        [SerializeField] private float lineWidth = 0.2f;
        [SerializeField] private float lineHeightOffset = 0.1f;
        [SerializeField] private float blinkSpeed = 2f;

        private LineRenderer _line;
        private Material _mat;

        private void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 2;
            _line.startWidth = _line.endWidth = lineWidth;
            _line.useWorldSpace = true;
            _line.enabled = false;

            _mat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);
            _line.material = _mat;
        }

        private void Update()
        {
            if (BallSelector.Instance == null || StageManager.Instance == null)
            {
                _line.enabled = false;
                return;
            }

            int index = BallSelector.Instance.CurrentBallIndex;
            if (!StageManager.Instance.TryGetGateBalls(index, out var ballA, out var ballB))
            {
                _line.enabled = false;
                return;
            }

            Vector3 a = ballA.transform.position + Vector3.up * lineHeightOffset;
            Vector3 b = ballB.transform.position + Vector3.up * lineHeightOffset;
            Vector3 dir = (b - a).normalized;
            float r = ballA.SurfaceRadius;
            _line.SetPosition(0, a + dir * r);
            _line.SetPosition(1, b + dir * -r);
            _line.enabled = true;

            float alpha = Mathf.Sin(Time.time * blinkSpeed * Mathf.PI) * 0.4f + 0.6f;
            CyberFx.ApplyColor(_mat, CyberFx.Current.WithAlpha(alpha));
        }
    }
}
