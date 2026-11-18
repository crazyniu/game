# Skin redesign

## Direction (owner, 2026-11-18)
- A skin is worn on the bird's body. Nothing floats around it: no leaves, petals, sparkles, fireflies, glows or auras.
  The skin particles and the aura of `BirdView` are removed.
- The five skins of one species must differ strongly from each other and from the other species' skins: a different
  garment class, a different colour family, a different silhouette. No set of five small accessories.
- Every skin is a whole-bird painting per frame (see `whole-bird-skins`); frames of one animation must be continuous
  (same size, position, face, eyes, costume shape and colour in neighbouring frames).
- Existing skin art for slots 1-3 (small overlays) and the unapproved trials are replaced. `cocoa_outfit_4` (propeller beanie,
  with its spinning propeller on the cap button) is kept.

## Rules for every costume
1. Worn only: hat, hood, helmet, glasses, scarf, vest, jacket, apron, armour, tail-less suit. No held props, no objects beside the bird.
2. Costume stays inside the bird's own outline except the part that belongs to the head (hat top, hood ears, plume) and may rise at most
   one head-height above the head. No cape or wing-like part that extends beyond the wings.
3. Motion belongs to the costume (a propeller turning, a pompom swinging with the head), never to separate objects.
4. Colour family differs from the plain bird and from the other four skins of the species.
5. Exactly one bird, face, eyes, eyelids, beak, feet and wings stay the plain bird's.

## Classes
`H` head-and-shoulders outfit (hat plus top), `S` full hooded suit, `B` body garment with head accessory, `F` full character costume.
Each species has one of each of H, B, S, F plus one free class so no two skins share a silhouette.

## The 45 skins
Slot 1-3 are the cheaper tiers, 4 and 5 the premium ones (prices unchanged).

| Species | Slot | Name | Class | Costume | Palette |
|---|---|---|---|---|---|
| cocoa | 1 | Scholar | B | round gold glasses, green knitted waistcoat over the chest, small brown bow tie | forest green, gold |
| cocoa | 2 | Baker | H | tall white chef hat, white double-breasted jacket with red buttons | white, red |
| cocoa | 3 | Pyjama | B | blue striped nightcap with pompom, blue striped pyjama top | navy, white |
| cocoa | 4 | Propeller beanie | H | blue-and-white cap with a spinning propeller (existing) | blue, white |
| cocoa | 5 | Gingerbread | F | gingerbread-biscuit hood and body with white icing and candy buttons | biscuit brown, white, red |
| berry | 1 | Cupcake | H | swirl of cream frosting with a cherry on top as a hat, brown paper cupcake-liner collar worn round the belly | cream, chocolate brown, cherry red |
| berry | 2 | Painter | H | tilted navy beret, cream smock with paint smears | navy, cream, paint colours |
| berry | 3 | Ballerina | B | silver tiara, pale pink tutu worn tight round the belly | pale pink, silver |
| berry | 4 | Strawberry | S | red strawberry hood with green calyx, seeds on the face frame | red, green |
| berry | 5 | Sakura | F | cream kimono top with a wide pink obi, pink blossom hairpins | cream, pink, gold |
| sunny | 1 | Farmer | H | straw hat with a red band, blue denim dungarees | straw, denim blue |
| sunny | 2 | Pilot | B | brown leather flying cap with goggles pushed up, white scarf | leather brown, white |
| sunny | 3 | Chick in an eggshell | S | cracked eggshell hat and a white shell half worn like a skirt | eggshell white, orange |
| sunny | 4 | Bee | S | yellow-and-black striped suit, two small antennae on the hood | black, honey yellow |
| sunny | 5 | Sunflower | F | sunflower-petal hood framing the face, green leaf collar worn tight | gold, brown, green |
| snow | 1 | Skier | B | red bobble hat, ski goggles on the forehead, puffy teal vest | red, teal |
| snow | 2 | Snowman | H | black top hat, red-and-white scarf, two coal buttons on the chest | black, red |
| snow | 3 | Penguin | S | black-and-white penguin hood with orange beak trim | black, white |
| snow | 4 | Bunny | S | pale pink bunny-ear hood with fluffy cheeks (existing concept) | pale pink, white |
| snow | 5 | Ice queen | F | tall crystal crown, white fur collar with frost-blue cloak worn on the shoulders | ice blue, white |
| sky | 1 | Sailor | B | navy sailor collar, white sailor cap with anchor | navy, white |
| sky | 2 | Astronaut | H | clear bubble helmet with a white collar ring, orange suit top | white, orange |
| sky | 3 | Raincoat | B | yellow rain hat, yellow raincoat with buttons | yellow |
| sky | 4 | Whale | S | blue whale hood with a spout on top and a white belly front | blue, white |
| sky | 5 | Cloud fairy | F | fluffy white cloud hat, pastel rainbow scarf, small star pin | white, pastel |
| ember | 1 | Firefighter | H | red helmet with a gold badge, yellow reflective jacket | red, yellow |
| ember | 2 | Samurai | B | black kabuto-style helmet, red lacquered chest armour | black, red |
| ember | 3 | Chef of fire | B | high-collar black coat with orange trim, black bandana | black, orange |
| ember | 4 | Fox | S | orange fox hood with white cheeks and pointed ears | orange, white |
| ember | 5 | Dragon | F | red dragon hood with small horns, scaled chest plate | crimson, gold |
| hoot | 1 | Wizard | H | tall purple wizard hat with stars, indigo robe with silver trim | purple, indigo |
| hoot | 2 | Professor | B | black graduation cap with a tassel, wire glasses, tweed jacket | black, tweed grey |
| hoot | 3 | Knight | H | silver helmet with a plume, round shield-shaped chest plate | silver, blue |
| hoot | 4 | Panda | S | black-and-white panda hood with round ears | black, white |
| hoot | 5 | Ghost | F | white sheet-ghost hood with cut-out eye holes and a tiny scarf | white, pale blue |
| ink | 1 | Detective | H | tweed deerstalker cap, brown checked cloak worn on the shoulders | tweed, brown |
| ink | 2 | Cowboy | H | tan cowboy hat, brown leather vest, red bandana | tan, brown, red |
| ink | 3 | Rock star | B | purple mohawk-wig band, black leather jacket with silver studs | black, purple |
| ink | 4 | Pirate | H | black tricorn hat with skull, red coat with gold buttons | black, red, gold |
| ink | 5 | Ninja | F | indigo ninja hood and mask leaving only the eyes, grey chest wrap | indigo, grey |
| mint | 1 | Mushroom | H | red mushroom cap with white spots as a hat, cream collar | red, white |
| mint | 2 | Scout | B | olive scout hat, yellow neckerchief, green sash | olive, yellow |
| mint | 3 | Pumpkin | S | orange pumpkin hood with a green stem and curly tendril, carved smile stitched on the chest | orange, green stem |
| mint | 4 | Frog | S | bright green frog hood with big eye bumps (existing concept) | bright green |
| mint | 5 | Pharaoh | F | gold-and-blue striped pharaoh headdress framing the face, broad gold collar with blue beads | gold, lapis blue |

Cocoa slot 4 and its propeller are already built; every other row is new art, including the rows that reuse an earlier concept.

## Production
1. One reference painting per skin (the costumed bird from the front), approved before any frames are painted.
2. 70 frames per skin painted by local Codex from the plain frames (the pipeline in `tools/birds/costume_parts`), 12 in parallel on each
   of two machines.
3. Per-frame alignment and continuity checks (silhouette, feet, eyes, colour); a frame that fails is repainted.
4. Published into `PerchSort/assets/resources/birds/` as `<species>_outfit_<slot>_<frame>.png`; old overlay art and anchors removed.
5. Pilot first: one skin (`sunny_outfit_1`) is painted fully and reviewed by the owner before the rest.
