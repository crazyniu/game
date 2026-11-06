#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
python3 - "$repo_root" <<'PY'
import datetime
import hashlib
import os
import pathlib
import plistlib
import re
import subprocess
import sys

root = pathlib.Path(sys.argv[1])
output = root / 'build/sound-probe'
output.mkdir(parents=True, exist_ok=True)
log = output / 'probe.log'
log.write_text('Silent sound probe ' + datetime.datetime.now(datetime.timezone.utc).isoformat() + '\n')
settings = root / 'PerchSort/ProjectSettings/ProjectSettings.asset'
original_settings = settings.read_bytes()
source = original_settings.decode()
store = (root / 'PerchSort/Assets/Scripts/Game/ProgressStore.cs').read_text()
muted_key = re.search(r'public const string MutedKey = "([^"]+)";', store)[1]
company = re.search(r'^  companyName: (.+)$', source, re.M)[1]
product = re.search(r'^  productName: (.+)$', source, re.M)[1]
preferences = pathlib.Path.home() / 'Library/Preferences'
real_prefs = preferences / f'unity.{company}.{product}.plist'
real_bytes = real_prefs.read_bytes() if real_prefs.exists() else None
domain = f'unity.PerchSortSoundProbe.{product}'
probe_prefs = preferences / (domain + '.plist')
if probe_prefs.exists() and plistlib.loads(probe_prefs.read_bytes()):
    raise SystemExit('The dedicated sound-probe preference domain already exists; preserve it before rerunning.')
seed = output / 'profile-copy.plist'
seed.write_bytes(real_bytes if real_bytes is not None else plistlib.dumps({}))

def record(message):
    print(message, flush=True)
    with log.open('a') as stream:
        stream.write(message + '\n')

record('base=' + subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=root, text=True).strip())
record(f'realSavedMute={plistlib.loads(seed.read_bytes()).get(muted_key, 0)} isolatedCompany=PerchSortSoundProbe')
record('command=Unity -batchmode -projectPath PerchSort -executeMethod PerchSort.Editor.GameAudioProbe.Run -logFile build/sound-probe/unity.log')
try:
    subprocess.run(['defaults', 'import', domain, str(seed)], check=True)
    settings.write_text(re.sub(r'^  companyName: .+$', '  companyName: PerchSortSoundProbe', source, count=1, flags=re.M))
    unity = os.environ.get('UNITY_EDITOR', '/Applications/Unity/Hub/Editor/6000.6.7f1/Unity.app/Contents/MacOS/Unity')
    result = subprocess.run([unity, '-batchmode', '-projectPath', str(root / 'PerchSort'),
        '-executeMethod', 'PerchSort.Editor.GameAudioProbe.Run', '-logFile', str(output / 'unity.log')], timeout=1800)
    record(f'unityExitCode={result.returncode}')
finally:
    settings.write_bytes(original_settings)
    subprocess.run(['defaults', 'delete', domain], check=True)
    after = real_prefs.read_bytes() if real_prefs.exists() else None
    record(f'realProfileByteIdentical={after == real_bytes}')
    record('realProfileSHA256=' + (hashlib.sha256(after).hexdigest() if after is not None else 'absent'))
    if after != real_bytes:
        raise RuntimeError('The original profile changed during the probe; it has not been overwritten or restored.')
sys.exit(result.returncode)
PY
