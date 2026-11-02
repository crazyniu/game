using System.Collections.Generic;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class TutorialHand : MonoBehaviour
    {
        private static readonly Dictionary<int, Move[]> scripts = new Dictionary<int, Move[]>
        {
            { 1, new[] { new Move(0, 1), new Move(2, 0) } }
        };
        private Image hand;
        private V5Motion motion;
        private Move[] steps;
        private int cursor;
        private bool enabledForAttempt;
        public bool Completed => steps != null && cursor == steps.Length;
        public void Configure(V5Art art)
        {
            hand = V5UI.Image(transform, "Tutorial hand", art.Get("hand_pointer"), Vector2.zero, new Vector2(160, 160));
            hand.rectTransform.pivot = new Vector2(0.115f, 0.97f);
            motion = gameObject.AddComponent<V5Motion>(); hand.gameObject.SetActive(false);
        }
        public void Begin(int level, bool firstPlay)
        {
            scripts.TryGetValue(level, out steps); cursor = 0; enabledForAttempt = firstPlay; Hide();
        }
        public void ActionCompleted(GameSession session)
        {
            if (!enabledForAttempt || steps == null || Completed) return;
            var expected = steps[cursor];
            foreach (var item in session.Events)
            {
                bool match = item is Transferred transfer && expected.Kind == MoveKind.Transfer && transfer.From == expected.From && transfer.To == expected.To;
                match |= item is Swivelled swivel && expected.Kind == MoveKind.Swivel && swivel.Branch == expected.From;
                if (match) { cursor++; return; }
                if (item is Transferred || item is Swivelled) { DisableForAttempt(); return; }
            }
        }
        public void Refresh(GameSession session, BoardView board, bool blocked)
        {
            Hide();
            if (!enabledForAttempt || steps == null || Completed || blocked || session.State.IsWon) return;
            var move = steps[cursor];
            Vector2 point;
            if (move.Kind == MoveKind.Swivel)
            {
                if (!GameSession.CanSwivel(session.State, move.From)) return;
                point = board.Branches[move.From].KnotPoint;
            }
            else
            {
                if (!GameSession.CanMove(session.State, move.From, move.To)) return;
                int branch = session.SelectedBranch == move.From ? move.To : move.From;
                int slot = Mathf.Max(0, session.State.Branches[branch].Birds.Count - 1);
                point = board.Branches[branch].Slot(slot) + new Vector2(0, 55);
            }
            hand.gameObject.SetActive(true); hand.rectTransform.anchoredPosition = new Vector2(point.x, -point.y); hand.rectTransform.localScale = Vector3.one;
            motion.Loop(0.9f, t => hand.rectTransform.localScale = Vector3.one * (0.92f + 0.08f * Mathf.Cos(t * Mathf.PI * 2)));
        }
        public void Hide() { motion.CancelAll(); hand.gameObject.SetActive(false); }
        public void DisableForAttempt() { enabledForAttempt = false; Hide(); }
    }
}
