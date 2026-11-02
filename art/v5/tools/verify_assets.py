"""Check T30 deliverables and create asset-only review composites (not Unity proof)."""
import hashlib
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import numpy as np
from process_assets import ROOT, REPO, OUT, QA, MANIFEST, LANCZOS, FONT

records=[]
for item in MANIFEST:
    path=OUT/(item['name']+'.png')
    assert path.exists(), f'Missing {path}'
    im=Image.open(path)
    assert im.size==tuple(item['size']), f'Wrong size: {path}'
    assert im.mode=='RGBA', f'Wrong mode: {path}'
    a=np.array(im)
    assert a[:,:,3].max()==255, f'No opaque pixels: {path}'
    assert a[:,:,3].min()==(0 if item['transparent'] else 255), f'Alpha failure: {path}'
    assert not np.any(a[a[:,:,3]==0,:3]), f'Nonzero invisible RGB: {path}'
    assert (ROOT/'raw'/path.name).exists(), f'Missing raw: {path}'
    records.append(dict(file=path.name,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
assert len(records)==33
assert len(list(OUT.glob('*.png')))==33
assert not (OUT/'branch_right.png').exists()
for s in ('sunny','sky'):
    for pose in ('idle','blink'):
        a=np.array(Image.open(OUT/f'bird_{s}_{pose}.png'))[:,:,3]
        assert np.where(a>=128)[0].max()==244
    assert (OUT/f'bird_{s}_fly_up.png').read_bytes()!=(OUT/f'bird_{s}_fly_down.png').read_bytes()
for name,point in [('icon_settings',(96,96)),('icon_restart',(96,96)),('icon_home',(96,170))]:
    assert Image.open(OUT/f'{name}.png').getpixel(point)[3]==0, f'Filled icon hole: {name}'
branch=np.array(Image.open(OUT/'branch_left.png'))[:,:,3]
top=[int(np.flatnonzero(branch[:,x]>=128)[0]) for x in (110,230,350,470)]
assert all(37<=v<=42 for v in top)
(QA/'verification.json').write_text(json.dumps(dict(status='PASS',runtime_png_count=33,
    branch_top_samples=dict(zip([110,230,350,470],top)),hand_fingertip_top_left=[23,6],files=records),indent=2)+'\n')

def sprite(name,size=None,mirror=False):
    im=Image.open(OUT/(name+'.png'))
    if size:im=im.resize(size,LANCZOS)
    return ImageOps.mirror(im) if mirror else im

# A review composite using the documented portrait design canvas, not a screenshot.
scene=sprite('bg_forest_day')
scene.alpha_composite(sprite('birdhouse'),(390,60))
scene.alpha_composite(sprite('pill_cream'),(40,105))
scene.alpha_composite(sprite('coin',(64,64)),(55,118))
scene.alpha_composite(sprite('btn_round_purple',(120,120)),(920,90))
scene.alpha_composite(sprite('icon_settings',(76,76)),(942,112))
scene.alpha_composite(sprite('pill_cream',(120,60)),(480,400))
for side,row,birds in [('L',0,['sunny','sunny','sunny','sky']),('R',0,['sky']*3),('L',1,['sunny']),('R',1,[])]:
    center=720+300*row+(40 if side=='R' else 0)
    by=center-56
    scene.alpha_composite(sprite('branch_left',(600,112),side=='R'),(0 if side=='L' else 480,by))
    scene.alpha_composite(sprite('bush_edge',(130,110),side=='R'),(-30 if side=='L' else 980,by-15))
    foot=by+37
    for idx,s in enumerate(birds):
        x=110+120*idx if side=='L' else 970-120*idx
        scene.alpha_composite(sprite(f'bird_{s}_idle',(150,150)),(x-75,round(foot-244*150/256)))
for x,icon in zip((300,540,780),('undo','hint','add_branch')):
    scene.alpha_composite(sprite('btn_square_green',(170,170)),(x-85,1675))
    scene.alpha_composite(sprite('icon_'+icon,(110,110)),(x-55,1705))
d=ImageDraw.Draw(scene)
large=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf',38)
d.text((145,124),'0',font=large,fill='#5a3a22')
d.text((526,405),'1',font=large,fill='#5a3a22')
d.text((40,1880),'ASSET COMPOSITE / NOT A UNITY SCREENSHOT',font=FONT,fill='#243b38')
scene.convert('RGB').save(QA/'level1_asset_composite.png')

# This GIF proves frame swapping only; flight paths and runtime timing remain untested.
frames=[];durations=[]
for pose,duration in [('idle',1600),('blink',120),('idle',500)]+[('fly_up',70),('fly_down',70)]*8+[('idle',900)]:
    frame=Image.new('RGBA',(512,294),'#263239')
    for x,s in ((0,'sunny'),(256,'sky')):
        frame.alpha_composite(sprite(f'bird_{s}_{pose}'),(x,0))
    ImageDraw.Draw(frame).text((8,270),'OFFLINE FRAME PREVIEW / NOT UNITY RUNTIME',font=FONT,fill='white')
    frames.append(frame.convert('RGB'));durations.append(duration)
frames[0].save(QA/'bird_frame_preview.gif',save_all=True,append_images=frames[1:],duration=durations,loop=0,disposal=2)
gif=Image.open(QA/'bird_frame_preview.gif')
assert gif.n_frames==len(frames)

# Full-resolution hand anchor evidence and a compact icon check at 48 pixels.
hand=Image.new('RGBA',(200,200),'#263239');hand.alpha_composite(sprite('hand_pointer'))
d=ImageDraw.Draw(hand);d.line((17,6,29,6),fill='#ff3355',width=1);d.line((23,0,23,12),fill='#ff3355',width=1)
hand.resize((600,600),Image.Resampling.NEAREST).save(QA/'hand_fingertip.png')
icons=[i['name'] for i in MANIFEST if i['name'].startswith('icon_')]
sheet=Image.new('RGB',(720,180),'#e0d9c7');d=ImageDraw.Draw(sheet)
for idx,name in enumerate(icons):
    x=(idx%5)*144;y=(idx//5)*90
    tile=Image.new('RGBA',(48,48),'#264533');tile.alpha_composite(sprite(name,(48,48)))
    sheet.paste(tile.convert('RGB'),(x+48,y+4))
    d.text((x+4,y+58),name.removeprefix('icon_'),font=FONT,fill='#352d21')
sheet.save(QA/'icons_48.png')
print('PASS: 33 PNGs; exact dimensions; RGBA; alpha; raw provenance; bird standing baseline; distinct wing frames; icon holes; branch top; offline GIF frame count.')
