using FE.Core;

namespace FE.Presentation
{
    public static class BattleHudTextFormatter
    {
        public static string FormatTurn(int turnNumber)
        {
            return $"TURN {turnNumber}";
        }

        public static string FormatPhase(BattlePhase phase)
        {
            return phase switch
            {
                BattlePhase.PlayerPhase => "PLAYER PHASE",
                BattlePhase.EnemyPhase => "ENEMY PHASE",
                BattlePhase.BattleEnded => "BATTLE ENDED",
                _ => string.Empty
            };
        }
    }
}
