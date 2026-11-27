using FE.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(
    fileName = "BattleMapConfig",
    menuName = "FE/Battle/Battle Map Config")]
    public class BattleMapConfig : ScriptableObject
    {
        [SerializeField, Min(1)]
        private int width = 10;

        [SerializeField, Min(1)]
        private int height = 8;

        [SerializeField]
        private List<TerrainPlacementData> terrainPlacements = new();

        public int Width => width;
        public int Height => height;

        public BattleMap CreateMap()
        {
            BattleMap map = new BattleMap(width, height);

            foreach (TerrainPlacementData placement in terrainPlacements)
            {
                if (placement == null)
                {
                    continue;
                }

                map.SetTerrain(
                    placement.ToGridCoord(),
                    placement.TerrainType);
            }

            return map;
        }

        private void OnValidate()
        {
            var occupiedPositions = new HashSet<GridCoord>();

            foreach (TerrainPlacementData placement in terrainPlacements)
            {
                if (placement == null)
                {
                    continue;
                }

                GridCoord coord = placement.ToGridCoord();
                if (coord.x < 0 ||
                    coord.y < 0 ||
                    coord.x >= width ||
                    coord.y >= height)
                {
                    Debug.LogWarning(
                        $"{name}: terrain placement {coord} is outside the map.",
                        this);
                }

                if (!occupiedPositions.Add(coord))
                {
                    Debug.LogWarning(
                        $"{name}: terrain placement {coord} is duplicated.",
                        this);
                }
            }
        }
    }

    [Serializable]
    public sealed class TerrainPlacementData
    {
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private TerrainType terrainType;

        public Vector2Int GridPosition => gridPosition;
        public TerrainType TerrainType => terrainType;

        public GridCoord ToGridCoord()
        {
            return new GridCoord(
                gridPosition.x,
                gridPosition.y);
        }
    }
}

