using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PerchSort.Game
{
    // Real-session input adapter; bubble geometry and lifetime are owned by IBoardView.
    public sealed class SleepBubble : MonoBehaviour
    {
        public const float RefusalSeconds = 0.25f, BubbleSeconds = 1.6f, FadeSeconds = 0.2f, DriftSeconds = 1.6f;
        public const float BobPixels = 6, RisePixels = 30;
        private GameRoot root;
        public bool IsVisible => root != null && root.BoardScreen.BubbleVisible;
        public void Configure(GameRoot owner, GameArt assets) { root = owner; }
        public bool Refuse(int branch)
        {
            var state = root.Session.State; var birds = state.Branches[branch].Birds;
            BirdView target = null;
            for (int seat = birds.Count - 1; seat >= 0; seat--)
            {
                var bird = birds[seat];
                if (bird.IsEgg || !state.IsAsleep(bird) || !root.Board.Birds.TryGetValue(bird.Id, out var view)) continue;
                bool hit = Pointer.current != null && RectTransformUtility.RectangleContainsScreenPoint(
                    (RectTransform)view.transform, Pointer.current.position.ReadValue(), root.RenderCamera);
                if (hit || seat == birds.Count - 1) { target = view; break; }
            }
            if (target == null) return false;
            var icons = new List<string>();
            if (target.Bird.Species.IsNocturnal && state.NocturnalDaySleep) icons.Add("dial_moon");
            if (target.Bird.IsAsleep) icons.Add("gift_sun_note");
            root.BoardScreen.ShowBubble(new BubblePresentation { Visible = true,
                Point = target.Position + new Vector2(0, -200 * target.DisplayScale), IconKeys = icons.ToArray() });
            return true;
        }
        public void Hide() => root?.BoardScreen.ShowBubble(null);
    }
}
