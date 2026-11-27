using System;
using System.Collections.Generic;

namespace FE.Core
{
    public static class BattleSnapshotService
    {
        public static BattleSnapshot Capture(BattleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            IReadOnlyList<BattleUnit> units = context.Units.AllUnits;
            var unitStates = new BattleUnitState[units.Count];

            for (int i = 0; i < units.Count; i++)
            {
                BattleUnit unit = units[i];
                unitStates[i] = new BattleUnitState(
                    unit.Id,
                    unit.CurHP,
                    unit.CurGrid,
                    unit.HasActed,
                    unit.EquippedWeapon);
            }

            return new BattleSnapshot(
                context.Map.Width,
                context.Map.Height,
                context.Turns.TurnNumber,
                context.Turns.CurrentPhase,
                context.Turns.CurrentOutcome,
                context.BattleRandom.CaptureState(),
                unitStates,
                context.Map.CaptureTerrainTypes());
        }

        public static void Restore(
            BattleContext context,
            BattleSnapshot snapshot)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            Validate(context, snapshot);

            context.Map.RestoreTerrainTypes(
                snapshot.CaptureTerrainTypes());

            foreach (BattleUnitState unitState in snapshot.UnitStates)
            {
                BattleUnit unit =
                    context.Units.GetUnitById(unitState.UnitId);
                unit.RestoreState(
                    unitState.CurrentHP,
                    unitState.Grid,
                    unitState.HasActed,
                    unitState.EquippedWeapon);
            }

            context.Units.RebuildIndexes();
            context.Turns.RestoreState(
                snapshot.TurnNumber,
                snapshot.Phase,
                snapshot.Outcome);
            context.BattleRandom.RestoreState(
                snapshot.RandomState);
        }

        private static void Validate(
            BattleContext context,
            BattleSnapshot snapshot)
        {
            if (context.Map.Width != snapshot.MapWidth ||
                context.Map.Height != snapshot.MapHeight)
            {
                throw new InvalidOperationException(
                    "Snapshot map dimensions do not match the battle.");
            }

            if (context.Units.AllUnits.Count !=
                snapshot.UnitStates.Count)
            {
                throw new InvalidOperationException(
                    "Snapshot unit count does not match the battle.");
            }

            var knownIds = new HashSet<UnitId>();
            var occupiedGrids = new HashSet<GridCoord>();

            foreach (BattleUnitState unitState in snapshot.UnitStates)
            {
                if (!knownIds.Add(unitState.UnitId))
                {
                    throw new InvalidOperationException(
                        $"Snapshot contains duplicate unit " +
                        $"{unitState.UnitId}.");
                }

                BattleUnit unit =
                    context.Units.GetUnitById(unitState.UnitId);
                if (unit == null)
                {
                    throw new InvalidOperationException(
                        $"Snapshot unit {unitState.UnitId} is missing.");
                }

                if (unitState.CurrentHP > unit.BaseStats.MaxHP)
                {
                    throw new InvalidOperationException(
                        $"Snapshot HP for {unitState.UnitId} is too high.");
                }

                if (!unit.UnitClass.CanEquip(
                        unitState.EquippedWeapon.Category))
                {
                    throw new InvalidOperationException(
                        $"Snapshot weapon for {unitState.UnitId} " +
                        "is invalid for its class.");
                }

                if (!context.Map.IsInside(unitState.Grid))
                {
                    throw new InvalidOperationException(
                        $"Snapshot grid for {unitState.UnitId} is outside the map.");
                }

                if (unitState.CurrentHP > 0 &&
                    !occupiedGrids.Add(unitState.Grid))
                {
                    throw new InvalidOperationException(
                        $"Snapshot has multiple living units on " +
                        $"{unitState.Grid}.");
                }
            }

            bool outcomeMatchesPhase =
                snapshot.Phase == BattlePhase.BattleEnded
                    ? snapshot.Outcome.IsBattleEnded
                    : !snapshot.Outcome.IsBattleEnded;

            if (!outcomeMatchesPhase)
            {
                throw new InvalidOperationException(
                    "Snapshot phase and battle outcome are inconsistent.");
            }
        }
    }
}
