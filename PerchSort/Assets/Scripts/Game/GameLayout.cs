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
                case "board_win_panel":
                    switch (element)
                    {
                        case "board": return new Rect(0f, 0f, 1080f, 1920f);
                        case "coin_hud": return new Rect(37f, 50f, 304f, 116f);
                        case "coin_hud_icon": return new Rect(39f, 51f, 126f, 112f);
                        case "coin_hud_value": return new Rect(174f, 78f, 131f, 54f);
                        case "moves_hud": return new Rect(34f, 164f, 405f, 116f);
                        case "moves_hud_feet": return new Rect(50f, 199f, 63f, 47f);
                        case "moves_hud_value": return new Rect(120f, 193f, 44f, 55f);
                        case "moves_hud_divider": return new Rect(179f, 181f, 9f, 79f);
                        case "hud_star_1": return new Rect(208f, 173f, 50f, 49f);
                        case "hud_threshold_1": return new Rect(211f, 227f, 39f, 36f);
                        case "hud_star_2": return new Rect(278f, 173f, 51f, 49f);
                        case "hud_threshold_2": return new Rect(279f, 227f, 41f, 36f);
                        case "hud_star_3": return new Rect(349f, 173f, 51f, 49f);
                        case "hud_threshold_3": return new Rect(349f, 227f, 57f, 36f);
                        case "goal_count": return new Rect(35f, 282f, 202f, 94f);
                        case "goal_sun": return new Rect(49f, 292f, 94f, 73f);
                        case "goal_number": return new Rect(157f, 303f, 46f, 48f);
                        case "settings": return new Rect(952f, 79f, 114f, 104f);
                        case "settings_icon": return new Rect(981f, 100f, 57f, 57f);
                        case "house_rope": return new Rect(550f, 0f, 50f, 182f);
                        case "goal_house": return new Rect(417f, 174f, 323f, 227f);
                        case "house_entry": return new Rect(542f, 269f, 83f, 81f);
                        case "goal_ring_yellow": return new Rect(371f, 402f, 100f, 94f);
                        case "goal_bird_yellow": return new Rect(385f, 419f, 71f, 69f);
                        case "goal_badge_yellow": return new Rect(431f, 459f, 53f, 48f);
                        case "goal_ring_blue": return new Rect(510f, 402f, 98f, 94f);
                        case "goal_bird_blue": return new Rect(523f, 418f, 72f, 70f);
                        case "goal_badge_blue": return new Rect(571f, 459f, 53f, 48f);
                        case "goal_ring_owl": return new Rect(648f, 400f, 98f, 96f);
                        case "goal_bird_owl": return new Rect(661f, 415f, 76f, 73f);
                        case "goal_badge_owl": return new Rect(709f, 459f, 54f, 48f);
                        case "branch_yellow_1": return new Rect(59f, 688f, 128f, 137f);
                        case "branch_blue_1": return new Rect(184f, 696f, 130f, 129f);
                        case "branch_owl_1": return new Rect(312f, 708f, 75f, 89f);
                        case "branch_blue_fragment": return new Rect(844f, 837f, 41f, 23f);
                        case "branch_yellow_2": return new Rect(951f, 831f, 95f, 136f);
                        case "branch_yellow_3": return new Rect(58f, 1049f, 55f, 99f);
                        case "branch_yellow_4": return new Rect(968f, 1158f, 80f, 135f);
                        case "tools_bar": return new Rect(193f, 1727f, 729f, 146f);
                        case "tool_undo": return new Rect(245f, 1750f, 99f, 103f);
                        case "tool_icon_undo": return new Rect(269f, 1773f, 51f, 49f);
                        case "tool_hint": return new Rect(488f, 1749f, 101f, 105f);
                        case "tool_icon_hint": return new Rect(517f, 1770f, 44f, 62f);
                        case "tool_branch": return new Rect(730f, 1749f, 103f, 104f);
                        case "tool_icon_branch": return new Rect(748f, 1769f, 67f, 59f);
                        case "win_panel": return new Rect(110f, 856f, 860f, 514f);
                        case "panel_frame": return new Rect(110f, 856f, 860f, 514f);
                        case "panel_cream": return new Rect(143f, 889f, 795f, 442f);
                        case "hero": return new Rect(369f, 616f, 330f, 173f);
                        case "win_ribbon": return new Rect(220f, 777f, 638f, 137f);
                        case "ribbon_leaves_left": return new Rect(217f, 797f, 111f, 114f);
                        case "ribbon_leaves_right": return new Rect(753f, 795f, 105f, 115f);
                        case "win_title": return new Rect(357f, 796f, 366f, 69f);
                        case "win_star_1": return new Rect(286f, 934f, 107f, 98f);
                        case "threshold_pill_1": return new Rect(257f, 1056f, 166f, 58f);
                        case "threshold_value_1": return new Rect(305f, 1063f, 69f, 42f);
                        case "win_star_2": return new Rect(486f, 934f, 107f, 98f);
                        case "threshold_pill_2": return new Rect(457f, 1056f, 166f, 58f);
                        case "threshold_value_2": return new Rect(505f, 1063f, 69f, 42f);
                        case "win_star_3": return new Rect(686f, 934f, 107f, 98f);
                        case "threshold_pill_3": return new Rect(657f, 1056f, 166f, 58f);
                        case "threshold_value_3": return new Rect(698f, 1063f, 84f, 42f);
                        case "move_pill": return new Rect(405f, 1138f, 270f, 74f);
                        case "move_footprint": return new Rect(470f, 1155f, 62f, 40f);
                        case "move_value": return new Rect(567f, 1157f, 91f, 39f);
                        case "reward_pill": return new Rect(380f, 1236f, 320f, 94f);
                        case "reward_coin": return new Rect(380f, 1237f, 104f, 93f);
                        case "reward_value": return new Rect(522f, 1252f, 126f, 56f);
                        case "action_retry": return new Rect(339f, 1432f, 88f, 93f);
                        case "action_icon_retry": return new Rect(360f, 1453f, 47f, 51f);
                        case "action_play": return new Rect(484f, 1419f, 111f, 120f);
                        case "action_icon_play": return new Rect(517f, 1451f, 54f, 60f);
                        case "action_home": return new Rect(654f, 1432f, 95f, 96f);
                        case "action_icon_home": return new Rect(681f, 1452f, 49f, 52f);
                    }
                    break;
                case "board_win_panel_two_stars":
                    switch (element)
                    {
                        case "board": return new Rect(0f, 0f, 1080f, 1920f);
                        case "board_dim": return new Rect(0f, 0f, 1080f, 1920f);
                        case "coin_hud": return new Rect(91f, 57f, 251f, 103f);
                        case "coin_hud_icon": return new Rect(38f, 48f, 127f, 124f);
                        case "coin_hud_leaf": return new Rect(71f, 77f, 63f, 60f);
                        case "coin_hud_value": return new Rect(176f, 79f, 126f, 56f);
                        case "moves_hud": return new Rect(32f, 165f, 406f, 115f);
                        case "moves_hud_feet": return new Rect(49f, 198f, 64f, 45f);
                        case "moves_hud_value": return new Rect(122f, 195f, 40f, 51f);
                        case "moves_hud_divider": return new Rect(180f, 181f, 9f, 79f);
                        case "hud_star_1": return new Rect(208f, 173f, 50f, 47f);
                        case "hud_threshold_1": return new Rect(209f, 228f, 46f, 33f);
                        case "hud_star_2": return new Rect(282f, 173f, 50f, 47f);
                        case "hud_threshold_2": return new Rect(282f, 229f, 48f, 33f);
                        case "hud_star_3": return new Rect(354f, 173f, 52f, 47f);
                        case "hud_threshold_3": return new Rect(356f, 231f, 48f, 29f);
                        case "sun_hud": return new Rect(33f, 283f, 207f, 94f);
                        case "sun_icon": return new Rect(51f, 288f, 87f, 79f);
                        case "sun_value": return new Rect(157f, 303f, 43f, 49f);
                        case "settings": return new Rect(953f, 78f, 115f, 105f);
                        case "settings_gear": return new Rect(975f, 102f, 71f, 57f);
                        case "goal_rope": return new Rect(556f, 0f, 41f, 177f);
                        case "goal_house": return new Rect(414f, 173f, 324f, 229f);
                        case "goal_roof": return new Rect(414f, 173f, 324f, 143f);
                        case "goal_hole": return new Rect(535f, 269f, 77f, 80f);
                        case "goal_yellow_ring": return new Rect(370f, 401f, 104f, 96f);
                        case "goal_yellow": return new Rect(383f, 415f, 78f, 67f);
                        case "goal_yellow_badge": return new Rect(430f, 456f, 56f, 52f);
                        case "goal_yellow_gift": return new Rect(443f, 467f, 33f, 33f);
                        case "goal_blue_ring": return new Rect(509f, 401f, 105f, 96f);
                        case "goal_blue": return new Rect(522f, 415f, 80f, 68f);
                        case "goal_blue_badge": return new Rect(568f, 456f, 58f, 52f);
                        case "goal_blue_gift": return new Rect(581f, 467f, 35f, 33f);
                        case "goal_owl_ring": return new Rect(649f, 401f, 105f, 96f);
                        case "goal_owl": return new Rect(661f, 415f, 82f, 67f);
                        case "goal_owl_badge": return new Rect(707f, 456f, 58f, 52f);
                        case "goal_owl_gift": return new Rect(720f, 467f, 35f, 33f);
                        case "branch1_yellow": return new Rect(58f, 689f, 127f, 138f);
                        case "branch1_blue": return new Rect(181f, 695f, 134f, 118f);
                        case "branch1_owl": return new Rect(312f, 710f, 86f, 84f);
                        case "branch2_blue_tuft": return new Rect(831f, 834f, 53f, 24f);
                        case "branch2_yellow": return new Rect(967f, 833f, 83f, 134f);
                        case "branch3_yellow": return new Rect(58f, 1051f, 55f, 94f);
                        case "branch4_yellow": return new Rect(967f, 1158f, 83f, 128f);
                        case "tools_bar": return new Rect(195f, 1728f, 731f, 144f);
                        case "tool_undo": return new Rect(246f, 1750f, 110f, 106f);
                        case "tool_undo_icon": return new Rect(269f, 1774f, 64f, 51f);
                        case "tool_hint": return new Rect(484f, 1749f, 112f, 108f);
                        case "tool_hint_icon": return new Rect(514f, 1770f, 49f, 65f);
                        case "tool_branch": return new Rect(730f, 1751f, 110f, 104f);
                        case "tool_branch_icon": return new Rect(749f, 1775f, 72f, 53f);
                        case "panel": return new Rect(110f, 854f, 860f, 516f);
                        case "panel_frame": return new Rect(110f, 854f, 860f, 516f);
                        case "panel_cream": return new Rect(145f, 891f, 790f, 439f);
                        case "hero": return new Rect(367f, 616f, 335f, 174f);
                        case "ribbon": return new Rect(214f, 778f, 655f, 135f);
                        case "ribbon_left_leaves": return new Rect(214f, 794f, 114f, 119f);
                        case "ribbon_right_leaves": return new Rect(748f, 795f, 121f, 117f);
                        case "ribbon_face": return new Rect(305f, 778f, 477f, 113f);
                        case "title": return new Rect(353f, 795f, 373f, 72f);
                        case "star_1": return new Rect(285f, 933f, 109f, 101f);
                        case "star_2": return new Rect(485f, 933f, 109f, 101f);
                        case "star_3": return new Rect(682f, 932f, 114f, 102f);
                        case "threshold_pill_1": return new Rect(256f, 1055f, 168f, 61f);
                        case "threshold_1": return new Rect(306f, 1062f, 68f, 41f);
                        case "threshold_pill_2": return new Rect(456f, 1055f, 168f, 61f);
                        case "threshold_2": return new Rect(507f, 1062f, 68f, 42f);
                        case "threshold_pill_3": return new Rect(656f, 1055f, 168f, 61f);
                        case "threshold_3": return new Rect(703f, 1066f, 78f, 33f);
                        case "result_pill": return new Rect(403f, 1138f, 289f, 75f);
                        case "result_feet": return new Rect(470f, 1155f, 61f, 42f);
                        case "result_value": return new Rect(548f, 1151f, 66f, 47f);
                        case "reward_pill": return new Rect(414f, 1238f, 283f, 92f);
                        case "reward_coin": return new Rect(379f, 1235f, 107f, 96f);
                        case "reward_leaf": return new Rect(401f, 1255f, 60f, 57f);
                        case "reward_value": return new Rect(521f, 1254f, 132f, 50f);
                        case "retry": return new Rect(339f, 1432f, 88f, 94f);
                        case "retry_glyph": return new Rect(360f, 1452f, 47f, 53f);
                        case "play": return new Rect(483f, 1419f, 112f, 120f);
                        case "play_glyph": return new Rect(520f, 1442f, 51f, 71f);
                        case "home": return new Rect(653f, 1432f, 96f, 97f);
                        case "home_glyph": return new Rect(674f, 1450f, 56f, 57f);
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
                        case "board": return new Rect(0f, 0f, 1080f, 1920f);
                        case "coin_pill": return new Rect(36f, 50f, 305f, 121f);
                        case "coin": return new Rect(37f, 50f, 128f, 120f);
                        case "coin_leaf": return new Rect(66f, 76f, 65f, 64f);
                        case "coins": return new Rect(174f, 80f, 126f, 50f);
                        case "moves_pill": return new Rect(31f, 165f, 408f, 115f);
                        case "bird_feet": return new Rect(51f, 198f, 64f, 44f);
                        case "moves": return new Rect(122f, 196f, 39f, 49f);
                        case "hud_divider": return new Rect(183f, 181f, 8f, 83f);
                        case "star1": return new Rect(203f, 172f, 57f, 49f);
                        case "star2": return new Rect(278f, 172f, 57f, 49f);
                        case "star3": return new Rect(350f, 172f, 57f, 49f);
                        case "threshold1": return new Rect(207f, 228f, 48f, 33f);
                        case "threshold2": return new Rect(282f, 228f, 49f, 33f);
                        case "threshold3": return new Rect(354f, 228f, 52f, 33f);
                        case "sun_pill": return new Rect(32f, 280f, 207f, 98f);
                        case "sun": return new Rect(49f, 288f, 93f, 78f);
                        case "sun_count": return new Rect(156f, 304f, 44f, 46f);
                        case "gear": return new Rect(955f, 78f, 112f, 105f);
                        case "gear_icon": return new Rect(979f, 99f, 64f, 61f);
                        case "goal_rope": return new Rect(556f, 0f, 39f, 179f);
                        case "goal_house": return new Rect(414f, 171f, 326f, 228f);
                        case "house_hole": return new Rect(529f, 253f, 88f, 98f);
                        case "goal_yellow_ring": return new Rect(370f, 402f, 100f, 97f);
                        case "goal_yellow": return new Rect(382f, 410f, 75f, 76f);
                        case "gift_yellow": return new Rect(432f, 455f, 54f, 52f);
                        case "goal_blue_ring": return new Rect(509f, 401f, 99f, 98f);
                        case "goal_blue": return new Rect(520f, 412f, 76f, 76f);
                        case "gift_blue": return new Rect(573f, 456f, 51f, 51f);
                        case "goal_owl_ring": return new Rect(649f, 400f, 101f, 101f);
                        case "goal_owl": return new Rect(660f, 407f, 78f, 82f);
                        case "gift_owl": return new Rect(712f, 455f, 53f, 53f);
                        case "upper_yellow": return new Rect(58f, 690f, 91f, 137f);
                        case "middle_yellow": return new Rect(967f, 832f, 85f, 134f);
                        case "lower_left_yellow": return new Rect(59f, 1016f, 126f, 139f);
                        case "lower_left_blue": return new Rect(184f, 1039f, 131f, 117f);
                        case "lower_left_yellow2": return new Rect(310f, 1039f, 133f, 116f);
                        case "lower_right_yellow": return new Rect(667f, 1154f, 132f, 140f);
                        case "lower_right_blue": return new Rect(792f, 1162f, 128f, 133f);
                        case "lower_right_yellow2": return new Rect(919f, 1157f, 130f, 138f);
                        case "tools_bar": return new Rect(195f, 1728f, 730f, 146f);
                        case "tool_undo": return new Rect(247f, 1751f, 107f, 100f);
                        case "tool_undo_icon": return new Rect(269f, 1773f, 63f, 51f);
                        case "tool_hint": return new Rect(488f, 1752f, 106f, 101f);
                        case "tool_hint_icon": return new Rect(507f, 1764f, 69f, 75f);
                        case "tool_branch": return new Rect(728f, 1751f, 106f, 102f);
                        case "tool_branch_icon": return new Rect(741f, 1771f, 77f, 60f);
                        case "panel": return new Rect(110f, 680f, 860f, 360f);
                        case "panel_fill": return new Rect(146f, 722f, 790f, 277f);
                        case "ribbon": return new Rect(335f, 616f, 409f, 156f);
                        case "leaves_left": return new Rect(258f, 632f, 92f, 127f);
                        case "leaves_right": return new Rect(729f, 632f, 88f, 125f);
                        case "title": return new Rect(394f, 632f, 290f, 85f);
                        case "close": return new Rect(898f, 658f, 94f, 97f);
                        case "close_x": return new Rect(926f, 680f, 41f, 47f);
                        case "sound": return new Rect(342f, 790f, 100f, 100f);
                        case "sound_icon": return new Rect(365f, 814f, 58f, 46f);
                        case "sound_plate": return new Rect(311f, 910f, 149f, 68f);
                        case "sound_label": return new Rect(347f, 922f, 89f, 39f);
                        case "restart": return new Rect(490f, 790f, 100f, 100f);
                        case "restart_icon": return new Rect(515f, 813f, 55f, 51f);
                        case "restart_plate": return new Rect(470f, 910f, 141f, 68f);
                        case "restart_label": return new Rect(488f, 923f, 109f, 38f);
                        case "home": return new Rect(638f, 790f, 100f, 100f);
                        case "home_icon": return new Rect(659f, 811f, 58f, 56f);
                        case "home_plate": return new Rect(623f, 911f, 146f, 67f);
                        case "home_label": return new Rect(654f, 922f, 84f, 40f);
                        case "replay": return new Rect(490f, 790f, 100f, 100f);
                    }
                    break;
                case "level_card":
                    switch (element)
                    {
                        case "forest": return new Rect(0f, 0f, 1080f, 1920f);
                        case "forest_dim": return new Rect(0f, 0f, 1080f, 1920f);
                        case "panel_frame": return new Rect(110f, 340f, 860f, 1040f);
                        case "panel": return new Rect(135f, 365f, 810f, 980f);
                        case "forest_ribbon": return new Rect(290f, 76f, 510f, 109f);
                        case "forest_title": return new Rect(370f, 94f, 346f, 56f);
                        case "coin_pill": return new Rect(39f, 87f, 266f, 87f);
                        case "coin": return new Rect(39f, 88f, 112f, 86f);
                        case "coin_leaf": return new Rect(66f, 107f, 55f, 43f);
                        case "coin_count": return new Rect(158f, 110f, 121f, 44f);
                        case "level_ribbon": return new Rect(210f, 282f, 660f, 118f);
                        case "level_leaves_l": return new Rect(236f, 296f, 90f, 59f);
                        case "level_leaves_r": return new Rect(768f, 296f, 84f, 59f);
                        case "level_title": return new Rect(389f, 303f, 317f, 54f);
                        case "close": return new Rect(896f, 314f, 100f, 100f);
                        case "close_x": return new Rect(927f, 344f, 39f, 39f);
                        case "star_1": return new Rect(284f, 434f, 120f, 114f);
                        case "star_2": return new Rect(484f, 434f, 120f, 114f);
                        case "star_3": return new Rect(684f, 434f, 120f, 114f);
                        case "threshold_12": return new Rect(263f, 577f, 154f, 52f);
                        case "threshold_12_text": return new Rect(314f, 585f, 52f, 36f);
                        case "threshold_16": return new Rect(463f, 577f, 154f, 52f);
                        case "threshold_16_text": return new Rect(514f, 585f, 52f, 36f);
                        case "threshold_inf": return new Rect(663f, 577f, 154f, 52f);
                        case "threshold_inf_text": return new Rect(704f, 585f, 72f, 36f);
                        case "best_pill": return new Rect(340f, 656f, 400f, 69f);
                        case "best_tracks": return new Rect(377f, 665f, 72f, 49f);
                        case "best_text": return new Rect(468f, 668f, 238f, 43f);
                        case "yellow_ring": return new Rect(171f, 788f, 177f, 164f);
                        case "yellow_bird": return new Rect(185f, 767f, 148f, 167f);
                        case "yellow_badge": return new Rect(264f, 885f, 79f, 78f);
                        case "yellow_ability": return new Rect(274f, 901f, 59f, 44f);
                        case "blue_ring": return new Rect(356f, 790f, 176f, 163f);
                        case "blue_bird": return new Rect(368f, 774f, 151f, 158f);
                        case "blue_badge": return new Rect(450f, 886f, 80f, 79f);
                        case "blue_ability": return new Rect(462f, 901f, 57f, 45f);
                        case "pink_ring": return new Rect(543f, 790f, 178f, 163f);
                        case "pink_bird": return new Rect(556f, 766f, 152f, 167f);
                        case "pink_badge": return new Rect(641f, 885f, 81f, 80f);
                        case "pink_ability": return new Rect(654f, 900f, 44f, 50f);
                        case "mint_ring": return new Rect(730f, 790f, 177f, 163f);
                        case "mint_bird": return new Rect(743f, 769f, 152f, 166f);
                        case "mint_badge": return new Rect(831f, 886f, 80f, 79f);
                        case "mint_ability": return new Rect(843f, 901f, 57f, 43f);
                        case "owl_ring": return new Rect(247f, 973f, 187f, 182f);
                        case "owl_bird": return new Rect(261f, 984f, 161f, 155f);
                        case "owl_badge": return new Rect(350f, 1087f, 82f, 79f);
                        case "owl_ability": return new Rect(364f, 1110f, 55f, 31f);
                        case "white_ring": return new Rect(448f, 978f, 184f, 179f);
                        case "white_bird": return new Rect(463f, 975f, 155f, 165f);
                        case "white_badge": return new Rect(549f, 1087f, 83f, 80f);
                        case "white_ability": return new Rect(565f, 1104f, 51f, 42f);
                        case "fire_ring": return new Rect(643f, 977f, 187f, 180f);
                        case "fire_bird": return new Rect(657f, 960f, 162f, 180f);
                        case "fire_badge": return new Rect(745f, 1086f, 83f, 81f);
                        case "fire_ability": return new Rect(761f, 1099f, 53f, 52f);
                        case "pink_count": return new Rect(696f, 917f, 18f, 30f);
                        case "lower_card": return new Rect(183f, 1194f, 707f, 148f);
                        case "sleep_owl": return new Rect(242f, 1213f, 163f, 110f);
                        case "sleep_caption": return new Rect(369f, 1203f, 64f, 44f);
                        case "nest_leaves_l": return new Rect(450f, 1251f, 35f, 41f);
                        case "nest_leaves_r": return new Rect(598f, 1251f, 36f, 39f);
                        case "egg": return new Rect(483f, 1208f, 114f, 93f);
                        case "nest": return new Rect(464f, 1277f, 149f, 48f);
                        case "daynight": return new Rect(665f, 1204f, 159f, 130f);
                        case "day_sun": return new Rect(684f, 1236f, 45f, 45f);
                        case "night_moon": return new Rect(768f, 1243f, 32f, 38f);
                        case "daynight_hand": return new Rect(728f, 1210f, 38f, 116f);
                        case "play": return new Rect(477f, 1427f, 126f, 126f);
                        case "play_icon": return new Rect(521f, 1460f, 50f, 58f);
                        case "nav_frame": return new Rect(60f, 1770f, 960f, 150f);
                        case "nav_bar": return new Rect(83f, 1794f, 915f, 104f);
                        case "nav_shop": return new Rect(249f, 1795f, 110f, 105f);
                        case "nav_shop_icon": return new Rect(284f, 1823f, 39f, 44f);
                        case "nav_home": return new Rect(488f, 1795f, 111f, 105f);
                        case "nav_home_icon": return new Rect(517f, 1826f, 42f, 40f);
                        case "nav_settings": return new Rect(722f, 1795f, 112f, 105f);
                        case "nav_settings_icon": return new Rect(754f, 1825f, 43f, 45f);
                        case "leaf_number": return new Rect(210f, 282f, 660f, 118f);
                        case "star_left": return new Rect(284f, 434f, 120f, 114f);
                        case "star_center": return new Rect(484f, 434f, 120f, 114f);
                        case "star_right": return new Rect(684f, 434f, 120f, 114f);
                        case "stat_pill": return new Rect(340f, 656f, 400f, 69f);
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
                        case "board": return new Rect(0f, 0f, 1080f, 1920f);
                        case "coin_pill": return new Rect(36f, 49f, 309f, 117f);
                        case "coin": return new Rect(36f, 49f, 130f, 115f);
                        case "coin_leaf": return new Rect(65f, 72f, 70f, 67f);
                        case "coins": return new Rect(174f, 77f, 128f, 60f);
                        case "moves_pill": return new Rect(32f, 166f, 407f, 114f);
                        case "footprints": return new Rect(51f, 195f, 65f, 48f);
                        case "moves": return new Rect(120f, 193f, 44f, 54f);
                        case "hud_divider": return new Rect(179f, 181f, 8f, 78f);
                        case "star_1": return new Rect(207f, 174f, 54f, 49f);
                        case "threshold_1": return new Rect(208f, 229f, 47f, 33f);
                        case "star_2": return new Rect(283f, 174f, 54f, 49f);
                        case "threshold_2": return new Rect(282f, 229f, 49f, 33f);
                        case "star_3": return new Rect(355f, 174f, 54f, 49f);
                        case "threshold_3": return new Rect(354f, 229f, 53f, 33f);
                        case "sun_pill": return new Rect(34f, 281f, 210f, 96f);
                        case "hud_sun": return new Rect(55f, 290f, 85f, 75f);
                        case "sun_count": return new Rect(156f, 300f, 46f, 50f);
                        case "settings": return new Rect(953f, 77f, 113f, 107f);
                        case "settings_gear": return new Rect(980f, 98f, 59f, 61f);
                        case "house_rope": return new Rect(553f, 0f, 44f, 181f);
                        case "goal_house": return new Rect(414f, 170f, 332f, 231f);
                        case "house_roof": return new Rect(414f, 170f, 332f, 149f);
                        case "house_opening": return new Rect(535f, 276f, 89f, 78f);
                        case "goal_sunny_ring": return new Rect(371f, 400f, 101f, 66f);
                        case "goal_sunny": return new Rect(382f, 416f, 76f, 50f);
                        case "goal_blue_ring": return new Rect(510f, 399f, 101f, 67f);
                        case "goal_blue": return new Rect(525f, 415f, 75f, 51f);
                        case "goal_owl_ring": return new Rect(650f, 400f, 100f, 80f);
                        case "goal_owl": return new Rect(665f, 416f, 73f, 62f);
                        case "goal_owl_badge": return new Rect(720f, 461f, 37f, 20f);
                        case "branch_left_yellow_1": return new Rect(58f, 721f, 54f, 104f);
                        case "branch_right_yellow_1": return new Rect(968f, 832f, 81f, 134f);
                        case "branch_left_yellow_2": return new Rect(58f, 1045f, 54f, 110f);
                        case "branch_bottom_yellow_1": return new Rect(668f, 1220f, 126f, 73f);
                        case "branch_bottom_blue": return new Rect(792f, 1220f, 133f, 73f);
                        case "branch_bottom_yellow_2": return new Rect(919f, 1157f, 131f, 137f);
                        case "tools_bar": return new Rect(195f, 1727f, 730f, 146f);
                        case "undo": return new Rect(246f, 1745f, 108f, 110f);
                        case "undo_icon": return new Rect(266f, 1772f, 70f, 50f);
                        case "hint": return new Rect(486f, 1745f, 109f, 110f);
                        case "hint_icon": return new Rect(511f, 1762f, 65f, 70f);
                        case "branch": return new Rect(727f, 1745f, 108f, 110f);
                        case "branch_icon": return new Rect(746f, 1768f, 75f, 60f);
                        case "modal": return new Rect(109f, 499f, 863f, 724f);
                        case "modal_frame": return new Rect(109f, 499f, 863f, 724f);
                        case "modal_cream": return new Rect(135f, 526f, 807f, 669f);
                        case "ribbon": return new Rect(266f, 462f, 515f, 137f);
                        case "leaves_left": return new Rect(220f, 466f, 117f, 138f);
                        case "leaves_right": return new Rect(735f, 466f, 124f, 137f);
                        case "sunny_title": return new Rect(399f, 477f, 301f, 86f);
                        case "close": return new Rect(895f, 474f, 101f, 103f);
                        case "close_x": return new Rect(920f, 500f, 52f, 48f);
                        case "sunny_ring": return new Rect(316f, 590f, 431f, 431f);
                        case "portrait_well": return new Rect(345f, 625f, 370f, 362f);
                        case "sunny_hero": return new Rect(352f, 630f, 357f, 355f);
                        case "ability_plate": return new Rect(294f, 1024f, 582f, 135f);
                        case "ability_disc": return new Rect(212f, 1007f, 167f, 163f);
                        case "ability_sun": return new Rect(239f, 1038f, 91f, 91f);
                        case "ability_note": return new Rect(312f, 1077f, 48f, 56f);
                        case "ability_text": return new Rect(393f, 1055f, 453f, 65f);
                        case "panel": return new Rect(109f, 499f, 863f, 724f);
                        case "bird_frame": return new Rect(316f, 590f, 431f, 431f);
                        case "name": return new Rect(399f, 477f, 301f, 86f);
                        case "gift_badge": return new Rect(212f, 1007f, 167f, 163f);
                        case "description": return new Rect(294f, 1024f, 582f, 135f);
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
                        case "coin_pill": return new Rect(50f, 20f, 256f, 106f);
                        case "gear": return new Rect(930f, 18f, 116f, 116f);
                        case "gate": return new Rect(484f, 146f, 444f, 364f);
                        case "node_1": return new Rect(278f, 1390f, 216f, 188f);
                        case "node_2": return new Rect(360f, 1180f, 212f, 188f);
                        case "node_3": return new Rect(480f, 1016f, 198f, 166f);
                        case "node_4": return new Rect(320f, 812f, 228f, 202f);
                        case "node_5": return new Rect(486f, 600f, 192f, 142f);
                        case "node_6": return new Rect(632f, 496f, 180f, 126f);
                        case "current_bird": return new Rect(312f, 656f, 192f, 204f);
                        case "shop": return new Rect(214f, 1752f, 152f, 152f);
                        case "aviary": return new Rect(460f, 1752f, 152f, 152f);
                        case "settings": return new Rect(706f, 1752f, 152f, 152f);
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
