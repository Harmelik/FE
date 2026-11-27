using System;

namespace FE.Core
{
    public enum MovementType
    {
        Foot,
        Flying
    }

    public enum TerrainType
    {
        Plain,
        EmpowermentArray,
        Forest,
        Mountain,
        Wall
    }

    /// <summary>
    /// Immutable terrain rules used by movement and combat queries.
    /// </summary>
    public sealed class TerrainProfile
    {
        public TerrainType Type { get; }
        public string DisplayName { get; }
        public int MovementCost { get; }
        public bool WalkableByFoot { get; }
        public bool WalkableByFlying { get; }
        public int PAtkBonus { get; }
        public int MAtkBonus { get; }
        public int PhyDefBonus { get; }
        public int MagicDefBonus { get; }
        public int AvoidBonus { get; }

        public TerrainProfile(
            TerrainType type,
            string displayName,
            int movementCost,
            bool walkableByFoot,
            bool walkableByFlying,
            int pAtkBonus = 0,
            int mAtkBonus = 0,
            int phyDefBonus = 0,
            int magicDefBonus = 0,
            int avoidBonus = 0)
        {
            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException(
                    "Terrain display name cannot be empty.",
                    nameof(displayName));
            }

            if (movementCost <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(movementCost),
                    movementCost,
                    "Terrain movement cost must be greater than zero.");
            }

            if (pAtkBonus < 0 ||
                mAtkBonus < 0 ||
                phyDefBonus < 0 ||
                magicDefBonus < 0 ||
                avoidBonus < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pAtkBonus),
                    "Terrain stat bonuses cannot be negative.");
            }

            Type = type;
            DisplayName = displayName;
            MovementCost = movementCost;
            WalkableByFoot = walkableByFoot;
            WalkableByFlying = walkableByFlying;
            PAtkBonus = pAtkBonus;
            MAtkBonus = mAtkBonus;
            PhyDefBonus = phyDefBonus;
            MagicDefBonus = magicDefBonus;
            AvoidBonus = avoidBonus;
        }

        public bool CanEnter(MovementType movementType)
        {
            return movementType == MovementType.Flying
                ? WalkableByFlying
                : WalkableByFoot;
        }

        public int GetAttackBonus(DamageType damageType)
        {
            return damageType == DamageType.Magical
                ? MAtkBonus
                : PAtkBonus;
        }

        public int GetDefenseBonus(DamageType damageType)
        {
            return damageType == DamageType.Magical
                ? MagicDefBonus
                : PhyDefBonus;
        }
    }

    public static class TerrainCatalog
    {
        private static readonly TerrainProfile Plain =
            new TerrainProfile(
                TerrainType.Plain,
                "平地",
                1,
                true,
                true);

        private static readonly TerrainProfile EmpowermentArray =
            new TerrainProfile(
                TerrainType.EmpowermentArray,
                "强化阵",
                1,
                true,
                true,
                pAtkBonus: 3,
                mAtkBonus: 3,
                avoidBonus: 20);

        private static readonly TerrainProfile Forest =
            new TerrainProfile(
                TerrainType.Forest,
                "森林",
                2,
                true,
                true,
                phyDefBonus: 1,
                magicDefBonus: 1,
                avoidBonus: 30);

        private static readonly TerrainProfile Mountain =
            new TerrainProfile(
                TerrainType.Mountain,
                "山地",
                1,
                false,
                true);

        private static readonly TerrainProfile Wall =
            new TerrainProfile(
                TerrainType.Wall,
                "墙壁",
                1,
                false,
                false);

        public static TerrainProfile Get(TerrainType type)
        {
            return type switch
            {
                TerrainType.Plain => Plain,
                TerrainType.EmpowermentArray => EmpowermentArray,
                TerrainType.Forest => Forest,
                TerrainType.Mountain => Mountain,
                TerrainType.Wall => Wall,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unknown terrain type.")
            };
        }
    }
}
