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

        private UiState loadingState;

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

            loadingState = newState;

            HashSet<int> currentlyVisibleElementIds = GetVisibleElementIds();

            if (!additive && stateStack.Count > 0)
            {
                await LoadStateScenesAsync(newState, context, true, currentlyVisibleElementIds);

                HashSet<int> sharedElementIds = GetSharedElementIds(currentlyVisibleElementIds, newState);

                Task hideTask = PlayHideTransitionsForAllStatesAsync(sharedElementIds);
                Task showTask = PlayShowTransitionAsync(newState, sharedElementIds);
                await Task.WhenAll(hideTask, showTask);

                await UnloadAllPreviousStates(false);
                stateStack.Clear();
                stateStack.Push(newState);
                loadingState = null;
                return;
            }

            await LoadStateScenesAsync(newState, context, true, currentlyVisibleElementIds);
            stateStack.Push(newState);

            loadingState = null;

            HashSet<int> sharedInAdditive = GetSharedElementIds(currentlyVisibleElementIds, newState);
            await PlayShowTransitionAsync(newState, sharedInAdditive);
        }

        private static void PrepareShowTransition(UiState state, HashSet<int> skipElementIds)
        {
            if (state == null)
            {
                return;
            }

            IReadOnlyList<IUiElement> elements = state.GetActiveUiElements();
            if (elements == null || elements.Count == 0)
            {
                return;
            }

            HashSet<int> visited = new HashSet<int>();

            for (int i = 0; i < elements.Count; i++)
            {
                MonoBehaviour uiElementBehaviour = elements[i] as MonoBehaviour;
                if (uiElementBehaviour == null || uiElementBehaviour.gameObject == null)
                {
                    continue;
                }

                int id = uiElementBehaviour.gameObject.GetInstanceID();
                if (!visited.Add(id))
                {
                    continue;
                }

                if (skipElementIds != null && skipElementIds.Contains(id))
                {
                    continue;
                }

                UiElement uiElement = elements[i] as UiElement;
                if (uiElement == null)
                {
                    continue;
                }

                uiElement.PrepareForShow();
            }
        }

        private async Task PlayShowTransitionAsync(UiState state, HashSet<int> skipElementIds)
        {
            if (state == null)
            {
                return;
            }

            await PlayTransitionAsync(state, true, skipElementIds);
        }

        private async Task PlayHideTransitionsForAllStatesAsync(HashSet<int> skipElementIds)
        {
            if (stateStack == null || stateStack.Count == 0)
            {
                return;
            }

            UiState[] states = stateStack.ToArray();
            for (int i = 0; i < states.Length; i++)
            {
                await PlayTransitionAsync(states[i], false, skipElementIds);
            }
        }

        private static async Task PlayTransitionAsync(UiState state, bool isShow, HashSet<int> skipElementIds)
        {
            if (state == null)
            {
                return;
            }

            IReadOnlyList<IUiElement> elements = state.GetActiveUiElements();
            if (elements == null || elements.Count == 0)
            {
                return;
            }

            HashSet<int> visited = new HashSet<int>();
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < elements.Count; i++)
            {
                MonoBehaviour uiElementBehaviour = elements[i] as MonoBehaviour;
                if (uiElementBehaviour == null)
                {
                    continue;
                }

                GameObject uiElementGameObject = uiElementBehaviour.gameObject;
                if (uiElementGameObject == null)
                {
                    continue;
                }

                int id = uiElementGameObject.GetInstanceID();
                if (!visited.Add(id))
                {
                    continue;
                }

                if (skipElementIds != null && skipElementIds.Contains(id))
                {
                    continue;
                }

                UiElement uiElement = elements[i] as UiElement;
                if (uiElement != null)
                {
                    tasks.Add(isShow ? uiElement.ShowAsync() : uiElement.HideAsync());
                    continue;
                }

                // Non-UiElement implementations of IUiElement do not have built-in animation.
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks);
            }
        }

        private async Task LoadStateScenesAsync(UiState state, object context, bool prepareForShow, HashSet<int> skipPrepareElementIds)
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

                if (prepareForShow)
                {
                    PrepareShowTransition(state, skipPrepareElementIds);
                }
            }
        }

        private HashSet<int> GetVisibleElementIds()
        {
            HashSet<int> result = new HashSet<int>();

            if (stateStack == null || stateStack.Count == 0)
            {
                return result;
            }

            UiState[] states = stateStack.ToArray();
            for (int s = 0; s < states.Length; s++)
            {
                UiState state = states[s];
                if (state == null)
                {
                    continue;
                }

                IReadOnlyList<IUiElement> elements = state.GetActiveUiElements();
                if (elements == null)
                {
                    continue;
                }

                for (int i = 0; i < elements.Count; i++)
                {
                    MonoBehaviour behaviour = elements[i] as MonoBehaviour;
                    if (behaviour == null || behaviour.gameObject == null)
                    {
                        continue;
                    }

                    result.Add(behaviour.gameObject.GetInstanceID());
                }
            }

            return result;
        }

        private static HashSet<int> GetSharedElementIds(HashSet<int> currentlyVisibleElementIds, UiState newState)
        {
            HashSet<int> shared = new HashSet<int>();

            if (currentlyVisibleElementIds == null || currentlyVisibleElementIds.Count == 0 || newState == null)
            {
                return shared;
            }

            IReadOnlyList<IUiElement> elements = newState.GetActiveUiElements();
            if (elements == null)
            {
                return shared;
            }

            for (int i = 0; i < elements.Count; i++)
            {
                MonoBehaviour behaviour = elements[i] as MonoBehaviour;
                if (behaviour == null || behaviour.gameObject == null)
                {
                    continue;
                }

                int id = behaviour.gameObject.GetInstanceID();
                if (currentlyVisibleElementIds.Contains(id))
                {
                    shared.Add(id);
                }
            }

            return shared;
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

            await PlayTransitionAsync(popped, false, null);

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

        private async Task UnloadAllPreviousStates(bool playHideTransitions = true)
        {
            while (stateStack.Count > 0)
            {
                UiState poppedState = stateStack.Pop();

                if (poppedState != null)
                {
                    Debug.Log($"[UiManager] Unloading UI state: {poppedState.StateName}");

                    if (playHideTransitions)
                    {
                        await PlayTransitionAsync(poppedState, false, null);
                    }

                    await ReleaseStateScenesAsync(poppedState);
                    poppedState.Dispose();
                }
            }
        }

        public static UiState GetCurrentState()
        {
            if (instance == null)
            {
                return null;
            }

            if (instance.loadingState != null)
            {
                return instance.loadingState;
            }

            return instance.stateStack.Count > 0 ? instance.stateStack.Peek() : null;
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
