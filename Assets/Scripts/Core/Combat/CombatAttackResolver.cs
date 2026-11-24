using System;

namespace FE.Core
{
    /// <summary>
    /// Resolves an already validated combat exchange and applies its results.
    /// </summary>
    public static class CombatAttackResolver
    {
        public static CombatAttackResult Resolve(
            BattleContext context,
            BattleUnit attacker,
            BattleUnit defender,
            CombatForecastResult forecast)
        {
            if (context == null ||
                attacker == null ||
                defender == null ||
                context.BattleRandom == null)
            {
                throw new ArgumentNullException(
                    nameof(context),
                    "Combat context, participants, and random source are required.");
            }

            if (forecast.AttackerId != attacker.Id ||
                forecast.DefenderId != defender.Id)
            {
                throw new ArgumentException(
                    "Forecast participants do not match combat participants.");
            }

            int attackerRoll = context.BattleRandom.RollPercent();
            bool attackerHit =
                attackerRoll < forecast.AttackerForecast.HitChance;

            int attackerDamage = 0;
            if (attackerHit)
            {
                attackerDamage = defender.ApplyDamage(
                    forecast.AttackerForecast.Damage);
            }

            bool countered =
                !defender.IsDead &&
                forecast.DefenderForecast.CanAttack;

            int defenderRoll = -1;
            int defenderDamage = 0;
            bool defenderHit = false;
            int defenderHitChance = 0;

            if (countered)
            {
                defenderHitChance =
                    forecast.DefenderForecast.HitChance;
                defenderRoll = context.BattleRandom.RollPercent();
                defenderHit =
                    defenderRoll < defenderHitChance;

                if (defenderHit)
                {
                    defenderDamage = attacker.ApplyDamage(
                        forecast.DefenderForecast.Damage);
                }
            }

            if (defender.IsDead)
            {
                context.Units.RemoveFromBoard(defender);
            }

            if (attacker.IsDead)
            {
                context.Units.RemoveFromBoard(attacker);
            }

            return new CombatAttackResult(
                attacker.Id,
                defender.Id,
                forecast.AttackerForecast.HitChance,
                attackerRoll,
                attackerHit,
                attackerDamage,
                countered,
                defenderHitChance,
                defenderRoll,
                defenderHit,
                defenderDamage,
                attacker.CurHP,
                defender.CurHP,
                attacker.CurGrid,
                defender.CurGrid);
        }
    }
}
