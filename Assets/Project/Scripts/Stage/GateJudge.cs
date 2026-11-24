using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public class GateJudge : Singleton<GateJudge>
    {
        [SerializeField] private float teleportFadeTime = TeleportAnimator.DefaultFadeTime;
        [SerializeField] [Range(0f, 1f)] private float teleportOutVolume = TeleportAnimator.DefaultRewindOutVolume;
        [SerializeField] [Range(0f, 1f)] private float teleportInVolume = TeleportAnimator.DefaultRewindInVolume;

        private const float MovedThreshold = 0.1f;
        private const float SuccessHoldTime = 1.0f;
        private const float FailDelayTime = 0.5f;
        private const float RewindGapBeforeIn = 0.2f;

        private bool _judged, _collisionFailed, _passed;
        private Vector3 _guardBallA, _guardBallB;
        private BallController _shotBall;

        public void BeginShot(BallController shotBall, BallController ballA, BallController ballB)
        {
            _shotBall = shotBall;
            _guardBallA = ballA.transform.position;
            _guardBallB = ballB.transform.position;
            _judged = _collisionFailed = _passed = false;
        }

        public void CheckGateDuringFlight(BallController ball, Vector3 prev, Vector3 curr)
        {
            if (_judged || ball != _shotBall || _passed)
            {
                return;
            }
            if (!CrossedGateLine(prev, curr, _guardBallA, _guardBallB))
            {
                return;
            }
            _passed = true;
            HoleGoal.Instance?.NotifyGatePassed(ball);
        }

        public void OnBallStopped(BallController ball)
        {
            if (ball != _shotBall || _judged)
            {
                return;
            }
            if (HoleGoal.Instance != null && HoleGoal.Instance.IsCleared)
            {
                return;
            }
            StartCoroutine(JudgeGatePass());
        }

        public void OnBallCollision(BallController shotBall, BallController other)
        {
            if (_judged || shotBall != _shotBall)
            {
                return;
            }
            _collisionFailed = _judged = true;
            StartCoroutine(HandleFailed(FailReason.BallCollision));
        }

        public void OnBallOutOfBounds(BallController ball)
        {
            if (_judged)
            {
                return;
            }
            _judged = true;
            StartCoroutine(HandleFailed(FailReason.OutOfBounds));
        }

        public void ForceJudged() => _judged = true;

        private IEnumerator JudgeGatePass()
        {
            _judged = true;
            if (_collisionFailed)
            {
                yield break;
            }
            if (GameManager.Instance?.CurrentState == GameManager.GameState.StageClear)
            {
                yield break;
            }
            yield return StartCoroutine(_passed ? HandleSuccess() : HandleFailed(FailReason.DidNotPassThrough));
        }

        private static bool CrossedGateLine(Vector3 prev, Vector3 curr, Vector3 gateA, Vector3 gateB)
            => GateGeometry.SegmentsIntersect(prev.x, prev.z, curr.x, curr.z, gateA.x, gateA.z, gateB.x, gateB.z);

        private IEnumerator HandleSuccess()
        {
            GameManager.Instance?.ChangeState(GameManager.GameState.Success);
            AudioManager.Instance?.PlaySuccess();
            CyberFlashEffect.Instance?.PlaySuccess();
            yield return new WaitForSeconds(SuccessHoldTime);
            if (GameManager.Instance?.CurrentState == GameManager.GameState.StageClear)
            {
                yield break;
            }
            if (HoleGoal.Instance != null && HoleGoal.Instance.IsCleared)
            {
                yield break;
            }
            BallSelector.Instance?.SaveAndIdle();
        }

        private IEnumerator HandleFailed(FailReason reason)
        {
            if (GameManager.Instance?.CurrentState == GameManager.GameState.StageClear)
            {
                yield break;
            }
            GameManager.Instance?.AnnounceFailedShot(reason);

            yield return new WaitForSeconds(FailDelayTime);
            yield return StartCoroutine(TeleportRewindMovedBalls());
            StageManager.Instance?.RewindBalls();
        }

        private IEnumerator TeleportRewindMovedBalls()
        {
            var balls = StageManager.Instance?.Balls;
            if (balls == null)
            {
                yield break;
            }

            var movedBalls = new List<BallController>();
            var movedScales = new List<Vector3>();
            foreach (var ball in balls)
            {
                if (ball == null)
                {
                    continue;
                }
                if (Vector3.Distance(ball.transform.position, ball.SavedPosition) < MovedThreshold)
                {
                    continue;
                }
                ball.Freeze();
                movedBalls.Add(ball);
                movedScales.Add(ball.transform.localScale);
            }

            if (movedBalls.Count == 0)
            {
                yield break;
            }

            yield return StartCoroutine(TeleportAnimator.RewindCycle(
                this, movedBalls, movedScales, teleportFadeTime, gapBeforeIn: RewindGapBeforeIn,
                onOut: () => AudioManager.Instance?.PlayFailedTeleportOut(teleportOutVolume),
                onIn: () => AudioManager.Instance?.PlayFailedTeleportIn(teleportInVolume)));
        }
    }
}
