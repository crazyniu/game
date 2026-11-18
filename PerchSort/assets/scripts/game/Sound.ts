import {sys} from 'cc';
import { AudioSource, AudioClip, Node, resources, tween, Tween, game, Game } from 'cc';
import { Art } from './Art';
const ALIASES:Record<string,string>={collect:'reward',win:'win_fanfare',click:'ui_tap',sleep:'sleep_z',sapling:'sapling_grow',owl_eye:'star_1',morning_bell:'chirp_05',scarecrow:'star_3',spring_rain:'star_2'};
export class Sound {
  muted=false;music_muted=false;vibration=true;private current='';private active:AudioSource;private previous:AudioSource;private effect:AudioSource;private clips=new Map<string,AudioClip>();private last=new Map<string,number>();
  constructor(host:Node){this.active=host.addComponent(AudioSource);this.previous=host.addComponent(AudioSource);this.effect=host.addComponent(AudioSource);this.active.loop=this.previous.loop=true;game.on(Game.EVENT_HIDE,this.pause,this);game.on(Game.EVENT_SHOW,this.resume,this);}
  async init():Promise<void>{await Promise.all(Object.entries(Art.manifest.sounds).map(([key,path])=>new Promise<void>((resolve,reject)=>resources.load(path,AudioClip,(error,clip)=>{if(error){reject(error);return;}this.clips.set(key,clip);resolve();}))));}
  play(name:string):void{
    const pulse=name==='select'?15:name==='move'?25:name==='nest_home'?40:0;
    if(this.vibration && pulse && typeof navigator!=='undefined') navigator.vibrate?.(pulse);
    if(this.muted)return;const now=sys.now();if(now-(this.last.get(name)??-1000)<50)return;this.last.set(name,now);const key=name==='select'||name==='move'?`chirp_0${1+Math.floor(Math.random()*5)}`:ALIASES[name]??name,clip=this.clips.get(key);if(clip)this.effect.playOneShot(clip,.8);}
  music(name:string):void{if(name===this.current&&this.active.clip)return;this.current=name;if(!this.clips.has(name))return;Tween.stopAllByTarget(this.active);Tween.stopAllByTarget(this.previous);this.previous.stop();[this.active,this.previous]=[this.previous,this.active];this.active.clip=this.clips.get(name)!;this.active.volume=0;if(this.music_muted)return;this.active.play();tween(this.active).to(1,{volume:.4}).start();tween(this.previous).to(1,{volume:0}).call(()=>this.previous.stop()).start();}
  setMuted(value:boolean):void{this.muted=value;this.setMusicMuted(value);}
  set_muted(value:boolean):void{this.muted=value;}
  setMusicMuted(value:boolean):void{if(this.music_muted===value)return;this.music_muted=value;if(value){this.active.pause();this.previous.pause();}else{this.active.volume=.4;this.active.play();}}
  set_music_muted(value:boolean):void{this.setMusicMuted(value);}
  pause():void{this.active.pause();this.previous.pause();}
  resume():void{if(!this.music_muted&&this.active.clip&&!this.active.playing)this.active.play();}
  get debug():object{return {track:this.current,musicMuted:this.music_muted,muted:this.muted,playing:this.active.playing,volume:this.active.volume};}
}
