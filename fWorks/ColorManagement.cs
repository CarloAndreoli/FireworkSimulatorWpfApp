using System.Windows.Media;

namespace fWorks
{
    public static class FireworkColor
    {
        public static readonly Color FireworkRed = Color.FromRgb(255, 255, 67);
        public static readonly Color FireworkGreen = Color.FromRgb(20, 252, 86);
        public static readonly Color FireworkBlue = Color.FromRgb(30, 127, 255);
        public static readonly Color FireworkPurple = Color.FromRgb(230, 10, 255);
        public static readonly Color FireworkGold = Color.FromRgb(255, 191, 54);
        public static readonly Color FireworkWhite = Color.FromRgb(255, 255, 255);

        private static readonly Color[] Colors =
        {
            FireworkRed,
            FireworkGreen,
            FireworkBlue,
            FireworkPurple,
            FireworkGold,
            FireworkWhite
        };

        private static CryptoRandom Random => SimulationContext.Current.Random;

        public static Color GenerateRandomColor(bool limitWhite = false, Color? notColor = null)
        {
            Color color = Colors[Random.Next(0, Colors.Length)];

            if (limitWhite && color.Equals(FireworkWhite) && Random.NextDouble() < 0.6)
            {
                return GenerateRandomColor(limitWhite: true, notColor: notColor);
            }

            if (notColor.HasValue && color.Equals(notColor.Value))
            {
                return GenerateRandomColor(limitWhite: limitWhite, notColor: notColor);
            }

            return color;
        }

        public static Color GeneratePistilColor(Color shellColor)
        {
            return shellColor.Equals(FireworkWhite) || shellColor.Equals(FireworkGold)
                ? GenerateRandomColor(notColor: shellColor)
                : GenerateWhiteOrGoldColor();
        }

        public static Color GenerateWhiteOrGoldColor()
        {
            return Random.NextBool() ? FireworkGold : FireworkWhite;
        }
    }
}
