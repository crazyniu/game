using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.V5.Game
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        private V5GameRoot root;
        private V5Art art;
        private Image soundIcon;
        private Button restart, map;
        public bool IsOpen => gameObject.activeSelf;
        public void Configure(V5GameRoot owner, V5Art assets)
        {
            root = owner; art = assets;
            var dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V6Layout.PanelDimAlpha); dim.raycastTarget = true;
            var body = V5UI.Surface(transform, "Settings panel", art, "ui_panel_cream", V6Layout.SettingsPanel); body.raycastTarget = true;
            V5UI.Button(body.transform, art, "Close settings", "ui_close_x", Local(V6Layout.CloseButton), root.CloseSettings);
            var sound = V5UI.Button(body.transform, art, "Toggle mute", "ico_sound_on", Local(V6Layout.SoundButton), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = V5UI.Button(body.transform, art, "Restart level", "ico_replay", Local(V6Layout.RestartButton), root.Restart);
            map = V5UI.Button(body.transform, art, "Return to map", "ico_home", Local(V6Layout.ReturnMapButton), root.OpenMap);
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - V6Layout.SettingsPanel.position, rect.size);
        public void Show()
        {
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            restart.gameObject.SetActive(!root.IsMap); map.gameObject.SetActive(!root.IsMap); RefreshSound();
        }
        public void Hide() => gameObject.SetActive(false);
        private void ToggleMute() { root.SetMuted(!root.Muted); RefreshSound(); }
        private void RefreshSound() => soundIcon.sprite = art.Icon(root.Muted ? "ico_sound_off" : "ico_sound_on");
    }
}
