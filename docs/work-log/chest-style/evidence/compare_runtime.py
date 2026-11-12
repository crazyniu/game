from PIL import Image,ImageDraw
from pathlib import Path
r=Path(__file__).resolve().parent; game=r.parent/'game-approved-ui-integration'; gui=r/'gui'
items=[('Before: partial',game/'build/approved-ui-integration/gui/chest-layout/chest_18_stars.png'),('After: partial',gui/'chest_18_stars.png'),('After: full',gui/'chest_30_stars.png'),('After: English',gui/'chest_en.png'),('After: rewards',gui/'chest_actual_rewards.png')]
sheet=Image.new('RGB',(360*len(items),680),'#f1f1e9');d=ImageDraw.Draw(sheet)
for i,(label,p) in enumerate(items):
 im=Image.open(p).convert('RGB'); im.thumbnail((360,640));sheet.paste(im,(i*360,40));d.text((i*360+12,12),label,fill='black')
sheet.save(r/'runtime-comparison.png')
# The live homepage and chest actions shown at the same design-pixel scale.
rows=[('Accepted Home Play',gui/'home_lettering_reference.png',(270,1330,810,1580)),('Chest Play: reused Home word',gui/'chest_18_stars.png',(270,1610,810,1860)),('Chest Open: enabled',gui/'chest_30_stars.png',(270,1420,810,1670)),('Chest Open: disabled',gui/'chest_18_stars.png',(270,1420,810,1670)),('Chest Confirm',gui/'chest_actual_rewards.png',(270,1500,810,1750))]
words=Image.new('RGB',(560,300*len(rows)),'#f1f1e9');d=ImageDraw.Draw(words)
for i,(label,p,box) in enumerate(rows):
 im=Image.open(p).convert('RGB').crop(box);words.paste(im,(10,i*300+35));d.text((10,i*300+10),label,fill='black')
words.save(r/'home-chest-lettering.png')
