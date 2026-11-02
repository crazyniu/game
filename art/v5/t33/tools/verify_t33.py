"""Static compositions and deterministic file checks for T33; no Unity execution."""
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont, ImageOps
from process_t33 import ROOT, REPO, ART, QA, STAGE, SPECIES, POSES, expected, get_asset, metrics, LANCZOS

def contact(names, target, tile=(180,180), cols=4, small=False):
    w,h=tile
    sheet=Image.new('RGB',(cols*w,((len(names)+cols-1)//cols)*h),'#c9c9c3')
    d=ImageDraw.Draw(sheet)
    font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',12)
    for i,n in enumerate(names):
        x=i%cols*w;y=i//cols*h
        d.text((x+3,y+4),n,font=font,fill='#222222')
        im=get_asset(n);im.thumbnail((48,48) if small else (w//2-8,h-30),LANCZOS)
        for j,bg in enumerate(('#fff4df','#253139')):
            t=Image.new('RGBA',(w//2-2,h-25),bg)
            t.alpha_composite(im,((t.width-im.width)//2,(t.height-im.height)//2))
            sheet.paste(t.convert('RGB'),(x+j*w//2,y+24))
    sheet.save(QA/target)

def board():
    out=get_asset('bg_w1_golden')
    br=Image.open(ART/'branch_left.png').convert('RGBA')
    scale=495/br.width;br=br.resize((495,round(br.height*scale)),LANCZOS)
    rows=[(0,730,['sunny','berry','mint']),(585,775,['sky','cocoa','snow']),
          (0,1010,['egg','ember_sleep']),(585,1055,['crow']),
          (0,1290,[]),(585,1335,[])]
    def blit(im,x,y):
        out.alpha_composite(im,(round(x),round(y)))
    for x,y,ids in rows:
        branch=br if x==0 else ImageOps.mirror(br)
        blit(branch,x,y)
        for j,ident in enumerate(ids):
            name='egg_idle' if ident=='egg' else 'bird_'+ident+('' if ident.endswith('_sleep') else '_idle')
            sprite=get_asset(name).resize((164,164),LANCZOS)
            seat=x+125+j*111 if x==0 else x+146+j*111
            # Use the colored wood at the seat to avoid treating a leaf as the perch.
            a=np.array(branch)
            bx=max(0,min(branch.width-1,round(seat-x)))
            strip=a[:,max(0,bx-3):min(branch.width,bx+4)]
            wood=(strip[:,:,3]>128)&(strip[:,:,0]>strip[:,:,1]*1.15)&(strip[:,:,0]>strip[:,:,2]*1.35)
            ys=np.where(wood)[0]
            top=int(np.percentile(ys,5)) if len(ys) else 35
            base=y+top+2
            blit(sprite,seat-82,base-244*164/256)
            if ident.endswith('_sleep'):
                z=get_asset('zzz').resize((66,66),LANCZOS);blit(z,seat+43,base-168)
    # Original root foliage hides the branch cut ends when available.
    bushes=list(ART.glob('*bush*.png'))
    if bushes:
        bush=Image.open(bushes[0]).convert('RGBA')
        bush=ImageOps.contain(bush,(165,200),LANCZOS)
        for x,y,_ in rows:
            tile=bush if x==0 else ImageOps.mirror(bush)
            blit(tile,-40 if x==0 else 955,y-45)
    d=ImageDraw.Draw(out)
    font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',22)
    d.text((40,1815),'STATIC ASSET COMPOSITE - NOT A UNITY SCREENSHOT',font=font,fill='#5a3a22')
    out.save(QA/'w1_board_composite.png')
    out.resize((540,960),LANCZOS).save(QA/'w1_board_composite_preview.png')
    if (STAGE/'night_overlay.png').exists():
        overlay=get_asset('night_overlay');night=Image.alpha_composite(out,overlay)
        night.save(QA/'night_overlay_composite.png')
        night.resize((540,960),LANCZOS).save(QA/'night_overlay_composite_preview.png')

def registration():
    # Manually identified crown-to-belly bounds; exclude appendages, hats, feet and wings.
    bounds={'sunny':(66,222),'sky':(58,224),'berry':(77,224),'mint':(65,223),
            'cocoa':(66,224),'hoot':(73,223),'snow':(65,223),'ember':(75,223),
            'ink':(64,223),'crow':(65,223)}
    result=[]
    sheet=Image.new('RGB',(5*260,2*290),'#28343c');d=ImageDraw.Draw(sheet)
    for i,s in enumerate(SPECIES):
        im=get_asset('bird_'+s+'_idle')
        top,bottom=bounds[s]
        result.append({'species':s,'body_top_y':top,'body_bottom_y':bottom,'body_height':bottom-top+1,
                       'method':'visual crown-to-belly annotation; excludes crest/hat, feet, wings; approximate +/-3 px'})
        t=Image.new('RGBA',(256,256),'#28343c');t.alpha_composite(im)
        x=i%5*260;y=i//5*290;sheet.paste(t.convert('RGB'),(x,y))
        d.line((x+48,y+top,x+210,y+top),fill='#72f4d4',width=1)
        d.line((x+48,y+bottom,x+210,y+bottom),fill='#72f4d4',width=1)
        d.text((x+8,y+267),s+' body '+str(bottom-top+1)+'px',fill='white')
    sheet.save(QA/'body_bbox_annotations.png')
    (QA/'body_bounds.json').write_text(json.dumps(result,indent=2)+'\n')
    pair=Image.new('RGB',(304,105),'#c9c9c9')
    gray=Image.open(QA/'grayscale_64_all.png')
    for j,s in enumerate(['cocoa','hoot']):
        i=SPECIES.index(s);pair.paste(gray.crop((i*152,0,(i+1)*152,105)),(j*152,0))
    pair.save(QA/'hoot_cocoa_grayscale_64.png')

def verify():
    rows=[];fail=[];missing=[]
    for n,size in expected().items():
        path=ART/(n+'.png')
        if not path.exists():
            missing.append(path.name);continue
        im=Image.open(path);m=metrics(n,im)
        m['sha256']=hashlib.sha256(path.read_bytes()).hexdigest()
        m['expected_size']=list(size)
        m['checks']={'size':im.size==size,'rgba':im.mode=='RGBA',
                    'alpha':m['alpha_min']==(255 if n.startswith('bg_') else 140 if n=='night_overlay' else 0) and m['alpha_max']==(140 if n=='night_overlay' else 255),
                    'hidden_rgb_zero':m['hidden_rgb_zero']}
        if n.startswith(('egg_','bird_')) and '_fly_' not in n:
            m['checks']['standing_baseline']=m['solid_bottom_y']==244
        if n.startswith('bird_') and '_fly_' in n:
            m['note']='Airborne: feet tucked; idle body registration retained. Solid bottom is wing/feet bound, not a standing baseline.'
        if not all(m['checks'].values()):fail.append(n)
        rows.append(m)
    baseline=json.loads((QA/'baseline.json').read_text())['existing_art']
    changed=[p for p,h in baseline.items() if not (REPO/p).exists() or hashlib.sha256((REPO/p).read_bytes()).hexdigest()!=h]
    result={'expected':len(expected()),'delivered':len(rows),'missing':missing,'failed':fail,
            'preexisting_files_checked':len(baseline),'preexisting_files_changed':changed,
            'runtime_validation':'Not run: development machine has no Unity Editor',
            'files':rows}
    (QA/'verification.json').write_text(json.dumps(result,indent=2)+'\n')
    (ROOT/'delivered_files.txt').write_text(''.join(str(Path('PerchSort/Assets/Resources/V5/Art')/(r['file']))+'\n' for r in rows))
    print(json.dumps({k:v for k,v in result.items() if k!='files'}))
    return result

if __name__=='__main__':
    board();registration()
    icons=[n for n in expected() if n.startswith(('icon_','badge_','gift_','dial_')) and (STAGE/(n+'.png')).exists()]
    contact(icons,'icons_48.png',(210,95),4,True)
    specials=[n for n in expected() if not n.startswith(('bird_','bg_','icon_','badge_','gift_','dial_')) and n!='night_overlay' and (STAGE/(n+'.png')).exists()]
    contact(specials,'special_light_dark.png',(310,210),3)
    additions=[n for n in expected() if n.startswith(('gift_','dial_')) or n in ('mama_bow','chick_fluff','swivel_knot')]
    additions=[n for n in additions if (STAGE/(n+'.png')).exists()]
    contact(additions,'addendum_light_dark.png',(280,180),4)
    verify()
