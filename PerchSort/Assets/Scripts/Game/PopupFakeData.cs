#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Game
{
    // All state changes run inside the root's nonpersistent preview transaction.
    public sealed class PopupFakeData : IScreenStates
    {
        private readonly GameRoot root;
        private readonly Dictionary<string, Action<PanelsView>> states = new Dictionary<string, Action<PanelsView>>(StringComparer.Ordinal);
        private readonly List<string> names = new List<string>();
        public string ScreenName => "Popups";
        public IReadOnlyList<string> StateNames => names;

        public PopupFakeData(GameRoot root)
        {
            this.root = root;
            Add("Win / Three stars", p => root.Win.Preview(3, 30, 12, 12));
            Add("Win / Two stars", p => root.Win.Preview(2, 20, 13, 12));
            Add("Win / One star", p => root.Win.Preview(1, 10, 17, 12));
            Add("Win / Zero reward", p => root.Win.Preview(3, 0, 0, 12));
            Add("Win / Maximum counters", p => root.Win.Preview(3, 99999, 999, 999));
            Add("Win / Disabled next", p => root.Win.Preview(3, 30, 12, 12, false));
            Add("Settings / Sound on", p => p.Settings());
            Add("Settings / Muted", p => { root.SetMuted(true); p.Settings(); });
            Add("Settings / Map disabled actions", p => { root.PreviewMap(); p.Settings(); });
            string[] species = { "sunny", "sky", "berry", "mint", "cocoa", "snow", "ember", "crow", "ink", "hoot" };
            foreach (string id in species)
            {
                string bird = id;
                if (bird != "crow") Add("Goal info / " + bird, p => p.GoalInfo(bird));
                Add("New friend / " + bird, p => p.Popup(PopupKind.NewFriend, bird, 20));
            }
            foreach (BoosterKind kind in Enum.GetValues(typeof(BoosterKind)))
            {
                BoosterKind booster = kind;
                Add("Unlock / " + PanelsView.BoosterName(booster), p => p.Unlock(booster));
            }
            Add("Daily gift / Claim", p => p.Popup(PopupKind.DailyGift, "sunny", 50));
            Add("Daily gift / Zero reward", p => p.Popup(PopupKind.DailyGift, "sunny", 0));
            Add("Daily gift / Maximum reward", p => p.Popup(PopupKind.DailyGift, "sunny", 99999));
            for (int i = 1; i <= 4; i++)
            {
                int world = i;
                Add("World complete / Forest " + world, p => p.Popup(PopupKind.WorldComplete, "sunny", 200, world: world));
                Add("World unlock / Forest " + world, p => p.Popup(PopupKind.WorldUnlock, "sunny", 0, world: world));
            }
            Add("Level card / First visit", p => Level(p, 1, 0, -1));
            Add("Level card / Two stars", p => Level(p, 20, 2, 28));
            Add("Level card / Full roster", p => Level(p, 40, 3, 28));
            Add("Level card / Maximum moves", p => Level(p, 40, 3, 999));
            Add("Edges / Zero coins", p => { Coins(0); p.Popup(PopupKind.DailyGift, "sunny", 50); });
            Add("Edges / Maximum coins", p => { Coins(99999); p.Popup(PopupKind.DailyGift, "sunny", 50); });
            Add("Level card / Locked", p => { root.PreviewMap(); p.PreviewLevelCard(40, 0, -1, false); });
            Add("Edges / Longest bird name", p => p.GoalInfo("snow"));
        }

        private void Add(string name, Action<PanelsView> render) { names.Add(name); states.Add(name, render); }
        public void Apply(string stateName)
        {
            if (!states.TryGetValue(stateName, out var render)) throw new ArgumentException("Unknown popup state: " + stateName);
            root.BoardStates.Apply("Data / Typical");
            root.BoardScreen.SetOverlay(new OverlayPresentation { BlocksInput = true, Kind = stateName });
            var panels = root.GetComponentInChildren<PanelsView>(true);
            render(panels);
        }
        private void Coins(int value)
        {
            root.PreviewCoins(value);
            var snapshot = root.BoardScreen.Snapshot.Hud; snapshot.Coins = value;
            root.BoardScreen.UpdateHud(snapshot);
        }
        private void Level(PanelsView panels, int id, int stars, int moves)
        {
            root.PreviewMap();
            panels.PreviewLevelCard(id, stars, moves);
        }
    }
}
#endif
