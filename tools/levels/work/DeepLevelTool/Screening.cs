using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using PerchSort.Core;

namespace PerchSort.LevelTool
{
    internal sealed class ScreenRow
    {
        public int Id { get; set; }
        public int Par { get; set; }
        public long Milliseconds { get; set; }
        public string Failure { get; set; }
        public bool Pass => Failure == null;
    }
    internal static class Screening
    {
        internal static int Run(LevelDef[] levels, string selection, string output)
        {
            var ids = selection.Split(',').Select(int.Parse).ToHashSet();
            if (!ids.SetEquals(levels.Where(l => ids.Contains(l.id)).Select(l => l.id)))
                throw new ArgumentException("Screen contains an unknown level id.");
            var rows = new List<ScreenRow>();
            foreach (var level in levels.Where(l => ids.Contains(l.id)).OrderBy(l => l.id))
            {
                var clock = Stopwatch.StartNew();
                string failure = DeepGates.CheckData(level, 200000, 1000);
                rows.Add(new ScreenRow { Id = level.id, Par = level.par, Milliseconds = clock.ElapsedMilliseconds, Failure = failure });
                Console.WriteLine("L" + level.id + " par=" + level.par + " ms=" + clock.ElapsedMilliseconds + " " + (failure ?? "PASS"));
            }
            File.WriteAllText(output, JsonSerializer.Serialize(new { MaxStates = 200000, Milliseconds = 1000, Levels = rows }, Program.Json) + Environment.NewLine);
            return rows.All(r => r.Pass) ? 0 : 2;
        }
    }
}
