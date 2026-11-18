# Shared Cocos presentation API

Import `UI, Rect, Point` from './UI', `Art` from './Art', `BirdView` from './BirdView'. Use `Node` from 'cc'. Root coordinate system is top-left, x right/y down in design pixels; helpers convert to Cocos. `Rect` = `[x,y,w,h]`, `Point` = `[x,y]`.

All helpers are synchronous (Art preloads images before screen creation):
- `UI.group(parent, name, rect?: Rect): Node` (default [0,0,1080,1920]); `UI.clear(node)` destroys children.
- `UI.place(node, rect)`, `UI.rect(node): Rect`, `UI.move(node,x,y)`, `UI.opacity(node,0..1)`.
- `UI.image(parent, assetName, rect, fit=false): Node` raw full image stretched or aspect-fit.
- `UI.art(parent, assetName, rect): Node` fit visible alpha ink into rect, preserving aspect.
- `UI.piece(parent,name,[x,y],width)`, `UI.piece_fit(parent,name,center,box)` preserve full image ratio.
- `UI.nine(parent,name,rect,margins=[40,40,40,40])`: sliced Sprite.
- `UI.panel(parent,name,rect)` uses nine-slice.
- `UI.word(parent,id,rect,style='brown',align=0): Node` locale word identifier from art/locale pack; `UI.painted(parent,id,rect,align=0)` painted pack identifier.
- `UI.number(parent,value,rect,style='brown',align=0)`, `UI.digits(parent,value,rect)`, `UI.set_number(node,value,style?)` use painted glyphs.
- `UI.button(parent,name,assetName,rect,action): Node` painted art button; `UI.disc(parent,name,rect,icon,action,base='btn_wood',ratio=.55)` round button; `UI.hit_area(parent,name,rect,action)`.
- `UI.enabled(node,bool)` dims/noninteractive same size; `UI.badge(parent,center,count,red=false)`, `UI.check_badge(parent,center)`.
- `UI.feather_bar(parent,[x,y],balance,onPurchase?): Node`, `UI.price_pill(parent,rect,price)`.
- `UI.hanging_sign(parent,cx,cy,width): Rect` returns painted word box.
- `UI.fill(parent,name,rect,colorHex, radius=0): Node` Graphics; `UI.line(parent,points,colorHex,width)`.
- `UI.scroll(parent,name,rect,contentHeight): {node:Node,content:Node,scroll:ScrollView}`.
- `UI.modal(parent,name,panelRect,titleId?,close?): {node:Node,content:Node,close:()=>void}` dim full canvas, absolute content coords, wood panel, close control; returns removable layer.
- `Art.has(name)`, `Art.size(name): [w,h]`, `Art.frames(prefix): string[]`, `Art.ink(name): Rect`, `Art.locale` ('en'|'zh'), `Art.setLocale(code)`; resource keys are original bare name except locale paths handled by UI.
- `BirdView.create(parent,species,rect, options?:{instant?:boolean,pose?:string,seed?:number}): BirdView`; `.node`, `.setPose(pose)`, `.flyTo(point,duration?)`, `.destroy()`, `.setSleeping(bool)`; continuous drawn idle frames/breathing.

Screen classes accept `host: Node`, `profile: Profile`, callbacks. Expose `node:Node`, `destroy()`, static `preview(...)` and constructor/build; callbacks navigate through root. Send exact screen interfaces to root early.

Root implements App with `profile`, `open_home()`, `open_map(world?)`, `open_board(level)`, `open_shop(tab?)`, `open_aviary()`, `settings()`, `daily_reward()`, `lucky_draw()`, `chest()`, `save()`, `sound.play(name)`, `sound.music(name)`, `sound.setMuted(bool)`. For screens prefer callback object instead of importing App to avoid circular imports. `UI.onTap` callback can be assigned by root for sounds.
