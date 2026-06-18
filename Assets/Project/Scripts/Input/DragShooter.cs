using UnityEngine;

namespace CrackShot
{
    [RequireComponent(typeof(LineRenderer))]
    public class DragShooter : MonoBehaviour
    {
        [Header("Drag Arrow")]
        [SerializeField] private float maxDragDistance = 7f;
        [SerializeField] private float dragPlaneY = 0.5f;
        [Tooltip("ドラッグ開始時に球を掴める最大距離。クリック位置と球の距離がこれ以上なら掴まない。")]
        [SerializeField] private float grabMaxDistance = 1.5f;
        [Tooltip("ショットが成立する最小ドラッグ量。これ未満は誤操作としてキャンセル扱いにする。")]
        [SerializeField] private float minDragToShoot = 0.05f;
        [SerializeField] private int arrowSegments = 20;
        [SerializeField] private float arrowWidthStart = 0.2f;
        [SerializeField] private float arrowWidthEnd = 0.05f;

        [Header("Shot Volume")]
        [SerializeField] [Range(0f, 1f)] private float minShotVolumeRatio = 0.2f;

        [Header("Cancel Zone")]
        [SerializeField] private float cancelRadius = 0.8f;
        [SerializeField] private float cancelAnimSpeed = 10f;
        [SerializeField] private int cancelSegments = 48;
        [SerializeField] private float cancelRingWidth = 0.2f;

        private Camera _cam;
        private bool _isDragging;
        private Vector3 _dragStart, _dragCurrent;
        private BallController _currentBall;
        private Plane _dragPlane;
        private DragShotEffects _fx;

        private void Awake()
        {
            _fx = new DragShotEffects(GetComponent<LineRenderer>(), arrowSegments, arrowWidthStart, arrowWidthEnd,
                cancelSegments, cancelRingWidth, cancelAnimSpeed);
        }

        private void Start()
        {
            _cam = Camera.main;
            _dragPlane = new Plane(Vector3.up, new Vector3(0, dragPlaneY, 0));
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsControllable())
            {
                return;
            }
            if (CameraController.Instance != null && CameraController.Instance.IsRotating)
            {
                return;
            }

            if (GameInput.SelectPressed)
            {
                if (BallSelector.Instance == null || !BallSelector.Instance.SelectedThisFrame)
                {
                    TryStartDrag();
                }
            }
            else if (GameInput.SelectHeld && _isDragging)
            {
                UpdateDrag();
            }
            else if (GameInput.SelectReleased && _isDragging)
            {
                ReleaseDrag();
            }

            UpdateCancelZone();
        }

        private void TryStartDrag()
        {
            if (BallSelector.Instance == null)
            {
                return;
            }
            _currentBall = BallSelector.Instance.CurrentBall;
            if (_currentBall == null || _currentBall.IsMoving)
            {
                return;
            }

            Ray ray = _cam.ScreenPointToRay(GameInput.PointerPosition);
            if (!_dragPlane.Raycast(ray, out float enter))
            {
                return;
            }

            Vector3 hit = ray.GetPoint(enter);
            if (Vector3.Distance(hit, _currentBall.transform.position) >= grabMaxDistance)
            {
                return;
            }

            _dragStart = _currentBall.transform.position;
            _dragCurrent = hit;
            _isDragging = true;
            GameManager.Instance.ChangeState(GameManager.GameState.Aiming);
        }

        private bool IsInCancelZone()
        {
            if (_currentBall == null)
            {
                return false;
            }
            Vector3 drag = _dragCurrent - _currentBall.transform.position;
            drag.y = 0f;
            return drag.magnitude <= cancelRadius;
        }

        private void UpdateCancelZone()
        {
            if (!_isDragging || _currentBall == null)
            {
                _fx.HideCancelZone();
                return;
            }
            _fx.UpdateCancelZone(_currentBall.transform.position, cancelRadius, IsInCancelZone());
        }

        private void UpdateDrag()
        {
            Ray ray = _cam.ScreenPointToRay(GameInput.PointerPosition);
            if (!_dragPlane.Raycast(ray, out float enter))
            {
                return;
            }

            _dragCurrent = ray.GetPoint(enter);
            Vector3 drag = _dragCurrent - _dragStart;
            if (drag.magnitude > maxDragDistance)
            {
                _dragCurrent = _dragStart + drag.normalized * maxDragDistance;
            }
            _fx.DrawArrow(_dragStart, _dragCurrent, _currentBall.transform.position, _currentBall.SurfaceRadius);
        }

        private void ReleaseDrag()
        {
            _isDragging = false;
            _fx.ClearArrow();
            _fx.HideCancelZone();
            if (_currentBall == null)
            {
                return;
            }

            if (IsInCancelZone())
            {
                GameManager.Instance?.ChangeState(GameManager.GameState.Idle);
                return;
            }

            Vector3 dragDelta = _dragCurrent - _dragStart;
            if (dragDelta.magnitude < minDragToShoot)
            {
                GameManager.Instance?.ChangeState(GameManager.GameState.Idle);
                return;
            }

            GameManager.Instance?.ChangeState(GameManager.GameState.Shooting);
            ScoreManager.Instance?.AddShot();
            float dragRatio = Mathf.Clamp01(dragDelta.magnitude / maxDragDistance);
            float eased = Mathf.Sqrt(dragRatio);
            float volumeRatio = Mathf.Lerp(minShotVolumeRatio, 1f, eased);
            AudioManager.Instance?.PlayShot(volumeRatio);
            HoleGoal.Instance?.ResetForNextShot();

            int index = BallSelector.Instance.CurrentBallIndex;
            if (StageManager.Instance != null &&
                StageManager.Instance.TryGetGateBalls(index, out var gateA, out var gateB))
            {
                GateJudge.Instance?.BeginShot(_currentBall, gateA, gateB);
            }
            _currentBall.Shoot(dragDelta);
        }
    }
}
