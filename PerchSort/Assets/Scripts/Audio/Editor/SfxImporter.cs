using System.IO;
using UnityEditor;
using UnityEngine;

namespace PerchSort.Audio.Editor
{
    public sealed class SfxImporter : AssetPostprocessor
    {
        public override uint GetVersion() => 2;

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/Audio/")) return;
            bool music = Path.GetFileNameWithoutExtension(assetPath) .StartsWith("bgm_");
            var importer = (AudioImporter)assetImporter;
            importer.forceToMono = assetPath.StartsWith("Assets/Audio/Resources/Audio/");
            importer.loadInBackground = music;
            var settings = importer.defaultSampleSettings;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.quality = music ? 0.7f : 1f;
            settings.preloadAudioData = !music;
            importer.defaultSampleSettings = settings;
        }
    }
}
