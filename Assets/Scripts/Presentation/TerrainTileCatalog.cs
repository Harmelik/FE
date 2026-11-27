using System;
using System.Collections.Generic;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    [CreateAssetMenu(
        fileName = "TerrainTileCatalog",
        menuName = "FE/Battle/Terrain Tile Catalog")]
    public class TerrainTileCatalog : ScriptableObject
    {
        [Serializable]
        private sealed class Entry
        {
            [SerializeField] private TerrainType terrainType;
            [SerializeField] private GridTileView tilePrefab;

            public TerrainType TerrainType => terrainType;
            public GridTileView TilePrefab => tilePrefab;
        }

        [SerializeField] private List<Entry> entries = new();

        public GridTileView GetPrefab(TerrainType terrainType)
        {
            foreach (Entry entry in entries)
            {
                if (entry != null &&
                    entry.TerrainType == terrainType)
                {
                    return entry.TilePrefab;
                }
            }

            throw new InvalidOperationException(
                $"Terrain tile catalog has no prefab for {terrainType}.");
        }

        private void OnValidate()
        {
            var terrainTypes = new HashSet<TerrainType>();

            foreach (Entry entry in entries)
            {
                if (entry == null)
                {
                    continue;
                }

                if (!terrainTypes.Add(entry.TerrainType))
                {
                    Debug.LogWarning(
                        $"{name}: terrain {entry.TerrainType} is duplicated.",
                        this);
                }

                if (entry.TilePrefab == null)
                {
                    Debug.LogWarning(
                        $"{name}: terrain {entry.TerrainType} has no prefab.",
                        this);
                }
            }
        }
    }
}
