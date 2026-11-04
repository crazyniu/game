using UnityEngine;
using UnityEngine.Rendering;

namespace PerchSort.View
{
    [System.Serializable]
    public sealed class BirdArt
    {
        public Sprite stand, blink, sleep, fly;
    }

    public sealed class BirdView : MonoBehaviour
    {
        public int Species { get; private set; }
        public Vector3 RestScale { get; private set; }
        public bool IsAsleep { get; private set; }
        public bool IsSelected { get; private set; }
        public bool IsBlinking => !flying && !IsAsleep && blinkUntil > Time.unscaledTime;
        public float NextBlinkAt { get; private set; }
        private SpriteRenderer sprite, outline, star;
        private BirdArt art;
        private bool flying;
        private SortingGroup sorting;
        private int slot;
        private float blinkUntil;
        private static Material silhouette;

        public void Initialize(int species, BirdArt sprites, float canvasWidth)
        {
            Species = species; art = sprites;
            sorting = gameObject.AddComponent<SortingGroup>(); sorting.sortingOrder = 10;
            sprite = ViewArt.Sprite("Art", transform, art.stand, Vector3.zero, Vector3.one, 10);
            RestScale = Vector3.one * (canvasWidth / art.stand.bounds.size.x);
            outline = ViewArt.Sprite("Selection outline", transform, art.stand, Vector3.zero, Vector3.one * 1.09f, 9);
            if (silhouette == null) silhouette = new Material(Shader.Find("GUI/Text Shader"));
            outline.sharedMaterial = silhouette;
            star = ViewArt.Sprite("Selection star", transform, ViewArt.Get("fx/sparkle"), new Vector3(0,.8f / RestScale.x,0), Vector3.one * (.16f / RestScale.x), 32);
            NextBlinkAt = Time.unscaledTime + Random.Range(3f,6f);
            Select(false); Sleep(false);
        }
        public void SetCanvasWidth(float width)
        {
            RestScale = Vector3.one * (width / art.stand.bounds.size.x);
            transform.localScale = RestScale;
        }
        public void Select(bool selected) { IsSelected = selected; outline.enabled = star.enabled = selected; }
        public void SetSlot(int value) { slot = value; sorting.sortingOrder = (flying ? 30 : 10) + slot; }
        public void Sleep(bool asleep)
        {
            IsAsleep = asleep;
            SetPose();
            sprite.color = Color.white;
            transform.localScale = Vector3.Scale(RestScale, asleep ? new Vector3(1.03f,.87f,1) : Vector3.one);
        }
        public void Flying(bool value)
        {
            flying = value; sorting.sortingOrder = (flying ? 30 : 10) + slot;
            SetPose();
        }
        private void SetPose()
        {
            sprite.sprite = outline.sprite = flying ? art.fly : IsAsleep ? art.sleep : IsBlinking ? art.blink : art.stand;
        }
        private void Update()
        {
            if (!flying && !IsAsleep && Time.unscaledTime >= NextBlinkAt)
            {
                blinkUntil = Time.unscaledTime + .12f;
                NextBlinkAt = Time.unscaledTime + Random.Range(3f,6f);
            }
            SetPose();
            if (IsSelected) outline.color = new Color(1,1,1,.65f+.35f*Mathf.Sin(Time.unscaledTime*8));
        }
        public void Fade(float alpha) { var color = sprite.color; color.a = alpha; sprite.color = color; }
    }
}
