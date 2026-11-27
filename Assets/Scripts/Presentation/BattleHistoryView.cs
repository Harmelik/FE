using System;
using System.Collections.Generic;
using FE.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    public class BattleHistoryView : MonoBehaviour
    {
        private const float EntryHeight = 64f;
        private const float EntrySpacing = 8f;

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private BattleHistoryEntryView entryPrefab;
        [SerializeField] private BattleHistoryButtonView confirmButton;
        [SerializeField] private BattleHistoryButtonView cancelButton;
        [SerializeField] private TMP_Text emptyHint;

        private readonly List<BattleHistoryEntryView> entryViews = new();
        private int selectedIndex = -1;

        public event Action<int> EntrySelected;
        public event Action ConfirmRequested;
        public event Action CancelRequested;

        public int SelectedIndex => selectedIndex;

        private void Awake()
        {
            canvasGroup ??= GetComponent<CanvasGroup>();
            confirmButton?.Bind("回溯到此行动前", OnConfirmClicked);
            cancelButton?.Bind("取消", OnCancelClicked);
            Hide();
        }

        public void Show(
            IReadOnlyList<BattleHistoryEntry> entries,
            BattleContext context)
        {
            ClearEntryViews();
            SetVisible(true);

            if (entries == null || entries.Count == 0)
            {
                if (emptyHint != null)
                {
                    emptyHint.gameObject.SetActive(true);
                }

                confirmButton?.SetInteractable(false);
                selectedIndex = -1;
                return;
            }

            if (emptyHint != null)
            {
                emptyHint.gameObject.SetActive(false);
            }

            confirmButton?.SetInteractable(true);

            for (int i = 0; i < entries.Count; i++)
            {
                BattleHistoryEntryView entryView =
                    Instantiate(entryPrefab, content);
                RectTransform rect =
                    (RectTransform)entryView.transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.sizeDelta = new Vector2(0f, EntryHeight);
                rect.anchoredPosition = new Vector2(
                    0f,
                    -i * (EntryHeight + EntrySpacing));
                entryView.Bind(
                    i,
                    entries[i],
                    context,
                    SelectIndex);
                entryViews.Add(entryView);
            }

            float contentHeight =
                entries.Count * EntryHeight +
                Mathf.Max(0, entries.Count - 1) * EntrySpacing;

            if (content != null)
            {
                content.sizeDelta =
                    new Vector2(content.sizeDelta.x, contentHeight);
            }

            Canvas.ForceUpdateCanvases();
            SelectIndex(entries.Count - 1);
            ScrollToSelection();
        }

        public void Hide()
        {
            SetVisible(false);
        }

        public void Scroll(float delta)
        {
            if (content == null || viewport == null)
            {
                return;
            }

            float maxScroll = Mathf.Max(
                0f,
                content.rect.height - viewport.rect.height);
            Vector2 position = content.anchoredPosition;
            position.y = Mathf.Clamp(
                position.y - delta,
                0f,
                maxScroll);
            content.anchoredPosition = position;
        }

        public void SelectPrevious()
        {
            SelectIndex(selectedIndex - 1);
        }

        public void SelectNext()
        {
            SelectIndex(selectedIndex + 1);
        }

        public void ConfirmSelection()
        {
            OnConfirmClicked();
        }

        public void Cancel()
        {
            OnCancelClicked();
        }

        private void SelectIndex(int index)
        {
            if (entryViews.Count == 0)
            {
                return;
            }

            int clamped = Mathf.Clamp(
                index,
                0,
                entryViews.Count - 1);

            if (selectedIndex >= 0 &&
                selectedIndex < entryViews.Count)
            {
                entryViews[selectedIndex].SetSelected(false);
            }

            selectedIndex = clamped;
            entryViews[selectedIndex].SetSelected(true);
            EntrySelected?.Invoke(selectedIndex);
            ScrollToSelection();
        }

        private void ScrollToSelection()
        {
            if (content == null ||
                viewport == null ||
                selectedIndex < 0)
            {
                return;
            }

            float rowTop =
                selectedIndex * (EntryHeight + EntrySpacing);
            float rowBottom = rowTop + EntryHeight;
            float visibleTop = content.anchoredPosition.y;
            float visibleBottom =
                visibleTop + viewport.rect.height;
            float offset = 0f;

            if (rowTop < visibleTop)
            {
                offset = rowTop - visibleTop;
            }
            else if (rowBottom > visibleBottom)
            {
                offset = rowBottom - visibleBottom;
            }

            if (Mathf.Abs(offset) <= 0.001f)
            {
                return;
            }

            float maxScroll = Mathf.Max(
                0f,
                content.rect.height - viewport.rect.height);
            Vector2 position = content.anchoredPosition;
            position.y = Mathf.Clamp(
                position.y + offset,
                0f,
                maxScroll);
            content.anchoredPosition = position;
        }

        private void ClearEntryViews()
        {
            foreach (BattleHistoryEntryView view in entryViews)
            {
                if (view != null)
                {
                    view.gameObject.SetActive(false);
                    Destroy(view.gameObject);
                }
            }

            entryViews.Clear();
            selectedIndex = -1;
        }

        private void SetVisible(bool isVisible)
        {
            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }

        private void OnConfirmClicked()
        {
            if (selectedIndex < 0)
            {
                return;
            }

            ConfirmRequested?.Invoke();
        }

        private void OnCancelClicked()
        {
            CancelRequested?.Invoke();
        }
    }
}
