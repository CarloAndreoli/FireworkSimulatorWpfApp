using System;

namespace fWorks
{
    public sealed class SimulationContext
    {
        public static SimulationContext Current { get; } = new SimulationContext();

        private SimulationContext()
        {
            Random = new CryptoRandom();
        }

        public double StageWidth { get; set; }
        public double StageHeight { get; set; }
        public double SimulationSpeed { get; set; } = 1.0;
        public double Gravity { get; set; } = 0.8;
        public double Quality { get; set; } = 1.0;
        public bool IsHighQuality { get; set; } = true;
        public CryptoRandom Random { get; }

        public double Pi2 => Math.PI * 2.0;
        public double PiHalf => Math.PI / 2.0;
    }
}
