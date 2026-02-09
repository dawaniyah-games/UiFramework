using System;
using System.Threading.Tasks;
using UiFramework.Runtime.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiNavigationSmokeTest : MonoBehaviour
{
    [Header("State Keys")]
    [SerializeField] private string firstStateKey;
    [SerializeField] private string secondStateKey;

    [Header("Input")]
    [SerializeField] private KeyCode showFirstKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode showSecondKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode hideKey = KeyCode.BackQuote;

    private bool isInitialized;

    private async void Start()
    {
        await UiManager.InitializeAsync();
        isInitialized = true;

        Debug.Log("[UiNavigationSmokeTest] Initialized. Press 1/2 to show states, ` to hide.");
        LogLoadedScenes("After Initialize");
    }

    private async void Update()
    {
        if (!isInitialized)
        {
            return;
        }

        if (Input.GetKeyDown(showFirstKey))
        {
            await Show(firstStateKey);
        }

        if (Input.GetKeyDown(showSecondKey))
        {
            await Show(secondStateKey);
        }

        if (Input.GetKeyDown(hideKey))
        {
            await UiManager.HideUI();
            LogLoadedScenes("After HideUI");
        }
    }

    private async Task Show(string stateKey)
    {
        if (string.IsNullOrEmpty(stateKey))
        {
            Debug.LogWarning("[UiNavigationSmokeTest] State key is empty.");
            return;
        }

        DateTime start = DateTime.UtcNow;
        await UiManager.ShowStateByKey(stateKey, null, false);
        TimeSpan duration = DateTime.UtcNow - start;

        Debug.Log("[UiNavigationSmokeTest] ShowStateByKey: " + stateKey + " (" + duration.TotalMilliseconds.ToString("0") + "ms)");
        LogLoadedScenes("After Show " + stateKey);
    }

    private void LogLoadedScenes(string label)
    {
        int count = SceneManager.sceneCount;
        string names = "";

        for (int i = 0; i < count; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (i > 0)
            {
                names += ", ";
            }

            names += scene.name;
        }

        Debug.Log("[UiNavigationSmokeTest] " + label + " | Loaded scenes (" + count + "): " + names);
    }
}
