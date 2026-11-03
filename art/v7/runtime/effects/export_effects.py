"""Resize generated sprites and build static QA; never synthesizes artwork."""
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw
import json, hashlib
root=Path(__file__).resolve().parents[4]
work=root/'art/v7/runtime/effects'
out=root/'PerchSort/Assets/Resources/V7/Art'
records=json.loads((work/'generation_records.json').read_text())
delivery=[]
thumbs=[]
for r in records:
    name=r['name']; im=Image.open(work/'raw'/f'{name}.png').convert('RGBA')
    size=r['size']; target=(size,size)
    if name.startswith('cloud_soft'): target=(768,384)
    # Preserve all alpha, including faint additive light. Uniform scale and transparent padding only.
    sprite=ImageOps.contain(im,target,Image.Resampling.LANCZOS)
    final=Image.new('RGBA',target,(0,0,0,0))
    final.paste(sprite,((target[0]-sprite.width)//2,(target[1]-sprite.height)//2))
    path=out/f'{name}.png';final.save(path)
    a=final.getchannel('A');hist=a.histogram()
    bg=Image.open(root/'art/v7'/f"{r['ref']}.png").convert('RGBA')
    if name=='gate_light': pos=(660,400); dims=(450,530)
    elif name=='house_glow': pos=(525,298);dims=(150,150)
    elif name=='leaf_wipe':pos=(540,960);dims=(1080,1080)
    elif name=='light_rays':pos=(540,960);dims=(1000,1000)
    elif name=='gift_trail':pos=(540,600);dims=(300,450)
    elif name.startswith('cloud'):pos=(540,600);dims=(650,325)
    elif size<=192:pos=(540,600);dims=target
    else:pos=(540,600);dims=(350,350)
    over=ImageOps.contain(final,dims,Image.Resampling.LANCZOS)
    bg.alpha_composite(over,(pos[0]-over.width//2,pos[1]-over.height//2))
    bg.convert('RGB').save(work/'qa'/f'{name}_overlay.jpg',quality=90)
    panel=Image.new('RGB',(400,320),'#d9d9d9');draw=ImageDraw.Draw(panel)
    draw.text((10,8),name,fill='black')
    view=ImageOps.contain(final,(180,240),Image.Resampling.LANCZOS)
    panel.paste(Image.new('RGB',(200,280),'#172a39'),(200,40))
    panel.paste(view,(10+(180-view.width)//2,45+(240-view.height)//2),view)
    panel.paste(view,(210+(180-view.width)//2,45+(240-view.height)//2),view)
    thumbs.append(panel)
    delivery.append(dict(file=str(path.relative_to(root)),size=list(target),mode='RGBA',alpha_min=min(i for i,v in enumerate(hist) if v),alpha_max=max(i for i,v in enumerate(hist) if v),transparent_pixels=hist[0],partial_alpha_pixels=sum(hist[1:255]),pivot_normalized=[0.5,0.5],reference=f"art/v7/{r['ref']}.png",source=f'art/v7/runtime/effects/raw/{name}.png',prompt_record='art/v7/runtime/effects/generation_records.json',sha256=hashlib.sha256(path.read_bytes()).hexdigest(),qa_overlay=f'art/v7/runtime/effects/qa/{name}_overlay.jpg'))
sheet=Image.new('RGB',(1600,320*((len(thumbs)+3)//4)), 'white')
for i,thumb in enumerate(thumbs):sheet.paste(thumb,((i%4)*400,(i//4)*320))
sheet.save(work/'qa'/'effects_contact.jpg',quality=94)
(work/'delivery.json').write_text(json.dumps(delivery,indent=2)+'\n')
print(f'Exported {len(delivery)} RGBA sprites, {len(delivery)} overlays and contact sheet.')
