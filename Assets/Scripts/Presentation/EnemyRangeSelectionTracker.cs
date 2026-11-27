using System.Collections.Generic;
using FE.Core;

namespace FE.Presentation
{
    /// <summary>
    /// Keeps the enemies explicitly selected for range inspection while
    /// allowing their range visuals to be hidden during another phase.
    /// </summary>
    public sealed class EnemyRangeSelectionTracker
    {
        private readonly HashSet<UnitId> trackedIds = new();

        public bool IsTracked(UnitId unitId)
        {
            return trackedIds.Contains(unitId);
        }

        /// <summary>
        /// Returns true when the unit is tracked after toggling.
        /// </summary>
        public bool Toggle(UnitId unitId)
        {
            if (trackedIds.Remove(unitId))
            {
                return false;
            }

            trackedIds.Add(unitId);
            return true;
        }

        public bool Remove(UnitId unitId)
        {
            return trackedIds.Remove(unitId);
        }

        public void Clear()
        {
            trackedIds.Clear();
        }

        public List<UnitId> CreateSnapshot()
        {
            return new List<UnitId>(trackedIds);
        }
    }
}
