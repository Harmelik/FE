
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

        public BattleContext(
            BattleMap map,
            BattleUnitController units)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            Units = units ?? throw new ArgumentNullException(nameof(units));
        }
    }
}

