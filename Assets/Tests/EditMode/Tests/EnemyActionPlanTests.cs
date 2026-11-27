using System;
using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class EnemyActionPlanTests
    {
        [Test]
        public void Attack_WithoutTarget_Throws()
        {
            Assert.Throws<ArgumentException>(() =>
                EnemyActionPlan.Attack(
                    new UnitId(1),
                    new GridCoord(2, 3),
                    null,
                    100,
                    5,
                    false,
                    0));
        }

        [Test]
        public void Attack_StoresCombatValues()
        {
            EnemyActionPlan plan = EnemyActionPlan.Attack(
                new UnitId(1),
                new GridCoord(2, 3),
                new UnitId(4),
                1200,
                8,
                true,
                0);

            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Attack));
            Assert.That(plan.UnitId, Is.EqualTo(new UnitId(1)));
            Assert.That(
                plan.Destination,
                Is.EqualTo(new GridCoord(2, 3)));
            Assert.That(
                plan.TargetUnitId,
                Is.EqualTo(new UnitId(4)));
            Assert.That(plan.Score, Is.EqualTo(1200));
            Assert.That(plan.ExpectedDamage, Is.EqualTo(8));
            Assert.That(plan.CanKill, Is.True);
            Assert.That(plan.ExpectedCounterDamage, Is.EqualTo(0));
        }

        [Test]
        public void Move_HasNoTargetOrCombatValues()
        {
            EnemyActionPlan plan = EnemyActionPlan.Move(
                new UnitId(2),
                new GridCoord(5, 4),
                -3);

            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Move));
            Assert.That(plan.TargetUnitId, Is.Null);
            Assert.That(plan.ExpectedDamage, Is.EqualTo(0));
            Assert.That(plan.CanKill, Is.False);
            Assert.That(plan.ExpectedCounterDamage, Is.EqualTo(0));
        }

        [Test]
        public void Wait_UsesCurrentPosition()
        {
            EnemyActionPlan plan = EnemyActionPlan.Wait(
                new UnitId(3),
                new GridCoord(7, 6));

            Assert.That(
                plan.ActionType,
                Is.EqualTo(EnemyActionType.Wait));
            Assert.That(plan.UnitId, Is.EqualTo(new UnitId(3)));
            Assert.That(
                plan.Destination,
                Is.EqualTo(new GridCoord(7, 6)));
            Assert.That(plan.TargetUnitId, Is.Null);
        }
    }
}
