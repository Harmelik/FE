using System;

namespace FE.Core
{
    public sealed class AggressiveEnemyActionProvider :
        IEnemyActionProvider
    {
        public bool TryPlan(
            BattleContext context,
            BattleUnit enemy,
            out EnemyActionPlan plan)
        {
            plan = default;

            if (context == null || enemy == null)
            {
                return false;
            }

            if (enemy.IsDead || !context.Turns.CanUnitAct(enemy))
            {
                return false;
            }

            bool isSuccess = MovementQuery.TryCalculate(enemy, context.Map, context.Units, out var movement);
            if (!isSuccess) return false;

            bool hasPlan = false;
            EnemyActionPlan bestPlan = default;
            GridCoord bestTargetGrid = default;

            foreach (var destination in movement.Destinations)
            {
                var range = AttackRangeQuery.TryCalculateAttackRangesByGrid(enemy, context.Map, destination);
                var targets = AttackTargetQuery.FindTargetsInRange(context, enemy, range);
                if(targets == null || targets.Count == 0) continue;
                movement.TryGetCost(destination, out int moveCost);

                foreach (var target in targets)
                {
                    bool isSuccessForecast = CombatForecastQuery.TryCalculate(context, enemy, target, destination, out var result);
                    if(!isSuccessForecast) continue;

                    int damage = result.AttackerForecast.Damage;
                    int expectedDamage = damage * result.AttackerForecast.HitChance / 100;
                    bool canKill =
                        result.DefenderForecast.ProjectedHp <= 0;

                    int expectedCounterDamage = 0;
                    if(!canKill)
                    {
                        expectedCounterDamage = result.CanCounter
                            ? result.DefenderForecast.Damage * result.DefenderForecast.HitChance / 100
                            : 0;
                    }
                    int score =
                        (canKill ? 10000 : 0) +
                        expectedDamage * 100 -
                        expectedCounterDamage * 50 -
                        moveCost;

                    var newPlan = EnemyActionPlan.Attack(
                        enemy.Id,
                        destination,
                        target.Id,
                        score,
                        expectedDamage,
                        canKill,
                        expectedCounterDamage);
                    if (!hasPlan ||
                        IsBetterAttack(
                            newPlan,
                            target.CurGrid,
                            bestPlan,
                            bestTargetGrid))
                    {
                        bestPlan = newPlan;
                        bestTargetGrid = target.CurGrid;
                        hasPlan = true;
                    }
                }
            }

            if (hasPlan)
            {
                plan = bestPlan;
                return true;
            }

            return TryCreateFallbackPlan(
                context,
                enemy,
                movement,
                out plan);
        }

        private static bool TryCreateFallbackPlan(
            BattleContext context,
            BattleUnit enemy,
            MovementRange movement,
            out EnemyActionPlan plan)
        {
            var players = new System.Collections.Generic.List<BattleUnit>();

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                if (!unit.IsDead && unit.Fac == Faction.Player)
                {
                    players.Add(unit);
                }
            }

            if (players.Count == 0)
            {
                plan = EnemyActionPlan.Wait(
                    enemy.Id,
                    enemy.CurGrid);
                return true;
            }

            GridCoord bestDestination = enemy.CurGrid;
            int bestDistance = int.MaxValue;
            int bestCost = int.MaxValue;

            foreach (GridCoord destination in movement.Destinations)
            {
                int nearestDistance = int.MaxValue;

                foreach (BattleUnit player in players)
                {
                    int distance =
                        Math.Abs(destination.x - player.CurGrid.x) +
                        Math.Abs(destination.y - player.CurGrid.y);

                    nearestDistance = Math.Min(
                        nearestDistance,
                        distance);
                }

                movement.TryGetCost(destination, out int cost);

                bool isBetter =
                    nearestDistance < bestDistance ||
                    nearestDistance == bestDistance &&
                    cost < bestCost ||
                    nearestDistance == bestDistance &&
                    cost == bestCost &&
                    destination.x < bestDestination.x ||
                    nearestDistance == bestDistance &&
                    cost == bestCost &&
                    destination.x == bestDestination.x &&
                    destination.y < bestDestination.y;

                if (!isBetter)
                {
                    continue;
                }

                bestDestination = destination;
                bestDistance = nearestDistance;
                bestCost = cost;
            }

            plan = bestDestination == enemy.CurGrid
                ? EnemyActionPlan.Wait(enemy.Id, enemy.CurGrid)
                : EnemyActionPlan.Move(
                    enemy.Id,
                    bestDestination,
                    -bestDistance);
            return true;
        }

        private static bool IsBetterAttack(
            EnemyActionPlan candidate,
            GridCoord candidateTargetGrid,
            EnemyActionPlan current,
            GridCoord currentTargetGrid)
        {
            if (candidate.Score != current.Score)
            {
                return candidate.Score > current.Score;
            }

            if (candidateTargetGrid.x != currentTargetGrid.x)
            {
                return candidateTargetGrid.x < currentTargetGrid.x;
            }

            if (candidateTargetGrid.y != currentTargetGrid.y)
            {
                return candidateTargetGrid.y < currentTargetGrid.y;
            }

            if (candidate.Destination.x != current.Destination.x)
            {
                return candidate.Destination.x < current.Destination.x;
            }

            if (candidate.Destination.y != current.Destination.y)
            {
                return candidate.Destination.y < current.Destination.y;
            }

            return false;
        }
    }
}
