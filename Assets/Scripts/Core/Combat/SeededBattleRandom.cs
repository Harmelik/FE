using System;

namespace FE.Core
{
    /// <summary>
    /// Small deterministic percent random source that can be recreated from a seed.
    /// </summary>
    public sealed class SeededBattleRandom : IBattleRandom
    {
        private const uint ZeroSeedFallback = 0x9E3779B9u;

        private uint state;

        public int Seed { get; }

        public SeededBattleRandom(int seed)
        {
            Seed = seed;
            state = seed == 0
                ? ZeroSeedFallback
                : unchecked((uint)seed);
        }

        public int RollPercent()
        {
            uint value = state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            state = value;

            return (int)(value % 100u);
        }
    }
}
