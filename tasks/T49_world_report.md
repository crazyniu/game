# T49 world asset delivery

Status: IN PROGRESS. Branch `mac-art/world`, based on accepted T48 at `825ca1d`. World assets only; birds/eggs remain owned by mac-birds. No Unity or runtime code changes.

## W1 first delivery

Four 1080x1920 spring backgrounds: morning, noon, golden, rain. Built-in image-tool generations reference the approved `art/v7/ui7_level_k3_v2.png`; preserve pale haze, empty playfield and top-corner leaves. No birds, perches or UI. Exact prompts/raw sources are under `art/v7/world/backgrounds/`; `delivery.json` records dimensions, alpha and hashes. Only proportional center-fit resizing used. Backgrounds are opaque RGBA.

W1 transparent pieces delivered: branch620x90, twig400x80, crown600x400, trunk160x1920. Branch flat top y=17, usable x60..580; twig top y=18, usable x60..370. Crown attachment(138,319). Exact alpha/pivots in `art/v7/world/pieces/w1/metadata.json`. Crop/clip trunk above crown attachment in scene; mirror left pieces for the right side. W4, W2 and W3 follow after W1 is pushed. Lead addendum: six coloured256x256 booster illustrations are also in progress, in V7/Art. Rendering-style matching is static reference review, not runtime acceptance.
