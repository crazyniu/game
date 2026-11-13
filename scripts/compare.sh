#!/usr/bin/env bash
# Design | capture side by side, saved in the project so it can be shown:
#   scripts/compare.sh <design name in art/approved_ui> <preview state> [--lang=zh]
# e.g. scripts/compare.sh shop shop_view:boosters      -> docs/work-log/island-screens/evidence/shop.png
# A screen is not reported done without this image.
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
design="$root/art/approved_ui/$1.png"; state="$2"; shift 2
out_dir="$root/docs/work-log/island-screens/evidence"
mkdir -p "$out_dir"
shot="$root/build/screens/${state/:/__}.png"
bash "$root/scripts/capture.sh" "$shot" "--preview=$state" "$@"
python3 - "$design" "$shot" "$out_dir/$(basename "$design")" <<'PY'
import sys
from PIL import Image
a, b, out = (Image.open(sys.argv[1]).convert("RGB"), Image.open(sys.argv[2]).convert("RGB"), sys.argv[3])
h = 1280
a, b = (i.resize((round(i.width * h / i.height), h)) for i in (a, b))
sheet = Image.new("RGB", (a.width + b.width + 24, h), "white")
sheet.paste(a, (0, 0)); sheet.paste(b, (a.width + 24, 0))
sheet.save(out)
print(out)
PY
