using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eraflo.Catalyst.Events;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Eraflo.Catalyst.Tests
{
    public class SceneLoaderServiceTests
    {
        private SceneLoaderService _service;
        private MockLoadingScreen _mockLoadingScreen;
        private MockSceneManager _mockSceneManager;
        private EventBus _eventBus;

        [SetUp]
        public void SetUp()
        {
            _service = new SceneLoaderService();
            _mockLoadingScreen = new MockLoadingScreen();
            _mockSceneManager = new MockSceneManager();
            _eventBus = new EventBus();

            // Direct injection instead of ServiceLocator
            _service.SetSceneManager(_mockSceneManager);
            _service.SetLoadingScreen(_mockLoadingScreen);
            
            ((IGameService)_service).Initialize();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var task = _mockSceneManager.CleanupAsync();
            yield return new WaitUntil(() => task.IsCompleted);
            
            ((IGameService)_service).Shutdown();
        }

        [UnityTest]
        public IEnumerator LoadGroupAsync_ExecutesFullFlow()
        {
            // 1. Setup Group
            var group = new SceneGroup 
            { 
                Name = "TestGroup",
                Scenes = new List<string> { "SceneA", "SceneB" },
                ActiveScene = "SceneA"
            };
            _service.RegisterGroup(group);

            // 2. Start Loading
            Task loadTask = _service.LoadGroupAsync("TestGroup", showLoadingScreen: true, waitForInput: false);
            
            yield return new WaitUntil(() => loadTask.IsCompleted || loadTask.IsFaulted);

            if (loadTask.IsFaulted)
            {
                Assert.Fail(loadTask.Exception?.ToString());
            }

            // 3. Verify Flow
            Assert.AreEqual(2, _mockSceneManager.LoadedScenes.Count, "Should have loaded 2 scenes.");
            Assert.Contains("SceneA", _mockSceneManager.LoadedScenes);
            Assert.Contains("SceneB", _mockSceneManager.LoadedScenes);
            
            Assert.AreEqual(1, _mockLoadingScreen.ShowCount, "Loading screen should have been shown once.");
            Assert.AreEqual(1, _mockLoadingScreen.HideCount, "Loading screen should have been hidden once.");
            Assert.AreEqual(1.0f, _mockLoadingScreen.Progress, "Progress should be 1.0 at completion.");
        }

        [UnityTest]
        public IEnumerator LoadGroupAsync_UnloadsCurrentScenes()
        {
            // Setup current scenes in mock
            _mockSceneManager.LoadedScenes.Add("OldScene");

            var group = new SceneGroup 
            { 
                Name = "NewGroup",
                Scenes = new List<string> { "NewScene" }
            };
            _service.RegisterGroup(group);

            Task loadTask = _service.LoadGroupAsync("NewGroup", showLoadingScreen: false);
            yield return new WaitUntil(() => loadTask.IsCompleted);

            // Verify
            Assert.AreEqual(1, _mockSceneManager.LoadedScenes.Count);
            Assert.AreEqual("NewScene", _mockSceneManager.LoadedScenes[0]);
        }
    }
}
