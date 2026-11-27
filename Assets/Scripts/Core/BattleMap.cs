using System;
using System.Collections.Generic;

namespace FE.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleMap
    {
        private readonly TerrainType[,] _terrainTypes;

        public int Width { get; }
        public int Height { get; }

        public BattleMap(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), width, "width error:" + width.ToString());
            }

            Width = width;
            Height = height;
            _terrainTypes = new TerrainType[Width, Height];

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    _terrainTypes[i, j] = TerrainType.Plain;
                }
            }
        }

        public bool IsInside(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        public bool IsInside(GridCoord grid)
        {
            return IsInside(grid.x, grid.y);
        }

        public bool IsWalkable(GridCoord grid)
        {
            return CanEnter(grid, MovementType.Foot);
        }

        public bool CanEnter(
            GridCoord grid,
            MovementType movementType)
        {
            return IsInside(grid) &&
                   GetTerrain(grid).CanEnter(movementType);
        }

        public int GetMovementCost(GridCoord grid)
        {
            return GetTerrain(grid).MovementCost;
        }

        public TerrainProfile GetTerrain(GridCoord grid)
        {
            return TerrainCatalog.Get(GetTerrainType(grid));
        }

        public TerrainType GetTerrainType(GridCoord grid)
        {
            if (!IsInside(grid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(grid),
                    grid,
                    "Grid coordinate must be inside the map.");
            }

            return _terrainTypes[grid.x, grid.y];
        }

        public void SetTerrain(
            GridCoord grid,
            TerrainType terrainType)
        {
            if (!IsInside(grid))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(grid),
                    grid,
                    "Grid coordinate must be inside the map.");
            }

            _terrainTypes[grid.x, grid.y] = terrainType;
        }

        internal TerrainType[,] CaptureTerrainTypes()
        {
            return (TerrainType[,])_terrainTypes.Clone();
        }

        internal void RestoreTerrainTypes(
            TerrainType[,] terrainTypes)
        {
            if (terrainTypes == null ||
                terrainTypes.GetLength(0) != Width ||
                terrainTypes.GetLength(1) != Height)
            {
                throw new ArgumentException(
                    "Terrain snapshot dimensions do not match the map.",
                    nameof(terrainTypes));
            }

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _terrainTypes[x, y] = terrainTypes[x, y];
                }
            }
        }
    }
}


