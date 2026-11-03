using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class V6ArtImporter : AssetPostprocessor
{
    private const string Folder = "Assets/Resources/V6/Art/";
    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith(Folder, StringComparison.Ordinal) && assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            Configure((TextureImporter)assetImporter);
    }
    private static Vector4 Border(string name)
    {
        if (name.StartsWith("ui_btn_square", StringComparison.Ordinal)) return Vector4.one * 36;
        if (name.StartsWith("ui_btn_primary_green", StringComparison.Ordinal)) return Vector4.one * 48;
        switch (name)
        {
            case "ui_chip_coin": return Vector4.one * 28;
            case "ui_tray_plank": return new Vector4(120, 34, 120, 34);
            case "ui_panel_cream": case "ui_card_cream": return Vector4.one * 80;
            case "ui_pill_small": return Vector4.one * 24;
            default: return Vector4.zero;
        }
    }
    private static bool Configure(TextureImporter importer)
    {
        string name = Path.GetFileNameWithoutExtension(importer.assetPath); Vector4 border = Border(name);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        Vector2 pivot = name == "ui_rope_short" ? new Vector2(0.5f, 1) : name.StartsWith("gate_", StringComparison.Ordinal) ? new Vector2(0.5f, 0) : Vector2.one * 0.5f;
        bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
            !importer.alphaIsTransparency || importer.mipmapEnabled || importer.maxTextureSize != 2048 ||
            importer.alphaSource != TextureImporterAlphaSource.FromInput || importer.spriteBorder != border ||
            settings.spriteMeshType != SpriteMeshType.FullRect || settings.spritePivot != pivot || settings.spriteAlignment != 9;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.mipmapEnabled = false; importer.maxTextureSize = 2048; importer.spriteBorder = border;
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = 9; settings.spritePivot = pivot;
        importer.SetTextureSettings(settings); return changed;
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
