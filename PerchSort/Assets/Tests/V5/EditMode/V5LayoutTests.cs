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
