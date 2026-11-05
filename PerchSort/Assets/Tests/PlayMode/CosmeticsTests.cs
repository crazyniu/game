using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Meta = PerchSort.Meta;
using Object = UnityEngine.Object;

namespace PerchSort.PlayModeTests
{
    public sealed class CosmeticsTests : CollectionScreenTests
    {
        private static string CosmeticEvidence => Environment.GetEnvironmentVariable("PERCH_COSMETICS_EVIDENCE");
        private IEnumerator Fund(int coins)
        {
            root.gameObject.SetActive(false);
            var profile = ProgressStore.Load(); profile.Coins = coins; ProgressStore.Save(profile);
            yield return Reload();
        }
        private void Buy(string id)
        {
            Meta.Shop.TryGetItem(id, out var item);
            root.Shop.ShowTab(item.Kind); root.Shop.TapCosmetic(id);
            Assert.That(root.Shop.ConfirmOpen, Is.True, id);
            Assert.That(root.Collection.OwnsOutfit(id) || root.Collection.OwnsBirdhouseStyle(item.StyleId), Is.False);
            SaveFrame(item.Kind == Meta.ShopItemKind.Outfit ? "shop-outfit-confirm" : "shop-house-confirm");
            Button(root.Shop, "Confirm purchase").onClick.Invoke();
            Assert.That(root.Shop.ConfirmOpen, Is.False);
        }
        [UnityTest]
        public IEnumerator PurchasesDeductExactPricesAndPersistImmediately()
        {
            yield return Fund(5000); root.OpenShop();
            foreach (string id in new[] { "outfit.berry.1", "outfit.berry.2", "outfit.berry.3", "house.mushroom" })
            {
                Meta.Shop.TryGetItem(id, out var item); int before = root.Coins;
                Buy(id);
                Assert.That(root.Coins, Is.EqualTo(before - item.Price));
                var saved = ProgressStore.Load();
                Assert.That(saved.Coins, Is.EqualTo(root.Coins));
                Assert.That(item.Kind == Meta.ShopItemKind.Outfit ? saved.Notifications.RewardLedger.OwnsOutfit(id) : saved.Notifications.RewardLedger.OwnsBirdhouseStyle(item.StyleId), Is.True);
            }
            root.Shop.TapCosmetic("house.mushroom");
            Assert.That(ProgressStore.Load().Notifications.RewardLedger.EquippedBirdhouseStyle, Is.EqualTo("mushroom"));
            yield return Reload();
            Assert.That(root.Coins, Is.EqualTo(2800));
            Assert.That(root.Collection.OwnsOutfit("outfit.berry.3"), Is.True);
            Assert.That(root.Collection.OwnsBirdhouseStyle("mushroom"), Is.True);
            Assert.That(root.Collection.EquippedBirdhouseStyle, Is.EqualTo("mushroom"));
            root.OpenShop(); root.Shop.ShowTab(Meta.ShopItemKind.Birdhouse); root.Shop.TapCosmetic("house.mushroom");
            yield return Reload();
            Assert.That(root.Collection.EquippedBirdhouseStyle, Is.EqualTo("default"));
            Assert.That(root.Collection.OwnsBirdhouseStyle("mushroom"), Is.True);
        }
        [UnityTest]
        public IEnumerator OutfitSelectionIsPerSpeciesAndDetailToggleRestoresAnimatedDefault()
        {
            yield return Fund(5000); root.OpenShop();
            Buy("outfit.berry.1"); Buy("outfit.berry.2"); Buy("outfit.sky.1");
            root.Shop.ShowTab(Meta.ShopItemKind.Outfit);
            root.Shop.TapCosmetic("outfit.berry.1"); root.Shop.TapCosmetic("outfit.sky.1"); root.Shop.TapCosmetic("outfit.berry.2");
            Assert.That(root.Collection.EquippedOutfits["berry"], Is.EqualTo("outfit.berry.2"));
            Assert.That(root.Collection.EquippedOutfits["sky"], Is.EqualTo("outfit.sky.1"));
            Assert.That(root.Collection.EquippedOutfits.Count, Is.EqualTo(2));
            Assert.That(ProgressStore.Load().Notifications.RewardLedger.EquippedOutfits["berry"], Is.EqualTo("outfit.berry.2"));
            SaveFrame("ui_shop_outfits");
            var scroll = root.Shop.GetComponentInChildren<ScrollRect>(); scroll.verticalNormalizedPosition = 0;
            yield return null; SaveFrame("shop-outfits-bottom");
            root.ClosePanel(); root.OpenAviary(); root.Aviary.OpenDetail("berry");
            var bird = root.Aviary.transform.Find("Species detail/Living bird/Bird").GetComponent<Image>();
            Assert.That(bird.sprite.name, Is.EqualTo("berry_outfit_2"));
            Assert.That(bird.GetComponent<UiBirdLife>(), Is.Null);
            SaveFrame("ui_aviary_detail");
            var wrapper = (RectTransform)bird.transform.parent; Vector2 rest = wrapper.anchoredPosition;
            GameMotion.Instant = false; float min = float.MaxValue, max = 0;
            for (int i = 0; i < 12; i++)
            {
                yield return new WaitForSeconds(0.25f);
                min = Mathf.Min(min, bird.transform.localScale.x); max = Mathf.Max(max, bird.transform.localScale.x);
                Assert.That(wrapper.anchoredPosition, Is.EqualTo(rest));
                Assert.That(bird.sprite.name, Is.EqualTo("berry_outfit_2")); Assert.That(bird.transform.localScale.x, Is.Positive);
                SaveFrame("outfit-breath/" + i.ToString("D2"));
                if (!string.IsNullOrEmpty(CosmeticEvidence)) File.AppendAllText(Path.Combine(CosmeticEvidence, "outfit-breath.tsv"), Time.time.ToString("F4") + "\t" + bird.transform.localScale.x.ToString("F5") + "\t" + wrapper.anchoredPosition + "\t" + bird.sprite.name + "\n");
            }
            Assert.That(max - min, Is.GreaterThan(0.01f)); GameMotion.Instant = true;
            Button(root.Aviary, "Outfit slot 2").onClick.Invoke();
            Assert.That(root.Collection.EquippedOutfits.ContainsKey("berry"), Is.False);
            Assert.That(root.Collection.EquippedOutfits["sky"], Is.EqualTo("outfit.sky.1"));
            var normal = root.Aviary.transform.Find("Species detail/Living bird/Bird").GetComponent<Image>();
            Assert.That(normal.GetComponent<UiBirdLife>(), Is.Not.Null);
            SaveFrame("aviary-default");
            Button(root.Aviary, "Outfit slot 1").onClick.Invoke();
            Assert.That(root.Collection.EquippedOutfits["berry"], Is.EqualTo("outfit.berry.1"));
            Button(root.Aviary, "Default look").onClick.Invoke();
            Assert.That(root.Collection.EquippedOutfits.ContainsKey("berry"), Is.False);
            Button(root.Aviary, "Outfit slot 1").onClick.Invoke();
            yield return Reload();
            Assert.That(root.Collection.EquippedOutfits["berry"], Is.EqualTo("outfit.berry.1"));
        }
        [UnityTest]
        public IEnumerator EveryHouseUsesSelectedStyleWithoutChangingDeliveryOrUndo()
        {
            root.LoadFixture(LevelLoader.Load(1), coins: 5000);
            var originalDoors = root.Hud.Houses.ToDictionary(pair => pair.Key, pair => pair.Value.DoorPoint());
            var boardSprites = root.Board.Birds.Values.Select(view => view.GetComponentInChildren<Image>().sprite).ToArray();
            SaveFrame("houses-default"); root.OpenShop();
            foreach (string style in new[] { "mushroom", "teapot", "lantern", "acorn" })
            {
                Buy("house." + style); root.Shop.TapCosmetic("house." + style);
                Assert.That(root.Collection.EquippedBirdhouseStyle, Is.EqualTo(style));
                root.ClosePanel();
                foreach (var pair in root.Hud.Houses)
                {
                    var house = pair.Value.transform.Find("Birdhouse");
                    Assert.That(house.Find("House body").GetComponent<Image>().sprite.name, Is.EqualTo("house_style_" + style + "_body"));
                    Assert.That(house.Find("Colored roof").gameObject.activeSelf, Is.False);
                    Assert.That(Vector2.Distance(pair.Value.DoorPoint(), originalDoors[pair.Key]), Is.LessThan(0.01f));
                }
                SaveFrame("houses-" + style);
                Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.Hud.Houses["sky"].IsHome, Is.True);
                SaveFrame("houses-" + style + "-delivered");
                root.Undo(); Assert.That(root.Hud.Houses["sky"].IsHome, Is.False);
                Assert.That(root.TryMove(0, 1), Is.True); Assert.That(root.Hud.Houses["sky"].IsHome, Is.True);
                root.Undo(); root.OpenShop(); root.Shop.ShowTab(Meta.ShopItemKind.Birdhouse);
            }
            SaveFrame("shop-birdhouses");
            root.Shop.TapCosmetic("house.acorn"); Assert.That(root.Collection.EquippedBirdhouseStyle, Is.EqualTo("default"));
            root.ClosePanel();
            foreach (var house in root.Hud.Houses.Values)
            {
                Assert.That(house.transform.Find("Birdhouse/House body").GetComponent<Image>().sprite.name, Is.EqualTo("house_body"));
                Assert.That(house.transform.Find("Birdhouse/Colored roof").gameObject.activeSelf, Is.True);
            }
            Assert.That(root.Board.Birds.Values.Select(view => view.GetComponentInChildren<Image>().sprite).ToArray(), Is.EqualTo(boardSprites));
            yield return null;
        }
        [UnityTest]
        public IEnumerator InsufficientCoinsAndUnownedEquipmentLeaveSavedStateUntouched()
        {
            root.OpenShop(); root.Shop.ShowTab(Meta.ShopItemKind.Outfit);
            string before = root.Collection.ToText(), saved = PlayerPrefs.GetString(ProgressStore.NotificationsKey);
            root.Shop.TapCosmetic("outfit.sunny.1");
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(root.Coins, Is.EqualTo(240));
            Assert.That(root.ToggleCosmetic("outfit.sunny.1"), Is.False);
            Assert.That(root.BuyCosmetic("outfit.sunny.1"), Is.False);
            root.Shop.ShowTab(Meta.ShopItemKind.Birdhouse); root.Shop.TapCosmetic("house.mushroom");
            Assert.That(root.Shop.ConfirmOpen, Is.False); Assert.That(root.ToggleCosmetic("house.mushroom"), Is.False);
            Assert.That(root.Collection.ToText(), Is.EqualTo(before));
            Assert.That(PlayerPrefs.GetString(ProgressStore.NotificationsKey), Is.EqualTo(saved));
            Assert.That(ProgressStore.Load().Coins, Is.EqualTo(240)); yield return null;
        }
        private void SaveFrame(string name)
        {
            if (string.IsNullOrEmpty(CosmeticEvidence)) return;
            var camera = root.RenderCamera; var previous = camera.targetTexture; var active = RenderTexture.active;
            var target = RenderTexture.GetTemporary(1080, 1920, 24);
            Texture2D image = null;
            try
            {
                camera.targetTexture = target; Canvas.ForceUpdateCanvases(); root.Canvas.GetComponent<PerchSort.Game.SafeArea>().Apply();
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)root.Canvas.transform); Canvas.ForceUpdateCanvases();
                camera.Render(); RenderTexture.active = target;
                image = new Texture2D(1080, 1920, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, 1080, 1920), 0, 0); image.Apply();
                string path = Path.Combine(CosmeticEvidence, name + ".png"); Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previous; RenderTexture.active = active; RenderTexture.ReleaseTemporary(target);
                if (image != null) Object.Destroy(image);
            }
        }
    }
}
