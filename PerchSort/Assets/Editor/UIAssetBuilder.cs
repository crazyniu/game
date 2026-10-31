using System.IO;
using System.Linq;
using PerchSort.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class UIAssetBuilder
{
    public static UiTheme Build()
    {
        AssetDatabase.Refresh();
        ImportKit();
        ImportAudio();
        const string fontPath = "Assets/Fonts/Fredoka SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (font == null)
        {
            font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Fredoka-Medium.ttf"));
            font.name = "Fredoka SDF";
            AssetDatabase.CreateAsset(font, fontPath);
            AssetDatabase.AddObjectToAsset(font.material, font);
            foreach (var texture in font.atlasTextures) AssetDatabase.AddObjectToAsset(texture, font);
            font.TryAddCharacters(string.Concat(Enumerable.Range(32, 95).Select(i => (char)i)));
            font.atlasPopulationMode = AtlasPopulationMode.Static;
            EditorUtility.SetDirty(font);
        }
        const string chinesePath = "Assets/Fonts/Noto SC SDF.asset";
        var chinese = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(chinesePath);
        if (chinese == null || !chinese.HasCharacters(Strings.AllCharacters))
        {
            if (chinese != null) AssetDatabase.DeleteAsset(chinesePath);
            chinese = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansCJKsc-Regular.otf"),
                48, 5, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 2048, 2048);
            chinese.name = "Noto SC SDF";
            if (!chinese.TryAddCharacters(Strings.AllCharacters, out string missing))
                throw new System.InvalidOperationException("UI font missing characters: " + missing);
            chinese.atlasPopulationMode = AtlasPopulationMode.Static;
            AssetDatabase.CreateAsset(chinese, chinesePath);
            AssetDatabase.AddObjectToAsset(chinese.material, chinese);
            foreach (var texture in chinese.atlasTextures) AssetDatabase.AddObjectToAsset(texture, chinese);
        }
        font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset> { chinese };
        EditorUtility.SetDirty(chinese); EditorUtility.SetDirty(font);
        return new UiTheme { font = font, sprites = Directory.GetFiles("Assets/Art/ui_kit", "*.png").Where(path => !Path.GetFileName(path).Contains("bird_")).Select(path => AssetDatabase.LoadAssetAtPath<Sprite>(path)).ToArray(),
            panel = Sprite("panel_wood"), cream = Sprite("btn_secondary"), green = Sprite("btn_primary"), wood = Sprite("hud_bar"),
            undo = Sprite("icon_undo"), restart = Sprite("icon_restart"), pause = Sprite("icon_pause"), back = Sprite("icon_back"), check = Sprite("icon_check"), locked = Sprite("icon_lock"),
            perch = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/env/perch.png"),
            bird = FrontBirds()[2].stand, crown = Sprite("menu_tree"),
            frontBirds = FrontBirds(),
            menuBirds = new[] {1,3,2}.Select(id => AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/birds_front/bird_{id:00}.png")).ToArray() };
    }
    public static PerchSort.View.BirdArt[] FrontBirds() => Enumerable.Range(1,10).Select(id => new PerchSort.View.BirdArt {
        stand = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/birds_front/bird_{id:00}.png"),
        blink = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/birds_front/bird_{id:00}_blink.png"),
        sleep = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/birds_front/bird_{id:00}_sleep.png"),
        fly = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Art/birds_front/bird_{id:00}_fly.png") }).ToArray();
    private static Sprite Sprite(string name) => AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/ui_kit/" + name + ".png");
    private static void ImportKit()
    {
        const string target = "Assets/Art/ui_kit";
        Directory.CreateDirectory(target);
        string source = Path.GetFullPath(Path.Combine(Application.dataPath,"../../art/ui_kit"));
        string manifest = string.Join("\n",Directory.GetFiles(source,"manifest_*.json").Select(File.ReadAllText));
        foreach (string file in Directory.GetFiles(source,"*.png"))
        {
            string name = Path.GetFileNameWithoutExtension(file);
            if (name.StartsWith("contact_") || name.StartsWith("review_")) continue;
            string path = target + "/" + Path.GetFileName(file);
            if (!File.Exists(path) || !File.ReadAllBytes(file).SequenceEqual(File.ReadAllBytes(path))) File.Copy(file,path,true);
        }
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(target,"*.png"))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center; settings.spritePivot = Vector2.one*.5f;
            importer.SetTextureSettings(settings);
            string name = Path.GetFileNameWithoutExtension(path);
            var match = System.Text.RegularExpressions.Regex.Match(manifest, "\"" + name + "\"\\s*:\\s*\\{[^{}]*\"nine_slice\"\\s*:\\s*\\{\\s*\"L\":\\s*(\\d+),\\s*\"R\":\\s*(\\d+),\\s*\"T\":\\s*(\\d+),\\s*\"B\":\\s*(\\d+)");
            importer.spriteBorder = match.Success ? new Vector4(int.Parse(match.Groups[1].Value),int.Parse(match.Groups[4].Value),int.Parse(match.Groups[2].Value),int.Parse(match.Groups[3].Value)) : Vector4.zero;
            importer.SaveAndReimport();
        }
    }
    private static void ImportAudio()
    {
        const string target = "Assets/Audio/Resources"; Directory.CreateDirectory(target);
        foreach (string file in Directory.GetFiles(Path.Combine(Application.dataPath,"../../audio_src"),"*.wav"))
        {
            string path = target+"/"+Path.GetFileName(file);
            if (!File.Exists(path)) File.Copy(file,path);
        }
        AssetDatabase.Refresh();
        foreach (string path in Directory.GetFiles(target,"*.wav"))
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(path);
            bool music = Path.GetFileName(path).StartsWith("bgm_");
            var settings = importer.defaultSampleSettings;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = music ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.ADPCM;
            settings.quality = .85f; settings.preloadAudioData = !music;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings; importer.SaveAndReimport();
        }
    }
}
