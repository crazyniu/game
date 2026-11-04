"""World-only static compositions at measured approved board heights; no drawn art."""
from pathlib import Path
from PIL import Image,ImageOps,ImageDraw
import json
P=Path(__file__).resolve().parent;ROOT=P.parents[3];W=ROOT/'PerchSort/Assets/Resources/World';V=ROOT/'art/approved_ui'
layout=json.loads((V/'layout.json').read_text())
for world,key,variant,ref in [('w1','level_k3','noon','ui_level_k3.png'),('w4','level_k7','aurora','ui_level_k7.png')]:
    meta=json.loads((V/f'world/pieces/{world}/metadata.json').read_text())['assets']
    out=Image.open(W/f'bg_level_{world}_{variant}.png').convert('RGBA')
    trunk=Image.open(W/f'trunk_left_{world}.png').resize((80,1920))
    # Start below crown; keep trunk proportions and clip excess beyond bottom.
    for side in ['left','right']:
        x=0 if side=='left' else 1000
        out.alpha_composite(trunk if side=='left' else ImageOps.mirror(trunk),(x,650))
    crown=Image.open(W/f'tree_crown_{world}.png').resize((400,267))
    out.alpha_composite(crown,(-10,470));out.alpha_composite(ImageOps.mirror(crown),(690,470))
    for b in layout['screens'][key]['branches']:
        (x0,y0),(x1,y1)=b['perch_line'];width=x1-x0;scale=width/620
        branch=Image.open(W/f'branch_left_{world}.png').resize((width,round(90*scale)))
        if b['side']=='right':branch=ImageOps.mirror(branch)
        top=round((y0+y1)/2-meta['branch']['flat_top_y']*scale)
        out.alpha_composite(branch,(x0,top))
    if world=='w4':
        twig=ImageOps.mirror(Image.open(W/'twig_left_w4.png')).resize((346,69))
        out.alpha_composite(twig,(734,round(715-meta['twig']['flat_top_y']*.865)))
    out.save(P/f'{world}_world_only.png')
    sheet=Image.new('RGB',(1080,984),'white');d=ImageDraw.Draw(sheet)
    sheet.paste(Image.open(V/ref).convert('RGB').resize((540,960)),(0,24));sheet.paste(out.convert('RGB').resize((540,960)),(540,24))
    d.text((8,6),'Approved screen',fill='black');d.text((548,6),'New world-only static composition (no birds/HUD)',fill='black')
    sheet.save(P/f'{world}_comparison.png')
