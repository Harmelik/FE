using System;
using System.Collections.Generic;

namespace FE.Core
{
    public sealed class BattleCommandProcessor
    {
        private readonly BattleContext context;
        private UnitActionTransaction activeUnitAction;

        public BattleHistoryService History { get; }

        public BattleCommandProcessor(BattleContext context)
        {
            this.context = context ??
                throw new ArgumentNullException(nameof(context));
            History = new BattleHistoryService(context);
        }

        public bool BeginUnitAction(UnitId unitId)
        {
            if (activeUnitAction != null)
            {
                return activeUnitAction.UnitId == unitId;
            }

            BattleUnit unit = context.Units.GetUnitById(unitId);
            if (unit == null || unit.IsDead)
            {
                return false;
            }

            activeUnitAction = new UnitActionTransaction(
                unit.Id,
                unit.Fac,
                BattleSnapshotService.Capture(context));
            return true;
        }

        public bool CommitUnitAction()
        {
            if (activeUnitAction == null ||
                activeUnitAction.Steps.Count == 0)
            {
                return false;
            }

            History.RecordUnitAction(
                activeUnitAction.BeforeState,
                activeUnitAction.UnitId,
                activeUnitAction.Faction,
                activeUnitAction.Steps);
            activeUnitAction = null;
            return true;
        }

        public void CancelUnitAction()
        {
            activeUnitAction = null;
        }

        public bool TryRewindToBefore(int historyIndex)
        {
            if (historyIndex < 0 ||
                historyIndex >= History.Count)
            {
                return false;
            }

            CancelUnitAction();
            return History.TryRewindToBefore(historyIndex);
        }

        public BattleCommandResult Execute(IBattleCommand command)
        {
            if (command == null)
            {
                return BattleCommandResult.Failure(
                    "Cannot execute a null battle command.");
            }

            BattleCommandHistoryDescriptor? descriptor =
                command is IBattleHistoryCommand historyCommand
                    ? (BattleCommandHistoryDescriptor?)
                      historyCommand.GetHistoryDescriptor()
                    : null;

            UnitActionTransaction transaction = null;
            bool createdAutomaticTransaction = false;
            BattleSnapshot phaseSnapshot = null;

            if (descriptor.HasValue)
            {
                if (descriptor.Value.Kind ==
                    BattleHistoryKind.UnitAction)
                {
                    if (!TryPrepareUnitTransaction(
                            descriptor.Value,
                            out transaction,
                            out createdAutomaticTransaction,
                            out string transactionError))
                    {
                        return BattleCommandResult.Failure(
                            transactionError);
                    }
                }
                else
                {
                    if (activeUnitAction != null)
                    {
                        return BattleCommandResult.Failure(
                            "Cannot change phase while a unit action is open.");
                    }

                    phaseSnapshot =
                        BattleSnapshotService.Capture(context);
                }
            }

            BattleCommandResult result = command.Execute(context);

            if (!result.IsSuccess ||
                context.Turns.CurrentPhase == BattlePhase.BattleEnded)
            {
                if (createdAutomaticTransaction)
                {
                    activeUnitAction = null;
                }

                return result;
            }

            if (BattleOutcomeQuery.TryEvaluate(
                    context,
                    out BattleOutcomeResult outcome))
            {
                context.Turns.EndBattle(outcome);
                result = result.WithBattleOutcome(outcome);
            }

            if (descriptor.HasValue)
            {
                if (descriptor.Value.Kind ==
                    BattleHistoryKind.UnitAction &&
                    transaction != null)
                {
                    transaction.Steps.Add(
                        CreateStep(
                            descriptor.Value,
                            result));

                    if (createdAutomaticTransaction)
                    {
                        CommitUnitAction();
                    }
                }
                else if (descriptor.Value.Kind ==
                         BattleHistoryKind.PhaseTransition &&
                         phaseSnapshot != null &&
                         result.IsSuccess &&
                         result.PhaseTransition.HasValue)
                {
                    History.RecordPhaseTransition(
                        phaseSnapshot,
                        result.PhaseTransition.Value);
                }
            }

            if (context.Turns.CurrentPhase ==
                    BattlePhase.BattleEnded &&
                activeUnitAction != null)
            {
                CommitUnitAction();
            }

            return result;
        }

        private bool TryPrepareUnitTransaction(
            BattleCommandHistoryDescriptor descriptor,
            out UnitActionTransaction transaction,
            out bool createdAutomaticTransaction,
            out string error)
        {
            transaction = null;
            createdAutomaticTransaction = false;
            error = string.Empty;

            if (!descriptor.ActorUnitId.HasValue)
            {
                error = "Unit action history has no actor.";
                return false;
            }

            UnitId actorId = descriptor.ActorUnitId.Value;

            if (activeUnitAction != null)
            {
                if (activeUnitAction.UnitId != actorId)
                {
                    error =
                        $"Unit action transaction belongs to " +
                        $"{activeUnitAction.UnitId}, not {actorId}.";
                    return false;
                }

                transaction = activeUnitAction;
                return true;
            }

            BattleUnit actor =
                context.Units.GetUnitById(actorId);
            if (actor == null || actor.IsDead)
            {
                return true;
            }

            transaction = new UnitActionTransaction(
                actor.Id,
                actor.Fac,
                BattleSnapshotService.Capture(context));
            activeUnitAction = transaction;
            createdAutomaticTransaction = true;
            return true;
        }

        private static BattleHistoryStep CreateStep(
            BattleCommandHistoryDescriptor descriptor,
            BattleCommandResult result)
        {
            return new BattleHistoryStep(
                descriptor.ActionType,
                descriptor.TargetUnitId,
                descriptor.Destination,
                result.AttackResult);
        }

        private sealed class UnitActionTransaction
        {
            public UnitId UnitId { get; }
            public Faction Faction { get; }
            public BattleSnapshot BeforeState { get; }
            public List<BattleHistoryStep> Steps { get; } = new();

            public UnitActionTransaction(
                UnitId unitId,
                Faction faction,
                BattleSnapshot beforeState)
            {
                UnitId = unitId;
                Faction = faction;
                BeforeState = beforeState;
            }
        }
    }
}
