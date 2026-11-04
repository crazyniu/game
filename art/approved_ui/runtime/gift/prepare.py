"""Prepare supplied generated pieces with alpha-preserving trim/resize and QA composition."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,shutil
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
OUT=ROOT/'PerchSort/Assets/Resources/Art'
sources=json.loads((HERE.parent/'assemblies_wip/sources.json').read_text())
rects={'gift_nest':(70,390,628,350),'gift_box_base':(212,305,344,300),'gift_box_lid':(188,170,392,250)}
sprites={};records=[]
for name,rect in rects.items():
 raw=HERE.parent/'assemblies_wip/raw'/f'{name}.png';shutil.copyfile(raw,HERE/'raw'/raw.name)
 im=Image.open(raw).convert('RGBA');bbox=im.getchannel('A').point(lambda a:255 if a>8 else 0).getbbox()
 im=im.crop(bbox).resize(rect[2:],Image.Resampling.LANCZOS)
 s=Image.new('RGBA',(768,768));s.alpha_composite(im,rect[:2]);s.save(OUT/f'{name}.png');sprites[name]=s
 records.append({'file':name+'.png','canvas':[768,768],'placement_xywh':rect,'source_crop_xyxy':bbox,'alpha_extrema':s.getchannel('A').getextrema(),'reference':'art/approved_ui/ui_popup_daily_gift.png'})
 sheet=Image.new('RGBA',(1536,768),'#182433');sheet.alpha_composite(Image.new('RGBA',(768,768),'#e5dfd2'),(768,0));sheet.alpha_composite(s);sheet.alpha_composite(s,(768,0));sheet.convert('RGB').save(HERE/'qa'/f'{name}_alpha.jpg',quality=92)
for state,dy in [('closed',0),('open',-130)]:
 q=Image.new('RGBA',(768,768));q.alpha_composite(sprites['gift_nest']);q.alpha_composite(sprites['gift_box_base']);q.alpha_composite(sprites['gift_box_lid'],(0,dy))
 # Reuse exact foreground pixels of nest, no painted or synthesized content.
 foreground=Image.new('RGBA',(768,768))
 for x in range(768):
  y=round(600-0.001*(x-384)**2)
  foreground.alpha_composite(sprites['gift_nest'].crop((x,y,x+1,768)),(x,y))
 q.alpha_composite(foreground)
 foreground.save(HERE/'nest_foreground.png')
 foreground.save(OUT/'gift_nest_front.png')
 q.save(HERE/'qa'/f'assembled_{state}.png')
 screen=Image.open(ROOT/'art/approved_ui/ui_popup_daily_gift.png').convert('RGBA');screen.alpha_composite(q.resize((660,660)),(210,560));screen.convert('RGB').save(HERE/'qa'/f'overlay_{state}.jpg',quality=92)
 contact=Image.new('RGBA',(768,768),'#26313b');contact.alpha_composite(q);contact.convert('RGB').save(HERE/'qa'/f'{state}_dark.jpg',quality=92)
d={'coordinate_system':'top-left pixels; common untrimmed 768x768 canvas','parent_canvas':[768,768],'all_parts_offset':[0,0],
 'lid_closed_pivot_xy':[384,390],'lid_pivot_unity_normalized':[0.5,1-390/768],
 'lid_open_translation_xy':[0,-130],'lid_closed_translation_xy':[0,0],'box_opening_center_xy':[384,355],
 'draw_order':['gift_nest full','gift_box_base','gift_box_lid','gift_nest_front'],
 'nest_foreground_crop':'for each x column retain y >= round(600 - 0.001*(x-384)^2); existing pixels only',
 'assets':records+[{'file':'gift_nest_front.png','canvas':[768,768],'derived_from':'gift_nest.png','derivation':'per-column foreground crop, unchanged source pixels; see nest_foreground_crop','alpha_extrema':[0,255]}],'runtime_verified':False}
(HERE/'delivery.json').write_text(json.dumps(d,indent=2)+'\n')
(HERE/'sources.json').write_text(json.dumps([s for s in sources if s['name'] in rects],indent=2)+'\n')
print(json.dumps(d))
