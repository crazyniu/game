using System.Collections.Generic;
using UnityEngine;

namespace PerchSort.Game
{
    // A branch-local presentation lease; it never reads or changes Core state.
    internal sealed class BirdIdleGroup
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
                if (Mathf.Abs(i - index) == 1 &&
                    (now - other.LastIdleStart < 0.18f || other.LastIdleAction == action && now - other.LastIdleStart < 0.6f))
                    return false;
            }
            return active < 3;
        }
    }
}
