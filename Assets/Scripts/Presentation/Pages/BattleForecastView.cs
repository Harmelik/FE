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
        public Action EscapeAction;
        public Action ChangeToPreviousTarget;
        public Action ChangeToNextTarget;

        private void Awake()
        {
            SetVisible(false);
            //Input.GetKey(KeyCode.Escape)
        }

        public void Hide()
        {
            this.SetVisible(false);
        }

        public void Show(BattleUnit player, BattleUnit enemy)
        {
            SetVisible(true);
            //TODO 先在这里做简单计算，后面估计得委托给别的地方做然后在这里只负责展示
            playerUnitName.text = player.DisplayName;
            enemyUnitName.text = enemy.DisplayName;

            var playerGotDmg = Math.Max(0, enemy.BaseStats.PAtk - player.BaseStats.PhyDef);
            playerLeftHP.text = (player.CurHP - playerGotDmg).ToString();
            playerHPSlider.value = ((player.CurHP - playerGotDmg) / (float)player.BaseStats.MaxHP);
            enemyAtk.text = playerGotDmg.ToString();
            playerHit.text = $"{player.BaseStats.Hit - enemy.BaseStats.Avoid}%";

            var enemyGotDmg = Math.Max(0, player.BaseStats.PAtk - enemy.BaseStats.PhyDef);
            enemyLeftHP.text = (enemy.CurHP - enemyGotDmg).ToString();
            enemyHPSlider.value = ((enemy.CurHP - enemyGotDmg) / (float)enemy.BaseStats.MaxHP);
            playerAtk.text = enemyGotDmg.ToString();
            enemyHit.text = $"{enemy.BaseStats.Hit - player.BaseStats.Avoid}%";
        }

        private void Update()
        {
            if (canvasGroup == null || canvasGroup.alpha != 1) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                EscapeAction?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.Q))
            {
                ChangeToPreviousTarget?.Invoke();
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                ChangeToNextTarget?.Invoke();
            }
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
