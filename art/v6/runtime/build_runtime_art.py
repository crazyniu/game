"""T44 deterministic production derivation and static QA.

Authorized by coordinator msg_ae57f91a98df and msg_3dc211a89a75.
Only existing image-generated raster artwork is cropped, masked, resized,
9-sliced or composited. No replacement illustration is drawn in code.
"""
from pathlib import Path
import hashlib
import json
import math
import numpy as np
from PIL import Image, ImageOps, ImageDraw, ImageFont, ImageFilter

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'art/v6/runtime'
QA = OUT / 'qa'
DEST = ROOT / 'PerchSort/Assets/Resources/V6/Art'
PARTS = ROOT / 'art/v6/derived'
V5 = ROOT / 'PerchSort/Assets/Resources/V5/Art'
R = Image.Resampling.LANCZOS
MANIFEST = {}
SOURCES = {}
CREAM = '#fff8e8'
BROWN = '#593b27'
FONT = ROOT / 'PerchSort/Assets/Fonts/Fredoka-Medium.ttf'


def read(path):
    path = Path(path)
    SOURCES[str(path.relative_to(ROOT))] = hashlib.sha256(path.read_bytes()).hexdigest()
    return Image.open(path).convert('RGBA')


def part(name):
    return read(PARTS / f'{name}.png')


def trim(im):
    return im.crop(im.getchannel('A').getbbox())


def fit(im, size, inset=0):
    result = Image.new('RGBA', size)
    obj = ImageOps.contain(im, (size[0] - 2 * inset, size[1] - 2 * inset), R)
    result.alpha_composite(obj, ((size[0] - obj.width) // 2, (size[1] - obj.height) // 2))
    return result


def stamp(canvas, im, center, size):
    obj = ImageOps.contain(im, size, R)
    canvas.alpha_composite(obj, (round(center[0] - obj.width / 2), round(center[1] - obj.height / 2)))


def nine(im, size, borders):
    left, bottom, right, top = borders
    w, h = im.size
    dw, dh = size
    assert dw > left + right and dh > top + bottom
    xs, ys = [0, left, w - right, w], [0, top, h - bottom, h]
    xd, yd = [0, left, dw - right, dw], [0, top, dh - bottom, dh]
    result = Image.new('RGBA', size)
    for row in range(3):
        for col in range(3):
            tile = im.crop((xs[col], ys[row], xs[col+1], ys[row+1]))
            tile = tile.resize((xd[col+1]-xd[col], yd[row+1]-yd[row]), R)
            result.paste(tile, (xd[col], yd[row]))
    return result


def surface(kind, size, corner=34):
    return nine(part(f'surface_{kind}').resize((116,116), R), size, [corner]*4)


def save(name, im, provenance, border=None, pivot=(.5,.5), notes=None):
    im = im.convert('RGBA')
    # Remove invisible RGB garbage without altering visible antialiasing.
    data = np.array(im)
    data[data[:,:,3] == 0, :3] = 0
    im = Image.fromarray(data)
    im.save(DEST / f'{name}.png')
    a = data[:,:,3]
    item = {'file':f'{name}.png', 'size':list(im.size), 'mode':'RGBA',
            'alpha_min':int(a.min()), 'alpha_max':int(a.max()),
            'transparent_pixels':int((a==0).sum()), 'partial_alpha_pixels':int(((a>0)&(a<255)).sum()),
            'bounds_alpha_gt_0':im.getchannel('A').getbbox(),
            'bounds_alpha_gt_16':Image.fromarray((a>16).astype('uint8')*255).getbbox(),
            'pivot_unity_normalized':list(pivot), 'source':provenance,
            'sha256':hashlib.sha256((DEST/f'{name}.png').read_bytes()).hexdigest()}
    if border is not None: item['border_left_bottom_right_top'] = border
    if notes: item['notes'] = notes
    MANIFEST[name] = item
    return im


def shade_pressed(im):
    data=np.array(im).copy()
    data[:,:,:3]=np.round(data[:,:,:3].astype(float)*.9).astype('uint8')
    return Image.fromarray(data)


def runtime(name):
    return Image.open(DEST/f'{name}.png').convert('RGBA')


def bases():
    sq=save('ui_btn_square',surface('cream',(128,128)), 'art/v6/derived/surface_cream.png', [36]*4)
    save('ui_btn_square_pressed',shade_pressed(sq),'ui_btn_square.png; RGB x0.90, same alpha and silhouette',[36]*4)
    green=save('ui_btn_primary_green',surface('green',(200,200)), 'art/v6/derived/surface_green.png',[48]*4)
    save('ui_btn_primary_green_pressed',shade_pressed(green),'ui_btn_primary_green.png; RGB x0.90, same alpha and silhouette',[48]*4)
    save('ui_chip_coin',surface('cream',(190,72),28),'art/v6/derived/surface_cream.png',[28]*4)
    save('ui_badge_level',fit(part('surface_leaf'),(96,96),2),'art/v6/derived/surface_leaf.png')
    save('ui_badge_count',fit(part('badge_red'),(40,40),1),'art/v6/derived/badge_red.png')
    badge=fit(part('surface_badge'),(40,40),1)
    stamp(badge,part('plus'),(20,20),(26,26))
    save('ui_badge_plus',badge,'art/v6/derived/surface_badge.png + plus.png',notes='Cream circle with green plus, matching the approved mock; no stock numeral.')
    tray=surface('cream',(920,150))
    # Isolate only the tied loop below the old house hanger; no moss roof pixels.
    knot=read(V5/'birdhouse.png').crop((134,48,166,75))
    knot=fit(knot,(28,24))
    stamp(tray,knot,(100,18),(28,24))
    stamp(tray,knot,(820,18),(28,24))
    save('ui_tray_plank',tray,'T40 cream surface + V5 birdhouse hanger knot crop',[120,34,120,34],notes='Knots centered at local top-left coordinates (100,18) and (820,18), inside the fixed horizontal edge regions.')
    rope=read(V5/'rope_segment.png').resize((16,80),R)
    # The existing rope is a repeating tile; equalize its outer scanlines only.
    rope_data=np.array(rope)
    for i in range(3):
        blend=np.round((rope_data[i].astype(float)+rope_data[-1-i].astype(float))/2).astype('uint8')
        rope_data[i]=blend; rope_data[-1-i]=blend
    save('ui_rope_short',Image.fromarray(rope_data),'V5 rope_segment.png, resized with symmetric three-row seam blend',pivot=(.5,1),notes='Vertical repeat; first and last RGBA rows are byte-identical. Do not stretch in width.')
    save('ui_panel_cream',surface('cream',(820,760)),'art/v6/derived/surface_cream.png',[80]*4)
    save('ui_ribbon_green',fit(part('surface_ribbon'),(860,150),2),'art/v6/derived/surface_ribbon.png',notes='Aspect-fit, not stretched; wide transparent canvas preserves approved ribbon geometry.')
    save('ui_card_cream',surface('cream',(760,620)),'art/v6/derived/surface_cream.png',[80]*4)
    save('ui_pill_small',surface('cream',(220,64),24),'art/v6/derived/surface_cream.png',[24]*4)
    close=part('surface_cream').resize((72,72),R)
    stamp(close,part('close'),(36,36),(36,36))
    save('ui_close_x',close,'T40 cream surface + close.png',notes='Complete close button including its X; do not place another square behind it.')


ICON_MAP = {'ico_undo':'undo','ico_hint':'hint','ico_add_branch':'add_branch',
 'ico_peek_eggs':'peek','ico_wake_bell':'wake','ico_scarecrow':'scarecrow',
 'ico_rain_sprout':'rain','ico_settings':'gear','ico_shop':'shop','ico_aviary':'aviary',
 'ico_home':'home','ico_replay':'replay','ico_play':'play','ico_next':'forward',
 'ico_sound_on':'sound','ico_check':'check','ico_lock':'lock','ico_coin':'coin'}


def icons():
    for name, source in ICON_MAP.items():
        big=save(name,fit(part(source),(128,128),2),f'art/v6/derived/{source}.png',notes='ico_coin is an additional source-only companion needed by the asset-only K3 composite.' if name=='ico_coin' else None)
        save(name+'_64',big.resize((64,64),R),f'{name}.png, Lanczos downsample')


def house():
    source=part('house_sunny')
    data=np.array(source)
    y,x=np.mgrid[0:source.height,0:source.width]
    # Separate only the existing golden roof fill. Keep the original brown
    # contour in the body layer so Unity tint does not recolor that contour.
    rgb16=data[:,:,:3].astype('int16')
    roof_fill=(y<174)&(y<=abs(x-145)+54)&(rgb16[:,:,0]>150)&(rgb16[:,:,1]>95)&(rgb16[:,:,2]<145)&((rgb16[:,:,0]-rgb16[:,:,2])>75)
    mask=Image.fromarray(roof_fill.astype('uint8')*255)
    roof_alpha=np.minimum(data[:,:,3],np.array(mask))
    roof=data.copy(); roof[:,:,3]=roof_alpha
    # A neutral roof permits species multiply-tints without yellow contamination.
    # The generated shading is preserved; the body retains the brown outlines.
    lum=np.max(roof[:,:,:3],axis=2)
    roof[:,:,:3]=lum[:,:,None]
    body=data.copy();body[:,:,3]=np.minimum(body[:,:,3],255-np.array(mask))
    # Remove the existing dark arched door using its connected component.
    rgb=data[:,:,:3].astype(float)
    dark=(rgb[:,:,0]<140)&(rgb[:,:,1]<105)&(rgb[:,:,2]<80)&(x>40)&(x<250)&(y>140)&(y<292)
    todo=[(210,145)]; seen=np.zeros(dark.shape,dtype=bool)
    while todo:
        yy,xx=todo.pop()
        if yy<0 or xx<0 or yy>=dark.shape[0] or xx>=dark.shape[1] or seen[yy,xx] or not dark[yy,xx]:continue
        seen[yy,xx]=True
        todo.extend([(yy-1,xx),(yy+1,xx),(yy,xx-1),(yy,xx+1)])
    body[seen,3]=0
    # Shared scaling and alignment, so the two sprites register exactly.
    size=(88,90); scaled=(82,90); offset=(3,0)
    roof_out=Image.new('RGBA',size);roof_out.alpha_composite(Image.fromarray(roof).resize(scaled,R),offset)
    body_out=Image.new('RGBA',size);body_out.alpha_composite(Image.fromarray(body).resize(scaled,R),offset)
    save('goal_house_roof',roof_out,'art/v6/derived/house_sunny.png, golden fill mask and neutral maximum-channel tint base',notes='Aligned 88x90 canvas. Unity multiply-tint this sprite. The brown roof contour stays in goal_house_body so it retains its original color. Render roof then body.')
    save('goal_house_body',body_out,'art/v6/derived/house_sunny.png, roof fill removed and connected dark doorway cut out',notes='Aligned 88x90 canvas, including the original brown roof outline. Clear door opening bounds top-left: (23,43)-(65,82), center (44,62.5). Put the species portrait behind the body, above the tray; use an arch portrait mask to prevent leaks.')
    save('goal_dot_empty',fit(part('dot_empty'),(14,14)), 'art/v6/derived/dot_empty.png')
    save('goal_dot_full',fit(part('dot_full'),(14,14)), 'art/v6/derived/dot_full.png')
    save('goal_check',fit(part('check'),(28,28),1), 'art/v6/derived/check.png')


GIFT_MAP={'gift_wake':'wake_song','gift_breeze':'breeze','gift_stare':'stare',
 'gift_gardener':'gardener','gift_light_step':'light_step','gift_warm':'warm_glow',
 'gift_night':'night_call','gift_moonbeam':'moonbeam'}


def gifts():
    for name,src in GIFT_MAP.items():
        badge=fit(part('surface_badge'),(44,44))
        stamp(badge,part(src),(22,22),(32,32))
        save(name,badge,f'art/v6/derived/surface_badge.png + {src}.png')
    # Keep the original footprint and feather; remove the old +1 rectangle.
    encore=part('no_count');a=np.array(encore)
    a[160:,159:,3]=0
    encore=Image.fromarray(a)
    # The counter is an existing empty circular badge crossed by the existing X.
    counter=fit(part('surface_badge'),(67,67))
    stamp(counter,part('close'),(33.5,33.5),(43,43))
    encore.alpha_composite(counter,(181,180))
    badge=fit(part('surface_badge'),(44,44))
    stamp(badge,encore,(22,22),(32,32))
    save('gift_encore',badge,'T40 no_count.png with +1 masked; surface_badge.png and close.png counter composite',notes='Footprint + feather + crossed empty counter. No +1, letters or numerals; the tiny crossed counter is secondary detail at 44 px.')


def map_assets():
    for name,src,size in [('star_full','star',96),('star_empty','star_empty',96),('star_small_full','star',32),('star_small_empty','star_empty',32)]:
        save(name,fit(part(src),(size,size),1),f'art/v6/derived/{src}.png')
    for name,src in [('node_cleared','disc_green'),('node_current','disc_cream'),('node_open','disc_cream'),('node_locked','disc_gray')]:
        save(name,fit(part(src),(120,120),2),f'art/v6/derived/{src}.png',notes='Node base only. Code composes stars, bird, lock and number separately. Current and open bases are intentionally identical, as in T40.')
    save('node_bounce_shadow',fit(part('bounce_shadow'),(120,32)),'art/v6/derived/bounce_shadow.png',notes='Static shadow sprite only; no animation claim.')
    for name,src in [('gate_locked','gate_closed'),('gate_open','gate_open')]:
        save(name,fit(part(src),(720,300),2),f'art/v6/derived/{src}.png',pivot=(.5,0),notes='Aspect-fit gate, preserving T40 proportions; 720x300 canvas has transparent side margins. Actual art bounds are in this manifest; do not stretch horizontally.')
    # Extract clean sand from the approved generated map, not a screen screenshot.
    terrain=read(ROOT/'art/v6/raw/map_background.png')
    tile=terrain.crop((520,1510,610,1600)).resize((360,360),R)
    pixels=np.array(tile).astype(float)
    # Crossfade opposite rows into a symmetric overlap, then mask only side edges.
    for i in range(32):
        t=(32-i)/32
        avg=(pixels[i]+pixels[-1-i])/2
        pixels[i]=pixels[i]*(1-t)+avg*t
        pixels[-1-i]=pixels[-1-i]*(1-t)+avg*t
    xx=np.arange(360)
    alpha=np.minimum(np.clip(xx/24,0,1),np.clip((359-xx)/24,0,1))
    pixels[:,:,3]=255*alpha[None,:]
    save('map_path_segment',Image.fromarray(np.round(pixels).astype('uint8')),'art/v6/raw/map_background.png, clean sand crop (520,1510,610,1600), resize, 32-row seam blend, side alpha mask',pivot=(.5,.5),notes='360x360 vertical repeating sand strip, first/last rows identical. This is a straight texture segment; the scrolling map must position/curve the path. No baked trees, nodes, labels or background.')
    # Match the card's original bird/egg imagery for these two mechanics.
    egg=read(V5/'egg_idle.png')
    save('mechanic_egg',fit(trim(egg),(56,56),1),'V5 egg_idle.png, matching the egg in T40 level_card',notes='Question mark is an intentional mechanic pictogram, not a UI label.')
    save('mechanic_sleep',fit(trim(read(V5/'bird_sky_sleep.png')),(56,56),1),'V5 bird_sky_sleep.png, matching T40 level_card; no zzz text')
    save('mechanic_crow',fit(trim(read(V5/'bird_crow_idle.png')),(56,56),1),'V5 bird_crow_idle.png')
    save('mechanic_swivel',fit(trim(read(V5/'swivel_knot.png')),(56,56),1),'V5 swivel_knot.png')
    save('mechanic_growing',fit(part('add_branch'),(56,56),1),'T40 add_branch.png: growing branch plus pictogram',notes='Uses the established branch-plus silhouette; not a newly generated dedicated growing-state illustration.')
    save('mechanic_night',fit(trim(read(V5/'dial_moon.png')),(56,56),1),'V5 dial_moon.png')


def shadow_stamp(canvas,im,xy):
    shadow=Image.new('RGBA',(im.width+12,im.height+12),'#59432e00')
    a=Image.new('L',shadow.size)
    a.paste(im.getchannel('A').point(lambda z:round(z*.18)),(6,6))
    shadow.putalpha(a.filter(ImageFilter.GaussianBlur(3)))
    canvas.alpha_composite(shadow,(xy[0]-6,xy[1]))
    canvas.alpha_composite(im,xy)


def level_composite(background_name='bg_forest_day.png'):
    canvas=read(V5/background_name)
    placements=[]
    configs=[('left',710,['sunny','berry']),('right',770,['berry','sunny']),
             ('left',1180,['berry','sunny']),('right',1240,['sunny','berry'])]
    for side,foot_y,birds in configs:
        branch=read(V5/'branch_left.png').resize((510,96),R)
        if side=='right':branch=ImageOps.mirror(branch)
        x,y=(0 if side=='left' else 570),foot_y-32
        canvas.alpha_composite(branch,(x,y))
        bush=ImageOps.contain(read(V5/'bush_edge.png'),(156,130),R)
        if side=='right':bush=ImageOps.mirror(bush)
        canvas.alpha_composite(bush,(0 if side=='left' else 1080-bush.width,foot_y-65))
        alpha=np.array(branch.getchannel('A'))
        for cx,species in zip(([190,350] if side=='left' else [730,890]),birds):
            bird=read(V5/f'bird_{species}_idle.png').resize((136,136),R)
            wood_top=int(np.where(alpha[:,cx-x]>128)[0][0])
            by=y+wood_top-128
            canvas.alpha_composite(bird,(cx-68,by))
            placements.append({'species':species,'rect':[cx-68,by,136,136]})
    rope=runtime('ui_rope_short').resize((12,80),R)
    for x in [180,900]:
        layer=Image.new('RGBA',(12,118))
        layer.alpha_composite(rope,(0,0));layer.alpha_composite(rope,(0,80))
        canvas.alpha_composite(layer,(x-6,75))
    shadow_stamp(canvas,runtime('ui_chip_coin'),(40,64))
    stamp(canvas,runtime('ico_coin_64'),(76,100),(48,48))
    stamp(canvas,runtime('ui_badge_level'),(540,100),(72,72))
    shadow_stamp(canvas,runtime('ui_btn_square').resize((80,80),R),(960,60))
    stamp(canvas,runtime('ico_settings_64'),(1000,100),(40,40))
    shadow_stamp(canvas,runtime('ui_tray_plank'),(80,175))
    house_colors={'sunny':(255,203,70),'berry':(244,116,157),'sky':(117,184,233)}
    gift_names={'sunny':'gift_wake','berry':'gift_encore','sky':'gift_breeze'}
    for cx,species in zip([280,540,800],['sunny','berry','sky']):
        house_xy=(cx-44,198)
        face=ImageOps.fit(read(V5/f'bird_{species}_idle.png').crop((62,45,196,170)),(40,38),R)
        mask=Image.new('L',face.size)
        ImageDraw.Draw(mask).rounded_rectangle((0,0,39,44),radius=19,fill=255)
        face.putalpha(Image.fromarray(np.minimum(np.array(face.getchannel('A')),np.array(mask))))
        canvas.alpha_composite(face,(cx-20,241))
        roof=np.array(runtime('goal_house_roof')).copy()
        roof[:,:,:3]=np.round(roof[:,:,:3].astype(float)*np.array(house_colors[species])[None,None,:]/255).astype('uint8')
        canvas.alpha_composite(Image.fromarray(roof),house_xy)
        canvas.alpha_composite(runtime('goal_house_body'),house_xy)
        stamp(canvas,runtime(gift_names[species]),(cx+30,212),(44,44))
        for dx in [-27,-9,9,27]:
            stamp(canvas,runtime('goal_dot_full' if species=='sky' else 'goal_dot_empty'),(cx+dx,298),(11,11))
        if species=='sky':stamp(canvas,runtime('goal_check'),(cx+38,275),(20,20))
    for i,name in enumerate(['ico_undo','ico_hint','ico_add_branch']):
        cx=225+210*i
        shadow_stamp(canvas,runtime('ui_btn_square').resize((116,116),R),(cx-58,1687))
        stamp(canvas,runtime(name+'_64'),(cx,1745),(64,64))
        if i==1:stamp(canvas,runtime('ui_badge_count'),(cx+56,1689),(40,40))
        if i==2:stamp(canvas,runtime('ui_badge_plus'),(cx+56,1689),(40,40))
    return canvas,placements


def label(im,text,xy,size=17,fill=BROWN):
    ImageDraw.Draw(im).text(xy,text,font=ImageFont.truetype(str(FONT),size),fill=fill)


def qa():
    names=list(MANIFEST)
    cols,cellw,cellh=6,220,176
    rows=math.ceil(len(names)/cols)
    sky=read(V5/'bg_forest_day.png').crop((300,430,780,1000)).resize((cols*cellw,rows*cellh),R)
    for kind in ['cream','sky']:
        sheet=Image.new('RGBA',(cols*cellw,rows*cellh),CREAM) if kind=='cream' else sky.copy()
        for i,name in enumerate(names):
            x=(i%cols)*cellw;y=(i//cols)*cellh
            obj=runtime(name)
            stamp(sheet,obj,(x+110,y+73),(190,124))
            label(sheet,name,(x+8,y+140),14)
            label(sheet,f'{obj.width} x {obj.height}',(x+8,y+158),11)
        sheet.convert('RGB').save(QA/f'contact_{kind}.png')
    icon_names=[k for k in MANIFEST if (k.startswith('ico_') and k.endswith('_64')) or k.startswith('mechanic_') or k.startswith('gift_')]
    sheet=Image.new('RGBA',(1000,math.ceil(len(icon_names)/5)*130),CREAM)
    for i,name in enumerate(icon_names):
        x=(i%5)*200;y=(i//5)*130
        im=runtime(name)
        gray=ImageOps.grayscale(im).convert('RGBA');gray.putalpha(im.getchannel('A'))
        # Native 64 / 56 / 44 pixel presentation, without enlargement.
        stamp(sheet,gray,(x+100,y+46),im.size)
        label(sheet,name,(x+8,y+88),13)
    sheet.convert('RGB').save(QA/'icons_grayscale_native.png')
    assembled,placements=level_composite()
    assembled.save(QA/'level_k3_assembled.png')
    matched,_=level_composite('bg_w1_morning.png')
    matched.save(QA/'level_k3_same_background.png')
    mock=read(ROOT/'art/v6/mock/level_k3_a.png')
    compare=Image.new('RGB',(1080*3,1980),'#fff8e8')
    for i,(name,im) in enumerate([('T40 approved mock',mock),('T44 required bg_forest_day',assembled),('T44 same background for comparison',matched)]):
        compare.paste(im.convert('RGB'),(1080*i,60))
        label(compare,name,(1080*i+25,15),30)
    compare.save(QA/'level_k3_comparison.png')
    compare.resize((1620,990),R).save(QA/'level_k3_comparison_preview.png')
    (QA/'assembly_placements.json').write_text(json.dumps({'bird_rects':placements,'branches_tip_gap':60,'goal_centers':[280,540,800], 'goal_pitch':260,'dynamic_numerals':'omitted: asset-only static composite, no baked digits'},indent=2)+'\n')
    # Stretch samples use actual declared sprite borders, not arbitrary rescales.
    samples=[('ui_btn_square',(240,128)),('ui_btn_square_pressed',(240,128)),('ui_btn_primary_green',(360,116)),
             ('ui_chip_coin',(320,72)),('ui_tray_plank',(1200,150)),('ui_panel_cream',(1050,480)),
             ('ui_card_cream',(900,360)),('ui_pill_small',(400,64))]
    height=sum(size[1]+55 for _,size in samples)
    sheet=Image.new('RGBA',(1260,height),CREAM)
    y=0
    for name,size in samples:
        label(sheet,name+' '+str(size),(20,y+5),18)
        stretched=nine(runtime(name),size,MANIFEST[name]['border_left_bottom_right_top'])
        sheet.alpha_composite(stretched,(20,y+34));y+=size[1]+55
    sheet.convert('RGB').save(QA/'nine_slice_stretch.png')
    sheet=Image.new('RGBA',(860,1100),'#a1c6ee')
    rope=runtime('ui_rope_short');path=runtime('map_path_segment')
    for i in range(3):sheet.alpha_composite(path,(60,10+360*i))
    for i in range(12):sheet.alpha_composite(rope,(500,10+80*i))
    label(sheet,'Path: 3 vertical repeats',(60,5),20);label(sheet,'Rope: 12 repeats',(535,20),20)
    sheet.convert('RGB').save(QA/'vertical_repeat.png')


def validate():
    problems=[]
    expected={
      'ui_btn_square':(128,128),'ui_btn_square_pressed':(128,128),
      'ui_btn_primary_green':(200,200),'ui_btn_primary_green_pressed':(200,200),
      'ui_chip_coin':(190,72),'ui_badge_level':(96,96),'ui_badge_count':(40,40),
      'ui_badge_plus':(40,40),'ui_tray_plank':(920,150),'ui_rope_short':(16,80),
      'ui_panel_cream':(820,760),'ui_ribbon_green':(860,150),'ui_card_cream':(760,620),
      'ui_pill_small':(220,64),'ui_close_x':(72,72),
      'goal_house_roof':(88,90),'goal_house_body':(88,90),
      'goal_dot_empty':(14,14),'goal_dot_full':(14,14),'goal_check':(28,28),
      'star_full':(96,96),'star_empty':(96,96),'star_small_full':(32,32),'star_small_empty':(32,32),
      'node_cleared':(120,120),'node_current':(120,120),'node_open':(120,120),'node_locked':(120,120),
      'node_bounce_shadow':(120,32),'map_path_segment':(360,360),
      'gate_locked':(720,300),'gate_open':(720,300),'ico_flag':(28,28),'ico_footsteps':(28,28)}
    for name in list(ICON_MAP)+['ico_sound_off']:
        if name=='ico_coin':continue
        expected[name]=(128,128);expected[name+'_64']=(64,64)
    for name in list(GIFT_MAP)+['gift_encore']:expected[name]=(44,44)
    for name in ['egg','sleep','crow','swivel','growing','night']:expected['mechanic_'+name]=(56,56)
    missing=[]
    for name,size in expected.items():
        file=DEST/f'{name}.png'
        if not file.exists():missing.append(file.name);continue
        im=Image.open(file)
        if im.size!=size or im.mode!='RGBA':problems.append(name+': incorrect size/mode')
    for name,item in MANIFEST.items():
        if item['alpha_min']!=0 or item['alpha_max']==0:problems.append(name+': alpha coverage invalid')
        border=item.get('border_left_bottom_right_top')
        if border and (border[0]+border[2]>=item['size'][0] or border[1]+border[3]>=item['size'][1]):problems.append(name+': slice border invalid')
    seams={}
    for name in ['ui_rope_short','map_path_segment']:
        a=np.array(runtime(name));seams[name]=bool(np.array_equal(a[0],a[-1]))
        if not seams[name]:problems.append(name+': repeat seam differs')
    original_change=[p for p,sha in SOURCES.items() if hashlib.sha256((ROOT/p).read_bytes()).hexdigest()!=sha]
    if original_change:problems.append('Read-only sources changed: '+str(original_change))
    result={'delivered_png_count':len(MANIFEST),'required_png_count':len(expected),'optional_png_count':2,
            'missing_required':missing,'all_required_delivered':not missing,'asset_checks_pass':not problems,'problems':problems,
            'repeat_edge_rgba_equal':seams,'unity_import':'NOT_RUN','runtime_animation':'NOT_RUN',
            'runtime_screenshots':'NOT_RUN','built_in_imagegen':'6 calls failed with connection failed: error sending request; no output',
            'api_fallback':'Not authorized; not attempted'}
    (OUT/'required_assets.json').write_text(json.dumps({k:list(v) for k,v in expected.items()},indent=2)+'\n')
    (QA/'validation.json').write_text(json.dumps(result,indent=2)+'\n')
    return result


if __name__=='__main__':
    DEST.mkdir(parents=True,exist_ok=True);QA.mkdir(parents=True,exist_ok=True)
    bases();icons();house();gifts();map_assets();qa()
    (OUT/'asset_manifest.json').write_text(json.dumps(MANIFEST,indent=2)+'\n')
    (QA/'source_hashes.json').write_text(json.dumps(SOURCES,indent=2)+'\n')
    print(json.dumps(validate(),indent=2))
