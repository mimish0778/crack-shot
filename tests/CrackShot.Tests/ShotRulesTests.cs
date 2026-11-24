using NUnit.Framework;

namespace CrackShot.Tests
{
    public class ShotRulesTests
    {
        private const float MaxDrag = 7f;
        private const float MinRatio = 0.2f;

        [Test]
        public void NoDragGivesTheMinimumVolume()
        {
            Assert.That(ShotRules.ShotVolumeRatio(0f, MaxDrag, MinRatio), Is.EqualTo(MinRatio).Within(1e-6f));
        }

        [Test]
        public void FullDragGivesFullVolume()
        {
            Assert.That(ShotRules.ShotVolumeRatio(MaxDrag, MaxDrag, MinRatio), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(ShotRules.ShotVolumeRatio(MaxDrag * 2f, MaxDrag, MinRatio), Is.EqualTo(1f).Within(1e-6f));
        }

        [Test]
        public void QuarterDragIsHalfwayBecauseOfTheSquareRootCurve()
        {
            float expected = MinRatio + (1f - MinRatio) * 0.5f;
            Assert.That(ShotRules.ShotVolumeRatio(MaxDrag / 4f, MaxDrag, MinRatio), Is.EqualTo(expected).Within(1e-6f));
        }

        [Test]
        public void VolumeGrowsWithDragDistance()
        {
            float previous = 0f;
            for (int i = 0; i <= 10; i++)
            {
                float volume = ShotRules.ShotVolumeRatio(MaxDrag * i / 10f, MaxDrag, MinRatio);
                Assert.That(volume, Is.GreaterThanOrEqualTo(previous));
                previous = volume;
            }
        }
    }
}
