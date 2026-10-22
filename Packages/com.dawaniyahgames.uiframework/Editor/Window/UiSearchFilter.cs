using System;
using System.Collections.Generic;

namespace UiFramework.Editor.Window
{
    // Name search for the editor lists. Matches part of a name, ignoring case; several words must all match.
    // Results are ordered: names that start with the text, then names where it starts a word (e.g. "menu" in
    // "MainMenuUiState"), then any other match, keeping the original order within each group.
    public static class UiSearchFilter
    {
        private const int RankStartsWith = 0;
        private const int RankWordStart = 1;
        private const int RankContains = 2;
        private const int RankNoMatch = int.MaxValue;

        public static bool Matches(string name, string query)
        {
            return Rank(name, query) != RankNoMatch;
        }

        public static List<T> Filter<T>(IList<T> items, Func<T, string> getName, string query)
        {
            List<T> results = new List<T>();
            if (items == null)
            {
                return results;
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                results.AddRange(items);
                return results;
            }

            List<KeyValuePair<int, int>> ranked = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < items.Count; i++)
            {
                int rank = Rank(getName(items[i]), query);
                if (rank != RankNoMatch)
                {
                    ranked.Add(new KeyValuePair<int, int>(rank, i));
                }
            }

            ranked.Sort(CompareRanks);

            for (int i = 0; i < ranked.Count; i++)
            {
                results.Add(items[ranked[i].Value]);
            }

            return results;
        }

        // Lower is better; int.MaxValue means no match.
        public static int Rank(string name, string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RankContains;
            }

            if (string.IsNullOrEmpty(name))
            {
                return RankNoMatch;
            }

            string[] terms = query.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            int worst = RankStartsWith;

            for (int t = 0; t < terms.Length; t++)
            {
                int index = name.IndexOf(terms[t], StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    return RankNoMatch;
                }

                int termRank = RankContains;
                if (index == 0)
                {
                    termRank = RankStartsWith;
                }
                else if (StartsWordAt(name, terms[t]))
                {
                    termRank = RankWordStart;
                }

                if (termRank > worst)
                {
                    worst = termRank;
                }
            }

            return worst;
        }

        private static bool StartsWordAt(string name, string term)
        {
            int index = name.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            while (index > 0)
            {
                char previous = name[index - 1];
                char current = name[index];
                if (char.IsUpper(current) || !char.IsLetterOrDigit(previous))
                {
                    return true;
                }

                index = name.IndexOf(term, index + 1, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static int CompareRanks(KeyValuePair<int, int> a, KeyValuePair<int, int> b)
        {
            int byRank = a.Key.CompareTo(b.Key);
            return byRank != 0 ? byRank : a.Value.CompareTo(b.Value);
        }
    }
}
