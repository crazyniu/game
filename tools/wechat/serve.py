#!/usr/bin/env python3
"""Serve generated WeChat remote assets locally for Developer Tools preview."""
import argparse
from functools import partial
from http.server import SimpleHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path

root=Path(__file__).resolve().parents[2]/'PerchSort/build/wechatgame/remote'
parser=argparse.ArgumentParser(description=__doc__)
parser.add_argument('--port',type=int,default=8766)
args=parser.parse_args()
if not root.is_dir():
    parser.error('Build the WeChat game first: ./run.sh wechat-build')
class Server(ThreadingHTTPServer):
    request_queue_size=128
print(f'WeChat remote assets: http://127.0.0.1:{args.port}/',flush=True)
Server(('127.0.0.1',args.port),partial(SimpleHTTPRequestHandler,directory=str(root))).serve_forever()
