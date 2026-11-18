#!/usr/bin/env python3
"""Give an exported Xcode project the simulator archives provided with Creator."""
import re
import sys
from pathlib import Path

project = Path(sys.argv[1]).resolve()
path = project / 'project.pbxproj'
source = path.read_text()
lines = []
for line in source.splitlines(keepends=True):
    if 'OTHER_LDFLAGS[sdk=iphonesimulator*]' in line:
        continue
    if 'VALID_ARCHS[sdk=iphonesimulator*]' in line:
        line = line.replace('= x86_64;', '= "arm64 x86_64";')
    if 'EXCLUDED_ARCHS[sdk=iphonesimulator*]' in line:
        line = line.replace('= arm64;', '= "";')
    if 'OTHER_LDFLAGS = (' in line and '/external/ios/libs/' in line:
        libs = project.parent.parent / 'libwebp'
        pattern = r'"?[^,()"]*(?:/external/ios/libs/libwebp|/libwebp/device)\.a"?'
        lines.append(re.sub(pattern, '"' + str(libs / 'device.a') + '"', line))
        # The iOS arm64 archive has a different target platform from arm64 simulator.
        sim_line = re.sub(pattern, '"' + str(libs / 'simulator.a') + '"', line).replace(',"$(inherited)"','')
        lines.append(sim_line.replace('OTHER_LDFLAGS =', '"OTHER_LDFLAGS[sdk=iphonesimulator*]" =')
                     .replace('/external/ios/libs/', '/external/ios-m1-simulator/libs/'))
    else:
        lines.append(line)
result = ''.join(lines)
if result != source:
    path.write_text(result)
