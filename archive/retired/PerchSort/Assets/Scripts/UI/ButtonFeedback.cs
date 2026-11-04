using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PerchSort.UI
{
    public sealed class ButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public static bool ReducedMotion;
        public Sprite Pressed;
        public bool KeepColor;
        public bool IsAnimating => held || animating;
        private Button button;
        private Image image;
        private Sprite normal;
        private Vector3 origin;
        private float started, from, duration;
        private bool held, animating, wasEnabled = true;
        private Graphic[] graphics;
        private static Material desaturated;
        private void Start()
        {
            button = GetComponent<Button>(); image = GetComponent<Image>(); normal = image.sprite;
            origin = transform.localScale; graphics = GetComponentsInChildren<Graphic>(true);
            if (desaturated == null) desaturated = new Material(Shader.Find("Perch/UI Desaturated"));
        }
        public void OnPointerDown(PointerEventData data)
        {
            if (!button.interactable) return;
            if (Pressed != null) image.sprite = Pressed;
            held = true; Begin(.08f);
        }
        public void OnPointerUp(PointerEventData data) { held = false; image.sprite = normal; Begin(.18f); }
        public void OnPointerExit(PointerEventData data) { if (held) OnPointerUp(data); }
        private void Begin(float seconds) { from = transform.localScale.x/origin.x; started = Time.unscaledTime; duration = seconds; animating = true; }
        private void Update()
        {
            bool enabledColor=button.interactable||KeepColor;
            if (wasEnabled != enabledColor)
            {
                wasEnabled = enabledColor;
                foreach (var graphic in graphics) if (graphic is Image) graphic.material = wasEnabled ? null : desaturated;
            }
            if (!animating) return;
            float t = Mathf.Clamp01((Time.unscaledTime-started)/duration);
            float scale = held ? Mathf.Lerp(from,.92f,t) : t < .55f ? Mathf.Lerp(from,1.06f,1-Mathf.Pow(1-t/.55f,3)) : Mathf.Lerp(1.06f,1,(t-.55f)/.45f);
            transform.localScale = origin*(ReducedMotion ? 1 : scale);
            if (t >= 1) animating = false;
        }
        private void OnDisable() { animating = held = false; if (image != null) { transform.localScale = origin; image.sprite = normal; } }
    }
    public sealed class BreathingButton:MonoBehaviour
    {
        private ButtonFeedback feedback;private void Start()=>feedback=GetComponent<ButtonFeedback>();
        private void Update(){if(!feedback.IsAnimating)transform.localScale=Vector3.one*(ButtonFeedback.ReducedMotion?1:1+.025f*(1-Mathf.Cos(Time.unscaledTime*Mathf.PI*2/1.6f)));}
    }
}
