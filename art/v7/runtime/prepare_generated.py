"""Trim and resize generated sprite art; never synthesize artwork."""
import json
import shutil
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
WORK = ROOT / 'art/v7/runtime'
OUT = ROOT / 'PerchSort/Assets/Resources/V7/Art'
SIZES = {'btn_purple': (160,160), 'btn_purple_pressed': (160,160),
         'btn_green': (220,220), 'btn_green_pressed': (220,220),
         'coin_pill': (320,128), 'btn_green_pill': (320,112), 'badge_red': (56,56), 'chain_bonus': (64,64)}

if __name__ == '__main__':
    metrics = {}
    for item in json.loads((WORK/'generation_manifest.json').read_text()):
        name = item['name']
        source = Path(item['source'])
        raw = WORK/'raw'/f'{name}.png'
        shutil.copy2(source, raw)
        im = Image.open(raw).convert('RGBA')
        alpha = im.getchannel('A')
        # Remove nearly transparent generation specks before measuring bounds.
        alpha = alpha.point(lambda p: 0 if p < 8 else p)
        if name in ('icon_branch_plus', 'icon_check', 'icon_next_arrow', 'chain_bonus'):
            alpha = alpha.point(lambda p: 0 if p < 160 else round((p-160)*255/95))
        im.putalpha(alpha)
        bounds = alpha.point(lambda p: 255 if p >= 32 else 0).getbbox()
        im = im.crop(bounds)
        size = SIZES.get(name, (96,96))
        padding = 3 if min(size) >= 96 else 2
        im.thumbnail((size[0]-padding*2,size[1]-padding*2),Image.Resampling.LANCZOS)
        canvas = Image.new('RGBA',size)
        canvas.alpha_composite(im,((size[0]-im.width)//2,(size[1]-im.height)//2))
        canvas.save(OUT/f'{name}.png')
        metrics[name] = {'size':size,'source_bounds':bounds,'alpha_bounds':canvas.getchannel('A').getbbox()}
    (WORK/'generated_metrics.json').write_text(json.dumps(metrics,indent=2)+'\n')
