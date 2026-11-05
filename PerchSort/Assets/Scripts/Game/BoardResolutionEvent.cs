using System;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Game
{
    public sealed class BoardResolutionEvent
    {
        public enum EventKind { Wave, Transfer, Follow, Swivel, EggWobble, EggReveal, Hatch, Wake, Grow, TwigUpgrade, CrowsLeave, FlockHome, Gift, Phase, Booster, Won, Refuse }
        public EventKind Kind;
        public GameEvent Source;
        public BoardSnapshot Snapshot;
        public int From, To, TargetStart, BirdId, Seats, Chain;
        public int[] BirdIds = Array.Empty<int>(), TargetSeats = Array.Empty<int>();
        public string Species, Icon;
        public bool LightStep, FreeMove;
        public DayPhase Phase;
        public BoosterKind Booster;
        public int Remaining;
        public Vector2[] Targets = Array.Empty<Vector2>();
    }
}
