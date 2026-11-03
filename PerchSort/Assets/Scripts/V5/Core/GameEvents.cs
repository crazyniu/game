using System;
using System.Collections.Generic;

namespace PerchSort.V5.Core
{
    public abstract class GameEvent { }

    // Presentation boundary only; rule resolution and search do not depend on this event.
    public sealed class ResolutionWave : GameEvent
    {
        internal static readonly ResolutionWave Instance = new ResolutionWave();
        private ResolutionWave() { }
    }

    public readonly struct BirdTravel
    {
        public int BirdId { get; }
        public int FromSeat { get; }
        public int ToSeat { get; }
        internal BirdTravel(int birdId, int fromSeat, int toSeat)
        { BirdId = birdId; FromSeat = fromSeat; ToSeat = toSeat; }
    }

    public sealed class Transferred : GameEvent
    {
        public int From { get; }
        public int To { get; }
        public int SourceStart { get; }
        public int TargetStart { get; }
        public IReadOnlyList<int> BirdIds { get; }
        public int Count => BirdIds.Count;
        // A run is contiguous; derive seat pairs instead of allocating two redundant arrays.
        public BirdTravel BirdAt(int index) => new BirdTravel(BirdIds[index], SourceStart + index, TargetStart + index);
        internal Transferred(int from, int to, int sourceStart, int targetStart, int[] ids)
        { From = from; To = to; SourceStart = sourceStart; TargetStart = targetStart; BirdIds = Array.AsReadOnly(ids); }
    }

    public sealed class ChicksFollowed : GameEvent
    {
        public int From { get; }
        public int To { get; }
        public IReadOnlyList<BirdTravel> Birds { get; }
        internal ChicksFollowed(int from, int to, List<BirdTravel> birds)
        { From = from; To = to; Birds = birds.AsReadOnly(); }
    }

    public abstract class BirdEvent : GameEvent
    {
        public int BirdId { get; }
        public int Branch { get; }
        public int Seat { get; }
        protected BirdEvent(int birdId, int branch, int seat) { BirdId = birdId; Branch = branch; Seat = seat; }
    }
    public sealed class Hatched : BirdEvent
    {
        public Species Species { get; }
        internal Hatched(Bird bird, int branch, int seat) : base(bird.Id, branch, seat) { Species = bird.Species; }
    }
    public sealed class EggRevealed : BirdEvent
    {
        public Species Species { get; }
        internal EggRevealed(Bird bird, int branch, int seat) : base(bird.Id, branch, seat) { Species = bird.Species; }
    }
    public sealed class EggWobbled : BirdEvent
    {
        internal EggWobbled(int birdId, int branch, int seat) : base(birdId, branch, seat) { }
    }
    public sealed class Woke : BirdEvent
    {
        internal Woke(int birdId, int branch, int seat) : base(birdId, branch, seat) { }
    }
    public sealed class FlockHome : GameEvent
    {
        public int Branch { get; }
        public Species Species { get; }
        public IReadOnlyList<int> BirdIds { get; }
        internal FlockHome(int branch, Species species, int[] ids)
        { Branch = branch; Species = species; BirdIds = Array.AsReadOnly(ids); }
    }

    public enum GiftEffect { WakeUpSong, GentleBreeze, Encore, Gardener, GrumpyStare, LightStep, WarmGlow, NightCall, Moonbeam }
    public sealed class Gift : GameEvent
    {
        public int Branch { get; }
        public Species Species { get; }
        public GiftEffect Effect { get; }
        internal Gift(int branch, Species species, GiftEffect effect) { Branch = branch; Species = species; Effect = effect; }
    }
    public sealed class BranchGrew : GameEvent
    {
        public int Branch { get; }
        public int BeforeSeats { get; }
        public int Seats { get; }
        internal BranchGrew(int branch, int beforeSeats, int seats) { Branch = branch; BeforeSeats = beforeSeats; Seats = seats; }
    }
    public sealed class TwigUpgraded : GameEvent
    {
        public int Branch { get; }
        public int BeforeSeats { get; }
        public int Seats { get; }
        internal TwigUpgraded(int branch, int beforeSeats, int seats) { Branch = branch; BeforeSeats = beforeSeats; Seats = seats; }
    }
    public sealed class CrowsLeft : GameEvent
    {
        public int Branch { get; }
        public IReadOnlyList<BirdTravel> Birds { get; }
        internal CrowsLeft(int branch, List<BirdTravel> birds) { Branch = branch; Birds = birds.AsReadOnly(); }
    }
    public sealed class Swivelled : GameEvent
    {
        public int Branch { get; }
        public IReadOnlyList<BirdTravel> Birds { get; }
        internal Swivelled(int branch, BirdTravel[] birds) { Branch = branch; Birds = Array.AsReadOnly(birds); }
    }
    public enum PhaseChangeCause { Clock, InkGift }
    public sealed class PhaseChanged : GameEvent
    {
        public DayPhase Phase { get; }
        public PhaseChangeCause Cause { get; }
        internal PhaseChanged(DayPhase phase, PhaseChangeCause cause) { Phase = phase; Cause = cause; }
    }
    public sealed class BoosterUsed : GameEvent
    {
        public BoosterKind Booster { get; }
        public bool ConsumedInventory { get; }
        public int Remaining { get; }
        internal BoosterUsed(BoosterKind booster, bool consumedInventory, int remaining)
        { Booster = booster; ConsumedInventory = consumedInventory; Remaining = remaining; }
    }
    public sealed class Won : GameEvent { }
}
