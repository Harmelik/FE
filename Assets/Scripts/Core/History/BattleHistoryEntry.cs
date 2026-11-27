using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FE.Core
{
    public readonly struct BattleHistoryStep
    {
        public BattleHistoryActionType ActionType { get; }
        public UnitId? TargetUnitId { get; }
        public GridCoord? Destination { get; }
        public CombatAttackResult? AttackResult { get; }

        public BattleHistoryStep(
            BattleHistoryActionType actionType,
            UnitId? targetUnitId,
            GridCoord? destination,
            CombatAttackResult? attackResult)
        {
            ActionType = actionType;
            TargetUnitId = targetUnitId;
            Destination = destination;
            AttackResult = attackResult;
        }
    }

    public sealed class BattleHistoryEntry
    {
        private readonly BattleHistoryStep[] steps;

        public BattleHistoryKind Kind { get; }
        public BattleSnapshot BeforeState { get; }
        public int TurnNumber => BeforeState.TurnNumber;
        public UnitId? ActorUnitId { get; }
        public Faction? ActorFaction { get; }
        public BattlePhaseTransitionResult? PhaseTransition { get; }
        public ReadOnlyCollection<BattleHistoryStep> Steps { get; }
        public BattleHistoryActionType PrimaryActionType =>
            Kind == BattleHistoryKind.PhaseTransition
                ? BattleHistoryActionType.PhaseTransition
                : steps.Length > 0
                    ? steps[steps.Length - 1].ActionType
                    : BattleHistoryActionType.Other;
        public UnitId? TargetUnitId =>
            steps.Length > 0
                ? steps[steps.Length - 1].TargetUnitId
                : null;
        public GridCoord? Destination =>
            steps.Length > 0
                ? steps[steps.Length - 1].Destination
                : null;
        public CombatAttackResult? AttackResult =>
            steps.Length > 0
                ? steps[steps.Length - 1].AttackResult
                : null;

        internal BattleHistoryEntry(
            BattleHistoryKind kind,
            BattleSnapshot beforeState,
            UnitId? actorUnitId,
            Faction? actorFaction,
            BattleHistoryStep[] steps,
            BattlePhaseTransitionResult? phaseTransition = null)
        {
            if (beforeState == null)
            {
                throw new ArgumentNullException(nameof(beforeState));
            }

            if (kind == BattleHistoryKind.UnitAction &&
                (!actorUnitId.HasValue || !actorFaction.HasValue))
            {
                throw new ArgumentException(
                    "Unit history entries require an actor.");
            }

            if (kind == BattleHistoryKind.PhaseTransition &&
                !phaseTransition.HasValue)
            {
                throw new ArgumentException(
                    "Phase history entries require transition data.");
            }

            this.steps = steps ?? Array.Empty<BattleHistoryStep>();
            Kind = kind;
            BeforeState = beforeState;
            ActorUnitId = actorUnitId;
            ActorFaction = actorFaction;
            PhaseTransition = phaseTransition;
            Steps = Array.AsReadOnly(this.steps);
        }
    }
}
