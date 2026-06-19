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

        private const float MaxTime = 999.999f;
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

        // 「秒.ミリ秒」表記（分は使わない・最大 999.999）。unityroom ランキングの桁構成と揃える。
        public static string GetTimeString(float time)
        {
            int totalMillis = Mathf.Clamp((int)(time * 1000f), 0, (int)(MaxTime * 1000f));
            return $"{totalMillis / 1000:000}.{totalMillis % 1000:000}";
        }

        // unityroom ランキング用の合成スコア（3桁区切り整数表示・昇順）。
        // 桁構成: 打数×1,000,000 + 秒×1,000 + ミリ秒 → カンマ区切りで [打数],[秒],[ミリ秒] と読める。
        // 例: 2打 / 7.027秒 → 2,007,027。「打数が少ない順 → 同打数はタイムが短い順」で小さいほど上位(HighScoreAsc)。
        // float送信のため打数15以下はミリ秒まで厳密、16以上は末尾がわずかに丸まる（下位順位のみで実用上無視可）。
        private const int MaxRankingMillis = 999 * 1000 + 999; // 999.999秒（約16分）で頭打ち。
        public static float CalcRankingScore(int shots, float time)
        {
            int totalMillis = Mathf.Clamp(Mathf.RoundToInt(time * 1000f), 0, MaxRankingMillis);
            return shots * 1_000_000 + (totalMillis / 1000) * 1_000 + totalMillis % 1000;
        }

        // unityroom のスコアボードは2枠のみ使用。ステージ3・4(index 2・3)だけをボード1・2へ送る。
        // 昇順ボード(HighScoreAsc)なのでサーバー側が自動でベストのみ保持する。
        private const int FirstRankedStageIndex = 2; // ステージ3 → ボード1、ステージ4 → ボード2。
        public void SubmitRanking(int stageIndex)
        {
            int boardNo = stageIndex - FirstRankedStageIndex + 1;
            if (boardNo < 1 || boardNo > 2)
            {
                return; // ステージ1・2はランキング対象外。
            }
            UnityroomApiClient.Instance?.SendScore(
                boardNo, CalcRankingScore(ShotCount, ElapsedTime), ScoreboardWriteMode.HighScoreAsc);
        }
    }
}
