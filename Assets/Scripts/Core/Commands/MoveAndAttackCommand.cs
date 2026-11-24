namespace FE.Core
{
    public sealed class MoveAndAttackCommand : IBattleCommand
    {
        public UnitId AttackerId { get; }
        public UnitId DefenderId { get; }
        public GridCoord Destination { get; }

        public MoveAndAttackCommand(
            UnitId attackerId,
            UnitId defenderId,
            GridCoord destination)
        {
            AttackerId = attackerId;
            DefenderId = defenderId;
            Destination = destination;
        }

        public BattleCommandResult Execute(BattleContext context)
        {
            if (context == null)
            {
                return BattleCommandResult.Failure(
                    "Battle context is null.");
            }

            BattleUnit attacker = context.Units.GetUnitById(AttackerId);
            BattleUnit defender = context.Units.GetUnitById(DefenderId);

            if (attacker == null || defender == null)
            {
                return BattleCommandResult.Failure(
                    "Attacker or defender does not exist.");
            }

            if (attacker.IsDead || defender.IsDead)
            {
                return BattleCommandResult.Failure(
                    "Defeated units cannot participate in combat.");
            }

            if (attacker.Fac == defender.Fac)
            {
                return BattleCommandResult.Failure(
                    "Friendly units cannot attack each other.");
            }

            if (attacker.HasActed)
            {
                return BattleCommandResult.Failure(
                    $"Unit {AttackerId} has already acted.");
            }

            if (!MovementQuery.TryCalculate(
                    attacker,
                    context.Map,
                    context.Units,
                    out MovementRange range))
            {
                return BattleCommandResult.Failure(
                    $"Could not calculate movement for unit {AttackerId}.");
            }

            if (!range.CanStopAt(Destination))
            {
                return BattleCommandResult.Failure(
                    $"Unit {AttackerId} cannot stop at {Destination}.");
            }

            if (!CombatForecastQuery.TryCalculate(
                    context,
                    attacker,
                    defender,
                    Destination,
                    out CombatForecastResult forecast))
            {
                return BattleCommandResult.Failure(
                    $"Unit {AttackerId} cannot attack {DefenderId} " +
                    $"from {Destination}.");
            }

            if (attacker.CurGrid != Destination)
            {
                GridCoord? movedPosition = context.Units.MoveUnit(
                    attacker,
                    Destination,
                    context.Map);

                if (!movedPosition.HasValue)
                {
                    return BattleCommandResult.Failure(
                        $"Unit {AttackerId} failed to move to {Destination}.");
                }
            }

            CombatAttackResult attackResult =
                CombatAttackResolver.Resolve(
                    context,
                    attacker,
                    defender,
                    forecast);

            attacker.MarkActed();
            return BattleCommandResult.Success(attackResult);
        }
    }
}
