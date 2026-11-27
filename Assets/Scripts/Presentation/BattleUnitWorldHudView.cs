using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// Root data binder for all world-space information shown on one unit.
    /// Future portrait, weapon and stat widgets should be added here rather
    /// than creating independent world-space canvases.
    /// </summary>
    public class BattleUnitWorldHudView : MonoBehaviour
    {
        [SerializeField] private BattleUnitHealthBarView healthBar;
        [SerializeField] private WorldBillboard billboard;

        private BattleUnit unit;

        public void Bind(BattleUnit unit, Camera battleCamera)
        {
            this.unit = unit;

            healthBar?.Init(unit.Fac);
            billboard?.Init(battleCamera);
            RefreshHealth();
        }

        public void RefreshHealth()
        {
            if (unit == null || healthBar == null)
            {
                return;
            }

            healthBar.SetHealth(
                unit.CurHP,
                unit.BaseStats.MaxHP);
        }

        private void Awake()
        {
            healthBar ??= GetComponentInChildren<
                BattleUnitHealthBarView>(true);
            billboard ??= GetComponent<WorldBillboard>();
        }
    }
}
