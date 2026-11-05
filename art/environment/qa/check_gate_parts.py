from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import numpy as np
import hashlib, json

ROOT = Path(__file__).resolve().parents[3]
WORLD = ROOT / 'PerchSort/Assets/Resources/World'
QA = ROOT / 'art/environment/qa'
EVIDENCE = ROOT / 'docs/work-log/environment-art/evidence'
FONT = ImageFont.truetype('/System/Library/Fonts/Helvetica.ttc', 18)

def load(name):
    return Image.open(WORLD / (name + '.png')).convert('RGBA')

def label(im, xy, text):
    ImageDraw.Draw(im).text(xy, text, font=FONT, fill='#233744')

checks, comparisons = [], []
lock = Image.open(ROOT / 'PerchSort/Assets/Resources/Art/gate_padlock.png').convert('RGBA')
lock = lock.crop(lock.getchannel('A').getbbox()).resize((73, 99), Image.Resampling.LANCZOS)
for n in range(1, 4):
    frame = load(f'gate_arch_frame_w{n}')
    left, right = load(f'gate_door_left_w{n}'), load(f'gate_door_right_w{n}')
    light = load(f'gate_light_w{n}')
    closed = Image.new('RGBA', (560, 600))
    closed.alpha_composite(left, (118, 274)); closed.alpha_composite(right, (278, 274))
    closed.alpha_composite(frame); closed.alpha_composite(lock, (242, 364))
    opened = Image.new('RGBA', (560, 600))
    opened.alpha_composite(light, (120, 130)); opened.alpha_composite(frame)
    closed.save(QA / f'gate_parts_closed_w{n}.png')
    opened.save(QA / f'gate_parts_clear_w{n}.png')
    master = load(f'gate_arch_closed_w{n}')
    pair = Image.new('RGB', (1120, 660), '#e9eee8')
    pair.paste(master, (0, 40), master); pair.paste(closed, (560, 40), closed)
    label(pair, (15, 10), 'ACCEPTED CLOSED MASTER')
    label(pair, (575, 10), 'LAYER RECONSTRUCTION - MATCH INCOMPLETE')
    label(pair, (15, 630), 'Generated grain / registration and existing standalone padlock differ; not an identical reconstruction.')
    pair.save(EVIDENCE / f'gate_parts_comparison_w{n}.jpg', quality=94)
    a = np.asarray(master, dtype=np.int16); b = np.asarray(closed, dtype=np.int16)
    region = (a[:, :, 3] > 128) | (b[:, :, 3] > 128)
    comparisons.append(dict(world=n,exact_rgba_match=bool(np.array_equal(a,b)),
                            mean_absolute_rgba_difference=float(np.abs(a-b)[region].mean()),
                            changed_opaque_pixels=int(np.any(a!=b,axis=2)[region].sum())))
    sheet = Image.new('RGB', (1260, 860), '#edf0e9')
    for i, (kind, im) in enumerate([('arch_frame',frame), ('door_left',left), ('door_right',right), ('light',light)]):
        x, y = (i%2)*630, (i//2)*430
        label(sheet, (x+12,y+10), f'gate_{kind}_w{n} / light + dark + 64 px')
        thumb = im.copy(); thumb.thumbnail((255,330),Image.Resampling.LANCZOS)
        sheet.paste(thumb,(x+15,y+48),thumb)
        ImageDraw.Draw(sheet).rectangle((x+290,y+44,x+575,y+390),fill='#142530')
        sheet.paste(thumb,(x+300,y+48),thumb)
        tiny=im.copy();tiny.thumbnail((64,64),Image.Resampling.LANCZOS)
        sheet.paste(tiny,(x+580,y+50),tiny)
        alpha=np.asarray(im.getchannel('A'))
        checks.append(dict(asset=f'gate_{kind}_w{n}',canvas=im.size,
                           transparent_pixels=int((alpha==0).sum()),
                           fractional_alpha_pixels=int(((alpha>0)&(alpha<255)).sum()),
                           alpha_edge_max=int(max(alpha[0,:].max(),alpha[-1,:].max(),alpha[:,0].max(),alpha[:,-1].max())),
                           central_doorway_alpha_max=int(alpha[260:550,180:380].max()) if kind=='arch_frame' else None,
                           sha256=hashlib.sha256((WORLD/f'gate_{kind}_w{n}.png').read_bytes()).hexdigest()))
    sheet.save(EVIDENCE / f'gate_parts_assets_w{n}.jpg',quality=93)
    lightpair=Image.new('RGB',(1120,640),'#e9eee8')
    ImageDraw.Draw(lightpair).rectangle((560,40,1119,639),fill='#142530')
    lightpair.paste(opened,(0,40),opened);lightpair.paste(opened,(560,40),opened)
    label(lightpair,(15,10),'FRAME + LIGHT / DOORS OMITTED FOR ALPHA REVIEW')
    lightpair.save(EVIDENCE/f'gate_parts_light_w{n}.jpg',quality=93)
(QA/'gate_parts_checks.json').write_text(json.dumps(checks,indent=2)+'\n')
(QA/'gate_parts_comparison.json').write_text(json.dumps(comparisons,indent=2)+'\n')
print(json.dumps(comparisons,indent=2))
