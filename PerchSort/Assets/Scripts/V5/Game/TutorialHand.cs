using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class TutorialHand : MonoBehaviour
    {
        private Image hand;
        private V5Motion motion;
        private bool enabledForAttempt;
        public void Configure(V5Art art, bool show)
        {
            enabledForAttempt = show;
            hand = V5UI.Image(transform, "Tutorial hand", art.Get("hand_pointer"), Vector2.zero, new Vector2(160, 160));
            // The production hand contract puts its fingertip in the top-left area.
            hand.rectTransform.pivot = new Vector2(0.22f, 0.8f);
            motion = gameObject.AddComponent<V5Motion>();
            hand.gameObject.SetActive(false);
        }

        public void Refresh(GameSession session, BoardView board, bool blocked)
        {
            motion.CancelAll();
            hand.gameObject.SetActive(false);
            if (!enabledForAttempt || blocked || session.State.IsWon) return;
            int source = session.MoveCount == 0 ? 0 : 2;
            int target = session.MoveCount == 0 ? 1 : 0;
            // Stop guiding a changed route rather than pointing at an illegal move.
            if (session.MoveCount > 1 || !GameSession.CanMove(session.State, source, target)) return;
            int branch = session.SelectedBranch == source ? target : source;
            int slot = Mathf.Max(0, session.State.Branches[branch].Birds.Count - 1);
            Vector2 point = board.Branches[branch].Slot(slot) + new Vector2(0, 55);
            hand.gameObject.SetActive(true);
            hand.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
            hand.rectTransform.localScale = Vector3.one;
            motion.Loop(0.9f, t => hand.rectTransform.localScale = Vector3.one * (0.92f + 0.08f * Mathf.Cos(t * Mathf.PI * 2)));
        }
        public void Hide() { motion.CancelAll(); hand.gameObject.SetActive(false); }
        public void DisableForAttempt() { enabledForAttempt = false; Hide(); }
    }
}
