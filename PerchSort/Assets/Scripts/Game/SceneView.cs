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
        public void SetLevel(BoardSnapshot snapshot)
        {
            BuildWorld(snapshot.World, snapshot.Variant, snapshot.Hud.Phase, snapshot.Rows);
        }
        public void SetWorld(int world, string variant = "noon", DayPhase lighting = DayPhase.Day)
            => BuildWorld(world, variant, lighting, 2);

        private void BuildWorld(int world, string variant, DayPhase lighting, int rows)
        {
            Cancel();
            background.sprite = art.Background(world, variant);
            for (int i = scenery.childCount - 1; i >= 0; i--) GameUI.Dispose(scenery.GetChild(i).gameObject);
            var trunk = Resources.Load<Sprite>("World/trunk_curved_w" + world);
            var crown = art.Get("tree_crown_w" + world);
            RectTransform leftCrown = null, rightCrown = null;
            Rect trunkBounds = GameLayout.WorldTrunk(world, false), leftLeaves = GameLayout.WorldCrown(world, false);
            if (rows >= 3) ClearFirstRow(ref trunkBounds, ref leftLeaves, false, rows);
            for (int side = 0; side < 2; side++)
            {
                bool right = side == 1;
                Rect wood = GameLayout.WorldTrunk(world, right), leaves = GameLayout.WorldCrown(world, right);
                if (rows >= 3) ClearFirstRow(ref wood, ref leaves, right, rows);
                // The supplied curved silhouette already joins the unchanged crown attachment.
                wood = trunkBounds;
                if (right) wood.x = GameLayout.DesignWidth - wood.xMax;
                var stem = GameUI.Image(scenery, "Edge trunk", trunk, wood.center, wood.size);
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
        private static void ClearFirstRow(ref Rect wood, ref Rect leaves, bool right, int rows)
        {
            float attachmentY = wood.y + 35;
            float attachmentX = right ? GameLayout.DesignWidth - 25 : 25;
            float scale = leaves.width > 400 ? 0.72f : 1;
            float crownBottom = GameLayout.RowY(0, rows) - GameLayout.BirdSize * GameLayout.BirdScale(rows) * 1.1f - 16;
            float rootOffset = (attachmentY - leaves.y) * scale;
            leaves.x = attachmentX + (leaves.x - attachmentX) * scale;
            leaves.size *= scale;
            // Clear the full first-row bird silhouette, with breathing and crown-sway headroom.
            leaves.y = Mathf.Min(leaves.y, crownBottom - leaves.height);
            wood.y = leaves.y + rootOffset - 35;
            wood.height = GameLayout.DesignHeight - wood.y + 80;
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
