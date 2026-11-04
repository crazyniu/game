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
        private RectTransform particles, scenery, atmosphere;
        private V7Ambient ambient;
        private int phaseGeneration;
        private float phaseTarget;
        private CanvasGroup entranceFade;
        public void Configure(V5Art assets, Action emptyTap)
        {
            art = assets;
            background = V5UI.Image(transform, "Forest background", null, V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            background.raycastTarget = true;
            background.gameObject.AddComponent<V5EmptyClick>().Click = emptyTap;
            night = V5UI.Image(transform, "Night lighting", art.Get("night_overlay"), V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            phase = gameObject.AddComponent<V5Motion>();
            entranceFade = gameObject.AddComponent<CanvasGroup>();
            atmosphere = V5UI.Rect(transform, "World atmosphere", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            scenery = V5UI.Rect(transform, "World trees", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            particles = V5UI.Rect(transform, "Ambient particles", V7Layout.DesignSize * 0.5f, V7Layout.DesignSize);
            ambient = gameObject.AddComponent<V7Ambient>(); ambient.Configure(art, atmosphere, particles);
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
            for (int i = scenery.childCount - 1; i >= 0; i--) V5UI.Dispose(scenery.GetChild(i).gameObject);
            var trunk = art.Get("trunk_left_w" + world);
            var crown = art.Get("tree_crown_w" + world);
            RectTransform leftCrown = null, rightCrown = null;
            for (int side = 0; side < 2; side++)
            {
                bool right = side == 1;
                Rect wood = V7Layout.WorldTrunk(world, right), leaves = V7Layout.WorldCrown(world, right);
                var stem = V5UI.Image(scenery, "Edge trunk", trunk, wood.center, wood.size);
                var foliage = V5UI.Image(scenery, "Leaf crown", crown, leaves.center, leaves.size);
                if (right)
                {
                    stem.rectTransform.localScale = new Vector3(-1, 1, 1);
                    foliage.rectTransform.localScale = new Vector3(-1, 1, 1);
                }
                if (right) rightCrown = foliage.rectTransform; else leftCrown = foliage.rectTransform;
            }
            phaseTarget = lighting == DayPhase.Night ? 1 : 0;
            night.color = new Color(1, 1, 1, phaseTarget);
            ambient.SetWorld(world, variant, lighting == DayPhase.Night, leftCrown, rightCrown);
        }
        public void Entrance(float alpha)
        {
            background.color = new Color(1, 1, 1, alpha); entranceFade.alpha = alpha;
        }
        public void ChangePhase(DayPhase value, Action complete)
        {
            int token = ++phaseGeneration;
            phase.CancelAll(); float start = night.color.a;
            phaseTarget = value == DayPhase.Night ? 1 : 0;
            ambient.SetNight(value == DayPhase.Night);
            phase.Play(0.6f, t =>
            {
                if (token == phaseGeneration) night.color = new Color(1, 1, 1, Mathf.Lerp(start, phaseTarget, t));
            }, () =>
            {
                if (token != phaseGeneration) return;
                phaseGeneration++; night.color = new Color(1, 1, 1, phaseTarget); complete?.Invoke();
            });
        }
        public void Cancel()
        {
            phaseGeneration++;
            if (phase != null) phase.CancelAll();
            if (ambient != null) ambient.Cancel();
            if (night != null) night.color = new Color(1, 1, 1, phaseTarget);
            if (entranceFade != null) Entrance(1);
        }
        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
