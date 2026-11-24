using System;
using System.Collections;
using System.Collections.Generic;
using FE.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleForecastView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerUnitName;
        [SerializeField] private TMP_Text playerLeftHP;
        [SerializeField] private TMP_Text playerAtk;
        [SerializeField] private TMP_Text playerHit;
        [SerializeField] private Slider playerHPSlider;
        [SerializeField] private TMP_Text enemyUnitName;
        [SerializeField] private TMP_Text enemyLeftHP;
        [SerializeField] private TMP_Text enemyAtk;
        [SerializeField] private TMP_Text enemyHit;
        [SerializeField] private Slider enemyHPSlider;
        [SerializeField] private CanvasGroup canvasGroup;
        private Action onEscape;
        private Action onChangeToPreviousTarget;
        private Action onChangeToNextTarget;
        private Action onConfirm;

        private void Awake()
        {
            SetVisible(false);
            //Input.GetKey(KeyCode.Escape)
        }

        public void Hide()
        {
            ClearActions();
            SetVisible(false);
        }

        public void SetActions(
            Action escapeAction,
            Action changeToPreviousTarget,
            Action changeToNextTarget,
            Action confirmAction)
        {
            onEscape = escapeAction;
            onChangeToPreviousTarget = changeToPreviousTarget;
            onChangeToNextTarget = changeToNextTarget;
            onConfirm = confirmAction;
        }

        public void Show(
            CombatForecastResult forecast,
            BattleUnit player,
            BattleUnit enemy)
        {
            if (player == null || enemy == null)
            {
                Hide();
                return;
            }

            SetVisible(true);
            playerUnitName.text = player.DisplayName;
            enemyUnitName.text = enemy.DisplayName;

            CombatSideForecast playerForecast = forecast.AttackerForecast;
            CombatSideForecast enemyForecast = forecast.DefenderForecast;

            playerLeftHP.text = playerForecast.ProjectedHp.ToString();
            playerHPSlider.value =
                playerForecast.ProjectedHp / (float)playerForecast.MaxHp;
            playerAtk.text = playerForecast.Damage.ToString();
            playerHit.text = $"{playerForecast.HitChance}%";

            enemyLeftHP.text = enemyForecast.ProjectedHp.ToString();
            enemyHPSlider.value =
                enemyForecast.ProjectedHp / (float)enemyForecast.MaxHp;
            enemyAtk.text = enemyForecast.CanAttack
                ? enemyForecast.Damage.ToString()
                : "--";
            enemyHit.text = enemyForecast.CanAttack
                ? $"{enemyForecast.HitChance}%"
                : "--";
        }

        private void Update()
        {
            if (canvasGroup == null || canvasGroup.alpha != 1) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                onEscape?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                onConfirm?.Invoke();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                onChangeToPreviousTarget?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                onChangeToNextTarget?.Invoke();
            }
        }

        private void ClearActions()
        {
            onEscape = null;
            onChangeToPreviousTarget = null;
            onChangeToNextTarget = null;
            onConfirm = null;
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
