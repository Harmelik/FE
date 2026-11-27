using System;

namespace FE.Core
{
    public enum BattleHistoryKind
    {
        UnitAction,
        PhaseTransition
    }

    public enum BattleHistoryActionType
    {
        Attack,
        Wait,
        Equip,
        PhaseTransition,
        Other
    }

    public readonly struct BattleCommandHistoryDescriptor
    {
        public BattleHistoryKind Kind { get; }
        public BattleHistoryActionType ActionType { get; }
        public UnitId? ActorUnitId { get; }
        public UnitId? TargetUnitId { get; }
        public GridCoord? Destination { get; }

        private BattleCommandHistoryDescriptor(
            BattleHistoryKind kind,
            BattleHistoryActionType actionType,
            UnitId? actorUnitId,
            UnitId? targetUnitId,
            GridCoord? destination)
        {
            Kind = kind;
            ActionType = actionType;
            ActorUnitId = actorUnitId;
            TargetUnitId = targetUnitId;
            Destination = destination;
        }

        public static BattleCommandHistoryDescriptor UnitAction(
            BattleHistoryActionType actionType,
            UnitId actorUnitId,
            UnitId? targetUnitId = null,
            GridCoord? destination = null)
        {
            if (actionType == BattleHistoryActionType.PhaseTransition)
            {
                throw new ArgumentException(
                    "A unit action cannot use the phase transition type.",
                    nameof(actionType));
            }

            return new BattleCommandHistoryDescriptor(
                BattleHistoryKind.UnitAction,
                actionType,
                actorUnitId,
                targetUnitId,
                destination);
        }

        public static BattleCommandHistoryDescriptor PhaseTransition()
        {
            return new BattleCommandHistoryDescriptor(
                BattleHistoryKind.PhaseTransition,
                BattleHistoryActionType.PhaseTransition,
                null,
                null,
                null);
        }
    }

    public interface IBattleHistoryCommand
    {
        BattleCommandHistoryDescriptor GetHistoryDescriptor();
    }
}
