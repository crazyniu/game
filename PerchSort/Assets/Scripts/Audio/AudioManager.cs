using System;
using UnityEngine;
namespace PerchSort.Audio
{
    public enum Sfx { Select,Move,Collect,Win,Error,Undo,Click,Flutter,Sleep,Crate,Star1,Star2,Star3,Hint,Back }
    public enum MusicPage { None,Menu,Game }
    public sealed class AudioManager:MonoBehaviour
    {
        public static AudioManager Instance{get;private set;}
        public bool SfxEnabled{get;private set;}=true;
        public bool MusicEnabled{get;private set;}=true;
        public MusicPage CurrentMusic{get;private set;}
        public bool IsCrossfading{get;private set;}
        public const float CrossfadeSeconds=.5f;
        public static readonly string[] SoundKeys={"chirp_01","chirp_02","chirp_03","chirp_04","chirp_05","flutter","sleep_z","nest_home","crate_open","star_1","star_2","star_3","win_fanfare","hint","ui_tap","ui_back"};
        public static readonly string[] EffectKeys={"chirp","chirp","nest_home","win_fanfare","error","undo","ui_tap","flutter","sleep_z","crate_open","star_1","star_2","star_3","hint","ui_back"};
        private static readonly string[] Legacy={"select","move","collect","win","error","undo","click"};
        private readonly AudioClip[] fallback=new AudioClip[7],effects=new AudioClip[15],chirps=new AudioClip[5],music=new AudioClip[2];
        private readonly AudioSource[] voices=new AudioSource[8],songs=new AudioSource[2];
        private readonly double[] nextAllowed=new double[15];
        private int voiceIndex,lastChirp=-1,active;
        private float musicVolume=1,fadeStart,fadeFrom0,fadeFrom1;
        private bool focused=true,paused;
        internal Func<double> Clock=()=>Time.realtimeSinceStartupAsDouble;
        private void Awake()
        {
            if(Instance!=null&&Instance!=this){Destroy(gameObject);return;}Instance=this;if(Application.isPlaying)DontDestroyOnLoad(gameObject);
            for(int i=0;i<7;i++)fallback[i]=Resources.Load<AudioClip>("Audio/"+Legacy[i]);
            for(int i=0;i<5;i++)chirps[i]=Resources.Load<AudioClip>($"chirp_{i+1:00}");
            for(int i=0;i<15;i++){effects[i]=Resources.Load<AudioClip>(EffectKeys[i]);nextAllowed[i]=double.NegativeInfinity;}
            for(int i=0;i<8;i++)voices[i]=Source(false);
            for(int i=0;i<2;i++){songs[i]=Source(true);songs[i].volume=0;music[i]=Resources.Load<AudioClip>(i==0?"bgm_menu":"bgm_game");}
        }
        private AudioSource Source(bool loop){var s=gameObject.AddComponent<AudioSource>();s.playOnAwake=false;s.spatialBlend=0;s.volume=.14f;s.loop=loop;return s;}
        public void Play(Sfx effect)
        {
            if(!SfxEnabled)return;int index=(int)effect;double now=Clock();if(now<nextAllowed[index])return;
            AudioClip clip;
            if(effect==Sfx.Select||effect==Sfx.Move){int pick=UnityEngine.Random.Range(0,4);if(pick>=lastChirp)pick++;lastChirp=pick;clip=chirps[pick];}
            else clip=effects[index];
            // Requested compatibility fallback for the original seven synthesized effects.
            if(clip==null&&index<7)clip=fallback[index];
            if(clip==null)return;
            nextAllowed[index]=now+.05;var voice=voices[voiceIndex];voice.Stop();voice.clip=clip;voice.Play();voiceIndex=(voiceIndex+1)%voices.Length;
        }
        public void SetVolumes(float sound,float background){foreach(var voice in voices)voice.volume=.14f*sound;musicVolume=background;ApplyMusicVolumes();}
        public void SetSfxEnabled(bool enabled){SfxEnabled=enabled;foreach(var v in voices){v.mute=!enabled||!focused||paused;if(!enabled){v.Stop();v.clip=null;}}if(!enabled)for(int i=0;i<15;i++)nextAllowed[i]=double.NegativeInfinity;}
        public void SetMusicEnabled(bool enabled){MusicEnabled=enabled;foreach(var s in songs)s.mute=!enabled||!focused||paused;}
        public void PlayMusic()=>SwitchMusic(MusicPage.Menu);
        public void SwitchMusic(MusicPage page)
        {
            if(page==CurrentMusic)return;
            CurrentMusic=page;active=page==MusicPage.Game?1:0;
            fadeFrom0=songs[0].volume;fadeFrom1=songs[1].volume;fadeStart=Time.unscaledTime;IsCrossfading=true;
            songs[active].clip=music[active];if(!songs[active].isPlaying)songs[active].Play();
        }
        public void StopMusic(){CurrentMusic=MusicPage.None;IsCrossfading=false;foreach(var s in songs){s.Stop();s.clip=null;s.volume=0;}}
        private void Update()=>ApplyMusicVolumes();
        private void ApplyMusicVolumes()
        {
            if(CurrentMusic==MusicPage.None)return;
            float t=IsCrossfading?Mathf.Clamp01((Time.unscaledTime-fadeStart)/CrossfadeSeconds):1;
            songs[0].volume=Mathf.Lerp(fadeFrom0,active==0?.20f*musicVolume:0,t);
            songs[1].volume=Mathf.Lerp(fadeFrom1,active==1?.20f*musicVolume:0,t);
            if(IsCrossfading&&t>=1){IsCrossfading=false;songs[1-active].Stop();}
        }
        private void OnApplicationFocus(bool focus){focused=focus;ApplyMute();}
        private void OnApplicationPause(bool pause){paused=pause;ApplyMute();}
        private void ApplyMute(){foreach(var v in voices)v.mute=!SfxEnabled||!focused||paused;foreach(var s in songs)s.mute=!MusicEnabled||!focused||paused;}
        private void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
