using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class TutorialHand : MonoBehaviour
    {
        private static readonly Dictionary<int, Move[]> scripts = new Dictionary<int, Move[]>
        {
            { 1, new[] { new Move(0, 1), new Move(2, 0) } },
            { 2, new[] { new Move(0, 2), new Move(1, 0), new Move(2, 1) } },
            { 3, new[] { new Move(0, 3), new Move(1, 0), new Move(3, 1), new Move(4, 2) } },
            { 6, new[] { new Move(2, 3), new Move(2, 3), new Move(0, 2), new Move(1, 0), new Move(2, 1) } },
            { 11, new[] { new Move(0, 4), new Move(1, 0), new Move(4, 1) } },
            { 15, new[] { new Move(0, 1), new Move(0, 2) } },
            { 21, new[] { new Move(0, 1), new Move(2, 0), new Move(6, 2) } },
            { 24, new[] { Move.Swivel(0), new Move(0, 2), new Move(0, 3) } },
            { 31, new[] { new Move(0, 6), new Move(1, 0), new Move(1, 6) } },
            { 33, new[] { new Move(0, 6), new Move(1, 0), new Move(1, 6) } }
        };
        private Image hand;
        private GameMotion motion;
        private Move[] steps;
        private RectTransform targetBody;
        private int cursor;
        private bool enabledForAttempt, suggestFirefly;
        public bool Completed => steps != null && cursor == steps.Length;
        public bool SaplingPromptPending { get; private set; }
        public void Configure(GameArt art)
        {
            hand = GameUI.Image(transform, "Tutorial hand", art.Get("hand_pointer"), Vector2.zero, new Vector2(160, 160));
            hand.rectTransform.pivot = new Vector2(0.115f, 0.97f);
            motion = gameObject.AddComponent<GameMotion>(); hand.gameObject.SetActive(false);
        }
        public void Begin(int level, bool firstPlay, bool ownsSapling)
        {
            scripts.TryGetValue(level, out steps); cursor = 0; enabledForAttempt = firstPlay; Hide();
            suggestFirefly = level == 2;
            SaplingPromptPending = level == 4 && firstPlay && ownsSapling;
        }
        public void ActionCompleted(GameSession session)
        {
            if (!enabledForAttempt || steps == null || Completed) return;
            var expected = steps[cursor];
            foreach (var item in session.Events)
            {
                if (item is BoosterUsed used && used.Booster == BoosterKind.Firefly) suggestFirefly = false;
                bool match = item is Transferred transfer && expected.Kind == MoveKind.Transfer && transfer.From == expected.From && transfer.To == expected.To;
                match |= item is Swivelled swivel && expected.Kind == MoveKind.Swivel && swivel.Branch == expected.From;
                if (match) { suggestFirefly = false; cursor++; return; }
                if (item is Transferred || item is Swivelled) { DisableForAttempt(); return; }
            }
        }
        public void Refresh(GameSession session, BoardView board, bool blocked)
        {
            Hide();
            if (!enabledForAttempt || blocked || session.State.IsWon) return;
            Vector2 point;
            if (SaplingPromptPending) point = GameLayout.ToolButton(2).center;
            else
            {
                if (steps == null || Completed) return;
                var move = steps[cursor];
                if (suggestFirefly && session.Inventory[BoosterKind.Firefly] > 0 && session.SelectedBranch < 0)
                    point = GameLayout.ToolButton(1).center;
                else if (move.Kind == MoveKind.Swivel)
                {
                    if (!GameSession.CanSwivel(session.State, move.From)) return;
                    point = board.Branches[move.From].KnotPoint;
                }
                else
                {
                    if (!GameSession.CanMove(session.State, move.From, move.To)) return;
                    if (session.SelectedBranch >= 0) return;
                    var birds = session.State.Branches[move.From].Birds;
                    if (birds.Count == 0) return;
                    targetBody = (RectTransform)board.Birds[birds[birds.Count - 1].Id].transform.Find("Bird body");
                    point = Vector2.zero;
                }
            }
            hand.gameObject.SetActive(true); hand.rectTransform.anchoredPosition = new Vector2(point.x, -point.y); hand.rectTransform.localScale = Vector3.one;
            PlaceHand();
            motion.Loop(0.9f, t => hand.rectTransform.localScale = Vector3.one * (0.96f + 0.04f * Mathf.Cos(t * Mathf.PI * 2)));
        }
        private void LateUpdate()
        {
            if (hand != null && hand.gameObject.activeSelf) PlaceHand();
        }
        private void PlaceHand()
        {
            var rect = hand.rectTransform;
            if (targetBody != null)
            {
                Rect body = targetBody.rect;
                Vector3 tip = targetBody.TransformPoint(new Vector3(body.center.x + 30, body.yMin + body.height * 0.32f));
                var layer = (RectTransform)transform;
                Vector3 local = layer.InverseTransformPoint(tip);
                rect.anchoredPosition = new Vector2(local.x - layer.rect.xMin, local.y - layer.rect.yMax);
            }
            Vector2 position = rect.anchoredPosition;
            position.x = Mathf.Clamp(position.x, rect.pivot.x * rect.rect.width,
                GameLayout.DesignWidth - 24 - (1 - rect.pivot.x) * rect.rect.width);
            position.y = Mathf.Clamp(position.y, -GameLayout.DesignHeight + rect.pivot.y * rect.rect.height,
                -(1 - rect.pivot.y) * rect.rect.height);
            rect.anchoredPosition = position;
        }
        public void DismissSaplingPrompt()
        {
            if (!SaplingPromptPending) return;
            SaplingPromptPending = false; Hide();
        }
        public void Hide() { targetBody = null; motion.CancelAll(); hand.gameObject.SetActive(false); }
        public void DisableForAttempt() { enabledForAttempt = false; SaplingPromptPending = false; Hide(); }
    }
}
