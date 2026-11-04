using System;
using UnityEngine;
using UnityEngine.UI;
namespace PerchSort.UI
{
    public sealed class SettingsPanel:MonoBehaviour
    {
        private GameController game;private UiTheme theme;private Image soundIcon;
        public void Build(UiTheme style,GameController controller,bool paused,Action close,Action home)
        {
            game=controller;theme=style;
            theme.Round("Close",transform,theme.Art("icon_close"),new Vector2(paused?315:215,paused?315:165),close);
            if(paused)
            {
                theme.Round("resume",transform,theme.Art("icon_play"),new Vector2(0,130),close,220,"green");
                theme.Round("Restart",transform,theme.restart,new Vector2(-220,-170),game.Hud.RestartFromPause);
                theme.Round("menu",transform,theme.Art("icon_home"),new Vector2(0,-170),home);
            }
            var sound=theme.Round("Sound",transform,theme.Art("icon_sound"),paused?new Vector2(220,-170):Vector2.zero,()=>game.Hud.ToggleSound(),paused?150:200);
            soundIcon=sound.transform.Find("Icon").GetComponent<Image>();
        }
        public void Refresh()=>soundIcon.sprite=theme.Art(game.Settings.soundEnabled?"icon_sound":"icon_sound_off");
    }
}
