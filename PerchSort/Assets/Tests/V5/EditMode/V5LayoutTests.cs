using System;
using NUnit.Framework;
using PerchSort.V5.Game;
using UnityEngine;

namespace PerchSort.V5.Tests
{
    public sealed class V5LayoutTests
    {
        private static float ClearGap(Rect a, Rect b)
        {
            float dx = Mathf.Max(0, Mathf.Max(a.xMin - b.xMax, b.xMin - a.xMax));
            float dy = Mathf.Max(0, Mathf.Max(a.yMin - b.yMax, b.yMin - a.yMax));
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private static void AssertInside(Rect outer, Rect inner)
        {
            Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin));
            Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin));
            Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax));
            Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax));
        }

        [Test]
        public void IndependentHudGroupsHaveAtLeastFortyPixelsClearance()
        {
            // Decorative limb/canopy overlap and nested pill contents are intentionally excluded.
            var named = new[]
            {
                ("coins", V5Layout.CoinPill), ("settings", V5Layout.SettingsButton),
                ("Song", V5Layout.SongPill), ("level", V5Layout.LevelSign), ("phase", V5Layout.PhaseDial),
                ("undo", V5Layout.ToolButton(0)), ("Firefly", V5Layout.ToolButton(1)),
                ("Sapling", V5Layout.ToolButton(2)), ("world tool", V5Layout.ToolButton(3)),
                ("stuck", V5Layout.StuckBubble), ("feather", V5Layout.Centered(V5Layout.LightStepPoint, Vector2.one * 70))
            };
            for (int i = 0; i < named.Length; i++)
            {
                AssertInside(new Rect(0, 0, 1080, 1920), named[i].Item2);
                for (int j = i + 1; j < named.Length; j++)
                    Assert.That(ClearGap(named[i].Item2, named[j].Item2), Is.GreaterThanOrEqualTo(40),
                        named[i].Item1 + " / " + named[j].Item1);
            }
        }

        [TestCase(2)]
        [TestCase(3)]
        [TestCase(7)]
        public void SpeciesHousesAndGiftSignsFitWithoutCoveringHudOrBoard(int count)
        {
            var hud = new[] { V5Layout.CoinPill, V5Layout.SettingsButton, V5Layout.LevelSign, V5Layout.PhaseDial, V5Layout.SongPill };
            for (int i = 0; i < count; i++)
            {
                Rect house = V5Layout.SpeciesHouse(i, count), gift = V5Layout.GiftSign(i, count);
                AssertInside(V5Layout.SafeDesignArea, house); AssertInside(V5Layout.SafeDesignArea, gift);
                foreach (Rect other in hud)
                {
                    Assert.That(ClearGap(house, other), Is.GreaterThanOrEqualTo(39.999f));
                    Assert.That(ClearGap(gift, other), Is.GreaterThanOrEqualTo(39.999f));
                }
                Assert.That(gift.yMin - house.yMax, Is.GreaterThanOrEqualTo(39.999f));
                Assert.That(gift.yMax, Is.LessThan(V5Layout.BoardTop - 40));
                if (i > 0) Assert.That(house.xMin - V5Layout.SpeciesHouse(i - 1, count).xMax, Is.EqualTo(20).Within(0.001f));
            }
        }

        [Test]
        public void TwoRowsAreCenteredAt1160AndKeepTheRightHandOffset()
        {
            float first = V5Layout.RowY(0, 2);
            float second = V5Layout.RowY(1, 2);
            Assert.That((first + second) / 2, Is.EqualTo(1160));
            Assert.That(first, Is.EqualTo(1010));
            Assert.That(second, Is.EqualTo(1310));
            Assert.That(V5Layout.RowY(0, 2, true) - first, Is.EqualTo(40));
            Assert.That(V5Layout.RowY(1, 2, true) - second, Is.EqualTo(40));
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(4)]
        public void RowPitchShrinksToFitWithoutMovingTheGroupCenter(int rows)
        {
            float expectedPitch = Math.Min(300, 800f / rows);
            Assert.That(V5Layout.RowPitch(rows), Is.EqualTo(expectedPitch).Within(0.001f));
            Assert.That((V5Layout.RowY(0, rows) + V5Layout.RowY(rows - 1, rows)) / 2,
                Is.EqualTo(1160).Within(0.001f));
            for (int i = 0; i < rows; i++)
                Assert.That(V5Layout.RowY(i, rows), Is.InRange(760, 1560));
        }

        [TestCase(2)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void DenseBranchTargetsStayDisjointAndSeatsRemainInsideTheirHalf(int rows)
        {
            float scale = V5Layout.BirdScale(rows);
            Assert.That(scale, Is.InRange(0.72f, 1));
            for (int row = 0; row < rows; row++)
                foreach (bool right in new[] { false, true })
                {
                    Rect hit = V5Layout.BranchHit(row, rows, right);
                    AssertInside(V5Layout.SafeDesignArea, hit);
                    if (row > 0) Assert.That(hit.yMin, Is.GreaterThanOrEqualTo(V5Layout.BranchHit(row - 1, rows, right).yMax - 0.001f));
                    if (!right) Assert.That(hit.Overlaps(V5Layout.BranchHit(row, rows, true)), Is.False);
                    for (int seat = 0; seat < 4; seat++)
                    {
                        Vector2 point = V5Layout.BirdSlot(seat, V5Layout.RowY(row, rows, right), right, scale);
                        Assert.That(point.x, Is.GreaterThanOrEqualTo(hit.xMin));
                        Assert.That(point.x, Is.LessThanOrEqualTo(hit.xMax));
                    }
                }
        }

        [Test]
        public void WinControlsHaveSixtyPixelGapsInsideThePanel()
        {
            AssertInside(V5Layout.WinPanel, V5Layout.ReplayButton);
            AssertInside(V5Layout.WinPanel, V5Layout.NextButton);
            AssertInside(V5Layout.WinPanel, V5Layout.HomeButton);
            Assert.That(V5Layout.NextButton.xMin - V5Layout.ReplayButton.xMax, Is.EqualTo(60));
            Assert.That(V5Layout.HomeButton.xMin - V5Layout.NextButton.xMax, Is.EqualTo(60));
        }

        [Test]
        public void WinDecorationAndRewardRowsHaveTheirSpecifiedGaps()
        {
            Assert.That(V5Layout.Star(0).yMin - V5Layout.Ribbon.yMax, Is.EqualTo(40));
            Assert.That(V5Layout.Ribbon.yMin, Is.LessThan(V5Layout.WinPanel.yMin));
            Assert.That(V5Layout.Ribbon.yMax, Is.GreaterThan(V5Layout.WinPanel.yMin));
            Assert.That(V5Layout.Star(1).xMin - V5Layout.Star(0).xMax, Is.EqualTo(30));
            Assert.That(V5Layout.Star(2).xMin - V5Layout.Star(1).xMax, Is.EqualTo(30));
            Assert.That(V5Layout.RewardText.xMin - V5Layout.RewardCoin.xMax, Is.EqualTo(20));
            AssertInside(V5Layout.WinPanel, V5Layout.RewardCoin);
            AssertInside(V5Layout.WinPanel, V5Layout.RewardText);
        }

        [Test]
        public void SettingsControlsStayInsideThePanelAndFortyPixelsApart()
        {
            var buttons = new[] { V5Layout.SoundButton, V5Layout.RestartButton, V5Layout.CloseButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                AssertInside(V5Layout.SettingsPanel, buttons[i]);
                for (int j = i + 1; j < buttons.Length; j++)
                    Assert.That(ClearGap(buttons[i], buttons[j]), Is.GreaterThanOrEqualTo(40));
            }
            Assert.That(V5Layout.SettingsPanel.xMax - V5Layout.CloseButton.xMax, Is.EqualTo(24));
            Assert.That(V5Layout.CloseButton.yMin - V5Layout.SettingsPanel.yMin, Is.EqualTo(24));
        }
    }
}
