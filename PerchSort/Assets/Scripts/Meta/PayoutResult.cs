namespace PerchSort.Meta
{
    public readonly struct PayoutResult
    {
        public PlayerProfile Profile { get; }
        public WinPayout Payout { get; }
        public int Stars { get; }
        internal PayoutResult(PlayerProfile profile, WinPayout payout, int stars = 0)
        { Profile = profile; Payout = payout; Stars = stars; }
    }
}
