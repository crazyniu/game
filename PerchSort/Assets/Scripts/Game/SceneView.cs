using System;
using PerchSort.Core;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class SceneView : MonoBehaviour
    {
        private GameArt art;
        private Image background, night;
        private GameMotion phase;
        private RectTransform particles, scenery, atmosphere;
        private Ambient ambient;
        private int phaseGeneration;
        private float phaseTarget;
        private CanvasGroup entranceFade;
        public void Configure(GameArt assets, Action emptyTap)
        {
            art = assets;
            background = GameUI.Image(transform, "Forest background", null, GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            background.raycastTarget = true;
            background.gameObject.AddComponent<EmptyClick>().Click = emptyTap;
            night = GameUI.Image(transform, "Night lighting", art.Get("night_overlay"), GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            phase = gameObject.AddComponent<GameMotion>();
            entranceFade = gameObject.AddComponent<CanvasGroup>();
            atmosphere = GameUI.Rect(transform, "World atmosphere", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            scenery = GameUI.Rect(transform, "World trees", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            particles = GameUI.Rect(transform, "Ambient particles", GameLayout.DesignSize * 0.5f, GameLayout.DesignSize);
            ambient = gameObject.AddComponent<Ambient>(); ambient.Configure(art, atmosphere, particles);
        }
        public void FillScreen(Transform canvas)
        {
            background.rectTransform.SetParent(canvas, false);
            background.transform.SetAsFirstSibling();
            GameUI.Stretch(background.rectTransform);
            var fit = background.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectRatio = GameLayout.DesignWidth / GameLayout.DesignHeight;
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
            for (int i = scenery.childCount - 1; i >= 0; i--) GameUI.Dispose(scenery.GetChild(i).gameObject);
            var trunk = art.Get("trunk_left_w" + world);
            var crown = art.Get("tree_crown_w" + world);
            RectTransform leftCrown = null, rightCrown = null;
            for (int side = 0; side < 2; side++)
            {
                bool right = side == 1;
                Rect wood = GameLayout.WorldTrunk(world, right), leaves = GameLayout.WorldCrown(world, right);
                var stem = GameUI.Rect(scenery, "Edge trunk", wood.center, wood.size).gameObject.AddComponent<ForestTrunkImage>();
                stem.sprite = trunk; stem.raycastTarget = false;
                var foliage = GameUI.Image(scenery, "Leaf crown", crown, leaves.center, leaves.size);
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

    // Bend and taper the existing bark texture into the crown's attachment, without new art.
    internal sealed class ForestTrunkImage : Image
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            if (sprite == null) return;
            Rect rect = GetPixelAdjustedRect();
            Vector4 uv = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            const int segments = 32;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float halfWidth = rect.width * Mathf.Lerp(0.2f, 0.425f, t);
                float bend = 25 * (1 - t) * (1 - t) - 10 * t;
                float y = Mathf.Lerp(rect.yMax, rect.yMin, t);
                float textureY = Mathf.Lerp(uv.w, uv.y, t);
                mesh.AddVert(new Vector3(bend - halfWidth, y, 0), color, new Vector2(uv.x, textureY));
                mesh.AddVert(new Vector3(bend + halfWidth, y, 0), color, new Vector2(uv.z, textureY));
                if (i == 0) continue;
                int previous = (i - 1) * 2;
                mesh.AddTriangle(previous, previous + 1, previous + 3);
                mesh.AddTriangle(previous, previous + 3, previous + 2);
            }
        }
    }
}
