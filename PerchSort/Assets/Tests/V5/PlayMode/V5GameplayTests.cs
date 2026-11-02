using System.Collections;
using NUnit.Framework;
using PerchSort.V5.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PerchSort.V5.Tests
{
    public sealed class V5GameplayTests
    {
        private readonly string[] keys = { V5Progress.CoinsKey, V5Progress.TutorialKey, V5Progress.MutedKey, "v5.bestStars.1" };
        private readonly bool[] existed = new bool[4];
        private readonly int[] values = new int[4];
        private bool previousInstant;
        private V5GameRoot root;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = V5Motion.Instant;
            V5Motion.Instant = true;
            for (int i = 0; i < keys.Length; i++)
            {
                existed[i] = PlayerPrefs.HasKey(keys[i]);
                values[i] = PlayerPrefs.GetInt(keys[i]);
                PlayerPrefs.DeleteKey(keys[i]);
            }
            PlayerPrefs.SetInt(V5Progress.CoinsKey, 100);
            yield return SceneManager.LoadSceneAsync("V5", LoadSceneMode.Single);
            for (int i = 0; i < 60; i++)
            {
                root = Object.FindAnyObjectByType<V5GameRoot>();
                if (root != null && root.IsReady) break;
                yield return null;
            }
            Assert.That(root, Is.Not.Null);
            Assert.That(root.IsReady, Is.True);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var empty = SceneManager.CreateScene("V5 test cleanup");
            SceneManager.SetActiveScene(empty);
            var scene = SceneManager.GetSceneByName("V5");
            if (scene.IsValid() && scene.isLoaded) yield return SceneManager.UnloadSceneAsync(scene);
            for (int i = 0; i < keys.Length; i++)
            {
                if (existed[i]) PlayerPrefs.SetInt(keys[i], values[i]);
                else PlayerPrefs.DeleteKey(keys[i]);
            }
            PlayerPrefs.Save();
            V5Motion.Instant = previousInstant;
        }

        [UnityTest]
        public IEnumerator AuthoredMovesOpenWinAndPersistTheReward()
        {
            int before = root.Coins;
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(root.Win.IsOpen, Is.False);
            Assert.That(root.TryMove(2, 0), Is.True);
            Assert.That(root.Win.gameObject.activeInHierarchy, Is.True);
            Assert.That(root.Session.MoveCount, Is.EqualTo(2));
            Assert.That(root.Coins, Is.EqualTo(before + 25));
            Assert.That(PlayerPrefs.GetInt(V5Progress.CoinsKey), Is.EqualTo(before + 25));
            Assert.That(PlayerPrefs.GetInt(V5Progress.StarsKey(1)), Is.EqualTo(3));
            Assert.That(root.IsBusy, Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator BranchPointerClicksSelectThenTransferAndHome()
        {
            var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left };
            root.Board.Branches[0].OnPointerClick(pointer);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(0));
            root.Board.Branches[1].OnPointerClick(pointer);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(root.Session.MoveCount, Is.EqualTo(1));
            Assert.That(root.Session.State.Branches[0].Birds.Count, Is.EqualTo(3));
            Assert.That(root.Session.State.Branches[1].Birds, Is.Empty);
            Assert.That(root.Session.State.HomedSpecies[0].Id, Is.EqualTo("sky"));
            yield return null;
            Assert.That(root.Board.GetComponentsInChildren<BirdView>().Length, Is.EqualTo(4));
        }

        [UnityTest]
        public IEnumerator AnimatedTransferLocksInputAndUndoCancelsTheCommittedMove()
        {
            V5Motion.Instant = false;
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(root.IsBusy, Is.True);
            Assert.That(root.Session.MoveCount, Is.EqualTo(1));
            Assert.That(root.TapBranch(2), Is.False);
            Assert.That(root.Session.SelectedBranch, Is.EqualTo(-1));
            Assert.That(root.Undo(), Is.True);
            Assert.That(root.Session.MoveCount, Is.Zero);
            Assert.That(root.Session.State.HomedSpecies, Is.Empty);
            yield return new WaitForSecondsRealtime(1.4f);
            Assert.That(root.IsBusy, Is.False);
            Assert.That(root.Win.IsOpen, Is.False);
            Assert.That(root.Coins, Is.EqualTo(100));
            Assert.That(root.Board.GetComponentsInChildren<BirdView>().Length, Is.EqualTo(8));
        }
    }
}
