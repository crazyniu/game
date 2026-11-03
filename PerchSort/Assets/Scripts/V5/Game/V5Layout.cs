using UnityEngine;

namespace PerchSort.V5.Game
{
    // Top-left design coordinates, GAME_DESIGN_V5 sections 3 and 10.
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
        public const float HookY = 245;
        public const float HouseWidth = 300;
        public const float HouseHeight = 340;
        public const float HouseSwayDegrees = 2;
        public const float HouseSwaySeconds = 4;

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

        public const float LevelBadgeFontSize = 44;
        public static readonly Rect LevelSign = new Rect(48, 245, 100, 100);
        public static readonly Rect PhaseDial = new Rect(48, 385, 100, 130);
        // Beside the coin pill: 42px clearance; 212px clear of Settings.
        public static readonly Rect SongPill = new Rect(350, 85, 320, 90);
        public static readonly Vector2 LightStepPoint = new Vector2(985, 1595);
        public static float SpeciesHouseWidth(int count) => Mathf.Min(HouseWidth, (840f - (count - 1) * 20) / count);
        public static Rect SpeciesHouse(int index, int count)
        {
            float width = SpeciesHouseWidth(count);
            float total = count * width + (count - 1) * 20;
            return new Rect(210 + (840 - total) * 0.5f + index * (width + 20), HookY, width, width * HouseHeight / HouseWidth);
        }
        public static Rect GiftSign(int index, int count)
        {
            Rect house = SpeciesHouse(index, count);
            const float size = 72;
            return Centered(new Vector2(house.center.x, house.yMax + 40 + size * 0.5f), Vector2.one * size);
        }
        public static float RopeStart(float x)
        {
            // Start inside the curved timber; its alpha silhouette masks the rope's upper end.
            float offset = x / CenterX - 1;
            return 150 - 90 * offset * offset;
        }


        public const float BoardTop = 760;
        public const float BoardBottom = 1560;
        public const float BoardHeight = BoardBottom - BoardTop;
        public const float BoardCenterY = (BoardTop + BoardBottom) / 2;
        public const float MaximumRowPitch = 380;
        public const float RightRowOffset = 40;
        public const float BranchWidth = 600;
        public const float BranchHeight = 112.5f;
        public const float BirdSize = 150;
        public const float BirdSlotPitch = 120;
        public const float FirstBirdSlot = 110;
        public const float BirdCenterAboveBranch = 87;
        public const float BranchHitHeight = 210;
        public const float BranchHitLift = 60;
        public const float BranchHitWidth = DesignWidth / 2;
        public static readonly Vector2 BirdDisplaySize = Vector2.one * BirdSize;
        public static readonly Vector2 BranchDisplaySize = new Vector2(BranchWidth, BranchHeight);
        public static readonly Vector2 BushSize = new Vector2(240, 200);
        public const float BushAboveBranch = 15;
        public static float RowPitch(int rowCount) => Mathf.Min(MaximumRowPitch, BoardHeight / Mathf.Max(1, rowCount));
        public static float BirdScale(int rowCount) => Mathf.Clamp(RowPitch(rowCount) / 220, 0.72f, rowCount <= 3 ? 1.08f : 1);
        public static float SeatPitch(float scale) => BirdSlotPitch * Mathf.Min(1, scale);
        public static float RowY(int row, int rowCount, bool right = false) => BoardCenterY +
            (row - (rowCount - 1) * 0.5f) * RowPitch(rowCount) + (right ? RightRowOffset : 0);
        public static Rect BranchHit(int row, int rowCount, bool right) => Centered(new Vector2(
            right ? DesignWidth - BranchHitWidth / 2 : BranchHitWidth / 2,
            RowY(row, rowCount, right) - BranchHitLift * BirdScale(rowCount)),
            new Vector2(BranchHitWidth, Mathf.Min(BranchHitHeight * BirdScale(rowCount), RowPitch(rowCount))));
        public static Vector2 BirdSlot(int slot, float branchY, bool right, float scale = 1)
        {
            float x = FirstBirdSlot + SeatPitch(scale) * slot;
            return new Vector2(right ? DesignWidth - x : x, branchY - BirdCenterAboveBranch * scale);
        }

        public const float ToolY = 1760;
        public const float ToolFirstX = 225;
        public const float ToolPitch = 210;
        public const float ToolGap = ToolPitch - SecondaryButtonSize;
        public const float ToolBottom = ToolY + SecondaryButtonSize / 2;
        public const int ToolCount = 4;
        public static Rect ToolButton(int index) => Centered(new Vector2(ToolFirstX + index * ToolPitch, ToolY), Vector2.one * SecondaryButtonSize);
        public static readonly Rect StuckBubble = Centered(new Vector2(CenterX, 1595), new Vector2(740, 100));

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
