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

        private BattleSelectionController battleSelectionController;
        public void Start()
        {
            BattleSetUpFactory factory = new BattleSetUpFactory();
            battleSelectionController = new BattleSelectionController();
            BattleContext context = factory.CreateContext(encounterConfig);

            GridGeometry gg = new GridGeometry(gridMap.transform.position, cellSize);
            gridMap.CreateTiles(context.Map, gg);
            gridPointerController.Init(gridMap, gg, context.Map);
            battlePathView.Init(gg);
            battleSelectionController.Init(gridMap, context, gridPointerController, battlePathView);
            battleUnitsViewController.Init(context, gg);
        }

        private void OnDestroy()
        {
            battleSelectionController?.Dispose();
        }
    }
}

