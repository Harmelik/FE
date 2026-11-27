using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// Global battle menu that blocks map input while open.
    /// Only the end-phase command is available in this first version.
    /// </summary>
    public class BattleMenuView : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Button endPhaseButton;
        private Action onEndPhase;

        public bool IsVisible => canvasGroup != null &&
                                 canvasGroup.alpha > 0f;

        public void Init(Action endPhaseAction)
        {
            onEndPhase = endPhaseAction;
            EnsureRuntimeUI();
            Hide();
        }

        public void Show()
        {
            EnsureRuntimeUI();
            SetVisible(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    endPhaseButton.gameObject);
            }
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void EnsureRuntimeUI()
        {
            if (canvasGroup != null &&
                endPhaseButton != null)
            {
                return;
            }

            Canvas canvas = GetComponent<Canvas>() ??
                             gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;

            CanvasScaler scaler = GetComponent<CanvasScaler>() ??
                                  gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            RectTransform root = GetComponent<RectTransform>();
            if (root == null)
            {
                root = gameObject.AddComponent<RectTransform>();
            }
            Stretch(root);

            canvasGroup = GetComponent<CanvasGroup>() ??
                          gameObject.AddComponent<CanvasGroup>();

            Image backdrop = CreateImage(
                root,
                "Backdrop",
                new Color(0f, 0f, 0f, 0.68f));
            Stretch(backdrop.rectTransform);

            Image panel = CreateImage(
                root,
                "MenuPanel",
                new Color(0.08f, 0.11f, 0.14f, 0.97f));
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(560f, 330f);

            TMP_Text title = CreateText(
                panelRect,
                "Title",
                "BATTLE MENU",
                40f,
                new Vector2(0f, 96f),
                new Vector2(480f, 70f));
            title.color = new Color(0.92f, 0.86f, 0.65f, 1f);

            endPhaseButton = CreateButton(
                panelRect,
                "EndPhaseButton",
                "END PHASE",
                new Vector2(0f, -45f));
            endPhaseButton.onClick.AddListener(OnEndPhaseClicked);

            TMP_Text hint = CreateText(
                panelRect,
                "Hint",
                "Esc: Close",
                20f,
                new Vector2(0f, -126f),
                new Vector2(420f, 40f));
            hint.color = new Color(0.68f, 0.74f, 0.8f, 1f);
        }

        private Button CreateButton(
            RectTransform parent,
            string objectName,
            string label,
            Vector2 position)
        {
            GameObject buttonObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect =
                buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(340f, 74f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.2f, 0.34f, 0.46f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.88f, 0.94f, 1f, 1f);
            colors.pressedColor = new Color(0.72f, 0.82f, 0.92f, 1f);
            button.colors = colors;

            TMP_Text text = CreateText(
                rect,
                "Label",
                label,
                28f,
                Vector2.zero,
                new Vector2(320f, 60f));
            text.color = Color.white;
            return button;
        }

        private static Image CreateImage(
            RectTransform parent,
            string objectName,
            Color color)
        {
            GameObject imageObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            imageObject.transform.SetParent(parent, false);

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static TMP_Text CreateText(
            RectTransform parent,
            string objectName,
            string value,
            float fontSize,
            Vector2 position,
            Vector2 size)
        {
            GameObject textObject = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);

            RectTransform rect =
                textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI text =
                textObject.GetComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void OnEndPhaseClicked()
        {
            Action callback = onEndPhase;
            callback?.Invoke();
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

        private void OnDestroy()
        {
            if (endPhaseButton != null)
            {
                endPhaseButton.onClick.RemoveListener(
                    OnEndPhaseClicked);
            }
        }
    }
}
