"""Align generated house parts by trimming/resizing; no artwork is drawn."""
from pathlib import Path
from PIL import Image, ImageDraw
import json,shutil
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
OUT=ROOT/'PerchSort/Assets/Resources/Art'
sources=json.loads((HERE/'sources.json').read_text())
records=[];objects={}
def cut(name):
 src=Path(sources[name]);raw=HERE/'raw'/f'{name}.png';shutil.copyfile(src,raw)
 im=Image.open(raw).convert('RGBA');box=im.getchannel('A').point(lambda a:255 if a>8 else 0).getbbox()
 return im.crop(box),box
for name in sources:
 im,box=cut(name)
 if name=='house_body': size=(512,512);rect=(64,96,384,360)
 elif name.startswith('house_roof_'):size=(512,512);rect=(32,58,448,180)
 elif name=='house_door_leaf':size=(512,512);rect=(145,198,216,216)
 elif name=='house_door_open':size=(512,512);rect=(70,198,80,216)
 elif name=='house_rope':size=(64,256);rect=(20,4,24,248)
 else:size=(1080,200);rect=(4,4,1072,192)
 sprite=Image.new('RGBA',size);sprite.alpha_composite(im.resize(rect[2:],Image.Resampling.LANCZOS),rect[:2])
 objects[name]=sprite
 records.append({'file':name+'.png','canvas':size,'visible_rect_xywh':rect,'source_crop_xyxy':box,'alpha_extrema':sprite.getchannel('A').getextrema()})
 if name!='house_door_open':sprite.save(OUT/(name+'.png'))
# Fully-open fallback preserves exact body pixels and adds the independently generated leaf.
opened=objects['house_body'].copy();opened.alpha_composite(objects['house_door_open']);opened.save(OUT/'house_door_open.png')
objects['house_open_leaf']=objects['house_door_open'];objects['house_door_open']=opened
colors=['teal','red','yellow','purple','green','blue','orange','pink']
tiles=[]
for color in colors:
 if 'house_roof_'+color not in objects:continue
 house=objects['house_body'].copy();house.alpha_composite(objects['house_roof_'+color]);house.save(HERE/'qa'/('assembled_'+color+'.png'))
 tile=Image.new('RGBA',(512,540),'#26333b');tile.alpha_composite(house);ImageDraw.Draw(tile).text((12,520),color,fill='white');tiles.append(tile)
 screen=Image.open(ROOT/'art/approved_ui/ui_level_k3.png').convert('RGBA');screen.alpha_composite(house.resize((285,285)),(380,112));screen.convert('RGB').save(HERE/'qa'/('overlay_'+color+'.jpg'),quality=90)
for state in ['closed','open']:
 house=objects['house_body'].copy() if state=='closed' else opened.copy()
 if state=='closed':house.alpha_composite(objects['house_door_leaf'])
 house.alpha_composite(objects['house_roof_teal']);house.save(HERE/'qa'/('assembled_'+state+'.png'))
 tile=Image.new('RGBA',(512,540),'#26333b');tile.alpha_composite(house);ImageDraw.Draw(tile).text((12,520),state,fill='white');tiles.append(tile)
sheet=Image.new('RGBA',(512*5,540*2),'#26333b')
for i,tile in enumerate(tiles):sheet.alpha_composite(tile,((i%5)*512,(i//5)*540))
sheet.convert('RGB').save(HERE/'qa/assembled_sheet.jpg',quality=94)
for name,im in objects.items():
 tile=Image.new('RGBA',(im.width*2,im.height),'#1a2732');tile.alpha_composite(Image.new('RGBA',im.size,'#e8ddd0'),(im.width,0));tile.alpha_composite(im);tile.alpha_composite(im,(im.width,0));tile.convert('RGB').save(HERE/'qa'/(name+'_alpha.jpg'),quality=90)
contract={'coordinate_system':'Top-left pixels in parent canvas; shared house frame 512x512. Composite at zero offset, no trimming at import.',
 'shared_canvas':[512,512],'body_peak_xy':[256,96],'roof_suspension_anchor_xy':[256,58],
 'door_center_xy':[253,306],'door_dark_radius':102,'door_closed_outer_radius':108,'door_left_hinge_xy':[145,306],
 'door_leaf_pivot_unity_normalized':[145/512,1-306/512],
 'closed_transform':{'position_xy':[0,0],'scale_xy':[1,1],'rotation_deg':0},
 'open_transform':{'hinge_xy':[145,306],'rotation_about_vertical_deg':70,'projected_width_px':80,'fallback_file':'house_door_open.png','contains_body':True,'contains_roof':False},
 'rope':{'canvas':[64,256],'top_anchor_xy':[32,4],'bottom_anchor_xy':[32,252],'connect_bottom_to_house':[256,58]},
 'beams':{'canvas':[1080,200],'house_suspension_y_by_world':{'w1':170,'w2':170,'w3':170,'w4':120},'seven_rope_anchors_x':[96,244,392,540,688,836,984]},
 'assets':records}
(HERE/'delivery.json').write_text(json.dumps(contract,indent=2)+'\n')
print('Saved',len(records),'house assets')
