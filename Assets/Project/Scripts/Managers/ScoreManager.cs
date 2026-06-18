using UnityEngine;
using UnityEngine.Events;

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

        private const float MaxTime = 99 * 60 + 59.99f;
        private const int MaxShots = 99;
        private const int DefaultPar = 3;

        public const int CondorThreshold = -4;
        public const int AlbatrossThreshold = -3;
        public const int EagleThreshold = -2;
        public const int BirdieThreshold = -1;

        public const int MaxNamedOverParDiff = 3;

        private void Update()
        {
            if (!IsTimerRunning)
            {
                return;
            }
            ElapsedTime = Mathf.Min(ElapsedTime + Time.deltaTime, MaxTime);
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
            ShotCount = Mathf.Min(ShotCount + 1, MaxShots);
            OnShotCountChanged.Invoke(ShotCount);
        }

        public void AddPenalty() => AddShot();

        public string GetLabel() => GetLabel(ScoreToPar);

        public static string GetLabel(int diff) => diff switch
        {
            <= CondorThreshold => "CONDOR",
            AlbatrossThreshold => "ALBATROSS",
            EagleThreshold => "EAGLE",
            BirdieThreshold => "BIRDIE",
            0 => "PAR",
            1 => "BOGEY",
            2 => "DOUBLE BOGEY",
            3 => "TRIPLE BOGEY",
            _ => $"+{diff}",
        };

        public (int shot, int par, string label) GetResult() => (ShotCount, Par, GetLabel());

        private static string ScoreKey(int stageIndex) => $"BestScore_Stage{stageIndex}";
        private static string TimeKey(int stageIndex) => $"BestTime_Stage{stageIndex}";

        public void SaveBestScore(int stageIndex)
        {
            string scoreKey = ScoreKey(stageIndex);
            string timeKey = TimeKey(stageIndex);
            int current = PlayerPrefs.GetInt(scoreKey, int.MaxValue);

            if (ShotCount < current)
            {
                PlayerPrefs.SetInt(scoreKey, ShotCount);
                PlayerPrefs.SetFloat(timeKey, ElapsedTime);
                PlayerPrefs.Save();
            }
            else if (ShotCount == current)
            {
                float currentTime = PlayerPrefs.GetFloat(timeKey, float.MaxValue);
                if (ElapsedTime < currentTime)
                {
                    PlayerPrefs.SetFloat(timeKey, ElapsedTime);
                    PlayerPrefs.Save();
                }
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

        public static string GetTimeString(float time)
        {
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);
            int millis = (int)((time % 1) * 100);
            return $"{minutes:00}:{seconds:00}.{millis:00}";
        }
    }
}
