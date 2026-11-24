using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleCombatPresentationController : MonoBehaviour
    {
        [SerializeField] private BattlePopTextController popTextController;
        [SerializeField, Min(0f)] private float popupHeightOffset = 0.8f;
        public IEnumerator PlayCombatRoutine(BattleUnitView attackerView, BattleUnitView defenderView,
        CombatAttackResult? result, Action onComplete)
        {
            if (attackerView == null || defenderView == null || result is not {} attackResult)
            {
                onComplete?.Invoke();
                yield return null;
                yield break;
            }

            yield return attackerView.PlayActionLunge(attackResult.DefenderGrid).WaitForCompletion();

            if (attackResult.AttackerHit)
            {
                ShowCombatFeedback(defenderView, true, attackResult.AttackerDamage, false);

                yield return defenderView.PlayHitReaction().WaitForCompletion();
            }
            else
            {
                ShowCombatFeedback(defenderView, false, 0, false);
            }

            yield return attackerView.PlayReturnToGrid(attackResult.AttackerGrid).WaitForCompletion();

            if (attackResult.DefenderDefeated)
            {
                yield return defenderView.PlayDefeat().WaitForCompletion();
                onComplete?.Invoke();
                yield break;
            }

            if (!attackResult.Countered)
            {
                onComplete?.Invoke();
                yield break;
            }

            yield return defenderView.PlayActionLunge(attackResult.AttackerGrid).WaitForCompletion();
            if (attackResult.DefenderHit)
            {
                ShowCombatFeedback(attackerView, true, attackResult.DefenderDamage, true);

                yield return attackerView.PlayHitReaction().WaitForCompletion();
            }
            else
            {
                ShowCombatFeedback(attackerView, false, 0, true);
            }

            yield return defenderView.PlayReturnToGrid(attackResult.DefenderGrid).WaitForCompletion();

            if (attackResult.AttackerDefeated)
            {
                yield return attackerView.PlayDefeat().WaitForCompletion();
            }

            onComplete?.Invoke();
        }

        private void ShowCombatFeedback(
            BattleUnitView targetView,
            bool hit,
            int damage,
            bool isCounterattack)
        {
            if (targetView == null || popTextController == null)
            {
                return;
            }

            Vector3 popupPosition =
                targetView.transform.position +
                Vector3.up * popupHeightOffset;

            if (hit)
            {
                popTextController.ShowDamage(
                    popupPosition,
                    damage,
                    isCounterattack);
            }
            else
            {
                popTextController.ShowMiss(
                    popupPosition,
                    isCounterattack);
            }
        }
    }
}

