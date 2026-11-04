using NUnit.Framework;
using PerchSort.Game;

namespace PerchSort.EditModeTests
{
    public sealed class MapTests
    {
        [Test]
        public void NewProfileHasOneCurrentNodeAndLockedLaterWorlds()
        {
            var profile = new GameProfile();
            Assert.That(MapModel.State(profile, 1), Is.EqualTo(LevelNodeState.Current));
            Assert.That(MapModel.State(profile, 2), Is.EqualTo(LevelNodeState.Locked));
            Assert.That(MapModel.WorldOpen(profile, 2), Is.False);
            Assert.That(MapModel.CurrentLevel(profile), Is.EqualTo(1));
        }
        [Test]
        public void ClearedCurrentAndOpenNodesRemainDistinctAndWorldGateNeedsPreviousFinale()
        {
            var profile = new GameProfile { HighestUnlocked = 12 };
            for (int i = 1; i <= 8; i++) profile.Stars[i] = 2;
            Assert.That(MapModel.State(profile, 8), Is.EqualTo(LevelNodeState.Cleared));
            Assert.That(MapModel.State(profile, 9), Is.EqualTo(LevelNodeState.Current));
            Assert.That(MapModel.State(profile, 10), Is.EqualTo(LevelNodeState.Open));
            Assert.That(MapModel.State(profile, 11), Is.EqualTo(LevelNodeState.Locked));
            profile.Stars[9] = profile.Stars[10] = 3;
            Assert.That(MapModel.WorldOpen(profile, 2), Is.True);
            Assert.That(MapModel.State(profile, 11), Is.EqualTo(LevelNodeState.Current));
            Assert.That(MapModel.State(profile, 12), Is.EqualTo(LevelNodeState.Open));
            Assert.That(MapModel.State(profile, 13), Is.EqualTo(LevelNodeState.Locked));
        }
        [Test]
        public void TestModeShowsAllClearedWithoutMutatingActualRecords()
        {
            var profile = new GameProfile(new DevOptions { unlockAllLevels = true, grantTestStock = true });
            for (int i = 1; i <= 40; i++)
            {
                Assert.That(MapModel.State(profile, i), Is.EqualTo(LevelNodeState.Cleared));
                Assert.That(profile.BestStars(i), Is.EqualTo(3)); Assert.That(profile.Stars[i], Is.Zero);
            }
            Assert.That(profile.HighestUnlocked, Is.EqualTo(1));
            Assert.That(MapModel.CurrentLevel(profile), Is.EqualTo(40));
        }
        [Test]
        public void MapWorldFollowsTheScrolledPositionAcrossEveryBoundary()
        {
            for (int level = 1; level <= 40; level++)
            {
                Assert.That(MapModel.WorldAt(MapModel.Position(level).y), Is.EqualTo((level - 1) / 10 + 1));
                if (level > 1) Assert.That(MapModel.Position(level).y, Is.LessThan(MapModel.Position(level - 1).y));
            }
        }
        [Test]
        public void BestMovesKeepsTheMinimumAcrossReplayAndWorseStarResults()
        {
            var profile = new GameProfile(); profile.RecordBestMoves(1, 9); profile.RecordBestMoves(1, 7); profile.RecordBestMoves(1, 12);
            Assert.That(profile.BestMoves[1], Is.EqualTo(7)); Assert.That(profile.BestMoves[2], Is.EqualTo(-1));
        }
        [Test]
        public void ZeroCountedMoveWinRemainsARealBestScore()
        {
            var profile = new GameProfile(); profile.RecordBestMoves(1, 0); profile.RecordBestMoves(1, 4);
            Assert.That(profile.BestMoves[1], Is.Zero);
        }
    }
}
