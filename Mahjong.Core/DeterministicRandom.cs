using System;

namespace Mahjong.Core
{
    /// <summary>
    /// A seeded random generator (SplitMix64) that gives the same sequence on every runtime and
    /// .NET version, so the browser and the server deal identical boards from the same seed.
    /// </summary>
    public sealed class DeterministicRandom : Random
    {
        private ulong state;

        public DeterministicRandom(long seed)
            : base(0)
        {
            state = unchecked((ulong)seed);
        }

        public override int Next() => (int)(NextUInt64() >> 33);

        public override int Next(int maxValue)
        {
            if (maxValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxValue));
            }

            return maxValue == 0 ? 0 : (int)(NextUInt64() % (ulong)maxValue);
        }

        public override int Next(int minValue, int maxValue)
        {
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(minValue));
            }

            ulong range = (ulong)((long)maxValue - minValue);
            return range == 0 ? minValue : (int)(minValue + (long)(NextUInt64() % range));
        }

        public override double NextDouble() => Sample();

        public override void NextBytes(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)NextUInt64();
            }
        }

        protected override double Sample() => (NextUInt64() >> 11) * (1.0 / (1UL << 53));

        private ulong NextUInt64()
        {
            unchecked
            {
                ulong z = state += 0x9E3779B97F4A7C15UL;
                z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
                z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
                return z ^ (z >> 31);
            }
        }
    }
}
