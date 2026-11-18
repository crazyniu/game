#!/usr/bin/env python3
"""sync_skins.py: install every finished skin into the game resources, forever (Ctrl-C to stop).

Two sources: skins this machine finished (a "done <key>" line in out/run_skins.status, published with publish_skin.py) and zips the
Mac uploaded to the transfer gateway (perchsort/skins/<key>.zip, unpacked with install_skin.py). Installed keys are listed in
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
                if subprocess.run([sys.executable, HERE + "/publish_skin.py", key]).returncode == 0:
                    mark(key, "251"); have.add(key)
    try:
        entries = json.load(urllib.request.urlopen(BASE + "/list?path=perchsort%2Fskins", timeout=20))["entries"]
        for e in entries:
            key = e["name"][:-4]
            if e["name"].endswith(".zip") and key not in have:
                zp = "%s/%s.zip" % (OUT, key)
                urllib.request.urlretrieve(BASE + "/file?path=" + urllib.parse.quote("perchsort/skins/" + e["name"], safe=""), zp)
                if subprocess.run([sys.executable, HERE + "/install_skin.py", key, zp]).returncode == 0:
                    mark(key, "mac"); have.add(key)
    except Exception as ex:
        print("gateway:", ex, flush=True)
    time.sleep(60)
