using System.Collections;
using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using Unity.Profiling;
using UnityEngine;

namespace FE.Presentation
{
    public class BattleEnemyPhaseController : MonoBehaviour
    {
        private static readonly ProfilerMarker PlanMarker =
            new ProfilerMarker("EnemyAI.Plan");

        [SerializeField, Min(0f)] private float stepDelay = 0.15f;

        private BattleContext context;
        private BattleCommandProcessor commandProcessor;
        private GridMapView gridMapView;
        private GridGeometry gridGeometry;
        private BattleUnitsViewController unitsViewController;
        private BattleCombatPresentationController combatPresentation;
        private EnemyAiDebugView debugView;
        private IEnemyActionProvider actionProvider;

        public BattleOutcomeResult? BattleOutcome { get; private set; }

        public void Init(
            BattleContext context,
            BattleCommandProcessor commandProcessor,
            GridMapView gridMapView,
            GridGeometry gridGeometry,
            BattleUnitsViewController unitsViewController,
            BattleCombatPresentationController combatPresentation,
            EnemyAiDebugView debugView,
            IEnemyActionProvider actionProvider)
        {
            this.context = context;
            this.commandProcessor = commandProcessor;
            this.gridMapView = gridMapView;
            this.gridGeometry = gridGeometry;
            this.unitsViewController = unitsViewController;
            this.combatPresentation = combatPresentation;
            this.debugView = debugView;
            this.actionProvider = actionProvider;
            Rigidbody2D rb = new Rigidbody2D();
        }

        public IEnumerator PlayEnemyPhaseRoutine()
        {
            BattleOutcome = null;
            var enemyIds = new List<UnitId>();

            foreach (BattleUnit unit in context.Units.AllUnits)
            {
                if (!unit.IsDead && unit.Fac == Faction.Enemy)
                {
                    enemyIds.Add(unit.Id);
                }
            }

            foreach (UnitId enemyId in enemyIds)
            {
                BattleUnit enemy =
                    context.Units.GetUnitById(enemyId);

                if (enemy == null ||
                    enemy.IsDead ||
                    !context.Turns.CanUnitAct(enemy))
                {
                    continue;
                }

                yield return ExecuteEnemyAction(enemy);

                if (BattleOutcome.HasValue)
                {
                    break;
                }

                if (stepDelay > 0f)
                {
                    yield return new WaitForSeconds(stepDelay);
                }
            }

            debugView?.ClearPlan();
        }

        private IEnumerator ExecuteEnemyAction(BattleUnit enemy)
        {
            EnemyActionPlan plan;
            bool planned;

            using (PlanMarker.Auto())
            {
                planned = actionProvider.TryPlan(
                    context,
                    enemy,
                    out plan);
            }

            if (!planned)
            {
                yield return ExecuteWaitFallback(enemy);
                yield break;
            }

            if (!unitsViewController.TryGetUnitViewById(
                    enemy.Id,
                    out BattleUnitView enemyView))
            {
                ExecuteWaitFallbackImmediate(enemy);
                yield break;
            }

            if (!MovementQuery.TryCalculate(
                    enemy,
                    context.Map,
                    context.Units,
                    out MovementRange movement) ||
                !movement.TryGetPath(
                    plan.Destination,
                    out IReadOnlyList<GridCoord> path))
            {
                yield return ExecuteWaitFallback(enemy);
                yield break;
            }

            BattleUnitView targetView = null;
            if (plan.TargetUnitId.HasValue)
            {
                unitsViewController.TryGetUnitViewById(
                    plan.TargetUnitId.Value,
                    out targetView);
            }

            Vector3 destinationPosition =
                gridGeometry.GetCellCenter(plan.Destination);

            if (targetView != null)
            {
                debugView?.SetPlan(
                    enemyView.transform.position,
                    destinationPosition,
                    targetView.transform.position);
            }
            else
            {
                debugView?.SetPlan(
                    enemyView.transform.position,
                    destinationPosition);
            }

            Debug.Log(
                $"[EnemyAI] Unit={enemy.Id} " +
                $"Action={plan.ActionType} " +
                $"Destination={plan.Destination} " +
                $"Target={plan.TargetUnitId} " +
                $"Score={plan.Score} " +
                $"ExpectedDamage={plan.ExpectedDamage} " +
                $"CanKill={plan.CanKill} " +
                $"CounterDamage={plan.ExpectedCounterDamage}");

            gridMapView.SetSelected(plan.Destination);
            yield return enemyView.MoveToPositionByPathRoutine(path);

            BattleCommandResult result;

            if (plan.ActionType == EnemyActionType.Attack &&
                plan.TargetUnitId.HasValue)
            {
                BattleUnit target =
                    context.Units.GetUnitById(
                        plan.TargetUnitId.Value);

                if (target == null || target.IsDead)
                {
                    gridMapView.ClearSelected();
                    yield return ExecuteWaitFallback(enemy);
                    yield break;
                }

                result = commandProcessor.Execute(
                    new MoveAndAttackCommand(
                        enemy.Id,
                        target.Id,
                        plan.Destination));

                if (result.BattleOutcome.HasValue)
                {
                    BattleOutcome = result.BattleOutcome;
                }

                if (result.IsSuccess &&
                    result.AttackResult.HasValue &&
                    targetView != null)
                {
                    yield return combatPresentation.PlayCombatRoutine(
                        enemyView,
                        targetView,
                        result.AttackResult,
                        null);
                }
            }
            else
            {
                result = commandProcessor.Execute(
                    new MoveAndWaitCommand(
                        enemy.Id,
                        plan.Destination));

                if (result.BattleOutcome.HasValue)
                {
                    BattleOutcome = result.BattleOutcome;
                }
            }

            gridMapView.ClearSelected();

            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
                unitsViewController.RefreshAfterCommand(context);
                ExecuteWaitFallbackImmediate(enemy);
                yield break;
            }

            unitsViewController.RefreshAfterCommand(context);
        }

        private IEnumerator ExecuteWaitFallback(BattleUnit enemy)
        {
            ExecuteWaitFallbackImmediate(enemy);
            yield return null;
        }

        private void ExecuteWaitFallbackImmediate(BattleUnit enemy)
        {
            BattleCommandResult result = commandProcessor.Execute(
                new MoveAndWaitCommand(
                    enemy.Id,
                    enemy.CurGrid));

            if (!result.IsSuccess)
            {
                Debug.LogError(result.ErrorMessage);
            }

            unitsViewController.RefreshAfterCommand(context);
        }
    }
}
