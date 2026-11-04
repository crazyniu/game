"""Prepare generated cutouts, fixed-scale egg frames and isolated static QA."""
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import json,math,hashlib
P=Path(__file__).resolve().parent; ROOT=P.parents[2]; OUT=ROOT/'PerchSort/Assets/Resources/V7/Birds'
QA=P/'qa';PARTS=P/'prepared_parts';PARTS.mkdir(exist_ok=True)
records={r['name']:r for r in json.loads((P/'generation.json').read_text())};prep={}
def cut(name,threshold):
    im=Image.open(P/'raw'/(name+'.png')).convert('RGBA');a=im.getchannel('A')
    a=a.point(lambda v:round(max(0,v-threshold)*255/(255-threshold)))
    im.putalpha(a);box=a.getbbox();im=im.crop(box)
    prep[name]={'source_size':list(Image.open(P/'raw'/(name+'.png')).size),'crop_xyxy':list(box),'alpha_remap_threshold':threshold,'method':'remove generated exterior alpha haze; linear remap remaining alpha; trim and resize only'}
    return im
nest=cut('nest',160).resize((272,110),Image.Resampling.LANCZOS)
back=Image.new('RGBA',(320,320));back.alpha_composite(nest,(24,190));back.save(PARTS/'shared_nest.png')
# Reuse front-rim pixels of the same nest for physical occlusion; no new artwork.
front=back.copy();a=front.getchannel('A');pix=a.load()
for x in range(320):
    for y in range(max(0,round(250-0.0015*(x-160)**2))):pix[x,y]=0
front.putalpha(a);front.save(PARTS/'nest_foreground.png')
placements={}
for name in ['idle','wobble_l','wobble_r','crack','shell']:
    part=cut(name,32)
    if name=='shell':
        # Lower cup keeps the same180px body diameter; detached cap adds width.
        size=(250,163);xy=(60,117)
    else:
        size=(180,240) if name=='crack' else (180,225) if name=='idle' else (186,222)
        shift={'idle':0,'wobble_l':-12,'wobble_r':12,'crack':0}[name]
        xy=(160+shift-size[0]//2,280-size[1])
    part=part.resize(size,Image.Resampling.LANCZOS);frame=back.copy();frame.alpha_composite(part,xy);frame.alpha_composite(front)
    frame.save(OUT/f'egg_{name}.png');placements['egg_'+name]={'egg_part_rect_xywh':[*xy,*size],'support_baseline_y':300,'pivot_normalized_unity':[.5,.0625],'nest_rect_xywh':[24,190,272,110],'physical_body_reference':[180,225],'support_identical':True}
for name,box in [('mama_bow',(112,88)),('chick_fluff',(104,100))]:
    part=cut(name,160);part.thumbnail(box,Image.Resampling.LANCZOS)
    frame=Image.new('RGBA',(128,128));xy=((128-part.width)//2,(128-part.height)//2);frame.alpha_composite(part,xy);frame.save(OUT/(name+'.png'))
    placements[name]={'pivot_normalized_unity':[.5,.5],'source_attachment_xy':[64,64],'source_attachment_kind':'knot center' if name=='mama_bow' else 'tuft center','recommended_display_size_on_320_bird':[72,72] if name=='mama_bow' else [88,88]}
# Recommended artist anchors on the read-only current320px idle snapshots.
anchors={'sunny':{'mama_bow':[104,140],'chick_fluff':[165,244]},'sky':{'mama_bow':[215,123],'chick_fluff':[163,243]},'berry':{'mama_bow':[100,142],'chick_fluff':[161,241]},'mint':{'mama_bow':[110,114],'chick_fluff':[166,243]}}
assets={}
for name,placement in placements.items():
    path=OUT/(name+'.png');im=Image.open(path);a=im.getchannel('A');h=a.histogram();bounds=a.getbbox()
    assets[name]={'file':str(path.relative_to(ROOT)),'size':list(im.size),'mode':im.mode,'alpha_extrema':list(a.getextrema()),'alpha_bounds_xyxy':list(bounds),'solid_bounds_alpha128_xyxy':list(a.point(lambda v:255 if v>=128 else 0).getbbox()),'transparent_pixels':h[0],'partial_alpha_pixels':sum(h[1:255]),'bottom_alpha_exclusive':bounds[3],'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),**placement}
manifest={'status':'prepared_pending_visual_review','coordinate_system':'top-left pixels; Unity pivot normalized from bottom-left','assets':assets,'preparation':prep,'overlay_anchors_on_320_bird':anchors,'reference_sources':json.loads((P/'references/sources.json').read_text()),'scale_note':'Idle body180x225; wobble projected bounds186x222 with center shift12px; crack body width180 with extra15px cap opening. Shell lower cup retains about180px diameter and detached cap adds width. Nest identical in every frame.','nest_foreground_cut':'reuse nest pixels y>=round(250-0.0015*(x-160)^2)','runtime_verified':False}
(P/'manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
# Full-size contact strip; no labels inside production sprites.
eggs=[Image.open(OUT/f'egg_{n}.png') for n in ['idle','wobble_l','wobble_r','crack','shell']]
strip=Image.new('RGB',(1600,680),(245,235,214));draw=ImageDraw.Draw(strip)
for i,(name,im) in enumerate(zip(['idle','wobble_l','wobble_r','crack','shell'],eggs)):
    draw.text((i*320+8,6),name,fill='black');strip.paste(im,(i*320,20),im)
    dark=Image.new('RGBA',(320,320),(34,45,56,255));dark.alpha_composite(im);strip.paste(dark.convert('RGB'),(i*320,360))
strip.save(QA/'egg_contact_strip.png')
small=Image.new('RGB',(1000,380),(223,232,233));d=ImageDraw.Draw(small)
for i,(name,im) in enumerate(zip(['idle','wobble_l','wobble_r','crack','shell'],eggs)):
    d.text((i*200+4,5),name+' 200px',fill='black');tile=im.resize((200,200),Image.Resampling.LANCZOS);small.paste(tile,(i*200,24),tile)
    tiny=im.resize((64,64),Image.Resampling.LANCZOS);small.paste(tiny,(i*200+24,260),tiny);gray=ImageOps.grayscale(tiny).convert('RGBA');gray.putalpha(tiny.getchannel('A'));small.paste(gray,(i*200+106,260),gray)
small.save(QA/'egg_game_size_64_grayscale.png')
# Overlays on full-size current bird references and140px chick display.
sheet=Image.new('RGB',(1280,750),(222,235,238));d=ImageDraw.Draw(sheet)
for i,species in enumerate(anchors):
    bird=Image.open(P/'references'/(species+'_idle.png')).convert('RGBA')
    for row,overlay in enumerate(['mama_bow','chick_fluff']):
        comp=bird.copy();sz=placements[overlay]['recommended_display_size_on_320_bird'];ov=Image.open(OUT/(overlay+'.png')).resize(tuple(sz),Image.Resampling.LANCZOS);cx,cy=anchors[species][overlay];comp.alpha_composite(ov,(cx-sz[0]//2,cy-sz[1]//2));comp.save(QA/(species+'_'+overlay+'.png'))
        tile=comp if row==0 else comp.resize((224,224),Image.Resampling.LANCZOS)
        sheet.paste(tile,(i*320+(320-tile.width)//2,row*375+35),tile);d.text((i*320+8,row*375+8),species+' '+overlay,fill='black')
sheet.save(QA/'overlay_bird_composites.png')
print('Prepared seven runtime PNGs, manifest and isolated QA.')
# Actual nominal game-size views from current V7Layout:200px adult,140px chick.
game=Image.new('RGB',(800,470),(231,237,229));d=ImageDraw.Draw(game)
for i,species in enumerate(anchors):
    adult=Image.open(QA/(species+'_mama_bow.png')).resize((200,200),Image.Resampling.LANCZOS)
    chick=Image.open(QA/(species+'_chick_fluff.png')).resize((140,140),Image.Resampling.LANCZOS)
    game.paste(adult,(i*200,24),adult);game.paste(chick,(i*200+30,255),chick)
    d.text((i*200+4,5),species+' mama200 / chick140',fill='black')
game.save(QA/'overlay_game_size.png')
access=Image.new('RGB',(512,300),(232,224,209));d=ImageDraw.Draw(access)
for i,name in enumerate(['mama_bow','chick_fluff']):
    im=Image.open(OUT/(name+'.png'));access.paste(im,(i*256+64,20),im)
    dark=Image.new('RGBA',(128,128),(32,43,56,255));dark.alpha_composite(im);access.paste(dark.convert('RGB'),(i*256,160))
    tiny=im.resize((64,64),Image.Resampling.LANCZOS);gray=ImageOps.grayscale(tiny).convert('RGBA');gray.putalpha(tiny.getchannel('A'));access.paste(gray,(i*256+160,192),gray);d.text((i*256+10,4),name,fill='black')
access.save(QA/'overlays_alpha_64_grayscale.png')
