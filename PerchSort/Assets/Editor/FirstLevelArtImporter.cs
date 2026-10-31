using UnityEditor;
using UnityEngine;

public sealed class FirstLevelArtImporter : AssetPostprocessor
{
    public override int GetPostprocessOrder()=>1000;
    public override uint GetVersion()=>1;
    private void OnPreprocessTexture()
    {
        if(!assetPath.StartsWith("Assets/Art/Resources/FirstLevelArt/") || !assetPath.EndsWith(".png"))return;
        var importer=(TextureImporter)assetImporter;
        importer.textureType=TextureImporterType.Sprite;
        importer.spriteImportMode=SpriteImportMode.Single;
        importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
        importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.maxTextureSize=4096;importer.filterMode=FilterMode.Bilinear;
        bool bird=System.IO.Path.GetFileName(assetPath).StartsWith("red_") ||
            System.IO.Path.GetFileName(assetPath).StartsWith("blue_");
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);
        settings.spriteMeshType=SpriteMeshType.FullRect;
        settings.spriteAlignment=(int)SpriteAlignment.Custom;
        settings.spritePivot=bird?new Vector2(.5f,73f/512):Vector2.one*.5f;
        settings.spritePixelsPerUnit=assetPath.EndsWith("nest_front_mask.png")?512:100;
        importer.SetTextureSettings(settings);
    }
}
