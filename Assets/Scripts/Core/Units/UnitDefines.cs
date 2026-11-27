using System;

namespace FE.Core
{
    //阵营
    public enum Faction
    {
        Player,
        Enemy
    }
    //标识符Id
    public readonly struct UnitId : IEquatable<UnitId>
    {
        public int Value { get; }

        public UnitId(int value)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "UnitId cannot be negative.");
            }

            Value = value;
        }

        public bool Equals(UnitId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is UnitId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override string ToString()
        {
            return $"UnitId({Value})";
        }

        public static bool operator ==(UnitId left, UnitId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(UnitId left, UnitId right)
        {
            return !left.Equals(right);
        }
    }
    //基础属性，暂时先只做这些
    public readonly struct UnitStats
    {
        public int MaxHP { get; }
        public int PAtk { get; }
        public int PhyDef { get; }
        public int MAtk { get; }
        public int MagicDef { get; }
        public int Hit { get; }
        public int Avoid { get; }
        public int Movement { get; }
        public int MinAttackRange { get; }
        public int MaxAttackRange { get; }

        public UnitStats(
            int maxHP,
            int patk,
            int phyDef,
            int matk,
            int magicDef,
            int hit,
            int avoid)
            : this(
                maxHP,
                patk,
                phyDef,
                matk,
                magicDef,
                hit,
                avoid,
                0,
                0,
                0)
        {
        }

        /// <summary>
        /// Compatibility constructor for isolated rule tests and older callers.
        /// New unit data reads movement from the class and range from the weapon.
        /// </summary>
        public UnitStats(
            int maxHP,
            int patk,
            int phyDef,
            int matk,
            int magicDef,
            int hit,
            int avoid,
            int movement,
            int minAttackRange,
            int maxAttackRange)
        {
            if (maxHP <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxHP),
                    maxHP,
                    "Max health must be greater than zero.");
            }

            ThrowIfNegative(patk, nameof(patk));
            ThrowIfNegative(phyDef, nameof(phyDef));
            ThrowIfNegative(matk, nameof(matk));
            ThrowIfNegative(magicDef, nameof(magicDef));
            ThrowIfNegative(hit, nameof(hit));
            ThrowIfNegative(avoid, nameof(avoid));
            ThrowIfNegative(movement, nameof(movement));
            ThrowIfNegative(minAttackRange, nameof(minAttackRange));
            ThrowIfNegative(maxAttackRange, nameof(maxAttackRange));

            if (maxAttackRange < minAttackRange)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxAttackRange),
                    maxAttackRange,
                    "Max attack range cannot be smaller than min attack range.");
            }

            MaxHP = maxHP;
            PAtk = patk;
            PhyDef = phyDef;
            MAtk = matk;
            MagicDef = magicDef;
            Hit = hit;
            Avoid = avoid;
            Movement = movement;
            MinAttackRange = minAttackRange;
            MaxAttackRange = maxAttackRange;
        }

        public bool CanAttackAtDistance(int distance)
        {
            if (distance < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(distance),
                    distance,
                    "Attack distance cannot be negative.");
            }

            return distance >= MinAttackRange &&
                   distance <= MaxAttackRange;
        }

        private static void ThrowIfNegative(
            int value,
            string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    parameterName,
                    value,
                    "Stat value cannot be negative.");
            }
        }
    }
}
