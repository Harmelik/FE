using System;

namespace FE.Core
{
    /// <summary>
    /// Calculates deterministic combat forecasts without changing battle state
    /// or consuming random numbers.
    /// </summary>
    public static class CombatForecastQuery
    {
        public static bool TryCalculate(
            BattleContext context,
            BattleUnit attacker,
            BattleUnit defender,
            GridCoord attackPosition,
            out CombatForecastResult result)
        {
            result = default;

            if (context == null ||
                attacker == null ||
                defender == null ||
                attacker.IsDead ||
                defender.IsDead ||
                ReferenceEquals(attacker, defender) ||
                attacker.Id == defender.Id ||
                attacker.Fac == defender.Fac ||
                !context.Map.IsInside(attackPosition) ||
                !context.Map.IsInside(defender.CurGrid))
            {
                return false;
            }

            int distance = CalculateDistance(
                attackPosition,
                defender.CurGrid);

            if (!attacker.BaseStats.CanAttackAtDistance(distance))
            {
                return false;
            }

            int attackerDamage = CalculateDamage(
                attacker.BaseStats.PAtk,
                defender.BaseStats.PhyDef);

            int attackerHitChance = CalculateHitChance(
                attacker.BaseStats.Hit,
                defender.BaseStats.Avoid);

            bool defenderCanAttack =
                defender.BaseStats.CanAttackAtDistance(distance);

            int defenderDamage = defenderCanAttack ? 
                CalculateDamage(defender.BaseStats.PAtk, attacker.BaseStats.PhyDef) : 0;

            int defenderHitChance = defenderCanAttack ? 
                CalculateHitChance(defender.BaseStats.Hit, attacker.BaseStats.Avoid) : 0;

            int defenderProjectedHp = Math.Max(0, defender.CurHP - attackerDamage);

            int attackerProjectedHp = defenderCanAttack 
                ? Math.Max(0, attacker.CurHP - defenderDamage)
                : attacker.CurHP;

            var attackerForecast = new CombatSideForecast(
                attacker.Id,
                attacker.CurHP,
                attacker.BaseStats.MaxHP,
                attackerDamage,
                attackerHitChance,
                attackerProjectedHp,
                true);

            var defenderForecast = new CombatSideForecast(
                defender.Id,
                defender.CurHP,
                defender.BaseStats.MaxHP,
                defenderDamage,
                defenderHitChance,
                defenderProjectedHp,
                defenderCanAttack);

            result = new CombatForecastResult(
                attacker.Id,
                defender.Id,
                distance,
                attackerForecast,
                defenderForecast);

            return true;
        }

        private static int CalculateDistance(
            GridCoord from,
            GridCoord to)
        {
            return Math.Abs(from.x - to.x) +
                   Math.Abs(from.y - to.y);
        }

        private static int CalculateDamage(
            int attack,
            int defense)
        {
            return Math.Max(0, attack - defense);
        }

        private static int CalculateHitChance(
            int hit,
            int avoid)
        {
            return Math.Max(0, Math.Min(100, hit - avoid));
        }
    }
}
