using System;
using LitMotion;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class BirdView : MonoBehaviour
    {
        private RectTransform rect, graphicRect;
        private Image graphic;
        private Sprite idle, blink, flyUp, flyDown;
        private V5Motion motion, breathing, blinking;
        private Vector2 perch;
        private bool selected;
        public int Id { get; private set; }

        public void Configure(Bird bird, V5Art art, Vector2 point)
        {
            Id = bird.Id;
            rect = (RectTransform)transform;
            string prefix = "bird_" + bird.Species.Id;
            idle = art.Get(prefix + "_idle"); blink = art.Get(prefix + "_blink");
            flyUp = art.Get(prefix + "_fly_up"); flyDown = art.Get(prefix + "_fly_down");
            graphic = V5UI.Image(transform, prefix, idle, V5Layout.BirdDisplaySize * 0.5f, V5Layout.BirdDisplaySize);
            graphicRect = graphic.rectTransform;
            // The art contract puts the feet at y=244 in a 256px frame.
            graphicRect.pivot = new Vector2(0.5f, 12f / 256f);
            graphicRect.anchoredPosition = new Vector2(V5Layout.BirdSize / 2, -143f);
            motion = gameObject.AddComponent<V5Motion>();
            breathing = gameObject.AddComponent<V5Motion>();
            blinking = gameObject.AddComponent<V5Motion>();
            Place(point);
        }

        public void Place(Vector2 point)
        {
            Cancel();
            perch = new Vector2(point.x, -point.y);
            rect.anchoredPosition = perch;
            selected = false;
            Idle();
        }

        private void Idle()
        {
            breathing.CancelAll(); blinking.CancelAll();
            graphic.sprite = idle;
            float phase = UnityEngine.Random.value;
            breathing.Loop(1.6f, t => graphicRect.localScale = Vector3.one *
                (1 + 0.015f * (1 - Mathf.Cos((t + phase) * Mathf.PI * 2))));
            ScheduleBlink();
        }

        private void ScheduleBlink()
        {
            if (V5Motion.Instant) return;
            float wait = UnityEngine.Random.Range(2f, 5f);
            blinking.Play(wait + 0.12f, t => graphic.sprite = t * (wait + 0.12f) >= wait ? blink : idle,
                ScheduleBlink);
        }

        public void Select(bool value)
        {
            if (selected == value) return;
            selected = value;
            motion.CancelAll();
            Vector2 start = rect.anchoredPosition;
            Vector2 end = perch + (value ? Vector2.up * 40 : Vector2.zero);
            motion.Play(0.15f, t => rect.anchoredPosition = Vector2.LerpUnclamped(start, end, t), ease: Ease.OutBack);
        }

        public void Shake()
        {
            motion.CancelAll();
            selected = false;
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.25f, t =>
            {
                graphicRect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * Mathf.PI * 4) * 10);
                rect.anchoredPosition = Vector2.Lerp(start, perch, t);
            }, () => graphicRect.localRotation = Quaternion.identity);
        }

        public void Fly(Vector2 destination, float delay, bool home, Action arrived, Func<Vector2> movingDestination = null)
        {
            Cancel();
            selected = false;
            Vector2 start = rect.anchoredPosition;
            Vector2 end = new Vector2(destination.x, -destination.y);
            motion.Play(0.45f, t =>
            {
                if (movingDestination != null)
                {
                    Vector2 point = movingDestination();
                    end = new Vector2(point.x, -point.y);
                }
                Vector2 control = (start + end) * 0.5f + Vector2.up * 320;
                float inverse = 1 - t;
                rect.anchoredPosition = inverse * inverse * start + 2 * inverse * t * control + t * t * end;
                graphic.sprite = ((int)(t * 0.45f / 0.07f) & 1) == 0 ? flyUp : flyDown;
                graphicRect.localScale = Vector3.one * (home ? Mathf.Lerp(1, 0.4f, t) : 1);
            }, () =>
            {
                if (home) { gameObject.SetActive(false); arrived(); return; }
                perch = end;
                graphic.sprite = idle;
                motion.Play(0.12f, t => graphicRect.localScale = Vector3.Lerp(new Vector3(1.15f, 0.85f, 1), Vector3.one, t),
                    () => { Idle(); arrived(); });
            }, delay);
        }

        public void Cheer(Action complete)
        {
            Cancel();
            Vector2 start = rect.anchoredPosition;
            motion.Play(0.2f, t => rect.anchoredPosition = start + Vector2.up * (Mathf.Sin(t * Mathf.PI) * 40), complete);
        }

        public void Cancel()
        {
            if (motion == null) return;
            motion.CancelAll(); breathing.CancelAll(); blinking.CancelAll();
            graphicRect.localScale = Vector3.one;
            graphicRect.localRotation = Quaternion.identity;
            graphic.sprite = idle;
        }
    }
}
