using System;

namespace FE.Core
{
    /// <summary>
    /// One side's immutable values in a combat forecast.
    /// </summary>
    public readonly struct CombatSideForecast
    {
        public UnitId UnitId { get; }
        public int CurrentHp { get; }
        public int MaxHp { get; }
        public int Damage { get; }
        public int HitChance { get; }
        public int ProjectedHp { get; }
        public bool CanAttack { get; }

        public CombatSideForecast(
            UnitId unitId,
            int currentHp,
            int maxHp,
            int damage,
            int hitChance,
            int projectedHp,
            bool canAttack)
        {
            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHp),
                    maxHp,
                    "Max HP must be greater than zero.");
            }

            if (currentHp < 0 || currentHp > maxHp)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHp),
                    currentHp,
                    "Current HP must be within the valid HP range.");
            }

            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(damage),
                    damage,
                    "Damage cannot be negative.");
            }

            if (hitChance < 0 || hitChance > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hitChance),
                    hitChance,
                    "Hit chance must be between 0 and 100.");
            }

            if (projectedHp < 0 || projectedHp > maxHp)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(projectedHp),
                    projectedHp,
                    "Projected HP must be within the valid HP range.");
            }

            UnitId = unitId;
            CurrentHp = currentHp;
            MaxHp = maxHp;
            Damage = damage;
            HitChance = hitChance;
            ProjectedHp = projectedHp;
            CanAttack = canAttack;
        }

        public bool IsDefeatedAfterForecast => ProjectedHp <= 0;
    }

    /// <summary>
    /// Immutable combat forecast calculated for one attacker and one defender.
    /// </summary>
    public readonly struct CombatForecastResult
    {
        public UnitId AttackerId { get; }
        public UnitId DefenderId { get; }
        public int Distance { get; }
        public CombatSideForecast AttackerForecast { get; }
        public CombatSideForecast DefenderForecast { get; }
        public bool CanCounter => DefenderForecast.CanAttack;

        public CombatForecastResult(
            UnitId attackerId,
            UnitId defenderId,
            int distance,
            CombatSideForecast attackerForecast,
            CombatSideForecast defenderForecast)
        {
            if (attackerId == defenderId)
            {
                throw new ArgumentException(
                    "Attacker and defender must be different units.");
            }

            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distance),
                    distance,
                    "Combat distance cannot be negative.");
            }

            if (attackerForecast.UnitId != attackerId ||
                defenderForecast.UnitId != defenderId)
            {
                throw new ArgumentException(
                    "Forecast unit IDs must match the combat participants.");
            }

            AttackerId = attackerId;
            DefenderId = defenderId;
            Distance = distance;
            AttackerForecast = attackerForecast;
            DefenderForecast = defenderForecast;
        }
    }
}
