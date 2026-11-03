using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BirdhouseView : MonoBehaviour
    {
        private RectTransform chip;
        private GameObject check;
        private readonly Image[] dots = new Image[4];
        private V5Motion motion;
        private Rect bounds;
        private V5Art art;
        public string Species { get; private set; }
        public Vector2 GiftPoint { get; private set; }
        public bool IsHome { get; private set; }
        public int ArrivalCount { get; private set; }
        public void Configure(V5GameRoot root, V5Art assets, string species, int index, int count)
        {
            art = assets; Species = species; bounds = V7Layout.GoalChip(index, count);
            chip = V5UI.Rect(transform, "Goal chip", bounds.center, bounds.size);
            var target = chip.gameObject.AddComponent<Image>(); target.color = Color.clear; target.raycastTarget = true;
            chip.gameObject.AddComponent<V5LongPress>().Configure(() => root.OpenGoalInfo(Species));
            var mask = V5UI.Image(chip, "Doorway mask", art.DoorMask(), V7Layout.GoalFace.center, V7Layout.GoalFace.size);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            V5UI.Fit(mask.transform, "Species face", art.Portrait(species), V7Layout.GoalFace.size * 0.5f, V7Layout.GoalFace.size);
            var roof = V5UI.Fit(chip, "Tinted roof", art.Get("goal_house_roof"), V7Layout.GoalHouseCenter, V7Layout.GoalHouseSize);
            roof.color = SpeciesColor(species);
            V5UI.Fit(chip, "House doorway", art.Get("goal_house_body"), V7Layout.GoalHouseCenter, V7Layout.GoalHouseSize);
            V5UI.Fit(chip, "Gift", art.Get(V5ViewModel.GiftIcon(species)), V7Layout.GoalGiftCenter, Vector2.one * V7Layout.GiftBadgeSize);
            GiftPoint = bounds.position + V7Layout.GoalGiftCenter;
            for (int i = 0; i < dots.Length; i++) dots[i] = V5UI.Fit(chip, "Progress " + i, art.Get("goal_dot_empty"),
                V7Layout.GoalDot(i), Vector2.one * V7Layout.GoalDotSize);
            check = V5UI.Fit(chip, "Home check", art.Get("goal_check"), V7Layout.GoalCheckCenter, Vector2.one * V7Layout.GoalCheckSize).gameObject;
            motion = gameObject.AddComponent<V5Motion>(); SetHome(false);
        }
        public static Color SpeciesColor(string species)
        {
            switch (species)
            {
                case "sunny": return new Color32(255, 218, 64, 255);
                case "sky": return new Color32(78, 192, 241, 255);
                case "berry": return new Color32(227, 65, 142, 255);
                case "mint": return new Color32(74, 205, 170, 255);
                case "cocoa": return new Color32(153, 100, 67, 255);
                case "snow": return new Color32(220, 205, 245, 255);
                case "ember": return new Color32(255, 143, 49, 255);
                case "ink": return new Color32(64, 82, 154, 255);
                default: return new Color32(218, 180, 117, 255);
            }
        }
        public Vector2 DoorPoint()
        {
            Vector2 local = V7Layout.GoalDoorLocal - bounds.size * 0.5f;
            var point = transform.InverseTransformPoint(chip.TransformPoint(new Vector3(local.x, -local.y, 0)));
            var area = ((RectTransform)transform).rect;
            return new Vector2(point.x - area.xMin, area.yMax - point.y);
        }
        public void Arrival()
        {
            ArrivalCount = Mathf.Min(4, ArrivalCount + 1); DrawProgress();
            motion.CancelAll(); motion.Play(0.2f, t => chip.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f), () => chip.localScale = Vector3.one);
        }
        public void SetHome(bool value)
        {
            IsHome = value; ArrivalCount = value ? 4 : 0; check.SetActive(value); DrawProgress();
        }
        private void DrawProgress() { for (int i = 0; i < dots.Length; i++) dots[i].sprite = art.Get(i < ArrivalCount ? "goal_dot_full" : "goal_dot_empty"); }
        public void Cancel() { motion.CancelAll(); chip.localScale = Vector3.one; }
    }
}
