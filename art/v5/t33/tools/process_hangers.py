"""Derive two hanger assets; preserve the source limb outside its local rope repair."""
import argparse
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT=Path(__file__).resolve().parents[1]
REPO=ROOT.parents[2]
ART=REPO/'PerchSort/Assets/Resources/V5/Art'
RAW=ROOT/'raw'
QA=ROOT/'qa'
STAGE=QA/'hangers_staged'
STAGE.mkdir(exist_ok=True)
RESAMPLE=Image.Resampling.LANCZOS
EDIT=(510,95,570,250)

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def clean(im):
    a=np.array(im.convert('RGBA'))
    a[:,:,3]=np.clip((a[:,:,3].astype(float)-20)*255/233,0,255).astype('uint8')
    a[a[:,:,3]==0,:3]=0
    return Image.fromarray(a)

def plain_limb():
    original=np.array(Image.open(ART/'limb_top.png').convert('RGBA'))
    patch=np.array(clean(Image.open(RAW/'limb_center_plain_followup.png')).resize((240,180),RESAMPLE))
    result=original.copy()
    left,top,right,bottom=EDIT
    # These two unoccluded columns provide the original branch's edge antialiasing.
    anchors=(505,575)
    yy=np.arange(original.shape[0])
    for x in range(left,right):
        t=(x-anchors[0])/(anchors[1]-anchors[0])
        alpha=(original[:,anchors[0],3]*(1-t)+original[:,anchors[1],3]*t)
        solid=np.where(alpha>=128)[0]
        ty,by=int(solid.min()),int(solid.max())
        gx=x-420
        gen_solid=np.where(patch[:,gx,3]>=128)[0]
        gt,gb=int(gen_solid.min()),int(gen_solid.max())
        # Align the generated wood texture to the preserved branch silhouette.
        sample=gt+(yy-ty)*(gb-gt)/(by-ty)
        rgb=np.stack([np.interp(sample,np.arange(180),patch[:,gx,c]) for c in range(3)],axis=1)
        replacement=np.column_stack((rgb,alpha))
        # Feather only through clean wood; all wrapped rope pixels are inside the full-weight region.
        weight=min(1.0,(x-left)/7,(right-1-x)/7)
        value=original[top:bottom,x].astype(float)*(1-weight)+replacement[top:bottom]*weight
        # Clear every old hanger pixel outside the reconstructed branch, including faint fringe.
        outside_wood=alpha[top:bottom]==0
        value[outside_wood]=replacement[top:bottom][outside_wood]
        result[top:bottom,x]=np.rint(value).clip(0,255).astype('uint8')
    result[result[:,:,3]==0,:3]=0
    return Image.fromarray(result)

def rope_tile():
    im=clean(Image.open(RAW/'rope_segment_followup.png'))
    alpha=np.array(im.getchannel('A'))
    xs=np.where(alpha>=128)[1]
    crop=im.crop((int(xs.min())-2,0,int(xs.max())+3,im.height))
    strip=np.array(crop.resize((18,round(crop.height*18/crop.width)),RESAMPLE))
    # Find two cuts of matching twist phase, using surrounding rows as well as the seam itself.
    premul=strip.astype(float)
    premul[:,:,:3]*=premul[:,:,3:4]/255
    best=None
    for length in range(110,145):
        for start in range(15,len(strip)-length-15):
            a=premul[start-3:start+4]
            b=premul[start+length-3:start+length+4]
            score=float(np.mean((a-b)**2))
            if best is None or score<best[0]:
                best=(score,start,length)
    score,start,length=best
    segment=Image.fromarray(strip[start:start+length+1]).resize((18,128),RESAMPLE)
    a=np.array(segment).astype(float)
    seam=(a[0]+a[-1])/2
    first_delta=seam-a[0]
    last_delta=seam-a[-1]
    for k in range(6):
        weight=(1-k/6)**2
        a[k]+=first_delta*weight
        a[-1-k]+=last_delta*weight
    a=np.rint(a).clip(0,255).astype('uint8')
    a[a[:,:,3]<3,3]=0
    a[a[:,:,3]==0,:3]=0
    a[-1]=a[0]
    out=Image.new('RGBA',(24,128))
    out.paste(Image.fromarray(a),(3,0))
    return out,dict(source_strip_size=[18,len(strip)],phase_start=start,phase_length=length,matching_window_mse=score,seam_blend_rows=6)

def preview(limb,rope):
    lightdark=Image.new('RGBA',(1080,640),'#fff4df')
    lightdark.paste(Image.new('RGBA',(1080,320),'#1b2730'),(0,320))
    lightdark.alpha_composite(limb,(0,0));lightdark.alpha_composite(limb,(0,320))
    lightdark.save(QA/'limb_top_plain_light_dark.png')
    close=Image.new('RGBA',(480,360),'#253139')
    close.alpha_composite(limb.crop((420,80,660,260)).resize((480,360),RESAMPLE))
    close.save(QA/'limb_plain_repair_detail.png')
    tiled=Image.new('RGBA',(180,512),'#fff4df')
    tiled.paste(Image.new('RGBA',(90,512),'#253139'),(90,0))
    for x in [33,123]:
        for y in range(0,512,128):tiled.alpha_composite(rope,(x,y))
    tiled.resize((360,1024),Image.Resampling.NEAREST).save(QA/'rope_vertical_tiling.png')

    bg=Image.open(ART/'bg_forest_day.png').convert('RGBA')
    board=bg.copy()
    house=Image.open(ART/'birdhouse.png').convert('RGBA').resize((180,204),RESAMPLE)
    small_rope=rope.resize((12,64),RESAMPLE)
    positions=[(210,220),(430,250),(650,250),(870,220)]
    alpha=np.array(limb.getchannel('A'))
    joints=[]
    for center,house_y in positions:
        branch_top=int(np.where(alpha[:,center]>=128)[0].max())-8
        # Rope travels behind the limb and joins the existing little roof loop.
        end=house_y+8
        column=Image.new('RGBA',(12,end-branch_top+1))
        for y in range(0,column.height,64):column.alpha_composite(small_rope,(0,y))
        board.alpha_composite(column,(center-6,branch_top))
        board.alpha_composite(house,(center-90,house_y))
        joints.append(dict(center_x=center,rope_top_y=branch_top,rope_end_y=end,house_top_y=house_y,house_size=[180,204]))
    board.alpha_composite(limb,(0,0))
    d=ImageDraw.Draw(board)
    font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',22)
    d.text((35,610),'T33 - four independent rope hangers - static asset composite',font=font,fill='#5a3a22')
    board.save(QA/'limb_multi_house.png')
    board.crop((0,0,1080,670)).save(QA/'limb_multi_house_detail.png')
    board.resize((540,960),RESAMPLE).save(QA/'limb_multi_house_preview.png')
    return joints

def run(publish=False):
    limb=plain_limb();rope,period=rope_tile()
    outputs={'limb_top_plain':limb,'rope_segment':rope}
    for n,im in outputs.items():
        im.save(STAGE/(n+'.png'),optimize=True)
        if publish:
            dest=ART/(n+'.png');data=(STAGE/(n+'.png')).read_bytes()
            if dest.exists():assert dest.read_bytes()==data,f'Refusing overwrite: {dest}'
            else:
                with dest.open('xb') as f:f.write(data)
    joints=preview(limb,rope)
    source=np.array(Image.open(ART/'limb_top.png'));a=np.array(limb);b=np.array(rope)
    different=np.any(source!=a,axis=2)
    allowed=np.zeros(different.shape,dtype=bool)
    x0,y0,x1,y1=EDIT;allowed[y0:y1,x0:x1]=True
    y,x=np.where(different)
    baseline=json.loads((QA/'hanger_followup_baseline.json').read_text())
    changed=[name for name,digest in baseline.items() if sha(REPO/name)!=digest]
    checks=dict(limb_size=limb.size==(1080,320),rope_size=rope.size==(24,128),
                rgba=limb.mode==rope.mode=='RGBA',limb_alpha_range=(int(a[:,:,3].min()),int(a[:,:,3].max()))==(0,255),
                rope_alpha_range=(int(b[:,:,3].min()),int(b[:,:,3].max()))==(0,255),
                outside_repair_pixel_identical=not bool(different[~allowed].any()),
                no_pixels_below_y260=not bool(a[261:,:,3].any()),
                center_hanger_removed=not bool(a[175:250,510:570,3].any()),
                rope_top_bottom_rgba_identical=bool(np.array_equal(b[0],b[-1])),
                rope_present_every_row=bool(np.all(b[:,:,3].max(axis=1)==255)),
                rope_side_gutters_transparent=not bool(b[:,:3,3].any() or b[:,-3:,3].any()),
                existing_runtime_pngs_unchanged=not changed)
    files={}
    for n,im in outputs.items():
        px=np.array(im)
        files[n+'.png']=dict(size=list(im.size),mode=im.mode,alpha_min=int(px[:,:,3].min()),
                            alpha_max=int(px[:,:,3].max()),visible_bounds=im.getbbox(),
                            sha256=sha(STAGE/(n+'.png')))
    result=dict(status='PASS' if all(checks.values()) else 'FAIL',checks=checks,files=files,
                allowed_repair_rectangle=list(EDIT),actual_changed_bbox=[int(x.min()),int(y.min()),int(x.max())+1,int(y.max())+1],
                changed_pixel_count=int(different.sum()),unchanged_pixel_count=int((~different).sum()),
                original_runtime_pngs_checked=len(baseline),changed_original_files=changed,
                rope_period_selection=period,composite_houses=joints,published=publish,
                runtime_testing='Not performed: static raster QA on Linux, no Unity Editor')
    (QA/'hanger_followup_verification.json').write_text(json.dumps(result,indent=2)+'\n')
    print(json.dumps(result,indent=2))
    assert all(checks.values()),checks

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--publish',action='store_true')
    run(parser.parse_args().publish)
