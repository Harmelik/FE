using System;
using FE.Core;
using UnityEngine;

namespace FE.Battle
{
    /// <summary>
    /// 单个战棋开场时的部署信息
    /// </summary>
    [Serializable]
    public class UnitDeploymentData
    {
        [SerializeField] private UnitDataDefinition _unitDefinition;
        [SerializeField] private Vector2Int _gridPosition;

        public UnitDataDefinition UnitDefinition => _unitDefinition;
        public Vector2Int GridPosition => _gridPosition;

        public GridCoord ToGridCoord()
        {
            return new GridCoord(
                _gridPosition.x,
                _gridPosition.y);
        }
    }
}

