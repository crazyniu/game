using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class Ambient : MonoBehaviour
    {
        private const int Capacity = 40;
        private enum Kind { Cloud, Mist, Fall, Rain, Aurora, Firefly }
        private struct Mote
        {
            public Image Image;
            public Kind Kind;
            public Vector2 Origin;
            public float Period, Offset, Alpha, Drift;
            public Color Tint;
        }
        private readonly Mote[] motes = new Mote[Capacity];
        private readonly RectTransform[] crowns = new RectTransform[2];
        private readonly Quaternion[] crownRest = new Quaternion[2];
        private readonly Vector3[] crownScale = new Vector3[2];
        private GameArt art;
        private RectTransform sky, particles;
        private GameMotion motion;
        private int count;
        private bool dusk, night;

        public void Configure(GameArt assets, RectTransform skyLayer, RectTransform particleLayer)
        {
            art = assets; sky = skyLayer; particles = particleLayer;
            motion = gameObject.AddComponent<GameMotion>();
        }

        public void SetWorld(int world, string variant, bool isNight, RectTransform left, RectTransform right)
        {
            Cancel(); night = isNight;
            dusk = variant == "dusk" || variant == "lanterns" || variant == "sunset" || variant == "bluehour" || variant == "aurora";
            crowns[0] = left; crowns[1] = right;
            for (int i = 0; i < crowns.Length; i++)
            {
                crownRest[i] = crowns[i].localRotation; crownScale[i] = crowns[i].localScale;
            }
            for (int i = 0; i < 3; i++)
                Add(Kind.Cloud, "cloud_soft_" + (i + 1), new Vector2(i * 420 - 80, 280 + i * 145),
                    new Vector2(420 + i * 65, 190), 48 + i * 12, i * 0.29f, 0.13f, 0);
            if (world == 2) for (int i = 0; i < 2; i++)
                Add(Kind.Mist, "cloud_soft_2", new Vector2(240 + i * 560, 1280 + i * 330),
                    new Vector2(1250, 260), 36, i * 0.5f, 0.12f, 90);
            if (world == 4) for (int i = 0; i < 3; i++)
                Add(Kind.Aurora, "glow_soft", new Vector2(260 + i * 280, 300 + i * 80),
                    new Vector2(1100, 230), 12, i / 3f, variant == "aurora" ? 0.14f : 0, 26,
                    i == 1 ? new Color(0.65f, 0.55f, 1) : new Color(0.45f, 1, 0.82f));
            string sprite = "leaf_fall_w" + world;
            for (int i = 0; i < 12; i++)
                Add(Kind.Fall, sprite, new Vector2((i * 97 + 31) % 1080, 0),
                    Vector2.one * (world == 4 ? 18 : 22), world == 3 && variant == "windy" ? 7 : 12,
                    i * 0.137f, world == 4 ? 0.55f : 0.48f, world == 3 ? 65 : 36);
            if (ViewModel.Rain(variant)) for (int i = 0; i < 16; i++)
                Add(Kind.Rain, "raindrop", new Vector2((i * 139 + 47) % 1080, 0),
                    new Vector2(9, 26), 1.6f, i * 0.113f, 0.28f, 8);
            for (int i = 0; i < 6; i++)
                Add(Kind.Firefly, "firefly", new Vector2(90 + i * 177, 700 + (i * 173) % 900),
                    Vector2.one * 18, 6, i * 0.157f, 0.6f, 32);
            Tick(0);
            motion.Loop(5040, t => Tick(t * 5040));
        }

        private void Add(Kind kind, string sprite, Vector2 origin, Vector2 size, float period,
            float offset, float alpha, float drift, Color? tint = null)
        {
            int slot = count++;
            Image image = motes[slot].Image;
            Transform parent = kind == Kind.Cloud || kind == Kind.Mist || kind == Kind.Aurora ? sky : particles;
            if (image == null) image = GameUI.Image(parent, "Ambient " + kind, art.Get(sprite), origin, size);
            else
            {
                image.rectTransform.SetParent(parent, false); image.sprite = art.Get(sprite);
                image.rectTransform.sizeDelta = size; image.gameObject.SetActive(true);
            }
            image.rectTransform.localScale = Vector3.one;
            image.rectTransform.localRotation = Quaternion.identity;
            image.raycastTarget = false;
            motes[slot] = new Mote { Image = image, Kind = kind, Origin = origin, Period = period,
                Offset = offset, Alpha = alpha, Drift = drift, Tint = tint ?? Color.white };
        }

        private void Tick(float seconds)
        {
            for (int i = 0; i < crowns.Length; i++)
            {
                if (crowns[i] == null) continue;
                float sway = Mathf.Sin(seconds * Mathf.PI * 2 / 8 + i * Mathf.PI * 0.5f);
                crowns[i].localRotation = crownRest[i] * Quaternion.Euler(0, 0, sway * 0.65f);
                crowns[i].localScale = Vector3.Scale(crownScale[i], new Vector3(1 + sway * 0.002f, 1, 1));
            }
            for (int i = 0; i < count; i++)
            {
                ref Mote mote = ref motes[i];
                float u = Mathf.Repeat(seconds / mote.Period + mote.Offset, 1);
                float wave = Mathf.Sin(u * Mathf.PI * 2);
                Vector2 point = mote.Origin; float alpha = mote.Alpha;
                switch (mote.Kind)
                {
                    case Kind.Cloud:
                        point.x = -500 + 2080 * u;
                        alpha *= Mathf.Min(1, Mathf.Min(u, 1 - u) * 10);
                        break;
                    case Kind.Mist:
                        point.x += wave * mote.Drift;
                        alpha *= 0.75f + 0.25f * Mathf.Cos(u * Mathf.PI * 2);
                        break;
                    case Kind.Aurora:
                        point.x += wave * mote.Drift;
                        alpha *= 0.65f + 0.35f * wave;
                        mote.Image.rectTransform.localRotation = Quaternion.Euler(0, 0, 12 + wave * 4);
                        break;
                    case Kind.Fall:
                    case Kind.Rain:
                        point += new Vector2(wave * mote.Drift, -40 + 2000 * u);
                        alpha *= Mathf.Min(1, Mathf.Min(u, 1 - u) * 12);
                        if (mote.Kind == Kind.Fall)
                            mote.Image.rectTransform.localRotation = Quaternion.Euler(0, 0, wave * 70 + i * 23);
                        break;
                    case Kind.Firefly:
                        point += new Vector2(wave * mote.Drift, Mathf.Cos(u * Mathf.PI * 2) * 20);
                        alpha *= dusk || night ? 0.25f + 0.75f * wave * wave : 0;
                        break;
                }
                mote.Image.rectTransform.anchoredPosition = new Vector2(point.x, -point.y);
                mote.Image.color = new Color(mote.Tint.r, mote.Tint.g, mote.Tint.b, alpha);
            }
        }

        public void SetNight(bool value) => night = value;

        public void Cancel()
        {
            if (motion != null) motion.CancelAll();
            for (int i = 0; i < crowns.Length; i++)
            {
                if (crowns[i] != null) { crowns[i].localRotation = crownRest[i]; crowns[i].localScale = crownScale[i]; }
                crowns[i] = null;
            }
            for (int i = 0; i < count; i++) if (motes[i].Image != null)
            {
                var rect = motes[i].Image.rectTransform;
                rect.anchoredPosition = new Vector2(motes[i].Origin.x, -motes[i].Origin.y);
                rect.localRotation = Quaternion.identity; rect.localScale = Vector3.one;
                motes[i].Image.gameObject.SetActive(false);
            }
            count = 0;
        }

        private void OnDisable() => Cancel();
        private void OnDestroy() => Cancel();
    }
}
