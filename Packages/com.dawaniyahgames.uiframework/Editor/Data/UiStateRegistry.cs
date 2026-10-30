using System.Collections.Generic;
using UnityEngine;

namespace UiFramework.Editor.Data
{
    [CreateAssetMenu(fileName = "UiStateRegistry", menuName = "UI Framework/State Registry")]
    public class UiStateRegistry : ScriptableObject
    {
        public List<UiStateMetadata> States = new List<UiStateMetadata>();
    }

    [System.Serializable]
    public class UiStateMetadata
    {
        public string StateName;
        public List<string> ElementSceneNames = new List<string>();
    }
}
