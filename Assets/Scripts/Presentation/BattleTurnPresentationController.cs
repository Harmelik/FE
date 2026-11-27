using System;
using System.Collections;
using FE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleTurnPresentationController : MonoBehaviour
    {
        [SerializeField] private Button endTurnButton;

        private BattleContext context;
        private BattleCommandProcessor commandProcessor;
        private BattleSelectionController selectionController;
        private BattleEnemyPhaseController enemyPhaseController;
        private GridPointerController pointer;
        private BattleUnitsViewController unitsViewController;
        private Coroutine enemyPhaseRoutine;
        private bool isTransitioning;
        private bool isExternalInputSuspended;

        public event Action<BattlePhaseTransitionResult> PhaseChanged;
        public event Action<BattleOutcomeResult> BattleEnded;

        public bool CanOpenBattleMenu =>
            context != null &&
            !isExternalInputSuspended &&
            !isTransitioning &&
            context.Turns.CurrentPhase == BattlePhase.PlayerPhase &&
            selectionController != null &&
            selectionController.CanOpenBattleMenu;

        public void Init(
            BattleContext context,
            BattleCommandProcessor commandProcessor,
            BattleSelectionController selectionController,
            BattleEnemyPhaseController enemyPhaseController,
            GridPointerController pointer,
            BattleUnitsViewController unitsViewController)
        {
            this.context = context;
            this.commandProcessor = commandProcessor;
            this.selectionController = selectionController;
            this.enemyPhaseController = enemyPhaseController;
            this.pointer = pointer;
            this.unitsViewController = unitsViewController;
            selectionController.BattleEnded -= OnBattleEnded;
            selectionController.BattleEnded += OnBattleEnded;

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            UpdateView();
        }

        private void Update()
        {
            if (context == null ||
                isTransitioning ||
                isExternalInputSuspended)
            {
                return;
            }

            if (context.Turns.CurrentPhase == BattlePhase.PlayerPhase &&
                Input.GetKeyDown(KeyCode.T))
            {
                TryEndPlayerPhase();
            }
        }

        private void OnEndTurnClicked()
        {
            TryEndPlayerPhase();
        }

        public bool TryEndPlayerPhase()
        {
            if (context == null ||
                commandProcessor == null ||
                selectionController == null ||
                isExternalInputSuspended ||
                isTransitioning ||
                context.Turns.CurrentPhase != BattlePhase.PlayerPhase)
            {
                return false;
            }

            if (!selectionController.TryBeginPhaseTransition())
            {
                return false;
            }

            pointer?.SetGameplayInputEnabled(false);

            BattleCommandResult result = commandProcessor.Execute(
                new EndPhaseCommand());

            if (!result.IsSuccess ||
                !result.PhaseTransition.HasValue)
            {
                Debug.LogError(result.ErrorMessage);
                selectionController.CompletePhaseTransition();
                pointer?.SetGameplayInputEnabled(true);
                UpdateView();
                return false;
            }

            isTransitioning = true;
            ApplyTransition(result.PhaseTransition.Value);

            enemyPhaseRoutine = StartCoroutine(
                FinishEnemyPhaseRoutine());
            return true;
        }

        private IEnumerator FinishEnemyPhaseRoutine()
        {
            yield return enemyPhaseController.PlayEnemyPhaseRoutine();

            if (enemyPhaseController.BattleOutcome.HasValue)
            {
                BattleOutcomeResult outcome =
                    enemyPhaseController.BattleOutcome.Value;
                selectionController.EnterBattleEnded(outcome);
                enemyPhaseRoutine = null;
                yield break;
            }

            BattleCommandResult result = commandProcessor.Execute(
                new EndPhaseCommand());

            if (result.IsSuccess && result.PhaseTransition.HasValue)
            {
                ApplyTransition(result.PhaseTransition.Value);
            }
            else
            {
                Debug.LogError(result.ErrorMessage);
            }

            enemyPhaseRoutine = null;
            isTransitioning = false;
            selectionController.CompletePhaseTransition();
            pointer?.SetGameplayInputEnabled(true);
            UpdateView();
        }

        private void OnBattleEnded(BattleOutcomeResult outcome)
        {
            enemyPhaseRoutine = null;
            isTransitioning = false;
            pointer?.SetGameplayInputEnabled(false);
            Debug.Log(
                $"[BattleOutcome] {outcome.OutcomeType}: " +
                $"{outcome.Reason}");
            BattleEnded?.Invoke(outcome);
            UpdateView();
        }

        private void ApplyTransition(
            BattlePhaseTransitionResult transition)
        {
            unitsViewController?.RefreshActionStates(context);
            PhaseChanged?.Invoke(transition);
            UpdateView();
        }

        private void UpdateView()
        {
            if (context == null)
            {
                return;
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable =
                    !isTransitioning &&
                    !isExternalInputSuspended &&
                    context.Turns.CurrentPhase ==
                    BattlePhase.PlayerPhase;
            }
        }

        private void OnDestroy()
        {
            if (selectionController != null)
            {
                selectionController.BattleEnded -= OnBattleEnded;
            }
            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
            }

            if (enemyPhaseRoutine != null)
            {
                StopCoroutine(enemyPhaseRoutine);
                enemyPhaseRoutine = null;
            }

            pointer?.SetGameplayInputEnabled(true);
        }

        public void SetExternalInputSuspended(bool isSuspended)
        {
            isExternalInputSuspended = isSuspended;
        }
    }
}
