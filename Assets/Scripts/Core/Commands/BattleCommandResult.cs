namespace FE.Core
{
    public sealed class BattleCommandResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public CombatAttackResult? AttackResult { get; }
        public BattlePhaseTransitionResult? PhaseTransition { get; }
        public BattleOutcomeResult? BattleOutcome { get; }

        private BattleCommandResult(
            bool isSuccess,
            string errorMessage,
            CombatAttackResult? attackResult,
            BattlePhaseTransitionResult? phaseTransition,
            BattleOutcomeResult? battleOutcome)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            AttackResult = attackResult;
            PhaseTransition = phaseTransition;
            BattleOutcome = battleOutcome;
        }

        public static BattleCommandResult Success()
        {
            return new BattleCommandResult(
                true,
                string.Empty,
                null,
                null,
                null);
        }

        public static BattleCommandResult Success(
            CombatAttackResult attackResult)
        {
            return new BattleCommandResult(
                true,
                string.Empty,
                attackResult,
                null,
                null);
        }

        public static BattleCommandResult Success(
            BattlePhaseTransitionResult phaseTransition)
        {
            return new BattleCommandResult(
                true,
                string.Empty,
                null,
                phaseTransition,
                null);
        }

        public static BattleCommandResult Failure(string errorMessage)
        {
            return new BattleCommandResult(
                false,
                errorMessage,
                null,
                null,
                null);
        }

        public BattleCommandResult WithBattleOutcome(
            BattleOutcomeResult battleOutcome)
        {
            return new BattleCommandResult(
                IsSuccess,
                ErrorMessage,
                AttackResult,
                PhaseTransition,
                battleOutcome);
        }
    }
}
