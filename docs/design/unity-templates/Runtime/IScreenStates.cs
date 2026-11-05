using System.Collections.Generic;

namespace Game.UI
{
    // Implemented by every fake data source. The state names are exactly the state list
    // of the screen spec, so the state switcher and the tests can reach every state.
    public interface IScreenStates
    {
        string ScreenName { get; }
        IReadOnlyList<string> StateNames { get; }
        void Apply(string stateName);
    }
}
