using System;

namespace CrackShot
{
    // ドラッグ操作から打球を決める計算。UnityEngine に依存しないので単体テストできる
    public static class ShotRules
    {
        // 引っ張った距離から打球音の音量比を返す。弱い打球でも minRatio は鳴らし、平方根で小さい力の差を聞き分けやすくする
        public static float ShotVolumeRatio(float dragDistance, float maxDragDistance, float minRatio)
        {
            float dragRatio = Clamp01(dragDistance / maxDragDistance);
            float eased = (float)Math.Sqrt(dragRatio);
            return minRatio + (1f - minRatio) * eased;
        }

        private static float Clamp01(float value) => value < 0f ? 0f : value > 1f ? 1f : value;
    }
}
