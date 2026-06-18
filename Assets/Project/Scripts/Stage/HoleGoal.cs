using System.Collections;
using UnityEngine;

namespace CrackShot
{
    public class HoleGoal : Singleton<HoleGoal>
    {
        [SerializeField] private float teleportFadeTime = TeleportAnimator.DefaultFadeTime;
        [SerializeField] [Range(0f, 1f)] private float penaltyHoleInVolume = TeleportAnimator.DefaultRewindOutVolume;
        [SerializeField] [Range(0f, 1f)] private float penaltyTeleportReturnVolume = TeleportAnimator.DefaultRewindInVolume;

        private const float PenaltyGapBeforeIn = 0.5f;
        private const float ClearBurstHoldTime = 0.5f;

        private bool _cleared, _gatePassed, _processing;
        private BallController _targetBall;

        public bool IsCleared => _cleared;

        protected override bool ReplaceOnDuplicate => true;

        public void NotifyGatePassed(BallController ball) { _gatePassed = true; _targetBall = ball; }

        public void ResetForNextShot()
        {
            _cleared = _gatePassed = _processing = false;
            _targetBall = null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_cleared || _processing)
            {
                return;
            }
            if (!other.CompareTag(Tags.Ball))
            {
                return;
            }

            var state = GameManager.Instance?.CurrentState;
            if (state == GameManager.GameState.StageClear || state == GameManager.GameState.Failed)
            {
                return;
            }

            var ball = other.GetComponent<BallController>();
            var rb = other.GetComponent<Rigidbody>();
            if (ball == null || rb == null || !ball.IsMoving)
            {
                return;
            }
            if (_gatePassed && ball != _targetBall)
            {
                return;
            }

            _cleared = _processing = true;
            ball.Freeze();

            StartCoroutine(_gatePassed ? StageClear(ball) : PenaltyCoroutine(ball));
        }

        private IEnumerator PenaltyCoroutine(BallController ball)
        {
            GateJudge.Instance?.ForceJudged();

            var balls = new[] { ball };
            var scales = new[] { ball.transform.localScale };

            yield return StartCoroutine(TeleportAnimator.RewindCycle(
                this, balls, scales, teleportFadeTime, gapBeforeIn: PenaltyGapBeforeIn,
                onOut: () => AudioManager.Instance?.PlayFailedTeleportOut(penaltyHoleInVolume),
                onIn: () => AudioManager.Instance?.PlayFailedTeleportIn(penaltyTeleportReturnVolume),
                onHidden: () =>
                {
                    if (GameManager.Instance?.CurrentState == GameManager.GameState.StageClear)
                    {
                        return false;
                    }
                    GameManager.Instance?.AnnounceFailedShot(FailReason.NotThroughGate);
                    ball.Unfreeze();
                    return true;
                }));

            if (GameManager.Instance?.CurrentState == GameManager.GameState.StageClear)
            {
                yield break;
            }
            StageManager.Instance?.RewindBalls();
        }

        private IEnumerator StageClear(BallController ball)
        {
            ScoreManager.Instance?.StopTimer();
            GameManager.Instance?.ChangeState(GameManager.GameState.StageClear);
            AudioManager.Instance?.PlayTeleport();
            ScoreManager.Instance?.SaveBestScore(GameManager.Instance?.SelectedStageIndex ?? 0);

            ball.Freeze();

            Vector3 startScale = ball.transform.localScale;
            yield return StartCoroutine(TeleportAnimator.Out(ball.transform, startScale, teleportFadeTime));
            TeleportAnimator.BurstAndHide(ball);
            yield return new WaitForSeconds(ClearBurstHoldTime);
            GameManager.Instance?.LoadResultScene();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            var sc = GetComponent<SphereCollider>();
            if (sc != null)
            {
                Gizmos.DrawWireSphere(transform.position, sc.radius);
            }
        }
    }
}
