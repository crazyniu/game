import { Node, UITransform, Sprite, Color, UIOpacity, Graphics, Mask, ScrollView, Vec2, Vec3, Vec4, BlockInputEvents, Button, Tween, tween, EventTouch, Layers, Material, EffectAsset, resources } from 'cc';
import { Art } from './Art';
export type Rect=[number,number,number,number];
export type Point=[number,number];
const rects=new WeakMap<Node,Rect>();
const numbers=new WeakMap<Node,{style:string;align:number;digits:boolean}>();
const actions=new WeakMap<Node,()=>void>();
const disabled=new WeakSet<Node>();
export class UI {
  static onTap:()=>void=()=>{};
  static group(parent:Node,name:string,r:Rect=[0,0,1080,1920]):Node {const n=new Node(name);n.layer=Layers.Enum.UI_2D;n.addComponent(UITransform).setAnchorPoint(0,1);parent.addChild(n);this.place(n,r);return n;}
  static place(n:Node,r:Rect):Node {rects.set(n,[...r]);n.getComponent(UITransform)!.setContentSize(r[2],r[3]);n.setPosition(r[0],-r[1]);return n;}
  static rect(n:Node):Rect{return rects.get(n)!;}
  static move(n:Node,x:number,y:number):void{const r=this.rect(n);this.place(n,[x,y,r[2],r[3]]);}
  static clear(n:Node):void {for(const child of [...n.children]){Tween.stopAllByTarget(child);child.removeFromParent();child.destroy();}}
  static opacity(n:Node,a:number):void{(n.getComponent(UIOpacity)??n.addComponent(UIOpacity)).opacity=Math.round(a*255);}
  static tint(n:Node,color:string|Color):void{const c=typeof color==='string'?new Color().fromHEX(color):color;for(const sprite of n.getComponentsInChildren(Sprite))sprite.color=c;}
  static image(parent:Node,name:string,r:Rect,fit=false):Node {
    if(fit){const [w,h]=Art.size(name),k=Math.min(r[2]/w,r[3]/h);r=[r[0]+(r[2]-w*k)/2,r[1]+(r[3]-h*k)/2,w*k,h*k];}
    const n=this.group(parent,name,r),sprite=n.addComponent(Sprite);sprite.sizeMode=Sprite.SizeMode.CUSTOM;sprite.trim=false;
    Art.load(name).then(frame=>{if(n.isValid){sprite.spriteFrame=frame;sprite.sizeMode=Sprite.SizeMode.CUSTOM;}}).catch(e=>console.error(e));return n;
  }
  static cover(parent:Node,name:string,r:Rect):Node{
    const [w,h]=Art.size(name),k=Math.max(r[2]/w,r[3]/h);return this.image(parent,name,[r[0]+(r[2]-w*k)/2,r[1]+(r[3]-h*k)/2,w*k,h*k]);
  }
  static silhouette(n:Node):void{const [w,h]=Art.texture_size(n.name);this.effect(n,'silhouette',{drawSize:new Vec4(w,h,0,0)});}
  static image_region(parent:Node,name:string,source:Rect,r:Rect):Node {
    const n=this.group(parent,name,r),sprite=n.addComponent(Sprite);sprite.sizeMode=Sprite.SizeMode.CUSTOM;sprite.trim=false;
    Art.load(name).then(frame=>{if(n.isValid){sprite.spriteFrame=Art.region(frame,source);sprite.sizeMode=Sprite.SizeMode.CUSTOM;}}).catch(e=>console.error(e));return n;
  }
  static art(parent:Node,name:string,r:Rect,content?:Rect):Node{const c=content??Art.ink(name),[w,h]=Art.size(name),k=Math.min(r[2]/c[2],r[3]/c[3]);return this.image(parent,name,[r[0]+(r[2]-c[2]*k)/2-c[0]*k,r[1]+(r[3]-c[3]*k)/2-c[1]*k,w*k,h*k]);}
  static piece(parent:Node,name:string,p:Point,w:number):Node{const s=Art.size(name);return this.image(parent,name,[p[0],p[1],w,w*s[1]/s[0]]);}
  static piece_fit(parent:Node,name:string,c:Point,b:Point):Node{return this.image(parent,name,[c[0]-b[0]/2,c[1]-b[1]/2,b[0],b[1]],true);}
  static nine(parent:Node,name:string,r:Rect,m:Rect=[40,40,40,40],scale=1):Node{return this.nine_region(parent,name,[0,0,...Art.size(name)],r,m,scale);}
  static nine_region(parent:Node,name:string,source:Rect,r:Rect,m:Rect,scale=1):Node{
    if(scale!==1){const holder=this.group(parent,name,r);const inner=this.nine_region(holder,name,source,[0,0,r[2]/scale,r[3]/scale],m);inner.setScale(scale,scale,1);return holder;}
    const n=this.group(parent,name,r),s=n.addComponent(Sprite);s.type=Sprite.Type.SLICED;s.sizeMode=Sprite.SizeMode.CUSTOM;
    Art.load(name).then(f=>{if(n.isValid){const sf=Art.region(f,source);sf.insetLeft=m[0];sf.insetTop=m[1];sf.insetRight=m[2];sf.insetBottom=m[3];s.spriteFrame=sf;s.sizeMode=Sprite.SizeMode.CUSTOM;}}).catch(e=>console.error(e));return n;
  }
  static panel(parent:Node,name:string,r:Rect):Node{return this.nine(parent,name,r);}
  static paint(parent:Node,entry:{name:string;region?:Rect},r:Rect,align=0):Node{
    const c=entry.region??Art.ink(entry.name),k=Math.min(r[2]/c[2],r[3]/c[3]),w=c[2]*k,h=c[3]*k;
    return this.image_region(parent,entry.name,c,[r[0]+(r[2]-w)*(.5+.5*align),r[1]+(r[3]-h)/2,w,h]);
  }
  static word(parent:Node,id:string,r:Rect,style='brown',align=0):Node{const n=this.paint(parent,Art.word(id,style),r,align);if(style==='brown'||style==='caption')this.warm(n);return n;}
  static painted(parent:Node,id:string,r:Rect,align=0):Node{const n=this.paint(parent,Art.painted(id),r,align);if(/^(brown_|row_|tab_|cap_|caption_)/.test(id))this.warm(n);return n;}
  static number(parent:Node,value:string|number,r:Rect,style='brown',align=0):Node{const n=this.group(parent,'Number',r);numbers.set(n,{style,align,digits:false});this.set_number(n,String(value));return n;}
  static digits(parent:Node,value:string|number,r:Rect):Node{const n=this.group(parent,'Digits',r);numbers.set(n,{style:'cream',align:0,digits:true});this.set_number(n,String(value));return n;}
  static set_number(n:Node,value:string|number,style?:string):void{
    this.clear(n);const config=numbers.get(n)!;if(style)config.style=style;const r=this.rect(n),v=String(value);
    const names=Array.from(v).map(c=>config.digits?`icon_home_island_digit_${c}`:`art/locale/${Art.locale}/${Art.pack().numbers.styles[config.style==='white'?'cream':config.style].glyphs[c].replace(/\.png$/,'')}`);
    if(!names.length)return;
    if(config.digits){const cell=Art.size(names[0]),k=Math.min(r[2]/(cell[0]*names.length),r[3]/cell[1]);const top=Math.min(...names.map(name=>Art.ink(name)[1])),bottom=Math.max(...names.map(name=>Art.ink(name)[1]+Art.ink(name)[3])),h=bottom-top;names.forEach((name,i)=>this.image(n,name,[(r[2]-cell[0]*names.length*k)/2+i*cell[0]*k,r[3]/2-(top+h/2)*k,cell[0]*k,cell[1]*k]));return;}
    const inks=names.map(name=>Art.ink(name)),width=inks.reduce((sum,c)=>sum+c[2]+14,0),k=Math.min(r[2]/width,r[3]/128);let x=(r[2]-width*k)*(.5+.5*config.align);
    names.forEach((name,i)=>{const s=Art.size(name);this.image(n,name,[x+(7-inks[i][0])*k,(r[3]-128*k)/2,s[0]*k,128*k]);x+=(inks[i][2]+14)*k;});
  }
  static hit_area(parent:Node,name:string,r:Rect,action:()=>void):Node{
    const n=this.group(parent,name,r);actions.set(n,action);let start=new Vec2(),drag=0;
    n.on(Node.EventType.TOUCH_START,(e:EventTouch)=>{start=e.getUILocation();drag=0;if(!disabled.has(n))this.opacity(n,.84);});
    n.on(Node.EventType.TOUCH_MOVE,(e:EventTouch)=>{drag=Vec2.distance(start,e.getUILocation());});
    n.on(Node.EventType.TOUCH_END,(e:EventTouch)=>{this.opacity(n,disabled.has(n)?.45:1);if(!disabled.has(n)&&drag<18){this.onTap();action();}});
    n.on(Node.EventType.TOUCH_CANCEL,()=>this.opacity(n,disabled.has(n)?.45:1));return n;
  }
  static button(parent:Node,name:string,asset:string,r:Rect,action:()=>void):Node{const n=this.hit_area(parent,name,r,action);this.image(n,asset,[0,0,r[2],r[3]]);return n;}
  static icon(parent:Node,name:string,r:Rect):Node{
    const asset=name==='close'?'icon_aviary_close':name==='back'?'icon_left':Art.has(`icon_control_${name}`)?`icon_control_${name}`:Art.has(`icon_${name}`)?`icon_${name}`:name;
    return this.art(parent,asset,r);
  }
  static disc(parent:Node,name:string,r:Rect,icon:string,action:()=>void,base='btn_wood',ratio=.55):Node{
    const key=Art.has(base)?base:base.includes('green')?'kit/disc_green':'kit/disc_cream';const n=this.button(parent,name,key,r,action);this.icon(n,icon,[r[2]*(1-ratio)/2,r[3]*(1-ratio)/2,r[2]*ratio,r[3]*ratio]);return n;
  }
  static enabled(n:Node,v:boolean):void{if(v)disabled.delete(n);else disabled.add(n);this.opacity(n,v?1:.45);}
  static invoke(n:Node):void{if(!disabled.has(n)){this.onTap();actions.get(n)?.();}}
  static badge(parent:Node,c:Point,count:number,red=false):Node|null{if(count<=0)return null;const n=this.group(parent,'Badge',[c[0]-33,c[1]-33,66,66]);this.image(n,red?'island_badge_red':'island_badge_green',[0,0,66,66]);this.digits(n,Math.min(count,99),[9,11,48,44]);return n;}
  static check_badge(parent:Node,c:Point):Node{return this.image(parent,'island_badge_check',[c[0]-33,c[1]-33,66,66]);}
  static feather_bar(parent:Node,p:Point,balance:number,onPurchase?:()=>void):Node{
    const n=this.group(parent,'Coins',[p[0],p[1],320,320*117/350]);this.image(n,'panel_home_island_coin',[0,0,320,320*117/350]);this.digits(n,Math.max(0,Math.min(balance,99999)),[117.5*320/350,40.5*320/350,135*320/350,38*320/350]);if(onPurchase)this.hit_area(n,'BuyCoins',[0,0,320,112],onPurchase);return n;
  }
  static price_pill(parent:Node,r:Rect,price:number):Node{const s=Art.size('island_price_gold'),k=Math.min(r[2]/s[0],r[3]/s[1]),w=s[0]*k,h=s[1]*k,n=this.group(parent,'Price',[r[0]+(r[2]-w)/2,r[1]+(r[3]-h)/2,w,h]);this.image(n,'island_price_gold',[0,0,w,h]);this.digits(n,price,[w*.38,h*.22,w*.50,h*.56]);return n;}
  static hanging_sign(parent:Node,cx:number,cy:number,w:number):Rect{const s=Art.size('island_sign_blank'),h=w*s[1]/s[0],top=cy-.68*h,left=cx-w/2;this.image(parent,'island_sign_blank',[left,top,w,h]);return [left+w*.27,top+h*.5,w*.46,h*.38];}
  static fill(parent:Node,name:string,r:Rect,hex:string,radius=0):Node{const n=this.group(parent,name,r),g=n.addComponent(Graphics);g.fillColor=new Color().fromHEX(hex);if(radius)g.roundRect(0,-r[3],r[2],r[3],radius);else g.rect(0,-r[3],r[2],r[3]);g.fill();return n;}
  static line(parent:Node,points:Point[],hex:string,width:number):Node{const n=this.group(parent,'Line'),g=n.addComponent(Graphics);g.strokeColor=new Color().fromHEX(hex);g.lineWidth=width;g.moveTo(points[0][0],-points[0][1]);points.slice(1).forEach(p=>g.lineTo(p[0],-p[1]));g.stroke();return n;}
  static scroll(parent:Node,name:string,r:Rect,h:number):{node:Node;content:Node;scroll:ScrollView}{
    const n=this.group(parent,name,r),viewport=this.group(n,'Viewport',[0,0,r[2],r[3]]);viewport.addComponent(Mask).type=Mask.Type.GRAPHICS_RECT;
    const content=this.group(viewport,'Content',[0,0,r[2],h]),scroll=n.addComponent(ScrollView);scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.inertia=true;scroll.elastic=true;return {node:n,content,scroll};
  }
  static modal(parent:Node,name:string,r:Rect,title?:string,onClose?:()=>void):{node:Node;content:Node;close:()=>void}{
    const n=this.group(parent,name),dim=this.fill(n,'Dim',[0,0,1080,1920],'#000000B8');dim.addComponent(BlockInputEvents);const content=this.group(n,'Content');this.nine(content,'kit/panel',r,[60,60,60,60]);if(title)this.word(content,title,[r[0]+100,r[1]+30,r[2]-200,100],'gold');const close=()=>{n.destroy();onClose?.();};this.disc(content,'Close',[r[0]+r[2]-100,r[1]-35,112,112],'close',close);return {node:n,content,close};
  }
  static effect(n:Node,name:string,properties:Record<string,any>={}):void{
    resources.load('effects/'+name,EffectAsset,(error,effect)=>{if(error)throw error;if(!n.isValid)return;const material=new Material();material.initialize({effectAsset:effect,defines:{USE_TEXTURE:true}});for(const [key,value] of Object.entries(properties))material.setProperty(key,value);n.getComponent(Sprite)!.customMaterial=material;});
  }
  static warm(n:Node):void{this.effect(n,'warm_ink');}
  static outline(n:Node):void{const r=this.rect(n);this.effect(n,'home_icon_outline',{drawSize:new Vec4(r[2],r[3],0,0)});this.place(n,[r[0]-12,r[1]-12,r[2]+24,r[3]+24]);}
  static calm_backdrop(n:Node):void {
    resources.load('effects/calm',EffectAsset,(error,effect)=>{if(error)throw error;if(!n.isValid)return;
      const material=new Material();material.initialize({effectAsset:effect,defines:{USE_TEXTURE:true}});
      const size=Art.size(n.name);material.setProperty('calmParams',new Vec4(1/size[0],1/size[1],3,.38));n.getComponent(Sprite)!.customMaterial=material;
    });
  }
}
