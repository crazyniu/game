using System;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V7LevelEntrance : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Motion motion;
        private Action completed;
        private int revision;
        public bool IsPlaying { get; private set; }
        public void Configure(V5GameRoot owner)
        {
            root = owner; motion = gameObject.AddComponent<V5Motion>();
            var skip = gameObject.AddComponent<Image>(); skip.color = Color.clear; skip.raycastTarget = true;
            gameObject.AddComponent<V5EmptyClick>().Click = Skip;
            gameObject.SetActive(false);
        }
        public void Play(Action done)
        {
            Cancel(); completed = done; IsPlaying = true;
            int token = ++revision;
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            root.Board.Cancel(); Pose(0);
            root.PresentationInputChanged();
            int index = 0;
            foreach (var branch in root.Board.Branches)
            {
                var state = root.Session.State.Branches[branch.Index];
                for (int seat = 0; seat < state.Birds.Count; seat++)
                {
                    var bird = root.Board.Birds[state.Birds[seat].Id];
                    Vector2 target = branch.Slot(seat);
                    Vector2 start = bird.Bird.IsEgg || index % 5 == 0 ? new Vector2(target.x, 420) :
                        new Vector2(branch.IsRight ? 1240 : -160, target.y - 120);
                    root.Board.Enter(bird, start, target, 0.55f + index % 7 * 0.05f, branch);
                    index++;
                }
            }
            motion.Play(1.6f, t => { if (token == revision) Pose(t * 1.6f); },
                () => { if (token == revision) Finish(); });
        }
        private void Pose(float elapsed)
        {
            float drop = Mathf.Clamp01(elapsed / 0.4f);
            float spring = 1 + 2.7f * Mathf.Pow(drop - 1, 3) + 1.7f * Mathf.Pow(drop - 1, 2);
            root.Hud.Entrance(spring, Mathf.Clamp01((elapsed - 1.15f) / 0.45f));
            root.Scene.Entrance(Mathf.Clamp01(elapsed / 0.25f));
            var branches = root.Board.Branches;
            for (int i = 0; i < branches.Count; i++)
            {
                var branch = branches[i];
                float progress = Mathf.Clamp01((elapsed - 0.25f - root.Session.State.Branches[branch.Index].Row * 0.05f) / 0.25f);
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
            bool playing = IsPlaying;
            revision++; completed = null; IsPlaying = false; motion.CancelAll();
            if (playing && root != null && root.Board != null)
            {
                root.Board.Cancel(); root.Board.Settle(root.Session.State);
            }
            ResetPose(); gameObject.SetActive(false);
        }
        private void OnDisable() { if (IsPlaying) Cancel(); }
        private void OnDestroy() { revision++; completed = null; }
    }
}
