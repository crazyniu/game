using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BirdhouseView : MonoBehaviour
    {
        private RectTransform house;
        private GameObject check;
        private V5Motion motion;
        private Rect bounds;
        public string Species { get; private set; }
        public Vector2 GiftPoint { get; private set; }
        public bool IsHome { get; private set; }
        public int ArrivalCount { get; private set; }
        public void Configure(V5GameRoot root, V5Art art, string species, int index, int count)
        {
            Species = species; bounds = V7Layout.HouseCanvas(index, count);
            house = V5UI.Rect(transform, "Birdhouse", bounds.center, bounds.size);
            var target = house.gameObject.AddComponent<Image>(); target.color = Color.clear; target.raycastTarget = true;
            house.gameObject.AddComponent<V5LongPress>().Configure(() => root.OpenGoalInfo(Species));
            Vector2 center = bounds.size * 0.5f;
            V5UI.Image(house, "House body", art.Get("house_body"), center, bounds.size);
            Vector2 door = V7Layout.HouseDoorLocal(bounds.size);
            float diameter = V7Layout.HouseDoorDiameter(bounds.width);
            var mask = V5UI.Image(house, "Doorway mask", art.DoorMask(), door, Vector2.one * diameter);
            mask.gameObject.AddComponent<Mask>().showMaskGraphic = false;
            V5UI.Fit(mask.transform, "Species face", art.Portrait(species), Vector2.one * diameter * 0.5f, Vector2.one * diameter);
            V5UI.Image(house, "Colored roof", art.Get(Roof(species)), center, bounds.size);
            check = V5UI.Fit(house, "Home check", art.Get("check_green"), new Vector2(bounds.width * 0.78f, bounds.height * 0.78f), Vector2.one * (count == 1 ? 62 : 44)).gameObject;
            GiftPoint = bounds.position + new Vector2(center.x, bounds.height * 0.2f);
            motion = gameObject.AddComponent<V5Motion>(); SetHome(false);
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
            Vector2 local = V7Layout.HouseDoorLocal(bounds.size) - bounds.size * 0.5f;
            var point = transform.InverseTransformPoint(house.TransformPoint(new Vector3(local.x, -local.y, 0)));
            var area = ((RectTransform)transform).rect;
            return new Vector2(point.x - area.xMin, area.yMax - point.y);
        }
        public void Arrival()
        {
            ArrivalCount = Mathf.Min(4, ArrivalCount + 1);
            motion.CancelAll(); motion.Play(0.2f, t => house.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f), () => house.localScale = Vector3.one);
        }
        public void SetHome(bool value) { IsHome = value; ArrivalCount = value ? 4 : 0; check.SetActive(value); }
        public void Cancel() { motion.CancelAll(); house.localScale = Vector3.one; }
    }
}
