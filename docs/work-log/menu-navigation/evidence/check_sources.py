from pathlib import Path
import hashlib
import json

root = Path(__file__).resolve().parents[4]
log = root / 'docs/work-log/menu-navigation'
expected = json.loads((log / 'frozen-sources.json').read_text())
for name, digest in expected.items():
    if name == 'settings_function':
        content = (root / 'PerchSort/game/popups.gd').read_text()
        start = content.index('static func settings(')
        stop = content.index('\n## A document page', start)
        data = content[start:stop].encode()
    else:
        data = (root / 'PerchSort/game' / name).read_bytes()
    assert hashlib.sha256(data).hexdigest() == digest, f'Frozen source changed: {name}'
    print('PASS frozen source', name)
for asset in json.loads((log / 'assets.json').read_text()):
    path = root / asset['path']
    assert hashlib.sha256(path.read_bytes()).hexdigest() == asset['sha256'], f'Accepted art changed: {path}'
    print('PASS retained art', asset['path'])
