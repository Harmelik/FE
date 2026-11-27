using System.Collections.Generic;
using FE.Battle;
using FE.Core;
using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class BattlePathView : MonoBehaviour
    {
        private GridGeometry gg;
        private GridMapView gridMapView;
        [SerializeField] private LineRenderer lineRenderer;
        public void Init(
            GridGeometry gg,
            GridMapView gridMapView)
        {
            this.gg = gg;
            this.gridMapView = gridMapView;
        }

        public void ShowPath(IReadOnlyList<GridCoord> path)
        {
            if (path == null || path.Count < 2)
            {
                this.HidePath();
                return;
            }
            enabled = true;
            lineRenderer.positionCount = path.Count;
            var pos = new Vector3[path.Count];
            for(var i = 0; i < path.Count; i++)
            {
                Vector3 p = gridMapView.GetWorldPosition(
                    path[i],
                    0.03f);
                pos[i] = p;
            }
            lineRenderer.SetPositions(pos);
        }

        public void HidePath()
        {
            if (lineRenderer != null)
            {
                lineRenderer.positionCount = 0;
            }
            enabled = false;
        }
    }
}

