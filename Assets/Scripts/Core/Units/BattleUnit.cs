
using System;

namespace FE.Core
{
    /// <summary>
    /// 保存单位身份、阵营和基础属性。
    /// 保存当前 HP、当前格子和是否已行动。
    /// 提供受伤、移动、标记已行动和回合重置等最小行为。
    /// 判断单位是否存活、是否可以行动。
    /// </summary>
    public class BattleUnit
    {
        public UnitId Id { get; }
        public Faction Fac { get; }
        public string DisplayName { get; }
        public int Level { get; }
        public UnitClassProfile UnitClass { get; }
        public WeaponProfile EquippedWeapon { get; private set; }
        public UnitStats BaseStats { get; }
        public int Movement => UnitClass.Movement;
        public MovementType MovementType => UnitClass.MovementType;
        public int AttackPower =>
            (EquippedWeapon.DamageType == DamageType.Magical
                ? BaseStats.MAtk
                : BaseStats.PAtk) +
            EquippedWeapon.Might;

        public int GetAttackPower(TerrainProfile terrain)
        {
            return AttackPower +
                   (terrain?.GetAttackBonus(
                       EquippedWeapon.DamageType) ?? 0);
        }

        public int CurHP { get; private set; }
        public GridCoord CurGrid { get; private set; }
        public bool HasActed { get; private set; }
        public bool CanAct => !HasActed && CurHP > 0;
        public bool IsDead => CurHP <= 0;

        public BattleUnit(
            UnitId id,
            Faction fac,
            string displayName,
            int level,
            UnitClassProfile unitClass,
            WeaponProfile equippedWeapon,
            UnitStats stats,
            GridCoord curGrid)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Unit display name cannot be empty.",
                    nameof(displayName));
            }

            if (level <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(level),
                    level,
                    "Unit level must be greater than zero.");
            }

            if (unitClass == null)
            {
                throw new ArgumentNullException(nameof(unitClass));
            }

            if (equippedWeapon == null)
            {
                throw new ArgumentNullException(nameof(equippedWeapon));
            }

            if (!unitClass.CanEquip(equippedWeapon.Category))
            {
                throw new ArgumentException(
                    $"Class {unitClass.DisplayName} cannot equip " +
                    $"{equippedWeapon.DisplayName}.");
            }

            Id = id;
            Fac = fac;
            DisplayName = displayName;
            Level = level;
            UnitClass = unitClass;
            EquippedWeapon = equippedWeapon;
            BaseStats = stats;
            CurHP = stats.MaxHP;
            CurGrid = curGrid;
            HasActed = false;
        }

        /// <summary>
        /// Compatibility constructor for focused tests that do not exercise
        /// class or weapon data. Gameplay setup uses the full constructor.
        /// </summary>
        public BattleUnit(
            UnitId id,
            Faction fac,
            string displayName,
            UnitStats stats,
            GridCoord curGrid)
            : this(
                id,
                fac,
                displayName,
                1,
                new UnitClassProfile(
                    "TestClass",
                    WeaponCategory.Sword,
                    stats.Movement),
                new WeaponProfile(
                    "TestWeapon",
                    WeaponCategory.Sword,
                    DamageType.Physical,
                    0,
                    0,
                    0,
                    stats.MinAttackRange,
                    stats.MaxAttackRange),
                stats,
                curGrid)
        {
        }

        public int ApplyDamage(int amount)
        {
            if (amount < 0 || IsDead)
            {
                return 0;
            }
            int finalDmg = amount;
            int oldHP = CurHP;
            CurHP = Math.Max(0, CurHP - finalDmg);
            return oldHP - CurHP;
        }

        internal void MoveTo(GridCoord destination)
        {
            if (IsDead) return;
            CurGrid = destination;
        }

        internal void MarkActed()
        {
            if (HasActed || IsDead) return;
            HasActed = true;
        }

        public void ResetActionState()
        {
            if(IsDead) return;
            HasActed = false;
        }

        internal void RestoreState(
            int curHP,
            GridCoord curGrid,
            bool hasActed,
            WeaponProfile equippedWeapon)
        {
            if (curHP < 0 || curHP > BaseStats.MaxHP)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(curHP),
                    curHP,
                    "Restored HP must be within the unit's HP range.");
            }

            if (equippedWeapon == null ||
                !UnitClass.CanEquip(equippedWeapon.Category))
            {
                throw new ArgumentException(
                    "Restored weapon is not valid for the unit class.",
                    nameof(equippedWeapon));
            }

            CurHP = curHP;
            CurGrid = curGrid;
            HasActed = hasActed;
            EquippedWeapon = equippedWeapon;
        }
    }
}

