import { Color, Graphics, Mask, Node, Tween, UIOpacity, Vec3, tween } from 'cc';
import { UI, Point, Rect } from './UI';
import { Art } from './Art';
import { BirdView } from './BirdView';
import { Kit, Dialog } from './Popups';
import { Board, Kind } from '../core/Board';
import { Levels, LevelDefinition } from '../core/Levels';
import { Profile } from '../meta/Profile';

type Slot = { name: string; at: Point; perch: number; tip: number };
const STAGE: Point = [704, 660], SCALE = 704 / 1080, BIRD_SIDE = 170 * 1.1;
const PERCHES = [470, 690, 910], SLOTS = ['L0', 'R0', 'L1'];
const HOUSE: Rect = [430, 24, 220, 248], HOUSE_INK: Rect = [122,62,778,878], HOUSE_DOOR: Rect = [313,418,378,380];
const HAND_REST: Point = [800,900];
const LESSONS = ['sleep','mama','egg','crow','twig','swivel','growing'];
const STARTS: string[][][] = [
  [['sky','sunny','sunny'],['berry','berry','sky'],['sky','berry']],
  [['sky','sunny','sunny'],['berry','sunny'],[]],
  [['sky','sunny'],['sunny','sunny','sunny'],['berry','sky']],
  [['sunny','sunny','sunny'],['sky','sky','sky'],['sky','sunny']],
];
const LESSON_BIRDS: Record<string,string[][]> = {
  sleep: [['berry','berry','berry:sleep'],['sky','berry'],['sky','sky']],
  mama: [['sunny:chick','sky','sky','sunny:mama'],[],['sky','sky']],
  egg: [['egg:mint','sky'],['sky','sky','sky'],['mint','mint','mint']],
  crow: [['sunny','sunny','sunny','crow'],[],['sunny']],
  twig: [['sky','sky','berry','berry'],[],['sky','sky']],
  swivel: [['sky','berry','berry','berry'],['sky','sky','sky'],[]],
  growing: [['mint','mint','mint'],['berry','berry'],['mint']],
};
const SPECIAL: Record<string,Record<number,[number,number]>> = { twig: {1:[Kind.TWIG,2]}, swivel:{0:[Kind.SWIVEL,4]}, growing:{1:[Kind.GROWING,2]} };
const LINES = ['tutorial.pick','tutorial.move.a','tutorial.home.a','tutorial.win.a'];

/** The original illustrated lessons, built from the same world painting, branches and animated birds. */
export class Tutorial {
  static readonly FLAG = 'tutorial.how_to_play';
  static readonly LESSONS = LESSONS;
  readonly node: Node;
  readonly modal: Dialog;
  step = 0;
  private layer!: Node;
  private worldNode!: Node;
  private birdLayer!: Node;
  private house!: Node;
  private doorway!: Node;
  private door: Rect = [0,0,0,0];
  private resident: BirdView | null = null;
  private hand!: Node;
  private glow!: Node;
  private knot: Node | null = null;
  private knotPoint: Point = [0,0];
  private branchArt: Node[] = [];
  private kinds: [number,number][] = [];
  private birds: BirdView[][] = [];
  private stars: Node[] = [];
  private slots: Slot[];
  private run = 0;

  static open(host: Node, done: () => void = () => {}, instant = false): Tutorial { return new Tutorial(host, '', 1, done, instant); }
  static open_lesson(host: Node, mechanic: string, world: number, done: () => void = () => {}, instant = false): Tutorial { return new Tutorial(host, mechanic, Math.max(1,Math.min(4,world)), done, instant); }
  static lesson_flag(mechanic: string): string { return 'lesson.' + mechanic; }
  static mechanics_in(def: Partial<LevelDefinition>): string[] {
    const found = new Set<string>();
    for (const branch of def.branches ?? []) {
      if (['twig','swivel','growing'].includes(branch.kind ?? '')) found.add(branch.kind!);
      for (const token of branch.birds) {
        if (token.startsWith('egg:')) found.add('egg');
        else if (token.endsWith(':sleep')) found.add('sleep');
        else if (token.endsWith(':mama') || token.endsWith(':chick')) found.add('mama');
        else if (token === 'crow') found.add('crow');
      }
    }
    return LESSONS.filter(mechanic => found.has(mechanic));
  }
  static first_level_with(mechanic: string): number {
    const levels = Levels.all().filter(level => this.mechanics_in(level).includes(mechanic));
    return levels.length ? Math.min(...levels.map(level => level.id)) : 0;
  }
  static due_lessons(profile: Profile, def: LevelDefinition): string[] { return this.mechanics_in(def).filter(mechanic => this.first_level_with(mechanic) === def.id && !profile.has_tutorial(this.lesson_flag(mechanic))); }

  private constructor(host: Node, readonly lesson: string, readonly world: number, done: () => void, readonly instant: boolean) {
    this.slots = Art.data(`world/scene/board_w${world}_slots`).slots;
    const heights = lesson ? [660,72] : [660,72,24], height = 236 + heights.reduce((a,b)=>a+b,0) + 24*(heights.length-1) + 168;
    const top = Math.max(32,(1920-height-48-162)/2);
    this.modal = Kit.layer(host, lesson ? 'Lesson' : 'Tutorial', () => { this.run++; done(); });
    this.node = this.modal.node;
    this.modal.rect = [60,top,960,height]; this.modal.actionY = top + height + 48;
    let y = top + 236; this.modal.ys = heights.map(h => { const at=y; y+=h+24; return at; });
    const dim = this.node.getChildByName('Dim')!.getComponent(Graphics)!; dim.clear(); dim.fillColor = new Color(5,18,8,209); dim.rect(0,-1920,1080,1920); dim.fill();
    Kit.warmFrame(this.modal.content,this.modal.rect);
    Kit.sign(this.modal.content, lesson ? `lesson_${lesson}_title` : 'tutorial_title',[220,top-14,640,223]);
    if (!lesson) Kit.close(this.modal.content,[1020-129.72-56,top+156],this.modal.close);
    if (!instant) Kit.pop(this.modal.content);
    this.show_step(0);
  }
  close(): void { this.modal.close(); }
  show_step(index: number): void {
    this.step = Math.max(0,Math.min(3,index)); this.run++;
    if (this.layer) Kit.destroy(this.layer);
    this.layer = UI.group(this.modal.content,'IllustratedStep'); this.resident = null;
    this.build_stage([188,this.modal.ys[0],...STAGE]);
    this.caption(this.lesson ? `lesson.${this.lesson}.line` : LINES[this.step],this.modal.ys[1]);
    if (!this.lesson) for(let i=0;i<4;i++) UI.fill(this.layer,'StepDot',[462+i*44,this.modal.ys[2],24,24],i===this.step?'#5ebc35':'#b8ae8f',12);
    const word = this.lesson ? '' : this.step === 3 ? 'white_play' : 'white_tutorial_next';
    const button = Kit.button(this.layer,this.lesson?'GotIt':this.step===3?'Play':'Next',[200,this.modal.actionY,680,Kit.BUTTON_H],word,()=>this.lesson||this.step===3?this.close():this.show_step(this.step+1),'wide');
    if (this.lesson) UI.word(button,'action.got_it',[108.8,32.4,462.4,97.2],'white');
    if (this.instant) this.pose_still();
    else { UI.opacity(this.layer,0); tween(this.layer.getComponent(UIOpacity)!).to(.2,{opacity:255}).start(); void this.loop(this.run); }
  }
  private caption(id:string,top:number):void {
    const entry=Art.pack().words[id].caption, name=`art/locale/${Art.locale}/${entry.file.replace(/\.png$/,'')}`;
    const [width,height]=Art.size(name), cap=Math.min(44,720*entry.cap/width), lineHeight=71.5*cap/44;
    const scale=Math.min(cap/entry.cap,720/width,lineHeight/height),w=width*scale,h=height*scale;
    UI.image(this.layer,name,[540-w/2,top+(72-h)/2,w,h]);
  }
  private build_stage(rect: Rect): void {
    const stage = UI.group(this.layer,'Stage',rect); stage.addComponent(Mask).type=Mask.Type.GRAPHICS_RECT;
    this.worldNode = UI.group(stage,'Scene',[0,0,1080,660/SCALE]); this.worldNode.setScale(SCALE,SCALE,1);
    UI.image_region(this.worldNode,`board_w${this.world}_bg`,[0,380,1080,660/SCALE],[0,0,1080,660/SCALE]);
    this.build_house(); this.build_branches(); this.birdLayer = UI.group(this.worldNode,'Birds');
    this.glow = UI.image(this.worldNode,'glow_soft',[-130,-130,260,260]); UI.opacity(this.glow,0);
    this.stars = [];
    if (!this.lesson && this.step===3) {
      this.stars.push(UI.art(this.worldNode,'glow_soft',[160,395,760,380]));
      for(let i=0;i<3;i++)this.stars.push(UI.image(this.worldNode,'island_star_full',[280+i*180,505,160,160],true));
      this.stars.forEach(star=>UI.opacity(star,0));
    }
    this.hand=UI.group(this.worldNode,'Hand',[...HAND_REST,150,180]);
    const [w,h]=Art.size('hand_pointer');UI.image(this.hand,'hand_pointer',[-25,-9,w,h]);this.hand.setScale(.8,.8,1);
    const frame=UI.group(this.layer,'SceneFrame',rect).addComponent(Graphics);
    frame.lineWidth=48;frame.strokeColor=new Color('#e4efaf');frame.roundRect(-24,-684,752,708,64);frame.stroke();
    frame.lineWidth=6;frame.strokeColor=new Color('#66a438');frame.roundRect(0,-660,704,660,40);frame.stroke();
    this.set_birds(this.starts());
  }
  private build_house(): void {
    const scale=HOUSE[2]/HOUSE_INK[2],bottom=HOUSE[1]+(70-HOUSE_INK[1])*scale;
    const rope=UI.group(this.worldNode,'Rope',[532,-40,16,bottom+40]);rope.addComponent(Mask).type=Mask.Type.GRAPHICS_RECT;
    UI.art(rope,'goal_rope_single',[0,0,16,1260*16/108],[6,0,108,1260]);
    this.house=UI.group(this.worldNode,'House',HOUSE);UI.art(this.house,'house_default',[0,0,HOUSE[2],HOUSE[3]],HOUSE_INK);
    const opening:Rect=[(HOUSE_DOOR[0]-HOUSE_INK[0])*scale,(HOUSE_DOOR[1]-HOUSE_INK[1])*scale,HOUSE_DOOR[2]*scale,HOUSE_DOOR[3]*scale];
    this.door=[HOUSE[0]+opening[0],HOUSE[1]+opening[1],opening[2],opening[3]];
    this.doorway=UI.group(this.house,'Doorway',opening);this.doorway.addComponent(Mask).type=Mask.Type.GRAPHICS_ELLIPSE;
  }
  private build_branches(): void {
    this.branchArt=[];this.kinds=[];this.knot=null;
    const parent=UI.group(this.worldNode,'Branches');
    for(let b=0;b<3;b++) {
      const kind=(SPECIAL[this.lesson]?.[b]??[Kind.NORMAL,4]).slice() as [number,number];this.kinds.push(kind);
      const slot=this.slot(b),right=b===1,origin:Point=[right?1080:0,PERCHES[b]+22];
      const anchor=UI.group(parent,'Branch',[...origin,0,0]),art=UI.group(anchor,'Art',[0,0,0,0]),size=Art.size(slot.name);
      UI.image(art,slot.name,[slot.at[0]-origin[0],slot.at[1]+PERCHES[b]-slot.perch-origin[1],...size]);
      art.setScale(this.branch_scale(b,kind[1]),kind[0]===Kind.TWIG?.75:1,1);this.branchArt.push(art);
      if(kind[0]===Kind.SWIVEL){const reach=this.length(b)*this.branch_scale(b,kind[1])-22;this.knotPoint=[origin[0]+(right?-reach:reach),origin[1]-8.8];this.knot=UI.image(this.worldNode,'btn_swivel_green',[this.knotPoint[0]-55,this.knotPoint[1]-55,110,110]);}
    }
  }
  private slot(b:number):Slot{return this.slots.find(slot=>slot.name===`board_w${this.world}_branch_${SLOTS[b]}`)!;}
  private length(b:number):number {const tip=this.slot(b).tip;return b===1?1080-tip:tip;}
  private pitch(b:number):number{return Math.min(165,Math.max(110,(this.length(b)-107.8-125)/3));}
  private branch_scale(b:number,seats:number):number{return Math.min(this.length(b),107.8+(Math.max(seats,2)-1)*this.pitch(b)+125)/this.length(b);}
  private seat(b:number,i:number):Point {const distance=107.8+i*this.pitch(b)-24;return [b===1?1080-distance:distance,PERCHES[b]];}
  private bird_rect(feet:Point,side=BIRD_SIDE):Rect{return [feet[0]-side/2,feet[1]-side*300/320,side,side];}
  private starts():string[][] {return this.lesson?LESSON_BIRDS[this.lesson]:STARTS[this.step];}
  private set_birds(layout:string[][]):void {
    UI.clear(this.birdLayer);
    this.birds=layout.map((row,b)=>row.map((token,i)=>{const code=this.code(b*8+i,token);return BirdView.create(this.birdLayer,Board.SPECIES[Board.species_of(code)],this.bird_rect(this.seat(b,i)),{code,asleep:!!(code&Board.ASLEEP),instant:this.instant});}));
  }
  private code(id:number,token:string):number {
    const parts=token.split(':');if(parts[0]==='egg')return Board.bird(id,Board.species_index(parts[1]),Board.EGG);
    return Board.bird(id,Board.species_index(parts[0]),({sleep:Board.ASLEEP,mama:Board.MAMA,chick:Board.CHICK} as Record<string,number>)[parts[1]]??0);
  }
  private top_run(b:number):BirdView[] {
    const row=this.birds[b],out:BirdView[]=[];if(!row.length)return out;const species=row[row.length-1].species;
    for(let i=row.length-1;i>=0;i--){const bird=row[i];if(bird.species!==species||bird.asleep||(bird.code&Board.EGG))break;out.unshift(bird);}return out;
  }
  private point_at(b:number):Point {const row=this.birds[b],seat=this.seat(b,Math.max(0,row.length-1));return row.length?[seat[0]+18,seat[1]-78]:[seat[0],seat[1]-24];}
  private set_resident(species:string):void {
    this.resident?.destroy();this.resident=null;if(!species)return;
    const r=UI.rect(this.doorway),rect=this.bird_rect([r[2]/2,r[3]*.97],170*.58);
    this.resident=BirdView.create(this.doorway,species,rect,{instant:this.instant});
    if(!this.instant){const position=this.resident.node.position.clone();this.resident.node.setPosition(position.x,position.y-90);tween(this.resident.node).to(.3,{position},{easing:'backOut'}).start();}
  }
  private alive(run:number):boolean{return run===this.run&&this.node.isValid&&this.layer.isValid;}
  private wait(seconds:number):Promise<void>{return new Promise(resolve=>tween(this.layer).delay(seconds).call(()=>resolve()).start());}
  private hand_to(point:Point,seconds=.55):Promise<void>{return new Promise(resolve=>tween(this.hand).to(seconds,{position:new Vec3(point[0],-point[1],0)},{easing:'sineInOut'}).call(()=>resolve()).start());}
  private tap():Promise<void>{
    const position=this.hand.position.clone();this.glow.setPosition(position.x-130,position.y+130);UI.opacity(this.glow,.85);this.glow.setScale(.5,.5,1);
    tween(this.glow).to(.35,{scale:new Vec3(1.1,1.1,1)}).start();tween(this.glow.getComponent(UIOpacity)!).to(.35,{opacity:0}).start();
    return new Promise(resolve=>tween(this.hand).to(.08,{scale:new Vec3(.736,.704,1),position:new Vec3(position.x,position.y-10,0)}).to(.14,{scale:new Vec3(.8,.8,1),position},{easing:'backOut'}).call(()=>resolve()).start());
  }
  private async move(run:number,from:number,to:number):Promise<void>{
    await this.hand_to(this.point_at(from));if(!this.alive(run))return;await this.tap();if(!this.alive(run))return;
    const group=this.top_run(from);group.forEach(bird=>{bird.setSelected(true);bird.setPose('flap');});
    await this.wait(.7);if(!this.alive(run))return;const point=this.point_at(to);if(!this.birds[to].length)point[1]-=24;
    await this.hand_to(point);if(!this.alive(run))return;await this.tap();if(!this.alive(run))return;
    const flights:Promise<void>[]=[];
    for(const bird of group){this.birds[from].splice(this.birds[from].indexOf(bird),1);bird.selected=false;const rect=this.bird_rect(this.seat(to,this.birds[to].length));this.birds[to].push(bird);flights.push(bird.flyTo([rect[0],rect[1]],.45));await this.wait(.07);if(!this.alive(run))return;}
    await Promise.all(flights);
  }
  private async fly_home(run:number,b:number):Promise<void>{
    const group=this.birds[b].slice();this.birds[b]=[];const rect=this.bird_rect([this.door[0]+this.door[2]/2,this.door[1]+this.door[3]/2]);
    const flights:Promise<void>[]=[];
    for(let i=group.length-1;i>=0;i--){const bird=group[i];flights.push(bird.flyTo([rect[0],rect[1]],.6).then(()=>{if(this.alive(run))bird.destroy();}));await this.wait(.12);if(!this.alive(run))return;}
    await Promise.all(flights);if(!this.alive(run))return;this.set_resident(group[0]?.species??'');
    tween(this.house).to(.1,{scale:new Vec3(1.08,.94,1)}).to(.25,{scale:new Vec3(1,1,1)},{easing:'backOut'}).start();await this.wait(.35);
  }
  private async reset(run:number):Promise<void>{
    UI.opacity(this.birdLayer,1);tween(this.birdLayer.getComponent(UIOpacity)!).to(.25,{opacity:0}).start();
    if(this.resident){UI.opacity(this.resident.node,1);tween(this.resident.node.getComponent(UIOpacity)!).to(.25,{opacity:0}).start();}
    this.stars.forEach(star=>tween(star.getComponent(UIOpacity)!).to(.25,{opacity:0}).start());void this.hand_to(HAND_REST,.4);
    for(const [key,kind] of Object.entries(SPECIAL[this.lesson]??{})){const b=Number(key);this.kinds[b][1]=kind[1];tween(this.branchArt[b]).to(.4,{scale:new Vec3(this.branch_scale(b,kind[1]),kind[0]===Kind.TWIG?.75:1,1)},{easing:'sineInOut'}).start();}
    await this.wait(.4);if(!this.alive(run))return;this.set_resident('');this.set_birds(this.starts());
    tween(this.birdLayer.getComponent(UIOpacity)!).to(.3,{opacity:255}).start();await this.wait(.3);
  }
  private async loop(run:number):Promise<void>{
    await this.wait(.5);let cycle=0;
    while(this.alive(run)){
      if(this.lesson){await this.lesson_cycle(run);if(!this.alive(run))return;await this.wait(.4);continue;}
      if(this.step===0){await this.hand_to(this.point_at(0));if(!this.alive(run))return;await this.tap();if(!this.alive(run))return;this.top_run(0).forEach(b=>{b.setSelected(true);b.setPose('flap');});await this.wait(1.6);if(!this.alive(run))return;this.top_run(0).forEach(b=>b.setSelected(false));await this.hand_to(HAND_REST,.6);if(!this.alive(run))return;await this.wait(.6);}
      else if(this.step===1){await this.move(run,0,cycle%2===0?1:2);if(!this.alive(run))return;await this.wait(1);if(!this.alive(run))return;await this.reset(run);}
      else if(this.step===2){await this.move(run,0,1);if(!this.alive(run))return;await this.wait(.35);if(!this.alive(run))return;await this.fly_home(run,1);if(!this.alive(run))return;await this.wait(1.4);if(!this.alive(run))return;await this.reset(run);}
      else {await this.move(run,2,0);if(!this.alive(run))return;await this.fly_home(run,0);if(!this.alive(run))return;await this.move(run,2,1);if(!this.alive(run))return;await this.fly_home(run,1);if(!this.alive(run))return;await this.hand_to(HAND_REST,.5);if(!this.alive(run))return;await this.stars_pop(run);if(!this.alive(run))return;this.resident?.showcase();await this.wait(1.8);if(!this.alive(run))return;await this.reset(run);}
      cycle++;if(!this.alive(run))return;await this.wait(.4);
    }
  }
  private async lesson_cycle(run:number):Promise<void>{
    switch(this.lesson){
      case 'sleep':await this.move(run,1,0);if(!this.alive(run))return;{const sleeper=this.birds[0][2];sleeper.setCode(sleeper.code&~Board.ASLEEP,false);sleeper.setPose('flap');}await this.wait(.6);if(!this.alive(run))return;await this.fly_home(run,0);break;
      case 'mama':await this.move(run,0,1);if(!this.alive(run))return;await this.chicks_follow(run,0,1);break;
      case 'egg':await this.move(run,0,1);if(!this.alive(run))return;{const egg=this.birds[0][0];egg.setCode(egg.code&~Board.EGG);egg.setPose('hop');}await this.wait(.6);if(!this.alive(run))return;await this.fly_home(run,1);break;
      case 'crow':case 'twig':await this.move(run,0,1);if(!this.alive(run))return;await this.wait(.4);if(!this.alive(run))return;await this.move(run,2,0);if(!this.alive(run))return;await this.fly_home(run,0);break;
      case 'swivel':await this.swivel(run,0);if(!this.alive(run))return;await this.wait(.4);if(!this.alive(run))return;await this.move(run,0,1);if(!this.alive(run))return;await this.fly_home(run,1);break;
      case 'growing':await this.move(run,2,0);if(!this.alive(run))return;await this.fly_home(run,0);if(!this.alive(run))return;await this.grow(1);break;
    }
    if(!this.alive(run))return;await this.hand_to(HAND_REST,.5);if(!this.alive(run))return;await this.wait(1.4);if(!this.alive(run))return;await this.reset(run);
  }
  private async chicks_follow(run:number,from:number,to:number):Promise<void>{
    const target=this.birds[to],species=target[target.length-1].species;
    for(let i=this.birds[from].length-1;i>=0;i--){const bird=this.birds[from][i];if(!(bird.code&Board.CHICK)||bird.species!==species)continue;this.birds[from].splice(i,1);const rect=this.bird_rect(this.seat(to,target.length));target.push(bird);await bird.flyTo([rect[0],rect[1]],.55);if(!this.alive(run))return;await this.wait(.2);}
  }
  private async swivel(run:number,b:number):Promise<void>{
    await this.hand_to([this.knotPoint[0],this.knotPoint[1]+6]);if(!this.alive(run))return;await this.tap();if(!this.alive(run))return;
    tween(this.knot!).to(.1,{scale:new Vec3(1.25,1.25,1)}).to(.25,{scale:new Vec3(1,1,1)},{easing:'backOut'}).start();
    this.birds[b].reverse().forEach((bird,i)=>{bird.setPose('hop');const rect=this.bird_rect(this.seat(b,i));void bird.flyTo([rect[0],rect[1]],.4);});await this.wait(.6);
  }
  private async grow(b:number):Promise<void>{
    this.kinds[b][1]++;tween(this.branchArt[b]).to(.6,{scale:new Vec3(this.branch_scale(b,this.kinds[b][1]),1,1)},{easing:'backOut'}).start();
    const point=this.seat(b,this.kinds[b][1]-1);this.glow.setPosition(point[0]-130,-point[1]+170);this.glow.setScale(.6,.6,1);UI.opacity(this.glow,0);
    tween(this.glow).delay(.2).to(.6,{scale:new Vec3(1.4,1.4,1)}).start();tween(this.glow.getComponent(UIOpacity)!).delay(.2).to(.3,{opacity:230}).to(.5,{opacity:0}).start();await this.wait(1);
  }
  private async stars_pop(run:number):Promise<void>{
    tween(this.stars[0].getComponent(UIOpacity)!).to(.3,{opacity:230}).start();
    for(const star of this.stars.slice(1)){UI.opacity(star,1);star.setScale(.2,.2,1);tween(star).to(.32,{scale:new Vec3(1,1,1)},{easing:'backOut'}).start();await this.wait(.28);if(!this.alive(run))return;}
  }
  private pose_still():void{
    if(this.lesson){
      switch(this.lesson){
        case 'sleep':case 'egg':this.top_run(this.lesson==='sleep'?1:0).forEach(b=>b.setSelected(true));this.place_hand(this.point_at(this.lesson==='sleep'?0:1));break;
        case 'mama':this.set_birds([['sky','sky'],['sunny:mama','sunny:chick'],['sky','sky']]);this.place_hand(this.point_at(1));break;
        case 'crow':this.set_birds([['sunny','sunny','sunny'],['crow'],['sunny']]);this.top_run(2).forEach(b=>b.setSelected(true));this.place_hand(this.point_at(0));break;
        case 'twig':this.set_birds([['sky','sky'],['berry','berry'],['sky','sky']]);this.place_hand(this.point_at(1));break;
        case 'swivel':this.place_hand([this.knotPoint[0],this.knotPoint[1]+6]);break;
        case 'growing':this.set_birds([[],['berry','berry'],[]]);this.kinds[1][1]=3;this.branchArt[1].setScale(this.branch_scale(1,3),1,1);this.set_resident('mint');break;
      }return;
    }
    if(this.step<2){this.top_run(0).forEach(b=>b.setSelected(true));this.place_hand(this.point_at(this.step===0?0:1));}
    else if(this.step===2){this.set_birds([['sky'],['sunny','sunny','sunny','sunny'],['berry','sky']]);this.place_hand(this.point_at(1));}
    else{this.set_birds([[],[],[]]);this.set_resident('sky');this.stars.forEach(star=>UI.opacity(star,1));UI.opacity(this.stars[0],.9);}
  }
  private place_hand(point:Point):void{this.hand.setPosition(point[0],-point[1]);}
}
