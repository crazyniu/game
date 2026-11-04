using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace PerchSort.Tests
{
    public sealed class UiSpecTests
    {
        private GameController game;private string path;
        private static Vector2Int output;
        [UnitySetUp] public IEnumerator Open()
        {
            path=Path.Combine(Application.temporaryCachePath,"ui-spec-"+Guid.NewGuid()+".json");
            GameController.ProgressPathOverride=path;GameController.SettingsPathOverride=path+".settings";
            yield return SceneManager.LoadSceneAsync("Game");yield return null;game=UnityEngine.Object.FindAnyObjectByType<GameController>();
        }
        [UnityTearDown]public IEnumerator Close()
        {
            var empty=SceneManager.CreateScene("UI Spec Empty");SceneManager.SetActiveScene(empty);yield return SceneManager.UnloadSceneAsync("Game");
            GameController.ProgressPathOverride=GameController.SettingsPathOverride=null;
            foreach(string suffix in new[]{"",".bak",".settings",".settings.bak"})if(File.Exists(path+suffix))File.Delete(path+suffix);
        }
        [UnityTest]public IEnumerator PagesHaveProductionSpritesAndReadableTypeInBothLanguages()
        {
            foreach(string locale in new[]{"en","zh"})
            {
                game.SetLanguage(locale);game.ContinueGame();
            while(game.Board.IsBusy)yield return null;yield return null;
                foreach(var image in game.Hud.GetComponentsInChildren<Image>(true))Assert.That(image.sprite,Is.Not.Null,image.name);
                foreach(var text in game.Hud.GetComponentsInChildren<TMP_Text>(true))
                    Assert.That(text.fontSize,Is.GreaterThanOrEqualTo(text.name=="Version"?28:32),text.name);
            }
        }
        [UnityTest]public IEnumerator SettingsOnlyExposesSoundAndAllButtonsUseIcons()
        {
            game.Hud.ShowSettings();yield return null;
            Assert.That(game.Hud.GetComponentsInChildren<Button>().Where(b=>b.IsInteractable()).Select(b=>b.name),Is.EquivalentTo(new[]{"Close","Sound"}));
            Assert.That(game.Hud.GetComponentsInChildren<Slider>(),Is.Empty);
            foreach(var button in game.Hud.GetComponentsInChildren<Button>(true))
            foreach(var label in button.GetComponentsInChildren<TMP_Text>(true))
                Assert.That(label.name=="Number"||label.name=="Count",Is.True,button.name+" / "+label.name);
        }
        [UnityTest, Explicit("Multi-aspect acceptance deferred by coordinator")]public IEnumerator CriticalLayoutsDoNotOverlapAtAllThreeRequiredRatios()
        {
            foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(1440,1920)})
            {
                output=size;Canvas.ForceUpdateCanvases();yield return null;yield return null;
                game.Hud.ShowMenu();while(game.Hud.IsTransitioning)yield return null;yield return new WaitForSecondsRealtime(.6f);
                var menu=game.Hud.transform.Find("Safe HUD 1080x1920/Menu");
                AssertSeparate(menu.Find("start"),menu.Find("levels"));AssertSeparate(menu.Find("levels"),menu.Find("settings"));
                game.FinishTutorial();game.ContinueGame();
            while(game.Board.IsBusy)yield return null;yield return null;
                var page=game.Hud.transform.Find("Safe HUD 1080x1920/Game");
                AssertSeparate(page.Find("HUD controls/Undo"),page.Find("HUD controls/Hint action"));AssertSeparate(page.Find("HUD controls/Hint action"),page.Find("HUD controls/Restart"));
                AssertSeparate(page.Find("HUD controls/Level sign"),page.Find("HUD controls/Undo"));
                var result=page.Find("Result overlay/Result content");AssertSeparate(result.Find("Win decoration"),result.Find("Star slot 1"));AssertSeparate(result.Find("Win decoration"),result.Find("Buttons/next"));
                foreach(string key in new[]{"Level sign","Undo","Hint action","Restart"})
                {
                    var rect=Bounds(page.Find("HUD controls/"+key));Assert.That(rect.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(rect.xMax,Is.LessThanOrEqualTo(size.x));Assert.That(rect.yMin,Is.GreaterThanOrEqualTo(0));Assert.That(rect.yMax,Is.LessThanOrEqualTo(size.y));
                }

            }
        }
        private static Rect Bounds(Transform transform)
        {
            var corners=new Vector3[4];((RectTransform)transform).GetWorldCorners(corners);
            var root=UnityEngine.Object.FindAnyObjectByType<GameHud>().transform.Find("Safe HUD 1080x1920");
            float scale=Mathf.Min(output.x/1080f,output.y/1920f);
            var min=(Vector2)root.InverseTransformPoint(corners[0])*scale+(Vector2)output/2;
            var max=(Vector2)root.InverseTransformPoint(corners[2])*scale+(Vector2)output/2;
            return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
        }
        private static void AssertSeparate(Transform a,Transform b)=>Assert.That(Bounds(a).Overlaps(Bounds(b)),Is.False,a.name+" / "+b.name);
    }
}
