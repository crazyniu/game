"""Extract source artwork only; masking, trimming, and resizing, never redrawing."""
from PIL import Image, ImageDraw, ImageFilter, ImageOps
from pathlib import Path
from collections import deque
import json, math
P=Path(__file__).resolve().parent
SOURCE=P.parents[1]/'ui7_icons.png'
im=Image.open(SOURCE).convert('RGB')
items={
'coin':((1274,538,1492,766),96,'color'),
'star_full':((546,788,747,981),160,'color'),
'star_empty':((787,788,986,981),160,'color'),
'footprints':((1033,814,1234,981),64,'color'),
'flag':((1284,786,1488,982),64,'flag'),
'padlock':((65,1012,239,1217),64,'color'),
'check_green':((300,1037,496,1201),64,'color'),
'gift_sun_note':((523,1006,754,1234),96,'disc'),
'gift_wind':((771,1006,1001,1234),96,'disc'),
'gift_grumpy_eyes':((1019,1006,1247,1234),96,'disc'),
'gift_watering_can':((1269,1006,1497,1234),96,'disc'),
'gift_feather':((32,1250,265,1486),96,'disc'),
'gift_flame':((280,1250,513,1486),96,'disc'),
'gift_moon':((525,1250,755,1486),96,'disc'),
'gift_moon_ray':((770,1250,1002,1486),96,'disc'),
'gift_feather_counter':((1018,1250,1249,1486),96,'disc'),
'icon_gear':((94,92,214,211),96,'glyph'),
'icon_undo':((339,96,464,207),96,'glyph'),
'icon_bulb':((581,67,721,223),96,'glyph'),
'icon_home':((1071,327,1204,456),96,'glyph'),
'icon_replay':((1320,329,1452,463),96,'glyph'),
'icon_sound_on':((83,591,224,705),96,'glyph'),
'icon_sound_off':((335,584,465,707),96,'glyph'),
'icon_close':((593,592,701,702),96,'glyph'),
'icon_play':((851,591,952,703),96,'glyph'),
}
def keep_components(mask, number):
 w,h=mask.size;pix=mask.load();seen=set();components=[]
 for y in range(h):
  for x in range(w):
   if (x,y) in seen or pix[x,y]<16:continue
   q=deque([(x,y)]);seen.add((x,y));component=[]
   while q:
    a,b=q.popleft();component.append((a,b))
    for c,d in ((a-1,b),(a+1,b),(a,b-1),(a,b+1)):
     if 0<=c<w and 0<=d<h and (c,d) not in seen and pix[c,d]>=16:seen.add((c,d));q.append((c,d))
   components.append(component)
 out=Image.new('L',mask.size);op=out.load()
 for component in sorted(components,key=len,reverse=True)[:number]:
  if len(component)<30:continue
  for x,y in component:op[x,y]=pix[x,y]
 return out

def fill_holes(mask):
 w,h=mask.size;pix=mask.load();seen=set();q=deque()
 for x in range(w):
  for y in (0,h-1):
   if pix[x,y]<128:q.append((x,y));seen.add((x,y))
 for y in range(h):
  for x in (0,w-1):
   if pix[x,y]<128:q.append((x,y));seen.add((x,y))
 while q:
  x,y=q.popleft()
  for a,b in ((x-1,y),(x+1,y),(x,y-1),(x,y+1)):
   if 0<=a<w and 0<=b<h and (a,b) not in seen and pix[a,b]<128:seen.add((a,b));q.append((a,b))
 out=mask.copy();p=out.load()
 for y in range(h):
  for x in range(w):
   if (x,y) not in seen:p[x,y]=255
 return out
metadata={}
for name,(crop,size,kind) in items.items():
 rgb=im.crop(crop);mask=Image.new('L',rgb.size);mp=mask.load();pix=rgb.load()
 for y in range(rgb.height):
  for x in range(rgb.width):
   r,g,b=pix[x,y];mx=max(r,g,b);mn=min(r,g,b);sat=(mx-mn)/max(mx,1)
   if kind=='glyph':
    # White/lavender artwork is separated from saturated purple. RGB stays unchanged.
    score=min((r/max(b,1)-.74)/.12,(g/max(b,1)-.61)/.15,(mn-125)/45)
    mp[x,y]=int(255*max(0,min(1,score)))
   else:
    gold_or_color=sat>.23
    cloth=False
    mp[x,y]=255 if gold_or_color or cloth else 0
 if kind=='flag':
  # Hand-traced silhouette mask for the low-contrast cream cloth; source RGB unchanged.
  points=[(1324,831),(1337,825),(1350,822),(1365,822),(1380,823),(1400,827),(1420,835),(1440,839),(1460,837),(1474,833),(1477,835),(1472,853),(1466,871),(1465,884),(1467,898),(1472,913),(1477,925),(1473,930),(1465,935),(1450,940),(1434,942),(1415,941),(1393,935),(1370,924),(1350,917),(1330,917),(1314,923)]
  ImageDraw.Draw(mask).polygon([(x-crop[0],y-crop[1]) for x,y in points],fill=255)
 if kind!='glyph':
  # Close only subpixel highlight cracks, then fill enclosed original artwork.
  mask=mask.filter(ImageFilter.MaxFilter(3)).filter(ImageFilter.MinFilter(3))
  mask=fill_holes(mask)
  if name=='padlock':
   mp=mask.load()
   for yy in range(mask.height):
    for xx in range(mask.width):
     sx,sy=crop[0]+xx,crop[1]+yy
     r,g,b=pix[xx,yy]
     if 122<sx<183 and 1045<sy<1094 and (max(r,g,b)-min(r,g,b))/max(r,g,b)<.23:mp[xx,yy]=0
  # Inset by one original source pixel removes gray background antialias contamination.
  mask=mask.filter(ImageFilter.MinFilter(3)).filter(ImageFilter.GaussianBlur(.45))
 else:
  mask=mask.filter(ImageFilter.MedianFilter(3))
  if name=='icon_bulb':
   mp=mask.load()
   for yy in range(mask.height):
    for xx in range(mask.width):
     if math.hypot(crop[0]+xx-650,crop[1]+yy-145)>81 or (crop[0]+xx<630 and crop[1]+yy<87):mp[xx,yy]=0
   mask=keep_components(mask,12)
  else:
   mask=keep_components(mask,{'icon_sound_on':3,'icon_sound_off':2,'icon_replay':2,'icon_home':2}.get(name,1))
 rgba=rgb.convert('RGBA');rgba.putalpha(mask);bounds=mask.getbbox()
 if not bounds:continue
 rgba=rgba.crop(bounds);rgba.thumbnail((size-4,size-4),Image.Resampling.LANCZOS)
 out=Image.new('RGBA',(size,size));out.alpha_composite(rgba,((size-rgba.width)//2,(size-rgba.height)//2));out.save(P/(name+'.png'))
 metadata[name]={'source':'art/v7/ui7_icons.png','source_crop_xyxy':list(crop),'source_mask_bounds_xyxy':[crop[0]+bounds[0],crop[1]+bounds[1],crop[0]+bounds[2],crop[1]+bounds[3]],'output_size':[size,size],'alpha_bounds_xyxy':list(out.getchannel('A').getbbox()),'mask_method':kind,'rgb':'unaltered source colors before Lanczos resize','review':'pending_visual'}
 if name in ('star_full','star_empty'):
  small=out.resize((48,48),Image.Resampling.LANCZOS);small.save(P/(name+'_small.png'));metadata[name+'_small']={**metadata[name],'output_size':[48,48],'alpha_bounds_xyxy':list(small.getchannel('A').getbbox())}
(P/'provenance.json').write_text(json.dumps(metadata,indent=2)+'\n')
names=list(metadata);W=6*200;H=((len(names)+5)//6)*210
for gray in (False,True):
 sheet=Image.new('RGB',(W,H),(42,58,65));draw=ImageDraw.Draw(sheet)
 for i,n in enumerate(names):
  x=i%6*200;y=i//6*210;a=Image.open(P/(n+'.png')).convert('RGBA');a.thumbnail((160,160));
  if gray:a=Image.merge('RGBA',(*[ImageOps.grayscale(a)]*3,a.getchannel('A')))
  sheet.paste(a,(x+(200-a.width)//2,y+10+(160-a.height)//2),a);draw.text((x+6,y+180),n,fill='white')
 sheet.save(P/('qa_grayscale.jpg' if gray else 'qa_color.jpg'))
print('extracted',len(metadata))
