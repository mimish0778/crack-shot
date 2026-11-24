using System;

namespace CrackShot
{
    // GATE（残り 2 つの球を結ぶ線分）の判定に使う幾何計算。UnityEngine に依存しないので単体テストできる
    public static class GateGeometry
    {
        public const float ParallelEpsilon = 0.0001f;

        // XZ 平面上で、線分 P1-P2（球の移動）が線分 P3-P4（GATE）と交わるかを返す。端点上も交差として扱う
        public static bool SegmentsIntersect(
            float p1x, float p1z, float p2x, float p2z,
            float p3x, float p3z, float p4x, float p4z)
        {
            float d1x = p2x - p1x, d1z = p2z - p1z;
            float d2x = p4x - p3x, d2z = p4z - p3z;
            float cross = d1x * d2z - d1z * d2x;
            if (Math.Abs(cross) < ParallelEpsilon)
            {
                return false;
            }
            float t = ((p3x - p1x) * d2z - (p3z - p1z) * d2x) / cross;
            float u = ((p3x - p1x) * d1z - (p3z - p1z) * d1x) / cross;
            return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
        }

        // 打つ球の番号から、GATE を作る残り 2 つの球の番号を返す
        public static (int a, int b) GateBallIndices(int shooterIndex, int ballCount)
            => ((shooterIndex + 1) % ballCount, (shooterIndex + 2) % ballCount);
    }
}
