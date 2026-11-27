using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridMapView : MonoBehaviour
    {
        [SerializeField] private TerrainTileCatalog terrainTileCatalog;

        private GridTileView[,] tileObjects;
        private GridGeometry geometry;

        private bool hasHoveredGrid;
        private bool hasSelectedGrid;
        private GridCoord curHoveredGrid;
        private GridCoord curSelectedGrid;
        private Dictionary<GridCoord, HashSet<UnitId>> inMovementGrids = new();
        private Dictionary<GridCoord, HashSet<UnitId>> inAttackGrids = new();
        private Dictionary<GridCoord, HashSet<UnitId>> inHoverMovementGrids = new();
        private Dictionary<GridCoord, HashSet<UnitId>> inHoverAttackGrids = new();

        public void CreateTiles(BattleMap map, GridGeometry gg)
        {
            ClearTiles();

            geometry = gg;
            tileObjects = new GridTileView[map.Width, map.Height];

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    GridCoord grid = new GridCoord(i, j);
                    TerrainType terrainType =
                        map.GetTerrainType(grid);
                    GridTileView tilePrefab =
                        terrainTileCatalog.GetPrefab(terrainType);
                    GridTileView tile =
                        Instantiate(tilePrefab, transform);
                    tile.name =
                        $"{i}_{j}_{terrainType}";
                    tile.transform.position =
                        gg.GetCellCenter(grid);
                    tileObjects[i, j] = tile;
                }
            }
        }

        public Vector3 GetWorldPosition(
            GridCoord grid,
            float extraHeight = 0f)
        {
            float surfaceHeight =
                TryGetTile(grid, out GridTileView tile)
                    ? tile.SurfaceHeight
                    : 0f;

            return geometry.GetCellCenter(grid) +
                   Vector3.up * (surfaceHeight + extraHeight);
        }

        public void RefreshTerrain(BattleMap map)
        {
            if (map == null || geometry == null)
            {
                return;
            }

            CreateTiles(map, geometry);
        }

        private void ClearTiles()
        {
            if (tileObjects != null)
            {
                foreach (var tile in tileObjects)
                {
                    if (tile != null)
                    {
                        Destroy(tile);
                    }
                }
            }

            tileObjects = null;
            geometry = null;
            hasHoveredGrid = false;
            hasSelectedGrid = false;
            inMovementGrids.Clear();
            inAttackGrids.Clear();
            inHoverMovementGrids.Clear();
            inHoverAttackGrids.Clear();
        }

        public void SetHovered(GridCoord grid)
        {
            if (curHoveredGrid == grid && hasHoveredGrid) return;

            if (hasHoveredGrid && TryGetTile(curHoveredGrid, out var oldTile))
            {
                oldTile.SetHovered(false);
            }

            hasHoveredGrid = false;

            if (!TryGetTile(grid, out GridTileView tileView))
            {
                return;
            }

            hasHoveredGrid = true;
            curHoveredGrid = grid;
            tileView.SetHovered(true);
        }

        public void ClearHovered()
        {
            if (!hasHoveredGrid) return;

            if (TryGetTile(curHoveredGrid, out var tile))
            {
                tile.SetHovered(false);
            }

            hasHoveredGrid = false;
        }

        public void SetSelected(GridCoord grid)
        {
            if (curSelectedGrid == grid && hasSelectedGrid) return;

            if (!TryGetTile(grid, out GridTileView tileView))
            {
                return;
            }

            if (hasSelectedGrid && TryGetTile(curSelectedGrid, out var oldTile))
            {
                oldTile.SetSelected(false);
            }

            hasSelectedGrid = true;
            curSelectedGrid = grid;
            tileView.SetSelected(true);
        }

        public void ClearSelected()
        {
            if (!hasSelectedGrid)
            {
                return;
            }

            if (TryGetTile(curSelectedGrid, out GridTileView tile))
            {
                tile.SetSelected(false);
            }

            hasSelectedGrid = false;
        }

        public void SetInMovement(MovementRange range, UnitId id)
        {
            RemoveMovementRangeSource(id);
            foreach (var grid in range.Destinations)
            {
                if (!inMovementGrids.TryGetValue(grid, out var ids))
                {
                    if (ids == null) ids = new HashSet<UnitId>();
                    ids.Add(id);
                    inMovementGrids.Add(grid, ids);
                }
                else
                { 
                    ids.Add(id);
                }
            }

            RefreshMovement();
        }

        public void SetOffMovement(UnitId id)
        {
            if (inMovementGrids == null) return;
            RemoveMovementRangeSource(id);
            //if()inMovementGrids = null;
            RefreshMovement();
        }

        private void RefreshMovement()
        {
            RefreshRangeTiles();
        }

        private void RemoveMovementRangeSource(UnitId unitId)
        {
            foreach (HashSet<UnitId> ids in inMovementGrids.Values)
            {
                ids.Remove(unitId);
            }
        }

        public void SetInAttackRange(
            IEnumerable<GridCoord> attackRange,
            UnitId unitId)
        {
            if (attackRange == null)
            {
                return;
            }

            RemoveAttackRangeSource(unitId);

            foreach (GridCoord grid in attackRange)
            {
                if (!inAttackGrids.TryGetValue(
                        grid,
                        out HashSet<UnitId> ids))
                {
                    ids = new HashSet<UnitId>();
                    inAttackGrids.Add(grid, ids);
                }

                ids.Add(unitId);
            }

            RefreshAttackRange();
        }

        private void RemoveAttackRangeSource(UnitId unitId)
        {
            foreach (HashSet<UnitId> ids in inAttackGrids.Values)
            {
                ids.Remove(unitId);
            }
        }

        public void SetOffAttackRange(UnitId unitId)
        {
            RemoveAttackRangeSource(unitId);
            RefreshAttackRange();
        }

        public void SetInHoverMovement(
            MovementRange range,
            UnitId unitId)
        {
            if (range == null)
            {
                return;
            }

            RemoveHoverMovementRangeSource(unitId);

            foreach (GridCoord grid in range.Destinations)
            {
                if (!inHoverMovementGrids.TryGetValue(
                        grid,
                        out HashSet<UnitId> ids))
                {
                    ids = new HashSet<UnitId>();
                    inHoverMovementGrids.Add(grid, ids);
                }

                ids.Add(unitId);
            }

            RefreshMovement();
        }

        public void SetOffHoverMovement(UnitId unitId)
        {
            RemoveHoverMovementRangeSource(unitId);
            RefreshMovement();
        }

        public void SetInHoverAttackRange(
            IEnumerable<GridCoord> attackRange,
            UnitId unitId)
        {
            if (attackRange == null)
            {
                return;
            }

            RemoveHoverAttackRangeSource(unitId);

            foreach (GridCoord grid in attackRange)
            {
                if (!inHoverAttackGrids.TryGetValue(
                        grid,
                        out HashSet<UnitId> ids))
                {
                    ids = new HashSet<UnitId>();
                    inHoverAttackGrids.Add(grid, ids);
                }

                ids.Add(unitId);
            }

            RefreshAttackRange();
        }

        public void SetOffHoverAttackRange(UnitId unitId)
        {
            RemoveHoverAttackRangeSource(unitId);
            RefreshAttackRange();
        }

        private void RefreshAttackRange()
        {
            RefreshRangeTiles();
        }

        private void RemoveHoverMovementRangeSource(UnitId unitId)
        {
            foreach (HashSet<UnitId> ids in
                     inHoverMovementGrids.Values)
            {
                ids.Remove(unitId);
            }
        }

        private void RemoveHoverAttackRangeSource(UnitId unitId)
        {
            foreach (HashSet<UnitId> ids in
                     inHoverAttackGrids.Values)
            {
                ids.Remove(unitId);
            }
        }

        private void RefreshRangeTiles()
        {
            if (tileObjects == null)
            {
                return;
            }

            for (int x = 0; x < tileObjects.GetLength(0); x++)
            {
                for (int y = 0; y < tileObjects.GetLength(1); y++)
                {
                    GridTileView tileView = tileObjects[x, y];
                    if (tileView == null)
                    {
                        continue;
                    }

                    var grid = new GridCoord(x, y);
                    tileView.SetMovement(
                        HasRangeSource(inMovementGrids, grid));
                    tileView.SetAttack(
                        HasRangeSource(inAttackGrids, grid));
                    tileView.SetHoverMovement(
                        HasRangeSource(
                            inHoverMovementGrids,
                            grid));
                    tileView.SetHoverAttack(
                        HasRangeSource(
                            inHoverAttackGrids,
                            grid));
                }
            }
        }

        private static bool HasRangeSource(
            Dictionary<GridCoord, HashSet<UnitId>> ranges,
            GridCoord grid)
        {
            return ranges.TryGetValue(
                       grid,
                       out HashSet<UnitId> ids) &&
                   ids.Count > 0;
        }

        private bool TryGetTile(GridCoord grid, out GridTileView tileView)
        {
            tileView = null;

            if (tileObjects == null)
            {
                return false;
            }

            var width = tileObjects.GetLength(0);
            var height = tileObjects.GetLength(1);
            if (grid.x >= width || grid.y >= height || grid.x < 0 || grid.y < 0)
            {
                return false;
            }

            tileView = tileObjects[grid.x, grid.y];
            return tileView != null;
        }

        private void OnValidate()
        {
            if (terrainTileCatalog == null)
            {
                Debug.LogWarning(
                    $"{name}: terrainTileCatalog is not assigned.",
                    this);
            }
        }
    }
}

