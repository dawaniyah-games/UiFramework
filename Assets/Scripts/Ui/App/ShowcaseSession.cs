using System;
using System.Collections.Generic;
using UnityEngine;

namespace UiShowcase.App
{
    public enum ShowcaseSetting
    {
        Music = 0,
        Sound = 1,
        Vibration = 2,
        Notifications = 3
    }

    // The player's demo data. Elements read it in Populate and listen to its events to update what is already on screen
    // (the framework does not bind data; it only hands each element its context).
    public sealed class ShowcaseSession
    {
        private const int StartCoins = 1250;
        private const int StartGems = 40;

        private static ShowcaseSession current;

        private readonly HashSet<string> ownedProducts = new HashSet<string>();
        private readonly Dictionary<ShowcaseSetting, bool> settings = new Dictionary<ShowcaseSetting, bool>();

        private int coins;
        private int gems;
        private bool dailyRewardClaimed;

        private ShowcaseSession()
        {
            ResetData();
        }

        public static ShowcaseSession Current
        {
            get
            {
                if (current == null)
                {
                    current = new ShowcaseSession();
                }

                return current;
            }
        }

        public event Action<ShowcaseCurrency, int> CurrencyChanged;
        public event Action<ShowcaseProduct> ProductUnlocked;
        public event Action DataReset;

        public string PlayerName
        {
            get
            {
                return "Nova Quinn";
            }
        }

        public string PlayerTitle
        {
            get
            {
                return "Neon Rookie";
            }
        }

        public int Level
        {
            get
            {
                return 12;
            }
        }

        public int Experience
        {
            get
            {
                return 6420;
            }
        }

        public int ExperienceToNextLevel
        {
            get
            {
                return 10000;
            }
        }

        public int Wins
        {
            get
            {
                return 148;
            }
        }

        public int BestStreak
        {
            get
            {
                return 23;
            }
        }

        public int Stars
        {
            get
            {
                return 1260;
            }
        }

        public bool DailyRewardClaimed
        {
            get
            {
                return dailyRewardClaimed;
            }
        }

        // Domain reload can be disabled in Play Mode options, so statics are reset explicitly.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = null;
        }

        public int GetBalance(ShowcaseCurrency currency)
        {
            return currency == ShowcaseCurrency.Gems ? gems : coins;
        }

        public void Add(ShowcaseCurrency currency, int amount)
        {
            if (amount == 0)
            {
                return;
            }

            if (currency == ShowcaseCurrency.Gems)
            {
                gems = Mathf.Max(0, gems + amount);
            }
            else
            {
                coins = Mathf.Max(0, coins + amount);
            }

            if (CurrencyChanged != null)
            {
                CurrencyChanged(currency, GetBalance(currency));
            }
        }

        public bool IsOwned(ShowcaseProduct product)
        {
            return product != null && ownedProducts.Contains(product.Id);
        }

        // Spends the price and unlocks one-time products. Coins a product grants are left to the caller, so it can
        // fly them into the TopBar (see ShowcaseRewards).
        public bool TryPurchase(ShowcaseProduct product)
        {
            if (product == null || IsOwned(product) || GetBalance(product.Currency) < product.Price)
            {
                return false;
            }

            Add(product.Currency, -product.Price);

            if (!product.IsRepeatable)
            {
                ownedProducts.Add(product.Id);
                if (ProductUnlocked != null)
                {
                    ProductUnlocked(product);
                }
            }

            return true;
        }

        public void ClaimDailyReward()
        {
            dailyRewardClaimed = true;
        }

        public bool GetSetting(ShowcaseSetting setting)
        {
            bool value;
            return settings.TryGetValue(setting, out value) && value;
        }

        public void SetSetting(ShowcaseSetting setting, bool value)
        {
            settings[setting] = value;
        }

        public void ResetData()
        {
            coins = StartCoins;
            gems = StartGems;
            dailyRewardClaimed = false;
            ownedProducts.Clear();

            settings[ShowcaseSetting.Music] = true;
            settings[ShowcaseSetting.Sound] = true;
            settings[ShowcaseSetting.Vibration] = false;
            settings[ShowcaseSetting.Notifications] = true;

            if (CurrencyChanged != null)
            {
                CurrencyChanged(ShowcaseCurrency.Coins, coins);
                CurrencyChanged(ShowcaseCurrency.Gems, gems);
            }

            if (DataReset != null)
            {
                DataReset();
            }
        }
    }
}
