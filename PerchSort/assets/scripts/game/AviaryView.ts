import { Color, Graphics, Node, Sprite, Tween, Vec2 } from 'cc';
import { UI, Rect } from './UI';
import { Art } from './Art';
import { BirdView } from './BirdView';
import { Profile } from '../meta/Profile';
import { Aviary } from '../meta/Aviary';
import { Worlds } from '../meta/Worlds';
import { Shop } from '../meta/Shop';
import { Kit, PageCallbacks } from './Popups';

/** The canonical forest collection and six-look bird detail page. */
export class AviaryView {
  readonly node:Node;
  private page!:Node;
  private species='';
  private readonly fromShop:boolean;
  private listScroll=0;
  private world=1;
  private hero?:BirdView;
  private coins?:Node;
  private tiles?:Node;
  constructor(host:Node,public profile:Profile,private callbacks:PageCallbacks,species='') {
    this.node=UI.group(host,'Aviary');this.fromShop=species!=='';
    UI.calm_backdrop(UI.cover(this.node,'background_aviary_forest',[0,0,1080,1920]));
    species?this.show_detail(species):this.show_list();
  }
  destroy():void { Kit.destroy(this.node); }
  private reset():void { if(this.page)Kit.destroy(this.page);this.page=UI.group(this.node,'Page');this.coins=undefined;this.hero=undefined; }
  private outfit(species:string):string { const id=this.profile.equipped[species];return id?`${species}_outfit_${id.split('.')[2]}`:''; }
  private navigation(back:boolean,action:()=>void):void {const key=back?'btn_forest_back':'btn_purchase_close',size=Art.size(key),ink=Art.ink(key),height=133.07;Kit.close(this.page,[48,119-(ink[1]+ink[3]/2)*height/size[1]],action,back);}
  private title(key:string,rect:Rect):void { const h=rect[3]*.7,y=119-h/2;Kit.lettering(this.page,key,[rect[0]+rect[2]*.12,y+h*.17,rect[2]*.76,h*.76]); }
  show_list():void {
    this.species='';this.reset();
    const ids=this.world_ids(this.world),top=232,view=1696-top;
    const height=Math.ceil(ids.length/2)*500-32+64;
    const scroll=UI.scroll(this.page,'Scroll',[48,top,960,view],height);
    scroll.scroll.scrollToOffset(new Vec2(0,this.listScroll),0);
    ids.forEach((id,i)=>{
      const card=UI.group(scroll.content,'Card_'+id,[i%2*496,Math.floor(i/2)*500,464,468]);const known=this.profile.has_met(id);
      Kit.perch(card,[232,364],416);
      if(known){Kit.liveBird(card,id,[52,8,360,356],this.outfit(id));Kit.lettering(card,'bird_'+id,[38,400,388,80]);
        if(this.world===1)UI.hit_area(card,'Select',[0,0,464,468],()=>{this.listScroll=scroll.scroll.getScrollOffset().y;this.show_detail(id);});
      }else{
        const sil=UI.art(card,id+'_front',[52,8,360,356]);UI.silhouette(sil);Kit.breathe(sil);
        UI.art(card,'icon_aviary_question',[180,154,104,116]);UI.piece_fit(card,'island_padlock',[232,450],[72,80]);
      }
    });
    const max=Math.max(0,height-view);if(max>0){const track=UI.fill(this.page,'Track',[1036,top,18,view-64],'#fff7c7e6',9);UI.fill(track,'Rail',[3,3,12,view-70],'#21a68c80',6);const thumb=UI.fill(this.page,'Thumb',[1029,top,32,96],'#fff7c7e6',16);UI.fill(thumb,'Fill',[3,3,26,90],'#ffc322',13);scroll.node.on('scrolling',()=>UI.move(thumb,1029,top+(view-160)*Math.max(0,scroll.scroll.getScrollOffset().y)/max));}
    this.navigation(false,this.callbacks.close);this.title('aviary',[300,32,480,174]);
    const header=UI.group(this.page,'WorldSwitch',[48,1716,960,120]),last=this.last_world();
    const previous=UI.button(header,'WorldPrevious','btn_world_previous',[160,4,112,112],()=>this.switch_world(-1));
    const next=UI.button(header,'WorldNext','btn_world_next',[688,4,112,112],()=>this.switch_world(1));
    Kit.lettering(header,'world_'+Worlds.NAMES[this.world-1],[284,22,392,76]);
    UI.enabled(previous,this.world>1);UI.enabled(next,this.world<last);
    const chip=UI.group(this.page,'MetCounter',[784,79,248,80]);const rail=Art.data('art/kit/kit').hud_bar;UI.nine(chip,'kit/hud_bar',[0,0,248,80],rail.margins,80/rail.size[1]);UI.art(chip,'fall_leaf_green',[0,0,80,80]);Kit.number(chip,ids.filter(id=>this.profile.has_met(id)).length+'/'+ids.length,[80,12,152,56]);
  }
  private world_ids(world:number):string[] { return world===1?Aviary.SPECIES.map(([id])=>id):Worlds.birds(world); }
  /** The last world whose birds all have their pictures and painted names; worlds beyond it are not offered. */
  private last_world():number {
    let last=1;
    for(let world=2;world<=Worlds.COUNT;world++){if(Worlds.birds(world).every(id=>Art.has(id+'_front')&&!!Art.pack().painted['lettering_bird_'+id]))last=world;else break;}
    return last;
  }
  private switch_world(step:number):void { this.world=Math.max(1,Math.min(this.last_world(),this.world+step));this.listScroll=0;this.show_list(); }
  show_detail(id:string):void {
    this.species=id;this.reset();Kit.glow(this.page,[232,204,616,472]);Kit.perch(this.page,[540,644],640);
    this.hero=Kit.liveBird(this.page,id,[336,212,408,432],this.outfit(id));UI.hit_area(this.page,'Showcase',[336,212,408,432],()=>this.hero?.showcase());
    UI.art(this.page,'panel_aviary_ability',[232,714,616,104]);const ability=UI.paint(this.page,Art.word('bird.'+id+'.ability'),[304,740,472,52]);UI.effect(ability,'ability_ink');
    this.navigation(true,()=>this.fromShop?this.callbacks.close():this.show_list());this.title('bird_'+id,[308,32,464,174]);this.refresh_balance();
    if(id!=='crow'){this.tiles=UI.group(this.page,'Outfits');this.buildTiles();}
  }
  refresh_balance():void { if(!this.species)return;if(this.coins)Kit.destroy(this.coins);this.coins=UI.feather_bar(this.page,[760,65],this.profile.coins,this.callbacks.coins); }
  private buildTiles():void {
    const tiles=this.tiles!;UI.clear(tiles);const current=this.profile.equipped[this.species]||`outfit.${this.species}.default`;
    for(let i=0;i<=Shop.OUTFIT_COUNT;i++){
      const id=i?`outfit.${this.species}.${i}`:`outfit.${this.species}.default`,owned=i===0||this.profile.owns_outfit(id),selected=current===id;
      const tile=UI.group(tiles,'Tile_'+i,[24+i%3*352,940+Math.floor(i/3)*424,328,400]);Kit.perch(tile,[164,280],328);Kit.liveBird(tile,this.species,[28,0,272,280],i?`${this.species}_outfit_${i}`:'');
      const face=Kit.plate(tile,[32,306,264,88]);if(owned)Kit.word(face,selected?'white_equipped':'white_equip',[264,88]);else Kit.price(face,[264,88],Shop.item(id)!.price);
      const tap=UI.hit_area(tile,owned?'Equip':'Locked',[0,0,328,400],()=>owned?this.equip(id):this.confirm_outfit(id));UI.enabled(tap,!selected);
      if(selected)UI.check_badge(tile,[288,40]);else if(!owned)UI.piece_fit(tile,'island_padlock',[288,40],[56,56]);
    }
  }
  private equip(id:string):void {
    if(Shop.equip(this.profile,id)!=='')return;this.callbacks.changed();this.callbacks.sound?.('equip');this.hero?.destroy();this.hero=Kit.liveBird(this.page,this.species,[336,212,408,432],this.outfit(this.species));this.hero.showcase();this.buildTiles();
  }
  confirm_outfit(id:string):void {
    if(this.node.getChildByName('Purchase'))return;const it=Shop.item(id)!,d=Kit.purchase(this.node,id.replace(/\./g,'_'),[500]);
    Kit.glow(d.content,[290,d.ys[0],500,500]);Kit.liveBird(d.content,this.species,[306,d.ys[0]+16,468,468],`${this.species}_outfit_${it.index}`);
    const buy=Kit.button(d.content,'Buy',[200,d.actionY,680,162],'',()=>{if(Shop.purchase(this.profile,id)!=='')return;this.callbacks.sound?.('collect');d.close();this.refresh_balance();this.equip(id);},'wide');Kit.price(buy,[680,162],it.price);UI.enabled(buy,this.profile.coins>=it.price);
  }
  static preview(host:Node,state:string,callbacks:PageCallbacks):AviaryView {
    const p=new Profile();p.coins=state==='detail_maximum'?99999:400;Aviary.SPECIES.forEach(([id])=>p.met[id]=true);
    if(state==='list_none_met')p.met={};if(state==='list_some_unmet')p.met={sunny:true,sky:true,berry:true,mint:true};
    if(state==='buy_outfit_short')p.coins=120;
    const id=state.includes('berry')?'berry':state.includes('ember')?'ember':state.includes('crow')?'crow':state.includes('sunny')?'sunny':'hoot';
    if(state.includes('bought')||state==='detail_berry'){p.outfits[`outfit.${id}.1`]=true;p.equipped[id]=`outfit.${id}.1`;}
    if(state==='list_world_2')Worlds.birds(2).slice(0,4).forEach(bird=>p.met[bird]=true);
    const v=new AviaryView(host,p,callbacks);if(state==='list_world_2'){v.world=2;v.show_list();}else if(!state.startsWith('list'))v.show_detail(id);if(state.startsWith('buy_outfit'))v.confirm_outfit(`outfit.${id}.1`);return v;
  }
}
