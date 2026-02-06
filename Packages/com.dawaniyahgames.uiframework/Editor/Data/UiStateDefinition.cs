using System.Collections.Generic;
using UnityEngine;

namespace UiFramework.Editor.Data
{
    [CreateAssetMenu(fileName = "UiStateDefinition", menuName = "Scripts/UiFramework/State Definition")]
    public sealed class UiStateDefinition : ScriptableObject
    {
        [SerializeField] private string stateKey;
        [SerializeField] private List<string> elementSceneNames = new List<string>();

        public string StateKey
        {
            get
            {
                return stateKey;
            }
            set
            {
                stateKey = value;
            }
        }

        public List<string> ElementSceneNames
        {
            get
            {
                return elementSceneNames;
            }
        }

        private void OnValidate()
        {
            if (stateKey != null)
            {
                stateKey = stateKey.Trim();
            }

            if (elementSceneNames == null)
            {
                elementSceneNames = new List<string>();
            }

            for (int i = elementSceneNames.Count - 1; i >= 0; i--)
            {
                string name = elementSceneNames[i];
                if (string.IsNullOrWhiteSpace(name))
                {
                    elementSceneNames.RemoveAt(i);
                    continue;
                }

                elementSceneNames[i] = name.Trim();
            }
        }
    }
}
