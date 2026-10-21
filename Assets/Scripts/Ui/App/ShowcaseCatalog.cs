using System.Collections.Generic;
using UnityEngine;

namespace UiShowcase.App
{
    public enum ShowcaseCurrency
    {
        Coins = 0,
        Gems = 1
    }

    public enum ShowcaseRarity
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legendary = 3
    }

    public sealed class ShowcaseProduct
    {
        public ShowcaseProduct(string id, string name, string description, ShowcaseIcon icon, ShowcaseRarity rarity,
            ShowcaseCurrency currency, int price, int grantsCoins, bool isRepeatable)
        {
            Id = id;
            Name = name;
            Description = description;
            Icon = icon;
            Rarity = rarity;
            Currency = currency;
            Price = price;
            GrantsCoins = grantsCoins;
            IsRepeatable = isRepeatable;
        }

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }
        public ShowcaseIcon Icon { get; }
        public ShowcaseRarity Rarity { get; }
        public ShowcaseCurrency Currency { get; }
        public int Price { get; }
        public int GrantsCoins { get; }

        // Coin packs can be bought again; items are unlocked once.
        public bool IsRepeatable { get; }
    }

    public sealed class ShowcaseQuest
    {
        public ShowcaseQuest(string title, ShowcaseIcon icon, int progress, int goal, int rewardCoins)
        {
            Title = title;
            Icon = icon;
            Progress = progress;
            Goal = goal;
            RewardCoins = rewardCoins;
        }

        public string Title { get; }
        public ShowcaseIcon Icon { get; }
        public int Progress { get; }
        public int Goal { get; }
        public int RewardCoins { get; }

        public bool IsComplete
        {
            get
            {
                return Progress >= Goal;
            }
        }
    }

    public sealed class ShowcaseRankEntry
    {
        public ShowcaseRankEntry(int rank, string name, int score, Color avatarColor, bool isPlayer)
        {
            Rank = rank;
            Name = name;
            Score = score;
            AvatarColor = avatarColor;
            IsPlayer = isPlayer;
        }

        public int Rank { get; }
        public string Name { get; }
        public int Score { get; }
        public Color AvatarColor { get; }
        public bool IsPlayer { get; }

        public string Initials
        {
            get
            {
                return ShowcaseCatalog.GetInitials(Name);
            }
        }
    }

    public sealed class ShowcaseAchievement
    {
        public ShowcaseAchievement(string title, ShowcaseIcon icon, bool isUnlocked)
        {
            Title = title;
            Icon = icon;
            IsUnlocked = isUnlocked;
        }

        public string Title { get; }
        public ShowcaseIcon Icon { get; }
        public bool IsUnlocked { get; }
    }

    // Static demo content. A real game would load this from its backend; the UI only reads it.
    public static class ShowcaseCatalog
    {
        private static readonly List<ShowcaseProduct> items = new List<ShowcaseProduct>
        {
            new ShowcaseProduct("neon_blade", "Neon Blade", "Cuts through the night. +15% speed on boost pads.", ShowcaseIcon.Sword, ShowcaseRarity.Epic, ShowcaseCurrency.Coins, 1200, 0, false),
            new ShowcaseProduct("aegis", "Aegis Shield", "Shrugs off the first crash of every race.", ShowcaseIcon.Shield, ShowcaseRarity.Rare, ShowcaseCurrency.Coins, 800, 0, false),
            new ShowcaseProduct("vital", "Vital Potion", "One extra life, once per level.", ShowcaseIcon.Heart, ShowcaseRarity.Common, ShowcaseCurrency.Coins, 150, 0, false),
            new ShowcaseProduct("storm", "Storm Bolt", "Chain lightning clears the lane ahead.", ShowcaseIcon.Bolt, ShowcaseRarity.Rare, ShowcaseCurrency.Coins, 650, 0, false),
            new ShowcaseProduct("phoenix", "Phoenix Flame", "Rise again with a full boost bar.", ShowcaseIcon.Flame, ShowcaseRarity.Legendary, ShowcaseCurrency.Gems, 45, 0, false),
            new ShowcaseProduct("lucky", "Lucky Star", "Double stars for your next three races.", ShowcaseIcon.Star, ShowcaseRarity.Common, ShowcaseCurrency.Coins, 300, 0, false)
        };

        private static readonly List<ShowcaseProduct> coinPacks = new List<ShowcaseProduct>
        {
            new ShowcaseProduct("coins_s", "Pouch of Coins", "A little something for the road.", ShowcaseIcon.Coin, ShowcaseRarity.Common, ShowcaseCurrency.Gems, 10, 500, true),
            new ShowcaseProduct("coins_m", "Chest of Coins", "Most popular. Enough for a new item.", ShowcaseIcon.Coin, ShowcaseRarity.Rare, ShowcaseCurrency.Gems, 25, 1500, true),
            new ShowcaseProduct("coins_l", "Vault of Coins", "Best value. Buy the whole store.", ShowcaseIcon.Coin, ShowcaseRarity.Legendary, ShowcaseCurrency.Gems, 70, 5000, true)
        };

        private static readonly ShowcaseProduct starterBundle =
            new ShowcaseProduct("starter_bundle", "Starter Bundle", "Neon Crown skin and 500 coins. Once per account.", ShowcaseIcon.Crown, ShowcaseRarity.Legendary, ShowcaseCurrency.Gems, 30, 500, false);

        private static readonly List<ShowcaseQuest> quests = new List<ShowcaseQuest>
        {
            new ShowcaseQuest("Win 3 races", ShowcaseIcon.Trophy, 2, 3, 80),
            new ShowcaseQuest("Collect 500 stars", ShowcaseIcon.Star, 380, 500, 120),
            new ShowcaseQuest("Use a power-up", ShowcaseIcon.Bolt, 1, 1, 50)
        };

        private static readonly List<ShowcaseAchievement> achievements = new List<ShowcaseAchievement>
        {
            new ShowcaseAchievement("First win", ShowcaseIcon.Trophy, true),
            new ShowcaseAchievement("Hot streak", ShowcaseIcon.Flame, true),
            new ShowcaseAchievement("Stargazer", ShowcaseIcon.Star, true),
            new ShowcaseAchievement("Speed demon", ShowcaseIcon.Bolt, true),
            new ShowcaseAchievement("Royalty", ShowcaseIcon.Crown, false),
            new ShowcaseAchievement("Big heart", ShowcaseIcon.Heart, false)
        };

        private static readonly string[] rivalNames =
        {
            "Aria Moon", "Kai Storm", "Zane Volt", "Luna Ray", "Milo Fox", "Ivy Blaze", "Rex Nova"
        };

        private static readonly Color[] avatarColors =
        {
            ShowcasePalette.Pink, ShowcasePalette.Cyan, ShowcasePalette.Orange, ShowcasePalette.Green,
            ShowcasePalette.Primary, ShowcasePalette.Gold, ShowcasePalette.Silver
        };

        public static IReadOnlyList<ShowcaseProduct> Items
        {
            get
            {
                return items;
            }
        }

        public static IReadOnlyList<ShowcaseProduct> CoinPacks
        {
            get
            {
                return coinPacks;
            }
        }

        public static ShowcaseProduct StarterBundle
        {
            get
            {
                return starterBundle;
            }
        }

        public static IReadOnlyList<ShowcaseQuest> Quests
        {
            get
            {
                return quests;
            }
        }

        public static IReadOnlyList<ShowcaseAchievement> Achievements
        {
            get
            {
                return achievements;
            }
        }

        // Top seven rivals by score, then the player's own row (rank far below, so the sticky "You" row has a job).
        public static List<ShowcaseRankEntry> GetLeaderboard(bool weekly, string playerName)
        {
            List<ShowcaseRankEntry> entries = new List<ShowcaseRankEntry>(rivalNames.Length + 1);
            int topScore = weekly ? 48250 : 1284300;
            float falloff = weekly ? 0.86f : 0.9f;
            int offset = weekly ? 0 : 3;

            for (int i = 0; i < rivalNames.Length; i++)
            {
                int nameIndex = (i + offset) % rivalNames.Length;
                int score = Mathf.RoundToInt(topScore * Mathf.Pow(falloff, i));
                entries.Add(new ShowcaseRankEntry(i + 1, rivalNames[nameIndex], score, avatarColors[nameIndex], false));
            }

            int playerRank = weekly ? 14 : 132;
            int playerScore = weekly ? 18420 : 214900;
            entries.Add(new ShowcaseRankEntry(playerRank, playerName, playerScore, ShowcasePalette.Primary, true));
            return entries;
        }

        public static string GetInitials(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "?";
            }

            string[] parts = name.Trim().Split(' ');
            if (parts.Length == 1)
            {
                return parts[0].Substring(0, Mathf.Min(2, parts[0].Length)).ToUpperInvariant();
            }

            return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
        }
    }
}
