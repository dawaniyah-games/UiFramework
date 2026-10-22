using System.Collections.Generic;
using System.Threading.Tasks;
using UiShowcase.App;
using UiShowcase.Components;
using UiShowcase.Displays;
using UnityEngine;

namespace UiShowcase.Elements
{
    // Leaderboard tab: a podium whose pillars rise (carrying the avatars), a ranked list that slides in row by row, and a
    // sticky "you" row. Switching Weekly / All time rebuilds the data and replays the same show effects in place.
    public class LeaderboardUiElement : ShowcaseUiElement
    {
        [SerializeField] private SegmentedControl boardTabs;
        [Tooltip("First, second and third place, in that order.")]
        [SerializeField] private List<PodiumSlotUiDisplay> podium = new List<PodiumSlotUiDisplay>();
        [SerializeField] private RectTransform podiumRoot;
        [SerializeField] private RectTransform rowContainer;
        [SerializeField] private LeaderboardRowUiDisplay rowTemplate;
        [SerializeField] private LeaderboardRowUiDisplay playerRow;
        [SerializeField] private float rowSpacing = 112f;
        [SerializeField] private int listedRows = 4;

        private static readonly float[] podiumHeights = { 1f, 0.78f, 0.62f };

        private readonly List<LeaderboardRowUiDisplay> rows = new List<LeaderboardRowUiDisplay>();
        private bool initialized;

        public override void Populate(object context = null)
        {
            base.Populate(context);

            if (!initialized)
            {
                initialized = true;
                rowTemplate.gameObject.SetActive(false);
                boardTabs.SelectionChanged += OnBoardSelected;
            }

            boardTabs.Select(0, false);
            Rebuild(true);
        }

        private void OnBoardSelected(int index)
        {
            Rebuild(index == 0);
            ShowcaseTween.Forget(Task.WhenAll(
                UiShowEffects.ReplayAsync(podiumRoot),
                UiShowEffects.ReplayAsync(rowContainer),
                UiShowEffects.ReplayAsync(playerRow)));
        }

        private void Rebuild(bool weekly)
        {
            List<ShowcaseRankEntry> entries = ShowcaseCatalog.GetLeaderboard(weekly, ShowcaseSession.Current.PlayerName);
            ShowcaseRankEntry player = null;
            List<ShowcaseRankEntry> rivals = new List<ShowcaseRankEntry>(entries.Count);

            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].IsPlayer)
                {
                    player = entries[i];
                }
                else
                {
                    rivals.Add(entries[i]);
                }
            }

            for (int i = 0; i < podium.Count && i < rivals.Count; i++)
            {
                podium[i].Bind(rivals[i], podiumHeights[Mathf.Min(i, podiumHeights.Length - 1)]);
            }

            ShowcaseDisplays.DestroyAll(rows);
            for (int i = 0; i < listedRows && podium.Count + i < rivals.Count; i++)
            {
                LeaderboardRowUiDisplay row = ShowcaseDisplays.Spawn(rowTemplate, rowContainer, new Vector2(0f, -i * rowSpacing));
                row.Bind(rivals[podium.Count + i]);
                rows.Add(row);
            }

            if (player != null)
            {
                playerRow.Bind(player);
            }
        }
    }
}
