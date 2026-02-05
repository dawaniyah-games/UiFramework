namespace UiFramework.Editor.States
{
    using UiFramework.Core;
    using System.Collections.Generic;
    using UnityEngine.AddressableAssets;

    // Auto-generated: UI State
    public class UserProfileUiState : UiState
    {
        // Your UiState base expects (string name, List<AssetReference> uiElementScenes)
        public UserProfileUiState() : base("UserProfileUiState", new List<AssetReference>())
        {
            // Add addressable UI prefabs/scenes to the list if needed
        }

        public override string ToString()
        {
            return nameof(UserProfileUiState);
        }
    }
}
