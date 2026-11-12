"""Verify accepted draw/purchase PNGs are shipped without altering their bytes."""
from pathlib import Path
import hashlib
import json
import struct

root = Path(__file__).resolve().parent.parent
manifest = json.loads((root / 'docs/design/draw-purchase-assets.json').read_text())
assets = manifest['assets']
assert manifest['gate'] == 'complete before implementation'
assert len(assets) == manifest['material_count'] == 48
for name, item in assets.items():
    path = root / item['destination']
    data = path.read_bytes()
    assert hashlib.sha256(data).hexdigest() == item['sha256'], name
    assert data[:8] == b'\x89PNG\r\n\x1a\n', name
    width, height, depth, color = struct.unpack('>IIBB', data[16:26])
    assert color == 6 and depth == 8, name
    assert [width, height] == item['size_px'], name
    left, top, right, bottom = item['alpha_128_bbox']
    assert 0 < left < right < width and 0 < top < bottom < height, name
    assert item['source_name'] != 'lottery_filled_star', name
print(f'Draw/purchase materials: {len(assets)} unchanged RGBA PNGs; recorded alpha bounds clear every edge.')
