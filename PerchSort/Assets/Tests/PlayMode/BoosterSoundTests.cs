using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class BoosterSoundTests
    {
        private GameRoot root;
        private bool instant;
        private DevOptions previousOptions;
        private Component audio;
        private EventInfo playedEvent;
        private Delegate listener;
        private readonly List<string> played = new List<string>();
        private static readonly string[] Sounds = { "Hint", "Sapling", "OwlEye", "MorningBell", "Scarecrow", "SpringRain" };
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            instant = GameMotion.Instant; GameMotion.Instant = true;
            previousOptions = DevOptions.OverrideForTests; DevOptions.OverrideForTests = new DevOptions();
            SceneManager.sceneLoaded += Initialize;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            SceneManager.sceneLoaded -= Initialize;
            // The game test assembly intentionally has no direct dependency on the shared Audio assembly.
            audio = Object.FindObjectsByType<MonoBehaviour>().Single(c => c.GetType().FullName == "PerchSort.Audio.AudioManager");
            playedEvent = audio.GetType().GetEvent("EffectPlayed");
            var method = GetType().GetMethod(nameof(Heard), BindingFlags.NonPublic | BindingFlags.Instance)
                .MakeGenericMethod(playedEvent.EventHandlerType.GenericTypeArguments[0]);
            listener = Delegate.CreateDelegate(playedEvent.EventHandlerType, this, method); playedEvent.AddEventHandler(audio, listener);
        }
        private void Initialize(Scene scene, LoadSceneMode mode)
        {
            root = Object.FindAnyObjectByType<GameRoot>(); root.Initialize(false, false);
        }
        private void Heard<T>(T effect) => played.Add(effect.ToString());
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= Initialize;
            if (audio != null) playedEvent.RemoveEventHandler(audio, listener);
            var empty = SceneManager.CreateScene("Booster sound cleanup"); SceneManager.SetActiveScene(empty);
            yield return SceneManager.UnloadSceneAsync("Game"); GameMotion.Instant = instant;
            DevOptions.OverrideForTests = previousOptions;
        }
        [UnityTest] public IEnumerator SuccessfulBoostersAndUndoPlayTheirDistinctEffectOnce() => Exercise(false);
        [UnityTest] public IEnumerator MutedBoostersAndUndoCommitWithoutPlayingEffects() => Exercise(true);
        private IEnumerator Exercise(bool muted)
        {
            var rows = new List<string> { "seconds,booster,muted,effects" };
            for (int i = 0; i < Sounds.Length; i++)
            {
                var kind = (BoosterKind)i;
                var stock = new Dictionary<BoosterKind, int>(); foreach (BoosterKind item in Enum.GetValues(typeof(BoosterKind))) stock[item] = 5;
                string fixture = kind == BoosterKind.SpringRain ? "growing" : kind == BoosterKind.Scarecrow ? "crow_twig" :
                    kind == BoosterKind.MorningBell ? "sleep" : kind == BoosterKind.OwlEye ? "eggs" : "sleep";
                root.LoadFixture(GameFixtures.Create(fixture), new BoosterInventory(stock)); root.SetMuted(muted); played.Clear();
                if (kind == BoosterKind.Firefly)
                {
                    root.Hint(); float deadline = Time.realtimeSinceStartup + 10;
                    while (root.HintPending && Time.realtimeSinceStartup < deadline) yield return null;
                    Assert.That(root.HintPending, Is.False);
                }
                else Assert.That(root.UseBooster(kind), Is.True, kind.ToString());
                Assert.That(root.Session.Inventory[kind], Is.EqualTo(4), kind.ToString());
                Assert.That(played.Count(s => s == Sounds[i]), Is.EqualTo(muted ? 0 : 1), string.Join(",", played));
                Assert.That(played.Where(s => Sounds.Contains(s) && s != Sounds[i]), Is.Empty);
                if (muted) Assert.That(played, Is.Empty);
                rows.Add(Time.realtimeSinceStartup.ToString("F3", System.Globalization.CultureInfo.InvariantCulture) + "," + kind + "," + muted + "," + string.Join(";", played));
                yield return new WaitForSecondsRealtime(0.06f);
                played.Clear(); Assert.That(root.Undo(), Is.True);
                Assert.That(played.Count(s => s == "Undo"), Is.EqualTo(muted ? 0 : 1));
                Assert.That(root.Session.Inventory[kind], Is.EqualTo(5));
                if (muted) Assert.That(audio.GetComponents<AudioSource>().Where(s => !s.loop).All(s => s.mute && s.clip == null), Is.True);
                yield return new WaitForSecondsRealtime(0.06f);
            }
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../../build/booster-sounds-" + (muted ? "muted" : "audible") + ".csv"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllLines(path, rows);
        }
        [UnityTest]
        public IEnumerator RejectedBoosterDoesNotEmitSuccessSound()
        {
            root.LoadFixture(LevelLoader.Load(1), new BoosterInventory(new Dictionary<BoosterKind, int> { { BoosterKind.MorningBell, 1 } }));
            root.SetMuted(false); played.Clear();
            Assert.That(root.UseBooster(BoosterKind.MorningBell), Is.False);
            Assert.That(root.Session.Inventory[BoosterKind.MorningBell], Is.EqualTo(1)); Assert.That(played, Is.Empty);
            yield return null;
        }
    }
}
