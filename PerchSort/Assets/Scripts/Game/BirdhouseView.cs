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
            public GameObject Gift;
            public UiBirdLife Life;
            public bool Delivered;
        }
        private readonly Dictionary<string, Goal> goals = new Dictionary<string, Goal>();
        private readonly Dictionary<string, Button> badges = new Dictionary<string, Button>();
        private RectTransform house, doorway;
        private Image face, openDoorway, body, roof, glow;
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
        internal float EntryRadius => Mathf.Min(doorway.rect.width, doorway.rect.height) * 0.5f;
        public bool IsDelivered(string species) => goals.TryGetValue(species, out var goal) && goal.Delivered;

        public void Configure(BoardPresentation screen, GameArt assets, BoardSnapshot snapshot)
        {
            art = assets; bounds = HudView.Measured("goal_house");
            doorMotion = gameObject.AddComponent<GameMotion>(); feedback = gameObject.AddComponent<GameMotion>();
            house = GameUI.Rect(transform, "Birdhouse", bounds.center, bounds.size);
            Vector2 center = bounds.size * 0.5f;
            glow = GameUI.Fit(house, "Delivery glow", art.Get("glow_soft"), center, bounds.size * 1.3f);
            glow.color = Color.clear;
            var houseCrop = GameUI.Rect(house, "House art crop", center, bounds.size);
            houseCrop.gameObject.AddComponent<RectMask2D>();
            Rect houseArt = HudView.Pixels(45, 110, 624, 580, 720, 720);
            body = HudView.Artwork(houseCrop, "House body", art.Get("goal_house_single"), new Rect(Vector2.zero, bounds.size), houseArt);
            Vector2 doorPoint = new Vector2(bounds.width * 310f / 624, bounds.height * 336f / 580);
            Vector2 opening = new Vector2(bounds.width * 228f / 624, bounds.height * 228f / 580);
            openDoorway = HudView.Artwork(houseCrop, "Open doorway", art.Get("goal_house_door_open"), new Rect(Vector2.zero, bounds.size), houseArt);
            var mask = GameUI.Image(house, "Doorway mask", art.DoorMask(), doorPoint, opening);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false; doorway = mask.rectTransform;
            face = GameUI.Fit(doorway, "Species face", null, opening * 0.5f, opening * 1.2f);
            face.gameObject.SetActive(false);
            roof = GameUI.Image(house, "Colored roof", art.Get("house_roof_teal"), center, bounds.size);
            roof.gameObject.SetActive(false);
            ApplyStyle(snapshot.HouseStyle);
            Rect firstRing = HudView.Measured("goal_ring_yellow");
            for (int i = 0; i < snapshot.Goals.Length; i++)
            {
                var data = snapshot.Goals[i]; string id = data.Species;
                Vector2 point = new Vector2(560.5f + (i - (snapshot.Goals.Length - 1) * 0.5f) * 139.5f, 449);
                var hit = GameUI.Image(transform, id + " goal info", null, point, Vector2.one * 108);
                hit.color = Color.clear; hit.raycastTarget = true;
                var button = hit.gameObject.AddComponent<GameButton>(); button.targetGraphic = hit;
                button.transition = Selectable.Transition.None; button.navigation = new Navigation { mode = Navigation.Mode.None };
                button.onClick.AddListener(() => screen.RequestGoal(id));
                button.gameObject.AddComponent<BoardControlFeedback>().Configure(button, screen);
                var ring = HudView.Artwork(hit.transform, "Gold ring", art.Get("badge_ring_gold"),
                    GameLayout.Centered(Vector2.one * 54, firstRing.size), HudView.Pixels(5, 7, 175, 177, 192, 192));
                var portraitMask = GameUI.Image(hit.transform, "Portrait crop", art.DoorMask(), new Vector2(54, 54), new Vector2(80, 78));
                portraitMask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
                var portraitSprite = art.Optional(id + "_front");
                if (portraitSprite == null) { screen.Fail(id + "_front"); continue; }
                Rect crop = PortraitCrop(id);
                var portrait = HudView.Artwork(portraitMask.transform, "Front portrait", portraitSprite, new Rect(0, 0, 80, 78), crop);
                var life = portrait.gameObject.AddComponent<UiBirdLife>();
                life.Configure(portrait, art, id, feetPivot: false, frontFacing: true, friend: true); life.Begin();
                var gift = HudView.Artwork(hit.transform, "Species gift badge", art.Get(data.GiftKey),
                    new Rect(62, 60, 58, 53), HudView.Pixels(3, 4, 135, 137, 144, 144));
                var check = GameUI.Fit(hit.transform, "Delivery check", art.Get("badge_check"), new Vector2(91, 86.5f), new Vector2(58, 53));
                goals.Add(id, new Goal { Button = button, Portrait = portrait, Check = check.gameObject, Gift = gift.gameObject, Life = life });
                badges.Add(id, button); SetBadge(id, data.Home);
            }
            GiftPoint = bounds.position + new Vector2(center.x, bounds.height * 0.2f);
            SetResident(snapshot.ResidentSpecies); DoorPose(0);
        }
        // Front frames share a padded canvas. Crop the head and upper body inside the portrait mask.
        private static Rect PortraitCrop(string species)
        {
            switch (species)
            {
                case "sunny": return HudView.Pixels(66, 74, 189, 184, 320, 320);
                case "sky": return HudView.Pixels(63, 66, 195, 190, 320, 320);
                case "hoot": return HudView.Pixels(63, 119, 178, 174, 320, 320);
                default: return HudView.Pixels(64, 70, 192, 187, 320, 320);
            }
        }
        public void ApplyStyle(string style)
        {
            bool styled = !string.IsNullOrEmpty(style) && style != "default";
            body.sprite = styled ? art.Get("house_style_" + style + "_body") : art.Get("goal_house_single");
            roof.gameObject.SetActive(false);
        }
        public Vector2 DoorPoint()
        {
            var point = transform.InverseTransformPoint(doorway.TransformPoint(Vector3.zero));
            var area = ((RectTransform)transform).rect;
            return new Vector2(point.x - area.xMin, area.yMax - point.y);
        }
        internal void MaskEntry(RectTransform bird) => bird.SetParent(doorway, true);
        public void BeginDelivery(string species) { delivering = species; ArrivalCount = 0; }
        public void OpenDoor(Action done)
        {
            int run = BeginTransition(); face.gameObject.SetActive(false); AnimateDoor(1, done, run);
        }
        public void CloseDoor(Action done) => AnimateDoor(0, done, BeginTransition());
        private int BeginTransition()
        {
            generation++; doorMotion.CancelAll(); feedback.CancelAll(); resident?.Stop();
            glow.color = Color.clear;
            foreach (var goal in goals.Values) goal.Portrait.transform.parent.localScale = Vector3.one;
            return generation;
        }
        private void AnimateDoor(float target, Action done, int run)
        {
            float start = openness;
            doorMotion.Play(0.25f, t => { if (run == generation) DoorPose(Mathf.Lerp(start, target, t)); }, () =>
            {
                if (run != generation) return;
                DoorPose(target);
                if (target == 0 && IsHome && isActiveAndEnabled) resident?.Begin();
                done?.Invoke();
            });
        }
        private void DoorPose(float value)
        {
            openness = value; openDoorway.gameObject.SetActive(value > 0);
            openDoorway.color = new Color(1, 1, 1, value);
        }
        public void Arrival() => ArrivalCount++;
        public void CompleteFlock(Action done)
        {
            int run = BeginTransition(); string species = delivering;
            SetBadge(species, true); SetResident(species);
            var goal = goals[species];
            feedback.Play(0.3f, t =>
            {
                if (run != generation) return;
                float scale = 1 + 0.3f * Mathf.Sin(t * Mathf.PI);
                goal.Portrait.transform.parent.localScale = Vector3.one * scale;
                glow.color = new Color(1, 0.77f, 0.2f, 0.35f * Mathf.Sin(t * Mathf.PI));
            }, () =>
            {
                if (run != generation) return;
                goal.Portrait.transform.parent.localScale = Vector3.one;
                glow.color = Color.clear; AnimateDoor(0, done, run);
            });
        }
        private void SetBadge(string species, bool delivered)
        {
            if (!goals.TryGetValue(species, out var goal)) return;
            goal.Delivered = delivered;
            goal.Gift.SetActive(!delivered); goal.Check.SetActive(delivered);
        }
        public void SyncState(HudSnapshot snapshot)
        {
            bool changed = Species != snapshot.ResidentSpecies;
            foreach (var goal in snapshot.Goals) changed |= IsDelivered(goal.Species) != goal.Home;
            if (changed) BeginTransition();
            foreach (var goal in snapshot.Goals) SetBadge(goal.Species, goal.Home);
            ApplyStyle(snapshot.HouseStyle); SetResident(snapshot.ResidentSpecies);
            if (changed && IsHome && isActiveAndEnabled) resident?.Begin();
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
            face.sprite = art.Get(species + "_front");
            resident = face.gameObject.AddComponent<UiBirdLife>();
            resident.Configure(face, art, species, feetPivot: false, frontFacing: true, friend: true); resident.Begin();
        }
        public void Cancel()
        {
            if (doorMotion == null) { generation++; return; }
            BeginTransition(); delivering = null; DoorPose(0);
            if (face != null) { face.gameObject.SetActive(IsHome); if (IsHome && isActiveAndEnabled) resident?.Begin(); }
        }
        private void OnEnable()
        {
            if (resident != null && IsHome) resident.Begin();
            foreach (var goal in goals.Values) goal.Life.Begin();
        }
        private void OnDisable()
        {
            Cancel(); resident?.Stop();
            foreach (var goal in goals.Values) goal.Life.Stop();
        }
        private void OnDestroy() => Cancel();
    }
}
