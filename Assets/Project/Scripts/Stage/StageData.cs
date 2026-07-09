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
        public Vector3[] BallStartPositions = new Vector3[BallCount]
        {
            new Vector3( 0f, 0.5f, 0f),
            new Vector3( 3f, 0.5f, 3f),
            new Vector3(-3f, 0.5f, 3f),
        };

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
