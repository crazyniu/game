using System;
using System.Collections.Generic;
using PerchSort.Meta;
using UnityEngine;
using CoreBooster = PerchSort.Core.BoosterKind;

namespace PerchSort.Game
{
    public enum NotificationKind { WorldComplete, WorldUnlock, DailyGift, NewFriend, BoosterUnlock }

    public sealed class Notification
    {
        public NotificationKind Kind { get; }
        public string Species { get; }
        public int World { get; }
        public CoreBooster Booster { get; }
        internal Notification(NotificationKind kind, string species = null, int world = 0, CoreBooster booster = default)
        { Kind = kind; Species = species; World = world; Booster = booster; }
    }

    // Scheduling is independent of art and tween callbacks. Only acknowledgement commits a claim.
    public sealed class PopupFlow
    {
        [Serializable]
        private sealed class SaveData
        {
            public string rewardLedger;
            public int completeDue, unlockDue, nextLevel, level;
            public bool mapAfterWin;
            public string[] friends;
            public int[] boosters;
        }
        private readonly GameProfile owner;
        private PlayerProfile ledger;
        private int completeDue, unlockDue, nextLevel, level;
        private bool dailyDue, mapAfterWin;
        private readonly List<string> friends = new List<string>(10);
        private readonly List<CoreBooster> boosters = new List<CoreBooster>(6);
        public Notification Current { get; private set; }
        public bool IntroFinished { get; private set; }
        public PlayerProfile RewardLedger => ledger;
        public bool HasWorldSequence => completeDue != 0 || unlockDue != 0;
        public int DeferredLevel => nextLevel;
        public bool NeedsMap => mapAfterWin;

        public PopupFlow(GameProfile profile, PlayerProfile receipts = null)
        { owner = profile ?? throw new ArgumentNullException(nameof(profile)); ledger = receipts ?? new PlayerProfile(); }

        public void BeginMap(IClock clock)
        {
            if (Current != null && Current.Kind != NotificationKind.WorldComplete && Current.Kind != NotificationKind.WorldUnlock)
                Current = null;
            dailyDue = Economy.ClaimDailyGift(ledger.WithCoins(owner.Coins), clock).Payout.DailyGift != 0;
            mapAfterWin = false; IntroFinished = false;
        }
        public void BeginLevel(int level, IEnumerable<string> species)
        {
            this.level = level;
            Current = null; IntroFinished = false; dailyDue = false;
            // Meta supplies canonical debut order, without committing the preview's paid profile.
            friends.Clear();
            foreach (var card in Aviary.OnLevelStarted(ledger, species).NewSpecies) friends.Add(card.Id);
            boosters.Clear();
            for (int i = 0; i < ViewModel.BoosterCount; i++)
                if (level >= ViewModel.Unlock((CoreBooster)i) && !owner.UnlockSeen[i]) boosters.Add((CoreBooster)i);
        }
        public void FinishIntro() => IntroFinished = true;
        public void RecordWin(int level, bool firstClear)
        {
            if (firstClear && level % 10 == 0 && !ledger.HasWorldCompleteReward(level / 10))
                completeDue |= 1 << (level / 10 - 1);
        }
        public void RecordHomecomings(IEnumerable<PerchSort.Core.Species> species)
        {
            foreach (var home in species) ledger = Aviary.IncrementFlocksHome(ledger, home.Id);
        }
        public void LeaveWin(int level, bool next)
        {
            nextLevel = next && level < 40 ? level + 1 : 0;
            mapAfterWin = true;
        }
        public Notification Next(bool map)
        {
            if (Current != null) return Current;
            // A replay may start before leaving the original finale panel. Keep its award pending.
            if (map || mapAfterWin)
            {
                for (int world = 1; world <= 4; world++)
                    if ((completeDue & (1 << (world - 1))) != 0)
                        return Current = new Notification(NotificationKind.WorldComplete, world: world);
                for (int world = 1; world <= 3; world++)
                    if ((unlockDue & (1 << (world - 1))) != 0)
                        return Current = new Notification(NotificationKind.WorldUnlock, world: world + 1);
            }
            if (map)
                return dailyDue ? Current = new Notification(NotificationKind.DailyGift) : null;
            if (!IntroFinished) return null;
            while (friends.Count > 0 && ledger.HasMetSpecies(friends[0])) friends.RemoveAt(0);
            if (friends.Count > 0)
                return Current = new Notification(NotificationKind.NewFriend, friends[0]);
            while (boosters.Count > 0 && owner.UnlockSeen[(int)boosters[0]]) boosters.RemoveAt(0);
            if (boosters.Count > 0)
            {
                var booster = boosters[0];
                // Retain the existing booster-card once-on-presentation contract.
                owner.UnlockSeen[(int)booster] = true;
                return Current = new Notification(NotificationKind.BoosterUnlock, booster: booster);
            }
            return null;
        }
        public int Acknowledge(Notification request, IClock clock)
        {
            if (request == null || !ReferenceEquals(Current, request)) return 0;
            int before = owner.Coins;
            var paid = ledger.WithCoins(before);
            switch (request.Kind)
            {
                case NotificationKind.NewFriend:
                    paid = Aviary.OnLevelStarted(paid, new[] { request.Species }).Profile;
                    friends.Remove(request.Species);
                    break;
                case NotificationKind.DailyGift:
                    paid = Economy.ClaimDailyGift(paid, clock).Profile;
                    dailyDue = false;
                    break;
                case NotificationKind.WorldComplete:
                    paid = Economy.ClaimWorldComplete(paid, request.World).Profile;
                    completeDue &= ~(1 << (request.World - 1));
                    if (request.World < 4) unlockDue |= 1 << (request.World - 1);
                    break;
                case NotificationKind.WorldUnlock:
                    unlockDue &= ~(1 << (request.World - 2));
                    break;
                case NotificationKind.BoosterUnlock:
                    boosters.Remove(request.Booster);
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
            // Pending level requests are suspended on the map; BeginLevel derives its own queue.
            // Earned finale receipts and deferred Next survive leaving/reloading the presentation.
        }
        public string ToText()
        {
            ledger = ledger.WithCoins(owner.Coins);
            var pendingBoosters = new int[boosters.Count];
            for (int i = 0; i < pendingBoosters.Length; i++) pendingBoosters[i] = (int)boosters[i];
            return JsonUtility.ToJson(new SaveData { rewardLedger = ledger.ToText(), completeDue = completeDue,
                unlockDue = unlockDue, nextLevel = nextLevel, level = level, mapAfterWin = mapAfterWin,
                friends = friends.ToArray(), boosters = pendingBoosters });
        }
        public void Restore(string text)
        {
            var data = JsonUtility.FromJson<SaveData>(text);
            ledger = PlayerProfile.FromText(data.rewardLedger);
            owner.Coins = ledger.Coins; completeDue = data.completeDue; unlockDue = data.unlockDue; nextLevel = data.nextLevel;
            level = data.level; mapAfterWin = data.mapAfterWin;
            friends.Clear(); if (data.friends != null) friends.AddRange(data.friends);
            boosters.Clear();
            if (data.boosters != null) foreach (int booster in data.boosters) boosters.Add((CoreBooster)booster);
            CancelPresentation();
        }
    }
}
