
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
        private readonly List<BattleUnit> _allUnitsList = new List<BattleUnit>();
        private readonly Dictionary<UnitId, BattleUnit> _allUnitsDictionaryById = new Dictionary<UnitId, BattleUnit>();
        private readonly Dictionary<GridCoord, BattleUnit> _allUnitsDictionaryByGrid = new Dictionary<GridCoord, BattleUnit>();
        public IReadOnlyList<BattleUnit> AllUnits => _allUnitsList;

        public BattleUnit AddUnit(BattleUnit unit, BattleMap map)
        {
            if (unit == null || unit.IsDead) return null;
            if (_allUnitsDictionaryById.ContainsKey(unit.Id)) return null;
            if (!map.IsInside(unit.CurGrid)) return null;
            if (GetUnitByGrid(unit.CurGrid) != null)
            {
                return null;
            }

            _allUnitsList.Add(unit);
            _allUnitsDictionaryByGrid.Add(unit.CurGrid, unit);
            _allUnitsDictionaryById.Add(unit.Id, unit);
            return unit;
        }

        private void Clear()
        {
            _allUnitsDictionaryById.Clear();
            _allUnitsList.Clear();
            _allUnitsDictionaryByGrid.Clear();
        }

        public BattleUnit GetUnitById(UnitId id)
        {
            if (!_allUnitsDictionaryById.TryGetValue(id, out var battleUnit))
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
            if (!_allUnitsDictionaryByGrid.TryGetValue(grid, out BattleUnit battleUnit))
            {
                return null;
            }

            if (battleUnit.IsDead)
            {
                _allUnitsDictionaryByGrid.Remove(grid);
                return null;
            }

            return battleUnit;
        }

        public bool IsOccupied(GridCoord grid)
        {
            return GetUnitByGrid(grid) != null;
        }

        internal GridCoord? MoveUnitById(
            UnitId id,
            GridCoord destination,
            BattleMap map)
        {
            if (!_allUnitsDictionaryById.TryGetValue(id, out BattleUnit unit))
            {
                return null;
            }

            return MoveUnit(unit, destination, map);
        }

        internal GridCoord? MoveUnit(
            BattleUnit unit,
            GridCoord destination,
            BattleMap map)
        {
            if (unit == null || unit.IsDead) return null;
            if (!map.IsInside(destination)) return null;
            if (!_allUnitsDictionaryById.ContainsKey(unit.Id)) return null;
            if (_allUnitsDictionaryByGrid.ContainsKey(destination)) return null;
            if (_allUnitsDictionaryById[unit.Id] != unit) return null;

            var oldPos = unit.CurGrid;
            if (oldPos == destination) return destination;

            _allUnitsDictionaryByGrid.Remove(oldPos);
            unit.MoveTo(destination);
            _allUnitsDictionaryByGrid.Add(destination, unit);
            return destination;
        }

        public void RemoveFromBoard(BattleUnit unit)
        {
            if (unit == null)
            {
                return;
            }

            if (!_allUnitsDictionaryById.TryGetValue(unit.Id, out BattleUnit registeredUnit)
                || !ReferenceEquals(registeredUnit, unit))
            {
                return;
            }

            if (!_allUnitsDictionaryByGrid.TryGetValue(unit.CurGrid, out BattleUnit occupant) 
                || !ReferenceEquals(occupant, unit))
            {
                return;
            }

            _allUnitsDictionaryByGrid.Remove(unit.CurGrid);
        }
    }
}

