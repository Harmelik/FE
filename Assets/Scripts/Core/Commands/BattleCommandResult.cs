namespace FE.Core
{
    public sealed class BattleCommandResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public CombatAttackResult? AttackResult { get; }
        public BattlePhaseTransitionResult? PhaseTransition { get; }

        private BattleCommandResult(
            bool isSuccess,
            string errorMessage,
            CombatAttackResult? attackResult,
            BattlePhaseTransitionResult? phaseTransition)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            AttackResult = attackResult;
            PhaseTransition = phaseTransition;
        }

        public static BattleCommandResult Success()
        {
            return new BattleCommandResult(
                true,
                string.Empty,
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
                null);
        }

        public static BattleCommandResult Success(
            BattlePhaseTransitionResult phaseTransition)
        {
            return new BattleCommandResult(
                true,
                string.Empty,
                null,
                phaseTransition);
        }

        public static BattleCommandResult Failure(string errorMessage)
        {
            return new BattleCommandResult(
                false,
                errorMessage,
                null,
                null);
        }
    }
}
