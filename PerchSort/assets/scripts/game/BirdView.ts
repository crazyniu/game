import {sys} from 'cc';
import { _decorator, Component, EffectAsset, Material, Node, Sprite, UITransform, Tween, Vec4, resources, tween } from 'cc';
import { Art } from './Art';
import { UI, Rect, Point } from './UI';

const { ccclass } = _decorator;
const clamp = (v:number,a=0,b=1):number => Math.max(a,Math.min(b,v));
const smooth = (a:number,b:number,v:number):number => { const t=clamp((v-a)/(b-a));return t*t*(3-2*t); };
const lerp = (a:number,b:number,t:number):number => a+(b-a)*t;
const POSES = ['fluff','stretch','yawn','tilt_left','tilt_right','look_up','look_down','quirk1','quirk2'];
const FAMILY:Record<string,Record<string,number[]>> = {
  sunny:{idle:[104,140,165,244,0],tilt_left:[94,164,165,244,-20],tilt_right:[121,124,165,244,20]},
  sky:{idle:[215,123,163,243,0],tilt_left:[193,112,163,243,-20],tilt_right:[232,142,163,243,20],quirk2:[224,136,163,243,15],look_down:[215,140,163,243,0],preen:[198,132,179,255,8]},
};
const AURA:Record<string,string>={ember:'#ff6a28',berry:'#ff8cc4',cocoa:'#e0a868',hoot:'#ffd75a',ink:'#7a68ff',mint:'#6cf0a0',sky:'#7cc8ff',snow:'#b8e4ff',sunny:'#ffd23a'};
interface Idle { key:string;time:number;duration:number;hold:number;twice:boolean; }
interface Wear { group:Node;left:Node;right:Node;pivot:Point;frames:Record<string,number[]>; }
interface SkinParticle { node:Node;mode:string;phase:number;speed:number;seed:number;size:number; }
export interface BirdOptions { instant?:boolean;pose?:string;seed?:number;outfit?:string;code?:number;asleep?:boolean; }

@ccclass('BirdAnimator')
class BirdAnimator extends Component {
  bird!:BirdView;
  update(dt:number):void { this.bird.update(dt); }
  onDestroy():void { this.bird.release(); }
}

/** Drawn frame animation registered at the same 320px canvas and y=300 feet as the source art. */
export class BirdView {
  readonly node:Node;
  readonly species:string;
  readonly outfit:string;
  code=0;
  asleep=false;
  selected=false;
  acting=false;
  sag=0;
  private body:Node;
  private picture:Node;
  private blend:Node;
  private sprite:Sprite;
  private blendSprite:Sprite;
  private bow:Node;
  private fluff:Node;
  private zzz:Node;
  private effects:Node;
  private aura:Node|null=null;
  private particles:SkinParticle[]=[];
  private grade=0;
  private wear:Wear|null=null;
  private prefix:string;
  private rest:string;
  private frame='';
  private nextFrame='';
  private blendAmount=0;
  private sequences:Record<string,string[]>={};
  private frameNames=new Map<string,string>();
  private idle:Idle|null=null;
  private clock=0;
  private due=1;
  private lastAction='';
  private seed:number;
  private instant=false;
  private ready=false;
  private generation=0;
  private lift=0;
  private selectedAt=0;
  private kx:number;
  private ky:number;
  private bodyBase:Point;
  private animations=new Map<Tween<{t:number}>,()=>void>();
  private flightLeft=false;
  private flying=false;
  private breathPeriod:number;
  static lastIdle=0;
  private static branchFree=new Map<string,number>();

  static create(parent:Node,species:string,rect:Rect,options:BirdOptions={}):BirdView {return new BirdView(parent,species,rect,options);}
  constructor(parent:Node,species:string,rect:Rect,options:BirdOptions={}) {
    this.species=species;this.outfit=options.outfit??'';this.code=options.code??0;this.asleep=!!options.asleep&&!(this.code&1);
    this.instant=!!options.instant;this.seed=options.seed??Math.random()*6;this.due=.6+this.seed%2.4;this.breathPeriod=2.4+this.seed%.8;
    this.kx=rect[2]/320;this.ky=rect[3]/320;this.bodyBase=[rect[2]/2,rect[3]*300/320];
    this.node=UI.group(parent,`Bird_${species}`,rect);
    this.body=UI.group(this.node,'Body',[this.bodyBase[0],this.bodyBase[1],0,0]);
    this.prefix=this.outfit&&Art.has(this.outfit+'_hop_01')?this.outfit:species;
    this.rest=Art.has(this.prefix+'_front')?this.prefix+'_front':this.prefix+'_idle';
    this.buildSkin();
    const pictureRect:Rect=[-160*this.kx,-300*this.ky,320*this.kx,320*this.ky];
    this.picture=UI.image(this.body,this.code&1?'egg_idle':this.rest,pictureRect);this.sprite=this.picture.getComponent(Sprite)!;
    this.blend=UI.image(this.body,this.rest,pictureRect);this.blendSprite=this.blend.getComponent(Sprite)!;UI.opacity(this.blend,0);
    this.loadFrames();this.buildWear();
    this.bow=UI.image(this.body,'mama_bow',[0,0,72*this.kx,72*this.ky]);
    this.fluff=UI.image(this.body,'chick_fluff',[0,0,88*this.kx,88*this.ky]);
    this.zzz=UI.image(this.body,'zzz',[(90-38.4)*this.kx,(-230-38.4)*this.ky,76.8*this.kx,76.8*this.ky]);
    this.effects=UI.group(this.node,'BirdEffects');
    for(const particle of this.particles)particle.node.setSiblingIndex(this.body.children.length-1);
    this.syncOverlays();this.zzz.active=this.asleep;
    const preload=[this.rest,...Object.values(this.sequences).flat(),...POSES.map(p=>this.prefix+'_'+p).filter(p=>Art.has(p)),
      'egg_idle','egg_crack','egg_shell','egg_wobble_l','egg_wobble_r'];
    void Art.preload(preload).then(()=>{if(this.node.isValid){this.ready=true;this.show(this.restFrame());if(options.pose)this.setPose(options.pose);}});
    this.node.addComponent(BirdAnimator).bird=this;
  }
  private loadFrames():void {
    this.frameNames.set(this.rest,'front');
    for(const action of ['front_blink','turn_left','turn_right','preen','flap','hop','cheer','flight_takeoff','flight_flap','flight_land','flight_takeoff_l','flight_flap_l','flight_land_l']){
      const frames=Art.frames(this.prefix+'_'+action);if(frames.length)this.sequences[action]=frames;
      frames.forEach((name,i)=>this.frameNames.set(name,`${action}_${String(i+1).padStart(2,'0')}`));
    }
    for(const pose of POSES)if(Art.has(this.prefix+'_'+pose))this.frameNames.set(this.prefix+'_'+pose,pose);
    if(!this.outfit||this.prefix!==this.species)return;
    const table=Art.data('birds/outfit_anchors')?.[this.outfit];
    if(table){
      const bad=new Set<string>(table.repaired.map((name:string)=>name.split(':')[0]));
      for(const name of ['turn_left','turn_right']){
        const frames=this.sequences[name]??[],cut=frames.findIndex(frame=>bad.has(this.frameNames.get(frame)!));
        if(cut>=0)this.sequences[name]=frames.slice(0,cut);
      }
      for(const name of ['flap','hop']){
        const frames=this.sequences[name]??[],good=frames.filter(frame=>!bad.has(this.frameNames.get(frame)!));
        if(good.length) this.sequences[name]=frames.map((frame,i)=>bad.has(this.frameNames.get(frame)!)?frames.reduce((best,candidate,j)=>!bad.has(this.frameNames.get(candidate)!)&&Math.abs(j-i)<Math.abs(frames.indexOf(best)-i)?candidate:best,good[0]):frame);
      }
      this.sequences.preen=[];
      for(const pose of POSES)if(bad.has(pose))this.frameNames.delete(this.prefix+'_'+pose);
    }
    for(const action of ['cheer','turn_left','turn_right']){
      const painted=Art.frames(this.outfit+'_'+action);
      if(painted.length>=(action==='cheer'?1:4))this.sequences[action]=painted;
    }
  }
  private buildWear():void {
    if(!this.outfit||this.prefix!==this.species||!Art.has(this.outfit+'_front_overlay'))return;
    const table=Art.data('birds/outfit_anchors')[this.outfit],pivot:Point=table.pivot;
    const group=UI.group(this.body,'Outfit',[0,0,0,0]),name=this.outfit+'_front_overlay';
    const left=UI.image_region(group,name,[0,0,pivot[0],320],[0,0,pivot[0]*this.kx,320*this.ky]);
    const right=UI.image_region(group,name,[pivot[0],0,320-pivot[0],320],[0,0,(320-pivot[0])*this.kx,320*this.ky]);
    this.wear={group,left,right,pivot,frames:table.frames};
    if(this.outfit==='berry_outfit_5')resources.load('effects/costume_alive',EffectAsset,(error,effect)=>{
      if(error)throw error;if(!group.isValid)return;
      const material=new Material();material.initialize({effectAsset:effect,defines:{USE_TEXTURE:true}});
      material.setProperty('costume',new Vec4(.48,.56,.20,this.seed));
      left.getComponent(Sprite)!.customMaterial=material;right.getComponent(Sprite)!.customMaterial=material;
    });
  }
  private restFrame():string {if(this.code&1)return 'egg_idle';return this.asleep?(this.sequences.front_blink?.at(-1)??this.rest):this.rest;}
  private cached(name:string):ReturnType<typeof Art.cache.get> {return Art.cache.get(Art.entry(name).path);}
  private show(name:string,next='',amount=0):void {
    this.frame=name;this.nextFrame=next;this.blendAmount=next?amount:0;
    const frame=this.cached(name);if(frame)this.sprite.spriteFrame=frame;
    if(next){const blend=this.cached(next);if(blend)this.blendSprite.spriteFrame=blend;}
    UI.opacity(this.blend,this.blendAmount);this.syncOverlays();
  }
  private play(frames:string[],position:number):void {
    if(!frames.length){this.show(this.restFrame());return;}
    const index=Math.min(frames.length-1,Math.floor(position)),fraction=position-index;
    this.show(frames[index],frames[Math.min(index+1,frames.length-1)],smooth(.65,1,fraction));
  }
  private poseAnchor(name:string):number[] {const table=FAMILY[this.species];return table?.[this.frameNames.get(name)??'idle']??table?.idle??[168,51,120,165,0];}
  private syncOverlays():void {
    if(!this.bow)return;
    const egg=!!(this.code&1);this.bow.active=!!(this.code&4)&&!egg;this.fluff.active=!!(this.code&8)&&!egg;
    const from=this.poseAnchor(this.frame),to=this.poseAnchor(this.nextFrame||this.frame),p=from.map((n,i)=>lerp(n,to[i],this.blendAmount));
    UI.move(this.bow,(p[0]-160-36)*this.kx,(p[1]-300-36)*this.ky);this.bow.angle=-p[4];
    UI.move(this.fluff,(p[2]-160-44)*this.kx,(p[3]-300-44)*this.ky);
    if(this.wear){
      const wear=this.wear,fromName=this.frameNames.get(this.frame),toName=this.frameNames.get(this.nextFrame||this.frame);
      const a=fromName?wear.frames[fromName]??[0,0,1,1,1,0]:null,b=toName?wear.frames[toName]??[0,0,1,1,1,0]:null;
      wear.group.active=!egg&&!!(a||b);
      if(a||b){
        const value=a&&b?a.map((n,i)=>lerp(n,b[i],this.blendAmount)):(a??b)!;
        UI.opacity(wear.group,a&&b?1:a?1-this.blendAmount:this.blendAmount);
        UI.move(wear.group,(wear.pivot[0]+value[0]-160)*this.kx,(wear.pivot[1]+value[1]-300)*this.ky);wear.group.angle=-value[5];
        UI.place(wear.left,[-wear.pivot[0]*value[2]*this.kx,-wear.pivot[1]*value[4]*this.ky,wear.pivot[0]*value[2]*this.kx,320*value[4]*this.ky]);
        UI.place(wear.right,[0,-wear.pivot[1]*value[4]*this.ky,(320-wear.pivot[0])*value[3]*this.kx,320*value[4]*this.ky]);
      }
    }
    if(this.aura)this.aura.active=!egg&&!this.asleep;
    for(const particle of this.particles)particle.node.active=!egg&&!this.asleep;
  }
  private run(seconds:number,step:(t:number)=>void):Promise<void> {
    if(this.instant){step(1);return Promise.resolve();}
    const value={t:0};return new Promise(resolve=>{
      const animation=tween(value).to(seconds,{t:1},{onUpdate:()=>step(value.t)}).call(()=>{this.animations.delete(animation);resolve();});
      this.animations.set(animation,resolve);animation.start();
    });
  }
  release():void {
    this.generation++;for(const [animation,resolve]of this.animations){animation.stop();resolve();}this.animations.clear();
    Tween.stopAllByTarget(this.node);
  }
  stop():void {
    this.release();this.idle=null;this.flying=false;this.acting=false;this.sag=0;this.body.angle=0;
    this.body.setScale(1,1,1);UI.move(this.body,this.bodyBase[0],this.bodyBase[1]-this.lift);
    if(this.effects)UI.clear(this.effects);
  }
  destroy():void {this.stop();this.node.removeFromParent();this.node.destroy();}
  private current(generation:number):boolean{return this.node.isValid&&generation===this.generation;}
  setCode(code:number,asleep=false):void {
    const changed=code!==this.code||asleep!==this.asleep;this.code=code;this.asleep=asleep&&!(code&1);this.zzz.active=this.asleep;
    if(changed){this.idle=null;this.show(this.restFrame());}this.syncOverlays();
  }
  setSleeping(value:boolean):void {this.asleep=value&&!(this.code&1);this.zzz.active=this.asleep;this.idle=null;this.show(this.restFrame());this.syncOverlays();}
  setSelected(value:boolean):void {
    if(this.selected===value)return;
    this.stop();this.selected=value;this.selectedAt=this.clock;const generation=this.generation,from=this.lift,target=value?34*this.kx/(170/320):0;
    void this.run(.18,t=>this.lift=lerp(from,target,smooth(0,1,t))).then(()=>{if(this.current(generation)&&!value)this.show(this.restFrame());});
  }
  setPose(pose:string):void {
    if(pose==='wobble'){void this.wobble();return;}
    if(pose==='idle'||pose==='front'){this.idle=null;this.show(this.restFrame());return;}
    if(pose==='cheer'){void this.cheer();return;}
    const key=pose==='blink'?'front_blink':pose;
    if(this.sequences[key]?.length||this.frameNames.has(this.prefix+'_'+key))this.startIdle(key);
  }
  showcase():void {void this.cheer();}
  private startIdle(key:string):void {
    const hold=.6+(this.seed+this.clock)%.6,twice=key==='front_blink'&&Math.random()<.2;
    const duration=key==='front_blink'?(twice?.84:.32):key.startsWith('turn')?.84+hold:key==='preen'?1.05:key==='flap'?1:key==='hop'?.82:hold+.5;
    this.lastAction=key;this.idle={key,time:0,duration,hold,twice};
  }
  private idleFrame(idle:Idle):number {
    const {key,time,duration,hold,twice}=idle,seq=this.sequences[key]??[],ease=(u:number)=>lerp(clamp(u),smooth(0,1,u),.6);
    if(key==='front_blink'){
      let t=time;if(twice&&t>=.52)t-=.52;else if(t>=.32){this.show(this.rest);return 0;}
      this.play([this.rest,seq[0],seq[1],seq[0],this.rest],4*t/.32);return 0;
    }
    if(key.startsWith('turn')){
      const frames=[this.rest,...seq],span=frames.length-1;
      if(time<.42)this.play(frames,span*ease(time/.42));else if(time<.42+hold)this.show(frames[span]);else this.play(frames,span*(1-ease((time-.42-hold)/.42)));return 0;
    }
    if(key==='hop'){
      this.play([this.rest,...seq,this.rest],(seq.length+1)*ease(time/duration));return 16*this.ky*Math.sin(clamp((time-.14)/.42)*Math.PI);
    }
    if(key==='preen'||key==='flap'){
      const frames=key==='flap'?[this.rest,...seq,...seq,this.rest]:[this.rest,...seq,this.rest];this.play(frames,(frames.length-1)*ease(time/duration));return key==='flap'?5*this.ky*Math.sin(time/duration*Math.PI)**2:0;
    }
    if(seq.length){this.play([this.rest,...seq,this.rest],(seq.length+1)*time/duration);return 0;}
    this.show(this.rest,this.prefix+'_'+key,smooth(0,1,Math.min(time,duration-time)/.25));return 2*this.ky;
  }
  update(dt:number):void {
    if(!this.ready)return;
    if(this.instant){this.show(this.restFrame());return;}
    this.clock+=dt;this.updateSkin();
    if(this.asleep){const phase=this.clock%2.4/2.4;UI.move(this.zzz,(90-38.4)*this.kx,(-230-38.4-40*phase)*this.ky);UI.opacity(this.zzz,1-phase);}
    if(this.acting)return;
    let rise=0;
    if(this.selected){
      const left=UI.rect(this.node)[0]+UI.rect(this.node)[2]/2>540,elapsed=this.clock-this.selectedAt,flap=this.sequences[left?'flight_flap_l':'flight_flap'];
      if(flap?.length)this.show(flap[Math.floor(elapsed*18)%flap.length]);
    }else if(this.idle){
      this.idle.time+=dt;
      if(this.idle.time>=this.idle.duration){this.idle=null;this.show(this.restFrame());this.due=this.clock+1+Math.random()*2;}
      else rise=this.idleFrame(this.idle);
    }else{
      this.show(this.restFrame());
      if(!this.asleep&&!(this.code&1)&&this.clock>=this.due&&this.node.activeInHierarchy){
        const now=sys.now()/1000,key=`${this.node.parent?.uuid}:${Math.round(UI.rect(this.node)[1])}`;
        if(now-BirdView.lastIdle>=.22&&(BirdView.branchFree.get(key)??0)<=now){
          const options:[string,number][]=[['front_blink',30],['turn_left',7],['turn_right',7],['preen',10],['flap',8],['hop',6],...POSES.map(p=>[p,4] as [string,number])];
          const available=options.filter(([name])=>name!==this.lastAction&&(this.sequences[name]?.length||this.frameNames.has(this.prefix+'_'+name)));
          let pick=Math.random()*available.reduce((sum,p)=>sum+p[1],0);
          for(const [name,weight]of available){pick-=weight;if(pick<0){this.startIdle(name);BirdView.lastIdle=now;BirdView.branchFree.set(key,now+this.idle!.duration+.3);break;}}
        }
      }
    }
    const breath=.5*(1-Math.cos((this.clock/this.breathPeriod+this.seed)*Math.PI*2));
    this.body.setScale(1-.008*breath,1+.02*breath,1);UI.move(this.body,this.bodyBase[0],this.bodyBase[1]-rise-this.lift+this.sag);
  }

  async flyTo(point:Point,duration=.45,options:{home?:boolean;doorway?:Node;leap?:boolean}={}):Promise<void> {
    this.stop();this.selected=false;this.acting=true;this.flying=true;
    const generation=this.generation,rect=UI.rect(this.node),start:Point=[this.node.position.x,-this.node.position.y-this.lift];this.lift=0;
    UI.move(this.body,this.bodyBase[0],this.bodyBase[1]);this.zzz.active=false;
    const left=point[0]<start[0];this.flightLeft=left;
    const takeoff=this.sequences[left?'flight_takeoff_l':'flight_takeoff']??[],flap=this.sequences[left?'flight_flap_l':'flight_flap']??[],land=this.sequences[left?'flight_land_l':'flight_land']??[];
    const from=this.frame||this.restFrame();
    await this.run(.06,t=>{this.body.setScale(1+.08*t,1-.08*t,1);if(takeoff.length)this.show(from,takeoff[0],smooth(0,1,t));});
    if(!this.current(generation))return;
    let target:Point=[...point],entryScale=.4;
    if(options.doorway){
      const door=UI.rect(options.doorway);entryScale=Math.min(door[2],door[3])*.66/(320*this.kx);
      target=[point[0],point[1]+140*this.ky*entryScale];
    }
    const approach=duration*(options.doorway?.75:1),height=options.doorway?Math.min(160,(start[1]-target[1])*.25):options.leap?215:160;
    await this.run(approach,t=>{
      UI.move(this.node,lerp(start[0],target[0],t),lerp(start[1],target[1],t)-4*t*(1-t)*height);
      const elapsed=t*approach;
      if(flap.length){
        if(elapsed<.08&&takeoff.length)this.play([...takeoff,flap[0]],elapsed/.08*takeoff.length);
        else if(t>.82&&!options.doorway&&land.length)this.play(land,(t-.82)/.18*(land.length-1));
        else this.show(flap[Math.floor(elapsed*18)%flap.length]);
      }else this.play(takeoff,t*(takeoff.length-1));
      const vertical=target[1]-start[1]-4*(1-2*t)*height;
      this.body.angle=-clamp(Math.atan2(vertical,Math.abs(target[0]-start[0])+1),-.21,.21)*180/Math.PI*(left?-1:1)*(options.doorway?1-smooth(.7,1,t):1);
      const shrink=options.home?lerp(1,entryScale,options.doorway?smooth(.45,1,t):t):1;this.body.setScale(shrink,shrink,1);
    });
    if(!this.current(generation))return;
    this.body.angle=0;
    if(options.doorway){
      const door=options.doorway,box=UI.rect(door);this.node.setParent(door);
      await this.run(duration-approach,t=>{
        const shrink=entryScale*lerp(1,.15,smooth(0,1,t));this.body.setScale(shrink,shrink,1);
        UI.place(this.node,[box[2]/2-160*this.kx,box[3]/2+140*this.ky*shrink-300*this.ky,rect[2],rect[3]]);
        if(flap.length)this.show(flap[Math.floor((approach+t*(duration-approach))*18)%flap.length]);UI.opacity(this.node,1-smooth(.6,1,t));
      });
      if(!this.current(generation))return;
    }
    if(options.home){this.acting=false;this.flying=false;this.node.active=false;return;}
    const last=this.frame;
    await this.run(.1,t=>{const k=1-smooth(0,1,t);this.body.setScale(1+.15*k,1-.15*k,1);this.show(last,this.restFrame(),smooth(.1,.9,t));});
    if(!this.current(generation))return;
    UI.place(this.node,[point[0],point[1],rect[2],rect[3]]);this.flying=false;this.acting=false;this.show(this.restFrame());this.due=this.clock+1+Math.random()*2;
  }
  async hopTo(point:Point,duration=.4):Promise<void> {
    this.stop();this.acting=true;const generation=this.generation,rect=UI.rect(this.node),from:Point=[this.node.position.x,-this.node.position.y];
    const frames=[this.rest,...(this.sequences.hop??[]),this.rest];
    await this.run(Math.max(duration,(frames.length-2)/10),t=>{
      const u=smooth(0,1,t);UI.move(this.node,lerp(from[0],point[0],u),lerp(from[1],point[1],u)-(frames.length>2?60*Math.sin(t*Math.PI):0));
      this.play(frames,t*(frames.length-1));
    });
    if(!this.current(generation))return;
    this.acting=false;this.show(this.restFrame());UI.place(this.node,[point[0],point[1],rect[2],rect[3]]);
  }
  async cheer():Promise<void> {
    if(this.acting||this.selected||this.asleep||this.code&1)return;
    const frames=this.sequences.cheer;if(!frames?.length)return;
    this.stop();this.acting=true;const generation=this.generation;
    await this.run((frames.length+1)/10,t=>this.play([this.rest,...frames,this.rest],(frames.length+1)*lerp(t,smooth(0,1,t),.6)));
    if(this.current(generation)){this.acting=false;this.show(this.restFrame());this.due=this.clock+1;}
  }
  async hatch():Promise<void> {
    this.stop();this.acting=true;const generation=this.generation;
    this.code&=~(1|16);this.asleep=false;
    await this.run(.35,t=>{
      this.show(t<.3?'egg_crack':t<.5?'egg_shell':this.rest);
      UI.move(this.body,this.bodyBase[0],this.bodyBase[1]-45*Math.sin(t*Math.PI));
      const wave=Math.sin(t*Math.PI*2);this.body.setScale(1+.12*wave,1-.12*wave,1);
    });
    if(this.current(generation)){this.acting=false;this.show(this.rest);UI.move(this.body,...this.bodyBase);this.body.setScale(1,1,1);}
  }
  async wobble():Promise<void> {
    if(!(this.code&1))return;
    this.stop();this.acting=true;const generation=this.generation;
    await this.run(.3,t=>this.show(t<.35?'egg_wobble_l':t<.75?'egg_wobble_r':'egg_idle'));
    if(this.current(generation)){this.acting=false;this.show(this.restFrame());}
  }
  async reveal():Promise<void> {
    this.code|=16;const generation=this.generation;
    await this.run(.5,t=>{const glow=Math.sin(t*Math.PI);UI.tint(this.picture,`#ffff${Math.round(255-35*glow).toString(16).padStart(2,'0')}`);});
    if(this.current(generation))UI.tint(this.picture,'#ffffff');
  }
  async wake():Promise<void> {
    this.stop();this.acting=true;this.asleep=false;this.code&=~2;this.zzz.active=false;
    const generation=this.generation,closed=this.frame||this.rest;this.ringBell();
    await this.run(.55,t=>{
      const crouch=t<.18?1-smooth(0,.18,t):0,air=Math.sin(clamp((t-.15)/.6)*Math.PI);
      UI.move(this.body,this.bodyBase[0],this.bodyBase[1]-46*air*this.kx/(170/320));
      this.body.setScale(1+.12*crouch-.05*air,1-.12*crouch+.12*air,1);this.show(closed,this.rest,smooth(.15,.5,t));
    });
    if(!this.current(generation))return;
    await this.run(.25,t=>{const k=1-smooth(0,1,t);this.body.setScale(1+.12*k,1-.12*k,1);});
    if(this.current(generation)){this.acting=false;this.show(this.rest);UI.move(this.body,...this.bodyBase);this.syncOverlays();}
  }
  private ringBell():void {
    const generation=this.generation,side=84*this.kx/(170/320),x=this.bodyBase[0]-side/2,y=this.bodyBase[1]-320*this.ky-side*.7;
    const bell=UI.image(this.effects,'island_icon_bell',[x,y,side,side],true);
    void this.run(.9,t=>{bell.angle=Math.sin(t*Math.PI*8)*28*(1-t);const scale=Math.min(1,t/.16);bell.setScale(scale,scale,1);UI.move(bell,x,y-40*t);UI.opacity(bell,1-smooth(.65,1,t));})
      .then(()=>{if(this.current(generation)){bell.removeFromParent();bell.destroy();}});
    for(let i=0;i<6;i++){
      const spark=UI.image(this.effects,'pic_sparkle',[x+side/2,y+side*.5,28*this.kx/(170/320),28*this.ky/(170/320)]);
      void this.run(.65,t=>{UI.move(spark,x+side/2+(i%2?1:-1)*90*t,y+side*.5+(-.4+Math.floor(i/2)*.25)*90*t);UI.opacity(spark,1-t);})
        .then(()=>{if(this.current(generation)){spark.removeFromParent();spark.destroy();}});
    }
  }
  async nudgeAsleep():Promise<void> {
    if(!this.asleep||this.acting)return;
    this.stop();this.acting=true;const generation=this.generation,closed=this.restFrame(),half=this.sequences.front_blink?.[0]??closed;
    await this.run(.7,t=>{
      const pulse=Math.sin(t*Math.PI);UI.move(this.body,this.bodyBase[0],this.bodyBase[1]-14*pulse*this.kx/(170/320));
      this.show(t>.2&&t<.6?half:closed);this.zzz.setScale(1+.7*pulse,1+.7*pulse,1);UI.opacity(this.zzz,1);
    });
    if(this.current(generation)){this.acting=false;this.show(closed);this.zzz.setScale(1,1,1);}
  }
  private buildSkin():void {
    this.grade=Number(this.outfit.split('_outfit_')[1]??0);if(this.grade<3||this.outfit==='berry_outfit_5')return;
    const color=AURA[this.species]??'#ffd23a';
    this.aura=UI.image(this.body,'glow_soft',[-160*this.kx,-285*this.ky,320*this.kx,320*this.ky]);UI.tint(this.aura,color);
    type Spec=[string,string,string,number,number];
    const specs:Record<string,[Spec,Spec]>={
      ember:[['rise','firefly','#ff8a3c',8,.8],['rise','sparkle','#ff5a22',11,.38]],
      berry:[['fall','fall_petal_pink','#ffffff',6,.26],['fall','fall_petal_pink','#ffd0e8',9,.3]],
      cocoa:[['rise','glow_soft','#f4dcc0',6,.12],['twinkle','sparkle','#ffd58a',9,.34]],
      hoot:[['fall','fall_leaf_gold','#ffffff',6,.24],['orbit','sparkle','#ffe27a',7,.32]],
      ink:[['twinkle','sparkle','#b79cff',8,.34],['orbit','sparkle','#8fa6ff',8,.34]],
      mint:[['rise','firefly','#9bff9b',8,.8],['fall','fall_leaf_green','#ffffff',9,.24]],
      sky:[['fall','fall_petal_cream','#e8f6ff',6,.26],['orbit','sparkle','#9fdcff',8,.34]],
      snow:[['fall','sparkle','#ffffff',9,.26],['fall','sparkle','#cfeaff',13,.32]],
      sunny:[['rise','sparkle','#ffe066',8,.3],['orbit','sparkle','#ffd23a',8,.36]],
    };
    const selected:Spec[]=this.grade===3?[['twinkle','sparkle',color,3,.26]]:[specs[this.species][this.grade===4?0:1]];
    if(this.grade>=5&&selected[0][0]!=='orbit')selected.push(['orbit','sparkle',color,4,.3]);
    for(const [mode,picture,tint,count,size]of selected)for(let i=0;i<count;i++){
      const side=size*2.3*128,node=UI.image(this.body,picture,[0,0,side*this.kx,side*this.ky]);UI.tint(node,tint);
      this.particles.push({node,mode,phase:(i*.618+this.seed)%1,speed:.7+(i%7)*.1,seed:i*2.399+this.seed,size:side});
    }
  }
  private updateSkin():void {
    if(!this.aura||this.asleep||this.code&1)return;
    const pulse=.5+.5*Math.sin(this.clock*(this.grade>=5?1.6:1.1));
    UI.opacity(this.aura,this.grade===3?.16+.10*pulse:this.grade===4?.22+.12*pulse:.50+.22*pulse);
    const auraSize=(.62+.10*pulse)*(this.grade>=5?1.2:1),center:Point=[0,-125*this.ky];
    UI.place(this.aura,[center[0]-160*this.kx*auraSize,center[1]-160*this.ky*auraSize,320*this.kx*auraSize,320*this.ky*auraSize]);
    for(const p of this.particles){
      const phase=(p.phase+this.clock*.22*p.speed)%1,fade=Math.sin(phase*Math.PI),sign=Math.sign(Math.sin(p.seed));let x=0,y=0,scale=1,alpha=fade;
      if(p.mode==='rise'){x=sign*(62+30*Math.abs(Math.cos(p.seed*3)))+Math.sin(this.clock*1.7+p.seed)*14;y=-20-phase*250;scale=1-.5*phase;alpha=fade*.95;}
      if(p.mode==='fall'){x=sign*(72+30*Math.abs(Math.cos(p.seed*3)))+Math.sin(this.clock*1.3+p.seed*2)*22;y=-290+phase*290;p.node.angle=-(this.clock*.9*p.speed+p.seed)*180/Math.PI;alpha=fade*.9;}
      if(p.mode==='twinkle'){x=Math.sign(Math.cos(p.seed))*(88+24*Math.abs(Math.sin(p.seed*2)));y=-125+Math.sin(p.seed*1.7)*125;scale=.3+.9*fade*fade;alpha=fade*fade;p.node.angle=-(this.clock*.8+p.seed)*180/Math.PI;}
      if(p.mode==='orbit'){const a=p.seed+this.clock*1.3*p.speed;x=Math.cos(a)*112;y=-155+Math.sin(a)*46+Math.sin(this.clock*2+p.seed)*10;scale=.75+.35*Math.sin(this.clock*4+p.seed);alpha=Math.sin(a)<0?.95:0;p.node.angle=-this.clock*1.5*180/Math.PI;}
      UI.place(p.node,[(x-p.size*scale/2)*this.kx,(y-p.size*scale/2)*this.ky,p.size*scale*this.kx,p.size*scale*this.ky]);UI.opacity(p.node,alpha);
    }
  }
  get debug():object {return {species:this.species,frame:this.frame,acting:this.acting,asleep:this.asleep,clock:this.clock,selected:this.selected};}
}
