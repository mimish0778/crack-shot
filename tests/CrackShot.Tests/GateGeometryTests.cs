using NUnit.Framework;

namespace CrackShot.Tests
{
    public class GateGeometryTests
    {
        // GATE は (-1, 0) から (1, 0) の線分
        private static bool Crosses(float p1x, float p1z, float p2x, float p2z)
            => GateGeometry.SegmentsIntersect(p1x, p1z, p2x, p2z, -1f, 0f, 1f, 0f);

        [Test]
        public void BallMovingThroughTheGateCrossesIt()
        {
            Assert.That(Crosses(0f, -1f, 0f, 1f), Is.True);
        }

        [Test]
        public void BallPassingOutsideTheGateDoesNotCrossIt()
        {
            Assert.That(Crosses(2f, -1f, 2f, 1f), Is.False);
        }

        [Test]
        public void BallStoppingBeforeTheGateDoesNotCrossIt()
        {
            Assert.That(Crosses(0f, -2f, 0f, -0.5f), Is.False);
        }

        [Test]
        public void TouchingTheGateCountsAsCrossing()
        {
            Assert.That(Crosses(0f, -1f, 0f, 0f), Is.True, "移動の終点が GATE 上");
            Assert.That(Crosses(1f, -1f, 1f, 1f), Is.True, "GATE の端点を通過");
        }

        [Test]
        public void MovingParallelToTheGateDoesNotCrossIt()
        {
            Assert.That(Crosses(-2f, 0f, 2f, 0f), Is.False, "GATE と重なって平行に移動");
            Assert.That(Crosses(-2f, 1f, 2f, 1f), Is.False);
        }

        [Test]
        public void DiagonalGateIsHandled()
        {
            Assert.That(GateGeometry.SegmentsIntersect(0f, 2f, 2f, 0f, 0f, 0f, 3f, 3f), Is.True);
            Assert.That(GateGeometry.SegmentsIntersect(0f, 2f, 0.5f, 1.5f, 0f, 0f, 3f, 3f), Is.False);
        }

        [Test]
        public void ZeroLengthMoveNeverCrosses()
        {
            Assert.That(Crosses(0f, 0f, 0f, 0f), Is.False);
        }

        [TestCase(0, 1, 2)]
        [TestCase(1, 2, 0)]
        [TestCase(2, 0, 1)]
        public void GateIsMadeOfTheOtherTwoBalls(int shooter, int expectedA, int expectedB)
        {
            var (a, b) = GateGeometry.GateBallIndices(shooter, 3);
            Assert.That(a, Is.EqualTo(expectedA));
            Assert.That(b, Is.EqualTo(expectedB));
        }
    }
}
