using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Eraflo.Catalyst.Tests
{
    public class MockSceneManager : ISceneManager
    {
        public void Initialize() { }
        public void Shutdown() { }

        private readonly List<Scene> _mockScenes = new List<Scene>();
        public List<string> LoadedScenes => _mockScenes.Select(s => s.name).ToList();
        public string ActiveSceneName;
        public int UnloadCount;

        public async Task LoadSceneAsync(string name, LoadSceneMode mode, Action<float> onProgress = null)
        {
            onProgress?.Invoke(0.5f);
            await Task.Yield();
            
            // Create a real in-memory scene to get a valid Scene struct
            var scene = SceneManager.CreateScene(name);
            _mockScenes.Add(scene);
            
            onProgress?.Invoke(1.0f);
        }

        public async Task UnloadSceneAsync(Scene scene)
        {
            UnloadCount++;
            if (_mockScenes.Contains(scene))
            {
                _mockScenes.Remove(scene);
                var op = SceneManager.UnloadSceneAsync(scene);
                if (op != null)
                {
                    while (!op.isDone) await Task.Yield();
                }
            }
        }

        public void SetActiveScene(Scene scene)
        {
            ActiveSceneName = scene.name;
            // Note: In a mock, we don't necessarily want to call SceneManager.SetActiveScene 
            // because it might fail if the scene isn't "really" loaded in Unity's eyes.
        }

        public Scene GetSceneAt(int index)
        {
            if (index >= 0 && index < _mockScenes.Count)
            {
                return _mockScenes[index];
            }
            return default; 
        }

        public Scene GetSceneByName(string name)
        {
            return _mockScenes.FirstOrDefault(s => s.name == name);
        }

        public int SceneCount => _mockScenes.Count;

        /// <summary>
        /// Helper to clear all mock scenes (call in TearDown).
        /// </summary>
        public async Task CleanupAsync()
        {
            foreach (var scene in _mockScenes.ToList())
            {
                if (scene.isLoaded)
                {
                    var op = SceneManager.UnloadSceneAsync(scene);
                    if (op != null)
                    {
                        while (!op.isDone) await Task.Yield();
                    }
                }
            }
            _mockScenes.Clear();
        }
    }
}
