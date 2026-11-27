using System;
using FE.Core;

namespace FE.Presentation
{
    public sealed class BattleRewindCoordinator
    {
        private readonly BattleContext context;
        private readonly BattleCommandProcessor commandProcessor;
        private readonly GridMapView gridMapView;
        private readonly BattleUnitsViewController unitsViewController;
        private readonly BattleSelectionController selectionController;

        public BattleHistoryService History =>
            commandProcessor.History;

        public BattleRewindCoordinator(
            BattleContext context,
            BattleCommandProcessor commandProcessor,
            GridMapView gridMapView,
            BattleUnitsViewController unitsViewController,
            BattleSelectionController selectionController)
        {
            this.context = context ??
                throw new ArgumentNullException(nameof(context));
            this.commandProcessor = commandProcessor ??
                throw new ArgumentNullException(nameof(commandProcessor));
            this.gridMapView = gridMapView ??
                throw new ArgumentNullException(nameof(gridMapView));
            this.unitsViewController = unitsViewController ??
                throw new ArgumentNullException(
                    nameof(unitsViewController));
            this.selectionController = selectionController ??
                throw new ArgumentNullException(
                    nameof(selectionController));
        }

        public bool TryRewindToBefore(int historyIndex)
        {
            if (!commandProcessor.TryRewindToBefore(historyIndex))
            {
                return false;
            }

            gridMapView.RefreshTerrain(context.Map);
            unitsViewController.RefreshAfterCommand(context);
            selectionController.RefreshAfterRewind(context);
            return true;
        }
    }
}
