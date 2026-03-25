using System.Windows.Media;

namespace fWorks
{
    public static class FireworkFactory
    {
        private static SimulationContext Context => SimulationContext.Current;

        public static Firework CreateShell(FireworkType fireworkType, double size)
        {
            Firework shell = new Firework();

            switch (fireworkType)
            {
                case FireworkType.Crysanthemum:
                {
                    bool glitter = Context.Random.NextDouble() < 0.25;
                    bool singleColor = Context.Random.NextDouble() < 0.72;
                    Color color = FireworkColor.GenerateRandomColor(limitWhite: true);
                    Color? colorB = null;

                    if (!singleColor)
                    {
                        color = FireworkColor.GenerateRandomColor();
                        colorB = FireworkColor.GenerateRandomColor(notColor: color);
                    }

                    bool pistil = singleColor && Context.Random.NextDouble() < 0.42;
                    Color? pistilColor = pistil ? FireworkColor.GeneratePistilColor(color) : null;
                    Color? secondColor = null;

                    if (singleColor && (Context.Random.NextDouble() < 0.2 || color.Equals(FireworkColor.FireworkWhite)))
                    {
                        secondColor = pistilColor ?? FireworkColor.GenerateRandomColor(notColor: color, limitWhite: true);
                    }

                    bool streamers = !pistil && !color.Equals(FireworkColor.FireworkWhite) && Context.Random.NextDouble() < 0.42;
                    double starDensity = glitter ? 1.1 : 1.25;
                    starDensity = Context.IsHighQuality ? 1.2 : starDensity * 0.8;

                    shell._SpreadSize = 300 + size * 100;
                    shell._StarLife = 900 + size * 200;
                    shell._StarDensity = starDensity;
                    shell._color = color;
                    shell._colorB = colorB;
                    shell._secondColor = secondColor;
                    shell._GlitterType = glitter ? GlitterType.Light : GlitterType.None;
                    shell._glitterColor = FireworkColor.GenerateWhiteOrGoldColor();
                    shell._Pistil = pistil;
                    shell._PistilColor = pistilColor;
                    shell._Streamers = streamers;
                    break;
                }
                case FireworkType.Ghost:
                {
                    shell = CreateShell(FireworkType.Crysanthemum, size);
                    shell._StarLife *= 1.5;
                    Color ghostColor = FireworkColor.GenerateRandomColor(notColor: FireworkColor.FireworkWhite);
                    shell._Streamers = true;
                    shell._Pistil = Context.Random.NextDouble() < 0.42;
                    shell._PistilColor = shell._Pistil ? FireworkColor.GeneratePistilColor(ghostColor) : null;
                    shell._colorS = ColorTypeEnum.Invisible;
                    shell._secondColor = ghostColor;
                    shell._GlitterType = GlitterType.None;
                    break;
                }
                case FireworkType.Strobe:
                {
                    shell._SpreadSize = 280 + size * 92;
                    shell._StarLife = 1100 + size * 200;
                    shell._StarLifeVariation = 0.40;
                    shell._StarDensity = 1.1;
                    shell._color = FireworkColor.GenerateRandomColor(limitWhite: true);
                    shell._GlitterType = GlitterType.Light;
                    shell._glitterColor = FireworkColor.FireworkWhite;
                    shell._Strobe = true;
                    shell._StrobeColor = Context.Random.NextDouble() < 0.5 ? FireworkColor.FireworkWhite : (Color?)null;
                    shell._Pistil = Context.Random.NextDouble() < 0.5;
                    shell._PistilColor = FireworkColor.GeneratePistilColor(shell._color.Value);
                    break;
                }
                case FireworkType.Palm:
                {
                    bool thick = Context.Random.NextDouble() < 0.5;
                    shell._color = FireworkColor.GenerateRandomColor();
                    shell._SpreadSize = 250 + size * 75;
                    shell._StarDensity = thick ? 0.15 : 0.4;
                    shell._StarLife = 1800 + size * 200;
                    shell._GlitterType = thick ? GlitterType.Thick : GlitterType.Heavy;
                    shell._glitterColor = shell._color.Value;
                    break;
                }
                case FireworkType.Ring:
                {
                    shell._Ring = true;
                    shell._color = FireworkColor.GenerateRandomColor();
                    shell._SpreadSize = 300 + size * 100;
                    shell._StarLife = 900 + size * 200;
                    shell._StarCount = (int)(2.2 * Context.Pi2 * (size + 1));
                    shell._Pistil = Context.Random.NextDouble() < 0.75;
                    shell._PistilColor = FireworkColor.GeneratePistilColor(shell._color.Value);
                    shell._GlitterType = !shell._Pistil ? GlitterType.Light : GlitterType.None;
                    shell._glitterColor = shell._color.Equals(FireworkColor.FireworkGold) ? FireworkColor.FireworkGold : FireworkColor.FireworkWhite;
                    shell._Streamers = Context.Random.NextDouble() < 0.3;
                    break;
                }
                case FireworkType.Crossette:
                {
                    shell._SpreadSize = 300 + size * 100;
                    shell._StarLife = 750 + size * 160;
                    shell._StarLifeVariation = 0.4;
                    shell._StarDensity = 0.85;
                    shell._color = FireworkColor.GenerateRandomColor(limitWhite: true);
                    shell._Pistil = Context.Random.NextDouble() < 0.5;
                    shell._PistilColor = FireworkColor.GeneratePistilColor(shell._color.Value);
                    break;
                }
                case FireworkType.Floral:
                {
                    shell._SpreadSize = 300 + size * 120;
                    shell._StarDensity = 0.12;
                    shell._StarLife = 500 + size * 50;
                    shell._StarLifeVariation = 0.5;
                    if (Context.Random.NextDouble() < 0.65)
                    {
                        shell._colorS = ColorTypeEnum.Random;
                    }
                    else if (Context.Random.NextDouble() < 0.15)
                    {
                        shell._color = FireworkColor.GenerateRandomColor();
                    }
                    else
                    {
                        shell._color = FireworkColor.GenerateRandomColor();
                        shell._colorB = FireworkColor.GenerateRandomColor(notColor: shell._color);
                    }
                    break;
                }
                case FireworkType.FallingLeaves:
                {
                    shell._colorS = ColorTypeEnum.Invisible;
                    shell._SpreadSize = 300 + size * 120;
                    shell._StarDensity = 0.12;
                    shell._StarLife = 500 + size * 50;
                    shell._StarLifeVariation = 0.5;
                    shell._GlitterType = GlitterType.Willow;
                    shell._glitterColor = FireworkColor.FireworkGold;

					break;
                }
                case FireworkType.Willow:
                {
                    shell._SpreadSize = 300 + size * 100;
                    shell._StarDensity = 0.6;
                    shell._StarLife = 3000 + size * 300;
                    shell._GlitterType = GlitterType.Willow;
                    shell._glitterColor = FireworkColor.FireworkGold;
                    shell._colorS = ColorTypeEnum.Invisible;
                    break;
                }
                case FireworkType.Crackle:
                {
                    shell._SpreadSize = 380 + size * 75;
                    shell._StarDensity = Context.IsHighQuality ? 1 : 0.65;
                    shell._StarLife = 600 + size * 100;
                    shell._StarLifeVariation = 0.32;
                    shell._GlitterType = GlitterType.Light;
                    shell._glitterColor = FireworkColor.FireworkGold;
                    shell._color = Context.Random.NextDouble() < 0.75 ? FireworkColor.FireworkGold : FireworkColor.GenerateRandomColor();
                    shell._Pistil = Context.Random.NextDouble() < 0.65;
                    shell._PistilColor = FireworkColor.GeneratePistilColor(shell._color.Value);
                    break;
                }
                case FireworkType.Horsetail:
                {
                    shell._color = FireworkColor.GenerateRandomColor();
                    shell._SpreadSize = 250 + size * 38;
                    shell._StarDensity = 0.9;
                    shell._StarLife = 2500 + size * 300;
                    shell._GlitterType = GlitterType.Medium;
                    shell._glitterColor = Context.Random.NextDouble() < 0.5 ? FireworkColor.GenerateWhiteOrGoldColor() : shell._color.Value;
                    shell._Strobe = shell._color.Equals(FireworkColor.FireworkWhite);
                    break;
                }
            }

            shell._FireworkType = fireworkType;
            shell.Init();
            return shell;
        }
    }
}
