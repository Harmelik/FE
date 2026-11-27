using FE.Core;
using UnityEngine;

namespace FE.Battle
{
    [CreateAssetMenu(
        fileName = "UnitClass",
        menuName = "FE/Battle/Unit Class Definition")]
    public class UnitClassDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "新职业";

        [SerializeField]
        private WeaponCategory equippableWeaponCategories =
            WeaponCategory.Sword;

        [SerializeField, Min(0)] private int movement = 4;
        [SerializeField] private MovementType movementType =
            MovementType.Foot;

        public string DisplayName => displayName;
        public WeaponCategory EquippableWeaponCategories =>
            equippableWeaponCategories;
        public int Movement => movement;
        public MovementType MovementType => movementType;

        public UnitClassProfile CreateProfile()
        {
            return new UnitClassProfile(
                displayName,
                equippableWeaponCategories,
                movement,
                movementType);
        }

        private void OnValidate()
        {
            if (equippableWeaponCategories == WeaponCategory.None)
            {
                Debug.LogWarning(
                    $"{name}: a unit class must allow at least one weapon category.",
                    this);
            }
        }
    }
}
