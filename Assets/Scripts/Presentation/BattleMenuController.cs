using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// Owns global-menu input and keeps map input suspended while it is open.
    /// </summary>
    public class BattleMenuController : MonoBehaviour
    {
        private BattleMenuView menuView;
        private BattleTurnPresentationController turnController;
        private BattleSelectionController selectionController;
        private bool isBattleEnded;

        public void Init(
            BattleMenuView view,
            BattleTurnPresentationController turns,
            BattleSelectionController selection)
        {
            menuView = view;
            turnController = turns;
            selectionController = selection;
            isBattleEnded = false;

            menuView.Init(OnEndPhaseRequested);
            turnController.BattleEnded -= OnBattleEnded;
            turnController.BattleEnded += OnBattleEnded;
        }

        private void Update()
        {
            if (menuView == null || isBattleEnded ||
                !Input.GetKeyDown(KeyCode.Escape))
            {
                return;
            }

            if (menuView.IsVisible)
            {
                CloseMenu();
            }
            else if (turnController != null &&
                     turnController.CanOpenBattleMenu)
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            selectionController?.SetExternalInputSuspended(true);
            menuView.Show();
        }

        private void CloseMenu()
        {
            menuView.Hide();
            selectionController?.SetExternalInputSuspended(false);
        }

        private void OnEndPhaseRequested()
        {
            if (turnController == null ||
                !turnController.TryEndPlayerPhase())
            {
                return;
            }

            CloseMenu();
        }

        private void OnBattleEnded(BattleOutcomeResult outcome)
        {
            isBattleEnded = true;

            if (menuView != null && menuView.IsVisible)
            {
                CloseMenu();
            }
        }

        private void OnDestroy()
        {
            if (turnController != null)
            {
                turnController.BattleEnded -= OnBattleEnded;
            }
        }
    }
}
