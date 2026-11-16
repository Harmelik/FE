using System;

namespace FE.Core
{
    /// <summary>
    /// 
    /// </summary>
    public readonly struct GridCoord : IEquatable<GridCoord>
    {
        public readonly int x;
        public readonly int y;

        public GridCoord(int a, int b)
        {
            x = a; y = b;
        }

        public bool Equals(GridCoord other)
        {
            return x == other.x && y == other.y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridCoord other && Equals(other);
        }

        public static bool operator ==(GridCoord lhs, GridCoord rhs) { return lhs.Equals(rhs); }

        public static bool operator !=(GridCoord lhs, GridCoord rhs) { return !lhs.Equals(rhs); }

        public override string ToString()
        {
            return $"({x},{y})";
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 189) ^ y;
            }
        }
    }
}


