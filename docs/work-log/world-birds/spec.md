# World birds: roster and remaining work

## Where the game stands (2026-11-18)
- Every one of the ten worlds plays with the same ten painted birds: nine collectible species (sunny, sky, berry, mint, cocoa, snow,
  ember, ink, hoot) and the crow ("Rook"), which is never collected and is not in the shop.
- Each species has 71 drawn animation frames. Skins: 9 species x 5 outfit slots. Only `cocoa_outfit_4` (propeller beanie) is a whole-bird skin;
  `ember_outfit_5` is a whole-bird trial; the other 43 skins are small overlays that the owner finds wrong, alike and discontinuous.
  The crow has no skins at all.
- 81 world-bird designs (reference pictures, 1024 px) exist in `tools/birds/world_designs/birds/` (`w<world>_<id>.png`, list in
  `birds.tsv`). No animation frames of any world bird exist in the game, and the game has no per-world bird mapping yet.

## Plan carried over (bird-species decisions, 2026-11-10)
Sort by species (skins are cosmetic only); the slot-to-species map of a world keeps every level and stored solution valid; a world's bird in a slot
replaces the world-1 bird of that slot; species are unlocked by play only. The crow stays the same bird in every world.

## Roster
Slot order is the order of the designs in `birds.tsv` (colour and role match the world-1 bird of the slot). "Design" = reference picture exists.
Status of every world bird: design done, 0 of 71 frames, 0 of 5 skins.

### World 1 - Green (the birds in the game today)

| Slot | Bird | Frames | Skins |
|---|---|---|---|
| sunny | Sunny | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| sky | Skye | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| berry | Berry | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| mint | Minty | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| cocoa | Cocoa | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| snow | Flurry | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| ember | Ember | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| ink | Inky | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| hoot | Hoot | 71 | 5 (small overlays, 1 whole-bird for cocoa 4 and ember 5) |
| crow | Rook | 71 | 0 of 5 |

### World 2 - Bamboo

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | oriole | black-naped oriole: golden yellow with a black eye stripe and a coral beak | yes | 0 of 71 | 0 of 5 |
| sky | magpie | red-billed blue magpie: blue with a black head, red beak, very long tail hanging down | yes | 0 of 71 | 0 of 5 |
| berry | hoopoe | hoopoe: pinkish buff with black-and-white striped wings and a fan crest | yes | 0 of 71 | 0 of 5 |
| mint | whiteeye | Japanese white-eye: olive green with a white eye ring | yes | 0 of 71 | 0 of 5 |
| cocoa | laughingthrush | hwamei laughingthrush: warm brown with a white eyebrow line | yes | 0 of 71 | 0 of 5 |
| snow | longtailedtit | silver-throated long-tailed tit: fluffy white and pink with a long thin tail hanging down | yes | 0 of 71 | 0 of 5 |
| ember | pheasant | golden pheasant: red and gold with a golden crest, long barred tail hanging down | yes | 0 of 71 | 0 of 5 |
| ink | scopsowl | collared scops owl: grey-brown with ear tufts, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |
| hoot | woodowl | brown wood owl: dark brown with a round face and big dark eyes | yes | 0 of 71 | 0 of 5 |

### World 3 - Autumn

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | greattit | great tit: yellow belly, black cap and white cheeks | yes | 0 of 71 | 0 of 5 |
| sky | bluetit | blue tit: blue cap and wings, yellow belly | yes | 0 of 71 | 0 of 5 |
| berry | bullfinch | Eurasian bullfinch: rosy pink breast, black cap | yes | 0 of 71 | 0 of 5 |
| mint | greenwoodpecker | European green woodpecker: green with a red cap and a pointed beak | yes | 0 of 71 | 0 of 5 |
| cocoa | wren | Eurasian wren: tiny brown with a cocked tail | yes | 0 of 71 | 0 of 5 |
| snow | wagtail | white wagtail: white and black with a long tail | yes | 0 of 71 | 0 of 5 |
| ember | robin | European robin: orange breast and face, brown back | yes | 0 of 71 | 0 of 5 |
| ink | nightjar | European nightjar: mottled grey-brown, wide mouth, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |
| hoot | barnowl | barn owl: sandy gold with a white heart-shaped face | yes | 0 of 71 | 0 of 5 |

### World 4 - Snow

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | emperorpenguin | emperor penguin: black, white, with yellow ear patches | yes | 0 of 71 | 0 of 5 |
| sky | bluepenguin | little blue penguin: slate blue back and white front | yes | 0 of 71 | 0 of 5 |
| berry | gull | Ross's gull: soft grey with a pink breast and a small black beak | yes | 0 of 71 | 0 of 5 |
| mint | puffin | Atlantic puffin: black and white with a big orange-red beak | yes | 0 of 71 | 0 of 5 |
| cocoa | waxwing | Bohemian waxwing: brown-grey with a crest and a yellow tail tip | yes | 0 of 71 | 0 of 5 |
| snow | snowbunting | snow bunting: white with brown wing patches | yes | 0 of 71 | 0 of 5 |
| ember | ptarmigan | rock ptarmigan: white with feathered feet and a red eyebrow | yes | 0 of 71 | 0 of 5 |
| ink | snowyowl | snowy owl: white with dark specks, yellow eyes, sleepy half-closed | yes | 0 of 71 | 0 of 5 |
| hoot | shortearedowl | short-eared owl: sandy brown, yellow eyes, round face | yes | 0 of 71 | 0 of 5 |

### World 5 - Blossom

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | canary | yellow canary: bright yellow | yes | 0 of 71 | 0 of 5 |
| sky | bluebird | eastern bluebird: bright blue back with an orange breast | yes | 0 of 71 | 0 of 5 |
| berry | galah | galah cockatoo: rose pink and grey with a soft crest | yes | 0 of 71 | 0 of 5 |
| mint | hummingbird | ruby-throated hummingbird: green with a ruby throat, long thin beak | yes | 0 of 71 | 0 of 5 |
| cocoa | sparrow | house sparrow: brown streaked with a grey cap | yes | 0 of 71 | 0 of 5 |
| snow | javasparrow | white Java sparrow: white with a red beak and a grey head | yes | 0 of 71 | 0 of 5 |
| ember | cardinal | northern cardinal: red with a crest and a black mask | yes | 0 of 71 | 0 of 5 |
| ink | nightingale | common nightingale: plain warm brown, singing, sleepy | yes | 0 of 71 | 0 of 5 |
| hoot | screechowl | eastern screech owl: grey with ear tufts, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |

### World 6 - Mushroom

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | goldcrest | goldcrest: tiny olive with a yellow crown stripe | yes | 0 of 71 | 0 of 5 |
| sky | flycatcher | verditer flycatcher: turquoise blue | yes | 0 of 71 | 0 of 5 |
| berry | jay | Eurasian jay: pinkish brown with a blue wing patch | yes | 0 of 71 | 0 of 5 |
| mint | greenmagpie | common green magpie: bright green with a red beak and a long tail hanging down | yes | 0 of 71 | 0 of 5 |
| cocoa | treecreeper | Eurasian treecreeper: brown with a curved beak | yes | 0 of 71 | 0 of 5 |
| snow | whitecrested | white-crested laughingthrush: white head and crest, chestnut body | yes | 0 of 71 | 0 of 5 |
| ember | scarletfinch | scarlet finch: bright red-orange | yes | 0 of 71 | 0 of 5 |
| ink | frogmouth | Sri Lanka frogmouth: sandy brown with a wide beak, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |
| hoot | orientalscops | Oriental scops owl: grey with ear tufts, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |

### World 7 - Lakeside

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | wagtailyellow | yellow wagtail: yellow with a long tail | yes | 0 of 71 | 0 of 5 |
| sky | kingfisher | common kingfisher: blue and orange with a long dagger beak | yes | 0 of 71 | 0 of 5 |
| berry | flamingo | baby flamingo: pink with long legs and a bent beak | yes | 0 of 71 | 0 of 5 |
| mint | mallard | mallard drake: green head, white collar, orange feet | yes | 0 of 71 | 0 of 5 |
| cocoa | grebe | little grebe: chestnut and brown, round | yes | 0 of 71 | 0 of 5 |
| snow | egret | little egret: white with a long neck and a black beak | yes | 0 of 71 | 0 of 5 |
| ember | mandarin | mandarin duck: orange sail feathers, colourful | yes | 0 of 71 | 0 of 5 |
| ink | nightheron | black-crowned night heron: navy-grey with red eyes, hunched, sleepy | yes | 0 of 71 | 0 of 5 |
| hoot | fishowl | Blakiston's fish owl: brown with big ear tufts, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |

### World 8 - Misty Pine

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | siskin | Eurasian siskin: yellow-green with streaks | yes | 0 of 71 | 0 of 5 |
| sky | bluejay | blue jay: blue with a crest and black collar | yes | 0 of 71 | 0 of 5 |
| berry | grosbeak | pine grosbeak: rosy pink with a thick beak | yes | 0 of 71 | 0 of 5 |
| mint | greentit | green-backed tit: green and yellow | yes | 0 of 71 | 0 of 5 |
| cocoa | nutcracker | spotted nutcracker: brown with white spots | yes | 0 of 71 | 0 of 5 |
| snow | spottedwoodpecker | great spotted woodpecker: black and white with a red patch | yes | 0 of 71 | 0 of 5 |
| ember | crossbill | red crossbill: red with a crossed beak | yes | 0 of 71 | 0 of 5 |
| ink | longearedowl | long-eared owl: orange face with long ear tufts, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |
| hoot | borealowl | boreal owl: brown with a white face, sleepy half-closed eyes | yes | 0 of 71 | 0 of 5 |

### World 9 - Rainforest

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | conure | sun conure: yellow-orange parrot with green wing tips | yes | 0 of 71 | 0 of 5 |
| sky | bluegoldmacaw | blue-and-gold macaw: blue back, yellow front, long tail hanging down | yes | 0 of 71 | 0 of 5 |
| berry | spoonbill | roseate spoonbill: pink with a spoon-shaped beak | yes | 0 of 71 | 0 of 5 |
| mint | lovebird | peach-faced lovebird: green with a peach face | yes | 0 of 71 | 0 of 5 |
| cocoa | toucan | toco toucan: black with a big orange beak | yes | 0 of 71 | 0 of 5 |
| snow | cockatoo | sulphur-crested cockatoo: white with a yellow crest | yes | 0 of 71 | 0 of 5 |
| ember | scarletmacaw | scarlet macaw: red with blue and yellow wings, long tail hanging down | yes | 0 of 71 | 0 of 5 |
| ink | potoo | common potoo: grey-brown bark-like, big eyes half closed, sleepy | yes | 0 of 71 | 0 of 5 |
| hoot | spectacledowl | spectacled owl: dark brown with white spectacles and a cream belly, sleepy | yes | 0 of 71 | 0 of 5 |

### World 10 - Wisteria

| Slot | World bird | Description (design brief) | Design | Frames | Skins |
|---|---|---|---|---|---|
| sunny | narcissus | narcissus flycatcher: yellow and black | yes | 0 of 71 | 0 of 5 |
| sky | blueflycatcher | blue-and-white flycatcher: blue and white | yes | 0 of 71 | 0 of 5 |
| berry | ibis | crested ibis: soft pink and white with a long curved beak | yes | 0 of 71 | 0 of 5 |
| mint | greenpheasant | green pheasant: green with a long tail hanging down | yes | 0 of 71 | 0 of 5 |
| cocoa | warbler | Japanese bush warbler: olive brown | yes | 0 of 71 | 0 of 5 |
| snow | craneChick | red-crowned crane chick: white with a red cap and long legs | yes | 0 of 71 | 0 of 5 |
| ember | redstart | Daurian redstart: orange and grey | yes | 0 of 71 | 0 of 5 |
| ink | hawkowl | brown hawk-owl: dark brown with yellow eyes, sleepy half-closed | yes | 0 of 71 | 0 of 5 |
| hoot | eagleowl | Eurasian eagle-owl: tawny with big ear tufts, sleepy half-closed | yes | 0 of 71 | 0 of 5 |

## Work remaining, in numbers
| Item | Birds | Frames each | Frame paintings |
|---|---|---|---|
| Animation frames of the 81 world birds | 81 | 71 | 5,751 |
| Skins of the nine world-1 birds still to do (45 slots, 1 done) | 44 | 71 | 3,124 |
| Skins of the crow | 5 | 71 | 355 |
| Skins of the 81 world birds (5 each) | 405 | 71 | 28,755 |
| Total | | | 37,985 |

Measured on the owl pilot: one frame painting by local Codex takes about 1.5 minutes and 12 run in parallel, so about 8 frames a minute.
That is about 79 hours for everything (12 h birds, 7 h world-1 skins and crow, 60 h world-bird skins), before retries. Three skins per world bird instead of
five would cut the last item to 17,253 frames (about 36 h).

## Open decisions (owner)
1. Method: keep painting every frame with Codex, or build the birds as 3D models and render the frames (see report.md).
2. Skins per world bird: 5 (like today) or fewer.
3. Game logic for world birds: one collection per world or one per slot, how the aviary and shop list 90 birds, how the profile stores them.


## Revised production plan (owner, 2026-11-19)
The painted frame count is cut to what the match game uses (see `skin-redesign`, "Frame set of a skin"):
- A world bird: 25 painted frames (standing, blinking x2, hop x6, cheer x6, flight flap x6, take-off x3, landing x3) plus the left-facing flight
  frames (12) mirrored; the decorative idle actions (turns, preening, nine small poses) and the unused `fly_up`/`fly_down` are not painted.
- A skin: 9 painted frames plus 6 mirrored.
- Totals: 81 birds x 25 = 2,025, 405 skins x 9 = 3,645, about 5,700 paintings instead of about 34,500 (one Codex usage window is about 2,000 paintings).
- Still open: skins per world bird (1, 3 or 5) and the game logic for 90 birds.
