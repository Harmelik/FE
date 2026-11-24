using UnityEngine;

namespace FE.Presentation
{
    public class BattlePopTextController : MonoBehaviour
    {
        [SerializeField] private Camera battleCamera;
        [SerializeField] private RectTransform floatingTextRoot;
        [SerializeField] private BattlePopTextView floatingTextPrefab;
        [SerializeField]
        private Color normalDamageColor =
            new Color(1f, 0.92f, 0.35f, 1f);
        [SerializeField]
        private Color counterDamageColor =
            new Color(1f, 0.45f, 0.25f, 1f);
        [SerializeField]
        private Color missColor =
            new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField, Min(0f)] private float horizontalJitter = 14f;

        public void ShowDamage(
            Vector3 worldPosition,
            int damage,
            bool isCounterattack = false)
        {
            Color color = isCounterattack
                ? counterDamageColor
                : normalDamageColor;

            Show(
                worldPosition,
                Mathf.Max(0, damage).ToString(),
                color);
        }

        public void ShowMiss(
            Vector3 worldPosition,
            bool isCounterattack = false)
        {
            Show(
                worldPosition,
                "MISS",
                missColor);
        }

        private void Show(
            Vector3 worldPosition,
            string value,
            Color color)
        {
            if (battleCamera == null ||
                floatingTextRoot == null ||
                floatingTextPrefab == null)
            {
                Debug.LogError(
                    "BattleFloatingTextController has missing scene references.",
                    this);
                return;
            }

            Vector3 screenPoint =
                battleCamera.WorldToScreenPoint(worldPosition);

            if (screenPoint.z < 0f)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    floatingTextRoot,
                    screenPoint,
                    null,
                    out Vector2 localPoint))
            {
                return;
            }

            BattlePopTextView popup = Instantiate(
                floatingTextPrefab,
                floatingTextRoot,
                false);

            float jitter = horizontalJitter > 0f
                ? Random.Range(-horizontalJitter, horizontalJitter)
                : 0f;

            popup.RectTransform.anchoredPosition =
                localPoint + new Vector2(jitter, 0f);

            popup.Play(value, color, () =>
            {
                if (popup != null)
                {
                    Destroy(popup.gameObject);
                }
            });
        }
    }
}