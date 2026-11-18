import { Node, UITransform, Vec2 } from 'cc';
import { UI, Rect } from './UI';
import { Profile } from '../meta/Profile';
import { Shop } from '../meta/Shop';
import { Aviary } from '../meta/Aviary';
import { Progress } from '../meta/Progress';
import { Levels } from '../core/Levels';
import { AviaryView } from './AviaryView';
import { BOOSTER_ART, BOOSTER_WORD, Kit, PageCallbacks, Popups } from './Popups';

type CatalogItem=NonNullable<ReturnType<typeof Shop.item>>;
export interface ShopCallbacks extends PageCallbacks { boosterPurchased?: (kind:number,count:number)=>void }

/** Three canonical categories share their painted cards, fixed action sizes and scroll viewport. */
export class ShopView {
  readonly node:Node;
  private page!:Node;
  private outfitPage?:AviaryView;
  private scroll?:ReturnType<typeof UI.scroll>;
  constructor(host:Node,public profile:Profile,private callbacks:ShopCallbacks,public tab=0) {this.node=UI.group(host,'Shop');this.build();}
  destroy():void { Kit.destroy(this.node); }
  private items():CatalogItem[] {
    const items = Shop.catalog().filter(it => it.kind === this.tab && (this.tab !== Shop.BOOSTER || it.quantity === 1));
    if (this.tab === Shop.HOUSE) {
      items.push({ id: 'house.default', style: 'default', kind: Shop.HOUSE, price: 0, quantity: 1 });
    }
    return items;
  }
  static outfit_species():string[] {return Aviary.SPECIES.filter(([id])=>id!=='crow').map(([id])=>id);}
  owned_outfits(species:string):number {return 1+Shop.catalog().filter(it=>it.kind===Shop.OUTFIT&&it.species===species&&this.profile.owns_outfit(it.id)).length;}
  private build():void {
    if(this.page)Kit.destroy(this.page);this.page=UI.group(this.node,'Page');UI.image(this.page,'background_home_jungle',[0,0,1080,1920]);
    Kit.close(this.page,[48,48],this.callbacks.close);Kit.lettering(this.page,'tab_'+['boosters','outfits','nests'][this.tab],[284,232,512,132]);UI.feather_bar(this.page,[736,48],this.profile.coins,this.callbacks.coins);
    const count=this.tab===Shop.OUTFIT?ShopView.outfit_species().length:this.items().length;
    const height=Math.max(1284,this.tab===Shop.BOOSTER?count*256-20+8:Math.ceil(count/2)*432-20+8);
    this.scroll=UI.scroll(this.page,'Scroll',[48,384,984,1284],height);
    if(this.tab===Shop.OUTFIT)ShopView.outfit_species().forEach((id,i)=>this.kindTile(id,[i%2*502,Math.floor(i/2)*432,482,412],i));
    else this.items().forEach((it,i)=>this.itemTile(it,this.tab===Shop.BOOSTER?[0,i*256,984,236]:[i%2*502,Math.floor(i/2)*432,482,412],i));
    ['boosters','outfits','nests'].forEach((name,i)=>{const button=Kit.button(this.page,['Boosters','Outfits','Nests'][i],[198+i*244,1680,196,192],'',()=>{if(this.tab!==i){this.tab=i;this.build();}},i===this.tab?'key_green':'key_cream');UI.art(button,'island_tab_'+name,[32,26,132,132]);});
  }
  refresh_balance():void {
    if (this.outfitPage) {
      this.outfitPage.refresh_balance();
      return;
    }
    const offset = this.scroll?.scroll.getScrollOffset().y ?? 0;
    this.build();
    this.scroll?.scroll.scrollToOffset(new Vec2(0, offset), 0);
  }
  private locked(it:CatalogItem):boolean {return it.kind===Shop.BOOSTER&&!Progress.is_booster_unlocked(this.profile,it.booster!);}
  private owned(it:CatalogItem):boolean {return it.id==='house.default'||it.kind!==Shop.BOOSTER&&(it.kind===Shop.OUTFIT?this.profile.owns_outfit(it.id):this.profile.owns_house(it.style!));}
  private equipped(it:CatalogItem):boolean {return it.kind===Shop.OUTFIT?this.profile.equipped[it.species!]===it.id:this.profile.equipped_house===it.style;}
  private picture(parent:Node,it:CatalogItem,rect:Rect):void {UI.art(parent,it.kind===Shop.BOOSTER?'island_icon_'+BOOSTER_ART[it.booster!]:'house_style_'+it.style+'_body',rect);}
  private itemTile(it:CatalogItem,rect:Rect,index:number):void {
    const card=UI.group(this.scroll!.content,it.id,rect);Kit.panel(card,[0,0,rect[2],rect[3]]);const booster=it.kind===Shop.BOOSTER;
    if (it.id === 'house.default') {
      UI.art(card, 'house_default', [86, 12, 310, 256]);
    } else {
      this.picture(card,it,booster?[36,18,200,200]:[81,12,320,180]);
      Kit.lettering(card,booster?'item_'+BOOSTER_WORD[it.booster!]:'house_'+it.style,booster?[262,78,380,80]:[41,210,400,66]);
    }
    const action=Kit.plate(card,booster?[660,64,288,108]:[97,284,288,108]);if(this.owned(it)){Kit.word(action,this.equipped(it)?'white_equipped':'white_equip',[288,108]);if(this.equipped(it))UI.check_badge(card,[rect[2]-44,44]);}else Kit.price(action,[288,108],it.price);
    if(booster){if(this.locked(it)){UI.opacity(card,.45);UI.piece_fit(card,'island_padlock',[222,196],[66,66]);}else{UI.image(card,'island_badge_green',[184,158,76,76]);Kit.number(card,Math.min(99,this.profile.inventory[it.booster!]),[194.64,173.2,54.72,42.56]);}}
    const hit=UI.hit_area(card,'Select',[0,0,rect[2],rect[3]],()=>this.tap(it));UI.enabled(hit,!this.locked(it));Kit.pop(card,index*.04);
  }
  private kindTile(species:string,rect:Rect,index:number):void {
    const card=UI.group(this.scroll!.content,'Kind_'+species,rect);Kit.panel(card,[0,0,482,412]);const met=this.profile.has_met(species);
    if(met){Kit.perch(card,[241,188],300.8);const current=this.profile.equipped[species];Kit.liveBird(card,species,[81,12,320,180],current?species+'_outfit_'+current.split('.')[2]:'');Kit.lettering(card,'bird_'+species,[41,210,400,66]);const plate=Kit.plate(card,[97,284,288,108]);Kit.number(plate,this.owned_outfits(species)+'/'+(Shop.OUTFIT_COUNT+1),[46,22.68,144,56.16]);const arrow=UI.art(plate,'island_chevron',[202.176,28.296,44.928,44.928]);const r=UI.rect(arrow);arrow.getComponent(UITransform)!.setAnchorPoint(.5,.5);arrow.setPosition(r[0]+r[2]/2,-r[1]-r[3]/2);arrow.angle=90;UI.effect(arrow,'white_ink');UI.hit_area(card,'Select',[0,0,482,412],()=>this.open_outfits(species));}
    else UI.piece_fit(card,'island_unknown_bird',[241,102],[288,162]);Kit.pop(card,index*.04);
  }
  private tap(it:CatalogItem):void {
    if(this.owned(it)){if(!this.equipped(it)&&Shop.equip(this.profile,it.id)===''){this.callbacks.sound?.('equip');this.callbacks.changed();this.refresh_balance();}return;}
    if(it.kind===Shop.BOOSTER){const popup=new Popups(this.node,this.profile,{save:()=>this.callbacks.changed(),play:()=>{},sound:this.callbacks.sound});const before=this.profile.inventory[it.booster!];popup.tool_purchase(it.booster!,()=>{this.callbacks.boosterPurchased?.(it.booster!,this.profile.inventory[it.booster!]-before);this.refresh_balance();});return;}
    const d=Kit.purchase(this.node,'house_'+it.style,[500]);Kit.glow(d.content,[290,d.ys[0],500,500]);this.picture(d.content,it,[290,d.ys[0],500,500]);const buy=Kit.button(d.content,'Buy',[200,d.actionY,680,162],'',()=>{if(Shop.purchase(this.profile,it.id)!=='')return;this.callbacks.sound?.('collect');this.callbacks.changed();d.close();this.refresh_balance();},'wide');Kit.price(buy,[680,162],it.price);UI.enabled(buy,this.profile.coins>=it.price);
  }
  open_outfits(species:string):AviaryView {
    this.page.active=false;
    this.outfitPage=new AviaryView(this.node,this.profile,{close:()=>{this.outfitPage?.destroy();this.outfitPage=undefined;this.refresh_balance();},changed:this.callbacks.changed,coins:this.callbacks.coins,sound:this.callbacks.sound},species);return this.outfitPage;
  }
  static preview(host:Node,state:string,callbacks:ShopCallbacks):ShopView {
    const p=new Profile();p.coins=state==='boosters_maximum'?99999:1280;p.highest_unlocked=Levels.COUNT;p.inventory=[3,2,1,1,0,2];Aviary.SPECIES.forEach(([id])=>p.met[id]=true);
    if(state==='boosters_empty'){p.coins=0;p.inventory=[0,0,0,0,0,0];}if(state==='locked_tools')p.highest_unlocked=3;
    const tab=state.includes('outfit')?1:state.includes('house')||state.includes('nest')?2:0;const view=new ShopView(host,p,callbacks,tab);
    if(state==='bird_outfits'||state.includes('confirm_outfit')){const detail=view.open_outfits('berry');if(state.includes('confirm'))detail.confirm_outfit('outfit.berry.3');}
    if(state==='confirm_booster')view.tap(Shop.item('booster.firefly')!);if(state==='confirm_nest')view.tap(Shop.item('house.teapot')!);return view;
  }
}
