using UnityEngine;

namespace PerchSort.V5.Game
{
    // Top-left 1080x1920 coordinates from T40 spec_notes. No legacy HUD geometry.
    public static class V7Layout
    {
        public const float DesignWidth = 1080, DesignHeight = 1920, CenterX = 540;
        public static readonly Vector2 DesignSize = new Vector2(DesignWidth, DesignHeight);
        public static readonly Rect SafeDesignArea = new Rect(0, 0, DesignWidth, DesignHeight);
        public const float MinimumHudGap = 40, TopRowGap = 60;
        public const float SecondaryButtonSize = 116, PrimaryButtonSize = 200, IconSize = 64, SmallIconSize = 40;
        public const string SecondaryButtonSprite = "ui_btn_square", PrimaryButtonSprite = "ui_btn_primary_green";
        public const float DisabledAlpha = 0.45f, DisabledScale = 1, ButtonIconRatio = 64f / 116;
        public const float ButtonPressScale = 0.92f, ButtonPressSeconds = 0.08f, ButtonReleaseSeconds = 0.18f;
        public const float CoinFontSize = 38, CoinMinimumFontSize = 20, LevelBadgeFontSize = 38;
        public static readonly Rect CoinPill = new Rect(40, 64, 190, 72);
        public static readonly Rect CoinIcon = Centered(new Vector2(76, 100), Vector2.one * 48);
        public static readonly Rect CoinNumber = Centered(new Vector2(159, 100), new Vector2(124, 64));
        public static readonly Rect SettingsButton = new Rect(960, 60, 80, 80);
        public static readonly Rect LevelSign = Centered(new Vector2(540, 100), Vector2.one * 72);
        public static readonly Vector2 LevelNumberOffset = new Vector2(0, 2);
        // Coordinator msg_c5024efb6a0e: tray and its children +8px for 43px top-row clearance.
        public const float TrayShift = 8;
        public static readonly Rect GoalTray = new Rect(80, 175 + TrayShift, 920, 150);
        public static readonly Rect LeftRope = new Rect(174, 75, 12, 110 + TrayShift);
        public static readonly Rect RightRope = new Rect(894, 75, 12, 110 + TrayShift);
        public static readonly Vector2 GoalChipSize = new Vector2(104, 120);
        public static readonly Vector2 GoalHouseSize = new Vector2(88, 90);
        public static readonly Vector2 GoalHouseCenter = new Vector2(52, 53);
        public static readonly Rect GoalFace = new Rect(32, 51, 40, 37);
        public static readonly Vector2 GoalDoorLocal = new Vector2(52, 70.5f);
        public static readonly Vector2 GoalGiftCenter = new Vector2(82, 22);
        public const float GiftBadgeSize = 44, GoalDotSize = 11, GoalDotPitch = 18;
        public static readonly Vector2 GoalCheckCenter = new Vector2(90, 85);
        public const float GoalCheckSize = 20;
        public static Vector2 GoalDot(int index) => new Vector2(25 + index * GoalDotPitch, 108);
        public static Rect GoalChip(int index, int count)
        {
            float pitch = count <= 3 ? 260 : 720f / (count - 1);
            return Centered(new Vector2(CenterX + (index - (count - 1) * 0.5f) * pitch, 250 + TrayShift), GoalChipSize);
        }
        // Left of the badge, 64px clear of coins, 130px clear of badge, 43px above tray.
        public static readonly Rect PhaseDial = new Rect(294, 60, 80, 80);
        public static readonly Vector2 PhaseIconCenter = new Vector2(25, 40), PhaseCountCenter = new Vector2(62, 40);
        public const float PhaseIconSize = 44, PhaseFontSize = 24;
        public static readonly Vector2 LightStepPoint = new Vector2(1000, 1640);
        public const float FeatherSize = 44;
        public static readonly Rect ChainRibbon = Centered(new Vector2(900, 359 + TrayShift), new Vector2(112, 44));
        public const float ChainFontSize = 25;
        public const float BoardTop = 420, BoardBottom = 1600, BoardHeight = BoardBottom - BoardTop;
        public const float MaximumRowPitch = 470, RightRowOffset = 60, BranchWidth = 510, BranchHeight = 96;
        public const float BirdSize = 136, BirdSlotPitch = 110, FirstBirdSlot = 100, BirdCenterAboveBranch = 61;
        public const float BranchHitHeight = 190, BranchHitLift = 45, BranchHitWidth = 510;
        public static readonly Vector2 BirdDisplaySize = Vector2.one * BirdSize;
        public static readonly Vector2 BranchDisplaySize = new Vector2(BranchWidth, BranchHeight);
        public static readonly Vector2 TwigDisplaySize = new Vector2(300, 72), BushSize = new Vector2(156, 130);
        public const float BushAboveBranch = 0, BranchCenterBelowFeet = 16;
        public static float RowPitch(int rows) => Mathf.Min(MaximumRowPitch, 800f / Mathf.Max(1, rows - 1));
        public static float BirdScale(int rows) => Mathf.Clamp(RowPitch(rows) / 220, 0.72f, rows <= 3 ? 1.08f : 1);
        public static float SeatPitch(float scale) => BirdSlotPitch * Mathf.Min(1, scale);
        public static float RowY(int row, int rows, bool right = false) => (rows <= 2 ? 945 : 1010) +
            (row - (rows - 1) * 0.5f) * RowPitch(rows) + (right ? RightRowOffset : 0);
        public static Rect BranchHit(int row, int rows, bool right) => Centered(new Vector2(
            right ? DesignWidth - BranchHitWidth / 2 : BranchHitWidth / 2, RowY(row, rows, right) - BranchHitLift),
            new Vector2(BranchHitWidth, Mathf.Min(BranchHitHeight * BirdScale(rows), RowPitch(rows))));
        public static Vector2 BirdSlot(int slot, float y, bool right, float scale = 1)
        {
            float x = FirstBirdSlot + SeatPitch(scale) * slot;
            return new Vector2(right ? DesignWidth - x : x, y - BirdCenterAboveBranch * scale);
        }
        public const float ToolY = 1745, ToolFirstX = 225, ToolPitch = 210;
        public const int ToolCount = 4;
        public const float ToolGap = ToolPitch - SecondaryButtonSize, ToolBottom = ToolY + SecondaryButtonSize / 2;
        public static Rect ToolButton(int index) => Centered(new Vector2(ToolFirstX + index * ToolPitch, ToolY), Vector2.one * SecondaryButtonSize);
        public static readonly Vector2 StockCenter = new Vector2(114, 2);
        public const float StockSize = 40, StockFontSize = 26;
        public static readonly Vector2 PhaseCountSize = new Vector2(32, 44);
        public const float PanelDimAlpha = 0.72f, WinInitialScale = 0.6f;
        public const int StarCount = 3;
        public static readonly Rect WinPanel = new Rect(130, 580, 820, 760), Ribbon = new Rect(190, 567, 700, 145);
        public static Rect Star(int index) => Centered(new Vector2(350 + index * 190, index == 1 ? 825 : 850), Vector2.one * (index == 1 ? 180 : 150));
        public static readonly Rect RewardChip = new Rect(425, 1000, 230, 72);
        public static readonly Rect RewardCoin = Centered(new Vector2(468, 1036), Vector2.one * 48);
        public static readonly Rect RewardText = Centered(new Vector2(563, 1036), new Vector2(158, 64));
        public const float RewardFontSize = 42;
        public static readonly Rect ChorusBonus = Centered(new Vector2(540, 1110), new Vector2(200, 40));
        public static readonly Vector2 BonusIconCenter = new Vector2(24, 20), BonusValueCenter = new Vector2(126, 20);
        public static readonly Vector2 BonusValueSize = new Vector2(140, 40);
        public const float BonusIconSize = 40, BonusFontSize = 30;
        public static readonly Rect ReplayButton = Centered(new Vector2(285, 1220), Vector2.one * 100);
        public static readonly Rect NextButton = new Rect(410, 1162, 260, 116);
        public static readonly Rect HomeButton = Centered(new Vector2(795, 1220), Vector2.one * 100);
        public static readonly Rect SettingsPanel = new Rect(260, 730, 560, 460);
        public static readonly Rect SoundButton = Centered(new Vector2(430, 960), Vector2.one * 116);
        public static readonly Rect RestartButton = Centered(new Vector2(650, 960), Vector2.one * 116);
        public static readonly Rect CloseButton = new Rect(740, 730, 80, 80);
        // Required map action absent in the mock; msg_f10a4693396d approves y=1116 for a 40px gap.
        public static readonly Rect ReturnMapButton = Centered(new Vector2(540, 1116), Vector2.one * 116);
        public static readonly Rect UnlockPanel = new Rect(190, 600, 700, 720);
        public static readonly Rect UnlockIcon = Centered(new Vector2(540, 805), Vector2.one * 192);
        public static readonly Rect UnlockTitle = Centered(new Vector2(540, 975), new Vector2(620, 72));
        public static readonly Rect UnlockBody = Centered(new Vector2(540, 1045), new Vector2(620, 64));
        public static readonly Rect UnlockCheck = new Rect(482, 1147, 116, 116);
        public const float UnlockTitleSize = 52, UnlockBodySize = 32;
        public static readonly Rect PurchaseCoin = Centered(new Vector2(470, 1010), Vector2.one * 48);
        public static readonly Rect PurchasePrice = Centered(new Vector2(580, 1010), new Vector2(140, 64));
        public static Rect InformationClose(Rect panel) => new Rect(panel.width - 72, 8, 64, 64);
        public static readonly Rect GoalInfoPanel = new Rect(220, 380, 640, 280);
        public static readonly Rect GoalInfoPortrait = Centered(new Vector2(330, 520), Vector2.one * 160);
        public static readonly Rect GoalInfoName = Centered(new Vector2(570, 450), new Vector2(400, 64));
        public static readonly Rect GoalInfoGift = Centered(new Vector2(460, 532), Vector2.one * 42);
        public static readonly Rect GoalInfoEffect = Centered(new Vector2(646, 546), new Vector2(300, 132));
        public const float GoalInfoNameSize = 42, GoalInfoBodySize = 28;
        public static readonly Rect LevelCard = new Rect(160, 650, 760, 620);
        public static readonly Rect CardBadge = Centered(new Vector2(540, 695), Vector2.one * 72);
        public static readonly Rect CardClose = new Rect(848, 658, 64, 64);
        public static readonly Rect CardMoves = new Rect(370, 823, 340, 64);
        public static readonly Rect CardPlay = new Rect(410, 1140, 260, 100);
        public static readonly Rect CardBestIcon = Centered(new Vector2(405, 855), Vector2.one * 36);
        public static readonly Rect CardBestValue = Centered(new Vector2(470, 855), new Vector2(80, 54));
        public static readonly Rect CardParIcon = Centered(new Vector2(568, 855), Vector2.one * 36);
        public static readonly Rect CardParValue = Centered(new Vector2(638, 855), new Vector2(80, 54));
        public const float CardMovesFontSize = 30;
        public const float CardStarY = 772, CardStarSize = 40, CardStarPitch = 52;
        public const float PortraitY = 962, PortraitSize = 80, PortraitPitch = 100, MechanicY = 1060, MechanicSize = 56, MechanicPitch = 80;
        public static Rect CardStar(int index) => Centered(new Vector2(CenterX + (index - 1) * CardStarPitch, CardStarY), Vector2.one * CardStarSize);
        public static Vector2 CardPortrait(int index, int count) => new Vector2(CenterX + (index - (count - 1) * 0.5f) * PortraitPitch, PortraitY);
        public static Rect CardMechanic(int index, int count) => Centered(new Vector2(CenterX + (index - (count - 1) * 0.5f) * MechanicPitch, MechanicY), Vector2.one * MechanicSize);
        public const float MapNodeSize = 132, MapNumberSize = 44, MapStarSize = 28, MapStarPitch = 34, MapStarOffsetY = 87;
        public const float MapBirdSize = 112, MapBirdOffsetY = -58, MapShadowOffsetY = 78, MapBounceHeight = 12;
        public const float MapBounceSeconds = 1.8f, MapHeight = 13000, MapViewportTop = 180, MapViewportBottom = 1610;
        public static readonly Rect MapViewport = new Rect(0, MapViewportTop, DesignWidth, MapViewportBottom - MapViewportTop);
        public static readonly Vector2 MapShadowSize = new Vector2(128, 28), MapGateSize = new Vector2(250, 225);
        public static readonly Vector2 MapSettingsCenter = new Vector2(750, ToolY);
        public const float MapPathWidth = 110, MapPathOverlap = 5;
        public static readonly Vector2 MapLockOffset = new Vector2(43, 43), MapNumberOffset = new Vector2(0, 4);
        public const float MapLockSize = 32, MapScrollSensitivity = 50, MapFocusRatio = 0.6f;
        public static Rect Centered(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);
        public static Vector2 InParent(Rect parent, Vector2 point) => point - parent.position;
    }
}
