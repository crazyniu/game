from pathlib import Path
import hashlib,json
from PIL import Image,ImageDraw
root=Path(__file__).resolve().parents[4]
shots=root/'build/dialog-surface/screens'; out=root/'docs/work-log/dialog-surface/evidence'
def sheet(paths,target,cols=7,width=216):
 rows=(len(paths)+cols-1)//cols;h=424
 im=Image.new('RGB',(cols*width,rows*h),(24,32,24)); d=ImageDraw.Draw(im)
 for i,p in enumerate(paths):
  x=i%cols*width;y=i//cols*h
  pic=Image.open(p).convert('RGB');pic.thumbnail((width-6,386));im.paste(pic,(x+3,y+24))
  d.text((x+4,y+5),p.stem[:31],fill=(240,242,224))
 im.save(target,quality=90)
for lang in ['en','zh']:
 sheet(sorted(p for p in shots.glob('*_'+lang+'.png') if not p.name.startswith('reference_')),out/('states-'+lang+'.jpg'))
refs=[shots/'reference_shop_view_boosters.png',shots/'reference_coin_shop_view_normal.png',shots/'reference_coin_shop_view_purchase_preview.png',shots/'reference_popups_chest_ready.png',shots/'win_three_stars_en.png',shots/'new_friend_berry_en.png',shots/'level_card_full_roster_en.png']
sheet(refs,out/'actual-surface-comparison.jpg',7,300)
for state in ['win_three_stars','level_card_full_roster','rewards_challenge_all','new_friend_berry','unlock_owl_eye','world_complete_1','world_unlock_2']:
 before=Path('/workspace/scratch/2bd695cff14d/game-approved-ui-integration/build/dialog-consistency/screens')/(state+'_en.png')
 sheet([before,shots/(state+'_en.png')],out/(state+'-comparison.jpg'),2,380)
manifest=[]
for p in sorted(shots.glob('*.png')):
 im=Image.open(p)
 assert im.size==(1080,1920),p
 manifest.append({'file':p.name,'size':list(im.size),'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
(out/'screens.json').write_text(json.dumps(manifest,indent=2))
for directory in sorted((root/'build/dialog-surface/motion').glob('*')):
 frames=sorted(directory.glob('f*.png'))
 if len(frames)==16: sheet(frames,out/(directory.name+'-motion.jpg'),8,135)
