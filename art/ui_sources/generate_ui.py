"""Original Perch Sort UI geometry. Run with Python + Pillow; no art originals read or changed."""
from pathlib import Path
from PIL import Image, ImageDraw
import math
OUT = Path(__file__).resolve().parents[2] / 'PerchSort/Assets/Art/UI'
OUT.mkdir(exist_ok=True)
S = 4
ink = '#435437'
def canvas():
    im = Image.new('RGBA', (128*S,128*S))
    return im, ImageDraw.Draw(im)
def save(im,name):
    im.resize((128,128),Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
for name,fill,border in [('panel','#FFF5DC','#C4B18A'),('green','#526E40','#3D5630'),('cream','#F3E6C7','#B9A47C'),('wood','#DDBB82','#9E7B4B')]:
    im,d=canvas()
    d.rounded_rectangle((2*S,2*S,126*S,126*S),radius=30*S,fill=fill,outline=border,width=3*S)
    save(im,name)
for name in ['undo','restart','pause','lock','check','back','home']:
    im,d=canvas()
    def line(points,width=9): d.line([(x*S,y*S) for x,y in points],fill=ink,width=width*S,joint='curve')
    if name == 'undo':
        line([(102,94),(102,67),(86,49),(30,49)])
        d.polygon([(17*S,49*S),(48*S,24*S),(48*S,74*S)],fill=ink)
    elif name == 'restart':
        d.arc((25*S,29*S,102*S,105*S),195,500,fill=ink,width=9*S)
        d.polygon([(14*S,38*S),(49*S,14*S),(47*S,57*S)],fill=ink)
    elif name=='pause':
        for x in (37,73):d.rounded_rectangle((x*S,29*S,(x+17)*S,99*S),radius=5*S,fill=ink)
    elif name=='lock':
        d.arc((39*S,20*S,89*S,80*S),180,360,fill=ink,width=8*S)
        d.rounded_rectangle((29*S,55*S,99*S,108*S),radius=10*S,fill=ink)
        d.ellipse((59*S,70*S,69*S,80*S),fill='#F3E6C7')
        line([(64,78),(64,91)],5)
    elif name=='check':line([(26,64),(51,87),(101,36)],12)
    elif name=='back':line([(74,29),(39,64),(74,99)])
    elif name=='home':
        line([(20,62),(64,24),(108,62)])
        line([(34,56),(34,105),(94,105),(94,56)])
    save(im,name)
