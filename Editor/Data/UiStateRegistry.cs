using System.Collections.Generic;
using UnityEngine;

namespace UiFramework.Editor.Data
{
    [CreateAssetMenu(fileName = "UiStateRegistry", menuName = "Scripts/UiFramework/State Registry")]
    public class UiStateRegistry : ScriptableObject
    {
        public List<UiStateMetadata> States = new();
    }

    [System.Serializable]
    public class UiStateMetadata
    {
        public string StateName;
        public List<string> ElementSceneNames = new();
    }
}
