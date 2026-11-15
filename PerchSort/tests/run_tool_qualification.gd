extends SceneTree
var checks := 0
var failures := 0
func check(ok: bool, label: String):
 checks += 1
 if not ok:
  failures += 1
  if failures < 8: print("FAIL ",label)
func _init():
 Progress._booster_levels=PackedInt32Array([2,4,22,5,106,64])
 for schema in range(1,6):
  for frontier in range(1,211):
   var p := Profile.from_dict({"schemaVersion":schema,"highestUnlockedLevel":frontier})
   check(p.inventory==PackedInt32Array([0,0,0,0,0,0]) and p.starters.is_empty(),"migration grants nothing")
   for kind in 6:
    check(Progress.is_booster_unlocked(p,kind)==(p.highest_unlocked>=Profile.LEGACY_BOOSTER_GATES[kind]),"old entitlement")
   var restored := Profile.from_dict(JSON.parse_string(JSON.stringify(p.to_dict())))
   check(restored.to_dict()==p.to_dict(),"roundtrip idempotence")
   var grants := Progress.grant_starter_gifts(restored)
   check(grants.size()==p.legacy_booster_qualifications.size(),"pending grants preserved")
   check(Progress.grant_starter_gifts(restored).is_empty(),"grants once")
 for frontier in [11,15,17,22,64,106]:
  var fresh := Profile.new()
  fresh.highest_unlocked=frontier
  fresh=Profile.from_dict(JSON.parse_string(JSON.stringify(fresh.to_dict())))
  for kind in 6: check(Progress.is_booster_unlocked(fresh,kind)==(frontier>=Progress.booster_unlock_level(kind)),"fresh new gates only")
 for invalid in [null,{},[true],[1,1],[6],[-1],[1.5],["1"]]:
  var data := Profile.new().to_dict()
  data["legacyBoosterQualifications"]=invalid
  check(Profile.from_dict(data).needs_recovery(),"malformed state blocks")
 var stock := Profile.new().to_dict()
 stock["schemaVersion"]=5
 stock["boosters"]={"owlEye":1}
 var p := Profile.from_dict(stock)
 p.inventory[2]=0
 check(Progress.is_booster_unlocked(p,2),"stock qualification survives spending")
 # Compatibility-first release: persist eligibility earned before catalog replacement.
 Progress._booster_levels=PackedInt32Array([2,4,11,5,15,17])
 var interim:=Profile.new()
 interim.highest_unlocked=17
 var saved:=interim.to_dict()
 check(interim.inventory==PackedInt32Array([0,0,0,0,0,0]) and interim.starters.is_empty(),"serialization gives no reward")
 Progress._booster_levels=PackedInt32Array([2,4,22,5,106,64])
 var later:=Profile.from_dict(JSON.parse_string(JSON.stringify(saved)))
 check(Progress.is_booster_unlocked(later,2),"qualification survives staged release")
 check(later.inventory==interim.inventory and later.starters==interim.starters,"staged release no reward")
 Progress._booster_levels=PackedInt32Array()
 print("QUALIFICATION checks=%d failures=%d"%[checks,failures])
 quit(1 if failures else 0)
