"""Resize generated background plates and produce static QA; never draws artwork."""
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import json,hashlib
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
OUT=ROOT/'PerchSort/Assets/Resources/V7/World'
records=json.loads((HERE/'records.json').read_text())
metadata=[]
worlds={}
for r in records:
    src=HERE/'raw'/(r['name']+'.png')
    im=Image.open(src).convert('RGBA')
    out=ImageOps.fit(im,(1080,1920),method=Image.Resampling.LANCZOS)
    target=OUT/(r['name']+'.png');out.save(target)
    metadata.append({'file':str(target.relative_to(ROOT)),'size':[1080,1920],'mode':'RGBA','alpha_extrema':list(out.getchannel('A').getextrema()),'source_size':list(im.size),'preparation':'Lanczos center-fit to portrait9:16; no color or artwork changes','reference':r['reference'],'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
    world=r['name'].split('_')[2];worlds.setdefault(world,[]).append((r['name'],out))
for world,items in worlds.items():
    sheet=Image.new('RGB',(270*len(items),504),'white');draw=ImageDraw.Draw(sheet)
    for i,(name,im) in enumerate(items):
        sheet.paste(im.resize((270,480)),(i*270,24));draw.text((i*270+5,5),name,fill='black')
    sheet.save(HERE/'qa'/(world+'_backgrounds.jpg'))
(HERE/'delivery.json').write_text(json.dumps(metadata,indent=2)+'\n')
print('Prepared',len(metadata),'background plates.')
