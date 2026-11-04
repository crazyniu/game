using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.V5.Game
{
    // A branch-local presentation lease; it never reads or changes Core state.
    internal sealed class V7BirdIdleGroup
    {
        private readonly List<BirdView> birds = new List<BirdView>();
        public void Clear() => birds.Clear();
        public void Add(BirdView bird) => birds.Add(bird);
        public bool TryStart(BirdView bird, int action, float now)
        {
            int active = 0, index = birds.IndexOf(bird);
            for (int i = 0; i < birds.Count; i++)
            {
                var other = birds[i];
                if (other.IdleActing) active++;
                if (Mathf.Abs(i - index) == 1 && other.LastIdleAction == action && now - other.LastIdleStart < 0.6f)
                    return false;
            }
            return active < 3;
        }
        public float NeighbourX(BirdView bird)
        {
            int index = birds.IndexOf(bird);
            if (index < 0 || birds.Count < 2) return bird.Position.x;
            return birds[index == 0 ? 1 : index - 1].Position.x;
        }
    }
}
