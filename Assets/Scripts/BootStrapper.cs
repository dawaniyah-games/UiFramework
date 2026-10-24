using System;
using UiFramework.Runtime.Manager;
using UiShowcase.App;
using UiShowcase.Components;
using UiShowcase.States;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

public class BootStrapper : MonoBehaviour
{
    [Tooltip("Root that survives scene changes and holds the camera and the EventSystem. Created when empty.")]
    [SerializeField] private GameObject dontDestroyGameObject;

    // The persistent root kept by the first bootstrap; reloading the bootstrap scene must not add a second camera
    // or EventSystem (two EventSystems log errors, two AudioListeners warn every frame).
    private static GameObject persistentRoot;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        persistentRoot = null;
    }

    private void Awake()
    {
        if (persistentRoot != null)
        {
            if (dontDestroyGameObject != null && dontDestroyGameObject != persistentRoot)
            {
                Destroy(dontDestroyGameObject);
            }

            return;
        }

        persistentRoot = CreatePersistentRoot();
    }

    // async void is acceptable for Unity lifecycle methods, as long as exceptions are caught and logged.
    private async void Start()
    {
        try
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;

            if (!UiManager.IsInitialized)
            {
                await UiManager.InitializeAsync();
            }

            string introKey;
            if (!UiManager.TryGetUiStateKeyName<IntroUiState>(out introKey))
            {
                Debug.LogError("[BootStrapper] IntroUiState is not in the runtime UiConfig, so there is nothing to show. Run Window > UI Framework > UI Setup Manager > General > Build Runtime UiConfig, then press Play again.");
                return;
            }

            // The intro plays its loading animation, then ShowcaseNavigator moves on to the Home tab.
            await ShowcaseNavigator.ShowIntroAsync();
        }
        catch (Exception exception)
        {
            Debug.LogError("[BootStrapper] Startup failed: " + exception);
        }
    }

    private GameObject CreatePersistentRoot()
    {
        GameObject root = dontDestroyGameObject;
        if (root == null)
        {
            root = new GameObject("PersistentRoot");
        }

        // DontDestroyOnLoad only works on root objects.
        root.transform.SetParent(null, true);

        EnsureCamera(root.transform);
        EnsureEventSystem(root.transform);
        DontDestroyOnLoad(root);
        return root;
    }

    private static void EnsureCamera(Transform root)
    {
        Camera camera = root.GetComponentInChildren<Camera>(true);
        if (camera == null)
        {
            camera = FindAnyObjectByType<Camera>();
        }

        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = ShowcasePalette.Background;
            Debug.Log("[BootStrapper] No camera in the bootstrap scene; created one.");
        }

        camera.transform.SetParent(root, true);

        if (camera.GetComponent<AudioListener>() == null && FindAnyObjectByType<AudioListener>() == null)
        {
            camera.gameObject.AddComponent<AudioListener>();
        }

        // The element canvases are Screen Space - Camera; this is the camera every one of them renders with.
        UiCanvasCamera.Register(camera);
    }

    private static void EnsureEventSystem(Transform root)
    {
        EventSystem eventSystem = root.GetComponentInChildren<EventSystem>(true);
        if (eventSystem == null)
        {
            eventSystem = FindAnyObjectByType<EventSystem>();
        }

        if (eventSystem == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
            Debug.Log("[BootStrapper] No EventSystem in the bootstrap scene; created one.");
        }

        // Every UI element scene relies on this one EventSystem; element scenes never carry their own.
        eventSystem.transform.SetParent(root, true);
    }
}
