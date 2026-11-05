using System.Collections;
using System.Linq;
using Game.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game.Tests
{
    // Play Mode checks that hold for every screen: each state applies without errors, and
    // same-role controls keep identical on-screen size in every state.
    // Set SceneName to the project's game scene and add it to the build settings.
    public sealed class UiConsistencyTests
    {
        private const string SceneName = "Game";
        private const float Tolerance = 0.5f;

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            UiMotion.Instant = true;
            yield return SceneManager.LoadSceneAsync(SceneName);
        }

        [UnityTearDown]
        public IEnumerator Restore()
        {
            UiMotion.Instant = false;
            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryState_Applies_WithoutErrors()
        {
            foreach (var screen in Screens())
            {
                foreach (var state in screen.StateNames)
                {
                    screen.Apply(state);
                    yield return null;
                    LogAssert.NoUnexpectedReceived();
                }
            }
        }

        [UnityTest]
        public IEnumerator SameRoleControls_HaveIdenticalSize_InEveryState()
        {
            foreach (var screen in Screens())
            {
                foreach (var state in screen.StateNames)
                {
                    screen.Apply(state);
                    yield return null;
                    Canvas.ForceUpdateCanvases();

                    var groups = Object.FindObjectsByType<UiRole>()
                        .Where(r => r.isActiveAndEnabled)
                        .GroupBy(r => r.Role);

                    foreach (var group in groups)
                    {
                        var sizes = group.Select(ScreenSize).ToList();
                        var first = sizes[0];
                        foreach (var size in sizes)
                        {
                            Assert.That(Vector2.Distance(size, first), Is.LessThanOrEqualTo(Tolerance),
                                $"{screen.ScreenName} / {state}: role '{group.Key}' has sizes {first} and {size}");
                        }
                    }
                }
            }
        }

        private static IScreenStates[] Screens() =>
            Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IScreenStates>()
                .ToArray();

        // World-space size, so a scaled-down disabled button fails the test.
        private static Vector2 ScreenSize(UiRole role)
        {
            var rect = (RectTransform)role.transform;
            var scale = rect.lossyScale;
            return new Vector2(rect.rect.width * scale.x, rect.rect.height * scale.y);
        }
    }
}
