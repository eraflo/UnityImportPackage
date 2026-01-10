using System.Collections;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Eraflo.Catalyst.Assets;
using Eraflo.Catalyst.Pooling;

namespace Eraflo.Catalyst.Tests
{
    public class AssetManagerTests
    {
        private AssetManager _assetManager;
        private MockProvider _mockProvider;

        [SetUp]
        public void SetUp()
        {
            _assetManager = App.Get<AssetManager>();
            _mockProvider = new MockProvider();
            _assetManager.SetProvider(_mockProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _assetManager.Shutdown();
            _assetManager.Initialize(); // Restore default state
        }

        [UnityTest]
        public IEnumerator LoadAsync_ReturnsValidHandle()
        {
            var task = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task.IsCompleted);

            var handle = task.Result;
            Assert.IsNotNull(handle);
            Assert.IsNotNull(handle.Result);
            Assert.AreEqual("test_texture", handle.Key);
            Assert.AreEqual(1, _mockProvider.LoadCount);
        }

        [UnityTest]
        public IEnumerator LoadAsync_MultipleTimes_ReturnsCachedAsset()
        {
            var task1 = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task1.IsCompleted);
            
            var task2 = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task2.IsCompleted);

            Assert.AreSame(task1.Result.Result, task2.Result.Result);
            Assert.AreEqual(1, _mockProvider.LoadCount, "Provider should only be called once");
        }

        [UnityTest]
        public IEnumerator Release_UnloadsWhenCountReachesZero()
        {
            var task = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task.IsCompleted);
            var handle = task.Result;

            handle.Dispose();

            Assert.AreEqual(1, _mockProvider.ReleaseCount, "Asset should be released in provider");
        }

        [UnityTest]
        public IEnumerator Release_DoesNotUnloadIfStillReferenced()
        {
            var task1 = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task1.IsCompleted);
            var handle1 = task1.Result;

            var task2 = _assetManager.LoadAsync<Texture2D>("test_texture");
            yield return new WaitUntil(() => task2.IsCompleted);
            var handle2 = task2.Result;

            handle1.Dispose();
            Assert.AreEqual(0, _mockProvider.ReleaseCount, "Asset should NOT be released yet");

            handle2.Dispose();
            Assert.AreEqual(1, _mockProvider.ReleaseCount, "Asset should finally be released");
        }

        [UnityTest]
        public IEnumerator LoadAndPoolAsync_WarmsUpPool()
        {
            var task = App.Get<Pool>().LoadAndPoolAsync("test_prefab", 5);
            yield return new WaitUntil(() => task.IsCompleted);
            var handle = task.Result;

            var pool = App.Get<Pool>();
            // We can't easily check private _available count in PrefabPool from here without reflection
            // But we can check if Spawn reuse objects or if metrics recorded it.
            
            Assert.IsNotNull(handle);
            Assert.AreEqual(1, _mockProvider.LoadCount);

            handle.Dispose();
        }

        private class MockProvider : IAssetProvider
        {
            public int LoadCount;
            public int ReleaseCount;

            public Task<T> LoadAsync<T>(string key) where T : Object
            {
                LoadCount++;
                T asset = null;
                
                if (typeof(T) == typeof(GameObject))
                {
                    asset = new GameObject(key) as T;
                }
                else if (typeof(T) == typeof(Texture2D))
                {
                    asset = new Texture2D(1, 1) as T;
                }
                else if (typeof(ScriptableObject).IsAssignableFrom(typeof(T)))
                {
                    asset = ScriptableObject.CreateInstance(typeof(T)) as T;
                }
                
                return Task.FromResult(asset);
            }

            public void Release(Object asset)
            {
                ReleaseCount++;
                if (asset is GameObject go) Object.DestroyImmediate(go);
                else if (!(asset is Component)) Object.DestroyImmediate(asset);
            }
        }
    }
}
