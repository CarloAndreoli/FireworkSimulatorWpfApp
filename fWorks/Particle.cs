using System;
using System.Windows;
using System.Windows.Media;

namespace fWorks
{
    public enum GlitterType
    {
        None,
        Light,
        Medium,
        Heavy,
        Thick,
        Streamer,
        Willow
    }

    public enum ColorTypeEnum
    {
        None,
        Invisible,
        Random
    }

    public class Particle
    {
        public Point Position = new Point();
        public Point PreviousPosition = new Point();
        public SpColor PrimaryColor = new SpColor();
        public double SpeedX;
        public double SpeedY;
        public double Life;
    }

    public sealed class Star : Particle
    {
        public static double DrawWidth = 3;
        public static double AirDrag = 0.98;
        public static double AirDragHeavy = 0.992;

        public bool Visible = true;
        public bool Heavy;
        public SpColor SecondaryColor = new SpColor();
        public bool ColorChanged;
        public double TransitionTime;
        public double FullLife;
        public double SpinAngle;
        public double SpinSpeed;
        public double SpinRadius;
        public double SparkFreq;
        public double SparkSpeed;
        public double SparkTimer;
        public SpColor SparkColor = new SpColor();
        public double SparkLife;
        public double SparkLifeVariation;
        public bool Strobe;
        public double StrobeFreq;

        public delegate void DLTOnDeath(Star sender);
        public DLTOnDeath EVTOnDeath;

        public void RaiseOnDeath()
        {
            EVTOnDeath?.Invoke(this);
        }
    }

    public sealed class Spark : Particle
    {
        public static double DrawWidth = 0.75;
        public static double AirDrag = 0.9;
    }

    public sealed class SpColor : ICloneable
    {
        public SpColor(Color? color = null, ColorTypeEnum colorType = ColorTypeEnum.None)
        {
            Color = color;
            ColorType = colorType;
        }

        public Color? Color;
        public ColorTypeEnum ColorType;

        public object Clone()
        {
            return MemberwiseClone();
        }
    }

    public sealed class BurstFlash
    {
        public Point Position = new Point();
        public double Radius = 8;
    }
}
