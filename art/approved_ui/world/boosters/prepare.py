from pathlib import Path
import hashlib
import json
import shutil
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(__file__).resolve().parents[4]
BASE = Path(__file__).resolve().parent
SOURCE = Path('/Users/apple/.codex/generated_images/01a1a5bd-ba48-7962-87aa-ae4204ff1933')
OUTPUT = ROOT / 'PerchSort/Assets/Resources/Art'
RAW = {
    'hint': 'exec-dbf54c78-fbf3-4423-ad0a-6e7c0ae72bb7.png',
    'sapling': 'exec-3f983792-e8bc-4cb7-9d9f-d470509811e8.png',
    'owl_eye': 'exec-dd4183ab-e4f0-4844-87b0-8bcc02ebd20a.png',
    'bell': 'exec-28356667-925b-47da-98be-f713492e00bb.png',
    'scarecrow': 'exec-63c0caea-d876-44ca-942b-6b8c82febe66.png',
    'rain': 'exec-1efa88b3-7922-4be2-95e4-675d9b4444d3.png',
}
shutil.copy2(SOURCE / 'exec-cda3a542-3859-4593-8ab2-416f8891721d.png', BASE / 'raw/owl_eye_rejected_glow.png')
stats = []
images = {}
for name, filename in RAW.items():
    raw = BASE / 'raw' / f'{name}.png'
    shutil.copy2(SOURCE / filename, raw)
    im = Image.open(raw).convert('RGBA')
    bounds = im.getchannel('A').point(lambda value: 255 if value > 16 else 0).getbbox()
    im = im.crop(bounds)
    im.thumbnail((224, 224), Image.Resampling.LANCZOS)
    canvas = Image.new('RGBA', (256, 256))
    canvas.alpha_composite(im, ((256-im.width)//2, (256-im.height)//2))
    output = OUTPUT / f'booster_{name}_illus.png'
    canvas.save(output)
    images[name] = canvas
    alpha = canvas.getchannel('A')
    hist = alpha.histogram()
    stats.append({'name': name, 'raw_source': str(SOURCE / filename), 'output': str(output.relative_to(ROOT)), 'size': canvas.size, 'mode': canvas.mode, 'alpha_bounds': alpha.getbbox(), 'transparent_pixels': hist[0], 'partial_alpha_pixels': sum(hist[1:255]), 'opaque_pixels': hist[255], 'pivot_recommendation': [0.5, 0.5], 'sha256': hashlib.sha256(output.read_bytes()).hexdigest()})

# QA layouts only: labels/backgrounds are never included in production assets.
sheet = Image.new('RGB', (1536, 640), '#fff1d4')
draw = ImageDraw.Draw(sheet)
for i, (name, im) in enumerate(images.items()):
    x = i*256
    sheet.paste(im, (x, 20), im)
    draw.rectangle((x, 296, x+255, 575), fill='#202831')
    sheet.paste(im, (x, 306), im)
    draw.text((x+8, 4), name, fill='#252525')
    small = im.resize((64,64), Image.Resampling.LANCZOS)
    grey = ImageOps.grayscale(small).convert('RGBA')
    grey.putalpha(small.getchannel('A'))
    sheet.paste(small, (x+48, 576), small)
    sheet.paste(grey, (x+136, 576), grey)
sheet.save(BASE/'qa/contact_light_dark_64_grayscale.png')

# Static comparison of approved screen crops and new illustration composites.
reference = Image.open(ROOT/'art/approved_ui/ui_shop_boosters.png').convert('RGBA')
disc = Image.open(OUTPUT/'btn_purple.png').convert('RGBA').resize((256,256),Image.Resampling.LANCZOS)
centers = [(306,638),(790,638),(306,1080),(790,1080),(306,1510),(790,1510)]
comparison = Image.new('RGB',(1536,560),'#fff1d4')
overlay = reference.copy()
for i, ((name, im), (cx,cy)) in enumerate(zip(images.items(),centers)):
    crop = reference.crop((cx-128,cy-128,cx+128,cy+128))
    comparison.paste(crop,(i*256,0))
    composite = Image.new('RGBA',(256,256),'#fff1d4')
    composite.alpha_composite(disc)
    composite.alpha_composite(im)
    comparison.paste(composite,(i*256,280))
    # Rectangular QA patch masks the old illustration; it is not delivered UI art.
    patch = Image.new('RGBA',(300,280),'#ffe5b2')
    patch.alpha_composite(disc,(22,12))
    patch.alpha_composite(im,(22,12))
    overlay.alpha_composite(patch,(cx-150,cy-140))
comparison.save(BASE/'qa/approved_shop_comparison.png')
overlay.save(BASE/'qa/approved_shop_static_overlay.png')
unlock = Image.open(ROOT/'art/approved_ui/ui_unlock_card.png').convert('RGBA')
patch = Image.new('RGBA',(440,420),'#ffe5b2')
owl = images['owl_eye'].resize((420,420),Image.Resampling.LANCZOS)
patch.alpha_composite(owl,(10,0))
unlock.alpha_composite(patch,(320,650))
unlock.save(BASE/'qa/approved_unlock_static_overlay.png')
(BASE/'qa/metrics.json').write_text(json.dumps(stats,indent=2)+'\n')
print(json.dumps(stats,indent=2))
