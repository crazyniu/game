from pathlib import Path
from PIL import Image
import json, shutil, sys

world = sys.argv[1]
base = Path(__file__).parent / world
manifest = json.loads((base / 'generation.json').read_text())
out = Path('PerchSort/Assets/Resources/V7/World')
out.mkdir(parents=True, exist_ok=True)
(base/'raw').mkdir(exist_ok=True)
sizes = {'branch': (620,90), 'twig': (400,80), 'crown': (600,400), 'trunk': (160,1920)}
names = {'branch':'branch_left', 'twig':'twig_left', 'crown':'tree_crown', 'trunk':'trunk_left'}
metadata = {'world':world, 'coordinates':'Pixels from top-left. Unity pivots normalized from bottom-left.', 'preparation':'Alpha below 32 removed as generated background speckle; retained subject alpha preserved. Alpha bounds trimmed, resized and aligned only.', 'assets':{}}
for row in manifest['assets']:
    kind = row['kind']
    raw = Path(row['raw_path'])
    shutil.copy2(raw, base/'raw'/f'{kind}.png')
    im = Image.open(raw).convert('RGBA')
    im.putalpha(im.getchannel('A').point(lambda a: 0 if a < 32 else a))
    crop = im.getbbox()
    im = im.crop(crop)
    size = sizes[kind]
    canvas = Image.new('RGBA',size)
    if kind == 'trunk':
        im = im.resize((145,1920), Image.Resampling.LANCZOS)
        pos = (0,0)
    else:
        im.thumbnail((size[0],size[1]-12),Image.Resampling.LANCZOS)
        pos = (0,6)
    canvas.alpha_composite(im,pos)
    name = names[kind]+'_'+world+'.png'
    canvas.save(out/name)
    a=canvas.getchannel('A'); h=a.histogram()
    entry={'file':str(out/name),'size':size,'alpha_bounds':a.getbbox(),'source_crop':crop,'transparent_pixels':h[0],'partial_alpha_pixels':sum(h[1:255]),'pivot_normalized':[0,0.5]}
    if kind in ('branch','twig'):
        interval = [round(size[0]*0.30), round(size[0]*0.91)]
        tops=[]
        for x in range(*interval):
            tops.append(next(y for y in range(size[1]) if a.getpixel((x,y))>=128))
        top=sorted(tops)[len(tops)//2]
        entry.update(flat_top_y=top,usable_perch_interval_x=interval,top_y_range=[min(tops),max(tops)],pivot_normalized=[0,1-top/size[1]])
    elif kind == 'crown':
        entry['attachment_pixel']=[round(im.width*0.23),pos[1]+im.height-5]
        entry['pivot_normalized']=[entry['attachment_pixel'][0]/size[0],1-entry['attachment_pixel'][1]/size[1]]
    metadata['assets'][kind]=entry
(base/'metadata.json').write_text(json.dumps(metadata,indent=2)+'\n')
print(json.dumps(metadata,indent=2))
