using UiFramework.Tweening;
using UnityEngine;

namespace UiFramework.Core
{
    // Optional animation override for a single ShowState / ShowStateByKey / HideUI call, e.g.
    //
    //   await UiManager.ShowState<StoreUiState>(context, transition: UiTransition.ShowWith(TweenPresets.ScaleKey));
    //   await UiManager.HideUI(UiTransition.HideWith(TweenPresets.SlideRightKey));
    //
    // ShowPreset replaces the show animation of every element that appears in that call; HidePreset replaces the hide
    // animation of every element that disappears in it. A null preset keeps each element's own animation (set in the
    // Elements tab). The override only applies to that call; the elements' configured animations don't change.
    public sealed class UiTransition
    {
        public static readonly UiTransition Default = new UiTransition(null, null);

        // Built-in preset key (TweenPresets.*Key), or null to keep each element's own show animation.
        public string ShowPreset { get; }

        // Built-in preset key (TweenPresets.*Key), or null to keep each element's own hide animation.
        public string HidePreset { get; }

        // Unknown preset names are logged and ignored (the elements' own animations play instead).
        public UiTransition(string showPreset, string hidePreset)
        {
            ShowPreset = ResolvePreset(showPreset, "show");
            HidePreset = ResolvePreset(hidePreset, "hide");
        }

        public static UiTransition ShowWith(string showPreset)
        {
            return new UiTransition(showPreset, null);
        }

        public static UiTransition HideWith(string hidePreset)
        {
            return new UiTransition(null, hidePreset);
        }

        public override string ToString()
        {
            return "UiTransition(show: " + (ShowPreset ?? "element default") + ", hide: " + (HidePreset ?? "element default") + ")";
        }

        private static string ResolvePreset(string preset, string role)
        {
            if (string.IsNullOrWhiteSpace(preset))
            {
                return null;
            }

            string presetKey;
            if (TweenPresets.TryGetPresetKey(preset, out presetKey))
            {
                return presetKey;
            }

            Debug.LogWarning("[UiTransition] Unknown " + role + " animation '" + preset + "'. Each element keeps its own animation. Use one of the TweenPresets.*Key constants.");
            return null;
        }
    }
}
