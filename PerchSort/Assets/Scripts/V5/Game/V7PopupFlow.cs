using System;
using System.Collections.Generic;
using PerchSort.V5.Meta;
using UnityEngine;
using CoreBooster = PerchSort.V5.Core.BoosterKind;

namespace PerchSort.V5.Game
{
    public enum V7NotificationKind { WorldComplete, WorldUnlock, DailyGift, NewFriend, BoosterUnlock }

    public sealed class V7Notification
    {
        public V7NotificationKind Kind { get; }
        public string Species { get; }
        public int World { get; }
        public CoreBooster Booster { get; }
        internal V7Notification(V7NotificationKind kind, string species = null, int world = 0, CoreBooster booster = default)
        { Kind = kind; Species = species; World = world; Booster = booster; }
    }

    // Scheduling is independent of art and tween callbacks. Only acknowledgement commits a claim.
    public sealed class V7PopupFlow
    {
        [Serializable]
        private sealed class SaveData
        {
            public string rewardLedger;
            public int completeDue, unlockDue, nextLevel;
        }
        private readonly V5Profile owner;
        private PlayerProfile ledger;
        private int completeDue, unlockDue, nextLevel, friendIndex, boosterIndex;
        private bool dailyDue;
        private IReadOnlyList<SpeciesCard> friends = Array.Empty<SpeciesCard>();
        private readonly List<CoreBooster> boosters = new List<CoreBooster>(6);
        public V7Notification Current { get; private set; }
        public bool IntroFinished { get; private set; }
        public PlayerProfile RewardLedger => ledger;
        public bool HasWorldSequence => completeDue != 0 || unlockDue != 0;
        public int DeferredLevel => nextLevel;

        public V7PopupFlow(V5Profile profile, PlayerProfile receipts = null)
        { owner = profile ?? throw new ArgumentNullException(nameof(profile)); ledger = receipts ?? new PlayerProfile(); }

        public void BeginMap(IClock clock)
        {
            dailyDue = Economy.ClaimDailyGift(ledger.WithCoins(owner.Coins), clock).Payout.DailyGift != 0;
            friends = Array.Empty<SpeciesCard>(); boosters.Clear(); friendIndex = boosterIndex = 0;
        }
        public void BeginLevel(int level, IEnumerable<string> species)
        {
            Current = null; IntroFinished = false; dailyDue = false; friendIndex = boosterIndex = 0;
            // This immutable preview determines canonical debut order; do not store its paid profile.
            friends = Aviary.OnLevelStarted(ledger, species).NewSpecies;
            boosters.Clear();
            for (int i = 0; i < V5ViewModel.BoosterCount; i++)
                if (level >= V5ViewModel.Unlock((CoreBooster)i) && !owner.UnlockSeen[i]) boosters.Add((CoreBooster)i);
        }
        public void FinishIntro() => IntroFinished = true;
        public void RecordWin(int level, bool firstClear)
        {
            if (firstClear && level % 10 == 0 && !ledger.HasWorldCompleteReward(level / 10))
                completeDue |= 1 << (level / 10 - 1);
        }
        public void LeaveWin(int level, bool next) => nextLevel = next && level < 40 ? level + 1 : 0;
        public V7Notification Next(bool map)
        {
            if (Current != null) return Current;
            for (int world = 1; world <= 4; world++)
                if ((completeDue & (1 << (world - 1))) != 0)
                    return Current = new V7Notification(V7NotificationKind.WorldComplete, world: world);
            for (int world = 1; world <= 3; world++)
                if ((unlockDue & (1 << (world - 1))) != 0)
                    return Current = new V7Notification(V7NotificationKind.WorldUnlock, world: world + 1);
            if (map)
                return dailyDue ? Current = new V7Notification(V7NotificationKind.DailyGift) : null;
            if (!IntroFinished) return null;
            while (friendIndex < friends.Count && ledger.HasMetSpecies(friends[friendIndex].Id)) friendIndex++;
            if (friendIndex < friends.Count)
                return Current = new V7Notification(V7NotificationKind.NewFriend, friends[friendIndex].Id);
            if (boosterIndex < boosters.Count)
            {
                var booster = boosters[boosterIndex];
                // Retain the existing booster-card once-on-presentation contract.
                owner.UnlockSeen[(int)booster] = true;
                return Current = new V7Notification(V7NotificationKind.BoosterUnlock, booster: booster);
            }
            return null;
        }
        public int Acknowledge(V7Notification request, IClock clock)
        {
            if (request == null || !ReferenceEquals(Current, request)) return 0;
            int before = owner.Coins;
            var paid = ledger.WithCoins(before);
            switch (request.Kind)
            {
                case V7NotificationKind.NewFriend:
                    paid = Aviary.OnLevelStarted(paid, new[] { request.Species }).Profile;
                    friendIndex++;
                    break;
                case V7NotificationKind.DailyGift:
                    paid = Economy.ClaimDailyGift(paid, clock).Profile;
                    dailyDue = false;
                    break;
                case V7NotificationKind.WorldComplete:
                    paid = Economy.ClaimWorldComplete(paid, request.World).Profile;
                    completeDue &= ~(1 << (request.World - 1));
                    if (request.World < 4) unlockDue |= 1 << (request.World - 1);
                    break;
                case V7NotificationKind.WorldUnlock:
                    unlockDue &= ~(1 << (request.World - 2));
                    break;
                case V7NotificationKind.BoosterUnlock:
                    boosterIndex++;
                    break;
            }
            ledger = paid; owner.Coins = paid.Coins; Current = null;
            return owner.Coins - before;
        }
        public int TakeDeferredLevel()
        {
            if (Current != null || HasWorldSequence || dailyDue) return 0;
            int level = nextLevel; nextLevel = 0; return level;
        }
        public void CancelPresentation()
        {
            Current = null; dailyDue = false; IntroFinished = false;
            friends = Array.Empty<SpeciesCard>(); boosters.Clear(); friendIndex = boosterIndex = 0;
            // Earned finale receipts and deferred Next survive leaving/reloading the presentation.
        }
        public string ToText()
        {
            ledger = ledger.WithCoins(owner.Coins);
            return JsonUtility.ToJson(new SaveData { rewardLedger = ledger.ToText(), completeDue = completeDue,
                unlockDue = unlockDue, nextLevel = nextLevel });
        }
        public void Restore(string text)
        {
            var data = JsonUtility.FromJson<SaveData>(text);
            ledger = PlayerProfile.FromText(data.rewardLedger);
            owner.Coins = ledger.Coins; completeDue = data.completeDue; unlockDue = data.unlockDue; nextLevel = data.nextLevel;
            CancelPresentation();
        }
    }
}
