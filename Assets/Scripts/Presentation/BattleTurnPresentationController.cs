using System;
using System.Collections;
using FE.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleTurnPresentationController : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private Button endTurnButton;
        [SerializeField, Min(0f)] private float enemyPhaseDuration = 0.8f;

        private BattleContext context;
        private BattleCommandProcessor commandProcessor;
        private BattleSelectionController selectionController;
        private GridPointerController pointer;
        private BattleUnitsViewController unitsViewController;
        private Coroutine enemyPhaseRoutine;
        private bool isTransitioning;

        public event Action<BattlePhaseTransitionResult> PhaseChanged;

        public void Init(
            BattleContext context,
            BattleCommandProcessor commandProcessor,
            BattleSelectionController selectionController,
            GridPointerController pointer,
            BattleUnitsViewController unitsViewController)
        {
            this.context = context;
            this.commandProcessor = commandProcessor;
            this.selectionController = selectionController;
            this.pointer = pointer;
            this.unitsViewController = unitsViewController;

            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveListener(OnEndTurnClicked);
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }

            UpdateView();
        }

        private void Update()
        {
            if (context == null || isTransitioning)
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
                FinishEnemyPhaseAfterDelay());
            return true;
        }

        private IEnumerator FinishEnemyPhaseAfterDelay()
        {
            yield return new WaitForSeconds(enemyPhaseDuration);

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

            if (turnText != null)
            {
                turnText.text = $"Turn {context.Turns.TurnNumber}";
            }

            if (phaseText != null)
            {
                phaseText.text =
                    context.Turns.CurrentPhase == BattlePhase.PlayerPhase
                        ? "Player Phase"
                        : "Enemy Phase";
            }

            if (endTurnButton != null)
            {
                endTurnButton.interactable =
                    !isTransitioning &&
                    context.Turns.CurrentPhase ==
                    BattlePhase.PlayerPhase;
            }
        }

        private void OnDestroy()
        {
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
    }
}
