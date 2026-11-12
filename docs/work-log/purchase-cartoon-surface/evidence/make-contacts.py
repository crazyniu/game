from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import hashlib, json
root=Path(__file__).resolve().parents[4]
out=Path(__file__).resolve().parent
raw=root/'build/purchase-cartoon-surface'
font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',14)
states=['confirm_booster','confirm_multiple','confirm_maximum','insufficient','confirm_nest','confirm_nest_short','confirm_outfit','confirm_outfit_short']
packs=['preview_stack','preview_cluster','preview_pouch','preview_basket','preview_chest','preview_hoard']
manifest=[]
for lang in ['en','zh']:
 for group,names,cols in [('purchases',states,4),('preview',packs,3)]:
  sheet=Image.new('RGB',(cols*270,2*506),'#253831')
  draw=ImageDraw.Draw(sheet)
  for i,name in enumerate(names):
   path=raw/f'{name}-{lang}.png'
   img=Image.open(path)
   assert img.size==(1080,1920),(path,img.size)
   manifest.append({'state':name,'language':lang,'file':str(path.relative_to(root)),'width':1080,'height':1920,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
   img.thumbnail((270,480))
   x,y=(i%cols)*270,(i//cols)*506
   sheet.paste(img,(x,y+26)); draw.text((x+4,y+5),name+' '+lang,fill='white',font=font)
  sheet.save(out/f'{group}-{lang}.jpg',quality=92)
(out/'capture-manifest.json').write_text(json.dumps(manifest,indent=2)+'\n')
print(f'Contact sheets and manifest cover {len(manifest)} actual 1080x1920 images.')
