namespace FE.Core
{
    /// <summary>
    /// Supplies deterministic random rolls for battle rules.
    /// </summary>
    public interface IBattleRandom
    {
        int RollPercent();
    }
}
