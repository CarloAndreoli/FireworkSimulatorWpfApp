using System;
using System.Security.Cryptography;

namespace fWorks
{
    public sealed class CryptoRandom : IDisposable
    {
        private readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

        public double NextDouble()
        {
            byte[] buffer = new byte[8];
            _rng.GetBytes(buffer);
            ulong value = BitConverter.ToUInt64(buffer, 0);
            return value / (double)ulong.MaxValue;
        }

        public int Next()
        {
            return Next(0, int.MaxValue);
        }

        public int Next(int maxValue)
        {
            return Next(0, maxValue);
        }

        public int Next(int minValue, int maxValue)
        {
            if (maxValue <= minValue)
            {
                return minValue;
            }

            return minValue + (int)Math.Floor(NextDouble() * (maxValue - minValue));
        }

        public bool NextBool(double trueProbability = 0.5)
        {
            return NextDouble() < trueProbability;
        }

        public void Dispose()
        {
            _rng.Dispose();
        }
    }
}
