using System;
using FE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleActionMenuView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button waitButton;
        [SerializeField] private Button cancelButton;

        private Action onAttack;
        private Action onWait;
        private Action onCancel;
        private bool isReady;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
            //attackButton.onClick.AddListener();
            attackButton.onClick.RemoveListener(OnAttackClicked);
            waitButton.onClick.RemoveListener(OnWaitClicked);
            cancelButton.onClick.RemoveListener(OnCancelClicked);

            attackButton.onClick.AddListener(OnAttackClicked);
            waitButton.onClick.AddListener(OnWaitClicked);
            cancelButton.onClick.AddListener(OnCancelClicked);

            isReady = true;
            Hide();
        }

        private void OnAttackClicked()
        {
            Action callback = onAttack;
            Hide();
            callback?.Invoke();
        }

        private void OnWaitClicked()
        {
            Action callback = onWait;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            Action callback = onCancel;
            Hide();
            callback?.Invoke();
        }

        public void Show(
            BattleUnit unit,
            bool canAttack,
            Action attackCallback,
            Action waitCallback,
            Action cancelCallback)
        {
            if (!isReady || unit == null)
            {
                return;
            }

            onAttack = attackCallback;
            onWait = waitCallback;
            onCancel = cancelCallback;

            attackButton.interactable = canAttack && attackCallback != null;

            SetVisible(true);
        }

        public void Hide()
        {
            onAttack = null;
            onWait = null;
            onCancel = null;

            SetVisible(false);
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
            if (attackButton != null)
            {
                attackButton.onClick.RemoveListener(OnAttackClicked);
            }

            if (waitButton != null)
            {
                waitButton.onClick.RemoveListener(OnWaitClicked);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(OnCancelClicked);
            }
        }
    }
}

