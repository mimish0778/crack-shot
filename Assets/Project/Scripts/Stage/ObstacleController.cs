using UnityEngine;

namespace CrackShot
{
    public class ObstacleController : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveDistance = 10f;
        [SerializeField] private float moveSpeed = 3f;

        private Vector3 _startPosition;
        private Vector3 _moveDirNormalized;
        private float _phase;

        private void Start()
        {
            _startPosition = transform.position;

            Vector3 dir = moveDirection;
            dir.y = 0f;
            _moveDirNormalized = dir.normalized;
        }

        private void FixedUpdate()
        {
            _phase += Time.fixedDeltaTime * moveSpeed;

            transform.position = _startPosition
                + _moveDirNormalized * (Mathf.Sin(_phase) * moveDistance * 0.5f);
        }
    }
}
