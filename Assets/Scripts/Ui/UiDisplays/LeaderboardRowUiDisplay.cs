using UiShowcase.App;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Displays
{
    public class LeaderboardRowUiDisplay : MonoBehaviour
    {
        [SerializeField] private Text rankLabel;
        [SerializeField] private Image avatar;
        [SerializeField] private Text initialsLabel;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text scoreLabel;

        public void Bind(ShowcaseRankEntry entry)
        {
            rankLabel.text = "#" + entry.Rank;
            avatar.color = entry.AvatarColor;
            initialsLabel.text = entry.Initials;
            nameLabel.text = entry.IsPlayer ? entry.Name + " (you)" : entry.Name;
            scoreLabel.text = entry.Score.ToString("N0");
        }
    }
}
