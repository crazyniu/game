using NUnit.Framework;
using PerchSort.V5.Game;
using UnityEngine;

namespace PerchSort.V5.Tests
{
    public sealed class V5LayoutTests
    {
        private static void Inside(Rect outer, Rect inner)
        {
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin - 0.01f));
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - 0.01f));
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax + 0.01f));
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + 0.01f));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void EveryHouseHasAVisibleUnambiguousDoorAndItsSharedCanvasStaysInBounds(int count)
        {
            float previous = -1;
            for (int index = 0; index < count; index++)
            {
                Rect canvas = V7Layout.HouseCanvas(index, count);
                Vector2 door = canvas.position + V7Layout.HouseDoorLocal(canvas.size);
                float diameter = V7Layout.HouseDoorDiameter(canvas.width);
                Inside(V7Layout.SafeDesignArea, V7Layout.Centered(door, Vector2.one * diameter));
                Assert.That(door.x, Is.GreaterThan(previous + diameter));
                previous = door.x;
            }
        }
        [TestCase(3)] [TestCase(4)]
        public void ToolHitTargetsAndTopChromeDoNotOverlap(int count)
        {
            Assert.That(V7Layout.SettingsButton.Overlaps(V7Layout.CoinPill), Is.False);
            for (int i = 0; i < count; i++)
            {
                Rect button = V7Layout.ToolButton(i, count); Inside(V7Layout.SafeDesignArea, button);
                Assert.That(button.width, Is.GreaterThanOrEqualTo(100));
                if (i > 0) Assert.That(button.xMin - V7Layout.ToolButton(i - 1, count).xMax, Is.GreaterThanOrEqualTo(40));
            }
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void SeatsRemainVisibleAndBranchTouchRegionsDoNotOverlapOnTheSameEdge(int rows)
        {
            float scale = V7Layout.BirdScale(rows);
            for (int row = 0; row < rows; row++) foreach (bool right in new[] { false, true })
            {
                Rect hit = V7Layout.BranchHit(row, rows, right); Inside(V7Layout.SafeDesignArea, hit);
                float foot = V7Layout.RowY(row, rows, right);
                for (int seat = 0; seat < 4; seat++)
                    Inside(V7Layout.SafeDesignArea, V7Layout.Centered(V7Layout.BirdSlot(seat, foot, right, scale), V7Layout.BirdDisplaySize * scale));
                if (row > 0) Assert.That(hit.Overlaps(V7Layout.BranchHit(row - 1, rows, right)), Is.False);
            }
        }
        [Test, Combinatorial]
        public void OppositeBirdCanvasesAndBranchTipsNeverIntersect(
            [Values(2, 3, 4, 5)] int rows, [Values(2, 3, 4, 5, 6, 7)] int species)
        {
            float scale = V7Layout.BirdScale(rows);
            Vector2 size = V7Layout.BirdDisplaySize * scale;
            // Cover the full four-seat envelope, not just occupied seats in one authored board.
            // Species count changes the house band, never the cross-board seat clearance.
            for (int left = 0; left < rows; left++) for (int right = 0; right < rows; right++)
            {
                float leftY = V7Layout.RowY(left, rows), rightY = V7Layout.RowY(right, rows, true);
                var leftWood = new Rect(0, leftY + V7Layout.BranchCenterBelowFeet - V7Layout.BranchHeight / 2,
                    V7Layout.BranchWidth, V7Layout.BranchHeight);
                var rightWood = new Rect(V7Layout.DesignWidth - V7Layout.BranchWidth,
                    rightY + V7Layout.BranchCenterBelowFeet - V7Layout.BranchHeight / 2, V7Layout.BranchWidth, V7Layout.BranchHeight);
                for (int a = 0; a < 4; a++) for (int b = 0; b < 4; b++)
                {
                    Rect first = V7Layout.Centered(V7Layout.BirdSlot(a, leftY, false, scale), size);
                    Rect second = V7Layout.Centered(V7Layout.BirdSlot(b, rightY, true, scale), size);
                    Assert.That(first.Overlaps(second), Is.False, $"L{left}:{a}/R{right}:{b}");
                    Assert.That(leftWood.Overlaps(second), Is.False, "Left tip covers an opposite bird.");
                    Assert.That(rightWood.Overlaps(first), Is.False, "Right tip covers an opposite bird.");
                    for (int house = 0; house < species; house++)
                    {
                        Rect roof = V7Layout.HouseCanvas(house, species);
                        Assert.That(roof.Overlaps(first) || roof.Overlaps(second), Is.False, "House band covers a perched bird.");
                    }
                }
            }
            for (int row = 0; row < rows; row++)
                Assert.That(V7Layout.RowY(row, rows, true) - V7Layout.RowY(row, rows),
                    Is.EqualTo(V7Layout.RowPitch(rows) / 2).Within(0.01f), "Opposite rows must interleave.");
        }
        [TestCase("win", "replay", "next", "home")]
        [TestCase("settings", "sound", "replay", "home")]
        public void ModalActionTargetsRemainSeparate(string screen, string first, string second, string third)
        {
            var controls = new[] { V7Layout.Screen(screen, first), V7Layout.Screen(screen, second), V7Layout.Screen(screen, third) };
            for (int i = 0; i < controls.Length; i++)
            {
                Inside(V7Layout.SafeDesignArea, controls[i]);
                for (int j = 0; j < i; j++) Assert.That(controls[i].Overlaps(controls[j]), Is.False);
            }
        }
        [Test]
        public void EveryMapNodeFitsItsMeasuredWorldAndLevelsClimbContinuously()
        {
            float previousY = float.MaxValue;
            for (int world = 1; world <= 4; world++) for (int index = 0; index < 10; index++)
            {
                Vector2 point = V7Layout.MapNode(world, index);
                Inside(new Rect(0, V7Layout.WorldTop(world), 1080, 3000), V7Layout.Centered(point, new Vector2(180, 150)));
                Assert.That(point.y, Is.LessThan(previousY)); previousY = point.y;
            }
        }
    }
}
