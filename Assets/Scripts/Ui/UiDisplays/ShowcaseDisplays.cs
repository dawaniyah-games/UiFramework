using System.Collections.Generic;
using UnityEngine;

namespace UiShowcase.Displays
{
    public static class ShowcaseDisplays
    {
        // Spawns rows and cards from an inactive template kept in the scene, the pattern Populate uses for lists.
        public static T Spawn<T>(T template, Transform parent, Vector2 anchoredPosition) where T : Component
        {
            T view = Object.Instantiate(template, parent);
            view.gameObject.SetActive(true);

            RectTransform rect = view.transform as RectTransform;
            if (rect != null)
            {
                rect.anchoredPosition = anchoredPosition;
            }

            return view;
        }

        // Destroy is deferred to the end of the frame, so views are deactivated first: Populate rebuilds a list and the
        // show effects collect it in the same frame, and they must not pick up the old rows.
        public static void DestroyAll<T>(List<T> views) where T : Component
        {
            for (int i = 0; i < views.Count; i++)
            {
                if (views[i] != null)
                {
                    views[i].gameObject.SetActive(false);
                    Object.Destroy(views[i].gameObject);
                }
            }

            views.Clear();
        }

        // Grid cell position for item index, centred horizontally, rows going down from the top.
        public static Vector2 GridPosition(int index, int columns, Vector2 cellSize, Vector2 spacing)
        {
            int column = index % columns;
            int row = index / columns;
            float rowWidth = columns * cellSize.x + (columns - 1) * spacing.x;
            float x = -rowWidth * 0.5f + cellSize.x * 0.5f + column * (cellSize.x + spacing.x);
            float y = -row * (cellSize.y + spacing.y);
            return new Vector2(x, y);
        }
    }
}
