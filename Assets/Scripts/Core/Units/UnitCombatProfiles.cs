using System;

namespace FE.Core
{
    [Flags]
    public enum WeaponCategory
    {
        None = 0,
        Sword = 1 << 0,
        Lance = 1 << 1,
        Axe = 1 << 2,
        Bow = 1 << 3,
        Tome = 1 << 4,
        Fist = 1 << 5
    }

    public enum DamageType
    {
        Physical,
        Magical
    }

    /// <summary>
    /// Runtime snapshot of a unit class. It deliberately contains no Unity types.
    /// </summary>
    public sealed class UnitClassProfile
    {
        public string DisplayName { get; }
        public WeaponCategory EquippableWeaponCategories { get; }
        public int Movement { get; }
        public MovementType MovementType { get; }

        public UnitClassProfile(
            string displayName,
            WeaponCategory equippableWeaponCategories,
            int movement,
            MovementType movementType = MovementType.Foot)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Class display name cannot be empty.",
                    nameof(displayName));
            }

            if (equippableWeaponCategories == WeaponCategory.None)
            {
                throw new ArgumentException(
                    "A class must allow at least one weapon category.",
                    nameof(equippableWeaponCategories));
            }

            if (movement < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(movement),
                    movement,
                    "Movement cannot be negative.");
            }

            DisplayName = displayName;
            EquippableWeaponCategories = equippableWeaponCategories;
            Movement = movement;
            MovementType = movementType;
        }

        public bool CanEquip(WeaponCategory category)
        {
            return category != WeaponCategory.None &&
                   (EquippableWeaponCategories & category) == category;
        }
    }

    /// <summary>
    /// Runtime snapshot of one weapon. Weapon might and hit are modifiers added
    /// to the wielder's personal stats.
    /// </summary>
    public sealed class WeaponProfile
    {
        public string DisplayName { get; }
        public WeaponCategory Category { get; }
        public DamageType DamageType { get; }
        public int Might { get; }
        public int Hit { get; }
        public int Crit { get; }
        public int MinRange { get; }
        public int MaxRange { get; }

        public WeaponProfile(
            string displayName,
            WeaponCategory category,
            DamageType damageType,
            int might,
            int hit,
            int crit,
            int minRange,
            int maxRange)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Weapon display name cannot be empty.",
                    nameof(displayName));
            }

            int categoryValue = (int)category;
            if (categoryValue == 0 ||
                (categoryValue & (categoryValue - 1)) != 0)
            {
                throw new ArgumentException(
                    "A weapon must have exactly one weapon category.",
                    nameof(category));
            }

            if (might < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(might),
                    might,
                    "Weapon might cannot be negative.");
            }

            if (crit < 0 || crit > 100)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(crit),
                    crit,
                    "Weapon crit must be between 0 and 100.");
            }

            if (minRange <= 0 || maxRange < minRange)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxRange),
                    maxRange,
                    "Weapon range must be positive and ordered.");
            }

            DisplayName = displayName;
            Category = category;
            DamageType = damageType;
            Might = might;
            Hit = hit;
            Crit = crit;
            MinRange = minRange;
            MaxRange = maxRange;
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

            return distance >= MinRange && distance <= MaxRange;
        }
    }
}
