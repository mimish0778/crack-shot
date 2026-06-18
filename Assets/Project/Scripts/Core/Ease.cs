using UnityEngine;

namespace CrackShot
{
    public static class Ease
    {
        public static float InQuad(float t) => t * t;

        public static float OutQuad(float t) => 1f - (1f - t) * (1f - t);

        public static float OutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);

        public static float InOutQuad(float t)
            => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    }
}
