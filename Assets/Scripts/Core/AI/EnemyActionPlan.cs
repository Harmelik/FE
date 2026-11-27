using System;

namespace FE.Core
{
    public enum EnemyActionType
    {
        Attack,
        Move,
        Wait
    }

    public readonly struct EnemyActionPlan
    {
        public EnemyActionType ActionType { get; }
        public UnitId UnitId { get; }
        public GridCoord Destination { get; }
        public UnitId? TargetUnitId { get; }
        public int Score { get; }
        public int ExpectedDamage { get; }
        public bool CanKill { get; }
        public int ExpectedCounterDamage { get; }

        private EnemyActionPlan(
            EnemyActionType actionType,
            UnitId unitId,
            GridCoord destination,
            UnitId? targetUnitId,
            int score,
            int expectedDamage,
            bool canKill,
            int expectedCounterDamage)
        {
            ActionType = actionType;
            UnitId = unitId;
            Destination = destination;
            TargetUnitId = targetUnitId;
            Score = score;
            ExpectedDamage = expectedDamage;
            CanKill = canKill;
            ExpectedCounterDamage = expectedCounterDamage;
        }

        public static EnemyActionPlan Attack(
            UnitId unitId,
            GridCoord destination,
            UnitId? targetUnitId,
            int score,
            int expectedDamage,
            bool canKill,
            int expectedCounterDamage)
        {
            if (!targetUnitId.HasValue)
            {
                throw new ArgumentException(
                    "Attack plan requires a target.",
                    nameof(targetUnitId));
            }
            return new EnemyActionPlan(EnemyActionType.Attack, unitId, destination, targetUnitId, score, expectedDamage, canKill, expectedCounterDamage);
        }

        public static EnemyActionPlan Move(
            UnitId unitId,
            GridCoord destination,
            int score)
        {
            return new EnemyActionPlan(EnemyActionType.Move, unitId, destination,null, score,0,false, 0);
        }

        public static EnemyActionPlan Wait(
            UnitId unitId,
            GridCoord destination)
        {
            return new EnemyActionPlan(EnemyActionType.Wait, unitId, destination, null, 0, 0, false, 0);

        }
    }
}
