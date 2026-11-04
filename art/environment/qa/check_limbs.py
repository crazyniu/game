from pathlib import Path
from PIL import Image, ImageDraw, ImageFont
import json, hashlib, re

ROOT = Path(__file__).resolve().parents[3]
WORLD = ROOT / 'PerchSort/Assets/Resources/World'
QA = ROOT / 'art/environment/qa'
EVIDENCE = ROOT / 'docs/work-log/environment-art/evidence'
FONT = ImageFont.truetype('/System/Library/Fonts/Helvetica.ttc', 20)

def label(im, xy, text, color='#20303b'):
    ImageDraw.Draw(im).text(xy, text, font=FONT, fill=color)

checks = []
for n in range(1, 5):
    sheet = Image.new('RGB', (1320, 390), '#f2f3ed')
    label(sheet, (18, 10), f'World {n}: exact canvas / light and dark / 64 px width')
    for row, kind in enumerate(('branch', 'twig')):
        p = WORLD / f'{kind}_limb_w{n}.png'
        im = Image.open(p).convert('RGBA')
        old = Image.open(WORLD / f'{kind}_left_w{n}.png')
        assert im.size == old.size
        meta = p.with_suffix('.png.meta').read_text()
        oldmeta = (WORLD / f'{kind}_left_w{n}.png.meta').read_text()
        pivot = re.search(r'spritePivot: .*', meta).group()
        assert pivot == re.search(r'spritePivot: .*', oldmeta).group()
        y = 75 + row * 155
        label(sheet, (18, y-30), p.stem)
        sheet.paste(im, (18, y), im)
        ImageDraw.Draw(sheet).rectangle((670, y-8, 1305, y+100), fill='#142530')
        sheet.paste(im, (680, y), im)
        tiny = im.copy(); tiny.thumbnail((64, 64), Image.Resampling.LANCZOS)
        sheet.paste(tiny, (565, y+102), tiny)
        alpha = im.getchannel('A')
        checks.append(dict(asset=p.name, canvas=im.size, pivot=pivot,
                           transparent_pixels=alpha.histogram()[0],
                           sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
    sheet.save(EVIDENCE / f'limb_assets_w{n}.jpg', quality=93)
    board = Image.new('RGBA', (780, 960), '#c4dfe8' if n != 4 else '#bbbde3')
    trunk = Image.open(WORLD / f'trunk_curved_w{n}.png').convert('RGBA').crop((0, 960, 380, 1920))
    joint = Image.open(WORLD / f'branch_junction_w{n}.png').convert('RGBA')
    for y, kind, count in ((300, 'branch', 4), (680, 'twig', 2)):
        limb = Image.open(WORLD / f'{kind}_limb_w{n}.png').convert('RGBA')
        board.alpha_composite(joint, (45, y-55))
        board.alpha_composite(limb, (110, y))
        for i in range(count):
            bird = Image.open(ROOT / f'PerchSort/Assets/Resources/Birds/{("sunny", "sky", "berry")[i % 3]}_idle.png').convert('RGBA')
            bird = bird.crop(bird.getchannel('A').getbbox())
            bird.thumbnail((120, 160), Image.Resampling.LANCZOS)
            board.alpha_composite(bird, (195+i*120, y+18-bird.height))
    board.alpha_composite(trunk)
    label(board, (205, 40), 'OFFLINE ASSET ASSEMBLY - NOT RUNTIME')
    label(board, (205, 75), 'Bird reference: 120 px bounding width')
    label(board, (205, 900), 'Trunk drawn over overlapping limb root')
    board.convert('RGB').save(QA / f'limb_assembly_w{n}.png')
    ref = Image.open(ROOT / f'art/approved_ui/ui_level_k{7 if n == 4 else 3}.png').convert('RGB')
    ref = ref.resize((540, 960), Image.Resampling.LANCZOS)
    pair = Image.new('RGB', (1320, 1000), '#f2f3ed')
    pair.paste(ref, (0, 40)); pair.paste(board.convert('RGB'), (540, 40))
    label(pair, (12, 10), 'APPROVED BOARD'); label(pair, (555, 10), f'WORLD {n} - FULL PIXEL SIZE ASSEMBLY')
    pair.save(EVIDENCE / f'limb_reference_w{n}.jpg', quality=92)
(QA / 'limb_checks.json').write_text(json.dumps(checks, indent=2) + '\n')
print('Checked 8 exact canvases and pivots; wrote 8 JPEG evidence sheets.')
