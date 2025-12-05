using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UiFramework.Runtime
{
    [CreateAssetMenu(fileName = "UiConfig", menuName = "UiFramework/UiConfig")]
    public class UiConfig : ScriptableObject
    {
        public List<UiStateEntry> entries = new();

        public UiStateEntry GetStateEntry(string stateKey)
        {
            return entries.Find(e => e.stateKey == stateKey);
        }

        public void AddOrUpdateEntry(string stateKey, List<AssetReference> uiElementScenes)
        {
            UiStateEntry existing = entries.Find(e => e.stateKey == stateKey);
            if (existing != null)
            {
                existing.uiElementScenes = uiElementScenes;
            }
            else
            {
                entries.Add(new UiStateEntry
                {
                    stateKey = stateKey,
                    uiElementScenes = uiElementScenes
                });
            }
        }
    }

    [Serializable]
    public class UiStateEntry
    {
        public string stateKey;
        public List<AssetReference> uiElementScenes = new();
    }
}
