using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class BattleOutcomeCommandTests
    {
        [Test]
        public void PlayerKillsLastEnemy_EndsBattleWithVictory()
        {
            BattleContext context = CreateContext(
                Faction.Player,
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            BattleCommandResult attackResult =
                processor.Execute(
                    new MoveAndAttackCommand(
                        player.Id,
                        enemy.Id,
                        new GridCoord(1, 0)));

            Assert.That(attackResult.IsSuccess, Is.True);
            Assert.That(attackResult.BattleOutcome.HasValue, Is.True);
            Assert.That(
                attackResult.BattleOutcome.Value.OutcomeType,
                Is.EqualTo(BattleOutcomeType.PlayerVictory));
            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.BattleEnded));

            BattleCommandResult endPhaseResult =
                processor.Execute(new EndPhaseCommand());

            Assert.That(endPhaseResult.IsSuccess, Is.False);
        }

        [Test]
        public void EnemyKillsLastPlayer_EndsBattleWithDefeat()
        {
            BattleContext context = CreateContext(
                Faction.Enemy,
                out BattleUnit player,
                out BattleUnit enemy);
            var processor = new BattleCommandProcessor(context);

            BattleCommandResult attackResult =
                processor.Execute(
                    new MoveAndAttackCommand(
                        enemy.Id,
                        player.Id,
                        new GridCoord(1, 0)));

            Assert.That(attackResult.IsSuccess, Is.True);
            Assert.That(attackResult.BattleOutcome.HasValue, Is.True);
            Assert.That(
                attackResult.BattleOutcome.Value.OutcomeType,
                Is.EqualTo(BattleOutcomeType.PlayerDefeat));
            Assert.That(
                context.Turns.CurrentPhase,
                Is.EqualTo(BattlePhase.BattleEnded));
        }

        private static BattleContext CreateContext(
            Faction actingFaction,
            out BattleUnit player,
            out BattleUnit enemy)
        {
            var map = new BattleMap(5, 1);
            var units = new BattleUnitController();

            player = new BattleUnit(
                new UnitId(1),
                Faction.Player,
                "Player",
                new UnitStats(
                    actingFaction == Faction.Enemy ? 5 : 20,
                    10,
                    1,
                    0,
                    0,
                    100,
                    0,
                    1,
                    1,
                    1),
                new GridCoord(2, 0));

            enemy = new BattleUnit(
                new UnitId(2),
                Faction.Enemy,
                "Enemy",
                new UnitStats(
                    actingFaction == Faction.Player ? 5 : 20,
                    10,
                    1,
                    0,
                    0,
                    100,
                    0,
                    1,
                    1,
                    1),
                new GridCoord(0, 0));

            Assert.That(units.AddUnit(player, map), Is.Not.Null);
            Assert.That(units.AddUnit(enemy, map), Is.Not.Null);

            var turns = new BattleTurnController(units);
            var context = new BattleContext(
                map,
                units,
                new SeededBattleRandom(1),
                turns);

            if (actingFaction == Faction.Enemy)
            {
                BattleCommandResult transition =
                    new BattleCommandProcessor(context).Execute(
                        new EndPhaseCommand());
                Assert.That(transition.IsSuccess, Is.True);
            }

            return context;
        }
    }
}
