using PerchSort.Meta;

namespace PerchSort.Meta.EditModeTests
{
    internal static class Profiles
    {
        internal static PlayerProfile ClearThrough(int last, int moves = 10, int par = 10)
        {
            var profile = new PlayerProfile();
            for (int level = 1; level <= last; level++) profile = Economy.OnWin(profile, level, moves, par).Profile;
            return profile;
        }
    }
}
