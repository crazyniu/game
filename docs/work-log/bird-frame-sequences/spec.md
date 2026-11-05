# T54G Bird animation frame sequences (art), pilot species sunny, sky, berry
Read docs/work-log/integration-contract/spec.md and docs/work-log/integration-contract/spec-animation.md. This is an ART task; the contract's no-artwork rule does not apply to you, but the rules of the project do:
the existing approved art stays untouched; you only ADD derived files. Owned: new files `PerchSort/Assets/Resources/Birds/{sunny,sky,berry}_{flap,hop,turn_left,turn_right,cheer,preen,peek,blink}_NN.png` (+ their .meta),
`art/approved_ui/` is read-only reference, working files under `art/birds_sequences/` (raw, qa, prompts). Nothing under `Assets/Scripts`.
Method: the Mac has Codex with image generation. Use the species' approved idle, cheer, fly_up and fly_down stills and the approved popup/bird style as references; keep the
plush 3D look, no outlines, identical proportions, palette and eye style. Generate each sequence as one sheet per animation if that keeps the frames consistent, then cut into frames.
Requirements per frame: 320x320 RGBA with real transparency (no white/black fringe, check on dark and light backgrounds), feet baseline and pivot (0.5,0.5) identical to the idle still so
swapping frames never makes the bird jump, same on-screen scale, 64 px and grayscale readability, import settings copied from `sunny_idle.png.meta`. Reject and regenerate any frame whose
face, colour or proportions drift. Build a contact sheet per sequence and a short looping GIF/APNG preview in `art/birds_sequences/qa/`; commit downscaled JPEG contact sheets to
`docs/work-log/bird-frame-sequences/evidence/` so the coordinator can view them from 251.
Deliver sunny first and push (so T54C/T54F can integrate), then sky, then berry; push after each species. Report exact frame counts, rejected frames, and the species still missing.
