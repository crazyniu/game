"""Trim, resize, slice and inspect generated art. No artwork is drawn here."""
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import json, shutil

ROOT = Path(__file__).resolve().parents[4]
HERE = Path(__file__).resolve().parent
OUT = ROOT / 'PerchSort/Assets/Resources/V7/Art'
specs = json.loads((HERE/'specs.json').read_text())
sources = json.loads((HERE/'sources.json').read_text())
placements = {
 'panel_wood': (130,840,800,510), 'card_wood':(70,450,470,430),
 'ribbon_green':(265,840,530,160), 'leaf_badge':(425,485,220,145),
 'name_ribbon':(210,495,290,90),'stat_pill':(360,738,355,70),
 'aviary_frame':(200,255,310,310),'aviary_board':(40,105,1000,1790),
 'silhouette_frame':(188,1408,320,320),'house_count_chip':(285,553,145,64),
 'panel_rope_left':(230,650,42,225),'panel_rope_right':(805,650,42,225),
 'tab_selected_ring':(266,254,170,175)}
borders = {'panel_wood':[150,100,150,110], 'card_wood':[120,115,120,120],
 'aviary_board':[160,220,160,160], 'name_ribbon':[82,38,82,38],
 'stat_pill':[48,42,48,42], 'house_count_chip':[83,33,35,33]}
delivery=[]
for name, ref, prompt, size in specs:
 if name not in sources: continue
 source=Path(sources[name]); raw=HERE/'raw'/f'{name}.png'
 shutil.copyfile(source,raw)
 im=Image.open(raw).convert('RGBA')
 alpha=im.getchannel('A'); bbox=alpha.point(lambda a:255 if a>8 else 0).getbbox()
 im=im.crop(bbox)
 im=ImageOps.contain(im,(size[0]-8,size[1]-8),Image.Resampling.LANCZOS)
 out=Image.new('RGBA',size);out.alpha_composite(im,((size[0]-im.width)//2,(size[1]-im.height)//2))
 out.save(OUT/f'{name}.png')
 q=Image.open(ROOT/'art/approved_ui'/ref).convert('RGBA').resize((1080,1920))
 x,y,w,h=placements[name];q.alpha_composite(out.resize((w,h),Image.Resampling.LANCZOS),(x,y))
 q.convert('RGB').save(HERE/'qa'/f'{name}_overlay.jpg',quality=90)
 # Contrasting surfaces reveal baked backgrounds and edge contamination.
 bg=Image.new('RGBA',(size[0]*2,size[1]),'#101b28'); light=Image.new('RGBA',size,'#e5e0d4')
 bg.alpha_composite(light,(size[0],0));bg.alpha_composite(out,(0,0));bg.alpha_composite(out,(size[0],0))
 bg.convert('RGB').save(HERE/'qa'/f'{name}_alpha.jpg',quality=90)
 record={'file':f'{name}.png','size':size,'reference':ref,'source':str(raw.relative_to(ROOT)),
 'alpha_extrema':out.getchannel('A').getextrema(),'qa_overlay':[x,y,w,h], 'pivot_normalized':[0.5,0.5]}
 if name in borders: record['nine_slice_lbrt']=borders[name]
 if 'rope' in name: record.update({'pivot_normalized':[0.5,1.0],'top_anchor_xy':[size[0]/2,4], 'board_attach_xy':[size[0]/2,size[1]-14], 'aligned_settings_board_attach_xy': [250 if name.endswith('left') else 826, 860]})
 delivery.append(record)
 if name=='ribbon_green':
  for suffix,box in [('left',(0,0,180,size[1])),('middle',(180,0,460,size[1])),('right',(460,0,640,size[1]))]:
   out.crop(box).save(OUT/f'ribbon_green_{suffix}.png')
   delivery.append({'file':f'ribbon_green_{suffix}.png','size':[box[2]-box[0],box[3]],'source_crop_xyxy':box,'stretch':suffix=='middle'})
(HERE/'delivery.json').write_text(json.dumps({'coordinate_system':'pixels top-left; nine-slice left,bottom,right,top; normalized pivots Unity bottom-left','assets':delivery},indent=2)+'\n')
tiles=[]
for item in delivery:
 im=Image.open(OUT/item['file']); tile=Image.new('RGB',(360,290),'#303640');im.thumbnail((344,250))
 tile.paste(im,((360-im.width)//2,4),im);ImageDraw.Draw(tile).text((8,267),item['file'],fill='white');tiles.append(tile)
sheet=Image.new('RGB',(360*4,290*((len(tiles)+3)//4)),'#303640')
for k,tile in enumerate(tiles):sheet.paste(tile,((k%4)*360,(k//4)*290))
sheet.save(HERE/'qa/contact_sheet.jpg',quality=92)
print(json.dumps({'delivered':len(delivery),'files':[x['file'] for x in delivery]}))
