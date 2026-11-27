using FE.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleOutcomeView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text reasonText;

        private bool isVisible;

        private void Awake()
        {
            EnsureRuntimeUI();
            Hide();
        }

        public void Show(BattleOutcomeResult outcome)
        {
            EnsureRuntimeUI();

            if (!outcome.IsBattleEnded || isVisible)
            {
                return;
            }

            titleText.text = GetTitle(outcome.OutcomeType);
            reasonText.text = GetReason(outcome.Reason);
            titleText.color =
                outcome.OutcomeType == BattleOutcomeType.PlayerVictory
                    ? new Color(1f, 0.85f, 0.3f, 1f)
                    : new Color(0.95f, 0.3f, 0.25f, 1f);

            isVisible = true;
            SetVisible(true);
        }

        public void Hide()
        {
            isVisible = false;
            SetVisible(false);
        }

        private static string GetTitle(BattleOutcomeType outcomeType)
        {
            return outcomeType switch
            {
                BattleOutcomeType.PlayerVictory => "VICTORY",
                BattleOutcomeType.PlayerDefeat => "DEFEAT",
                _ => string.Empty
            };
        }

        private static string GetReason(BattleOutcomeReason reason)
        {
            return reason switch
            {
                BattleOutcomeReason.AllEnemiesDefeated =>
                    "All enemies defeated",
                BattleOutcomeReason.AllPlayersDefeated =>
                    "All player units defeated",
                BattleOutcomeReason.BothSidesDefeated =>
                    "Both sides defeated",
                _ => string.Empty
            };
        }

        private void EnsureRuntimeUI()
        {
            if (canvasGroup != null &&
                titleText != null &&
                reasonText != null)
            {
                return;
            }

            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            if (GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler =
                    gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode =
                    CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution =
                    new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }

            RectTransform root = GetComponent<RectTransform>();
            if (root == null)
            {
                root = gameObject.AddComponent<RectTransform>();
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            canvasGroup ??= GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            CreateInputBlocker(root);
            titleText ??= CreateText(
                root,
                "OutcomeTitle",
                new Vector2(1000f, 130f),
                new Vector2(0f, 60f),
                72f);
            reasonText ??= CreateText(
                root,
                "OutcomeReason",
                new Vector2(1000f, 80f),
                new Vector2(0f, -45f),
                32f);
        }

        private static void CreateInputBlocker(RectTransform root)
        {
            GameObject blocker = new GameObject(
                "InputBlocker",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            blocker.transform.SetParent(root, false);

            RectTransform rect =
                blocker.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = blocker.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.65f);
            image.raycastTarget = true;
        }

        private static TMP_Text CreateText(
            RectTransform root,
            string name,
            Vector2 size,
            Vector2 position,
            float fontSize)
        {
            GameObject textObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            textObject.transform.SetParent(root, false);

            RectTransform rect =
                textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            TextMeshProUGUI text =
                textObject.GetComponent<TextMeshProUGUI>();
            text.alignment = TextAlignmentOptions.Center;
            text.fontSize = fontSize;
            text.raycastTarget = false;
            return text;
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
