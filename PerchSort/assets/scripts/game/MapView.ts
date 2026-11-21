import { Node, Mask, EventTouch, EventMouse, Graphics, Color, UITransform, UIOpacity, tween, Vec3, Tween } from 'cc';
import { UI, Rect, Point } from './UI';
import { Art } from './Art';
import { BirdView } from './BirdView';
import { Kit } from './Popups';
import { Profile } from '../meta/Profile';
import { Progress } from '../meta/Progress';
import { Levels } from '../core/Levels';
interface MapCallbacks { home: () => void; level: (level: number) => void; gate: (world: number) => void; coins: () => void }
const WORLD = ['green','bamboo','autumn','snow','blossom','mushroom','lakeside','pine','rainforest','wisteria'];
const CARD_WIDTH = 626, STRIDE = 666, CARD_TOP = 140, CARD_HEIGHT = 900, SIDE_MARGIN = 227;

export class MapView {
  node: Node;
  world = 1;
  page = false;
  private content!: Node;
  private cards: Node[] = [];
  private pageNode: Node | null = null;
  private strip!: Node;
  private play!: Node;
  private bar!: Node;
  private current: number;
  private scroll = 0;
  private travel = 0;
  private start = 0;
  private pressed = false;
  private scrollTween: Tween<{ x: number }> | null = null;

  constructor(host: Node, private profile: Profile, private callbacks: MapCallbacks, world = 0, private instant = false) {
    this.node = UI.group(host,'Map'); this.current = Progress.resume_level(profile);
    this.world = world || Progress.world_of_level(this.current); this.build();
  }
  private build(): void {
    UI.cover(this.node,'background_map_forest',[0,0,1080,1920]);
    this.strip = UI.group(this.node,'WorldStrip',[0,300,1080,1096]); this.strip.addComponent(Mask).type = Mask.Type.GRAPHICS_RECT;
    this.content = UI.group(this.strip,'Cards',[0,0,SIDE_MARGIN*2+Levels.WORLDS*STRIDE-40,1096]);
    for (let world=1; world<=Levels.WORLDS; world++) this.cards.push(this.card(world));
    this.strip.on(Node.EventType.TOUCH_START,(event:EventTouch)=>{
      this.pressed=true; this.start=this.localPoint(event)[0]; this.travel=0; this.stopScroll();
    });
    this.strip.on(Node.EventType.TOUCH_MOVE,(event:EventTouch)=>{
      if (!this.pressed) return;
      const x=this.localPoint(event)[0],delta=x-this.start; this.start=x; this.travel+=Math.abs(delta);
      if (this.travel>=18) { this.setScroll(this.scroll-delta); this.world=Math.max(1,Math.min(Levels.WORLDS,Math.round(this.scroll/STRIDE)+1)); this.selection(); }
    });
    this.strip.on(Node.EventType.TOUCH_END,(event:EventTouch)=>{
      if (!this.pressed) return; this.pressed=false;
      if (this.travel<18) {
        const [localX,y]=this.localPoint(event),x=localX+this.scroll;
        const index=Math.floor((x-SIDE_MARGIN)/STRIDE),left=SIDE_MARGIN+index*STRIDE;
        if (index>=0 && index<Levels.WORLDS && x>=left && x<=left+CARD_WIDTH && y>=CARD_TOP && y<=CARD_TOP+CARD_HEIGHT) {
          const world=index+1,enter=world===this.world||Progress.is_world_unlocked(this.profile,world);
          UI.onTap(); this.showWorld(world); if (enter) this.enter(world);
        }
      } else this.showWorld(this.world);
    });
    this.strip.on(Node.EventType.TOUCH_CANCEL,()=>{ this.pressed=false; this.showWorld(this.world); });
    this.strip.on(Node.EventType.MOUSE_WHEEL,(event:EventMouse)=>this.showWorld(this.world+(event.getScrollY()>0?-1:1)));
    this.play=Kit.button(this.node,'Play',[220,1432,640,Kit.PLAY_H],'white_play',()=>this.enter(this.world));
    UI.art(this.play,`label_home_island_play_${Art.locale}`,[164,40,312,120]);
    this.bar=UI.group(this.node,'Bar'); this.buildBar(); this.showWorld(this.world,true);
  }
  private localPoint(event:EventTouch):Point {
    const location=event.getUILocation(),point=this.strip.getComponent(UITransform)!.convertToNodeSpaceAR(new Vec3(location.x,location.y,0));
    return [point.x,-point.y];
  }
  private stopScroll():void { this.scrollTween?.stop(); this.scrollTween=null; }
  private buildBar():void {
    UI.clear(this.bar); UI.feather_bar(this.bar,[1080-48-320*2008/2172,48],this.profile.coins,this.callbacks.coins);
    Kit.close(this.bar,[48,48],()=>this.page?this.closeWorld():this.callbacks.home(),!this.page);
  }
  private card(world:number):Node {
    const card=UI.group(this.content,`World_${world}`,[SIDE_MARGIN+(world-1)*STRIDE,CARD_TOP,CARD_WIDTH,CARD_HEIGHT]);
    UI.nine(card,'island_grid_card',[0,0,CARD_WIDTH,CARD_HEIGHT],[60,60,60,60]);
    UI.nine(card,'island_grid_card_selected',[0,0,CARD_WIDTH,CARD_HEIGHT],[60,60,60,60]).name='Chosen';
    const window=UI.group(card,'Window',[24,24,578,852]),mask=window.addComponent(Mask); mask.type=Mask.Type.GRAPHICS_STENCIL;
    const shape=mask.subComp as Graphics; shape.clear(); shape.roundRect(0,-852,578,852,30); shape.fill();
    const size=Art.size(`card_scene_w${world}`),height=578*size[1]/size[0];
    UI.image(window,`card_scene_w${world}`,[0,-(height-852)/2,578,height]);
    const shade=UI.group(window,'NameShade',[0,852*.55,578,852*.45]).addComponent(Graphics),step=852*.45/128;
    for(let i=0;i<128;i++) { shade.fillColor=new Color(61,31,8,Math.round(255*.55*(i+.5)/128)); shade.rect(0,-step*(i+1),578,step); shade.fill(); }
    this.starProgress(card,world,[93,620,440,96]);
    Kit.lettering(card,`world_${WORLD[world-1]}`,[126.2,754.4,373.6,103.76]);
    const badge=UI.image(card,'world_medallion',[217,-84,192,192]); badge.name='WorldNumber'; Kit.number(badge,world,[51.84,53.76,88.32,80.64]);
    if(!Progress.is_world_unlocked(this.profile,world)) { UI.tint(window,'#9e9ea8'); shade.color=new Color('#9e9ea8'); UI.piece_fit(card,'island_padlock',[313,390],[220,220]); }
    else if(Progress.is_world_complete(this.profile,world)) UI.check_badge(card,[576,50]);
    return card;
  }
  private worldStars(world:number):number {
    let stars=0; for(let level=(world-1)*Levels.PER_WORLD+1;level<=world*Levels.PER_WORLD;level++) stars+=this.profile.best_stars(level); return stars;
  }
  private starProgress(parent:Node,world:number,bounds:Rect):void {
    const stars=this.worldStars(world),total=Levels.PER_WORLD*3,fit=bounds[3]/140;
    const holder=UI.group(parent,'Stars',[bounds[0]+14*fit,bounds[1]+14*fit,bounds[2]/fit-14,112]); holder.setScale(fit,fit,1);
    const width=bounds[2]/fit-14,track:Rect=[63,13.44,width-63,85.12];
    UI.nine(holder,'island_progress_track',track,[44,40,44,40]);
    if(stars>0) {
      const inset=track[3]*.18,inner:Rect=[track[0]+inset,track[1]+inset,track[2]-inset*2,track[3]-inset*2];
      const clip=UI.group(holder,'ProgressFill',[inner[0],inner[1],inner[2]*stars/total,inner[3]]); clip.addComponent(Mask).type=Mask.Type.GRAPHICS_RECT;
      UI.image_region(clip,'island_pill_green',[26,24,362,70],[0,0,inner[2],inner[3]]);
    }
    UI.art(holder,'island_star_full',[-14,-14,140,140]);
    Kit.number(holder,`${stars}/${total}`,[track[0]+56,12.32,track[2]-84,87.36]);
  }
  private selection():void {
    this.cards.forEach((card,i)=>{card.getChildByName('Chosen')!.active=i+1===this.world;UI.tint(card.getChildByName('WorldNumber')!,i+1===this.world?'#ffffff':'#d9d9d9');});
    if(this.play) UI.enabled(this.play,Progress.is_world_unlocked(this.profile,this.world));
  }
  private setScroll(value:number):void { this.scroll=Math.max(0,Math.min((Levels.WORLDS-1)*STRIDE,value)); UI.move(this.content,-this.scroll,0); }
  showWorld(world:number,instant=false):void {
    this.world=Math.max(1,Math.min(Levels.WORLDS,world)); this.selection(); this.stopScroll(); const target=(this.world-1)*STRIDE;
    if(instant||this.instant) this.setScroll(target);
    else { const state={x:this.scroll}; this.scrollTween=tween(state).to(.3,{x:target},{easing:'cubicOut',onUpdate:()=>this.setScroll(state.x)}).call(()=>{this.scrollTween=null;}).start(); }
  }
  private enter(world:number):void { if(Progress.is_world_unlocked(this.profile,world))this.open_world(world);else this.callbacks.gate(world); }
  open_world(world:number):void {
    this.stopScroll(); if(this.pageNode)Kit.destroy(this.pageNode); this.world=world; this.page=true; this.pressed=false;
    this.strip.active=false; this.play.active=false; this.pageNode=UI.group(this.node,'WorldPage'); this.pageNode.setSiblingIndex(this.bar.getSiblingIndex()); const page=this.pageNode;
    UI.image(page,world===1||world===Levels.WORLDS?`board_w${world}_open_bg`:`board_w${world}_bg`,[0,0,1080,1920]);
    Kit.sign(page,`world_${WORLD[world-1]}`,[240,96,600,209]);
    const value=`${this.worldStars(world)}/${Levels.PER_WORLD*3}`,numbers=Art.pack().numbers;
    const numberWidth=Array.from(value).reduce((width,c)=>width+Art.ink(`art/locale/${Art.locale}/${numbers.styles.brown.glyphs[c].replace(/\.png$/,'')}`)[2]+14,0)*44/numbers.cap;
    this.starProgress(page,world,[540-(numberWidth+140)/2,300,numberWidth+140,72]);
    const previous=UI.button(page,'WorldPrevious','btn_world_previous',[24,934,112,112],()=>this.enter(world-1));
    const next=UI.button(page,'WorldNext','btn_world_next',[944,934,112,112],()=>this.enter(world+1));
    UI.enabled(previous,world>1); UI.enabled(next,world<Levels.WORLDS);
    for(let i=0;i<Levels.PER_WORLD;i++) this.levelNode(page,(world-1)*Levels.PER_WORLD+i+1);
    page.getChildByName(`Level_${this.current}`)?.setSiblingIndex(page.children.length-1);
    this.buildBar();
    if(!this.instant){UI.opacity(page,0);tween(page.getComponent(UIOpacity)!).to(.2,{opacity:255}).start();}
  }
  private levelNode(parent:Node,level:number):void {
    const ordinal=(level-1)%Levels.PER_WORLD,x=[230,540,850][ordinal%3],y=470+Math.floor(ordinal/3)*208,state=Progress.node_state(this.profile,level);
    const layer=UI.group(parent,`Level_${level}`);
    UI.image(layer,state==='locked'?'level_tile_locked':'level_tile_open',[x-62,y-62,124,124]);
    const rect:Rect=state==='locked'?[x-39.68,y-24.8,79.36,49.6]:[x-42.16,y-31,84.32,57.04];
    const number=Kit.number(layer,ordinal+1,rect);
    if(state==='locked'){UI.opacity(number,.55);UI.art(layer,'island_padlock',[x+27.28,y-76.88,46,46]);}
    else for(let i=0;i<3;i++){const picture=i<this.profile.best_stars(level)?'island_star_full':'island_star_empty',ink=Art.ink(picture),height=38*ink[3]/ink[2];UI.art(layer,picture,[x-61+i*42,y+106-height,38,height],ink);}
    if(state==='current'&&level===this.current){
      const species=Levels.species_ids(Levels.create_board(Levels.get_level(level)))[0],side=x>540?-1:1;
      const geometry=Art.data('layout/map_bird_geometry').species[species],animation:Rect=geometry.animation,front:Rect=geometry.front;
      const fit=Math.min(128/animation[2],128/animation[3]),canvas=320*fit;
      const feet:Point=[x+side*140-(front[0]+front[2]/2-160)*fit,y+57.04-(front[1]+front[3]-300)*fit];
      BirdView.create(layer,species,[feet[0]-canvas/2,feet[1]-300*fit,canvas,canvas],{instant:this.instant});
    }
    const hit=UI.hit_area(layer,'Open',[x-82,y-76,164,190],()=>this.callbacks.level(level)); UI.enabled(hit,state!=='locked');
  }
  closeWorld():void { if(this.pageNode)Kit.destroy(this.pageNode);this.pageNode=null;this.page=false;this.strip.active=true;this.play.active=true;this.showWorld(this.world,true);this.buildBar(); }
  refresh():void { this.buildBar(); }
  destroy():void { this.stopScroll();Kit.destroy(this.node); }
  static preview(host:Node,profile:Profile,callbacks:MapCallbacks,state='cards'):MapView {
    const sample=profile.duplicate_profile();sample.coins=1240;sample.highest_unlocked=8;
    for(let level=1;level<8;level++)sample.stars[level]=level%3===0?2:3;
    const view=new MapView(host,sample,callbacks,1,true);if(state.includes('world'))view.open_world(1);return view;
  }
}
