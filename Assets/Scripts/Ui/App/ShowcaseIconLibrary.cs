using System.Collections.Generic;
using UnityEngine;

namespace UiShowcase.App
{
    // Append new icons at the end: the library stores its sprites in this order.
    public enum ShowcaseIcon
    {
        Home = 0,
        Store = 1,
        Trophy = 2,
        Profile = 3,
        Coin = 4,
        Gem = 5,
        Star = 6,
        Gear = 7,
        Close = 8,
        Check = 9,
        Lock = 10,
        Bolt = 11,
        Crown = 12,
        Gift = 13,
        Play = 14,
        Plus = 15,
        Bell = 16,
        Music = 17,
        Sound = 18,
        Sword = 19,
        Shield = 20,
        Heart = 21,
        Flame = 22,
        Chevron = 23
    }

    // Icon sprites for views that pick their icon from data (store items, quests, achievements).
    // One sprite per ShowcaseIcon, in enum order (Assets/Art/Showcase/Icons).
    [CreateAssetMenu(fileName = "ShowcaseIconLibrary", menuName = "UiShowcase/Icon Library")]
    public sealed class ShowcaseIconLibrary : ScriptableObject
    {
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();

        // Without a library, views keep their template icon, which looks like wrong data rather than missing setup.
        public static void WarnIfMissing(ShowcaseIconLibrary library, Object owner)
        {
            if (library == null)
            {
                Debug.LogWarning("[" + owner.GetType().Name + "] No ShowcaseIconLibrary assigned, so data-driven icons keep their template sprite. Assign Assets/Art/Showcase/ShowcaseIconLibrary.asset to its Icons field.", owner);
            }
        }

        public Sprite Get(ShowcaseIcon icon)
        {
            int index = (int)icon;
            if (index < 0 || index >= sprites.Count)
            {
                Debug.LogWarning("[ShowcaseIconLibrary] No sprite for icon '" + icon + "'. Add it to Assets/Art/Showcase/ShowcaseIconLibrary.asset at the icon's enum index.", this);
                return null;
            }

            return sprites[index];
        }
    }
}
