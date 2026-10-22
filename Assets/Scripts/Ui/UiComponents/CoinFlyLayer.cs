using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UiFramework.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UiShowcase.Components
{
    // Flies icons (coins, gems) from any point on screen into a target, the classic "reward collected" effect.
    // Lives on a nested Canvas with a high sorting order so the pieces draw above popups. All showcase canvases render
    // with the same UI camera at the same plane distance (see UiCanvasCamera), so a world position taken from another
    // canvas lands on the same spot on screen here and can be used as-is.
    public class CoinFlyLayer : MonoBehaviour
    {
        [Tooltip("Inactive Image the pieces are copied from.")]
        [SerializeField] private Image pieceTemplate;
        [SerializeField] private float burstRadius = 170f;
        [SerializeField] private float burstDuration = 0.3f;
        [SerializeField] private float flyDuration = 0.6f;
        [SerializeField] private float pieceInterval = 0.05f;

        private readonly List<Image> pool = new List<Image>();

        // Calls onPieceArrived as each piece lands, so counters can tick up in step with the pieces.
        public Task FlyAsync(Sprite sprite, Color color, Vector3 fromWorldPosition, RectTransform target, int pieceCount, Action onPieceArrived)
        {
            if (pieceTemplate == null || target == null || pieceCount <= 0)
            {
                return Task.CompletedTask;
            }

            List<Task> flights = new List<Task>(pieceCount);
            for (int i = 0; i < pieceCount; i++)
            {
                flights.Add(FlyPieceAsync(GetPiece(sprite, color), fromWorldPosition, target, i * pieceInterval, onPieceArrived));
            }

            return Task.WhenAll(flights);
        }

        private async Task FlyPieceAsync(Image piece, Vector3 from, RectTransform target, float startDelay, Action onArrived)
        {
            RectTransform rect = piece.rectTransform;
            float canvasScale = transform.lossyScale.x;
            Vector2 scatter = UnityEngine.Random.insideUnitCircle * burstRadius * canvasScale;
            Vector3 burstPoint = from + new Vector3(scatter.x, scatter.y, 0f);
            float side = UnityEngine.Random.value < 0.5f ? -1f : 1f;

            rect.position = from;
            rect.localScale = Vector3.zero;
            piece.gameObject.SetActive(true);

            // 1. Burst out of the source with a little overshoot.
            await Tween.RunAsync(rect, burstDuration, TweenCurves.Linear, true, CancellationToken.None, null, progress =>
            {
                rect.position = Vector3.LerpUnclamped(from, burstPoint, TweenCurves.EaseOut.Evaluate(progress));
                rect.localScale = Vector3.one * ShowcaseCurves.BackOut.Evaluate(progress);
            });

            await ShowcaseTween.DelayAsync(rect, startDelay, null);

            // 2. Curve into the target, speeding up and shrinking as it lands. The target is read every frame in case
            //    it is still moving (the TopBar may be sliding in).
            Vector3 control = burstPoint + new Vector3(side * 260f, 220f, 0f) * canvasScale;
            await Tween.RunAsync(rect, flyDuration, TweenCurves.EaseIn, true, CancellationToken.None, null, eased =>
            {
                Vector3 end = target != null ? target.position : burstPoint;
                rect.position = QuadraticBezier(burstPoint, control, end, eased);
                rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.6f, eased);
            });

            if (piece != null)
            {
                piece.gameObject.SetActive(false);
            }

            // Credit the piece even if the layer was destroyed mid-flight, so no reward is lost.
            if (onArrived != null)
            {
                onArrived();
            }
        }

        private Image GetPiece(Sprite sprite, Color color)
        {
            Image piece = null;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pool[i] != null && !pool[i].gameObject.activeSelf)
                {
                    piece = pool[i];
                    break;
                }
            }

            if (piece == null)
            {
                piece = Instantiate(pieceTemplate, pieceTemplate.transform.parent);
                piece.raycastTarget = false;
                pool.Add(piece);
            }

            piece.sprite = sprite;
            piece.color = color;
            // Reserve the piece now; FlyPieceAsync activates it on its first frame anyway.
            piece.gameObject.SetActive(true);
            piece.rectTransform.localScale = Vector3.zero;
            return piece;
        }

        private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float u = 1f - t;
            return u * u * a + 2f * u * t * b + t * t * c;
        }
    }
}
