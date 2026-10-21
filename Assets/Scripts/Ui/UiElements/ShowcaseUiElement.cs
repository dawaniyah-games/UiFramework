using System.Threading.Tasks;
using UiFramework.Core;
using UiShowcase.Components;

namespace UiShowcase.Elements
{
    // Base for the showcase elements. The element's own show preset (picked in the UI Setup Manager's Elements tab, or
    // overridden per call with a UiTransition) moves the whole screen; on top of that, every show effect component in
    // the scene (RevealOnShow, StaggeredReveal, FillBar, CountUpText) plays at the same time. Designers choreograph a
    // screen by adding those components in the Inspector, without code.
    //
    // This uses the extension points UiElement exposes for custom animation: PrepareForShow (both overloads) and
    // ShowAsync are virtual.
    public abstract class ShowcaseUiElement : UiElement
    {
        public override void PrepareForShow()
        {
            base.PrepareForShow();
            UiShowEffects.PrepareAll(this);
        }

        // UiManager calls this overload only when a UiTransition overrides the show preset.
        public override void PrepareForShow(string presetKey)
        {
            if (string.IsNullOrWhiteSpace(presetKey))
            {
                PrepareForShow();
                return;
            }

            base.PrepareForShow(presetKey);
            UiShowEffects.PrepareAll(this);
        }

        public override Task ShowAsync(string presetKey = null)
        {
            return Task.WhenAll(base.ShowAsync(presetKey), UiShowEffects.PlayAllAsync(this));
        }
    }
}
