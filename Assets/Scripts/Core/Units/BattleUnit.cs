
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
        public UnitStats BaseStats { get; }

        public int CurHP { get; private set; }
        public GridCoord CurGrid { get; private set; }
        public bool HasActed { get; private set; }
        public bool CanAct => !HasActed && CurHP > 0;
        public bool IsDead => CurHP <= 0;

        public BattleUnit(UnitId id, Faction fac, string displayName, UnitStats stats,
            GridCoord curGrid)
        {
            Id = id;
            Fac = fac;
            DisplayName = displayName;
            BaseStats = stats;
            CurHP = stats.MaxHP;
            CurGrid = curGrid;
            HasActed = false;
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
            return oldHP > finalDmg ? oldHP - finalDmg : oldHP;
        }

        internal GridCoord MoveTo(GridCoord destination)
        {
            if (IsDead) return destination;
            GridCoord des = destination;
            CurGrid = des;
            return des;
        }

        public void MarkActed()
        {
            if (HasActed || IsDead) return;
            HasActed = true;
        }

        public void ResetActionState()
        {
            if(IsDead) return;
            HasActed = false;
        }
    }
}

