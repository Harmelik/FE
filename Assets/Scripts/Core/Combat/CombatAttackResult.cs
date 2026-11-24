using System;

namespace FE.Core
{
    /// <summary>
    /// Values produced after one attack and an optional counterattack.
    /// </summary>
    public readonly struct CombatAttackResult
    {
        public UnitId AttackerId { get; }
        public UnitId DefenderId { get; }
        public int AttackerHitChance { get; }
        public int AttackerRoll { get; }
        public bool AttackerHit { get; }
        public int AttackerDamage { get; }
        public bool Countered { get; }
        public int DefenderHitChance { get; }
        public int DefenderRoll { get; }
        public bool DefenderHit { get; }
        public int DefenderDamage { get; }
        public int AttackerRemainingHp { get; }
        public int DefenderRemainingHp { get; }
        public GridCoord AttackerGrid { get; }
        public GridCoord DefenderGrid { get; }
        public bool AttackerDefeated => AttackerRemainingHp <= 0;
        public bool DefenderDefeated => DefenderRemainingHp <= 0;

        public CombatAttackResult(
            UnitId attackerId,
            UnitId defenderId,
            int attackerHitChance,
            int attackerRoll,
            bool attackerHit,
            int attackerDamage,
            bool countered,
            int defenderHitChance,
            int defenderRoll,
            bool defenderHit,
            int defenderDamage,
            int attackerRemainingHp,
            int defenderRemainingHp,
            GridCoord attackerGrid,
            GridCoord defenderGrid)
        {
            if (attackerId == defenderId)
            {
                throw new ArgumentException(
                    "Attacker and defender must be different units.");
            }

            if (!IsPercent(attackerHitChance) ||
                !IsPercent(attackerRoll) ||
                !IsPercent(defenderHitChance))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackerHitChance),
                    "Hit chances and rolls must be between 0 and 100.");
            }

            if (countered && !IsPercent(defenderRoll))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(defenderRoll),
                    "A counterattack must contain a valid roll.");
            }

            if (!countered && defenderRoll != -1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(defenderRoll),
                    "A skipped counterattack must use -1 as its roll.");
            }

            if (!countered &&
                (defenderHit ||
                 defenderDamage != 0 ||
                 defenderHitChance != 0))
            {
                throw new ArgumentException(
                    "A skipped counterattack cannot hit or deal damage.");
            }

            if ((!attackerHit && attackerDamage != 0) ||
                (!defenderHit && defenderDamage != 0))
            {
                throw new ArgumentException(
                    "A missed attack cannot deal damage.");
            }

            if (attackerDamage < 0 ||
                defenderDamage < 0 ||
                attackerRemainingHp < 0 ||
                defenderRemainingHp < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(attackerDamage),
                    "Damage and remaining HP cannot be negative.");
            }

            AttackerId = attackerId;
            DefenderId = defenderId;
            AttackerHitChance = attackerHitChance;
            AttackerRoll = attackerRoll;
            AttackerHit = attackerHit;
            AttackerDamage = attackerDamage;
            Countered = countered;
            DefenderHitChance = defenderHitChance;
            DefenderRoll = defenderRoll;
            DefenderHit = defenderHit;
            DefenderDamage = defenderDamage;
            AttackerRemainingHp = attackerRemainingHp;
            DefenderRemainingHp = defenderRemainingHp;
            AttackerGrid = attackerGrid;
            DefenderGrid = defenderGrid;
        }

        private static bool IsPercent(int value)
        {
            return value >= 0 && value <= 100;
        }
    }
}
