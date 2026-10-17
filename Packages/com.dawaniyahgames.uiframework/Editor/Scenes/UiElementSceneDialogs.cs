using UnityEngine;

namespace UiFramework.Editor.Scenes
{
    // Console output after a structure fix. Confirmation dialogs live in Window/Popups/UiElementChangeDialog.
    internal static class UiElementSceneDialogs
    {
        public static void LogRemaining(string sceneName, UiElementStructureReport remaining)
        {
            if (remaining.HasIssues)
            {
                Debug.LogWarning("[UiFramework] Fixed structure of '" + sceneName + "'. Remaining issues:\n" + remaining.GetIssuesText());
                return;
            }

            Debug.Log("[UiFramework] Fixed structure of '" + sceneName + "'; it now follows the UI element scene contract.");
        }
    }
}
