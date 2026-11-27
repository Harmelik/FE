namespace FE.Core
{
    public enum BattleOutcomeType
    {
        None,
        PlayerVictory,
        PlayerDefeat
    }

    public enum BattleOutcomeReason
    {
        None,
        AllEnemiesDefeated,
        AllPlayersDefeated,
        BothSidesDefeated
    }

    public readonly struct BattleOutcomeResult
    {
        public BattleOutcomeType OutcomeType { get; }
        public BattleOutcomeReason Reason { get; }
        public bool IsBattleEnded =>
            OutcomeType != BattleOutcomeType.None;

        public BattleOutcomeResult(
            BattleOutcomeType outcomeType,
            BattleOutcomeReason reason)
        {
            bool invalidNone =
                outcomeType == BattleOutcomeType.None &&
                reason != BattleOutcomeReason.None;
            bool invalidEnded =
                outcomeType != BattleOutcomeType.None &&
                reason == BattleOutcomeReason.None;

            if (invalidNone || invalidEnded)
            {
                throw new System.ArgumentException(
                    "Battle outcome type and reason are inconsistent.");
            }

            OutcomeType = outcomeType;
            Reason = reason;
        }
    }
}
