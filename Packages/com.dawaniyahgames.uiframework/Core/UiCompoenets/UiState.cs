using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace UiFramework.Core
{
    public class UiState : IDisposable
    {
        public string StateName { get; }

        private readonly List<AssetReference> uiElementScenes;
        private readonly Dictionary<string, SceneInstance> loadedScenes = new Dictionary<string, SceneInstance>();
        private readonly List<IUiElement> activeUiElements = new List<IUiElement>();
        private readonly List<AsyncOperationHandle<SceneInstance>> pendingLoads = new List<AsyncOperationHandle<SceneInstance>>();

        public UiState(string stateName, List<AssetReference> uiElementScenes)
        {
            StateName = stateName;
            this.uiElementScenes = uiElementScenes ?? new List<AssetReference>();
        }

        public void Init(object context = null)
        {
            for (int i = 0; i < uiElementScenes.Count; i++)
            {
                AssetReference sceneRef = uiElementScenes[i];

                AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneRef, UnityEngine.SceneManagement.LoadSceneMode.Additive, true);
                pendingLoads.Add(handle);

                object capturedContext = context;
                handle.Completed += op => OnSceneLoadCompleted(op, capturedContext);
            }
        }

        public async Task WaitForInitializationAsync()
        {
            if (pendingLoads == null || pendingLoads.Count == 0)
            {
                Debug.Log("[UiState] No pending loads to wait for.");
                return;
            }

            List<Task> waits = new List<Task>(pendingLoads.Count);
            for (int i = 0; i < pendingLoads.Count; i++)
            {
                waits.Add(pendingLoads[i].Task);
            }

            if (waits.Count > 0)
            {
                await Task.WhenAll(waits);
            }
        }

        private void OnSceneLoadCompleted(AsyncOperationHandle<SceneInstance> op, object context)
        {
            if (op.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("Failed to load scene (Addressables): " + op.DebugName);
                pendingLoads.Remove(op);
                return;
            }

            SceneInstance sceneInstance = op.Result;
            string sceneName = sceneInstance.Scene.name;

            if (!loadedScenes.ContainsKey(sceneName))
            {
                loadedScenes[sceneName] = sceneInstance;
            }

            GameObject[] roots = sceneInstance.Scene.GetRootGameObjects();
            for (int r = 0; r < roots.Length; r++)
            {
                IUiElement[] uiElements = roots[r].GetComponentsInChildren<IUiElement>(true);
                for (int u = 0; u < uiElements.Length; u++)
                {
                    uiElements[u].Populate(context);
                    activeUiElements.Add(uiElements[u]);
                }
            }

            pendingLoads.Remove(op);
        }

        public async Task UnloadUiState(List<string> keepScenes)
        {
            if (keepScenes == null)
            {
                keepScenes = new List<string>();
            }

            if (pendingLoads.Count > 0)
            {
                List<Task> waits = new List<Task>(pendingLoads.Count);
                for (int i = 0; i < pendingLoads.Count; i++)
                {
                    waits.Add(pendingLoads[i].Task);
                }
                await Task.WhenAll(waits);
            }

            List<string> toUnload = new List<string>();
            foreach (KeyValuePair<string, SceneInstance> kvp in loadedScenes)
            {
                if (!keepScenes.Contains(kvp.Key))
                {
                    toUnload.Add(kvp.Key);
                }
            }

            for (int i = 0; i < toUnload.Count; i++)
            {
                string sceneName = toUnload[i];
                SceneInstance instance;
                if (loadedScenes.TryGetValue(sceneName, out instance))
                {
                    await Addressables.UnloadSceneAsync(instance).Task;
                    loadedScenes.Remove(sceneName);
                }
            }

            activeUiElements.RemoveAll(uiElement =>
            {
                MonoBehaviour mono = uiElement as MonoBehaviour;
                if (mono == null || mono.gameObject == null)
                {
                    return true;
                }
                return !keepScenes.Contains(mono.gameObject.scene.name);
            });

            if (keepScenes.Count == 0 && activeUiElements.Count > 0)
            {
                for (int i = 0; i < activeUiElements.Count; i++)
                {
                    MonoBehaviour mono = activeUiElements[i] as MonoBehaviour;
                    if (mono != null && mono.gameObject != null)
                    {
                        UnityEngine.Object.Destroy(mono.gameObject);
                    }
                }
                activeUiElements.Clear();
            }
        }

        public IReadOnlyList<IUiElement> GetActiveUiElements()
        {
            return activeUiElements.AsReadOnly();
        }

        public List<string> GetUiElementSceneNames()
        {
            return loadedScenes.Keys.ToList();
        }

        public void Dispose()
        {
            loadedScenes.Clear();
            activeUiElements.Clear();
            pendingLoads.Clear();
        }
    }
}
