#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
namespace PerchSort.Game
{
    public interface IScreenStates
    {
        string ScreenName { get; }
        IReadOnlyList<string> StateNames { get; }
        void Apply(string stateName);
    }
}
#endif
