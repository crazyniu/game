#!/usr/bin/env bash
# Compare a reference with a newly captured current Cocos route.
# Usage: scripts/compare.sh <reference PNG or approved_ui name> <preview alias> [capture options]
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
if [[ "$#" -lt 2 ]]; then
  echo 'Usage: scripts/compare.sh <reference PNG or approved_ui name> <preview alias> [capture options]' >&2
  echo 'Example: scripts/compare.sh home home_view:home --lang=zh' >&2
  exit 2
fi
reference="$1"; state="$2"; shift 2
if [[ ! -f "$reference" ]]; then reference="$root/art/approved_ui/$reference.png"; fi
if [[ ! -f "$reference" ]]; then echo "Reference does not exist: $reference" >&2; exit 2; fi
name="${state//:/__}"
name="${name//\//_}"
out_dir="$root/docs/work-log/cocos-port/evidence"
shot="$root/build/screens/$name.png"
mkdir -p "$out_dir"
bash "$root/scripts/capture.sh" "$shot" "--preview=$state" "$@"
node - "$root" "$reference" "$shot" "$out_dir/$name.png" <<'JS'
const [root, reference, capture, output] = process.argv.slice(2);
const sharp = require(root + '/PerchSort/node_modules/sharp');
(async () => {
  const height = 1280;
  const pictures = await Promise.all([reference, capture].map(async file => {
    const metadata = await sharp(file).metadata();
    const width = Math.round(metadata.width * height / metadata.height);
    return { width, input: await sharp(file).resize(width, height).png().toBuffer() };
  }));
  await sharp({ create: { width: pictures[0].width + pictures[1].width + 24, height, channels: 3, background: '#ffffff' } })
    .composite([{ input: pictures[0].input, left: 0, top: 0 }, { input: pictures[1].input, left: pictures[0].width + 24, top: 0 }])
    .png().toFile(output);
  console.log(output);
})().catch(error => { console.error('Comparison failed:', error.message); process.exitCode = 1; });
JS
