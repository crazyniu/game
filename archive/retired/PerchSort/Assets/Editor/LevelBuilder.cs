using System.IO;
using PerchSort.Core;
using UnityEditor;
using UnityEngine;

public static class LevelBuilder
{
    // Also callable with -batchmode -executeMethod LevelBuilder.BuildAll.
    [MenuItem("Perch Sort/Build 40 Levels")]
    public static void BuildAll()
    {
        var catalog = LevelGenerator.GenerateCampaign((level, solution) =>
            Debug.Log($"Level {level.id:D2}: K={level.speciesCount} shelves={level.perchCount} baskets={level.baskets} " +
                $"minMoves={level.solverMinMoves} nodes={solution.VisitedNodes}"));
        string directory = Path.Combine(Application.dataPath, "Resources/Levels");
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "levels.json"), LevelJson.Serialize(catalog) + "\n");
        AssetDatabase.Refresh();
        Debug.Log("Built all 40 v2 levels with proven shortest solutions.");
    }
}
