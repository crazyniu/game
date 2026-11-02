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
            dim.color = new Color(0, 0, 0, V5Layout.PanelDimAlpha); dim.raycastTarget = true;
            var body = V5UI.Image(transform, "Settings panel", art.Get("panel_win"), V5Layout.SettingsPanel.center, V5Layout.SettingsPanel.size);
            body.raycastTarget = true;
            V5UI.Button(body.transform, art, "Close settings", "icon_close",
                V5Layout.InParent(V5Layout.SettingsPanel, V5Layout.CloseButton.center), root.CloseSettings);
            var sound = V5UI.Button(body.transform, art, "Toggle mute", "icon_sound_on",
                V5Layout.InParent(V5Layout.SettingsPanel, V5Layout.SoundButton.center), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            V5UI.Button(body.transform, art, "Restart level", "icon_restart",
                V5Layout.InParent(V5Layout.SettingsPanel, V5Layout.RestartButton.center), root.Restart);
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
