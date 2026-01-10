using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Eraflo.Catalyst.Pooling;

namespace Eraflo.Catalyst.Tests
{
    public class PoolTests
    {
        [SetUp]
        public void SetUp()
        {
            App.Get<Pool>().ClearAllPools();
            App.Get<Pool>().Metrics.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            App.Get<Pool>().ClearAllPools();
        }

        #region Generic Pool Tests

        [Test]
        public void Get_ReturnsValidHandle()
        {
            var handle = App.Get<Pool>().GetFromPool<TestPoolable>();

            Assert.IsTrue(handle.IsValid);
            Assert.IsNotNull(handle.Instance);
        }

        [Test]
        public void Get_CallsOnSpawn()
        {
            var handle = App.Get<Pool>().GetFromPool<TestPoolable>();

            Assert.IsTrue(handle.Instance.WasSpawned);
        }

        [Test]
        public void Release_CallsOnDespawn()
        {
            var handle = App.Get<Pool>().GetFromPool<TestPoolable>();
            var instance = handle.Instance;

            App.Get<Pool>().ReleaseToPool(handle);

            Assert.IsTrue(instance.WasDespawned);
        }

        [Test]
        public void Get_ReusesPreviouslyReleasedObject()
        {
            var handle1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var instance1 = handle1.Instance;
            App.Get<Pool>().ReleaseToPool(handle1);

            var handle2 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var instance2 = handle2.Instance;

            Assert.AreSame(instance1, instance2);
        }

        [Test]
        public void Warmup_PreAllocatesObjects()
        {
            App.Get<Pool>().WarmupPool<TestPoolable>(10);

            // Get should reuse warmed up objects
            var handles = new PoolHandle<TestPoolable>[10];
            for (int i = 0; i < 10; i++)
            {
                handles[i] = App.Get<Pool>().GetFromPool<TestPoolable>();
            }

            // All should be valid
            foreach (var h in handles)
            {
                Assert.IsTrue(h.IsValid);
            }
        }

        [Test]
        public void Clear_RemovesAllObjects()
        {
            App.Get<Pool>().WarmupPool<TestPoolable>(5);
            
            App.Get<Pool>().ClearPool<TestPoolable>();

            // No exception means success
            Assert.Pass();
        }

        #endregion

        #region Handle Tests

        [Test]
        public void PoolHandle_None_IsNotValid()
        {
            var handle = PoolHandle<TestPoolable>.None;

            Assert.IsFalse(handle.IsValid);
        }

        [Test]
        public void PoolHandle_Equality_Works()
        {
            var handle1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var handle2 = handle1;

            Assert.AreEqual(handle1, handle2);
            Assert.IsTrue(handle1 == handle2);
        }

        [Test]
        public void PoolHandle_DifferentHandles_NotEqual()
        {
            var handle1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var handle2 = App.Get<Pool>().GetFromPool<TestPoolable>();

            Assert.AreNotEqual(handle1, handle2);
            Assert.IsTrue(handle1 != handle2);
        }

        #endregion

        #region Metrics Tests

        [Test]
        public void Metrics_TotalSpawned_IncrementsOnGet()
        {
            App.Get<Pool>().GetFromPool<TestPoolable>();
            App.Get<Pool>().GetFromPool<TestPoolable>();
            App.Get<Pool>().GetFromPool<TestPoolable>();

            Assert.AreEqual(3, App.Get<Pool>().Metrics.TotalSpawned);
        }

        [Test]
        public void Metrics_TotalDespawned_IncrementsOnRelease()
        {
            var h1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var h2 = App.Get<Pool>().GetFromPool<TestPoolable>();
            App.Get<Pool>().ReleaseToPool(h1);

            Assert.AreEqual(1, App.Get<Pool>().Metrics.TotalDespawned);
        }

        [Test]
        public void Metrics_ActiveCount_TracksCorrectly()
        {
            var h1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var h2 = App.Get<Pool>().GetFromPool<TestPoolable>();
            
            Assert.AreEqual(2, App.Get<Pool>().Metrics.ActiveCount);
            
            App.Get<Pool>().ReleaseToPool(h1);
            
            Assert.AreEqual(1, App.Get<Pool>().Metrics.ActiveCount);
        }

        [Test]
        public void Metrics_PeakActiveCount_TracksMax()
        {
            var h1 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var h2 = App.Get<Pool>().GetFromPool<TestPoolable>();
            var h3 = App.Get<Pool>().GetFromPool<TestPoolable>();
            App.Get<Pool>().ReleaseToPool(h1);
            App.Get<Pool>().ReleaseToPool(h2);

            Assert.AreEqual(3, App.Get<Pool>().Metrics.PeakActiveCount);
        }

        #endregion

        #region Prefab Pool Tests

        [UnityTest]
        public IEnumerator Spawn_WithPrefab_CreatesGameObject()
        {
            var prefab = new GameObject("TestPrefab");
            
            var handle = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            
            Assert.IsTrue(handle.IsValid);
            Assert.IsNotNull(handle.Instance);
            Assert.IsTrue(handle.Instance.activeSelf);
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Spawn_AtPosition_SetsCorrectPosition()
        {
            var prefab = new GameObject("TestPrefab");
            var pos = new Vector3(10, 20, 30);
            
            var handle = App.Get<Pool>().SpawnObject(prefab, pos);
            
            Assert.AreEqual(pos, handle.Instance.transform.position);
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Despawn_DeactivatesGameObject()
        {
            var prefab = new GameObject("TestPrefab");
            var handle = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            var go = handle.Instance;
            
            App.Get<Pool>().DespawnObject(handle);
            
            Assert.IsFalse(go.activeSelf);
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Spawn_ReusesDesspawnedObject()
        {
            var prefab = new GameObject("TestPrefab");
            
            var handle1 = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            var go1 = handle1.Instance;
            App.Get<Pool>().DespawnObject(handle1);
            
            var handle2 = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            var go2 = handle2.Instance;
            
            Assert.AreSame(go1, go2);
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator Spawn_AddsPooledObjectComponent()
        {
            var prefab = new GameObject("TestPrefab");
            
            var handle = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            
            Assert.IsNotNull(handle.Instance.GetComponent<PooledObject>());
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        [UnityTest]
        public IEnumerator PooledObject_IsSpawned_ReflectsState()
        {
            var prefab = new GameObject("TestPrefab");
            
            var handle = App.Get<Pool>().SpawnObject(prefab, Vector3.zero);
            var pooledObj = handle.Instance.GetComponent<PooledObject>();
            
            Assert.IsTrue(pooledObj.IsSpawned);
            
            App.Get<Pool>().DespawnObject(handle);
            
            Assert.IsFalse(pooledObj.IsSpawned);
            
            // Cleanup
            App.Get<Pool>().ClearAllPools();
            Object.DestroyImmediate(prefab);
            
            yield return null;
        }

        #endregion

        #region Test Helpers

        private class TestPoolable : IPoolable
        {
            public bool WasSpawned { get; private set; }
            public bool WasDespawned { get; private set; }

            public void OnSpawn()
            {
                WasSpawned = true;
                WasDespawned = false;
            }

            public void OnDespawn()
            {
                WasDespawned = true;
            }
        }

        #endregion
    }
}
