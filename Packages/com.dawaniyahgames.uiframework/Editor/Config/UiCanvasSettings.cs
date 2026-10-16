using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UiFramework.Editor.Config
{
    // Project-wide Canvas/CanvasScaler values for UI element scenes. Written into scenes at authoring time only
    // (Fix / Reset / Apply actions); nothing enforces them at runtime.
    [CreateAssetMenu(fileName = "UiCanvasSettings", menuName = "Scripts/UiFramework/Canvas Settings")]
    public sealed class UiCanvasSettings : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/UiConfigs/UiCanvasSettings.asset";

        [Header("Canvas")]
        [SerializeField] private RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
        [SerializeField] private bool pixelPerfect = false;

        [Header("Canvas Scaler")]
        [SerializeField] private CanvasScaler.ScaleMode uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        [SerializeField] private Vector2 referenceResolution = new Vector2(1080.0f, 1920.0f);
        [SerializeField] private CanvasScaler.ScreenMatchMode screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        [Tooltip("Only used when Screen Match Mode is MatchWidthOrHeight.")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        [SerializeField] private float referencePixelsPerUnit = 100.0f;

        [Header("Apply To All")]
        [Tooltip("Skipped by 'Apply Canvas Settings To All'. Structure checks and the per-scene 'Reset Canvas' still apply to these scenes.")]
        [SerializeField] private List<SceneAsset> excludedScenes = new List<SceneAsset>();

        public RenderMode RenderMode
        {
            get
            {
                return renderMode;
            }
        }

        public bool PixelPerfect
        {
            get
            {
                return pixelPerfect;
            }
        }

        public CanvasScaler.ScaleMode UiScaleMode
        {
            get
            {
                return uiScaleMode;
            }
        }

        public Vector2 ReferenceResolution
        {
            get
            {
                return referenceResolution;
            }
        }

        public CanvasScaler.ScreenMatchMode ScreenMatchMode
        {
            get
            {
                return screenMatchMode;
            }
        }

        public float MatchWidthOrHeight
        {
            get
            {
                return matchWidthOrHeight;
            }
        }

        public float ReferencePixelsPerUnit
        {
            get
            {
                return referencePixelsPerUnit;
            }
        }

        public IReadOnlyList<SceneAsset> ExcludedScenes
        {
            get
            {
                if (excludedScenes == null)
                {
                    excludedScenes = new List<SceneAsset>();
                }

                return excludedScenes.AsReadOnly();
            }
        }

        public bool IsExcluded(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath) || excludedScenes == null)
            {
                return false;
            }

            for (int i = 0; i < excludedScenes.Count; i++)
            {
                SceneAsset sceneAsset = excludedScenes[i];
                if (sceneAsset == null)
                {
                    continue;
                }

                if (string.Equals(AssetDatabase.GetAssetPath(sceneAsset), scenePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        // Returns true when the exclusion list changed. Undo-recorded; the caller decides when to save the asset.
        public bool SetExcluded(string scenePath, bool excluded)
        {
            if (IsExcluded(scenePath) == excluded)
            {
                return false;
            }

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset == null)
            {
                return false;
            }

            Undo.RecordObject(this, excluded ? "Exclude Scene From Canvas Settings" : "Include Scene In Canvas Settings");

            if (excludedScenes == null)
            {
                excludedScenes = new List<SceneAsset>();
            }

            if (excluded)
            {
                excludedScenes.Add(sceneAsset);
            }
            else
            {
                for (int i = excludedScenes.Count - 1; i >= 0; i--)
                {
                    SceneAsset existing = excludedScenes[i];
                    if (existing != null && string.Equals(AssetDatabase.GetAssetPath(existing), scenePath, StringComparison.OrdinalIgnoreCase))
                    {
                        excludedScenes.RemoveAt(i);
                    }
                }
            }

            EditorUtility.SetDirty(this);
            return true;
        }
    }
}
