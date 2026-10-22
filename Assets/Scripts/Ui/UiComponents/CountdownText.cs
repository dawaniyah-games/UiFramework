using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // A ticking "Ends in 02:14:33" label for limited-time offers. Restarts from startSeconds whenever it is enabled.
    [RequireComponent(typeof(Text))]
    public class CountdownText : MonoBehaviour
    {
        [SerializeField] private string prefix = "Ends in ";
        [SerializeField] private int startSeconds = 8 * 3600;

        private Text label;
        private float enabledAt;
        private int lastShown = -1;

        private void OnEnable()
        {
            label = GetComponent<Text>();
            enabledAt = Time.unscaledTime;
            lastShown = -1;
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            int remaining = Mathf.Max(0, startSeconds - Mathf.FloorToInt(Time.unscaledTime - enabledAt));
            if (remaining == lastShown)
            {
                return;
            }

            lastShown = remaining;
            int days = remaining / 86400;
            int hours = remaining / 3600 % 24;
            int minutes = remaining / 60 % 60;
            int seconds = remaining % 60;

            string time = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
            label.text = days > 0 ? prefix + days + "d " + time : prefix + time;
        }
    }
}
