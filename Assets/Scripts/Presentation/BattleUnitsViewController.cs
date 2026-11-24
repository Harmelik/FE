using System;
using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleUnitsViewController : MonoBehaviour
    {
        [SerializeField] private BattleUnitView unitView;
        private Dictionary<UnitId, BattleUnitView> unitViews;

        public void Init(BattleContext context, GridGeometry gg)
        {
            unitViews = new Dictionary<UnitId, BattleUnitView>();
            foreach (var unit in context.Units.AllUnits)
            {
                var newUnitView = GameObject.Instantiate(unitView, this.transform);
                newUnitView.Init(unit, gg);
                newUnitView.name = $"Unit_{unit.Id}_{unit.DisplayName}";
                unitViews.Add(unit.Id, newUnitView);
            }
        }

        public bool TryGetUnitViewById(UnitId id, out BattleUnitView unitView)
        {
            unitView = null;

            return unitViews != null &&
                   unitViews.TryGetValue(id, out unitView) &&
                   unitView != null;
        }

        public void RefreshAfterCommand(BattleContext context)
        {
            if (context == null || unitViews == null)
            {
                return;
            }

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                if (!unitViews.TryGetValue(unit.Id, out BattleUnitView view) ||
                    view == null)
                {
                    continue;
                }

                view.gameObject.SetActive(!unit.IsDead);

                if (!unit.IsDead)
                {
                    view.RefreshToDefaultPosition();
                    view.SetGrayed(unit.HasActed);
                }
            }
        }

        public void RefreshActionStates(BattleContext context)
        {
            if (context == null || unitViews == null)
            {
                return;
            }

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                if (unit.IsDead ||
                    !unitViews.TryGetValue(unit.Id, out BattleUnitView view) ||
                    view == null)
                {
                    continue;
                }

                view.SetGrayed(unit.HasActed);
            }
        }
    }
}

