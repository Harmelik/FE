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
        [SerializeField] private GridTileView tilePrefab;

        private GridTileView[,] tileObjects;

        private bool hasHoveredGrid;
        private bool hasSelectedGrid;
        private GridCoord curHoveredGrid;
        private GridCoord curSelectedGrid;

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

