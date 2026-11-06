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
                case "shop_boosters":
                    switch (element)
                    {
                        case "background": return new Rect(0f, 0f, 1080f, 1920f);
                        case "balance": return new Rect(105f, 76f, 275f, 96f);
                        case "balance_coin": return new Rect(49f, 64f, 143f, 115f);
                        case "balance_leaf": return new Rect(86f, 95f, 71f, 53f);
                        case "balance_value": return new Rect(220f, 96f, 122f, 48f);
                        case "close": return new Rect(925f, 76f, 108f, 105f);
                        case "close_x": return new Rect(948f, 98f, 57f, 56f);
                        case "tab_glow": return new Rect(289f, 196f, 141f, 142f);
                        case "tab_boosters": return new Rect(306f, 216f, 108f, 107f);
                        case "tab_boosters_glyph": return new Rect(331f, 235f, 58f, 64f);
                        case "tab_outfits": return new Rect(486f, 216f, 108f, 107f);
                        case "tab_outfits_glyph": return new Rect(508f, 244f, 61f, 51f);
                        case "tab_home": return new Rect(666f, 216f, 108f, 107f);
                        case "tab_home_glyph": return new Rect(689f, 239f, 60f, 59f);
                        case "tab_selected": return new Rect(328f, 316f, 65f, 30f);
                        case "hint_card": return new Rect(60f, 430f, 459f, 420f);
                        case "hint_frame": return new Rect(60f, 430f, 459f, 420f);
                        case "hint_cream": return new Rect(88f, 458f, 399f, 365f);
                        case "hint_ribbon": return new Rect(113f, 454f, 355f, 92f);
                        case "hint_leaf_l": return new Rect(104f, 487f, 88f, 64f);
                        case "hint_leaf_r": return new Rect(382f, 492f, 86f, 57f);
                        case "hint_title": return new Rect(234f, 468f, 112f, 44f);
                        case "hint_ring": return new Rect(155f, 529f, 260f, 227f);
                        case "hint_backing": return new Rect(177f, 546f, 216f, 194f);
                        case "hint_price": return new Rect(180f, 750f, 241f, 75f);
                        case "hint_coin": return new Rect(145f, 740f, 105f, 92f);
                        case "hint_coin_leaf": return new Rect(175f, 765f, 46f, 40f);
                        case "hint_price_value": return new Rect(290f, 765f, 72f, 45f);
                        case "hint_owned": return new Rect(427f, 429f, 82f, 76f);
                        case "hint_owned_value": return new Rect(449f, 442f, 38f, 46f);
                        case "branch_card": return new Rect(560f, 430f, 459f, 420f);
                        case "branch_frame": return new Rect(560f, 430f, 459f, 420f);
                        case "branch_cream": return new Rect(588f, 458f, 399f, 365f);
                        case "branch_ribbon": return new Rect(613f, 454f, 355f, 92f);
                        case "branch_leaf_l": return new Rect(604f, 487f, 88f, 64f);
                        case "branch_leaf_r": return new Rect(882f, 492f, 86f, 57f);
                        case "branch_title": return new Rect(625f, 468f, 307f, 46f);
                        case "branch_ring": return new Rect(655f, 529f, 260f, 227f);
                        case "branch_backing": return new Rect(677f, 546f, 216f, 194f);
                        case "branch_price": return new Rect(680f, 750f, 241f, 75f);
                        case "branch_coin": return new Rect(645f, 740f, 105f, 92f);
                        case "branch_coin_leaf": return new Rect(675f, 765f, 46f, 40f);
                        case "branch_price_value": return new Rect(790f, 765f, 72f, 45f);
                        case "branch_owned": return new Rect(927f, 429f, 82f, 76f);
                        case "branch_owned_value": return new Rect(949f, 442f, 38f, 46f);
                        case "reveal_card": return new Rect(60f, 880f, 459f, 420f);
                        case "reveal_frame": return new Rect(60f, 880f, 459f, 420f);
                        case "reveal_cream": return new Rect(88f, 908f, 399f, 365f);
                        case "reveal_ribbon": return new Rect(113f, 904f, 355f, 92f);
                        case "reveal_leaf_l": return new Rect(104f, 937f, 88f, 64f);
                        case "reveal_leaf_r": return new Rect(382f, 942f, 86f, 57f);
                        case "reveal_title": return new Rect(208f, 917f, 159f, 46f);
                        case "reveal_ring": return new Rect(155f, 979f, 260f, 227f);
                        case "reveal_backing": return new Rect(177f, 996f, 216f, 194f);
                        case "reveal_price": return new Rect(180f, 1200f, 241f, 75f);
                        case "reveal_coin": return new Rect(145f, 1190f, 105f, 92f);
                        case "reveal_coin_leaf": return new Rect(175f, 1215f, 46f, 40f);
                        case "reveal_price_value": return new Rect(290f, 1215f, 72f, 45f);
                        case "reveal_owned": return new Rect(427f, 879f, 82f, 76f);
                        case "reveal_owned_value": return new Rect(449f, 892f, 38f, 46f);
                        case "bell_card": return new Rect(560f, 880f, 459f, 420f);
                        case "bell_frame": return new Rect(560f, 880f, 459f, 420f);
                        case "bell_cream": return new Rect(588f, 908f, 399f, 365f);
                        case "bell_ribbon": return new Rect(613f, 904f, 355f, 92f);
                        case "bell_leaf_l": return new Rect(604f, 937f, 88f, 64f);
                        case "bell_leaf_r": return new Rect(882f, 942f, 86f, 57f);
                        case "bell_title": return new Rect(739f, 917f, 100f, 46f);
                        case "bell_ring": return new Rect(655f, 979f, 260f, 227f);
                        case "bell_backing": return new Rect(677f, 996f, 216f, 194f);
                        case "bell_price": return new Rect(680f, 1200f, 241f, 75f);
                        case "bell_coin": return new Rect(645f, 1190f, 105f, 92f);
                        case "bell_coin_leaf": return new Rect(675f, 1215f, 46f, 40f);
                        case "bell_price_value": return new Rect(790f, 1215f, 72f, 45f);
                        case "bell_owned": return new Rect(927f, 879f, 82f, 76f);
                        case "bell_owned_value": return new Rect(949f, 892f, 38f, 46f);
                        case "shoo_card": return new Rect(60f, 1330f, 459f, 420f);
                        case "shoo_frame": return new Rect(60f, 1330f, 459f, 420f);
                        case "shoo_cream": return new Rect(88f, 1358f, 399f, 365f);
                        case "shoo_ribbon": return new Rect(113f, 1354f, 355f, 92f);
                        case "shoo_leaf_l": return new Rect(104f, 1387f, 88f, 64f);
                        case "shoo_leaf_r": return new Rect(382f, 1392f, 86f, 57f);
                        case "shoo_title": return new Rect(225f, 1366f, 130f, 47f);
                        case "shoo_ring": return new Rect(155f, 1429f, 260f, 227f);
                        case "shoo_backing": return new Rect(177f, 1446f, 216f, 194f);
                        case "shoo_price": return new Rect(180f, 1650f, 241f, 75f);
                        case "shoo_coin": return new Rect(145f, 1640f, 105f, 92f);
                        case "shoo_coin_leaf": return new Rect(175f, 1665f, 46f, 40f);
                        case "shoo_price_value": return new Rect(276f, 1665f, 100f, 45f);
                        case "rain_card": return new Rect(560f, 1330f, 459f, 420f);
                        case "rain_frame": return new Rect(560f, 1330f, 459f, 420f);
                        case "rain_cream": return new Rect(588f, 1358f, 399f, 365f);
                        case "rain_ribbon": return new Rect(613f, 1354f, 355f, 92f);
                        case "rain_leaf_l": return new Rect(604f, 1387f, 88f, 64f);
                        case "rain_leaf_r": return new Rect(882f, 1392f, 86f, 57f);
                        case "rain_title": return new Rect(730f, 1366f, 121f, 47f);
                        case "rain_ring": return new Rect(655f, 1429f, 260f, 227f);
                        case "rain_backing": return new Rect(677f, 1446f, 216f, 194f);
                        case "rain_price": return new Rect(680f, 1650f, 241f, 75f);
                        case "rain_coin": return new Rect(645f, 1640f, 105f, 92f);
                        case "rain_coin_leaf": return new Rect(675f, 1665f, 46f, 40f);
                        case "rain_price_value": return new Rect(776f, 1665f, 100f, 45f);
                        case "hint_bulb": return new Rect(242f, 597f, 87f, 126f);
                        case "hint_rays": return new Rect(207f, 563f, 155f, 96f);
                        case "branch_log": return new Rect(699f, 637f, 173f, 66f);
                        case "branch_sprout": return new Rect(705f, 600f, 66f, 53f);
                        case "branch_plus": return new Rect(783f, 582f, 65f, 61f);
                        case "reveal_eye": return new Rect(239f, 1006f, 97f, 49f);
                        case "reveal_egg": return new Rect(231f, 1057f, 106f, 88f);
                        case "reveal_nest": return new Rect(197f, 1093f, 167f, 81f);
                        case "bell_icon": return new Rect(706f, 1005f, 157f, 163f);
                        case "shoo_bird": return new Rect(193f, 1451f, 173f, 187f);
                        case "shoo_x": return new Rect(287f, 1551f, 90f, 85f);
                        case "rain_cloud": return new Rect(701f, 1462f, 165f, 77f);
                        case "rain_drop_left": return new Rect(721f, 1527f, 33f, 43f);
                        case "rain_drop_middle": return new Rect(764f, 1532f, 33f, 42f);
                        case "rain_drop_right": return new Rect(814f, 1527f, 33f, 43f);
                        case "rain_sprout": return new Rect(732f, 1566f, 105f, 44f);
                        case "rain_soil": return new Rect(725f, 1601f, 111f, 23f);
                    }
                    break;
                case "shop_outfits":
                    switch (element)
                    {
                        case "bg": return new Rect(0f, 0f, 1080f, 1920f);
                        case "hud_coin": return new Rect(103f, 72f, 268f, 109f);
                        case "close": return new Rect(920f, 74f, 115f, 115f);
                        case "tab_light": return new Rect(302f, 213f, 118f, 118f);
                        case "tab_outfits": return new Rect(477f, 213f, 118f, 118f);
                        case "tab_home": return new Rect(661f, 213f, 118f, 118f);
                        case "hud_leaf_coin": return new Rect(47f, 60f, 132f, 126f);
                        case "close_x": return new Rect(946f, 102f, 66f, 59f);
                        case "tab_light_glyph": return new Rect(326f, 238f, 66f, 73f);
                        case "tab_outfits_glyph": return new Rect(505f, 242f, 69f, 63f);
                        case "tab_home_glyph": return new Rect(689f, 240f, 64f, 64f);
                        case "hud_value": return new Rect(198f, 94f, 142f, 56f);
                        case "tab_selected": return new Rect(498f, 325f, 82f, 28f);
                        case "equipped_glow": return new Rect(535f, 407f, 508f, 682f);
                        case "card_straw": return new Rect(54f, 424f, 468f, 646f);
                        case "card_scarf": return new Rect(551f, 424f, 474f, 647f);
                        case "card_glasses": return new Rect(54f, 1095f, 468f, 646f);
                        case "card_flower": return new Rect(551f, 1095f, 474f, 646f);
                        case "straw_frame": return new Rect(54f, 424f, 468f, 646f);
                        case "straw_cream": return new Rect(83f, 453f, 410f, 588f);
                        case "scarf_frame": return new Rect(551f, 424f, 474f, 647f);
                        case "scarf_cream": return new Rect(580f, 453f, 416f, 589f);
                        case "glasses_frame": return new Rect(54f, 1095f, 468f, 646f);
                        case "glasses_cream": return new Rect(83f, 1124f, 410f, 588f);
                        case "flower_frame": return new Rect(551f, 1095f, 474f, 646f);
                        case "flower_cream": return new Rect(580f, 1124f, 416f, 588f);
                        case "straw_bird": return new Rect(118f, 568f, 348f, 368f);
                        case "scarf_bird": return new Rect(637f, 569f, 306f, 367f);
                        case "glasses_bird": return new Rect(139f, 1238f, 306f, 368f);
                        case "flower_bird": return new Rect(638f, 1237f, 298f, 368f);
                        case "straw_hat": return new Rect(118f, 569f, 316f, 194f);
                        case "cozy_scarf": return new Rect(651f, 770f, 281f, 157f);
                        case "round_glasses": return new Rect(183f, 1383f, 228f, 93f);
                        case "flower_accessory": return new Rect(808f, 1307f, 89f, 72f);
                        case "hat_daisy": return new Rect(163f, 595f, 78f, 80f);
                        case "straw_ribbon": return new Rect(97f, 463f, 382f, 102f);
                        case "scarf_ribbon": return new Rect(594f, 463f, 388f, 102f);
                        case "glasses_ribbon": return new Rect(97f, 1134f, 382f, 102f);
                        case "flower_ribbon": return new Rect(594f, 1134f, 388f, 102f);
                        case "straw_price": return new Rect(178f, 941f, 255f, 96f);
                        case "glasses_price": return new Rect(178f, 1611f, 255f, 96f);
                        case "flower_wear": return new Rect(730f, 1603f, 113f, 113f);
                        case "straw_leaves_l": return new Rect(97f, 488f, 73f, 75f);
                        case "straw_leaves_r": return new Rect(387f, 488f, 92f, 75f);
                        case "scarf_leaves_l": return new Rect(594f, 488f, 73f, 75f);
                        case "scarf_leaves_r": return new Rect(890f, 488f, 92f, 75f);
                        case "glasses_leaves_l": return new Rect(97f, 1159f, 73f, 75f);
                        case "glasses_leaves_r": return new Rect(387f, 1159f, 92f, 75f);
                        case "flower_leaves_l": return new Rect(594f, 1159f, 73f, 75f);
                        case "flower_leaves_r": return new Rect(890f, 1159f, 92f, 75f);
                        case "straw_coin": return new Rect(141f, 934f, 126f, 109f);
                        case "glasses_coin": return new Rect(141f, 1604f, 126f, 109f);
                        case "wear_hanger": return new Rect(753f, 1624f, 68f, 62f);
                        case "straw_name": return new Rect(178f, 478f, 233f, 53f);
                        case "scarf_name": return new Rect(670f, 477f, 247f, 54f);
                        case "glasses_name": return new Rect(143f, 1147f, 303f, 54f);
                        case "flower_name": return new Rect(710f, 1146f, 180f, 55f);
                        case "straw_amount": return new Rect(285f, 958f, 114f, 55f);
                        case "glasses_amount": return new Rect(285f, 1628f, 114f, 55f);
                        case "equipped_badge": return new Rect(980f, 415f, 56f, 56f);
                        case "equipped_check": return new Rect(991f, 429f, 34f, 28f);
                    }
                    break;
                case "shop_houses":
                    switch (element)
                    {
                        case "bg": return new Rect(0f, 0f, 1080f, 1920f);
                        case "hud": return new Rect(93f, 69f, 281f, 116f);
                        case "hud_num": return new Rect(198f, 95f, 140f, 54f);
                        case "hud_coin": return new Rect(45f, 59f, 136f, 135f);
                        case "hud_leaf": return new Rect(78f, 88f, 67f, 66f);
                        case "close": return new Rect(919f, 73f, 124f, 120f);
                        case "close_x": return new Rect(952f, 103f, 58f, 58f);
                        case "tab_boost": return new Rect(301f, 213f, 117f, 119f);
                        case "icon_bulb": return new Rect(329f, 240f, 61f, 65f);
                        case "tab_clothes": return new Rect(480f, 213f, 119f, 120f);
                        case "icon_hanger": return new Rect(506f, 244f, 69f, 53f);
                        case "selected_glow": return new Rect(645f, 195f, 151f, 147f);
                        case "tab_houses": return new Rect(662f, 214f, 117f, 114f);
                        case "icon_home": return new Rect(688f, 238f, 66f, 65f);
                        case "selected_pill": return new Rect(685f, 327f, 70f, 28f);
                        case "mush_card": return new Rect(47f, 422f, 484f, 654f);
                        case "mush_frame": return new Rect(47f, 422f, 484f, 654f);
                        case "mush_cream": return new Rect(78f, 453f, 423f, 591f);
                        case "mush_ribbon": return new Rect(128f, 461f, 313f, 101f);
                        case "mush_leaf_l": return new Rect(90f, 498f, 90f, 75f);
                        case "mush_leaf_r": return new Rect(400f, 498f, 78f, 75f);
                        case "mush_title": return new Rect(175f, 478f, 234f, 45f);
                        case "mush_house": return new Rect(94f, 573f, 394f, 350f);
                        case "mush_price": return new Rect(146f, 942f, 298f, 95f);
                        case "mush_coin": return new Rect(143f, 936f, 121f, 110f);
                        case "mush_coin_leaf": return new Rect(173f, 966f, 59f, 55f);
                        case "mush_num": return new Rect(289f, 964f, 103f, 44f);
                        case "tea_card": return new Rect(548f, 422f, 484f, 655f);
                        case "tea_frame": return new Rect(548f, 422f, 484f, 655f);
                        case "tea_cream": return new Rect(579f, 453f, 422f, 591f);
                        case "tea_ribbon": return new Rect(638f, 461f, 309f, 102f);
                        case "tea_leaf_l": return new Rect(591f, 498f, 91f, 75f);
                        case "tea_leaf_r": return new Rect(904f, 498f, 78f, 75f);
                        case "tea_title": return new Rect(711f, 476f, 166f, 46f);
                        case "tea_house": return new Rect(590f, 585f, 408f, 335f);
                        case "tea_price": return new Rect(640f, 942f, 304f, 95f);
                        case "tea_coin": return new Rect(636f, 936f, 121f, 110f);
                        case "tea_coin_leaf": return new Rect(666f, 966f, 59f, 55f);
                        case "tea_num": return new Rect(786f, 964f, 102f, 44f);
                        case "lan_card": return new Rect(48f, 1090f, 484f, 658f);
                        case "lan_frame": return new Rect(48f, 1090f, 484f, 658f);
                        case "lan_cream": return new Rect(79f, 1122f, 422f, 594f);
                        case "lan_ribbon": return new Rect(129f, 1130f, 312f, 104f);
                        case "lan_leaf_l": return new Rect(91f, 1167f, 90f, 76f);
                        case "lan_leaf_r": return new Rect(400f, 1167f, 79f, 76f);
                        case "lan_title": return new Rect(193f, 1146f, 202f, 47f);
                        case "lan_house": return new Rect(103f, 1238f, 374f, 362f);
                        case "lan_price": return new Rect(145f, 1614f, 299f, 97f);
                        case "lan_coin": return new Rect(143f, 1609f, 121f, 109f);
                        case "lan_coin_leaf": return new Rect(173f, 1639f, 59f, 55f);
                        case "lan_num": return new Rect(289f, 1637f, 103f, 44f);
                        case "acorn_card": return new Rect(548f, 1090f, 484f, 658f);
                        case "acorn_frame": return new Rect(548f, 1090f, 484f, 658f);
                        case "acorn_cream": return new Rect(579f, 1122f, 422f, 594f);
                        case "acorn_ribbon": return new Rect(638f, 1130f, 310f, 104f);
                        case "acorn_leaf_l": return new Rect(591f, 1167f, 92f, 76f);
                        case "acorn_leaf_r": return new Rect(904f, 1167f, 78f, 76f);
                        case "acorn_title": return new Rect(708f, 1144f, 177f, 51f);
                        case "acorn_house": return new Rect(596f, 1239f, 384f, 361f);
                        case "acorn_price": return new Rect(640f, 1614f, 304f, 97f);
                        case "acorn_coin": return new Rect(636f, 1609f, 121f, 109f);
                        case "acorn_coin_leaf": return new Rect(666f, 1639f, 59f, 55f);
                        case "acorn_num": return new Rect(786f, 1637f, 102f, 44f);
                    }
                    break;
                case "shop_confirm":
                    switch (element)
                    {
                        case "forest": return new Rect(0f, 0f, 1080f, 1920f);
                        case "canopy": return new Rect(0f, 0f, 1080f, 273f);
                        case "hud": return new Rect(145f, 71f, 224f, 105f);
                        case "hudcoin": return new Rect(44f, 57f, 140f, 132f);
                        case "hudnum": return new Rect(208f, 94f, 125f, 56f);
                        case "pagex": return new Rect(923f, 73f, 117f, 112f);
                        case "pagexg": return new Rect(952f, 103f, 56f, 54f);
                        case "tabhint": return new Rect(300f, 212f, 123f, 119f);
                        case "tabhintg": return new Rect(328f, 239f, 65f, 66f);
                        case "tabward": return new Rect(479f, 216f, 123f, 114f);
                        case "tabwardg": return new Rect(515f, 241f, 53f, 53f);
                        case "tabhome": return new Rect(657f, 216f, 125f, 115f);
                        case "tabhomeg": return new Rect(694f, 239f, 49f, 57f);
                        case "selected": return new Rect(322f, 321f, 78f, 30f);
                        case "cardhint": return new Rect(54f, 427f, 468f, 428f);
                        case "cardbranch": return new Rect(559f, 427f, 466f, 428f);
                        case "cardmidl": return new Rect(54f, 875f, 468f, 434f);
                        case "cardmidr": return new Rect(559f, 875f, 466f, 434f);
                        case "cardshoo": return new Rect(54f, 1327f, 468f, 433f);
                        case "cardrain": return new Rect(559f, 1327f, 466f, 433f);
                        case "rh": return new Rect(104f, 449f, 371f, 90f);
                        case "rht": return new Rect(245f, 466f, 100f, 39f);
                        case "rb": return new Rect(607f, 449f, 371f, 89f);
                        case "rbt": return new Rect(690f, 465f, 267f, 42f);
                        case "rs": return new Rect(104f, 1344f, 369f, 89f);
                        case "rst": return new Rect(244f, 1363f, 102f, 42f);
                        case "rr": return new Rect(607f, 1344f, 371f, 89f);
                        case "rrt": return new Rect(751f, 1363f, 95f, 42f);
                        case "bh": return new Rect(452f, 429f, 62f, 62f);
                        case "bhn": return new Rect(469f, 442f, 30f, 39f);
                        case "bb": return new Rect(953f, 429f, 63f, 62f);
                        case "bbn": return new Rect(972f, 442f, 29f, 39f);
                        case "bm": return new Rect(970f, 880f, 46f, 56f);
                        case "bmn": return new Rect(977f, 891f, 20f, 36f);
                        case "midlring": return new Rect(200f, 1170f, 190f, 23f);
                        case "midrring": return new Rect(704f, 1170f, 186f, 23f);
                        case "midlp": return new Rect(155f, 1206f, 263f, 83f);
                        case "midlpc": return new Rect(155f, 1202f, 103f, 92f);
                        case "midlpn": return new Rect(300f, 1223f, 62f, 41f);
                        case "midrp": return new Rect(659f, 1206f, 263f, 83f);
                        case "midrpc": return new Rect(659f, 1202f, 103f, 92f);
                        case "midrpn": return new Rect(802f, 1223f, 62f, 41f);
                        case "shoop": return new Rect(155f, 1655f, 263f, 83f);
                        case "shoopc": return new Rect(155f, 1651f, 103f, 92f);
                        case "shoopn": return new Rect(293f, 1674f, 85f, 41f);
                        case "rainp": return new Rect(659f, 1655f, 263f, 83f);
                        case "rainpc": return new Rect(659f, 1651f, 103f, 92f);
                        case "rainpn": return new Rect(792f, 1675f, 85f, 40f);
                        case "shooface": return new Rect(166f, 1423f, 245f, 241f);
                        case "shoofacer": return new Rect(166f, 1423f, 245f, 241f);
                        case "rainface": return new Rect(670f, 1423f, 245f, 241f);
                        case "rainfacer": return new Rect(670f, 1423f, 245f, 241f);
                        case "bird": return new Rect(227f, 1480f, 107f, 128f);
                        case "shoox": return new Rect(291f, 1550f, 61f, 58f);
                        case "raincloud": return new Rect(730f, 1480f, 135f, 78f);
                        case "raindrops": return new Rect(750f, 1541f, 91f, 46f);
                        case "rainsprout": return new Rect(753f, 1581f, 82f, 54f);
                        case "dim": return new Rect(0f, 0f, 1080f, 1920f);
                        case "panel": return new Rect(110f, 550f, 860f, 620f);
                        case "cushion": return new Rect(159f, 600f, 762f, 531f);
                        case "frame": return new Rect(110f, 550f, 860f, 620f);
                        case "hintdisc": return new Rect(332f, 594f, 410f, 362f);
                        case "hintring": return new Rect(332f, 594f, 410f, 362f);
                        case "bulb": return new Rect(421f, 624f, 233f, 258f);
                        case "price": return new Rect(299f, 955f, 474f, 178f);
                        case "pricecoin": return new Rect(348f, 973f, 154f, 148f);
                        case "pricenum": return new Rect(511f, 984f, 194f, 122f);
                        case "ribbon": return new Rect(220f, 475f, 640f, 150f);
                        case "leafl": return new Rect(232f, 477f, 98f, 105f);
                        case "leafr": return new Rect(747f, 477f, 105f, 105f);
                        case "title": return new Rect(393f, 493f, 296f, 91f);
                        case "close": return new Rect(896f, 524f, 100f, 100f);
                        case "closeg": return new Rect(917f, 546f, 59f, 59f);
                        case "confirm": return new Rect(478f, 1218f, 124f, 124f);
                        case "confirmg": return new Rect(493f, 1246f, 92f, 77f);
                    }
                    break;
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
