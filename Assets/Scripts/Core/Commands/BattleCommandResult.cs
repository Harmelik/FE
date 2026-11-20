namespace FE.Core
{
    public sealed class BattleCommandResult
    {
        public bool IsSuccess { get; }
        public string ErrorMessage { get; }

        private BattleCommandResult(bool isSuccess, string errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public static BattleCommandResult Success()
        {
            return new BattleCommandResult(true, string.Empty);
        }

        public static BattleCommandResult Failure(string errorMessage)
        {
            return new BattleCommandResult(false, errorMessage);
        }
    }
}
