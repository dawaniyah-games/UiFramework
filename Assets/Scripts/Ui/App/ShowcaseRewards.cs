using System.Threading.Tasks;
using UnityEngine;

namespace UiShowcase.App
{
    // Something on screen that rewards can fly into (the TopBar's coin and gem counters).
    public interface IRewardFlyTarget
    {
        Task FlyRewardAsync(ShowcaseCurrency currency, Vector3 fromWorldPosition, int amount);
    }

    // Credits rewards to the session. When a fly target is on screen, the coins and gems fly into it and are
    // credited as they land; otherwise they are credited at once.
    public static class ShowcaseRewards
    {
        private static IRewardFlyTarget flyTarget;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            flyTarget = null;
        }

        public static void RegisterFlyTarget(IRewardFlyTarget target)
        {
            flyTarget = target;
        }

        public static void UnregisterFlyTarget(IRewardFlyTarget target)
        {
            if (flyTarget == target)
            {
                flyTarget = null;
            }
        }

        public static Task CollectAsync(Vector3 fromWorldPosition, int coins, int gems)
        {
            return Task.WhenAll(
                CollectAsync(ShowcaseCurrency.Coins, fromWorldPosition, coins),
                CollectAsync(ShowcaseCurrency.Gems, fromWorldPosition, gems));
        }

        private static Task CollectAsync(ShowcaseCurrency currency, Vector3 fromWorldPosition, int amount)
        {
            if (amount <= 0)
            {
                return Task.CompletedTask;
            }

            if (flyTarget == null)
            {
                ShowcaseSession.Current.Add(currency, amount);
                return Task.CompletedTask;
            }

            return flyTarget.FlyRewardAsync(currency, fromWorldPosition, amount);
        }
    }
}
