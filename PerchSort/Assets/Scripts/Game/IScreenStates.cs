#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
namespace PerchSort.Game
{
    // Review-only fixtures. Public sealed implementations expose a (GameRoot) constructor.
    // Apply must use the root's nonpersistent preview path; it must never read or write player saves.
    public interface IScreenStates
    {
        string ScreenName { get; }
        IReadOnlyList<string> StateNames { get; }
        void Apply(string stateName);
    }
}
#endif
