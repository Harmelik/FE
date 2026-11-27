using System;
using System.Collections.Generic;

namespace FE.Core
{
    public static class AttackRangeQuery
    {
        public static HashSet<GridCoord> TryCalculateAttackRangesByGrid(
            BattleUnit unit,
            BattleMap map,
            GridCoord startPosition)
        {
            if (unit == null ||
                map == null ||
                unit.IsDead ||
                !map.IsInside(startPosition))
            {
                return null;
            }

            int minRange = unit.EquippedWeapon.MinRange;
            int maxRange = unit.EquippedWeapon.MaxRange;
            var result = new HashSet<GridCoord>();

            for (int y = startPosition.y - maxRange;
                 y <= startPosition.y + maxRange;
                 y++)
            {
                for (int x = startPosition.x - maxRange;
                     x <= startPosition.x + maxRange;
                     x++)
                {
                    var coord = new GridCoord(x, y);

                    if (!map.IsInside(coord))
                    {
                        continue;
                    }

                    int distance =
                        Math.Abs(startPosition.x - coord.x) +
                        Math.Abs(startPosition.y - coord.y);

                    if (distance < minRange ||
                        distance > maxRange)
                    {
                        continue;
                    }

                    result.Add(coord);
                }
            }

            return result;
        }

        public static HashSet<GridCoord> CalculateCoverage(
            BattleUnit unit,
            BattleMap map,
            MovementRange movementRange)
        {
            var result = new HashSet<GridCoord>();

            if (unit == null ||
                map == null ||
                movementRange == null ||
                unit.IsDead ||
                movementRange.UnitId != unit.Id)
            {
                return result;
            }

            foreach (GridCoord destination in movementRange.Destinations)
            {
                HashSet<GridCoord> exactRange = TryCalculateAttackRangesByGrid(unit, map, destination);

                if (exactRange == null)
                {
                    continue;
                }

                result.UnionWith(exactRange);
            }

            return result;
        }
    }

    public static class AttackTargetQuery
    {
        public static List<BattleUnit> TryCalculateCanAttackTargets(
            BattleContext context,
            BattleUnit attacker,
            GridCoord startPosition)
        {
            var result = new List<BattleUnit>();

            if (context == null ||
                attacker == null ||
                attacker.IsDead)
            {
                return result;
            }

            HashSet<GridCoord> attackRange =
                AttackRangeQuery.TryCalculateAttackRangesByGrid(
                    attacker,
                    context.Map,
                    startPosition);

            if (attackRange == null)
            {
                return result;
            }

            return FindTargetsInRange(
                context,
                attacker,
                attackRange);
        }

        public static List<BattleUnit> FindTargetsInRange(
            BattleContext context,
            BattleUnit attacker,
            ISet<GridCoord> attackRange)
        {
            var result = new List<BattleUnit>();

            if (context == null ||
                attacker == null ||
                attackRange == null ||
                attacker.IsDead)
            {
                return result;
            }

            foreach (BattleUnit target in context.Units.AllUnits)
            {
                if (target == null ||
                    target.IsDead ||
                    ReferenceEquals(target, attacker) ||
                    target.Fac == attacker.Fac)
                {
                    continue;
                }

                if (attackRange.Contains(target.CurGrid))
                {
                    result.Add(target);
                }
            }

            return result;
        }
    }
}
