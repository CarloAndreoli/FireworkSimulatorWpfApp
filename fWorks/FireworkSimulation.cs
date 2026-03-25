using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace fWorks
{
    public sealed class FireworkSimulation
    {
        private readonly SimulationContext _context = SimulationContext.Current;
        private readonly SolidColorBrush _backgroundBrush;
        private readonly RadialGradientBrush _burstGradient;
        private readonly Pen _secondaryStarPen;

        public FireworkSimulation()
        {
            BurstFlashList = new List<BurstFlash>();
            SparkList = new List<Spark>();
            StarList = new List<Star>();

            _backgroundBrush = new SolidColorBrush(Color.FromArgb(70, 0, 0, 0));
            _backgroundBrush.Freeze();

            _burstGradient = new RadialGradientBrush();
            _burstGradient.GradientStops.Add(new GradientStop(Color.FromArgb(255, 255, 255, 255), 0.024));
            _burstGradient.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * 0.2), 255, 160, 20), 0.125));
            _burstGradient.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * 0.11), 255, 140, 20), 0.32));
            _burstGradient.GradientStops.Add(new GradientStop(Color.FromArgb(0, 255, 120, 20), 1));
            _burstGradient.Freeze();

            _secondaryStarPen = new Pen(new SolidColorBrush(Colors.White), 1);
            _secondaryStarPen.Freeze();
        }

        public List<BurstFlash> BurstFlashList { get; }
        public List<Spark> SparkList { get; }
        public List<Star> StarList { get; }

        public Firework AddNewFirework(FireworkType fireworkType, double shellSize = 2, double px = -1, double py = -1)
        {
            Firework firework = FireworkFactory.CreateShell(fireworkType, shellSize);
            if (px == -1)
            {
                px = MathHelpers.Random(0.2, 0.8);
            }

            if (py == -1)
            {
                py = MathHelpers.Random(0.5, 0.8);
            }

            firework.EVTAddBurst += burst => BurstFlashList.Add(burst);
            firework.EVTAddSpark += spark => SparkList.Add(spark);
            firework.EVTAddStar += star => StarList.Add(star);
            firework.Launch(px, py);
            return firework;
        }

        public void Update(double frameTime, double lag)
        {
            double timeStep = frameTime * _context.SimulationSpeed;
            double speed = _context.SimulationSpeed * lag;
            double starDrag = 1 - (1 - Star.AirDrag) * speed;
            double starDragHeavy = 1 - (1 - Star.AirDragHeavy) * speed;
            double sparkDrag = 1 - (1 - Spark.AirDrag) * speed;
            double gravityAcceleration = timeStep / 1000 * _context.Gravity;

            for (int i = StarList.Count - 1; i >= 0; i--)
            {
                Star star = StarList[i];
                star.Life -= timeStep;

                if (star.Life <= 0)
                {
                    StarList.RemoveAt(i);
                    star.RaiseOnDeath();
                    continue;
                }

                double burnRate = Math.Pow(star.Life / star.FullLife, 0.5);
                double burnRateInverse = 1 - burnRate;

                star.PreviousPosition = star.Position;
                star.Position = new Point(star.Position.X + star.SpeedX * speed, star.Position.Y + star.SpeedY * speed);

                if (star.Heavy)
                {
                    star.SpeedX *= starDragHeavy;
                    star.SpeedY *= starDragHeavy;
                }
                else
                {
                    star.SpeedX *= starDrag;
                    star.SpeedY *= starDrag;
                }

                star.SpeedY += gravityAcceleration;

                if (star.SpinRadius > 0)
                {
                    star.SpinAngle += star.SpinSpeed * speed;
                    star.Position = new Point(
                        star.Position.X + Math.Sin(star.SpinAngle) * star.SpinRadius * speed,
                        star.Position.Y + Math.Cos(star.SpinAngle) * star.SpinRadius * speed);
                }

                if (star.SparkFreq > 0)
                {
                    star.SparkTimer -= timeStep;
                    while (star.SparkTimer < 0)
                    {
                        star.SparkTimer += star.SparkFreq * 0.75 + star.SparkFreq * burnRateInverse * 4;
                        double newStarAngle = _context.Random.NextDouble() * _context.Pi2;
                        double newStarSpeed = _context.Random.NextDouble() * star.SparkSpeed * burnRate;
                        double newStarLife = star.SparkLife * 0.8 + _context.Random.NextDouble() * star.SparkLifeVariation * star.SparkLife;

                        Spark spark = new Spark
                        {
                            Position = star.Position,
                            PreviousPosition = star.Position,
                            SpeedX = Math.Sin(newStarAngle) * newStarSpeed,
                            SpeedY = Math.Cos(newStarAngle) * newStarSpeed,
                            Life = newStarLife
                        };
                        spark.PrimaryColor.Color = star.SparkColor.Color;
                        SparkList.Add(spark);
                    }
                }

                if (star.Life < star.TransitionTime)
                {
                    if (star.SecondaryColor != null && !star.ColorChanged)
                    {
                        star.ColorChanged = true;
                        if (star.SecondaryColor.Color != null)
                        {
                            star.PrimaryColor.Color = star.SecondaryColor.Color;
                        }
                        star.PrimaryColor.ColorType = ColorTypeEnum.None;

                        if (star.SecondaryColor.ColorType == ColorTypeEnum.Invisible)
                        {
                            star.SparkFreq = 0;
                        }
                    }

                    if (star.Strobe)
                    {
                        star.Visible = Math.Floor(star.Life / star.StrobeFreq) % 3 == 0;
                    }
                }
            }

            for (int i = SparkList.Count - 1; i >= 0; i--)
            {
                Spark spark = SparkList[i];
                spark.Life -= timeStep;
                if (spark.Life <= 0)
                {
                    SparkList.RemoveAt(i);
                    continue;
                }

                spark.PreviousPosition = spark.Position;
                spark.Position = new Point(spark.Position.X + spark.SpeedX * speed, spark.Position.Y + spark.SpeedY * speed);
                spark.SpeedX *= sparkDrag;
                spark.SpeedY *= sparkDrag;
                spark.SpeedY += gravityAcceleration;
            }
        }

        public DrawingVisual Render()
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawRectangle(_backgroundBrush, null, new Rect(0, 0, _context.StageWidth, _context.StageHeight));

                for (int i = BurstFlashList.Count - 1; i >= 0; i--)
                {
                    BurstFlash flash = BurstFlashList[i];
                    BurstFlashList.RemoveAt(i);
                    drawingContext.DrawRectangle(
                        _burstGradient,
                        null,
                        new Rect(flash.Position.X - flash.Radius, flash.Position.Y - flash.Radius, flash.Radius * 2, flash.Radius * 2));
                }

                foreach (Spark spark in SparkList)
                {
                    Pen pen = new Pen(new SolidColorBrush(spark.PrimaryColor.Color ?? Colors.White), Spark.DrawWidth)
                    {
                        StartLineCap = _context.IsHighQuality ? PenLineCap.Round : PenLineCap.Flat,
                        EndLineCap = _context.IsHighQuality ? PenLineCap.Round : PenLineCap.Flat
                    };
                    pen.Freeze();
                    drawingContext.DrawLine(pen, spark.Position, spark.PreviousPosition);
                }

                foreach (Star star in StarList)
                {
                    if (!star.Visible || star.PrimaryColor.ColorType == ColorTypeEnum.Invisible)
                    {
                        continue;
                    }

                    Pen pen = new Pen(new SolidColorBrush(star.PrimaryColor.Color ?? Colors.White), Star.DrawWidth)
                    {
                        StartLineCap = PenLineCap.Round,
                        EndLineCap = PenLineCap.Round
                    };
                    pen.Freeze();

                    drawingContext.DrawLine(pen, star.Position, star.PreviousPosition);
                    drawingContext.DrawLine(
                        _secondaryStarPen,
                        star.Position,
                        new Point(star.Position.X - star.SpeedX * 1.6, star.Position.Y - star.SpeedY * 1.6));
                }
            }

            return visual;
        }
    }
}
