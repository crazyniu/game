using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    // Reuses the painted board in patches: the arch stays shallow while the cream body fills the list.
    // The foreground uses the same UVs, with a hole following the cream opening.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AviaryBoardGraphic : MaskableGraphic
    {
        private Sprite board;
        private bool foreground;
        public override Texture mainTexture => board.texture;

        public static void Create(Transform parent, Sprite sprite, bool foreground)
        {
            var rect = GameUI.Rect(parent, foreground ? "Aviary frame foreground" : "Aviary board", new Vector2(540, 960), GameLayout.DesignSize);
            var graphic = rect.gameObject.AddComponent<AviaryBoardGraphic>();
            graphic.board = sprite;
            graphic.foreground = foreground;
            graphic.raycastTarget = false;
            graphic.SetAllDirty();
        }

        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            float[] xs = { 0, 112, 776, 900 };
            float[] ys = { 0, 400, 1430, 1640 };
            for (int x = 0; x < 3; x++)
                for (int y = 0; y < 3; y++)
                    if (!foreground || x != 1 || y == 2)
                        Quad(mesh, xs[x], xs[x + 1], ys[y], ys[y], ys[y + 1], ys[y + 1]);
            if (!foreground) return;
            const int segments = 32;
            for (int i = 0; i < segments; i++)
            {
                float left = Mathf.Lerp(112, 776, i / (float)segments);
                float right = Mathf.Lerp(112, 776, (i + 1) / (float)segments);
                Quad(mesh, left, right, 0, 0, Arch(left), Arch(right));
            }
        }

        private static float Arch(float x)
        {
            float offset = (x - 444) / 332;
            return 382 - 210 * Mathf.Sqrt(Mathf.Max(0, 1 - offset * offset));
        }

        private void Quad(VertexHelper mesh, float left, float right, float topLeft, float topRight, float bottomLeft, float bottomRight)
        {
            int start = mesh.currentVertCount;
            Vertex(mesh, left, topLeft); Vertex(mesh, right, topRight);
            Vertex(mesh, right, bottomRight); Vertex(mesh, left, bottomLeft);
            mesh.AddTriangle(start, start + 1, start + 2);
            mesh.AddTriangle(start, start + 2, start + 3);
        }

        private void Vertex(VertexHelper mesh, float x, float y)
        {
            float px = x <= 112 ? x * 110 / 112 : x <= 776 ? 110 + (x - 112) * 860 / 664 : 970 + (x - 776) * 110 / 124;
            float py = y <= 400 ? 150 + y * 270 / 400 : y <= 1430 ? 420 + (y - 400) * 1370 / 1030 : 1790 + (y - 1430) * 0.75f;
            var uv = new Vector2((board.rect.x + x) / board.texture.width, (board.rect.yMax - y) / board.texture.height);
            mesh.AddVert(new Vector3(px - 540, 960 - py), color, uv);
        }
    }
}
