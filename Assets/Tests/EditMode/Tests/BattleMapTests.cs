using System;
using FE.Core;
using NUnit.Framework;

namespace FE.Tests
{
    public class BattleMapTests
    {
        [Test]
        public void Constructor_StoresDimensions()
        {
            var map = new BattleMap(10, 8);

            Assert.That(map.Width, Is.EqualTo(10));
            Assert.That(map.Height, Is.EqualTo(8));
        }

        [TestCase(0, 0, true)]
        [TestCase(9, 7, true)]
        [TestCase(-1, 0, false)]
        [TestCase(0, -1, false)]
        [TestCase(10, 0, false)]
        [TestCase(0, 8, false)]
        public void IsInside_UsesHalfOpenBounds(int x, int y, bool expected)
        {
            var map = new BattleMap(10, 8);

            Assert.That(
                map.IsInside(new GridCoord(x, y)),
                Is.EqualTo(expected));
        }

        [Test]
        public void IsWalkable_IsTrueForEveryCellByDefault()
        {
            var map = new BattleMap(10, 8);

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    Assert.That(
                        map.IsWalkable(new GridCoord(x, y)),
                        Is.True,
                        $"Cell ({x}, {y}) should be walkable.");
                }
            }
        }

        [TestCase(0, 8)]
        [TestCase(10, 0)]
        [TestCase(-1, 8)]
        [TestCase(10, -1)]
        public void Constructor_WithInvalidDimension_Throws(int width, int height)
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new BattleMap(width, height));
        }
    }
}