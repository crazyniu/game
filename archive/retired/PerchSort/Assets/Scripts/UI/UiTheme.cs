using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PerchSort.UI
{
    [Serializable]
    public sealed class UiTheme
    {
        public TMP_FontAsset font;
        public Sprite[] sprites, menuBirds;
        public View.BirdArt[] frontBirds;
        public Sprite panel, green, cream, wood;
        public Sprite undo, restart, pause, back, check, locked, bird, crown, perch;
        public static readonly Color Ink = new Color(90/255f,58/255f,34/255f);
        public static readonly Color Cream = new Color(1,.965f,.867f);
        public Sprite Art(string key)=>Array.Find(sprites,s=>s.name==key);
        public RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent,false); rect.anchorMin = rect.anchorMax = Vector2.one*.5f;
            rect.sizeDelta = size; rect.anchoredPosition = position; return rect;
        }
        public Image Image(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size, bool sliced = false)
        {
            var image = Rect(name,parent,position,size).gameObject.AddComponent<Image>();
            image.sprite = sprite; image.type = sliced ? UnityEngine.UI.Image.Type.Sliced : UnityEngine.UI.Image.Type.Simple;
            image.preserveAspect = !sliced; image.raycastTarget = false; return image;
        }
        public RectTransform Panel(string name, Transform parent, Vector2 position, Vector2 size, Sprite art = null)
        {
            var image = Image(name,parent,art ?? panel,position,size,true); image.raycastTarget = true; return image.rectTransform;
        }
        public TMP_Text Label(string name, Transform parent, string value, Vector2 position, Vector2 size, float fontSize, Color? color = null)
        {
            var label = Rect(name,parent,position,size).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.text = value; label.fontSize = name == "Version" ? 28 : Mathf.Max(32,fontSize);
            foreach (var pair in Strings.Tables[Strings.Locale])
                if (pair.Value == value) { label.gameObject.AddComponent<LocalizedText>().Key = pair.Key; break; }
            bool bright=color.HasValue&&color.Value!=Ink || !color.HasValue&&(fontSize>=52||name=="Caption"||name=="Completion"||name=="Total stars"||name=="Direction"||name=="Number"||name=="Chapter"||name=="Count");
            label.color=bright?Color.white:Ink;label.alignment=TextAlignmentOptions.Center;
            label.raycastTarget=false;label.textWrappingMode=TextWrappingModes.Normal;
            if(bright)
            {
                label.fontStyle=FontStyles.Bold;label.outlineColor=Ink;label.outlineWidth=.22f;
                label.fontMaterial.EnableKeyword("UNDERLAY_ON");
                label.fontMaterial.SetColor("_UnderlayColor",new Color(58/255f,36/255f,20/255f,.6f));
                label.fontMaterial.SetFloat("_UnderlayOffsetY",-.5f);
                var shadow=label.gameObject.AddComponent<Shadow>();shadow.effectDistance=new Vector2(0,-4);shadow.effectColor=new Color(58/255f,36/255f,20/255f,.6f);
            }
            return label;
        }
        public Button Round(string name, Transform parent, Sprite icon, Vector2 position, Action action,float diameter=150,string color=null)
        {
            string key=name=="Close"?"btn_round_red":color==null?"btn_round":"btn_round_"+color;
            var image=Image(name,parent,Art(key),position,Vector2.one*diameter);image.raycastTarget=true;
            var button=AttachButton(image.rectTransform,action,key+"_pressed");
            Image("Icon",image.transform,icon,Vector2.zero,Vector2.one*(diameter*.6f));return button;
        }
        public Button AttachButton(RectTransform rect, Action action, string pressed = null)
        {
            var button = rect.gameObject.AddComponent<Button>(); button.targetGraphic = rect.GetComponent<Image>();
            button.transition = Selectable.Transition.None;
            var feedback = rect.gameObject.AddComponent<ButtonFeedback>(); feedback.Pressed = pressed == null ? null : Art(pressed);
            button.onClick.AddListener(() => { if(action.Method.Name!="Back") Audio.AudioManager.Instance.Play(Audio.Sfx.Click); action(); }); return button;
        }
    }
}
