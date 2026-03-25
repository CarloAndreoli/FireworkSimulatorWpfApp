using System;

namespace fWorks
{
    public static class MathHelpers
    {
        public static double PointDistance(double x1, double y1, double x2, double y2)
        {
            double dx = x2 - x1;
            double dy = y2 - y1;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        public static double PointAngle(double x1, double y1, double x2, double y2)
        {
            return SimulationContext.Current.PiHalf + Math.Atan2(y2 - y1, x2 - x1);
        }

        public static double Random(double min, double max)
        {
            return SimulationContext.Current.Random.NextDouble() * (max - min) + min;
        }
    }
}
