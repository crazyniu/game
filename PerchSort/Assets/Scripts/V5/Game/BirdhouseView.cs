using System;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BirdhouseView : MonoBehaviour
    {
        private RectTransform pivot, house, door;
        private Image glow;
        private GameObject check;
        private V5Motion motion, sway;
        private Rect bounds;
        private V5Art art;
        public string Species { get; private set; }
        public Vector2 GiftPoint { get; private set; }
        public bool IsHome { get; private set; }
        public void Configure(V5Art assets, string species, int index, int count, Transform ropes)
        {
            art = assets; Species = species; bounds = V5Layout.SpeciesHouse(index, count);
            float scale = bounds.width / 300;
            art.Rope(ropes, "House rope " + species, bounds.center.x, V5Layout.RopeStart(bounds.center.x), bounds.yMin + 5 * scale);
            pivot = V5UI.Rect(transform, "Hook", new Vector2(bounds.center.x, bounds.yMin), bounds.size);
            pivot.pivot = new Vector2(0.5f, 1);
            house = V5UI.Rect(pivot, "House body", new Vector2(bounds.width / 2, 0), bounds.size);
            house.pivot = new Vector2(0.5f, 1);
            glow = V5UI.Image(house, "Home glow", art.Get("sparkle"), bounds.size * 0.5f, bounds.size * 1.15f);
            glow.color = Color.clear;
            V5UI.Image(house, "Birdhouse", art.Get("birdhouse"), bounds.size * 0.5f, bounds.size);
            var band = V5UI.Image(house, "Species roof band", art.Get("roof_band"), new Vector2(150, 133) * scale, new Vector2(300, 60) * scale);
            band.color = SpeciesColor(species);
            door = V5UI.Image(house, "Door shutter", art.Get("badge_count"), new Vector2(114, 215) * scale, Vector2.one * 72 * scale).rectTransform;
            door.pivot = new Vector2(0, 0.5f); door.GetComponent<Image>().color = new Color32(139, 88, 41, 255);
            var emblem = V5UI.Image(house, "Resident", art.Get("bird_" + species + "_idle"), new Vector2(150, 292) * scale, Vector2.one * 55 * scale);
            emblem.color = Color.white;
            Rect sign = V5Layout.GiftSign(index, count); GiftPoint = sign.center;
            art.Rope(transform, "Sign rope", sign.center.x, bounds.yMax, sign.yMin, 4);
            var plaque = V5UI.Image(transform, "Gift sign", art.Get("btn_square_wood"), sign.center, sign.size);
            V5UI.Image(plaque.transform, "Gift", art.Get(V5ViewModel.GiftIcon(species)), sign.size * 0.5f, sign.size * 0.8f);
            check = V5UI.Rect(house, "Home check", new Vector2(240, 260) * scale, Vector2.one * 36 * scale).gameObject;
            var shortStroke = V5UI.Image(check.transform, "Short stroke", null, new Vector2(9, 22) * scale, new Vector2(19, 6) * scale);
            var longStroke = V5UI.Image(check.transform, "Long stroke", null, new Vector2(24, 17) * scale, new Vector2(32, 6) * scale);
            shortStroke.color = longStroke.color = new Color32(54, 150, 65, 255);
            shortStroke.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);
            longStroke.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            motion = gameObject.AddComponent<V5Motion>(); sway = gameObject.AddComponent<V5Motion>();
            SetHome(false); StartSway();
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
            Vector3 local = transform.InverseTransformPoint(house.TransformPoint(new Vector3(0, -215 * bounds.width / 300, 0)));
            Rect area = ((RectTransform)transform).rect;
            return new Vector2(local.x - area.xMin, area.yMax - local.y);
        }
        private void StartSway()
        {
            sway.Loop(4, t => pivot.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2) * 2));
        }
        public void OpenDoor(Action done) => motion.Play(0.15f, t => door.localRotation = Quaternion.Euler(0, 85 * t, 0), done);
        public void CloseDoor(Action done) => motion.Play(0.15f, t => door.localRotation = Quaternion.Euler(0, 85 * (1 - t), 0), done);
        public void Arrival()
        {
            motion.Play(0.2f, t => house.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f));
            var note = V5UI.Image(transform, "Chimney note", art.Get("gift_encore"), new Vector2(bounds.xMax - 20, bounds.yMin + 100), Vector2.one * 32);
            Vector2 start = note.rectTransform.anchoredPosition;
            motion.Play(0.55f, t =>
            {
                note.rectTransform.anchoredPosition = start + new Vector2(25 * Mathf.Sin(t * Mathf.PI), 80 * t);
                note.color = new Color(1, 1, 1, 1 - t);
            }, () => V5UI.Dispose(note.gameObject));
        }
        public void SetHome(bool value)
        {
            IsHome = value; check.SetActive(value); glow.color = value ? new Color(1, 0.9f, 0.35f, 0.6f) : Color.clear;
        }
        public void Cancel()
        {
            motion.CancelAll(); sway.CancelAll(); house.localScale = Vector3.one; door.localRotation = Quaternion.identity;
            pivot.localRotation = Quaternion.identity;
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name == "Chimney note") V5UI.Dispose(transform.GetChild(i).gameObject);
            StartSway();
        }
    }
}
