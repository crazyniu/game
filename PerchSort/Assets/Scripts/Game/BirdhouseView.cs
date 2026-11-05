using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BirdhouseView : MonoBehaviour
    {
        private sealed class Goal
        {
            public Button Button;
            public Image Portrait;
            public GameObject Check;
            public bool Delivered;
        }
        private readonly Dictionary<string, Goal> goals = new Dictionary<string, Goal>();
        private readonly Dictionary<string, Button> badges = new Dictionary<string, Button>();
        private RectTransform house, doorway;
        private Image door, face, openDoorway, body, roof, glow;
        private UiBirdLife resident;
        private GameMotion doorMotion, feedback;
        private GameArt art;
        private Rect bounds;
        private float openness;
        private int generation;
        private string delivering;
        public string Species { get; private set; }
        public bool IsHome => Species != null;
        public int ArrivalCount { get; private set; }
        public Vector2 GiftPoint { get; private set; }
        public IReadOnlyDictionary<string, Button> Badges => badges;
        internal float EntryRadius => GameLayout.HouseDoorDiameter(bounds.width) * 0.5f;
        public bool IsDelivered(string species) => goals.TryGetValue(species, out var goal) && goal.Delivered;

        public void Configure(BoardPresentation screen, GameArt assets, BoardSnapshot snapshot)
        {
            art = assets; bounds = GameLayout.NestCanvas;
            doorMotion = gameObject.AddComponent<GameMotion>(); feedback = gameObject.AddComponent<GameMotion>();
            house = GameUI.Rect(transform, "Birdhouse", bounds.center, bounds.size);
            Vector2 center = bounds.size * 0.5f;
            glow = GameUI.Fit(house, "Delivery glow", art.Get("glow_soft"), center, bounds.size * 1.3f);
            glow.color = Color.clear;
            body = GameUI.Image(house, "House body", Optional("goal_house_single", "house_body"), center, bounds.size);
            Vector2 doorPoint = GameLayout.HouseDoorLocal(bounds.size);
            Vector2 opening = bounds.size * (204f / 512);
            openDoorway = GameUI.Image(house, "Open doorway", Optional("goal_house_door_open", "house_door_open"), center, bounds.size);
            var mask = GameUI.Image(house, "Doorway mask", art.DoorMask(), doorPoint, opening);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false; doorway = mask.rectTransform;
            face = GameUI.Fit(doorway, "Species face", null, opening * 0.5f, opening * 1.2f);
            face.gameObject.SetActive(false);
            roof = GameUI.Image(house, "Colored roof", art.Get("house_roof_teal"), center, bounds.size);
            roof.gameObject.SetActive(art.Optional("goal_house_single") == null);
            ApplyStyle(snapshot.HouseStyle);
            door = GameUI.Image(house, "Door leaf", art.Get("house_door_leaf"), doorPoint, bounds.size);
            door.rectTransform.pivot = new Vector2(145 / 512f, 206 / 512f);
            door.rectTransform.anchoredPosition += Vector2.left * bounds.width * 108 / 512;
            float size = GameLayout.GoalBadgeSize(snapshot.Goals.Length);
            for (int i = 0; i < snapshot.Goals.Length; i++)
            {
                var data = snapshot.Goals[i]; string id = data.Species;
                var hit = GameUI.Image(transform, id + " goal info", null, GameLayout.GoalBadgePoint(i, snapshot.Goals.Length), Vector2.one * 88);
                hit.color = Color.clear; hit.raycastTarget = true;
                var button = hit.gameObject.AddComponent<GameButton>(); button.targetGraphic = hit;
                button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => screen.RequestGoal(id));
                var ring = GameUI.Fit(hit.transform, "Gold ring", Optional("badge_ring_gold", "btn_wood"), Vector2.one * 44, Vector2.one * size);
                var portraitSprite = art.Optional(data.PortraitKey);
                if (portraitSprite == null) { screen.Fail(data.PortraitKey); continue; }
                var portrait = GameUI.Fit(ring.transform, "Front portrait", portraitSprite, Vector2.one * size * 0.5f, Vector2.one * size * 0.8f);
                var coin = GameUI.Fit(ring.transform, "Gift coin", art.Get("coin_pill"), new Vector2(size - 4, size - 4), Vector2.one * 22);
                coin.sprite = art.Optional("gift_coin_" + data.GiftKey) ?? art.Get("btn_wood");
                GameUI.Fit(coin.transform, "Gift", art.Get(data.GiftKey), Vector2.one * 11, Vector2.one * 17);
                var check = GameUI.Fit(ring.transform, "Delivery check", Optional("badge_check", "btn_green"), new Vector2(4, size - 4), Vector2.one * 20);
                GameUI.Fit(check.transform, "Check", art.Icon("icon_check"), Vector2.one * 10, Vector2.one * 14);
                goals.Add(id, new Goal { Button = button, Portrait = portrait, Check = check.gameObject });
                badges.Add(id, button); SetBadge(id, data.Home);
            }
            GiftPoint = bounds.position + new Vector2(center.x, bounds.height * 0.2f);
            SetResident(snapshot.ResidentSpecies); DoorPose(0);
        }
        private Sprite Optional(string key, string retained)
        {
            var supplied = art.Optional(key);
            if (supplied != null) return supplied;
            GameArt.Warn(key, "Missing Art/" + key + "; using retained " + retained + " for board shell review.");
            return art.Get(retained);
        }
        public void ApplyStyle(string style)
        {
            bool styled = !string.IsNullOrEmpty(style) && style != "default";
            body.sprite = styled ? art.Get("house_style_" + style + "_body") : art.Optional("goal_house_single") ?? art.Get("house_body");
            roof.gameObject.SetActive(!styled && art.Optional("goal_house_single") == null);
        }
        public Vector2 DoorPoint()
        {
            Vector2 local = GameLayout.HouseDoorLocal(bounds.size) - bounds.size * 0.5f;
            var point = transform.InverseTransformPoint(house.TransformPoint(new Vector3(local.x, -local.y, 0)));
            var area = ((RectTransform)transform).rect;
            return new Vector2(point.x - area.xMin, area.yMax - point.y);
        }
        internal void MaskEntry(RectTransform bird) => bird.SetParent(doorway, true);
        public void BeginDelivery(string species) { delivering = species; ArrivalCount = 0; }
        public void OpenDoor(Action done) { resident?.Stop(); face.gameObject.SetActive(false); AnimateDoor(1, done); }
        public void CloseDoor(Action done) => AnimateDoor(0, done);
        private void AnimateDoor(float target, Action done)
        {
            doorMotion.CancelAll(); float start = openness; int run = generation;
            doorMotion.Play(0.25f, t => DoorPose(Mathf.Lerp(start, target, t)), () =>
            {
                if (run != generation) return;
                DoorPose(target); done?.Invoke();
            });
        }
        private void DoorPose(float value)
        {
            openness = value; openDoorway.gameObject.SetActive(value > 0);
            door.gameObject.SetActive(value > 0 && value < 1);
            door.rectTransform.localScale = new Vector3(Mathf.Lerp(1, 0.12f, value), 1, 1);
        }
        public void Arrival() => ArrivalCount++;
        public void CompleteFlock(Action done)
        {
            int run = generation; string species = delivering;
            SetBadge(species, true); SetResident(species);
            var goal = goals[species];
            feedback.Play(0.3f, t =>
            {
                float scale = 1 + 0.3f * Mathf.Sin(t * Mathf.PI);
                goal.Portrait.transform.parent.localScale = Vector3.one * scale;
                glow.color = new Color(1, 0.77f, 0.2f, 0.35f * Mathf.Sin(t * Mathf.PI));
            }, () =>
            {
                if (run != generation) return;
                goal.Portrait.transform.parent.localScale = Vector3.one;
                glow.color = Color.clear; CloseDoor(done);
            });
        }
        private void SetBadge(string species, bool delivered)
        {
            if (!goals.TryGetValue(species, out var goal)) return;
            goal.Delivered = delivered; goal.Portrait.color = new Color(1, 1, 1, delivered ? 1 : 0.45f);
            goal.Check.SetActive(delivered);
        }
        public void SyncState(HudSnapshot snapshot)
        {
            foreach (var goal in snapshot.Goals) SetBadge(goal.Species, goal.Home);
            ApplyStyle(snapshot.HouseStyle); SetResident(snapshot.ResidentSpecies);
        }
        private void SetResident(string species)
        {
            if (Species == species) { face.gameObject.SetActive(species != null); return; }
            resident?.Stop();
            if (resident != null) GameUI.Dispose(resident);
            for (int i = face.transform.childCount - 1; i >= 0; i--) GameUI.Dispose(face.transform.GetChild(i).gameObject);
            Species = species; face.gameObject.SetActive(species != null);
            face.rectTransform.localScale = Vector3.one;
            if (species == null) { ArrivalCount = 0; return; }
            face.sprite = art.Optional(species + "_front") ?? art.Get(species + "_face");
            resident = face.gameObject.AddComponent<UiBirdLife>();
            resident.Configure(face, art, species, feetPivot: false, frontFacing: true, friend: true); resident.Begin();
        }
        public void SetHome(bool value) => SetResident(value ? Species : null);
        public void Cancel()
        {
            generation++;
            if (doorMotion == null) return;
            doorMotion.CancelAll(); feedback.CancelAll(); delivering = null;
            glow.color = Color.clear; DoorPose(0);
            foreach (var goal in goals.Values) goal.Portrait.transform.parent.localScale = Vector3.one;
            if (face != null) face.gameObject.SetActive(IsHome);
        }
        private void OnEnable() { if (resident != null && IsHome) resident.Begin(); }
        private void OnDisable() { Cancel(); resident?.Stop(); }
        private void OnDestroy() => Cancel();
    }
}
