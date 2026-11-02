using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Art art;
        private Image soundIcon;
        public bool IsOpen => gameObject.activeSelf;
        public void Configure(V5GameRoot owner, V5Art assets)
        {
            root = owner; art = assets;
            var dim = gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, 0.6f); dim.raycastTarget = true;
            var body = V5UI.Image(transform, "Settings panel", art.Get("panel_win"), new Vector2(540, 960), new Vector2(740, 600));
            body.raycastTarget = true;
            V5UI.Button(body.transform, art, "Close settings", "icon_close", "btn_square_wood", new Vector2(625, 105), new Vector2(110, 110), root.CloseSettings);
            var sound = V5UI.Button(body.transform, art, "Toggle mute", "icon_sound_on", "btn_square_wood", new Vector2(230, 330), new Vector2(170, 170), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            V5UI.Button(body.transform, art, "Restart level", "icon_restart", "btn_square_green", new Vector2(510, 330), new Vector2(170, 170), root.Restart);
            gameObject.SetActive(false);
        }
        public void Show()
        {
            gameObject.SetActive(true);
            RefreshSound();
        }
        public void Hide() => gameObject.SetActive(false);
        private void ToggleMute() { root.SetMuted(!V5Progress.Muted); RefreshSound(); }
        private void RefreshSound() => soundIcon.sprite = art.Get(V5Progress.Muted ? "icon_sound_off" : "icon_sound_on");
    }
}
