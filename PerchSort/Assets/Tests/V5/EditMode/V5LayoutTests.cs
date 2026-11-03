using NUnit.Framework;
using PerchSort.V5.Game;
using UnityEngine;

namespace PerchSort.V5.Tests
{
    public sealed class V5LayoutTests
    {
        private static float Gap(Rect a, Rect b)
        {
            float dx = Mathf.Max(0, Mathf.Max(a.xMin - b.xMax, b.xMin - a.xMax));
            float dy = Mathf.Max(0, Mathf.Max(a.yMin - b.yMax, b.yMin - a.yMax));
            return Mathf.Sqrt(dx * dx + dy * dy);
        }
        private static void Inside(Rect outer, Rect inner)
        {
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin - 0.001f));
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin - 0.001f));
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax + 0.001f));
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax + 0.001f));
        }
        [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)]
        public void IndependentControlsStayClearAndGoalChildrenFitTheirTray(int species)
        {
            var groups = new[] { V7Layout.CoinPill, V7Layout.LevelSign, V7Layout.SettingsButton, V7Layout.PhaseDial,
                V7Layout.GoalTray, V7Layout.ToolButton(0), V7Layout.ToolButton(1), V7Layout.ToolButton(2), V7Layout.ToolButton(3),
                V7Layout.Centered(V7Layout.LightStepPoint, Vector2.one * V7Layout.FeatherSize) };
            for (int i = 0; i < groups.Length; i++)
            {
                Inside(V7Layout.SafeDesignArea, groups[i]);
                for (int j = i + 1; j < groups.Length; j++) Assert.That(Gap(groups[i], groups[j]), Is.GreaterThanOrEqualTo(39.999f), i + "/" + j);
            }
            for (int i = 0; i < species; i++)
            {
                Rect chip = V7Layout.GoalChip(i, species); Inside(V7Layout.GoalTray, chip);
                Inside(chip, V7Layout.Centered(chip.position + V7Layout.GoalGiftCenter, Vector2.one * V7Layout.GiftBadgeSize));
                if (i > 0) Assert.That(Gap(chip, V7Layout.GoalChip(i - 1, species)), Is.GreaterThanOrEqualTo(15.999f));
            }
        }
        [Test]
        public void TopRowHasSixtyPixelGapsIncludingTheDayDial()
        {
            var row = new[] { V7Layout.CoinPill, V7Layout.PhaseDial, V7Layout.LevelSign, V7Layout.SettingsButton };
            for (int i = 0; i < row.Length - 1; i++) Assert.That(row[i + 1].xMin - row[i].xMax, Is.GreaterThanOrEqualTo(60));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void BoardArtworkAndSeatFramesFitTheBandWithoutTouchingOppositeTips(int rows)
        {
            float scale = V7Layout.BirdScale(rows);
            Assert.That(scale, Is.InRange(0.72f, rows <= 3 ? 1.08f : 1));
            Rect band = new Rect(0, V7Layout.BoardTop, V7Layout.DesignWidth, V7Layout.BoardHeight);
            for (int row = 0; row < rows; row++)
            {
                Assert.That(V7Layout.RowY(row, rows, true) - V7Layout.RowY(row, rows), Is.EqualTo(60));
                foreach (bool right in new[] { false, true })
                {
                    float foot = V7Layout.RowY(row, rows, right);
                    var branch = new Rect(right ? 570 : 0, foot - 32, V7Layout.BranchWidth, V7Layout.BranchHeight);
                    Inside(band, branch); Inside(band, V7Layout.BranchHit(row, rows, right));
                    for (int seat = 0; seat < 4; seat++)
                    {
                        Rect bird = V7Layout.Centered(V7Layout.BirdSlot(seat, foot, right, scale), Vector2.one * V7Layout.BirdSize * scale);
                        Inside(band, bird);
                        Assert.That(right ? bird.xMin : 1080 - bird.xMax, Is.GreaterThanOrEqualTo(570));
                    }
                    if (row > 0) Assert.That(V7Layout.BranchHit(row, rows, right).yMin,
                        Is.GreaterThanOrEqualTo(V7Layout.BranchHit(row - 1, rows, right).yMax - 0.001f));
                }
            }
            Assert.That(V7Layout.DesignWidth - 2 * V7Layout.BranchWidth, Is.GreaterThanOrEqualTo(60));
        }
        [Test]
        public void ModalButtonsStayWithinTheirPanelsWithFortyPixelClearance()
        {
            var panels = new[] { V7Layout.WinPanel, V7Layout.SettingsPanel };
            var controls = new[] { new[] { V7Layout.ReplayButton, V7Layout.NextButton, V7Layout.HomeButton },
                new[] { V7Layout.SoundButton, V7Layout.RestartButton, V7Layout.CloseButton, V7Layout.ReturnMapButton } };
            for (int p = 0; p < panels.Length; p++) for (int i = 0; i < controls[p].Length; i++)
            {
                Inside(panels[p], controls[p][i]);
                for (int j = i + 1; j < controls[p].Length; j++) Assert.That(Gap(controls[p][i], controls[p][j]), Is.GreaterThanOrEqualTo(39.999f));
            }
        }
    }
}
