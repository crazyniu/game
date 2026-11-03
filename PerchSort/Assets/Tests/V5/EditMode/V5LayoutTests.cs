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
            var groups = new[] { V6Layout.CoinPill, V6Layout.LevelSign, V6Layout.SettingsButton, V6Layout.PhaseDial,
                V6Layout.GoalTray, V6Layout.ToolButton(0), V6Layout.ToolButton(1), V6Layout.ToolButton(2), V6Layout.ToolButton(3),
                V6Layout.Centered(V6Layout.LightStepPoint, Vector2.one * V6Layout.FeatherSize) };
            for (int i = 0; i < groups.Length; i++)
            {
                Inside(V6Layout.SafeDesignArea, groups[i]);
                for (int j = i + 1; j < groups.Length; j++) Assert.That(Gap(groups[i], groups[j]), Is.GreaterThanOrEqualTo(39.999f), i + "/" + j);
            }
            for (int i = 0; i < species; i++)
            {
                Rect chip = V6Layout.GoalChip(i, species); Inside(V6Layout.GoalTray, chip);
                Inside(chip, V6Layout.Centered(chip.position + V6Layout.GoalGiftCenter, Vector2.one * V6Layout.GiftBadgeSize));
                if (i > 0) Assert.That(Gap(chip, V6Layout.GoalChip(i - 1, species)), Is.GreaterThanOrEqualTo(15.999f));
            }
        }
        [Test]
        public void TopRowHasSixtyPixelGapsIncludingTheDayDial()
        {
            var row = new[] { V6Layout.CoinPill, V6Layout.PhaseDial, V6Layout.LevelSign, V6Layout.SettingsButton };
            for (int i = 0; i < row.Length - 1; i++) Assert.That(row[i + 1].xMin - row[i].xMax, Is.GreaterThanOrEqualTo(60));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
        public void BoardArtworkAndSeatFramesFitTheBandWithoutTouchingOppositeTips(int rows)
        {
            float scale = V6Layout.BirdScale(rows);
            Assert.That(scale, Is.InRange(0.72f, rows <= 3 ? 1.08f : 1));
            Rect band = new Rect(0, V6Layout.BoardTop, V6Layout.DesignWidth, V6Layout.BoardHeight);
            for (int row = 0; row < rows; row++)
            {
                Assert.That(V6Layout.RowY(row, rows, true) - V6Layout.RowY(row, rows), Is.EqualTo(60));
                foreach (bool right in new[] { false, true })
                {
                    float foot = V6Layout.RowY(row, rows, right);
                    var branch = new Rect(right ? 570 : 0, foot - 32, V6Layout.BranchWidth, V6Layout.BranchHeight);
                    Inside(band, branch); Inside(band, V6Layout.BranchHit(row, rows, right));
                    for (int seat = 0; seat < 4; seat++)
                    {
                        Rect bird = V6Layout.Centered(V6Layout.BirdSlot(seat, foot, right, scale), Vector2.one * V6Layout.BirdSize * scale);
                        Inside(band, bird);
                        Assert.That(right ? bird.xMin : 1080 - bird.xMax, Is.GreaterThanOrEqualTo(570));
                    }
                    if (row > 0) Assert.That(V6Layout.BranchHit(row, rows, right).yMin,
                        Is.GreaterThanOrEqualTo(V6Layout.BranchHit(row - 1, rows, right).yMax - 0.001f));
                }
            }
            Assert.That(V6Layout.DesignWidth - 2 * V6Layout.BranchWidth, Is.GreaterThanOrEqualTo(60));
        }
        [Test]
        public void ModalButtonsStayWithinTheirPanelsWithFortyPixelClearance()
        {
            var panels = new[] { V6Layout.WinPanel, V6Layout.SettingsPanel };
            var controls = new[] { new[] { V6Layout.ReplayButton, V6Layout.NextButton, V6Layout.HomeButton },
                new[] { V6Layout.SoundButton, V6Layout.RestartButton, V6Layout.CloseButton, V6Layout.ReturnMapButton } };
            for (int p = 0; p < panels.Length; p++) for (int i = 0; i < controls[p].Length; i++)
            {
                Inside(panels[p], controls[p][i]);
                for (int j = i + 1; j < controls[p].Length; j++) Assert.That(Gap(controls[p][i], controls[p][j]), Is.GreaterThanOrEqualTo(39.999f));
            }
        }
    }
}
