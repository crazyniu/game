# Environment artwork delivery

## Batch A: trunks and junctions

Eight PNG assets exported in `PerchSort/Assets/Resources/World/`: four 380x1920 curved trunks and four 240x220 branch junctions, each with real RGBA transparency and metadata derived from its existing world counterpart. Existing artwork and code are unchanged. Generation used the built-in image tool; exact prompts and source output paths are in `art/environment/generation.json`. macOS `sips` handled export sizing and transparent canvas cropping.

Measured opaque lower-trunk width ranges: green forest 154–183 px, bamboo 150–183 px, autumn 163–190 px, snow 153–173 px. The original crown attachment x=73 remains inside each trunk's opaque top section. All four composites contain one trunk, five joints, existing branches and existing crown. The primary crown overlap and fork coverage still require final placement by the integration owner; these offline arrangements are not proof of an unchanged runtime crown attachment.

The specification's world labels conflict with actual source art. Existing world 2 is bamboo; existing world 3 is autumn. New material follows existing assets to preserve branch compatibility. The discrepancy was raised with the coordinator; no response was received before continuing. Gate decorations will follow their explicit specification.

## Quality evidence and boundaries

Per-asset JPEG sheets in `evidence/` show light/dark backgrounds and 64 px thumbnails. `board_assembly_w*.jpg` compares the supplied approved reference against each offline five-junction assembly. Full-resolution composites and alpha/dimension measurements are in `art/environment/qa/`.

Joints are intended for the lower two thirds of the trunk. Render branches, then joints, then the trunk over their vertical overlap; crown renders above the fork. Exposed vertical joint ends do not blend seamlessly. Material colors are visually close, but generated grain is not pixel-identical to the existing textures. Do not treat these offline assets as integrated visual acceptance. No runtime wiring was requested or changed.

Rejected drafts retained under `art/environment/rejected/`: initial green trunk (lower-width drift); initial autumn joint (white end fades). Both were regenerated. Remaining gates and map extras are not yet generated. Unity compilation, full suites and stable captures are pending the shared lock.

## Batch B: arch gates

Three closed 560x600 RGBA gates are delivered: spring flowers, autumn leaves/berries, and frosted vines/icicles. All three open gates were generated, rejected for stationary-frame registration drift, and regenerated once with a localized-edit prompt preserving the closed master. The retry still redrew the arch. They are intentionally excluded from Resources: `gate_arch_open_w1.png`, `gate_arch_open_w2.png`, `gate_arch_open_w3.png` remain missing accepted assets.

All six rejected open drafts are retained under `art/environment/rejected/`. JPEG pair/alignment evidence is explicitly labelled REJECTED. `art/environment/qa/gate_registration.json` records upper-arch RGB differences after the retry. Do not wire rejected images as approved cross-fade states. This batch does not satisfy the complete gate-pair requirement.
