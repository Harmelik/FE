using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class BattleOutcomeQueryTests
    {
        [Test]
        public void TryEvaluate_AllEnemiesDefeated_ReturnsPlayerVictory()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);

            enemy.ApplyDamage(9999);

            bool ended = BattleOutcomeQuery.TryEvaluate(
                context,
                out BattleOutcomeResult result);

            Assert.That(ended, Is.True);
            Assert.That(
                result.OutcomeType,
                Is.EqualTo(BattleOutcomeType.PlayerVictory));
            Assert.That(
                result.Reason,
                Is.EqualTo(BattleOutcomeReason.AllEnemiesDefeated));
        }

        [Test]
        public void TryEvaluate_AllPlayersDefeated_ReturnsPlayerDefeat()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);

            player.ApplyDamage(9999);

            bool ended = BattleOutcomeQuery.TryEvaluate(
                context,
                out BattleOutcomeResult result);

            Assert.That(ended, Is.True);
            Assert.That(
                result.OutcomeType,
                Is.EqualTo(BattleOutcomeType.PlayerDefeat));
            Assert.That(
                result.Reason,
                Is.EqualTo(BattleOutcomeReason.AllPlayersDefeated));
        }

        [Test]
        public void TryEvaluate_BothSidesDefeated_PrefersPlayerDefeat()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);

            player.ApplyDamage(9999);
            enemy.ApplyDamage(9999);

            bool ended = BattleOutcomeQuery.TryEvaluate(
                context,
                out BattleOutcomeResult result);

            Assert.That(ended, Is.True);
            Assert.That(
                result.OutcomeType,
                Is.EqualTo(BattleOutcomeType.PlayerDefeat));
            Assert.That(
                result.Reason,
                Is.EqualTo(BattleOutcomeReason.BothSidesDefeated));
        }

        [Test]
        public void TryEvaluate_BothSidesAlive_ReturnsFalse()
        {
            BattleContext context = CreateContext(
                out BattleUnit player,
                out BattleUnit enemy);

            bool ended = BattleOutcomeQuery.TryEvaluate(
                context,
                out BattleOutcomeResult result);

            Assert.That(ended, Is.False);
            Assert.That(
                result.OutcomeType,
                Is.EqualTo(BattleOutcomeType.None));
        }

        private static BattleContext CreateContext(
            out BattleUnit player,
            out BattleUnit enemy)
        {
            var map = new BattleMap(5, 1);
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
                new GridCoord(4, 0));

            Assert.That(units.AddUnit(player, map), Is.Not.Null);
            Assert.That(units.AddUnit(enemy, map), Is.Not.Null);

            var turns = new BattleTurnController(units);
            return new BattleContext(
                map,
                units,
                new SeededBattleRandom(1),
                turns);
        }
    }
}
