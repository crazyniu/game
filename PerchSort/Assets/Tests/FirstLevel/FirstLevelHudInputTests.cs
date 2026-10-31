#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.FirstLevelRuntime;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace PerchSort.FirstLevelTests
{
    // Routes a programmatic pointer through the real HUD; this is not a physical-device test.
    public sealed class FirstLevelHudInputTests
    {
        private Scene loadedScene;
        private GameObject previousAudio;

        [UnityTest]
        public IEnumerator PointerPauseDuringHomingWaitsForWholeCommittedMove()
        {
            previousAudio = GameObject.Find("First-level audio");
            loadedScene = EditorSceneManager.LoadSceneInPlayMode("Assets/Scenes/FirstLevel.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
            FirstLevelEntry entry = null;
            float deadline = Time.realtimeSinceStartup + 5f;
            while (Time.realtimeSinceStartup < deadline)
            {
                entry = Object.FindFirstObjectByType<FirstLevelEntry>();
                if (entry != null && entry.IsReady && entry.Flow != null &&
                    entry.Flow.Phase == FirstLevelPhase.Idle) break;
                yield return null;
            }
            Assert.That(entry, Is.Not.Null, "Generated FirstLevel scene must contain its runtime entry.");
            Assert.That(entry.IsReady, Is.True, "Accepted scene assets must initialize within five seconds.");
            var flow = entry.Flow;
            Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Idle));
            Assert.That(flow.Pick(1), Is.True);
            Assert.That(flow.Pick(0), Is.True);
            Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Presenting));

            var pause = entry.Hud.GetComponentsInChildren<Button>(true).Single(b => b.name == "Pause");
            Assert.That(pause.IsInteractable(), Is.True);
            foreach (var group in pause.GetComponentsInParent<CanvasGroup>(true))
            {
                Assert.That(group.interactable, Is.True, group.name + " must allow Pause interaction.");
                Assert.That(group.blocksRaycasts, Is.True, group.name + " must allow Pause raycasts.");
            }
            Assert.That(Camera.main, Is.Not.Null);
            Assert.That(EventSystem.current, Is.Not.Null);
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)pause.transform;
            var point = RectTransformUtility.WorldToScreenPoint(Camera.main, rect.TransformPoint(rect.rect.center));
            for (int click = 0; click < 2; click++)
            {
                var pointer = new PointerEventData(EventSystem.current)
                    { position = point, button = PointerEventData.InputButton.Left };
                var hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pointer, hits);
                Assert.That(hits, Is.Not.Empty, "Pause screen point must hit the actual HUD.");
                Assert.That(hits[0].gameObject.GetComponentInParent<Button>(), Is.SameAs(pause),
                    "The top raycast must route to Pause, without an overlay blocking it.");
                Assert.That(ExecuteEvents.ExecuteHierarchy(hits[0].gameObject, pointer,
                    ExecuteEvents.pointerClickHandler), Is.SameAs(pause.gameObject));
                Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Presenting),
                    "Pause requests must wait for the whole Move/Home/Retire presentation.");
                Assert.That(flow.Current.MoveCount, Is.EqualTo(1));
            }
            deadline = Time.realtimeSinceStartup + 5f;
            while (flow.Phase == FirstLevelPhase.Presenting && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(flow.Phase, Is.EqualTo(FirstLevelPhase.Paused));
            Assert.That(flow.Current.MoveCount, Is.EqualTo(1), "Double Pause must not create another move.");
            Assert.That(flow.Current.Sequence, Is.EqualTo(1));
            Assert.That(flow.Current.Branches[0].Retired, Is.True);
            Assert.That(flow.Current.HomedSpecies, Is.EqualTo(new[] { 0 }));
            Assert.That(flow.Current.IsWon, Is.False);
        }

        [UnityTearDown]
        public IEnumerator UnloadOnlyTheTestSceneAndItsNewAudio()
        {
            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                SceneManager.CreateScene("FirstLevelHudInputCleanup-" + Time.frameCount);
                yield return SceneManager.UnloadSceneAsync(loadedScene);
            }
            var audio = GameObject.Find("First-level audio");
            if (audio != null && audio != previousAudio) Object.Destroy(audio);
            yield return null;
        }
    }
}
#endif
