"""Assemble measured approved layout and delivered asset metadata; no runtime code."""
from pathlib import Path
from PIL import Image
import json,hashlib
P=Path(__file__).resolve().parent;ROOT=P.parents[3];APPROVED=ROOT/'art/approved_ui';ART=ROOT/'PerchSort/Assets/Resources/Art';R=APPROVED/'runtime'
read=lambda p:json.loads(p.read_text())
draft=read(P/'layout_draft.json');panels=read(R/'panels/delivery.json');effects=read(R/'effects/delivery.json');hud=read(R/'generated_metrics.json');extracted=read(R/'extracted/provenance.json');maps=read(R/'map/delivery.json');map_sprites=read(R/'map/sprites.json');worlds=read(R/'map/world_paths.json');manifest=read(R/'generation_manifest.json')
assets={}
for f in sorted(ART.glob('*.png')):
 im=Image.open(f);a=im.getchannel('A');assets[f.stem]={'file':str(f.relative_to(ROOT)),'resource':'Art/'+f.stem,'size':list(im.size),'mode':im.mode,'alpha_bounds_xyxy':list(a.getbbox()),'alpha_extrema':list(a.getextrema()),'pivot_normalized_unity':[.5,.5],'sha256':hashlib.sha256(f.read_bytes()).hexdigest(),'evidence':'delivered_file_inspected','metadata_sources':[]}
for n,m in hud.items():
 assets[n]['metadata_sources'].append('art/approved_ui/runtime/generated_metrics.json');assets[n]['generated_source_bounds_xyxy']=m['source_bounds']
for m in manifest:
 if m['name'] in assets:
  assets[m['name']]['reference']=m['reference'];assets[m['name']]['generation_record']='art/approved_ui/runtime/generation_manifest.json'
for n,m in extracted.items():
 if n in assets:
  assets[n]['metadata_sources'].append('art/approved_ui/runtime/extracted/provenance.json');assets[n]['reference']=m['source'];assets[n]['extraction']={'source_crop_xyxy':m['source_crop_xyxy'],'source_mask_bounds_xyxy':m['source_mask_bounds_xyxy'],'mask_method':m['mask_method'],'rgb':m['rgb']}
for m in panels['assets']:
 n=Path(m['file']).stem;dest=assets[n];dest['metadata_sources'].append('art/approved_ui/runtime/panels/delivery.json')
 for k in ['nine_slice_lbrt','top_anchor_xy','board_attach_xy','aligned_settings_board_attach_xy','source_crop_xyxy','stretch']:
  if k in m:dest[k]=m[k]
 if 'reference' in m:dest['reference']='art/approved_ui/'+m['reference']
 if 'pivot_normalized' in m:dest['pivot_normalized_unity']=m['pivot_normalized']
for m in effects:
 n=Path(m['file']).stem;dest=assets[n];dest['metadata_sources'].append('art/approved_ui/runtime/effects/delivery.json');dest['reference']=m['reference'];dest['pivot_normalized_unity']=m['pivot_normalized']
 for k in ['light_aperture_anchor_normalized_top_left','integration_note']:
  if k in m:dest[k]=m[k]
for n,m in map_sprites.items():
 assets[n]['metadata_sources'].append('art/approved_ui/runtime/map/sprites.json');assets[n]['reference']='art/approved_ui/ui_map.png'
for i in range(1,5):
 assets[f'map_bg_w{i}']['metadata_sources'].append('art/approved_ui/runtime/map/delivery.json');assets[f'map_bg_w{i}']['reference']='art/approved_ui/ui_map.png';assets[f'map_bg_w{i}']['alpha_preparation']='Top rows 0..159 fade from 0 to 255; remaining rows opaque. See map.stacking.'
houses=read(R/'houses/delivery.json')
for m in houses['assets']:
 n=Path(m['file']).stem;dest=assets[n];assert dest['size']==m['canvas'],n
 dest['metadata_sources'].append('art/approved_ui/runtime/houses/delivery.json')
 dest['source_crop_xyxy']=m['source_crop_xyxy']
 dest['part_placement_rect_xywh']=m['visible_rect_xywh']
 dest['preserve_canvas_on_import']=True
 if n=='house_door_leaf':dest['pivot_normalized_unity']=houses['door_leaf_pivot_unity_normalized']
house_contract={k:v for k,v in houses.items() if k!='assets'}
house_contract['asset_ids']={'body':'house_body','roof_by_color':{c:'house_roof_'+c for c in ['teal','red','yellow','purple','green','blue','orange','pink']},'door_leaf':'house_door_leaf','open_fallback':'house_door_open','rope':'house_rope','beam_by_world':{f'w{i}':f'house_beam_w{i}' for i in range(1,5)}}
house_contract['composition']={'closed':'Place body, selected roof, then door leaf at zero offset on shared 512x512 canvas. Preserve transparent canvas at import.','open_fallback':'Replace body+door-leaf with house_door_open, then add selected roof at zero offset. Fallback already contains body and excludes roof.','bird_face':'Center bird face on door_center_xy and clip to the circular dark doorway; bird sprite scale is not defined by this asset contract.','rope':'Connect rope.bottom_anchor_xy to roof_suspension_anchor_xy after applying each part display scale.','beam':'Beam anchors are beam-local asset pixels, not approved-screen house positions; scale beam-local anchors with the beam.'}
house_contract['evidence']='delivered_aligned_sprite_contract_static_QA_reviewed; door angle is a supplied engineering animation target, not runtime-verified motion'
house_contract['qa_reviewed']=['art/approved_ui/runtime/houses/qa/assembled_sheet.jpg','art/approved_ui/runtime/houses/qa/beams_sheet.jpg']
gift=read(R/'gift/delivery.json')
for m in gift['assets']:
 n=Path(m['file']).stem;dest=assets[n];assert dest['size']==m['canvas'],n
 dest['metadata_sources'].append('art/approved_ui/runtime/gift/delivery.json');dest['preserve_canvas_on_import']=True
 for key in ['source_crop_xyxy','placement_xywh','derived_from','derivation','reference']:
  if key in m:dest[key]=m[key]
assets['gift_box_lid']['pivot_normalized_unity']=gift['lid_pivot_unity_normalized']
gift_contract={k:v for k,v in gift.items() if k!='assets'}
gift_contract['draw_order_asset_ids']=['gift_nest','gift_box_base','gift_box_lid','gift_nest_front']
gift_contract['evidence']='delivered_aligned_sprite_contract_static_QA_reviewed; lid offset is an engineering animation target, not runtime-verified motion'
gift_contract['qa_reviewed']=['art/approved_ui/runtime/gift/qa/closed_dark.jpg','art/approved_ui/runtime/gift/qa/open_dark.jpg']
gates=read(R/'gates/delivery.json')
for m in gates['files']:
 n=Path(m['file']).stem;dest=assets[n];assert dest['size']==m['size'],n;assert dest['sha256']==m['sha256'],n
 dest['metadata_sources'].append('art/approved_ui/runtime/gates/delivery.json');dest['preserve_canvas_on_import']=True
 if 'pivot_normalized_unity_bottom_left' in m:dest['pivot_normalized_unity']=m['pivot_normalized_unity_bottom_left']
 for key in ['hinge_pixels_top_left','pivot_normalized_top_left']:
  if key in m:dest[key]=m[key]
gate_contract={k:v for k,v in gates.items() if k!='files'}
gate_contract['asset_ids']={'arch':'gate_arch','left_door':'gate_door_left','right_door':'gate_door_right','padlock':'gate_padlock','light':'gate_light','backdrop_by_world':{f'w{i}':f'gate_backdrop_w{i}' for i in range(2,5)},'locked_fallback':'gate_locked','open_fallback':'gate_open'}
gate_contract['evidence']='delivered_aligned_sprite_contract; static projected open fallback, not proof of animated rotation'
gate_contract['preserve_canvas_on_import']=True
gate_contract['all_parts_offset_xy']=[0,0]
assets['gate_light']['metadata_sources'].append('art/approved_ui/runtime/gates/delivery.json')
assets['gate_light']['integration_note']='Align source aperture (0.5,0.25) to gate-canvas (512,375) using draw rect [192,215,640,640]. See assemblies.gates.contract.gate_light.'
assert assets['gate_light']['sha256']==gates['gate_light']['sha256']
# These are measured blank interiors on the final stored sprites, NOT the concept numeral bounds.
assets['coin_pill']['text_box']={'rect':[142,32,140,60],'center':[212,62],'size':[140,60],'coordinate_space':'asset_pixels','alignment':'center','vertical_alignment':'middle','evidence':'observed_safe_blank_area_on_delivered_320x128_asset','source':'PerchSort/Assets/Resources/Art/coin_pill.png','note':'Coin occupies the left side; this box excludes coin and rounded edge. Scale the text box with the displayed sprite.'}
assets['btn_green_pill']['nine_slice_lbrt']=[54,42,54,42];assets['btn_green_pill']['nine_slice_evidence']='inferred_border_selection_from_delivered_asset; protects capsule end caps and gold edge; runtime stretch untested'
# Geometry remains exactly the measured source geometry.
screens=draft['screens']
for s in screens.values():
 s['coordinate_space']='screen';s['geometry_kind']='visible_art_bounds_not_hit_targets'
D={
 'schema_version':1,
 'status':'delivered_static_layout',
 'resources':{'filesystem_root':'PerchSort/Assets/Resources/Art','unity_resources_root':'Art','load_key_rule':'assets[id].resource is extensionless Resources.Load key; file is repository-relative PNG path','icon_prefix':'icon_','gift_badge_prefix':'gift_'},
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
 'assemblies':{'houses':{'status':'delivered_static_alignment','source_metadata':'art/approved_ui/runtime/houses/delivery.json','contract':house_contract},'gates':{'status':'delivered_static_alignment','source_metadata':'art/approved_ui/runtime/gates/delivery.json','contract':gate_contract},'daily_gift':{'status':'delivered_static_alignment','source_metadata':'art/approved_ui/runtime/gift/delivery.json','contract':gift_contract},'settings_ropes':{'status':'delivered_static_alignment','source_metadata':'art/approved_ui/runtime/panels/delivery.json','left_asset_id':'panel_rope_left','right_asset_id':'panel_rope_right','board_asset_id':'panel_wood','anchor_coordinate_note':'Rope-local anchors are in assets; aligned_settings_board_attach_xy values are screen pixels.'}},
 'validation':{'status':'static_verified','screen_count':len(screens),'measured_element_count':sum(len(s['elements']) for s in screens.values()),'asset_count':len(assets),'map_world_count':4,'measured_map_node_count':40,'checks':['All screen rectangles lie inside 1080x1920.','All asset PNG dimensions and alpha bounds read from files.','All nine-slice borders leave a positive center region.','All node centers lie within measured tan-path interval and 1080x3000 canvas.','All referenced HUD asset IDs exist.'],'runtime_verified':False,'pending':['Unity import and rendered layout','Topmost map viewport clamp or opaque-source backing','Animation and hit targets']},
 'measurement_notes':{'method':draft['measurement_method'],'source_metadata':['art/approved_ui/runtime/measurements/layout_draft.json','art/approved_ui/runtime/map/world_paths.json','art/approved_ui/runtime/panels/delivery.json','art/approved_ui/runtime/effects/delivery.json','art/approved_ui/runtime/generated_metrics.json','art/approved_ui/runtime/extracted/provenance.json','art/approved_ui/runtime/houses/delivery.json','art/approved_ui/runtime/gift/delivery.json','art/approved_ui/runtime/gates/delivery.json'],'exclusions':['ui_icons.png is a 1536x1536 atlas, not a portrait screen.','Unapproved K3 alt1/alt2 are not layout authority.','No source screen exists for every K1..K6 multi-house layout.']}
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
(APPROVED/'layout.json').write_text(json.dumps(D,indent=2)+'\n')
print('Wrote layout.json:',len(assets),'assets;',len(screens),'screens;',D['validation']['measured_element_count'],'element boxes; 40 nodes. All house, gate and gift assemblies integrated; runtime remains unverified.')
