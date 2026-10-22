using UnityEngine;
using UnityEngine.SceneManagement;

namespace UiShowcase.Components
{
    // Assigns the one UI camera to every Screen Space - Camera canvas in the UI element scenes.
    // Element scenes are loaded additively and can't reference the camera that lives in the bootstrap scene, so their
    // canvases are saved without one. The bootstrap registers its camera once; from then on every scene that loads
    // (UiManager loads element scenes as they are needed) has its camera-less canvases bound before its first frame.
    public static class UiCanvasCamera
    {
        private static Camera uiCamera;
        private static bool isListening;

        public static Camera Current
        {
            get
            {
                return uiCamera;
            }
        }

        // Domain reload can be disabled in Play Mode options, so statics are reset explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            if (isListening)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }

            uiCamera = null;
            isListening = false;
        }

        public static void Register(Camera camera)
        {
            if (camera == null)
            {
                Debug.LogWarning("[UiCanvasCamera] Register was called without a camera, so UI canvases stay unbound and render like Overlay. Pass the bootstrap scene's UI camera.");
                return;
            }

            uiCamera = camera;

            if (!isListening)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
                isListening = true;
            }

            // Scenes that were already loaded (the bootstrap scene itself, or anything opened before it).
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Bind(SceneManager.GetSceneAt(i));
            }
        }

        public static void Bind(Scene scene)
        {
            if (uiCamera == null || !scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                Canvas[] canvases = roots[i].GetComponentsInChildren<Canvas>(true);
                for (int c = 0; c < canvases.Length; c++)
                {
                    Bind(canvases[c]);
                }
            }
        }

        // Nested canvases follow their root canvas, and a canvas that already has a camera keeps it.
        public static void Bind(Canvas canvas)
        {
            if (canvas == null || uiCamera == null || canvas.renderMode != RenderMode.ScreenSpaceCamera || canvas.worldCamera != null)
            {
                return;
            }

            canvas.worldCamera = uiCamera;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Bind(scene);
        }
    }
}
