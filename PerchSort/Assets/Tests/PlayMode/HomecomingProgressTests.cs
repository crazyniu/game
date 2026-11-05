using System.Collections;
using System.Linq;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class HomecomingProgressTests : CollectionScreenTests
    {
        private void WinLevel()
        {
            var solution = Solver.Solve(root.Session.State);
            Assert.That(solution.Status, Is.EqualTo(SolveStatus.Solved));
            foreach (var move in solution.Moves)
                Assert.That(move.Kind == MoveKind.Swivel ? root.TapSwivel(move.From) : root.TryMove(move.From, move.To), Is.True);
            Assert.That(root.Win.IsOpen, Is.True);
        }
        private void AssertCounts(int wins)
        {
            foreach (var home in root.Session.State.HomedSpecies.GroupBy(species => species.Id))
            {
                int expected = wins * home.Count();
                Assert.That(root.Collection.FlocksHome(home.Key), Is.EqualTo(expected), home.Key);
                Assert.That(ProgressStore.Load().Notifications.RewardLedger.FlocksHome(home.Key), Is.EqualTo(expected), "saved " + home.Key);
            }
            Assert.That(root.Collection.FlocksHome("crow"), Is.Zero);
            Assert.That(root.Collection.FlocksHome("berry"), Is.EqualTo(7), "Unrelated saved counts must survive.");
        }
        [UnityTest]
        public IEnumerator NormalWinPersistsEveryHomedFlockAcrossSceneReload()
        {
            root.LoadLevel(1); WinLevel(); AssertCounts(1);
            var homes = root.Session.State.HomedSpecies.Select(species => species.Id).ToArray();
            yield return Reload();
            foreach (var species in homes) Assert.That(root.Collection.FlocksHome(species), Is.EqualTo(1));
            root.LoadLevel(1); WinLevel(); AssertCounts(2);
        }
        [UnityTest]
        public IEnumerator UndoAndRedoWinningMoveDoesNotDuplicateCommittedHomecomings()
        {
            root.LoadLevel(1); WinLevel(); AssertCounts(1);
            Assert.That(root.Undo(), Is.True);
            WinLevel(); AssertCounts(1);
            yield return null;
        }
        [UnityTest]
        public IEnumerator RestartDiscardsPartialAttemptAndCountsEachNewCompletedAttemptOnce()
        {
            root.LoadLevel(1);
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(root.Session.State.IsWon, Is.False);
            Assert.That(root.Session.State.HomedSpecies.Count, Is.GreaterThan(0));
            foreach (var species in root.Session.State.HomedSpecies)
                Assert.That(root.Collection.FlocksHome(species.Id), Is.Zero);
            root.Restart(); WinLevel(); AssertCounts(1);
            root.Restart(); WinLevel(); AssertCounts(2);
            yield return null;
        }
    }
}
