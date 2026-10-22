using UiShowcase.App;
using UiShowcase.Components;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    // A podium pillar. The avatar is a child of the pillar's fill, anchored to its top, so it rises with the pillar.
    public class PodiumSlotUiDisplay : MonoBehaviour
    {
        [SerializeField] private FillBar pillar;
        [SerializeField] private Image avatar;
        [SerializeField] private Text initialsLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private CountUpText scoreCounter;

        public void Bind(ShowcaseRankEntry entry, float pillarHeight)
        {
            avatar.color = entry.AvatarColor;
            initialsLabel.text = entry.Initials;
            nameLabel.text = entry.Name.Split(' ')[0];
            scoreCounter.SetValue(entry.Score, false);
            pillar.SetValue(pillarHeight, false);
        }
    }
}
