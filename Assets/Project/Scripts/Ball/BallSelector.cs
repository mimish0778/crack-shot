using UnityEngine;

namespace CrackShot
{
    public class BallSelector : Singleton<BallSelector>
    {
        public int CurrentBallIndex { get; private set; }
        public BallController CurrentBall => _balls != null ? _balls[CurrentBallIndex] : null;
        public bool SelectedThisFrame { get; private set; }

        private const float BallSelectRadius = 1.0f;

        private BallController[] _balls;
        private Camera _cam;

        private void Start()
        {
            _cam = Camera.main;
        }

        private void Update()
        {
            SelectedThisFrame = false;
            if (GameManager.Instance == null || !GameManager.Instance.IsControllable())
            {
                return;
            }
            if (GameInput.SelectPressed)
            {
                TrySelectBall();
            }

            if (GameManager.Instance?.CurrentState != GameManager.GameState.Aiming)
            {
                if (GameInput.NextBall)
                {
                    CycleToNextBall(1);
                }
                if (GameInput.PrevBall)
                {
                    CycleToNextBall(-1);
                }
            }
        }

        private void CycleToNextBall(int direction)
        {
            if (_balls == null)
            {
                return;
            }
            int next = (CurrentBallIndex + direction + _balls.Length) % _balls.Length;
            SelectBall(next);
        }

        private void TrySelectBall()
        {
            if (_cam == null || _balls == null)
            {
                return;
            }

            Ray ray = _cam.ScreenPointToRay(GameInput.PointerPosition);
            for (int i = 0; i < _balls.Length; i++)
            {
                if (_balls[i] == null || _balls[i].IsMoving || i == CurrentBallIndex)
                {
                    continue;
                }

                Vector3 ballPos = _balls[i].transform.position;
                Vector3 toBall = ballPos - ray.origin;
                float dot = Vector3.Dot(toBall, ray.direction);
                Vector3 closest = ray.origin + ray.direction * Mathf.Max(0f, dot);
                float dist = Vector3.Distance(closest, ballPos);

                if (dist < BallSelectRadius)
                {
                    SelectBall(i);
                    break;
                }
            }
        }

        private void SelectBall(int index)
        {
            CurrentBallIndex = index;
            SelectedThisFrame = true;
            AudioManager.Instance?.PlayBallChange();
        }

        public void InitializeBalls(BallController[] balls)
        {
            _balls = balls;
        }

        public void SaveAndIdle()
        {
            if (_balls == null)
            {
                return;
            }
            foreach (var b in _balls)
            {
                b.SaveState();
            }

            GameManager.Instance?.ChangeState(GameManager.GameState.Idle);
        }

        public void RewindToSavedState()
        {
            if (_balls == null)
            {
                return;
            }
            foreach (var b in _balls)
            {
                b.RestoreState();
            }

            GameManager.Instance?.ChangeState(GameManager.GameState.Idle);
        }
    }
}
