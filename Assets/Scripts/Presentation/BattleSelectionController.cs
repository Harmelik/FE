using FE.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

namespace FE.Presentation
{
    /// <summary>
    /// 判断点击具体点击的是哪个物体
    /// </summary>
    public class BattleSelectionController:IDisposable
    {
        private BattleUnit curSelectPlayer;
        private HashSet<BattleUnit> selectedEnemies = new HashSet<BattleUnit>();
        private GridMapView gridMapView;
        private BattleContext context;
        private GridPointerController pointer;
        private BattlePathView battlePath;
        private MovementRange curMovementRange;
        private BattleUnitsViewController battleUnitsViewController;
        private BattleActionMenuView actionMenuView;
        private GridCoord? previewGrid;
        private bool isMovePreview;

        public void Init(GridMapView mapView, BattleContext context, 
            GridPointerController gridPointerController, BattlePathView battlePathView,
            BattleUnitsViewController battleUnitsViewController, BattleActionMenuView actionMenuView)
        {
            if (mapView == null || context == null 
            || gridPointerController == null || battlePathView == null
            || battleUnitsViewController == null || actionMenuView == null) return;

            gridMapView = mapView;
            this.context = context;
            battlePath = battlePathView;
            this.battleUnitsViewController = battleUnitsViewController;
            this.actionMenuView = actionMenuView;

            if (pointer != null)
            {
                pointer.TileSelected -= OnClickGrid;
                pointer.HoverGridChanged -= OnHoverChanged;
            }

            pointer = gridPointerController;
            pointer.TileSelected += OnClickGrid;
            pointer.HoverGridChanged += OnHoverChanged;
        }

        public void OnClickGrid(GridCoord grid)
        {
            if (isMovePreview) return;
            if (curSelectPlayer == null)
            {
                var unit = context.Units.GetUnitByGrid(grid);
                if (unit == null)
                {
                    Debug.Log("点击空地");
                    gridMapView.SetSelected(grid);
                }
                else if (unit.Fac == Faction.Enemy)
                {
                    OnClickEnemies(unit);
                }
                else if (unit.Fac == Faction.Player)
                {
                    OnClickPlayer(unit);
                }
            }
            else
            {
                var unit = context.Units.GetUnitByGrid(grid);
                if (unit != null)
                {
                    if (unit.Fac == Faction.Player)
                    {
                        OnClickPlayer(unit);
                    }
                    return;
                }

                if (!curMovementRange.CanStopAt(grid)) return;

                if (battleUnitsViewController.TryGetUnitViewById(curSelectPlayer.Id, 
                        out var curOpUnitView))
                {
                    previewGrid = grid;
                    isMovePreview = true;

                    curOpUnitView.MoveToPreviewPosition(grid);

                    curMovementRange = null;
                    gridMapView.SetOffMovement(curSelectPlayer.Id);
                    gridMapView.ClearSelected();
                    battlePath.HidePath();

                    actionMenuView.Show(curSelectPlayer, false, null, () =>
                    {
                        var destination = context.Units.MoveUnit(curSelectPlayer, grid, context.Map);
                        if (!destination.HasValue)
                        {
                            curOpUnitView.RefreshToDefaultPosition();
                            ClearAllSelectionStatus(curSelectPlayer);
                            return;
                        }

                        curSelectPlayer.MarkActed();
                        curOpUnitView.RefreshToDefaultPosition();
                        ClearAllSelectionStatus(curSelectPlayer);
                    }, () =>
                    {
                        curOpUnitView.RefreshToDefaultPosition();
                        ClearAllSelectionStatus(curSelectPlayer);
                    });
                }
            }
        }

        private void ClearAllSelectionStatus(BattleUnit unit)
        {
            curSelectPlayer = null;
            curMovementRange = null;
            gridMapView.SetOffMovement(unit.Id);
            gridMapView.ClearSelected();
            battlePath.HidePath();
            isMovePreview = false;
            previewGrid = null;
        }

        public void OnClickPlayer(BattleUnit unit)
        {
            if (unit == null || unit.IsDead || unit.Fac != Faction.Player || unit.HasActed) return;
            
            //点击正在操作中的弟子，取消该弟子的当前选择
            if (unit == curSelectPlayer)
            {
                ClearAllSelectionStatus(unit);
            }
            else
            {
                if(curSelectPlayer != null)
                {
                    ClearAllSelectionStatus(curSelectPlayer);
                }
                if (MovementQuery.TryCalculate(unit, context.Map, context.Units, out var result))
                { 
                    gridMapView.SetInMovement(result,unit.Id);
                    gridMapView.SetSelected(unit.CurGrid);
                    curMovementRange = result;

                    curSelectPlayer = unit;
                }
            }
        }

        public void OnClickEnemies(BattleUnit unit)
        {
            if (unit == null || unit.IsDead || unit.Fac != Faction.Enemy) return;

            //点击已经被选中的敌人，取消该敌人的范围显示
            if (selectedEnemies.Contains(unit))
            {
                selectedEnemies.Remove(unit);
                gridMapView.SetOffMovement(unit.Id);
            }
            else
            {
                if (MovementQuery.TryCalculate(unit, context.Map, context.Units, out var result))
                {
                    gridMapView.SetInMovement(result, unit.Id);
                    selectedEnemies.Add(unit);
                }
            }
        }

        public void OnHoverChanged(GridCoord? grid)
        {
            if (battlePath == null || curSelectPlayer == null 
            || curMovementRange == null || !grid.HasValue)
            {
                battlePath?.HidePath();
                return;
            }

            if (!curMovementRange.TryGetPath(grid.Value, out var path))
            {
                battlePath?.HidePath();
                return;
            }

            battlePath.ShowPath(path);
        }

        public void Dispose()
        {
            if (pointer != null)
            {
                pointer.TileSelected -= OnClickGrid;
                pointer.HoverGridChanged -= OnHoverChanged;
                battlePath?.HidePath();
            }
        }
    }
}
