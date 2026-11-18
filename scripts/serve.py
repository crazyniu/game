#!/usr/bin/env python3
"""Serve the local Cocos build and read the prior Godot save without modifying it."""
import argparse
import json
import os
import platform
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument('--port',type=int,default=8765)
args=parser.parse_args()
if platform.system()=='Darwin':
    legacy=Path.home()/'Library/Application Support/Godot/app_userdata/Perch Sort/profile.json'
elif platform.system()=='Windows':
    legacy=Path(os.environ['APPDATA'])/'Godot/app_userdata/Perch Sort/profile.json'
else:
    legacy=Path.home()/'.local/share/godot/app_userdata/Perch Sort/profile.json'
class Handler(SimpleHTTPRequestHandler):
    def __init__(self,*a,**kw):
        super().__init__(*a,directory=str(root/'PerchSort/build/web-mobile'),**kw)
    def do_GET(self):
        if self.path=='/__profile':
            payload=legacy.read_bytes() if legacy.exists() else b'null'
            self.send_response(200)
            self.send_header('Content-Type','application/json')
            self.send_header('Cache-Control','no-store')
            self.send_header('Content-Length',str(len(payload)))
            self.end_headers(); self.wfile.write(payload)
        else:
            super().do_GET()
    def log_message(self,*args):
        pass
print(f'Perch Sort: http://127.0.0.1:{args.port}',flush=True)
class LocalGameServer(ThreadingHTTPServer):
    request_queue_size = 128

LocalGameServer(('127.0.0.1', args.port), Handler).serve_forever()
