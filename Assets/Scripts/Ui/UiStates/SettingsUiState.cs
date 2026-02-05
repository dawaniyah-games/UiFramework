namespace UiFramework.Editor.States
{
    using UiFramework.Core;
    using System.Collections.Generic;
    using UnityEngine.AddressableAssets;

    // Auto-generated: UI State
    public class SettingsUiState : UiState
    {
        // Your UiState base expects (string name, List<AssetReference> uiElementScenes)
        public SettingsUiState() : base("SettingsUiState", new List<AssetReference>())
        {
            // Add addressable UI prefabs/scenes to the list if needed
        }

        public override string ToString()
        {
            return nameof(SettingsUiState);
        }
    }
}
