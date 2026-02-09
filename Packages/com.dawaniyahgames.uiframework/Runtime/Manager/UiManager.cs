using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UiFramework.Core;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace UiFramework.Runtime.Manager
{
    public class UiManager : MonoBehaviour
    {
        private static UiManager instance;
        public static bool IsInitialized
        {
            get
            {
                return instance != null;
            }
        }
        private static void SetInstance(UiManager instance)
        {
            UiManager.instance = instance;
        }

        private UiConfig config;

        private readonly Stack<UiState> stateStack = new Stack<UiState>();
        private readonly Dictionary<string, UiStateEntry> cachedStates = new Dictionary<string, UiStateEntry>(StringComparer.Ordinal);
        private readonly Dictionary<Type, string> typeToKeyMap = new Dictionary<Type, string>();

        private readonly Dictionary<string, SceneInstance> loadedScenesByGuid = new Dictionary<string, SceneInstance>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> sceneRefCountsByGuid = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> loadingHandlesByGuid = new Dictionary<string, AsyncOperationHandle<SceneInstance>>(StringComparer.Ordinal);

        private async Task InitializeInternal(UiState defaultState = null)
        {
            UiConfig loadedConfig = await LoadRuntimeUiConfigFromAddressablesAsync();
            
            if (loadedConfig == null)
            {
                Debug.LogError("❌ UiConfig failed to load via Addressables label 'RuntimeUiConfig'.");
                return;
            }

            config = loadedConfig;
            
            cachedStates.Clear();
            typeToKeyMap.Clear();

            foreach (UiStateEntry entry in config.entries)
            {
                cachedStates[entry.stateKey] = entry;
                typeToKeyMap[GetTypeForKey(entry.stateKey)] = entry.stateKey;
            }
        }

        public static async Task<UiManager> InitializeAsync(UiState defaultState = null)
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject gameObjectUiManager = new GameObject("UiManager");
            UiManager created = gameObjectUiManager.AddComponent<UiManager>();
            DontDestroyOnLoad(gameObjectUiManager);
            SetInstance(created);

            await created.InitializeInternal(defaultState);
            return created;
        }

        private async Task<UiConfig> LoadRuntimeUiConfigFromAddressablesAsync()
        {
            AsyncOperationHandle<UiConfig> handle = Addressables.LoadAssetAsync<UiConfig>("UiConfig");

            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("❌ Failed to locate UiConfig via Addressables key/label 'UiConfig'. Ensure the UI Editor Window generated the runtime config and assigned this label in the Global Configs group.");
                return null;
            }

            UiConfig cfg = handle.Result;

            if (cfg == null)
            {
                Debug.LogError("❌ Failed to locate UiConfig via Addressables label 'UiConfig'. Ensure the UI Editor Window generated the runtime config and assigned this label in the Global Configs group.");
            }

            return cfg;
        }

        public static async Task ShowState<T>(object context = null, bool additive = false) where T : UiState
        {
            if (instance == null)
            {
                Debug.LogError("❌ UiManager instance not set.");
                return;
            }

            if (!instance.typeToKeyMap.TryGetValue(typeof(T), out string key))
            {
                Debug.LogError($"❌ No UI state key registered for {typeof(T).Name}");
                return;
            }

            await ShowStateByKey(key, context, additive);
        }

        public static async Task ShowStateByKey(string stateKey, object context = null, bool additive = false)
        {
            if (instance == null)
            {
                Debug.LogError("❌ UiManager instance not set.");
                return;
            }

            if (!instance.cachedStates.TryGetValue(stateKey, out UiStateEntry entry))
            {
                Debug.LogError($"❌ State '{stateKey}' not found in cache.");
                return;
            }

            await instance.LoadAndPushState(entry, context, additive);
        }

        private async Task LoadAndPushState(UiStateEntry entry, object context, bool additive)
        {
            UiState newState = new UiState(entry.stateKey, entry.uiElementScenes);

            if (!additive && stateStack.Count > 0)
            {
                await LoadStateScenesAsync(newState, context);
                await UnloadAllPreviousStates();
                stateStack.Clear();
                stateStack.Push(newState);
                return;
            }

            await LoadStateScenesAsync(newState, context);
            stateStack.Push(newState);
        }

        private async Task LoadStateScenesAsync(UiState state, object context)
        {
            if (state == null)
            {
                return;
            }

            IReadOnlyList<AssetReference> sceneRefs = state.GetUiElementScenes();
            if (sceneRefs == null)
            {
                return;
            }

            HashSet<string> uniqueGuids = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < sceneRefs.Count; i++)
            {
                AssetReference sceneRef = sceneRefs[i];
                if (sceneRef == null)
                {
                    continue;
                }

                string sceneGuid = sceneRef.AssetGUID;
                if (string.IsNullOrEmpty(sceneGuid))
                {
                    Debug.LogWarning("[UiManager] Scene reference has no AssetGUID; cannot dedupe shared scenes.");
                    continue;
                }

                if (!uniqueGuids.Add(sceneGuid))
                {
                    continue;
                }

                SceneInstance sceneInstance = await LoadSceneIfNeededAsync(sceneRef, sceneGuid);
                if (!sceneInstance.Scene.isLoaded)
                {
                    Debug.LogError("[UiManager] Failed to load UI element scene. Guid: " + sceneGuid);
                    continue;
                }

                IncrementSceneRefCount(sceneGuid);
                state.RegisterLoadedScene(sceneGuid, sceneInstance.Scene, context);
            }
        }

        private async Task<SceneInstance> LoadSceneIfNeededAsync(AssetReference sceneRef, string sceneGuid)
        {
            if (loadedScenesByGuid.TryGetValue(sceneGuid, out SceneInstance existing))
            {
                return existing;
            }

            if (loadingHandlesByGuid.TryGetValue(sceneGuid, out AsyncOperationHandle<SceneInstance> inFlight))
            {
                await inFlight.Task;

                if (loadedScenesByGuid.TryGetValue(sceneGuid, out SceneInstance loadedAfterWait))
                {
                    return loadedAfterWait;
                }

                return default;
            }

            AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneRef, LoadSceneMode.Additive, true);
            loadingHandlesByGuid.Add(sceneGuid, handle);

            await handle.Task;
            loadingHandlesByGuid.Remove(sceneGuid);

            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[UiManager] Failed to load scene (Addressables): " + handle.DebugName);
                return default;
            }

            SceneInstance sceneInstance = handle.Result;
            loadedScenesByGuid[sceneGuid] = sceneInstance;
            return sceneInstance;
        }

        private void IncrementSceneRefCount(string sceneGuid)
        {
            if (sceneRefCountsByGuid.TryGetValue(sceneGuid, out int count))
            {
                sceneRefCountsByGuid[sceneGuid] = count + 1;
                return;
            }

            sceneRefCountsByGuid.Add(sceneGuid, 1);
        }

        private async Task DecrementSceneRefCountAndUnloadIfNeededAsync(string sceneGuid)
        {
            if (!sceneRefCountsByGuid.TryGetValue(sceneGuid, out int count))
            {
                return;
            }

            int nextCount = count - 1;
            if (nextCount > 0)
            {
                sceneRefCountsByGuid[sceneGuid] = nextCount;
                return;
            }

            sceneRefCountsByGuid.Remove(sceneGuid);

            if (loadingHandlesByGuid.TryGetValue(sceneGuid, out AsyncOperationHandle<SceneInstance> inFlight))
            {
                await inFlight.Task;
            }

            if (loadedScenesByGuid.TryGetValue(sceneGuid, out SceneInstance instance))
            {
                loadedScenesByGuid.Remove(sceneGuid);
                await Addressables.UnloadSceneAsync(instance).Task;
            }
        }

        private async Task ReleaseStateScenesAsync(UiState state)
        {
            if (state == null)
            {
                return;
            }

            List<string> guids = state.GetAcquiredSceneGuids();
            for (int i = 0; i < guids.Count; i++)
            {
                await DecrementSceneRefCountAndUnloadIfNeededAsync(guids[i]);
            }
        }

        public static async Task HideUI()
        {
            if (instance == null || instance.stateStack.Count == 0)
            {
                return;
            }

            UiState popped = instance.stateStack.Pop();

            await instance.ReleaseStateScenesAsync(popped);
            popped.Dispose();
        }

        public static bool IsLastSceneActive<T>()
        {
            if (instance == null)
            {
                return false;
            }

            if (!instance.typeToKeyMap.TryGetValue(typeof(T), out string key))
            {
                return false;
            }

            UiState popped = instance.stateStack.Peek();

            if (popped.StateName == key)
            {
                return true;
            }

            return false;
        }

        public static bool TryGetUiStateKeyName<T>(out string keyName) where T : UiState
        {
            keyName = "";

            if (instance == null)
            {
                return false;
            }

            if (!instance.typeToKeyMap.TryGetValue(typeof(T), out string key))
            {
                return false;
            }

            keyName = key;
            return true;
        }

        public void ResetUI()
        {
            Task task = UnloadAllPreviousStates();
            task.Wait();
        }

        private async Task UnloadAllPreviousStates()
        {
            while (stateStack.Count > 0)
            {
                UiState poppedState = stateStack.Pop();

                if (poppedState != null)
                {
                    Debug.Log($"[UiManager] Unloading UI state: {poppedState.StateName}");
                    await ReleaseStateScenesAsync(poppedState);
                    poppedState.Dispose();
                }
            }
        }

        public static UiState GetCurrentState()
        {
            return instance?.stateStack.Count > 0 ? instance.stateStack.Peek() : null;
        }

        private Type GetTypeForKey(string key)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(asm => asm.GetTypes())
                .FirstOrDefault(t => typeof(UiState).IsAssignableFrom(t) && t.Name == key)
                ?? typeof(UiState);
        }
    }
}
