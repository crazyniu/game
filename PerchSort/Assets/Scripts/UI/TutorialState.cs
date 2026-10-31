namespace PerchSort.UI
{
    // A lesson becomes seen on completion or Skip, not merely on opening the level.
    public sealed class TutorialState
    {
        public static readonly int[] Levels = { 1, 2, 3, 8, 11, 12, 20 };
        private readonly ProgressData progress;
        public int Level { get; private set; }
        public int Step { get; private set; }
        public bool Active { get; private set; }
        public TutorialState(ProgressData data) => progress = data;
        public void Begin(int level)
        {
            Level = level; Step = 0;
            Active = System.Array.IndexOf(Levels, level) >= 0 && !progress.tutorialsSeen.Contains(level);
        }
        public void Observe(int selected, bool moved)
        {
            if (!Active || Level != 1) return;
            Step = moved ? 2 : selected >= 0 ? 1 : 0;
        }
        public void Finish()
        {
            if (!Active) return;
            progress.tutorialsSeen.Add(Level); Active = false;
        }
        public string Key => Level == 1 ? "tutorial1_" + Step : "tutorial" + Level;
    }
}
