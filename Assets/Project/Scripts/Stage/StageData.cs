using UnityEngine;
using UnityEngine.Serialization;

namespace CrackShot
{
    [CreateAssetMenu(fileName = "StageData", menuName = "Crack_Shot/StageData")]
    public class StageData : ScriptableObject
    {
        public const int BallCount = 3;

        [FormerlySerializedAs("stageName")]
        public string StageName = "Stage 1";
        [FormerlySerializedAs("par")]
        public int Par = 3;
        [FormerlySerializedAs("ballStartPositions")]
        [Tooltip("ステージオブジェクト原点からの相対座標。実際のスポーン位置は stageObject.position + この値。")]
        public Vector3[] BallStartPositions = new Vector3[BallCount]
        {
            new Vector3( 0f, 0.5f, 0f),
            new Vector3( 3f, 0.5f, 3f),
            new Vector3(-3f, 0.5f, 3f),
        };

        [Tooltip("背景演出（CyberSpaceBackground）の Z 方向オフセット。ステージごとの奥行きの見え方を調整する。")]
        public float BackgroundZOffset = 20f;

        private void OnValidate()
        {
            if (BallStartPositions.Length != BallCount)
            {
                System.Array.Resize(ref BallStartPositions, BallCount);
            }
        }
    }
}
