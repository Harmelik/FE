using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class AggressiveEnemyActionProviderTests
    {
        [Test]
        public void TryPlan_PrefersKillOverMoreNonLethalDamage()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(2, 0),
                20,
                10,
                2,
                1,
                1,
                1);
            BattleUnit weakPlayer = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(4, 0),
                5,
                4,
                0,
                1,
                1,
                1);
            BattleUnit healthyPlayer = CreateUnit(
                2,
                Faction.Player,
                new GridCoord(3, 1),
                20,
                4,
                0,
                1,
                1,
                1);
            BattleContext context = CreateContext(
                enemy,
                weakPlayer,
                healthyPlayer);

            bool planned =
                new AggressiveEnemyActionProvider().TryPlan(
                    context,
                    enemy,
                    out EnemyActionPlan plan);

            Assert.That(planned, Is.True);
            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Attack));
            Assert.That(
                plan.TargetUnitId,
                Is.EqualTo(weakPlayer.Id));
            Assert.That(plan.CanKill, Is.True);
        }

        [Test]
        public void TryPlan_PrefersLowerExpectedCounterDamage()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(2, 0),
                20,
                10,
                2,
                1,
                1,
                1);
            BattleUnit safeTarget = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(3, 0),
                20,
                2,
                1,
                1,
                1,
                1);
            BattleUnit dangerousTarget = CreateUnit(
                2,
                Faction.Player,
                new GridCoord(2, 1),
                20,
                8,
                1,
                1,
                1,
                1);
            BattleContext context = CreateContext(
                enemy,
                safeTarget,
                dangerousTarget);

            new AggressiveEnemyActionProvider().TryPlan(
                context,
                enemy,
                out EnemyActionPlan plan);

            Assert.That(
                plan.TargetUnitId,
                Is.EqualTo(safeTarget.Id));
        }

        [Test]
        public void TryPlan_UsesTargetGridTieBreak()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(1, 1),
                20,
                10,
                2,
                0,
                2,
                2);
            BattleUnit leftTarget = CreateUnit(
                2,
                Faction.Player,
                new GridCoord(0, 0),
                20,
                1,
                1,
                1,
                1,
                1);
            BattleUnit rightTarget = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(2, 2),
                20,
                1,
                1,
                1,
                1,
                1);
            BattleContext context = CreateContext(
                enemy,
                leftTarget,
                rightTarget);

            new AggressiveEnemyActionProvider().TryPlan(
                context,
                enemy,
                out EnemyActionPlan plan);

            Assert.That(
                plan.TargetUnitId,
                Is.EqualTo(leftTarget.Id));
        }

        [Test]
        public void TryPlan_WithoutAttack_StopsAtClosestReachableCell()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(0, 0),
                20,
                10,
                2,
                2,
                1,
                1);
            BattleUnit player = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(5, 0),
                20,
                4,
                1,
                1,
                1,
                1);
            BattleContext context = CreateContext(enemy, player);

            bool planned =
                new AggressiveEnemyActionProvider().TryPlan(
                    context,
                    enemy,
                    out EnemyActionPlan plan);

            Assert.That(planned, Is.True);
            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Move));
            Assert.That(
                plan.Destination,
                Is.EqualTo(new GridCoord(2, 0)));
            Assert.That(plan.TargetUnitId, Is.Null);
        }

        [Test]
        public void TryPlan_WithoutPlayers_WaitsInPlace()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(2, 2),
                20,
                10,
                2,
                2,
                1,
                1);
            BattleUnit player = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(9, 0),
                20,
                4,
                1,
                1,
                1,
                1);
            BattleContext context = CreateContext(enemy, player);
            player.ApplyDamage(9999);

            bool planned =
                new AggressiveEnemyActionProvider().TryPlan(
                    context,
                    enemy,
                    out EnemyActionPlan plan);

            Assert.That(planned, Is.True);
            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Wait));
            Assert.That(
                plan.Destination,
                Is.EqualTo(new GridCoord(2, 2)));
        }

        [Test]
        public void TryPlan_IsPureAndDeterministic()
        {
            BattleUnit enemy = CreateUnit(
                10,
                Faction.Enemy,
                new GridCoord(2, 0),
                20,
                10,
                2,
                1,
                1,
                1);
            BattleUnit player = CreateUnit(
                1,
                Faction.Player,
                new GridCoord(4, 0),
                20,
                4,
                1,
                1,
                1,
                1);
            BattleContext context = CreateContext(enemy, player);
            var provider = new AggressiveEnemyActionProvider();

            GridCoord enemyGridBefore = enemy.CurGrid;
            GridCoord playerGridBefore = player.CurGrid;
            int enemyHpBefore = enemy.CurHP;
            int playerHpBefore = player.CurHP;
            bool enemyActedBefore = enemy.HasActed;
            bool playerActedBefore = player.HasActed;

            provider.TryPlan(
                context,
                enemy,
                out EnemyActionPlan first);
            provider.TryPlan(
                context,
                enemy,
                out EnemyActionPlan second);

            Assert.That(
                second.ActionType,
                Is.EqualTo(first.ActionType));
            Assert.That(
                second.Destination,
                Is.EqualTo(first.Destination));
            Assert.That(
                second.TargetUnitId,
                Is.EqualTo(first.TargetUnitId));
            Assert.That(second.Score, Is.EqualTo(first.Score));
            Assert.That(enemy.CurGrid, Is.EqualTo(enemyGridBefore));
            Assert.That(player.CurGrid, Is.EqualTo(playerGridBefore));
            Assert.That(enemy.CurHP, Is.EqualTo(enemyHpBefore));
            Assert.That(player.CurHP, Is.EqualTo(playerHpBefore));
            Assert.That(enemy.HasActed, Is.EqualTo(enemyActedBefore));
            Assert.That(player.HasActed, Is.EqualTo(playerActedBefore));
        }

        private static BattleUnit CreateUnit(
            int id,
            Faction faction,
            GridCoord grid,
            int hp,
            int attack,
            int defense,
            int movement,
            int minRange,
            int maxRange)
        {
            return new BattleUnit(
                new UnitId(id),
                faction,
                $"Unit{id}",
                new UnitStats(
                    hp,
                    attack,
                    defense,
                    0,
                    0,
                    100,
                    0,
                    movement,
                    minRange,
                    maxRange),
                grid);
        }

        private static BattleContext CreateContext(
            params BattleUnit[] units)
        {
            var map = new BattleMap(10, 4);
            var roster = new BattleUnitController();

            foreach (BattleUnit unit in units)
            {
                Assert.That(roster.AddUnit(unit, map), Is.Not.Null);
            }

            var turns = new BattleTurnController(roster);
            var context = new BattleContext(
                map,
                roster,
                new SeededBattleRandom(1),
                turns);

            BattleCommandResult transition =
                new BattleCommandProcessor(context).Execute(
                    new EndPhaseCommand());

            Assert.That(transition.IsSuccess, Is.True);
            return context;
        }
    }
}
