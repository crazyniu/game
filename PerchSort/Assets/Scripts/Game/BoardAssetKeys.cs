using System;
namespace PerchSort.Game
{
    public static class BoardAssetKeys
    {
        public static string GiftCoin(string species)
        {
            switch (species)
            {
                case "sunny": return "gift_coin_wake";
                case "sky": return "gift_coin_reveal";
                case "berry": return "gift_coin_encore";
                case "mint": return "gift_coin_grow";
                case "cocoa": return "gift_coin_scare";
                case "snow": return "gift_coin_lightstep";
                case "ember": return "gift_coin_hatch";
                case "ink": return "gift_coin_nightcall";
                case "hoot": return "gift_coin_moonbeam";
                default: throw new ArgumentException("No board gift coin for " + species);
            }
        }
    }
}
