using PerchSort.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Refresh observes committed state without adding input handlers to the bubble.
    public sealed class StuckHint : MonoBehaviour
    {
        private GameRoot root;
        private GameSession session;
        private BoardState state;
        private int moves, inventoryHash, changedFrame;
        private bool awaiting, visible, closing;
        private float deadline, shownAt, closingAt;
        private RectTransform panel, undo;
        private CanvasGroup fade;
        private GameArt art;

        public static void Observe(GameRoot root)
        {
            var hint = root.GetComponent<StuckHint>();
            if (hint == null) { hint = root.gameObject.AddComponent<StuckHint>(); hint.root = root; }
            hint.RefreshState();
        }

        private static int InventoryHash(BoardState board)
        {
            int hash = 17;
            for (int i = 0; i < 6; i++) hash = unchecked(hash * 31 + board.Inventory[(BoosterKind)i]);
            return hash;
        }

        private void RefreshState()
        {
            var current = root.Session;
            if (current == null) return;
            var board = current.State;
            int stock = InventoryHash(board);
            bool fresh = session != current;
            bool changed = !fresh && (state != board || moves != board.Moves || inventoryHash != stock);
            session = current; state = board; moves = board.Moves; inventoryHash = stock;
            if (fresh || changed)
            {
                Hide(); awaiting = changed && current.CanUndo;
                deadline = 0; changedFrame = Time.frameCount;
            }
            if (root.IsMap || root.HasModal || board.IsWon)
            {
                Hide(); awaiting = false; deadline = 0;
                return;
            }
            if (root.IsBusy || root.IsAirborne) { deadline = 0; return; }
            if (awaiting && deadline == 0)
            {
                if (GameSession.HasLegalMove(board)) awaiting = false;
                else deadline = Time.unscaledTime + 0.8f;
            }
        }

        private void Update()
        {
            bool tap = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
            if (tap && Time.frameCount != changedFrame) { awaiting = false; deadline = 0; Hide(); }
            if (root.Session != session || root.IsMap || root.HasModal || root.IsBusy || root.IsAirborne ||
                (session != null && session.State.IsWon))
            {
                if (visible) Hide();
                if (root.IsMap || root.HasModal || root.Session != session) { awaiting = false; deadline = 0; }
            }
            if (awaiting && deadline > 0 && Time.unscaledTime >= deadline)
            {
                awaiting = false; deadline = 0;
                if (!root.IsMap && !root.HasModal && !root.IsBusy && !root.IsAirborne &&
                    !session.State.IsWon && !GameSession.HasLegalMove(session.State)) Show();
            }
            if (panel == null) return;
            if (closing)
            {
                float t = GameMotion.Instant ? 1 : Mathf.Clamp01((Time.unscaledTime - closingAt) / 0.12f);
                panel.localScale = Vector3.one * Mathf.Lerp(1, 0.9f, t); fade.alpha = 1 - t;
                if (t >= 1) { closing = false; panel.gameObject.SetActive(false); }
            }
            else if (visible)
            {
                float elapsed = Time.unscaledTime - shownAt;
                float t = GameMotion.Instant ? 1 : Mathf.Clamp01(elapsed / 0.2f);
                float x = t - 1; float ease = 1 + 2.70158f * x * x * x + 1.70158f * x * x;
                panel.localScale = Vector3.one * Mathf.LerpUnclamped(0.9f, 1, ease);
                var position = panel.anchoredPosition;
                position.y = baseY + (GameMotion.Instant ? 0 : 6 * Mathf.Sin(elapsed * Mathf.PI));
                panel.anchoredPosition = position;
                if (undo != null) undo.localScale = Vector3.one * (GameMotion.Instant || elapsed < 0.2f ? 1 :
                    1 + 0.05f * (1 - Mathf.Cos((elapsed - 0.2f) * 2 * Mathf.PI / 0.8f)));
            }
        }

        private float baseY;
        private void Show()
        {
            if (panel == null) Build();
            var bar = root.Hud.transform.Find("Bottom bar");
            undo = (RectTransform)bar.Find("Undo");
            float top = -undo.anchoredPosition.y - undo.rect.height * 0.5f;
            baseY = -(top - 30 - 85);
            panel.anchoredPosition = new Vector2(540, baseY);
            var tail = (RectTransform)panel.Find("Tail");
            float tailX = Mathf.Clamp(undo.anchoredPosition.x - 330, 30, 390);
            tail.anchoredPosition = new Vector2(tailX, -170);
            tail.GetComponent<StuckHintTail>().PointX = undo.anchoredPosition.x - 330 - tailX;
            var context = panel.Find("Context booster").gameObject;
            bool hasContext;
            var kind = ViewModel.ContextBooster(session.State, out hasContext);
            context.SetActive(hasContext && root.HighestUnlocked >= ViewModel.Unlock(kind));
            context.GetComponent<Image>().sprite = art.Icon(ViewModel.Icon(kind));
            fade.alpha = 1; panel.gameObject.SetActive(true); closing = false; visible = true; shownAt = Time.unscaledTime;
        }

        private void Build()
        {
            art = new GameArt();
            panel = GameUI.Surface(root.Hud.transform.parent, "Stuck hint", art, "panel_wood", new Rect(0, 0, 420, 170)).rectTransform;
            panel.GetComponent<Image>().type = Image.Type.Simple;
            panel.SetSiblingIndex(root.Hud.transform.GetSiblingIndex() + 1);
            fade = panel.gameObject.AddComponent<CanvasGroup>(); fade.blocksRaycasts = false; fade.interactable = false;
            var tail = GameUI.Rect(panel, "Tail", Vector2.zero, new Vector2(42, 24));
            tail.pivot = new Vector2(0.5f, 1); tail.gameObject.AddComponent<StuckHintTail>();
            GameUI.Fit(panel, "Leaf sprig", art.Get("leaf_badge"), new Vector2(30, 25), new Vector2(68, 48));
            var disc = GameUI.Fit(panel, "Undo disc", art.Get(GameLayout.SecondaryButtonSprite), new Vector2(82, 85), Vector2.one * 72);
            GameUI.DiscGlyph(disc.transform, "Undo icon", art.Icon("icon_undo"));
            GameUI.Fit(panel, "Hint booster", art.Icon(ViewModel.Icon(BoosterKind.Firefly)), new Vector2(190, 85), Vector2.one * 56);
            GameUI.Fit(panel, "Branch booster", art.Icon(ViewModel.Icon(BoosterKind.Sapling)), new Vector2(270, 85), Vector2.one * 56);
            GameUI.Fit(panel, "Context booster", art.Icon(ViewModel.Icon(BoosterKind.OwlEye)), new Vector2(350, 85), Vector2.one * 56);
            panel.gameObject.SetActive(false);
        }

        private void Hide()
        {
            if (!visible) return;
            visible = false; closing = true; closingAt = Time.unscaledTime;
            if (undo != null) undo.localScale = Vector3.one;
            if (GameMotion.Instant && panel != null) { closing = false; panel.gameObject.SetActive(false); }
        }
        private void OnDestroy() { if (undo != null) undo.localScale = Vector3.one; art?.Dispose(); }
    }

    public sealed class StuckHintTail : MaskableGraphic
    {
        public float PointX { get; set; }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var vertex = UIVertex.simpleVert; vertex.color = new Color32(255, 239, 198, 255);
            vertex.position = new Vector3(-21, 0); mesh.AddVert(vertex);
            vertex.position = new Vector3(21, 0); mesh.AddVert(vertex);
            vertex.position = new Vector3(PointX, -24); mesh.AddVert(vertex);
            mesh.AddTriangle(0, 1, 2);
        }
        protected override void Awake() { base.Awake(); raycastTarget = false; }
    }
}
