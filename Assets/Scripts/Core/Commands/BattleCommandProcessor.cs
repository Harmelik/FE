using System;

namespace FE.Core
{
    public sealed class BattleCommandProcessor
    {
        private readonly BattleContext context;

        public BattleCommandProcessor(BattleContext context)
        {
            this.context = context ??
                throw new ArgumentNullException(nameof(context));
        }

        public BattleCommandResult Execute(IBattleCommand command)
        {
            if (command == null)
            {
                return BattleCommandResult.Failure(
                    "Cannot execute a null battle command.");
            }

            return command.Execute(context);
        }
    }
}
