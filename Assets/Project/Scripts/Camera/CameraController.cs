using UnityEngine;

namespace CrackShot
{
    public class CameraController : Singleton<CameraController>
    {
        [SerializeField] private float minVerticalAngle = 5f;
        [SerializeField] private float maxVerticalAngle = 89f;
        [SerializeField] private float minZoomDistance = 5f;
        [SerializeField] private float maxZoomDistance = 40f;
        [SerializeField] private float defaultZoomDistance = 20f;
        [SerializeField] private float defaultPitch = 30f;
        [SerializeField] private float defaultYaw = 0f;
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private float resetSpeed = 8f;

        private float RotationSpeed => SettingsManager.Instance?.RotationSpeed ?? SettingsManager.DefaultRotationSpeedValue;
        private float ZoomSpeed => SettingsManager.Instance?.ZoomSpeed ?? SettingsManager.DefaultZoomSpeedValue;

        public bool IsRotating { get; private set; }

        private bool InvertX => SettingsManager.Instance?.InvertX ?? false;
        private bool InvertY => SettingsManager.Instance?.InvertY ?? false;

        private const float ResetCompleteEpsilon = 0.1f;

        private float _yaw, _pitch, _distance;
        private float _targetYaw, _targetPitch, _targetDistance;
        private bool _smoothReset;

        private Vector3 _targetPos;
        private Vector2 _lastMousePos;

        protected override void OnAwake()
        {
            _pitch = defaultPitch;
            _yaw = defaultYaw;
            _distance = defaultZoomDistance;
        }

        private void LateUpdate()
        {
            var ball = BallSelector.Instance?.CurrentBall;
            if (ball != null)
            {
                _targetPos = Vector3.Lerp(_targetPos, ball.transform.position, Time.deltaTime * followSpeed);
            }

            if (GameInput.RotatePressed)
            {
                _lastMousePos = GameInput.PointerPosition;
                IsRotating = true;
                _smoothReset = false;
            }
            if (GameInput.RotateHeld)
            {
                Vector2 d = (Vector2)GameInput.PointerPosition - _lastMousePos;
                float invertX = InvertX ? -1f : 1f;
                float invertY = InvertY ? -1f : 1f;
                _yaw += d.x * RotationSpeed * Time.deltaTime * invertX;
                _pitch -= d.y * RotationSpeed * Time.deltaTime * invertY;
                _pitch = Mathf.Clamp(_pitch, minVerticalAngle, maxVerticalAngle);
                _lastMousePos = GameInput.PointerPosition;
            }
            if (GameInput.RotateReleased)
            {
                IsRotating = false;
            }

            _distance = Mathf.Clamp(_distance - GameInput.ZoomDelta * ZoomSpeed, minZoomDistance, maxZoomDistance);

            if (GameInput.ResetView)
            {
                _targetYaw = defaultYaw;
                _targetPitch = defaultPitch;
                _targetDistance = defaultZoomDistance;
                _smoothReset = true;
            }

            if (GameInput.TopView)
            {
                _targetYaw = _yaw;
                _targetPitch = maxVerticalAngle;
                _targetDistance = maxZoomDistance;
                _smoothReset = true;
            }

            if (_smoothReset)
            {
                _yaw = Mathf.LerpAngle(_yaw, _targetYaw, Time.deltaTime * resetSpeed);
                _pitch = Mathf.Lerp(_pitch, _targetPitch, Time.deltaTime * resetSpeed);
                _distance = Mathf.Lerp(_distance, _targetDistance, Time.deltaTime * resetSpeed);

                if (Mathf.Abs(_pitch - _targetPitch) < ResetCompleteEpsilon &&
                    Mathf.Abs(_distance - _targetDistance) < ResetCompleteEpsilon)
                {
                    _smoothReset = false;
                }
            }

            var rot = Quaternion.Euler(_pitch, _yaw, 0f);
            transform.position = _targetPos + rot * new Vector3(0, 0, -_distance);
            transform.LookAt(_targetPos);
        }
    }
}
