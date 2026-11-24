
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
        public BattleTurnController Turns { get; }

        public BattleContext(
            BattleMap map,
            BattleUnitController units,
            IBattleRandom battleRandom,
            BattleTurnController turns)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Units = units ?? throw new ArgumentNullException(nameof(units));
            BattleRandom = battleRandom ??
                throw new ArgumentNullException(nameof(battleRandom));
            Turns = turns ?? throw new ArgumentNullException(nameof(turns));
        }
    }
}

