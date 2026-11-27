using System.Collections;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    public class BattleHistoryPanelController : MonoBehaviour
    {
        [SerializeField] private BattleHistoryView view;

        private BattleContext context;
        private BattleHistoryService history;
        private BattleRewindCoordinator rewindCoordinator;
        private GridPointerController pointer;
        private BattleSelectionController selectionController;
        private BattleTurnPresentationController turnController;
        private bool initialized;
        private bool isOpen;

        public bool IsOpen => isOpen;

        public void Init(
            BattleContext context,
            BattleRewindCoordinator rewindCoordinator,
            GridPointerController pointer,
            BattleSelectionController selectionController,
            BattleTurnPresentationController turnController)
        {
            this.context = context;
            this.rewindCoordinator = rewindCoordinator;
            this.pointer = pointer;
            this.selectionController = selectionController;
            this.turnController = turnController;
            history = rewindCoordinator.History;

            view ??= GetComponent<BattleHistoryView>();
            if (view == null)
            {
                Debug.LogError(
                    "BattleHistoryPanelController requires " +
                    "a BattleHistoryView.",
                    this);
                enabled = false;
                return;
            }

            view.ConfirmRequested -= OnConfirmRequested;
            view.ConfirmRequested += OnConfirmRequested;
            view.CancelRequested -= OnCancelRequested;
            view.CancelRequested += OnCancelRequested;

            if (turnController != null)
            {
                turnController.BattleEnded -= OnBattleEnded;
                turnController.BattleEnded += OnBattleEnded;
            }

            initialized = true;
            view.Hide();
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (isOpen)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    Close(true);
                    return;
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    Close();
                    return;
                }

                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    view.SelectPrevious();
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    view.SelectNext();
                }
                else if (Input.GetKeyDown(KeyCode.Return) ||
                         Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    view.ConfirmSelection();
                }

                float scroll = Input.mouseScrollDelta.y;
                if (Mathf.Abs(scroll) > 0.01f)
                {
                    view.Scroll(scroll * 70f);
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.R) && CanOpen())
            {
                Open();
            }
        }

        private bool CanOpen()
        {
            return context != null &&
                   context.Turns.CurrentPhase ==
                   BattlePhase.PlayerPhase &&
                   turnController != null &&
                   turnController.CanOpenBattleMenu;
        }

        private void Open()
        {
            if (isOpen)
            {
                return;
            }

            isOpen = true;
            pointer?.SetGameplayInputEnabled(false);
            selectionController?.SetExternalInputSuspended(true);
            turnController?.SetExternalInputSuspended(true);
            view.Show(history.Entries, context);
        }

        private void Close(bool releaseInputNextFrame = false)
        {
            if (!isOpen)
            {
                return;
            }

            isOpen = false;
            view.Hide();

            if (releaseInputNextFrame)
            {
                StartCoroutine(ReleaseInputNextFrame());
                return;
            }

            ReleaseInput();
        }

        private IEnumerator ReleaseInputNextFrame()
        {
            yield return null;
            ReleaseInput();
        }

        private void ReleaseInput()
        {
            turnController?.SetExternalInputSuspended(false);
            selectionController?.SetExternalInputSuspended(false);
            pointer?.SetGameplayInputEnabled(true);
        }

        private void OnConfirmRequested()
        {
            if (!isOpen || view.SelectedIndex < 0)
            {
                return;
            }

            if (rewindCoordinator.TryRewindToBefore(
                    view.SelectedIndex))
            {
                Close();
            }
        }

        private void OnCancelRequested()
        {
            Close();
        }

        private void OnBattleEnded(BattleOutcomeResult outcome)
        {
            if (isOpen)
            {
                Close();
            }
        }

        private void OnDestroy()
        {
            if (view != null)
            {
                view.ConfirmRequested -= OnConfirmRequested;
                view.CancelRequested -= OnCancelRequested;
            }

            if (turnController != null)
            {
                turnController.BattleEnded -= OnBattleEnded;
            }

            turnController?.SetExternalInputSuspended(false);
            selectionController?.SetExternalInputSuspended(false);
            pointer?.SetGameplayInputEnabled(true);
        }
    }
}
