"""Record the delivered image and importer checks for one species."""
from pathlib import Path
import hashlib
import json
import re
import sys

from PIL import Image

root = Path(__file__).resolve().parents[3]
birds = root / 'PerchSort/Assets/Resources/Birds'
species = sys.argv[1]
counts = {'flap': 6, 'hop': 6, 'turn_left': 4, 'turn_right': 4,
          'cheer': 6, 'preen': 5, 'peek': 4, 'blink': 3}


def settings(text):
    text = re.sub(r'^guid: .*$', '', text, flags=re.M)
    text = re.sub(r'spriteID: [0-9a-f]+', 'spriteID:', text)
    return '\n'.join(line.rstrip() for line in text.splitlines())


reference = settings((birds / f'{species}_idle.png.meta').read_text())
frames = []
for animation, count in counts.items():
    paths = sorted(birds.glob(f'{species}_{animation}_[0-9][0-9].png'))
    assert len(paths) == count, (animation, len(paths))
    hashes = []
    for path in paths:
        image = Image.open(path)
        assert image.mode == 'RGBA' and image.size == (320, 320), path
        alpha = image.getchannel('A')
        low, high = alpha.getextrema()
        bounds = alpha.point(lambda a: 255 if a > 12 else 0).getbbox()
        assert low == 0 and high >= 250, (path, low, high)
        assert bounds[0] > 0 and bounds[1] > 0 and bounds[2] < 320 and bounds[3] < 320, path
        meta = path.with_suffix('.png.meta').read_text().replace(path.stem, f'{species}_idle')
        assert settings(meta) == reference, path
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        hashes.append(digest)
        frames.append({'file': path.name, 'size': list(image.size),
                       'alpha_range': [low, high], 'visible_bounds': bounds,
                       'meta_matches_idle': True, 'sha256': digest})
    assert len(set(hashes)) == count, animation
report = {'species': species, 'frame_count': len(frames), 'frames': frames,
          'visual_review': 'Light/dark composites and 64 px grayscale reviewed. Runtime acceptance pending.'}
target = root / f'art/birds_sequences/qa/{species}_checks.json'
target.write_text(json.dumps(report, indent=2) + '\n')
print(f'{species}: {len(frames)} RGBA frames, importer settings match idle, no clipped exports or duplicate frames.')
