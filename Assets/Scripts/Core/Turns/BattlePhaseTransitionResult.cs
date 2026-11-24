using System;

namespace FE.Core
{
    public readonly struct BattlePhaseTransitionResult
    {
        public BattlePhase PreviousPhase { get; }
        public BattlePhase NewPhase { get; }
        public int PreviousTurnNumber { get; }
        public int NewTurnNumber { get; }

        public BattlePhaseTransitionResult(
            BattlePhase previousPhase,
            BattlePhase newPhase,
            int previousTurnNumber,
            int newTurnNumber)
        {
            if (previousPhase == BattlePhase.BattleEnded)
            {
                throw new ArgumentException(
                    "BattleEnded cannot transition to another phase.");
            }

            if (previousTurnNumber < 1 || newTurnNumber < previousTurnNumber)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(previousTurnNumber),
                    "Turn numbers must be positive and cannot decrease.");
            }

            PreviousPhase = previousPhase;
            NewPhase = newPhase;
            PreviousTurnNumber = previousTurnNumber;
            NewTurnNumber = newTurnNumber;
        }
    }
}
