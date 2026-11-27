using FE.Battle;
using FE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [SerializeField] private BattleEncounterConfig encounterConfig;
        [SerializeField] private GridMapView gridMap;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        [SerializeField] private GridPointerController gridPointerController;
        [SerializeField] private BattleUnitsViewController battleUnitsViewController;
        [SerializeField] private BattlePathView battlePathView;
        [SerializeField] private BattleActionMenuView battleActionMenuView;
        [SerializeField] private BattleForecastView battleForecastView;
        [SerializeField] private int battleRandomSeed = 1;
        [SerializeField] private BattleCombatPresentationController battleCombatPresentationController;
        [SerializeField] private UnitInfoView unitInfoView;
        [SerializeField] private BattleOutcomeView battleOutcomeView;
        [SerializeField] private BattleHistoryPanelController battleHistoryPanelController;


        private BattleSelectionController battleSelectionController;
        private BattleTurnPresentationController battleTurnPresentationController;
        private BattleEnemyPhaseController battleEnemyPhaseController;
        private EnemyAiDebugView enemyAiDebugView;
        private BattleMenuController battleMenuController;
        public BattleRewindCoordinator RewindCoordinator { get; private set; }

        public void Start()
        {
            BattleSetUpFactory factory = new BattleSetUpFactory();
            battleSelectionController = new BattleSelectionController();
            BattleContext context = factory.CreateContext(
                encounterConfig,
                battleRandomSeed);
            BattleCommandProcessor commandProcessor =
                new BattleCommandProcessor(context);

            GridGeometry gg = new GridGeometry(gridMap.transform.position, cellSize);
            gridMap.CreateTiles(context.Map, gg);
            gridPointerController.Init(gridMap, gg, context.Map);
            battlePathView.Init(gg, gridMap);
            battleUnitsViewController.Init(
                context,
                gg,
                gridMap);
            EnsureUnitInfoView();

            battleSelectionController.Init(gridMap, context, gridPointerController, 
                battlePathView, battleUnitsViewController, battleActionMenuView,
                commandProcessor, battleForecastView,
                battleCombatPresentationController,
                unitInfoView);
            RewindCoordinator = new BattleRewindCoordinator(
                context,
                commandProcessor,
                gridMap,
                battleUnitsViewController,
                battleSelectionController);
            enemyAiDebugView = GetComponent<EnemyAiDebugView>();
            if (enemyAiDebugView == null)
            {
                enemyAiDebugView =
                    gameObject.AddComponent<EnemyAiDebugView>();
            }

            battleEnemyPhaseController =
                GetComponent<BattleEnemyPhaseController>();
            if (battleEnemyPhaseController == null)
            {
                battleEnemyPhaseController =
                    gameObject.AddComponent<BattleEnemyPhaseController>();
            }

            battleEnemyPhaseController.Init(
                context,
                commandProcessor,
                gridMap,
                gg,
                battleUnitsViewController,
                battleCombatPresentationController,
                enemyAiDebugView,
                new AggressiveEnemyActionProvider());

            battleTurnPresentationController =
                GetComponent<BattleTurnPresentationController>();

            if (battleTurnPresentationController == null)
            {
                battleTurnPresentationController =
                    gameObject.AddComponent<BattleTurnPresentationController>();
            }

            battleTurnPresentationController.Init(
                context,
                commandProcessor,
                battleSelectionController,
                battleEnemyPhaseController,
                gridPointerController,
                battleUnitsViewController);
            battleHistoryPanelController ??=
                FindObjectOfType<BattleHistoryPanelController>();
            battleHistoryPanelController?.Init(
                context,
                RewindCoordinator,
                gridPointerController,
                battleSelectionController,
                battleTurnPresentationController);

            EnsureBattleMenu();
            EnsureBattleOutcomeView();
            battleTurnPresentationController.BattleEnded -=
                battleOutcomeView.Show;
            battleTurnPresentationController.BattleEnded +=
                battleOutcomeView.Show;
        }

        private void EnsureBattleMenu()
        {
            BattleMenuView menuView =
                FindObjectOfType<BattleMenuView>();

            if (menuView == null)
            {
                GameObject menuCanvas = new GameObject(
                    "BattleMenuCanvas",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasScaler),
                    typeof(GraphicRaycaster),
                    typeof(CanvasGroup),
                    typeof(BattleMenuView),
                    typeof(BattleMenuController));
                menuView =
                    menuCanvas.GetComponent<BattleMenuView>();
                battleMenuController =
                    menuCanvas.GetComponent<BattleMenuController>();
            }
            else
            {
                battleMenuController =
                    menuView.GetComponent<BattleMenuController>();
                if (battleMenuController == null)
                {
                    battleMenuController =
                        menuView.gameObject.AddComponent<
                            BattleMenuController>();
                }
            }

            battleMenuController.Init(
                menuView,
                battleTurnPresentationController,
                battleSelectionController);
        }

        private void EnsureBattleOutcomeView()
        {
            if (battleOutcomeView != null)
            {
                return;
            }

            battleOutcomeView =
                FindObjectOfType<BattleOutcomeView>();

            if (battleOutcomeView != null)
            {
                return;
            }

            GameObject outcomeCanvas = new GameObject(
                "BattleOutcomeCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(BattleOutcomeView));
            battleOutcomeView =
                outcomeCanvas.GetComponent<BattleOutcomeView>();
        }

        private void EnsureUnitInfoView()
        {
            if (unitInfoView != null)
            {
                return;
            }

            GameObject infoPanel =
                GameObject.Find("UnitInfoCanvas/InfoPanel");

            if (infoPanel == null)
            {
                Debug.LogError(
                    "UnitInfoCanvas/InfoPanel was not found.");
                return;
            }

            unitInfoView =
                infoPanel.GetComponent<UnitInfoView>();

            if (unitInfoView == null)
            {
                unitInfoView =
                    infoPanel.AddComponent<UnitInfoView>();
            }
        }

        private void OnDestroy()
        {
            if (battleTurnPresentationController != null &&
                battleOutcomeView != null)
            {
                battleTurnPresentationController.BattleEnded -=
                    battleOutcomeView.Show;
            }

            battleSelectionController?.Dispose();
        }
    }
}

