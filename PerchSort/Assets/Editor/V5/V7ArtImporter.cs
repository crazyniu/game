using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class V7ArtImporter : AssetPostprocessor
{
    private const string Folder = "Assets/Resources/V7/";
    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith(Folder, StringComparison.Ordinal) && assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            Configure((TextureImporter)assetImporter);
    }
    private static Vector4 Border(string name)
    {
        switch (name)
        {
            case "panel_wood": return new Vector4(150, 100, 150, 110);
            case "card_wood": return new Vector4(120, 115, 120, 120);
            case "name_ribbon": return new Vector4(82, 38, 82, 38);
            case "stat_pill": return new Vector4(48, 42, 48, 42);
            case "btn_green_pill": return new Vector4(56, 24, 56, 24);
            case "ribbon_green": return new Vector4(180, 0, 180, 0);
            default: return Vector4.zero;
        }
    }
    private static bool Configure(TextureImporter importer)
    {
        string name = Path.GetFileNameWithoutExtension(importer.assetPath);
        int maxSize = name.StartsWith("map_bg_w", StringComparison.Ordinal) || name.StartsWith("bg_level_", StringComparison.Ordinal) ? 4096 : 2048;
        Vector4 border = Border(name);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        Vector2 pivot = name == "house_door_leaf" ? new Vector2(145f / 512, 206f / 512) : Vector2.one * 0.5f;
        if (name == "gift_box_lid") pivot = new Vector2(0.5f, 378f / 768);
        if (name == "gate_door_left") pivot = new Vector2(246f / 1024, 414f / 1024);
        if (name == "gate_door_right") pivot = new Vector2(778f / 1024, 414f / 1024);
        bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
            !importer.alphaIsTransparency || importer.mipmapEnabled || importer.maxTextureSize != maxSize ||
            importer.textureCompression != TextureImporterCompression.Uncompressed || importer.spriteBorder != border ||
            importer.alphaSource != TextureImporterAlphaSource.FromInput || settings.spriteMeshType != SpriteMeshType.FullRect ||
            settings.spriteAlignment != 9 || settings.spritePivot != pivot;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.mipmapEnabled = false; importer.maxTextureSize = maxSize;
        importer.textureCompression = TextureImporterCompression.Uncompressed; importer.spriteBorder = border;
        importer.ReadTextureSettings(settings); settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spriteAlignment = 9; settings.spritePivot = pivot;
        importer.SetTextureSettings(settings);
        return changed;
    }
    public static void ImportExisting()
    {
        if (!AssetDatabase.IsValidFolder(Folder.TrimEnd('/'))) return;
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Folder.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && Configure(importer)) importer.SaveAndReimport();
        }
    }
}
