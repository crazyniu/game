"""Normalize generated assets and emit reproducible static QA evidence."""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageOps
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parents[1]
OUT = REPO / 'PerchSort/Assets/Resources/V5/Art'
QA = ROOT / 'qa'
MANIFEST = json.loads((ROOT/'manifest.json').read_text())
LANCZOS = Image.Resampling.LANCZOS
FONT = ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf', 14)

def clean(im, name):
    im = im.convert('RGBA')
    if name == 'bg_forest_day':
        im.putalpha(255)
        return im
    a = np.array(im)
    if name != 'sparkle':
        # Suppress the generated near-transparent fringe without flattening fur edges.
        a[:,:,3] = np.clip((a[:,:,3].astype(float)-20)*255/233,0,255).astype('uint8')
    a[a[:,:,3] == 0,:3] = 0
    return Image.fromarray(a)

def fit(im, size, padding=4, stretch=False):
    bbox=im.getbbox()
    if bbox: im=im.crop(bbox)
    space=(size[0]-2*padding,size[1]-2*padding)
    im=im.resize(space,LANCZOS) if stretch else ImageOps.contain(im,space,LANCZOS)
    out=Image.new('RGBA',size)
    out.alpha_composite(im,((size[0]-im.width)//2,(size[1]-im.height)//2))
    return out

def build(item):
    name=item['name'];size=tuple(item['size'])
    path=ROOT/'raw'/f'{name}.png'
    if not path.exists(): return None
    im=clean(Image.open(path),name)
    if name == 'bg_forest_day':
        out=ImageOps.fit(im,size,LANCZOS)
    elif name.startswith('bird_'):
        # All eight image-tool outputs share the same reference canvas and body anchors.
        # Never fit flight frames by wing bounds, which would shrink the torso.
        out=Image.new('RGBA',size)
        resized=im.resize((238,238),LANCZOS)
        out.alpha_composite(resized,(9,16 if '_sunny_' in name else 15))
    elif name == 'branch_left':
        crop=im.crop(im.getbbox())
        out=Image.new('RGBA',size)
        crop=crop.resize((640,98),LANCZOS)
        out.alpha_composite(crop,(0,15))
    elif name == 'bush_edge':
        crop=im.crop(im.getbbox()).resize((360,280),LANCZOS)
        out=Image.new('RGBA',size)
        out.alpha_composite(crop,(0,10))
    elif name in ('pill_cream','panel_win','ribbon') or name.startswith('btn_'):
        out=fit(im,size,4,stretch=True)
    else:
        out=fit(im,size,5 if name.startswith('icon_') else 4)
    a=np.array(out)
    if name != 'sparkle': a[a[:,:,3]<3,3]=0
    a[a[:,:,3]==0,:3]=0
    out=Image.fromarray(a)
    out.save(OUT/f'{name}.png',optimize=True)
    alpha=np.array(out.getchannel('A'))
    solid=Image.fromarray((alpha>=128).astype('uint8')*255).getbbox()
    return dict(file=name+'.png',size=list(out.size),mode=out.mode,
                alpha_min=int(alpha.min()),alpha_max=int(alpha.max()),
                transparent_pixels=int((alpha==0).sum()),partial_alpha_pixels=int(((alpha>0)&(alpha<255)).sum()),
                visible_bounds=out.getbbox(),solid_bounds=solid)

def make_sheets(records):
    cellw,cellh,cols=410,250,3
    rows=(len(records)+cols-1)//cols
    sheet=Image.new('RGB',(cols*cellw,rows*cellh+100),'#d4d3cf')
    d=ImageDraw.Draw(sheet)
    d.text((16,12),'Perch Sort v5 | every runtime PNG on light and dark | static asset review',font=FONT,fill='#232a2d')
    for i,r in enumerate(records):
        x=(i%cols)*cellw;y=(i//cols)*cellh+40
        d.text((x+8,y+4),r['file'],font=FONT,fill='#232a2d')
        im=Image.open(OUT/r['file'])
        thumb=ImageOps.contain(im,(186,192),LANCZOS)
        for j,bg in enumerate(('#fff4df','#1b2730')):
            tile=Image.new('RGBA',(198,202),bg)
            tile.alpha_composite(thumb,((198-thumb.width)//2,(202-thumb.height)//2))
            sheet.paste(tile.convert('RGB'),(x+5+j*201,y+29))
    sheet.save(ROOT/'contact_sheet.png')
    strip=Image.new('RGB',(336,102),'#d5d5d5');d=ImageDraw.Draw(strip)
    for idx,species in enumerate(('sunny','sky')):
        im=Image.open(OUT/f'bird_{species}_idle.png').resize((64,64),LANCZOS)
        gray=ImageOps.grayscale(im).convert('RGBA');gray.putalpha(im.getchannel('A'))
        for j,bg in enumerate(('#f7f7f7','#202020')):
            tile=Image.new('RGBA',(64,64),bg);tile.alpha_composite(gray)
            strip.paste(tile.convert('RGB'),(8+idx*168+j*72,4))
        d.text((8+idx*168,76),species+' | 64 px',font=FONT,fill='#222222')
    strip.save(ROOT/'bird_idles_grayscale_64.png')
    frames=Image.new('RGB',(1040,568),'#263239');d=ImageDraw.Draw(frames)
    for row,species in enumerate(('sunny','sky')):
        for col,pose in enumerate(('idle','blink','fly_up','fly_down')):
            name=f'bird_{species}_{pose}.png';im=Image.open(OUT/name)
            frames.paste(Image.alpha_composite(Image.new('RGBA',(256,256),'#263239'),im).convert('RGB'),(col*260,row*284))
            d.text((col*260+4,row*284+260),name,font=FONT,fill='white')
    frames.save(QA/'bird_frames.png')

if __name__=='__main__':
    records=[r for item in MANIFEST if (r:=build(item))]
    (QA/'asset_metrics.json').write_text(json.dumps(records,indent=2)+'\n')
    if all((OUT/f'bird_{s}_{p}.png').exists() for s in ('sunny','sky') for p in ('idle','blink','fly_up','fly_down')):
        make_sheets(records)
    print(f'Built {len(records)}/{len(MANIFEST)} assets')
    for r in records:
        print(r['file'],r['size'],r['mode'],f"alpha {r['alpha_min']}..{r['alpha_max']}",r['solid_bounds'])
