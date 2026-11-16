using System;
using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using FE.Core;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 
    /// </summary>
    public class GridGeometry
    {
        public Vector3 Origin { get; }
        public float CellSize { get; }

        public GridGeometry(Vector3 origin, float cellSize)
        {
            if (cellSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cellSize), cellSize, "cellSize out of Range, must greater than 0");
            }

            Origin = new Vector3();
            Origin = origin;
            CellSize = cellSize;
        }

        public Vector3 GetCellCenter(GridCoord grid)
        {
            float halfCell = CellSize * 0.5f;

            return Origin + new Vector3(grid.x * CellSize + halfCell
                , 0f
                , grid.y * CellSize + halfCell) ;
        }

        public GridCoord WorldPosToGrid(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - Origin;

            int x = Mathf.FloorToInt((localPos.x) / CellSize);
            int y = Mathf.FloorToInt((localPos.y) / CellSize);

            return new GridCoord(x, y);
        }
    }
}

