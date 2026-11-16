using System.Collections;
using System.Collections.Generic;
using Codice.CM.Common;
using FE.Battle;
using FE.Core;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEngine;
using UnityEngine.Serialization;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridMapView : MonoBehaviour
    {
        [SerializeField] private GameObject tilePrefab;

        private readonly List<GameObject> tileObjects = new List<GameObject>();

        public void CreateTiles(BattleMap map, GridGeometry gg)
        {
            ClearTiles();

            for (var i = 0; i < map.Width; i++)
            {
                for (var j = 0; j < map.Height; j++)
                {
                    GridCoord grid = new GridCoord(i, j);
                    var go = Instantiate(tilePrefab, transform);
                    go.name = $"{i}x{j}";
                    go.transform.position = gg.GetCellCenter(grid);
                    tileObjects.Add(go);
                }
            }
        }

        private void ClearTiles()
        {
            foreach(var tile in tileObjects)
            {
                if (tile != null)
                {
                    Destroy(tile);
                }
            }

            tileObjects.Clear();
        }
    }
}

