"""Trim genuine generated alpha, remove isolated matte noise, resize and QA."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps, ImageFilter
from collections import deque
import numpy as np,json,hashlib
P=Path(__file__).resolve().parent;ROOT=P.parents[3];ART=ROOT/'PerchSort/Assets/Resources/Art'
names=['node_wood','node_wood_current_glow','node_stone_locked']+[f'world_sign_w{i}' for i in range(1,5)]
def largest_component_alpha(im):
 a=np.array(im.getchannel('A'));solid=a>16;h,w=solid.shape;seen=np.zeros(solid.shape,bool);largest=[]
 for y,x in zip(*np.where(solid)):
  if seen[y,x]:continue
  q=deque([(int(x),int(y))]);seen[y,x]=True;c=[]
  while q:
   xx,yy=q.popleft();c.append((xx,yy))
   for nx,ny in ((xx-1,yy),(xx+1,yy),(xx,yy-1),(xx,yy+1)):
    if 0<=nx<w and 0<=ny<h and solid[ny,nx] and not seen[ny,nx]:seen[ny,nx]=True;q.append((nx,ny))
  if len(c)>len(largest):largest=c
 m=Image.new('L',im.size);mp=m.load()
 for x,y in largest:mp[x,y]=255
 m=m.filter(ImageFilter.MaxFilter(3));clean=np.where(np.array(m)>0,a,0).astype('uint8');im.putalpha(Image.fromarray(clean));return im
records={};sheet=Image.new('RGB',(4*400,2*380),'#204753');ds=ImageDraw.Draw(sheet)
for idx,n in enumerate(names):
 raw=Image.open(P/f'raw/{n}.png').convert('RGBA');im=largest_component_alpha(raw.copy());bb=im.getchannel('A').getbbox();im=im.crop(bb);size=(384,320) if n.startswith('world') else (320,288);im.thumbnail((size[0]-12,size[1]-12),Image.Resampling.LANCZOS);out=Image.new('RGBA',size);out.alpha_composite(im,((size[0]-im.width)//2,(size[1]-im.height)//2));out.save(ART/(n+'.png'))
 alpha=np.array(out.getchannel('A'));records[n]={'file':f'PerchSort/Assets/Resources/Art/{n}.png','size':list(size),'raw_source':f'art/approved_ui/runtime/map/raw/{n}.png','source_alpha_bounds':list(bb),'alpha_bounds':list(out.getchannel('A').getbbox()),'alpha_extrema':[int(alpha.min()),int(alpha.max())],'partial_alpha_pixels':int(((alpha>0)&(alpha<255)).sum()),'sha256':hashlib.sha256((ART/(n+'.png')).read_bytes()).hexdigest(),'processing':'Largest connected generated-alpha component above 16 retained with one source-pixel antialias envelope; original RGBA colors preserved; trim, proportional Lanczos resize, transparent pad.'}
 x=idx%4*400;y=idx//4*380;sheet.paste(out,(x+(400-size[0])//2,y+15),out);ds.text((x+10,y+345),n,fill='white')
 # Overlay each sprite at target-like scale on the actual approved concept.
 bg=Image.open(ROOT/'art/approved_ui/ui_map.png').convert('RGBA');thumb=out.copy()
 if n.startswith('world'):thumb.thumbnail((300,250));pos=(770,1080)
 else:thumb.thumbnail((210,190));pos=(285,1395)
 bg.alpha_composite(thumb,pos);bg.convert('RGB').resize((540,960)).save(P/f'qa/{n}_over_approved.jpg')
sheet.save(P/'qa/sprite_sheet_dark.jpg')
light=Image.new('RGB',(7*150,240),'white');d=ImageDraw.Draw(light)
for idx,n in enumerate(names):
 im=Image.open(ART/(n+'.png'));im.thumbnail((64,64));x=idx*150;light.paste(im,(x+43,12),im);g=ImageOps.grayscale(im);gr=Image.merge('RGBA',(g,g,g,im.getchannel('A')));light.paste(gr,(x+43,92),gr);d.text((x+5,174),n.replace('world_sign_','sign_').replace('node_',''),fill='black')
light.save(P/'qa/sprites_64px_color_gray.png');(P/'sprites.json').write_text(json.dumps(records,indent=2)+'\n')
print([(n,r['alpha_bounds'],r['alpha_extrema']) for n,r in records.items()])
