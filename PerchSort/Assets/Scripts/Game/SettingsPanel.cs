using System;
using LitMotion;
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
        private Image dim, panelSurface, ribbon;
        private RectTransform rig;
        private GameMotion motion;
        private CanvasGroup panelFade;
        private bool closing;
        public bool IsOpen => gameObject.activeSelf;
        // Include the artwork's transparent margin around the approximately 720 x 400 visible frame.
        private static readonly Rect PanelBounds = new Rect(156, 864, 768, 420);
        public void Configure(GameRoot owner, GameArt assets)
        {
            root = owner; art = assets;
            dim = gameObject.AddComponent<Image>(); dim.raycastTarget = true;
            ModalLayer.Configure(gameObject);
            motion = gameObject.AddComponent<GameMotion>();
            rig = GameUI.Rect(transform, "Settings panel rig", PanelBounds.center, PanelBounds.size);
            panelFade = rig.gameObject.AddComponent<CanvasGroup>();
            var body = GameUI.Surface(rig, "Settings panel", art, "panel_wood", new Rect(Vector2.zero, PanelBounds.size)); body.raycastTarget = true;
            panelSurface = body;
            ribbon = GameUI.Fit(body.transform, "Settings ribbon", art.Get("ribbon_green"), new Vector2(384, 34), new Vector2(570, 166));
            var sound = GameUI.Button(body.transform, art, "Toggle mute", "icon_sound_on", ControlBounds(0), ToggleMute);
            ((GameButton)sound).SetDisabledAppearance(false);
            soundIcon = sound.transform.GetChild(0).GetComponent<Image>();
            restart = GameUI.Button(body.transform, art, "Restart level", "icon_undo", ControlBounds(1), () => Close(root.Restart));
            map = GameUI.Button(body.transform, art, "Return to map", "icon_home", ControlBounds(2), () => Close(root.OpenMap));
            ((GameButton)restart).SetDisabledAppearance(false);
            ((GameButton)map).SetDisabledAppearance(false);
            // The close control is created last so it also wins overlapping UI raycasts.
            var close = GameUI.Button(body.transform, art, "Close settings", "icon_close", new Rect(648, -24, 120, 120), () => Close(root.CloseSettings));
            ((GameButton)close).SetDisabledAppearance(false);
            gameObject.SetActive(false);
        }
        private static Rect ControlBounds(int index) => new Rect(88 + index * 206, 124, 180, 180);
        public void Show()
        {
            motion.CancelAll(); closing = false;
            restart.interactable = map.interactable = !root.IsMap;
            panelFade.interactable = panelFade.blocksRaycasts = true;
            panelFade.alpha = 1;
            transform.SetAsLastSibling(); gameObject.SetActive(true);
            RefreshSound();
            // The scrim is never faded in: the board must already be dim on the first modal frame.
            dim.color = new Color(0, 0, 0, ModalLayer.DimAlpha);
            Pose(GameMotion.Instant ? 1 : 0);
            motion.Play(0.18f, Pose, ease: Ease.OutCubic);
        }
        private void Pose(float progress)
        {
            rig.localScale = Vector3.one * Mathf.Lerp(0.92f, 1, progress);
            // Fade the panel artwork, not the controls: all four buttons are opaque from frame one.
            panelSurface.color = ribbon.color = new Color(1, 1, 1, Mathf.Lerp(0.35f, 1, progress));
        }
        private void Close(Action complete)
        {
            if (closing) return;
            closing = true; motion.CancelAll();
            panelFade.interactable = false;
            float alpha = panelFade.alpha, scale = rig.localScale.x, shade = dim.color.a;
            motion.Play(0.12f, t =>
            {
                rig.localScale = Vector3.one * Mathf.Lerp(scale, 0.92f, t);
                panelFade.alpha = alpha * (1 - t);
                dim.color = new Color(0, 0, 0, shade * (1 - t));
            }, complete, ease: Ease.InCubic);
        }
        public void Hide()
        {
            if (motion != null) motion.CancelAll();
            closing = false;
            gameObject.SetActive(false);
        }
        private void OnDisable() { if (motion != null) motion.CancelAll(); }
        private void ToggleMute() { root.SetMuted(!root.Muted); RefreshSound(); }
        private void RefreshSound() => soundIcon.sprite = art.Icon(root.Muted ? "icon_sound_off" : "icon_sound_on");
    }
}
