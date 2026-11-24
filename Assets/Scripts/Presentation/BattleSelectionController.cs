using FE.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FE.Presentation
{
    public enum BattleStatus
    {
        Idle,
        UnitSelected,
        MovePreviewing,
        ActionMenu,
        BattleForecast,
        ResolvingCombat,
        PhaseTransition
    }

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
        private int curSelectEnemyIndex;
        private List<BattleUnit> curAttackTargets;
        private BattleUnit curAttackTarget;
        private BattleUnitView curOpUnitView;
        private BattleStatus battleStatus;
        private BattleCombatPresentationController combatPresentationController;

        public void Init(GridMapView mapView, BattleContext context, 
            GridPointerController gridPointerController, BattlePathView battlePathView,
            BattleUnitsViewController battleUnitsViewController, BattleActionMenuView actionMenuView,
            BattleCommandProcessor commandProcessor, BattleForecastView resultView,
            BattleCombatPresentationController combatPresentationController)
        {
            if (mapView == null || context == null 
            || gridPointerController == null || battlePathView == null
            || battleUnitsViewController == null || actionMenuView == null
            || commandProcessor == null || resultView == null
            || combatPresentationController == null) return;

            gridMapView = mapView;
            this.context = context;
            battlePath = battlePathView;
            this.battleUnitsViewController = battleUnitsViewController;
            this.actionMenuView = actionMenuView;
            this.commandProcessor = commandProcessor;
            this.combatPresentationController = combatPresentationController;

            if (pointer != null)
            {
                pointer.TileSelected -= OnClickGrid;
                pointer.HoverGridChanged -= OnHoverChanged;
            }

            pointer = gridPointerController;
            pointer.TileSelected += OnClickGrid;
            pointer.HoverGridChanged += OnHoverChanged;

            this.resultView = resultView;
        }

        public void OnClickGrid(GridCoord grid)
        {
            if (!CanHandleGridInput())
            {
                return;
            }

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

                //点击的不是当前选中的人且是我方的人，切换选中的我方弟子
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
                        out BattleUnitView unitView))
                {
                    return;
                }

                if (!curMovementRange.TryGetPath(
                        grid,
                        out IReadOnlyList<GridCoord> path))
                {
                    return;
                }

                GridCoord destination = grid;
                BattleUnit selectedUnit = curSelectPlayer;

                previewGrid = destination;
                ChangeState(BattleStatus.MovePreviewing);
                this.curOpUnitView = unitView;
                curAttackTargets = null;
                curAttackTarget = null;
                curMovementRange = null;
                gridMapView.SetOffMovement(selectedUnit.Id);
                gridMapView.SetOffAttackRange(selectedUnit.Id);
                gridMapView.ClearSelected();
                battlePath.HidePath();

                unitView.MoveToPreviewPositionByPath(
                    path,
                    () => OnMovePreviewCompleted(
                        selectedUnit,
                        destination,
                        unitView));
            }
        }

        private void OnMovePreviewCompleted(
            BattleUnit selectedUnit,
            GridCoord destination,
            BattleUnitView unitView)
        {
            if (battleStatus != BattleStatus.MovePreviewing ||
                !ReferenceEquals(curSelectPlayer, selectedUnit) ||
                previewGrid != destination ||
                !ReferenceEquals(curOpUnitView, unitView))
            {
                return;
            }

            HashSet<GridCoord> exactAttackRange =
                AttackRangeQuery.TryCalculateAttackRangesByGrid(
                    selectedUnit,
                    context.Map,
                    destination);

            List<BattleUnit> attackTargets = new List<BattleUnit>();

            if (exactAttackRange != null)
            {
                attackTargets = AttackTargetQuery.FindTargetsInRange(
                    context,
                    selectedUnit,
                    exactAttackRange);
            }

            if (attackTargets.Count > 0)
            {
                gridMapView.SetInAttackRange(
                    exactAttackRange,
                    selectedUnit.Id);
            }

            curSelectEnemyIndex = 0;
            curAttackTargets = attackTargets;
            ShowMoveActionMenu();
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
            resultView.SetActions(
                OnEscapePressed,
                OnQPressed,
                OnEPressed,
                OnConfirmPressed);
            ChangeState(BattleStatus.BattleForecast);

            curSelectEnemyIndex = 0;
            RefreshCurrentTarget();
        }

        private void RefreshCurrentTarget()
        {
            if (battleStatus != BattleStatus.BattleForecast ||
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

            if (previewGrid == null ||
                !CombatForecastQuery.TryCalculate(
                    context,
                    curSelectPlayer,
                    target,
                    previewGrid.Value,
                    out CombatForecastResult forecast))
            {
                Debug.LogError(
                    $"Failed to calculate combat forecast for " +
                    $"{curSelectPlayer.Id} and {target.Id}.");
                ReturnToActionMenu();
                return;
            }

            curAttackTarget = target;
            resultView.Show(forecast, curSelectPlayer, target);

            gridMapView.ClearSelected();
            gridMapView.SetSelected(target.CurGrid);
        }

        private void OnQPressed()
        {
            if (battleStatus != BattleStatus.BattleForecast ||
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
            if (battleStatus != BattleStatus.BattleForecast ||
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
            if (battleStatus != BattleStatus.BattleForecast) return;
            if (curSelectPlayer == null || curAttackTargets == null) return;

            ReturnToActionMenu();
        }

        private void OnConfirmPressed()
        {
            if (battleStatus != BattleStatus.BattleForecast ||
                curSelectPlayer == null ||
                curAttackTarget == null ||
                !previewGrid.HasValue)
            {
                return;
            }

            BattleUnit attacker = curSelectPlayer;
            BattleUnit defender = curAttackTarget;
            GridCoord destination = previewGrid.Value;

            resultView.Hide();
            ChangeState(BattleStatus.ResolvingCombat);
            BattleCommandResult result = commandProcessor.Execute(
                new MoveAndAttackCommand(
                    attacker.Id,
                    defender.Id,
                    destination));

            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                ShowMoveActionMenu();
                return;
            }

            battleUnitsViewController.TryGetUnitViewById(attacker.Id, out var attackerView);
            battleUnitsViewController.TryGetUnitViewById(defender.Id, out var defenderView);
            if (attackerView == null || defenderView == null)
            {
                battleUnitsViewController.RefreshAfterCommand(context);
                ClearInactiveEnemySelections();
                ClearAllSelectionStatus(attacker);
                return;
            }

            combatPresentationController.StartCoroutine(
                combatPresentationController.PlayCombatRoutine(
                    attackerView,
                    defenderView,
                    result.AttackResult,
                    () => OnCombatPresentationCompleted(
                        attacker,
                        defender,
                        destination)));
        }

        private void OnCombatPresentationCompleted(
            BattleUnit attacker,
            BattleUnit defender,
            GridCoord destination)
        {
            if (battleStatus != BattleStatus.ResolvingCombat ||
                !ReferenceEquals(curSelectPlayer, attacker) ||
                !ReferenceEquals(curAttackTarget, defender) ||
                previewGrid != destination)
            {
                return;
            }

            battleUnitsViewController.RefreshAfterCommand(context);
            ClearInactiveEnemySelections();
            ClearAllSelectionStatus(attacker);
        }

        private void ReturnToActionMenu()
        {
            resultView.Hide();
            curSelectEnemyIndex = 0;
            curAttackTarget = null;
            ShowMoveActionMenu();
        }

        private void ShowMoveActionMenu()
        {
            if (curSelectPlayer == null ||
                curOpUnitView == null ||
                !previewGrid.HasValue)
            {
                return;
            }

            bool canAttack =
                curAttackTargets != null &&
                curAttackTargets.Count > 0;

            BattleUnit selectedUnit = curSelectPlayer;
            BattleUnitView selectedUnitView = curOpUnitView;
            List<BattleUnit> attackTargets = curAttackTargets;
            GridCoord destination = previewGrid.Value;

            ChangeState(BattleStatus.ActionMenu);
            actionMenuView.Show(
                selectedUnit,
                canAttack,
                canAttack
                    ? () => CommitMoveAndAttack(
                        selectedUnit,
                        attackTargets)
                    : null,
                () => CommitMoveAndWait(
                    selectedUnit,
                    selectedUnitView,
                    destination),
                () => CancelMovePreview(
                    selectedUnit,
                    selectedUnitView));
        }

        private void ClearInactiveEnemySelections()
        {
            List<BattleUnit> inactiveEnemies = null;

            foreach (BattleUnit enemy in selectedEnemies)
            {
                if (!enemy.IsDead)
                {
                    continue;
                }

                inactiveEnemies ??= new List<BattleUnit>();
                inactiveEnemies.Add(enemy);
            }

            if (inactiveEnemies == null)
            {
                return;
            }

            foreach (BattleUnit enemy in inactiveEnemies)
            {
                selectedEnemies.Remove(enemy);
                gridMapView.SetOffMovement(enemy.Id);
                gridMapView.SetOffAttackRange(enemy.Id);
            }
        }

        private void CancelMovePreview(
            BattleUnit unit,
            BattleUnitView unitView)
        {
            //TODO 这里cancel应该回到上一步而不是直接取消选中
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
            ChangeState(BattleStatus.Idle);
            previewGrid = null;
            curAttackTargets = null;
            curAttackTarget = null;
            resultView.Hide();
            actionMenuView.Hide();
            battleUnitsViewController.RefreshActionStates(context);
        }

        public void OnClickPlayer(BattleUnit unit)
        {
            if (unit == null ||
                unit.IsDead ||
                unit.Fac != Faction.Player ||
                !context.Turns.CanUnitAct(unit))
            {
                return;
            }
            
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

                ChangeState(BattleStatus.UnitSelected);
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

        public bool TryBeginPhaseTransition()
        {
            if (battleStatus != BattleStatus.Idle &&
                battleStatus != BattleStatus.UnitSelected)
            {
                return false;
            }

            if (curSelectPlayer != null)
            {
                ClearAllSelectionStatus(curSelectPlayer);
            }

            ClearEnemySelections();
            ChangeState(BattleStatus.PhaseTransition);
            return true;
        }

        public void CompletePhaseTransition()
        {
            if (battleStatus != BattleStatus.PhaseTransition)
            {
                return;
            }

            ChangeState(BattleStatus.Idle);
            battleUnitsViewController.RefreshActionStates(context);
        }

        public void OnHoverChanged(GridCoord? grid)
        {
            if (battleStatus != BattleStatus.UnitSelected ||
                battlePath == null ||
                curSelectPlayer == null ||
                curMovementRange == null ||
                !grid.HasValue)
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

        private bool CanHandleGridInput()
        {
            return battleStatus == BattleStatus.Idle ||
                   battleStatus == BattleStatus.UnitSelected;
        }

        private void ChangeState(BattleStatus next)
        {
            battleStatus = next;

            if (pointer != null)
            {
                pointer.SetGameplayInputEnabled(
                    next == BattleStatus.Idle ||
                    next == BattleStatus.UnitSelected);
            }
        }

        private void ClearEnemySelections()
        {
            foreach (BattleUnit enemy in selectedEnemies)
            {
                gridMapView.SetOffMovement(enemy.Id);
                gridMapView.SetOffAttackRange(enemy.Id);
            }

            selectedEnemies.Clear();
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
                resultView.Hide();
            }

            if (pointer != null)
            {
                pointer.SetGameplayInputEnabled(true);
            }
        }
    }
}
