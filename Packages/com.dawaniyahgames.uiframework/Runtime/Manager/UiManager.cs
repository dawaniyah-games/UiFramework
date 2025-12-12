using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading.Tasks;
using UiFramework.Core;

namespace UiFramework.Runtime.Manager
{
    public class UiManager : MonoBehaviour
    {
        private static UiManager instance;
        public static void SetInstance(UiManager instance)
        {
            UiManager.instance = instance;
        }

        [SerializeField] private UiConfig config;

        private readonly Stack<UiState> stateStack = new();
        private readonly Dictionary<string, UiStateEntry> cachedStates = new();
        private readonly Dictionary<Type, string> typeToKeyMap = new();

        public async Task Init(UiState defaultState = null)
        {
            if (config == null)
            {
                Debug.LogError("❌ UiConfig not assigned to UiManager.");
                return;
            }

            cachedStates.Clear();
            typeToKeyMap.Clear();

            foreach (var entry in config.entries)
            {
                cachedStates[entry.stateKey] = entry;
                typeToKeyMap[GetTypeForKey(entry.stateKey)] = entry.stateKey;
            }

            SetInstance(this);

            if (defaultState != null)
            {
                // await ShowState(defaultState.GetType());
            }
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
            UiState newState = new(entry.stateKey, entry.uiElementScenes);

            if (!additive && stateStack.Count > 0)
            {
                newState.Init(context);
                await newState.WaitForInitializationAsync();
                List<string> keepScenes = newState.GetUiElementSceneNames();
                await UnloadAllPreviousStates(keepScenes);
                stateStack.Clear();
                stateStack.Push(newState);
                return;
            }

            stateStack.Push(newState);
            newState.Init(context);
        }

        public static async Task HideUI()
        {
            if (instance == null || instance.stateStack.Count == 0)
            {
                return;
            }

            UiState popped = instance.stateStack.Pop();
            List<string> nextScenes = instance.stateStack.Count > 0 ? instance.stateStack.Peek().GetUiElementSceneNames() : new List<string>();

            await popped.UnloadUiState(nextScenes);
            popped.Dispose();

            if (instance.stateStack.Count > 0)
            {
                instance.stateStack.Peek().Init();
            }
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
            while (stateStack.Count > 0)
            {
                stateStack.Pop().Dispose();
            }
        }

        private async Task UnloadAllPreviousStates(List<string> keepScenes = null)
        {
            while (stateStack.Count > 0)
            {
                UiState poppedState = stateStack.Pop();

                if (poppedState != null)
                {
                    Debug.Log($"[UiManager] Unloading UI state: {poppedState.StateName}");
                    await poppedState.UnloadUiState(keepScenes);
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
