"""Build and verify the T31 top limb without modifying any T30 runtime asset."""
import hashlib
import json
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
REPO = ROOT.parents[1]
ART = REPO/'PerchSort/Assets/Resources/V5/Art'
QA = ROOT/'qa'
RAW = ROOT/'raw/limb_top_t31_v1.png'
OUT = ART/'limb_top.png'

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

before = {p.name: digest(p) for p in ART.glob('*.png') if p.name != OUT.name}
im = Image.open(RAW).convert('RGBA')
a = np.array(im)
# Use the same alpha-haze cleanup as T30; preserve antialiased leaf and rope edges.
a[:,:,3] = np.clip((a[:,:,3].astype(float)-20)*255/233,0,255).astype('uint8')
a[a[:,:,3] == 0,:3] = 0
clean = Image.fromarray(a)
# The generated 2172x724 composition is registered to a 1080x314 content rectangle.
# This brings the hanger's visible bottom to y=239 (full opacity y=238), within tolerance,
# and meets the existing house's visible rope top at y=235+4 without changing the house.
content = clean.resize((1080,314),Image.Resampling.LANCZOS)
out = Image.new('RGBA',(1080,320))
out.alpha_composite(content,(0,0))
a = np.array(out)
a[a[:,:,3]<3,3] = 0
a[a[:,:,3] == 0,:3] = 0
out = Image.fromarray(a)
out.save(OUT,optimize=True)

# Composite with the unmodified house canvas centered exactly at (540,405).
background = Image.open(ART/'bg_forest_day.png').convert('RGBA')
house = Image.open(ART/'birdhouse.png').convert('RGBA')
house_origin = (390,235)
house_center = (house_origin[0]+house.width/2,house_origin[1]+house.height/2)
preview = background.copy()
preview.alpha_composite(house,house_origin)
preview.alpha_composite(out,(0,0))
preview.save(QA/'limb_top_composite.png',optimize=True)
preview.crop((0,0,1080,640)).save(QA/'limb_top_composite_detail.png',optimize=True)
preview.crop((500,200,580,280)).resize((640,640),Image.Resampling.NEAREST).save(QA/'limb_top_attachment_detail.png')

sheet = Image.new('RGBA',(1080,640),'#fff4df')
sheet.paste(Image.new('RGBA',(1080,320),'#1b2730'),(0,320))
sheet.alpha_composite(out,(0,0))
sheet.alpha_composite(out,(0,320))
sheet.save(QA/'limb_top_light_dark.png',optimize=True)

alpha = np.array(out.getchannel('A'))
thresholds = {}
for threshold in (1,128,255):
    ys,xs = np.where(alpha>=threshold)
    bottom=int(ys.max())
    last=np.flatnonzero(alpha[bottom]>=threshold)
    thresholds[str(threshold)] = dict(lowest_y=bottom,x_min=int(last.min()),
                                    x_max=int(last.max()),x_mid=float((last.min()+last.max())/2))
house_layer=Image.new('RGBA',(1080,320))
house_layer.alpha_composite(house,house_origin)
house_alpha=np.array(house_layer.getchannel('A'))
joint=(alpha>=128)&(house_alpha>=128)
jy,jx=np.where(joint)
joint_bounds=[int(jx.min()),int(jy.min()),int(jx.max()),int(jy.max())] if len(jx) else None
below_nonzero=int(np.count_nonzero(alpha[261:]))
lower_nonrope=int(np.count_nonzero(np.concatenate((alpha[210:,:500],alpha[210:,580:]),axis=1)))
after = {p.name: digest(p) for p in ART.glob('*.png') if p.name != OUT.name}
checks=dict(exact_dimensions=out.size==(1080,320),rgba=out.mode=='RGBA',
            alpha_range=(int(alpha.min()),int(alpha.max()))==(0,255),
            transparent_below_y260=below_nonzero==0,
            no_low_nonrope_pixels=lower_nonrope==0,
            opaque_hook_in_tolerance=all(abs(thresholds[t]['lowest_y']-235)<=4 and abs(thresholds[t]['x_mid']-540)<=4 for t in ('128','255')),
            house_center_y405=house_center==(540,405),house_loop_contacts_hook=bool(len(jx)),
            originals_unchanged=before==after)
metrics=dict(status='PASS' if all(checks.values()) else 'FAIL',checks=checks,
             final_file=str(OUT.relative_to(REPO)),size=list(out.size),mode=out.mode,
             alpha_min=int(alpha.min()),alpha_max=int(alpha.max()),
             alpha_threshold_bounds=thresholds,nonzero_pixels_below_y260=below_nonzero,
             nonrope_pixels_at_or_below_y210=lower_nonrope,house_canvas_top_left=[390,235],
             house_canvas_center=[540,405],joint_alpha128_overlap_bounds=joint_bounds,
             joint_alpha128_overlap_pixel_count=int(joint.sum()),
             final_sha256=digest(OUT),raw_sha256=digest(RAW),reference_sha256=before,
             normalization=dict(raw_size=list(im.size),content_resize=[1080,314],canvas=[1080,320]))
(QA/'limb_top_verification.json').write_text(json.dumps(metrics,indent=2)+'\n')
print(json.dumps({k:v for k,v in metrics.items() if k!='reference_sha256'},indent=2))
assert all(checks.values()), checks
