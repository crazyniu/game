#!/usr/bin/env python3
"""Re-archive Creator's unaligned iOS device objects for the arm64 Apple linker."""
import struct
import subprocess
import sys
import tempfile
from pathlib import Path

source, output = map(lambda arg: Path(arg).resolve(), sys.argv[1:3])
if output.exists() and output.stat().st_mtime > max(source.stat().st_mtime, Path(__file__).stat().st_mtime):
    sys.exit(0)
output.parent.mkdir(parents=True, exist_ok=True)
with tempfile.TemporaryDirectory() as tmp:
    directory = Path(tmp)
    thin = directory / 'arm64.a'
    archive = source.read_bytes()
    if archive[:8] != b'!<arch>\n':
        subprocess.run(['xcrun', 'lipo', str(source), '-thin', 'arm64', '-output', str(thin)], check=True)
        archive = thin.read_bytes()
    if archive[:8] != b'!<arch>\n':
        raise ValueError('Not an ar archive: ' + str(source))
    pos = 8
    objects = []
    while pos + 60 <= len(archive):
        header = archive[pos:pos + 60]
        name = header[:16].strip()
        size = int(header[48:58])
        data = archive[pos + 60:pos + 60 + size]
        if name.startswith(b'#1/'):
            data = data[int(name[3:]):]
        # Exclude the old symbol table while retaining identically named members.
        if data[:4] == struct.pack('<I', 0xfeedfacf):
            obj = directory / f'{len(objects)}.o'
            obj.write_bytes(data)
            objects.append(str(obj))
        pos += 60 + size + size % 2
    if not objects:
        raise ValueError('No arm64 Mach-O objects in ' + str(source))
    subprocess.run(['xcrun', 'libtool', '-static', '-o', str(output), *objects], check=True)
