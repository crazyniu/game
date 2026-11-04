using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PerchSort.Tests
{
    public sealed class UiKitTests
    {
        [Test]
        public void ManifestSpritesArePresentFullRectAndLossless()
        {
            string source=Path.GetFullPath(Path.Combine(Application.dataPath,"../../art/ui_kit"));
            foreach(string manifest in Directory.GetFiles(source,"manifest_*.json"))
            foreach(Match match in Regex.Matches(File.ReadAllText(manifest),"\\\"file\\\"\\s*:\\s*\\\"([^\\\"]+\\.png)\\\""))
            {
                string file=match.Groups[1].Value;if(file.StartsWith("contact_"))continue;
                string path="Assets/Art/ui_kit/"+file;
                var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);Assert.That(sprite,Is.Not.Null,file);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
                Assert.That(settings.spriteMeshType,Is.EqualTo(SpriteMeshType.FullRect),file);
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed),file);
                if(file.StartsWith("level_cell")){Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(300,340)));Assert.That(sprite.border,Is.EqualTo(Vector4.zero));}
            }
        }
    }
}
