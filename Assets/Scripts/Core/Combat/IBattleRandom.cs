namespace FE.Core
{
    /// <summary>
    /// Supplies deterministic random rolls for battle rules.
    /// </summary>
    public interface IBattleRandom
    {
        int RollPercent();
        BattleRandomState CaptureState();
        void RestoreState(BattleRandomState state);
    }

    public readonly struct BattleRandomState
    {
        public uint Value { get; }

        public BattleRandomState(uint value)
        {
            if (value == 0)
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Random state cannot be zero.");
            }

            Value = value;
        }
    }
}
