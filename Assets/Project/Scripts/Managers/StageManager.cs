using UnityEngine;

namespace CrackShot
{
    public class StageManager : Singleton<StageManager>
    {
        public const int BallCount = StageData.BallCount;

        [SerializeField] private StageData[] stageDatas;
        [SerializeField] private GameObject[] stageObjects;
        [SerializeField] private GameObject ballPrefab;

        public StageData CurrentStageData { get; private set; }
        public BallController[] Balls { get; private set; }

        private void Start()
        {
            GameManager.Instance?.ChangeState(GameManager.GameState.Idle);
            int index = GameManager.Instance != null ? GameManager.Instance.SelectedStageIndex : 0;
            LoadStage(index);
        }

        public void LoadStage(int index)
        {
            if (stageDatas == null || index >= stageDatas.Length)
            {
                return;
            }

            CurrentStageData = stageDatas[index];

            GameObject stageGo = null;
            if (stageObjects != null)
            {
                for (int i = 0; i < stageObjects.Length; i++)
                {
                    if (stageObjects[i] == null)
                    {
                        continue;
                    }
                    stageObjects[i].SetActive(i == index);
                    if (i == index)
                    {
                        stageGo = stageObjects[i];
                    }
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.TotalStageCount = stageDatas.Length;
            }

            Vector3 origin = stageGo != null ? stageGo.transform.position : Vector3.zero;
            SpawnBalls(origin);
            ScoreManager.Instance?.SetPar(CurrentStageData.Par);
            ScoreManager.Instance?.StartTimer();

            if (stageGo != null)
            {
                CyberSpaceBackground.Instance?.SetCenter(stageGo.transform.position, CurrentStageData.BackgroundZOffset);
                CyberStageEffect.Instance?.ApplyTo(stageGo);
            }
        }

        private void SpawnBalls(Vector3 origin)
        {
            Balls = new BallController[BallCount];
            var layout = CurrentStageData.BallStartPositions;
            for (int i = 0; i < BallCount; i++)
            {
                var go = Instantiate(ballPrefab, origin + layout[i], Quaternion.identity);
                go.name = $"Ball_{i}";
                Balls[i] = go.GetComponent<BallController>();
            }
            BallSelector.Instance?.InitializeBalls(Balls);
        }

        public void RewindBalls() => BallSelector.Instance?.RewindToSavedState();

        public bool TryGetGateBalls(int shooterIndex, out BallController ballA, out BallController ballB)
        {
            ballA = ballB = null;
            if (Balls == null || Balls.Length != BallCount)
            {
                return false;
            }
            ballA = Balls[(shooterIndex + 1) % BallCount];
            ballB = Balls[(shooterIndex + 2) % BallCount];
            return ballA != null && ballB != null;
        }
    }
}
