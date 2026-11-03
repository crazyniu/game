using NUnit.Framework;
using PerchSort.V5.Game;

namespace PerchSort.V5.Tests
{
    public sealed class V6MapTests
    {
        [Test]
        public void NewProfileHasOneCurrentNodeAndLockedLaterWorlds()
        {
            var profile = new V5Profile();
            Assert.That(V6MapModel.State(profile, 1), Is.EqualTo(V6NodeState.Current));
            Assert.That(V6MapModel.State(profile, 2), Is.EqualTo(V6NodeState.Locked));
            Assert.That(V6MapModel.WorldOpen(profile, 2), Is.False);
            Assert.That(V6MapModel.CurrentLevel(profile), Is.EqualTo(1));
        }
        [Test]
        public void ClearedCurrentAndOpenNodesRemainDistinctAndWorldGateNeedsPreviousFinale()
        {
            var profile = new V5Profile { HighestUnlocked = 12 };
            for (int i = 1; i <= 8; i++) profile.Stars[i] = 2;
            Assert.That(V6MapModel.State(profile, 8), Is.EqualTo(V6NodeState.Cleared));
            Assert.That(V6MapModel.State(profile, 9), Is.EqualTo(V6NodeState.Current));
            Assert.That(V6MapModel.State(profile, 10), Is.EqualTo(V6NodeState.Open));
            Assert.That(V6MapModel.State(profile, 11), Is.EqualTo(V6NodeState.Locked));
            profile.Stars[9] = profile.Stars[10] = 3;
            Assert.That(V6MapModel.WorldOpen(profile, 2), Is.True);
            Assert.That(V6MapModel.State(profile, 11), Is.EqualTo(V6NodeState.Current));
            Assert.That(V6MapModel.State(profile, 12), Is.EqualTo(V6NodeState.Open));
            Assert.That(V6MapModel.State(profile, 13), Is.EqualTo(V6NodeState.Locked));
        }
        [Test]
        public void TestModeShowsAllClearedWithoutMutatingActualRecords()
        {
            var profile = new V5Profile(new V5DevOptions { unlockAllLevels = true, grantTestStock = true });
            for (int i = 1; i <= 40; i++)
            {
                Assert.That(V6MapModel.State(profile, i), Is.EqualTo(V6NodeState.Cleared));
                Assert.That(profile.BestStars(i), Is.EqualTo(3)); Assert.That(profile.Stars[i], Is.Zero);
            }
            Assert.That(profile.HighestUnlocked, Is.EqualTo(1));
            Assert.That(V6MapModel.CurrentLevel(profile), Is.EqualTo(40));
        }
        [Test]
        public void MapWorldFollowsTheScrolledPositionAcrossEveryBoundary()
        {
            for (int level = 1; level <= 40; level++)
            {
                Assert.That(V6MapModel.WorldAt(V6MapModel.Position(level).y), Is.EqualTo((level - 1) / 10 + 1));
                if (level > 1) Assert.That(V6MapModel.Position(level).y, Is.LessThan(V6MapModel.Position(level - 1).y));
            }
        }
        [Test]
        public void BestMovesKeepsTheMinimumAcrossReplayAndWorseStarResults()
        {
            var profile = new V5Profile(); profile.RecordBestMoves(1, 9); profile.RecordBestMoves(1, 7); profile.RecordBestMoves(1, 12);
            Assert.That(profile.BestMoves[1], Is.EqualTo(7)); Assert.That(profile.BestMoves[2], Is.EqualTo(-1));
        }
        [Test]
        public void ZeroCountedMoveWinRemainsARealBestScore()
        {
            var profile = new V5Profile(); profile.RecordBestMoves(1, 0); profile.RecordBestMoves(1, 4);
            Assert.That(profile.BestMoves[1], Is.Zero);
        }
    }
}
