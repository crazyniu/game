"""Align image-tool sprites and create static/animated QA; no artwork synthesis."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps
import json
import sys

ROOT = Path(__file__).resolve().parents[4]
SOURCE = ROOT / 'art/v7/birds/source'
OUT = ROOT / 'PerchSort/Assets/Resources/V7/Birds'
QA = ROOT / 'art/v7/birds/qa'
STATES = 'idle blink quirk1 quirk2 fly_up fly_down cheer sleep tilt_left tilt_right look_up look_down look_side preen fluff stretch yawn'.split()
SPECIES = sys.argv[1:] or 'sunny sky berry mint cocoa snow ember ink hoot crow'.split()

def bounds(im):
    return im.getchannel('A').point(lambda a: 255 if a > 12 else 0).getbbox()

def feet(im, species):
    box = bounds(im)
    x0, y0, x1, y1 = box
    points = []
    px = im.load()
    for y in range(int(y0 + (y1-y0)*.96), y1):
        for x in range(x0, x1):
            r,g,b,a = px[x,y]
            if a > 160 and ((r > 170 and 65 < g < 210 and b < 100 and r > g*1.15) if species != 'crow' else True):
                points.append((x,y))
    return (min(x for x,y in points), min(y for x,y in points), max(x for x,y in points)+1, max(y for x,y in points)+1)

records = []
for species in SPECIES:
    idle_path = SOURCE / f'{species}_idle.png'
    if not idle_path.exists():
        continue
    idle = Image.open(idle_path).convert('RGBA')
    ib = bounds(idle)
    base_scale = 195 / (ib[2]-ib[0])
    idle_feet = feet(idle, species)
    foot_width = (idle_feet[2]-idle_feet[0])*base_scale
    # Preserve lead-reviewed torso scale when Berry's corrected plumes widen its silhouette.
    if species == 'berry':
        foot_width = 98.311
    prepared = {}
    for state in STATES:
        src = SOURCE / f'{species}_{state}.png'
        if not src.exists():
            continue
        im = Image.open(src).convert('RGBA')
        anchor = feet(im, species)
        scale = foot_width / (anchor[2]-anchor[0])
        w,h = round(im.width*scale), round(im.height*scale)
        scaled = im.resize((w,h), Image.Resampling.LANCZOS)
        dx = round(160-(anchor[0]+anchor[2])/2*scale)
        dy = round(300-anchor[3]*scale)
        canvas = Image.new('RGBA',(320,320))
        canvas.alpha_composite(scaled,(dx,dy))
        canvas.save(OUT / src.name)
        prepared[state] = canvas
        bb = bounds(canvas)
        records.append(dict(file=src.name,canvas=[320,320],source_size=list(im.size),source_feet=list(anchor),scale=round(scale,5),translation=[dx,dy],alpha_extrema=canvas.getchannel('A').getextrema(),visible_bounds=bb,foot_baseline=300,edge_clipped=bb[0]==0 or bb[1]==0 or bb[2]==320 or bb[3]==320))
    # Portrait crop is derived from idle and circularly clipped, not new artwork.
    face = prepared['idle'].crop((40,45,280,285)).resize((160,160),Image.Resampling.LANCZOS)
    mask=Image.new('L',(160,160)); ImageDraw.Draw(mask).ellipse((1,1,158,158),fill=255)
    from PIL import ImageChops
    face.putalpha(ImageChops.multiply(face.getchannel('A'),mask))
    face.save(OUT/f'{species}_face.png')
    strip=Image.new('RGB',(160*len(STATES),185),'#e9e5df'); draw=ImageDraw.Draw(strip)
    frames=[]
    for index,state in enumerate(STATES):
        draw.text((index*160+6,164),state,fill='#302e2c')
        if state in prepared:
            thumb=prepared[state].resize((160,160),Image.Resampling.LANCZOS)
            strip.paste(thumb,(index*160,0),thumb)
        else:
            draw.text((index*160+15,70),'PENDING',fill='#943b39')
    strip.save(QA/f'{species}_frames.png')
    for state in ['idle','blink','idle','quirk1','quirk2','idle']+STATES[8:]:
        if state not in prepared:
            continue
        frame=Image.new('RGB',(320,345),'#e9e5df')
        frame.paste(prepared[state],(0,0),prepared[state])
        ImageDraw.Draw(frame).text((12,325),f'{species}: {state}',fill='#302e2c')
        frames.append(frame)
    frames[0].save(QA/f'{species}_idle_actions.gif',save_all=True,append_images=frames[1:],duration=550,loop=0,disposal=2)
previous = json.loads((QA/'alignment.json').read_text())
records = [r for r in previous if r['file'].split('_')[0] not in SPECIES] + records
(QA/'alignment.json').write_text(json.dumps(records,indent=2)+'\n')
print(json.dumps({'frames':len(records),'clipped':[r['file'] for r in records if r['edge_clipped']]}))
