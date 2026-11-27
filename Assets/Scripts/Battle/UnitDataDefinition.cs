using FE.Core;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(
    fileName = "BattleUnitConfig",
    menuName = "FE/Battle/Battle Unit Definition")]
    public class UnitDataDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;
        [SerializeField, Min(1)] private int level = 1;
        [SerializeField] private UnitClassDefinition unitClass;
        [SerializeField] private WeaponDefinition equippedWeapon;
        [SerializeField, Min(1)] private int maxHP = 25;
        [SerializeField, Min(0)] private int pAtk = 10;
        [SerializeField, Min(0)] private int phyDef = 3;
        [SerializeField, Min(0)] private int mAtk = 10;
        [SerializeField, Min(0)] private int magicDef = 3;
        [SerializeField, Min(0)] private int hit = 80;
        [SerializeField, Min(0)] private int avoid = 0;

        public string DisplayName => displayName;
        public int Level => level;
        public UnitClassDefinition UnitClass => unitClass;
        public WeaponDefinition EquippedWeapon => equippedWeapon;

        public UnitStats CreateStats()
        {
            return new UnitStats(
                maxHP:this.maxHP,
                patk:this.pAtk,
                phyDef:this.phyDef,
                matk:this.mAtk,
                magicDef:this.magicDef,
                hit: this.hit,
                avoid: this.avoid
            );
        }

        private void OnValidate()
        {
            if (unitClass == null)
            {
                Debug.LogWarning(
                    $"{name}: unitClass is not assigned.",
                    this);
            }

            if (equippedWeapon == null)
            {
                Debug.LogWarning(
                    $"{name}: equippedWeapon is not assigned.",
                    this);
            }

            if (unitClass != null &&
                equippedWeapon != null &&
                (unitClass.EquippableWeaponCategories &
                 equippedWeapon.Category) != equippedWeapon.Category)
            {
                Debug.LogWarning(
                    $"{name}: {unitClass.DisplayName} cannot equip " +
                    $"{equippedWeapon.DisplayName}.",
                    this);
            }
        }
    }
}

