using System;
using System.Collections.Generic;
using PerchSort.Audio;
using PerchSort.V5.Core;
using UnityEngine;

namespace PerchSort.V5.Game
{
    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<BranchView> branches = new List<BranchView>();
        private readonly Dictionary<int, BirdView> birds = new Dictionary<int, BirdView>();
        private V5GameRoot root;
        private V5Art art;
        private RectTransform branchLayer, birdLayer;
        public IReadOnlyList<BranchView> Branches => branches;

        public void Configure(V5GameRoot owner, V5Art assets)
        {
            root = owner; art = assets;
            branchLayer = V5UI.Rect(transform, "Branches", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
            birdLayer = V5UI.Rect(transform, "Birds", V5Layout.DesignSize * 0.5f, V5Layout.DesignSize);
        }

        public void Rebuild(BoardState state)
        {
            Cancel();
            foreach (var bird in birds.Values) V5UI.Dispose(bird.gameObject);
            foreach (var branch in branches) V5UI.Dispose(branch.gameObject);
            birds.Clear(); branches.Clear();
            int rowCount = 0;
            foreach (var branch in state.Branches) rowCount = Mathf.Max(rowCount, branch.Row + 1);
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var branch = state.Branches[b];
                Rect hit = V5Layout.BranchHit(branch.Row, rowCount, branch.IsRight);
                // Hit areas meet at the center without overlapping, even where the art overlaps.
                var view = V5UI.Rect(branchLayer, "B" + b, hit.center, hit.size).gameObject.AddComponent<BranchView>();
                view.Configure(root, art, branch, b, rowCount);
                branches.Add(view);
                for (int slot = 0; slot < branch.Birds.Count; slot++)
                {
                    var bird = branch.Birds[slot];
                    Vector2 point = view.Slot(slot);
                    var birdView = V5UI.Rect(birdLayer, "Bird " + bird.Id, point, V5Layout.BirdDisplaySize)
                        .gameObject.AddComponent<BirdView>();
                    birdView.Configure(bird, art, point);
                    birds.Add(bird.Id, birdView);
                }
            }
        }

        public void ShowSelection(BoardState state, int selected)
        {
            ClearHints();
            for (int b = 0; b < state.Branches.Count; b++)
            {
                var branch = state.Branches[b];
                for (int slot = 0; slot < branch.Birds.Count; slot++)
                    if (birds.TryGetValue(branch.Birds[slot].Id, out var bird))
                        bird.Select(b == selected && slot >= branch.Birds.Count - branch.RunCount);
            }
        }

        public void Illegal(BoardState state, int source)
        {
            var branch = state.Branches[source];
            for (int slot = branch.Birds.Count - branch.RunCount; slot < branch.Birds.Count; slot++)
                birds[branch.Birds[slot].Id].Shake();
        }

        public void Hint(Move move)
        {
            ClearHints(); branches[move.From].Highlight(0); branches[move.To].Highlight(0.8f);
        }

        public void ClearHints() { foreach (var branch in branches) branch.ClearHighlight(); }

        public void Play(IReadOnlyList<GameEvent> events, Action complete) => PlayEvent(events, 0, complete);

        private void PlayEvent(IReadOnlyList<GameEvent> events, int index, Action complete)
        {
            if (index == events.Count) { complete(); return; }
            Action next = () => PlayEvent(events, index + 1, complete);
            if (events[index] is Transferred transfer)
            {
                int remaining = transfer.Count;
                root.PlaySound(Sfx.Flutter);
                for (int i = 0; i < transfer.Count; i++)
                {
                    var bird = birds[transfer.BirdIds[i]];
                    bird.transform.SetAsLastSibling();
                    bird.Fly(branches[transfer.To].Slot(transfer.TargetStart + i), i * 0.06f, false, () =>
                    {
                        root.PlaySound(Sfx.Move);
                        if (--remaining == 0) next();
                    });
                }
            }
            else if (events[index] is FlockHome home)
            {
                int remaining = home.BirdIds.Count;
                for (int i = 0; i < home.BirdIds.Count; i++)
                {
                    var bird = birds[home.BirdIds[i]];
                    float delay = i * 0.1f;
                    bird.Cheer(() => bird.Fly(root.Hud.DoorPosition(), delay, true, () =>
                    {
                        root.Hud.HomeArrival();
                        root.PlaySound(Sfx.Collect);
                        if (--remaining == 0) next();
                    }, root.Hud.DoorPosition));
                }
            }

            else next();
        }
        public void Settle(BoardState state, IReadOnlyList<GameEvent> events)
        {
            foreach (var item in events)
                if (item is FlockHome home)
                    foreach (int id in home.BirdIds)
                        if (birds.TryGetValue(id, out var bird))
                        {
                            V5UI.Dispose(bird.gameObject);
                            birds.Remove(id);
                        }
            for (int b = 0; b < state.Branches.Count; b++)
                for (int slot = 0; slot < state.Branches[b].Birds.Count; slot++)
                    birds[state.Branches[b].Birds[slot].Id].Place(branches[b].Slot(slot));
        }

        public void Cancel()
        {
            foreach (var bird in birds.Values) bird.Cancel();
            ClearHints();
        }
    }
}
