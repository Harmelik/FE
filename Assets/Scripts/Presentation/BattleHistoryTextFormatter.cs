using FE.Core;

namespace FE.Presentation
{
    public static class BattleHistoryTextFormatter
    {
        public static string Format(
            BattleHistoryEntry entry,
            BattleContext context)
        {
            if (entry == null || context == null)
            {
                return string.Empty;
            }

            if (entry.Kind == BattleHistoryKind.PhaseTransition &&
                entry.PhaseTransition.HasValue)
            {
                BattlePhaseTransitionResult transition =
                    entry.PhaseTransition.Value;
                string phaseName =
                    transition.NewPhase == BattlePhase.EnemyPhase
                        ? "敌方阶段开始"
                        : "我方阶段开始";
                return $"第 {transition.NewTurnNumber} 回合 · {phaseName}";
            }

            string actorName = GetUnitName(
                context,
                entry.ActorUnitId);

            switch (entry.PrimaryActionType)
            {
                case BattleHistoryActionType.Attack:
                    return FormatAttack(entry, context, actorName);

                case BattleHistoryActionType.Wait:
                    return FormatWait(entry, context, actorName);

                case BattleHistoryActionType.Equip:
                    return $"{actorName}更换装备";

                default:
                    return $"{actorName}完成行动";
            }
        }

        private static string FormatAttack(
            BattleHistoryEntry entry,
            BattleContext context,
            string actorName)
        {
            string targetName = GetUnitName(
                context,
                entry.TargetUnitId);

            if (!entry.AttackResult.HasValue)
            {
                return $"{actorName}攻击{targetName}";
            }

            CombatAttackResult attack = entry.AttackResult.Value;
            string result;

            if (!attack.AttackerHit)
            {
                result = $"{actorName}攻击{targetName}未命中";
            }
            else
            {
                result =
                    $"{actorName}攻击{targetName}，" +
                    $"造成{attack.AttackerDamage}伤害";
            }

            if (attack.Countered)
            {
                result += attack.DefenderHit
                    ? $"；受到{attack.DefenderDamage}伤害反击"
                    : "；反击未命中";
            }

            if (attack.DefenderDefeated)
            {
                result += "，目标被击败";
            }
            else if (attack.AttackerDefeated)
            {
                result += "，攻击者被击败";
            }

            return result;
        }

        private static string FormatWait(
            BattleHistoryEntry entry,
            BattleContext context,
            string actorName)
        {
            if (!entry.Destination.HasValue ||
                !entry.ActorUnitId.HasValue)
            {
                return $"{actorName}待命";
            }

            BattleUnitState? beforeActor = null;
            foreach (BattleUnitState unitState in
                     entry.BeforeState.UnitStates)
            {
                if (unitState.UnitId == entry.ActorUnitId.Value)
                {
                    beforeActor = unitState;
                    break;
                }
            }

            if (beforeActor.HasValue &&
                beforeActor.Value.Grid != entry.Destination.Value)
            {
                GridCoord destination = entry.Destination.Value;
                return
                    $"{actorName}移动到({destination.x},{destination.y})后待命";
            }

            return $"{actorName}待命";
        }

        private static string GetUnitName(
            BattleContext context,
            UnitId? unitId)
        {
            if (!unitId.HasValue)
            {
                return "未知单位";
            }

            BattleUnit unit =
                context.Units.GetUnitById(unitId.Value);
            return unit != null ? unit.DisplayName : "未知单位";
        }
    }
}
