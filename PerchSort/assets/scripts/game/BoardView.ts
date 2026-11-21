import {sys} from 'cc';
import { _decorator, Component, Color, EffectAsset, EventTouch, Graphics, Mask, Material, Node, Sprite, Tween, UITransform, Vec3, Vec4, resources, tween } from 'cc';
import { Board, Booster, GameEvent, Kind, Move } from '../core/Board';
import { LevelDefinition, Levels } from '../core/Levels';
import { Worlds } from '../meta/Worlds';
import { GameSession } from '../core/GameSession';
import { Rules } from '../core/Rules';
import { Solver } from '../core/Solver';
import { Profile } from '../meta/Profile';
import { Progress } from '../meta/Progress';
import { UI, Point, Rect } from './UI';
import { Art } from './Art';
import { BirdView } from './BirdView';
import { Flat, ON_DARK } from './Flat';

export interface BoardCallbacks {
  settle(session: GameSession): void;
  won(result: { session: GameSession }): void;
  settings(): void;
  buy(kind: number): void;
  undo_more(): void;
  bird_info(species: string): void;
  rescue_offer(): void;
  restart_offer(picture: string): void;
  profile_changed(): void;
  sound(name: string): void;
}
export interface BoardOptions { instant?: boolean; entrance?: boolean }
interface BranchSlot { side: string; row: number; name: string; at: Point; perch: number; tip: number }
interface BirdEntry { bird: BirdView; code: number; outfit: string }
interface HandPointer { node: Node; trail: Graphics; origin: Point; target: Point; control: Point; start: number; kind: 'move' | 'swivel' | 'tool'; glow?: Node }
interface AmbientPiece { node: Node; x: number; y: number; speed: number; sway: number; phase: number; opacity: number; kind: string }
const { ccclass } = _decorator;

@ccclass('BoardTicker')
class BoardTicker extends Component {
  owner!: BoardView;
  update(dt: number): void { this.owner.tick(dt); }
}

/** The canonical board uses the original painted geometry in a top-left 1080 x 1920 canvas. */
export class BoardView {
  static readonly ENTRANCE_SECONDS = 4;
  static readonly BIRD_SCALE = 1.1;
  static readonly BIRD_SIDE = 170 * BoardView.BIRD_SCALE;
  static readonly PERCH_TOP = 600;
  static readonly PERCH_BOTTOM = 1640;
  static readonly TOOL_SIZE = 136;
  static readonly TOOL_Y = 1736;
  static readonly HINT_HAND_SCALE = .75;
  static readonly HINT_HOLE_HEAD = 165;
  static readonly HINT_HOLE_FOOT = 30;
  static readonly HINT_EDGE = 14;
  static readonly HINT_DOTS = 9;
  static readonly TOOL_PICTURES = ['island_icon_firefly', 'island_icon_sprout', 'island_icon_owl_eye', 'island_icon_bell', 'island_icon_scarecrow', 'island_icon_rain'];
  static readonly GIFT_COIN: Record<string, string> = { sunny: 'wake', sky: 'reveal', berry: 'encore', mint: 'grow', cocoa: 'scare', snow: 'undo', ember: 'hatch', ink: 'encore', hoot: 'wake' };
  static readonly HOUSES: Record<string, [string, Rect, Rect, number]> = {
    default: ['house_default', [122,62,778,878], [313,418,378,380], 70],
    acorn: ['house_style_acorn_body', [56,51,397,403], [148,205,204,199], 51],
    teapot: ['house_style_teapot_body', [29,55,458,398], [148,205,204,199], 55],
    mushroom: ['house_style_mushroom_body', [41,51,426,401], [148,204,204,200], 51],
    lantern: ['house_style_lantern_body', [50,61,408,393], [148,204,204,200], 61],
    pumpkin: ['house_style_pumpkin_body', [49,37,413,423], [148,205,204,199], 37],
    beehive: ['house_style_beehive_body', [54,53,401,403], [148,205,204,199], 53],
    teacup: ['house_style_teacup_body', [83,136,389,324], [165,221,170,169], 136],
    pinecone: ['house_style_pinecone_body', [59,14,390,441], [148,205,204,199], 14],
    cage: ['house_style_cage_body', [58,28,390,430], [150,205,204,200], 28],
  };
  readonly node: Node;
  readonly profile: Profile;
  readonly level: LevelDefinition;
  readonly callbacks: BoardCallbacks;
  session: GameSession;
  busy = false;
  instant = false;
  entrance_running = false;
  ready = false;
  entrance_started_at = 0;
  entrance_finished_at = 0;
  rescue_tool = -2;
  private suspended = false;
  private alive = true;
  private generation = 0;
  private clock = 0;
  private idle = 0;
  private idleNudged = false;
  private hintCancel: boolean[] = [false];
  private hintSearchKey = '';
  private hintWaiting = false;
  private hintRequest = '';
  private hintShown = '';
  private hintMove: Move | null = null;
  private hints = new Map<string, Move>();
  private failures = new Map<string, number>();
  private winnable = new Set<string>();
  private slots: BranchSlot[];
  private rowOffset = 0;
  private perchTop = 0;
  private perchStep = 0;
  private branches: Node[] = [];
  private branchWidths = new Map<number, number>();
  private layoutPending: Promise<unknown> = Promise.resolve();
  private birds = new Map<number, BirdEntry>();
  private homing = new Set<BirdView>();
  private branchLayer: Node;
  private birdLayer: Node;
  private inputLayer: Node;
  private hud: Node;
  private house!: Node;
  private houseLayer: Node;
  private rope!: Node;
  private glow!: Node;
  private doorway!: Node;
  private doorRect: Rect = [0,0,0,0];
  private resident: BirdView | null = null;
  private residentSpecies = -1;
  private houseStyle = '';
  private status: Node;
  private tools: Node;
  private effects: Node;
  private skillRow: Node;
  private skillKeys = new Map<string, Node>();
  private toolSlots = new Map<number, Point>();
  private guideKind = -1;
  private pointedTool: {kind:number;dim:boolean} | null = null;
  private pausedPointer: {kind:number;dim:boolean} | null = null;
  private guideQueue: number[] = [];
  private hintLayer: Node | null = null;
  private guideLayer: Node | null = null;
  private handPointer: HandPointer | null = null;
  private queued: [number, boolean][] = [];
  private buffering = false;
  private rescueDeclined = false;
  private skipLayer: Node | null = null;
  private entranceTargets = new Map<Node, Rect>();
  private timers = new Map<ReturnType<typeof setTimeout>, () => void>();
  private animations = new Map<Tween<any>, () => void>();
  private ambient: AmbientPiece[] = [];
  /** Absent when its download failed; hints and guides then show without the dimmed surround. */
  private spotlightEffect?: EffectAsset;
  private spotlightReady: Promise<void>;
  private holdId = 0;
  private held = false;
  private holdAt: Point = [0,0];

  constructor(host: Node, profile: Profile, level: LevelDefinition | number, callbacks: BoardCallbacks, options: BoardOptions = {}) {
    this.profile = profile;
    this.level = typeof level === 'number' ? Levels.get_level(level) : level;
    this.callbacks = callbacks;
    this.instant = options.instant ?? false;
    this.session = new GameSession(this.level, profile.inventory);
    this.node = UI.group(host, 'Board');
    this.spotlightReady = new Promise(resolve => resources.load('effects/hint_spotlight', EffectAsset, (error, asset) => {
      if (error) console.error('Hint spotlight failed to load', error); else this.spotlightEffect = asset; resolve();
    }));
    const ticker = this.node.addComponent(BoardTicker); ticker.owner = this;
    this.slots = Art.data(`world/scene/board_w${this.session.state.world}_slots`).slots;
    const world = this.session.state.world;
    const background = `board_w${world}${world === 1 || world === 10 ? '_open' : ''}_bg`;
    const backdrop = UI.image(this.node, background, [0,0,1080,1920]);
    UI.calm_backdrop(backdrop);
    this.buildAmbient();
    this.branchLayer = UI.group(this.node, 'Branches');
    this.inputLayer = UI.group(this.node, 'BoardInput');
    this.birdLayer = UI.group(this.node, 'Birds');
    this.hud = UI.group(this.node, 'HUD');
    this.houseLayer = UI.group(this.hud, 'HouseLayer');
    this.buildHouse();
    this.status = UI.group(this.hud, 'Status', [28,48, this.statusWidth(),92]);
    const gear = UI.hit_area(this.hud, 'Settings', [928,36,128,128], () => { if(!this.state.is_won()){this.suspend_input(); callbacks.settings();} });
    UI.art(gear, 'hud_gear_button', [8,8,112,112]);
    this.skillRow = UI.group(this.hud, 'SkillRow');
    this.buildSkillRow();
    this.tools = UI.group(this.hud, 'Tools');
    this.effects = UI.group(this.hud, 'HomeEffects');
    this.layoutBranches();
    this.syncBirds();
    this.refresh_hud();
    const startHints: [number[], Move][] = Art.data('levels/hints')[String(this.level.id)] || [];
    for (const [key, move] of startHints) { this.hints.set(key.join(','), move); this.winnable.add(key.join(',')); }
    this.busy = true;
    void this.begin(options.entrance !== false);
  }

  get state(): Board { return this.session.state; }
  static preview(host: Node, profile: Profile, level = 12, callbacks: Partial<BoardCallbacks> = {}): BoardView {
    const silent = () => {};
    return new BoardView(host, profile.duplicate_profile(), level, {
      settle: silent, won: silent, settings: silent, buy: silent, undo_more: silent, bird_info: silent,
      rescue_offer: silent, restart_offer: silent, profile_changed: silent, sound: silent, ...callbacks,
    }, { instant: true, entrance: false });
  }
  destroy(): void {
    this.alive = false; this.cancelPlayback(); this.hintCancel[0] = true;
    for (const entry of this.birds.values()) entry.bird.destroy();
    for (const bird of this.homing) bird.destroy();
    this.resident?.destroy();
    this.node.removeFromParent(); this.node.destroy();
  }
  private wait(seconds: number): Promise<void> {
    if (this.instant || seconds === 0) return Promise.resolve();
    return new Promise(resolve => { const id = setTimeout(() => { this.timers.delete(id); resolve(); }, seconds * 1000); this.timers.set(id, resolve); });
  }
  private animate(seconds: number, step: (t: number) => void, easing: 'sineInOut' | 'quadOut' | 'backOut' | 'linear' = 'sineInOut'): Promise<void> {
    if (this.instant) { step(1); return Promise.resolve(); }
    const value = { t: 0 };
    return new Promise(resolve => {
      const animation = tween(value).to(seconds, { t: 1 }, { easing, onUpdate: () => step(value.t) })
        .call(() => { this.animations.delete(animation); resolve(); });
      this.animations.set(animation, resolve); animation.start();
    });
  }
  private cancelPlayback(): void {
    this.generation++; this.holdId++; this.queued.length = 0; this.buffering = false;
    this.hintWaiting = false; this.hintCancel[0] = true;
    for (const [id, resolve] of this.timers) { clearTimeout(id); resolve(); }
    this.timers.clear();
    for (const [animation, resolve] of this.animations) { animation.stop(); resolve(); }
    this.animations.clear();
    for (const bird of this.homing) bird.destroy();
    this.homing.clear();
    for (const entry of this.birds.values()) entry.bird.stop();
    this.hideHint(); this.hideGuide();
    if (this.effects) UI.clear(this.effects);
  }
  private async begin(entrance: boolean): Promise<void> {
    await Promise.all([Art.waitPending(), this.spotlightReady]);
    if (!this.alive) return;
    this.ready = true;
    if (!this.instant && entrance) await this.entrance();
    else { this.busy = false; this.prefetchHint(); this.tryGuide(); }
  }
  private valid(generation: number): boolean { return this.alive && generation === this.generation; }
  suspend_input(): void {
    if (this.suspended) return;
    this.suspended = true;
    this.queued.length = 0;
    this.hintWaiting = false;
    this.pausedPointer = this.pointedTool;
    this.hideHint();
    this.hideGuide();
  }
  resume_input(): void {
    if (!this.suspended) return;
    this.suspended = false;
    this.refresh_hud();
    const pointer = this.pausedPointer;
    this.pausedPointer = null;
    if (this.guideKind >= 0) this.point_at_tool(this.guideKind, true);
    else if (pointer) this.point_at_tool(pointer.kind, pointer.dim);
    this.tryGuide();
  }
  sync_inventory(): void { this.profile.inventory = this.state.inventory.slice(); this.callbacks.profile_changed(); }
  refresh(): void { this.refresh_hud(); this.syncBirds(); }
  refresh_hud(): void {
    if (this.houseStyle !== this.profile.equipped_house) { this.buildHouse(); this.buildSkillRow(); }
    this.syncAppearance(); this.buildStatus(); this.buildTools();
    UI.enabled(this.hud.getChildByName('Settings')!, !this.state.is_won());
    for (let b = 0; b < this.state.branch_count(); b++) {
      const swivel = this.inputLayer.getChildByName('Swivel_' + b);
      if (swivel) UI.enabled(swivel, Rules.can_swivel(this.state, b));
    }
    for (const [species, key] of this.skillKeys) key.getChildByName('Home')!.active = this.state.homed.includes(Board.species_index(species));
    this.setResident(this.state.homed.at(-1) ?? -1);
  }
  granted(kind: number, count: number): void { this.session.grant_booster(kind, count); this.refresh_hud(); }
  granted_undos(count: number): void { this.session.grant_undos(count); this.buildTools(); }
  private syncAppearance(): void {
    if (this.busy) return;
    for (const [id, entry] of this.birds) {
      const outfit = this.look(entry.code);
      if (outfit === entry.outfit) continue;
      const rect = UI.rect(entry.bird.node), selected = entry.bird.selected;
      entry.bird.destroy();
      const bird = BirdView.create(this.birdLayer, this.bird_id(entry.code), rect,
        { instant: this.instant, code: entry.code, asleep: this.state.is_asleep(entry.code), outfit, seed: id });
      bird.setSelected(selected); this.birds.set(id, { bird, code: entry.code, outfit });
    }
  }
  restart(): void {
    this.cancelPlayback(); this.session.restart(); this.rescueDeclined = false;
    this.pausedPointer = null;
    this.sync_inventory(); this.layoutBranches(false); this.syncBirds(); this.busy = false; this.suspended = false;
    this.refresh_hud(); this.prefetchHint();
    if (this.guideKind >= 0) this.point_at_tool(this.guideKind, true);
  }

  private slot(branch: number): BranchSlot {
    const s = this.state, rows = Math.max(...this.slots.map(slot => slot.row)) + 1;
    const row = Math.min(rows - 1, s.rows[branch] + this.rowOffset), side = s.rights[branch] ? 'R' : 'L';
    return this.slots.find(slot => slot.row === row && slot.side === side)!;
  }
  private paintedLength(slot: BranchSlot): number { return slot.side === 'R' ? 1080 - slot.tip : slot.tip; }
  private seatStep(branch: number): number {
    return Math.min(165, Math.max(103.4, (this.paintedLength(this.slot(branch)) - 107.8 - 125) / 3));
  }
  private branchWidth(branch: number, seats: number): number {
    return Math.min(this.paintedLength(this.slot(branch)), 107.8 + (seats - 1) * this.seatStep(branch) + 125);
  }
  perch_y(branch: number): number { return this.perchTop + (this.state.rows[branch] * 2 + this.state.rights[branch]) * this.perchStep; }
  seat_x(branch: number, seat: number): number {
    const distance = 107.8 + seat * this.seatStep(branch) - 38;
    return this.state.rights[branch] ? 1080 - distance : distance;
  }
  seat_point(branch: number, seat: number): Point { return [this.seat_x(branch, seat), this.perch_y(branch)]; }
  private birdRect(feet: Point, side = BoardView.BIRD_SIDE): Rect { return [feet[0] - side / 2, feet[1] - side * 300 / 320, side, side]; }
  private layoutBranches(animate = true): void {
    const previous = this.branches.map(node => UI.rect(node));
    const motions: Promise<void>[] = [];
    const s = this.state, rows = Math.max(...s.rows) + 1, painted = Math.max(...this.slots.map(slot => slot.row)) + 1;
    if (!this.branches.length) this.rowOffset = Math.max(0, Math.min(Math.floor((painted - rows) / 2), painted - rows - 1));
    const lastPerch = Math.max(...s.rows.map((row, b) => row * 2 + s.rights[b]));
    this.perchStep = Math.min(1040 / Math.max(1,lastPerch), 150);
    this.perchTop = 600 + (1040 - this.perchStep * lastPerch) / 2;
    UI.clear(this.branchLayer); UI.clear(this.inputLayer); this.branches.length = 0;
    for (let b = 0; b < s.branch_count(); b++) {
      const slot = this.slot(b), right = s.rights[b] === 1, y = this.perch_y(b), edge = right ? 1080 : 0;
      const group = UI.group(this.branchLayer, `Branch_${b}`, [edge,y + 22,0,0]);
      const [width,height] = Art.size(slot.name), ratio = this.branchWidth(b, Math.max(s.caps[b],2)) / this.paintedLength(slot);
      const thin = s.kinds[b] === Kind.TWIG ? .75 : 1;
      const art=UI.image(group, slot.name, [(slot.at[0] - edge)*ratio,(slot.at[1] - slot.perch - 22)*thin,width*ratio,height*thin]);
      const oldWidth=this.branchWidths.get(b)??ratio;this.branchWidths.set(b,ratio);
      if(animate&&!this.instant&&previous.length){
        if(b>=previous.length){group.setScale(.05,.6,1);motions.push(this.animate(.6,t=>{if(group.isValid)group.setScale(.05+.95*t,.6+.4*t,1);},'backOut'));}
        else if(Math.abs(previous[b][1]-(y+22))>.1){UI.move(group,edge,previous[b][1]);motions.push(this.animate(.45,t=>{if(group.isValid)UI.move(group,edge,previous[b][1]+(y+22-previous[b][1])*t);}));}
        if(Math.abs(oldWidth-ratio)>.01)motions.push(this.animate(.6,t=>{const k=oldWidth+(ratio-oldWidth)*t;if(art.isValid)UI.place(art,[(slot.at[0]-edge)*k,(slot.at[1]-slot.perch-22)*thin,width*k,height*thin]);},'backOut'));
      }
      this.branches.push(group);
      const hit = UI.hit_area(this.inputLayer, `BranchHit_${b}`, [right ? 560 : 0,y - 187,520,220], () => {
        if (!this.held) this.tap_branch(b); this.held = false; this.holdId++;
      });
      hit.on(Node.EventType.TOUCH_START, (event: EventTouch) => this.beginHold(event,b));
      hit.on(Node.EventType.TOUCH_MOVE, (event: EventTouch) => {
        const at = event.getUILocation(); if (Math.hypot(at.x-this.holdAt[0],at.y-this.holdAt[1]) > 24) this.holdId++;
      });
      hit.on(Node.EventType.TOUCH_CANCEL, () => { this.holdId++; this.held = false; });
      if (s.kinds[b] === Kind.SWIVEL) {
        const reach = this.branchWidth(b,s.capacity) - 22, x = right ? 1080-reach : reach;
        const swivel = UI.button(this.inputLayer, `Swivel_${b}`, 'btn_swivel_green', [x-55,y+22-8.8-55,110,110], () => this.tap_swivel(b));
        UI.enabled(swivel, Rules.can_swivel(s, b));
      }
    }
    if(animate&&!this.instant&&previous.length)for(let b=0;b<s.branch_count();b++)s.items[b].forEach((code,i)=>{
      const entry=this.birds.get(Board.id_of(code));if(!entry)return;
      const start=UI.rect(entry.bird.node),target=this.birdRect(this.seat_point(b,i));
      if(Math.abs(start[1]-target[1])>2)motions.push(this.animate(.45,t=>{if(entry.bird.node.isValid)UI.move(entry.bird.node,start[0]+(target[0]-start[0])*t,start[1]+(target[1]-start[1])*t);}));
    });
    this.layoutPending=Promise.all(motions);
  }
  branch_at(point: Point): number {
    let selected = -1, distance = Infinity;
    for (let b=0;b<this.state.branch_count();b++) {
      const side = this.state.rights[b] ? point[0] >= 560 : point[0] <= 520;
      const dy = Math.abs(this.perch_y(b)-77-point[1]);
      if (side && dy < 110 && dy < distance) { selected=b; distance=dy; }
    }
    return selected;
  }
  private bird_id(code: number): string { return Worlds.look(this.state.world, Board.SPECIES[Board.species_of(code)]); }
  private look(code: number): string { return this.outfitOf(this.bird_id(code)); }
  private outfitOf(bird: string): string {
    const outfit = this.profile.equipped[bird];
    return outfit ? outfit.replace('outfit.','').replace('.','_outfit_') : '';
  }
  private makeBird(code: number, feet: Point): BirdEntry {
    const outfit = this.look(code), bird = BirdView.create(this.birdLayer,this.bird_id(code),this.birdRect(feet),
      { instant:this.instant,code,asleep:this.state.is_asleep(code),outfit,seed:Board.id_of(code) });
    const entry = { bird,code,outfit }; this.birds.set(Board.id_of(code),entry); return entry;
  }
  private syncBirds(): void {
    const seen = new Set<number>();
    for (let b=0;b<this.state.branch_count();b++) this.state.items[b].forEach((code,i) => {
      const id = Board.id_of(code); seen.add(id); let entry = this.birds.get(id);
      if (entry && entry.outfit !== this.look(code)) { entry.bird.destroy(); this.birds.delete(id); entry=undefined; }
      if (!entry) entry=this.makeBird(code,this.seat_point(b,i));
      entry.code=code; entry.bird.setCode(code,this.state.is_asleep(code)); entry.bird.setSelected(false);
      UI.place(entry.bird.node,this.birdRect(this.seat_point(b,i)));
    });
    for (const [id,entry] of this.birds) if (!seen.has(id)) { entry.bird.destroy(); this.birds.delete(id); }
  }
  private lift(branch: number,on: boolean): void {
    const items=this.state.items[branch],run=this.state.run_count(branch);
    for(let i=items.length-run;i<items.length;i++) this.birds.get(Board.id_of(items[i]))?.bird.setSelected(on);
  }
  private async beginHold(event: EventTouch, branch: number): Promise<void> {
    this.held=false; this.idle=0; const id=++this.holdId, at=event.getUILocation(); this.holdAt=[at.x,at.y];
    const local=this.node.getComponent(UITransform)!.convertToNodeSpaceAR(new Vec3(at.x,at.y,0));
    const x=local.x, items=this.state.items[branch];
    let code=-1,dx=88;
    items.forEach((candidate,i)=>{const distance=Math.abs(x-this.seat_x(branch,i)); if(distance<dx){code=candidate;dx=distance;}});
    if(code<0 || code & Board.EGG) return;
    await this.wait(.45);
    if(!this.alive || id!==this.holdId || this.busy || this.suspended) return;
    this.held=true; this.session.cancel_selection(); this.syncBirds(); this.suspend_input(); this.callbacks.bird_info(Board.SPECIES[Board.species_of(code)]);
  }

  private houseRect(): Rect {
    const content=BoardView.HOUSES[this.profile.equipped_house][1],scale=220/content[2];
    return [430,40,220,content[3]*scale];
  }
  private buildHouse(): void {
    this.resident?.destroy(); this.resident=null; this.residentSpecies=-1;
    UI.clear(this.houseLayer); this.houseStyle=this.profile.equipped_house;
    const [picture,content,door,ring]=BoardView.HOUSES[this.houseStyle],rect=this.houseRect(),scale=220/content[2];
    this.glow=UI.art(this.houseLayer,'house_glow',[rect[0]-rect[2]*.4,rect[1]-rect[3]*.4,rect[2]*1.8,rect[3]*1.8]);
    UI.opacity(this.glow,0);
    const bottom=40+(ring-content[1])*scale;
    this.rope=UI.group(this.houseLayer,'Rope',[532,-60,16,bottom+60]);
    const mask=this.rope.addComponent(Mask); mask.type=Mask.Type.GRAPHICS_RECT;
    UI.art(this.rope,'goal_rope_single',[0,0,16,1260*16/108],[6,0,108,1260]);
    this.house=UI.group(this.houseLayer,'House',rect);
    UI.art(this.house,picture,[0,0,rect[2],rect[3]],content);
    const opening:Rect=[(door[0]-content[0])*scale,(door[1]-content[1])*scale,door[2]*scale,door[3]*scale];
    this.doorRect=[rect[0]+opening[0],rect[1]+opening[1],opening[2],opening[3]];
    this.doorway=UI.group(this.house,'Doorway',opening);
    const doorMask=this.doorway.addComponent(Mask); doorMask.type=Mask.Type.GRAPHICS_ELLIPSE;
  }
  private setResident(species: number): void {
    if(species===this.residentSpecies) return;
    this.residentSpecies=species; this.resident?.destroy(); this.resident=null;
    if(species<0) return;
    const rect=UI.rect(this.doorway), side=170*.58, bird=Worlds.look(this.state.world,Board.SPECIES[species]);
    this.resident=BirdView.create(this.doorway,bird,this.birdRect([rect[2]/2,rect[3]*.97],side),{instant:this.instant,outfit:this.outfitOf(bird)});
  }
  private doorPoint(): Point { return [this.doorRect[0]+this.doorRect[2]/2,this.doorRect[1]+this.doorRect[3]/2]; }
  /** Numbers in the game font, white with a navy outline (digits are about 0.6 of the size wide). */
  private numberWidth(value:string,height:number): number { return value.length*height*.6; }
  private number(parent:Node,value:string,rect:Rect,align=0):Node { return Flat.text(parent,value,rect,Math.round(rect[3]*.95),ON_DARK,align).node.parent!; }
  private statusWidth():number { return 60+3*46+4+14+46+6+this.numberWidth('88',44); }
  private hudPill(parent:Node,width:number,height:number):void {
    const end=320*height/532;
    UI.image_region(parent,'panel_hud_jungle',[0,96,320,532],[0,0,end,height]);
    UI.image_region(parent,'panel_hud_jungle',[320,96,1532,532],[end,0,width-end*2,height]);
    UI.image_region(parent,'panel_hud_jungle',[1852,96,320,532],[width-end,0,end,height]);
  }
  private buildStatus():void {
    UI.clear(this.status); const width=this.statusWidth(),keep=GameSession.moves_left_for_star(this.session.move_count(),this.session.par);
    const shadow=UI.group(this.status,'Shadow',[0,6,width,92]);this.hudPill(shadow,width,92);UI.tint(shadow,'#2e1f0d');UI.opacity(shadow,.38);
    this.hudPill(this.status,width,92);
    const stars=this.session.stars();
    for(let i=0;i<3;i++) UI.image(this.status,i<stars?'island_star_full':'island_star_empty',[30+i*48,23,46,46],true);
    if(keep>=0){ UI.art(this.status,'icon_star_budget_steps',[186,23,46,46],[317,217,652,801]);this.number(this.status,String(keep),[238,24,width-268,44],-1); }
  }
  private buildSkillRow():void {
    UI.clear(this.skillRow); this.skillKeys.clear();
    const fresh=Levels.create_board(this.level),names=Levels.species_ids(fresh,true).filter(name=>!fresh.goal_mask||fresh.in_goal(Board.species_index(name)));
    names.sort((a,b)=>Board.species_index(a)-Board.species_index(b));
    const side=names.length<=8?88:74,width=names.length*side+(names.length-1)*14,house=this.houseRect();
    UI.place(this.skillRow,[540-width/2,house[1]+house[3]+14,width,side]);
    names.forEach((species,i)=>{
      const key=UI.group(this.skillRow,`Key_${species}`,[i*(side+14),0,side,side]);
      UI.art(key,'hud_tool_slot',[0,0,side,side]); UI.art(key,`${Worlds.look(this.state.world,species)}_face`,[side*.11,side*.11-2,side*.78,side*.78]);
      const gift=BoardView.GIFT_COIN[species]; if(gift)UI.art(key,`gift_coin_${gift}`,[side*.648,side*.648,side*.44,side*.44]);
      if(fresh.goal_mask)UI.art(key,'pic_check_ring',[-side*.12,-side*.12,side*.42,side*.42]);
      const mark=UI.art(key,'pic_check',[-side*.12,-side*.12,side*.42,side*.42]);mark.name='Home';mark.active=false;
      UI.hit_area(key,`BirdInfo_${species}`,[0,0,side,side],()=>{if(this.busy||this.suspended)return;this.session.cancel_selection();this.syncBirds();this.suspend_input();this.callbacks.bird_info(species);});
      this.skillKeys.set(species,key);
    });
  }
  private visibleTools():number[] {
    const result=this.session.can_undo()?[-1]:[];
    for(const kind of [Booster.FIREFLY,Booster.SAPLING]) if(Progress.is_booster_unlocked(this.profile,kind))result.push(kind);
    for(const kind of [Booster.OWL_EYE,Booster.MORNING_BELL,Booster.SCARECROW,Booster.SPRING_RAIN])if(Progress.is_booster_unlocked(this.profile,kind)&&this.session.booster_has_effect(kind))result.push(kind);
    return result;
  }
  private buildTools():void {
    UI.clear(this.tools);this.toolSlots.clear(); const tools=this.visibleTools(),side=BoardView.TOOL_SIZE,gap=Math.min(24,(1000-tools.length*side)/Math.max(1,tools.length-1));
    const width=tools.length*side+(tools.length-1)*gap;
    tools.forEach((kind,i)=>{
      const center:Point=[540-width/2+side/2+i*(side+gap),BoardView.TOOL_Y];this.toolSlots.set(kind,center);
      const button=UI.button(this.tools,kind<0?'Undo':`Tool_${kind}`,'hud_tool_slot',[center[0]-side/2,center[1]-side/2,side,side],()=>{if(kind<0)void this.undo();else void this.use_tool(kind);});
      const content:Rect[]=[[19,11,304,371],[16,17,333,289],[17,12,366,359],[16,17,368,335],[16,16,387,395],[16,17,319,386]];
      UI.art(button,kind<0?'island_icon_undo':BoardView.TOOL_PICTURES[kind],[18,18,100,100],kind<0?undefined:content[kind]);
      const stock=kind<0?this.session.undos_left:this.state.inventory[kind];this.number(button,stock>0?String(stock):'+',[0,142,136,46]);
      UI.enabled(button,kind<0||stock===0||this.session.booster_has_effect(kind));
      if(kind===Booster.FIREFLY&&this.hintWaiting)UI.opacity(button,.6);
    });
  }
  private async entrance():Promise<void> {
    const generation=this.generation,start=sys.now();this.entrance_started_at=start;this.busy=true;this.entrance_running=true;
    this.entranceTargets.clear();
    for(const node of [this.house,this.rope]){
      const rect=UI.rect(node);this.entranceTargets.set(node,rect);UI.move(node,rect[0],rect[1]-420);
      void this.wait(.1).then(()=>{if(this.valid(generation))void this.animate(.7,t=>UI.move(node,rect[0],rect[1]-420*(1-t)),'backOut');});
    }
    this.branches.forEach((branch,i)=>{
      branch.setScale(.02,.6,1);void this.wait(.95+i*.1).then(()=>{if(this.valid(generation))void this.animate(.5,t=>branch.setScale(.02+.98*t,.6+.4*t,1),'backOut');});
    });
    const entries=Array.from(this.birds.values());
    entries.forEach((entry,i)=>{
      const rect=UI.rect(entry.bird.node);this.entranceTargets.set(entry.bird.node,rect);
      UI.move(entry.bird.node,rect[0]>540?1240:-260,rect[1]-420-(i%4)*40);
      void this.wait(1.6+.85*i/Math.max(1,entries.length-1)).then(()=>{if(this.valid(generation)){entry.bird.flyTo([rect[0],rect[1]],.8);if(i%3===0)this.callbacks.sound('flutter');}});
    });
    this.skipLayer=UI.hit_area(this.node,'SkipEntrance',[0,0,1080,1920],()=>this.skip_entrance());
    await this.wait(BoardView.ENTRANCE_SECONDS);
    if(!this.valid(generation))return;
    this.finishEntrance();console.info(`ENTRANCE level=${this.level.id} skipped=false ms=${Math.round(sys.now()-start)}`);
  }
  skip_entrance():void {
    if(!this.entrance_running)return;
    this.cancelPlayback();
    for(const [node,rect] of this.entranceTargets){Tween.stopAllByTarget(node);UI.place(node,rect);}
    for(const branch of this.branches)branch.setScale(1,1,1);
    this.syncBirds();this.finishEntrance();console.info(`ENTRANCE level=${this.level.id} skipped=true`);
  }
  private finishEntrance():void {
    if(this.skipLayer){this.skipLayer.removeFromParent();this.skipLayer.destroy();}this.skipLayer=null;this.entrance_running=false;this.busy=false;this.entrance_finished_at=sys.now();
    this.entranceTargets.clear();this.prefetchHint();this.tryGuide();
  }

  tap_branch(branch:number):void {
    this.idle=0;
    if(this.suspended||this.state.is_won()||this.guideKind>=0)return;
    this.hideGuide();
    if(this.busy){if(this.buffering&&this.queued.length<2)this.queued.push([branch,false]);return;}
    if(this.session.selected<0){
      if(Rules.can_swivel(this.state,branch)&&this.state.run_count(branch)===0){void this.action(()=>this.session.swivel(branch));return;}
      if(this.session.select(branch)){this.callbacks.sound('select');this.lift(branch,true);}else this.refuse(branch);
      this.refreshHintSelection();return;
    }
    const from=this.session.selected;this.lift(from,false);
    if(from===branch){this.session.cancel_selection();if(Rules.can_swivel(this.state,branch))void this.action(()=>this.session.swivel(branch));else this.refreshHintSelection();return;}
    if(Rules.can_transfer(this.state,from,branch)){void this.action(()=>this.session.try_move(from,branch));return;}
    this.session.cancel_selection();if(this.session.select(branch)){this.callbacks.sound('select');this.lift(branch,true);}else this.refuse(branch);
    this.refreshHintSelection();
  }
  tap_swivel(branch:number):void {
    if(this.suspended||this.state.is_won()||this.guideKind>=0)return;
    if(this.busy){if(this.buffering&&this.queued.length<2)this.queued.push([branch,true]);return;}
    if(this.session.selected>=0)this.lift(this.session.selected,false);
    void this.action(()=>this.session.swivel(branch));
  }
  private drainTaps():void {
    while(!this.busy&&!this.suspended&&this.queued.length){const [branch,swivel]=this.queued.shift()!;if(swivel)this.tap_swivel(branch);else this.tap_branch(branch);}
  }
  private async action(action:()=>boolean):Promise<void> {
    this.hintWaiting=false;this.hideHint();this.hideGuide();const generation=this.generation;
    this.busy=true;this.buffering=true;
    if(action()){if(this.state.is_won())this.callbacks.settle(this.session);this.buildStatus();this.prefetchHint();await this.play_events(this.session.events.slice());}
    if(this.valid(generation))await this.finishAction(generation);
  }
  private async finishAction(generation:number):Promise<void> {
    if(!this.valid(generation))return;
    await this.layoutPending;if(!this.valid(generation))return;
    this.syncBirds();this.refresh_hud();this.busy=false;this.buffering=false;
    if(this.state.is_won()){this.queued.length=0;await this.celebrate();}
    else {void this.watchStuck();this.drainTaps();this.tryGuide();}
  }
  async undo():Promise<void> {
    if(this.busy||this.suspended||this.guideKind>=0)return;
    if(this.session.can_undo()&&this.session.undos_left<=0){this.suspend_input();this.callbacks.undo_more();return;}
    if(!this.session.undo())return;
    this.cancelPlayback();this.sync_inventory();this.callbacks.sound('undo');this.layoutBranches();this.prefetchHint();
    await this.finishAction(this.generation);
  }
  async use_tool(kind:number):Promise<void> {
    if(this.busy||this.suspended||(this.guideKind>=0&&kind!==this.guideKind))return;
    if(kind===Booster.FIREFLY){this.requestHint();return;}
    if(this.state.inventory[kind]===0){this.suspend_input();this.callbacks.buy(kind);return;}
    this.hintWaiting=false;this.queued.length=0;this.hideHint();this.hideGuide();const generation=this.generation;
    this.busy=true;
    const result=this.session.use_booster(kind);
    if(result.applied){
      this.profile.set_tutorial(Progress.used_flag(kind));this.endGuide(kind);this.sync_inventory();
      if(this.state.is_won())this.callbacks.settle(this.session);
      this.prefetchHint();
      this.callbacks.sound(['hint','sapling','owl_eye','morning_bell','scarecrow','spring_rain'][kind]);
      await this.play_events(this.session.events.slice());
    }else this.callbacks.sound('error');
    if(!this.valid(generation))return;
    this.layoutBranches();await this.finishAction(generation);
  }
  private refuse(branch:number):void {
    const sleeping=this.state.items[branch].slice().reverse().find(code=>this.state.is_asleep(code));
    this.callbacks.sound(sleeping===undefined?'error':'sleep');
    if(sleeping!==undefined){void this.birds.get(Board.id_of(sleeping))?.bird.nudgeAsleep();this.teachWake(branch);}
    const node=this.branches[branch];void this.animate(.25,t=>node.angle=Math.sin(t*Math.PI*4)*1.4*(1-t),'linear');
  }
  private teachWake(branch:number):void {
    const top=this.state.items[branch].at(-1)!;
    if(!(top&Board.EGG)&&this.state.is_asleep(top)){
      for(let from=0;from<this.state.branch_count();from++)if(from!==branch&&this.state.run_count(from)>0&&Board.species_of(this.state.items[from].at(-1)!)===Board.species_of(top)&&Rules.can_transfer(this.state,from,branch)){this.show_hint([from,branch]);return;}
    }
    const wake=Array.from(this.skillKeys).find(([species])=>BoardView.GIFT_COIN[species]==='wake'&&!this.state.homed.includes(Board.species_index(species)));
    if(wake){this.hideHint();this.hideGuide();const row=UI.rect(this.skillRow),key=UI.rect(wake[1]),at:Point=[row[0]+key[0]+key[2]/2,row[1]+key[3]/2];this.guideLayer=UI.group(this.hud,'WakeGuide');this.makeHand(this.guideLayer,at,at,'tool');}
    else this.point_at_tool(Booster.MORNING_BELL);
  }
  private key():string{return this.state.search_key(false).join(',');}
  private prefetchHint():void {
    const key=this.key();
    if(!this.hintCancel[0] && this.hintSearchKey===key)return;
    this.hintCancel[0]=true;
    this.hintSearchKey='';
    if(this.state.is_won()||!this.alive)return;
    if(this.hints.has(key)||this.failures.has(key))return;
    const cancel=[false];this.hintCancel=cancel;this.hintSearchKey=key;
    const snapshot=this.state.clone(),previous=this.session.last_move.slice(),started=sys.now();
    void Solver.hint_async(snapshot,previous,400000,8000,cancel).then(result=>{
      if(cancel[0]||!this.alive)return;
      this.hintSearchKey='';
      if(result.status===Solver.Status.SOLVED){
        for(const [route,move] of Object.entries(result.continuation||{})){this.hints.set(route,move);this.winnable.add(route);}
      }else this.failures.set(key,result.status);
      console.info(`HINT_SEARCH level=${this.level.id} status=${result.status} ms=${Math.round(sys.now()-started)}`);
      if(this.hintWaiting&&this.hintRequest===this.key()&&!this.busy){
        this.hintWaiting=false;const move=this.hints.get(this.key());if(move)this.applyHint(move);else{this.buildTools();this.callbacks.sound('error');void this.watchStuck();}
      }else if(this.key()===key&&!this.busy)void this.watchStuck();
    });
  }
  private requestHint():void {
    const key=this.key(),move=this.hints.get(key);
    if(this.hintWaiting && this.hintRequest===key)return;
    if(this.hintShown===key&&move){this.show_hint(move);return;}
    if(this.state.inventory[Booster.FIREFLY]===0){this.suspend_input();this.callbacks.buy(Booster.FIREFLY);return;}
    if(move){this.applyHint(move);return;}
    if(this.failures.get(key)===Solver.Status.UNSOLVABLE){this.callbacks.sound('error');void this.watchStuck();return;}
    this.failures.delete(key);this.hintWaiting=true;this.hintRequest=key;this.prefetchHint();this.buildTools();
  }
  private applyHint(move:Move):void {
    if(this.session.selected>=0)this.lift(this.session.selected,false);
    if(!this.session.try_use_firefly(move))return;
    this.hintShown=this.key();this.profile.set_tutorial(Progress.used_flag(Booster.FIREFLY));this.endGuide(Booster.FIREFLY);
    this.sync_inventory();this.callbacks.sound('hint');this.refresh_hud();this.show_hint(move);
  }
  private refreshHintSelection():void {
    if(!this.hintMove)return;
    if(this.session.selected<0||this.session.selected===this.hintMove[0])this.show_hint(this.hintMove);else this.hideHint();
  }
  private hintSeats(branch:number,first:number,count:number):Rect {
    const a=this.seat_point(branch,first),b=this.seat_point(branch,first+Math.max(1,count)-1),half=this.seatStep(branch)/2+30;
    const left=Math.max(BoardView.HINT_EDGE,Math.min(a[0],b[0])-half),right=Math.min(1080-BoardView.HINT_EDGE,Math.max(a[0],b[0])+half);
    return [left,a[1]-BoardView.HINT_HOLE_HEAD,right-left,BoardView.HINT_HOLE_HEAD+BoardView.HINT_HOLE_FOOT];
  }
  private hintHole(branch:number):Rect {
    const width=this.branchWidth(branch,this.state.capacity)+34,y=this.perch_y(branch);
    const x=this.state.rights[branch]?1080-BoardView.HINT_EDGE-width:BoardView.HINT_EDGE;
    return [x,y-BoardView.HINT_HOLE_HEAD,width,BoardView.HINT_HOLE_HEAD+BoardView.HINT_HOLE_FOOT];
  }
  private spotlight(parent:Node,holes:Rect[]):void {
    if(!this.spotlightEffect)return;
    const size=Art.size('glow_soft');
    const node=UI.image_region(parent,'glow_soft',[0,0,size[0],size[1]],[0,0,1080,1920]);
    node.name='Spotlight';
    const material=new Material();material.initialize({effectAsset:this.spotlightEffect});
    material.setProperty('holeA',new Vec4(...holes[0]));
    material.setProperty('holeB',new Vec4(...(holes[1]??[0,0,0,0] as Rect)));
    material.setProperty('shape',new Vec4(holes.length>1?44:56,22,.58,1));
    node.getComponent(Sprite)!.customMaterial=material;
    if(!this.instant){UI.opacity(node,0);void this.animate(.3,t=>{if(parent.parent&&node.isValid)UI.opacity(node,t);});}
  }
  show_hint(move:Move):void {
    if(this.hintLayer&&this.hintMove?.[0]===move[0]&&this.hintMove[1]===move[1])return;
    this.hideHint();this.hideGuide();this.hintMove=[...move];this.hintLayer=UI.group(this.node,'Hint');
    const [from,to]=move,source=this.seat_point(from,this.state.items[from].length-1);source[1]-=60;
    let target:Point;
    if(to<0){const width=this.branchWidth(from,this.state.capacity)-22;target=[this.state.rights[from]?1080-width:width,this.perch_y(from)+13.2];this.spotlight(this.hintLayer,[this.hintHole(from)]);}
    else{const count=Math.min(this.state.run_count(from),this.state.free_slots(to)),size=this.state.items[to].length,join=size>0?1:0;target=this.seat_point(to,size);target[1]-=60;this.spotlight(this.hintLayer,[this.hintSeats(from,this.state.items[from].length-count,count),this.hintSeats(to,size-join,count+join)]);}
    this.makeHand(this.hintLayer,source,target,to<0?'swivel':'move');
  }
  private hideHint():void {
    if(this.hintLayer){
      if(this.handPointer?.node.parent===this.hintLayer)this.handPointer=null;
      this.hintLayer.removeFromParent();this.hintLayer.destroy();this.hintLayer=null;
    }
    this.hintMove=null;
  }
  private hideGuide():void {
    this.pointedTool=null;
    if(this.guideLayer){
      if(this.handPointer?.node.parent===this.guideLayer)this.handPointer=null;
      this.guideLayer.removeFromParent();this.guideLayer.destroy();this.guideLayer=null;
    }
  }
  point_at_tool(kind:number,dim=false):void {
    this.hideHint();this.hideGuide();const center=this.toolSlots.get(kind);if(!center)return;
    this.pointedTool={kind,dim};
    this.guideLayer=UI.group(this.hud,'ToolGuide');
    if(dim)this.spotlight(this.guideLayer,[[center[0]-88,center[1]-88,176,228]]);
    const glow=UI.image(this.guideLayer,'glow_soft',[center[0]-108,center[1]-108,216,216]);UI.tint(glow,'#ffcc4d');UI.opacity(glow,.25);
    this.makeHand(this.guideLayer,[center[0]+6,center[1]-6],[center[0]+6,center[1]-6],'tool',glow);
  }
  private makeHand(parent:Node,origin:Point,target:Point,kind:'move'|'swivel'|'tool',glow?:Node):void {
    const trail=UI.group(parent,'HintTrail').addComponent(Graphics),at=kind==='swivel'?target:origin;
    const node=UI.group(parent,kind==='move'||kind==='swivel'?'HintHand':'Hand',[at[0],at[1],0,0]);
    const size=Art.size('hand_pointer');UI.image(node,'hand_pointer',[40-size[0]/2,60-size[1]/2,size[0],size[1]]);
    node.setScale(BoardView.HINT_HAND_SCALE,BoardView.HINT_HAND_SCALE,1);
    const rise=Math.min(260,90+Math.hypot(target[0]-origin[0],target[1]-origin[1])*.35);
    const control:Point=[(origin[0]+target[0])/2,(origin[1]+target[1])/2-rise];
    this.handPointer={node,trail,origin,target,control,start:this.clock,kind,glow};
    this.tickHand(this.instant);
  }
  private tickHand(still=false):void {
    const hand=this.handPointer;if(!hand)return;
    const sine=(t:number)=>(1-Math.cos(Math.PI*t))/2;
    const back=(t:number)=>{const x=t-1,c=1.70158;return 1+(c+1)*x*x*x+c*x*x;};
    const atCurve=(t:number):Point=>[(1-t)*(1-t)*hand.origin[0]+2*(1-t)*t*hand.control[0]+t*t*hand.target[0],(1-t)*(1-t)*hand.origin[1]+2*(1-t)*t*hand.control[1]+t*t*hand.target[1]];
    const age=this.clock-hand.start;let at:Point=hand.kind==='swivel'?hand.target:hand.origin,scale=1,alpha=1,shown=still?1:0,ripple=-1,rippleAt=at;
    const tap=(elapsed:number,point:Point)=>{
      at=point;rippleAt=point;
      if(elapsed<.16)scale=1-.18*sine(elapsed/.16);
      else if(elapsed<.36)scale=.82+.18*back((elapsed-.16)/.2);
      if(elapsed>=.16&&elapsed<.61)ripple=(elapsed-.16)/.45;
    };
    if(!still&&hand.kind==='tool'){
      const phase=age%.9,press=phase<.18?sine(phase/.18):phase<.4?1-back((phase-.18)/.22):0;
      scale=1-.18*press;at=[hand.origin[0]+10*press,hand.origin[1]+10*press];
    }else if(!still&&hand.kind==='swivel'){
      const phase=age%1.26;if(phase<.86)tap(phase,hand.target);
    }else if(!still){
      const phase=age%3.07;
      if(phase<.86)tap(phase,hand.origin);
      else if(phase<1.76){shown=sine((phase-.86)/.9);at=atCurve(shown);}
      else{shown=1;at=hand.target;if(phase<2.62)tap(phase-1.76,hand.target);else if(phase>=2.72)alpha=Math.max(0,1-(phase-2.72)/.2);}
    }
    UI.move(hand.node,at[0],at[1]);hand.node.setScale(BoardView.HINT_HAND_SCALE*scale,BoardView.HINT_HAND_SCALE*scale,1);UI.opacity(hand.node,alpha);
    const g=hand.trail;g.clear();
    if(ripple>=0){g.strokeColor=new Color(255,230,128,Math.round(255*(1-ripple)));g.lineWidth=9*(1-ripple)+2;g.circle(rippleAt[0],-rippleAt[1],26+58*ripple);g.stroke();}
    if(hand.kind==='move')for(let k=0;k<BoardView.HINT_DOTS;k++){
      const t=(k+1)/(BoardView.HINT_DOTS+1);if(t>shown)break;const point=atCurve(t);
      g.fillColor=new Color(92,56,13,89);g.circle(point[0],-point[1],13);g.fill();
      g.fillColor=new Color(255,237,158,255);g.circle(point[0],-point[1],9);g.fill();
    }
    if(hand.glow&&!still){
      const wave=(1-Math.cos(age*Math.PI*2))/2,side=216*(.9+.2*wave);
      UI.place(hand.glow,[hand.origin[0]-6-side/2,hand.origin[1]+6-side/2,side,side]);UI.opacity(hand.glow,.12+.2*wave);
    }
  }
  guiding():boolean{return this.guideKind>=0;}
  guide_tool(kind:number):void {
    if(this.instant||this.profile.has_tutorial(Progress.guided_flag(kind))||this.guideQueue.includes(kind)||this.guideKind===kind)return;
    this.guideQueue.push(kind);this.tryGuide();
  }
  private tryGuide():void {
    if(this.guideKind>=0||this.busy||this.suspended||this.state.is_won())return;
    const kind=this.guideQueue.find(k=>this.state.inventory[k]>0&&this.session.booster_has_effect(k)&&this.toolSlots.has(k));
    if(kind===undefined)return;
    this.guideQueue.splice(this.guideQueue.indexOf(kind),1);this.guideKind=kind;this.hideHint();this.session.cancel_selection();this.syncBirds();this.point_at_tool(kind,true);
  }
  private endGuide(kind:number):void {
    if(this.guideKind!==kind)return;
    this.guideKind=-1;this.hideGuide();this.profile.set_tutorial(Progress.guided_flag(kind));this.callbacks.profile_changed();
  }

  private async watchStuck():Promise<void> {
    if(this.instant||this.busy||this.suspended||this.state.is_won()||!this.session.can_undo())return;
    const snapshot=this.state,key=this.key(),history=this.session.history_size();
    if(Rules.has_legal_move(snapshot)&&this.failures.get(key)!==Solver.Status.UNSOLVABLE)return;
    // Moves change the board in place, so the position is compared as well as the board object.
    const generation=this.generation,stale=()=>!this.valid(generation)||this.state!==snapshot||this.key()!==key||this.session.history_size()!==history;
    await this.wait(.8);
    if(stale()||this.busy||this.suspended)return;
    if(!this.rescueDeclined&&this.state.world===1&&!this.session.free_rescue_used){
      this.rescueDeclined=true;this.point_at_tool(-1,true);this.suspend_input();this.callbacks.rescue_offer();return;
    }
    // The one offer is spent only when it is shown; a search dropped because the board moved on keeps it.
    const kinds=Array.from(this.toolSlots.keys()).filter(k=>k!==Booster.FIREFLY).sort((a,b)=>
      (a<0?(this.session.undos_left>0?0:Progress.UNDO_PRICE):Progress.booster_price(a))-(b<0?(this.session.undos_left>0?0:Progress.UNDO_PRICE):Progress.booster_price(b)));
    let chosen=-1;
    for(const kind of kinds){
      let candidate:Board;
      if(kind<0){if(!this.session.can_undo())continue;candidate=this.session.history_state(1).clone();}
      else{if(!Rules.booster_has_effect(snapshot,kind))continue;candidate=snapshot.clone();new Rules().booster(candidate,kind,false);}
      const solved=this.winnable.has(candidate.search_key().join(','))||(await Solver.hint_async(candidate,[],20000,600)).status===Solver.Status.SOLVED;
      if(stale())return;
      if(solved){chosen=kind;break;}
    }
    if(stale()||this.busy||this.suspended)return;
    this.rescue_tool=chosen;this.point_at_tool(chosen,true);
    if(!this.rescueDeclined&&this.state.world>1){this.rescueDeclined=true;this.suspend_input();this.callbacks.restart_offer(chosen<0?'island_icon_undo':BoardView.TOOL_PICTURES[chosen]);}
  }
  async rescue():Promise<void> {
    if(this.busy||!this.session.history_size())return;
    this.suspended=false;this.busy=true;this.hideGuide();const generation=this.generation;
    // The whole search shares one time budget; past it the rescue returns to the start, which is always solvable.
    let steps=this.session.history_size();const deadline=sys.now()+4000;
    for(let back=1;back<this.session.history_size();back++){
      const snapshot=this.session.history_state(back),left=deadline-sys.now();
      if(left<=0)break;
      if(this.winnable.has(snapshot.search_key().join(','))||(await Solver.hint_async(snapshot,[],60000,Math.min(1500,left))).status===Solver.Status.SOLVED){steps=back;break;}
      if(!this.valid(generation))return;
    }
    if(!this.valid(generation))return;
    this.session.rewind(steps);this.session.free_rescue_used=true;this.winnable.add(this.key());
    this.cancelPlayback();this.sync_inventory();this.callbacks.sound('undo');this.layoutBranches();this.prefetchHint();
    console.info(`RESCUE level=${this.level.id} steps=${steps}`);await this.finishAction(this.generation);
  }
  async play_events(events:GameEvent[]):Promise<void> {
    if(this.instant)return;
    const generation=this.generation;
    for(let index=0;index<events.length;index++){
      if(!this.valid(generation))return;
      const event=events[index];
      switch(event.type){
        case 'transferred': case 'chicks_followed': await this.flyGroup(event);break;
        case 'swivelled':{
          const flights=(event.birds as {id:number;to_seat:number}[]).map(item=>{
            const entry=this.birds.get(item.id);if(!entry)return Promise.resolve();
            const rect=this.birdRect(this.seat_point(event.branch,item.to_seat));return entry.bird.hopTo([rect[0],rect[1]],.4);
          });
          this.callbacks.sound('move');await Promise.all(flights);break;
        }
        case 'egg_wobbled':this.birds.get(event.id)?.bird.setPose('wobble');break;
        case 'hatched':{
          const entry=this.birds.get(event.id);if(entry){this.callbacks.sound('egg_hatch');this.sparkles(this.birdCenter(entry.bird),8,80,.4);entry.code&=~(Board.EGG|Board.REVEALED);await entry.bird.hatch();}break;
        }
        case 'egg_revealed':{const entry=this.birds.get(event.id);if(entry){entry.code|=Board.REVEALED;void entry.bird.reveal();}break;}
        case 'woke':{const entry=this.birds.get(event.id);if(entry){entry.code&=~Board.ASLEEP;void entry.bird.wake();this.callbacks.sound('wake_bell');}break;}
        case 'flock_home':{
          const later=events.slice(index+1).filter(item=>item.type==='flock_home').length,chain=this.state.chain-later;
          if(!this.state.is_won())void this.flockHome(event,chain);
          else if(later){void this.flockHome(event,chain);await this.wait(.63);}
          else await this.flockHome(event,chain);break;
        }
        case 'gift':{
          const targets=this.giftTargets(events,index),later=events.slice(index+1).some(item=>item.type==='flock_home');
          if(this.state.is_won()&&later)void this.gift(event,[]);
          else if(this.state.is_won()||targets.length)await this.gift(event,targets);
          else void this.gift(event,targets);break;
        }
        case 'branch_grew':case 'twig_upgraded':this.layoutBranches();break;
        case 'crows_left':{
          const departing:BirdView[]=[];
          const flights=(event.birds as {id:number}[]).map(item=>{
            const entry=this.birds.get(item.id);if(!entry)return Promise.resolve();
            this.birds.delete(item.id);this.homing.add(entry.bird);departing.push(entry.bird);
            const rect=UI.rect(entry.bird.node);return entry.bird.flyTo([rect[0]+(rect[0]>540?600:-600),-200],.8,{home:true});
          });
          await Promise.all(flights);if(this.valid(generation))for(const bird of departing){this.homing.delete(bird);bird.destroy();}break;
        }
      }
    }
  }
  private async flyGroup(event:GameEvent):Promise<void> {
    const birds:{id:number;to_seat:number}[]=event.type==='transferred'?event.ids.map((id:number,k:number)=>({id,to_seat:event.to_seat+k})):event.birds;
    const generation=this.generation;
    const flights=birds.map(async(item,k)=>{
      const entry=this.birds.get(item.id);if(!entry)return;
      await this.wait(k*.04);if(!this.valid(generation))return;
      const rect=this.birdRect(this.seat_point(event.to,item.to_seat));await entry.bird.flyTo([rect[0],rect[1]],.28);
    });
    this.callbacks.sound('move');
    void this.wait(.34).then(()=>{if(this.valid(generation))void this.dipBranch(event.to);});
    await Promise.all(flights);
  }
  private async dipBranch(branch:number):Promise<void> {
    const node=this.branches[branch],rect=UI.rect(node),generation=this.generation;
    const edge=this.state.rights[branch]?1080:0,length=this.paintedLength(this.slot(branch));
    const riders=this.state.items[branch].map(code=>this.birds.get(Board.id_of(code))).filter((entry):entry is BirdEntry=>!!entry);
    await this.animate(.25,t=>{
      if(!this.valid(generation)||!node.isValid)return;
      let curve:number;
      if(t<.3)curve=Math.sin(t/.3*Math.PI/2);
      else{const v=(t-.3)/.7,c=1.70158;curve=-((c+1)*(v-1)**3+c*(v-1)**2);}
      UI.move(node,rect[0],rect[1]+7*.4*curve);
      node.angle=7*.6/length*curve*(this.state.rights[branch]?1:-1)*180/Math.PI;
      for(const entry of riders)if(entry.bird.node.isValid){const r=UI.rect(entry.bird.node),weight=Math.min(1,Math.abs(r[0]+r[2]/2-edge)/length);entry.bird.sag=7*(.4+.6*weight)*curve;}
    });
  }
  /** chain: flocks home in a row, counting this one (Rules.finish pays 5 feathers for each after the first). */
  private async flockHome(event:GameEvent,chain:number):Promise<void> {
    const generation=this.generation,door=this.doorPoint(),ids=(event.ids as number[]).slice().reverse();
    const flights:BirdView[]=[],travel:Promise<void>[]=[];
    ids.forEach((id,k)=>{
      const entry=this.birds.get(id);if(!entry)return;
      this.birds.delete(id);this.homing.add(entry.bird);flights.push(entry.bird);
      entry.bird.node.setParent(this.hud);entry.bird.node.setSiblingIndex(this.hud.children.length-1);
      travel.push(this.wait(k*.12).then(async()=>{if(this.valid(generation)){const rect=this.birdRect(door);await entry.bird.flyTo([rect[0],rect[1]],.9,{home:true,doorway:this.doorway});}}));
    });
    await Promise.all(travel);
    if(!this.valid(generation))return;
    for(const bird of flights){this.homing.delete(bird);bird.destroy();}
    this.callbacks.sound('nest_home');this.setResident(Board.species_index(event.species));this.sparkles(door,12,150,.6);
    console.info(`FLOCK_HOME species=${event.species} chain=${chain}`);
    if(chain>1)this.combo(chain);
    void this.animate(.35,t=>{this.house.setScale(1+Math.sin(t*Math.PI*2)*.1,1-Math.sin(t*Math.PI*2)*.1,1);});
    void this.animate(.8,t=>UI.opacity(this.glow,Math.sin(t*Math.PI)*.9));
  }
  /** The chorus combo beside the house: x2, x3 ... and the 5 feathers it pays, popping in and floating up. */
  private combo(chain:number):void {
    if(this.instant)return;
    const generation=this.generation,house=UI.rect(this.house),w=250,h=200,x=house[0]+house[2]+12,y=house[1]+house[3]*.2;
    const box=UI.group(this.effects,'Combo',[x,y,w,h]);
    UI.number(box,'x'+chain,[0,0,w,120],'gold',-1);UI.art(box,'icon_purchase_feather',[4,130,64,64]);UI.number(box,'+5',[76,134,150,56],'gold',-1);
    this.callbacks.sound('star_'+Math.min(3,chain-1));
    void this.animate(1.3,t=>{
      const s=t<.15?.5+.65*t/.15:t<.25?1.15-.15*(t-.15)/.1:1;
      box.setScale(s,s,1);UI.move(box,x+w*(1-s)/2,y-90*t+h*(1-s)/2);UI.opacity(box,t<.7?1:1-(t-.7)/.3);
    },'linear').then(()=>{if(this.valid(generation)&&box.isValid){box.removeFromParent();box.destroy();}});
  }
  private birdCenter(bird:BirdView):Point{const rect=UI.rect(bird.node);return [rect[0]+rect[2]/2,rect[1]+rect[3]*.45];}
  private giftTargets(events:GameEvent[],index:number):Point[] {
    const result:Point[]=[];
    for(let i=index+1;i<events.length;i++){
      const event=events[i];if(['gift','flock_home','transferred','swivelled','wave'].includes(event.type))break;
      if(['woke','hatched','egg_revealed'].includes(event.type)){const entry=this.birds.get(event.id);if(entry)result.push(this.birdCenter(entry.bird));}
      if(event.type==='crows_left')for(const item of event.birds){const entry=this.birds.get(item.id);if(entry)result.push(this.birdCenter(entry.bird));}
      if(['branch_grew','twig_upgraded'].includes(event.type)){const at=this.seat_point(event.branch,Math.max(0,event.after-1));at[1]-=60;result.push(at);}
    }
    const effect=BoardView.GIFT_COIN[events[index].species];
    if(effect==='encore')result.push([264,94]);
    if(effect==='undo'&&this.toolSlots.has(-1))result.push(this.toolSlots.get(-1)!);
    return result.slice(0,8);
  }
  private async gift(event:GameEvent,targets:Point[]):Promise<void> {
    const generation=this.generation,start=this.doorPoint(),gift=BoardView.GIFT_COIN[event.species];
    const coin=UI.art(this.effects,`gift_coin_${gift}`,[start[0]-40,start[1]-40,80,80]);
    this.callbacks.sound('collect');
    if(!targets.length){await this.animate(.6,t=>{UI.move(coin,start[0]-40,start[1]-40-t*90);UI.opacity(coin,1-t);});if(this.valid(generation)){coin.removeFromParent();coin.destroy();}return;}
    const paying=gift==='undo'||gift==='encore';
    if(paying){
      await this.animate(.32,t=>{UI.place(coin,[start[0]-40-60*t,start[1]-40+240*t,80+120*t,80+120*t]);},'backOut');
      await this.wait(.3);if(!this.valid(generation))return;
    }
    const from:Point=paying?[start[0],start[1]+300]:start;
    const flights=targets.map((target,i)=>{
      const copy=UI.art(this.effects,`gift_coin_${gift}`,[from[0]-28,from[1]-28,56,56]);
      return this.wait(i*.05).then(async()=>{
        if(!this.valid(generation))return;
        const control:Point=[(from[0]+target[0])/2+(target[1]<from[1]?220:0),(from[1]+target[1])/2-120];
        await this.animate(paying?.55:.4,t=>{
          const x=(1-t)*(1-t)*from[0]+2*(1-t)*t*control[0]+t*t*target[0],y=(1-t)*(1-t)*from[1]+2*(1-t)*t*control[1]+t*t*target[1];
          UI.move(copy,x-28,y-28);
        });
        if(!this.valid(generation))return;
        copy.removeFromParent();copy.destroy();this.sparkles(target,paying?14:6,paying?180:70,.5);
        if(paying){const plus=this.number(this.effects,'+1',[target[0]-70,target[1]+(target[1]<500?70:-150),140,100]);void this.animate(.8,t=>{UI.move(plus,target[0]-70,target[1]+(target[1]<500?70:-150)-t*70);UI.opacity(plus,1-t);}).then(()=>{if(this.valid(generation)){plus.removeFromParent();plus.destroy();}});}
      });
    });
    coin.removeFromParent();coin.destroy();await Promise.all(flights);
  }
  private sparkles(at:Point,count:number,radius:number,seconds:number):void {
    if(this.instant)return;
    const generation=this.generation;
    for(let i=0;i<count;i++){
      const side=i%2?32:46,angle=i/count*Math.PI*2,spark=UI.image(this.effects,'pic_sparkle',[at[0]-side/2,at[1]-side/2,side,side]);
      void this.animate(seconds,t=>{UI.move(spark,at[0]+Math.cos(angle)*radius*t-side/2,at[1]+Math.sin(angle)*radius*t-side/2);spark.angle=t*180;UI.opacity(spark,1-t);},'quadOut').then(()=>{if(this.valid(generation)){spark.removeFromParent();spark.destroy();}});
    }
  }
  private async celebrate():Promise<void> {
    const generation=this.generation;this.busy=true;
    if(this.state.goal_mask&&this.birds.size){
      await Promise.all(Array.from(this.birds.values()).map(entry=>{const rect=UI.rect(entry.bird.node);return entry.bird.flyTo([rect[0]+(rect[0]>540?700:-700),rect[1]-520],.9,{home:true});}));
      if(!this.valid(generation))return;
      for(const entry of this.birds.values())entry.bird.destroy();this.birds.clear();
    }
    this.callbacks.sound('win_fanfare');this.resident?.setPose('cheer');this.sparkles(this.doorPoint(),24,340,1.2);this.confetti();
    await this.wait(1.3);if(this.valid(generation))this.callbacks.won({session:this.session});
  }

  private confetti():void {
    if(this.instant)return;
    const origin=this.doorPoint(),generation=this.generation;
    for(let i=0;i<56;i++){
      const size=40+(i*13)%23,piece=UI.image(this.effects,`confetti_${1+i%4}`,[origin[0]-size/2,origin[1]-size/2,size,size]);
      const angle=(-170+(i*37)%160)*Math.PI/180,speed=620+(i*97)%620,life=1.3+(i%6)*.1;
      void this.animate(life,u=>{
        const t=u*life,drag=1-Math.exp(-3*t),x=origin[0]+Math.cos(angle)*speed*drag/3+Math.sin(t*10+i)*20;
        const y=origin[1]+Math.sin(angle)*speed*drag/3+520*t*t;
        UI.move(piece,x-size/2,y-size/2);piece.angle=t*(300+(i*47)%420)*(i%2?1:-1);UI.opacity(piece,Math.min(1,(life-t)*3));
      },'linear').then(()=>{if(this.valid(generation)){piece.removeFromParent();piece.destroy();}});
    }
  }

  private buildAmbient():void {
    const world=this.state.world,variant=this.state.variant;
    const fall=[[],['fall_leaf_lime','fall_leaf_green','fall_petal_pink','fall_petal_cream'],['fall_leaf_olive','fall_leaf_lime'],['fall_leaf_gold','fall_leaf_olive'],['glow_soft'],['fall_petal_pink','fall_petal_cream'],['fall_leaf_green','fall_leaf_lime'],['fall_leaf_lime','fall_leaf_olive'],['fall_leaf_olive'],['fall_leaf_green','fall_leaf_lime'],['fall_petal_pink','fall_petal_cream']][world];
    let count=[0,7,6,9,44,12,5,6,3,6,10][world],mist=[0,0,3,0,0,0,0,0,4,2,0][world],motes=[0,0,0,0,0,0,10,8,0,0,0][world];
    let sun=world===2||world===4||world===8?.6:world===9?.9:.8;
    if(variant==='rain'||variant==='drizzle'){sun*=variant==='rain'?0:.3;count=Math.floor(count*.6);mist++;}
    if(variant==='mist'){sun*=.5;mist+=4;}
    if(variant==='windy')count=Math.floor(count*1.6);
    if(['golden','sunset','morning'].includes(variant)){sun*=1.2;motes=10;}
    if(['bluehour','aurora'].includes(variant)){sun=0;motes=10;}
    if(variant==='lanterns'){sun*=.3;motes=14;}
    const holder=UI.group(this.node,'Ambient');
    for(let i=0;i<mist;i++){
      const x=-800+(i*337)%1200,y=700+(i*389)%1050,node=UI.image(holder,'glow_soft',[x,y,1500,360]);UI.opacity(node,.38);
      this.ambient.push({node,x,y,speed:i%2?-18:18,sway:0,phase:i,opacity:.38,kind:'mist'});
    }
    if(sun>0)for(let i=0;i<2;i++){
      const side=i?1050:1500,group=UI.group(holder,'Sunbeam',[-40,-40,0,0]);UI.image(group,'light_rays',[-side/2,-side/2,side,side]);
      UI.opacity(group,sun*.16);this.ambient.push({node:group,x:-40,y:-40,speed:i?-.7:.5,sway:0,phase:i,opacity:sun*.16,kind:'beam'});
    }
    for(let i=0;i<count;i++){
      const picture=fall[i%fall.length],native=Art.size(picture),scale=world===4?.06+(i%5)*.015:.28+(i%5)*.045;
      const x=(i*367+43)%1180-50,y=(i*433+111)%2000-80,node=UI.image(holder,picture,[x,y,native[0]*scale,native[1]*scale]);
      if(world===10)UI.tint(node,'#d1b2ff');if(world===3&&i%3===0)UI.tint(node,'#ff765c');UI.opacity(node,world===4?.65:.8);
      this.ambient.push({node,x,y,speed:(world===4?40:80)+(i%5)*9,sway:world===4?24:44,phase:i*.7,opacity:world===4?.65:.8,kind:world===4?'snow':'leaf'});
    }
    for(let i=0;i<motes;i++){
      const x=(i*283+81)%1080,y=700+(i*237)%1220,side=12+(i%4)*9,node=UI.image(holder,'glow_soft',[x,y,side,side]);
      if(variant==='lanterns')UI.tint(node,'#ffb866');
      this.ambient.push({node,x,y,speed:5+(i%3)*3,sway:32,phase:i*.9,opacity:.65,kind:'mote'});
    }
    if(['rain','drizzle'].includes(variant))for(let i=0;i<16;i++){
      const x=(i*139+47)%1080,y=i*113%1920,node=UI.art(holder,'raindrop',[x,y,9,26]);UI.opacity(node,.3);
      this.ambient.push({node,x,y,speed:1250,sway:8,phase:i*.4,opacity:.3,kind:'rain'});
    }
  }
  tick(dt:number):void {
    if(!this.alive||this.instant)return;
    this.clock+=dt;
    if(this.houseStyle==='cage'&&!this.entrance_running)this.house.angle=Math.sin(this.clock*Math.PI/1.5)*2.6;
    for(const piece of this.ambient){
      const t=this.clock,p=piece.phase;
      if(piece.kind==='beam'){piece.node.angle=t*piece.speed;UI.opacity(piece.node,piece.opacity*(.8+.2*Math.sin(t*.4+p)));continue;}
      if(piece.kind==='mist'){UI.move(piece.node,((piece.x+t*piece.speed+1800)%2700)-1300,piece.y+Math.sin(t*.1+p)*18);UI.opacity(piece.node,piece.opacity*(.8+.2*Math.sin(t*.2+p)));continue;}
      if(piece.kind==='mote'){UI.move(piece.node,piece.x+Math.sin(t*.45+p)*piece.sway,piece.y-Math.sin(t*.2+p)*100);UI.opacity(piece.node,piece.opacity*(.5+.5*Math.sin(t*.8+p)));continue;}
      const y=((piece.y+t*piece.speed+80)%2080)-80,x=piece.x+Math.sin(t*.8+p)*piece.sway+(this.state.variant==='windy'?t*35%220:0);
      UI.move(piece.node,x,y);
      if(piece.kind==='leaf')piece.node.angle=Math.sin(t*.6+p)*35;
    }
    this.tickHand();
    if(this.busy||this.suspended||this.state.is_won())return;
    this.idle+=dt;
    if(this.guideKind<0&&!this.guideLayer&&!this.hintLayer&&!this.idleNudged&&this.idle>=12&&this.state.inventory[Booster.FIREFLY]>0){this.idleNudged=true;this.point_at_tool(Booster.FIREFLY);}
    if(!this.hintLayer&&!this.guideLayer&&!this.profile.has_tutorial('keys.shown')&&this.idle>=1&&this.skillKeys.size){
      this.profile.set_tutorial('keys.shown');this.callbacks.profile_changed();
      const first=this.skillKeys.values().next().value as Node,rect=UI.rect(first),row=UI.rect(this.skillRow);
      if(!this.guideLayer){this.guideLayer=UI.group(this.hud,'KeysPointer');const pointer=UI.art(this.guideLayer,'hand_pointer',[row[0]+rect[0]+rect[2]/2,row[1]+rect[3]/2,110,150]);
        const layer=this.guideLayer;void this.animate(1.8,t=>{if(this.guideLayer===layer)pointer.setScale(1-Math.sin(t*Math.PI*4)*.1,1-Math.sin(t*Math.PI*4)*.1,1);},'linear').then(()=>{if(this.guideLayer===layer)this.hideGuide();});}
    }
  }
}
