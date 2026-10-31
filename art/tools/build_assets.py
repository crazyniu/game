#!/usr/bin/env python3
"""Package the generated masters and rebuild the art review images (Pillow)."""
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps

ROOT = Path(__file__).resolve().parents[1]
BASELINE = 870
RESAMPLE = Image.Resampling.LANCZOS
NAMES = [
    ("CORAL CROWN", "Tall crest / dark cheek / single bar"),
    ("SKY BIB", "Upturned tail / cream bib / two bars"),
    ("GOLDEN CURL", "Curled crest / three round spots"),
    ("LEAF PARROT", "Hooked bill / long tail / diamond"),
    ("IVORY FAN", "Wide fan tail / pale body / heart"),
    ("ROSE NECK", "S neck / bent bill / crescent"),
    ("EMBER BRUSH", "Swept crest / chisel bill / ladder"),
    ("PLUM SCISSOR", "Low body / split tail / chevrons"),
    ("CHESTNUT DOT", "Round body / cheek dot / oval dashes"),
    ("TEAL SPEAR", "Large head / long bill / lightning"),
]


def clean_alpha(image):
    image = image.convert("RGBA")
    image.putalpha(image.getchannel("A").point(lambda a: 0 if a < 8 else a))
    return image


def resize_rgba(image, size):
    # Premultiplied alpha keeps hidden transparent RGB out of the edge colors.
    return clean_alpha(image.convert("RGBa").resize(size, RESAMPLE).convert("RGBA"))


def cutout(asset, size, bounds):
    image = clean_alpha(Image.open(ROOT / "source" / f"{asset}.png"))
    image = image.crop(image.getchannel("A").getbbox())
    x, y, max_width, max_height = bounds
    scale = min(max_width / image.width, max_height / image.height)
    image = resize_rgba(image, (round(image.width * scale), round(image.height * scale)))
    canvas = Image.new("RGBA", size)
    canvas.alpha_composite(image, (x + (max_width - image.width) // 2, y))
    return canvas


def color_sheet(birds, dark=False):
    canvas = Image.new("RGB", (1440, 780), "#263b37" if dark else "#fff8e8")
    draw = ImageDraw.Draw(canvas)
    ink = "#fff8e8" if dark else "#563f32"
    draw.text((26, 20), "PERCH SORT / ORIGINAL BIRD FAMILY", font=ImageFont.load_default(size=25), fill=ink)
    draw.text((26, 56), "10 species - shared light, contour and feet baseline", font=ImageFont.load_default(size=16), fill=ink)
    for i, bird in enumerate(birds):
        x, y = 20 + (i % 5) * 280, 96 + (i // 5) * 332
        draw.rounded_rectangle((x, y, x + 272, y + 316), radius=18, fill="#314b45" if dark else "#ffffff")
        thumb = resize_rgba(bird, (240, 240))
        canvas.paste(thumb, (x + 16, y + 8), thumb)
        draw.text((x + 16, y + 257), f"{i + 1:02d}  {NAMES[i][0]}", font=ImageFont.load_default(size=18), fill=ink)
        draw.text((x + 16, y + 286), NAMES[i][1], font=ImageFont.load_default(size=12), fill=ink)
    return canvas


def gray_sheet(birds, silhouette=False):
    canvas = Image.new("L", (500, 252), 245)
    draw = ImageDraw.Draw(canvas)
    title = "64px SILHOUETTES" if silhouette else "64px GRAYSCALE / 5 x 2"
    draw.text((14, 11), title, font=ImageFont.load_default(size=16), fill=45)
    for i, bird in enumerate(birds):
        x, y = 10 + (i % 5) * 96, 42 + (i // 5) * 100
        draw.rounded_rectangle((x, y, x + 92, y + 95), radius=7, fill=255, outline=222)
        small = resize_rgba(bird, (64, 64))
        gray = Image.new("L", (64, 64), 35) if silhouette else ImageOps.grayscale(small)
        canvas.paste(gray, (x + 14, y + 5), small.getchannel("A"))
        draw.text((x + 39, y + 77), f"{i + 1:02d}", font=ImageFont.load_default(size=12), fill=50)
    return canvas


def main():
    for directory in ("raw/birds", "raw/env", "raw/ui", "qa"):
        (ROOT / directory).mkdir(parents=True, exist_ok=True)
    manifest = {"birdFeetYFromTop": BASELINE, "alphaNoiseCutoff": 8, "assets": {}}
    birds = []
    for index in range(1, 11):
        key = f"bird_{index:02d}"
        master = clean_alpha(Image.open(ROOT / "source" / f"{key}.png"))
        box = master.getchannel("A").getbbox()
        # The parrot's tail extends below its feet; align the feet, not that tail.
        foot_y = 1110 if index == 4 else box[3] - 1
        foot_strip = master.getchannel("A").crop((500 if index == 4 else 0, foot_y - 44, master.width, foot_y + 1))
        feet = foot_strip.getbbox()
        foot_x = (feet[0] + feet[2]) / 2 + (500 if index == 4 else 0)
        cropped = master.crop(box)
        scale = min(800 / cropped.width, 740 / (foot_y - box[1] + 1))
        sprite = resize_rgba(cropped, (round(cropped.width * scale), round(cropped.height * scale)))
        x = (1024 - sprite.width) // 2
        foot_pixel = round((foot_y - box[1] + 0.5) * scale - 0.5) if index == 4 else sprite.getchannel("A").getbbox()[3] - 1
        y = BASELINE - foot_pixel
        canvas = Image.new("RGBA", (1024, 1024))
        canvas.alpha_composite(sprite, (x, y))
        canvas.save(ROOT / "raw/birds" / f"{key}.png", optimize=True)
        anchor_x = round(x + (foot_x - box[0]) * scale, 2)
        manifest["assets"][key] = {
            "file": f"raw/birds/{key}.png", "size": [1024, 1024],
            "alphaBounds": list(canvas.getchannel("A").getbbox()),
            "footCenterPixels": [anchor_x, BASELINE],
            "unityPivot": [round(anchor_x / 1024, 6), round((1024 - BASELINE) / 1024, 6)],
            "sourceFeetY": foot_y,
        }
        birds.append(canvas)

    perch = cutout("perch", (1024, 256), (32, 96, 960, 128))
    crown = cutout("tree_crown", (1536, 768), (48, 40, 1440, 688))
    perch.save(ROOT / "raw/env/perch.png", optimize=True)
    crown.save(ROOT / "raw/env/tree_crown.png", optimize=True)
    background = Image.open(ROOT / "source/background.png").convert("RGB").resize((1080, 1920), RESAMPLE)
    icon = Image.open(ROOT / "source/icon_1024.png").convert("RGB").resize((1024, 1024), RESAMPLE)
    background.save(ROOT / "raw/env/background.png", optimize=True)
    icon.save(ROOT / "raw/ui/icon_1024.png", optimize=True)
    for key, image, file in (
        ("perch", perch, "raw/env/perch.png"),
        ("tree_crown", crown, "raw/env/tree_crown.png"),
        ("background", background, "raw/env/background.png"),
        ("icon_1024", icon, "raw/ui/icon_1024.png"),
    ):
        manifest["assets"][key] = {"file": file, "size": list(image.size), "mode": image.mode}
    manifest["assets"]["perch"].update({"standingYFromTop": 105, "slotCentersX": [188, 404, 620, 836]})
    (ROOT / "manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n")

    color_sheet(birds).save(ROOT / "contact_sheet.png", optimize=True)
    gray_sheet(birds).save(ROOT / "contact_sheet_gray_64.png", optimize=True)
    gray_sheet(birds, silhouette=True).save(ROOT / "qa/silhouettes_64.png", optimize=True)
    edge_review = Image.new("RGB", (1440, 1560))
    edge_review.paste(color_sheet(birds), (0, 0))
    edge_review.paste(color_sheet(birds, dark=True), (0, 780))
    edge_review.save(ROOT / "qa/alpha_edges.png", optimize=True)

    # An art composition only, not a Unity runtime screenshot.
    composition = background.copy()
    crown_thumb = resize_rgba(crown, (960, 480))
    composition.paste(crown_thumb, (60, 80), crown_thumb)
    branch = resize_rgba(perch, (920, 230))
    for row, indices in enumerate(((0, 1, 2, 3), (4, 5, 6, 7), (8, 9))):
        stand_y = 810 + row * 370
        composition.paste(branch, (80, stand_y - round(105 * 920 / 1024)), branch)
        for col, index in enumerate(indices):
            thumb = resize_rgba(birds[index], (224, 224))
            feet_x = manifest["assets"][f"bird_{index + 1:02d}"]["footCenterPixels"][0]
            x = round(80 + (188 + col * 216) * 920 / 1024 - feet_x * 224 / 1024)
            y = round(stand_y - BASELINE * 224 / 1024)
            composition.paste(thumb, (x, y), thumb)
    composition.save(ROOT / "qa/composition.png", optimize=True)
    print("Built 14 production assets, two contact sheets, three art review images and manifest.json")


if __name__ == "__main__":
    main()
