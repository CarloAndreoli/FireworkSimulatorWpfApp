using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace fWorks
{
    public enum FireworkType
    {
        Crackle,
        Crossette,
        Crysanthemum,
        FallingLeaves,
        Floral,
        Ghost,
        Horsetail,
        Palm,
        Ring,
        Strobe,
        Willow
    }

    public sealed class Firework
    {
        private readonly SimulationContext _context = SimulationContext.Current;
        private Star _comet;

        public delegate void DLTAddStar(Star star);
        public DLTAddStar EVTAddStar;

        public delegate void DLTAddSpark(Spark spark);
        public DLTAddSpark EVTAddSpark;

        public delegate void DLTAddBurst(BurstFlash burst);
        public DLTAddBurst EVTAddBurst;

        public double _SpreadSize;
        public int _StarCount;
        public double _StarLife;
        public double _StarLifeVariation;
        public double _StarDensity;

        public Color? _color;
        public ColorTypeEnum _colorS = ColorTypeEnum.None;
        public Color? _colorB;
        public Color? _secondColor;
        public Color _glitterColor;
        public GlitterType _GlitterType = GlitterType.None;

        public bool _Strobe;
        public double _StrobeFreq;
        public Color? _StrobeColor;

        public bool _Pistil;
        public Color? _PistilColor;

        public FireworkType _FireworkType;
        public bool _Streamers;
        public bool _Ring;

        public void Init()
        {
            if (_StarLifeVariation == 0)
            {
                _StarLifeVariation = 0.125;
            }

            if (_color == null)
            {
                _color = FireworkColor.GenerateRandomColor();
            }

            if (_glitterColor == default)
            {
                _glitterColor = _color ?? FireworkColor.FireworkWhite;
            }

            if (_StarCount == 0)
            {
                double density = _StarDensity > 0 ? _StarDensity : 1;
                double scaledSize = _SpreadSize / 54;
                _StarCount = (int)Math.Max(6, scaledSize * scaledSize * density);
            }
        }

        public void Launch(double position, double launchHeight)
        {
            double width = _context.StageWidth;
            double height = _context.StageHeight;
            double hpad = 60;
            double vpad = 100;
            double minHeightPercent = 0.45;
            double minHeight = height - height * minHeightPercent;

            double launchX = position * (width - hpad * 2) + hpad;
            double launchY = height;
            double burstY = minHeight - (launchHeight * (minHeight - vpad));

            double launchDistance = launchY - burstY;
            double launchVelocity = Math.Pow(launchDistance * 0.04, 0.64);

            _comet = AddStar(
                launchX,
                launchY,
                _colorS != ColorTypeEnum.Random ? _color : FireworkColor.FireworkWhite,
                Math.PI,
                launchVelocity * (_FireworkType == FireworkType.Horsetail ? 1.2 : 1),
                launchVelocity * (_FireworkType == FireworkType.Horsetail ? 100 : 400));

            _comet.Heavy = true;
            _comet.SpinRadius = MathHelpers.Random(0.32, 0.85);
            _comet.SparkFreq = (int)(32.0 / _context.Quality);
            if (_context.IsHighQuality)
            {
                _comet.SparkFreq = 8;
            }

            _comet.SparkLife = 320;
            _comet.SparkLifeVariation = 3;

            if (_GlitterType == GlitterType.Willow || _FireworkType == FireworkType.FallingLeaves)
            {
                _comet.SparkFreq = (int)(20.0 / _context.Quality);
                _comet.SparkSpeed = 0.5;
                _comet.SparkLife = 500;
            }

            if (_colorS == ColorTypeEnum.Invisible)
            {
                _comet.SparkColor.Color = FireworkColor.FireworkGold;
            }

            if (_context.Random.NextDouble() > 0.4 && _FireworkType != FireworkType.Horsetail)
            {
                _comet.SecondaryColor.ColorType = ColorTypeEnum.Invisible;
                _comet.TransitionTime = Math.Pow(_context.Random.NextDouble(), 1.5) * 700 + 500;
            }

            _comet.EVTOnDeath += Burst;
        }

        public void Burst(Star stp)
        {
            Burst(stp.Position.X, stp.Position.Y);
        }

        public void Burst(double x, double y)
        {
            double speed = _SpreadSize / 96;
            Color? color;
            double sparkFreq = 200;
            double sparkSpeed = 0.44;
            double sparkLife = 700;
            double sparkLifeVariation = 2;

            switch (_GlitterType)
            {
                case GlitterType.Light:
                    sparkFreq = 400;
                    sparkSpeed = 0.3;
                    sparkLife = 300;
                    sparkLifeVariation = 2;
                    break;
                case GlitterType.Medium:
                    sparkFreq = 200;
                    sparkSpeed = 0.44;
                    sparkLife = 700;
                    sparkLifeVariation = 2;
                    break;
                case GlitterType.Heavy:
                    sparkFreq = 80;
                    sparkSpeed = 0.8;
                    sparkLife = 1400;
                    sparkLifeVariation = 2;
                    break;
                case GlitterType.Thick:
                    sparkFreq = 16;
                    sparkSpeed = _context.IsHighQuality ? 1.65 : 1.5;
                    sparkLife = 1400;
                    sparkLifeVariation = 3;
                    break;
                case GlitterType.Streamer:
                    sparkFreq = 32;
                    sparkSpeed = 1.05;
                    sparkLife = 620;
                    sparkLifeVariation = 2;
                    break;
                case GlitterType.Willow:
                    sparkFreq = 10;
                    sparkSpeed = 0.20;
                    sparkLife = 1000;
                    sparkLifeVariation = 3.8;
                    break;
            }

            sparkFreq /= _context.Quality;

            if (_colorB == null)
            {
                color = _colorS == ColorTypeEnum.Random ? null : _color;

                if (_Ring)
                {
                    double ringStartAngle = _context.Random.NextDouble() * Math.PI;
                    double ringSquash = Math.Pow(_context.Random.NextDouble(), 2) * 0.85 + 0.15;
                    foreach (double angle in CreateParticleArc(0, _context.Pi2, _StarCount, 0))
                    {
                        double initSpeedX = Math.Sin(angle) * speed * ringSquash;
                        double initSpeedY = Math.Cos(angle) * speed;
                        double newSpeed = MathHelpers.PointDistance(0, 0, initSpeedX, initSpeedY);
                        double newAngle = MathHelpers.PointAngle(0, 0, initSpeedX, initSpeedY) + ringStartAngle;

                        Star star = AddStar(
                            x,
                            y,
                            color,
                            newAngle,
                            newSpeed,
                            _StarLife + _context.Random.NextDouble() * _StarLife * _StarLifeVariation);

                        AssociateDeath(star);
                        ApplyGlitter(star, sparkFreq, sparkSpeed, sparkLife, sparkLifeVariation);
                    }
                }
                else
                {
                    foreach (KeyValuePair<double, double> burst in CreateBurst(_StarCount))
                    {
                        StarFactory(x, y, speed, _comet, sparkFreq, sparkSpeed, sparkLife, sparkLifeVariation, burst.Key, burst.Value, color);
                    }
                }
            }
            else
            {
                List<KeyValuePair<double, double>> burst = CreateBurst(_StarCount);
                for (int i = 0; i < burst.Count; i++)
                {
                    Color? alternatingColor = i % 2 == 0 ? _color : _colorB;
                    StarFactory(x, y, speed, _comet, sparkFreq, sparkSpeed, sparkLife, sparkLifeVariation, burst[i].Key, burst[i].Value, alternatingColor);
                }
            }

            if (_Pistil)
            {
                Firework pistilShell = FireworkFactory.CreateShell(FireworkType.Crysanthemum, 1.2);
                pistilShell._SpreadSize = _SpreadSize * 0.5;
                pistilShell._StarLife = _StarLife * 0.6;
                pistilShell._StarLifeVariation = _StarLifeVariation;
                pistilShell._StarDensity = 1.4;
                pistilShell._color = _PistilColor;
                pistilShell._GlitterType = GlitterType.None;
                WireChildShellEvents(pistilShell);
                pistilShell.Burst(x, y);
            }

            if (_Streamers)
            {
                Firework streamerShell = FireworkFactory.CreateShell(FireworkType.Crysanthemum, 1.35);
                streamerShell._SpreadSize = _SpreadSize * 0.9;
                streamerShell._StarLife = _StarLife * 0.8;
                streamerShell._StarLifeVariation = _StarLifeVariation;
                streamerShell._StarCount = Math.Max(6, (int)(_StarCount * 0.4));
                streamerShell._color = FireworkColor.FireworkWhite;
                streamerShell._GlitterType = GlitterType.Streamer;
                streamerShell._glitterColor = FireworkColor.FireworkWhite;
                WireChildShellEvents(streamerShell);
                streamerShell.Burst(x, y);
            }

            AddBurst(x, y, 46);
        }

        private void WireChildShellEvents(Firework shell)
        {
            shell.EVTAddStar += star => EVTAddStar?.Invoke(star);
            shell.EVTAddSpark += spark => EVTAddSpark?.Invoke(spark);
            shell.EVTAddBurst += burst => EVTAddBurst?.Invoke(burst);
        }

        private void AssociateDeath(Star star)
        {
            switch (_FireworkType)
            {
                case FireworkType.Crossette:
                    star.EVTOnDeath += CrossetteEffect;
                    break;
                case FireworkType.Floral:
                    star.EVTOnDeath += FloralEffect;
                    break;
                case FireworkType.FallingLeaves:
                    star.EVTOnDeath += FallingLeavesEffect;
                    break;
                case FireworkType.Crackle:
                    star.EVTOnDeath += CrackleEffect;
                    break;
            }
        }

        private void CrossetteEffect(Star star)
        {
            double startAngle = _context.Random.NextDouble() * _context.PiHalf;
            foreach (double angle in CreateParticleArc(startAngle, _context.Pi2, 4, 0.5))
            {
                Star star2 = AddStar(
                    star.Position.X,
                    star.Position.Y,
                    star.PrimaryColor.Color,
                    angle,
                    MathHelpers.Random(0.75, 1.35),
                    600 + _context.Random.NextDouble() * 150,
                    star.SpeedX,
                    star.SpeedY);

                star2.Visible = true;
                star2.SparkFreq = 0;
            }

            AddBurst(star.Position.X, star.Position.Y, 46);
        }

        private void FloralEffect(Star star)
        {
            foreach (KeyValuePair<double, double> burst in CreateBurst(12))
            {
                Star star2 = AddStar(
                    star.Position.X,
                    star.Position.Y,
                    null,
                    burst.Key,
                    burst.Value * 2.4,
                    1000 + _context.Random.NextDouble() * 300,
                    star.SpeedX,
                    star.SpeedY);

                star2.SparkFreq = 0;
                star2.SecondaryColor.ColorType = ColorTypeEnum.Invisible;
            }

            AddBurst(star.Position.X, star.Position.Y, 46);
        }

        private void FallingLeavesEffect(Star star)
        {
            foreach (KeyValuePair<double, double> burst in CreateBurst(7))
            {
                Star star2 = AddStar(
                    star.Position.X,
                    star.Position.Y,
                    null,
                    burst.Key,
                    burst.Value * 2.4,
                    2400 + _context.Random.NextDouble() * 600,
                    star.SpeedX,
                    star.SpeedY);

                star2.PrimaryColor.ColorType = ColorTypeEnum.Invisible;
                star2.SparkColor.Color = FireworkColor.FireworkGold;
                star2.SparkFreq = 144 / _context.Quality;
                star2.SparkSpeed = 0.28;
                star2.SparkLife = 750;
                star2.SparkLifeVariation = 3.2;
            }

            AddBurst(star.Position.X, star.Position.Y, 46);
        }

        private void CrackleEffect(Star star)
        {
            double count = _context.IsHighQuality ? 32 : 16;
            foreach (double angle in CreateParticleArc(0, _context.Pi2, count, 1.8))
            {
                AddSpark(
                    star.Position.X,
                    star.Position.Y,
                    FireworkColor.FireworkGold,
                    angle,
                    Math.Pow(_context.Random.NextDouble(), 0.45) * 2.4,
                    300 + _context.Random.NextDouble() * 200);
            }
        }

        private void StarFactory(
            double x,
            double y,
            double speed,
            Star comet,
            double sparkFreq,
            double sparkSpeed,
            double sparkLife,
            double sparkLifeVariation,
            double angle,
            double speedMult,
            Color? colorOpt = null)
        {
            double standardInitialSpeed = _SpreadSize / 1800;

            Star star = AddStar(
                x,
                y,
                colorOpt ?? FireworkColor.GenerateRandomColor(),
                angle,
                speedMult * speed,
                _StarLife + _context.Random.NextDouble() * _StarLife * _StarLifeVariation,
                _FireworkType == FireworkType.Horsetail ? comet?.SpeedX ?? 0 : 0,
                _FireworkType == FireworkType.Horsetail ? comet?.SpeedY ?? -standardInitialSpeed : 0);

            star.PrimaryColor.ColorType = _colorS;

            if (_secondColor != null)
            {
                star.TransitionTime = _StarLife * (_context.Random.NextDouble() * 0.05 + 0.32);
                star.SecondaryColor.Color = _secondColor;
            }

            if (_Strobe)
            {
                star.TransitionTime = _StarLife * (_context.Random.NextDouble() * 0.08 + 0.46);
                star.Strobe = true;
                star.StrobeFreq = _context.Random.NextDouble() * 20 + 40;
                if (_StrobeColor != null)
                {
                    star.SecondaryColor.Color = _StrobeColor;
                }
            }

            ApplyGlitter(star, sparkFreq, sparkSpeed, sparkLife, sparkLifeVariation);
            AssociateDeath(star);
        }

        private void ApplyGlitter(Star star, double sparkFreq, double sparkSpeed, double sparkLife, double sparkLifeVariation)
        {
            if (_GlitterType == GlitterType.None)
            {
                return;
            }

            star.SparkFreq = sparkFreq;
            star.SparkSpeed = sparkSpeed;
            star.SparkLife = sparkLife;
            star.SparkLifeVariation = sparkLifeVariation;
            star.SparkColor.Color = _glitterColor;
            star.SparkTimer = _context.Random.NextDouble() * star.SparkFreq;
        }

        private static List<double> CreateParticleArc(double start, double arcLength, double count, double randomness)
        {
            SimulationContext context = SimulationContext.Current;
            List<double> values = new List<double>();
            double angleDelta = arcLength / count;
            double end = start + arcLength - (angleDelta * 0.5);

            if (end > start)
            {
                for (double angle = start; angle < end; angle += angleDelta)
                {
                    values.Add(angle + context.Random.NextDouble() * angleDelta * randomness);
                }
            }
            else
            {
                for (double angle = start; angle > end; angle += angleDelta)
                {
                    values.Add(angle + context.Random.NextDouble() * angleDelta * randomness);
                }
            }

            return values;
        }

        private static List<KeyValuePair<double, double>> CreateBurst(double count, double startAngle = 0, double arcLength = Math.PI * 2)
        {
            SimulationContext context = SimulationContext.Current;
            List<KeyValuePair<double, double>> retList = new List<KeyValuePair<double, double>>();
            double radius = 0.5 * Math.Sqrt(count / Math.PI);
            double circumference = 2 * radius * Math.PI;
            double halfCircumference = circumference / 2;

            for (int i = 0; i <= halfCircumference; i++)
            {
                double ringAngle = i / halfCircumference * context.PiHalf;
                double ringSize = Math.Cos(ringAngle);
                double partsPerFullRing = circumference * ringSize;
                double partsPerArc = partsPerFullRing * (arcLength / context.Pi2);
                double angleInc = context.Pi2 / partsPerFullRing;
                double angleOffset = context.Random.NextDouble() * angleInc + startAngle;
                double maxRandomAngleOffset = angleInc * 0.33;

                for (double i2 = 0; i2 < partsPerArc; i2++)
                {
                    double randomAngleOffset = context.Random.NextDouble() * maxRandomAngleOffset;
                    double angle = angleInc * i2 + angleOffset + randomAngleOffset;
                    retList.Add(new KeyValuePair<double, double>(angle, ringSize));
                }
            }

            return retList;
        }

        private BurstFlash AddBurst(double x, double y, double radius)
        {
            BurstFlash burstFlash = new BurstFlash
            {
                Position = new Point(x, y),
                Radius = radius
            };

            EVTAddBurst?.Invoke(burstFlash);
            return burstFlash;
        }

        private Spark AddSpark(double x, double y, Color? color, double angle, double speed, double life)
        {
            Spark spark = new Spark
            {
                Position = new Point(x, y),
                PreviousPosition = new Point(x, y),
                SpeedX = Math.Sin(angle) * speed,
                SpeedY = Math.Cos(angle) * speed,
                Life = life
            };
            spark.PrimaryColor.Color = color;

            EVTAddSpark?.Invoke(spark);
            return spark;
        }

        private Star AddStar(double x, double y, Color? color, double angle, double speed, double life, double speedOffX = 0, double speedOffY = 0)
        {
            Star star = new Star
            {
                Visible = true,
                Heavy = false,
                Position = new Point(x, y),
                PreviousPosition = new Point(x, y),
                SpeedX = Math.Sin(angle) * speed + speedOffX,
                SpeedY = Math.Cos(angle) * speed + speedOffY,
                Life = life,
                FullLife = life,
                SpinAngle = _context.Random.NextDouble() * _context.Pi2,
                SpinSpeed = 0.8,
                SpinRadius = 0,
                SparkFreq = 0,
                SparkSpeed = 1,
                SparkTimer = 0,
                SparkLife = 750,
                SparkLifeVariation = 0.25,
                Strobe = false
            };

            star.PrimaryColor.Color = color;
            star.SparkColor.Color = color;

            EVTAddStar?.Invoke(star);
            return star;
        }
    }
}
