"""Use the accepted pilot exporter with this batch's evidence and idle metadata."""
from pathlib import Path

source = Path(__file__).with_name('prepare_sequences.py')
script = source.read_text()
script = script.replace("ROOT / 'docs/work-log/bird-frame-sequences/evidence'",
                        "ROOT / 'docs/work-log/remaining-species-sequences/evidence'")
script = script.replace("OUT / 'sunny_idle.png.meta'", "OUT / f'{species}_idle.png.meta'")
script = script.replace("fresh.replace('sunny_idle', name)",
                        "fresh.replace(f'{species}_idle', name)")
# Crow has neutral grey toes; the pilot's orange selector would find its compass.
script = script.replace('    bottom = max(y for x, y in points)', '''    if species == 'crow':
        points = [(x, y) for y in range(round(im.height * .75), im.height)
                  for x in range(im.width)
                  if (lambda r, g, b, a: a > 235 and b >= r and
                      max(r, g, b) - min(r, g, b) < 55)(*im.getpixel((x, y)))]
    bottom = max(y for x, y in points)''')
exec(compile(script, str(source), 'exec'))
