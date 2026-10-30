using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace UiFramework.Core
{
    public class UiState : IDisposable
    {
        public string StateName { get; }

        private readonly List<AssetReference> uiElementScenes;
        private readonly HashSet<string> acquiredSceneGuids = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<IUiElement> activeUiElements = new List<IUiElement>();

        public UiState(string stateName, List<AssetReference> uiElementScenes)
        {
            StateName = stateName;
            this.uiElementScenes = uiElementScenes ?? new List<AssetReference>();
        }

        public IReadOnlyList<AssetReference> GetUiElementScenes()
        {
            return uiElementScenes.AsReadOnly();
        }

        public void RegisterLoadedScene(string sceneGuid, UnityEngine.SceneManagement.Scene loadedScene, object context)
        {
            if (string.IsNullOrEmpty(sceneGuid))
            {
                return;
            }

            if (!acquiredSceneGuids.Add(sceneGuid))
            {
                return;
            }

            if (!loadedScene.isLoaded)
            {
                Debug.LogWarning("[UiState] RegisterLoadedScene called with an unloaded scene.");
                return;
            }

            PopulateFromScene(loadedScene, context);
        }

        public List<string> GetAcquiredSceneGuids()
        {
            List<string> result = new List<string>(acquiredSceneGuids.Count);

            foreach (string guid in acquiredSceneGuids)
            {
                result.Add(guid);
            }

            return result;
        }

        private void PopulateFromScene(UnityEngine.SceneManagement.Scene scene, object context)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int r = 0; r < roots.Length; r++)
            {
                MonoBehaviour[] behaviours = roots[r].GetComponentsInChildren<MonoBehaviour>(true);
                for (int u = 0; u < behaviours.Length; u++)
                {
                    MonoBehaviour behaviour = behaviours[u];
                    if (behaviour == null)
                    {
                        continue;
                    }

                    IUiElement element = behaviour as IUiElement;
                    if (element == null)
                    {
                        continue;
                    }

                    element.Populate(context);

                    if (!activeUiElements.Contains(element))
                    {
                        activeUiElements.Add(element);
                    }
                }
            }
        }

        public IReadOnlyList<IUiElement> GetActiveUiElements()
        {
            return activeUiElements.AsReadOnly();
        }

        public void Dispose()
        {
            acquiredSceneGuids.Clear();
            activeUiElements.Clear();
        }
    }
}
