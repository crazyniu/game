"""Check bounded pixels and publish the specified offline art evidence."""
from pathlib import Path
import hashlib
import json
import re
import numpy as np
from PIL import Image, ImageDraw

ART = Path('PerchSort/Assets/Resources/Art')
ROOT = Path('art/map_seams')
EVIDENCE = Path('docs/work-log/map-seam-art/evidence')

def digest(data):
    return hashlib.sha256(data).hexdigest()

def path_edges(row):
    rgb = row[:, :3].astype(int)
    mask = (rgb[:, 0] > rgb[:, 1] + 18) & (rgb[:, 1] > rgb[:, 2] + 30) & (rgb[:, 0] > 200)
    left = right = 540
    while mask[left - 1]:
        left -= 1
    while mask[right + 1]:
        right += 1
    return dict(left=left, right=right, centre_x=(left + right) / 2, width=right - left + 1)

original = {n: Image.open(ART / f'map_bg_w{n}.png').convert('RGBA') for n in range(1, 5)}
finished = {n: Image.open(ART / f'map_bg_continuous_w{n}.png').convert('RGBA') for n in range(1, 5)}
qa = dict(protected_rows='320..2679 inclusive (2360 rows)',worlds={},boundaries={})
guids = []
for n in range(1, 5):
    source = ART / f'map_bg_w{n}.png'
    output = ART / f'map_bg_continuous_w{n}.png'
    a, b = np.array(original[n]), np.array(finished[n])
    before_meta, after_meta = (p.with_suffix('.png.meta').read_text() for p in [source, output])
    settings_same = re.sub(r'^guid: .*$', '', before_meta, flags=re.M) == re.sub(r'^guid: .*$', '', after_meta, flags=re.M)
    guid = re.search(r'^guid: (.*)$', after_meta, re.M)[1]
    guids.append(guid)
    changed_rows = np.where(np.any(a != b, axis=(1, 2)))[0]
    result = dict(size=list(finished[n].size),mode=finished[n].mode,
        source_sha256=digest(source.read_bytes()),output_sha256=digest(output.read_bytes()),
        middle_original_sha256=digest(a[320:2680].tobytes()),middle_derived_sha256=digest(b[320:2680].tobytes()),
        middle_pixel_bytes_equal=a[320:2680].tobytes() == b[320:2680].tobytes(),
        alpha_pixel_bytes_equal=a[:, :, 3].tobytes() == b[:, :, 3].tobytes(),
        changed_rows_count=len(changed_rows),changes_only_in_allowed_bands=bool(np.all((changed_rows < 320) | (changed_rows >= 2680))),
        import_settings_equal_except_new_guid=settings_same,new_guid=guid,
        top_edge=path_edges(b[0]),bottom_edge=path_edges(b[-1]))
    if n == 1:
        result['outer_bottom_band_unchanged'] = a[2680:].tobytes() == b[2680:].tobytes()
    if n == 4:
        result['outer_top_band_unchanged'] = a[:320].tobytes() == b[:320].tobytes()
    qa['worlds'][f'w{n}'] = result
    assert finished[n].size == (1080, 3000)
    assert all(result[k] for k in ['middle_pixel_bytes_equal','alpha_pixel_bytes_equal','changes_only_in_allowed_bands','import_settings_equal_except_new_guid'])
assert len(set(guids)) == 4
for lower in [1, 2, 3]:
    upper = lower + 1
    a, b = np.array(finished[upper]), np.array(finished[lower])
    difference = np.abs(a[2840:, :, :3].astype(int) - b[:160, :, :3].astype(int))
    bottom, top = path_edges(a[-1]), path_edges(b[0])
    result = dict(overlap_rows=160,rgb_equal=difference.max() == 0,max_rgb_channel_error=int(difference.max()),
        upper_bottom_edge=bottom,lower_top_edge=top,edge_centre_difference_px=abs(bottom['centre_x']-top['centre_x']),
        edge_width_difference_px=abs(bottom['width']-top['width']))
    result['rgb_equal'] = bool(result['rgb_equal'])
    qa['boundaries'][f'w{upper}_w{lower}'] = result
    assert result['rgb_equal'] and result['edge_centre_difference_px'] <= 4 and result['edge_width_difference_px'] <= 4
    Image.open(ROOT/'raw'/f'boundary_w{upper}_w{lower}_input.png').save(EVIDENCE/f'boundary_w{upper}_w{lower}_before.jpg',quality=95,subsampling=0)

def stack(strips, stride, canopy=False):
    height = 3 * stride + 3000
    canvas = Image.new('RGBA', (1080, height), (145, 175, 98, 255))
    for n in [4, 3, 2, 1]:
        y = (4 - n) * stride
        if canopy and n < 4:
            canvas.alpha_composite(strips[n].crop((0, 0, 1080, 1320)), (0, y - 1160))
        canvas.alpha_composite(strips[n], (0, y))
    return canvas.convert('RGB')

old, new = (stack(images, 2840) for images in [original, finished])
for name, im in [('stack_before',old),('stack_continuous',new)]:
    im.save(ROOT/'qa'/f'{name}.png')
    im.resize((270,2880),Image.Resampling.LANCZOS).save(EVIDENCE/f'{name}.jpg',quality=95,subsampling=0)
comparison=Image.new('RGB',(564,2912),'white');draw=ImageDraw.Draw(comparison)
draw.text((4,8),'Original | 25% | 160 px overlap',fill='black');draw.text((286,8),'Derived | 25% | 160 px overlap',fill='black')
comparison.paste(old.resize((270,2880),Image.Resampling.LANCZOS),(4,28));comparison.paste(new.resize((270,2880),Image.Resampling.LANCZOS),(286,28))
comparison.save(EVIDENCE/'stack_comparison.jpg',quality=95,subsampling=0)
for lower in [1,2,3]:
    y=(4-lower)*2840
    # The panel evidence must also equal the actual composited strips.
    expected=np.array(Image.open(ROOT/'qa'/f'boundary_w{lower+1}_w{lower}_panel.png'))
    actual=np.array(new.crop((0,y-280,1080,y+440)))
    assert np.array_equal(expected,actual)
# Show the unchanged runtime canopy mapping separately; it is not the requested overlap model.
for label,images in [('before',original),('derived',finished)]:
    runtime=stack(images,4000,canopy=True)
    runtime.resize((270,3750),Image.Resampling.LANCZOS).save(EVIDENCE/f'canopy_layout_{label}.jpg',quality=95,subsampling=0)
    runtime.crop((0,11800,1080,12360)).save(EVIDENCE/f'canopy_inner_join_{label}.jpg',quality=95,subsampling=0)
qa['requested_stack'] = dict(dimensions=[1080,11520],stride_px=2840,overlap_px=160,preview_scale=0.25,all_boundary_panels_equal_actual_composite=True)
qa['runtime_canopy_comparison'] = dict(stride_px=4000,canopy_height_px=1320,status='Existing layout still replays an internal lower-strip crop. Requires coordinator integration; no runtime fix claimed.')
(ROOT/'qa'/'verification.json').write_text(json.dumps(qa,indent=2)+'\n')
print(json.dumps(qa,indent=2))
