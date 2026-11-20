namespace FE.Core
{
    public sealed class MoveAndWaitCommand : IBattleCommand
    {
        public UnitId UnitId { get; }
        public GridCoord Destination { get; }

        public MoveAndWaitCommand(
            UnitId unitId,
            GridCoord destination)
        {
            UnitId = unitId;
            Destination = destination;
        }

        public BattleCommandResult Execute(BattleContext context)
        {
            if (context == null)
            {
                return BattleCommandResult.Failure(
                    "Battle context is null.");
            }

            BattleUnit unit = context.Units.GetUnitById(UnitId);

            if (unit == null)
            {
                return BattleCommandResult.Failure(
                    $"Unit {UnitId} does not exist.");
            }

            if (unit.IsDead)
            {
                return BattleCommandResult.Failure(
                    $"Unit {UnitId} is defeated.");
            }

            if (unit.HasActed)
            {
                return BattleCommandResult.Failure(
                    $"Unit {UnitId} has already acted.");
            }

            if (!MovementQuery.TryCalculate(
                    unit,
                    context.Map,
                    context.Units,
                    out MovementRange range))
            {
                return BattleCommandResult.Failure(
                    $"Could not calculate movement for unit {UnitId}.");
            }

            if (!range.CanStopAt(Destination))
            {
                return BattleCommandResult.Failure(
                    $"Unit {UnitId} cannot stop at {Destination}.");
            }

            if (unit.CurGrid != Destination)
            {
                GridCoord? movedPosition = context.Units.MoveUnit(
                    unit,
                    Destination,
                    context.Map);

                if (!movedPosition.HasValue)
                {
                    return BattleCommandResult.Failure(
                        $"Unit {UnitId} failed to move to {Destination}.");
                }
            }

            unit.MarkActed();
            return BattleCommandResult.Success();
        }
    }
}
