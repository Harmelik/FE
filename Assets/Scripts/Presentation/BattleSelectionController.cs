using System;
using System.Collections.Generic;
using FE.Core;
using UnityEngine;

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

        public void Init(GridMapView mapView, BattleContext context, 
            GridPointerController gridPointerController, BattlePathView battlePathView)
        {
            if (mapView == null || context == null 
            || gridPointerController == null || battlePathView == null) return;

            gridMapView = mapView;
            this.context = context;
            battlePath = battlePathView;

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

        public void OnClickPlayer(BattleUnit unit)
        {
            if (unit == null || unit.IsDead || unit.Fac != Faction.Player || unit.HasActed) return;
            
            //点击正在操作中的弟子，取消该弟子的当前选择
            if (unit == curSelectPlayer)
            {
                curSelectPlayer = null;
                curMovementRange = null;
                gridMapView.SetOffMovement(unit.Id);
                gridMapView.ClearSelected();
                battlePath.HidePath();
            }
            else
            {
                if(curSelectPlayer != null)
                {
                    gridMapView.SetOffMovement(curSelectPlayer.Id);
                    gridMapView.ClearSelected();

                    curSelectPlayer = null;
                    curMovementRange = null;
                    battlePath.HidePath();
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
