using PerchSort.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PerchSort.Game
{
    // Session-side policy. The board receives a supplied icon bubble and never searches moves.
    public sealed class StuckHint : MonoBehaviour
    {
        private GameRoot root;
        private GameSession session;
        private int moves = -1, inventoryHash;
        private bool pending;
        private float deadline;
        public static void Observe(GameRoot owner)
        {
            var hint = owner.GetComponent<StuckHint>();
            if (hint == null) hint = owner.gameObject.AddComponent<StuckHint>();
            hint.root = owner; hint.RefreshState();
        }
        private void RefreshState()
        {
            if (root.IsBoardPreview) { pending = false; return; }
            var current = root.Session;
            int hash = 17;
            for (int i = 0; i < 6; i++) hash = unchecked(hash * 31 + current.Inventory[(BoosterKind)i]);
            if (session != current || moves != current.State.Moves || hash != inventoryHash)
            {
                pending = session == current && current.CanUndo; deadline = 0;
                session = current; moves = current.State.Moves; inventoryHash = hash;
            }
            if (root.IsMap || root.HasModal || current.State.IsWon) { pending = false; deadline = 0; return; }
            if (root.IsBusy || root.IsAirborne) { deadline = 0; return; }
            if (pending && deadline == 0)
            {
                if (GameSession.HasLegalMove(current.State)) pending = false;
                else deadline = Time.unscaledTime + 0.8f;
            }
        }
        private void Update()
        {
            if (root == null || root.IsBoardPreview || !pending) return;
            if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame) { pending = false; return; }
            if (root.IsMap || root.HasModal || root.IsBusy || root.IsAirborne) { deadline = 0; return; }
            if (deadline <= 0 || Time.unscaledTime < deadline) return;
            pending = false;
            if (!session.State.IsWon && !GameSession.HasLegalMove(session.State))
                root.BoardScreen.ShowBubble(new BubblePresentation { Visible = true, Point = new Vector2(540, 1620),
                    IconKeys = new[] { "icon_undo", "tool_hint", "tool_branch" } });
        }
        private void OnDisable() { pending = false; deadline = 0; }
    }
}
