using NUnit.Framework;

namespace CrackShot.Tests
{
    public class EaseTests
    {
        private static readonly System.Func<float, float>[] Curves =
        {
            Ease.InQuad, Ease.OutQuad, Ease.OutCubic, Ease.InOutQuad,
        };

        [Test]
        public void EveryCurveStartsAtZeroAndEndsAtOne()
        {
            foreach (var curve in Curves)
            {
                Assert.That(curve(0f), Is.EqualTo(0f).Within(1e-6f));
                Assert.That(curve(1f), Is.EqualTo(1f).Within(1e-6f));
            }
        }

        [Test]
        public void InOutQuadIsHalfwayAtTheMidpoint()
        {
            Assert.That(Ease.InOutQuad(0.5f), Is.EqualTo(0.5f).Within(1e-6f));
        }

        [Test]
        public void InCurvesStartSlowAndOutCurvesStartFast()
        {
            Assert.That(Ease.InQuad(0.25f), Is.LessThan(0.25f));
            Assert.That(Ease.OutQuad(0.25f), Is.GreaterThan(0.25f));
            Assert.That(Ease.OutCubic(0.25f), Is.GreaterThan(Ease.OutQuad(0.25f)));
        }
    }
}
