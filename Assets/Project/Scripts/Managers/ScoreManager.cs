using UnityEngine;
using UnityEngine.Events;
using unityroom.Api;

namespace CrackShot
{
    public class ScoreManager : PersistentSingleton<ScoreManager>
    {
        public UnityEvent<int> OnShotCountChanged = new UnityEvent<int>();
        public UnityEvent<int> OnParChanged = new UnityEvent<int>();
        public UnityEvent<float> OnTimeChanged = new UnityEvent<float>();

        public int ShotCount { get; private set; }
        public int Par { get; private set; } = DefaultPar;
        public int ScoreToPar => ShotCount - Par;
        public float ElapsedTime { get; private set; }
        public bool IsTimerRunning { get; private set; }

        private const int DefaultPar = 3;

        public const int CondorThreshold = ScoreRules.CondorThreshold;
        public const int AlbatrossThreshold = ScoreRules.AlbatrossThreshold;
        public const int EagleThreshold = ScoreRules.EagleThreshold;
        public const int BirdieThreshold = ScoreRules.BirdieThreshold;

        public const int MaxNamedOverParDiff = ScoreRules.MaxNamedOverParDiff;

        private void Update()
        {
            if (!IsTimerRunning)
            {
                return;
            }
            ElapsedTime = Mathf.Min(ElapsedTime + Time.deltaTime, ScoreRules.MaxTime);
            OnTimeChanged.Invoke(ElapsedTime);
        }

        public void StartTimer() { ElapsedTime = 0f; IsTimerRunning = true; }
        public void StopTimer() { IsTimerRunning = false; }

        public void SetPar(int par)
        {
            Par = par;
            OnParChanged.Invoke(par);
        }

        public void AddShot()
        {
            ShotCount = ScoreRules.AddShot(ShotCount);
            OnShotCountChanged.Invoke(ShotCount);
        }

        public void AddPenalty() => AddShot();

        public string GetLabel() => GetLabel(ScoreToPar);

        public static string GetLabel(int diff) => ScoreRules.GetLabel(diff);

        public (int shot, int par, string label) GetResult() => (ShotCount, Par, GetLabel());

        private static string ScoreKey(int stageIndex) => $"BestScore_Stage{stageIndex}";
        private static string TimeKey(int stageIndex) => $"BestTime_Stage{stageIndex}";

        public void SaveBestScore(int stageIndex)
        {
            string scoreKey = ScoreKey(stageIndex);
            string timeKey = TimeKey(stageIndex);
            int bestShots = PlayerPrefs.GetInt(scoreKey, int.MaxValue);
            float bestTime = PlayerPrefs.GetFloat(timeKey, float.MaxValue);

            if (ScoreRules.IsNewBest(ShotCount, ElapsedTime, bestShots, bestTime))
            {
                PlayerPrefs.SetInt(scoreKey, ShotCount);
                PlayerPrefs.SetFloat(timeKey, ElapsedTime);
                PlayerPrefs.Save();
            }
        }

        public int GetBestScore(int stageIndex)
            => PlayerPrefs.GetInt(ScoreKey(stageIndex), -1);

        public float GetBestTime(int stageIndex)
            => PlayerPrefs.GetFloat(TimeKey(stageIndex), -1f);

        public void ResetScore()
        {
            ShotCount = 0;
            Par = DefaultPar;
            ElapsedTime = 0f;
            IsTimerRunning = false;
            OnShotCountChanged.Invoke(ShotCount);
            OnParChanged.Invoke(Par);
        }

        public static string GetTimeString(float time) => ScoreRules.FormatTime(time);

        public static float CalcRankingScore(int shots, float time) => ScoreRules.CalcRankingScore(shots, time);

        public void SubmitRanking(int stageIndex)
        {
            int boardNo = ScoreRules.GetRankingBoardNo(stageIndex);
            if (boardNo == 0)
            {
                return;
            }
            UnityroomApiClient.Instance?.SendScore(
                boardNo, CalcRankingScore(ShotCount, ElapsedTime), ScoreboardWriteMode.HighScoreAsc);
        }
    }
}
