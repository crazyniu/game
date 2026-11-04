using System;
using UnityEditor;

public sealed class V5ArtImporter : AssetPostprocessor
{
    private const string Folder = "Assets/Resources/V5/Art/";
    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith(Folder, StringComparison.Ordinal) && assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            Configure((TextureImporter)assetImporter);
    }

    private static bool Configure(TextureImporter importer)
    {
        bool changed = importer.textureType != TextureImporterType.Sprite ||
            importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency ||
            importer.mipmapEnabled || importer.maxTextureSize != 2048 ||
            importer.alphaSource != TextureImporterAlphaSource.FromInput;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
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
