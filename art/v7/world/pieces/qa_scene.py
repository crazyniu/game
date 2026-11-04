from PIL import Image
from pathlib import Path
import sys,json
world=sys.argv[1]
variant=sys.argv[2]
b=Path('PerchSort/Assets/Resources/V7/World')
meta=json.loads((Path(__file__).parent/world/'metadata.json').read_text())
bg=Image.open(b/f'bg_level_{world}_{variant}.png').convert('RGBA')
c=Image.open(b/f'tree_crown_{world}.png').resize((400,267),Image.Resampling.LANCZOS)
attach=meta['assets']['crown']['attachment_pixel']
cx=round((40 if world=='w2' else 70)-attach[0]*2/3)
cy=470
ty=round(cy+attach[1]*2/3-(65 if world=='w2' else 25))
t=Image.open(b/f'trunk_left_{world}.png').crop((0,0,160,1920-ty))
bg.alpha_composite(t,(-70,ty))
bg.alpha_composite(t.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(990,ty))
bg.alpha_composite(c,(cx,cy))
bg.alpha_composite(c.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(1080-cx-400,cy))
br=Image.open(b/f'branch_left_{world}.png')
for x,y in [(0,920),(460,1100),(0,1280),(460,1460)]:
    bg.alpha_composite(br if x==0 else br.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(x,y))
bg.save(Path(__file__).parent/world/'qa_scene.png')
