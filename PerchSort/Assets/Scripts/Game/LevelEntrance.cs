using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class LevelEntrance : MonoBehaviour
    {
        private const float HouseDuration = 0.8f, BranchDuration = 0.7f, RowStagger = 0.15f, BirdStagger = 0.08f;
        internal const float FlightDuration = 1.0f;
        private const float FlightSettleDuration = 0.06f + FlightDuration + 0.12f;
        internal const float SeatedDuration = 0.25f;
        private const float MinimumDuration = 3.5f;
        private readonly Dictionary<int, int> attempts = new Dictionary<int, int>();
        private int arrivals;
        private bool poseComplete, prepared;
        private BoardPresentation root;
        private GameMotion motion;
        private Action completed;
        private int revision;
        public bool IsPlaying { get; private set; }
        public void Configure(BoardPresentation owner)
        {
            root = owner; motion = gameObject.AddComponent<GameMotion>();
            var skip = gameObject.AddComponent<Image>(); skip.color = Color.clear; skip.raycastTarget = true;
            gameObject.AddComponent<EmptyClick>().Click = root.RequestEntranceSkip;
            gameObject.SetActive(false);
        }
        internal void Prepare()
        {
            Cancel();
            int level = root.Snapshot.LevelId;
            attempts.TryGetValue(level, out int attempt);
            attempts[level] = attempt + 1;
            var random = new System.Random(unchecked(level + attempt));
            foreach (var branch in root.Board.Branches)
            {
                var state = root.Snapshot.Branches[branch.Index];
                for (int seat = 0; seat < state.Birds.Length; seat++)
                {
                    var bird = root.Board.Birds[state.Birds[seat].Bird.Id];
                    bool seated = bird.Bird.IsEgg || bird.Sleeping;
                    bird.Place(seated ? branch.Slot(seat) : StartPoint(branch, random));
                }
            }
            // Hold the starting poses without idle motion while the wipe still covers the level.
            root.Board.Cancel();
            foreach (var bird in root.Board.Birds.Values)
                if (bird.Bird.IsEgg || bird.Sleeping) bird.SetEntranceAlpha(0);
            Pose(0);
            prepared = true;
        }
        private static Vector2 StartPoint(BranchView branch, System.Random random)
        {
            int side = random.Next(3);
            // Fold the opposite edge onto the branch's edge to avoid full-width crossings.
            if (side != 2) side = branch.IsRight ? 1 : 0;
            float x = side == 2 ? Mathf.Lerp(100, 980, (float)random.NextDouble()) : side == 0 ? -180 : 1260;
            float y = side == 2 ? -180 : Mathf.Lerp(150, 1500, (float)random.NextDouble());
            return new Vector2(x + Mathf.Lerp(-60, 60, (float)random.NextDouble()),
                y + Mathf.Lerp(-60, 60, (float)random.NextDouble()));
        }
        public void Play(Action done, float startAt = 0)
        {
            if (!prepared) Prepare();
            prepared = false; completed = done; IsPlaying = true;
            int token = ++revision;
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            Pose(startAt);
            root.PresentationInputChanged();
            arrivals = root.Board.Birds.Count; poseComplete = false;
            float duration = MinimumDuration;
            int row = -1, rowBird = 0;
            Action arrived = () =>
            {
                if (token != revision) return;
                arrivals--;
                if (poseComplete && arrivals == 0) Finish();
            };
            foreach (var branch in root.Board.Branches)
            {
                var state = root.Snapshot.Branches[branch.Index];
                if (state.Row != row) { row = state.Row; rowBird = 0; }
                for (int seat = 0; seat < state.Birds.Length; seat++)
                {
                    var bird = root.Board.Birds[state.Birds[seat].Bird.Id];
                    Vector2 target = branch.Slot(seat);
                    Vector2 start = bird.Position;
                    // Rows overlap, but birds within each row keep their own readable cadence.
                    float delay = HouseDuration + BranchDuration + state.Row * RowStagger + rowBird * BirdStagger;
                    float arrival = delay + (bird.Bird.IsEgg || bird.Sleeping ? SeatedDuration : FlightSettleDuration);
                    duration = Mathf.Max(duration, arrival);
                    if (startAt >= arrival) { bird.Place(target); bird.SetEntranceAlpha(1); arrived(); }
                    else root.Board.Enter(bird, start, target, Mathf.Max(0, delay - startAt), branch, arrived);
                    rowBird++;
                }
            }
            float remainingDuration = Mathf.Max(0, duration - startAt);
            motion.Play(remainingDuration, t => { if (token == revision) Pose(startAt + t * remainingDuration, duration); },
                () =>
                {
                    if (token != revision) return;
                    poseComplete = true;
                    if (arrivals == 0) Finish();
                });
        }
        private void Pose(float elapsed, float duration = MinimumDuration)
        {
            float drop = Mathf.Clamp01(elapsed / HouseDuration);
            float spring = 1 + 2.2f * Mathf.Pow(drop - 1, 3) + 1.2f * Mathf.Pow(drop - 1, 2);
            root.Hud.Entrance(spring, Mathf.Clamp01((elapsed - duration + 0.45f) / 0.45f));
            root.Scene.Entrance(Mathf.Clamp01(elapsed / HouseDuration));
            var branches = root.Board.Branches;
            for (int i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];
                float progress = Mathf.Clamp01((elapsed - HouseDuration - root.Snapshot.Branches[branch.Index].Row * RowStagger) / BranchDuration);
                branch.Entrance(1 - Mathf.Pow(1 - progress, 3));
            }
        }
        private void Finish()
        {
            if (!IsPlaying) return;
            IsPlaying = false; revision++; motion.CancelAll();
            root.Board.Cancel(); root.Board.Settle(root.Snapshot);
            ResetPose();
            var next = completed; completed = null;
            gameObject.SetActive(false); next?.Invoke();
        }
        public void Skip()
        {
            if (!IsPlaying) return;
            motion.CancelAll(); Finish();
        }
        private void ResetPose()
        {
            if (root == null || root.Hud == null || root.Scene == null || root.Board == null) return;
            root.Hud.Entrance(1, 1); root.Scene.Entrance(1);
            foreach (var branch in root.Board.Branches) branch.Entrance(1);
        }
        public void Cancel()
        {
            if (motion == null) return;
            bool restore = IsPlaying || prepared;
            revision++; completed = null; IsPlaying = prepared = false; motion.CancelAll();
            if (restore && root != null && root.Board != null)
            {
                root.Board.Cancel(); root.Board.Settle(root.Snapshot);
            }
            ResetPose(); gameObject.SetActive(false);
        }
        private void OnDisable() { if (IsPlaying) Cancel(); }
        private void OnDestroy() { revision++; completed = null; }
    }
}
