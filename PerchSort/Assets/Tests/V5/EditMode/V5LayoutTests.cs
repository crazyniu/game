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
                ("house", V5Layout.House), ("tracker", V5Layout.Tracker),
                ("undo", V5Layout.ToolButton(0)), ("hint", V5Layout.ToolButton(1)), ("add", V5Layout.ToolButton(2))
            };
            for (int i = 0; i < named.Length; i++)
            {
                AssertInside(new Rect(0, 0, 1080, 1920), named[i].Item2);
                for (int j = i + 1; j < named.Length; j++)
                    Assert.That(ClearGap(named[i].Item2, named[j].Item2), Is.GreaterThanOrEqualTo(40),
                        named[i].Item1 + " / " + named[j].Item1);
            }
        }

        [Test]
        public void ToolBarRemainsInsideTheSafeDesignAreaWithNinetyPixelGaps()
        {
            for (int i = 0; i < V5Layout.ToolCount; i++)
            {
                Rect button = V5Layout.ToolButton(i);
                AssertInside(V5Layout.SafeDesignArea, button);
                Assert.That(button.yMax, Is.EqualTo(1835));
                if (i > 0) Assert.That(button.xMin - V5Layout.ToolButton(i - 1).xMax, Is.EqualTo(90));
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

        [Test]
        public void BranchHitRegionsDoNotOverlapWhenTheArtDoes()
        {
            Rect left = V5Layout.BranchHit(0, 2, false);
            Rect right = V5Layout.BranchHit(0, 2, true);
            Assert.That(left.xMax, Is.EqualTo(right.xMin));
            Assert.That(left.Overlaps(right), Is.False);
            Assert.That(V5Layout.BirdSlot(0, 1010, false).x, Is.EqualTo(110));
            Assert.That(V5Layout.BirdSlot(0, 1050, true).x, Is.EqualTo(970));
            Assert.That(V5Layout.BirdSlot(3, 1010, false).x - V5Layout.BirdSlot(0, 1010, false).x, Is.EqualTo(360));
        }

        [Test]
        public void HouseAttachesToTheHookAndTrackerClearsItsBottom()
        {
            Assert.That(V5Layout.House.center.x, Is.EqualTo(V5Layout.HookPoint.x));
            Assert.That(V5Layout.House.yMin, Is.EqualTo(V5Layout.HookPoint.y));
            Assert.That(V5Layout.Tracker.yMin - V5Layout.House.yMax, Is.EqualTo(40));
            Assert.That(V5Layout.Tracker.yMax, Is.LessThan(V5Layout.BoardTop));
        }

        [Test]
        public void TrackerAndCoinContentsFitWithoutTextArtOverlap()
        {
            Rect badge = V5Layout.LevelBadge(2);
            Rect first = V5Layout.SpeciesIcon(0, 2);
            Rect second = V5Layout.SpeciesIcon(1, 2);
            AssertInside(V5Layout.Tracker, badge);
            AssertInside(V5Layout.Tracker, first);
            AssertInside(V5Layout.Tracker, second);
            Assert.That(first.xMin - badge.xMax, Is.EqualTo(28));
            Assert.That(second.xMin - first.xMax, Is.EqualTo(40));
            AssertInside(V5Layout.CoinPill, V5Layout.CoinIcon);
            AssertInside(V5Layout.CoinPill, V5Layout.CoinNumber);
            Assert.That(V5Layout.CoinNumber.xMin - V5Layout.CoinIcon.xMax, Is.GreaterThanOrEqualTo(24));
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
