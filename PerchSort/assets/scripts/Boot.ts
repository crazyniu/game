import {_decorator,Component,Node,Camera,Canvas,Color,Layers,UITransform,ResolutionPolicy,view,profiler,Widget,sys,BlockInputEvents,tween,Vec3,game,Mask} from 'cc';
import {DEBUG} from 'cc/env';
import {Art} from './game/Art';
import {UI} from './game/UI';
import {Sound} from './game/Sound';
import {HomeView} from './game/HomeView';
import {MapView} from './game/MapView';
import {BoardView} from './game/BoardView';
import {ShopView} from './game/ShopView';
import {AviaryView} from './game/AviaryView';
import {Popups,Kit,Dialog,AdEntry} from './game/Popups';
import {Tutorial} from './game/Tutorial';
import {Levels} from './core/Levels';
import {Board} from './core/Board';
import type {Move} from './core/Board';
import {GameSession} from './core/GameSession';
import {Solver} from './core/Solver';
import {Profile,ProfileStorage} from './meta/Profile';
import {Progress} from './meta/Progress';
import { Worlds } from './meta/Worlds';
import {Aviary} from './meta/Aviary';
import {Ads} from './meta/Ads';
import {Platform} from './platform/Platform';
import {DebugPlatform} from './platform/DebugPlatform';
import {createPlatform} from './platform/index';
import {MockAd} from './game/MockAd';
const {ccclass}=_decorator;
type Screen=HomeView|MapView|BoardView|ShopView|AviaryView;
@ccclass('Boot')
export class Boot extends Component{
 profile!:Profile;screen:Screen|null=null;popups!:Popups;sound!:Sound;root!:Node;screenHost!:Node;popupHost!:Node;overlayHost!:Node;page='home';ready=false;instant=false;testMode=false;autoplay=false;platform!:Platform;private pacing=Ads.pacing();overlay:ShopView|AviaryView|null=null;trial=0;
 private storage:ProfileStorage=sys.localStorage;private saveKey=Profile.SAVE_PATH;private args=new Map<string,string>();private loading!:Node;private nav=0;
 protected onLoad():void{view.setDesignResolutionSize(1080,1920,ResolutionPolicy.SHOW_ALL);profiler.hideStats();this.createCanvas();this.startGame().catch(e=>{console.error('Perch Sort startup failed',e);if(DEBUG)(globalThis as any).perchError=String(e);});}
 private createCanvas():void{
  const canvasNode=new Node('Canvas');canvasNode.layer=Layers.Enum.UI_2D;canvasNode.addComponent(UITransform).setContentSize(1080,1920);this.node.scene.addChild(canvasNode);
  const cameraNode=new Node('UICamera');cameraNode.layer=Layers.Enum.UI_2D;canvasNode.addChild(cameraNode);cameraNode.setPosition(0,0,1000);const camera=cameraNode.addComponent(Camera);camera.projection=Camera.ProjectionType.ORTHO;camera.visibility=Layers.Enum.UI_2D;camera.clearFlags=Camera.ClearFlag.SOLID_COLOR;camera.clearColor=new Color(31,61,30,255);camera.near=1;camera.far=2000;const canvas=canvasNode.addComponent(Canvas);canvas.cameraComponent=camera;
  const widget=canvasNode.addComponent(Widget);widget.isAlignTop=widget.isAlignBottom=widget.isAlignLeft=widget.isAlignRight=true;widget.top=widget.bottom=widget.left=widget.right=0;
  this.root=UI.group(canvasNode,'PerchSort',[-540,-960,1080,1920]);this.root.addComponent(Mask).type=Mask.Type.GRAPHICS_RECT;this.screenHost=UI.group(this.root,'Screens');this.overlayHost=UI.group(this.root,'Overlay');this.popupHost=UI.group(this.root,'Popups');this.popupHost.on(Node.EventType.CHILD_ADDED,()=>{if(this.screen instanceof BoardView)this.screen.suspend_input();});this.popupHost.on(Node.EventType.CHILD_REMOVED,()=>{if(this.screen instanceof BoardView&&!this.popupHost.children.length&&!this.overlay)this.screen.resume_input();});this.loading=UI.fill(this.root,'Loading',[0,0,1080,1920],'#234c27');this.loading.addComponent(BlockInputEvents);
 }
 private memoryStorage():ProfileStorage{
  const values=new Map<string,string>();
  return {getItem:key=>values.get(key)??null,setItem:(key,value)=>{values.set(key,value);}};
 }
 private async holdSaveLock():Promise<void>{
  if(!sys.isBrowser||!navigator.locks)return;
  let granted!:()=>void;
  const ready=new Promise<void>(resolve=>{granted=resolve;});
  const lockName='Perch Sort / '+this.saveKey;
  const hold=()=>new Promise<void>(release=>{
   window.addEventListener('pagehide',()=>release(),{once:true});
   this.node.once(Node.EventType.NODE_DESTROYED,release);
   granted();
  });
  const request=navigator.locks.request(lockName,{ifAvailable:true},async lock=>{
   if(lock){await hold();return;}
   const waiting=UI.group(this.loading,'WaitingForOtherTab');
   UI.cover(waiting,'background_home_jungle',[0,0,1080,1920]);
   UI.art(waiting,'island_padlock',[420,840,240,240]);
   const title=document.title;
   document.title=sys.language.startsWith('zh')?'请关闭另一个游戏标签页 · Perch Sort':'Close the other game tab · Perch Sort';
   await navigator.locks.request(lockName,async()=>{
    document.title=title;
    waiting.destroy();
    await hold();
   });
  });
  await Promise.race([ready,request]);
  // A cached page lost its lock on pagehide; reopen from the current saved value.
  window.addEventListener('pageshow',event=>{if(event.persisted)location.reload();});
 }
 private async startGame():Promise<void>{
  if(sys.isBrowser)new URLSearchParams(location.search).forEach((value,key)=>this.args.set(key,value));this.testMode=DEBUG&&this.args.has('test');this.instant=DEBUG&&this.args.has('instant');Kit.instant=this.instant;
  const requestedTrial=this.args.get('trial')??null;
  const trial=requestedTrial&&/^\d+$/.test(requestedTrial)?Number(requestedTrial):0;
  const requestedLevel=this.args.get('level')??null;
  const level=requestedLevel&&/^\d+$/.test(requestedLevel)?Number(requestedLevel):0;
  const validLevel=Number.isInteger(level)&&level>=1&&level<=Levels.COUNT;
  // Autoplay drives real moves through the board on an in-memory copy of the save, exactly like a level preview.
  this.autoplay=DEBUG&&this.args.has('autoplay')&&requestedTrial===null&&!this.testMode;
  const preview=requestedTrial===null&&(validLevel||this.autoplay)&&!this.testMode;
  const temporary=preview||(this.testMode&&this.args.has('fresh'));
  if(this.testMode)this.saveKey='perch-sort.test.'+(this.args.get('test')||'session');
  if(DEBUG)(globalThis as any).perchBoot=this;
  this.platform=createPlatform();await this.platform.init();if(this.platform instanceof DebugPlatform)this.platform.view=new MockAd(this.popupHost.parent!);
  await Art.init();Levels.configure(Art.data('levels/levels'),Art.data('levels/trial'));
  if(!temporary)await this.holdSaveLock();
  if(temporary){
   const memory=this.memoryStorage();
   if(preview){
    for(const key of [this.saveKey,this.saveKey+'.bak']){
     const value=this.storage.getItem(key);
     if(value!==null)memory.setItem(key,value);
    }
   }
   this.storage=memory;
  }
  if(!preview&&!this.testMode&&this.storage.getItem(this.saveKey)===null&&sys.isBrowser&&['localhost','127.0.0.1'].includes(location.hostname)){
    const response=await fetch('/__profile');if(response.ok){const legacy=await response.json();if(legacy){const p=Profile.from_dict(legacy);p.save(this.storage,this.saveKey);}}
  }
  this.profile=Profile.load_saved(this.storage,this.saveKey);
  if(this.testMode&&this.args.has('unlock')){this.profile.highest_unlocked=210;this.profile.coins=50000;this.profile.inventory=[10,10,10,10,10,10];for(const [species] of Aviary.SPECIES)this.profile.met[species]=true;for(let world=2;world<=Worlds.COUNT;world++)for(const bird of Worlds.birds(world))this.profile.met[bird]=true;}
  if(this.testMode&&this.args.has('outfit'))for(const id of this.args.get('outfit')!.split(',')){this.profile.outfits['outfit.'+id]=true;this.profile.equipped[id.split('.')[0]]='outfit.'+id;}
  Art.setLocale(this.args.get('lang')??(this.profile.language||sys.language));
  this.sound=new Sound(this.node);this.sound.init().then(()=>{this.applySound();this.sound.music(this.page==='board'?'bgm_game':'bgm_menu');}).catch(e=>console.error('Audio load failed',e));this.applySound();UI.onTap=()=>{this.sound.resume();this.sound.play('ui_tap');};
  this.popups=new Popups(this.popupHost,this.profile,{ad:p=>this.adEntry(p),rewarded:(p,done)=>void this.watchAd(p,done),save:()=>this.save(),play:n=>this.open_board(n),replay:()=>{if(this.screen instanceof BoardView){if(this.screen.session.state.is_won()){if(this.trial)this.open_trial(this.trial);else this.open_board(this.screen.level.id);}else this.screen.restart();}},home:()=>this.open_home(),language:()=>this.refreshLanguage(),mute:()=>this.applySound(),sound:n=>this.sound.play(n),coins:()=>this.coinShop(),help:()=>Tutorial.open(this.popupHost,()=>{},this.instant)});
  if(this.profile.needs_recovery()){this.saveRecovery();await Art.waitPending();this.loading.destroy();return;}
  if(DEBUG)(globalThis as any).perch={app:this,platform:this.platform,UI,Art,Levels,Progress,Profile,Board,Kit,get state(){const app=this.app;return {page:app.page,ready:app.ready,profile:app.profile.to_dict(),audio:app.sound.debug,errors:Art.errors,board:app.screen instanceof BoardView?{busy:app.screen.busy,level:app.screen.level.id,moves:app.screen.session.move_count(),won:app.screen.session.state.is_won(),branches:app.screen.session.state.items}:null};}};
  if(requestedTrial!==null)await this.open_trial(trial);else if(validLevel||this.autoplay)await this.open_board(validLevel?level:Math.min(Math.max(this.profile.highest_unlocked,1),Levels.COUNT));else if(requestedLevel!==null)await this.open_home();else if(this.args.get('page')==='map')await this.open_map(Number(this.args.get('world')||0));else if(this.args.get('page')==='shop')await this.open_shop();else if(this.args.get('page')==='aviary')await this.open_aviary();else await this.open_home();
  if(this.loading.isValid)this.loading.destroy();this.ready=true;
  if(this.autoplay)void this.drive(Number(this.args.get('autoTo')||Levels.COUNT));
  const popup=this.args.get('popup');if(DEBUG&&popup){if(popup==='settings')this.settings();else if(popup==='daily')this.popups.daily_reward();else if(popup==='draw')this.popups.lucky_draw();else if(popup==='chest')this.popups.chest();else if(popup==='win'){Popups.winLayout=this.args.get('layout')||Popups.winLayout;this.popups.win({stars:3,moves:12,par:14,reward:125,boosters:[2,2]},()=>{},()=>{},()=>{});}else if(popup==='level')this.popups.level_card(Number(this.args.get('card')||1));}
  if(!this.testMode&&!this.instant&&this.profile.best_stars(1)>0&&Progress.daily_reward_available(this.profile,Progress.today()))this.scheduleOnce(()=>{if(this.screen instanceof HomeView&&!this.popupHost.children.length)this.popups.daily_reward();},.9);
  console.log('PERCH_READY',JSON.stringify({engine:'Cocos Creator 3.8.8',page:this.page,levels:Levels.COUNT,test:this.testMode}));
 }
 private async setScreen(page:string,build:()=>Screen,trial=0):Promise<void>{
  const token=++this.nav;
  this.ready=false;
  const curtain=this.screen && !this.instant ? this.leafCurtain() : null;
  if(curtain)await curtain.slide(0,1);
  try{
   if(token!==this.nav)return;
   UI.clear(this.popupHost);
   this.closeOverlay();
   const previous=this.screen;
   this.screen=null;
   previous?.destroy();
   this.page=page;
   this.trial=trial;
   this.screen=build();
   UI.opacity(this.screen.node,0);
   await Art.waitPending();
   if(token!==this.nav)return;
   UI.opacity(this.screen.node,1);
   if(this.loading.isValid)this.loading.active=false;
   this.sound.music(page==='board'?'bgm_game':'bgm_menu');
   if(curtain)await curtain.slide(1,0);
   this.ready=true;
  }finally{curtain?.node.destroy();}
 }
 private leafCurtain(){
  const node=UI.group(this.root,'LeafWipe');
  node.addComponent(BlockInputEvents);
  const side=2304;
  const leaves=Array.from({length:6},(_,i)=>{
   const leaf=UI.image(node,'leaf_wipe',[0,0,side,side]);
   leaf.getComponent(UITransform)!.setAnchorPoint(.5,.5);
   if(i%2)leaf.setScale(-1,1,1);
   return leaf;
  });
  const pose=(cover:number)=>leaves.forEach((leaf,i)=>{
   const right=i%2===1,edge=right?1080:0;
   leaf.setPosition(edge+(right?1:-1)*side*.65*(1-cover),-1920*(.2+Math.floor(i/2)*.3));
   leaf.angle=(right?1:-1)*(1-cover)*12;
  });
  pose(0);
  const slide=(from:number,to:number)=>new Promise<void>(resolve=>{
   const state={cover:from};
   tween(state).to(.2,{cover:to},{easing:'sineInOut',onUpdate:()=>pose(state.cover)}).call(()=>resolve()).start();
  });
  return {node,slide};
 }
 async open_home():Promise<void>{await this.setScreen('home',()=>new HomeView(this.screenHost,this.profile,{play:n=>this.open_board(n),map:()=>this.open_map(),shop:()=>this.open_shop(),aviary:()=>this.open_aviary(),settings:()=>this.settings(),daily:()=>this.popups.daily_reward(),draw:()=>this.popups.lucky_draw(),chest:()=>this.popups.chest(),coins:()=>this.coinShop()},this.instant));}
 async open_map(world=0):Promise<void>{await this.setScreen('map',()=>new MapView(this.screenHost,this.profile,{home:()=>this.open_home(),level:n=>this.popups.level_card(n),gate:()=>this.sound.play('error'),coins:()=>this.coinShop()},world,this.instant));if(world>=1&&world<=Levels.WORLDS&&Progress.is_world_unlocked(this.profile,world))(this.screen as MapView).open_world(world);}
 async open_trial(number:number):Promise<void>{
  const def=Number.isInteger(number)?Levels.trial(number):undefined;
  if(!def){await this.open_home();return;}
  const player=this.profile.duplicate_profile();
  await this.setScreen('board',()=>new BoardView(this.screenHost,player,def,{
   settle:()=>{},
   won:({session})=>{
    const hasNext=number<Levels.trial_count();
    this.popups.win({stars:session.stars(),moves:session.move_count(),par:session.par,reward:0,boosters:[]},
     ()=>this.open_trial(number+1),()=>this.open_trial(number),()=>this.open_home(),hasNext);
   },
   settings:()=>this.settings(),buy:kind=>this.buyTool(kind),undo_more:()=>this.undoMore(),
   bird_info:species=>this.popups.goal_info(species,Levels.world_of(def)),
   rescue_offer:()=>this.rescueOffer(),restart_offer:picture=>this.restartOffer(picture),
   profile_changed:()=>{if(this.screen instanceof BoardView)this.screen.refresh_hud();},
   sound:name=>this.sound.play(name),
  },{instant:this.instant,entrance:!this.instant}),number);
  const board=this.screen as BoardView;
  if(number!==1||this.instant)return;
  await this.until(()=>!board.busy||this.screen!==board);
  if(this.screen!==board)return;
  const opening=await Solver.hint_async(board.session.state.clone());
  if(this.screen===board&&board.session.move_count()===0&&opening.status===Solver.Status.SOLVED&&opening.moves.length)board.show_hint(opening.moves[0]);
 }
 // Debug autoplay (./run.sh auto): plays the stored route, or the solver's move off-route, wins, then opens the next level until autoTo.
 private async drive(last:number):Promise<void>{
  const routes:Record<string,[number[],Move][]>=Art.data('levels/hints');
  const pause=(seconds:number)=>new Promise<void>(done=>this.scheduleOnce(()=>done(),seconds));
  while(this.node.isValid){
   await pause(.35);
   const board=this.screen;
   if(!(board instanceof BoardView))continue;
   const state=board.session.state,id=board.level.id;
   // The board stays busy behind the win popup.
   if(state.is_won()){
    if(id>=last){console.log('PERCH_AUTOPLAY_DONE',id);return;}
    await pause(2.5);if(this.screen===board)await this.open_board(id+1);continue;
   }
   if(board.busy)continue;
   const key=state.search_key(false).join(',');
   let move=(routes[String(id)]||[]).find(([route])=>route.join(',')===key)?.[1];
   if(!move){
    const result=await Solver.hint_async(state.clone(),[],400000,8000);
    if(this.screen!==board)continue;
    if(result.status!==Solver.Status.SOLVED||!result.moves.length){console.log('PERCH_AUTOPLAY_RESTART',id);board.restart();continue;}
    move=result.moves[0];
   }
   if(move[1]<0)board.tap_swivel(move[0]);else{board.tap_branch(move[0]);board.tap_branch(move[1]);}
  }
 }
 async open_board(level:number):Promise<void>{
  this.profile.last_level=level;const def=Levels.get_level(level),newSpecies=Aviary.on_level_started(this.profile,Levels.species_ids(Levels.create_board(def),true),def.world);Progress.grant_starter_gifts(this.profile);this.save();
  await this.setScreen('board',()=>new BoardView(this.screenHost,this.profile,level,{settle:session=>this.settle(level,session),won:r=>this.onWon(level,r.session),settings:()=>this.settings(),buy:k=>this.buyTool(k),undo_more:()=>this.undoMore(),bird_info:s=>this.popups.goal_info(s,def.world),rescue_offer:()=>this.rescueOffer(),restart_offer:p=>this.restartOffer(p),profile_changed:()=>this.save(),sound:n=>this.sound.play(n)},{instant:this.instant,entrance:!this.instant}));
  const board=this.screen as BoardView;await this.until(()=>!board.busy||this.screen!==board);if(this.screen!==board||this.testMode||this.autoplay)return;
  const queue:Array<(done:()=>void)=>void>=newSpecies.map(species=>done=>{this.popups.new_friend(species,Aviary.NEW_SPECIES_COINS,done);});
  for(const [kind,id] of Progress.BOOSTERS){const flag='unlock.'+id;if(Progress.booster_unlock_level(kind)===level&&!this.profile.has_tutorial(flag))queue.push(done=>{this.profile.set_tutorial(flag);this.save();this.popups.unlock(kind,done);});}
  if(level===1&&!this.profile.has_tutorial(Tutorial.FLAG))queue.push(done=>{this.profile.set_tutorial(Tutorial.FLAG);this.save();Tutorial.open(this.popupHost,done,this.instant);});
  for(const mechanic of Tutorial.due_lessons(this.profile,def))queue.push(done=>{this.profile.set_tutorial(Tutorial.lesson_flag(mechanic));this.save();Tutorial.open_lesson(this.popupHost,mechanic,Levels.world_of(def),done,this.instant);});
  this.runQueue(queue,()=>{board.resume_input();for(const [kind] of Progress.BOOSTERS)if(Progress.booster_unlock_level(kind)===level&&!this.profile.has_tutorial(Progress.guided_flag(kind))){const before=this.profile.inventory[kind];Progress.guide_unit(this.profile,kind);board.granted(kind,this.profile.inventory[kind]-before);board.guide_tool(kind);}},board);
 }
 private runQueue(queue:Array<(done:()=>void)=>void>,done:()=>void,scope:Screen):void{if(this.screen!==scope)return;const next=queue.shift();if(next){if(scope instanceof BoardView)scope.suspend_input();next(()=>this.runQueue(queue,done,scope));}else done();}
 async open_shop(tab=0):Promise<void>{if(this.screen instanceof BoardView){this.screen.suspend_input();this.overlay?.destroy();this.overlay=new ShopView(this.overlayHost,this.profile,{close:()=>this.closeOverlay(),changed:()=>{const board=this.screen as BoardView;this.profile.inventory.forEach((count,kind)=>{const delta=count-board.session.state.inventory[kind];if(delta>0)board.granted(kind,delta);});this.save();},coins:()=>this.coinShop(),sound:n=>this.sound.play(n)},tab);await Art.waitPending();return;}await this.setScreen('shop',()=>new ShopView(this.screenHost,this.profile,{close:()=>this.open_home(),changed:()=>this.save(),coins:()=>this.coinShop(),sound:n=>this.sound.play(n)},tab));}
 async open_aviary():Promise<void>{await this.setScreen('aviary',()=>new AviaryView(this.screenHost,this.profile,{close:()=>this.open_home(),changed:()=>this.save(),coins:()=>this.coinShop(),sound:n=>this.sound.play(n)}));}
 closeOverlay():void{this.overlay?.destroy();this.overlay=null;if(this.screen instanceof BoardView){this.screen.refresh_hud();this.screen.resume_input();}}
 settings():void{const board=this.screen instanceof BoardView?this.screen:null;board?.suspend_input();const dialog=this.popups.settings(!!board);dialog.node.on(Node.EventType.NODE_DESTROYED,()=>board?.node.isValid&&board.resume_input());}
 coinShop():void{this.popups.coin_shop();}
 private buyTool(kind:number):void{const board=this.screen as BoardView,before=this.profile.inventory[kind];this.popups.offer_tool(kind,()=>{board.granted(kind,this.profile.inventory[kind]-before);board.sync_inventory();board.refresh_hud();});}
 private undoMore():void{
  const board=this.screen as BoardView,d=Kit.jungle(this.popupHost,'undo_more',[280,88],{close:true});UI.art(d.content,'pic_undo',[400,d.ys[0],280,280]);Kit.number(d.content,'+1',[340,d.ys[1],400,88]);const entry=this.popups.adEntry(Ads.UNDO),buy=Kit.button(d.content,'Buy',[entry==='hidden'?325:94,d.actionY,430,162],'',()=>{if(this.profile.coins<Progress.UNDO_PRICE){this.coinShop();return;}this.profile.coins-=Progress.UNDO_PRICE;board.session.grant_undos(1);this.save();d.close();board.refresh_hud();});Kit.price(buy,[430,162],Progress.UNDO_PRICE);if(entry!=='hidden')this.popups.adButton(d.content,[556,d.actionY,430,162],entry,()=>this.popups.watch(Ads.UNDO,()=>{board.session.grant_undos(1);this.save();d.close();board.refresh_hud();}));
 }
 private rescueOffer():void{const board=this.screen as BoardView,d=Kit.jungle(this.popupHost,'stuck',[180],{width:760,compact:true,close:true});Kit.perch(d.content,[540,d.ys[0]+135],320);Kit.liveBird(d.content,'sunny',[418,d.ys[0]+5,112,130]);Kit.liveBird(d.content,'sky',[550,d.ys[0]+5,112,130]);Kit.button(d.content,'Rescue',[270,d.actionY,540,162],'white_free_rewind',()=>d.invoke(()=>board.rescue()),'wide');}
 private restartOffer(picture:string):void{const board=this.screen as BoardView,d=Kit.jungle(this.popupHost,'stuck',[180],{width:760,compact:true,close:true});UI.art(d.content,picture,[450,d.ys[0],180,180]);Kit.button(d.content,'Restart',[270,d.actionY,540,162],'white_action_replay',()=>d.invoke(()=>board.restart()),'wide');}
 private settlements = new WeakMap<GameSession, { payout: ReturnType<typeof Progress.on_win>; unlockedWorld: number }>();
 private settle(level:number,session:GameSession):void{
  const nextWorld=Progress.world_of_level(level)+1;
  const locked=nextWorld<=Levels.WORLDS && !Progress.is_world_unlocked(this.profile,nextWorld);
  for(const species of session.state.homed) Aviary.add_flock_home(this.profile,Board.SPECIES[species]);
  this.profile.inventory=session.state.inventory.slice();
  const next=level<Levels.COUNT ? Levels.get_level(level+1) : {};
  const payout=Progress.on_win(this.profile,level,session.move_count(),session.par,session.state.chorus_coins,next);
  const unlockedWorld=locked && Progress.is_world_unlocked(this.profile,nextWorld) ? nextWorld : 0;
  this.settlements.set(session,{payout,unlockedWorld});
  this.save();
 }
 private onWon(level:number,session:GameSession):void{
  const {payout,unlockedWorld}=this.settlements.get(session)!;
  this.settlements.delete(session);
  const hasNext=level<Levels.COUNT;
  const after=(action:()=>void)=>{
   if(payout.world>0)this.popups.world_complete(Progress.world_of_level(level),payout.world,action);
   else action();
  };
  const next=()=>{
   if(unlockedWorld)this.popups.world_unlock(unlockedWorld,()=>this.open_board((unlockedWorld-1)*Levels.PER_WORLD+1));
   else if(hasNext)this.open_board(level+1);
   else this.open_map();
  };
  const leave=(action:()=>void)=>()=>void this.levelEnd(level).then(()=>after(action));
  this.popups.win({stars:payout.stars,moves:session.move_count(),par:session.par,reward:payout.total-payout.world,boosters:payout.boosters},
   leave(next),leave(()=>this.open_board(level)),leave(()=>this.open_home()),hasNext);
 }
 private adEntry(placement:string):AdEntry{
  if(!this.platform.hasCapability('ads')||!this.platform.adsAvailable())return 'hidden';
  return Ads.left(this.profile,placement,Progress.today())>0?'ready':'used';
 }
 /** The reward runs only after the platform reports the ad watched to the end. */
 private async watchAd(placement:string,reward:()=>void):Promise<void>{
  if(this.adEntry(placement)!=='ready')return;
  this.sound.pause();const result=await this.platform.showRewarded(placement);this.sound.resume();
  if(result!=='done')return;
  Ads.record(this.profile,placement,Progress.today());this.pacing.lastRewarded=Date.now()/1000;this.save();reward();
 }
 /** Leaving a won level: an interstitial when Ads.INTERSTITIAL allows it. */
 private async levelEnd(level:number):Promise<void>{
  this.pacing.levelsSince++;const now=Date.now()/1000;
  if(!this.platform.hasCapability('ads')||!Ads.interstitial_due(this.pacing,level,now))return;
  if(await this.platform.showInterstitial(Ads.LEVEL_END)){this.pacing.lastShown=Date.now()/1000;this.pacing.levelsSince=0;}
 }
 save():void{this.profile.save(this.storage,this.saveKey);if(this.screen instanceof HomeView)this.screen.refresh();else if(this.screen instanceof BoardView)this.screen.refresh_hud();else if(this.screen instanceof ShopView||this.screen instanceof AviaryView)this.screen.refresh_balance();else if(this.screen instanceof MapView)this.screen.refresh();this.overlay?.refresh_balance();}
 private saveRecovery():void{
  this.page='save-recovery';UI.image(this.screenHost,'background_home_jungle',[0,0,1080,1920]);const d=Kit.jungle(this.popupHost,'',[400],{actionHeight:150});
  const en=Art.locale==='en',regions: [number,number,number,number][] = en?[[121,559,1537,269],[52,500,924,413],[324,1127,377,158],[365,1353,292,137]]:[[264,67,1248,346],[47,445,930,518],[382,1128,264,129],[381,1365,263,130]];
  const word=(parent:Node,index:number,r:[number,number,number,number])=>UI.paint(parent,{name:index===0?'label_save_error':'label_save_recovery_'+Art.locale,region:regions[index]},r);
  word(d.content,0,[284,d.rect[1]+80,512,125]);word(d.content,1,[150,d.ys[0]+30,780,340]);
  const retry=Kit.button(d.content,'Retry',[96,d.actionY,420,150],'',()=>{if(sys.isBrowser)location.reload();else void game.restart();});word(retry,2,[65,30,290,90]);
  const leave=Kit.button(d.content,'Exit',[564,d.actionY,420,150],'',()=>{this.sound.pause();game.end();});word(leave,3,[65,30,290,90]);
 }
 private applySound():void{this.sound.set_muted(this.profile.muted);this.sound.set_music_muted(this.profile.music_muted);this.sound.vibration=this.profile.vibration;}
 private refreshLanguage():void{if(this.screen instanceof HomeView)this.screen.refresh();else if(this.screen instanceof MapView){const world=this.screen.world,page=this.screen.page;this.screen.destroy();this.screen=new MapView(this.screenHost,this.profile,{home:()=>this.open_home(),level:n=>this.popups.level_card(n),gate:()=>this.sound.play('error'),coins:()=>this.coinShop()},world,this.instant);if(page)this.screen.open_world(world);}else if(this.screen instanceof BoardView)this.screen.refresh_hud();}
 // A fresh callback per poll: Cocos drops a scheduleOnce of the callback that is currently firing, which left this waiting forever.
 private until(predicate:()=>boolean):Promise<void>{return new Promise(resolve=>{const check=()=>{if(predicate())resolve();else this.scheduleOnce(()=>check(),.05);};check();});}
}
