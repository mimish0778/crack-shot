using UnityEngine;

namespace CrackShot
{
    [RequireComponent(typeof(BallController))]
    public class CyberBallEffect : MonoBehaviour
    {
        [Header("Ring")]
        [SerializeField] private float ringRadius = 0.52f;
        [SerializeField] private float ringWidth = 0.18f;
        [SerializeField] private int ringSegments = 32;

        [Header("Trail")]
        [SerializeField] private float trailTime = 0.25f;
        [SerializeField] private float trailStartWidth = 0.2f;
        [SerializeField] private float trailMinSpeed = 10f;

        private BallController _ball;
        private Rigidbody _rb;
        private Material _ringMat;
        private LineRenderer _ring;
        private Transform _ringTransform;
        private Camera _cam;
        private Material _trailMat;
        private TrailRenderer _trail;
        private Transform _trailAnchor;

        private void Awake()
        {
            _ball = GetComponent<BallController>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Start()
        {
            _cam = Camera.main;
            HideOriginalRenderer();
            SetupRing();
            SetupTrail();
        }

        private void HideOriginalRenderer()
        {
            var r = GetComponentInChildren<Renderer>();
            if (r == null)
            {
                return;
            }
            r.sharedMaterial = CyberFx.CreateUnlitMaterial(Color.black);
        }

        private void SetupRing()
        {
            _ringMat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);

            var go = new GameObject("BallRing");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _ringTransform = go.transform;

            _ring = CyberFx.AddLine(go, _ringMat, worldSpace: false, loop: true, width: ringWidth);
            _ring.textureMode = LineTextureMode.Tile;

            CyberFx.SetCirclePositions(_ring, ringSegments, ringRadius, xzPlane: false);
        }

        private void SetupTrail()
        {
            _trailMat = CyberFx.CreateTubeMaterial(CyberFx.Cyan);

            var go = new GameObject("Trail");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            _trailAnchor = go.transform;

            _trail = go.AddComponent<TrailRenderer>();
            CyberFx.SetupTrail(_trail, _trailMat, trailTime, trailStartWidth, minVertexDistance: 0.05f);
            _trail.emitting = false;
        }

        private void Update()
        {
            if (_ringMat == null || _trailMat == null)
            {
                return;
            }

            Color col = CyberFx.StageOrCurrentColor;

            if (_cam != null && _ringTransform != null)
            {
                _ringTransform.rotation = _cam.transform.rotation;
            }

            CyberFx.ApplyColor(_ringMat, col);
            CyberFx.ApplyColor(_trailMat, col);
            CyberFx.ApplyTrailGradient(_trail, col);

            bool fastEnough = _rb != null && _rb.velocity.magnitude >= trailMinSpeed;
            _trail.emitting = _ball.IsMoving && fastEnough;

            if (_ball.IsMoving && fastEnough && _rb.velocity.sqrMagnitude > 0.01f)
            {
                _trailAnchor.localPosition = -_rb.velocity.normalized * _ball.SurfaceRadius;
            }
            else
            {
                _trailAnchor.localPosition = Vector3.zero;
            }
        }
    }
}
