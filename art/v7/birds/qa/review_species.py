"""Game-size contact sheet for visual body-scale, head-feature and pose review."""
from pathlib import Path
from PIL import Image, ImageDraw
import sys
ROOT=Path(__file__).resolve().parents[4]
STATES='idle blink quirk1 quirk2 fly_up fly_down cheer sleep tilt_left tilt_right look_up look_down look_side preen fluff stretch yawn'.split()
for species in sys.argv[1:]:
    board=Image.new('RGB',(985,880),'#e9e5df')
    draw=ImageDraw.Draw(board)
    for index,state in enumerate(STATES):
        sprite=Image.open(ROOT/f'PerchSort/Assets/Resources/V7/Birds/{species}_{state}.png').resize((197,197),Image.Resampling.LANCZOS)
        x,y=(index%5)*197,(index//5)*220
        board.paste(sprite,(x,y),sprite)
        draw.text((x+6,y+200),state,fill='#302e2c')
    board.save(ROOT/f'art/v7/birds/qa/{species}_game_size.png')
