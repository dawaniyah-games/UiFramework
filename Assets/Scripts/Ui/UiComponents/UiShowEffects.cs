using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace UiShowcase.Components
{
    // A component that animates part of an element whenever the element shows (RevealOnShow, StaggeredReveal,
    // FillBar, CountUpText). ShowcaseUiElement finds them and runs them next to the element's show preset.
    public interface IUiShowEffect
    {
        // Puts the effect in its start pose. Runs right after Populate, before the element is drawn, so nothing flashes.
        void PrepareShowEffect();

        Task PlayShowEffectAsync();
    }

    public static class UiShowEffects
    {
        // Effects on inactive objects (templates, hidden pages) are skipped.
        public static void PrepareAll(Component root)
        {
            if (root == null)
            {
                return;
            }

            IUiShowEffect[] effects = root.GetComponentsInChildren<IUiShowEffect>(false);
            for (int i = 0; i < effects.Length; i++)
            {
                effects[i].PrepareShowEffect();
            }
        }

        public static Task PlayAllAsync(Component root)
        {
            if (root == null)
            {
                return Task.CompletedTask;
            }

            IUiShowEffect[] effects = root.GetComponentsInChildren<IUiShowEffect>(false);
            if (effects.Length == 0)
            {
                return Task.CompletedTask;
            }

            List<Task> tasks = new List<Task>(effects.Length);
            for (int i = 0; i < effects.Length; i++)
            {
                tasks.Add(effects[i].PlayShowEffectAsync());
            }

            return Task.WhenAll(tasks);
        }

        // Plays the effects under root again, e.g. after a screen rebuilt part of itself (new leaderboard, store page).
        public static Task ReplayAsync(Component root)
        {
            PrepareAll(root);
            return PlayAllAsync(root);
        }
    }
}
