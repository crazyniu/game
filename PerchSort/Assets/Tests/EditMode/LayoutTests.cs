using NUnit.Framework;
using PerchSort.Game;
using UnityEngine;

namespace PerchSort.EditModeTests
{
    public sealed class LayoutTests
    {
        private static void Inside(Rect outer, Rect inner)
        {
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin - 0.01f));
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - 0.01f));
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax + 0.01f));
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + 0.01f));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void SpeciesTargetsRemainSeparateAroundOneSharedHouse(int count)
        {
            Rect canvas = GameLayout.NestCanvas;
            Vector2 door = canvas.position + GameLayout.HouseDoorLocal(canvas.size);
            Inside(GameLayout.SafeDesignArea, GameLayout.Centered(door, Vector2.one * GameLayout.HouseDoorDiameter(canvas.width)));
            for (int index = 0; index < count; index++)
            {
                Rect target = GameLayout.Centered(GameLayout.GoalBadgePoint(index, count), Vector2.one * 88);
                Inside(GameLayout.SafeDesignArea, target);
                Assert.That(target.Overlaps(canvas), Is.False);
                if (index > 0) Assert.That(target.Overlaps(GameLayout.Centered(GameLayout.GoalBadgePoint(index - 1, count), Vector2.one * 88)), Is.False);
            }
        }
        [TestCase(3)] [TestCase(4)]
        public void ToolHitTargetsAndTopChromeDoNotOverlap(int count)
        {
            Assert.That(GameLayout.SettingsButton.Overlaps(GameLayout.CoinPill), Is.False);
            for (int i = 0; i < count; i++)
            {
                Rect button = GameLayout.ToolButton(i, count); Inside(GameLayout.SafeDesignArea, button);
                Assert.That(button.width, Is.GreaterThanOrEqualTo(100));
                if (i > 0) Assert.That(button.xMin - GameLayout.ToolButton(i - 1, count).xMax, Is.GreaterThanOrEqualTo(40));
            }
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void SeatsRemainVisibleAndBranchTouchRegionsDoNotOverlapOnTheSameEdge(int rows)
        {
            float scale = GameLayout.BirdScale(rows);
            for (int row = 0; row < rows; row++) foreach (bool right in new[] { false, true })
            {
                Rect hit = GameLayout.BranchHit(row, rows, right); Inside(GameLayout.SafeDesignArea, hit);
                float foot = GameLayout.RowY(row, rows, right);
                for (int seat = 0; seat < 4; seat++)
                    Inside(GameLayout.SafeDesignArea, GameLayout.Centered(GameLayout.BirdSlot(seat, foot, right, scale), GameLayout.BirdDisplaySize * scale));
                if (row > 0) Assert.That(hit.Overlaps(GameLayout.BranchHit(row - 1, rows, right)), Is.False);
            }
        }
        [Test, Combinatorial]
        public void OppositeBirdCanvasesAndBranchTipsNeverIntersect(
            [Values(2, 3, 4, 5)] int rows, [Values(2, 3, 4, 5, 6, 7)] int species)
        {
            float scale = GameLayout.BirdScale(rows);
            Vector2 size = GameLayout.BirdDisplaySize * scale;
            // Cover the full four-seat envelope, not just occupied seats in one authored board.
            // Species count changes the house band, never the cross-board seat clearance.
            for (int left = 0; left < rows; left++) for (int right = 0; right < rows; right++)
            {
                float leftY = GameLayout.RowY(left, rows), rightY = GameLayout.RowY(right, rows, true);
                var leftWood = new Rect(0, leftY + GameLayout.BranchCenterBelowFeet - GameLayout.BranchHeight / 2,
                    GameLayout.BranchWidth, GameLayout.BranchHeight);
                var rightWood = new Rect(GameLayout.DesignWidth - GameLayout.BranchWidth,
                    rightY + GameLayout.BranchCenterBelowFeet - GameLayout.BranchHeight / 2, GameLayout.BranchWidth, GameLayout.BranchHeight);
                for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++)
                {
                    Rect first = GameLayout.Centered(GameLayout.BirdSlot(a, leftY, false, scale), size);
                    Rect second = GameLayout.Centered(GameLayout.BirdSlot(b, rightY, true, scale), size);
                    Assert.That(first.Overlaps(second), Is.False, $"L{left}:{a}/R{right}:{b}");
                    Assert.That(leftWood.Overlaps(second), Is.False, "Left tip covers an opposite bird.");
                    Assert.That(rightWood.Overlaps(first), Is.False, "Right tip covers an opposite bird.");
                    for (int house = 0; house < species; house++)
                    {
                        Rect roof = GameLayout.HouseCanvas(house, species);
                        Assert.That(roof.Overlaps(first) || roof.Overlaps(second), Is.False, "House band covers a perched bird.");
                    }
                }
            }
            for (int row = 0; row < rows; row++)
                Assert.That(GameLayout.RowY(row, rows, true) - GameLayout.RowY(row, rows),
                    Is.EqualTo(GameLayout.RowPitch(rows) / 2).Within(1), "Authored integer feet lines round the interleave to the nearest pixel.");
        }
        [TestCase("win", "replay", "next", "home")]
        [TestCase("settings", "sound", "replay", "home")]
        public void ModalActionTargetsRemainSeparate(string screen, string first, string second, string third)
        {
            var controls = new[] { GameLayout.Screen(screen, first), GameLayout.Screen(screen, second), GameLayout.Screen(screen, third) };
            for (int i = 0; i < controls.Length; i++)
            {
                Inside(GameLayout.SafeDesignArea, controls[i]);
                for (int j = 0; j < i; j++) Assert.That(controls[i].Overlaps(controls[j]), Is.False);
            }
        }
        [Test]
        public void EveryMapNodeFitsItsMeasuredWorldAndLevelsClimbContinuously()
        {
            float previousY = float.MaxValue;
            for (int world = 1; world <= 4; world++) for (int index = 0; index < 10; index++)
            {
                Vector2 point = GameLayout.MapNode(world, index);
                Inside(new Rect(0, GameLayout.WorldTop(world), 1080, 3000), GameLayout.Centered(point, new Vector2(180, 150)));
                Assert.That(point.y, Is.LessThan(previousY)); previousY = point.y;
            }
        }
    }
}
