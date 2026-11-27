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
                BattleUnit unit = CreateUnit(
                    curId,
                    Faction.Player,
                    player);
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
                BattleUnit unit = CreateUnit(
                    curId,
                    Faction.Enemy,
                    enemy);
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

        private static BattleUnit CreateUnit(
            UnitId id,
            Faction faction,
            UnitDeploymentData deployment)
        {
            UnitDataDefinition definition = deployment?.UnitDefinition;
            if (definition == null)
            {
                throw new InvalidOperationException(
                    $"Deployment {id} has no unit definition.");
            }

            if (definition.UnitClass == null)
            {
                throw new InvalidOperationException(
                    $"Unit {definition.name} has no class assigned.");
            }

            if (definition.EquippedWeapon == null)
            {
                throw new InvalidOperationException(
                    $"Unit {definition.name} has no equipped weapon assigned.");
            }

            UnitClassProfile unitClass =
                definition.UnitClass.CreateProfile();
            WeaponProfile weapon =
                definition.EquippedWeapon.CreateProfile();

            if (!unitClass.CanEquip(weapon.Category))
            {
                throw new InvalidOperationException(
                    $"Unit {definition.name} cannot equip " +
                    $"{weapon.DisplayName} in class {unitClass.DisplayName}.");
            }

            return new BattleUnit(
                id,
                faction,
                definition.DisplayName,
                definition.Level,
                unitClass,
                weapon,
                definition.CreateStats(),
                deployment.ToGridCoord());
        }
    }
}

