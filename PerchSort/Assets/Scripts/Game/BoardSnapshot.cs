using System;
using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Game
{
    // Detached display data. No session, profile, solver, or eligibility query reaches a view.
    public sealed class BoardSnapshot
    {
        public sealed class Branch
        {
            public int Id, Row, Capacity;
            public bool IsRight;
            public BranchKind Kind;
            public BirdData[] Birds = Array.Empty<BirdData>();
        }
        public sealed class BirdData
        {
            public Bird Bird;
            public bool Sleeping;
            public string Outfit;
            public Vector2 Position;
        }
        public sealed class Goal
        {
            public string Species, PortraitKey, NameKey, GiftKey;
            public bool Home;
        }
        public int LevelId, World = 1, Rows = 2, Capacity = 4;
        public string Variant = "noon", HouseStyle = "default", ResidentSpecies;
        public Branch[] Branches = Array.Empty<Branch>();
        public Goal[] Goals = Array.Empty<Goal>();
        public HudSnapshot Hud;
        public InputPresentation Input = new InputPresentation();
        public bool Muted, Focused = true;
        public BoardMotionMode MotionMode;
        public string RequiredKey;
        public readonly Dictionary<int, bool> Sleeping = new Dictionary<int, bool>();
        public bool IsSleeping(int id) => Sleeping.TryGetValue(id, out bool sleeping) && sleeping;
    }
}
