"""Assemble measured v7 layout and delivered asset metadata; no runtime code."""
from pathlib import Path
from PIL import Image
import json,hashlib
P=Path(__file__).resolve().parent;ROOT=P.parents[3];V7=ROOT/'art/v7';ART=ROOT/'PerchSort/Assets/Resources/V7/Art';R=V7/'runtime'
read=lambda p:json.loads(p.read_text())
draft=read(P/'layout_draft.json');panels=read(R/'panels/delivery.json');effects=read(R/'effects/delivery.json');hud=read(R/'generated_metrics.json');extracted=read(R/'extracted/provenance.json');maps=read(R/'map/delivery.json');map_sprites=read(R/'map/sprites.json');worlds=read(R/'map/world_paths.json');manifest=read(R/'generation_manifest.json')
assets={}
for f in sorted(ART.glob('*.png')):
 im=Image.open(f);a=im.getchannel('A');assets[f.stem]={'file':str(f.relative_to(ROOT)),'resource':'V7/Art/'+f.stem,'size':list(im.size),'mode':im.mode,'alpha_bounds_xyxy':list(a.getbbox()),'alpha_extrema':list(a.getextrema()),'pivot_normalized_unity':[.5,.5],'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'evidence':'delivered_file_inspected','metadata_sources':[]}
for n,m in hud.items():
 assets[n]['metadata_sources'].append('art/v7/runtime/generated_metrics.json');assets[n]['generated_source_bounds_xyxy']=m['source_bounds']
for m in manifest:
 if m['name'] in assets:
  assets[m['name']]['reference']=m['reference'];assets[m['name']]['generation_record']='art/v7/runtime/generation_manifest.json'
for n,m in extracted.items():
 if n in assets:
  assets[n]['metadata_sources'].append('art/v7/runtime/extracted/provenance.json');assets[n]['reference']=m['source'];assets[n]['extraction']={'source_crop_xyxy':m['source_crop_xyxy'],'source_mask_bounds_xyxy':m['source_mask_bounds_xyxy'],'mask_method':m['mask_method'],'rgb':m['rgb']}
for m in panels['assets']:
 n=Path(m['file']).stem;dest=assets[n];dest['metadata_sources'].append('art/v7/runtime/panels/delivery.json')
 for k in ['nine_slice_lbrt','top_anchor_xy','board_attach_xy','aligned_settings_board_attach_xy','source_crop_xyxy','stretch']:
  if k in m:dest[k]=m[k]
 if 'reference' in m:dest['reference']='art/v7/'+m['reference']
 if 'pivot_normalized' in m:dest['pivot_normalized_unity']=m['pivot_normalized']
for m in effects:
 n=Path(m['file']).stem;dest=assets[n];dest['metadata_sources'].append('art/v7/runtime/effects/delivery.json');dest['reference']=m['reference'];dest['pivot_normalized_unity']=m['pivot_normalized']
 for k in ['light_aperture_anchor_normalized_top_left','integration_note']:
  if k in m:dest[k]=m[k]
for n,m in map_sprites.items():
 assets[n]['metadata_sources'].append('art/v7/runtime/map/sprites.json');assets[n]['reference']='art/v7/ui7_map.png'
for i in range(1,5):
 assets[f'map_bg_w{i}']['metadata_sources'].append('art/v7/runtime/map/delivery.json');assets[f'map_bg_w{i}']['reference']='art/v7/ui7_map.png';assets[f'map_bg_w{i}']['alpha_preparation']='Top rows 0..159 fade from 0 to 255; remaining rows opaque. See map.stacking.'
# These are measured blank interiors on the final stored sprites, NOT the concept numeral bounds.
assets['coin_pill']['text_box']={'rect':[142,32,140,60],'center':[212,62],'size':[140,60],'coordinate_space':'asset_pixels','alignment':'center','vertical_alignment':'middle','evidence':'observed_safe_blank_area_on_delivered_320x128_asset','source':'PerchSort/Assets/Resources/V7/Art/coin_pill.png','note':'Coin occupies the left side; this box excludes coin and rounded edge. Scale the text box with the displayed sprite.'}
assets['btn_green_pill']['nine_slice_lbrt']=[54,42,54,42];assets['btn_green_pill']['nine_slice_evidence']='inferred_border_selection_from_delivered_asset; protects capsule end caps and gold edge; runtime stretch untested'
# Geometry remains exactly the measured source geometry.
screens=draft['screens']
for s in screens.values():
 s['coordinate_space']='screen';s['geometry_kind']='visible_art_bounds_not_hit_targets'
D={
 'schema_version':1,
 'status':'measured_layout_with_house_gate_gift_assemblies_pending_parent',
 'resources':{'filesystem_root':'PerchSort/Assets/Resources/V7/Art','unity_resources_root':'V7/Art','load_key_rule':'assets[id].resource is extensionless Resources.Load key; file is repository-relative PNG path','icon_prefix':'icon_','gift_badge_prefix':'gift_'},
 'coordinate_spaces':{'screen':{'size':[1080,1920],'origin':'top_left','unit':'px','positive_x':'right','positive_y':'down'},'map_world':{'size':[1080,3000],'origin':'top_left','unit':'px','positive_y':'down'},'asset_pixels':{'size_source':'assets[id].size','origin':'top_left','unit':'px'},'pivot_normalized_unity':{'origin':'bottom_left','range':[0,1],'conversion_from_asset_xy':'[x / width, 1 - y / height]'}},
 'property_documentation':{
  'schema_version':'Integer contract version. Version 1 is measured static layout metadata, not gameplay rules.',
  'screens':'Keyed approved-screen layouts; source names the exact image. All center/size/rect values are full 1080x1920 source pixels.',
  'screens.elements.rect':'[left, top, width, height]; bounds include visible decorative art, not broad diffuse glow.',
  'screens.elements.center':'[left + width/2, top + height/2].',
  'screens.elements.size':'[width, height].',
  'screens.branches':'Observed source-side, perch-line endpoints and occupied bird center samples. Not level data or capacity rules.',
  'assets':'Keyed by final PNG stem; actual file dimensions, alpha bounds, resource key, and source metadata.',
  'assets.alpha_bounds_xyxy':'[left, top, right_exclusive, bottom_exclusive] of nonzero alpha in the final file.',
  'assets.nine_slice_lbrt':'Unity Sprite.border order [left,bottom,right,top] in original texture pixels. Keep asset-space borders when changing display dimensions.',
  'assets.text_box':'Safe code-rendered text area in actual sprite-local top-left pixels; scale with that sprite. Not a screen rectangle.',
  'hud':'Resource identities and observed three-/four-button screen element order. Concept positions remain in screens.',
  'house_count_adaptation':'Explicitly inferred centered layouts for unobserved K counts, separate from observed screen art. Do not promote to approved gameplay.',
  'map.worlds.nodes':'Ten measured placements per actual delivered background; index increases bottom to top. These indices do not assign gameplay level content.',
  'map.worlds.nodes.measured_path_edges':'[left,right_exclusive] of the tan path at the node y; center_x is the midpoint.',
  'map.worlds.path_samples':'Observed path samples in local map_world pixels; each has y, left, right and center_x.',
  'map.stacking':'Mandatory 160px baked-alpha overlap, 2840px stride, draw order, and topmost viewport requirement.',
  'assemblies':'Parent-owned composition contracts; pending status means do not infer sprite alignment/hinges from filenames. Assets may already exist while assembly contract is pending.',
  'evidence':'observed_manual_bounds = measured approved screen; delivered_file_inspected = actual PNG; inferred_* = disclosed layout/engineering proposal, not approved observation.',
  'validation':'Static file/geometry checks only. No Unity, input, animation or gameplay acceptance.'},
 'assets':assets,
 'hud':{
  'coin_pill':{'asset_id':'coin_pill','text_box_path':'assets.coin_pill.text_box'},
  'button_bases':{'secondary':'btn_purple','secondary_pressed':'btn_purple_pressed','primary':'btn_green','primary_pressed':'btn_green_pressed','price':'btn_green_pill'},
  'counter':{'asset_id':'badge_red','number_by_code':True},
  'icons':{n.removeprefix('icon_'):n for n in assets if n.startswith('icon_')},
  'gift_badges':{n.removeprefix('gift_'):n for n in assets if n in ['gift_sun_note','gift_wind','gift_grumpy_eyes','gift_watering_can','gift_feather','gift_flame','gift_moon','gift_moon_ray','gift_feather_counter']},
  'bottom_buttons':{'three':{'screen':'level_k3','ordered_element_ids':['undo','hint','branch_plus'],'ordered_icon_ids':['icon_undo','icon_bulb','icon_branch_plus'],'evidence':'observed_manual_bounds'},'four':{'screen':'level_k7','ordered_element_ids':['undo','hint','branch_plus','scarecrow'],'ordered_icon_ids':['icon_undo','icon_bulb','icon_branch_plus','icon_scarecrow'],'evidence':'observed_manual_bounds'}},
  'known_content_corrections':['Minty gift uses gift_watering_can, not the gift box pictured in the approved popup.','Scarecrow uses icon_scarecrow over btn_purple, consistent with the purple-disc family.']},
 'screens':screens,
 'house_count_adaptation':draft['house_count_adaptation'],
 'map':worlds,
 'assemblies':{'houses':{'status':'pending_parent_integration','source_metadata':'art/v7/runtime/houses/delivery.json','contract':None},'gates':{'status':'pending_parent_integration','source_metadata':None,'contract':None},'daily_gift':{'status':'pending_parent_integration','source_metadata':None,'contract':None},'settings_ropes':{'status':'delivered_static_alignment','source_metadata':'art/v7/runtime/panels/delivery.json','left_asset_id':'panel_rope_left','right_asset_id':'panel_rope_right','board_asset_id':'panel_wood','anchor_coordinate_note':'Rope-local anchors are in assets; aligned_settings_board_attach_xy values are screen pixels.'}},
 'validation':{'status':'static_verified','screen_count':len(screens),'measured_element_count':sum(len(s['elements']) for s in screens.values()),'asset_count':len(assets),'map_world_count':4,'measured_map_node_count':40,'checks':['All screen rectangles lie inside 1080x1920.','All asset PNG dimensions and alpha bounds read from files.','All nine-slice borders leave a positive center region.','All node centers lie within measured tan-path interval and 1080x3000 canvas.','All referenced HUD asset IDs exist.'],'runtime_verified':False,'pending':['Parent house/gate/daily-gift assembly integration','Unity import and rendered layout','Topmost map viewport clamp or opaque-source backing','Animation and hit targets']},
 'measurement_notes':{'method':draft['measurement_method'],'source_metadata':['art/v7/runtime/measurements/layout_draft.json','art/v7/runtime/map/world_paths.json','art/v7/runtime/panels/delivery.json','art/v7/runtime/effects/delivery.json','art/v7/runtime/generated_metrics.json','art/v7/runtime/extracted/provenance.json'],'exclusions':['ui7_icons.png is a 1536x1536 atlas, not a portrait screen.','Unapproved K3 alt1/alt2 are not layout authority.','No source screen exists for every K1..K6 multi-house layout.']}
}
# Compact, necessary document contract checks.
for sn,s in screens.items():
 for en,e in s['elements'].items():
  x,y,w,h=e['rect'];assert 0<=x<x+w<=1080 and 0<=y<y+h<=1920,(sn,en);assert e['center']==[x+w/2,y+h/2]
for n,a in assets.items():
 w,h=a['size'];l,t,r,b=a['alpha_bounds_xyxy'];assert 0<=l<r<=w and 0<=t<b<=h,n
 if 'nine_slice_lbrt' in a:
  l,b,r,t=a['nine_slice_lbrt'];assert l+r<w and t+b<h,n
for world in worlds['worlds'].values():
 assert len(world['nodes'])==10
 for n in world['nodes']:
  x,y=n['center'];l,r=n['measured_path_edges'];assert 0<=l<=x<=r<=1080 and 0<=y<3000
for layout in D['hud']['bottom_buttons'].values():
 assert all(n in assets for n in layout['ordered_icon_ids'])
(V7/'layout_v7.json').write_text(json.dumps(D,indent=2)+'\n')
print('Wrote layout_v7.json:',len(assets),'assets;',len(screens),'screens;',D['validation']['measured_element_count'],'element boxes; 40 nodes. House/gate/gift assembly contracts pending parent.')
