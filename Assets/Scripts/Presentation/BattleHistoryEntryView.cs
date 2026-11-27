using System;
using FE.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleHistoryEntryView :
        MonoBehaviour,
        IPointerClickHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private Image factionStripe;
        [SerializeField] private Image selectionBar;
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private TMP_Text summaryText;

        private int entryIndex;
        private Action<int> selectedCallback;
        private Color baseBackgroundColor;
        private Color baseStripeColor;

        public void Bind(
            int index,
            BattleHistoryEntry entry,
            BattleContext context,
            Action<int> onSelected)
        {
            entryIndex = index;
            selectedCallback = onSelected;

            if (turnText != null)
            {
                turnText.text = $"第 {entry.TurnNumber} 回合";
            }

            if (summaryText != null)
            {
                summaryText.text =
                    BattleHistoryTextFormatter.Format(entry, context);
            }

            baseBackgroundColor = entry.ActorFaction switch
            {
                Faction.Player =>
                    new Color(0.06f, 0.2f, 0.42f, 0.92f),
                Faction.Enemy =>
                    new Color(0.34f, 0.07f, 0.1f, 0.92f),
                _ => new Color(0.24f, 0.19f, 0.32f, 0.92f)
            };
            baseStripeColor = entry.ActorFaction switch
            {
                Faction.Player =>
                    new Color(0.32f, 0.72f, 1f, 1f),
                Faction.Enemy =>
                    new Color(1f, 0.35f, 0.32f, 1f),
                _ => new Color(0.72f, 0.62f, 1f, 1f)
            };

            if (background != null)
            {
                background.color = baseBackgroundColor;
            }

            if (factionStripe != null)
            {
                factionStripe.color = baseStripeColor;
            }

            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (background != null)
            {
                background.color = isSelected
                    ? Color.Lerp(
                        baseBackgroundColor,
                        Color.white,
                        0.24f)
                    : baseBackgroundColor;
            }

            if (selectionBar != null)
            {
                selectionBar.enabled = isSelected;
                selectionBar.color =
                    new Color(0.45f, 0.92f, 1f, 1f);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button !=
                PointerEventData.InputButton.Left)
            {
                return;
            }

            selectedCallback?.Invoke(entryIndex);
        }
    }
}
