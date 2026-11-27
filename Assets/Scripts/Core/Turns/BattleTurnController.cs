using System;

namespace FE.Core
{
    public class BattleTurnController
    {
        private readonly BattleUnitController units;

        public int TurnNumber { get; private set; }
        public BattlePhase CurrentPhase { get; private set; }
        public BattleOutcomeResult CurrentOutcome { get; private set; }

        public BattleTurnController(BattleUnitController units)
        {
            this.units = units ??
                throw new ArgumentNullException(nameof(units));

            TurnNumber = 1;
            CurrentPhase = BattlePhase.PlayerPhase;
            CurrentOutcome = default;
            ResetFactionActionState(Faction.Player);
        }

        public bool CanFactionAct(Faction faction)
        {
            return CurrentPhase switch
            {
                BattlePhase.PlayerPhase => faction == Faction.Player,
                BattlePhase.EnemyPhase => faction == Faction.Enemy,
                _ => false
            };
        }

        public bool CanUnitAct(BattleUnit unit)
        {
            return unit != null &&
                   !unit.IsDead &&
                   !unit.HasActed &&
                   CanFactionAct(unit.Fac);
        }

        internal BattlePhaseTransitionResult AdvancePhase()
        {
            BattlePhase previousPhase = CurrentPhase;
            int previousTurn = TurnNumber;

            switch (CurrentPhase)
            {
                case BattlePhase.PlayerPhase:
                    CurrentPhase = BattlePhase.EnemyPhase;
                    ResetFactionActionState(Faction.Enemy);
                    break;

                case BattlePhase.EnemyPhase:
                    CurrentPhase = BattlePhase.PlayerPhase;
                    TurnNumber++;
                    ResetFactionActionState(Faction.Player);
                    break;

                default:
                    throw new InvalidOperationException(
                        "The battle has already ended.");
            }

            return new BattlePhaseTransitionResult(
                previousPhase,
                CurrentPhase,
                previousTurn,
                TurnNumber);
        }

        internal bool EndBattle(BattleOutcomeResult outcome)
        {
            if (!outcome.IsBattleEnded ||
                CurrentPhase == BattlePhase.BattleEnded)
            {
                return false;
            }

            CurrentPhase = BattlePhase.BattleEnded;
            CurrentOutcome = outcome;
            return true;
        }

        private void ResetFactionActionState(Faction faction)
        {
            foreach (BattleUnit unit in units.AllUnits)
            {
                if (unit.Fac == faction && !unit.IsDead)
                {
                    unit.ResetActionState();
                }
            }
        }

        internal void RestoreState(
            int turnNumber,
            BattlePhase currentPhase,
            BattleOutcomeResult currentOutcome)
        {
            if (turnNumber < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(turnNumber),
                    turnNumber,
                    "Turn number must be positive.");
            }

            TurnNumber = turnNumber;
            CurrentPhase = currentPhase;
            CurrentOutcome = currentOutcome;
        }
    }
}
