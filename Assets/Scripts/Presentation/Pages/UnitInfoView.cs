using FE.Core;
using TMPro;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class UnitInfoView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text m_UnitName;
        [SerializeField] private TMP_Text m_OccupationName;
        [SerializeField] private TMP_Text m_Lv;
        [SerializeField] private TMP_Text m_HP;
        [SerializeField] private TMP_Text m_ATKLabel;
        [SerializeField] private TMP_Text m_ATK;
        [SerializeField] private TMP_Text m_PDEF;
        [SerializeField] private TMP_Text m_MDEF;

        private void Awake()
        {
            ResolveReferences();
            Hide();
        }

        public void Show(BattleUnit unit)
        {
            Show(unit, null);
        }

        public void Show(BattleUnit unit, BattleMap map)
        {
            if (unit == null || unit.IsDead)
            {
                Hide();
                return;
            }

            SetVisible(true);
            SetText(m_UnitName, unit.DisplayName);
            SetText(m_OccupationName, unit.UnitClass.DisplayName);
            SetText(m_Lv, unit.Level.ToString());
            bool isMagical =
                unit.EquippedWeapon.DamageType == DamageType.Magical;
            TerrainProfile terrain =
                map != null && map.IsInside(unit.CurGrid)
                    ? map.GetTerrain(unit.CurGrid)
                    : TerrainCatalog.Get(TerrainType.Plain);

            SetText(m_ATKLabel, isMagical ? "MATK" : "PATK");
            SetText(
                m_ATK,
                unit.GetAttackPower(terrain).ToString());
            SetText(
                m_HP,
                $"{unit.CurHP}/{unit.BaseStats.MaxHP}");
            SetText(
                m_PDEF,
                (unit.BaseStats.PhyDef +
                 terrain.PhyDefBonus).ToString());
            SetText(
                m_MDEF,
                (unit.BaseStats.MagicDef +
                 terrain.MagicDefBonus).ToString());
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void ResolveReferences()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            m_UnitName ??= FindText("UnitName");
            m_OccupationName ??= FindText("OccupationName");
            m_Lv ??= FindText("LvText") ??
                     FindText("Lv") ??
                     FindText("LevelText");
            m_HP ??= FindText("HPText");
            m_ATKLabel ??= FindText("Atk");
            m_ATK ??= FindText("AtkText");
            m_PDEF ??= FindText("PDEFText");
            m_MDEF ??= FindText("MDEFText");
        }

        private TMP_Text FindText(string objectName)
        {
            foreach (TMP_Text text in
                     GetComponentsInChildren<TMP_Text>(true))
            {
                if (text.gameObject.name == objectName)
                {
                    return text;
                }
            }

            return null;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
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
    }
}

