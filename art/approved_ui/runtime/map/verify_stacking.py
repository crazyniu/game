"""Verify delivered RGBA backgrounds with ordinary alpha compositing only."""
from PIL import Image,ImageDraw
from pathlib import Path
import json
P=Path(__file__).resolve().parent;ROOT=P.parents[3];ART=ROOT/'PerchSort/Assets/Resources/V7/Art'
pairs=[(2,1),(3,2),(4,3),(1,1),(2,2),(3,3),(4,4)];sheet=Image.new('RGB',(1080,len(pairs)*420),'white');checks=[]
for idx,(upper,lower) in enumerate(pairs):
 up=Image.open(ART/f'map_bg_w{upper}.png');lo=Image.open(ART/f'map_bg_w{lower}.png');canvas=Image.new('RGBA',(1080,5840));canvas.alpha_composite(up,(0,0));canvas.alpha_composite(lo,(0,2840));crop=canvas.crop((0,2600,1080,3240));alpha=crop.getchannel('A');rgb=crop.convert('RGB').resize((675,400));sheet.paste(rgb,(0,idx*420));ImageDraw.Draw(sheet).text((700,idx*420+10),f'w{upper} ABOVE w{lower}\nRGBA alpha_composite\nstride 2840\n160px baked fade\njoin alpha {alpha.getextrema()}',fill='black');checks.append({'upper':upper,'lower':lower,'overlap_rows':[2840,2999],'join_alpha_extrema':list(alpha.getextrema()),'output_alpha_top_row':list(canvas.getchannel('A').crop((0,0,1080,1)).getextrema()),'status':'static_composite_reviewed'})
sheet.save(P/'qa/final_rgba_all_joins.jpg');(P/'stacking_verified.json').write_text(json.dumps({'compositing':'Pillow Image.alpha_composite on delivered final RGBA files, no special blending shader or QA-only ramp','pairs':checks},indent=2)+'\n')
print(checks)
