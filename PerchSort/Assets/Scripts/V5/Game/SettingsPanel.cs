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
        private Image dim;
        private RectTransform rig;
        private V7PopupMotion presentation;
        public bool IsOpen => gameObject.activeSelf;
        public void Configure(V5GameRoot owner, V5Art assets)
        {
            root = owner; art = assets;
            dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, V7Layout.PanelDimAlpha); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<V7PopupMotion>();
            rig = V5UI.Rect(transform, "Settings hanging rig", new Vector2(V7Layout.SettingsPanel.center.x, V7Layout.SettingsPanel.y), V7Layout.SettingsPanel.size);
            rig.pivot = new Vector2(0.5f, 1);
            for (int i = 0; i < 2; i++)
            {
                string part = i == 0 ? "panel_rope_left" : "panel_rope_right";
                var rope = art.Optional(part);
                if (rope != null) V5UI.Fit(rig, part, rope, new Vector2(i == 0 ? 90 : rig.sizeDelta.x - 90, -140), new Vector2(48, 320));
                else V5Art.Warn("settings:" + part, "Missing settings rope part; using whole-board swing fallback.");
            }
            var body = V5UI.Surface(rig, "Settings panel", art, "panel_wood", new Rect(Vector2.zero, V7Layout.SettingsPanel.size)); body.raycastTarget = true;
            Rect ribbon = V7Layout.Screen("settings", "ribbon");
            V5UI.Fit(body.transform, "Settings ribbon", art.Get("ribbon_green"), ribbon.center - V7Layout.SettingsPanel.position, ribbon.size);
            V5UI.Button(body.transform, art, "Close settings", "icon_close", Local(V7Layout.CloseButton), () => presentation.Close(root.CloseSettings));
            var sound = V5UI.Button(body.transform, art, "Toggle mute", "icon_sound_on", Local(V7Layout.SoundButton), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = V5UI.Button(body.transform, art, "Restart level", "icon_replay", Local(V7Layout.RestartButton), () => presentation.Close(root.Restart));
            map = V5UI.Button(body.transform, art, "Return to map", "icon_home", Local(V7Layout.ReturnMapButton), () => presentation.Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - V7Layout.SettingsPanel.position, rect.size);
        public void Show()
        {
            presentation.Cancel(); transform.SetAsLastSibling(); gameObject.SetActive(true);
            restart.gameObject.SetActive(!root.IsMap); map.gameObject.SetActive(!root.IsMap); RefreshSound();
            presentation.Begin(dim); presentation.Swing(rig); presentation.ButtonsLast(rig, 0.65f);
        }
        public void Hide()
        {
            if (presentation != null) presentation.Cancel();
            gameObject.SetActive(false);
        }
        private void OnDisable() { if (presentation != null) presentation.Cancel(); }
        private void ToggleMute() { root.SetMuted(!root.Muted); RefreshSound(); }
        private void RefreshSound() => soundIcon.sprite = art.Icon(root.Muted ? "icon_sound_off" : "icon_sound_on");
    }
}
