using System;
using System.Collections;
using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleBootstrap : MonoBehaviour
    {
        [SerializeField] BattleMapConfig bmConfig;
        [SerializeField] private GridMapView gridMap;
        [SerializeField, Min(0.1f)] private float cellSize = 1f;
        public void Start()
        {
            BattleMap map = bmConfig.CreateMap();
            GridGeometry gg = new GridGeometry(gridMap.transform.position, cellSize);
            gridMap.CreateTiles(map, gg);
        }
    }
}

