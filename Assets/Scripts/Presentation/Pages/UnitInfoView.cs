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
        [SerializeField] private TMP_Text m_ATK;
        [SerializeField] private TMP_Text m_PDEF;
        [SerializeField] private TMP_Text m_MDEF;

        private void Awake()
        {
            Hide();
        }

        public void Show(BattleUnit unit)
        {
            SetVisible(true);
            m_UnitName.text = unit.DisplayName;
            m_OccupationName.text = unit.DisplayName;
            var patk = unit.BaseStats.PAtk;
            var matk = unit.BaseStats.MAtk;
            m_ATK.text = (patk > matk ? patk : matk).ToString();
            m_HP.text = $"{unit.CurHP}/{unit.BaseStats.MaxHP}";
            m_PDEF.text = unit.BaseStats.PhyDef.ToString();
            m_MDEF.text = unit.BaseStats.MagicDef.ToString();
        }

        private void Hide()
        {
            SetVisible(false);
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

