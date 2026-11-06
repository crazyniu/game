using System;
using UnityEngine;

namespace PerchSort.Game
{
    // Approved forest layout: screens, HUD, count adaptation, map and aligned house parts; see docs/CURRENT_GAME.md.
    // All points are top-left 1080x1920 pixels. House parts retain their shared 512x512 canvas.
    public static class GameLayout
    {
        public const float DesignWidth = 1080, DesignHeight = 1920, CenterX = 540;
        public static readonly Vector2 DesignSize = new Vector2(1080, 1920);
        public static readonly Rect SafeDesignArea = new Rect(0, 0, 1080, 1920);
        public const float MinimumHudGap = 40, TopRowGap = 60;
        public const float SecondaryButtonSize = 116, PrimaryButtonSize = 200, IconSize = 76, SmallIconSize = 64;
        public const string SecondaryButtonSprite = "btn_wood", PrimaryButtonSprite = "btn_green";
        public const float DisabledAlpha = 0.45f, DisabledScale = 0.9f;
        public const float WhiteGlyphRatio = 0.46f, ToolGlyphRatio = 0.54f;
        public const float ButtonPressScale = 0.92f, ButtonPressSeconds = 0.08f, ButtonReleaseSeconds = 0.18f;
        public const float CoinFontSize = 44, CoinMinimumFontSize = 24, LevelBadgeFontSize = 46;
        public static readonly Rect CoinPill = new Rect(48, 150, 190, 76);
        public static readonly Rect CoinNumber = new Rect(108.56f, 166.03f, 68.28f, 43.94f);
        public static readonly Rect CoinIcon = new Rect(CoinPill.x, CoinPill.y, CoinPill.height, CoinPill.height);
        public static readonly Rect SettingsButton = Centered(new Vector2(923, 195), Vector2.one * 108), LevelSign = new Rect(476, 56, 128, 86);
        public static readonly Vector2 LevelNumberOffset = Vector2.zero;
        public static readonly Rect PhaseDial = new Rect(48, 338, 150, 60);
        public static readonly Vector2 PhaseIconCenter = new Vector2(32, 30), PhaseCountCenter = new Vector2(104, 30), PhaseCountSize = new Vector2(60, 48);
        public const float PhaseIconSize = 48, PhaseFontSize = 28;
        public static readonly Rect MovePill = new Rect(48, 250, 190, 64);
        public static readonly Rect NestCanvas = new Rect(420, 150, 240, 240);
        public static float GoalBadgeSize(int count) => count > 7 ? 38 : 44;
        public static Vector2 GoalBadgePoint(int index, int count) => new Vector2(540 + (index - (count - 1) * 0.5f) * 88, 436);
        public static readonly Vector2 LightStepPoint = new Vector2(1010, 1700);
        public const float FeatherSize = 44;
        public static readonly Rect ChainRibbon = new Rect(830, 490, 180, 60), GoalTray = new Rect(0, 210, 1080, 310);
        public const float ChainFontSize = 32;
        public const float BoardTop = 570, BoardBottom = 1700, BoardHeight = BoardBottom - BoardTop;
        public const float BranchWidth = 530, BranchHeight = 90, MaximumRowPitch = 300;
        public const float BirdSize = 170, BirdSlotPitch = 100, FirstBirdSlot = 150, BirdCenterAboveBranch = 76.5f;
        public const float BranchHitWidth = 520, BranchHitHeight = 180, BranchHitLift = 85;
        public static readonly Vector2 BirdDisplaySize = Vector2.one * BirdSize, BranchDisplaySize = new Vector2(BranchWidth, BranchHeight), TwigDisplaySize = new Vector2(400, 80), BushSize = new Vector2(330, 250);
        public const float BushAboveBranch = 300, BranchCenterBelowFeet = 14;
        public static float RowPitch(int rows) => Mathf.Min(300, 800f / Mathf.Max(1, rows));
        public static float BirdScale(int rows) => rows <= 3 ? 1 : rows == 4 ? 0.88f : rows == 5 ? 0.76f : 0.66f;
        public static float SeatPitch(float scale) => BirdSlotPitch;
        public static float RowY(int row, int rows, bool right = false)
        {
            if (rows == 3)
            {
                if (right) return row == 0 ? 893 : row == 1 ? 1160 : 1427;
                return row == 0 ? 760 : row == 1 ? 1027 : 1293;
            }
            return (rows <= 2 ? 925 : 760) + (row + (right ? 0.5f : 0)) * RowPitch(rows);
        }
        public static Rect BranchHit(int row, int rows, bool right) => Centered(new Vector2(right ? DesignWidth - BranchHitWidth / 2 : BranchHitWidth / 2, RowY(row, rows, right) - BranchHitLift), new Vector2(BranchHitWidth, Mathf.Min(BranchHitHeight * BirdScale(rows), RowPitch(rows) * 0.75f)));
        public static Vector2 BirdSlot(int slot, float y, bool right, float scale = 1)
        {
            float x = FirstBirdSlot + SeatPitch(scale) * slot;
            return new Vector2(right ? DesignWidth - x : x, y - BirdCenterAboveBranch * scale);
        }
        public static float BranchPerchTop(int world, bool twig) => twig ?
            (world == 1 ? 18 : world == 2 ? 19 : world == 3 ? 8 : 22) :
            (world == 1 ? 17 : world == 2 ? 9 : world == 3 ? 22 : 30);
        // Put the junction bark on the trunk and its entire horizontal stub behind the native limb.
        public static Vector2 BranchJunction(int world) => new Vector2(120,
            world == 1 ? 40 : world == 2 ? 32 : world == 3 ? 32 : 14);
        private static Vector2 CrownAttachment(int world) => world == 1 ? new Vector2(138, 319) :
            world == 2 ? new Vector2(96, 332) : world == 3 ? new Vector2(111, 358) : new Vector2(105, 355);
        public static Rect WorldCrown(int world, bool right)
        {
            // The snowy board keeps its crowns at the outer edge, above the dense perch band.
            if (world == 4) return new Rect(right ? 990 : -250, 430, 340, 340f * 2 / 3);
            float width = right ? 540 : 500;
            float left = 25 - CrownAttachment(world).x * width / 600;
            return new Rect(right ? DesignWidth - left - width : left, 470, width, width * 2 / 3);
        }
        public const float TrunkVisibleWidth = 170;
        public static Rect WorldTrunk(int world, bool right)
        {
            Rect crown = WorldCrown(world, false);
            float top = crown.y + CrownAttachment(world).y * crown.width / 600 - 35;
            // The source crown attachment is x=73; normalize the lower bark without moving it.
            float barkWidth = world == 1 ? 178 : world == 2 ? 164 : world == 3 ? 186 : 161;
            float scale = (TrunkVisibleWidth - 25) / (barkWidth - 73);
            float width = 380 * scale, left = 25 - 73 * scale;
            return new Rect(right ? DesignWidth - left - width : left, top, width, DesignHeight - top + 80);
        }
        public const float ToolY = 1802, ToolFirstX = 260, ToolPitch = 182;
        public const int ToolCount = 4;
        public const float ToolGap = ToolPitch - SecondaryButtonSize, ToolBottom = ToolY + SecondaryButtonSize / 2;
        public static Rect ToolButton(int index, int count = 3)
        {
            float x = count == 3 ? 344 + index * 188 : index == 0 ? 260 : index == 1 ? 440 : index == 2 ? 622 : 808;
            return Centered(new Vector2(x, count == 3 ? 1802 : 1792), Vector2.one * 108);
        }
        public static readonly Vector2 StockCenter = new Vector2(87, 5);
        public const float StockSize = 64, StockFontSize = 28;
        public const float PanelDimAlpha = 0.55f, WinInitialScale = 0.7f;
        public const int StarCount = 3;
        public static readonly Rect WinPanel = Screen("win", "panel"), Ribbon = Screen("win", "ribbon");
        public static Rect Star(int index) => Screen("win", index == 0 ? "star_left" : index == 1 ? "star_center" : "star_right");
        public static readonly Rect RewardChip = Screen("win", "coin_reward"), RewardCoin = Centered(new Vector2(422, 1107), Vector2.one * 72), RewardText = new Rect(466, 1060, 210, 86);
        public const float RewardFontSize = 46;
        public static readonly Rect ChorusBonus = Screen("win", "chain_reward");
        public static readonly Vector2 BonusIconCenter = new Vector2(28, 32), BonusValueCenter = new Vector2(122, 32), BonusValueSize = new Vector2(120, 60);
        public const float BonusIconSize = 48, BonusFontSize = 34;
        public static readonly Rect ReplayButton = Screen("win", "replay"), NextButton = Screen("win", "next"), HomeButton = Screen("win", "home");
        public static readonly Rect SettingsPanel = Screen("settings", "panel"), SoundButton = Screen("settings", "sound"), RestartButton = Screen("settings", "replay"), CloseButton = Screen("settings", "close"), ReturnMapButton = Screen("settings", "home");
        public static readonly Rect UnlockPanel = Screen("unlock_card", "panel"), UnlockIcon = Screen("unlock_card", "eye_icon"), UnlockTitle = Screen("unlock_card", "title"), UnlockBody = Screen("unlock_card", "description"), UnlockCheck = Screen("unlock_card", "confirm");
        public static readonly Rect UnlockArtwork = new Rect(UnlockIcon.center.x - 180, UnlockIcon.y, 360, Screen("unlock_card", "egg").yMax - UnlockIcon.y);
        public const float UnlockTitleSize = 54, UnlockBodySize = 30;
        public static readonly Rect PurchaseCoin = Centered(new Vector2(445, 1180), Vector2.one * 64), PurchasePrice = Centered(new Vector2(590, 1180), new Vector2(180, 64));
        public static Rect InformationClose(Rect panel) => new Rect(panel.width - 90, 8, 82, 82);
        public static readonly Rect GoalInfoPanel = Screen("goal_info", "panel"), GoalInfoPortrait = Screen("goal_info", "bird_frame"), GoalInfoName = Screen("goal_info", "name"), GoalInfoGift = Screen("goal_info", "gift_badge"), GoalInfoEffect = Screen("goal_info", "description");
        public const float GoalInfoNameSize = 38, GoalInfoBodySize = 24;
        public static readonly Rect LevelCard = Screen("level_card", "panel"), CardBadge = Screen("level_card", "leaf_number"), CardClose = Screen("level_card", "close"), CardMoves = Screen("level_card", "stat_pill"), CardPlay = Screen("level_card", "play");
        public static readonly Rect CardBestIcon = Centered(new Vector2(400, 772), Vector2.one * 40), CardBestValue = Centered(new Vector2(460, 772), new Vector2(70, 54)), CardParIcon = Centered(new Vector2(565, 772), Vector2.one * 40), CardParValue = Centered(new Vector2(645, 772), new Vector2(70, 54));
        public const float CardMovesFontSize = 32, CardStarY = 672, CardStarSize = 110, CardStarPitch = 112;
        public const float PortraitY = 902, PortraitSize = 116, PortraitPitch = 132, MechanicY = 1050, MechanicSize = 100, MechanicPitch = 150;
        public static Rect CardStar(int index) => Screen("level_card", index == 0 ? "star_left" : index == 1 ? "star_center" : "star_right");
        public static Vector2 CardPortrait(int index, int count) => new Vector2(540 + (index - (count - 1) * 0.5f) * Mathf.Min(PortraitPitch, 470f / Mathf.Max(1, count - 1)), PortraitY);
        public static Rect CardMechanic(int index, int count) => Centered(new Vector2(540 + (index - (count - 1) * 0.5f) * MechanicPitch, MechanicY), Vector2.one * MechanicSize);
        public const float MapNodeSize = 180, MapNumberSize = 54, MapStarSize = 36, MapStarPitch = 40, MapStarOffsetY = 44;
        public const float MapBirdSize = 280, MapBirdOffsetY = -155, MapShadowOffsetY = 70, MapBounceHeight = 12;
        public const float MapBounceSeconds = 1.8f, MapSegmentHeight = 3000, MapOverlap = 160;
        public const float MapStride = MapSegmentHeight - MapOverlap, MapHeight = MapStride * 3 + MapSegmentHeight, MapFocusRatio = 0.4f;
        public static readonly Rect MapViewport = SafeDesignArea;
        public static readonly Vector2 MapShadowSize = new Vector2(170, 38), MapGateSize = Screen("map", "gate").size, MapSettingsCenter = Screen("map", "settings").center;
        public static readonly Vector2 MapLockOffset = new Vector2(0, 40), MapNumberOffset = new Vector2(0, -10);
        public const float MapLockSize = 48, MapScrollSensitivity = 70;
        public static float WorldTop(int world) => (4 - world) * MapStride;
        public static Vector2 HouseDoorLocal(Vector2 size) => new Vector2(size.x * 253 / 512, size.y * 306 / 512);
        public static Vector2 HouseHingeLocal(Vector2 size) => new Vector2(size.x * 145 / 512, size.y * 306 / 512);
        public static float HouseDoorDiameter(float size) => size * 204 / 512;
        public static Rect HouseCanvas(int index, int count) => NestCanvas;
        public static Rect Centered(Vector2 center, Vector2 size) => new Rect(center - size * 0.5f, size);
        public static Vector2 InParent(Rect parent, Vector2 point) => point - parent.position;

        public static Rect Screen(string screen, string element)
        {
            switch (screen)
            {
                case "level_k3":
                    switch (element)
                    {
                        case "coin_pill": return new Rect(56f, 148f, 228f, 90f);
                        case "coin_number_text": return new Rect(142f, 164f, 118f, 52f);
                        case "gear": return new Rect(866f, 140f, 114f, 110f);
                        case "house": return new Rect(400f, 152f, 246f, 232f);
                        case "house_rope": return new Rect(506f, 0f, 30f, 168f);
                        case "house_door": return new Rect(474f, 242f, 118f, 130f);
                        case "board_band": return new Rect(0f, 758f, 1080f, 720f);
                        case "branch_1": return new Rect(0f, 902f, 526f, 66f);
                        case "branch_2": return new Rect(482f, 1064f, 598f, 72f);
                        case "branch_3": return new Rect(0f, 1248f, 524f, 74f);
                        case "branch_4": return new Rect(474f, 1406f, 606f, 76f);
                        case "bird_nominal": return new Rect(144f, 776f, 122f, 152f);
                        case "undo": return new Rect(290f, 1748f, 108f, 108f);
                        case "hint": return new Rect(478f, 1748f, 108f, 108f);
                        case "branch_plus": return new Rect(666f, 1748f, 108f, 108f);
                        case "undo_count": return new Rect(356f, 1730f, 46f, 46f);
                        case "hint_count": return new Rect(542f, 1730f, 46f, 46f);
                        case "branch_plus_count": return new Rect(730f, 1730f, 46f, 46f);
                    }
                    break;
                case "level_k7":
                    switch (element)
                    {
                        case "coin_pill": return new Rect(54f, 108f, 240f, 96f);
                        case "coin_number_text": return new Rect(156f, 128f, 114f, 56f);
                        case "gear": return new Rect(894f, 98f, 114f, 114f);
                        case "house_beam": return new Rect(0f, 218f, 1080f, 74f);
                        case "house_1": return new Rect(42f, 296f, 150f, 176f);
                        case "house_2": return new Rect(184f, 296f, 150f, 176f);
                        case "house_3": return new Rect(324f, 296f, 150f, 176f);
                        case "house_4": return new Rect(464f, 296f, 150f, 176f);
                        case "house_5": return new Rect(602f, 296f, 150f, 176f);
                        case "house_6": return new Rect(742f, 296f, 150f, 176f);
                        case "house_7": return new Rect(882f, 296f, 150f, 176f);
                        case "board_band": return new Rect(0f, 572f, 1080f, 1112f);
                        case "branch_1": return new Rect(0f, 748f, 640f, 106f);
                        case "branch_2": return new Rect(430f, 960f, 650f, 82f);
                        case "branch_3": return new Rect(0f, 1176f, 614f, 100f);
                        case "branch_4": return new Rect(266f, 1376f, 814f, 106f);
                        case "branch_5": return new Rect(0f, 1590f, 644f, 114f);
                        case "scarecrow_perch": return new Rect(734f, 680f, 346f, 92f);
                        case "bird_nominal": return new Rect(210f, 598f, 136f, 168f);
                        case "undo": return new Rect(196f, 1726f, 128f, 132f);
                        case "hint": return new Rect(376f, 1726f, 128f, 132f);
                        case "branch_plus": return new Rect(558f, 1726f, 128f, 132f);
                        case "scarecrow": return new Rect(742f, 1726f, 132f, 132f);
                        case "undo_count": return new Rect(276f, 1710f, 50f, 50f);
                        case "hint_count": return new Rect(456f, 1710f, 50f, 50f);
                        case "branch_plus_count": return new Rect(638f, 1710f, 50f, 50f);
                        case "scarecrow_disabled": return new Rect(810f, 1784f, 58f, 58f);
                    }
                    break;
                case "win":
                    switch (element)
                    {
                        case "panel": return new Rect(178f, 742f, 716f, 772f);
                        case "ribbon": return new Rect(192f, 692f, 674f, 200f);
                        case "hero_bird": return new Rect(400f, 506f, 240f, 216f);
                        case "left_bird": return new Rect(86f, 938f, 248f, 308f);
                        case "right_bird": return new Rect(754f, 892f, 264f, 314f);
                        case "star_left": return new Rect(294f, 862f, 168f, 174f);
                        case "star_center": return new Rect(446f, 822f, 192f, 190f);
                        case "star_right": return new Rect(624f, 862f, 168f, 174f);
                        case "coin_reward": return new Rect(366f, 1056f, 332f, 102f);
                        case "chain_reward": return new Rect(448f, 1174f, 186f, 64f);
                        case "replay": return new Rect(272f, 1278f, 140f, 140f);
                        case "next": return new Rect(426f, 1254f, 208f, 206f);
                        case "home": return new Rect(646f, 1278f, 140f, 140f);
                    }
                    break;
                case "settings":
                    switch (element)
                    {
                        case "panel": return new Rect(170f, 864f, 736f, 474f);
                        case "ribbon": return new Rect(240f, 832f, 570f, 166f);
                        case "close": return new Rect(800f, 832f, 120f, 124f);
                        case "sound": return new Rect(244f, 1040f, 180f, 182f);
                        case "replay": return new Rect(448f, 1040f, 180f, 182f);
                        case "home": return new Rect(654f, 1040f, 180f, 182f);
                    }
                    break;
                case "level_card":
                    switch (element)
                    {
                        case "panel": return new Rect(230f, 560f, 612f, 698f);
                        case "leaf_number": return new Rect(424f, 482f, 214f, 146f);
                        case "close": return new Rect(740f, 542f, 92f, 94f);
                        case "star_left": return new Rect(368f, 618f, 110f, 108f);
                        case "star_center": return new Rect(480f, 618f, 110f, 108f);
                        case "star_right": return new Rect(592f, 618f, 110f, 108f);
                        case "stat_pill": return new Rect(358f, 738f, 356f, 68f);
                        case "bird_1": return new Rect(278f, 822f, 136f, 160f);
                        case "bird_2": return new Rect(410f, 822f, 136f, 160f);
                        case "bird_3": return new Rect(544f, 822f, 136f, 160f);
                        case "bird_4": return new Rect(674f, 822f, 136f, 160f);
                        case "sleeping_bird": return new Rect(372f, 984f, 142f, 126f);
                        case "hidden_egg": return new Rect(550f, 998f, 150f, 118f);
                        case "play": return new Rect(450f, 1100f, 174f, 180f);
                    }
                    break;
                case "unlock_card":
                    switch (element)
                    {
                        case "panel": return new Rect(212f, 570f, 650f, 858f);
                        case "eye_icon": return new Rect(444f, 652f, 188f, 128f);
                        case "egg": return new Rect(426f, 796f, 238f, 276f);
                        case "title": return new Rect(350f, 1080f, 376f, 88f);
                        case "description": return new Rect(302f, 1182f, 474f, 48f);
                        case "confirm": return new Rect(438f, 1252f, 192f, 194f);
                    }
                    break;
                case "goal_info":
                    switch (element)
                    {
                        case "panel": return new Rect(240f, 362f, 592f, 282f);
                        case "pointer": return new Rect(502f, 332f, 74f, 44f);
                        case "bird_frame": return new Rect(284f, 404f, 184f, 198f);
                        case "name": return new Rect(474f, 426f, 166f, 68f);
                        case "gift_badge": return new Rect(652f, 396f, 122f, 124f);
                        case "description": return new Rect(468f, 530f, 334f, 46f);
                    }
                    break;
                case "popup_new_friend":
                    switch (element)
                    {
                        case "hero_bird": return new Rect(296f, 522f, 474f, 580f);
                        case "ribbon": return new Rect(208f, 1062f, 640f, 232f);
                        case "name": return new Rect(362f, 1118f, 328f, 88f);
                        case "gift_badge": return new Rect(430f, 1244f, 198f, 182f);
                        case "confirm": return new Rect(422f, 1446f, 216f, 214f);
                    }
                    break;
                case "popup_daily_gift":
                    switch (element)
                    {
                        case "gift_basket": return new Rect(244f, 684f, 578f, 522f);
                        case "coin_reward": return new Rect(378f, 1192f, 302f, 112f);
                        case "confirm": return new Rect(368f, 1330f, 306f, 156f);
                    }
                    break;
                case "popup_world_complete":
                    switch (element)
                    {
                        case "gate": return new Rect(102f, 630f, 856f, 728f);
                        case "ribbon": return new Rect(174f, 448f, 730f, 242f);
                        case "star_left": return new Rect(338f, 474f, 128f, 124f);
                        case "star_center": return new Rect(478f, 456f, 130f, 124f);
                        case "star_right": return new Rect(618f, 474f, 130f, 124f);
                        case "hero_bird_yellow": return new Rect(414f, 690f, 230f, 274f);
                        case "hero_bird_blue": return new Rect(268f, 896f, 230f, 266f);
                        case "hero_bird_pink": return new Rect(526f, 944f, 264f, 298f);
                        case "coin_reward": return new Rect(374f, 1322f, 332f, 118f);
                        case "next": return new Rect(414f, 1436f, 256f, 258f);
                    }
                    break;
                case "popup_world_unlock":
                    switch (element)
                    {
                        case "gate": return new Rect(76f, 684f, 920f, 904f);
                        case "world_plaque": return new Rect(374f, 760f, 338f, 184f);
                        case "play": return new Rect(410f, 1418f, 250f, 246f);
                    }
                    break;
                case "map":
                    switch (element)
                    {
                        case "forest": return new Rect(0f, 0f, 1080f, 1920f);
                        case "gate": return new Rect(455f, 203f, 490f, 365f);
                        case "gate_lock": return new Rect(670f, 422f, 64f, 76f);
                        case "coin_pill": return new Rect(91f, 89f, 216f, 90f);
                        case "coin_count": return new Rect(160f, 108f, 119f, 47f);
                        case "coin": return new Rect(38f, 78f, 112f, 108f);
                        case "coin_leaf": return new Rect(69f, 103f, 52f, 54f);
                        case "world_ribbon": return new Rect(315f, 72f, 464f, 131f);
                        case "ribbon_leaf_l": return new Rect(300f, 109f, 75f, 122f);
                        case "ribbon_leaf_r": return new Rect(729f, 119f, 80f, 75f);
                        case "world_title": return new Rect(378f, 96f, 345f, 68f);
                        case "level_10": return new Rect(633f, 580f, 221f, 152f);
                        case "num_10": return new Rect(697f, 590f, 99f, 62f);
                        case "level_9": return new Rect(632f, 753f, 251f, 176f);
                        case "num_9": return new Rect(722f, 767f, 76f, 70f);
                        case "level_8": return new Rect(249f, 918f, 326f, 224f);
                        case "num_8": return new Rect(366f, 951f, 105f, 110f);
                        case "level_7": return new Rect(482f, 1127f, 308f, 228f);
                        case "num_7": return new Rect(598f, 1151f, 83f, 87f);
                        case "level_6": return new Rect(249f, 1320f, 309f, 232f);
                        case "num_6": return new Rect(365f, 1349f, 87f, 87f);
                        case "lock_10": return new Rect(711f, 649f, 70f, 73f);
                        case "lock_9": return new Rect(722f, 838f, 74f, 77f);
                        case "yellow_bird": return new Rect(265f, 618f, 295f, 331f);
                        case "sparkle_l": return new Rect(210f, 899f, 48f, 50f);
                        case "sparkle_r": return new Rect(576f, 895f, 47f, 53f);
                        case "star_7_1": return new Rect(536f, 1226f, 69f, 63f);
                        case "star_7_2": return new Rect(604f, 1238f, 70f, 63f);
                        case "star_7_3": return new Rect(671f, 1225f, 70f, 66f);
                        case "star_6_1": return new Rect(302f, 1423f, 74f, 66f);
                        case "star_6_2": return new Rect(373f, 1436f, 71f, 66f);
                        case "star_6_3": return new Rect(441f, 1422f, 73f, 66f);
                        case "nav_bar": return new Rect(51f, 1733f, 978f, 171f);
                        case "nav_shop": return new Rect(246f, 1763f, 108f, 108f);
                        case "nav_aviary": return new Rect(486f, 1763f, 108f, 108f);
                        case "nav_settings": return new Rect(726f, 1763f, 108f, 108f);
                        case "icon_shop": return new Rect(272f, 1782f, 56f, 62f);
                        case "icon_aviary": return new Rect(507f, 1781f, 68f, 63f);
                        case "icon_settings": return new Rect(748f, 1783f, 64f, 63f);
                        case "aviary_bird": return new Rect(530f, 1813f, 28f, 24f);
                        case "settings": return new Rect(726f, 1763f, 108f, 108f);
                    }
                    break;
            }
            throw new ArgumentException("Unmeasured layout element: " + screen + "/" + element);
        }
        public static Rect House(int index, int count) => NestCanvas;
        private static readonly Vector2[,] MapNodes = new Vector2[4, 10]
        {
            { new Vector2(514f, 2700f), new Vector2(310f, 2440f), new Vector2(744f, 2180f), new Vector2(660f, 1920f), new Vector2(252f, 1660f), new Vector2(672f, 1400f), new Vector2(640f, 1140f), new Vector2(305f, 880f), new Vector2(754f, 620f), new Vector2(613f, 360f) },
            { new Vector2(512f, 2700f), new Vector2(304f, 2440f), new Vector2(729f, 2180f), new Vector2(656f, 1920f), new Vector2(251f, 1660f), new Vector2(667f, 1400f), new Vector2(634f, 1140f), new Vector2(302f, 880f), new Vector2(752f, 620f), new Vector2(614f, 360f) },
            { new Vector2(508f, 2700f), new Vector2(306f, 2440f), new Vector2(752f, 2180f), new Vector2(640f, 1920f), new Vector2(240f, 1660f), new Vector2(699f, 1400f), new Vector2(620f, 1140f), new Vector2(298f, 880f), new Vector2(750f, 620f), new Vector2(598f, 360f) },
            { new Vector2(512f, 2700f), new Vector2(312f, 2440f), new Vector2(738f, 2180f), new Vector2(650f, 1920f), new Vector2(250f, 1660f), new Vector2(668f, 1400f), new Vector2(618f, 1140f), new Vector2(299f, 880f), new Vector2(750f, 620f), new Vector2(604f, 360f) },
        };
        public static Vector2 MapNode(int world, int localIndex) => MapNodes[world - 1, localIndex] + Vector2.up * WorldTop(world);
    }
}
