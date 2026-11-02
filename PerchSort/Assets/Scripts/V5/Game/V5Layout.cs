using UnityEngine;

namespace PerchSort.V5.Game
{
    // Top-left design coordinates. Values follow GAME_DESIGN_V5 section 3, including its T31 corrections.
    public static class V5Layout
    {
        public const float DesignWidth = 1080;
        public const float DesignHeight = 1920;
        public const float CenterX = DesignWidth / 2;
        public static readonly Vector2 DesignSize = new Vector2(DesignWidth, DesignHeight);
        public static readonly Rect SafeDesignArea = new Rect(0, 0, DesignWidth, DesignHeight);
        public const float MinimumHudGap = 40;
        public const float SecondaryButtonSize = 150;
        public const float PrimaryButtonSize = 200;
        public const string SecondaryButtonSprite = "btn_square_wood";
        public const string PrimaryButtonSprite = "btn_square_green";
        public const float DisabledAlpha = 0.45f;
        public const float DisabledScale = 0.9f;
        public const float ButtonIconRatio = 0.62f;
        public const float ButtonPressScale = 0.92f;
        public const float ButtonPressSeconds = 0.08f;
        public const float ButtonReleaseSeconds = 0.18f;

        public const float LimbHeight = 320;
        public static readonly Rect Limb = new Rect(0, 0, DesignWidth, LimbHeight);
        public const float HookY = 235;
        public static readonly Vector2 HookPoint = new Vector2(CenterX, HookY);
        public const float HouseWidth = 300;
        public const float HouseHeight = 340;
        public const float HouseCenterY = HookY + HouseHeight / 2;
        public const float HouseBottom = HookY + HouseHeight;
        public const float HouseSwayDegrees = 2;
        public const float HouseSwaySeconds = 4;
        public const float HouseDoorFromHook = 195;
        public static readonly Rect House = Centered(new Vector2(CenterX, HouseCenterY), new Vector2(HouseWidth, HouseHeight));

        public const float CoinPillLeft = 48;
        public const float TopHudY = 130;
        public const float CoinPillWidth = 260;
        public const float CoinPillHeight = 90;
        public const float CoinSize = 64;
        public const float CoinPadding = 24;
        public const float CoinNumberGap = 24;
        public const float CoinFontSize = 46;
        public const float CoinMinimumFontSize = 20;
        public static readonly Rect CoinPill = Centered(new Vector2(CoinPillLeft + CoinPillWidth / 2, TopHudY), new Vector2(CoinPillWidth, CoinPillHeight));
        public static readonly Rect CoinIcon = Centered(new Vector2(CoinPillLeft + CoinPadding + CoinSize / 2, TopHudY), Vector2.one * CoinSize);
        public static readonly Rect CoinNumber = Rect.MinMaxRect(CoinIcon.xMax + CoinNumberGap, CoinPill.yMin, CoinPill.xMax - CoinPadding, CoinPill.yMax);
        public const float SettingsRight = DesignWidth - CoinPillLeft;
        public const float SettingsCenterX = SettingsRight - SecondaryButtonSize / 2;
        public static readonly Rect SettingsButton = Centered(new Vector2(SettingsCenterX, TopHudY), Vector2.one * SecondaryButtonSize);

        public const float TrackerWidth = 380;
        public const float TrackerHeight = 96;
        public const float TrackerCenterY = HouseBottom + MinimumHudGap + TrackerHeight / 2;
        public const float LevelBadgeSize = 72;
        public const float LevelBadgeFontSize = 44;
        public const float BadgeSpeciesGap = 28;
        public const float SpeciesIconSize = 64;
        public const float SpeciesIconGap = 40;
        public const float UnhomedAlpha = 0.4f;
        public const float SpeciesOutlineWidth = 2;
        public const float HomeCheckSize = 24;
        public const float HomeCheckStroke = 5;
        public static readonly Rect Tracker = Centered(new Vector2(CenterX, TrackerCenterY), new Vector2(TrackerWidth, TrackerHeight));
        public static float TrackerContentWidth(int speciesCount) => LevelBadgeSize + BadgeSpeciesGap +
            speciesCount * SpeciesIconSize + (speciesCount - 1) * SpeciesIconGap;
        public static Rect LevelBadge(int speciesCount) => Centered(new Vector2(
            Tracker.xMin + (TrackerWidth - TrackerContentWidth(speciesCount)) / 2 + LevelBadgeSize / 2,
            TrackerCenterY), Vector2.one * LevelBadgeSize);
        public static Rect SpeciesIcon(int index, int speciesCount) => Centered(new Vector2(
            LevelBadge(speciesCount).xMax + BadgeSpeciesGap + SpeciesIconSize / 2 + index * (SpeciesIconSize + SpeciesIconGap),
            TrackerCenterY), Vector2.one * SpeciesIconSize);

        public const float BoardTop = 760;
        public const float BoardBottom = 1560;
        public const float BoardHeight = BoardBottom - BoardTop;
        public const float BoardCenterY = (BoardTop + BoardBottom) / 2;
        public const float MaximumRowPitch = 300;
        public const float RightRowOffset = 40;
        public const float BranchWidth = 600;
        public const float BranchHeight = 112.5f;
        public const float RightBranchLeft = DesignWidth - BranchWidth;
        public const float BirdSize = 150;
        public const float BirdSlotPitch = 120;
        public const float FirstBirdSlot = 110;
        public const float BirdCenterAboveBranch = 87;
        public const float BranchHitHeight = 210;
        public const float BranchHitLift = 60;
        public const float BranchHitWidth = DesignWidth / 2;
        public static readonly Vector2 BirdDisplaySize = Vector2.one * BirdSize;
        public static readonly Vector2 BranchDisplaySize = new Vector2(BranchWidth, BranchHeight);
        public static readonly Vector2 BranchHitSize = new Vector2(BranchHitWidth, BranchHitHeight);
        public static readonly Vector2 BushSize = new Vector2(240, 200);
        public static readonly Vector2 BranchHintSize = new Vector2(480, 18);
        public const float BushAboveBranch = 15;
        public const float BranchHintBelowBranch = 17;
        public static float RowPitch(int rowCount) => Mathf.Min(MaximumRowPitch, BoardHeight / rowCount);
        public static float RowY(int row, int rowCount, bool right = false) => BoardCenterY +
            (row - (rowCount - 1) * 0.5f) * RowPitch(rowCount) + (right ? RightRowOffset : 0);
        public static Rect BranchHit(int row, int rowCount, bool right) => Centered(new Vector2(
            right ? DesignWidth - BranchHitWidth / 2 : BranchHitWidth / 2,
            RowY(row, rowCount, right) - BranchHitLift), BranchHitSize);
        public static Vector2 BirdSlot(int slot, float branchY, bool right)
        {
            float x = FirstBirdSlot + BirdSlotPitch * slot;
            return new Vector2(right ? DesignWidth - x : x, branchY - BirdCenterAboveBranch);
        }

        public const float ToolY = 1760;
        public const float ToolFirstX = 300;
        public const float ToolPitch = 240;
        public const float ToolGap = ToolPitch - SecondaryButtonSize;
        public const float ToolBottom = ToolY + SecondaryButtonSize / 2;
        public const int ToolCount = 3;
        public static Rect ToolButton(int index) => Centered(new Vector2(ToolFirstX + index * ToolPitch, ToolY), Vector2.one * SecondaryButtonSize);
        public static readonly Rect StuckBubble = Centered(new Vector2(CenterX, 1595), new Vector2(740, 100));
        public const float StuckFontSize = 36;
        public const float StuckTextPadding = 20;
        public const float StuckPointerSize = 60;

        public const float PanelCenterY = 1010;
        public const float PanelDimAlpha = 0.6f;
        public const float WinPanelWidth = 860;
        public const float WinPanelHeight = 900;
        public const float WinInitialScale = 0.6f;
        public static readonly Rect WinPanel = Centered(new Vector2(CenterX, PanelCenterY), new Vector2(WinPanelWidth, WinPanelHeight));
        public const int StarCount = 3;
        public const float StarSize = 150;
        public const float StarGap = 30;
        public const float StarY = 870;
        public static Rect Star(int index) => Centered(new Vector2(CenterX + (index - 1) * (StarSize + StarGap), StarY), Vector2.one * StarSize);
        public const float RibbonWidth = 900;
        public const float RibbonHeight = 200;
        public const float RibbonStarGap = 40;
        public const float RibbonBottom = StarY - StarSize / 2 - RibbonStarGap;
        public static readonly Rect Ribbon = Centered(new Vector2(CenterX, RibbonBottom - RibbonHeight / 2), new Vector2(RibbonWidth, RibbonHeight));
        public const float RewardY = 1060;
        public const float RewardCoinGap = 20;
        public const float RewardFontSize = 56;
        public const float RewardTextWidth = 160;
        public const float RewardTextHeight = 90;
        public const float RewardRowWidth = CoinSize + RewardCoinGap + RewardTextWidth;
        public static readonly Rect RewardCoin = Centered(new Vector2(CenterX - RewardRowWidth / 2 + CoinSize / 2, RewardY), Vector2.one * CoinSize);
        public static readonly Rect RewardText = new Rect(RewardCoin.xMax + RewardCoinGap, RewardY - RewardTextHeight / 2, RewardTextWidth, RewardTextHeight);
        public const float WinButtonY = 1330;
        public const float WinButtonGap = 60;
        public const float WinButtonPitch = PrimaryButtonSize / 2 + WinButtonGap + SecondaryButtonSize / 2;
        public const float ReplayX = CenterX - WinButtonPitch;
        public const float HomeX = CenterX + WinButtonPitch;
        public static readonly Rect ReplayButton = Centered(new Vector2(ReplayX, WinButtonY), Vector2.one * SecondaryButtonSize);
        public static readonly Rect NextButton = Centered(new Vector2(CenterX, WinButtonY), Vector2.one * PrimaryButtonSize);
        public static readonly Rect HomeButton = Centered(new Vector2(HomeX, WinButtonY), Vector2.one * SecondaryButtonSize);

        public const float SettingsPanelWidth = 700;
        public const float SettingsPanelHeight = 520;
        public const float SettingsActionY = 1040;
        public const float SoundX = 400;
        public const float RestartX = 680;
        public const float CloseInset = 24;
        public static readonly Rect SettingsPanel = Centered(new Vector2(CenterX, PanelCenterY), new Vector2(SettingsPanelWidth, SettingsPanelHeight));
        public static readonly Rect SoundButton = Centered(new Vector2(SoundX, SettingsActionY), Vector2.one * SecondaryButtonSize);
        public static readonly Rect RestartButton = Centered(new Vector2(RestartX, SettingsActionY), Vector2.one * SecondaryButtonSize);
        public static readonly Rect CloseButton = new Rect(SettingsPanel.xMax - CloseInset - SecondaryButtonSize,
            SettingsPanel.yMin + CloseInset, SecondaryButtonSize, SecondaryButtonSize);

        public static Rect Centered(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);
        public static Vector2 InParent(Rect parent, Vector2 designPoint) => designPoint - parent.position;
    }
}
