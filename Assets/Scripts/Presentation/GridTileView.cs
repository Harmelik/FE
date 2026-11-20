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
        [SerializeField] private Color movementColor;
        [SerializeField] private Color attackColor;


        private bool isHovered;
        private bool isSelected;
        private bool isInMovement;
        private bool isInAttack;

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

        public void SetMovement(bool isMovement)
        {
            isInMovement = isMovement;

            RefreshTileColor();
        }

        public void SetAttack(bool isAttack)
        {
            isInAttack = isAttack;

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
            else if (isInMovement)
            {
                meshRenderer.material.color = movementColor;
            } 
            else if (isInAttack)
            {
                meshRenderer.material.color = attackColor;
            }
            else
            {
                meshRenderer.material.color = normalColor;
            }
        }
    }
}

