using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// 
    /// </summary>
    public class GridTileView : MonoBehaviour
    {
        [SerializeField] private MeshRenderer highlightRenderer;
        [SerializeField] private Color hoveredColor;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color movementColor;
        [SerializeField] private Color attackColor;
        [SerializeField] private Color hoveredMovementColor =
            new Color(0.45f, 0.9f, 1f, 1f);
        [SerializeField] private Color hoveredAttackColor =
            new Color(1f, 0.45f, 0.45f, 1f);
        [SerializeField, Min(0f)] private float surfaceHeight;

        private bool isHovered;
        private bool isSelected;
        private bool isInMovement;
        private bool isInAttack;
        private bool isInHoverMovement;
        private bool isInHoverAttack;
        private MaterialPropertyBlock propertyBlock;

        public float SurfaceHeight => surfaceHeight;

        private void Awake()
        {
            propertyBlock = new MaterialPropertyBlock();
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

        public void SetHoverMovement(bool isHoverMovement)
        {
            isInHoverMovement = isHoverMovement;

            RefreshTileColor();
        }

        public void SetHoverAttack(bool isHoverAttack)
        {
            isInHoverAttack = isHoverAttack;

            RefreshTileColor();
        }

        private void RefreshTileColor()
        {
            if (highlightRenderer == null)
            {
                return;
            }

            bool isVisible =
                isSelected ||
                isHovered ||
                isInMovement ||
                isInAttack ||
                isInHoverMovement ||
                isInHoverAttack;

            highlightRenderer.enabled = isVisible;
            if (!isVisible)
            {
                return;
            }

            Color color;

            if (isSelected)
            {
                color = selectedColor;
            } 
            else if (isHovered)
            {
                color = hoveredColor;
            }
            else if (isInMovement)
            {
                color = movementColor;
            } 
            else if (isInAttack)
            {
                color = attackColor;
            }
            else if (isInHoverMovement)
            {
                color = hoveredMovementColor;
            }
            else if (isInHoverAttack)
            {
                color = hoveredAttackColor;
            }
            else
            {
                color = selectedColor;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            highlightRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", color);
            highlightRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

