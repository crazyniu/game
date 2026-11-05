namespace PerchSort.Meta
{
    public readonly struct WinPayout
    {
        public int FirstClear { get; }
        public int Replay { get; }
        public int StarImprovement { get; }
        public int Chorus { get; }
        public int WorldComplete { get; }
        public int DailyGift { get; }
        public int NewSpecies { get; }
        public int Total => checked(FirstClear + Replay + StarImprovement + Chorus + WorldComplete + DailyGift + NewSpecies);
        internal WinPayout(int firstClear = 0, int replay = 0, int improvement = 0, int chorus = 0,
            int world = 0, int daily = 0, int species = 0)
        {
            FirstClear = firstClear; Replay = replay; StarImprovement = improvement;
            Chorus = chorus; WorldComplete = world; DailyGift = daily; NewSpecies = species;
        }
    }
}
