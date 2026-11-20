using System;
using System.Collections;
using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleEncounterConfig encounterConfig;
        [SerializeField] private GridMapView gridMap;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private GridPointerController gridPointerController;
        [SerializeField] private BattleUnitsViewController battleUnitsViewController;
        [SerializeField] private BattlePathView battlePathView;
        [SerializeField] private BattleActionMenuView battleActionMenuView;
        [SerializeField] private BattleForecastView battleForecastView;


        private BattleSelectionController battleSelectionController;
        public void Start()
        {
            BattleSetUpFactory factory = new BattleSetUpFactory();
            battleSelectionController = new BattleSelectionController();
            BattleContext context = factory.CreateContext(encounterConfig);
            BattleCommandProcessor commandProcessor =
                new BattleCommandProcessor(context);

            GridGeometry gg = new GridGeometry(gridMap.transform.position, cellSize);
            gridMap.CreateTiles(context.Map, gg);
            gridPointerController.Init(gridMap, gg, context.Map);
            battlePathView.Init(gg);
            battleUnitsViewController.Init(context, gg);

            battleSelectionController.Init(gridMap, context, gridPointerController, 
                battlePathView, battleUnitsViewController, battleActionMenuView,
                commandProcessor, battleForecastView);
        }

        private void OnDestroy()
        {
            battleSelectionController?.Dispose();
        }
    }
}

