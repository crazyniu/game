"""Compose exported sprites at 64px visible width for visual recognition review."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parents[4]
BIRDS = ROOT / 'PerchSort/Assets/Resources/V7/Birds'
QA = ROOT / 'art/approved_ui/birds/qa'
SPECIES = 'sunny sky berry mint cocoa snow ember ink hoot crow'.split()
sheet = Image.new('RGB', (1000, 260), '#e9e5df')
draw = ImageDraw.Draw(sheet)
for i, species in enumerate(SPECIES):
    sprite = Image.open(BIRDS / f'{species}_idle.png').convert('RGBA')
    box = sprite.getchannel('A').point(lambda a: 255 if a > 12 else 0).getbbox()
    crop = sprite.crop(box)
    thumb = crop.resize((64, round(crop.height * 64 / crop.width)), Image.Resampling.LANCZOS)
    grey = ImageOps.grayscale(thumb).convert('RGBA')
    grey.putalpha(thumb.getchannel('A'))
    for row, image in enumerate((thumb, grey)):
        x, y = i * 100 + 18, row * 125 + 100 - image.height
        sheet.paste(image, (x, y), image)
        draw.text((i * 100 + 18, row * 125 + 107), species, fill='#302e2c')
sheet.save(QA / 'all_species_64px_grayscale.png')
