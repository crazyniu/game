namespace Game.UI
{
    // Global motion switch. Tests and still screenshots set Instant so states settle in
    // one frame; timed evidence and the user's review always run with Instant false.
    public static class UiMotion
    {
        public static bool Instant { get; set; }

        // Reduced-motion preference from the settings screen or the device.
        public static bool Reduced { get; set; }

        public static float Duration(float seconds) => Instant ? 0f : seconds;
    }
}
