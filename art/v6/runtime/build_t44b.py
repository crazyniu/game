"""Size built-in generated T44b artwork; never draw replacement pictograms."""
import json
import shutil
import hashlib
from pathlib import Path
from PIL import Image, ImageOps, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
WORK = ROOT / 'art/v6/runtime'
DEST = ROOT / 'PerchSort/Assets/Resources/V6/Art'
SOURCE = WORK / 't44b_sources'
SOURCE.mkdir(exist_ok=True)
jobs = json.loads((WORK / 't44b_generation.json').read_text())
metrics = []
for job in jobs:
    name = job['name']
    source = SOURCE / (name + '.png')
    shutil.copyfile(job['source'], source)
    original = Image.open(source)
    assert original.mode == 'RGBA'
    assert original.getchannel('A').getextrema() == (0, 255)
    # Ignore nearly invisible generation specks when measuring visual bounds.
    bounds = original.getchannel('A').point(lambda alpha: 255 if alpha > 8 else 0).getbbox()
    cropped = original.crop((bounds[0]-3, bounds[1]-3, bounds[2]+3, bounds[3]+3))
    cropped.thumbnail((124, 124), Image.Resampling.LANCZOS)
    master = Image.new('RGBA', (128, 128))
    master.alpha_composite(cropped, ((128-cropped.width)//2, (128-cropped.height)//2))
    for size in ([128, 64] if name == 'ico_sound_off' else [128, 64, 28]):
        filename = name + ('' if size == 128 else '_' + str(size)) + '.png'
        path = DEST / filename
        output = master if size == 128 else master.resize((size, size), Image.Resampling.LANCZOS)
        output.save(path)
        metrics.append(dict(file=str(path.relative_to(ROOT)), size=list(output.size), mode=output.mode,
                            alpha_range=list(output.getchannel('A').getextrema()), bounds=list(output.getbbox()),
                            pivot=[0.5, 0.5], sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
(WORK / 't44b_manifest.json').write_text(json.dumps(metrics, indent=2) + '\n')
sheet = Image.new('RGBA', (640, 500), '#FFF8E8')
draw = ImageDraw.Draw(sheet)
draw.text((12, 12), 'T44b: 128 / 64 / 28 / grayscale 64; original speaker below', fill='#593B27')
for row, job in enumerate(jobs):
    y = 42 + row * 145
    name = job['name']
    draw.text((10, y), name, fill='#593B27')
    for x, size in [(145, 128), (300, 64), (390, 28)]:
        if size == 28 and name == 'ico_sound_off':
            continue
        path = DEST / (name + ('' if size == 128 else '_' + str(size)) + '.png')
        icon = Image.open(path)
        sheet.alpha_composite(icon, (x, y+12))
    icon = Image.open(DEST / (name + '_64.png'))
    gray = ImageOps.grayscale(icon).convert('RGBA')
    gray.putalpha(icon.getchannel('A'))
    sheet.alpha_composite(gray, (465, y+12))
sheet.alpha_composite(Image.open(DEST / 'ico_sound_on_64.png'), (550, 344))
sheet.convert('RGB').save(WORK / 't44b_qa_cream.png')
print(json.dumps(metrics, indent=2))
