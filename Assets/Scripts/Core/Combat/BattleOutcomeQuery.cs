namespace FE.Core
{
    public static class BattleOutcomeQuery
    {
        public static bool TryEvaluate(
            BattleContext context,
            out BattleOutcomeResult result)
        {
            result = default;

            if (context == null)
            {
                return false;
            }

            int playersAlive = 0;
            int enemiesAlive = 0;

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                if (unit.IsDead)
                {
                    continue;
                }

                if (unit.Fac == Faction.Player)
                {
                    playersAlive++;
                }
                else if (unit.Fac == Faction.Enemy)
                {
                    enemiesAlive++;
                }
            }

            if (playersAlive == 0 && enemiesAlive == 0)
            {
                result = new BattleOutcomeResult(
                    BattleOutcomeType.PlayerDefeat,
                    BattleOutcomeReason.BothSidesDefeated);
                return true;
            }

            if (enemiesAlive == 0)
            {
                result = new BattleOutcomeResult(
                    BattleOutcomeType.PlayerVictory,
                    BattleOutcomeReason.AllEnemiesDefeated);
                return true;
            }

            if (playersAlive == 0)
            {
                result = new BattleOutcomeResult(
                    BattleOutcomeType.PlayerDefeat,
                    BattleOutcomeReason.AllPlayersDefeated);
                return true;
            }

            return false;
        }
    }
}
