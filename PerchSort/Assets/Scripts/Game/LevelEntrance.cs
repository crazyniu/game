using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class LevelEntrance : MonoBehaviour
    {
        private const float HouseDuration = 0.8f, BranchDuration = 0.7f, RowStagger = 0.15f, BirdStagger = 0.08f;
        internal const float FlightDuration = 1.0f;
        private const float FlightSettleDuration = 0.06f + FlightDuration + 0.12f;
        private int arrivals;
        private bool poseComplete, prepared;
        private GameRoot root;
        private GameMotion motion;
        private Action completed;
        private int revision;
        public bool IsPlaying { get; private set; }
        public void Configure(GameRoot owner)
        {
            root = owner; motion = gameObject.AddComponent<GameMotion>();
            var skip = gameObject.AddComponent<Image>(); skip.color = Color.clear; skip.raycastTarget = true;
            gameObject.AddComponent<EmptyClick>().Click = Skip;
            gameObject.SetActive(false);
        }
        internal void Prepare()
        {
            Cancel();
            int index = 0;
            foreach (var branch in root.Board.Branches)
            {
                var state = root.Session.State.Branches[branch.Index];
                for (int seat = 0; seat < state.Birds.Count; seat++)
                {
                    var bird = root.Board.Birds[state.Birds[seat].Id];
                    bird.Place(StartPoint(bird, branch, branch.Slot(seat), index++));
                }
            }
            // Hold the starting poses without idle motion while the wipe still covers the level.
            root.Board.Cancel();
            Pose(0);
            prepared = true;
        }
        private static Vector2 StartPoint(BirdView bird, BranchView branch, Vector2 target, int index)
            => bird.Bird.IsEgg || index % 5 == 0 ? new Vector2(target.x, 420) :
                new Vector2(branch.IsRight ? 1240 : -160, target.y - 120);
        public void Play(Action done)
        {
            if (!prepared) Prepare();
            prepared = false; completed = done; IsPlaying = true;
            int token = ++revision;
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            Pose(0);
            root.PresentationInputChanged();
            arrivals = root.Board.Birds.Count; poseComplete = false;
            float duration = 3.2f;
            int row = -1, rowBird = 0;
            Action arrived = () =>
            {
                if (token != revision) return;
                arrivals--;
                if (poseComplete && arrivals == 0) Finish();
            };
            foreach (var branch in root.Board.Branches)
            {
                var state = root.Session.State.Branches[branch.Index];
                if (state.Row != row) { row = state.Row; rowBird = 0; }
                for (int seat = 0; seat < state.Birds.Count; seat++)
                {
                    var bird = root.Board.Birds[state.Birds[seat].Id];
                    Vector2 target = branch.Slot(seat);
                    Vector2 start = bird.Position;
                    // Rows overlap, but birds within each row keep their own readable cadence.
                    float delay = HouseDuration + BranchDuration + state.Row * RowStagger + rowBird * BirdStagger;
                    root.Board.Enter(bird, start, target, delay, branch, arrived);
                    duration = Mathf.Max(duration, delay + FlightSettleDuration);
                    rowBird++;
                }
            }
            motion.Play(duration, t => { if (token == revision) Pose(t * duration, duration); },
                () =>
                {
                    if (token != revision) return;
                    poseComplete = true;
                    if (arrivals == 0) Finish();
                });
        }
        private void Pose(float elapsed, float duration = 3.2f)
        {
            float drop = Mathf.Clamp01(elapsed / HouseDuration);
            float spring = 1 + 2.2f * Mathf.Pow(drop - 1, 3) + 1.2f * Mathf.Pow(drop - 1, 2);
            root.Hud.Entrance(spring, Mathf.Clamp01((elapsed - duration + 0.45f) / 0.45f));
            root.Scene.Entrance(Mathf.Clamp01(elapsed / HouseDuration));
            var branches = root.Board.Branches;
            for (int i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];
                float progress = Mathf.Clamp01((elapsed - HouseDuration - root.Session.State.Branches[branch.Index].Row * RowStagger) / BranchDuration);
                branch.Entrance(1 - Mathf.Pow(1 - progress, 3));
            }
        }
        private void Finish()
        {
            if (!IsPlaying) return;
            IsPlaying = false; revision++; motion.CancelAll();
            root.Board.Cancel(); root.Board.Settle(root.Session.State);
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
                root.Board.Cancel(); root.Board.Settle(root.Session.State);
            }
            ResetPose(); gameObject.SetActive(false);
        }
        private void OnDisable() { if (IsPlaying) Cancel(); }
        private void OnDestroy() { revision++; completed = null; }
    }
}
