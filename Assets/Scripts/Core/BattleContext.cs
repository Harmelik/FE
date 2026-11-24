
using System;

namespace FE.Core
{
    /// <summary>
    /// 战斗运行时
    /// </summary>
    public class BattleContext
    {
        public BattleMap Map { get; }
        public BattleUnitController Units { get; }
        public IBattleRandom BattleRandom { get; }

        public BattleContext(
            BattleMap map,
            BattleUnitController units,
            IBattleRandom battleRandom)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Units = units ?? throw new ArgumentNullException(nameof(units));
            BattleRandom = battleRandom ??
                throw new ArgumentNullException(nameof(battleRandom));
        }
    }
}

