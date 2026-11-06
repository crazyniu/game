using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PerchSort.Editor
{
    public sealed class GameArtImporter : AssetPostprocessor
    {
        private static readonly string[] Folders =
        {
            "Assets/Resources/Art", "Assets/Resources/Birds", "Assets/Resources/World"
        };
        private static bool IsCurrentPng(string path)
            => path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                (path.StartsWith("Assets/Resources/Art/", StringComparison.Ordinal) ||
                 path.StartsWith("Assets/Resources/Birds/", StringComparison.Ordinal) ||
                 path.StartsWith("Assets/Resources/World/", StringComparison.Ordinal));

        private static bool IsFallback(string path)
        {
            switch (path)
            {
                case "Assets/Resources/Art/dial_sun.png":
                case "Assets/Resources/Art/dial_moon.png":
                case "Assets/Resources/Art/hand_pointer.png":
                case "Assets/Resources/Art/night_overlay.png":
                case "Assets/Resources/Art/raindrop.png":
                case "Assets/Resources/Art/swivel_knot.png": return true;
                default: return false;
            }
        }
        private void OnPreprocessTexture()
        {
            if (IsCurrentPng(assetPath))
                Configure((TextureImporter)assetImporter);
        }
        private static bool IsAuthoredOverlay(string path)
        {
            switch (path)
            {
                case "Assets/Resources/Birds/egg_idle.png":
                case "Assets/Resources/Birds/egg_wobble_l.png":
                case "Assets/Resources/Birds/egg_wobble_r.png":
                case "Assets/Resources/Birds/egg_crack.png":
                case "Assets/Resources/Birds/egg_shell.png":
                case "Assets/Resources/Birds/chick_fluff.png":
                case "Assets/Resources/Birds/mama_bow.png": return true;
                default: return false;
            }
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
            if (IsFallback(importer.assetPath)) return ConfigureFallback(importer);
            if (IsPaintedLettering(importer.assetPath)) return ConfigurePaintedLettering(importer);
            string name = Path.GetFileNameWithoutExtension(importer.assetPath);
            bool overlay = IsAuthoredOverlay(importer.assetPath);
            int maxSize = overlay ? 1024 : name.StartsWith("map_bg_w", StringComparison.Ordinal) || name.StartsWith("map_bg_continuous_w", StringComparison.Ordinal) || name.StartsWith("bg_level_", StringComparison.Ordinal) ? 4096 : 2048;
            Vector4 border = Border(name);
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            Vector2 pivot = name == "house_door_leaf" ? new Vector2(145f / 512, 206f / 512) : Vector2.one * 0.5f;
            if (name == "gift_box_lid") pivot = new Vector2(0.5f, 378f / 768);
            if (name == "gate_door_left") pivot = new Vector2(246f / 1024, 414f / 1024);
            if (name == "gate_door_right") pivot = new Vector2(778f / 1024, 414f / 1024);
            // The five egg frames share a foot baseline; the two family overlays are centered.
            // These seven authored imports preserve FullRect (serialized 0), 1024 limit, and no generated physics shape.
            if (overlay && name.StartsWith("egg_", StringComparison.Ordinal)) pivot = new Vector2(0.5f, 0.0625f);
            const SpriteMeshType mesh = SpriteMeshType.FullRect;
            bool changed = importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single ||
                !importer.alphaIsTransparency || importer.mipmapEnabled || importer.maxTextureSize != maxSize ||
                importer.textureCompression != TextureImporterCompression.Uncompressed || importer.spriteBorder != border ||
                importer.alphaSource != TextureImporterAlphaSource.FromInput || settings.spriteMeshType != mesh ||
                settings.spriteAlignment != 9 || settings.spritePivot != pivot ||
                (overlay && (settings.spriteGenerateFallbackPhysicsShape || importer.compressionQuality != 100));
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true; importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.mipmapEnabled = false; importer.maxTextureSize = maxSize;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.spriteBorder = border;
            importer.ReadTextureSettings(settings); settings.spriteMeshType = mesh;
            settings.spriteAlignment = 9; settings.spritePivot = pivot;
            if (overlay)
            {
                settings.spriteGenerateFallbackPhysicsShape = false;
                importer.compressionQuality = 100;
            }
            importer.SetTextureSettings(settings);
            return changed;
        }
        public static void ImportExisting()
        {
            foreach (string folder in Folders)
            {
                if (!AssetDatabase.IsValidFolder(folder)) continue;
                foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (!IsCurrentPng(path)) continue;
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && Configure(importer)) importer.SaveAndReimport();
                }
            }
        }

        private static bool IsPaintedLettering(string path) =>
            path.StartsWith("Assets/Resources/Art/glyphs/", StringComparison.Ordinal) ||
            path.StartsWith("Assets/Resources/Art/labels/", StringComparison.Ordinal);

        private static bool ConfigurePaintedLettering(TextureImporter importer)
        {
            var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
            bool changed = importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                importer.alphaSource != TextureImporterAlphaSource.FromInput || !importer.alphaIsTransparency ||
                importer.mipmapEnabled || importer.maxTextureSize != 4096 ||
                importer.textureCompression != TextureImporterCompression.Uncompressed ||
                importer.crunchedCompression || importer.compressionQuality != 100 ||
                importer.filterMode != FilterMode.Bilinear || importer.wrapMode != TextureWrapMode.Clamp ||
                importer.spriteBorder != Vector4.zero || importer.spritePixelsPerUnit != 100 ||
                settings.spriteMeshType != SpriteMeshType.FullRect || settings.spriteAlignment != 0 ||
                settings.spritePivot != Vector2.one * 0.5f || settings.spriteGenerateFallbackPhysicsShape;
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false; importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false; importer.compressionQuality = 100;
            importer.filterMode = FilterMode.Bilinear; importer.wrapMode = TextureWrapMode.Clamp;
            importer.spriteBorder = Vector4.zero; importer.spritePixelsPerUnit = 100;
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; settings.spriteAlignment = 0;
            settings.spritePivot = Vector2.one * 0.5f; settings.spriteGenerateFallbackPhysicsShape = false;
            importer.SetTextureSettings(settings);
            foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
            {
                if (!importer.GetPlatformTextureSettings(platform).overridden) continue;
                importer.ClearPlatformTextureSettings(platform); changed = true;
            }
            return changed;
        }

        private static bool ConfigureFallback(TextureImporter importer)
        {
            // Keep these six compressed imports separate: do not apply full-art mesh, border or pivot settings.
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
    }
}
