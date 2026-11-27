using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleHistoryButtonView :
        MonoBehaviour,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color normalColor =
            new Color(0.08f, 0.22f, 0.4f, 0.96f);
        [SerializeField] private Color hoverColor =
            new Color(0.12f, 0.36f, 0.62f, 0.98f);
        [SerializeField] private Color disabledColor =
            new Color(0.14f, 0.15f, 0.18f, 0.8f);

        private Action clicked;
        private bool interactable = true;

        public void Bind(string text, Action onClicked)
        {
            clicked = onClicked;

            if (label != null)
            {
                label.text = text;
            }
        }

        public void SetInteractable(bool value)
        {
            interactable = value;

            if (background != null)
            {
                background.color =
                    value ? normalColor : disabledColor;
                background.raycastTarget = true;
            }

            if (label != null)
            {
                label.alpha = value ? 1f : 0.55f;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable ||
                eventData.button !=
                PointerEventData.InputButton.Left)
            {
                return;
            }

            clicked?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (interactable && background != null)
            {
                background.color = hoverColor;
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (background != null)
            {
                background.color =
                    interactable ? normalColor : disabledColor;
            }
        }
    }
}
