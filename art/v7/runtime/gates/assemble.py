"""Prepare generated gate parts by resizing, alpha cutout and compositing only."""
from pathlib import Path
from PIL import Image,ImageOps,ImageFilter,ImageChops,ImageDraw
import json,hashlib
root=Path(__file__).resolve().parents[4];work=root/'art/v7/runtime/gates';out=root/'PerchSort/Assets/Resources/V7/Art'
S=(1024,1024)
def canvas():return Image.new('RGBA',S,(0,0,0,0))
arch=canvas();arch.alpha_composite(Image.open(work/'raw/gate_arch.png').convert('RGBA').resize((860,786),Image.Resampling.LANCZOS),(82,100))
# Trace the existing arch's inner silhouette for the scenery cutout, with overlap under wood.
a=arch.getchannel('A');mask=Image.new('L',S,0);mp=mask.load();ap=a.load();bounds=[]
for y in range(180,852):
 if ap[512,y]>=128:continue
 left=[x for x in range(120,512) if ap[x,y]>=128];right=[x for x in range(512,905) if ap[x,y]>=128]
 if left and right:
  lo=max(left)-3;hi=min(right)+3
  for x in range(lo,hi+1):mp[x,y]=255
  bounds.append((y,lo,hi))
mask=mask.filter(ImageFilter.GaussianBlur(0.6));mask.save(work/'qa/aperture_mask.png')
bbox=mask.getbbox();print('opening',bbox)
# Keep the seam at x=512; generated paired doors are cut at their original seam.
pair=Image.open(work/'raw/doors_pair.png').convert('RGBA').crop((157,73,1144,1110)).resize((556,626),Image.Resampling.LANCZOS)
left=canvas();right=canvas();left.alpha_composite(pair.crop((0,0,278,626)),(234,234));right.alpha_composite(pair.crop((278,0,556,626)),(512,234))
# Closed leaves cover the opening; frame renders over outer edges.
lock=canvas();pad=Image.open(work/'raw/padlock.png').convert('RGBA');pad=ImageOps.contain(pad,(82,90),Image.Resampling.LANCZOS);lock.alpha_composite(pad,(512-pad.width//2,610-pad.height//2))
parts={'gate_arch':arch,'gate_door_left':left,'gate_door_right':right,'gate_padlock':lock}
for world in range(2,5):
 scene=Image.open(work/'raw'/f'gate_backdrop_w{world}.png').convert('RGBA');plate=canvas()
 x0,y0,x1,y1=bbox;scene=ImageOps.fit(scene,(x1-x0,y1-y0),Image.Resampling.LANCZOS);plate.alpha_composite(scene,(x0,y0));plate.putalpha(mask);parts[f'gate_backdrop_w{world}']=plate
locked=canvas();locked.alpha_composite(parts['gate_backdrop_w2']);locked.alpha_composite(left);locked.alpha_composite(right);locked.alpha_composite(arch);locked.alpha_composite(lock)
parts['gate_locked']=locked
opened=canvas();opened.alpha_composite(parts['gate_backdrop_w2']);opened.alpha_composite(arch)
# Static fully-open fallback: horizontal projection of generated leaves around outer hinges.
lo=ImageOps.mirror(pair.crop((0,0,278,626))).resize((176,626),Image.Resampling.LANCZOS)
ro=ImageOps.mirror(pair.crop((278,0,556,626))).resize((176,626),Image.Resampling.LANCZOS)
opened.alpha_composite(lo,(234-176,234));opened.alpha_composite(ro,(790,234));parts['gate_open']=opened
entries=[]
for name,im in parts.items():
 path=out/f'{name}.png';im.save(path);hist=im.getchannel('A').histogram()
 entries.append({'file':str(path.relative_to(root)),'size':[1024,1024],'mode':'RGBA','alpha_min':0,'alpha_max':255,'transparent_pixels':hist[0],'partial_alpha_pixels':sum(hist[1:255]),'pivot_normalized':[0.5,0.5],'sha256':hashlib.sha256(path.read_bytes()).hexdigest()})
# Approved screenshot backdrop composites inspect palette and silhouette, not runtime acceptance.
ref=Image.open(root/'art/v7/ui7_popup_world_unlock.png').convert('RGBA')
for name,im in parts.items():
 qa=ref.copy();sp=im.resize((900,900),Image.Resampling.LANCZOS);qa.alpha_composite(sp,(90,650));qa.convert('RGB').save(work/'qa'/f'{name}_overlay.jpg',quality=90)
contact=Image.new('RGB',(1536,1024),'#ccd6df');d=ImageDraw.Draw(contact)
for i,(name,im) in enumerate(parts.items()):
 x=(i%3)*512;y=(i//3)*341;sp=im.resize((310,310));contact.paste(sp,(x+100,y+25),sp);d.text((x+8,y+7),name,fill='black')
contact.save(work/'qa/gates_contact.jpg',quality=94)
meta={'canvas':[1024,1024],'coordinate_origin':'top-left','opening_bounds':list(bbox),'closed_center_seam_x':512,'hinges':{'left':[246,610],'right':[778,610]},'door_closed_rects':{'left':[234,234,278,626],'right':[512,234,278,626]},'padlock_center':[512,610],'gate_light':{'file':'PerchSort/Assets/Resources/V7/Art/gate_light.png','unchanged':True,'source_aperture_normalized':[0.5,0.25],'assembly_target_aperture':[512,375],'recommended_draw_rect':[192,215,640,640]},'open_fallback':'Bamboo backdrop; projected/mirrored generated door leaves swing outward. Static fallback only; not proof of animated rotation.','files':entries}
meta['opening_bounds_format']='xyxy: left, top, right-exclusive, bottom-exclusive'
meta['hinge_coordinate_origin']='top-left pixels'
meta['composition_order_back_to_front']={'locked':['gate_backdrop_w2','gate_door_left','gate_door_right','gate_arch','gate_padlock'],'open':['gate_backdrop_w2','gate_arch','projected_left_door','projected_right_door'],'animated':['gate_backdrop_wN','gate_light','gate_door_left','gate_door_right','gate_arch','gate_padlock (locked only)']}
meta['open_fallback_transforms']={'left':{'source_pair_crop_xyxy':[0,0,278,626],'mirror_horizontal':True,'resize_to':[176,626],'offset_xy':[58,234]},'right':{'source_pair_crop_xyxy':[278,0,556,626],'mirror_horizontal':True,'resize_to':[176,626],'offset_xy':[790,234]},'note':'Crops refer to prepared 556x626 doors_pair. Static projected view; runtime swing uses recorded outer hinges.'}
for entry in entries:
 name=Path(entry['file']).stem
 alpha=parts[name].getchannel('A');entry['alpha_min'],entry['alpha_max']=alpha.getextrema()
 if name in ('gate_door_left','gate_door_right'):
  x,y=meta['hinges']['left' if name.endswith('left') else 'right']
  entry.pop('pivot_normalized',None)
  entry['hinge_pixels_top_left']=[x,y]
  entry['pivot_normalized_top_left']=[x/1024,y/1024]
  entry['pivot_normalized_unity_bottom_left']=[x/1024,1-y/1024]
meta['gate_light']['sha256']=hashlib.sha256((out/'gate_light.png').read_bytes()).hexdigest()
(work/'delivery.json').write_text(json.dumps(meta,indent=2)+'\n')
