using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using PerchSort.Core;
using PerchSort.UI;
using UnityEngine;

namespace PerchSort.Tests
{
    public sealed class UiFlowTests
    {
        [TestCase(0,0,3)] [TestCase(1,0,2)] [TestCase(0,1,2)]
        [TestCase(3,0,2)] [TestCase(1,2,2)] [TestCase(0,3,2)]
        [TestCase(4,0,1)] [TestCase(2,2,1)] [TestCase(0,4,1)]
        public void StarsRewardUnassistedCompletion(int undo,int restart,int expected) =>
            Assert.That(StarRating.Calculate(undo,restart),Is.EqualTo(expected));

        [Test]
        public void LessonsAdvanceWithGameplayAndSkipPersistsOnlyTheirOwnLesson()
        {
            var progress = new ProgressData(); var tutorial = new TutorialState(progress);
            tutorial.Begin(1); Assert.That(tutorial.Active,Is.True);
            tutorial.Observe(2,false); Assert.That(tutorial.Step,Is.EqualTo(1));
            tutorial.Observe(-1,false); Assert.That(tutorial.Step,Is.Zero);
            tutorial.Observe(-1,true); Assert.That(tutorial.Step,Is.EqualTo(2));
            tutorial.Observe(-1,false); Assert.That(tutorial.Step,Is.Zero,"Undo/restart returns to selecting a source");
            tutorial.Observe(-1,true);
            tutorial.Finish(); tutorial.Begin(1); Assert.That(tutorial.Active,Is.False);
            foreach (int id in TutorialState.Levels.Skip(1))
            {
                tutorial.Begin(id); Assert.That(tutorial.Active,Is.True);
                tutorial.Finish(); tutorial.Begin(id); Assert.That(tutorial.Active,Is.False);
            }
            tutorial.Begin(4); Assert.That(tutorial.Active,Is.False);
            progress.tutorialsSeen.Clear(); tutorial.Begin(1); Assert.That(tutorial.Active,Is.True);
        }
        [Test]
        public void EveryLocaleHasIdenticalNonemptyKeysAndFormatArguments()
        {
            var english = Strings.Tables["en"];
            foreach (var table in Strings.Tables.Values)
            {
                Assert.That(table.Keys,Is.EquivalentTo(english.Keys));
                foreach (var pair in table)
                {
                    Assert.That(pair.Value,Is.Not.Empty,pair.Key);
                    string Slots(string value) => string.Join(",",Regex.Matches(value,@"\{\d+(?=[:}])").Cast<Match>().Select(m => m.Value));
                    Assert.That(Slots(pair.Value),Is.EqualTo(Slots(english[pair.Key])),pair.Key);
                }
            }
        }
        [TestCase(1)] [TestCase(8)] [TestCase(20)]
        public void HintIsLegalAndDecreasesProvenOptimalDistanceByOne(int level)
        {
            var catalog = LevelJson.Deserialize(Resources.Load<TextAsset>("Levels/levels").text);
            var state = catalog.levels[level-1].ToBoardState();
            var before = Solver.Solve(state);
            var hint = HintService.Find(state);
            Assert.That(hint.HasValue,Is.True);
            Assert.That(Rules.TryApply(state,hint.Value,out _),Is.True);
            var after = Solver.Solve(state);
            Assert.That(after.Optimal,Is.True);
            Assert.That(after.MinMoves,Is.EqualTo(before.MinMoves-1));
            Assert.That(HintService.Find(catalog.levels[level-1].ToBoardState(),1),Is.Null,"Budget exhaustion must not return an unproven hint");
        }
        [Test]
        public void V3MigrationRetainsMovesAndUnlocksThenV4RoundTripsNewState()
        {
            string dir = Path.Combine(Path.GetTempPath(),"perch-v4-"+Guid.NewGuid()); Directory.CreateDirectory(dir);
            string path = Path.Combine(dir,"progress.json");
            try
            {
                const string original = "{\"version\":3,\"levelId\":3,\"completedLevels\":[1,2],\"moves\":[{\"from\":0,\"to\":3}]}";
                File.WriteAllText(path,original); var store = new ProgressStore(path); var data = store.Load();
                Assert.That(data.version,Is.EqualTo(4)); Assert.That(data.levelId,Is.EqualTo(3));
                Assert.That(data.IsUnlocked(3),Is.True); Assert.That(data.moves[0].to,Is.EqualTo(3));
                Assert.That(data.ForLevel(1).stars,Is.EqualTo(1)); Assert.That(File.ReadAllText(path+".bak"),Is.EqualTo(original));
                data.language = "zh"; data.ForLevel(3).hintsUsed = 2; data.ForLevel(3).undos = 1; data.ForLevel(3).restarts = 1;
                data.tutorialsSeen.Add(1); store.Save(data); data = store.Load();
                Assert.That(data.language,Is.EqualTo("zh")); Assert.That(data.ForLevel(3).hintsUsed,Is.EqualTo(2));
                Assert.That(data.ForLevel(3).undos,Is.EqualTo(1)); Assert.That(data.ForLevel(3).restarts,Is.EqualTo(1));
                Assert.That(data.tutorialsSeen,Is.EqualTo(new[] {1}));
            }
            finally { Directory.Delete(dir,true); }
        }
    }
}
