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
        private GridMapView gridMapView;
        [SerializeField] private MeshRenderer mesh;
        [SerializeField] private Transform modelRoot;
        [SerializeField] private BattleUnitWorldHudView worldHudView;
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material enemyMaterial;
        [SerializeField] private float offsetY;
        private Sequence previewTween;
        private Tween combatTween;
        private Vector3 defaultScale;
        private MaterialPropertyBlock propertyBlock;
        private Color baseMaterialColor;

        public void Init(
            BattleUnit unit,
            GridGeometry gg,
            GridMapView gridMapView)
        {
            if(unit == null || gg == null || gridMapView == null) return;
            battleUnit = unit;
            this.gg = gg;
            this.gridMapView = gridMapView;
            modelRoot ??= transform;
            defaultScale = modelRoot.localScale;
            worldHudView?.Bind(unit, Camera.main);

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

            baseMaterialColor = mesh.sharedMaterial.color;
            transform.position = GetUnitWorldPosition(battleUnit.CurGrid);
            RefreshHealth();
        }

        public void MoveToPreviewPosition(GridCoord grid)
        {
            previewTween?.Kill();

            previewTween = DOTween.Sequence();

            previewTween.Append(
                DOTween.To(
                    () => transform.position,
                    value => transform.position = value,
                    GetUnitWorldPosition(grid),
                    0.2f));

            previewTween.Play();
        }

        public void MoveToPreviewPositionByPath(
            IReadOnlyList<GridCoord> path)
        {
            MoveToPreviewPositionByPath(path, null);
        }

        public void MoveToPreviewPositionByPath(
            IReadOnlyList<GridCoord> path, Action onComplete)
        {
            previewTween?.Kill();
            previewTween = null;

            if (path == null || path.Count == 0)
            {
                onComplete?.Invoke();
                return;
            }

            if (path.Count == 1)
            {
                transform.position =
                    GetUnitWorldPosition(path[0]);
                onComplete?.Invoke();
                return;
            }

            Sequence sequence = DOTween.Sequence();
            previewTween = sequence;

            foreach (GridCoord grid in path)
            {
                sequence.Append(
                    DOTween.To(
                        () => transform.position,
                        value => transform.position = value,
                        GetUnitWorldPosition(grid),
                        0.06f));
            }

            sequence.OnComplete(() =>
            {
                if (ReferenceEquals(previewTween, sequence))
                {
                    previewTween = null;
                }

                onComplete?.Invoke();
            }).Play();
        }

        public IEnumerator MoveToPositionByPathRoutine(
            IReadOnlyList<GridCoord> path)
        {
            previewTween?.Kill();
            previewTween = null;

            if (path == null || path.Count == 0)
            {
                yield break;
            }

            if (path.Count == 1)
            {
                transform.position =
                    GetUnitWorldPosition(path[0]);
                yield break;
            }

            Sequence sequence = DOTween.Sequence();
            previewTween = sequence;

            for (int i = 1; i < path.Count; i++)
            {
                GridCoord destination = path[i];
                sequence.Append(
                    DOTween.To(
                        () => transform.position,
                        value => transform.position = value,
                        GetUnitWorldPosition(destination),
                        0.06f));
            }

            yield return sequence
                .Play()
                .WaitForCompletion();

            if (ReferenceEquals(previewTween, sequence))
            {
                previewTween = null;
            }
        }

        public void RefreshToDefaultPosition()
        {
            previewTween?.Kill();
            previewTween = null;

            combatTween?.Kill();
            combatTween = null;

            transform.position = GetUnitWorldPosition(battleUnit.CurGrid);
            modelRoot.localScale = defaultScale;
        }


        public Tween PlayActionLunge(GridCoord targetGrid)
        {
            CancelCombatAnimation();

            Vector3 targetWorldPosition =
                GetUnitWorldPosition(targetGrid);
            Vector3 direction =
                targetWorldPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return null;
            }

            combatTween = transform
                .DOMove(
                    transform.position + direction.normalized * 0.25f,
                    0.12f)
                .SetEase(Ease.OutQuad);
            return combatTween;
        }

        public Tween PlayHitReaction()
        {
            CancelCombatAnimation();
            combatTween = transform
                .DOShakePosition(
                    0.16f,
                    0.08f,
                    6,
                    45f,
                    false,
                    true)
                .SetEase(Ease.OutQuad);
            return combatTween;
        }

        public Tween PlayReturnToGrid(GridCoord originGrid)
        {
            CancelCombatAnimation();

            combatTween = transform
                .DOMove(
                    GetUnitWorldPosition(originGrid),
                    0.16f)
                .SetEase(Ease.OutQuad);
            return combatTween;
        }

        public Tween PlayDefeat()
        {
            CancelCombatAnimation();
            combatTween = modelRoot
                .DOScale(0f, 0.25f)
                .SetEase(Ease.InQuad);
            return combatTween;
        }

        public void CancelCombatAnimation()
        {
            combatTween?.Kill();
            combatTween = null;
        }

        private Vector3 GetUnitWorldPosition(GridCoord grid)
        {
            return gridMapView.GetWorldPosition(
                grid,
                offsetY);
        }

        public void SetGrayed(bool isGrayed)
        {
            if (mesh == null || mesh.sharedMaterial == null)
            {
                return;
            }

            propertyBlock ??= new MaterialPropertyBlock();
            mesh.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(
                "_Color",
                isGrayed
                    ? Color.Lerp(baseMaterialColor, Color.gray, 0.7f)
                    : baseMaterialColor);
            mesh.SetPropertyBlock(propertyBlock);
        }

        public void RefreshHealth()
        {
            worldHudView?.RefreshHealth();
        }

        private void OnDestroy()
        {
            previewTween?.Kill();
            previewTween = null;

            combatTween?.Kill();
            combatTween = null;
        }
    }
}

