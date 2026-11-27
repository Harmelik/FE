using UnityEngine;

namespace FE.Presentation
{
    /// <summary>
    /// Orients one shared world-space HUD root toward the battle camera.
    /// Child UI elements keep normal local RectTransform layouts.
    /// </summary>
    [DisallowMultipleComponent]
    public class WorldBillboard : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;

        public void Init(Camera camera)
        {
            targetCamera = camera;
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                transform.rotation = targetCamera.transform.rotation;
            }
        }
    }
}
