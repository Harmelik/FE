using FE.Core;
using System;

namespace FE.Battle
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleSetUpFactory
    {
        public BattleContext CreateContext(
            BattleEncounterConfig config,
            int battleRandomSeed)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (config.MapConfig == null)
            {
                throw new InvalidOperationException(
                    "BattleEncounterConfig has no map config.");
            }

            if (config.PlayerDeployments == null ||
                config.EnemyDeployments == null)
            {
                throw new InvalidOperationException(
                    "BattleEncounterConfig deployment lists are null.");
            }

            BattleMap map = config.MapConfig.CreateMap();
            BattleUnitController controller = new BattleUnitController();
            int value = 0;
            UnitId curId = new UnitId(value);
            foreach (var player in config.PlayerDeployments)
            {
                var status = player.UnitDefinition.CreateStats();
                BattleUnit unit = new BattleUnit(curId, Faction.Player
                    , player.UnitDefinition.DisplayName, status,player.ToGridCoord());
                var addedUnit = controller.AddUnit(unit, map);
                if (addedUnit == null)
                {
                    throw new InvalidOperationException(
                        $"Player deployment {curId} could not be added. " +
                        $"Position: {player.GridPosition}.");
                }
                value++;
                curId = new UnitId(value);
            }

            foreach (var enemy in config.EnemyDeployments)
            {
                var status = enemy.UnitDefinition.CreateStats();
                BattleUnit unit = new BattleUnit(curId, Faction.Enemy
                    , enemy.UnitDefinition.DisplayName, status, enemy.ToGridCoord());
                var addedUnit = controller.AddUnit(unit, map);
                if (addedUnit == null)
                {
                    throw new InvalidOperationException(
                        $"Enemy deployment {curId} could not be added. " +
                        $"Position: {enemy.GridPosition}.");
                }
                value++;
                curId = new UnitId(value);
            }
            BattleTurnController turns =
                new BattleTurnController(controller);

            return new BattleContext(
                map,
                controller,
                new SeededBattleRandom(battleRandomSeed),
                turns);
        }
    }
}

