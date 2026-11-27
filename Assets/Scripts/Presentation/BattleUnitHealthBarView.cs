using FE.Core;
using UnityEngine;
using UnityEngine.UI;

namespace FE.Presentation
{
    /// <summary>
    /// Updates a prefab-authored world-space health bar.
    /// Transform, orientation and layout belong to the prefab hierarchy.
    /// </summary>
    public class BattleUnitHealthBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private void Awake()
        {
            ResolveReferences();
        }

        public void Init(Faction faction)
        {
            ResolveReferences();

            if (fillImage != null)
            {
                fillImage.color = GetFactionColor(faction);
            }
        }

        public void SetHealth(int currentHp, int maxHp)
        {
            ResolveReferences();

            if (fillImage == null)
            {
                return;
            }

            fillImage.fillAmount = maxHp <= 0
                ? 0f
                : Mathf.Clamp01(currentHp / (float)maxHp);
        }

        public static Color GetFactionColor(Faction faction)
        {
            return faction == Faction.Player
                ? new Color(0.16f, 0.62f, 1f, 1f)
                : new Color(1f, 0.18f, 0.12f, 1f);
        }

        private void ResolveReferences()
        {
            if (fillImage != null)
            {
                return;
            }

            Transform fill = transform.Find("Fill");
            if (fill != null)
            {
                fillImage = fill.GetComponent<Image>();
            }
        }
    }
}
