using UnityEngine;

namespace FE.Presentation
{
    public class EnemyAiDebugView : MonoBehaviour
    {
        private bool hasPlan;
        private bool hasTarget;
        private Vector3 enemyPosition;
        private Vector3 destinationPosition;
        private Vector3 targetPosition;

        public void SetPlan(
            Vector3 enemyPosition,
            Vector3 destinationPosition,
            Vector3 targetPosition)
        {
            this.enemyPosition = enemyPosition;
            this.destinationPosition = destinationPosition;
            this.targetPosition = targetPosition;
            hasPlan = true;
            hasTarget = true;

            Debug.DrawLine(
                enemyPosition,
                destinationPosition,
                Color.yellow,
                1f);

            Debug.DrawLine(
                destinationPosition,
                targetPosition,
                Color.red,
                1f);
        }

        public void SetPlan(
            Vector3 enemyPosition,
            Vector3 destinationPosition)
        {
            this.enemyPosition = enemyPosition;
            this.destinationPosition = destinationPosition;
            targetPosition = destinationPosition;
            hasPlan = true;
            hasTarget = false;

            Debug.DrawLine(
                enemyPosition,
                destinationPosition,
                Color.yellow,
                1f);
        }

        public void ClearPlan()
        {
            hasPlan = false;
            hasTarget = false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!hasPlan)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(enemyPosition, destinationPosition);
            Gizmos.DrawWireSphere(destinationPosition, 0.15f);

            if (hasTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(destinationPosition, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.15f);
            }
        }
    }
}
