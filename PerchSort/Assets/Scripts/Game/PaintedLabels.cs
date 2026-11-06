using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public static class PaintedLabels
    {
        private static readonly Dictionary<string, Sprite> Sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
        private static readonly Dictionary<string, string> Keys = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "Zzz", "label_small_zzz" },
            { "Cheerful. Always first to sing.", "label_caption_cheerful_always_first_to_sing" },
            { "Sleepy-eyed. Never in a hurry.", "label_caption_sleepy_eyed_never_in_a_hurry" },
            { "Show-off. Loves an encore.", "label_caption_show_off_loves_an_encore" },
            { "Gardener. Makes branches grow.", "label_caption_gardener_makes_branches_grow" },
            { "Grumpy. Scares crows away.", "label_caption_grumpy_scares_crows_away" },
            { "Light on her feet. Takes a shortcut.", "label_caption_light_on_her_feet_takes_a_shortcut" },
            { "Hot-headed. Cracks eggs open.", "label_caption_hot_headed_cracks_eggs_open" },
            { "Trouble in black feathers.", "label_caption_trouble_in_black_feathers" },
            { "Night owl. Flips the sky.", "label_caption_night_owl_flips_the_sky" },
            { "Dreamy. Calls the moon.", "label_caption_dreamy_calls_the_moon" },
            { "Wakes every sleeping bird", "label_caption_wakes_every_sleeping_bird" },
            { "Shows every egg's secret", "label_caption_shows_every_egg_s_secret" },
            { "Next move is free", "label_caption_next_move_is_free" },
            { "Branches grow one seat", "label_caption_branches_grow_one_seat" },
            { "Scares nearby crows away", "label_caption_scares_nearby_crows_away" },
            { "Next landing breaks the rules", "label_caption_next_landing_breaks_the_rules" },
            { "Every egg hatches now", "label_caption_every_egg_hatches_now" },
            { "Day and night swap", "label_caption_day_and_night_swap" },
            { "Three moves of night", "label_caption_three_moves_of_night" },
            { "Find a helpful move", "label_caption_find_a_helpful_move" },
            { "Add one branch", "label_caption_add_one_branch" },
            { "See inside every egg", "label_caption_see_inside_every_egg" },
            { "Wake sleeping birds", "label_caption_wake_sleeping_birds" },
            { "Send crows away", "label_caption_send_crows_away" },
            { "Grow branches fully", "label_caption_grow_branches_fully" },
            { "Wakes all sleepers", "label_caption_wakes_all_sleepers" },
            { "Reveals every egg", "label_caption_reveals_every_egg" },
            { "One free move", "label_caption_one_free_move" },
            { "Grows branches", "label_caption_grows_branches" },
            { "Sends crows away", "label_caption_sends_crows_away" },
            { "Land anywhere", "label_caption_land_anywhere" },
            { "Hatches every egg", "label_caption_hatches_every_egg" },
            { "Switches day/night", "label_caption_switches_day_night" },
            { "Moonlit moves", "label_caption_moonlit_moves" },
            { "Never goes home", "label_caption_never_goes_home" },
            { "Firefly", "label_booster_firefly" },
            { "Sapling", "label_booster_sapling" },
            { "Owl Eye", "label_booster_owl_eye" },
            { "Morning Bell", "label_booster_morning_bell" },
            { "Scarecrow", "label_booster_scarecrow" },
            { "Spring Rain", "label_booster_spring_rain" },
            { "Advance the day/night cycle", "label_small_advance_the_day_night_cycle" },
            { "Bring the flock together", "label_small_bring_the_flock_together" },
            { "Hatch, then gather", "label_small_hatch_then_gather" },
            { "Wake, then gather", "label_small_wake_then_gather" },
            { "Use the new seats", "label_small_use_the_new_seats" },
            { "Daily Gift", "label_title_daily_gift" },
            { "Well done!", "label_title_well_done" },
            { "New friend", "label_title_new_friend" },
            { "World Complete", "label_title_world_complete" },
            { "World Unlocked", "label_title_world_unlocked" },
            { "Aviary", "label_title_aviary" },
            { "Settings", "label_title_settings" },
            { "Forest Trail", "label_title_forest_trail" },
            { "Hint", "label_card_hint" },
            { "Extra Branch", "label_card_extra_branch" },
            { "Reveal", "label_card_reveal" },
            { "Bell", "label_card_bell" },
            { "Shoo", "label_card_shoo" },
            { "Rain", "label_card_rain" },
            { "Mushroom", "label_card_mushroom" },
            { "Teapot", "label_card_teapot" },
            { "Lantern", "label_card_lantern" },
            { "Acorn", "label_card_acorn" },
            { "Straw Hat", "label_card_straw_hat" },
            { "Cozy Scarf", "label_card_cozy_scarf" },
            { "Round Glasses", "label_card_round_glasses" },
            { "Flower", "label_card_flower" },
            { "Best", "label_caption_best" },
            { "Sound", "label_caption_sound" },
            { "Restart", "label_caption_restart" },
            { "Home", "label_caption_home" },
            { "Free move", "label_caption_free_move" },
            { "Wakes sleepers", "label_caption_wakes_sleepers" },
            { "Reveals eggs", "label_caption_reveals_eggs" },
            { "Autumn Forest", "label_caption_autumn_forest" },
            { "Green Forest", "label_caption_green_forest" },
            { "Sunny", "label_name_sunny" },
            { "Skye", "label_name_sky" },
            { "Berry", "label_name_berry" },
            { "Minty", "label_name_mint" },
            { "Cocoa", "label_name_cocoa" },
            { "Flurry", "label_name_snow" },
            { "Ember", "label_name_ember" },
            { "Rook", "label_name_crow" },
            { "Inky", "label_name_ink" },
            { "Hoot", "label_name_hoot" },
            { "?", "label_name_unknown" },
        };

        public static Sprite Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Missing painted label key.");
                return null;
            }
            if (Sprites.TryGetValue(key, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>("Art/labels/" + key);
            Sprites.Add(key, sprite);
            if (sprite == null) Debug.LogError("Missing painted label sprite: Art/labels/" + key);
            return sprite;
        }

        public static Image Create(Transform parent, string name, string key, Vector2 point, float height)
        {
            var sprite = Get(key);
            float width = sprite != null ? height * sprite.rect.width / sprite.rect.height : height;
            var image = GameUI.Fit(parent, name, sprite, point, new Vector2(width, height));
            // A missing plate keeps its layout slot without drawing uGUI's white rectangle.
            image.enabled = sprite != null;
            return image;
        }

        public static string KeyForText(string text)
        {
            if (text == null) return null;
            if (Keys.TryGetValue(text, out var key)) return key;
            const string prefix = "Level ";
            if (text.StartsWith(prefix, StringComparison.Ordinal) &&
                int.TryParse(text.Substring(prefix.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int level) &&
                level >= 1 && level <= 40 && text == prefix + level.ToString(CultureInfo.InvariantCulture))
                return "label_level_" + level.ToString(CultureInfo.InvariantCulture);
            return null;
        }
    }
}
