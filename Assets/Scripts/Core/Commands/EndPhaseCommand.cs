namespace FE.Core
{
    public sealed class EndPhaseCommand : IBattleCommand
    {
        public BattleCommandResult Execute(BattleContext context)
        {
            if (context == null)
            {
                return BattleCommandResult.Failure(
                    "Battle context is null.");
            }

            if (context.Turns.CurrentPhase == BattlePhase.BattleEnded)
            {
                return BattleCommandResult.Failure(
                    "The battle has already ended.");
            }

            BattlePhaseTransitionResult transition =
                context.Turns.AdvancePhase();
            return BattleCommandResult.Success(transition);
        }
    }
}
