using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FE.Core
{
    public sealed class BattleHistoryService
    {
        private readonly BattleContext context;
        private readonly List<BattleHistoryEntry> entries = new();
        private readonly ReadOnlyCollection<BattleHistoryEntry> entriesView;
        private int maxEntries = int.MaxValue;

        public IReadOnlyList<BattleHistoryEntry> Entries => entriesView;
        public int Count => entries.Count;
        public int MaxEntries => maxEntries;
        public bool CanRewind => entries.Count > 0;

        public BattleHistoryService(BattleContext context)
        {
            this.context = context ??
                throw new ArgumentNullException(nameof(context));
            entriesView =
                new ReadOnlyCollection<BattleHistoryEntry>(entries);
        }

        public void SetMaxEntries(int limit)
        {
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(limit),
                    limit,
                    "History limit must be greater than zero.");
            }

            maxEntries = limit;
            TrimOldestEntries();
        }

        public bool TryRewindToBefore(int entryIndex)
        {
            if (entryIndex < 0 || entryIndex >= entries.Count)
            {
                return false;
            }

            BattleSnapshot snapshot =
                entries[entryIndex].BeforeState;
            BattleSnapshotService.Restore(context, snapshot);
            entries.RemoveRange(entryIndex, entries.Count - entryIndex);
            return true;
        }

        internal void RecordUnitAction(
            BattleSnapshot beforeState,
            UnitId actorUnitId,
            Faction actorFaction,
            IReadOnlyList<BattleHistoryStep> steps)
        {
            var copiedSteps =
                new BattleHistoryStep[steps.Count];

            for (int i = 0; i < steps.Count; i++)
            {
                copiedSteps[i] = steps[i];
            }

            entries.Add(new BattleHistoryEntry(
                BattleHistoryKind.UnitAction,
                beforeState,
                actorUnitId,
                actorFaction,
                copiedSteps));
            TrimOldestEntries();
        }

        internal void RecordPhaseTransition(
            BattleSnapshot beforeState,
            BattlePhaseTransitionResult transition)
        {
            entries.Add(new BattleHistoryEntry(
                BattleHistoryKind.PhaseTransition,
                beforeState,
                null,
                null,
                Array.Empty<BattleHistoryStep>(),
                transition));
            TrimOldestEntries();
        }

        private void TrimOldestEntries()
        {
            int removeCount = entries.Count - maxEntries;
            if (removeCount > 0)
            {
                entries.RemoveRange(0, removeCount);
            }
        }
    }
}
