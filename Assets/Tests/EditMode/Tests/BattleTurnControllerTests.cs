using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class BattleTurnControllerTests
    {
        [Test]
        public void NewBattle_StartsTurnOnePlayerPhase()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);

            Assert.That(context.Turns.TurnNumber, Is.EqualTo(1));
            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.PlayerPhase));
            Assert.That(context.Turns.CanUnitAct(player), Is.True);
            Assert.That(context.Turns.CanUnitAct(enemy), Is.False);
        }

        [Test]
        public void EndPlayerPhase_EntersEnemyPhase()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            BattleCommandResult result = processor.Execute(
                new EndPhaseCommand());

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.EnemyPhase));
            Assert.That(context.Turns.TurnNumber, Is.EqualTo(1));
            Assert.That(context.Turns.CanUnitAct(enemy), Is.True);
            Assert.That(context.Turns.CanUnitAct(player), Is.False);
        }

        [Test]
        public void EndEnemyPhase_StartsNextPlayerTurnAndResetsPlayers()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            Assert.That(
                processor.Execute(
                    new MoveAndWaitCommand(
                        player.Id,
                        new GridCoord(1, 0))).IsSuccess,
                Is.True);
            Assert.That(player.HasActed, Is.True);

            processor.Execute(new EndPhaseCommand());
            processor.Execute(new EndPhaseCommand());

            Assert.That(context.Turns.TurnNumber, Is.EqualTo(2));
            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.PlayerPhase));
            Assert.That(player.HasActed, Is.False);
            Assert.That(context.Turns.CanUnitAct(player), Is.True);
        }

        [Test]
        public void EnemyPhase_RejectsPlayerMoveCommand()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            processor.Execute(new EndPhaseCommand());

            BattleCommandResult result = processor.Execute(
                new MoveAndWaitCommand(
                    player.Id,
                    new GridCoord(1, 0)));

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(player.CurGrid, Is.EqualTo(new GridCoord(0, 0)));
            Assert.That(player.HasActed, Is.False);
        }

        [Test]
        public void EnteringEnemyPhaseAgain_ResetsPreviouslyActedEnemy()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            processor.Execute(new EndPhaseCommand());
            Assert.That(
                processor.Execute(
                    new MoveAndWaitCommand(
                        enemy.Id,
                        new GridCoord(7, 0))).IsSuccess,
                Is.True);
            Assert.That(enemy.HasActed, Is.True);

            processor.Execute(new EndPhaseCommand());
            processor.Execute(new EndPhaseCommand());

            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.EnemyPhase));
            Assert.That(enemy.HasActed, Is.False);
            Assert.That(context.Turns.CanUnitAct(enemy), Is.True);
        }

        private static BattleContext CreateContext(
            out BattleUnit player,
            out BattleUnit enemy)
        {
            var map = new BattleMap(10, 1);
            var units = new BattleUnitController();

            player = new BattleUnit(
                new UnitId(1),
                Faction.Player,
                "Player",
                new UnitStats(20, 5, 1, 0, 0, 100, 0, 1, 1, 1),
                new GridCoord(0, 0));

            enemy = new BattleUnit(
                new UnitId(2),
                Faction.Enemy,
                "Enemy",
                new UnitStats(20, 5, 1, 0, 0, 100, 0, 1, 1, 1),
                new GridCoord(8, 0));

            units.AddUnit(player, map);
            units.AddUnit(enemy, map);

            var turns = new BattleTurnController(units);
            return new BattleContext(
                map,
                units,
                new SeededBattleRandom(1),
                turns);
        }
    }
}
