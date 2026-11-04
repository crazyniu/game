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
        // Include the transparent margin of panel_wood around the measured visible frame.
        private static readonly Rect PanelBounds = new Rect(144, 844, 768, 514);
        public void Configure(GameRoot owner, GameArt assets)
        {
            root = owner; art = assets;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            presentation = gameObject.AddComponent<PopupMotion>();
            rig = GameUI.Rect(transform, "Settings hanging rig", new Vector2(PanelBounds.center.x, PanelBounds.y), PanelBounds.size);
            rig.pivot = new Vector2(0.5f, 1);
            var body = GameUI.Surface(rig, "Settings panel", art, "panel_wood", new Rect(Vector2.zero, PanelBounds.size)); body.raycastTarget = true;
            Rect ribbon = GameLayout.Screen("settings", "ribbon");
            GameUI.Fit(body.transform, "Settings ribbon", art.Get("ribbon_green"), ribbon.center - PanelBounds.position, ribbon.size);
            GameUI.Button(body.transform, art, "Close settings", "icon_close", Local(GameLayout.CloseButton), () => presentation.Close(root.CloseSettings));
            var sound = GameUI.Button(body.transform, art, "Toggle mute", "icon_sound_on", Local(GameLayout.SoundButton), ToggleMute);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = GameUI.Button(body.transform, art, "Restart level", "icon_undo", Local(GameLayout.RestartButton), () => presentation.Close(root.Restart));
            map = GameUI.Button(body.transform, art, "Return to map", "icon_home", Local(GameLayout.ReturnMapButton), () => presentation.Close(root.OpenMap));
            gameObject.SetActive(false);
        }
        private static Rect Local(Rect rect) => new Rect(rect.position - PanelBounds.position, rect.size);
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
