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
        [SerializeField, Min(1)] private int maxHP = 25;
        [SerializeField, Min(0)] private int pAtk = 10;
        [SerializeField, Min(0)] private int phyDef = 3;
        [SerializeField, Min(0)] private int mAtk = 10;
        [SerializeField, Min(0)] private int magicDef = 3;
        [SerializeField, Min(0)] private int hit = 80;
        [SerializeField, Min(0)] private int avoid = 0;
        [SerializeField, Min(0)] private int movement = 3;
        [SerializeField, Min(0)] private int minAttackRange = 1;
        [SerializeField, Min(0)] private int maxAttackRange = 1;

        public string DisplayName => displayName;

        public UnitStats CreateStats()
        {
            return new UnitStats(
                maxHP:this.maxHP,
                patk:this.pAtk,
                phyDef:this.phyDef,
                matk:this.mAtk,
                magicDef:this.magicDef,
                hit: this.hit,
                avoid: this.avoid,
                movement: this.movement,
                minAttackRange: this.minAttackRange,
                maxAttackRange: this.maxAttackRange
            );
        }

        private void OnValidate()
        {
            if (maxAttackRange < minAttackRange)
            {
                Debug.LogWarning(
                    $"{name}: maxAttackRange cannot be smaller than minAttackRange.",
                    this);
            }
        }
    }
}

