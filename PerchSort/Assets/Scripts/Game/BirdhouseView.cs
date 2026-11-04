using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class BirdhouseView : MonoBehaviour
    {
        private RectTransform house, doorway;
        private Image door, face, openDoorway;
        private GameObject check;
        private GameMotion motion, doorMotion, checkMotion, particles, peekMotion;
        private GameArt art;
        private Rect bounds;
        private float openness;
        private int generation;
        public string Species { get; private set; }
        public Vector2 GiftPoint { get; private set; }
        public bool IsHome { get; private set; }
        public int ArrivalCount { get; private set; }
        internal float EntryRadius => GameLayout.HouseDoorDiameter(bounds.width) * 0.5f;
        internal static bool UsesBeam(int count) => count > 3;
        internal static Rect CanvasBounds(int index, int count)
        {
            // Visible house art spans (32, 58)..(480, 456) on the shared canvas.
            // Fit that region, rather than a square that makes seven roofs overlap.
            Rect visible;
            if (UsesBeam(count)) visible = GameLayout.House(index, count);
            else
            {
                Rect master = GameLayout.Screen("level_k3", "house");
                // Three suspension points must clear both the coin pill and gear.
                float center = count == 3 ? 566 + (index - 1) * 266
                    : master.center.x + (index - (count - 1) * 0.5f) * 290;
                visible = new Rect(center - master.width * 0.5f, count == 1 ? master.y : 260, master.width, master.height);
            }
            Vector2 size = new Vector2(visible.width * 512 / 448, visible.height * 512 / 398);
            return new Rect(visible.x - size.x * 32 / 512, visible.y - size.y * 58 / 512, size.x, size.y);
        }
        public void Configure(GameRoot root, GameArt art, string species, int index, int count)
        {
            this.art = art; Species = species; bounds = CanvasBounds(index, count);
            house = GameUI.Rect(transform, "Birdhouse", bounds.center, bounds.size);
            var target = house.gameObject.AddComponent<Image>(); target.color = Color.clear; target.raycastTarget = true;
            house.gameObject.AddComponent<LongPress>().Configure(() => root.OpenGoalInfo(Species));
            Vector2 center = bounds.size * 0.5f;
            GameUI.Image(house, "House body", art.Get("house_body"), center, bounds.size);
            Vector2 doorPoint = GameLayout.HouseDoorLocal(bounds.size);
            Vector2 opening = bounds.size * (204f / 512);
            var open = art.Optional("house_door_open");
            if (open != null) openDoorway = GameUI.Image(house, "Open doorway", open, center, bounds.size);
            else GameArt.Warn("motion:house_door_open", "Missing open doorway part; using the house-body doorway.");
            var mask = GameUI.Image(house, "Doorway mask", art.DoorMask(), doorPoint, opening);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false; doorway = mask.rectTransform;
            face = GameUI.Fit(doorway, "Species face", art.Portrait(species), opening * 0.5f, opening * (UsesBeam(count) ? 1.35f : 1));
            GameUI.Image(house, "Colored roof", art.Get(Roof(species)), center, bounds.size);
            var leaf = art.Optional("house_door_leaf");
            if (leaf != null)
            {
                // The padded T48 door occupies (145, 198, 216, 216) in a 512px image.
                // Put the transform pivot on its visible hinge without allocating a cropped sprite.
                door = GameUI.Image(house, "Door leaf", leaf, doorPoint, bounds.size);
                door.rectTransform.pivot = new Vector2(145 / 512f, 206 / 512f);
                door.rectTransform.anchoredPosition += Vector2.left * bounds.width * 108 / 512;
            }
            else GameArt.Warn("motion:house_door_leaf", "Missing door leaf; using whole-house scale during door motion.");
            float checkSize = UsesBeam(count) ? 44 : 62;
            check = GameUI.Rect(house, "Home check", new Vector2(bounds.width * 0.78f, bounds.height * 0.78f), Vector2.one * checkSize).gameObject;
            GameUI.Fit(check.transform, "Green badge", art.Get("btn_green"), Vector2.one * checkSize * 0.5f, Vector2.one * checkSize);
            GameUI.Fit(check.transform, "Check", art.Icon("icon_check"), Vector2.one * checkSize * 0.5f, Vector2.one * checkSize * 0.7f);
            GiftPoint = bounds.position + new Vector2(center.x, bounds.height * 0.2f);
            motion = gameObject.AddComponent<GameMotion>(); doorMotion = gameObject.AddComponent<GameMotion>();
            checkMotion = gameObject.AddComponent<GameMotion>(); particles = gameObject.AddComponent<GameMotion>(); peekMotion = gameObject.AddComponent<GameMotion>();
            SetHome(false); DoorPose(0);
        }
        private static string Roof(string species)
        {
            switch (species)
            {
                case "sunny": return "house_roof_teal";
                case "berry": return "house_roof_red";
                case "sky": return "house_roof_yellow";
                case "cocoa": return "house_roof_purple";
                case "mint": return "house_roof_green";
                case "snow": return "house_roof_blue";
                case "ember": return "house_roof_orange";
                default: return "house_roof_pink";
            }
        }
        public Vector2 DoorPoint()
        {
            Vector2 local = GameLayout.HouseDoorLocal(bounds.size) - bounds.size * 0.5f;
            var point = transform.InverseTransformPoint(house.TransformPoint(new Vector3(local.x, -local.y, 0)));
            var area = ((RectTransform)transform).rect;
            return new Vector2(point.x - area.xMin, area.yMax - point.y);
        }
        internal void MaskEntry(RectTransform bird) => bird.SetParent(doorway, true);
        public void OpenDoor(Action done)
        {
            face.gameObject.SetActive(false); AnimateDoor(1, done);
        }
        public void CloseDoor(Action done) => AnimateDoor(0, done);
        private void AnimateDoor(float target, Action done)
        {
            doorMotion.CancelAll(); float start = openness; int run = generation;
            doorMotion.Play(0.18f, t => DoorPose(Mathf.Lerp(start, target, t)), () =>
            {
                if (run != generation) return;
                DoorPose(target); done?.Invoke();
            });
        }
        private void DoorPose(float value)
        {
            openness = value;
            if (openDoorway != null) openDoorway.gameObject.SetActive(value > 0);
            if (door != null)
            {
                // Resting houses retain the approved visible portrait; only the moving leaf covers it.
                door.gameObject.SetActive(value > 0 && value < 1);
                door.rectTransform.localScale = new Vector3(Mathf.Lerp(1, 0.12f, value), 1, 1);
                door.rectTransform.localRotation = Quaternion.Euler(0, 0, -5 * value);
                door.color = new Color(1, 1, 1, 1 - 0.35f * value);
            }
            else house.localScale = Vector3.one * (1 + 0.025f * Mathf.Sin(value * Mathf.PI));
        }
        public void Peek(bool visible, Action done)
        {
            peekMotion.CancelAll(); int run = generation;
            Vector2 opening = doorway.sizeDelta;
            face.gameObject.SetActive(true);
            peekMotion.Play(0.2f, t =>
            {
                float progress = visible ? t : 1 - t;
                face.rectTransform.anchoredPosition = new Vector2(opening.x * 0.5f, -opening.y * 0.5f - opening.y * (1 - progress));
            }, () => { if (run == generation) { face.gameObject.SetActive(visible); done?.Invoke(); } });
        }
        public void Arrival()
        {
            ArrivalCount = Mathf.Min(4, ArrivalCount + 1);
            motion.CancelAll(); motion.Play(0.2f, t => house.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f), () => house.localScale = Vector3.one);
            Sparkles();
        }
        private void Sparkles()
        {
            for (int i = 0; i < 5; i++)
            {
                float angle = (i * 36 + 18) * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                var spark = GameUI.Image(house, "Arrival sparkle", art.Get("sparkle"), bounds.size * 0.5f, Vector2.one * (bounds.width * 0.09f));
                particles.Play(0.5f, t =>
                {
                    spark.rectTransform.anchoredPosition = new Vector2(bounds.width * 0.5f, -bounds.height * 0.5f) + direction * (bounds.width * 0.5f * t);
                    spark.rectTransform.localScale = Vector3.one * (1 - t);
                    spark.color = new Color(1, 1, 0.8f, 1 - t);
                }, () => GameUI.Dispose(spark.gameObject));
            }
        }
        public void CompleteFlock(Action done)
        {
            int run = generation;
            CloseDoor(() =>
            {
                if (run != generation) return;
                SetHome(true); check.transform.localScale = Vector3.zero;
                checkMotion.Play(0.25f, t => check.transform.localScale = Vector3.one * t,
                    () => { if (run == generation) { check.transform.localScale = Vector3.one; done?.Invoke(); } }, ease: Ease.OutBack);
            });
        }
        public void SetHome(bool value)
        {
            IsHome = value; ArrivalCount = value ? 4 : 0; check.SetActive(value);
            face.gameObject.SetActive(true); face.rectTransform.anchoredPosition = new Vector2(doorway.sizeDelta.x * 0.5f, -doorway.sizeDelta.y * 0.5f);
            check.transform.localScale = Vector3.one;
        }
        public void Cancel()
        {
            generation++;
            if (motion == null) return;
            motion.CancelAll(); doorMotion.CancelAll(); checkMotion.CancelAll(); particles.CancelAll(); peekMotion.CancelAll();
            house.localScale = Vector3.one; DoorPose(0); SetHome(IsHome);
            for (int i = house.childCount - 1; i >= 0; i--)
                if (house.GetChild(i).name == "Arrival sparkle") GameUI.Dispose(house.GetChild(i).gameObject);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
