using UnityEngine;
using LitMotion;

namespace PerchSort.FirstLevelRuntime
{
    public sealed class FirstLevelBird : MonoBehaviour
    {
        private SpriteRenderer body;
        private FirstLevelSpeciesArt art;
        private MotionHandle wingMotion;
        private int wingGeneration;
        public string TokenId { get; private set; }
        public int Species { get; private set; }
        public bool IsFlying { get; private set; }

        public void Initialize(string tokenId, int species, FirstLevelSpeciesArt acceptedArt, Material material)
        {
            TokenId = tokenId; Species = species; art = acceptedArt;
            var child = new GameObject("Accepted pose");
            child.transform.SetParent(transform, false);
            body = child.AddComponent<SpriteRenderer>();
            body.sharedMaterial = material;
            SetIdle();
        }

        public void SetOrder(int order) => body.sortingOrder = order;
        public void SetHomeMask(bool enabled) => body.maskInteraction = enabled ?
            SpriteMaskInteraction.VisibleOutsideMask : SpriteMaskInteraction.None;

        public void SetIdle()
        {
            StopWings(); IsFlying = false; Pose(art.Idle);
        }

        public void SetFolded()
        {
            StopWings(); IsFlying = false; Pose(art.Folded);
        }

        public void BeginWings()
        {
            if (IsFlying) return;
            StopWings(); IsFlying = true;
            int version = wingGeneration;
            // Ten phase samples traverse six real silhouettes and return smoothly.
            // LitMotion owns the cycle clock; body pixels are never distorted to fake wings.
            int[] cycle = { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1 };
            wingMotion = LMotion.Create(0f, 10f, .36f)
                .WithLoops(-1)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(value =>
                {
                    if (this == null || version != wingGeneration) return;
                    Pose(art.WingCycle[cycle[Mathf.Min(9, Mathf.FloorToInt(value))]]);
                });
        }

        public void StopWings()
        {
            wingGeneration++;
            wingMotion.TryCancel();
            IsFlying = false;
        }

        private void Pose(FirstLevelPose pose)
        {
            body.sprite = pose.Sprite;
            float scale = 2.5f * pose.Sprite.pixelsPerUnit / pose.Sprite.rect.width;
            body.transform.localScale = Vector3.one * scale;
            body.transform.localPosition = pose.RendererOffset * scale;
            body.color = Color.white; // No alpha fade conceals an unsuccessful nest mask.
        }

        private void OnDestroy() => StopWings();
    }
}
