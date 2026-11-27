namespace FE.Core
{
    public interface IEnemyActionProvider
    {
        bool TryPlan(
            BattleContext context,
            BattleUnit enemy,
            out EnemyActionPlan plan);
    }
}
