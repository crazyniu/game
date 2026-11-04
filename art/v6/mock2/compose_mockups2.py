"""T45 static concepts. Pillow composition is explicitly authorized in the task.

Original art is read only. New illustrations are image-generated. This script
performs slicing, aspect-fit, masking, UI-state tinting, nine-slicing and type.
It does not create game artwork procedurally or implement runtime behavior.
"""
from pathlib import Path
import hashlib
import json
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps, ImageFilter

ROOT = Path(__file__).resolve().parents[3]
OUT = Path(__file__).resolve().parent
V5 = ROOT / 'PerchSort/Assets/Resources/V5/Art'
V6 = ROOT / 'PerchSort/Assets/Resources/V6/Art'
FONT = ROOT / 'PerchSort/Assets/Fonts/Fredoka-Medium.ttf'
RESAMPLE = Image.Resampling.LANCZOS
BROWN = '#593B27'
RECORD = {}
SOURCES = {}
CURRENT = None


def read(path):
    path = Path(path)
    SOURCES[str(path.relative_to(ROOT))] = hashlib.sha256(path.read_bytes()).hexdigest()
    return Image.open(path).convert('RGBA')


def record(kind, **fields):
    if CURRENT:
        RECORD[CURRENT].append({'kind': kind, **fields})


def trim(im, threshold=16):
    a = np.asarray(im.getchannel('A')).copy()
    a[a < threshold] = 0
    im = im.copy()
    im.putalpha(Image.fromarray(a))
    bbox = im.getbbox()
    if not bbox:
        raise ValueError('Empty sprite')
    return im.crop(bbox)


def stamp(canvas, im, center, size, name='', opacity=1):
    if isinstance(size, int):
        size = (size, size)
    fitted = ImageOps.contain(im, tuple(size), RESAMPLE)
    if opacity != 1:
        fitted.putalpha(fitted.getchannel('A').point(lambda a: round(a * opacity)))
    x, y = round(center[0] - fitted.width / 2), round(center[1] - fitted.height / 2)
    canvas.alpha_composite(fitted, (x, y))
    record('sprite', name=name, frame=[center[0]-size[0]/2, center[1]-size[1]/2, *size],
           actual=[x,y,fitted.width,fitted.height], opacity=opacity)


def label(canvas, text, center, size=38, bold=False, fill=BROWN):
    font = ImageFont.truetype(str(FONT), size)
    draw = ImageDraw.Draw(canvas)
    box = draw.textbbox(center, str(text), font=font, anchor='mm', stroke_width=int(bold))
    draw.text(center, str(text), font=font, fill=fill, anchor='mm',
              stroke_width=int(bold), stroke_fill=fill)
    record('text', text=str(text), center=list(center), size=size, bold=bold, fill=fill, ink_bbox=box)


def nine_slice(base, size, c=34):
    base = base.resize((116,116), RESAMPLE)
    w,h = size
    c = min(c,w//2-1,h//2-1)
    cuts=[0,c,116-c,116]
    xd,yd=[0,c,w-c,w],[0,c,h-c,h]
    result=Image.new('RGBA', size)
    for j in range(3):
        for i in range(3):
            piece=base.crop((cuts[i],cuts[j],cuts[i+1],cuts[j+1]))
            piece=piece.resize((xd[i+1]-xd[i],yd[j+1]-yd[j]),RESAMPLE)
            result.alpha_composite(piece,(xd[i],yd[j]))
    return result


def panel(canvas, rect, green=False, opacity=1):
    x,y,w,h=rect
    base=UI['green' if green else 'cream']
    sprite=nine_slice(base,(w,h))
    tile=Image.new('RGBA',(w+12,h+18))
    shadow=Image.new('RGBA',tile.size,'#59432E00')
    alpha=Image.new('L',tile.size)
    alpha.paste(sprite.getchannel('A').point(lambda a: round(a*.18)),(6,12))
    shadow.putalpha(alpha.filter(ImageFilter.GaussianBlur(3)))
    tile.alpha_composite(shadow)
    tile.alpha_composite(sprite,(6,6))
    if opacity != 1:
        tile.putalpha(tile.getchannel('A').point(lambda a: round(a*opacity)))
    canvas.alpha_composite(tile,(x-6,y-6))
    record('panel',rect=list(rect),green=green,opacity=opacity,corner_region=34)


def icon(canvas, key, center, size=64, opacity=1):
    stamp(canvas,ICONS[key],center,size,key,opacity)


def button(canvas,key,center,size=116,green=False,opacity=1,width=None):
    w=width or size
    if key=='close':
        stamp(canvas,ICONS['close'],center,size,'complete_close_control',opacity)
        record('button',key=key,rect=[center[0]-size/2,center[1]-size/2,size,size],
               icon_frame='baked_into_control',green=False,opacity=opacity)
        return
    tile=Image.new('RGBA',(w+20,size+20))
    tile.alpha_composite(nine_slice(UI['green' if green else 'cream'],(w,size)),(10,10))
    sp=ImageOps.contain(ICONS[key],(64,64) if size>=100 else (40,40),RESAMPLE)
    tile.alpha_composite(sp,(round((w+20-sp.width)/2),round((size+20-sp.height)/2)))
    if opacity != 1:
        tile.putalpha(tile.getchannel('A').point(lambda a: round(a*opacity)))
    canvas.alpha_composite(tile,(round(center[0]-w/2-10),round(center[1]-size/2-10)))
    record('button',key=key,rect=[center[0]-w/2,center[1]-size/2,w,size],icon_frame=64 if size>=100 else 40,
           green=green,opacity=opacity)


def chip(canvas,center,value,width=190,kind='coin',height=72,fontsize=38):
    x,y=round(center[0]-width/2),round(center[1]-height/2)
    panel(canvas,(x,y,width,height))
    icon(canvas,kind,(x+38,center[1]),48)
    label(canvas,value,(x+width/2+24,center[1]),fontsize,True)


def badge(canvas,center,value,size=48):
    stamp(canvas,UI['count'],center,size,'count_badge')
    label(canvas,value,center,30,True)


def price(canvas,center,value,width=172,height=84,enabled=True):
    tile=Image.new('RGBA',(width+16,height+16))
    tile.alpha_composite(nine_slice(UI['green'],(width,height)),(8,8))
    compact=width<160
    coin_size=28 if compact else 38
    font_size=32 if compact else 36
    sp=ImageOps.contain(ICONS['coin'],(coin_size,coin_size),RESAMPLE)
    tile.alpha_composite(sp,(18 if compact else 22,round((height+16-sp.height)/2)))
    draw=ImageDraw.Draw(tile)
    font=ImageFont.truetype(str(FONT),font_size)
    draw.text((width/2+(20 if compact else 27),height/2+8),str(value),font=font,fill=BROWN,anchor='mm',stroke_width=1,stroke_fill=BROWN)
    if not enabled:
        alpha=tile.getchannel('A')
        tile=ImageOps.grayscale(tile).convert('RGBA')
        tile.putalpha(alpha.point(lambda a:round(a*.45)))
    canvas.alpha_composite(tile,(round(center[0]-width/2-8),round(center[1]-height/2-8)))
    record('price_button',rect=[center[0]-width/2,center[1]-height/2,width,height],price=value,
           coin_frame=coin_size,font_size=font_size,enabled=enabled,opacity=1 if enabled else .45)


def begin(name, blurred=True):
    global CURRENT
    CURRENT=name
    RECORD[name]=[]
    im=ImageOps.fit(read(ROOT/'art/v6/raw/map_background.png'),(1080,1920),RESAMPLE)
    if blurred:
        im=im.filter(ImageFilter.GaussianBlur(12))
    record('background',source='art/v6/raw/map_background.png',blur_radius=12 if blurred else 0)
    return im


def header(canvas,kind='shop',coins=1280):
    if kind=='shop':
        chip(canvas,(135,100),coins)
        button(canvas,'close',(1000,100),80)
        icon(canvas,'shop',(540,100),72)
    else:
        button(canvas,'back',(80,100),80)
        chip(canvas,(945,100),coins)
        icon(canvas,'aviary',(540,100),72)


def tabs(canvas,active):
    panel(canvas,(32,294,1016,1556))
    for i,key in enumerate(['hint','hanger','home']):
        cy=232 if i==active else 250
        button(canvas,key,(330+210*i,cy),116,green=i==active)
    record('tab_state',active=['boosters','outfits','houses'][active],raised_offset=18)


def save(canvas,name):
    canvas.convert('RGB').save(OUT/(name+'.png'))


def gray(im):
    a=im.getchannel('A')
    im=ImageOps.grayscale(im).convert('RGBA')
    im.putalpha(a)
    return im


def bird(canvas,species,center,size,pose='idle',outfit=False,silhouette=False):
    sp=read(V5/f'bird_{species}_{pose}.png')
    if silhouette:
        a=sp.getchannel('A')
        sp=Image.new('RGBA',sp.size,'#483E3C')
        sp.putalpha(a)
    stamp(canvas,sp,center,size,f'bird_{species}_{pose}')
    if outfit:
        stamp(canvas,ACCESS['scarf_blue'],(center[0]+size*.07,center[1]+size*.15),
              (round(size*.35),round(size*.23)),'equipped_scarf')


def portrait_disc(canvas,species,center,size,locked=False,outfit=False):
    stamp(canvas,UI['disc_locked' if locked else 'disc'],center,size,'portrait_round_frame')
    bird(canvas,species,center,round(size*.91),silhouette=locked,outfit=outfit)
    if locked:
        label(canvas,'?',(center[0],center[1]+8),52,True,'#FFF2D7')


def outfit_slots(canvas,species,x,y,scale=1,states=('owned','equipped','buy'),big=False):
    keys=['hat' if species in ('sunny','sky','cocoa') else 'beret','scarf_blue',
          'glasses' if species in ('sky','mint','cocoa') else 'flower']
    cellw=round(114*scale)
    pitch=round(132*scale)
    for i,(key,state) in enumerate(zip(keys,states)):
        cx=x+i*pitch
        panel(canvas,(round(cx-cellw/2),y,cellw,round(110*scale)))
        stamp(canvas,ACCESS[key],(cx,y+round(52*scale)),round(82*scale),f'outfit_{key}')
        if state=='buy':
            price(canvas,(cx,y+round(155*scale)),[300,500,800][i],width=round(114*scale),height=round(66*scale))
        else:
            button(canvas,'check' if state=='equipped' else 'hanger',
                   (cx,y+round(155*scale)),round(66*scale),green=state=='equipped')
        record('outfit_state',species=species,slot=i,key=key,state=state)


def boosters():
    im=begin('shop_boosters');header(im);tabs(im,0)
    keys=['hint','add','peek','bell','scarecrow','rain']
    prices=[30,60,80,80,100,100]
    stock=[3,2,2,1,2,0]
    for i,key in enumerate(keys):
        x=80+480*(i%2);y=360+480*(i//2);cx=x+220
        panel(im,(x,y,440,440))
        if i==5:
            stamp(im,gray(ICONS[key]),(cx,y+140),160,key,.6)
            stamp(im,gray(ICONS['lock']),(cx+74,y+189),56,'locked')
            stamp(im,UI['level'],(cx,y+261),72,'unlock_level_badge')
            label(im,'32',(cx,y+262),34,True)
        else:
            icon(im,key,(cx,y+140),172)
            label(im,'1x',(x+116,y+284),32,True)
            panel(im,(x+270,y+258,100,52))
            label(im,'3x',(x+320,y+284),32,True)
        badge(im,(x+390,y+50),stock[i])
        price(im,(x+116,y+366),prices[i],enabled=i!=5)
        price(im,(x+320,y+366),int(prices[i]*2.5),enabled=i!=5)
        if i==5:
            label(im,'1x',(x+116,y+284),32)
            panel(im,(x+270,y+258,100,52),opacity=.45)
            label(im,'3x',(x+320,y+284),32)
        record('booster_state',booster=key,owned=stock[i],locked=i==5,unlock_level=32 if i==5 else None,
               single=prices[i],bundle=int(prices[i]*2.5))
    save(im,'shop_boosters');return im


def outfits():
    im=begin('shop_outfits');header(im);tabs(im,1)
    for i,sp in enumerate(['sunny','sky','berry','mint','cocoa','snow']):
        x=80+480*(i%2);y=360+480*(i//2)
        panel(im,(x,y,440,440))
        portrait_disc(im,sp,(x+220,y+116),174,outfit=True)
        outfit_slots(im,sp,x+88,y+226)
    save(im,'shop_outfits')


def houses():
    im=begin('shop_houses');header(im);tabs(im,2)
    for i,name in enumerate(['mushroom','teapot','lantern','acorn']):
        x=80+480*(i%2);y=405+680*(i//2)
        panel(im,(x,y,440,600))
        stamp(im,HOUSES[name],(x+220,y+230),(350,350),'house_'+name)
        if i<2:
            button(im,'check' if i==1 else 'home',(x+220,y+489),116,green=i==1)
        else:
            price(im,(x+220,y+489),600,220,100)
        record('house_state',style=name,state=['owned','equipped','buy','buy'][i],price=600)
    save(im,'shop_houses')


def dim(im):
    rgb=np.array(im.convert('RGB')).astype(np.float64)/255
    linear=np.where(rgb<=.04045,rgb/12.92,((rgb+.055)/1.055)**2.4)*.28
    srgb=np.where(linear<=.0031308,12.92*linear,1.055*linear**(1/2.4)-.055)
    record('overlay',color='#000000',alpha=.72,color_space='linear',blur_radius=0)
    return Image.fromarray(np.clip(np.rint(srgb*255),0,255).astype(np.uint8)).convert('RGBA')


def purchase(shop,poor=False):
    global CURRENT
    name='shop_no_coins' if poor else 'shop_confirm'
    CURRENT=name;RECORD[name]=[]
    backdrop=shop.copy()
    if poor:
        chip(backdrop,(135,100),'20')
    im=dim(backdrop)
    panel(im,(190,550,700,820))
    button(im,'close',(830,610),80)
    icon(im,'hint',(540,814),220)
    panel(im,(490,961,100,54));label(im,'3x',(540,988),34,True)
    chip(im,(540,1080),'75',230)
    if poor:
        red=ICONS['coin'].copy()
        arr=np.asarray(red).copy();arr[:,:,:3]=(arr[:,:,:3]*np.array([1,.48,.42])).astype(np.uint8)
        stamp(im,Image.fromarray(arr),(463,1080),48,'coin_insufficient')
        icon(im,'cross',(486,1100),24)
    button(im,'check',(540,1240),116,green=True,opacity=.45 if poor else 1,width=260)
    record('purchase_state',item='Firefly',quantity=3,price=75,balance=20 if poor else 1280,
           confirm_enabled=not poor)
    save(im,name)


NAMES={'sunny':'Sunny','sky':'Skye','berry':'Berry','mint':'Minty','cocoa':'Cocoa',
       'snow':'Flurry','ember':'Ember','crow':'Rook','ink':'Inky','hoot':'Hoot'}


def aviary():
    im=begin('aviary_list');header(im,'aviary');panel(im,(32,190,1016,1660))
    species=['sunny','sky','berry','mint','cocoa','snow','ember','crow','ink','hoot']
    counts=[64,61,42,29,26,14,8,0,0,0]
    for i,sp in enumerate(species):
        x=80+480*(i%2);y=240+310*(i//2);locked=sp in ('ink','hoot')
        panel(im,(x,y,440,290))
        portrait_disc(im,sp,(x+125,y+117),186,locked,outfit=sp=='berry')
        label(im,'?' if locked else NAMES[sp],(x+302,y+82),38,True)
        if locked:
            stamp(im,gray(ICONS['lock']),(x+300,y+162),48,'unmet_lock')
        else:
            chip(im,(x+300,y+158),counts[i],162,'home',64,34)
        if sp=='crow':
            icon(im,'no_outfit',(x+300,y+243),56)
        elif not locked:
            icon(im,'hanger',(x+300,y+243),44)
        record('species_card',species=sp,state='unmet' if locked else 'met',home_count=counts[i],outfits=sp!='crow')
    panel(im,(1028,895,10,240),opacity=.6)
    save(im,'aviary_list');return im


def detail():
    im=begin('aviary_detail');header(im,'aviary');panel(im,(80,190,920,1660))
    portrait_disc(im,'berry',(540,452),374,outfit=True)
    label(im,'Berry',(540,688),64,True)
    label(im,'A show-off with a perfect wink.',(540,752),32)
    for i,pose in enumerate(['idle','quirk1','quirk2']):
        panel(im,(260+i*200,816,160,170))
        bird(im,'berry',(340+i*200,900),148,pose,outfit=True)
        if i<2:
            icon(im,'next',(440+i*200,900),28)
    panel(im,(174,1050,732,150))
    stamp(im,UI['disc'],(250,1125),100,'gift_badge')
    icon(im,'encore',(250,1125),76)
    label(im,'Encore',(581,1095),40,True)
    label(im,'Next move does not count.',(581,1150),32)
    outfit_slots(im,'berry',276,1290,scale=2,states=('owned','equipped','buy'),big=True)
    chip(im,(333,1734),'42',220,'home')
    stamp(im,UI['level'],(748,1734),72,'first_seen_level')
    label(im,'3',(748,1735),38,True)
    record('detail_state',species='berry',home_count=42,first_seen_level=3,
           personality='A show-off with a perfect wink.',gift='Encore',effect='Next move does not count.',
           preview_frames=['idle','quirk1','quirk2'],animation=False)
    save(im,'aviary_detail')


def popup_base(name,rect=(150,490,780,940)):
    im=begin(name,False)
    im=dim(im);panel(im,rect)
    return im


def sparkles(im,positions):
    for x,y,s in positions:
        stamp(im,SPARKLE,(x,y),s,'sparkle')


def new_friend():
    im=popup_base('popup_new_friend',(150,430,780,1060))
    portrait_disc(im,'berry',(540,732),340)
    sparkles(im,[(280,624,44),(794,647,38),(325,900,28),(770,873,50)])
    label(im,'Berry',(540,966),60,True)
    stamp(im,UI['disc'],(540,1080),100,'gift_badge')
    icon(im,'encore',(540,1080),76)
    chip(im,(540,1207),'+20',230)
    button(im,'check',(540,1372),116,green=True)
    record('popup_state',state='new_species',species='berry',gift='Encore',coin_reward=20)
    save(im,'popup_new_friend')


def daily():
    im=popup_base('popup_daily_gift')
    stamp(im,NEST,(540,845),(470,430),'daily_nest_gift')
    sparkles(im,[(304,743,40),(791,796,38),(720,1090,28)])
    chip(im,(540,1140),'+50',230)
    button(im,'check',(540,1310),116,green=True,width=260)
    record('popup_state',state='daily_nest_gift',coin_reward=50)
    save(im,'popup_daily_gift')


def scene_circle(canvas,center,size):
    source=read(ROOT/'art/v6/raw/map_background.png')
    crop=(376,0,916,540)
    scene=source.crop(crop).resize((size,size),RESAMPLE)
    mask=Image.new('L',(size,size));ImageDraw.Draw(mask).ellipse((0,0,size-1,size-1),fill=255)
    scene.putalpha(mask)
    stamp(canvas,UI['disc'],center,size+20,'world_scene_round_frame')
    stamp(canvas,scene,center,size,'bamboo_world_scene')
    record('scene_crop',source='art/v6/raw/map_background.png',source_box=crop)


def world_complete():
    im=popup_base('popup_world_complete',(130,460,820,1000))
    stamp(im,UI['ribbon'],(540,526),(700,145),'banner_ribbon')
    icon(im,'check',(540,526),64)
    scene_circle(im,(540,848),420)
    stamp(im,UI['gate'],(540,850),(356,352),'opened_world_gate')
    sparkles(im,[(292,775,40),(790,742,46),(763,998,32)])
    chip(im,(540,1160),'+200',260)
    button(im,'next',(540,1340),116,green=True,width=300)
    record('popup_state',state='world_complete',completed_world=1,next_world=2,coin_reward=200)
    save(im,'popup_world_complete')


def unlock():
    im=popup_base('popup_unlock_level',(130,460,820,1000))
    scene_circle(im,(540,845),480)
    leaf=read(V5/'leaf_bamboo.png')
    for angle,dx in [(35,-18),(0,0),(-35,18)]:
        stamp(im,leaf.rotate(angle,expand=True),(540+dx,574),86,'bamboo_world_emblem_leaf')
    stamp(im,UI['gate'],(540,1018),(240,224),'open_gate')
    stamp(im,UI['level'],(540,1182),92,'first_level_badge')
    label(im,'11',(540,1183),44,True)
    button(im,'play',(540,1340),116,green=True,width=300)
    record('popup_state',state='world_unlocked',world='Bamboo Grove',first_level=11,world_name_visible=False)
    save(im,'popup_unlock_level')


def prepare():
    global UI,ICONS,ACCESS,HOUSES,NEST,SPARKLE
    UI={k:read(V6/(v+'.png')) for k,v in {
        'cream':'ui_btn_square','green':'ui_btn_primary_green','count':'ui_badge_count',
        'level':'ui_badge_level','disc':'node_open','disc_locked':'node_locked',
        'ribbon':'ui_ribbon_green','gate':'gate_open'}.items()}
    UI['gate']=trim(UI['gate']);UI['ribbon']=trim(UI['ribbon'])
    ICONS={k:read(V6/(v+'.png')) for k,v in {
        'hint':'ico_hint','add':'ico_add_branch','peek':'ico_peek_eggs','bell':'ico_wake_bell',
        'scarecrow':'ico_scarecrow','rain':'ico_rain_sprout','close':'ui_close_x','coin':'ico_coin',
        'shop':'ico_shop','home':'ico_home','check':'ico_check','next':'ico_next',
        'aviary':'ico_aviary','lock':'ico_lock','play':'ico_play','encore':'gift_encore'}.items()}
    ICONS['back']=ImageOps.mirror(ICONS['next'])
    ICONS['cross']=ICONS['close'].crop((17,17,56,56))
    atlas=read(OUT/'raw/outfits_atlas.png')
    ACCESS={}
    for i,name in enumerate(['hat','scarf_blue','glasses','beret','scarf_coral','flower','hanger','no_outfit','scarf_green']):
        x,y=i%3,i//3
        box=(round(x*atlas.width/3),round(y*atlas.height/3),round((x+1)*atlas.width/3),round((y+1)*atlas.height/3))
        sp=trim(atlas.crop(box))
        sp.save(OUT/'derived'/f'{name}.png');ACCESS[name]=sp
    ICONS['hanger']=ACCESS['hanger'];ICONS['no_outfit']=ACCESS['no_outfit']
    HOUSES={}
    for name in ['mushroom','teapot','lantern','acorn']:
        sp=trim(read(OUT/'raw'/f'house_{name}.png'))
        sp.save(OUT/'derived'/f'house_{name}.png');HOUSES[name]=sp
    NEST=trim(read(OUT/'raw/nest_gift.png'));NEST.save(OUT/'derived/nest_gift.png')
    SPARKLE=read(V5/'sparkle.png')


def qa():
    global CURRENT
    CURRENT=None
    names=['shop_boosters','shop_outfits','shop_houses','shop_confirm','shop_no_coins','aviary_list',
           'aviary_detail','popup_new_friend','popup_daily_gift','popup_world_complete','popup_unlock_level']
    sheet=Image.new('RGB',(4*270,3*520),'#E6D8BD')
    for i,name in enumerate(names):
        thumb=Image.open(OUT/(name+'.png')).resize((270,480),RESAMPLE)
        sheet.paste(thumb,((i%4)*270,(i//4)*520))
        ImageDraw.Draw(sheet).text(((i%4)*270+8,(i//4)*520+486),name,font=ImageFont.truetype(str(FONT),17),fill=BROWN)
    sheet.save(OUT/'qa/contact_sheet.png')
    parts=list(HOUSES.items())+list(ACCESS.items())+[('nest_gift',NEST)]
    recog=Image.new('RGBA',(780,160*len(parts)),'#FDEECE')
    for i,(name,sp) in enumerate(parts):
        y=i*160+80
        label(recog,name,(115,y),22)
        stamp(recog,sp,(300,y),128,name)
        stamp(recog,sp,(450,y),64,name)
        stamp(recog,gray(sp),(590,y),64,name)
        stamp(recog,gray(sp),(710,y),44,name)
    recog.convert('RGB').save(OUT/'qa/recognition_sheet.png')
    metrics={}
    for p in sorted((OUT/'derived').glob('*.png')):
        im=Image.open(p);a=np.array(im.getchannel('A'))
        metrics[p.name]={'size':list(im.size),'mode':im.mode,'alpha_range':[int(a.min()),int(a.max())],
                         'fully_transparent_pixels':int((a==0).sum()),'bounds_alpha16':im.getbbox(),
                         'concept_pivot_normalized':[.5,.5],'production_pivot':'NOT_ACCEPTED'}
    (OUT/'qa/sprite_metrics.json').write_text(json.dumps(metrics,indent=2)+'\n')
    (OUT/'qa/layout_measurements.json').write_text(json.dumps(RECORD,indent=2)+'\n')
    (OUT/'qa/source_hashes.json').write_text(json.dumps(SOURCES,indent=2)+'\n')
    changes=[p for p,h in SOURCES.items() if hashlib.sha256((ROOT/p).read_bytes()).hexdigest()!=h]
    assert not changes, changes
    result={'screens':{},'source_files_unchanged':True,'runtime_tests':'NOT_RUN','animation':'STATIC_FRAMES_ONLY'}
    for name in names:
        p=OUT/(name+'.png');im=Image.open(p)
        assert im.size==(1080,1920)
        result['screens'][name]={'size':list(im.size),'mode':im.mode,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()}
    (OUT/'qa/validation.json').write_text(json.dumps(result,indent=2)+'\n')


if __name__=='__main__':
    prepare()
    shop=boosters();outfits();houses();purchase(shop);purchase(shop,True)
    aviary();detail();new_friend();daily();world_complete();unlock();qa()
    print('Rendered 11 concept PNGs; dimensions, source hashes, and alpha metrics recorded.')
