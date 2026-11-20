using FE.Core;
using System;
using System.Collections.Generic;
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
        private BattleUnitsViewController battleUnitsViewController;
        private BattleActionMenuView actionMenuView;
        private BattleForecastView resultView;
        private BattleCommandProcessor commandProcessor;
        private GridCoord? previewGrid;
        private bool isMovePreview;
        private int curSelectEnemyIndex;
        private bool isBattleForecastOpen;
        private List<BattleUnit> curAttackTargets;
        private BattleUnitView curOpUnitView;

        public void Init(GridMapView mapView, BattleContext context, 
            GridPointerController gridPointerController, BattlePathView battlePathView,
            BattleUnitsViewController battleUnitsViewController, BattleActionMenuView actionMenuView,
            BattleCommandProcessor commandProcessor, BattleForecastView resultView)
        {
            if (mapView == null || context == null 
            || gridPointerController == null || battlePathView == null
            || battleUnitsViewController == null || actionMenuView == null
            || commandProcessor == null || resultView == null) return;

            gridMapView = mapView;
            this.context = context;
            battlePath = battlePathView;
            this.battleUnitsViewController = battleUnitsViewController;
            this.actionMenuView = actionMenuView;
            this.commandProcessor = commandProcessor;

            if (pointer != null)
            {
                pointer.TileSelected -= OnClickGrid;
                pointer.HoverGridChanged -= OnHoverChanged;
            }

            pointer = gridPointerController;
            pointer.TileSelected += OnClickGrid;
            pointer.HoverGridChanged += OnHoverChanged;

            if (resultView != null)
            {
                resultView.ChangeToNextTarget -= OnEPressed;
                resultView.ChangeToPreviousTarget -= OnQPressed;
                resultView.EscapeAction -= OnEscapePressed;
            }
            this.resultView = resultView;
            resultView.ChangeToNextTarget += OnEPressed;
            resultView.ChangeToPreviousTarget += OnQPressed;
            resultView.EscapeAction += OnEscapePressed;
        }

        public void OnClickGrid(GridCoord grid)
        {
            if (isMovePreview || isBattleForecastOpen) return;

            if (curSelectPlayer == null)
            {
                BattleUnit unit = context.Units.GetUnitByGrid(grid);

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
                BattleUnit unitAtTile = context.Units.GetUnitByGrid(grid);

                if (unitAtTile != null && unitAtTile != curSelectPlayer)
                {
                    if (unitAtTile.Fac == Faction.Player)
                    {
                        OnClickPlayer(unitAtTile);
                    }

                    return;
                }

                if (curMovementRange == null ||
                    !curMovementRange.CanStopAt(grid))
                {
                    return;
                }

                if (!battleUnitsViewController.TryGetUnitViewById(
                        curSelectPlayer.Id,
                        out BattleUnitView curOpUnitView))
                {
                    return;
                }

                if (!curMovementRange.TryGetPath(
                        grid,
                        out IReadOnlyList<GridCoord> path))
                {
                    return;
                }

                previewGrid = grid;
                isMovePreview = true;

                BattleUnit selectedUnit = curSelectPlayer;

                //TODO 等角色到位置之后再打开菜单and显示战斗范围
                curOpUnitView.MoveToPreviewPositionByPath(path);
                curMovementRange = null;
                gridMapView.SetOffMovement(selectedUnit.Id);
                gridMapView.SetOffAttackRange(selectedUnit.Id);
                gridMapView.ClearSelected();
                battlePath.HidePath();

                HashSet<GridCoord> exactAttackRange = 
                    AttackRangeQuery.TryCalculateAttackRangesByGrid(
                        selectedUnit,
                        context.Map,
                        previewGrid.Value);

                bool canAttack = false;

                List<BattleUnit> attackTargets = new List<BattleUnit>();
                if (exactAttackRange != null)
                {
                    attackTargets = AttackTargetQuery.FindTargetsInRange(
                            context,
                            selectedUnit,
                            exactAttackRange);

                    canAttack = attackTargets.Count > 0;
                }

                if (canAttack)
                {
                    gridMapView.SetInAttackRange(exactAttackRange, curSelectPlayer.Id);
                }

                curSelectEnemyIndex = 0;
                curAttackTargets = attackTargets;
                this.curOpUnitView = curOpUnitView;
                actionMenuView.Show(
                    selectedUnit,
                    canAttack,
                    curAttackTargets.Count > 0 ? 
                        () => CommitMoveAndAttack(curSelectPlayer,attackTargets) : null,
                    () => CommitMoveAndWait(
                        selectedUnit,
                        curOpUnitView,
                        grid),
                    () => CancelMovePreview(
                        selectedUnit,
                        curOpUnitView));
            }
        }

        private void CommitMoveAndWait(
            BattleUnit unit,
            BattleUnitView unitView,
            GridCoord destination)
        {
            BattleCommandResult result = commandProcessor.Execute(
                new MoveAndWaitCommand(unit.Id, destination));

            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
            }

            unitView.RefreshToDefaultPosition();
            ClearAllSelectionStatus(unit);
        }

        private void CommitMoveAndAttack(BattleUnit attacker, List<BattleUnit> attackTargets)
        {
            if (attacker == null || attackTargets == null || attackTargets.Count <= 0) return;

            actionMenuView.Hide();
            isBattleForecastOpen = true;
            pointer.lockInput = true;

            curSelectEnemyIndex = 0;
            RefreshCurrentTarget();
        }

        private void RefreshCurrentTarget()
        {
            if (!isBattleForecastOpen ||
                curSelectPlayer == null ||
                curAttackTargets == null ||
                curAttackTargets.Count == 0)
            {
                return;
            }

            curSelectEnemyIndex =
                (curSelectEnemyIndex % curAttackTargets.Count +
                 curAttackTargets.Count) % curAttackTargets.Count;

            BattleUnit target = curAttackTargets[curSelectEnemyIndex];

            resultView.Show(curSelectPlayer, target);

            gridMapView.ClearSelected();
            gridMapView.SetSelected(target.CurGrid);
        }

        private void OnQPressed()
        {
            if (!isBattleForecastOpen ||
                curAttackTargets == null ||
                curAttackTargets.Count == 0)
            {
                return;
            }

            curSelectEnemyIndex = (curSelectEnemyIndex - 1 + curAttackTargets.Count) % curAttackTargets.Count;

            RefreshCurrentTarget();
        }

        private void OnEPressed()
        {
            if (!isBattleForecastOpen ||
                curAttackTargets == null ||
                curAttackTargets.Count == 0)
            {
                return;
            }

            curSelectEnemyIndex = (curSelectEnemyIndex + 1) % curAttackTargets.Count;

            RefreshCurrentTarget();
        }

        private void OnEscapePressed()
        {
            if (!isBattleForecastOpen) return;
            if (curSelectPlayer == null || curAttackTargets == null) return;

            resultView.Hide();
            isBattleForecastOpen = false;
            pointer.lockInput = false;

            curSelectEnemyIndex = 0;

            actionMenuView.Show(
                curSelectPlayer,
                true,
                curAttackTargets.Count > 0 ?
                    () => CommitMoveAndAttack(curSelectPlayer, curAttackTargets) : null,
                () => CommitMoveAndWait(
                    curSelectPlayer,
                    curOpUnitView,
                    previewGrid.Value),
                () => CancelMovePreview(
                    curSelectPlayer,
                    curOpUnitView));
        }

        private void CancelMovePreview(
            BattleUnit unit,
            BattleUnitView unitView)
        {
            unitView.RefreshToDefaultPosition();
            ClearAllSelectionStatus(unit);
        }

        private void ClearAllSelectionStatus(BattleUnit unit)
        {
            curSelectPlayer = null;
            curMovementRange = null;
            gridMapView.SetOffMovement(unit.Id);
            gridMapView.SetOffAttackRange(unit.Id);
            gridMapView.ClearSelected();
            battlePath.HidePath();
            isMovePreview = false;
            previewGrid = null;
            actionMenuView.Hide();
        }

        public void OnClickPlayer(BattleUnit unit)
        {
            if (unit == null || unit.IsDead || unit.Fac != Faction.Player || unit.HasActed) return;
            
            ////点击正在操作中的弟子，取消该弟子的当前选择
            //if (unit == curSelectPlayer)
            //{
            //    ClearAllSelectionStatus(unit);
            //}
            //else
            //{
                if(curSelectPlayer != null && curSelectPlayer != unit)
                {
                    ClearAllSelectionStatus(curSelectPlayer);
                }
                if (!MovementQuery.TryCalculate(unit, context.Map,
                        context.Units, out var result))
                {
                    if(curSelectPlayer != null) ClearAllSelectionStatus(curSelectPlayer);
                    return;
                }

                var range = AttackRangeQuery.CalculateCoverage(unit, context.Map, result);
                if (range == null) return;

                gridMapView.SetInMovement(result, unit.Id);
                gridMapView.SetInAttackRange(range,unit.Id);
                gridMapView.SetSelected(unit.CurGrid);
                curMovementRange = result;

                curSelectPlayer = unit;
            //}
        }

        public void OnClickEnemies(BattleUnit unit)
        {
            if (unit == null || unit.IsDead || unit.Fac != Faction.Enemy) return;

            //点击已经被选中的敌人，取消该敌人的范围显示
            if (selectedEnemies.Contains(unit))
            {
                selectedEnemies.Remove(unit);
                gridMapView.SetOffMovement(unit.Id);
                gridMapView.SetOffAttackRange(unit.Id);
            }
            else
            {
                if (!MovementQuery.TryCalculate(unit, context.Map, context.Units, out var result1))
                {
                    selectedEnemies.Remove(unit);
                    gridMapView.SetOffMovement(unit.Id);
                    gridMapView.SetOffAttackRange(unit.Id);
                    return;
                }
                var range = AttackRangeQuery.CalculateCoverage(unit, context.Map, result1);
                if(range == null) return;

                gridMapView.SetInMovement(result1, unit.Id);
                gridMapView.SetInAttackRange(range, unit.Id);

                selectedEnemies.Add(unit);
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

            if (resultView != null)
            {
                resultView.ChangeToNextTarget -= OnEPressed;
                resultView.ChangeToPreviousTarget -= OnQPressed;
                resultView.EscapeAction -= OnEscapePressed;
                resultView.Hide();
            }

            pointer.lockInput = false;
        }
    }
}
