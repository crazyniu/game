"""Use the accepted pilot exporter with this batch's evidence and idle metadata."""
from pathlib import Path

source = Path(__file__).with_name('prepare_sequences.py')
script = source.read_text()
script = script.replace("ROOT / 'tasks/evidence/T54G'",
                        "ROOT / 'docs/work-log/remaining-species-sequences/evidence'")
script = script.replace("OUT / 'sunny_idle.png.meta'", "OUT / f'{species}_idle.png.meta'")
script = script.replace("fresh.replace('sunny_idle', name)",
                        "fresh.replace(f'{species}_idle', name)")
exec(compile(script, str(source), 'exec'))
