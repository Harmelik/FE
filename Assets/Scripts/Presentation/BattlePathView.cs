using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
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
        [SerializeField] private LineRenderer lineRenderer;
        public void Init(GridGeometry gg)
        {
            this.gg = gg;
        }

        public void ShowPath(IReadOnlyList<GridCoord> path)
        {
            if (path == null || path.Count < 2)
            {
                this.HidePath();
                return;
            }
            lineRenderer.positionCount = path.Count;
            var pos = new Vector3[path.Count];
            for(var i = 0; i < path.Count; i++)
            {
                var p = gg.GetCellCenter(path[i]);
                pos[i] = p;
            }
            lineRenderer.SetPositions(pos);
        }

        public void HidePath()
        {
            lineRenderer.positionCount = 0;
        }
    }
}

