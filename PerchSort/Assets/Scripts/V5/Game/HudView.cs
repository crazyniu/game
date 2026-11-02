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
        private V5GameRoot root;
        private TMP_FontAsset font;
        private TextMeshProUGUI coins, phaseCount, chorusLabel;
        private Image phaseIcon;
        private GameObject phaseGroup, feather, stuck, chorus;
        private V5Motion motion;
        private RectTransform housesLayer, ropesLayer, effects;
        private readonly Dictionary<string, BirdhouseView> houses = new Dictionary<string, BirdhouseView>();
        private readonly Image[] notes = new Image[5];
        private readonly Button[] tools = new Button[4];
        private readonly Image[] badges = new Image[4];
        private readonly TextMeshProUGUI[] counts = new TextMeshProUGUI[4];
        private readonly BoosterKind[] kinds = new BoosterKind[4];
        private readonly int[] inventory = new int[V5ViewModel.BoosterCount];
        private Button settings;
        private bool contextMechanic;
        private TextMeshProUGUI levelNumber;
        public IReadOnlyDictionary<string, BirdhouseView> Houses => houses;
        public int DisplayedSong { get; private set; }
        public BoosterKind ContextBooster => kinds[3];

        public void Configure(V5GameRoot owner, V5Art assets, TMP_FontAsset typeface)
        {
            root = owner; art = assets; font = typeface;
            motion = gameObject.AddComponent<V5Motion>();
            art.Rope(transform, "Level sign rope", V5Layout.LevelSign.center.x, V5Layout.RopeStart(V5Layout.LevelSign.center.x), V5Layout.LevelSign.yMin);
            ropesLayer = V5UI.Rect(transform, "House ropes", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
            V5UI.Image(transform, "Top limb", art.OptionalRopeArt("limb_top_plain") ?? art.Get("limb_top"), V5Layout.Limb.center, V5Layout.Limb.size);
            V5UI.Image(transform, "Coin pill", art.Get("pill_cream"), V5Layout.CoinPill.center, V5Layout.CoinPill.size);
            V5UI.Image(transform, "Coin", art.Get("coin"), V5Layout.CoinIcon.center, V5Layout.CoinIcon.size);
            coins = V5UI.Text(transform, "Coins", "0", font, V5Layout.CoinNumber.center, V5Layout.CoinNumber.size, 46);
            coins.alignment = TextAlignmentOptions.MidlineLeft; coins.enableAutoSizing = true; coins.fontSizeMin = 20; coins.fontSizeMax = 46;
            settings = V5UI.Button(transform, art, "Settings", "icon_settings", V5Layout.SettingsButton.center, root.OpenSettings);
            var sign = V5UI.Image(transform, "Level sign (hold for picker)", art.Get("btn_square_wood"), V5Layout.LevelSign.center, V5Layout.LevelSign.size);
            sign.raycastTarget = true; sign.gameObject.AddComponent<V5LongPress>().Configure(root.OpenLevelPicker);
            levelNumber = V5UI.Text(sign.transform, "Level", "1", font, V5Layout.LevelSign.size * 0.5f, V5Layout.LevelSign.size, 44);
            var song = V5UI.Image(transform, "Song meter", art.Get("pill_cream"), V5Layout.SongPill.center, V5Layout.SongPill.size);
            for (int i = 0; i < notes.Length; i++)
                notes[i] = V5UI.Image(song.transform, "Song note " + i, art.Get("gift_encore"), new Vector2(40 + i * 60, 45), Vector2.one * 40);
            phaseGroup = V5UI.Rect(transform, "Day night dial", V5Layout.PhaseDial.center, V5Layout.PhaseDial.size).gameObject;
            phaseIcon = V5UI.Image(phaseGroup.transform, "Sun moon", art.Get("dial_sun"), new Vector2(50, 40), Vector2.one * 80);
            phaseCount = V5UI.Text(phaseGroup.transform, "Moves until phase", "0", font, new Vector2(50, 110), new Vector2(90, 40), 32);
            housesLayer = V5UI.Rect(transform, "Species houses", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
            tools[0] = V5UI.Button(transform, art, "Undo", "icon_undo", V5Layout.ToolButton(0).center, () => root.Undo());
            for (int i = 1; i < tools.Length; i++)
            {
                int slot = i;
                tools[i] = V5UI.Button(transform, art, "Booster " + i, V5ViewModel.Icon((BoosterKind)(i - 1)),
                    V5Layout.ToolButton(i).center, () => root.TapBooster(kinds[slot]));
                var icon = (RectTransform)tools[i].transform.GetChild(0);
                icon.anchoredPosition = new Vector2(60, -105); icon.sizeDelta = Vector2.one * 78;
                // Badge is inside the button bounds, preserving the 60px inter-button clearance.
                badges[i] = V5UI.Image(tools[i].transform, "Inventory badge", art.Get("badge_count"), new Vector2(118, 32), Vector2.one * 64);
                counts[i] = V5UI.Text(badges[i].transform, "Owned", "0", font, Vector2.one * 32, Vector2.one * 60, 30);
            }
            feather = V5UI.Image(transform, "Light step pending", art.Get("gift_lightstep"), V5Layout.LightStepPoint, Vector2.one * 70).gameObject;
            var bubble = V5UI.Image(transform, "Stuck assistance", art.Get("pill_cream"), V5Layout.StuckBubble.center, V5Layout.StuckBubble.size);
            foreach (int slot in new[] { 0, 1, 2 })
                V5UI.Image(bubble.transform, "Assistance pointer", art.Get("hand_pointer"),
                    new Vector2(V5Layout.ToolButton(slot).center.x - V5Layout.StuckBubble.xMin, 50), Vector2.one * 60);
            stuck = bubble.gameObject;
            chorus = V5UI.Image(transform, "Chorus ribbon", art.Get("ribbon"), new Vector2(630, 690), new Vector2(250, 65)).gameObject;
            chorusLabel = V5UI.Text(chorus.transform, "Chorus multiplier", "", font, new Vector2(125, 32), new Vector2(160, 60), 42);
            chorus.SetActive(false);
            effects = V5UI.Rect(transform, "HUD particles", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
        }
        public void SetLevel(BoardState state)
        {
            Cancel();
            foreach (var house in houses.Values) V5UI.Dispose(house.gameObject);
            for (int i = ropesLayer.childCount - 1; i >= 0; i--) V5UI.Dispose(ropesLayer.GetChild(i).gameObject);
            houses.Clear();
            var species = new List<string>();
            foreach (var branch in state.Branches)
                foreach (var bird in branch.Birds)
                    if (!bird.Species.IsCrow && !species.Contains(bird.Species.Id)) species.Add(bird.Species.Id);
            foreach (var homed in state.HomedSpecies) if (!species.Contains(homed.Id)) species.Add(homed.Id);
            for (int i = 0; i < species.Count; i++)
            {
                var house = V5UI.Rect(housesLayer, species[i] + " house", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize).gameObject.AddComponent<BirdhouseView>();
                house.Configure(art, species[i], i, species.Count, ropesLayer); houses.Add(species[i], house);
            }
            kinds[1] = BoosterKind.Firefly; kinds[2] = BoosterKind.Sapling;
            kinds[3] = V5ViewModel.ContextBooster(state, out contextMechanic);
            for (int i = 1; i < tools.Length; i++)
            {
                tools[i].name = kinds[i].ToString(); tools[i].transform.GetChild(0).GetComponent<Image>().sprite = art.Get(V5ViewModel.Icon(kinds[i]));
                tools[i].gameObject.SetActive(state.LevelId >= V5ViewModel.Unlock(kinds[i]));
            }
            levelNumber.text = state.LevelId.ToString();
            SyncState(state);
        }
        public BirdhouseView House(string species) => houses[species];
        public Vector2 BoosterPoint(BoosterKind kind)
        {
            for (int i = 1; i < kinds.Length; i++) if (kinds[i] == kind) return V5Layout.ToolButton(i).center;
            return V5Layout.ToolButton(3).center;
        }
        public void SyncState(BoardState state)
        {
            for (int i = 0; i < inventory.Length; i++) inventory[i] = state.Inventory[(BoosterKind)i];
            DrawCounts();
            foreach (var house in houses.Values)
            {
                bool home = false; foreach (var species in state.HomedSpecies) if (species.Id == house.Species) home = true;
                house.SetHome(home);
            }
            SetNotes(state.SongNotes); Phase(state.Phase, state.DayLength - state.PhaseCounter, state.DayLength > 0);
            feather.SetActive(state.LightStep);
        }
        public void Refresh(GameSession session, int coinCount, bool blocked, bool presenting)
        {
            coins.text = coinCount.ToString();
            if (!presenting) SyncState(session.State);
            tools[0].interactable = session.CanUndo;
            settings.interactable = !blocked;
            for (int i = 1; i < tools.Length; i++)
            {
                int count = inventory[(int)kinds[i]];
                tools[i].interactable = !blocked && !session.State.IsWon && (i != 3 || contextMechanic) &&
                    (kinds[i] != BoosterKind.Firefly || !root.HintPending) &&
                    (count == 0 || session.BoosterHasEffect(kinds[i]));
            }
            stuck.SetActive(!presenting && !blocked && !session.State.IsWon && !GameSession.HasLegalMove(session.State));
        }
        private void DrawCounts()
        {
            for (int i = 1; i < tools.Length; i++)
            {
                int count = inventory[(int)kinds[i]];
                badges[i].sprite = art.Get(count > 0 ? "badge_count" : "badge_plus");
                counts[i].gameObject.SetActive(count > 0); counts[i].text = count.ToString();
            }
        }
        public void BoosterSpent(BoosterUsed used)
        {
            inventory[(int)used.Booster] = used.Remaining; DrawCounts();
        }
        public void Phase(DayPhase value, int movesLeft, bool enabled)
        {
            phaseGroup.SetActive(enabled); phaseIcon.sprite = art.Get(value == DayPhase.Day ? "dial_sun" : "dial_moon"); phaseCount.text = movesLeft.ToString();
        }
        public void SetNotes(int value)
        {
            DisplayedSong = value;
            for (int i = 0; i < notes.Length; i++) notes[i].color = new Color(1, 1, 1, i < value ? 1 : 0.2f);
        }
        public void Note(SongNote note, Action done) => FlyIcon("gift_encore", new Vector2(630, 570), V5Layout.SongPill.center,
            () => { SetNotes(note.MeterAfter); done(); });
        public void Payout(SongPayout payout, Action done)
        {
            SetNotes(Mathf.Max(0, DisplayedSong - 5));
            bool visible = false;
            for (int i = 1; i < kinds.Length; i++) visible |= kinds[i] == payout.Booster && tools[i].gameObject.activeSelf;
            FlyIcon(V5ViewModel.Icon(payout.Booster), V5Layout.SongPill.center, BoosterPoint(payout.Booster), () =>
            {
                inventory[(int)payout.Booster]++; DrawCounts();
                if (visible) { done(); return; }
                // Preview above slot four; its underlying button keeps its own identity and action.
                var preview = V5UI.Image(effects, "Awarded " + payout.Booster + " +1", art.Get("pill_cream"),
                    new Vector2(845, 1595), new Vector2(130, 90));
                V5UI.Image(preview.transform, "Award", art.Get(V5ViewModel.Icon(payout.Booster)), new Vector2(37, 45), Vector2.one * 54);
                V5UI.Text(preview.transform, "Award count", "+1", font, new Vector2(97, 45), new Vector2(56, 60), 32);
                motion.Play(0.65f, t => preview.transform.localScale = Vector3.one * (0.9f + 0.1f * Mathf.Sin(t * Mathf.PI)),
                    () => { V5UI.Dispose(preview.gameObject); done(); });
            });
        }
        public void Chorus(int chain)
        {
            if (chain < 2) return;
            chorus.SetActive(true); chorusLabel.text = "x" + chain;
            motion.Play(0.3f, t => chorus.transform.localScale = Vector3.one * (0.6f + 0.4f * t));
            motion.Play(0.8f, t => { }, () => chorus.SetActive(false), 0.3f);
        }
        public void LightStep(bool value) => feather.SetActive(value);
        public void FlyIcon(string icon, Vector2 from, Vector2 to, Action done)
        {
            var image = V5UI.Image(effects, "Flying " + icon, art.Get(icon), from, Vector2.one * 64);
            motion.Play(0.4f, t =>
            {
                var p = Vector2.Lerp(from, to, t) + Vector2.up * (-60 * Mathf.Sin(t * Mathf.PI));
                image.rectTransform.anchoredPosition = new Vector2(p.x, -p.y);
                image.rectTransform.localScale = Vector3.one * (1 + 0.3f * Mathf.Sin(t * Mathf.PI));
            }, () => { V5UI.Dispose(image.gameObject); done(); });
        }
        public void CountCoins(int from, int to) => motion.Play(0.65f, t => coins.text = Mathf.RoundToInt(Mathf.Lerp(from, to, t)).ToString());
        public void Cancel()
        {
            motion.CancelAll(); foreach (var house in houses.Values) house.Cancel();
            if (effects != null) for (int i = effects.childCount - 1; i >= 0; i--) V5UI.Dispose(effects.GetChild(i).gameObject);
            if (chorus != null) chorus.SetActive(false);
        }
    }
}
