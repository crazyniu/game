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
}
export type AdEntry = 'hidden' | 'used' | 'ready';
export interface Dialog {
  node: Node; content: Node; rect: Rect; ys: number[]; actionY: number;
  close: () => void; invoke: (action: () => void) => void;
}
export interface CoinPack { id: string; coins: number; yuan: number }
export const BOOSTER_ART = ['firefly', 'sprout', 'owl_eye', 'bell', 'scarecrow', 'rain'];
export const BOOSTER_WORD = ['firefly', 'sprout', 'owl_eye', 'wake_bell', 'scarecrow', 'spring_rain'];
const WORLD_ID = ['', 'green', 'bamboo', 'autumn', 'snow', 'blossom', 'mushroom', 'lakeside', 'pine', 'rainforest', 'wisteria'];
const DRAW_PRIZE_ART = ['icon_draw_prize_forty','icon_draw_prize_eighty','icon_draw_mystery_single','icon_draw_prize_one_twenty','icon_draw_prize_two_hundred','icon_draw_mystery_double','icon_draw_prize_sixty','icon_draw_prize_four_hundred'];
const DRAW_SPOTS = [[0,0],[1,0],[2,0],[2,1],[2,2],[1,2],[0,2],[0,1]];
const DRAW_PICTURES: Rect[] = [[41,21,176,137],[32,15,203,142],[29,15,195,149],[7,3,234,161],[4,-2,240,165],[18,-3,221,166],[7,2,226,160],[9,0,222,161]];

/** The existing painted ButtonKit, with native Cocos nodes and unchanged design geometry. */
export class Kit {
  static instant = false;
  static numberWidth(value: string, height: number): number {
    return Array.from(value).reduce((width, c) => { const size = Art.size(Kit.glyph(c)); return width + size[0] * height / size[1] + height * .02; }, 0) - height * .02;
  }
  static glyph(c: string): string { return 'digit_ui_' + (({'/':'slash','+':'plus','×':'x','x':'x'} as Record<string,string>)[c] || c); }
  static number(parent: Node, value: string | number, rect: Rect, align = 0): Node {
    const text = String(value), box = UI.group(parent, 'Digits', rect);
    const h = Math.min(rect[3], rect[3] * rect[2] / Math.max(1, Kit.numberWidth(text, rect[3])));
    let x = (rect[2] - Kit.numberWidth(text,h)) * (.5 + align * .5);
    for (const c of text) { const key = Kit.glyph(c), size = Art.size(key), w = size[0] * h / size[1]; UI.image(box,key,[x,(rect[3]-h)/2,w,h]); x += w+h*.02; }
    return box;
  }
  static lettering(parent:Node,key:string,rect:Rect):Node { return UI.painted(parent,'lettering_'+key,rect); }
  static face(parent:Node, rect:Rect, kind='green'): Node {
    const asset = ({green:'btn_plate_green',key_green:'btn_key_green',key_cream:'btn_key_cream',field:'field_quantity'} as Record<string,string>)[kind];
    if (kind === 'wide') { const size = Art.size('btn_plate_green'); return UI.nine(parent,'btn_plate_green',rect,[size[1]*.6,0,size[1]*.6,0],rect[3]/size[1]); }
    return UI.image(parent,asset,rect,true);
  }
  static plate(parent:Node,rect:Rect,kind='green'):Node { const box=UI.group(parent,'Plate',rect); Kit.face(box,[0,0,rect[2],rect[3]],kind); return box; }
  static word(parent:Node,id:string,size:Point):Node { const cap=size[1]*.48; const rect:Rect=[size[0]*.16,(size[1]-cap*1.25)/2,size[0]*.68,cap*1.25]; const node=Art.pack().painted[id]?UI.painted(parent,id,rect):UI.word(parent,id,rect,'white');UI.effect(node,'white_ink');return node; }
  static button(parent:Node,name:string,rect:Rect,word:string,action:()=>void,kind='green'):Node {
    const hit=UI.hit_area(parent,name,rect,action); Kit.face(hit,[0,0,rect[2],rect[3]],kind); if(word) Kit.word(hit,word,[rect[2],rect[3]]); return hit;
  }
  static price(parent:Node,size:Point,value:number):Node {
    const h=size[1]*(size[0]>430?.58:.52), feather=h*.95, gap=h*.16;
    const width=Math.min(Kit.numberWidth(String(value),h),size[0]*.72-feather-gap), x=(size[0]-feather-gap-width)/2;
    const box=UI.group(parent,'FeatherPrice',[0,0,...size]);
    UI.image(box,'icon_purchase_feather',[x,size[1]*.47-feather/2,feather,feather],true);
    Kit.number(box,value,[x+feather+gap,size[1]*.47-h/2,width,h],-1); return box;
  }
  static panel(parent:Node,rect:Rect):Node { return UI.nine(parent,'panel_card_cream',rect,[120,120,120,120],.42); }
  static close(parent:Node,at:Point,action:()=>void,back=false):Node { return UI.button(parent,back?'Back':'Close',back?'btn_forest_back':'btn_purchase_close',[...at,129.72,133.07],action); }
  static sign(parent:Node,key:string,rect:Rect):Node { return Kit.lettering(parent,key,[rect[0]+rect[2]*.1,rect[1]+rect[3]*.42,rect[2]*.8,rect[3]*.56]); }
  static simpleWord(parent:Node,id:string,style:string,anchor:Point,cap:number,align=0):Node {
    const name=`label_${id}_${style}_${Art.locale}`,meta=Art.data('art/labels/labels'),asset='art/labels/'+name,size=Art.size(asset),k=cap/meta.cap;
    const node=UI.image(parent,asset,[anchor[0]-size[0]*k*(.5+.5*align),anchor[1]-meta[name]*k,size[0]*k,size[1]*k]);if(style==='ink'||style==='soft')UI.warm(node);return node;
  }
  static drawNumber(parent:Node,value:string,rect:Rect):Node {
    const names:Record<string,string>={'0':'zero','1':'one','2':'two','3':'three','4':'four','5':'five','6':'six','7':'seven','8':'eight','9':'nine','+':'plus','×':'multiply','¥':'yuan','/':'slash','-':'minus'},metadata=Art.data('art/island/draw_purchase_ink');
    const keys=Array.from(value).map(c=>'label_draw_purchase_'+names[c]),ink=keys.map(k=>metadata[k] as Rect),width=ink.reduce((a,r)=>a+r[2]/r[3]*rect[3]+rect[3]*.04,0)-rect[3]*.04,k=Math.min(1,rect[2]/width),box=UI.group(parent,'Number',rect);let x=(rect[2]-width*k)/2;
    keys.forEach((key,i)=>{const w=ink[i][2]/ink[i][3]*rect[3]*k;UI.art(box,key,[x,(rect[3]-rect[3]*k)/2,w,rect[3]*k],ink[i]);x+=w+rect[3]*.04*k;});return box;
  }
  static chestNumber(parent:Node,value:string,rect:Rect):Node {
    const keys=Array.from(value).map(c=>c==='/'?'label_chest_slash':'icon_home_island_digit_'+c),ink=keys.map(k=>k==='label_chest_slash'?[294,294,458,912] as Rect:Art.ink(k)),width=ink.reduce((a,r)=>a+r[2]/r[3]*rect[3]+rect[3]*.06,0)-rect[3]*.06,k=Math.min(1,rect[2]/width),box=UI.group(parent,'Number',rect);let x=(rect[2]-width*k)/2;keys.forEach((key,i)=>{const w=ink[i][2]/ink[i][3]*rect[3]*k;UI.art(box,key,[x,(rect[3]-rect[3]*k)/2,w,rect[3]*k],ink[i]);x+=w+rect[3]*.06*k;});return box;
  }
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
  static warmFrame(parent:Node,rect:Rect):void {
    UI.fill(parent,'OpaqueFace',[rect[0]+14,rect[1]+14,rect[2]-28,rect[3]-28],'#fff0c2',70*rect[2]/960);
    UI.nine_region(parent,'panel_warm_wood_rim',[28,28,1222,1174],rect,[100,100,100,100],rect[2]/1222);
    UI.fill(parent,'CleanFace',[rect[0]+26,rect[1]+26,rect[2]-52,rect[3]-52],'#fff0c2',70*rect[2]/960);
  }
  static toyFrame(parent:Node,rect:Rect):void {
    const k=rect[2]/1136;
    UI.fill(parent,'OpaqueFace',[rect[0]+24*k,rect[1]+24*k,rect[2]-48*k,rect[3]-48*k],'#fff0c2',90*k);
    UI.nine_region(parent,'panel_shop_toy_rim',[60,80,1136,1100],rect,[150,150,150,150],rect[2]/1136);
  }
  static jungle(host:Node,title:string,heights:number[],options:{width?:number; actionHeight?:number; close?:boolean; compact?:boolean; birdsAbove?:boolean; footer?:number; onClose?:()=>void}={}):Dialog {
    const width=options.width??960, head=options.compact?208:236, foot=options.compact?72:(options.footer??168), actionHeight=options.actionHeight??162;
    const inner=heights.reduce((a,b)=>a+b,0)+Math.max(0,heights.length-1)*24, above=options.birdsAbove?180:0;
    const height=head+inner+foot, top=Math.max(32,(1920-above-height-48-actionHeight)/2)+above;
    const d=Kit.layer(host,'Popup',options.onClose); d.rect=[(1080-width)/2,top,width,height]; d.actionY=top+height+48;
    Kit.warmFrame(d.content,d.rect); if(title) Kit.sign(d.content,title,[220,top-14,640,223]);
    let y=top+head; d.ys=heights.map(h=>{const at=y;y+=h+24;return at;});
    if(options.close) Kit.close(d.content,[d.rect[0]+width-185,top+(options.compact?48:156)],d.close);
    Kit.pop(d.content); return d;
  }
  static purchase(host:Node,title:string,heights:number[],actionHeight=162):Dialog {
    const width=heights.length>=3?880:840, inner=heights.reduce((a,b)=>a+b,0)+(heights.length-1)*24;
    const height=220+inner+(heights.length>=3?88:72), top=(1920-height-32-actionHeight)/2;
    const d=Kit.layer(host,'Purchase'); d.rect=[(1080-width)/2,top,width,height]; d.actionY=top+height+32;
    Kit.toyFrame(d.content,d.rect); Kit.sign(d.content,title,[270,top-14,540,223]); Kit.close(d.content,[d.rect[0]+width-185,top+48],d.close);
    let y=top+220; d.ys=heights.map(h=>{const at=y;y+=h+24;return at;}); Kit.pop(d.content); return d;
  }
  static page(host:Node,name:string,title:string,balance?:number,onClose?:()=>void,coins?:()=>void):Dialog {
    const background=['DailyReward','DailyDraw','DrawResult','CoinShop'].includes(name)?'background_shop_forest':'background_home_jungle';const d=Kit.layer(host,name,onClose,true,background); Kit.sign(d.content,title,[240,96,600,209]); Kit.close(d.content,[48,48],d.close);
    if(balance!==undefined) UI.feather_bar(d.content,[736,48],balance,coins); return d;
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
  settings(showRestart=false):Dialog {
    const h=24+128+132*4+40+(showRestart?256:116)+48, top=Math.round(960-h/2);
    const d=Kit.layer(this.host,'Settings');const shade=d.node.getChildByName('Dim')!.getComponent(Graphics)!;shade.clear();shade.fillColor=new Color(20,23,61,173);shade.rect(0,-1920,1080,1920);shade.fill();
    const paint=()=>{
      UI.clear(d.content); UI.fill(d.content,'Rim',[120,top,840,h],'#d6a870',48);UI.fill(d.content,'Card',[128,top+8,824,h-16],'#fff7e8',40);
      Kit.simpleWord(d.content,'settings','ink',[540,top+88],76);
      const close=UI.hit_area(d.content,'Close',[812,top+48,92,92],d.close);UI.fill(close,'Shadow',[-3,3,98,98],'#40230d28',49);UI.fill(close,'Lip',[0,0,92,92],'#ba8852',46);UI.fill(close,'Rim',[0,0,92,86],'#d6a870',46);UI.fill(close,'Face',[3,3,86,80],'#fff7e8',43);UI.line(close,[[28,25],[64,61]],'#965c2c',10);UI.line(close,[[64,25],[28,61]],'#965c2c',10);
      const rows=[['pic_music','music',!this.profile.music_muted],['pic_sound_on','sound',!this.profile.muted],['pic_vibration','vibration',this.profile.vibration],['pic_globe','language',false]] as const;
      rows.forEach(([icon,id,on],i)=>{
        const mid=top+152+132*(i+.5); if(i>0)UI.fill(d.content,'Divider',[168,mid-67.5,744,3],'#ead6b6',1.5);
        UI.art(d.content,icon,[168,mid-36,72,72]);Kit.simpleWord(d.content,id,'ink',[268,mid],54,-1);
        if(id==='language'){
          UI.fill(d.content,'LanguageRim',[572,mid-42,340,84],'#d6a870',42);UI.fill(d.content,'LanguageTrack',[575,mid-39,334,78],'#f1e2c8',39);
          ['en','zh'].forEach((code,k)=>{const selected=Art.locale===code; if(selected){UI.fill(d.content,'ChosenLip',[576+k*166,mid-38,162,76],'#3c8822',38);UI.fill(d.content,'Chosen',[576+k*166,mid-38,162,70],'#5cb83a',38);}Kit.simpleWord(d.content,'lang_'+code,selected?'white':'ink',[657+k*170,mid-2],40);UI.hit_area(d.content,'Language_'+code,[572+k*170,mid-42,170,84],()=>{this.profile.language=code;Art.setLocale(code);this.callbacks.language?.(code);this.save();paint();});});
        }else{
          const toggle=UI.hit_area(d.content,'Switch_'+id,[756,mid-42,156,84],()=>{if(id==='music')this.profile.music_muted=!this.profile.music_muted;else if(id==='sound')this.profile.muted=!this.profile.muted;else this.profile.vibration=!this.profile.vibration;this.callbacks.mute?.();this.save();paint();});
          UI.fill(toggle,'Lip',[0,0,156,84],on?'#3c8822':'#b8a892',42);UI.fill(toggle,'Track',[0,0,156,76],on?'#5cb83a':'#d6cab8',42);UI.fill(toggle,'Knob',[on?86:6,6,64,64],'#fffdf7',32);
        }
      });
      const y=top+152+528+40;
      const action=(id:string,x:number,at:number,fn:()=>void,green=true)=>{const b=UI.hit_area(d.content,id,[x,at,340,116],()=>d.invoke(fn));UI.fill(b,'Lip',[0,0,340,116],green?'#3c8822':'#d6a870',58);UI.fill(b,'Face',green?[0,0,340,108]:[3,3,334,102],green?'#5cb83a':'#fff7e8',58);Kit.simpleWord(b,id,green?'white':'ink',[170,54],52);};
      if(showRestart){action('restart',188,y,()=>this.callbacks.replay?.(),false);action('home',552,y,()=>this.callbacks.home?.());}
      action('help',370,y+(showRestart?140:0),()=>this.help());
    }; paint(); return d;
  }
  help():void { this.callbacks.help?.(); }
  level_card(level:number,onPlay?:()=>void):Dialog {
    const def=Levels.get_level(level), species=Levels.species_ids(Levels.create_board(def)), tools=Array.from(new Set([...Progress.fitting_boosters(def),Board.Booster.FIREFLY])).filter(kind=>Progress.is_booster_unlocked(this.profile,kind)).slice(0,4);
    const perRow=species.length<=4?4:5,tile=perRow===4?176:152,rows=Math.ceil(species.length/perRow);
    const mechanics=new Set<string>(); for(const b of def.branches){if(b.kind&&b.kind!=='normal')mechanics.add(b.kind);for(const v of b.birds){const t=String(v);if(t.endsWith(':sleep'))mechanics.add('sleep');if(t.startsWith('egg:'))mechanics.add('egg');if(t==='crow')mechanics.add('crow');if(t.endsWith(':mama')||t.endsWith(':chick'))mechanics.add('mama');}}
    const mechanicArt:Record<string,string>={sleep:'hoot_sleep',egg:'egg_idle',crow:'crow_front',twig:'icon_sapling',growing:'icon_sapling',swivel:'btn_swivel_green',mama:'mama_bow'};
    const listed=Array.from(mechanics).filter(k=>mechanicArt[k]);const heights=[224,100];if(rows)heights.push(rows*tile+(rows-1)*20);if(listed.length)heights.push(96);if(tools.length)heights.push(188);
    const d=Kit.jungle(this.host,'',heights,{actionHeight:200,close:true,compact:true});
    const titleHeight=223*.56,digitHeight=titleHeight*.92,text=String(level),digitWidths=Array.from(text).map(c=>Art.size('digit_title_'+c)[0]*digitHeight/Art.size('digit_title_'+c)[1]),digitsWidth=digitWidths.reduce((a,b)=>a+b,0)-Math.max(0,text.length-1)*digitHeight*.06;
    const word=Kit.lettering(d.content,'popup_level_title',[316,d.rect[1]+79.66,448-digitsWidth-16,titleHeight]);const wr=UI.rect(word),visibleWidth=wr[2],left=540-(visibleWidth+16+digitsWidth)/2;UI.move(word,left,wr[1]);let dx=left+visibleWidth+16;Array.from(text).forEach((c,i)=>{UI.image(d.content,'digit_title_'+c,[dx,d.rect[1]+84.6552,digitWidths[i],digitHeight]);dx+=digitWidths[i]-digitHeight*.06;});
    for(let i=0;i<3;i++)UI.art(d.content,i<this.profile.best_stars(level)?'island_star_full':'island_star_empty',[450+(i-1)*216,d.ys[0]+22,180,180]);UI.art(d.content,'icon_star_budget_steps',[420,d.ys[1]+14,72,72],[317,217,652,801]);Kit.number(d.content,def.par,[508,d.ys[1]+18,150,64]);
    let g=2;species.forEach((s:string,i:number)=>{const row=Math.floor(i/perRow),n=Math.min(perRow,species.length-row*perRow),x=540+(i%perRow-(n-1)/2)*(tile+20)-tile/2,y=d.ys[g]+row*(tile+20);UI.art(d.content,'hud_tool_slot',[x,y,tile,tile]);const inner=tile*.82;Kit.liveBird(d.content,Worlds.look(Levels.world_of(def),s),[x+(tile-inner)/2,y+(tile-inner)/2-6,inner,inner]);const effect=({sunny:'wake',sky:'reveal',berry:'encore',mint:'grow',cocoa:'scare',snow:'undo',ember:'hatch',ink:'encore',hoot:'wake'} as Record<string,string>)[s];if(effect){const badge=tile*.42;UI.art(d.content,'gift_coin_'+effect,[x+tile-badge*.85,y+tile-badge*.85,badge,badge]);}UI.hit_area(d.content,'Bird_'+s,[x,y,tile,tile],()=>this.goal_info(s,Levels.world_of(def)));});if(rows)g++;
    if(listed.length){const y=d.ys[g++];listed.forEach((key,i)=>UI.art(d.content,mechanicArt[key],[540+(i-(listed.length-1)/2)*132-44,y+4,88,88]));}
    if(tools.length){const y=d.ys[g]; tools.forEach((kind:number,i:number)=>{const x=540+(i-(tools.length-1)/2)*180-68;const slot=UI.button(d.content,'Tool_'+kind,'hud_tool_slot',[x,y,136,136],()=>this.offer_tool(kind,()=>{UI.clear(count);Kit.number(count,this.profile.inventory[kind]||'+',[0,0,136,46]);}));UI.art(slot,'island_icon_'+BOOSTER_ART[kind],[18,18,100,100]);const count=UI.group(d.content,'Stock_'+kind,[x,y+142,136,46]);Kit.number(count,this.profile.inventory[kind]||'+',[0,0,136,46]);});}
    const play=Kit.button(d.content,'Play',[160,d.actionY,760,200],'',()=>d.invoke(onPlay??(()=>this.callbacks.play(level))),'wide');UI.art(play,'label_home_island_play_'+Art.locale,[278,40,312,120]);const playIcon=UI.group(play,'PlayIcon',[170,58,84,84]).addComponent(Graphics);playIcon.fillColor=Color.WHITE;playIcon.moveTo(10,-4);playIcon.lineTo(78,-42);playIcon.lineTo(10,-80);playIcon.close();playIcon.fill();UI.enabled(play,Progress.is_level_unlocked(this.profile,level));return d;
  }
  item_offer(title:string,picture:string,price:number,onBuy:()=>void,placement:string,onAd:()=>void):Dialog {
    const d=Kit.jungle(this.host,title,[280,88],{close:true});UI.art(d.content,picture,[400,d.ys[0],280,280]);Kit.number(d.content,'+1',[340,d.ys[1],400,88]);
    const entry=this.adEntry(placement),buy=Kit.button(d.content,'Buy',[entry==='hidden'?325:94,d.actionY,430,162],'',()=>d.invoke(onBuy));Kit.price(buy,[430,162],price);
    if(entry!=='hidden')this.adButton(d.content,[556,d.actionY,430,162],entry,()=>this.watch(placement,()=>d.invoke(onAd)));return d;
  }
  /** Ad entries are hidden without ads in this build or a loaded ad; a placement past today's cap is dimmed. */
  adEntry(placement:string):AdEntry{return this.callbacks.ad?.(placement)??'hidden';}
  watch(placement:string,done:()=>void):void{this.callbacks.rewarded?.(placement,done);}
  adButton(parent:Node,r:Rect,entry:AdEntry,action:()=>void,suffix=''):Node{
    const ad=Kit.button(parent,'WatchAd',r,'',action),icon=90.72;
    if(!suffix)UI.art(ad,'icon_watch_ad',[(r[2]-icon)/2,(r[3]-icon)/2,icon,icon]);
    else{const w=icon+12+Kit.numberWidth(suffix,64),x=(r[2]-w)/2;UI.art(ad,'icon_watch_ad',[x,(r[3]-icon)/2,icon,icon]);Kit.number(ad,suffix,[x+icon+12,(r[3]-64)/2,w-icon-12,64],-1);}
    UI.enabled(ad,entry==='ready');return ad;
  }
  offer_tool(kind:number,done:()=>void=()=>{}):Dialog {
    const grant=()=>{this.profile.inventory[kind]++;this.save();done();this.sound('collect');};
    return this.item_offer('item_'+BOOSTER_WORD[kind],'island_icon_'+BOOSTER_ART[kind],Progress.booster_price(kind),()=>{if(this.profile.coins<Progress.booster_price(kind)){this.callbacks.coins?.();return;}this.profile.coins-=Progress.booster_price(kind);grant();},Ads.TOOL,grant);
  }
  tool_purchase(kind:number,done:()=>void=()=>{}):Dialog {
    const it=Shop.item('booster.'+Progress.BOOSTERS[kind][1])!,d=Kit.purchase(this.host,'item_'+BOOSTER_WORD[kind],[440,80,130]);
    Kit.glow(d.content,[320,d.ys[0],440,440]);UI.art(d.content,'island_icon_'+BOOSTER_ART[kind],[320,d.ys[0],440,440]);Kit.lettering(d.content,'cap_'+BOOSTER_WORD[kind],[164,d.ys[1],752,80]);
    let quantity=1;const max=Math.max(1,Math.floor(this.profile.coins/it.price));const qty=UI.group(d.content,'Quantity',[435,d.ys[2]+1,210,128]);Kit.face(qty,[0,0,210,128],'wide');
    const digits=UI.group(qty,'Value',[16,24,178,80]);let minus:Node,plus:Node,buy:Node,price:Node;let input:EditBox;
    const refresh=()=>{if(input)input.string=String(quantity);UI.clear(digits);Kit.number(digits,quantity,[0,0,178,80]);UI.enabled(minus,quantity>1);UI.enabled(plus,quantity<max);UI.enabled(buy,this.profile.coins>=quantity*it.price);UI.clear(price);Kit.price(price,[680,162],quantity*it.price);};
    minus=Kit.button(d.content,'Decrease',[281,d.ys[2],130,130],'',()=>{quantity--;refresh();},'key_cream');UI.fill(minus,'Minus',[43,55,44,12],'#298514',6);
    plus=Kit.button(d.content,'Increase',[669,d.ys[2],130,130],'',()=>{quantity++;refresh();},'key_cream');UI.fill(plus,'PlusHorizontal',[43,55,44,12],'#298514',6);UI.fill(plus,'PlusVertical',[59,39,12,44],'#298514',6);
    buy=Kit.button(d.content,'Buy',[200,d.actionY,680,162],'',()=>{if(Shop.purchase(this.profile,it.id,quantity)!=='')return;this.save();this.sound('collect');d.close();done();},'wide');price=UI.group(buy,'Price',[0,0,680,162]);refresh();
    // EditBox supplies keyboard input only. Its text stays transparent; the painted digits remain visible.
    const editorNode=UI.group(qty,'QuantityInput',[0,0,210,128]);editorNode.active=false;input=editorNode.addComponent(EditBox);input.inputMode=EditBox.InputMode.NUMERIC;input.returnType=EditBox.KeyboardReturnType.DONE;input.maxLength=String(max).length;
    const label=UI.group(editorNode,'InputText',[16,24,178,80]).addComponent(Label);label.color=new Color(255,255,255,0);label.fontSize=60;label.horizontalAlign=Label.HorizontalAlign.CENTER;input.textLabel=label;input.string=String(quantity);
    const caret=UI.fill(qty,'Caret',[164,32,3,56],'#ffffff',1);caret.active=false;
    editorNode.on('editing-did-began',()=>caret.active=true);editorNode.on('editing-did-ended',()=>{caret.active=false;refresh();});
    editorNode.on('text-changed',(edit:EditBox)=>{const number=parseInt(edit.string.replace(/[^0-9]/g,''),10);quantity=Number.isNaN(number)?1:Math.max(1,Math.min(max,number));refresh();});
    input.enabled=this.profile.coins>=it.price;editorNode.active=true;return d;
  }
  win(result:{stars:number;moves:number;par:number;reward:number;boosters:number[]},onNext:()=>void,onReplay:()=>void,onHome:()=>void,nextEnabled=true):Dialog {
    const double=result.reward>0?this.adEntry(Ads.DOUBLE_COINS):'hidden',heights=[280,96];if(double!=='hidden')heights.push(162);const dropsRow=heights.length;if(result.boosters.length)heights.push(200);const d=Kit.jungle(this.host,'win_title',heights,{footer:72,width:880,actionHeight:314,birdsAbove:true});
    const bg=UI.image(d.node,'background_home_jungle',[0,0,1080,1920]);bg.setSiblingIndex(0);
    this.celebration(d);Kit.stars(d.content,d.ys[0],result.stars,true);for(let i=0;i<result.stars;i++)Kit.after(d.node,.4+i*.18,()=>this.sound('star_'+(i+1)));
    const mid=d.ys[1]+48,cap=64,icon=73.6,mw=icon+12+Kit.numberWidth(String(result.moves),cap),rw=result.reward>0?icon+12+Kit.numberWidth('+'+result.reward,cap):0,gap=result.reward>0?72:0,x=540-(mw+gap+rw)/2;
    UI.art(d.content,'icon_star_budget_steps',[x,mid-icon/2,icon,icon],[317,217,652,801]);this.count(d.content,result.moves,[x+icon+12,mid-32,mw-icon-12,64],.5);
    if(result.reward>0){UI.image(d.content,'icon_purchase_feather',[x+mw+gap,mid-icon/2,icon,icon],true);this.count(d.content,result.reward,[x+mw+gap+icon+12,mid-32,rw-icon-12,64],1.35,'+');}
    // Watching an ad pays the level's feathers once more (the world bonus is not doubled).
    if(double!=='hidden'){const button=this.adButton(d.content,[325,d.ys[2],430,162],double,()=>this.watch(Ads.DOUBLE_COINS,()=>{this.profile.coins+=result.reward;this.save();this.sound('collect');Kit.destroy(button);const counter=d.content.getChildByName('Counter');if(counter){Kit.stop(counter);UI.clear(counter);const r=UI.rect(counter);Kit.number(counter,'+'+result.reward*2,[0,0,r[2]+Kit.numberWidth('0',64),r[3]]);Kit.pop(counter);}}),'×2');}
    if(result.boosters.length){const counts=new Map<number,number>();result.boosters.forEach(kind=>counts.set(kind,(counts.get(kind)||0)+1));Array.from(counts).forEach(([kind,count],i)=>{const drop=UI.group(d.content,'Drop'+kind,[540+(i-(counts.size-1)/2)*228-90,d.ys[dropsRow]+10,180,180]);UI.art(drop,'island_icon_'+BOOSTER_ART[kind],[0,0,180,180]);Kit.number(drop,'+'+count,[116,120,100,56],-1);Kit.pop(drop,1.8+i*.18);});}
    const next=Kit.button(d.content,'action_next',[182,d.actionY,716,162],'white_action_next',()=>d.invoke(onNext),'wide');UI.enabled(next,nextEnabled);
    this.woodAction(d.content,'Retry',[182,d.actionY+184,346,130],'white_action_replay',()=>d.invoke(onReplay));this.woodAction(d.content,'Home',[552,d.actionY+184,346,130],'white_settings_home',()=>d.invoke(onHome));
    this.confetti(d.node);return d;
  }
  private woodAction(parent:Node,name:string,rect:Rect,word:string,action:()=>void):Node { const b=UI.button(parent,name,'btn_dialog_wood',rect,action);Kit.word(b,word,[rect[2],rect[3]]);return b; }
  private count(parent:Node,value:number,rect:Rect,delay:number,prefix=''):void {
    const holder=UI.group(parent,'Counter',rect);let shown=-1;const state={n:Kit.instant?value:0};const update=()=>{const n=Math.round(state.n);if(n===shown)return;shown=n;UI.clear(holder);Kit.number(holder,prefix+n,[0,0,rect[2],rect[3]]);};update();
    if(!Kit.instant)tween(holder).delay(delay).to(.65,{}, {onUpdate:(_target,ratio)=>{state.n=value*ratio!;update();}}).start();
  }
  private celebration(d:Dialog):void { [['sunny',432,216],['sky',208,184],['berry',688,184]].forEach(([s,x,side],i)=>{const size=Number(side),b=Kit.liveBird(d.content,String(s),[Number(x),d.rect[1]+28-size,size,size]);if(!Kit.instant){const p=b.node.position.clone();b.node.setPosition(p.x+(i===1?-420:420),p.y+200,p.z);tween(b.node).delay(.45+i*.15).to(.9,{position:p},{easing:'quadOut'}).call(()=>b.showcase()).start();}}); }
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
    UI.word(d.content,'bird.'+species+'.ability',[172,y,736,128]);Kit.button(d.content,'Confirm',[208,d.actionY,664,162],'white_action_ok',d.close,'wide');return d;
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
    const take=(double:boolean)=>{if(busy)return;const got=Progress.claim_daily_reward(this.profile,Progress.today());if(!got.day)return;if(double){this.profile.coins+=got.coins;got.coins*=2;}busy=true;this.save();UI.enabled(claim,false);if(ad)UI.enabled(ad,false);const tile=tiles[got.day-1];Kit.stop(tile);UI.opacity(tile.getChildByName('Body')!,.45);UI.check_badge(tile,[UI.rect(tile)[2]-44,44]);this.coin_flight(d.content,[540,UI.rect(tile)[1]+h/2],[792,96],()=>{const counter=d.content.getChildByName('Coins');if(counter)Kit.destroy(counter);UI.feather_bar(d.content,[736,48],this.profile.coins,this.callbacks.coins);this.sound('collect');Kit.after(d.node,.3,()=>this.rewards('daily_reward.title',got.coins,got.boosters,()=>d.close(),'pic_calendar'));});};
    const claim=Kit.button(d.content,'Claim',[entry==='hidden'?325:94,1613,430,162],'white_claim',()=>take(false));UI.enabled(claim,available);
    const ad=entry==='hidden'?null:this.adButton(d.content,[556,1613,430,162],entry,()=>this.watch(Ads.DAILY_DOUBLE,()=>take(true)),'×2');return d;
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
    const spin=Kit.button(d.content,'Draw',[entry==='hidden'?325:94,1540,430,162],'white_spin',spinOnce);UI.enabled(spin,this.profile.draws_left>0);
    const ad=entry==='hidden'?null:this.adButton(d.content,[556,1540,430,162],entry,()=>{if(!busy)this.watch(Ads.DRAW,()=>{this.profile.draws_left++;this.save();spinOnce();});},'+1');return d;
  }
  chest():Dialog {
    const ready=Progress.chests_ready(this.profile),collected=Math.max(0,Math.min(Progress.CHEST_STARS,Progress.chest_progress(this.profile)));
    const d=Kit.page(this.host,'StarChest','star_chest_title',this.profile.coins,this.callbacks.close,this.callbacks.coins);this.chestHero(d,false);Kit.chestNumber(d.content,String(ready),[400,432,280,104]).name='ReadyCount';
    const progress=UI.group(d.content,'Progress',[168,1180,744,112]);UI.nine_region(progress,'panel_chest_progress',[112,285,1946,157],[63,13.44,681,85.12],[87,0,87,0],85.12/157);
    if(collected)UI.nine_region(progress,'fill_chest_progress',[160,273,1846,177],[72,22.44,Math.max(67,663*collected/Progress.CHEST_STARS),67.12],[98,0,98,0],67.12/177);UI.art(progress,'island_star_full',[-14,-14,140,140]);Kit.chestNumber(progress,`${collected}/${Progress.CHEST_STARS}`,[0,24,744,62]);
    let busy=false;const open=Kit.button(d.content,'OpenChest',[325,1343,430,162],'white_open',()=>{if(busy)return;busy=true;UI.enabled(open,false);UI.enabled(play,false);const got=Progress.open_chest(this.profile);if(got.coins===undefined)return;this.save();this.animateChest(d,()=>{d.close();this.chest_rewards(got.coins,got.boosters);});});UI.enabled(open,ready>0);
    const play=Kit.button(d.content,'Play',[325,1575,430,162],'white_play',()=>d.invoke(()=>this.callbacks.play(Progress.resume_level(this.profile))));return d;
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
    const paint=(degrees:number)=>{UI.clear(lid);const face=Math.sin(degrees*Math.PI/180)/Math.sin(70*Math.PI/180),extent=Math.abs(face),bottom=515+(face<0?125*extent:0),height=Math.max(14,(face<0?420:429)*extent),width=959+(face<0?32*extent:0),left=658.5-width/2,inset=24*(1-extent);
      const mask=UI.group(lid,'Projection',[0,0,1254,1254]);mask.addComponent(Mask).type=Mask.Type.GRAPHICS_STENCIL;const g=mask.getComponent(Graphics)!;g.clear();g.moveTo(left+inset,-bottom+height);g.lineTo(left+width-inset,-bottom+height);g.lineTo(left+width,-bottom);g.lineTo(left,-bottom);g.close();g.fill();
      UI.image_region(mask,face<0?'chest_lid_outer':'chest_star_open',face<0?[28,84,1714,691]:[179,86,959,429],[left,bottom-height,width,height]);
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
    const ok=Kit.button(d.content,'ConfirmRewards',[325,1423,430,162],'white_action_ok',()=>d.invoke(()=>this.callbacks.close?.()));UI.enabled(ok,false);Kit.after(d.node,.3,()=>UI.enabled(ok,true));return d;
  }
  rewards(title:string,coins:number,boosters:number[],onOK:()=>void=()=>{},picture='pic_chest_open'):Dialog {
    if(title==='draw.title')return this.draw_rewards(coins,boosters,onOK);
    const count=(coins>0?1:0)+new Set(boosters).size,rows=Math.ceil(count/3),hero=rows>1?320:440;
    const d=Kit.jungle(this.host,Kit.titleKey(title),count?[hero,rows*250+Math.max(0,rows-1)*20]:[hero],{footer:72,width:count>=3?880:840});Kit.glow(d.content,[540-hero/2,d.ys[0],hero,hero]);const image=UI.art(d.content,picture,[570-hero/2,d.ys[0]+30,hero-60,hero-60]);Kit.breathe(image);if(count)Kit.prizes(d.content,coins,boosters,d.ys[1]);Kit.button(d.content,'Confirm',[325,d.actionY,430,162],'white_action_ok',()=>d.invoke(onOK));this.sound('collect');return d;
  }
  private draw_rewards(coins:number,boosters:number[],onOK:()=>void):Dialog {
    const d=Kit.page(this.host,'DrawResult','lucky_spin_title'),close=d.content.getChildByName('Close');if(close)Kit.destroy(close);
    const picture=coins>0?DRAW_PRIZE_ART[Progress.DRAW_PRIZES.findIndex(p=>p[0]===coins)]:'island_icon_'+BOOSTER_ART[boosters[0]];
    const rays=UI.image(d.content,'light_rays',[180,570,720,720]);Kit.centerPivot(rays);UI.opacity(rays,.65);if(!Kit.instant)tween(rays).repeatForever(tween().by(28,{angle:360})).start();Kit.glow(d.content,[270,690,540,420]);Kit.breathe(UI.art(d.content,picture,[310,720,460,390]));Kit.number(d.content,'+'+(coins||boosters.length),[290,1170,500,100]);Kit.button(d.content,'Confirm',[325,1540,430,162],'white_action_ok',()=>d.invoke(onOK));this.sound('collect');return d;
  }
  new_friend(species:string,reward:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'popup_new_friend_title',[440,124.8,128,120],{footer:72,width:880});Kit.liveBird(d.content,species,[350,d.ys[0]+20,380,400]);Kit.lettering(d.content,'bird_'+species,[220,d.ys[1],640,124.8]);UI.word(d.content,'bird.'+Worlds.slot_of(species)+'.ability',[172,d.ys[2],736,128]);UI.price_pill(d.content,[350,d.ys[3],380,120],reward);Kit.button(d.content,'Confirm',[325,d.actionY,430,162],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  unlock(booster:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'item_'+BOOSTER_WORD[booster],[440,100],{footer:72,width:900});Kit.breathe(UI.art(d.content,'island_icon_'+BOOSTER_ART[booster],[360,d.ys[0]+40,360,360]));Kit.lettering(d.content,'cap_'+BOOSTER_WORD[booster],[132,d.ys[1]+18,816,64]);Kit.button(d.content,'Confirm',[325,d.actionY,430,162],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  world_complete(world:number,reward:number,onOK:()=>void=()=>{}):Dialog {
    const d=Kit.jungle(this.host,'popup_world_complete_title',[280,120],{footer:72,width:880,birdsAbove:true});this.celebration(d);Kit.stars(d.content,d.ys[0],3,true);UI.price_pill(d.content,[350,d.ys[1],380,120],reward);Kit.button(d.content,'Confirm',[325,d.actionY,430,162],'white_action_ok',()=>d.invoke(onOK));return d;
  }
  world_unlock(world:number,onPlay:()=>void):Dialog {
    const h=720*777/855,d=Kit.jungle(this.host,'popup_world_unlock_title',[h,124.8],{footer:72});const k=720/855,stage=UI.group(d.content,'Gate',[540-509.5*k,d.ys[0]-100*k,1024,1024]);stage.setScale(k,k,1);UI.image(stage,'gate_backdrop_w'+world,[0,0,1024,1024]);UI.image(stage,'gate_arch',[0,0,1024,1024]);
    const left=UI.image(stage,'gate_door_left',[0,0,1024,1024]),right=UI.image(stage,'gate_door_right',[0,0,1024,1024]);if(!Kit.instant){tween(left).delay(.5).to(1.1,{scale:new Vec3(.139,1,1)},{easing:'quadOut'}).start();tween(right).delay(.5).to(1.1,{scale:new Vec3(.139,1,1)},{easing:'quadOut'}).start();}else{left.setScale(.139,1,1);right.setScale(.139,1,1);}
    Kit.lettering(d.content,'world_'+WORLD_ID[world],[220,d.ys[1],640,124.8]);Kit.button(d.content,'Play',[325,d.actionY,430,162],'white_play',()=>d.invoke(onPlay));return d;
  }
  coin_flight(parent:Node,from:Point,to:Point,onLand:()=>void):void {
    if(Kit.instant){onLand();return;}for(let i=0;i<9;i++){const c=UI.art(parent,'coin',[from[0]-24,from[1]-24,48,48]);const start=c.position.clone();tween(c).delay(.2+i*.05).to(.26,{position:new Vec3(start.x+Math.cos(i*2.4)*90,start.y+Math.sin(i*2.4)*90,0)},{easing:'quadOut'}).to(.42,{position:new Vec3(to[0],-to[1],0)},{easing:'quadIn'}).call(()=>{Kit.destroy(c);if(i===8)onLand();}).start();}
  }
  coin_shop():Dialog {
    const d=Kit.layer(this.host,'CoinShop',this.callbacks.close,true,'background_shop_forest'),scale=1920/1717,canvas=UI.group(d.content,'ApprovedLayout',[(1080-916*scale)/2,0,916,1717]);canvas.setScale(scale,scale,1);UI.button(canvas,'Close','btn_purchase_close',[23,50,116,119],d.close);UI.art(canvas,'panel_purchase_balance',[619,48,270,92]);Kit.drawNumber(UI.group(canvas,'Balance',[707,73,157,44]),String(Math.min(99999,this.profile.coins)),[0,0,157,44]);Kit.sign(canvas,'coin_shop_title',[118,64,680,250]);
    const packs:CoinPack[]=[{id:'stack',coins:600,yuan:6},{id:'cluster',coins:2000,yuan:18},{id:'pouch',coins:3600,yuan:30},{id:'basket',coins:9000,yuan:68},{id:'chest',coins:20000,yuan:128},{id:'hoard',coins:33000,yuan:198}];const rects:Rect[]=[[138,328,239,165],[511,323,286,175],[122,668,271,237],[513,669,286,231],[119,1088,266,247],[496,1073,325,265]],prices=['six','eighteen','thirty','sixty_eight','one_twenty_eight','one_ninety_eight'];
    packs.forEach((pack,i)=>{UI.image(canvas,'icon_purchase_'+pack.id,rects[i]);Kit.drawNumber(canvas,String(pack.coins),[137+i%2*401,[506,915,1346][Math.floor(i/2)],236,50]);UI.button(canvas,'Buy_'+pack.id,'btn_purchase_'+prices[i],[117+i%2*401,[560,961,1395][Math.floor(i/2)],276,108],()=>{const p=Kit.purchase(d.node,'coins_buy',[300,88],136);UI.art(p.content,'icon_purchase_'+pack.id,[280,p.ys[0],520,300]);Kit.number(p.content,'+'+pack.coins,[280,p.ys[1],520,88]);const button=Kit.button(p.content,'Confirm',[264,p.actionY,552,136],'',()=>{if(!this.callbacks.purchase)return;UI.enabled(button,false);this.callbacks.purchase(pack,()=>{this.profile.coins+=pack.coins;this.save();p.close();this.coin_flight(d.content,[540,960],[792,96],()=>{d.close();this.coin_shop();});});},'wide');Kit.drawNumber(button,'¥'+pack.yuan,[80,35,392,66]);UI.enabled(button,!!this.callbacks.purchase);});});// Free feathers for an ad, Ads.DAILY_CAP times a day; the balance redraws after the reward.
    const entry=this.adEntry(Ads.FREE_COINS);if(entry!=='hidden'){const ad=this.adButton(canvas,[266,1548,384,145],entry,()=>this.watch(Ads.FREE_COINS,()=>{this.profile.coins+=Ads.FREE_COIN_AMOUNT;this.save();this.sound('collect');UI.enabled(ad,this.adEntry(Ads.FREE_COINS)==='ready');const balance=canvas.getChildByName('Balance');if(balance){UI.clear(balance);Kit.drawNumber(balance,String(Math.min(99999,this.profile.coins)),[0,0,157,44]);}}),'+'+Ads.FREE_COIN_AMOUNT);}
    return d;
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
