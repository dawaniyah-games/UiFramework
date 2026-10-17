using UnityEngine;
using UnityEngine.UI;

namespace UiFramework.Editor.Config
{
    // Default Canvas/CanvasScaler values for new UI element scenes (and the per-element "Use Default Settings" button).
    // Written into scenes at authoring time only; nothing enforces them at runtime, and each scene can differ.
    [CreateAssetMenu(fileName = "UiGlobalCanvasSettings", menuName = "Scripts/UiFramework/Global Canvas Settings")]
    public sealed class UiGlobalCanvasSettings : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/UiConfigs/UiGlobalCanvasSettings.asset";

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
    }
}
