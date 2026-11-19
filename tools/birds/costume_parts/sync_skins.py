#!/usr/bin/env python3
"""sync_skins.py: install every finished skin into the game resources, forever (Ctrl-C to stop).

Every "done <key>" line in out/run_skins.status is installed: a skin (sunny_1) with publish_skin.py, a world bird (sunny_oriole) with install_world.py. Installed keys are listed in
out/installed.txt. Run from the repository root with pillow available.
"""
import json, os, subprocess, sys, time, urllib.request, urllib.parse

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = HERE + "/out"
BASE = "http://192.168.1.250/api/transfer"
done_file = OUT + "/installed.txt"


def installed():
    return set(open(done_file).read().split()) if os.path.exists(done_file) else set()


def mark(key, source):
    open(done_file, "a").write(key + "\n")
    print("installed %s (%s)" % (key, source), flush=True)


while True:
    have = installed()
    status = OUT + "/run_skins.status"
    if os.path.exists(status):
        for line in open(status):
            parts = line.split()
            if len(parts) >= 4 and parts[2] == "done" and parts[3] not in have:
                key = parts[3]
                script = "publish_skin.py" if key.split("_")[1].isdigit() else "install_world.py"
                if subprocess.run([sys.executable, HERE + "/" + script, key]).returncode == 0:
                    mark(key, "251"); have.add(key)
    time.sleep(60)
