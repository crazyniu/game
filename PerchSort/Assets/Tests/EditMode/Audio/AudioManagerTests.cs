using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PerchSort.Audio;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace PerchSort.Tests.Audio
{
    public sealed class AudioManagerTests
    {
        private GameObject host;
        private AudioManager manager;
        private AudioClip testMusic;
        private double now;
        private AudioSource[] Voices => host.GetComponents<AudioSource>().Where(s => !s.loop).ToArray();
        private AudioSource[] Music => host.GetComponents<AudioSource>().Where(s => s.loop).ToArray();

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("Audio test host");
            manager = host.AddComponent<AudioManager>();
            // EditMode does not run MonoBehaviour.Awake for this component.
            typeof(AudioManager).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(manager, null);
            now = 0;
            manager.Clock = () => now;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            if (testMusic != null) Object.DestroyImmediate(testMusic);
        }

        [Test]
        public void SfxSwitchStopsExistingVoicesAndAllowsImmediateReenable()
        {
            Assert.That(AudioManager.Instance, Is.SameAs(manager));
            manager.Play(Sfx.Select);
            Assert.That(Voices.Count(s => s.clip != null), Is.EqualTo(1));

            manager.SetSfxEnabled(false);
            manager.Play(Sfx.Move);
            Assert.That(manager.SfxEnabled, Is.False);
            Assert.That(Voices.All(s => s.mute && s.clip == null && !s.isPlaying), Is.True);
            Assert.That(Music.All(s=>!s.mute), Is.True);

            manager.SetSfxEnabled(true);
            manager.Play(Sfx.Select);
            Assert.That(manager.SfxEnabled, Is.True);
            Assert.That(Voices.All(s => !s.mute), Is.True);
            Assert.That(Voices.Count(s => s.clip != null), Is.EqualTo(1));
        }

        [Test]
        public void ThrottleIsPerEffectAndAllowsExactlyFiftyMilliseconds()
        {
            manager.Play(Sfx.Select);
            now = 0.049;
            manager.Play(Sfx.Select);
            Assert.That(Voices.Count(s => s.clip != null), Is.EqualTo(1));
            manager.Play(Sfx.Move);
            Assert.That(Voices.Count(s => s.clip != null), Is.EqualTo(2));
            now = 0.050;
            manager.Play(Sfx.Select);
            Assert.That(Voices.Count(s => s.clip != null), Is.EqualTo(3));
        }

        [Test]
        public void MusicSwitchHonorsPendingPlayAndExplicitStop()
        {
            manager.SetMusicEnabled(false); manager.PlayMusic();
            Assert.That(manager.CurrentMusic,Is.EqualTo(MusicPage.Menu));
            Assert.That(Music.All(s=>s.mute),Is.True);
            manager.SetMusicEnabled(true);
            Assert.That(Music[0].clip.name,Is.EqualTo("bgm_menu"));
            manager.SwitchMusic(MusicPage.Game);
            Assert.That(manager.IsCrossfading,Is.True);
            Assert.That(Music[1].clip.name,Is.EqualTo("bgm_game"));
            SetField("fadeStart",Time.unscaledTime-.5f);
            typeof(AudioManager).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(manager,null);
            Assert.That(manager.IsCrossfading,Is.False);
            Assert.That(Music[0].volume,Is.Zero);
            Assert.That(Music[1].volume,Is.EqualTo(.2f).Within(.001));
            manager.StopMusic();manager.SetMusicEnabled(true);
            Assert.That(manager.CurrentMusic,Is.EqualTo(MusicPage.None));
            Assert.That(Music.All(s=>s.clip==null),Is.True);
        }
        [Test]
        public void OriginalEffectsRemainAvailableAsMissingFileFallback()
        {
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            ArrayClear("effects");ArrayClear("chirps");
            string[] names={"select","move","collect","win","error","undo","click"};
            for(int i=0;i<7;i++){now=i;manager.Play((Sfx)i);Assert.That(Voices[i].clip.name,Is.EqualTo(names[i]));}
            void ArrayClear(string name){var array=(System.Array)typeof(AudioManager).GetField(name,flags).GetValue(manager);System.Array.Clear(array,0,array.Length);}
        }
        [Test]
        public void EventMapAndProductionResourcesAreComplete()
        {
            Assert.That(AudioManager.EffectKeys.Length,Is.EqualTo(System.Enum.GetValues(typeof(Sfx)).Length));
            foreach(string key in AudioManager.SoundKeys)Assert.That(Resources.Load<AudioClip>(key),Is.Not.Null,key);
            foreach(string key in new[]{"bgm_menu","bgm_game"})
            {
                var clip=Resources.Load<AudioClip>(key);Assert.That(clip,Is.Not.Null,key);
                var importer=(AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
                Assert.That(importer.defaultSampleSettings.loadType,Is.EqualTo(AudioClipLoadType.Streaming));
                Assert.That(importer.defaultSampleSettings.compressionFormat,Is.EqualTo(AudioCompressionFormat.Vorbis));
            }
        }
        [Test]
        public void RepeatedChirpsNeverRepeatAndFocusRestoresPreferences()
        {
            AudioClip previous=null;
            for(int i=0;i<20;i++){now=i;manager.Play(Sfx.Select);var clip=Voices[i%8].clip;Assert.That(clip,Is.Not.SameAs(previous));previous=clip;}
            var focus=typeof(AudioManager).GetMethod("OnApplicationFocus",BindingFlags.Instance|BindingFlags.NonPublic);
            manager.SetMusicEnabled(false);focus.Invoke(manager,new object[]{false});
            Assert.That(host.GetComponents<AudioSource>().All(s=>s.mute),Is.True);
            focus.Invoke(manager,new object[]{true});Assert.That(Voices.All(s=>!s.mute),Is.True);Assert.That(Music.All(s=>s.mute),Is.True);
        }

        [Test]
        public void RepeatedPlaybackReusesFixedPool()
        {
            for (int i = 0; i < 24; i++)
            {
                now = i * 0.1;
                manager.Play((Sfx)(i % 7));
            }
            Assert.That(Voices.Length, Is.EqualTo(8));
            Assert.That(host.GetComponents<AudioSource>().Length, Is.EqualTo(10));
            Assert.That(Voices.All(s => s.clip != null && !s.playOnAwake && s.spatialBlend == 0), Is.True);
        }

        [TestCase("select")]
        [TestCase("move")]
        [TestCase("collect")]
        [TestCase("win")]
        [TestCase("error")]
        [TestCase("undo")]
        [TestCase("click")]
        public void DeliveredResourceUsesRequiredImportSettings(string name)
        {
            var clip = Resources.Load<AudioClip>("Audio/" + name);
            Assert.That(clip, Is.Not.Null);
            Assert.That(clip.channels, Is.EqualTo(1));
            Assert.That(clip.frequency, Is.EqualTo(44100));
            var importer = (AudioImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(clip));
            var settings = importer.defaultSampleSettings;
            Assert.That(settings.loadType, Is.EqualTo(AudioClipLoadType.DecompressOnLoad));
            Assert.That(settings.compressionFormat, Is.EqualTo(AudioCompressionFormat.ADPCM));
            Assert.That(settings.preloadAudioData, Is.True);
            Assert.That(settings.sampleRateSetting, Is.EqualTo(AudioSampleRateSetting.PreserveSampleRate));
        }

        private void SetField(string name, object value)
        {
            typeof(AudioManager).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(manager, value);
        }
    }
}
