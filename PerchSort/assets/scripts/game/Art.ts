import { JsonAsset, resources, SpriteFrame, Rect as CocosRect, Size, Vec2, Texture2D } from 'cc';
import type { Rect } from './UI';

/** A bird frame packed in its bird's sheet has `piece`: [x, y] of the piece in the sheet, then [left, top, width, height] of the same piece in the frame. */
type ImageEntry = {path:string;size:[number,number];ink:Rect;piece?:[number,number,number,number,number,number];tex?:[number,number]};
type Manifest = {images:Record<string,ImageEntry>;aliases:Record<string,string>;data:Record<string,any>;sounds:Record<string,string>};
export class Art {
  static manifest:Manifest;
  static locale='en';
  static pending = new Map<string,Promise<SpriteFrame>>();
  /** Loaded frames by manifest key. */
  static cache = new Map<string,SpriteFrame>();
  private static sheets = new Map<string,Promise<SpriteFrame>>();
  static errors:string[]=[];
  static async init():Promise<void> {
    this.manifest=await new Promise((resolve,reject)=>resources.load('manifest',JsonAsset,(e,a)=>e?reject(e):resolve(a.json as Manifest)));
  }
  static key(name:string):string { return this.manifest.images[name]?name:this.manifest.aliases[name]; }
  static has(name:string):boolean { return !!this.key(name); }
  static entry(name:string):ImageEntry { const key=this.key(name);if(!key)throw new Error(`Missing artwork: ${name}`);return this.manifest.images[key]; }
  static size(name:string):[number,number] { return this.entry(name).size; }
  /** Size of the texture a picture is drawn from: its own size, or its sheet's. */
  static texture_size(name:string):[number,number] { const e=this.entry(name);return e.tex??e.size; }
  static ink(name:string):Rect { return this.entry(name).ink; }
  /** The row of a bird's feet in its 320 pictures: 300 for the plain birds; a world bird that has a hanging tail or a crest records its own (birds/feet.json). */
  static feet(bird:string):number { return this.data('birds/feet')?.[bird]??300; }
  static data(path:string):any { return this.manifest.data[path.replace(/^res:\/\//,'').replace(/\.json$/,'')]; }
  static frames(prefix:string):string[] {const out:string[]=[];for(let i=1;this.has(`${prefix}_${String(i).padStart(2,'0')}`);i++)out.push(`${prefix}_${String(i).padStart(2,'0')}`);return out;}
  static setLocale(code:string):void {this.locale=code.startsWith('zh')?'zh':'en';}
  static pack():any{return this.data(`art/locale/${this.locale}/manifest`);}
  static load(name:string):Promise<SpriteFrame> {
    const key=this.key(name),e=this.entry(name);
    if(this.cache.has(key))return Promise.resolve(this.cache.get(key)!);
    if(this.pending.has(key))return this.pending.get(key)!;
    const source=e.piece?this.sheet(e.path).then(sheet=>this.cut(sheet,e)):this.fetch(e.path);
    const promise=source.then(frame=>{this.cache.set(key,frame);return frame;}).finally(()=>this.pending.delete(key));this.pending.set(key,promise);return promise;
  }
  private static fetch(path:string):Promise<SpriteFrame> {
    return new Promise<SpriteFrame>((resolve,reject)=>resources.load(path+'/spriteFrame',SpriteFrame,(error,frame)=>{
      if(error){this.errors.push(`${path}: ${error.message}`);reject(error);return;}
      frame.addRef();resolve(frame);
    }));
  }
  /** A bird's sheet is downloaded once; every frame of the bird is a piece of it. */
  private static sheet(path:string):Promise<SpriteFrame> {
    if(!this.sheets.has(path))this.sheets.set(path,this.fetch(path).catch(error=>{this.sheets.delete(path);throw error;}));
    return this.sheets.get(path)!;
  }
  /** The frame at its full size: the piece is placed where it sits in the frame, the rest is transparent (sprites draw untrimmed). */
  private static cut(sheet:SpriteFrame,e:ImageEntry):SpriteFrame {
    const [x,y,left,top,w,h]=e.piece!,[fw,fh]=e.size,sf=new SpriteFrame();
    sf.texture=sheet.texture;sf.rect=new CocosRect(x,y,w,h);sf.originalSize=new Size(fw,fh);sf.offset=new Vec2(left+w/2-fw/2,fh/2-top-h/2);sf.packable=false;return sf;
  }
  static async preload(names:string[]):Promise<void>{await Promise.all(Array.from(new Set(names)).map(n=>this.load(n)));}
  static async waitPending():Promise<void>{while(this.pending.size)await Promise.all(Array.from(this.pending.values()));}
  private static deferred:string[]=[];
  private static draining=false;
  /** Loads frames a screen does not need to open, a few at a time once nothing else is loading, so opening a page never waits for them. */
  static defer(names:string[]):void {
    for(const name of names)if(!this.cache.has(this.key(name))&&!this.deferred.includes(name))this.deferred.push(name);
    if(!this.draining){this.draining=true;this.drain();}
  }
  private static drain():void {
    setTimeout(()=>{
      if(this.pending.size){this.drain();return;}
      const batch=this.deferred.splice(0,6);
      if(!batch.length){this.draining=false;return;}
      void Promise.all(batch.map(name=>this.load(name))).finally(()=>this.drain());
    },30);
  }
  static region(source:SpriteFrame, region:Rect):SpriteFrame {
    const sf=new SpriteFrame();sf.texture=source.texture;sf.rect=new CocosRect(...region);sf.originalSize=new Size(region[2],region[3]);sf.offset=new Vec2(0,0);sf.packable=false;return sf;
  }
  static painted(id:string):{name:string;region?:Rect}{const e=this.pack().painted[id];if(!e)throw new Error(`Missing painted word ${this.locale}:${id}`);return {name:`art/locale/${this.locale}/${e.file.replace(/\.png$/,'')}`,region:e.region};}
  static word(id:string,style='brown'):{name:string;region?:Rect}{
    const variants=this.pack().words[id];
    if(!variants)throw new Error(`Missing word ${this.locale}:${id}`);
    const e=variants[style]??variants.caption??variants.brown??variants.white??variants.gold;
    return e.painted_id?this.painted(e.painted_id):{name:`art/locale/${this.locale}/${e.file.replace(/\.png$/,'')}`};
  }
}
