namespace FE.Core
{
    public interface IBattleCommand
    {
        BattleCommandResult Execute(BattleContext context);
    }
}
