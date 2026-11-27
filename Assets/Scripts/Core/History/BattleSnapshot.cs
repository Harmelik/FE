using System;
using System.Collections.ObjectModel;

namespace FE.Core
{
    public readonly struct BattleUnitState
    {
        public UnitId UnitId { get; }
        public int CurrentHP { get; }
        public GridCoord Grid { get; }
        public bool HasActed { get; }
        public WeaponProfile EquippedWeapon { get; }

        public BattleUnitState(
            UnitId unitId,
            int currentHP,
            GridCoord grid,
            bool hasActed,
            WeaponProfile equippedWeapon)
        {
            if (currentHP < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(currentHP),
                    currentHP,
                    "HP cannot be negative.");
            }

            UnitId = unitId;
            CurrentHP = currentHP;
            Grid = grid;
            HasActed = hasActed;
            EquippedWeapon = equippedWeapon ??
                throw new ArgumentNullException(nameof(equippedWeapon));
        }
    }

    public sealed class BattleSnapshot
    {
        private readonly BattleUnitState[] unitStates;
        private readonly TerrainType[,] terrainTypes;

        public int MapWidth { get; }
        public int MapHeight { get; }
        public int TurnNumber { get; }
        public BattlePhase Phase { get; }
        public BattleOutcomeResult Outcome { get; }
        public BattleRandomState RandomState { get; }
        public ReadOnlyCollection<BattleUnitState> UnitStates { get; }

        internal BattleSnapshot(
            int mapWidth,
            int mapHeight,
            int turnNumber,
            BattlePhase phase,
            BattleOutcomeResult outcome,
            BattleRandomState randomState,
            BattleUnitState[] unitStates,
            TerrainType[,] terrainTypes)
        {
            MapWidth = mapWidth;
            MapHeight = mapHeight;
            TurnNumber = turnNumber;
            Phase = phase;
            Outcome = outcome;
            RandomState = randomState;
            this.unitStates = unitStates ??
                throw new ArgumentNullException(nameof(unitStates));
            this.terrainTypes = terrainTypes ??
                throw new ArgumentNullException(nameof(terrainTypes));
            UnitStates = Array.AsReadOnly(this.unitStates);
        }

        public TerrainType GetTerrainType(int x, int y)
        {
            if (x < 0 || y < 0 ||
                x >= MapWidth || y >= MapHeight)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(x),
                    "Terrain coordinate is outside the snapshot.");
            }

            return terrainTypes[x, y];
        }

        internal TerrainType[,] CaptureTerrainTypes()
        {
            return (TerrainType[,])terrainTypes.Clone();
        }
    }
}
