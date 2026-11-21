#!/usr/bin/env bash
# Capture one real Cocos state in a separate, pinned Orca browser tab.
# Existing build/server required: ./run.sh serve (use NO_OPEN=1 if desired).
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
python3 - "$root" "$@" <<'PY'
import argparse
import base64
import json
import os
from pathlib import Path
import platform
import shutil
import subprocess
import sys
import time
from urllib.parse import urlencode, urlparse

root = Path(sys.argv[1])
parser = argparse.ArgumentParser(description='Capture the actual Cocos game through Orca; never touches an existing browser tab.')
parser.add_argument('output', type=Path, help='Local PNG output path')
parser.add_argument('--page', choices=['home', 'map', 'shop', 'aviary'], default='home')
parser.add_argument('--world', type=int)
parser.add_argument('--level', type=int)
parser.add_argument('--popup', choices=['settings', 'daily', 'draw', 'chest', 'level'])
parser.add_argument('--card', type=int, default=1)
parser.add_argument('--lang', choices=['en', 'zh'], default='en')
parser.add_argument('--unlock', action='store_true', help='Use an explicitly unlocked isolated test profile')
parser.add_argument('--viewport', action='store_true', help='Also save an Orca viewport screenshot; its color conversion may differ from the game framebuffer')
parser.add_argument('--motion', action='store_true', help='Keep animation enabled; a screenshot alone does not verify motion')
parser.add_argument('--wait', type=float, default=.5, help='Seconds after readiness before capture (0..60)')
parser.add_argument('--outfit', help='own and equip a skin in the isolated test profile, e.g. cocoa.4')
parser.add_argument('--frames', type=int, default=1, help='Number of framebuffer captures (1..120); names end in -NNN when more than one')
parser.add_argument('--step', type=float, default=.25, help='Seconds between captures')
parser.add_argument('--preview', help='Supported legacy state alias; unsupported states fail explicitly')
parser.add_argument('--eval', help='JavaScript run once the game is ready (for example perch.app.screen.show_detail("oriole")); the capture waits for the art it loads')
parser.add_argument('--url', default='http://127.0.0.1:' + os.environ.get('WEB_PORT', '8765') + '/')
args = parser.parse_args(sys.argv[2:])
if not 1 <= args.frames <= 120:
    parser.error('--frames must be within 1..120')
if args.wait < 0 or args.wait > 60:
    parser.error('--wait must be between 0 and 60 seconds')
if args.output.suffix.lower() != '.png':
    parser.error('output must be a PNG path')
parsed = urlparse(args.url)
if parsed.scheme != 'http' or parsed.hostname not in {'127.0.0.1', 'localhost', '::1'}:
    parser.error('--url must name the local Cocos development server')
if parsed.query or parsed.fragment:
    parser.error('--url is the base address; pass state with the capture options')
if args.preview:
    aliases = {
        'home_view:home': ('home', None, None),
        'home_view:home_new': ('home', None, None),
        'map_view:cards': ('map', None, None),
        'map_view:world_page': ('map', 1, None),
        'map_view:world_page_last': ('map', 10, None),
        'shop_view:boosters': ('shop', None, None),
        'aviary_view:list': ('aviary', None, None),
        'popups:settings_sound_on': ('home', None, 'settings'),
        'popups:daily_reward': ('home', None, 'daily'),
        'popups:lucky_draw': ('home', None, 'draw'),
        'popups:chest': ('home', None, 'chest'),
        'popups:level_card_first_visit': ('map', 1, 'level'),
    }
    if args.preview.startswith('board:level_'):
        args.level = int(args.preview.removeprefix('board:level_'))
    elif args.preview in aliases:
        args.page, args.world, args.popup = aliases[args.preview]
    else:
        parser.error('unsupported preview state; use --page/--world/--level/--popup for current Cocos routes')
if args.level is not None and not 1 <= args.level <= 210:
    parser.error('--level must be within 1..210')
if args.world is not None and not 1 <= args.world <= 10:
    parser.error('--world must be within 1..10')
if not 1 <= args.card <= 210:
    parser.error('--card must be within 1..210')

if os.environ.get('ORCA_CLI_COMMAND'):
    cli = os.environ['ORCA_CLI_COMMAND']
elif os.environ.get('ORCA_DEV_REPO_ROOT'):
    cli = 'orca-dev'
elif platform.system() == 'Linux':
    cli = 'orca-ide'
else:
    cli = 'orca'


def call(*command):
    completed = subprocess.run([cli, *command, '--json'], text=True, capture_output=True, cwd=root, timeout=60)
    if completed.returncode:
        raise RuntimeError(completed.stderr.strip() or completed.stdout.strip())
    try:
        response = json.loads(completed.stdout)
    except ValueError:
        raise RuntimeError('%s returned no JSON: %r' % (command[0], completed.stdout[:200]))
    if response.get('ok') is False:
        raise RuntimeError(json.dumps(response['error']))
    return response['result']


def evaluate(expression):
    result = call('eval', '--page', page, '--expression', expression)['result']
    if not isinstance(result, str):
        return result
    try:
        return json.loads(result)
    except ValueError:
        return result   # a plain string such as the canvas data URL


query = {'test': 'capture', 'fresh': '1', 'lang': args.lang, 'page': args.page}
if not args.motion:
    query['instant'] = '1'
if args.unlock:
    query['unlock'] = '1'
if args.outfit:
    query['outfit'] = args.outfit
for key in ('world', 'level', 'popup'):
    value = getattr(args, key)
    if value is not None:
        query[key] = str(value)
if args.popup == 'level':
    query['card'] = str(args.card)
url = args.url + '?' + urlencode(query)
page = None
try:
    page = call('tab', 'create', '--url', url)['browserPageId']
    call('viewport', '--page', page, '--width', '1080', '--height', '1920', '--scale', '1')
    call('snapshot', '--page', page, '--interactive')
    deadline = time.monotonic() + 120
    while True:
        state = evaluate("({ready:!!globalThis.perch?.app.ready, pending:globalThis.perch?.Art.pending.size, startupError:globalThis.perchError||null, errors:globalThis.perch?.Art.errors||[]})")
        if state['startupError'] or state['errors']:
            raise RuntimeError('Cocos runtime failed: ' + json.dumps(state))
        if state['ready'] and state['pending'] == 0:
            if args.eval:
                evaluate(args.eval + ';0')
                args.eval = None
                time.sleep(.5)
                continue
            break
        if time.monotonic() >= deadline:
            raise RuntimeError('Cocos did not become ready. Start the local server and use a debug build with the perch test API.')
        time.sleep(.5)
    time.sleep(args.wait)
    output = args.output.expanduser().resolve()
    output.parent.mkdir(parents=True, exist_ok=True)
    for index in range(args.frames):
        image = evaluate("new Promise(resolve=>requestAnimationFrame(()=>resolve(document.querySelector('canvas').toDataURL('image/png'))))")
        if not isinstance(image, str) or not image.startswith('data:image/png;base64,'):
            raise RuntimeError('Cocos canvas returned no PNG framebuffer')
        target = output if args.frames == 1 else output.with_name('%s-%03d%s' % (output.stem, index, output.suffix))
        target.write_bytes(base64.b64decode(image.partition(',')[2], validate=True))
        print('Cocos framebuffer: ' + str(target))
        if index + 1 < args.frames:
            time.sleep(args.step)
    if args.viewport:
        source = Path(call('screenshot', '--page', page, '--format', 'png')['path'])
        viewport = output.with_name(output.stem + '-viewport.png')
        shutil.copyfile(source, viewport)
        print('Orca viewport screenshot: ' + str(viewport))
except (OSError, ValueError, KeyError, RuntimeError, subprocess.TimeoutExpired) as error:
    print('Capture failed: ' + str(error), file=sys.stderr)
    sys.exit(1)
finally:
    if page:
        try:
            call('tab', 'close', '--page', page)
        except (OSError, ValueError, KeyError, RuntimeError, subprocess.TimeoutExpired) as error:
            print('Could not close capture tab: ' + str(error), file=sys.stderr)
PY
