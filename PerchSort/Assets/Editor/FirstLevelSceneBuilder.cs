using System;
using System.IO;
using System.Linq;
using PerchSort.FirstLevelRuntime;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class FirstLevelSceneBuilder
{
    public const string ScenePath="Assets/Scenes/FirstLevel.unity";
    private const string Art="Assets/Art/Resources/FirstLevelArt/";
    private const string Generated="Assets/FirstLevelGenerated/";

    [MenuItem("Perch Sort/Build First Level Slice")]
    public static void Build()
    {
        // Validate physical resources and build reviewed references BEFORE replacing
        // the current editor scene. No legacy Game scene or BuildSettings is edited.
        var assets=BuildAssets();
        assets.ValidateOrThrow(2);
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var black=new GameObject("Window clear",typeof(Camera)).GetComponent<Camera>();
        black.depth=-10;black.cullingMask=0;black.clearFlags=CameraClearFlags.SolidColor;black.backgroundColor=Color.black;
        var camera=new GameObject("First-level camera",typeof(Camera),typeof(AudioListener),typeof(Physics2DRaycaster)).GetComponent<Camera>();
        camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);
        camera.orthographic=true;camera.orthographicSize=9.6f;
        camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.94f,.96f,.89f);
        camera.nearClipPlane=.1f;camera.farClipPlane=100;
        var background=new GameObject("Forest backdrop").AddComponent<SpriteRenderer>();
        background.sprite=assets.Background;background.sharedMaterial=assets.SpriteMaterial;background.sortingOrder=-30;
        var world=new GameObject("Safe first-level world").transform;
        var canvas=new GameObject("First-level HUD",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)).GetComponent<Canvas>();
        canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=5;canvas.sortingOrder=1000;
        var scaler=canvas.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1080,1920);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var root=new GameObject("Safe HUD 1080x1920",typeof(RectTransform)).GetComponent<RectTransform>();
        root.SetParent(canvas.transform,false);root.anchorMin=root.anchorMax=Vector2.one*.5f;root.sizeDelta=new Vector2(1080,1920);
        var entry=new GameObject("First level entry").AddComponent<FirstLevelEntry>();
        entry.Configure(assets,camera,world,root,background);
        new GameObject("First-level EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();
        Debug.Log("Built isolated FirstLevel.unity. B content only; no legacy scene, save or BuildSettings mutation.");
    }

    private static FirstLevelAssets BuildAssets()
    {
        string[] names={"red_idle","blue_idle","forest_background","perch","nest_front_mask"};
        names=names.Concat(Enumerable.Range(1,6).Select(i=>"red_flight_"+i.ToString("D2")))
            .Concat(Enumerable.Range(1,6).Select(i=>"blue_flight_"+i.ToString("D2"))).ToArray();
        foreach(string name in names)
            if(!File.Exists(Art+name+".png"))throw new FileNotFoundException("Accepted first-level artwork missing: "+name);
        foreach(string name in names)AssetDatabase.ImportAsset(Art+name+".png",ImportAssetOptions.ForceUpdate);
        Directory.CreateDirectory(Generated);AssetDatabase.Refresh();
        const string path=Generated+"FirstLevelAssets.asset";
        var a=AssetDatabase.LoadAssetAtPath<FirstLevelAssets>(path);
        if(a==null){a=ScriptableObject.CreateInstance<FirstLevelAssets>();AssetDatabase.CreateAsset(a,path);}
        a.AcceptedForRuntime=true;
        a.ReviewIdentity="T21 static internal QA accepted; in-context/runtime acceptance pending";
        a.Species=new[]{Species("red"),Species("blue")};
        a.Background=NewSprite("forest_background");a.Branch=NewSprite("perch");
        if(a.Branch.rect.width!=2172 || a.Branch.rect.height!=724)
            throw new InvalidOperationException("Accepted branch must retain its exact2172x724 source geometry.");
        a.Nest=Existing("Assets/Art/Resources/env/nest.png");
        a.NestFrontMask=NewSprite("nest_front_mask");
        a.RetiredStump=BuildStump(a.Branch);
        a.ButtonSkin=Existing("Assets/Art/ui_kit/panel_wood.png");
        a.PanelSkin=Existing("Assets/Art/ui_kit/panel_wood.png");
        a.HandIcon=Existing("Assets/Art/ui_kit/hand_pointer.png");
        a.PauseIcon=Icon("pause");a.UndoIcon=Icon("undo");a.RestartIcon=Icon("restart");
        a.HomeIcon=Icon("home");a.PlayIcon=Icon("play");a.CloseIcon=Icon("close");
        a.SoundIcon=Icon("sound");a.MutedIcon=Icon("sound_off");
        a.Font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Fredoka SDF.asset");
        if(a.Font==null)throw new InvalidOperationException("Existing Fredoka SDF font is missing; do not rebuild legacy UI to obtain it.");
        a.SpriteMaterial=Material("FirstLevelSprites");
        a.ParticleMaterial=Material("FirstLevelParticles");
        EditorUtility.SetDirty(a);AssetDatabase.SaveAssets();return a;
    }

    private static FirstLevelSpeciesArt Species(string key)
    {
        var idle=Pose(NewSprite(key+"_idle"),true);
        return new FirstLevelSpeciesArt { SpeciesKey=key,Idle=idle,Folded=Pose(idle.Sprite,true),
            WingCycle=Enumerable.Range(1,6).Select(i=>Pose(NewSprite(key+"_flight_"+i.ToString("D2")),false)).ToArray() };
    }
    private static FirstLevelPose Pose(Sprite sprite,bool contact)=>new FirstLevelPose {
        Sprite=sprite,AnchorFromTop=new Vector2(.5f,439f/512),IsFootContact=contact };
    private static Sprite NewSprite(string key)=>Existing(Art+key+".png");
    private static Sprite Icon(string key)=>Existing("Assets/Art/ui_kit/icon_"+key+".png");
    private static Sprite Existing(string path)
    {
        var value=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(value==null)throw new InvalidOperationException("Required accepted sprite missing: "+path);
        return value;
    }
    private static Material Material(string name)
    {
        string path=Generated+name+".mat";
        var value=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(value==null)
        {
            var shader=Shader.Find("Sprites/Default");
            if(shader==null)throw new InvalidOperationException("Built-in sprite shader unavailable.");
            value=new Material(shader){name=name};AssetDatabase.CreateAsset(value,path);
        }
        // This Material asset is explicitly serialized by FirstLevelAssets and scene,
        // retaining its Shader in Player builds; no runtime string-only lookup exists.
        return value;
    }
    private static Sprite BuildStump(Sprite source)
    {
        string path=Generated+"SupportedStump.asset";
        var stump=AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if(stump==null)
        {
            stump=Sprite.Create(source.texture,new Rect(0,0,360,724),Vector2.one*.5f,source.pixelsPerUnit);
            stump.name="Accepted-branch technical root crop";AssetDatabase.CreateAsset(stump,path);
        }
        return stump;
    }
}
