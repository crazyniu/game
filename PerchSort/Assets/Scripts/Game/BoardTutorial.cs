using System.Collections.Generic;
using PerchSort.Core;
using UnityEngine;

namespace PerchSort.Game
{
    // Session-side tutorial adapter. The hand itself receives a target, never rules or progress.
    public sealed class BoardTutorial
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
        private readonly IBoardView view;
        private Move[] steps;
        private int cursor;
        private bool enabledForAttempt, suggestFirefly;
        public bool Completed => steps != null && cursor == steps.Length;
        public bool SaplingPromptPending { get; private set; }
        public BoardTutorial(IBoardView board) { view = board; }
        public void Begin(int level, bool firstPlay, bool ownsSapling)
        {
            scripts.TryGetValue(level, out steps); cursor = 0; enabledForAttempt = firstPlay; Hide();
            suggestFirefly = level == 2; SaplingPromptPending = level == 4 && firstPlay && ownsSapling;
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
            if (!enabledForAttempt || blocked || session.State.IsWon) { Hide(); return; }
            var target = new TutorialPresentation { Visible = true };
            if (SaplingPromptPending) { target.TargetKind = "tool"; target.TargetId = 2; target.Point = GameLayout.ToolButton(2).center; }
            else
            {
                if (steps == null || Completed) { Hide(); return; }
                var move = steps[cursor];
                if (suggestFirefly && session.Inventory[BoosterKind.Firefly] > 0 && session.SelectedBranch < 0)
                { target.TargetKind = "tool"; target.TargetId = 1; target.Point = GameLayout.ToolButton(1).center; }
                else if (move.Kind == MoveKind.Swivel)
                {
                    if (!GameSession.CanSwivel(session.State, move.From)) { Hide(); return; }
                    target.TargetKind = "knot"; target.TargetId = move.From; target.Point = board.Branches[move.From].KnotPoint;
                }
                else
                {
                    if (!GameSession.CanMove(session.State, move.From, move.To)) { Hide(); return; }
                    bool destination = session.SelectedBranch == move.From;
                    int branch = destination ? move.To : move.From;
                    if (session.SelectedBranch >= 0 && !destination) { Hide(); return; }
                    int seat = destination ? session.State.Branches[branch].Birds.Count : session.State.Branches[branch].Birds.Count - 1;
                    target.TargetKind = destination ? "branch" : "bird"; target.TargetId = branch;
                    target.Point = board.Branches[branch].Slot(Mathf.Max(0, seat)) + new Vector2(26, -20);
                }
            }
            view.SetTutorial(target);
        }
        public void DismissSaplingPrompt() { if (SaplingPromptPending) { SaplingPromptPending = false; Hide(); } }
        public void Hide() => view.SetTutorial(new TutorialPresentation());
        public void DisableForAttempt() { enabledForAttempt = false; SaplingPromptPending = false; Hide(); }
    }
}
