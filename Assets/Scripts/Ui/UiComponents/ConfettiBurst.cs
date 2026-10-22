using System.Collections.Generic;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // A burst of paper confetti: pieces shoot up from a point, tumble, fall under gravity and fade. Pieces are created
    // once and reused. Play() can be called again while a burst is still falling.
    [RequireComponent(typeof(RectTransform))]
    public class ConfettiBurst : MonoBehaviour
    {
        [SerializeField] private Sprite pieceSprite;
        [SerializeField] private int pieceCount = 64;
        [SerializeField] private List<Color> colors = new List<Color>();
        [SerializeField] private Vector2 pieceSize = new Vector2(20f, 32f);
        [SerializeField] private float minSpeed = 1100f;
        [SerializeField] private float maxSpeed = 2000f;
        [Tooltip("Degrees either side of straight up.")]
        [SerializeField] private float spread = 55f;
        [SerializeField] private float gravity = 2600f;
        [SerializeField] private float drag = 1.4f;
        [SerializeField] private float lifetime = 2.2f;

        private readonly List<Piece> pieces = new List<Piece>();
        private bool isPlaying;

        public void Play(Vector2 origin)
        {
            EnsurePieces();

            for (int i = 0; i < pieces.Count; i++)
            {
                Piece piece = pieces[i];
                float angle = (90f + Random.Range(-spread, spread)) * Mathf.Deg2Rad;
                float speed = Random.Range(minSpeed, maxSpeed);

                piece.Position = origin;
                piece.Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
                piece.Rotation = Random.Range(0f, 360f);
                piece.AngularVelocity = Random.Range(-540f, 540f);
                piece.FlipSpeed = Random.Range(6f, 14f);
                piece.Age = 0f;
                piece.Life = lifetime * Random.Range(0.7f, 1f);
                piece.Color = colors.Count > 0 ? colors[i % colors.Count] : Color.white;
                piece.Rect.gameObject.SetActive(true);
            }

            isPlaying = true;
        }

        private void Update()
        {
            if (!isPlaying)
            {
                return;
            }

            float deltaTime = Mathf.Min(Time.unscaledDeltaTime, Tween.MaxFrameStep);
            bool anyAlive = false;
            int count = pieces.Count;

            for (int i = 0; i < count; i++)
            {
                Piece piece = pieces[i];
                if (piece.Age >= piece.Life)
                {
                    continue;
                }

                piece.Age += deltaTime;
                piece.Velocity += new Vector2(0f, -gravity * deltaTime);
                piece.Velocity *= Mathf.Clamp01(1f - drag * deltaTime);
                piece.Position += piece.Velocity * deltaTime;
                piece.Rotation += piece.AngularVelocity * deltaTime;

                if (piece.Age >= piece.Life)
                {
                    piece.Rect.gameObject.SetActive(false);
                    continue;
                }

                anyAlive = true;
                float lifeLeft = 1f - piece.Age / piece.Life;
                Color color = piece.Color;
                color.a *= Mathf.Clamp01(lifeLeft / 0.3f);

                piece.Rect.anchoredPosition = piece.Position;
                piece.Rect.localRotation = Quaternion.Euler(0f, 0f, piece.Rotation);
                // Scaling x through a cosine reads as a paper strip flipping over.
                piece.Rect.localScale = new Vector3(Mathf.Cos(piece.Age * piece.FlipSpeed), 1f, 1f);
                piece.Image.color = color;
            }

            isPlaying = anyAlive;
        }

        private void EnsurePieces()
        {
            if (pieces.Count > 0)
            {
                return;
            }

            for (int i = 0; i < pieceCount; i++)
            {
                GameObject pieceObject = new GameObject("Confetti", typeof(RectTransform), typeof(Image));
                RectTransform rect = (RectTransform)pieceObject.transform;
                rect.SetParent(transform, false);
                rect.sizeDelta = pieceSize * Random.Range(0.75f, 1.15f);

                Image image = pieceObject.GetComponent<Image>();
                image.sprite = pieceSprite;
                image.raycastTarget = false;

                pieceObject.SetActive(false);
                pieces.Add(new Piece(rect, image));
            }
        }

        private sealed class Piece
        {
            public Piece(RectTransform rect, Image image)
            {
                Rect = rect;
                Image = image;
            }

            public RectTransform Rect { get; }
            public Image Image { get; }
            public Vector2 Position { get; set; }
            public Vector2 Velocity { get; set; }
            public float Rotation { get; set; }
            public float AngularVelocity { get; set; }
            public float FlipSpeed { get; set; }
            public float Age { get; set; }
            public float Life { get; set; }
            public Color Color { get; set; }
        }
    }
}
