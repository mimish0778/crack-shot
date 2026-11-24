using System;

namespace CrackShot
{
    // スコアの呼び名、タイム表示、ランキング用スコア、自己ベストの判定。UnityEngine に依存しないので単体テストできる
    public static class ScoreRules
    {
        public const float MaxTime = 999.999f;
        public const int MaxShots = 99;

        public const int CondorThreshold = -4;
        public const int AlbatrossThreshold = -3;
        public const int EagleThreshold = -2;
        public const int BirdieThreshold = -1;

        public const int MaxNamedOverParDiff = 3;

        // unityroom のスコアボードに送るステージ（ステージ番号 2 と 3 がボード 1 と 2）
        public const int FirstRankedStageIndex = 2;
        public const int RankingBoardCount = 2;

        private const int MaxRankingMillis = 999 * 1000 + 999;

        // パーとの差（打数 - パー）からゴルフの呼び名を返す
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

        public static int AddShot(int shotCount) => Math.Min(shotCount + 1, MaxShots);

        // 秒を「000.000」形式にする（ミリ秒未満は切り捨て、0〜999.999 に収める）
        public static string FormatTime(float time)
        {
            int totalMillis = Clamp((int)(time * 1000f), 0, (int)(MaxTime * 1000f));
            return $"{totalMillis / 1000:000}.{totalMillis % 1000:000}";
        }

        // 打数を優先し、同じ打数ならタイムが短いほど小さくなるランキング用スコア（小さいほど上位）
        public static float CalcRankingScore(int shots, float time)
        {
            int totalMillis = Clamp((int)Math.Round(time * 1000f), 0, MaxRankingMillis);
            return shots * 1_000_000 + (totalMillis / 1000) * 1_000 + totalMillis % 1000;
        }

        // 今回の記録が自己ベストを更新するか（打数が少ない、または同じ打数でタイムが短い）
        public static bool IsNewBest(int shots, float time, int bestShots, float bestTime)
            => shots < bestShots || (shots == bestShots && time < bestTime);

        // ステージ番号に対応するスコアボード番号。ランキング対象外なら 0
        public static int GetRankingBoardNo(int stageIndex)
        {
            int boardNo = stageIndex - FirstRankedStageIndex + 1;
            return boardNo >= 1 && boardNo <= RankingBoardCount ? boardNo : 0;
        }

        private static int Clamp(int value, int min, int max) => value < min ? min : value > max ? max : value;
    }
}
