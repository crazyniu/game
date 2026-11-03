using System;
using PerchSort.V5.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class V5SceneView : MonoBehaviour
    {
        private V5Art art;
        private Image background, night;
        private V5Motion phase;
        private RectTransform particles;
        public void Configure(V5Art assets, Action emptyTap)
        {
            art = assets;
            background = V5UI.Image(transform, "Forest background", null, V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            background.raycastTarget = true;
            background.gameObject.AddComponent<V5EmptyClick>().Click = emptyTap;
            night = V5UI.Image(transform, "Night lighting", art.Get("night_overlay"), V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            phase = gameObject.AddComponent<V5Motion>();
            particles = V5UI.Rect(transform, "Ambient particles", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
        }
        public void FillScreen(Transform canvas)
        {
            background.rectTransform.SetParent(canvas, false);
            background.transform.SetAsFirstSibling();
            V5UI.Stretch(background.rectTransform);
            var fit = background.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectRatio = V7Layout.DesignWidth / V7Layout.DesignHeight;
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }
        public void PlaceLighting(Transform board) => night.rectTransform.SetParent(board, false);
        public void SetLevel(BoardState state)
        {
            SetWorld(state.World, state.Variant, state.Phase);
        }
        public void SetWorld(int world, string variant = "noon", DayPhase lighting = DayPhase.Day)
        {
            Cancel();
            background.sprite = art.Background(world, variant);
            night.color = new Color(1, 1, 1, lighting == DayPhase.Night ? 1 : 0);
            bool rain = V5ViewModel.Rain(variant);
            string particleName = rain ? "raindrop" : world == 2 ? "leaf_bamboo" : world == 3 ? "leaf_maple" : world == 4 ? "snowflake" : "petal";
            for (int i = 0; i < (rain ? 28 : 12); i++)
            {
                float x = (i * 97 + 31) % 1080, offset = i * 0.137f;
                var particle = V5UI.Image(particles, rain ? "Rain drop" : particleName, art.Get(particleName),
                    new Vector2(x, (i * 173) % 1920), rain ? new Vector2(12, 28) : Vector2.one * 22);
                particle.color = new Color(1, 1, 1, rain ? 0.4f : 0.65f);
                particle.gameObject.AddComponent<V5Motion>().Loop(rain ? 1.6f : 8, t =>
                {
                    float u = (t + offset) % 1;
                    particle.rectTransform.anchoredPosition = new Vector2(x + Mathf.Sin(u * Mathf.PI * 2) * (rain ? 8 : 45), 40 - 2000 * u);
                    if (!rain) particle.rectTransform.localRotation = Quaternion.Euler(0, 0, u * 240);
                });
            }
        }
        public void ChangePhase(DayPhase value, Action complete)
        {
            phase.CancelAll(); float start = night.color.a, end = value == DayPhase.Night ? 1 : 0;
            phase.Play(0.6f, t => night.color = new Color(1, 1, 1, Mathf.Lerp(start, end, t)), complete);
        }
        public void Cancel()
        {
            phase.CancelAll();
            for (int i = particles.childCount - 1; i >= 0; i--) V5UI.Dispose(particles.GetChild(i).gameObject);
        }
    }
}
