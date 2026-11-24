using NUnit.Framework;

namespace CrackShot.Tests
{
    public class ScoreRulesTests
    {
        [TestCase(-6, "CONDOR")]
        [TestCase(-4, "CONDOR")]
        [TestCase(-3, "ALBATROSS")]
        [TestCase(-2, "EAGLE")]
        [TestCase(-1, "BIRDIE")]
        [TestCase(0, "PAR")]
        [TestCase(1, "BOGEY")]
        [TestCase(2, "DOUBLE BOGEY")]
        [TestCase(3, "TRIPLE BOGEY")]
        [TestCase(4, "+4")]
        [TestCase(12, "+12")]
        public void LabelFollowsGolfTerms(int diff, string expected)
        {
            Assert.That(ScoreRules.GetLabel(diff), Is.EqualTo(expected));
        }

        [Test]
        public void ShotCountStopsAtTheMaximum()
        {
            Assert.That(ScoreRules.AddShot(0), Is.EqualTo(1));
            Assert.That(ScoreRules.AddShot(98), Is.EqualTo(99));
            Assert.That(ScoreRules.AddShot(99), Is.EqualTo(99));
        }

        [TestCase(0f, "000.000")]
        [TestCase(1.5f, "001.500")]
        [TestCase(12.3456f, "012.345")]
        [TestCase(999.999f, "999.999")]
        [TestCase(5000f, "999.999")]
        [TestCase(-3f, "000.000")]
        public void TimeIsFormattedAsSecondsAndMilliseconds(float time, string expected)
        {
            Assert.That(ScoreRules.FormatTime(time), Is.EqualTo(expected));
        }

        [Test]
        public void RankingScorePutsShotsBeforeTime()
        {
            Assert.That(ScoreRules.CalcRankingScore(3, 12.345f), Is.EqualTo(3_012_345f));
            Assert.That(ScoreRules.CalcRankingScore(2, 999f), Is.LessThan(ScoreRules.CalcRankingScore(3, 0f)),
                "打数が少なければ、タイムが遅くても上位");
            Assert.That(ScoreRules.CalcRankingScore(3, 10f), Is.LessThan(ScoreRules.CalcRankingScore(3, 11f)),
                "同じ打数ならタイムが速いほうが上位");
        }

        [Test]
        public void RankingTimeIsClampedToTheBoardRange()
        {
            Assert.That(ScoreRules.CalcRankingScore(1, 2000f), Is.EqualTo(1_999_999f));
            Assert.That(ScoreRules.CalcRankingScore(1, -1f), Is.EqualTo(1_000_000f));
        }

        [Test]
        public void FirstRecordIsAlwaysANewBest()
        {
            // 記録が無いとき、ScoreManager は int.MaxValue と float.MaxValue を自己ベストとして渡す
            Assert.That(ScoreRules.IsNewBest(99, 999f, int.MaxValue, float.MaxValue), Is.True);
        }

        [Test]
        public void FewerShotsBeatTheBestEvenWhenSlower()
        {
            Assert.That(ScoreRules.IsNewBest(2, 60f, 3, 10f), Is.True);
            Assert.That(ScoreRules.IsNewBest(4, 1f, 3, 10f), Is.False);
        }

        [Test]
        public void SameShotsNeedAFasterTime()
        {
            Assert.That(ScoreRules.IsNewBest(3, 9.5f, 3, 10f), Is.True);
            Assert.That(ScoreRules.IsNewBest(3, 10f, 3, 10f), Is.False);
            Assert.That(ScoreRules.IsNewBest(3, 10.5f, 3, 10f), Is.False);
        }

        [TestCase(0, 0)]
        [TestCase(1, 0)]
        [TestCase(2, 1)]
        [TestCase(3, 2)]
        [TestCase(4, 0)]
        public void OnlyTheLastTwoStagesHaveRankingBoards(int stageIndex, int expectedBoard)
        {
            Assert.That(ScoreRules.GetRankingBoardNo(stageIndex), Is.EqualTo(expectedBoard));
        }
    }
}
