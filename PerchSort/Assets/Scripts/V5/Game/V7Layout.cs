using System;
using UnityEngine;

namespace PerchSort.V5.Game
{
    // Derived from art/v7/layout_v7.json: screens, HUD, count adaptation, map and aligned house parts.
    // All points are top-left 1080x1920 pixels. House parts retain their shared 512x512 canvas.
    public static class V7Layout
    {
        public const float DesignWidth = 1080, DesignHeight = 1920, CenterX = 540;
        public static readonly Vector2 DesignSize = new Vector2(1080, 1920);
        public static readonly Rect SafeDesignArea = new Rect(0, 0, 1080, 1920);
        public const float MinimumHudGap = 40, TopRowGap = 60;
        public const float SecondaryButtonSize = 116, PrimaryButtonSize = 200, IconSize = 76, SmallIconSize = 64;
        public const string SecondaryButtonSprite = "btn_purple", PrimaryButtonSprite = "btn_green";
        public const float DisabledAlpha = 0.45f, DisabledScale = 0.9f, ButtonIconRatio = 0.62f;
        public const float ButtonPressScale = 0.92f, ButtonPressSeconds = 0.08f, ButtonReleaseSeconds = 0.18f;
        public const float CoinFontSize = 44, CoinMinimumFontSize = 24, LevelBadgeFontSize = 46;
        public static readonly Rect CoinPill = Screen("level_k3", "coin_pill");
        public static readonly Rect CoinNumber = new Rect(CoinPill.x + CoinPill.width * 142 / 320, CoinPill.y + CoinPill.height * 32 / 128, CoinPill.width * 140 / 320, CoinPill.height * 60 / 128);
        public static readonly Rect CoinIcon = new Rect(CoinPill.x, CoinPill.y, CoinPill.height, CoinPill.height);
        public static readonly Rect SettingsButton = Screen("level_k3", "gear"), LevelSign = new Rect(476, 56, 128, 86);
        public static readonly Vector2 LevelNumberOffset = Vector2.zero;
        public static readonly Rect PhaseDial = new Rect(330, 160, 100, 80);
        public static readonly Vector2 PhaseIconCenter = new Vector2(26, 40), PhaseCountCenter = new Vector2(72, 40), PhaseCountSize = new Vector2(40, 48);
        public const float PhaseIconSize = 44, PhaseFontSize = 24;
        public static readonly Vector2 LightStepPoint = new Vector2(1010, 1700);
        public const float FeatherSize = 44;
        public static readonly Rect ChainRibbon = new Rect(830, 490, 180, 60), GoalTray = new Rect(0, 210, 1080, 310);
        public const float ChainFontSize = 32;
        public const float BoardTop = 570, BoardBottom = 1700, BoardHeight = BoardBottom - BoardTop;
        public const float BranchWidth = 530, BranchHeight = 90, MaximumRowPitch = 350;
        public const float BirdSize = 200, BirdSlotPitch = 110, FirstBirdSlot = 100, BirdCenterAboveBranch = 90;
        public const float BranchHitWidth = 520, BranchHitHeight = 180, BranchHitLift = 85;
        public static readonly Vector2 BirdDisplaySize = Vector2.one * BirdSize, BranchDisplaySize = new Vector2(BranchWidth, BranchHeight), TwigDisplaySize = new Vector2(400, 80), BushSize = new Vector2(330, 250);
        public const float BushAboveBranch = 300, BranchCenterBelowFeet = 14;
        public static float RowPitch(int rows) => rows <= 2 ? MaximumRowPitch : 880f / (rows - 0.5f);
        public static float BirdScale(int rows) => rows <= 5 ? 1 : Mathf.Clamp(5f / rows, 0.72f, 1);
        public static float SeatPitch(float scale) => BirdSlotPitch * Mathf.Min(1, scale);
        public static float RowY(int row, int rows, bool right = false) => (rows <= 2 ? 925 : 760) + (row + (right ? 0.5f : 0)) * RowPitch(rows);
        public static Rect BranchHit(int row, int rows, bool right) => Centered(new Vector2(right ? DesignWidth - BranchHitWidth / 2 : BranchHitWidth / 2, RowY(row, rows, right) - BranchHitLift), new Vector2(BranchHitWidth, Mathf.Min(BranchHitHeight * BirdScale(rows), RowPitch(rows) * 0.75f)));
        public static Vector2 BirdSlot(int slot, float y, bool right, float scale = 1)
        {
            float x = FirstBirdSlot + SeatPitch(scale) * slot;
            return new Vector2(right ? DesignWidth - x : x, y - BirdCenterAboveBranch * scale);
        }
        public static float BranchPerchTop(int world, bool twig) => twig ?
            (world == 1 ? 18 : world == 2 ? 19 : world == 3 ? 8 : 22) :
            (world == 1 ? 17 : world == 2 ? 9 : world == 3 ? 22 : 30);
        private static Vector2 CrownAttachment(int world) => world == 1 ? new Vector2(138, 319) :
            world == 2 ? new Vector2(96, 332) : world == 3 ? new Vector2(111, 358) : new Vector2(105, 355);
        public static Rect WorldCrown(int world, bool right)
        {
            float left = (world == 2 || world == 3 ? 40 : 70) - CrownAttachment(world).x * 2 / 3;
            return new Rect(right ? DesignWidth - left - 400 : left, 470, 400, 400f * 2 / 3);
        }
        public static Rect WorldTrunk(int world, bool right)
        {
            float top = 470 + CrownAttachment(world).y * 2 / 3 - (world == 3 ? 85 : world == 2 ? 65 : 25);
            return new Rect(right ? 990 : -70, top, 160, 1920);
        }
        public const float ToolY = 1802, ToolFirstX = 260, ToolPitch = 182;
        public const int ToolCount = 4;
        public const float ToolGap = ToolPitch - SecondaryButtonSize, ToolBottom = ToolY + SecondaryButtonSize / 2;
        public static Rect ToolButton(int index, int count = 3) => Screen(count == 3 ? "level_k3" : "level_k7", index == 0 ? "undo" : index == 1 ? "hint" : index == 2 ? "branch_plus" : "scarecrow");
        public static readonly Vector2 StockCenter = new Vector2(100, 8);
        public const float StockSize = 46, StockFontSize = 30;
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
        public const float MapBounceSeconds = 1.8f, MapStride = 2840, MapSegmentHeight = 3000, MapHeight = 11520, MapFocusRatio = 0.4f;
        public static readonly Rect MapViewport = SafeDesignArea;
        public static readonly Vector2 MapShadowSize = new Vector2(170, 38), MapGateSize = Screen("map", "gate").size, MapSettingsCenter = Screen("map", "settings").center;
        public static readonly Vector2 MapLockOffset = new Vector2(0, 40), MapNumberOffset = new Vector2(0, -10);
        public const float MapLockSize = 48, MapScrollSensitivity = 70;
        public static float WorldTop(int world) => (4 - world) * MapStride;
        public static Vector2 HouseDoorLocal(Vector2 size) => new Vector2(size.x * 253 / 512, size.y * 306 / 512);
        public static Vector2 HouseHingeLocal(Vector2 size) => new Vector2(size.x * 145 / 512, size.y * 306 / 512);
        public static float HouseDoorDiameter(float size) => size * 204 / 512;
        public static Rect HouseCanvas(int index, int count)
        {
            Rect visible = House(index, count);
            float size = Mathf.Max(visible.width * 512 / 448, visible.height * 512 / 398);
            return new Rect(visible.center.x - size / 2, visible.y - size * 58 / 512, size, size);
        }
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
            throw new ArgumentException("Unmeasured v7 element: " + screen + "/" + element);
        }
        public static Rect House(int index, int count)
        {
            if (count == 1) return Screen("level_k3", "house");
            switch (count)
            {
                case 2:
                    switch (index)
                    {
                        case 0: return new Rect(395f, 296f, 150f, 176f);
                        case 1: return new Rect(535f, 296f, 150f, 176f);
                    }
                    break;
                case 3:
                    switch (index)
                    {
                        case 0: return new Rect(325f, 296f, 150f, 176f);
                        case 1: return new Rect(465f, 296f, 150f, 176f);
                        case 2: return new Rect(605f, 296f, 150f, 176f);
                    }
                    break;
                case 4:
                    switch (index)
                    {
                        case 0: return new Rect(255f, 296f, 150f, 176f);
                        case 1: return new Rect(395f, 296f, 150f, 176f);
                        case 2: return new Rect(535f, 296f, 150f, 176f);
                        case 3: return new Rect(675f, 296f, 150f, 176f);
                    }
                    break;
                case 5:
                    switch (index)
                    {
                        case 0: return new Rect(185f, 296f, 150f, 176f);
                        case 1: return new Rect(325f, 296f, 150f, 176f);
                        case 2: return new Rect(465f, 296f, 150f, 176f);
                        case 3: return new Rect(605f, 296f, 150f, 176f);
                        case 4: return new Rect(745f, 296f, 150f, 176f);
                    }
                    break;
                case 6:
                    switch (index)
                    {
                        case 0: return new Rect(115f, 296f, 150f, 176f);
                        case 1: return new Rect(255f, 296f, 150f, 176f);
                        case 2: return new Rect(395f, 296f, 150f, 176f);
                        case 3: return new Rect(535f, 296f, 150f, 176f);
                        case 4: return new Rect(675f, 296f, 150f, 176f);
                        case 5: return new Rect(815f, 296f, 150f, 176f);
                    }
                    break;
                case 7:
                    switch (index)
                    {
                        case 0: return new Rect(45f, 296f, 150f, 176f);
                        case 1: return new Rect(185f, 296f, 150f, 176f);
                        case 2: return new Rect(325f, 296f, 150f, 176f);
                        case 3: return new Rect(465f, 296f, 150f, 176f);
                        case 4: return new Rect(605f, 296f, 150f, 176f);
                        case 5: return new Rect(745f, 296f, 150f, 176f);
                        case 6: return new Rect(885f, 296f, 150f, 176f);
                    }
                    break;
            }
            throw new ArgumentOutOfRangeException(nameof(count));
        }
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
