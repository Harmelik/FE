using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattleUnitView : MonoBehaviour
    {
        private BattleUnit battleUnit;
        private GridGeometry gg;
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private float offsetY;
        private Tweener previewTween;

        public void Init(BattleUnit unit, GridGeometry gg)
        {
            if(unit == null || gg == null) return;
            battleUnit = unit;
            this.gg = gg;

            switch (battleUnit.Fac)
            {
                case Faction.Player:
                {
                        mesh.sharedMaterial = playerMaterial;
                } break;
                case Faction.Enemy:
                {
                        mesh.sharedMaterial = enemyMaterial;
                } break;
            }

            transform.position = gg.GetCellCenter(battleUnit.CurGrid) + Vector3.up * offsetY;
        }

        public void MoveToPreviewPosition(GridCoord grid)
        {
            previewTween?.Kill();
            previewTween = DOTween.To(
                () => transform.position,        // 当前值
                value => transform.position = value, // 每帧写入新值
                gg.GetCellCenter(grid) + Vector3.up * offsetY,                   // 目标值
                0.5f                 // 持续时间
            );
            //transform.position = gg.GetCellCenter(grid) + Vector3.up * offsetY;
        }

        public void RefreshToDefaultPosition()
        {
            previewTween?.Kill();
            previewTween = null;
            transform.position = gg.GetCellCenter(battleUnit.CurGrid) + Vector3.up * offsetY;
        }
    }
}

