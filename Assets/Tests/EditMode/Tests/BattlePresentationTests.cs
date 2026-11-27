using FE.Core;
using FE.Presentation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace FE.Tests
{
    public class BattlePresentationTests
    {
        [Test]
        public void EnemyRangeTrackerSnapshotDoesNotRemoveTrackedUnit()
        {
            var tracker = new EnemyRangeSelectionTracker();
            var enemyId = new UnitId(7);

            tracker.Toggle(enemyId);
            tracker.CreateSnapshot();

            Assert.That(tracker.IsTracked(enemyId), Is.True);
        }

        [Test]
        public void EnemyRangeTrackerTogglingSameUnitRemovesTracking()
        {
            var tracker = new EnemyRangeSelectionTracker();
            var enemyId = new UnitId(7);

            tracker.Toggle(enemyId);
            tracker.Toggle(enemyId);

            Assert.That(tracker.IsTracked(enemyId), Is.False);
        }

        [Test]
        public void PlayerHealthBarIsVisiblyBlue()
        {
            Color color = BattleUnitHealthBarView.GetFactionColor(
                Faction.Player);

            Assert.That(color.b, Is.GreaterThan(color.r));
        }

        [Test]
        public void EnemyHealthBarIsVisiblyRed()
        {
            Color color = BattleUnitHealthBarView.GetFactionColor(
                Faction.Enemy);

            Assert.That(color.r, Is.GreaterThan(color.b));
        }

        [Test]
        public void TurnAndPhaseTextUseStableDisplayFormat()
        {
            Assert.That(
                BattleHudTextFormatter.FormatTurn(3),
                Is.EqualTo("TURN 3"));
            Assert.That(
                BattleHudTextFormatter.FormatPhase(
                    BattlePhase.PlayerPhase),
                Is.EqualTo("PLAYER PHASE"));
            Assert.That(
                BattleHudTextFormatter.FormatPhase(
                    BattlePhase.EnemyPhase),
                Is.EqualTo("ENEMY PHASE"));
        }

        [Test]
        public void BattleUnitPrefabSeparatesModelFromPrefabAuthoredWorldHud()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/Battle/BattleUnit.prefab");

            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));

            Transform modelRoot = prefab.transform.Find("ModelRoot");
            Transform worldHudRoot = prefab.transform.Find("WorldHudRoot");
            Assert.That(modelRoot, Is.Not.Null);
            Assert.That(worldHudRoot, Is.Not.Null);
            Assert.That(modelRoot.localScale, Is.Not.EqualTo(Vector3.one));

            Transform worldCanvas = worldHudRoot.Find("WorldCanvas");
            Assert.That(worldCanvas, Is.Not.Null);
            Canvas canvas = worldCanvas.GetComponent<Canvas>();
            Assert.That(canvas, Is.Not.Null);
            Assert.That(
                canvas.renderMode,
                Is.EqualTo(RenderMode.WorldSpace));
            Assert.That(
                worldCanvas.localScale.x,
                Is.GreaterThan(0f));
            Assert.That(
                ((RectTransform)worldCanvas).sizeDelta.x,
                Is.GreaterThan(0f));
            Assert.That(
                Mathf.DeltaAngle(
                    worldCanvas.localEulerAngles.z,
                    0f),
                Is.EqualTo(0f).Within(0.01f));

            Transform healthBar = worldCanvas.Find("HealthBar");
            Assert.That(healthBar, Is.Not.Null);
            Assert.That(
                healthBar.GetComponent<BattleUnitHealthBarView>(),
                Is.Not.Null);
            Assert.That(
                healthBar.GetComponent<CanvasRenderer>(),
                Is.Not.Null);
            Assert.That(
                worldHudRoot.GetComponent<BattleUnitWorldHudView>(),
                Is.Not.Null);
            Assert.That(
                worldHudRoot.GetComponent<WorldBillboard>(),
                Is.Not.Null);
        }
    }
}
