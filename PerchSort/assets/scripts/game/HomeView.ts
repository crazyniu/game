import {_decorator, Component, Node, Sprite} from 'cc';
import {UI,Rect} from './UI';
import {Art} from './Art';
import {Kit} from './Popups';
import {Profile} from '../meta/Profile';
import {Progress} from '../meta/Progress';
const {ccclass}=_decorator;
interface HomeCallbacks{play:(level:number)=>void;map:()=>void;shop:()=>void;aviary:()=>void;settings:()=>void;daily:()=>void;draw:()=>void;tasks:()=>void;chest:()=>void;coins:()=>void;}
interface HomeBird{node:Node;picture:Node;frames:string[];rest:number;phase:number;due:number;frame:number;time:number;scale:number;feet:number[];}
@ccclass('HomeMotion')
class HomeMotion extends Component{view!:HomeView;update(dt:number):void{this.view.update(dt);}}
export class HomeView{
 node:Node;private birds:HomeBird[]=[];private clock=0;private lastIdle=-1;
 constructor(host:Node,private profile:Profile,private callbacks:HomeCallbacks,private instant=false){this.node=UI.group(host,'Home');this.build();this.node.addComponent(HomeMotion).view=this;}
 private build():void{
  const p=this.node;UI.image(p,'background_home_jungle',[0,0,1080,1920]);const [w,h]=Art.size('panel_home_title_sign');UI.image(p,'panel_home_title_sign',[94,-30,w,h]);
  this.addBird('macaw',330,228,4);this.addBird('yellow_chick',500,176,0);this.addBird('blue_chick',665,178,2);this.addBird('green_parrot',830,184,4);
  const size=Art.size('panel_home_perch_branch');UI.image(p,'panel_home_perch_branch',[-74,760,...size]);UI.feather_bar(p,[48,48],this.profile.coins,this.callbacks.coins);
  this.icon('Settings','pic_gear',976,104,96,0,this.callbacks.settings);
  const play=Kit.button(p,'Play',[290,1360,500,Kit.PLAY_H],'white_play',()=>this.callbacks.play(Progress.resume_level(this.profile)));
  const daily=this.icon('DailyReward','pic_calendar',960,1230,120,184,this.callbacks.daily),draw=this.icon('LuckyDraw','icon_home_draw',960,1450,120,184,this.callbacks.draw);
  const tasks=this.icon('Tasks','pic_tasks',120,1230,120,184,this.callbacks.tasks);this.badge(tasks,3);
  if(Progress.daily_reward_available(this.profile,Progress.today()))this.badge(daily,1);
  if(Progress.draws_available(this.profile,Progress.today()))this.badge(draw,1);
  const entries:[string,string,()=>void][]=[['Map','pic_map',this.callbacks.map],['Shop','pic_shop',this.callbacks.shop],['Aviary','pic_aviary',this.callbacks.aviary],['Chest','pic_chest',this.callbacks.chest]];
  entries.forEach(([name,asset,action],i)=>{const n=this.icon(name,asset,174+244*i,1760,132,204,action);if(name==='Chest'&&Progress.chests_ready(this.profile)>0)this.badge(n,Progress.chests_ready(this.profile));});
 }
 private icon(name:string,asset:string,x:number,y:number,side:number,back:number,action:()=>void):Node{
  const content:Record<string,Rect>={pic_gear:[21,28,203,202],pic_calendar:[49,39,154,194],icon_home_draw:[175,75,958,972],pic_map:[18,36,226,177],pic_shop:[18,17,196,221],pic_aviary:[21,49,200,164],pic_chest:[36,50,185,170],pic_tasks:[0,0,256,256]};
  const n=UI.hit_area(this.node,name,[x-80,y-80,160,160],action);if(back){const frame=UI.image(n,'btn_home_icon_frame',[(160-back)/2,(160-back)/2,back,back],true);UI.opacity(frame,.88);}
  const face=UI.art(n,asset,[(160-side)/2,(160-side)/2,side,side],content[asset]);UI.outline(face);return n;
 }
 private badge(n:Node,count:number):void{const b=UI.badge(n,[144,20],count,true)!;b.setScale(48/66,48/66,1);b.setPosition(120,-4);}
 private addBird(kind:string,x:number,height:number,rest:number):void{
  const frames=Art.frames(`home_island_${kind}_idle`),feet=kind==='macaw'||kind==='green_parrot'?330:470,c=Art.ink(frames[rest]),scale=height/(feet-c[1]),size=Art.size(frames[rest]);
  const contact:Record<string,number[]>={macaw:[320,324,323,328,324,323,323,322],green_parrot:[326,328,325,331,327,326,328,327]};
  const n=UI.group(this.node,`Bird_${kind}`,[x,950,0,0]),picture=UI.image(n,frames[rest],[-256*scale,-(contact[kind]?.[rest]??feet)*scale,size[0]*scale,size[1]*scale]);
  Art.preload(frames);this.birds.push({node:n,picture,frames,rest,phase:this.birds.length*.91,due:.45+(this.birds.length%4)*.65,frame:rest,time:-1,scale,feet:contact[kind]??Array(8).fill(feet)});
 }
 update(dt:number):void{if(this.instant)return;this.clock+=dt;for(const b of this.birds){const breath=(Math.sin(this.clock*Math.PI*2/2.8+b.phase)+1)/2;b.node.setScale(1-.008*breath,1+.02*breath,1);if(b.time<0&&this.clock>=b.due&&this.clock-this.lastIdle>=.62){b.time=0;this.lastIdle=this.clock;}if(b.time>=0){b.time+=dt;const frame=Math.floor(b.time*8);if(frame>=8){b.time=-1;b.due=this.clock+1+Math.random()*2;}const f=frame>=8?b.rest:frame;if(f!==b.frame){b.frame=f;const sprite=Art.cache.get(Art.key(b.frames[f]));if(sprite)b.picture.getComponent(Sprite)!.spriteFrame=sprite;const r=UI.rect(b.picture);UI.move(b.picture,r[0],-b.feet[f]*b.scale);}}}}
 refresh():void{UI.clear(this.node);this.birds=[];this.build();}
 destroy():void{this.node.destroy();}
 static preview(host:Node,profile:Profile,callbacks:HomeCallbacks,state='home'):HomeView{const p=profile.duplicate_profile();p.coins=state==='home_new'?0:50270;return new HomeView(host,p,callbacks,true);}
}
