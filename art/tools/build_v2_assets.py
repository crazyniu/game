#!/usr/bin/env python3
"""Export T10a generated masters, append manifest entries and make review sheets."""
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SPECS = [
    ('crate', 'env', (1024, 320), '整根栖木及上方鸟的木箱遮罩'),
    ('basket', 'env', (512, 384), '单鸟临时篮子'),
    ('nest', 'env', (512, 512), '顶部目标空巢，中央叠鸟图标'),
    ('nest_done', 'env', (512, 512), '同巢三蛋完成态'),
    ('zzz', 'fx', (256, 256), '睡眠 Z 气泡'),
    ('sparkle', 'fx', (256, 256), '四角星闪光'),
    ('check', 'fx', (256, 256), '绿色完成徽章'),
    ('feather', 'fx', (256, 256), '归巢飘落羽毛'),
    ('ring', 'fx', (256, 256), '柔和金色光环'),
    ('confetti_01', 'fx', (256, 256), '珊瑚红矩形纸片'),
    ('confetti_02', 'fx', (256, 256), '青色三角纸片'),
    ('confetti_03', 'fx', (256, 256), '淡紫卷曲纸片'),
    ('trophy', 'ui', (1024, 1024), '结算金奖杯'),
    ('star', 'ui', (256, 256), '五角金星奖励'),
    ('closed_eye', 'fx', (128, 128), '单条闭眼弧线；现有鸟优先使用睡眠版'),
] + [(f'bird_{i:02d}s', 'birds', (1024, 1024), f'第 {i} 种鸟的闭眼睡眠版') for i in range(1, 11)]


def clean(im):
    im = im.convert('RGBA')
    im.putalpha(im.getchannel('A').point(lambda a: 0 if a < 8 else a))
    return im


def resize(im, size):
    return clean(im.convert('RGBa').resize(size, Image.Resampling.LANCZOS).convert('RGBA'))


def fit(im, size):
    ratio = min(size[0] / im.width, size[1] / im.height)
    return resize(im, (round(im.width * ratio), round(im.height * ratio)))


def paste_fit(canvas, im, box):
    x, y, w, h = box
    im = fit(im, (w, h))
    canvas.paste(im, (x + (w-im.width)//2, y + (h-im.height)//2), im)


def contact(images, dark):
    bg, ink = ('#263b37', '#fff8e8') if dark else ('#fff8e8', '#563f32')
    sheet = Image.new('RGB', (1000, 1060), bg)
    draw = ImageDraw.Draw(sheet)
    draw.text((20, 16), 'PERCH SORT / V2 / ' + ('DARK' if dark else 'LIGHT'), fill=ink, font=ImageFont.load_default(size=24))
    draw.text((20, 52), 'Original assets / large view + actual 64px below / reference bird & perch:', fill=ink, font=ImageFont.load_default(size=16))
    paste_fit(sheet, Image.open(ROOT/'raw/birds/bird_01.png'), (720, 4, 80, 100))
    paste_fit(sheet, Image.open(ROOT/'raw/env/perch.png'), (810, 34, 170, 50))
    for i, (key, _, size, _) in enumerate(SPECS[:15]):
        x, y = 10 + i%5*198, 115 + i//5*305
        draw.rounded_rectangle((x, y, x+188, y+290), 12, outline='#58736b' if dark else '#d9c9af', width=1)
        paste_fit(sheet, images[key], (x+8, y+5, 172, 158))
        draw.text((x+9, y+169), key, fill=ink, font=ImageFont.load_default(size=16))
        draw.text((x+9, y+193), f'{size[0]} x {size[1]} RGBA', fill=ink, font=ImageFont.load_default(size=12))
        paste_fit(sheet, images[key], (x+9, y+218, 64, 64))
        draw.text((x+88, y+240), '64px', fill=ink, font=ImageFont.load_default(size=13))
    return sheet


def sleep_contact(images):
    sheet = Image.new('RGB', (1600, 770), '#fff8e8')
    draw = ImageDraw.Draw(sheet)
    draw.text((20, 18), 'SLEEP VARIANTS / ORIGINAL - CLOSED EYE / SAME FEET BASELINE', fill='#563f32', font=ImageFont.load_default(size=22))
    for i in range(1, 11):
        key = f'bird_{i:02d}'
        x, y = 12+(i-1)%5*318, 65+(i-1)//5*350
        draw.rectangle((x, y, x+305, y+335), fill='#263b37' if i>5 else '#f6ecd9')
        paste_fit(sheet, Image.open(ROOT/f'raw/birds/{key}.png'), (x+3,y+3,148,200))
        paste_fit(sheet, images[key+'s'], (x+154,y+3,148,200))
        for dx, im in [(50, Image.open(ROOT/f'raw/birds/{key}.png')), (200,images[key+'s'])]:
            paste_fit(sheet, im, (x+dx,y+218,64,64))
        draw.text((x+12,y+302), key+' / '+key+'s / 64px', fill='#fff8e8' if i>5 else '#563f32', font=ImageFont.load_default(size=14))
    return sheet


def main():
    manifest = json.loads((ROOT/'manifest.json').read_text())
    masters = {key: clean(Image.open(ROOT/f'source/v2/{key}.png')) for key, *_ in SPECS}
    nest_boxes = [masters[k].getchannel('A').getbbox() for k in ('nest', 'nest_done')]
    nest_box = (min(b[0] for b in nest_boxes),min(b[1] for b in nest_boxes),max(b[2] for b in nest_boxes),max(b[3] for b in nest_boxes))
    images, lines = {}, []
    for key, group, size, use in SPECS:
        im = masters[key]
        canvas = Image.new('RGBA', size)
        box = nest_box if key in ('nest','nest_done') else im.getchannel('A').getbbox()
        im = im.crop(box)
        if group == 'birds':
            base = manifest['assets'][key[:-1]]
            x,y,r,b = base['alphaBounds']
            im = resize(im, (r-x,b-y))
        else:
            margin = 12 if key=='crate' else 16 if min(size)>=384 else 12
            im = fit(im, (96,32) if key=='closed_eye' else (size[0]-margin*2,size[1]-margin*2))
            x,y = (size[0]-im.width)//2, (size[1]-im.height)//2
        canvas.alpha_composite(im,(x,y))
        path = f'raw/{group}/{key}.png'
        (ROOT/'raw'/group).mkdir(exist_ok=True)
        canvas.save(ROOT/path,optimize=True)
        images[key] = canvas
        entry = {'file':path,'size':list(size),'mode':'RGBA','alphaBounds':list(canvas.getchannel('A').getbbox()),'unityPivot':[0.5,0.5],'anchorPixelsFromTop':[size[0]/2,size[1]/2],'usage':use,'source':f'source/v2/{key}.png','batch':'T10a'}
        if group=='birds':
            entry.update(unityPivot=base['unityPivot'],footCenterPixels=base['footCenterPixels'],anchorPixelsFromTop=base['footCenterPixels'],baseAsset=key[:-1],feetYFromTop=870)
        if key=='crate':
            entry['textRectPixelsFromTop']=[140,132,820,246]
            entry['anchorPixelsFromTop']=[512,304]
            entry['unityPivot']=[0.5,0.05]
        if key=='basket': entry['birdStandingPixelsFromTop']=[256,190]
        if key in ('nest','nest_done'): entry['iconCenterPixelsFromTop']=[256,248]
        if key=='nest': entry['iconSafeRadiusPixels']=84
        manifest['assets'][key] = entry
        # Deliverable checks only: requested size, actual alpha, safe border, content.
        check = Image.open(ROOT/path)
        alpha = check.getchannel('A')
        hist = alpha.histogram()
        assert check.size==size and check.mode=='RGBA',key
        assert hist[0]>0 and sum(hist[8:])>0,key
        assert alpha.crop((0,0,size[0],8)).getbbox() is None,key
        assert alpha.crop((0,size[1]-8,size[0],size[1])).getbbox() is None,key
        assert alpha.crop((0,0,8,size[1])).getbbox() is None,key
        assert alpha.crop((size[0]-8,0,size[0],size[1])).getbbox() is None,key
        if group=='birds': assert alpha.crop((400,870,900,871)).getbbox() is not None,key
        lines.append(f'PASS {path} {size[0]}x{size[1]} RGBA alpha={alpha.getextrema()} clearBorder=8px transparent={hist[0]} partial={sum(hist[1:255])}')
    assert images['ring'].getpixel((128,128))[3]==0
    (ROOT/'manifest.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n')
    light,dark = contact(images,False),contact(images,True)
    light.save(ROOT/'contact_v2_light.png')
    dark.save(ROOT/'contact_v2_dark.png')
    pair=Image.new('RGB',(2000,1060));pair.paste(light,(0,0));pair.paste(dark,(1000,0));pair.save(ROOT/'contact_v2.png')
    sleep_contact(images).save(ROOT/'qa/sleep_v2.png')
    (ROOT/'qa/check_v2.txt').write_text('\n'.join(lines)+'\nPASS 25 production assets; ring center transparent; 10 sleeping birds touch y=870.\n')
    print('\n'.join(lines))


if __name__=='__main__':
    main()
