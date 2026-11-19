#!/usr/bin/env python3
"""Write stable Cocos asset identities; generated caches never belong in Git."""
import json
import uuid
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1] / 'PerchSort/assets'
NS = uuid.UUID('9a21e085-c160-43cc-b108-f67df530e04d')
manifest = json.loads((ROOT / 'resources/manifest.json').read_text())
def num(v): return int(v) if v == int(v) else v
def vertices(w,h):
    # What Creator writes for an untrimmed sprite frame, so a build leaves the metas as they are.
    x,y=num(w/2),num(h/2)
    return {'rawPosition':[-x,-y,0,x,-y,0,-x,y,0,x,y,0],'indexes':[0,1,2,2,1,3],'uv':[0,h,w,h,0,0,w,0],'nuv':[0,0,1,0,0,1,1,1],'minPos':[-x,-y,0],'maxPos':[x,y,0]}
for p in sorted(ROOT.rglob('*')):
    if p.suffix in ('.meta', '.import', '.uid') or p.name.startswith('.'):
        continue
    target = Path(str(p) + '.meta')
    if target.exists():
        continue
    key = p.relative_to(ROOT).as_posix()
    ident = str(uuid.uuid5(NS, key))
    kind = 'directory' if p.is_dir() else {'.png':'image','.webp':'image','.ts':'typescript','.json':'json','.wav':'audio-clip','.effect':'effect','.scene':'scene','.svg':'text'}.get(p.suffix,'text')
    ver = {'directory':'1.2.0','image':'1.0.27','typescript':'4.0.24','json':'1.0.0','audio-clip':'1.0.1','effect':'1.7.1','scene':'1.1.50','text':'1.0.0'}[kind]
    data = {'ver':ver,'importer':kind,'imported':True,'uuid':ident,'files':[],'subMetas':{},'userData':{}}
    if kind=='image':
        w,h=manifest['images'][key.removeprefix('resources/').removesuffix(p.suffix)]['size']
        data['files']=['.json',p.suffix]; data['userData']={'type':'sprite-frame','redirect':ident+'@6c48a','hasAlpha':True,'fixAlphaTransparencyArtifacts':True}
        data['subMetas']['6c48a']={'ver':'1.0.22','importer':'texture','imported':True,'uuid':ident+'@6c48a','displayName':p.stem,'id':'6c48a','name':'texture','files':['.json'],'subMetas':{},'userData':{'wrapModeS':'clamp-to-edge','wrapModeT':'clamp-to-edge','minfilter':'linear','magfilter':'linear','mipfilter':'none','anisotropy':0,'isUuid':True,'imageUuidOrDatabaseUri':ident,'visible':False}}
        data['subMetas']['f9941']={'ver':'1.0.12','importer':'sprite-frame','imported':True,'uuid':ident+'@f9941','displayName':p.stem,'id':'f9941','name':'spriteFrame','files':['.json'],'subMetas':{},'userData':{'trimType':'none','trimThreshold':1,'rotated':False,'offsetX':0,'offsetY':0,'trimX':0,'trimY':0,'width':w,'height':h,'rawWidth':w,'rawHeight':h,'borderTop':0,'borderBottom':0,'borderLeft':0,'borderRight':0,'packable':False,'pixelsToUnit':100,'pivotX':.5,'pivotY':.5,'meshType':0,'isUuid':True,'imageUuidOrDatabaseUri':ident+'@6c48a','atlasUuid':'','vertices':vertices(w,h)}}
    target.write_text(json.dumps(data,indent=2)+'\n')
print('Cocos asset identities ready')
