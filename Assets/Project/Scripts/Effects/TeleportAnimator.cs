using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrackShot
{
    public static class TeleportAnimator
    {
        private const float InDurationMultiplier = 1.5f;
        private const float SettleTime = 0.3f;

        public const float DefaultFadeTime = 0.08f;
        public const float DefaultRewindOutVolume = 0.45f;
        public const float DefaultRewindInVolume = 0.25f;

        public static IEnumerator RewindCycle(
            MonoBehaviour host, IReadOnlyList<BallController> balls, IReadOnlyList<Vector3> scales,
            float fadeTime, float gapBeforeIn, Action onOut, Action onIn, Func<bool> onHidden = null)
        {
            onOut?.Invoke();
            yield return host.StartCoroutine(OutAndHide(host, balls, scales, fadeTime));

            if (onHidden != null && !onHidden())
            {
                yield break;
            }

            if (gapBeforeIn > 0f)
            {
                yield return new WaitForSeconds(gapBeforeIn);
            }

            yield return host.StartCoroutine(ResetAndIn(host, balls, scales, fadeTime * InDurationMultiplier, onIn));
            yield return new WaitForSeconds(SettleTime);
        }

        public static IEnumerator OutAndHide(
            MonoBehaviour host, IReadOnlyList<BallController> balls, IReadOnlyList<Vector3> scales, float fadeTime)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                if (balls[i] != null && balls[i].gameObject.activeSelf)
                {
                    host.StartCoroutine(Out(balls[i].transform, scales[i], fadeTime));
                }
            }

            yield return new WaitForSeconds(fadeTime);

            foreach (var ball in balls)
            {
                if (ball != null)
                {
                    BurstAndHide(ball);
                }
            }
        }

        public static IEnumerator ResetAndIn(
            MonoBehaviour host, IReadOnlyList<BallController> balls, IReadOnlyList<Vector3> scales,
            float inDuration, Action onBeforeBurst = null)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                if (balls[i] == null)
                {
                    continue;
                }
                ResetToSpawn(balls[i]);
                host.StartCoroutine(In(balls[i].transform, scales[i], inDuration));
            }

            yield return new WaitForSeconds(inDuration);

            onBeforeBurst?.Invoke();

            foreach (var ball in balls)
            {
                if (ball != null)
                {
                    CyberTeleportEffect.Instance?.Burst(ball.transform.position);
                }
            }
        }

        public static IEnumerator Out(Transform target, Vector3 startScale, float duration)
        {
            yield return CyberFx.Tween(duration, t =>
            {
                float squashX = Mathf.Lerp(1f, 1.6f, Mathf.Min(t * 2f, 1f));
                target.localScale = new Vector3(
                    startScale.x * squashX * Mathf.Lerp(1f, 0f, t),
                    startScale.y * Mathf.Lerp(1f, 0f, t),
                    startScale.z * Mathf.Lerp(1f, 0f, t));
            });
            target.localScale = Vector3.zero;
        }

        public static IEnumerator In(Transform target, Vector3 targetScale, float duration)
        {
            yield return CyberFx.Tween(duration, t =>
            {
                float sc = Mathf.Sin(t * Mathf.PI) * 0.5f + Mathf.SmoothStep(0f, 1f, t);
                target.localScale = targetScale * sc;
            });
            target.localScale = targetScale;
        }

        public static void BurstAndHide(BallController ball)
        {
            CyberTeleportEffect.Instance?.Burst(ball.transform.position);
            ball.gameObject.SetActive(false);
        }

        public static void ResetToSpawn(BallController ball)
        {
            ball.transform.position = ball.SavedPosition;
            ball.transform.localScale = Vector3.zero;
            ball.gameObject.SetActive(true);
        }
    }
}
