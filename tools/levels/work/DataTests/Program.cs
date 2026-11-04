using NUnitLite;
using PerchSort.EditModeTests;

internal static class Program
{
    private static int Main(string[] args) => new AutoRun(typeof(LevelsDataTests).Assembly).Execute(args);
}
