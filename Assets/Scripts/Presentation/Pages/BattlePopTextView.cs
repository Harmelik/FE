using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace FE.Presentation
{
    public class BattlePopTextView : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField, Min(10f)] private float floatDistance = 40f;
        [SerializeField, Min(0.1f)] private float duration = 0.6f;
        [SerializeField, Min(0f)] private float fadeStartTime = 0.35f;

        private Sequence popupTween;

        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            if (label == null)
            {
                label = GetComponent<TMP_Text>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Play(string value, Color color, Action onCompleted)
        {
            popupTween?.Kill();
            popupTween = null;

            if (label == null || canvasGroup == null || rectTransform == null)
            {
                onCompleted?.Invoke();
                return;
            }

            label.text = value;
            label.color = color;
            label.raycastTarget = false;
            canvasGroup.alpha = 1f;

            Vector2 startPosition = rectTransform.anchoredPosition;
            float clampedDuration = Mathf.Max(0.1f, duration);
            float fadeStart = Mathf.Clamp(
                fadeStartTime,
                0f,
                clampedDuration - 0.01f);
            float fadeDuration =
                Mathf.Max(0.01f, clampedDuration - fadeStart);

            popupTween = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(
                    rectTransform.DOAnchorPos(
                        startPosition + Vector2.up * floatDistance,
                        clampedDuration)
                    .SetEase(Ease.OutCubic))
                .Insert(
                    fadeStart,
                    canvasGroup.DOFade(0f, fadeDuration)
                        .SetEase(Ease.InQuad))
                .OnComplete(() =>
                {
                    popupTween = null;
                    onCompleted?.Invoke();
                })
                .Play();
        }

        private void OnDestroy()
        {
            popupTween?.Kill();
            popupTween = null;
        }
    }
}
