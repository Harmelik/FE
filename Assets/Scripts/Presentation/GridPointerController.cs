using System;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridPointerController : MonoBehaviour
    {
        [SerializeField] private LayerMask tileLayerMask;
        [SerializeField] private Camera battleCamera;
        private bool isInit;
        private GridGeometry geometry;
        private GridMapView gridMapView;
        private BattleMap bmMap;
        public event Action<GridCoord> TileSelected;

        private void Update()
        {
            if (!isInit) return;

            Ray ray = battleCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hitInfo, 100f, tileLayerMask))
            {
                var curGrid = geometry.WorldPosToGrid(hitInfo.point);
                if (!bmMap.IsInside(curGrid))
                {
                    gridMapView.ClearHovered();
                    return;
                }
                gridMapView.SetHovered(curGrid);
                if(Input.GetMouseButtonDown(0))
                {
                    gridMapView.SetSelected(curGrid);
                    TileSelected?.Invoke(curGrid);
                }
            }
            else
            {
                gridMapView.ClearHovered();
            }
        }

        public void Init(GridMapView map, GridGeometry geometry, BattleMap battleMap)
        {
            if (battleCamera == null)
            {
                Debug.LogError("GridPointerController: Battle Camera 未赋值。", this);
                enabled = false;
                return;
            }

            if (map == null || geometry == null || battleMap == null)
            {
                Debug.LogError("GridPointerController: 运行时依赖未注入。", this);
                enabled = false;
                return;
            }

            if (tileLayerMask.value == 0)
            {
                Debug.LogError("GridPointerController: Tile Layer Mask 没有选择任何 Layer。", this);
                enabled = false;
                return;
            }

            gridMapView = map;
            this.geometry = geometry;
            bmMap = battleMap;
            isInit = true;
        }
    }
}

