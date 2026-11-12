from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, os
root=Path('/workspace/scratch/2bd695cff14d/game-approved-ui-integration')
qa=root/'build/approved-ui-integration/gui'
out=qa/'comparisons';out.mkdir(exist_ok=True)
font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',22)
small=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',16)
def compare(name,ref,run,left='Approved design reference',right='Merged Godot render',crop=None):
 a=Image.open(ref).convert('RGB')
 if crop:a=a.crop(crop)
 b=Image.open(run).convert('RGB')
 # Derived comparison only. Original PNGs remain unchanged.
 panels=[]
 for im in [a,b]:
  panels.append(im.resize((round(im.width*1280/im.height),1280),Image.Resampling.LANCZOS))
 w=panels[0].width+24+panels[1].width
 canvas=Image.new('RGB',(w,1360),(245,245,240));d=ImageDraw.Draw(canvas)
 d.text((12,10),left,fill=(20,40,30),font=font)
 d.text((panels[0].width+36,10),right,fill=(20,40,30),font=font)
 d.text((12,42),'Derived side-by-side comparison; uniformly rescaled. Original captures are separate.',fill=(70,70,70),font=small)
 canvas.paste(panels[0],(0,80));canvas.paste(panels[1],(panels[0].width+24,80))
 canvas.save(out/(name+'.jpg'),quality=94)
 return {'name':name,'reference':str(ref),'capture':str(run),'crop':crop,'derived_comparison':str(out/(name+'.jpg'))}
refs=root/'art/approved_ui';runs=qa/'render-target';items=[]
for name,ref,run in [('map','map_carousel.png','map_view__cards_zh.png'),('aviary-list','aviary.png','aviary_view__list_some_unmet_zh.png'),('aviary-detail','aviary_detail.png','aviary_view__detail_berry_zh.png'),('draw','lucky_spin.png','popups__lucky_draw_zh.png'),('purchase','coin_shop.png','coin_shop_view__packs_zh.png')]:
 items.append(compare(name,refs/ref,qa/'map-aviary-corrected/aviary_sunny.png' if name == 'aviary-detail' else runs/run))
ref=refs/'star_chest.png';w,h=Image.open(ref).size
if os.environ.get('SKIP_CHEST') != '1':
 items.append(compare('chest-progress',ref,qa/'chest-layout/chest_18_stars.png',crop=(0,0,w//2,h)))
 items.append(compare('chest-rewards',ref,runs/'popups__rewards_chest_zh.png',crop=(w//2,0,w,h)))
items.append(compare('home-contrast',Path('/workspace/scratch/2bd695cff14d/game-home-forest/build/home-forest-review/home-jungle-zh.png'),qa/'home_view__home_zh.png',left='Prior Home foreground capture',right='Merged Home with icon backings'))
(out/'sources.json').write_text(json.dumps(items,indent=2))
print('created',len(items),'comparisons')
