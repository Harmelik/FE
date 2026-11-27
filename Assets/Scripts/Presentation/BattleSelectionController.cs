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
        private enum BattleStatus
        {
            Idle,
            UnitSelected,
            MovePreviewing,
            ActionMenu,
            BattleForecast,
            ResolvingCombat,
            PhaseTransition,
            BattleEnded
        }

        private BattleUnit curSelectPlayer;
        private readonly EnemyRangeSelectionTracker enemyRangeSelection =
            new EnemyRangeSelectionTracker();
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
        private UnitInfoView unitInfoView;
        private BattleUnit hoveredUnit;
        private bool isExternalInputSuspended;

        public event Action<BattleOutcomeResult> BattleEnded;

        public bool CanOpenBattleMenu =>
            !isExternalInputSuspended &&
            (battleStatus == BattleStatus.Idle ||
             battleStatus == BattleStatus.UnitSelected);

        public void Init(GridMapView mapView, BattleContext context, 
            GridPointerController gridPointerController, BattlePathView battlePathView,
            BattleUnitsViewController battleUnitsViewController, BattleActionMenuView actionMenuView,
            BattleCommandProcessor commandProcessor, BattleForecastView resultView,
            BattleCombatPresentationController combatPresentationController,
            UnitInfoView unitInfoView)
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
            this.unitInfoView = unitInfoView;

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
                        destination,
                        result.BattleOutcome)));
        }

        private void OnCombatPresentationCompleted(
            BattleUnit attacker,
            BattleUnit defender,
            GridCoord destination,
            BattleOutcomeResult? battleOutcome)
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

            if (battleOutcome.HasValue)
            {
                EnterBattleEnded(battleOutcome.Value);
            }
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
            List<UnitId> inactiveEnemies = null;

            foreach (UnitId enemyId in
                     enemyRangeSelection.CreateSnapshot())
            {
                BattleUnit enemy =
                    context.Units.GetUnitById(enemyId);

                if (enemy != null && !enemy.IsDead)
                {
                    continue;
                }

                inactiveEnemies ??= new List<UnitId>();
                inactiveEnemies.Add(enemyId);
            }

            if (inactiveEnemies == null)
            {
                return;
            }

            foreach (UnitId enemyId in inactiveEnemies)
            {
                enemyRangeSelection.Remove(enemyId);
                gridMapView.SetOffMovement(enemyId);
                gridMapView.SetOffAttackRange(enemyId);
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

            ClearHoveredUnitRange();
            
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

            ClearHoveredUnitRange();
            bool isTracked = enemyRangeSelection.Toggle(unit.Id);

            if (!isTracked)
            {
                gridMapView.SetOffMovement(unit.Id);
                gridMapView.SetOffAttackRange(unit.Id);
            }
            else
            {
                if (!MovementQuery.TryCalculate(unit, context.Map, context.Units, out var result1))
                {
                    enemyRangeSelection.Remove(unit.Id);
                    gridMapView.SetOffMovement(unit.Id);
                    gridMapView.SetOffAttackRange(unit.Id);
                    return;
                }
                var range = AttackRangeQuery.CalculateCoverage(unit, context.Map, result1);
                if(range == null) return;

                gridMapView.SetInMovement(result1, unit.Id);
                gridMapView.SetInAttackRange(range, unit.Id);
            }
        }

        public void RefreshAfterRewind(BattleContext context)
        {
            if (context == null)
            {
                return;
            }

            foreach (UnitId enemyId in
                     enemyRangeSelection.CreateSnapshot())
            {
                gridMapView.SetOffMovement(enemyId);
                gridMapView.SetOffAttackRange(enemyId);
            }

            enemyRangeSelection.Clear();

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                gridMapView.SetOffMovement(unit.Id);
                gridMapView.SetOffAttackRange(unit.Id);
                gridMapView.SetOffHoverMovement(unit.Id);
                gridMapView.SetOffHoverAttackRange(unit.Id);
            }

            curOpUnitView?.RefreshToDefaultPosition();
            curSelectPlayer = null;
            curMovementRange = null;
            previewGrid = null;
            curAttackTargets = null;
            curAttackTarget = null;
            curSelectEnemyIndex = 0;
            gridMapView.ClearHovered();
            gridMapView.ClearSelected();
            battlePath.HidePath();
            resultView.Hide();
            actionMenuView.Hide();
            unitInfoView?.Hide();
            ChangeState(BattleStatus.Idle);
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

            HideEnemyRangeVisuals();
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

            if (context.Turns.CurrentPhase == BattlePhase.PlayerPhase)
            {
                RestoreTrackedEnemyRanges();
            }

            battleUnitsViewController.RefreshActionStates(context);
        }

        public void EnterBattleEnded(BattleOutcomeResult outcome)
        {
            if (battleStatus == BattleStatus.BattleEnded)
            {
                return;
            }

            if (curSelectPlayer != null)
            {
                ClearAllSelectionStatus(curSelectPlayer);
            }
            else
            {
                gridMapView.ClearSelected();
                battlePath.HidePath();
                curMovementRange = null;
                previewGrid = null;
                curAttackTargets = null;
                curAttackTarget = null;
                resultView.Hide();
                actionMenuView.Hide();
            }

            ClearEnemySelections();
            ClearHoveredUnitRange();
            unitInfoView?.Hide();
            ChangeState(BattleStatus.BattleEnded);
            pointer?.SetGameplayInputEnabled(false);
            battleUnitsViewController.RefreshActionStates(context);
            BattleEnded?.Invoke(outcome);
        }

        public void OnHoverChanged(GridCoord? grid)
        {
            if (isExternalInputSuspended ||
                battleStatus == BattleStatus.PhaseTransition ||
                battleStatus == BattleStatus.ResolvingCombat ||
                battleStatus == BattleStatus.BattleEnded)
            {
                ClearHoveredUnitRange();
                battlePath?.HidePath();
                unitInfoView?.Hide();
                return;
            }

            RefreshUnitInfo(grid);

            BattleUnit unitAtGrid = grid.HasValue
                ? context.Units.GetUnitByGrid(grid.Value)
                : null;
            UpdateHoveredUnitRange(unitAtGrid);

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

        public void SetExternalInputSuspended(bool isSuspended)
        {
            if (isExternalInputSuspended == isSuspended)
            {
                return;
            }

            isExternalInputSuspended = isSuspended;

            if (isSuspended)
            {
                ClearHoveredUnitRange();
                battlePath?.HidePath();
                unitInfoView?.Hide();
            }

            RefreshPointerInputEnabled();
        }

        private void RefreshUnitInfo(GridCoord? grid)
        {
            if (unitInfoView == null)
            {
                return;
            }

            if (!grid.HasValue)
            {
                unitInfoView.Hide();
                return;
            }

            BattleUnit unit =
                context.Units.GetUnitByGrid(grid.Value);

            if (unit == null)
            {
                unitInfoView.Hide();
                return;
            }

            unitInfoView.Show(unit, context.Map);
        }

        private bool CanHandleGridInput()
        {
            return !isExternalInputSuspended &&
                   (battleStatus == BattleStatus.Idle ||
                    battleStatus == BattleStatus.UnitSelected);
        }

        private void ChangeState(BattleStatus next)
        {
            battleStatus = next;

            if (pointer != null)
            {
                RefreshPointerInputEnabled();
            }
        }

        private void ClearEnemySelections()
        {
            foreach (UnitId enemyId in
                     enemyRangeSelection.CreateSnapshot())
            {
                gridMapView.SetOffMovement(enemyId);
                gridMapView.SetOffAttackRange(enemyId);
            }

            enemyRangeSelection.Clear();
        }

        private void HideEnemyRangeVisuals()
        {
            foreach (UnitId enemyId in
                     enemyRangeSelection.CreateSnapshot())
            {
                gridMapView.SetOffMovement(enemyId);
                gridMapView.SetOffAttackRange(enemyId);
            }

        }

        private void RestoreTrackedEnemyRanges()
        {
            foreach (UnitId enemyId in
                     enemyRangeSelection.CreateSnapshot())
            {
                BattleUnit enemy =
                    context.Units.GetUnitById(enemyId);

                if (enemy == null || enemy.IsDead)
                {
                    enemyRangeSelection.Remove(enemyId);
                    gridMapView.SetOffMovement(enemyId);
                    gridMapView.SetOffAttackRange(enemyId);
                    continue;
                }

                if (!MovementQuery.TryCalculate(
                        enemy,
                        context.Map,
                        context.Units,
                        out MovementRange movement))
                {
                    enemyRangeSelection.Remove(enemyId);
                    gridMapView.SetOffMovement(enemyId);
                    gridMapView.SetOffAttackRange(enemyId);
                    continue;
                }

                HashSet<GridCoord> attackRange =
                    AttackRangeQuery.CalculateCoverage(
                        enemy,
                        context.Map,
                        movement);
                gridMapView.SetInMovement(movement, enemyId);
                gridMapView.SetInAttackRange(
                    attackRange,
                    enemyId);
            }

        }

        private void UpdateHoveredUnitRange(BattleUnit unit)
        {
            if (ReferenceEquals(hoveredUnit, unit))
            {
                return;
            }

            ClearHoveredUnitRange();
            hoveredUnit = unit;

            if (hoveredUnit == null ||
                hoveredUnit.IsDead ||
                ReferenceEquals(hoveredUnit, curSelectPlayer) ||
                enemyRangeSelection.IsTracked(hoveredUnit.Id))
            {
                hoveredUnit = null;
                return;
            }

            if (!MovementQuery.TryCalculate(
                    hoveredUnit,
                    context.Map,
                    context.Units,
                    out MovementRange movement))
            {
                hoveredUnit = null;
                return;
            }

            HashSet<GridCoord> attackRange =
                AttackRangeQuery.CalculateCoverage(
                    hoveredUnit,
                    context.Map,
                    movement);
            gridMapView.SetInHoverMovement(
                movement,
                hoveredUnit.Id);
            gridMapView.SetInHoverAttackRange(
                attackRange,
                hoveredUnit.Id);
        }

        private void ClearHoveredUnitRange()
        {
            if (hoveredUnit == null)
            {
                return;
            }

            UnitId unitId = hoveredUnit.Id;
            gridMapView.SetOffHoverMovement(unitId);
            gridMapView.SetOffHoverAttackRange(unitId);
            hoveredUnit = null;
        }

        private void RefreshPointerInputEnabled()
        {
            if (pointer == null)
            {
                return;
            }

            bool isPlayable =
                battleStatus == BattleStatus.Idle ||
                battleStatus == BattleStatus.UnitSelected;
            pointer.SetGameplayInputEnabled(
                isPlayable && !isExternalInputSuspended);
        }

        public void Dispose()
        {
            if (pointer != null)
            {
                pointer.TileSelected -= OnClickGrid;
                pointer.HoverGridChanged -= OnHoverChanged;
            }

            if (battlePath != null)
            {
                battlePath.HidePath();
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
