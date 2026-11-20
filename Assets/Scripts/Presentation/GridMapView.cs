using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridMapView : MonoBehaviour
    {
        [SerializeField] private GridTileView tilePrefab;

        private GridTileView[,] tileObjects;

        private bool hasHoveredGrid;
        private bool hasSelectedGrid;
        private GridCoord curHoveredGrid;
        private GridCoord curSelectedGrid;
        private Dictionary<GridCoord, HashSet<UnitId>> inMovementGrids = new();
        private Dictionary<GridCoord, HashSet<UnitId>> inAttackGrids = new();

        public void CreateTiles(BattleMap map, GridGeometry gg)
        {
            ClearTiles();

            tileObjects = new GridTileView[map.Width, map.Height];

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    GridCoord grid = new GridCoord(i, j);
                    var go = Instantiate(tilePrefab, transform);
                    go.name = $"{i}_{j}";
                    go.transform.position = gg.GetCellCenter(grid);
                    tileObjects[i,j] = go;
                }
            }
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
            hasHoveredGrid = false;
            hasSelectedGrid = false;
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
            foreach (var grid in inMovementGrids)
            {
                if (!TryGetTile(grid.Key, out GridTileView tileView))
                {
                    continue;
                }

                tileView.SetMovement(grid.Value.Count > 0);
            }
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

        private void RefreshAttackRange()
        {
            foreach (var grid in inAttackGrids)
            {
                if (!TryGetTile(grid.Key, out GridTileView tileView))
                {
                    continue;
                }

                tileView.SetAttack(grid.Value.Count > 0);
                if(!inMovementGrids.ContainsKey(grid.Key)) continue;
                tileView.SetMovement(inMovementGrids[grid.Key].Count > 0);
            }
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
    }
}

