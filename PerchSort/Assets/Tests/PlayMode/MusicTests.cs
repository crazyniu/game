using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PerchSort.PlayModeTests
{
    public sealed class MusicTests
    {
        private readonly Dictionary<string, int?> saved = new Dictionary<string, int?>();
        private string savedNotifications;
        private bool previousInstant;
        private float previousTimeScale;
        private GameRoot root;
        private AudioSource menu, game;
        private GameObject audio;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            previousInstant = GameMotion.Instant;
            previousTimeScale = Time.timeScale;
            GameMotion.Instant = true;
            Time.timeScale = 1;
            foreach (string key in ProgressStore.Keys())
                if (key != ProgressStore.NotificationsKey)
                    saved[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetInt(key) : (int?)null;
            savedNotifications = PlayerPrefs.HasKey(ProgressStore.NotificationsKey)
                ? PlayerPrefs.GetString(ProgressStore.NotificationsKey) : null;
            yield return UnloadGame();
            ProgressStore.Save(new GameProfile());
            SceneManager.sceneLoaded += InitializeWithoutNotifications;
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            Assert.That(root != null && root.IsReady && root.IsMap, Is.True);
            audio = GameObject.Find("Game audio");
            Assert.That(audio, Is.Not.Null, "Use the genuine AudioManager created by GameRoot.");
            var songs = audio.GetComponents<AudioSource>().Where(source => source.loop).ToArray();
            menu = songs.Single(source => source.clip != null && source.clip.name == "bgm_menu");
            game = songs.Single(source => source != menu);
            // Batchmode focus is host-dependent; establish the foreground state for this scenario.
            audio.SendMessage("OnApplicationPause", false);
            audio.SendMessage("OnApplicationFocus", true);
        }

        private void InitializeWithoutNotifications(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game") return;
            root = Object.FindAnyObjectByType<GameRoot>();
            root.Initialize(true, false);
        }

        private static IEnumerator UnloadGame()
        {
            var scene = SceneManager.GetSceneByName("Game");
            if (!scene.IsValid() || !scene.isLoaded) yield break;
            var cleanup = SceneManager.CreateScene("Music acceptance cleanup");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(scene);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            SceneManager.sceneLoaded -= InitializeWithoutNotifications;
            yield return UnloadGame();
            foreach (var pair in saved)
                if (pair.Value.HasValue) PlayerPrefs.SetInt(pair.Key, pair.Value.Value);
                else PlayerPrefs.DeleteKey(pair.Key);
            if (savedNotifications == null) PlayerPrefs.DeleteKey(ProgressStore.NotificationsKey);
            else PlayerPrefs.SetString(ProgressStore.NotificationsKey, savedNotifications);
            PlayerPrefs.Save();
            GameMotion.Instant = previousInstant;
            Time.timeScale = previousTimeScale;
        }

        private void Evidence(string phase)
        {
            Debug.Log($"T54A t={Time.realtimeSinceStartup:F3} page={(root.IsMap ? "map" : "level")} phase={phase} " +
                $"menu={Describe(menu)} game={Describe(game)}");
        }

        private static string Describe(AudioSource source) =>
            $"{(source.clip == null ? "none" : source.clip.name)},playing={source.isPlaying},loop={source.loop}," +
            $"mute={source.mute},volume={source.volume:F3},time={(source.clip == null ? 0 : source.time):F3}";

        private static void Playing(AudioSource source, string clip)
        {
            Assert.That(source.clip.name, Is.EqualTo(clip));
            Assert.That(source.loop && source.isPlaying && !source.mute, Is.True, clip);
        }

        [UnityTest]
        public IEnumerator PagesCrossfadeForOneSecondEvenWhenGameTimeIsStopped()
        {
            yield return new WaitForSecondsRealtime(1.15f);
            Playing(menu, "bgm_menu");
            Assert.That(menu.volume, Is.EqualTo(.2f).Within(.005f));
            Evidence("menu-settled");
            Time.timeScale = 0;
            root.LoadLevel(1);
            Evidence("level-crossfade-start");
            yield return new WaitForSecondsRealtime(.4f);
            Playing(menu, "bgm_menu");
            Playing(game, "bgm_game");
            Assert.That(menu.volume, Is.InRange(.04f, .17f));
            Assert.That(game.volume, Is.InRange(.04f, .17f));
            Evidence("level-crossfade-middle");
            yield return new WaitForSecondsRealtime(.75f);
            Assert.That(menu.isPlaying, Is.False);
            Playing(game, "bgm_game");
            Assert.That(game.volume, Is.EqualTo(.2f).Within(.005f));
            Evidence("level-settled");
            root.OpenMap();
            yield return new WaitForSecondsRealtime(.4f);
            Evidence("map-crossfade-middle");
            yield return new WaitForSecondsRealtime(.75f);
            Playing(menu, "bgm_menu");
            Assert.That(game.isPlaying, Is.False);
            Evidence("map-settled");
        }

        [UnityTest]
        public IEnumerator SettingsCardsAndWinKeepTheSameTrackAndPlayhead()
        {
            yield return new WaitForSecondsRealtime(1.15f);
            float before = menu.time;
            root.OpenLevelCard(1);
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(root.HasModal, Is.True);
            root.ClosePanel();
            Playing(menu, "bgm_menu");
            Assert.That(menu.time, Is.GreaterThan(before));
            Evidence("map-card-closed-no-restart");
            root.LoadLevel(1);
            yield return new WaitForSecondsRealtime(1.15f);
            before = game.time;
            root.OpenSettings();
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(root.HasModal, Is.True);
            Evidence("settings-open");
            root.CloseSettings();
            root.OpenPopup(PopupKind.NewFriend);
            yield return new WaitForSecondsRealtime(.25f);
            root.ClosePanel();
            Playing(game, "bgm_game");
            Assert.That(game.time, Is.GreaterThan(before));
            Evidence("level-popup-closed-no-restart");
            Assert.That(root.TryMove(0, 1), Is.True);
            Assert.That(root.TryMove(2, 0), Is.True);
            Assert.That(root.Win.IsOpen, Is.True);
            before = game.time;
            yield return new WaitForSecondsRealtime(.25f);
            root.Restart();
            Playing(game, "bgm_game");
            Assert.That(game.time, Is.GreaterThan(before));
            Evidence("win-and-restart-no-music-restart");
        }

        [UnityTest]
        public IEnumerator MutePersistsAndAppliesToBothTracksAcrossPageChangesAndReload()
        {
            root.SetMuted(true);
            Assert.That(PlayerPrefs.GetInt(ProgressStore.MutedKey), Is.EqualTo(1));
            root.LoadLevel(1);
            yield return new WaitForSecondsRealtime(1.15f);
            Assert.That(audio.GetComponents<AudioSource>().All(source => source.mute), Is.True);
            Evidence("level-muted");
            yield return UnloadGame();
            yield return SceneManager.LoadSceneAsync("Game", LoadSceneMode.Single);
            yield return null;
            audio = GameObject.Find("Game audio");
            var songs = audio.GetComponents<AudioSource>().Where(source => source.loop).ToArray();
            menu = songs.Single(source => source.clip != null && source.clip.name == "bgm_menu");
            game = songs.Single(source => source != menu);
            audio.SendMessage("OnApplicationFocus", true);
            Assert.That(root.Muted, Is.True);
            Assert.That(audio.GetComponents<AudioSource>().All(source => source.mute), Is.True);
            root.SetMuted(false);
            yield return new WaitForSecondsRealtime(1.15f);
            Playing(menu, "bgm_menu");
            Assert.That(PlayerPrefs.GetInt(ProgressStore.MutedKey), Is.Zero);
            Evidence("reloaded-map-unmuted");
        }

        [UnityTest]
        public IEnumerator FocusAndApplicationPauseFreezePlayheadsUntilBothResume()
        {
            root.LoadLevel(1);
            yield return new WaitForSecondsRealtime(1.15f);
            Playing(game, "bgm_game");
            AudioSettings.GetDSPBufferSize(out int bufferSamples, out _);
            int pauseQuantum = Mathf.CeilToInt(bufferSamples * (float)game.clip.frequency / AudioSettings.outputSampleRate);
            audio.SendMessage("OnApplicationFocus", false);
            int pauseRequestedSample = game.timeSamples;
            Assert.That(game.isPlaying, Is.False);
            // Pause reaches the mixer at a DSP-buffer boundary; allow that one boundary, not ongoing drift.
            yield return new WaitForSecondsRealtime((float)bufferSamples / AudioSettings.outputSampleRate);
            int pausedSample = game.timeSamples;
            Assert.That(pausedSample, Is.InRange(pauseRequestedSample, pauseRequestedSample + pauseQuantum));
            double until = Time.unscaledTimeAsDouble + .3;
            do
            {
                yield return null;
                Assert.That(game.isPlaying, Is.False);
                Assert.That(game.timeSamples, Is.EqualTo(pausedSample), "The settled paused playhead must not advance.");
            } while (Time.unscaledTimeAsDouble < until);
            Evidence("focus-lost-playhead-paused");
            audio.SendMessage("OnApplicationPause", true);
            audio.SendMessage("OnApplicationFocus", true);
            until = Time.unscaledTimeAsDouble + .2;
            do
            {
                yield return null;
                Assert.That(game.isPlaying, Is.False);
                Assert.That(game.timeSamples, Is.EqualTo(pausedSample), "Focus alone must not resume an application-paused song.");
            } while (Time.unscaledTimeAsDouble < until);
            // Sample both sides of UnPause in this frame, rather than comparing with a stale pre-pause clock.
            int beforeResume = game.timeSamples;
            audio.SendMessage("OnApplicationPause", false);
            int resumedSample = game.timeSamples;
            int resumeTolerance = Mathf.FloorToInt(game.clip.frequency / 60f);
            Assert.That(resumedSample, Is.EqualTo(beforeResume).Within(resumeTolerance));
            Debug.Log($"T54A pause-request={pauseRequestedSample} paused={pausedSample} quantum={pauseQuantum} " +
                $"before-resume={beforeResume} resumed={resumedSample} resume-tolerance={resumeTolerance} frequency={game.clip.frequency}");
            yield return new WaitForSecondsRealtime(.25f);
            Playing(game, "bgm_game");
            Assert.That(game.timeSamples, Is.GreaterThan(beforeResume));
            Evidence("focus-and-pause-resumed");
        }

        [UnityTest]
        public IEnumerator PageChangeWhileSuspendedResumesThePendingFadeWithoutElapsedBackgroundTime()
        {
            yield return new WaitForSecondsRealtime(1.15f);
            audio.SendMessage("OnApplicationFocus", false);
            yield return new WaitForSecondsRealtime(.3f);
            root.LoadLevel(1);
            yield return new WaitForSecondsRealtime(.3f);
            Assert.That(game.isPlaying, Is.False);
            Assert.That(game.volume, Is.Zero);
            audio.SendMessage("OnApplicationFocus", true);
            yield return new WaitForSecondsRealtime(.4f);
            Assert.That(game.volume, Is.InRange(.04f, .17f));
            Evidence("suspended-page-change-resumed-middle");
            yield return new WaitForSecondsRealtime(.75f);
            Playing(game, "bgm_game");
            Assert.That(menu.isPlaying, Is.False);
            Assert.That(game.volume, Is.EqualTo(.2f).Within(.005f));
        }

        [UnityTest]
        public IEnumerator UserInteractionRetriesBlockedPlaybackWithoutRestartingAnActiveSong()
        {
            yield return new WaitForSecondsRealtime(1.15f);
            menu.Stop();
            audio.SendMessage("ResumeMusicOnUserInteraction");
            yield return new WaitForSecondsRealtime(.25f);
            Playing(menu, "bgm_menu");
            float before = menu.time;
            audio.SendMessage("ResumeMusicOnUserInteraction");
            yield return new WaitForSecondsRealtime(.25f);
            Assert.That(menu.time, Is.GreaterThan(before));
            Evidence("interaction-retry-does-not-restart-active-track");
        }
    }
}
