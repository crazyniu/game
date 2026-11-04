using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public sealed class ArtImporter : AssetPostprocessor
{
    public override uint GetVersion() => 3;

    private void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/Art/")) return;
        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 100;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize = 2048;
        importer.filterMode = FilterMode.Bilinear;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        string name = Path.GetFileNameWithoutExtension(assetPath);
        if (name.EndsWith("_sleep")) name = name.Replace("_sleep", "s");
        if (assetPath.StartsWith("Assets/Art/birds_front/"))
        {
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(.5f,154f/1024);
            importer.maxTextureSize = 512;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
        }
        else if (name.StartsWith("bird_") || name == "crate")
        {
            string manifest = File.ReadAllText(Path.Combine(Application.dataPath, "../../art/manifest.json"));
            var match = Regex.Match(manifest, "\"" + name + "\"[\\s\\S]*?\"unityPivot\"\\s*:\\s*\\[\\s*([\\d.]+),\\s*([\\d.]+)");
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = new Vector2(float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
                float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
        }
        if (assetPath.Contains("/Resources/") && !name.StartsWith("bird_"))
        {
            importer.GetSourceTextureWidthAndHeight(out int width, out int height);
            settings.spritePixelsPerUnit = Mathf.Max(width, height);
        }
        importer.SetTextureSettings(settings);
    }

    [MenuItem("Perch Sort/Import Art")]
    public static void ImportAll()
    {
        foreach (string file in Directory.GetFiles("Assets/Art", "*.png", SearchOption.AllDirectories))
            AssetDatabase.ImportAsset(file, ImportAssetOptions.ForceUpdate);
    }
}
