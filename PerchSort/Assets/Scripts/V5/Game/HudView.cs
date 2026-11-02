using System;
using System.Collections.Generic;
using PerchSort.V5.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class HudView : MonoBehaviour
    {
        private V5Art art;
        private TextMeshProUGUI coins;
        private RectTransform house, housePivot;
        private V5Motion houseMotion, houseSway;
        private readonly List<string> species = new List<string>();
        private Image[] homeIcons;
        private Outline[] homeOutlines;
        private GameObject[] homeChecks;
        private Button undo, hint, add, settings;
        private GameObject stuck;
        public Func<Vector2> DoorPosition { get; private set; }

        public void Configure(V5GameRoot root, V5Art assets, TMP_FontAsset font, int levelId)
        {
            art = assets;
            V5UI.Image(transform, "Top limb", art.Get("limb_top"), V5Layout.Limb.center, V5Layout.Limb.size);
            V5UI.Image(transform, "Coin pill", art.Get("pill_cream"), V5Layout.CoinPill.center, V5Layout.CoinPill.size);
            V5UI.Image(transform, "Coin", art.Get("coin"), V5Layout.CoinIcon.center, V5Layout.CoinIcon.size);
            coins = V5UI.Text(transform, "Coins", "0", font, V5Layout.CoinNumber.center, V5Layout.CoinNumber.size, V5Layout.CoinFontSize);
            coins.alignment = TextAlignmentOptions.MidlineLeft;
            coins.enableAutoSizing = true;
            coins.fontSizeMin = V5Layout.CoinMinimumFontSize;
            coins.fontSizeMax = V5Layout.CoinFontSize;
            settings = V5UI.Button(transform, art, "Settings", "icon_settings", V5Layout.SettingsButton.center, root.OpenSettings);

            housePivot = V5UI.Rect(transform, "Birdhouse hook pivot", V5Layout.HookPoint, V5Layout.House.size);
            housePivot.pivot = new Vector2(0.5f, 1);
            house = V5UI.Image(housePivot, "Birdhouse", art.Get("birdhouse"), V5Layout.House.size * 0.5f, V5Layout.House.size).rectTransform;
            house.pivot = new Vector2(0.5f, 1);
            house.anchoredPosition = new Vector2(V5Layout.HouseWidth / 2, 0);
            houseMotion = house.gameObject.AddComponent<V5Motion>();
            houseSway = housePivot.gameObject.AddComponent<V5Motion>();
            DoorPosition = GetDoorPoint;
            StartSway();
            BuildTracker(root.Session.State, font, levelId);

            undo = V5UI.Button(transform, art, "Undo", "icon_undo", V5Layout.ToolButton(0).center, () => root.Undo());
            hint = V5UI.Button(transform, art, "Hint", "icon_hint", V5Layout.ToolButton(1).center, root.Hint);
            add = V5UI.Button(transform, art, "Add branch", "icon_add_branch", V5Layout.ToolButton(2).center, () => root.AddBranch());
            var bubble = V5UI.Image(transform, "Stuck assistance", art.Get("pill_cream"), V5Layout.StuckBubble.center, V5Layout.StuckBubble.size);
            V5UI.Text(bubble.transform, "Stuck message", "Try Undo or Add Branch", font,
                V5Layout.StuckBubble.size * 0.5f, V5Layout.StuckBubble.size - Vector2.right * V5Layout.StuckTextPadding * 2, V5Layout.StuckFontSize);
            foreach (int index in new[] { 0, 2 })
                V5UI.Image(bubble.transform, "Tool pointer", art.Get("hand_pointer"),
                    new Vector2(V5Layout.ToolButton(index).center.x - V5Layout.StuckBubble.xMin,
                        V5Layout.ToolButton(index).yMin - V5Layout.StuckBubble.yMin - V5Layout.StuckPointerSize / 2),
                    Vector2.one * V5Layout.StuckPointerSize);
            stuck = bubble.gameObject;
            stuck.SetActive(false);
        }

        private void BuildTracker(BoardState state, TMP_FontAsset font, int levelId)
        {
            foreach (var branch in state.Branches)
                foreach (var bird in branch.Birds)
                    if (!species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            var tracker = V5UI.Image(transform, "Flock tracker", art.Get("pill_cream"), V5Layout.Tracker.center, V5Layout.Tracker.size);
            Rect badgeBounds = V5Layout.LevelBadge(species.Count);
            var badge = V5UI.Image(tracker.transform, "Level badge", art.Get(V5Layout.SecondaryButtonSprite),
                V5Layout.InParent(V5Layout.Tracker, badgeBounds.center), badgeBounds.size);
            V5UI.Text(badge.transform, "Level", levelId.ToString(), font, badgeBounds.size * 0.5f, badgeBounds.size, V5Layout.LevelBadgeFontSize);
            homeIcons = new Image[species.Count];
            homeOutlines = new Outline[species.Count];
            homeChecks = new GameObject[species.Count];
            for (int i = 0; i < species.Count; i++)
            {
                Rect bounds = V5Layout.SpeciesIcon(i, species.Count);
                var icon = V5UI.Image(tracker.transform, species[i] + " home", art.Get("bird_" + species[i] + "_idle"),
                    V5Layout.InParent(V5Layout.Tracker, bounds.center), bounds.size);
                homeIcons[i] = icon;
                homeOutlines[i] = icon.gameObject.AddComponent<Outline>();
                homeOutlines[i].effectColor = V5UI.Ink;
                homeOutlines[i].effectDistance = new Vector2(V5Layout.SpeciesOutlineWidth, -V5Layout.SpeciesOutlineWidth);
                homeOutlines[i].useGraphicAlpha = false;
                homeChecks[i] = CreateCheck(icon.transform);
            }
        }

        private static GameObject CreateCheck(Transform parent)
        {
            float size = V5Layout.HomeCheckSize;
            var check = V5UI.Rect(parent, "Home check", Vector2.one * V5Layout.SpeciesIconSize, Vector2.one * size);
            var shortStroke = V5UI.Image(check, "Check short stroke", null,
                new Vector2(size * 0.23f, size * 0.6f), new Vector2(size * 0.45f, V5Layout.HomeCheckStroke));
            var longStroke = V5UI.Image(check, "Check long stroke", null,
                new Vector2(size * 0.65f, size * 0.48f), new Vector2(size * 0.8f, V5Layout.HomeCheckStroke));
            shortStroke.color = longStroke.color = new Color32(54, 150, 65, 255);
            shortStroke.rectTransform.localRotation = Quaternion.Euler(0, 0, -45);
            longStroke.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            check.gameObject.SetActive(false);
            return check.gameObject;
        }

        private void StartSway()
        {
            houseSway.CancelAll();
            housePivot.localRotation = Quaternion.identity;
            houseSway.Loop(V5Layout.HouseSwaySeconds, t => housePivot.localRotation =
                Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 2) * V5Layout.HouseSwayDegrees));
        }

        private Vector2 GetDoorPoint()
        {
            Vector3 local = transform.InverseTransformPoint(house.TransformPoint(new Vector3(0, -V5Layout.HouseDoorFromHook, 0)));
            Rect bounds = ((RectTransform)transform).rect;
            return new Vector2(local.x - bounds.xMin, bounds.yMax - local.y);
        }

        public void Refresh(GameSession session, int coinCount, bool locked)
        {
            coins.text = coinCount.ToString();
            undo.interactable = !locked && session.CanUndo;
            hint.interactable = !locked && !session.State.IsWon;
            add.interactable = !locked && !session.State.IsWon && !session.State.AddedBranch;
            settings.interactable = !locked;
            for (int i = 0; i < species.Count; i++)
            {
                bool home = false;
                foreach (var homed in session.State.HomedSpecies) if (homed.Id == species[i]) home = true;
                homeIcons[i].color = new Color(1, 1, 1, home ? 1 : V5Layout.UnhomedAlpha);
                homeOutlines[i].enabled = !home;
                homeChecks[i].SetActive(home);
            }
            stuck.SetActive(!locked && !session.State.IsWon && !GameSession.HasLegalMove(session.State));
        }

        public void HomeArrival()
        {
            houseMotion.CancelAll();
            houseMotion.Play(0.2f, t => house.localScale = Vector3.one * (1 + Mathf.Sin(t * Mathf.PI) * 0.1f));
            if (V5Motion.Instant) return;
            Vector2 door = GetDoorPoint();
            for (int i = 0; i < 6; i++)
            {
                float angle = i * Mathf.PI / 3;
                Vector2 delta = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 95;
                var sparkle = V5UI.Image(transform, "Home sparkle", art.Get("sparkle"), door, new Vector2(38, 38));
                sparkle.gameObject.AddComponent<V5Motion>().Play(0.4f, t =>
                {
                    Vector2 point = door + delta * t;
                    sparkle.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                    sparkle.color = new Color(1, 1, 1, 1 - t);
                }, () => V5UI.Dispose(sparkle.gameObject));
            }
        }

        public void Cancel()
        {
            foreach (var motion in GetComponentsInChildren<V5Motion>(true)) motion.CancelAll();
            house.localScale = Vector3.one;
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name == "Home sparkle") V5UI.Dispose(transform.GetChild(i).gameObject);
            StartSway();
        }
    }
}
