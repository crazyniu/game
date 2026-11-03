using NUnitLite;
using PerchSort.V5.Tests;

internal static class Program
{
    private static int Main(string[] args) => new AutoRun(typeof(V5LevelsDataTests).Assembly).Execute(args);
}
