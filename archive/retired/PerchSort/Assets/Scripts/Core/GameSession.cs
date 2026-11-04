using System.Collections.Generic;

namespace PerchSort.Core
{
    public sealed class GameSession
    {
        private readonly BoardState initialState;
        private readonly Stack<BoardState> history = new Stack<BoardState>();

        public BoardState State { get; private set; }
        public int UndoCount => history.Count;

        public GameSession(BoardState initialState)
        {
            this.initialState = initialState.Clone();
            State = initialState.Clone();
        }

        public bool TryMove(Move move, out MoveResult result)
        {
            var previous = State.Clone();
            if (!Rules.TryApply(State, move, out result))
                return false;
            history.Push(previous);
            return true;
        }

        public bool Undo()
        {
            if (history.Count == 0)
                return false;
            State = history.Pop();
            return true;
        }

        public void Restart()
        {
            State = initialState.Clone();
            history.Clear();
        }
    }
}
