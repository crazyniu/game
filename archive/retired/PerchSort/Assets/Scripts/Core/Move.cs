using System;
using System.Collections.Generic;

namespace PerchSort.Core
{
    public enum MoveEventKind { Moved, Completed, TargetsAdvanced, AutoHomed, CrateRevealed }
    public enum CompletionKind { Asleep, Homed }

    public sealed class MoveEvent
    {
        public MoveEventKind Kind { get; internal set; }
        public int From { get; internal set; } = -1;
        public int To { get; internal set; } = -1;
        public int Count { get; internal set; }
        public int Shelf { get; internal set; } = -1;
        public int Species { get; internal set; } = -1;
        public CompletionKind Completion { get; internal set; }
        public IReadOnlyList<int> ActiveTargets { get; internal set; } = Array.Empty<int>();
        public int CollectedCount { get; internal set; }
    }

    public readonly struct Move
    {
        public int From { get; }
        public int To { get; }
        public int Count { get; }
        public IReadOnlyList<MoveEvent> Events { get; }
        public Move(int from, int to) : this(from, to, 0, Array.Empty<MoveEvent>()) { }
        internal Move(int from, int to, int count, IReadOnlyList<MoveEvent> events)
        { From = from; To = to; Count = count; Events = events; }
    }

    public readonly struct MoveResult
    {
        public Move Move { get; }
        public int Count => Move.Count;
        public IReadOnlyList<MoveEvent> Events => Move.Events;
        public int Bird { get; }
        // Legacy animation summary: only the direct destination's immediate homing.
        // v2 presentation must consume Events, including automatic collections.
        public bool Collected => CollectedPerchIndex >= 0;
        public int CollectedPerchIndex { get; }
        public int CollectedSpecies => Collected ? Bird : -1;
        internal MoveResult(Move move, int bird, int collectedPerchIndex)
        { Move = move; Bird = bird; CollectedPerchIndex = collectedPerchIndex; }
    }
}
