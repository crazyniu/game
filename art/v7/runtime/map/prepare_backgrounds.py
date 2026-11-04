"""Resize generated backgrounds; measure their rendered paths; make QA only."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import numpy as np
import json,hashlib
P=Path(__file__).resolve().parent;ROOT=P.parents[3];ART=ROOT/'PerchSort/Assets/Resources/V7/Art'
result={'canvas':[1080,3000],'origin':'top_left','node_order':'ascending_level_bottom_to_top','stacking':{'stride_px':2840,'overlap_px':160,'alpha_top_fade':{'rows_inclusive':[0,159],'alpha_start':0,'alpha_end':255,'rule':'round(255*y/159)'},'bottom_alpha':255,'order':'Draw world N+1 above world N first; then alpha-composite lower world N at y=2840.','topmost_world':'Clamp the topmost visible viewport to local y>=160, or back the first 160px with its preserved opaque raw source. Do not expose the top fade to an empty background.'},'method':'At fixed sampling heights, detect the actual rendered tan path using source RGB thresholds, select its continuous interval nearest the previously measured path center, and take the interval midpoint. All 40 placements are visually reviewed on annotated composites. Sampling heights are layout choices; x coordinates and widths are image measurements, not a predefined curve.','worlds':{}}
for i in range(1,5):
 raw=Image.open(P/f'raw/w{i}.png');im=raw.convert('RGBA').resize((1080,3000),Image.Resampling.LANCZOS);alpha=Image.new('L',(1080,3000),255);fade=Image.new('L',(1080,160));fade.putdata([round(255*y/159) for y in range(160) for x in range(1080)]);alpha.paste(fade,(0,0));im.putalpha(alpha);im.save(ART/f'map_bg_w{i}.png');a=np.array(im.convert('RGB')).astype(float)
 mask=(a[:,:,0]>218)&(a[:,:,1]>155)&(a[:,:,2]>88)&(a[:,:,0]>a[:,:,1]*1.065)&((a[:,:,1]-a[:,:,2])>30)
 m=Image.fromarray((mask*255).astype('uint8')).filter(ImageFilter.MaxFilter(7)).filter(ImageFilter.MinFilter(7));a=np.array(m)>127
 curve=[];prior=540
 for y in range(0,3000,5):
  row=np.mean(a[max(0,y-2):min(3000,y+3)],axis=0)>.5;edges=np.diff(np.pad(row.astype(int),(1,1)));starts=np.where(edges==1)[0];ends=np.where(edges==-1)[0]
  runs=[(l,r) for l,r in zip(starts,ends) if r-l>90]
  l,r=min(runs,key=lambda q:abs((q[0]+q[1])/2-prior));prior=(l+r)/2;curve.append({'y':y,'left':int(l),'right':int(r),'center_x':round(prior,1)})
 nodes=[];qa=im.copy();d=ImageDraw.Draw(qa)
 for idx,y in enumerate(range(2700,359,-260),1):
  c=curve[y//5];x=round(c['center_x']);nodes.append({'index':idx,'center':[x,y],'measured_path_edges':[c['left'],c['right']],'measured_path_width':c['right']-c['left'],'recommended_node_size':[180,150]})
  d.ellipse((x-90,y-75,x+90,y+75),outline='magenta',width=5);d.line((c['left'],y,c['right'],y),fill='red',width=3);d.text((x-5,y-8),str(idx),fill='black')
 d.line([(c['center_x'],c['y']) for c in curve],fill=(255,0,255),width=2)
 qa.resize((360,1000)).save(P/f'qa/w{i}_measured_paths.png')
 result['worlds'][f'w{i}']={'file':f'PerchSort/Assets/Resources/V7/Art/map_bg_w{i}.png','source_generated':f'art/v7/runtime/map/raw/w{i}.png','sha256':hashlib.sha256((ART/f'map_bg_w{i}.png').read_bytes()).hexdigest(),'nodes':nodes,'path_samples':curve[::10],'top_anchor':{'center':[curve[0]['center_x'],0],'edges':[curve[0]['left'],curve[0]['right']]},'bottom_anchor':{'center':[curve[-1]['center_x'],3000],'edges':[curve[-1]['left'],curve[-1]['right']]}}
(P/'world_paths.json').write_text(json.dumps(result,indent=2)+'\n')
# Stack direction: later world sits above earlier world. Compare hard joins and 160 px overlaps.
metrics=[];joined=Image.new('RGB',(1080*2,600*3),'#333333')
for i in range(1,4):
 low=Image.open(ART/f'map_bg_w{i}.png').convert('RGB');up=Image.open(ART/f'map_bg_w{i+1}.png').convert('RGB');top=up.crop((0,2700,1080,3000));bottom=low.crop((0,0,1080,300));hard=Image.new('RGB',(1080,600));hard.paste(top,(0,0));hard.paste(bottom,(0,300));joined.paste(hard,(0,(i-1)*600))
 n=160;blend=Image.new('RGB',(1080,600));blend.paste(up.crop((0,2620,1080,3000)),(0,0));blend.paste(low.crop((0,0,1080,380)),(0,220));m=Image.new('L',(1080,n));m.putdata([round(255*y/(n-1)) for y in range(n) for x in range(1080)]);over=Image.composite(low.crop((0,0,1080,n)),up.crop((0,3000-n,1080,3000)),m);blend.paste(over,(0,220));joined.paste(blend,(1080,(i-1)*600))
 mae=float(np.abs(np.array(up)[-1].astype(float)-np.array(low)[0].astype(float)).mean());metrics.append({'lower_world':i,'upper_world':i+1,'hard_edge_rgb_mae':round(mae,3),'overlap_test_px':160})
joined.resize((1080,900)).save(P/'qa/joins_hard_left_overlap160_right.jpg')
(P/'join_metrics.json').write_text(json.dumps({'direction':'world N+1 above world N','hard_edge_exact_match':False,'pairs':metrics},indent=2)+'\n')
print('measured',[(k,[n['center'] for n in v['nodes']]) for k,v in result['worlds'].items()]);print(metrics)
