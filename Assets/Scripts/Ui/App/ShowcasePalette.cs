using UnityEngine;

namespace UiShowcase.App
{
    // The showcase colours. Views tint data-driven parts (rarity, currency) with these; the scenes use the same values.
    public static class ShowcasePalette
    {
        public static readonly Color Background = Hex(0x0F1020);
        public static readonly Color Surface = Hex(0x1B1D35);
        public static readonly Color SurfaceRaised = Hex(0x262A4D);
        public static readonly Color Primary = Hex(0x7C5CFF);
        public static readonly Color PrimaryDeep = Hex(0x4B34C9);
        public static readonly Color Cyan = Hex(0x2EC5FF);
        public static readonly Color Gold = Hex(0xFFC83D);
        public static readonly Color Pink = Hex(0xFF4F8B);
        public static readonly Color Green = Hex(0x34D399);
        public static readonly Color Orange = Hex(0xFF8A3D);
        public static readonly Color Silver = Hex(0xC9D1E8);
        public static readonly Color Bronze = Hex(0xE59866);
        public static readonly Color Text = Hex(0xFFFFFF);
        public static readonly Color TextMuted = Hex(0xA3A8C7);
        public static readonly Color TextDark = Hex(0x1A1530);
        public static readonly Color Backdrop = new Color(0.02f, 0.02f, 0.06f, 0.72f);

        public static Color Hex(int rgb)
        {
            return Hex(rgb, 1f);
        }

        public static Color Hex(int rgb, float alpha)
        {
            float r = ((rgb >> 16) & 0xFF) / 255f;
            float g = ((rgb >> 8) & 0xFF) / 255f;
            float b = (rgb & 0xFF) / 255f;
            return new Color(r, g, b, alpha);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color ForRarity(ShowcaseRarity rarity)
        {
            switch (rarity)
            {
                case ShowcaseRarity.Rare:
                    return Cyan;
                case ShowcaseRarity.Epic:
                    return Primary;
                case ShowcaseRarity.Legendary:
                    return Gold;
                default:
                    return Green;
            }
        }

        public static Color ForCurrency(ShowcaseCurrency currency)
        {
            return currency == ShowcaseCurrency.Gems ? Cyan : Gold;
        }
    }
}
