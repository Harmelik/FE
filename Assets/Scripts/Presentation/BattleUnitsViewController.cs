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
    }
}

