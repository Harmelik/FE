using System.Collections.Generic;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 单个战场的部署设置，保存战场上开局时的全部敌我方配置和角色数据信息
    /// </summary>
    [CreateAssetMenu(
    fileName = "BattleEncounterConfig",
    menuName = "FE/Battle/Battle Encounter Config")]
    public class BattleEncounterConfig : ScriptableObject
    {
        [SerializeField] private BattleMapConfig mapConfig;

        [SerializeField]
        private List<UnitDeploymentData> playerDeployments =
            new List<UnitDeploymentData>();

        [SerializeField]
        private List<UnitDeploymentData> enemyDeployments =
            new List<UnitDeploymentData>();

        public BattleMapConfig MapConfig => mapConfig;

        public IReadOnlyList<UnitDeploymentData> PlayerDeployments =>
            playerDeployments;

        public IReadOnlyList<UnitDeploymentData> EnemyDeployments =>
            enemyDeployments;
    }
}

