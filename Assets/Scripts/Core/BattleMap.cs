using System;
using System.Collections;
using System.Collections.Generic;

namespace FE.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleMap
    {
        //可进入状态
        private readonly bool[,] _walkable;

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
            _walkable = new bool[Width, Height];

            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    _walkable[i, j] = true;
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
            return IsInside(grid) && _walkable[grid.x, grid.y];
        }
    }
}


