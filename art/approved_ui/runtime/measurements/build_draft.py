"""Reproduce manually inspected 50%-scale screen measurements in source pixels."""
import json
from pathlib import Path
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent

def box(x,y,w,h):
    return {'center':[int(2*x+w),int(2*y+h)],'size':[int(2*w),int(2*h)],'rect':[int(2*x),int(2*y),int(2*w),int(2*h)]}
def screen(name, elements, **extra):
    return {'source':'art/approved_ui/ui_'+name+'.png','evidence':'observed_manual_bounds','uncertainty_px':8,'elements':{k:box(*v) for k,v in elements.items()},**extra}
S={}
S['level_k3']=screen('level_k3_v2',{
'coin_pill':(28,74,114,45),'coin_number_text':(71,82,59,26),'gear':(433,70,57,55),
'house':(200,76,123,116),'house_rope':(253,0,15,84),'house_door':(237,121,59,65),
'board_band':(0,379,540,360),'branch_1':(0,451,263,33),'branch_2':(241,532,299,36),'branch_3':(0,624,262,37),'branch_4':(237,703,303,38),
'bird_nominal':(72,388,61,76),'undo':(145,874,54,54),'hint':(239,874,54,54),'branch_plus':(333,874,54,54),
'undo_count':(178,865,23,23),'hint_count':(271,865,23,23),'branch_plus_count':(365,865,23,23)},
branches=[{'side':'left','perch_line':[[0,944],[510,920]],'occupied_bird_centers':[[98,866],[206,868],[322,861],[428,866]]},
{'side':'right','perch_line':[[500,1080],[1068,1105]],'occupied_bird_centers':[[592,1005],[714,1000],[819,1005],[937,1008]]},
{'side':'left','perch_line':[[0,1298],[514,1270]],'occupied_bird_centers':[[95,1188],[205,1194],[325,1194],[444,1200]]},
{'side':'right','perch_line':[[482,1428],[1080,1452]],'occupied_bird_centers':[[576,1350],[689,1346]]}],
notes=['The approved K3 master depicts one house and four branches. This is observed artwork, not a three-house gameplay rule.','Visible bird counts are artwork evidence only; they do not define level data.'])
S['level_k7']=screen('level_k7',{
'coin_pill':(27,54,120,48),'coin_number_text':(78,64,57,28),'gear':(447,49,57,57),'house_beam':(0,109,540,37),
'house_1':(21,148,75,88),'house_2':(92,148,75,88),'house_3':(162,148,75,88),'house_4':(232,148,75,88),'house_5':(301,148,75,88),'house_6':(371,148,75,88),'house_7':(441,148,75,88),
'board_band':(0,286,540,556),'branch_1':(0,374,320,53),'branch_2':(215,480,325,41),'branch_3':(0,588,307,50),'branch_4':(133,688,407,53),'branch_5':(0,795,322,57),
'scarecrow_perch':(367,340,173,46),'bird_nominal':(105,299,68,84),
'undo':(98,863,64,66),'hint':(188,863,64,66),'branch_plus':(279,863,64,66),'scarecrow':(371,863,66,66),
'undo_count':(138,855,25,25),'hint_count':(228,855,25,25),'branch_plus_count':(319,855,25,25),'scarecrow_disabled':(405,892,29,29)},
branches=[{'side':'left','perch_line':[[20,807],[618,769]],'occupied_bird_centers':[[144,695],[272,689],[405,695],[528,691]]},
{'side':'right','perch_line':[[447,985],[1060,1000]],'occupied_bird_centers':[[550,916],[688,918],[821,904],[958,916]]},
{'side':'left','perch_line':[[20,1246],[606,1197]],'occupied_bird_centers':[[130,1122],[256,1128],[388,1128],[515,1130]]},
{'side':'right','perch_line':[[285,1402],[1062,1432]],'occupied_bird_centers':[[389,1313],[513,1334],[648,1333],[787,1336],[926,1326]]},
{'side':'left','perch_line':[[10,1641],[613,1611]],'occupied_bird_centers':[[136,1538],[263,1532],[400,1545],[530,1540]]}])
S['win']=screen('win',{'panel':(89,371,358,386),'ribbon':(96,346,337,100),'hero_bird':(200,253,120,108),'left_bird':(43,469,124,154),'right_bird':(377,446,132,157),'star_left':(147,431,84,87),'star_center':(223,411,96,95),'star_right':(312,431,84,87),'coin_reward':(183,528,166,51),'chain_reward':(224,587,93,32),'replay':(136,639,70,70),'next':(213,627,104,103),'home':(323,639,70,70)})
S['settings']=screen('settings',{'panel':(85,432,368,237),'ribbon':(120,416,285,83),'close':(400,416,60,62),'sound':(122,520,90,91),'replay':(224,520,90,91),'home':(327,520,90,91)})
S['level_card']=screen('level_card',{'panel':(115,280,306,349),'leaf_number':(212,241,107,73),'close':(370,271,46,47),'star_left':(184,309,55,54),'star_center':(240,309,55,54),'star_right':(296,309,55,54),'stat_pill':(179,369,178,34),'bird_1':(139,411,68,80),'bird_2':(205,411,68,80),'bird_3':(272,411,68,80),'bird_4':(337,411,68,80),'sleeping_bird':(186,492,71,63),'hidden_egg':(275,499,75,59),'play':(225,550,87,90)})
S['unlock_card']=screen('unlock_card',{'panel':(106,285,325,429),'eye_icon':(222,326,94,64),'egg':(213,398,119,138),'title':(175,540,188,44),'description':(151,591,237,24),'confirm':(219,626,96,97)})
S['goal_info']=screen('goal_info',{'panel':(120,181,296,141),'pointer':(251,166,37,22),'bird_frame':(142,202,92,99),'name':(237,213,83,34),'gift_badge':(326,198,61,62),'description':(234,265,167,23)})
S['popup_new_friend']=screen('popup_new_friend',{'hero_bird':(148,261,237,290),'ribbon':(104,531,320,116),'name':(181,559,164,44),'gift_badge':(215,622,99,91),'confirm':(211,723,108,107)},notes=['Replace the depicted gift box with Minty watering-can badge per T48. The badge rectangle is measured from the approved popup.'])
S['popup_daily_gift']=screen('popup_daily_gift',{'gift_basket':(122,342,289,261),'coin_reward':(189,596,151,56),'confirm':(184,665,153,78)})
S['popup_world_complete']=screen('popup_world_complete',{'gate':(51,315,428,364),'ribbon':(87,224,365,121),'star_left':(169,237,64,62),'star_center':(239,228,65,62),'star_right':(309,237,65,62),'hero_bird_yellow':(207,345,115,137),'hero_bird_blue':(134,448,115,133),'hero_bird_pink':(263,472,132,149),'coin_reward':(187,661,166,59),'next':(207,718,128,129)})
S['popup_world_unlock']=screen('popup_world_unlock',{'gate':(38,342,460,452),'world_plaque':(187,380,169,92),'play':(205,709,125,123)})
S['map']=screen('map',{'coin_pill':(25,10,128,53),'gear':(465,9,58,58),'gate':(242,73,222,182),'node_1':(139,695,108,94),'node_2':(180,590,106,94),'node_3':(240,508,99,83),'node_4':(160,406,114,101),'node_5':(243,300,96,71),'node_6':(316,248,90,63),'current_bird':(156,328,96,102),'shop':(107,876,76,76),'aviary':(230,876,76,76),'settings':(353,876,76,76)},notes=['These six visible node positions belong to the approved 1080x1920 concept only, not the new 1080x3000 world backgrounds. Runtime world path node coordinates must be measured separately after generation.'])
S['shop_boosters']=screen('shop_boosters',{'coin_pill':(26,74,120,46),'close':(452,68,61,61),'tab_boosters':(134,128,81,86),'tab_outfits':(232,130,75,75),'tab_aviary':(328,130,75,75),'grid':(31,226,478,670),'card_1':(32,228,233,209),'card_2':(279,228,230,209),'card_3':(32,448,233,213),'card_4':(279,448,230,213),'card_5':(32,669,233,226),'card_6':(279,669,230,226),'locked_level_chip':(439,758,70,37)},notes=['Scarecrow card must use the purple round-disc icon family, correcting the concept.'])
S['shop_outfits']=screen('shop_outfits',{'coin_pill':(26,74,120,46),'close':(452,68,61,61),'tab_boosters':(135,130,75,75),'tab_outfits':(230,128,81,86),'tab_aviary':(328,130,75,75),'grid':(29,228,482,609),'card_1':(29,228,238,295),'card_2':(276,228,235,295),'card_3':(29,538,238,299),'card_4':(276,538,235,299)})
S['shop_confirm']=screen('shop_confirm',{'panel':(126,336,284,326),'close':(336,327,68,68),'product_icon':(196,367,151,147),'price_pill':(190,516,158,52),'confirm':(221,578,96,94)})
S['aviary_list']=screen('aviary_list',{'coin_pill':(24,43,120,47),'close':(460,40,55,58),'board':(10,53,516,907),'grid':(94,114,340,779),'scroll_track':(456,240,12,552),'scroll_thumb':(454,322,17,59), **{f'bird_frame_{i+1}':(95+(i%2)*182,116+(i//2)*196,159,158) for i in range(8)},**{f'name_ribbon_{i+1}':(103+(i%2)*182,246+(i//2)*193,148,49) for i in range(8)},**{f'house_count_chip_{i+1}':(142+(i%2)*182,282+(i//2)*193,70,31) for i in range(8)}})
S['aviary_detail']=screen('aviary_detail',{'coin_pill':(27,74,118,45),'close':(434,68,55,57),'name_ribbon':(106,106,322,79),'name':(202,114,135,44),'description':(111,189,319,28),'hero_bird':(66,223,398,367),'panel':(27,574,482,354),'outfit_1':(80,623,120,122),'outfit_2':(211,623,120,122),'outfit_3':(342,623,120,122),'gift_badge':(88,758,85,73),'effect_icon':(202,771,35,31),'effect_text':(253,773,203,31),'house_icon':(213,840,64,58),'house_count':(286,850,33,35)})
# Repeated grid children measured relative to each card's actual bounds.
for i in range(6):
    x=32 if i%2==0 else 279;y=[228,448,669][i//2]
    S['shop_boosters']['elements'][f'icon_{i+1}']=box(x+60,y+28,120,120)
    S['shop_boosters']['elements'][f'count_{i+1}']=box(x+165,y+11,42,42)
    S['shop_boosters']['elements'][f'price_{i+1}']=box(x+52,y+145+(13 if i>3 else 0),133,49)
for i in range(4):
    x=29 if i%2==0 else 276;y=228 if i<2 else 538
    S['shop_outfits']['elements'][f'price_{i+1}']=box(x+52,y+223,139,50)
    S['shop_outfits']['elements'][f'bird_{i+1}']=box(x+30,y+25,177,193)

inferred={}
for k in range(1,8):
    pitch=140; house_w=150; house_h=176
    inferred[str(k)]={'evidence':'inferred_from_k7_spacing' if k!=7 else 'observed_k7','source':'art/approved_ui/ui_level_k7.png','houses':[{'center':[540+(i-(k-1)/2)*pitch,384],'size':[house_w,house_h]} for i in range(k)],'beam':{'center':[540,255],'size':[1080,74]}}
D={'schema':'perch-sort-v7-layout-measurements-draft-1','canvas':{'width':1080,'height':1920,'origin':'top_left','unit':'px'},'measurement_method':'Manual visual bounds on 540x960 inspection copies; all values multiplied by 2. Bounds include visible decorative silhouette, excluding broad diffuse glow unless named. Centers and sizes are observed art positions, not UI hit targets. Approximate edge uncertainty +/-8 source px; natural branches and glow +/-16 px.','screens':S,'house_count_adaptation':{'status':'proposed_not_approved_screen_measurements','notes':['K3 approved image shows only a single house. The seven-house beam is observed only on K7.','K1 through K6 use centered subsets of the observed K7 spacing; these are implementation layout proposals and require integration review.','K1 single-house master option is screens.level_k3.elements.house; it is larger than K7 beam houses.'],'by_count':inferred},'world_paths':{'status':'await_generated_backgrounds','coordinate_canvas':[1080,3000],'worlds':{}},'integration_notes':['The concept icon atlas is 1536x1536; it is not a 1080x1920 screen.','Use code text rather than baked screen typography.','No runtime or Unity verification was performed.','These layouts preserve observed concept differences; do not silently normalize them to a new design.']}
(OUT/'layout_draft.json').write_text(json.dumps(D,indent=2)+'\n')
for name,s in S.items():
    im=Image.open(ROOT/Path(s['source']).name).convert('RGB');d=ImageDraw.Draw(im)
    for n,b in s['elements'].items():
        x,y,w,h=b['rect'];d.rectangle((x,y,x+w,y+h),outline='#ff00ff',width=3);d.text((x+3,y+3),n,fill='#ff00ff')
    im.resize((540,960)).save(OUT/(name+'_bounds.jpg'))
print('Wrote',len(S),'screen records and',sum(len(s['elements']) for s in S.values()),'element boxes.')
