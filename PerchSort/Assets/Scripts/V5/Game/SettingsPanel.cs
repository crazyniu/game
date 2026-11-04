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
            var dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha); dim.raycastTarget = true;
            var body = V5UI.Surface(transform, "Settings panel", art, "panel_wood", V7Layout.SettingsPanel); body.raycastTarget = true;
            Rect ribbon = V7Layout.Screen("settings", "ribbon");
            V5UI.Fit(body.transform, "Settings ribbon", art.Get("ribbon_green"), ribbon.center - V7Layout.SettingsPanel.position, ribbon.size);
            V5UI.Button(body.transform, art, "Close settings", "icon_close", Local(V7Layout.CloseButton), root.CloseSettings);
            var sound = V5UI.Button(body.transform, art, "Toggle mute", "icon_sound_on", Local(V7Layout.SoundButton), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = V5UI.Button(body.transform, art, "Restart level", "icon_replay", Local(V7Layout.RestartButton), root.Restart);
            map = V5UI.Button(body.transform, art, "Return to map", "icon_home", Local(V7Layout.ReturnMapButton), root.OpenMap);
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - V7Layout.SettingsPanel.position, rect.size);
        public void Show()
        {
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            restart.gameObject.SetActive(!root.IsMap); map.gameObject.SetActive(!root.IsMap); RefreshSound();
        }
        public void Hide() => gameObject.SetActive(false);
        private void ToggleMute() { root.SetMuted(!root.Muted); RefreshSound(); }
        private void RefreshSound() => soundIcon.sprite = art.Icon(root.Muted ? "icon_sound_off" : "icon_sound_on");
    }
}
