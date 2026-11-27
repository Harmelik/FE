using FE.Core;
using UnityEngine;

namespace FE.Battle
{
    [CreateAssetMenu(
        fileName = "Weapon",
        menuName = "FE/Battle/Weapon Definition")]
    public class WeaponDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "新武器";
        [SerializeField] private WeaponCategory category = WeaponCategory.Sword;
        [SerializeField] private DamageType damageType = DamageType.Physical;
        [SerializeField, Min(0)] private int might = 5;
        [SerializeField] private int hit;
        [SerializeField, Range(0, 100)] private int crit;
        [SerializeField, Min(1)] private int minRange = 1;
        [SerializeField, Min(1)] private int maxRange = 1;

        public string DisplayName => displayName;
        public WeaponCategory Category => category;
        public DamageType DamageType => damageType;
        public int Might => might;
        public int Hit => hit;
        public int Crit => crit;
        public int MinRange => minRange;
        public int MaxRange => maxRange;

        public WeaponProfile CreateProfile()
        {
            return new WeaponProfile(
                displayName,
                category,
                damageType,
                might,
                hit,
                crit,
                minRange,
                maxRange);
        }

        private void OnValidate()
        {
            int categoryValue = (int)category;
            if (categoryValue == 0 ||
                (categoryValue & (categoryValue - 1)) != 0)
            {
                Debug.LogWarning(
                    $"{name}: select exactly one weapon category.",
                    this);
            }

            if (maxRange < minRange)
            {
                Debug.LogWarning(
                    $"{name}: maxRange cannot be smaller than minRange.",
                    this);
            }
        }
    }
}
