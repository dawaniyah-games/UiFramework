using UiFramework.Runtime.Manager;
using UnityEngine;
using System.Threading.Tasks;
using UiFramework.Editor.States;

public class BootStrapper : MonoBehaviour
{
    [SerializeField] private GameObject dontDestroyGameObject;

    // Note: async void is acceptable for Unity lifecycle methods
    private async void Start()
    {
        if (dontDestroyGameObject != null)
        {
            DontDestroyOnLoad(dontDestroyGameObject);
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 60;

        if (!UiManager.IsInitialized)
        {
            await UiManager.InitializeAsync();
        }

        // await UiManager.ShowStateByKey("IntroUiState", additive: true);


        await UiManager.ShowState<MainMenuUiState>(additive: false);
    }
}
