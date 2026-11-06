using System.Globalization;
namespace PerchSort.Game
{
    public static class BoardNumber
    {
        // Review-only overflow convention; exact values remain on accessible object names.
        public static string Format(int value, int fullLimit = 999)
        {
            if (value <= fullLimit) return value.ToString(CultureInfo.InvariantCulture);
            if (value >= 1000000000) return (value / 1000000000d).ToString("0.#", CultureInfo.InvariantCulture) + "G";
            if (value >= 1000000) return (value / 1000000d).ToString("0.#", CultureInfo.InvariantCulture) + "M";
            return (value / 1000d).ToString("0.#", CultureInfo.InvariantCulture) + "k";
        }
    }
}
