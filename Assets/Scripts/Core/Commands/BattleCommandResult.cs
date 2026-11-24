namespace FE.Core
{
    public sealed class BattleCommandResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }
        public CombatAttackResult? AttackResult { get; }

        private BattleCommandResult(
            bool isSuccess,
            string errorMessage,
            CombatAttackResult? attackResult)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
            AttackResult = attackResult;
        }

        public static BattleCommandResult Success(
            CombatAttackResult? attackResult = null)
        {
            return new BattleCommandResult(
                true,
                string.Empty,
                attackResult);
        }

        public static BattleCommandResult Failure(string errorMessage)
        {
            return new BattleCommandResult(
                false,
                errorMessage,
                null);
        }
    }
}
