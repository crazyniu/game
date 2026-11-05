using System;
using PerchSort.Core;

namespace PerchSort.Game
{
    public sealed class HudSnapshot
    {
        public sealed class Tool
        {
            public BoosterKind Kind;
            public bool Visible, Enabled, ZeroPlus;
            public int? Stock;
        }
        public int? Coins, CountedMoves, ThreeStarLimit, TwoStarLimit, ReachableStars;
        public bool OneStarAnyClear = true, UndoAvailable, SettingsEnabled = true;
        public bool PhaseEnabled, MoonbeamActive, LightStep, FreeMove, Presenting;
        public DayPhase Phase;
        public int PhaseMovesLeft, Chain;
        public Tool[] Tools = Array.Empty<Tool>();
        public BoardSnapshot.Goal[] Goals = Array.Empty<BoardSnapshot.Goal>();
        public string ResidentSpecies, HouseStyle = "default";
    }
}
