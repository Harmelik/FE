using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridTileView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        [SerializeField] private Color normalColor;
        [SerializeField] private Color hoveredColor;
        [SerializeField] private Color selectedColor;

        private bool isHovered;
        private bool isSelected;

        private void Awake()
        {
            RefreshTileColor();
        }

        public void SetHovered(bool isHover)
        {
            isHovered = isHover;

            RefreshTileColor();
        }

        public void SetSelected(bool isSelect)
        {
            isSelected = isSelect;

            RefreshTileColor();
        }

        private void RefreshTileColor()
        {
            if (isSelected)
            {
                meshRenderer.material.color = selectedColor;
            } 
            else if (isHovered)
            {
                meshRenderer.material.color = hoveredColor;
            }
            else
            {
                meshRenderer.material.color = normalColor;
            }
        }
    }
}

