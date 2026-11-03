"""Prepare T39 note states with identical silhouette registration and static QA."""
import argparse
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageOps, ImageDraw

ROOT=Path(__file__).resolve().parents[1]
REPO=ROOT.parents[2]
ART=REPO/'PerchSort/Assets/Resources/V5/Art'
QA=ROOT/'qa'
STAGE=QA/'staged'
STAGE.mkdir(exist_ok=True)
RESAMPLE=Image.Resampling.LANCZOS

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def build():
    full=np.array(Image.open(ROOT/'raw/note_full.png').convert('RGBA'))
    empty=np.array(Image.open(ROOT/'raw/note_empty.png').convert('RGBA'))
    assert full.shape==empty.shape
    alpha=np.clip((full[:,:,3].astype(float)-20)*255/233,0,255).astype('uint8')
    # Use one alpha silhouette and crop for both independently generated material states.
    # Fill only sparse missing edge color in the edit, using its own dark outline palette.
    missing=(alpha>0)&(empty[:,:,3]<32)
    foreground=empty[empty[:,:,3]>=200,:3]
    luminance=foreground.mean(axis=1)
    outline=np.median(foreground[luminance<np.percentile(luminance,25)],axis=0).astype('uint8')
    empty[missing,:3]=outline
    full[:,:,3]=alpha;empty[:,:,3]=alpha
    bbox=Image.fromarray(alpha).getbbox()
    target=ImageOps.contain(Image.fromarray(full).crop(bbox),(52,52),RESAMPLE)
    position=((64-target.width)//2,(64-target.height)//2)
    outputs={}
    for name,a in [('note_full',full),('note_empty',empty)]:
        im=Image.fromarray(a).crop(bbox).resize(target.size,RESAMPLE)
        out=Image.new('RGBA',(64,64));out.alpha_composite(im,position)
        pixels=np.array(out)
        pixels[pixels[:,:,3]<3,3]=0
        if name=='note_empty':
            pixels[:,:,3]=np.rint(pixels[:,:,3].astype(float)*0.4).astype('uint8')
        pixels[pixels[:,:,3]==0,:3]=0
        outputs[name]=Image.fromarray(pixels)
    return outputs,dict(raw_crop=list(bbox),registered_size=list(target.size),
                        canvas_position=list(position),empty_opacity_factor=0.4,
                        missing_edit_edge_color_pixels=int(missing.sum()),
                        missing_edge_fill_rgb=outline.tolist())

def qa(outputs):
    full,empty=outputs['note_full'],outputs['note_empty']
    pill=Image.open(ART/'pill_cream.png').convert('RGBA').resize((384,132),RESAMPLE)
    for i in range(5):
        pill.alpha_composite(full if i<2 else empty,(32+i*64,34))
    pill.save(QA/'song_pill.png')
    review=Image.new('RGBA',(384,344),'#fff4df')
    review.alpha_composite(pill,(0,0))
    review.paste(Image.new('RGBA',(384,132),'#253139'),(0,132))
    review.alpha_composite(pill,(0,132))
    for i,(bg,im) in enumerate([('#fff4df',full),('#fff4df',empty),('#253139',full),('#253139',empty)]):
        tile=Image.new('RGBA',(96,80),bg);tile.alpha_composite(im,(16,8))
        review.alpha_composite(tile,(i*96,264))
    review.save(QA/'contact.png')
    ImageOps.grayscale(review.convert('RGB')).save(QA/'grayscale.png')
    # A diagnostic opacity-normalized mask proves geometry is shared.
    masks=Image.new('RGB',(128,64),'black')
    for i,im in enumerate((full,empty)):
        binary=Image.fromarray((np.array(im.getchannel('A'))>0).astype('uint8')*255).convert('RGB')
        masks.paste(binary,(i*64,0))
    masks.resize((512,256),Image.Resampling.NEAREST).save(QA/'matching_silhouettes.png')

def main(publish):
    outputs,normalization=build()
    qa(outputs)
    records={}
    for n,im in outputs.items():
        p=STAGE/(n+'.png');im.save(p,optimize=True)
        a=np.array(im)
        records[n+'.png']=dict(size=list(im.size),mode=im.mode,alpha_min=int(a[:,:,3].min()),
                              alpha_max=int(a[:,:,3].max()),visible_bounds=im.getbbox(),
                              hidden_rgb_zero=bool(np.all(a[a[:,:,3]==0,:3]==0)),sha256=digest(p))
    f=np.array(outputs['note_full']);e=np.array(outputs['note_empty'])
    baseline=json.loads((QA/'baseline.json').read_text())
    changed=[p for p,h in baseline.items() if digest(REPO/p)!=h]
    checks=dict(exact_dimensions=all(im.size==(64,64) for im in outputs.values()),
                rgba=all(im.mode=='RGBA' for im in outputs.values()),
                full_alpha_range=(int(f[:,:,3].min()),int(f[:,:,3].max()))==(0,255),
                empty_alpha_range=(int(e[:,:,3].min()),int(e[:,:,3].max()))==(0,102),
                same_visible_bounds=outputs['note_full'].getbbox()==outputs['note_empty'].getbbox(),
                same_silhouette_pixels=bool(np.array_equal(f[:,:,3]>0,e[:,:,3]>0)),
                empty_alpha_exactly_40_percent=bool(np.array_equal(e[:,:,3],np.rint(f[:,:,3].astype(float)*0.4).astype('uint8'))),
                zero_hidden_rgb=all(r['hidden_rgb_zero'] for r in records.values()),
                all_original_pngs_unchanged=not changed)
    assert all(checks.values()),checks
    if publish:
        for n in outputs:
            source=STAGE/(n+'.png');dest=ART/(n+'.png');data=source.read_bytes()
            if dest.exists():assert dest.read_bytes()==data,f'Refusing overwrite: {dest}'
            else:
                with dest.open('xb') as out:out.write(data)
    verification=dict(status='PASS',checks=checks,files=records,normalization=normalization,
                      original_pngs_checked=len(baseline),changed_originals=changed,
                      composite=dict(file='qa/song_pill.png',pill_size=[384,132],slots=5,full=2,empty=3),
                      published=publish,runtime='Not run: Linux development environment has no Unity Editor')
    (QA/'verification.json').write_text(json.dumps(verification,indent=2)+'\n')
    prompts=['# T39 exact image-generation prompts','','Tool: built-in image_gen.imagegen.','']
    for n in outputs:
        entry=json.loads((ROOT/'raw'/(n+'.json')).read_text())
        prompts+=['## '+n,'','Reference paths: '+', '.join(entry['refs']),'','```text',entry['prompt'],'```','']
    (ROOT/'prompts.md').write_text('\n'.join(prompts))
    print(json.dumps(verification,indent=2))

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('--publish',action='store_true');main(p.parse_args().publish)
