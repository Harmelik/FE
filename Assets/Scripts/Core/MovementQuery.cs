using System.Collections.Generic;

namespace FE.Core
{
    /// <summary>
    /// 保存一次移动范围查询的完整结果。
    /// </summary>
    public sealed class MovementRange
    {
        private readonly Dictionary<GridCoord, int> costs;
        private readonly Dictionary<GridCoord, GridCoord> parents;
        private readonly HashSet<GridCoord> destinationLookup;
        private readonly List<GridCoord> destinations;

        public UnitId UnitId { get; }
        public GridCoord Origin { get; }
        public int MovementLimit { get; }

        /// <summary>
        /// 合法可停留格，按 BFS 发现顺序排列。
        /// </summary>
        public IReadOnlyList<GridCoord> Destinations => destinations;

        /**
         * origin 起始点
         * costs 到某个格子的移动力消耗
         * parents 从哪个格子移动到的某个格子
         * destinationLookup和destinations表示可移动到的位置集合
         */
        internal MovementRange(
            UnitId unitId,
            GridCoord origin,
            int movementLimit,
            Dictionary<GridCoord, int> costs,
            Dictionary<GridCoord, GridCoord> parents,
            HashSet<GridCoord> destinationLookup,
            List<GridCoord> destinations)
        {
            UnitId = unitId;
            Origin = origin;
            MovementLimit = movementLimit;
            this.costs = costs;
            this.parents = parents;
            this.destinationLookup = destinationLookup;
            this.destinations = destinations;
        }

        public bool TryGetCost(GridCoord coord, out int cost)
        {
            return costs.TryGetValue(coord, out cost);
        }

        public bool CanStopAt(GridCoord coord)
        {
            return destinationLookup.Contains(coord);
        }

        /// <summary>
        /// 获取从起点到目标落点的路径，路径包含起点和目标格。
        /// </summary>
        public bool TryGetPath(
            GridCoord destination,
            out IReadOnlyList<GridCoord> path)
        {
            path = null;

            if (!CanStopAt(destination))
            {
                return false;
            }

            var reversedPath = new List<GridCoord>();
            GridCoord current = destination;
            reversedPath.Add(current);

            while (current != Origin)
            {
                current = parents[current];
                reversedPath.Add(current);
            }

            reversedPath.Reverse();
            path = reversedPath;
            return true;
        }
    }

    /// <summary>
    /// 计算单位能够经过和停留在哪些格子，不修改任何战斗状态。
    /// </summary>
    public static class MovementQuery
    {
        private static readonly GridCoord[] Directions =
        {
            new GridCoord(0, 1),
            new GridCoord(1, 0),
            new GridCoord(0, -1),
            new GridCoord(-1, 0)
        };

        public static bool TryCalculate(
            BattleUnit unit,
            BattleMap map,
            BattleUnitController units,
            out MovementRange result)
        {
            result = null;

            if (unit == null || map == null || units == null || unit.IsDead)
            {
                return false;
            }

            if (!ReferenceEquals(units.GetUnitById(unit.Id), unit))
            {
                return false;
            }

            GridCoord origin = unit.CurGrid;

            if (!map.IsInside(origin) || !map.IsWalkable(origin))
            {
                return false;
            }

            int movementLimit = unit.BaseStats.Movement;
            var pending = new Queue<GridCoord>();
            var costs = new Dictionary<GridCoord, int>();

            Dictionary<GridCoord, GridCoord> parents = new();
            HashSet<GridCoord> destinationLookup = new();
            List<GridCoord> destinations = new();

            costs.Add(origin, 0);
            pending.Enqueue(origin);
            destinations.Add(origin);
            destinationLookup.Add(origin);

            while (pending.Count > 0)
            {
                var curGrid = pending.Dequeue();
                int curCost = costs[curGrid];
                if(curCost >= movementLimit) continue;

                foreach (var direction in Directions)
                {
                    var destGrid = new GridCoord(curGrid.x + direction.x, curGrid.y + direction.y);
                    if(!map.IsInside(destGrid) || !map.IsWalkable(destGrid)) continue;

                    BattleUnit battleUnit = units.GetUnitByGrid(destGrid);
                    if(battleUnit != null && battleUnit.Fac != unit.Fac) continue;

                    int nextCost = curCost + 1;
                    //有更近的路径就不接着走了
                    if(costs.TryGetValue(destGrid, out int curValue) && curValue <= nextCost) continue;

                    costs[destGrid] = nextCost;
                    parents[destGrid] = curGrid;
                    pending.Enqueue(destGrid);

                    bool canStop = battleUnit == null || battleUnit == unit;
                    //Hash里没有的才往数组里加
                    if (canStop && destinationLookup.Add(destGrid))
                    {
                        destinations.Add(destGrid);
                    }
                }
            }

            result = new MovementRange(
                unit.Id,
                origin,
                movementLimit,
                costs,
                parents,
                destinationLookup,
                destinations);
            return true;
        }
    }
}