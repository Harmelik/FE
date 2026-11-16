using FE.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 
    /// </summary>
    [CreateAssetMenu(
    fileName = "BattleMapConfig",
    menuName = "FE/Battle/Battle Map Config")]
    public class BattleMapConfig : ScriptableObject
    {
        [SerializeField, Min(1)]
        private int width = 10;

        [SerializeField, Min(1)]
        private int height = 8;

        public int Width => width;
        public int Height => height;

        public BattleMap CreateMap()
        {
            return new BattleMap(width, height);
        }
    }
}

