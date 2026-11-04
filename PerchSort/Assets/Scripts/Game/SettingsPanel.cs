using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.Game
{
    public sealed class SettingsPanel : MonoBehaviour
    {
        private GameRoot root;
        private GameArt art;
        private Image soundIcon;
        private Button restart, map;
        private Image dim;
        private RectTransform rig;
        private PopupMotion presentation;
        public bool IsOpen => gameObject.activeSelf;
        public void Configure(GameRoot owner, GameArt assets)
        {
            root = owner; art = assets;
            dim = gameObject.AddComponent<Image>(); dim.color = new Color(0, 0, 0, GameLayout.PanelDimAlpha); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<PopupMotion>();
            rig = GameUI.Rect(transform, "Settings hanging rig", new Vector2(GameLayout.SettingsPanel.center.x, GameLayout.SettingsPanel.y), GameLayout.SettingsPanel.size);
            rig.pivot = new Vector2(0.5f, 1);
            for (int i = 0; i < 2; i++)
            {
                string part = i == 0 ? "panel_rope_left" : "panel_rope_right";
                var rope = art.Optional(part);
                if (rope != null) GameUI.Fit(rig, part, rope, new Vector2(i == 0 ? 90 : rig.sizeDelta.x - 90, -140), new Vector2(48, 320));
                else GameArt.Warn("settings:" + part, "Missing settings rope part; using whole-board swing fallback.");
            }
            var body = GameUI.Surface(rig, "Settings panel", art, "panel_wood", new Rect(Vector2.zero, GameLayout.SettingsPanel.size)); body.raycastTarget = true;
            Rect ribbon = GameLayout.Screen("settings", "ribbon");
            GameUI.Fit(body.transform, "Settings ribbon", art.Get("ribbon_green"), ribbon.center - GameLayout.SettingsPanel.position, ribbon.size);
            GameUI.Button(body.transform, art, "Close settings", "icon_close", Local(GameLayout.CloseButton), () => presentation.Close(root.CloseSettings));
            var sound = GameUI.Button(body.transform, art, "Toggle mute", "icon_sound_on", Local(GameLayout.SoundButton), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = GameUI.Button(body.transform, art, "Restart level", "icon_replay", Local(GameLayout.RestartButton), () => presentation.Close(root.Restart));
            map = GameUI.Button(body.transform, art, "Return to map", "icon_home", Local(GameLayout.ReturnMapButton), () => presentation.Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - GameLayout.SettingsPanel.position, rect.size);
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
