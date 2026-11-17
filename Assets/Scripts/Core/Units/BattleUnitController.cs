
using System.Collections.Generic;

namespace FE.Core
{
    /// <summary>
    /// 根据 UnitId 查找单位。
    /// 根据 GridCoord 查找占据该格的单位。
    /// 检查格子是否被占据。
    /// 添加单位时检查重复 ID 和重复位置。
    /// 单位移动时同步更新占位。
    /// 单位阵亡时释放格子。
    /// 提供稳定的单位遍历顺序。
    /// </summary>
    public class BattleUnitController
    {
        private List<BattleUnit> AllUnits = new List<BattleUnit>();
        private Dictionary<UnitId, BattleUnit> AllUnitsDictionaryById = new Dictionary<UnitId, BattleUnit>();
        private Dictionary<GridCoord, BattleUnit> AllUnitsDictionaryByGrid = new Dictionary<GridCoord, BattleUnit>();

        public BattleUnit AddUnit(BattleUnit unit, BattleMap map)
        {
            if (unit == null || unit.IsDead) return null;
            if (AllUnitsDictionaryById.ContainsKey(unit.Id)) return null;
            if (!map.IsInside(unit.CurGrid)) return null;
            if (GetUnitByGrid(unit.CurGrid) != null)
            {
                return null;
            }

            AllUnits.Add(unit);
            AllUnitsDictionaryByGrid.Add(unit.CurGrid, unit);
            AllUnitsDictionaryById.Add(unit.Id, unit);
            return unit;
        }

        private void Clear()
        {
            AllUnitsDictionaryById.Clear();
            AllUnits.Clear();
            AllUnitsDictionaryByGrid.Clear();
        }

        public BattleUnit GetUnitById(UnitId id)
        {
            if (!AllUnitsDictionaryById.TryGetValue(id, out var battleUnit))
            {
                return null;
            }
            else
            {
                return battleUnit;
            }
        }

        public BattleUnit GetUnitByGrid(GridCoord grid)
        {
            if (!AllUnitsDictionaryByGrid.TryGetValue(grid, out BattleUnit battleUnit))
            {
                return null;
            }

            if (battleUnit.IsDead)
            {
                AllUnitsDictionaryByGrid.Remove(grid);
                return null;
            }

            return battleUnit;
        }

        public bool IsOccupied(GridCoord grid)
        {
            return GetUnitByGrid(grid) != null;
        }

        public GridCoord? MoveUnitById(
            UnitId id,
            GridCoord destination,
            BattleMap map)
        {
            if (!AllUnitsDictionaryById.TryGetValue(id, out BattleUnit unit))
            {
                return null;
            }

            return MoveUnit(unit, destination, map);
        }

        public GridCoord? MoveUnit(BattleUnit unit, GridCoord destination, BattleMap map)
        {
            if (unit == null || unit.IsDead) return null;
            if (!map.IsInside(destination)) return null;
            if (!AllUnitsDictionaryById.ContainsKey(unit.Id)) return null;
            if (AllUnitsDictionaryByGrid.ContainsKey(destination)) return null;
            if (AllUnitsDictionaryById[unit.Id] != unit) return null;

            var oldPos = unit.CurGrid;
            if (oldPos == destination) return destination;

            AllUnitsDictionaryByGrid.Remove(oldPos);
            unit.MoveTo(destination);
            AllUnitsDictionaryByGrid.Add(destination, unit);
            return destination;
        }

        public void RemoveFromBoard(BattleUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            if (!AllUnitsDictionaryById.TryGetValue(unit.Id, out BattleUnit registeredUnit)
                || !ReferenceEquals(registeredUnit, unit))
            {
                return;
            }

            if (!AllUnitsDictionaryByGrid.TryGetValue(unit.CurGrid, out BattleUnit occupant) 
                || !ReferenceEquals(occupant, unit))
            {
                return;
            }

            AllUnitsDictionaryByGrid.Remove(unit.CurGrid);
        }
    }
}

