using System.Collections;
using UnityEngine;

namespace CrackShot
{
    [RequireComponent(typeof(Rigidbody))]
    public class BallController : MonoBehaviour
    {
        [SerializeField] private float shotPowerMultiplier = 10f;
        [SerializeField] private float maxShotPower = 60f;
        [SerializeField] private float stopVelocityThreshold = 0.1f;
        [SerializeField] private float stopCheckDelay = 0.2f;
        [SerializeField] private LayerMask outOfBoundsLayer;

        private const float FallbackSurfaceRadius = 0.5f;

        public bool IsMoving { get; private set; }
        public bool IsActive { get; private set; } = true;
        public float SurfaceRadius { get; private set; }

        private Rigidbody _rb;
        private Vector3 _savedPosition;
        private Quaternion _savedRotation;
        private Vector3 _prevPosition;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            var sc = GetComponent<SphereCollider>();
            SurfaceRadius = sc != null ? sc.radius * transform.lossyScale.x : FallbackSurfaceRadius;
            SaveState();
        }

        private void FixedUpdate()
        {
            if (!IsMoving)
            {
                return;
            }
            GateJudge.Instance?.CheckGateDuringFlight(this, _prevPosition, transform.position);
            _prevPosition = transform.position;
        }

        public void Shoot(Vector3 dragDelta)
        {
            if (!IsActive)
            {
                return;
            }
            _prevPosition = transform.position;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(Vector3.ClampMagnitude(-dragDelta * shotPowerMultiplier, maxShotPower), ForceMode.Impulse);
            IsMoving = true;
            StartCoroutine(WaitForStop());
        }

        private IEnumerator WaitForStop()
        {
            yield return new WaitForSeconds(stopCheckDelay);
            float sqrThreshold = stopVelocityThreshold * stopVelocityThreshold;
            while (_rb.velocity.sqrMagnitude > sqrThreshold)
            {
                yield return null;
            }

            if (_rb.isKinematic)
            {
                IsMoving = false;
                yield break;
            }

            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            IsMoving = false;
            GateJudge.Instance?.OnBallStopped(this);
        }

        public void SaveState()
        {
            _savedPosition = transform.position;
            _savedRotation = transform.rotation;
        }

        public void Freeze()
        {
            if (!_rb.isKinematic)
            {
                _rb.velocity = Vector3.zero;
                _rb.angularVelocity = Vector3.zero;
            }
            _rb.isKinematic = true;
        }

        public void Unfreeze()
        {
            _rb.isKinematic = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        public void RestoreState()
        {
            Unfreeze();
            transform.position = _savedPosition;
            transform.rotation = _savedRotation;
            IsMoving = false;
            IsActive = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & outOfBoundsLayer) == 0)
            {
                return;
            }
            IsActive = false;
            IsMoving = false;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            GateJudge.Instance?.OnBallOutOfBounds(this);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(Tags.Ball))
            {
                GateJudge.Instance?.OnBallCollision(this, collision.gameObject.GetComponent<BallController>());
            }
        }

        public Vector3 SavedPosition => _savedPosition;
    }
}
