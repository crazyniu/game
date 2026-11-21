import { BlockInputEvents, Color, EditBox, Graphics, Label, Mask, Node, Sprite, Tween, tween, UIOpacity, UITransform, Vec3, isValid } from 'cc';
import { UI, Rect, Point } from './UI';
import { Art } from './Art';
import { BirdView } from './BirdView';
import { Board } from '../core/Board';
import { Levels } from '../core/Levels';
import { Worlds } from '../meta/Worlds';
import { Profile } from '../meta/Profile';
import { Progress } from '../meta/Progress';
import { Shop } from '../meta/Shop';
import { Ads } from '../meta/Ads';
import { Flat, ON_DARK, ON_LIGHT, onTone } from './Flat';
import { Fx } from './Fx';
import { I18n, tr, word } from './I18n';

export interface PageCallbacks {
  close: () => void;
  changed: () => void;
  coins?: () => void;
  sound?: (name: string) => void;
}
export interface PopupCallbacks {
  save: () => void;
  play: (level: number) => void;
  replay?: () => void;
  help?: () => void;
  home?: () => void;
  language?: (code: string) => void;
  mute?: () => void;
  sound?: (name: string) => void;
  coins?: () => void;
  purchase?: (pack: CoinPack, paid: () => void) => void;
  /** Ad entry for a placement: hidden (no ads in this build or none loaded), used (today's cap reached) or ready. */
  ad?: (placement: string) => AdEntry;
  /** Show a rewarded ad; done runs only after the platform reports it watched to the end. */
  rewarded?: (placement: string, done: () => void) => void;
  close?: () => void;
  /** Feathers changed outside a page (free feathers): the screen redraws its balance. */
  changed?: () => void;
}
export type AdEntry = 'hidden' | 'used' | 'ready';
export interface Dialog {
  node: Node; content: Node; rect: Rect; ys: number[]; actionY: number;
  close: () => void; invoke: (action: () => void) => void;
}
export interface CoinPack { id: string; coins: number; yuan: number }
/** Each feature's home entry: picture, its drawn area, and the label key. */
export const FEATURE_ENTRY: Record<string, [string, Rect | undefined, string]> = {
  signin: ['pic_calendar', [49, 39, 154, 194], 'H_FEAT_SIGNIN'], draw: ['icon_home_draw', [175, 75, 958, 972], 'H_DRAW'], chest: ['pic_chest', [36, 50, 185, 170], 'H_CHEST'],
  album: ['pic_aviary', [21, 49, 200, 164], 'H_ALBUM'], tasks: ['pic_tasks', undefined, 'UI_TAB_TASKS'],
};
export const BOOSTER_ART = ['firefly', 'sprout', 'owl_eye', 'bell', 'scarecrow', 'rain'];
export const BOOSTER_WORD = ['firefly', 'sprout', 'owl_eye', 'wake_bell', 'scarecrow', 'spring_rain'];
const WORLD_ID = ['', 'green', 'bamboo', 'autumn', 'snow', 'blossom', 'mushroom', 'lakeside', 'pine', 'rainforest', 'wisteria'];
const DRAW_PRIZE_ART = ['icon_draw_prize_forty','icon_draw_prize_eighty','icon_draw_mystery_single','icon_draw_prize_one_twenty','icon_draw_prize_two_hundred','icon_draw_mystery_double','icon_draw_prize_sixty','icon_draw_prize_four_hundred'];
const DRAW_SPOTS = [[0,0],[1,0],[2,0],[2,1],[2,2],[1,2],[0,2],[0,1]];
const DRAW_PICTURES: Rect[] = [[41,21,176,137],[32,15,203,142],[29,15,195,149],[7,3,234,161],[4,-2,240,165],[18,-3,221,166],[7,2,226,160],[9,0,222,161]];

/** The existing painted ButtonKit, with native Cocos nodes and unchanged design geometry. */
export class Kit {
  static instant = false;
  /** Width of a number in the game font (digits are about 0.6 of the size). */
  static numberWidth(value: string, height: number): number { return String(value).length * height * .6; }
  static number(parent: Node, value: string | number, rect: Rect, align = 0): Node { return Flat.text(parent, String(value).replace(/x/g, '×'), rect, Math.round(rect[3] * .92), ON_DARK, align).node.parent!; }
  static lettering(parent:Node,key:string,rect:Rect):Node { return Flat.text(parent,word(key),rect,Math.round(Math.min(rect[3]*.8,72)),ON_LIGHT).node.parent!; }
  static readonly BUTTON_H=176;
  /** The play buttons (home, map, level card) share one larger height. */
  static readonly PLAY_H=200;
  /** Word heights never scale with the button: action buttons and compact card plates. */
  static readonly WORD_CAP=62;
  static readonly COMPACT_CAP=44;
  /** Vertical centre of the candy face (above the darker bottom edge), as a share of the plate height. */
  static readonly FACE_CENTRE=.435;
  static face(parent:Node, rect:Rect, kind='green'): Node {
    const flat = ({green:'j_btn_green',wide:'j_btn_green',plate:'j_btn_green',orange:'j_btn_amber',blue:'j_btn_blue'} as Record<string,string>)[kind];
    if (flat) return Flat.plate(parent, flat, rect);
    const asset = ({key_green:'btn_key_green',key_cream:'btn_key_cream',field:'field_quantity'} as Record<string,string>)[kind];
    return UI.image(parent,asset,rect,true);
  }
  static plate(parent:Node,rect:Rect,kind='green'):Node { const box=UI.group(parent,'Plate',rect); Kit.face(box,[0,0,rect[2],rect[3]],kind); return box; }
  /** A white painted word at a fixed height, centred on the candy face. */
  /** A button's words in the game font, centred on the face; tone picks the solid line under the words. */
  static word(parent:Node,id:string,size:Point,x=0,width=size[0],tone='green'):Node { const fs=Math.round(size[1]*(size[1]>=140?.38:.4)); return Flat.text(parent,word(id),[x+width*.06,Flat.face(size[1])-fs*.7,width*.88,fs*1.4],fs,onTone(Kit.tone(tone))).node.parent!; }
  static tone(kind:string):string { return ({orange:'amber',blue:'blue',key_green:'green',key_cream:'blue'} as Record<string,string>)[kind] ?? 'green'; }
  static wordWidth(id:string,size:Point):number { const n=new Node(),fs=Math.round(size[1]*(size[1]>=140?.38:.4)),w=Flat.width(Flat.text(n,word(id),[0,0,4000,fs*1.4],fs));n.destroy();return w; }
  static button(parent:Node,name:string,rect:Rect,word:string,action:()=>void,kind='green'):Node {
    const hit=UI.hit_area(parent,name,rect,action); Kit.face(hit,[0,0,rect[2],rect[3]],kind); if(word) Kit.word(hit,word,[rect[2],rect[3]],0,rect[2],kind); return hit;
  }
  /** A feather and a price on a button face. */
  static price(parent:Node,size:Point,value:number):Node {
    const fs=Math.round(size[1]*(size[1]>=140?.38:.4)), feather=fs*1.15, gap=fs*.2, width=Math.min(Kit.numberWidth(String(value),fs),size[0]*.72-feather-gap), x=(size[0]-feather-gap-width)/2;
    const box=UI.group(parent,'FeatherPrice',[0,0,...size]);
    UI.image(box,'icon_purchase_feather',[x,Flat.face(size[1])-feather/2,feather,feather],true);
    Flat.text(box,String(value),[x+feather+gap,Flat.face(size[1])-fs*.7,width+fs,fs*1.4],fs,onTone('green'),-1); return box;
  }
  static panel(parent:Node,rect:Rect):Node { return Flat.card(parent,'f_card',rect,240,116); }
  static close(parent:Node,at:Point|Rect,action:()=>void,back=false):Node { return Flat.key(parent,back?'Back':'Close',[at[0],at[1],at[2]??120,at[3]??120],back?'blue':'red',back?'f_g_back':'f_g_x',action,.5); }
  /** A title on a blue plate, centred where the painted sign used to be. */
  static sign(parent:Node,key:string,rect:Rect):Node { return Flat.title(parent,word(key),rect[0]+rect[2]/2,rect[1]+rect[3]*.6); }
  static centerPivot(node:Node):void { const r=UI.rect(node);node.getComponent(UITransform)!.setAnchorPoint(.5,.5);node.setPosition(r[0]+r[2]/2,-r[1]-r[3]/2); }
  static stop(node:Node):void { Tween.stopAllByTarget(node); const opacity=node.getComponent(UIOpacity); if(opacity) Tween.stopAllByTarget(opacity); node.children.forEach(Kit.stop); }
  static destroy(node:Node):void { if(!isValid(node,true))return;Kit.stop(node); node.removeFromParent(); node.destroy(); }
  static after(node:Node,seconds:number,action:()=>void):void { tween(node).delay(Kit.instant?0:seconds).call(action).start(); }
  static pop(node:Node,delay=0):void { if(Kit.instant)return;void Promise.resolve().then(()=>Art.waitPending()).then(()=>{if(!node.isValid)return;const rest=node.position.clone(),r=UI.rect(node);node.setScale(.94,.94,1);node.setPosition(rest.x+r[2]*.03,rest.y-r[3]*.03,rest.z);tween(node).delay(delay).to(.26,{scale:new Vec3(1,1,1),position:rest},{easing:'backOut'}).start();}); }
  static breathe(node:Node,delay=0):void { if(Kit.instant)return; tween(node).delay(delay).repeatForever(tween().to(1.4,{scale:new Vec3(.992,1.02,1)},{easing:'sineInOut'}).to(1.4,{scale:new Vec3(1,1,1)},{easing:'sineInOut'})).start(); }
  static layer(host:Node,name:string,onClose?:()=>void,page=false,background='background_home_jungle'):Dialog {
    const node=UI.group(host,name), block=UI.group(node,'InputBlock'); block.addComponent(BlockInputEvents);
    if(page) UI.cover(node,background,[0,0,1080,1920]); else UI.fill(node,'Dim',[0,0,1080,1920],'#0512088c');
    const content=UI.group(node,'Content'); UI.opacity(content,0);void Promise.resolve().then(()=>Art.waitPending()).then(()=>{if(content.isValid)UI.opacity(content,1);});let closed=false;
    const close=()=>{ if(closed)return; closed=true; Kit.destroy(node); onClose?.(); };
    return {node,content,rect:[0,0,1080,1920],ys:[],actionY:0,close,invoke(action){close();action();}};
  }
  static warmFrame(parent:Node,rect:Rect):void { Flat.card(parent,'f_popup',rect,300,146); }
  static toyFrame(parent:Node,rect:Rect):void { Flat.card(parent,'f_popup',rect,300,146); }
  static jungle(host:Node,title:string,heights:number[],options:{width?:number; actionHeight?:number; close?:boolean; compact?:boolean; birdsAbove?:boolean; footer?:number; onClose?:()=>void}={}):Dialog {
    const width=options.width??960, head=options.compact?96:options.birdsAbove?210:120, foot=options.compact?72:(options.footer??168), actionHeight=options.actionHeight??Kit.BUTTON_H;
    const inner=heights.reduce((a,b)=>a+b,0)+Math.max(0,heights.length-1)*24, above=options.birdsAbove?180:0;
    const height=head+inner+foot, top=Math.max(32,(1920-above-height-48-actionHeight)/2)+above;
    const d=Kit.layer(host,'Popup',options.onClose); d.rect=[(1080-width)/2,top,width,height]; d.actionY=top+height+48;
    Kit.warmFrame(d.content,d.rect); if(title) Flat.title(d.content,word(title),540,top+(options.birdsAbove?110:12));
    let y=top+head; d.ys=heights.map(h=>{const at=y;y+=h+24;return at;});
    if(options.close) Kit.close(d.content,[d.rect[0]+width-84,top-32],d.close);
    Kit.pop(d.content); return d;
  }
  static purchase(host:Node,title:string,heights:number[],actionHeight=Kit.BUTTON_H):Dialog {
    const width=heights.length>=3?880:840, inner=heights.reduce((a,b)=>a+b,0)+(heights.length-1)*24;
    const height=120+inner+(heights.length>=3?88:72), top=(1920-height-32-actionHeight)/2;
    const d=Kit.layer(host,'Purchase'); d.rect=[(1080-width)/2,top,width,height]; d.actionY=top+height+32;
    Kit.toyFrame(d.content,d.rect); Flat.title(d.content,word(title),540,top+12); Kit.close(d.content,[d.rect[0]+width-84,top-32],d.close);
    let y=top+120; d.ys=heights.map(h=>{const at=y;y+=h+24;return at;}); Kit.pop(d.content); return d;
  }
  /** A sub-page (fishsort): back key top left and the title plate; no balance bar. */
  static page(host:Node,name:string,title:string,_balance?:number,onClose?:()=>void,_coins?:()=>void):Dialog {
    const background=['DailyReward','DailyDraw','DrawResult','CoinShop'].includes(name)?'background_shop_forest':'background_home_jungle';const d=Kit.layer(host,name,onClose,true,background); Flat.title(d.content,word(title),540,Flat.HEAD_Y,115,640); Kit.close(d.content,[32,Flat.HEAD_Y-64,128,128],d.close,true);
    return d;
  }
  static glow(parent:Node,rect:Rect):void { const glow=UI.image(parent,'glow_soft',[rect[0]-rect[2]*.22,rect[1]-rect[3]*.22,rect[2]*1.44,rect[3]*1.44]); UI.opacity(glow,.65); }
  static stars(parent:Node,y:number,filled:number,animate=false):void {
    for(let i=0;i<3;i++){const side=i===1?223.2:180;const star=UI.art(parent,i<filled?'island_star_full':'island_star_empty',[540+(i-1)*237.6-side/2,y+140+(i===1?-14:8)-side/2,side,side]);Kit.centerPivot(star);star.angle=(i-1)*-12;if(animate&&i<filled&&!Kit.instant){star.setScale(.25,.25,1);tween(star).delay(.4+i*.18).to(.26,{scale:new Vec3(1.2,1.2,1)},{easing:'quadOut'}).to(.18,{scale:new Vec3(1,1,1)},{easing:'sineInOut'}).start();}}
  }
  static perch(parent:Node,contact:Point,width:number):Node { return UI.piece(parent,'branch_aviary_perch',[contact[0]-width/2,contact[1]-140*width/Art.size('branch_aviary_perch')[0]],width); }
  static liveBird(parent:Node,species:string,rect:Rect,outfit=''):BirdView {
    // One scale and one anchor for the plain bird and all its skins, taken from the largest of their pictures, so a skin with a tall hat does not shrink the body.
    const inks=[species+'_front',...[1,2,3,4,5].map(i=>`${species}_outfit_${i}_front`)].filter(name=>Art.has(name)).map(name=>Art.ink(name)),
      left=Math.min(...inks.map(ink=>ink[0])),top=Math.min(...inks.map(ink=>ink[1])),right=Math.max(...inks.map(ink=>ink[0]+ink[2])),
      size=Art.size(species+'_front'),k=Math.min(rect[2]/(right-left),rect[3]/(Art.feet(species)-top));
    return BirdView.create(parent,species,[rect[0]+rect[2]/2-(left+right)/2*k,rect[1]+rect[3]-size[1]*300/320*k,size[0]*k,size[1]*k],{outfit,instant:Kit.instant,lazy:true});
  }
  static titleKey(label:string):string { return label.startsWith('bird.')?'bird_'+label.split('.')[1]:label.replace(/\./g,'_'); }
  static prizes(parent:Node,coins:number,boosters:number[],y:number,columns=3):Node[] {
    const counts=new Map<number,number>(); boosters.forEach(k=>counts.set(k,(counts.get(k)||0)+1));
    const prizes:Array<[string,number]> = coins>0?[['icon_purchase_feather',coins]]:[]; counts.forEach((count,k)=>prizes.push(['island_icon_'+BOOSTER_ART[k],count]));
    return prizes.map(([art,count],i)=>{const col=i%columns, row=Math.floor(i/columns), n=Math.min(columns,prizes.length-row*columns);const tile=UI.group(parent,'Reward'+i,[540+(col-(n-1)/2)*250-125,y+row*270,250,250]);const icon=UI.art(tile,art,[20,0,210,170]);Kit.breathe(icon,i*.4);Kit.number(tile,'+'+count,[8,190,234,56]);Kit.pop(tile,.3+i*.16);return tile;});
  }
}

export class Popups {
  constructor(public host:Node,public profile:Profile,public callbacks:PopupCallbacks) {}
  private save():void { this.callbacks.save(); }
  private sound(name:string):void { this.callbacks.sound?.(name); }
  /** Settings (fishsort's settings popup): music, sound and vibration switches, the language, how to play; in a level also home and retry. */
  settings(inGame=false):Dialog {
    const w=893,rowH=138,step=161,pad=46,h=pad+(inGame?3:4)*step+127+(inGame?23+161:0)+pad;
    const p=Flat.popup(this.host,'Settings',w,h,tr(inGame?'PAUSE_TITLE':'UI_SETTINGS')),b=p.body;
    const d:Dialog={node:p.node,content:b,rect:p.rect,ys:[],actionY:0,close:p.close,invoke(action){p.close();action();}};
    const rows:[string,string,()=>boolean,()=>void][]=[
      ['f_g_music','H_MUSIC',()=>!this.profile.music_muted,()=>{this.profile.music_muted=!this.profile.music_muted;}],
      ['f_g_sound','H_SOUND',()=>!this.profile.muted,()=>{this.profile.muted=!this.profile.muted;}],
      ['f_g_vibrate','H_VIBRATION',()=>this.profile.vibration,()=>{this.profile.vibration=!this.profile.vibration;}],
    ];
    const row=(name:string,y:number,icon:string,word:string,action:()=>void):Node=>{const n=UI.hit_area(b,name,[pad,y,w-2*pad,rowH],action);Flat.card(n,'f_inset',[0,0,w-2*pad,rowH],180,88);UI.tint(Flat.image(n,icon,[35,35,69,69]),'#3F8DFF');Flat.text(n,tr(word),[132,0,420,rowH],46,ON_LIGHT,-1);return n;};
    let y=pad;
    rows.forEach(([icon,word,on,flip])=>{let toggle:Node;const n=row('Switch_'+word,y,icon,word,()=>{flip();this.callbacks.mute?.();this.save();Kit.destroy(toggle);toggle=Flat.image(n,on()?'f_toggle_on':'f_toggle_off',[w-2*pad-40-167,29.5,167,79]);});toggle=Flat.image(n,on()?'f_toggle_on':'f_toggle_off',[w-2*pad-40-167,29.5,167,79]);y+=step;});
    if(!inGame){
      const langs=I18n.languages();
      if(langs.length===2){
        const n=row('Language',y,'f_g_globe','H_LANGUAGE',()=>this.setLanguage(langs[I18n.lang===langs[0]?1:0],p.close)),seg=UI.group(n,'Switch',[w-2*pad-24-369,23,369,92]);
        Flat.plateIn(seg,'f_bar_track_light',[0,0,369,92]);const k=langs.indexOf(I18n.lang);Flat.plate(seg,'j_btn_blue',[k*184.5,0,184.5,92]);
        langs.forEach((code,i)=>Flat.text(seg,I18n.SHORT[code],[i*184.5,0,184.5,86],37,i===k?onTone('blue'):{color:'#7E9CCB'}));
      }else{const n=row('Language',y,'f_g_globe','H_LANGUAGE',()=>this.languages(p.close));Flat.text(n,I18n.NAMES[I18n.lang]+'  ›',[w-2*pad-40-360,0,360,rowH],43,ON_LIGHT,1);}
      y+=step;
    }
    Flat.button(b,'Help',[pad,y,w-2*pad,127],'blue',tr('H_HELP'),()=>this.help(),46);y+=127+23;
    if(inGame){Flat.button(b,'Home',[pad,y,389,161],'blue',tr('H_TO_HOME'),()=>d.invoke(()=>this.callbacks.home?.()),58);Flat.button(b,'Retry',[w-pad-389,y,389,161],'green',tr('H_RETRY'),()=>d.invoke(()=>this.callbacks.replay?.()),58);}
    return d;
  }
  /** Every language the build carries, each in its own words; the current one is green. */
  private languages(closeSettings:()=>void):void {
    const langs=I18n.languages(),rows=Math.ceil(langs.length/2),h=46+rows*134-24+46,p=Flat.popup(this.host,'Languages',893,h,tr('H_LANGUAGE'));
    langs.forEach((code,i)=>Flat.button(p.body,'Language_'+code,[46+(i%2)*(380+41),46+Math.floor(i/2)*134,380,110],code===I18n.lang?'green':'blue',I18n.NAMES[code],()=>{p.close();this.setLanguage(code,closeSettings);},40));
  }
  private setLanguage(code:string,closeSettings:()=>void):void {
    if(code===I18n.lang)return;
    this.profile.language=code;this.save();
    void Flat.loadFont(code).then(()=>{I18n.set(code);closeSettings();this.callbacks.language?.(code);this.settings(false);});
  }
  help():void { this.callbacks.help?.(); }
  level_card(level:number,onPlay?:()=>void):Dialog {
    const def=Levels.get_level(level), species=Levels.species_ids(Levels.create_board(def)), tools=Array.from(new Set([...Progress.fitting_boosters(def),Board.Booster.FIREFLY])).filter(kind=>Progress.is_booster_unlocked(this.profile,kind)).slice(0,4);
    const perRow=species.length<=4?4:5,tile=perRow===4?176:152,rows=Math.ceil(species.length/perRow);
    const mechanics=new Set<string>(); for(const b of def.branches){if(b.kind&&b.kind!=='normal')mechanics.add(b.kind);for(const v of b.birds){const t=String(v);if(t.endsWith(':sleep'))mechanics.add('sleep');if(t.startsWith('egg:'))mechanics.add('egg');if(t==='crow')mechanics.add('crow');if(t.endsWith(':mama')||t.endsWith(':chick'))mechanics.add('mama');}}
    const mechanicArt:Record<string,string>={sleep:'hoot_sleep',egg:'egg_idle',crow:'crow_front',twig:'icon_sapling',growing:'icon_sapling',swivel:'btn_swivel_green',mama:'mama_bow'};
    const listed=Array.from(mechanics).filter(k=>mechanicArt[k]);const heights=[224,100];if(rows)heights.push(rows*tile+(rows-1)*20);if(listed.length)heights.push(96);if(tools.length)heights.push(188);
    const d=Kit.jungle(this.host,'',heights,{actionHeight:200,close:true,compact:true});
    Flat.title(d.content,tr('UI_LEVEL',level),540,d.rect[1]+12);
    const kind=Progress.kind(level);if(kind!=='normal')Flat.kindTag(d.content,kind,d.rect[0]+40,d.rect[1]+70,58);
    for(let i=0;i<3;i++)UI.art(d.content,i<this.profile.best_stars(level)?'island_star_full':'island_star_empty',[450+(i-1)*216,d.ys[0]+22,180,180]);UI.art(d.content,'icon_star_budget_steps',[420,d.ys[1]+14,72,72],[317,217,652,801]);Kit.number(d.content,def.par,[508,d.ys[1]+18,150,64]);
    let g=2;species.forEach((s:string,i:number)=>{const row=Math.floor(i/perRow),n=Math.min(perRow,species.length-row*perRow),x=540+(i%perRow-(n-1)/2)*(tile+20)-tile/2,y=d.ys[g]+row*(tile+20);UI.art(d.content,'hud_tool_slot',[x,y,tile,tile]);const inner=tile*.82;Kit.liveBird(d.content,Worlds.look(Levels.world_of(def),s),[x+(tile-inner)/2,y+(tile-inner)/2-6,inner,inner]);const effect=({sunny:'wake',sky:'reveal',berry:'encore',mint:'grow',cocoa:'scare',snow:'undo',ember:'hatch',ink:'encore',hoot:'wake'} as Record<string,string>)[s];if(effect){const badge=tile*.42;UI.art(d.content,'gift_coin_'+effect,[x+tile-badge*.85,y+tile-badge*.85,badge,badge]);}UI.hit_area(d.content,'Bird_'+s,[x,y,tile,tile],()=>this.goal_info(s,Levels.world_of(def)));});if(rows)g++;
    if(listed.length){const y=d.ys[g++];listed.forEach((key,i)=>UI.art(d.content,mechanicArt[key],[540+(i-(listed.length-1)/2)*132-44,y+4,88,88]));}
    if(tools.length){const y=d.ys[g]; tools.forEach((kind:number,i:number)=>{const x=540+(i-(tools.length-1)/2)*180-68;const slot=UI.button(d.content,'Tool_'+kind,'hud_tool_slot',[x,y,136,136],()=>this.offer_tool(kind,()=>{UI.clear(count);Kit.number(count,this.profile.inventory[kind]||'+',[0,0,136,46]);}));UI.art(slot,'island_icon_'+BOOSTER_ART[kind],[18,18,100,100]);const count=UI.group(d.content,'Stock_'+kind,[x,y+142,136,46]);Kit.number(count,this.profile.inventory[kind]||'+',[0,0,136,46]);});}
    const play=Kit.button(d.content,'Play',[160,d.actionY,760,Kit.PLAY_H],'white_play',()=>d.invoke(onPlay??(()=>this.callbacks.play(level))));UI.enabled(play,Progress.is_level_unlocked(this.profile,level));return d;
  }
  item_offer(title:string,picture:string,price:number,onBuy:()=>void,placement:string,onAd:()=>void):Dialog {
    const d=Kit.jungle(this.host,title,[280,88],{close:true});UI.art(d.content,picture,[400,d.ys[0],280,280]);Kit.number(d.content,'+1',[340,d.ys[1],400,88]);
    const entry=this.adEntry(placement),buy=Kit.button(d.content,'Buy',[entry==='hidden'?325:94,d.actionY,430,Kit.BUTTON_H],'',()=>d.invoke(onBuy));Kit.price(buy,[430,Kit.BUTTON_H],price);
    if(entry!=='hidden')this.adButton(d.content,[556,d.actionY,430,Kit.BUTTON_H],entry,()=>this.watch(placement,()=>d.invoke(onAd)));return d;
  }
  /** Ad entries are hidden without ads in this build or a loaded ad; a placement past today's cap is dimmed. */
  adEntry(placement:string):AdEntry{return this.callbacks.ad?.(placement)??'hidden';}
  watch(placement:string,done:()=>void):void{this.callbacks.rewarded?.(placement,done);}
  adButton(parent:Node,r:Rect,entry:AdEntry,action:()=>void,suffix=''):Node{
    const ad=Kit.button(parent,'WatchAd',r,'',action,'orange'),cap=r[3]>=140?Kit.WORD_CAP:Kit.COMPACT_CAP,icon=cap*1.4,cy=r[3]*Kit.FACE_CENTRE;
    if(!suffix)UI.art(ad,'icon_watch_ad',[(r[2]-icon)/2,cy-icon/2,icon,icon]);
    else{const w=icon+12+Kit.numberWidth(suffix,cap),x=(r[2]-w)/2;UI.art(ad,'icon_watch_ad',[x,cy-icon/2,icon,icon]);Kit.number(ad,suffix,[x+icon+12,cy-cap/2,w-icon-12,cap],-1);}
    UI.enabled(ad,entry==='ready');return ad;
  }
  offer_tool(kind:number,done:()=>void=()=>{}):Dialog {
    const grant=()=>{this.profile.inventory[kind]++;this.save();done();this.sound('collect');};
    return this.item_offer('item_'+BOOSTER_WORD[kind],'island_icon_'+BOOSTER_ART[kind],Progress.booster_price(kind),()=>{if(this.profile.coins<Progress.booster_price(kind)){this.callbacks.coins?.();return;}this.profile.coins-=Progress.booster_price(kind);grant();},Ads.TOOL,grant);
  }
  tool_purchase(kind:number,done:()=>void=()=>{}):Dialog {
    const it=Shop.item('booster.'+Progress.BOOSTERS[kind][1])!,d=Kit.purchase(this.host,'item_'+BOOSTER_WORD[kind],[440,80,130]);
    Kit.glow(d.content,[320,d.ys[0],440,440]);UI.art(d.content,'island_icon_'+BOOSTER_ART[kind],[320,d.ys[0],440,440]);Kit.lettering(d.content,'cap_'+BOOSTER_WORD[kind],[164,d.ys[1],752,80]);
    let quantity=1;const max=Math.max(1,Math.floor(this.profile.coins/it.price));const qty=UI.group(d.content,'Quantity',[435,d.ys[2]+1,210,128]);Kit.face(qty,[0,0,210,128],'plate');
    const digits=UI.group(qty,'Value',[16,24,178,80]);let minus:Node,plus:Node,buy:Node,price:Node;let input:EditBox;
    const refresh=()=>{if(input)input.string=String(quantity);UI.clear(digits);Kit.number(digits,quantity,[0,0,178,80]);UI.enabled(minus,quantity>1);UI.enabled(plus,quantity<max);UI.enabled(buy,this.profile.coins>=quantity*it.price);UI.clear(price);Kit.price(price,[680,Kit.BUTTON_H],quantity*it.price);};
    minus=Kit.button(d.content,'Decrease',[281,d.ys[2],130,130],'',()=>{quantity--;refresh();},'key_cream');UI.fill(minus,'Minus',[43,55,44,12],'#298514',6);
    plus=Kit.button(d.content,'Increase',[669,d.ys[2],130,130],'',()=>{quantity++;refresh();},'key_cream');UI.fill(plus,'PlusHorizontal',[43,55,44,12],'#298514',6);UI.fill(plus,'PlusVertical',[59,39,12,44],'#298514',6);
    buy=Kit.button(d.content,'Buy',[200,d.actionY,680,Kit.BUTTON_H],'',()=>{if(Shop.purchase(this.profile,it.id,quantity)!=='')return;this.save();this.sound('collect');d.close();done();},'wide');price=UI.group(buy,'Price',[0,0,680,Kit.BUTTON_H]);refresh();
    // EditBox supplies keyboard input only. Its text stays transparent; the painted digits remain visible.
    const editorNode=UI.group(qty,'QuantityInput',[0,0,210,128]);editorNode.active=false;input=editorNode.addComponent(EditBox);input.inputMode=EditBox.InputMode.NUMERIC;input.returnType=EditBox.KeyboardReturnType.DONE;input.maxLength=String(max).length;
    const label=UI.group(editorNode,'InputText',[16,24,178,80]).addComponent(Label);label.color=new Color(255,255,255,0);label.fontSize=60;label.horizontalAlign=Label.HorizontalAlign.CENTER;input.textLabel=label;input.string=String(quantity);
    const caret=UI.fill(qty,'Caret',[164,32,3,56],'#ffffff',1);caret.active=false;
    editorNode.on('editing-did-began',()=>caret.active=true);editorNode.on('editing-did-ended',()=>{caret.active=false;refresh();});
    editorNode.on('text-changed',(edit:EditBox)=>{const number=parseInt(edit.string.replace(/[^0-9]/g,''),10);quantity=Number.isNaN(number)?1:Math.max(1,Math.min(max,number));refresh();});
    input.enabled=this.profile.coins>=it.price;editorNode.active=true;return d;
  }
  /**
   * The win page (fishsort's game/result.ts, spaced out): three stars; the red ribbon with the title; the played world's birds cheering
   * on a branch over turning rays; small badges for a first three-star clear and a reached combo goal; one reward bar holding the
   * feather total (counting up, the level-kind tag on it) and the tools won; a large Next; Home, Retry and Get x2 in one row.
   */
  win(result:{stars:number;moves:number;par:number;reward:number;boosters:number[];world?:number;kind?:string;first3?:number;first3Tool?:number;comboGoal?:number;comboCoins?:number},onNext:()=>void,onReplay:()=>void,onHome:()=>void,nextEnabled=true):Dialog {
    const world=result.world??1,entry=result.reward>0?this.adEntry(Ads.DOUBLE_COINS):'hidden';
    const d=Kit.layer(this.host,'Win',undefined,true,`board_w${world}${world===1||world===Levels.WORLDS?'_open':''}_bg`),c=d.content;
    UI.fill(c,'Veil',[0,0,1080,1920],'#00123C50');
    // stars
    for(let i=0;i<3;i++){const side=i===1?230:180,star=UI.art(c,i<result.stars?'island_star_full':'island_star_empty',[540+(i-1)*250-side/2,(i===1?170:200)-side/2,side,side]);Kit.centerPivot(star);
      if(i<result.stars&&!Kit.instant){star.setScale(.2,.2,1);tween(star).delay(.35+i*.2).to(.22,{scale:new Vec3(1.2,1.2,1)},{easing:'quadOut'}).to(.14,{scale:new Vec3(1,1,1)}).start();Kit.after(d.node,.35+i*.2,()=>this.sound('star_'+(i+1)));}}
    // ribbon and title
    const ribbon=UI.group(c,'Ribbon',[80,320,920,250]);Flat.plateIn(ribbon,'f_ribbon',[0,0,920,250]);
    Flat.text(ribbon,tr('WIN_TITLE'),[150,Flat.face(250)-70,620,140],104,onTone('red'));
    // the world's birds on a branch over turning rays
    const rays=Fx.piece(c,'fx_rays',540,800,760,760);UI.opacity(rays,.5);if(!Kit.instant)tween(rays).repeatForever(tween().by(24,{angle:360})).start();
    Kit.perch(c,[540,975],720);
    ([['sky',240,210],['sunny',540,260],['berry',840,210]] as [string,number,number][]).forEach(([slot,x,side],i)=>{const b=Kit.liveBird(c,Worlds.look(world,slot),[x-side/2,975-side*1.1,side,side*1.1]);Kit.after(d.node,.6+i*.15,()=>b.showcase());});
    // achievement badges: their feathers are in the total below
    const badges:[string,string][]=[];if(result.first3)badges.push(['medal',tr('RESULT_FIRST_3STAR')]);if(result.comboCoins)badges.push(['combo',tr('COMBO_GOAL')]);
    const bw=380,bgap=30;badges.forEach(([kind,label],i)=>{const x=540-(badges.length*bw+(badges.length-1)*bgap)/2+i*(bw+bgap),n=UI.group(c,'Badge_'+kind,[x,1030,bw,84]);Flat.plateIn(n,'f_dock',[0,0,bw,84]);
      if(kind==='medal')UI.art(n,'art/flat/medal_three_stars',[16,6,72,72]);else Flat.text(n,'×'+result.comboGoal,[8,0,96,84],52,{color:'#FFD640',outline:'#C47400'});
      Flat.text(n,label,[100,0,bw-120,84],40,ON_LIGHT,-1);Kit.pop(n,1.3+i*.15);});
    // the reward bar: feather total and tools won
    const total=result.reward+(result.first3??0)+(result.comboCoins??0),tools=new Map<number,number>();
    for(const kind of [...result.boosters,...(result.first3Tool!==undefined?[result.first3Tool]:[])])tools.set(kind,(tools.get(kind)||0)+1);
    const bar=UI.group(c,'Rewards',[90,1140,900,210]);Flat.card(bar,'f_card',[0,0,900,210],240,116);
    const cells:[string,number][]=[];if(total>0)cells.push(['icon_purchase_feather',total]);tools.forEach((n,kind)=>cells.push(['island_icon_'+BOOSTER_ART[kind],n]));
    const cw=cells.length?Math.min(420,860/cells.length):0;let feathers:Label|null=null;
    cells.forEach(([pic,n],i)=>{const cell=UI.group(bar,'Cell'+i,[450+(i-(cells.length-1)/2)*cw-cw/2,0,cw,210]),big=i===0&&total>0,ic=big?130:110;
      UI.art(cell,pic,[cw/2-ic-8,105-ic/2,ic,ic]);const l=Flat.text(cell,'+'+n,[cw/2+8,40,cw/2-8,130],big?76:64,ON_LIGHT,-1);if(big)feathers=l;
      if(big&&result.kind&&result.kind!=='normal')Flat.kindTag(cell,result.kind,cw/2-ic-30,-26,52);Kit.pop(cell,1+i*.15);});
    if(feathers&&!Kit.instant){const l:Label=feathers,state={n:0};l.string='+0';tween(state).delay(1).to(.7,{n:total},{onUpdate:()=>{if(l.isValid)l.string='+'+Math.round(state.n);}}).start();}
    if(!cells.length)Flat.text(bar,tr('UI_REPLAY_NO_REWARD'),[30,0,840,210],46,ON_LIGHT);
    // actions
    const next=Flat.button(c,'action_next',[160,1410,760,180],'green',tr('UI_NEXT_LEVEL'),()=>d.invoke(onNext),80);UI.enabled(next,nextEnabled);
    if(!Kit.instant)Kit.breathe(next,2);
    const row:[string,string,'blue'|'amber',()=>void][]=[['Home',tr('H_TO_HOME'),'blue',()=>d.invoke(onHome)],['Retry',tr('UI_REPLAY'),'blue',()=>d.invoke(onReplay)]];
    let doubled=false;
    if(entry!=='hidden')row.push(['WatchAd',tr('WIN_DOUBLE'),'amber',()=>{if(doubled)return;this.watch(Ads.DOUBLE_COINS,()=>{doubled=true;this.profile.coins+=result.reward;this.save();this.sound('collect');UI.enabled(c.getChildByName('WatchAd')!,false);if(feathers){const l:Label=feathers;l.string='+'+(total+result.reward);Kit.pop(l.node.parent!);}});}]);
    const w=row.length===3?280:360,gap=30,left=540-(row.length*w+(row.length-1)*gap)/2;
    row.forEach(([name,label,tone,act],i)=>{const r:Rect=[left+i*(w+gap),1640,w,130];const b=tone==='amber'?Flat.iconButton(c,name,r,'amber','f_g_video',label,act,44):Flat.button(c,name,r,'blue',label,act,46);if(name==='WatchAd')UI.enabled(b,entry==='ready');});
    this.confetti(d.node);return d;
  }
  /** "▶ Claim ×2": the doubled-feathers action, laid out like the other action words. */
  private claimDouble(parent:Node,r:Rect,entry:AdEntry,act:()=>void):Node{
    const b=Kit.button(parent,'WatchAd',r,'',act,'orange'),cap=Kit.WORD_CAP,cy=r[3]*Kit.FACE_CENTRE,icon=cap*1.4,ww=Kit.wordWidth('white_claim',[r[2],r[3]]),nw=Kit.numberWidth('×2',cap),gap=14,total=icon+gap+ww+gap+nw,x=(r[2]-total)/2;
    UI.art(b,'icon_watch_ad',[x,cy-icon/2,icon,icon]);Kit.word(b,'white_claim',[r[2],r[3]],x+icon+gap-ww*.08/.84,ww/.84);Kit.number(b,'×2',[x+icon+gap+ww+gap,cy-cap/2,nw,cap],-1);
    UI.enabled(b,entry==='ready');return b;
  }



  private count(parent:Node,value:number,rect:Rect,delay:number,prefix=''):void {
    const holder=UI.group(parent,'Counter',rect);let shown=-1;const state={n:Kit.instant?value:0};const update=()=>{const n=Math.round(state.n);if(n===shown)return;shown=n;UI.clear(holder);Kit.number(holder,prefix+n,[0,0,rect[2],rect[3]]);};update();
    if(!Kit.instant)tween(holder).delay(delay).to(.65,{}, {onUpdate:(_target,ratio)=>{state.n=value*ratio!;update();}}).start();
  }
  /** Three birds of the world just played cheer above the popup. */
  private celebration(d:Dialog,world:number):void { [['sunny',432,216],['sky',208,184],['berry',688,184]].forEach(([s,x,side],i)=>{const size=Number(side),b=Kit.liveBird(d.content,Worlds.look(world,String(s)),[Number(x),d.rect[1]+28-size,size,size]);if(!Kit.instant){const p=b.node.position.clone();b.node.setPosition(p.x+(i===1?-420:420),p.y+200,p.z);tween(b.node).delay(.45+i*.15).to(.9,{position:p},{easing:'quadOut'}).call(()=>b.showcase()).start();}}); }
  private confetti(parent:Node):void { if(Kit.instant)return;['#f5b53c','#71d850','#5bd7db','#ed82aa'].forEach((color,k)=>{for(let i=0;i<10;i++){const x=80+(i*97+k*43)%940,p=UI.fill(parent,'Confetti',[x,-50-k*40,12,22],color,3),pos=p.position.clone();tween(p).delay(.3+k*.1+i*.04).to(2.7,{position:new Vec3(pos.x+Math.sin(i)*160,pos.y-2050,0),angle:360+i*25}).call(()=>Kit.destroy(p)).start();}}); }
  goal_info(species:string,world=1):Dialog {
    const effects:Record<string,string>={sunny:'wake',sky:'reveal',berry:'encore',mint:'grow',cocoa:'scare',snow:'undo',ember:'hatch',ink:'encore',hoot:'wake'},effect=effects[species];
    const heights=effect?[330,140,128]:[330,128],body=140+heights.reduce((a,b)=>a+b,0)+(heights.length-1)*24,top=Math.round((1920-96-body-40-162)/2);
    const d=Kit.layer(this.host,'GoalInfo');const bg=UI.image(d.node,'background_home_jungle',[0,0,1080,1920]);bg.setSiblingIndex(0);d.rect=[120,top+96,840,body];d.actionY=top+96+body+40;Kit.panel(d.content,[120,top,840,body+96]);Kit.sign(d.content,'bird_'+Worlds.look(world,species),[220,top-44,640,223]);Kit.close(d.content,[775,top+30],d.close);
    const bird=Worlds.look(world,species);let y=top+196;Kit.liveBird(d.content,bird,[375,y,330,330]);y+=354;
    if(effect){
      const row=UI.group(d.content,'Trigger',[0,y,1080,140]),faces:BirdView[]=[];
      for(let i=0;i<4;i++)faces.push(Kit.liveBird(row,bird,[150+i*88,28,84,84]));
      const arrow=(x:number)=>{const node=UI.group(row,'Arrow',[x,46,48,48]),g=node.addComponent(Graphics);g.fillColor=new Color('#f8b33c');g.moveTo(2,-16);g.lineTo(28,-16);g.lineTo(28,-4);g.lineTo(46,-24);g.lineTo(28,-44);g.lineTo(28,-32);g.lineTo(2,-32);g.close();g.fill();};arrow(512);UI.art(row,'house_default',[574,18,104,104]);arrow(692);
      const result=UI.group(row,'Result',[754,8,124,124]),badge=UI.art(row,'gift_coin_'+effect,[594,38,64,64]);badge.active=false;
      const reset=()=>{UI.clear(result);if(effect==='wake')BirdView.create(result,Worlds.look(world,species==='sky'?'sunny':'sky'),[0,0,124,124],{asleep:true});else if(effect==='reveal'||effect==='hatch')BirdView.create(result,Worlds.look(world,'sky'),[0,0,124,124],{code:Board.bird(0,1,Board.EGG)});else if(effect==='scare')Kit.liveBird(result,'crow',[0,0,124,124]);else UI.art(result,effect==='grow'?'island_icon_sprout':effect==='undo'?'island_icon_undo':'icon_star_budget_steps',[12,12,100,100]);};reset();
      if(!Kit.instant){const originals=faces.map(b=>b.node.position.clone());const loop=tween(row).delay(.5);faces.forEach((bird,i)=>loop.call(()=>bird.showcase()).delay(.23));loop.call(()=>faces.forEach((bird,i)=>Kit.after(row,i*.09,()=>{bird.flyTo([590,38],.45);Kit.after(row,.45,()=>bird.node.active=false);}))).delay(.9).call(()=>{badge.active=true;badge.setPosition(594,-38,0);}).call(()=>{tween(badge).to(.4,{position:new Vec3(784,-38,0)},{easing:'sineInOut'}).call(()=>{badge.active=false;UI.clear(result);if(effect==='wake'||effect==='reveal'||effect==='hatch')Kit.liveBird(result,Worlds.look(world,'sky'),[0,0,124,124]).showcase();else if(effect==='scare')Kit.liveBird(result,'crow',[0,0,124,124]).flyTo([200,-100],.5);else if(effect==='grow')UI.art(result,'island_icon_sprout',[-8,-8,140,140]);else Kit.number(result,'+1',[8,36,108,60]);}).start();}).delay(1.8).call(()=>{faces.forEach((b,i)=>{b.node.active=true;b.node.setPosition(originals[i]);});reset();});tween(row).repeatForever(loop).start();}
      y+=164;
    }
    UI.word(d.content,'bird.'+species+'.ability',[172,y,736,128]);Kit.button(d.content,'Confirm',[208,d.actionY,664,Kit.BUTTON_H],'white_action_ok',d.close,'wide');return d;
  }
  daily_reward():Dialog {
    const today=Progress.daily_reward_day(this.profile,Progress.today()),available=Progress.daily_reward_available(this.profile,Progress.today());
    const d=Kit.page(this.host,'DailyReward','daily_reward_title',this.profile.coins,this.callbacks.close,this.callbacks.coins),h=(1196-60)/4,w=482;
    const tiles:Node[]=[];
    Progress.DAILY_REWARDS.forEach(([coins,gifts],i)=>{const day=i+1,big=i===6,width=big?984:w,x=48+(i%2)*(w+20),y=352+Math.floor(i/2)*(h+20);const tile=UI.group(d.content,'Day'+day,[x,y,width,h]);tiles.push(tile);const body=UI.group(tile,'Body',[0,0,width,h]);Kit.panel(body,[0,0,width,h]);
      const claimed=day<today||(day===today&&!available);const art=coins>=400?DRAW_PRIZE_ART[7]:coins>=200?DRAW_PRIZE_ART[4]:coins>=120?DRAW_PRIZE_ART[3]:coins>=80?DRAW_PRIZE_ART[1]:coins>=60?DRAW_PRIZE_ART[6]:DRAW_PRIZE_ART[0];
      const items:Array<[string,string]>=[[art,String(coins)]];if(gifts)items.push([gifts>1?DRAW_PRIZE_ART[5]:DRAW_PRIZE_ART[2],'×'+gifts]);const col=big?240:(width-72)/items.length,left=(width-col*items.length)/2;
      if(big){const glow=UI.image(body,'glow_soft',[width/2-300,-10,600,h+20]);UI.opacity(glow,.55);[[150,60],[790,70],[220,200],[720,190]].forEach(([x,y],i)=>Kit.breathe(UI.art(body,'sparkle',[x,y,48,48]),i*.35));}items.forEach(([pic,amount],n)=>{UI.art(body,pic,[left+n*col+10,36,col-20,h-132]);Kit.number(body,amount,[left+n*col,h-86,col,58]);});
      UI.image(body,'island_badge_green',[14,14,76,76]);Kit.number(body,day,[24.64,29.2,54.72,42.56]);if(claimed){UI.opacity(body,.45);UI.check_badge(tile,[width-44,44]);}else if(day===today){const g=UI.group(tile,'Today',[-4,-4,width+8,h+8]).addComponent(Graphics);g.lineWidth=8;g.strokeColor=new Color('#3fb52b');g.roundRect(0,-h-8,width+8,h+8,48);g.stroke();Kit.breathe(tile);}
    });
    // Watching an ad doubles today's feathers (tools are not doubled).
    const entry=available?this.adEntry(Ads.DAILY_DOUBLE):'hidden';let busy=false;
    const take=(double:boolean)=>{if(busy)return;const got=Progress.claim_daily_reward(this.profile,Progress.today());if(!got.day)return;if(double){this.profile.coins+=got.coins;got.coins*=2;}busy=true;this.save();UI.enabled(claim,false);if(ad)UI.enabled(ad,false);const tile=tiles[got.day-1];Kit.stop(tile);UI.opacity(tile.getChildByName('Body')!,.45);UI.check_badge(tile,[UI.rect(tile)[2]-44,44]);this.coin_flight(d.content,[540,UI.rect(tile)[1]+h/2],[792,96],()=>{this.sound('collect');Kit.after(d.node,.3,()=>this.rewards('daily_reward.title',got.coins,got.boosters,()=>{d.close();if(got.outfit)this.outfit_gift(got.outfit);},'pic_calendar'));});};
    const claim=Kit.button(d.content,'Claim',[entry==='hidden'?325:94,1613,430,Kit.BUTTON_H],'white_claim',()=>take(false));UI.enabled(claim,available);
    const ad=entry==='hidden'?null:this.adButton(d.content,[556,1613,430,Kit.BUTTON_H],entry,()=>this.watch(Ads.DAILY_DOUBLE,()=>take(true)),'×2');return d;
  }
  lucky_draw():Dialog {
    Progress.begin_draw_day(this.profile,Progress.today());this.save();const d=Kit.page(this.host,'DailyDraw','lucky_spin_title',undefined,this.callbacks.close),scale=1920/1717;
    const canvas=UI.group(d.content,'ApprovedLayout',[(1080-916*scale)/2,0,916,1717]);canvas.setScale(scale,scale,1);
    const slots=Math.min(Progress.DRAW_MAX_SPINS,Math.max(this.profile.draws_left,Progress.daily_reward_day(this.profile,Progress.today()))),stars:Node[]=[];
    for(let i=0;i<slots;i++)stars.push(UI.art(canvas,i<this.profile.draws_left?'star_draw_available':'star_draw_spent',[458+(i-(slots-1)/2)*122-54,341,108,104]));
    const board=UI.group(canvas,'DrawBoard',[39,486,838,846]);for(let i=0;i<9;i++)Kit.panel(board,[50+(i%3)*251,62+Math.floor(i/3)*247,232,224]);
    const cards=Progress.DRAW_PRIZES.map((prize,i)=>{const at=DRAW_SPOTS[i],tile=UI.group(board,'Prize'+i,[46+at[0]*251,58+at[1]*247,240,232]);UI.image(tile,DRAW_PRIZE_ART[i],DRAW_PICTURES[i]);Kit.number(tile,prize[0]>0?prize[0]:'×'+prize[1],[14,159,212,51]);return tile;});
    Kit.breathe(UI.art(board,'icon_draw_center_chest',[311,305,216,224]));let busy=false,at=0;
    // An ad buys one more spin (Ads.DAILY_CAP).
    const entry=this.adEntry(Ads.DRAW);
    const spinOnce=()=>{
      if(busy)return;const got=Progress.draw(this.profile,Progress.today());if(got.cell===undefined)return;this.save();busy=true;UI.enabled(spin,false);if(ad)UI.enabled(ad,false);
      if(!Progress.unlimited&&stars[this.profile.draws_left]){const old=stars[this.profile.draws_left],r=UI.rect(old);Kit.destroy(old);stars[this.profile.draws_left]=UI.art(canvas,'star_draw_spent',r);}
      const steps=16+(got.cell-at+8)%8,roll=tween(d.node);for(let n=0;n<steps;n++){const selected=(at+n+1)%8;roll.call(()=>{cards.forEach((c,i)=>{UI.opacity(c,i===selected?1:.72);c.setScale(i===selected?1.06:1,i===selected?1.06:1,1);});this.sound('ui_tap');}).delay(Kit.instant?0:.04+.2*Math.pow(n/(steps-1),3));}
      roll.delay(Kit.instant?0:.55).call(()=>{at=got.cell;const boosters=got.booster>=0?Array(got.count).fill(got.booster):[];this.draw_rewards(got.coins,boosters,()=>{busy=false;cards.forEach(c=>{UI.opacity(c,1);c.setScale(1,1,1);});UI.enabled(spin,this.profile.draws_left>0);if(ad)UI.enabled(ad,this.adEntry(Ads.DRAW)==='ready');});}).start();
    };
    const spin=Kit.button(d.content,'Draw',[entry==='hidden'?325:94,1540,430,Kit.BUTTON_H],'white_spin',spinOnce);UI.enabled(spin,this.profile.draws_left>0);
    const ad=entry==='hidden'?null:this.adButton(d.content,[556,1540,430,Kit.BUTTON_H],entry,()=>{if(!busy)this.watch(Ads.DRAW,()=>{this.profile.draws_left++;this.save();spinOnce();});},'+1');return d;
  }
  chest():Dialog {
    const ready=Progress.chests_ready(this.profile),collected=Math.max(0,Math.min(Progress.CHEST_STARS,Progress.chest_progress(this.profile)));
    const d=Kit.page(this.host,'StarChest','star_chest_title',this.profile.coins,this.callbacks.close,this.callbacks.coins);this.chestHero(d,false);Kit.number(d.content,String(ready),[400,432,280,104]).name='ReadyCount';
    const progress=UI.group(d.content,'Progress',[168,1180,744,112]);UI.nine_region(progress,'panel_chest_progress',[112,285,1946,157],[63,13.44,681,85.12],[87,0,87,0],85.12/157);
    if(collected)UI.nine_region(progress,'fill_chest_progress',[160,273,1846,177],[72,22.44,Math.max(67,663*collected/Progress.CHEST_STARS),67.12],[98,0,98,0],67.12/177);UI.art(progress,'island_star_full',[-14,-14,140,140]);Kit.number(progress,`${collected}/${Progress.CHEST_STARS}`,[0,24,744,62]);
    let busy=false;const open=Kit.button(d.content,'OpenChest',[325,1343,430,Kit.BUTTON_H],'white_open',()=>{if(busy)return;busy=true;UI.enabled(open,false);UI.enabled(play,false);const got=Progress.open_chest(this.profile);if(got.coins===undefined)return;this.save();this.animateChest(d,()=>{d.close();this.chest_rewards(got.coins,got.boosters);});});UI.enabled(open,ready>0);
    const play=Kit.button(d.content,'Play',[325,1575,430,Kit.BUTTON_H],'white_play',()=>d.invoke(()=>this.callbacks.play(Progress.resume_level(this.profile))));return d;
  }
  private chestHero(d:Dialog,opened:boolean):Node {
    const rays=UI.image(d.content,'light_rays',[60,320,960,960]);UI.opacity(rays,.4);Kit.centerPivot(rays);if(!Kit.instant)tween(rays).repeatForever(tween().by(28,{angle:360})).start();
    Kit.glow(d.content,[235,560,610,480]);UI.art(d.content,'chest_islet_support',[145,950,790,214],[43,112,2082,559]);
    const chest=this.chestModel(d.content,opened?70:-70);Kit.breathe(chest.node);return chest.node;
  }
  private chestModel(parent:Node,angle:number):{node:Node;paint:(angle:number)=>void} {
    const k=610/1029,hero=UI.group(parent,'ChestHero',[540,1040,0,0]),source=UI.group(hero,'Source',[-614.5*k,-1090*k,1254,1254]);source.setScale(k,k,1);
    UI.image_region(source,'chest_star_open',[0,515,1254,739],[0,515,1254,739]);const lid=UI.group(source,'Lid',[0,0,1254,1254]);
    const lock=UI.group(source,'Lock',[0,0,1254,1254]);lock.addComponent(Mask).type=Mask.Type.GRAPHICS_STENCIL;const stencil=lock.getComponent(Graphics)!;stencil.clear();
    const points=[[561,550],[620,640],[720,650],[755,700],[681,750],[690,860],[640,903],[560,850],[475,906],[423,870],[435,760],[370,714],[380,665],[491,643]];
    points.forEach(([x,y],i)=>i?stencil.lineTo(x,-y):stencil.moveTo(x,-y));stencil.close();stencil.fill();UI.image(lock,'chest_star_open',[0,0,1254,1254]);
    // The lid's mask and both faces are built once; each frame redraws the mask and moves the visible face.
    const mask=UI.group(lid,'Projection',[0,0,1254,1254]);mask.addComponent(Mask).type=Mask.Type.GRAPHICS_STENCIL;const g=mask.getComponent(Graphics)!;
    const outer=UI.image_region(mask,'chest_lid_outer',[28,84,1714,691],[0,0,0,0]),inner=UI.image_region(mask,'chest_star_open',[179,86,959,429],[0,0,0,0]);
    const paint=(degrees:number)=>{const face=Math.sin(degrees*Math.PI/180)/Math.sin(70*Math.PI/180),extent=Math.abs(face),bottom=515+(face<0?125*extent:0),height=Math.max(14,(face<0?420:429)*extent),width=959+(face<0?32*extent:0),left=658.5-width/2,inset=24*(1-extent);
      g.clear();g.moveTo(left+inset,-bottom+height);g.lineTo(left+width-inset,-bottom+height);g.lineTo(left+width,-bottom);g.lineTo(left,-bottom);g.close();g.fill();
      outer.active=face<0;inner.active=face>=0;UI.place(face<0?outer:inner,[left,bottom-height,width,height]);
    };paint(angle);return {node:hero,paint};
  }
  private animateChest(d:Dialog,done:()=>void):void {
    const old=d.content.getChildByName('ChestHero')!;Kit.destroy(old);const chest=this.chestModel(d.content,-70);
    for(const name of ['OpenChest','Play','Progress','ReadyCount']){const node=d.content.getChildByName(name);if(node)UI.opacity(node,0);}
    if(Kit.instant){chest.paint(70);done();return;}
    const motion=tween(chest.node);for(const angle of [2.5,-4,5.5,-7,0])motion.to(.1,{angle},{easing:'sineInOut'});
    motion.to(.12,{scale:new Vec3(1.08,.9,1)},{easing:'quadIn'}).call(()=>this.sound('crate_open'))
      .to(.42,{}, {easing:'backOut',onUpdate:(_target,ratio)=>{chest.paint(-70+144*ratio!);chest.node.setScale(1,1,1);}})
      .call(()=>this.sound('star_1')).to(.22,{}, {easing:'sineInOut',onUpdate:(_target,ratio)=>chest.paint(74-4*ratio!)})
      .delay(.3).call(done).start();
  }
  private chest_rewards(coins:number,boosters:number[]):Dialog {
    const d=Kit.page(this.host,'ChestRewards','star_chest_title',this.profile.coins);const close=d.content.getChildByName('Close');if(close)Kit.destroy(close);this.chestHero(d,true);const prizes=Kit.prizes(d.content,coins,boosters,1172,4);
    for(const p of prizes){const target=p.position.clone();p.setPosition(540,-710,0);p.setScale(.25,.25,1);tween(p).to(.85,{position:target,scale:new Vec3(1,1,1)},{easing:'quadOut'}).start();}
    const ok=Kit.button(d.content,'ConfirmRewards',[325,1423,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(()=>this.callbacks.close?.()));UI.enabled(ok,false);Kit.after(d.node,.3,()=>UI.enabled(ok,true));return d;
  }
  rewards(title:string,coins:number,boosters:number[],onOK:()=>void=()=>{},picture='pic_chest_open'):Dialog {
    if(title==='draw.title')return this.draw_rewards(coins,boosters,onOK);
    const count=(coins>0?1:0)+new Set(boosters).size,rows=Math.ceil(count/3),hero=rows>1?320:440;
    const d=Kit.jungle(this.host,Kit.titleKey(title),count?[hero,rows*250+Math.max(0,rows-1)*20]:[hero],{footer:72,width:count>=3?880:840});Kit.glow(d.content,[540-hero/2,d.ys[0],hero,hero]);const image=UI.art(d.content,picture,[570-hero/2,d.ys[0]+30,hero-60,hero-60]);Kit.breathe(image);if(count)Kit.prizes(d.content,coins,boosters,d.ys[1]);Kit.button(d.content,'Confirm',[325,d.actionY,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));this.sound('collect');return d;
  }
  private draw_rewards(coins:number,boosters:number[],onOK:()=>void):Dialog {
    const d=Kit.page(this.host,'DrawResult','lucky_spin_title'),close=d.content.getChildByName('Close');if(close)Kit.destroy(close);
    const picture=coins>0?DRAW_PRIZE_ART[Progress.DRAW_PRIZES.findIndex(p=>p[0]===coins)]:'island_icon_'+BOOSTER_ART[boosters[0]];
    const rays=UI.image(d.content,'light_rays',[180,570,720,720]);Kit.centerPivot(rays);UI.opacity(rays,.65);if(!Kit.instant)tween(rays).repeatForever(tween().by(28,{angle:360})).start();Kit.glow(d.content,[270,690,540,420]);Kit.breathe(UI.art(d.content,picture,[310,720,460,390]));Kit.number(d.content,'+'+(coins||boosters.length),[290,1170,500,100]);Kit.button(d.content,'Confirm',[325,1540,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));this.sound('collect');return d;
  }
  new_friend(species:string,reward:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'popup_new_friend_title',[440,124.8,128,120],{footer:72,width:880});Kit.liveBird(d.content,species,[350,d.ys[0]+20,380,400]);Kit.lettering(d.content,'bird_'+species,[220,d.ys[1],640,124.8]);UI.word(d.content,'bird.'+Worlds.slot_of(species)+'.ability',[172,d.ys[2],736,128]);UI.price_pill(d.content,[350,d.ys[3],380,120],reward);Kit.button(d.content,'Confirm',[325,d.actionY,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  /** A short message that fades by itself (fishsort's toast). */
  notice(words:string):void {
    const n=UI.group(this.host,'Notice',[140,880,800,120]);Flat.plateIn(n,'f_hud',[0,0,800,120]);Flat.text(n,words,[40,0,720,120],48,ON_DARK);this.sound('error');
    const op=n.addComponent(UIOpacity);tween(op).delay(1.2).to(.3,{opacity:0}).call(()=>Kit.destroy(n)).start();
  }
  /** A feature that just opened (fishsort's new-feature popup): its home icon and name. */
  feature_open(feature:string,onOK:()=>void=()=>{}):Dialog {
    const [icon,content,label]=FEATURE_ENTRY[feature],p=Flat.popup(this.host,'FeatureOpen',800,560,tr('H_UNLOCK_TITLE'),onOK);
    const d:Dialog={node:p.node,content:p.body,rect:p.rect,ys:[],actionY:0,close:p.close,invoke(action){p.close();action();}};
    Kit.glow(p.body,[260,30,280,280]);Kit.breathe(UI.art(p.body,icon,[270,40,260,260],content));
    Flat.text(p.body,tr(label),[0,320,800,80],58,ON_LIGHT);
    Flat.button(p.body,'Confirm',[200,420,400,120],'green',tr('UI_OK'),()=>d.invoke(()=>{}),50);this.sound('collect');return d;
  }
  /** A skin given as a prize (`<bird>_outfit_<n>`): the bird wearing it under the skin's name. */
  outfit_gift(outfit:string,onOK:()=>void=()=>{}):Dialog {
    const [bird,n]=outfit.split('_outfit_'),d=Kit.jungle(this.host,'outfit_'+Worlds.design(bird,Number(n)).join('_'),[440],{footer:72,width:880});
    Kit.glow(d.content,[320,d.ys[0],440,440]);Kit.liveBird(d.content,bird,[390,d.ys[0]+50,300,330],outfit).showcase();
    Kit.button(d.content,'Confirm',[325,d.actionY,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));this.sound('collect');return d;
  }
  unlock(booster:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'item_'+BOOSTER_WORD[booster],[440,100],{footer:72,width:900});Kit.breathe(UI.art(d.content,'island_icon_'+BOOSTER_ART[booster],[360,d.ys[0]+40,360,360]));Kit.lettering(d.content,'cap_'+BOOSTER_WORD[booster],[132,d.ys[1]+18,816,64]);Kit.button(d.content,'Confirm',[325,d.actionY,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  world_complete(world:number,reward:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'popup_world_complete_title',[280,120],{footer:72,width:880,birdsAbove:true});this.celebration(d,world);Kit.stars(d.content,d.ys[0],3,true);UI.price_pill(d.content,[350,d.ys[1],380,120],reward);Kit.button(d.content,'Confirm',[325,d.actionY,430,Kit.BUTTON_H],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  world_unlock(world:number,onPlay:()=>void):Dialog {
    const h=720*777/855,d=Kit.jungle(this.host,'popup_world_unlock_title',[h,124.8],{footer:72});const k=720/855,stage=UI.group(d.content,'Gate',[540-509.5*k,d.ys[0]-100*k,1024,1024]);stage.setScale(k,k,1);UI.image(stage,'gate_backdrop_w'+world,[0,0,1024,1024]);UI.image(stage,'gate_arch',[0,0,1024,1024]);
    const left=UI.image(stage,'gate_door_left',[0,0,1024,1024]),right=UI.image(stage,'gate_door_right',[0,0,1024,1024]);if(!Kit.instant){tween(left).delay(.5).to(1.1,{scale:new Vec3(.139,1,1)},{easing:'quadOut'}).start();tween(right).delay(.5).to(1.1,{scale:new Vec3(.139,1,1)},{easing:'quadOut'}).start();}else{left.setScale(.139,1,1);right.setScale(.139,1,1);}
    Kit.lettering(d.content,'world_'+WORLD_ID[world],[220,d.ys[1],640,124.8]);Kit.button(d.content,'Play',[325,d.actionY,430,Kit.BUTTON_H],'white_play',()=>d.invoke(onPlay));return d;
  }
  coin_flight(parent:Node,from:Point,to:Point,onLand:()=>void):void {
    if(Kit.instant){onLand();return;}for(let i=0;i<9;i++){const c=UI.art(parent,'coin',[from[0]-24,from[1]-24,48,48]);const start=c.position.clone();tween(c).delay(.2+i*.05).to(.26,{position:new Vec3(start.x+Math.cos(i*2.4)*90,start.y+Math.sin(i*2.4)*90,0)},{easing:'quadOut'}).to(.42,{position:new Vec3(to[0],-to[1],0)},{easing:'quadIn'}).call(()=>{Kit.destroy(c);if(i===8)onLand();}).start();}
  }
  /** Free feathers for a rewarded ad (Ads.FREE_COINS, a few a day); there is no paid pack. */
  coin_shop():Dialog {
    const w=893,h=700,p=Flat.popup(this.host,'CoinShop',w,h,tr('FREE_FEATHERS'),this.callbacks.close),b=p.body,cap=Ads.DAILY_CAP[Ads.FREE_COINS];
    const d:Dialog={node:p.node,content:b,rect:p.rect,ys:[],actionY:0,close:p.close,invoke(action){p.close();action();}};
    Kit.glow(b,[w/2-150,30,300,300]);Kit.breathe(UI.art(b,'icon_purchase_feather',[w/2-130,40,260,260]));
    Flat.text(b,'+'+Ads.FREE_COIN_AMOUNT,[0,310,w,110],90,ON_LIGHT);
    const paint=()=>{UI.clear(state);const left=Ads.left(this.profile,Ads.FREE_COINS,Progress.today()),entry=this.adEntry(Ads.FREE_COINS);
      Flat.text(state,left>0?tr('SHOP_AD_LEFT',cap-left,cap):tr('SHOP_AD_DONE'),[0,0,w,60],40,{color:'#5E86C0'});
      const button=Flat.iconButton(state,'WatchAd',[w/2-260,80,520,161],entry==='ready'?'amber':'gray','f_g_video',tr('H_WATCH_AD'),()=>{if(entry==='ready')this.watch(Ads.FREE_COINS,()=>{this.profile.coins+=Ads.FREE_COIN_AMOUNT;this.save();this.sound('collect');this.callbacks.changed?.();paint();});});UI.enabled(button,entry==='ready');};
    const state=UI.group(b,'State',[0,430,w,250]);paint();return d;
  }
  static preview(host:Node,state:string,callbacks:PopupCallbacks):Dialog {
    const profile=new Profile();profile.coins=1280;profile.highest_unlocked=Levels.COUNT;profile.inventory=[3,2,1,1,0,2];const p=new Popups(host,profile,callbacks);
    if(state.startsWith('settings'))return p.settings(state.includes('game'));
    if(state.startsWith('level'))return p.level_card(1);
    if(state.startsWith('daily'))return p.daily_reward();
    if(state.startsWith('draw'))return p.lucky_draw();
    if(state.startsWith('chest'))return p.chest();
    if(state.startsWith('friend'))return p.new_friend('sunny',20);
    if(state.startsWith('unlock'))return p.unlock(0);
    if(state.startsWith('world_complete'))return p.world_complete(1,500);
    if(state.startsWith('world_unlock'))return p.world_unlock(2,()=>{});
    return p.win({stars:3,moves:12,par:14,reward:120,boosters:[0,1]},()=>{},()=>{},()=>{});
  }
}
