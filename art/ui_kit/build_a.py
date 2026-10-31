"""Pack image_gen art and render nine-slice review sheets; no procedural artwork.

Run: uv run --with pillow python art/ui_kit/build_a.py
"""
import json
import shutil
from pathlib import Path
from PIL import Image, ImageChops, ImageFilter, ImageFont, ImageDraw, ImageOps

ROOT = Path(__file__).resolve().parent
SPECS = {
    'panel_wood': ((512, 512), (80, 80, 80, 80), '弹窗/设置木框和奶油内纸'),
    'paper_inner': ((512, 512), (40, 40, 40, 40), '独立奶油纸内底'),
    'btn_primary': ((512, 132), (70, 70, 16, 16), '主操作树叶绿胶囊按钮'),
    'btn_secondary': ((512, 112), (60, 60, 14, 14), '次操作奶油胶囊按钮'),
    'btn_round': ((224, 224), (0, 0, 0, 0), '圆形木底图标按钮；等比显示112px'),
    'toggle_track_on': ((160, 80), (40, 40, 18, 18), '开启藤编滑轨'),
    'toggle_track_off': ((160, 80), (40, 40, 18, 18), '关闭藤编滑轨'),
    'toggle_knob': ((80, 80), (0, 0, 0, 0), '奶油蛋形开关旋钮'),
    'slider_track': ((512, 48), (26, 26, 12, 12), '木制滑条底轨'),
    'slider_fill': ((512, 48), (26, 26, 12, 12), '树叶绿滑条填充；显示于轨道内槽'),
    'slider_knob': ((96, 96), (0, 0, 0, 0), '叶片滑条旋钮'),
    'hud_bar': ((1024, 160), (96, 96, 24, 24), '顶部HUD木牌'),
    'tip_card': ((768, 512), (90, 90, 100, 100), '纸卷轴引导卡'),
    'vine_divider': ((1024, 48), (0, 0, 0, 0), '横向重复平铺藤条分隔'),
}


def resize(im, size):
    return im.convert('RGBa').resize(size, Image.Resampling.LANCZOS).convert('RGBA')


def clean_and_crop(im):
    # Preserve every antialias pixel within 2 source pixels of the main silhouette.
    a = im.getchannel('A')
    keep = a.point(lambda p: 255 if p > 8 else 0).filter(ImageFilter.MaxFilter(5))
    im.putalpha(ImageChops.multiply(a, keep))
    return im.crop(im.getbbox())


def nine_slice(im, size, margins):
    left, right, top, bottom = margins
    w, h = im.size
    tw, th = size
    sx, sy = [0, left, w-right, w], [0, top, h-bottom, h]
    dx, dy = [0, left, tw-right, tw], [0, top, th-bottom, th]
    out = Image.new('RGBA', size)
    for row in range(3):
        for col in range(3):
            patch = im.crop((sx[col], sy[row], sx[col+1], sy[row+1]))
            patch = resize(patch, (dx[col+1]-dx[col], dy[row+1]-dy[row]))
            out.paste(patch, (dx[col], dy[row]))
    return out


sources = json.loads((ROOT/'sources_a.json').read_text())
(ROOT/'source_a').mkdir(exist_ok=True)
assets = {}
for entry in sources:
    name = entry['name']
    original = ROOT/'source_a'/f'{name}.png'
    if not original.exists():
        shutil.copyfile(entry['path'], original)
    entry['path'] = str(original.relative_to(ROOT))
    im = clean_and_crop(Image.open(original).convert('RGBA'))
    size, margins, purpose = SPECS[name]
    w, h = size
    out = Image.new('RGBA', size)
    if name == 'vine_divider':
        # Mirror a generated segment to give identical joining edge pixels.
        half = resize(im, (w//2, h-4))
        out.paste(half, (0, 2))
        out.paste(ImageOps.mirror(half), (w//2, 2))
    elif name == 'btn_round':
        out.paste(resize(im, (w-10, h-10)), (5, 2))
    else:
        bottom = 8 if name.startswith('btn_') else 2
        out.paste(resize(im, (w-4, h-2-bottom)), (2, 2))
    out.save(ROOT/f'{name}.png')
    assets[name] = {'file': f'{name}.png', 'width': w, 'height': h,
                    'mode': 'RGBA', 'nine_slice': dict(zip(('L','R','T','B'), margins)),
                    'usage': purpose,
                    'resize_mode': 'tile_x' if name == 'vine_divider' else ('nine_slice' if any(margins) else 'uniform'),
                    'source': entry['path']}
    if name.startswith('btn_'):
        pressed = Image.new('RGBA', size)
        dark = out.copy()
        dark.putdata([(round(r*.92), round(g*.92), round(b*.92), a) for r,g,b,a in out.getdata()])
        pressed.paste(dark, (0, 6))
        pressed.save(ROOT/f'{name}_pressed.png')
        assets[name+'_pressed'] = {**assets[name], 'file': f'{name}_pressed.png',
                                  'usage': purpose+'按下态', 'derived_from': name,
                                  'translation_px': [0,6], 'rgb_multiplier': .92}
(ROOT/'sources_a.json').write_text(json.dumps(sources, ensure_ascii=False, indent=2)+'\n')

manifest = {'version': 1, 'task': 'T11b1', 'generator': 'built-in image_gen',
            'units': 'source PNG pixels', 'slice_order': 'L/R/T/B; measured inward from full canvas edges',
            'alpha': 'straight RGBA; resizing uses premultiplied alpha',
            'pressed': '6px down, RGB multiplied by 0.92; no extra runtime displacement needed',
            'round_button': 'Coordinator confirmed uniform 112/168/224px preview; never stretch horizontally',
            'panel_corner_radius_target': 48,
            'preview_widths': [320,512,720], 'assets': assets}
(ROOT/'manifest_a.json').write_text(json.dumps(manifest, ensure_ascii=False, indent=2)+'\n')

fontpath = '/System/Library/Fonts/Supplemental/Arial.ttf'
font = ImageFont.truetype(fontpath, 22)
title = ImageFont.truetype(fontpath, 34)
images = {name: Image.open(ROOT/a['file']).convert('RGBA') for name,a in assets.items()}

def make_contact(theme, bg, fg):
    sheet = Image.new('RGBA', (1720, 2080), bg)
    draw = ImageDraw.Draw(sheet)
    def label(x,y,text,big=False):
        draw.text((x,y),text,font=title if big else font,fill=fg)
    def put(name,x,y,size=None):
        im = images[name]
        if size:
            margins = tuple(assets[name]['nine_slice'].values())
            im = nine_slice(im,size,margins) if any(margins) else resize(im,size)
        sheet.alpha_composite(im,(x,y))
    label(32,24,'PERCH SORT / UI KIT A / '+theme.upper(),True)
    label(32,72,'Generated artwork - real alpha - nine-slice corners preserved at source size')
    for name,x,size in [('panel_wood',32,(512,512)),('paper_inner',580,(360,512)),('tip_card',976,(712,512))]:
        label(x,112,name)
        put(name,x,146,size)
    label(32,680,'hud_bar / 1024 x 160 source -> 1656 x 160 nine-slice')
    put('hud_bar',32,716,(1656,160))
    widths = [320,512,720]
    xs = [32,384,928]
    for name,y in [('btn_primary',924),('btn_primary_pressed',1104),('btn_secondary',1284),('btn_secondary_pressed',1444)]:
        for x,w in zip(xs,widths):
            label(x,y-30,f'{name} / {w}px')
            put(name,x,y,(w,images[name].height))
    label(32,1588,'Round / normal + pressed / uniform 112, 168, 224px')
    x=32
    for name in ['btn_round','btn_round_pressed']:
        for size in [112,168,224]:
            put(name,x,1632+(224-size)//2,(size,size))
            x += size+16
    label(1160,1588,'Toggle on / off + egg knob')
    put('toggle_track_on',1160,1630)
    put('toggle_knob',1238,1630)
    put('toggle_track_off',1360,1630)
    put('toggle_knob',1362,1630)
    label(1160,1732,'Slider track / fill / leaf knob')
    put('slider_track',1160,1774)
    fill = nine_slice(images['slider_fill'],(308,48),(26,26,12,12))
    sheet.alpha_composite(resize(fill,(290,28)),(1171,1784))
    put('slider_knob',1408,1750)
    label(32,1890,'vine_divider / two repeated 1024px tiles shown at 75% / seam at x800')
    vine = images['vine_divider']
    for x in (32,800):
        sheet.alpha_composite(resize(vine,(768,36)),(x,1934))
    label(32,2000,'All control textures contain no lettering. Labels exist only on this review sheet.')
    sheet.save(ROOT/f'contact_a_{theme}.png')

make_contact('light','#F4EBD7','#493522')
make_contact('dark','#1E2B24','#F4EBD7')
print(f'Packed {len(assets)} RGBA assets and two review sheets.')
