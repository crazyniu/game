#!/usr/bin/env python3
"""Compose the supplied mockup and unmodified silent capture at equal 1280px height."""
import json
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
DOCS = Path(__file__).resolve().parent
summaries = json.loads((DOCS / 'rect-summary.json').read_text())
records = []
for row in summaries:
    reference = Path('/tmp') / (row['screen'] + '.png')
    candidates = sorted((ROOT / 'build/screens/popup-frame').glob('*/' + row['capture']))
    capture = candidates[0]
    canvas = Image.new('RGB', (1440, 1280))
    for index, source in enumerate((reference, capture)):
        with Image.open(source) as image:
            canvas.paste(image.convert('RGB').resize((720, 1280), Image.Resampling.LANCZOS), (720 * index, 0))
    stem = Path(row['capture']).with_suffix('').as_posix().replace('/', '-').replace('_', '-').lower()
    path = DOCS / (stem + '.jpg')
    canvas.save(path, quality=88, subsampling=0)
    records.append(dict(row, comparison=path.name, raw_capture=str(capture.relative_to(ROOT))))
(DOCS / 'comparisons.json').write_text(json.dumps(records, indent=2) + '\n')
print(f'{len(records)} comparisons, left mockup / right runtime, each 1440 x 1280.')
