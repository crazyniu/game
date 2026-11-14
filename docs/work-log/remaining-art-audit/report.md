# Remaining artwork and constraint audit report

## Coverage and decisions

Reviewed main f8603cb (including #228) together with pending #229 in an isolated
local integration workspace. The PR itself contains only documentation; #229's
image deletions and gameplay-document updates are not duplicated.

- Examined 2,539 runtime image files and import pairings: no orphan import files.
- Checked all Art.ROOTS for duplicate bare PNG names: no shadowed same-name files.
- Hash-identical images are not automatically unused: locale digit files and
  equal-ability labels retain distinct manifest IDs; island/kit shapes have
  separate direct or dynamic callers. No aliasing refactor is included.
- Bird still poses, faces and flight frames have dynamic consumers in BirdView
  and WorldBirds. Locale labels are loaded by Simple/UiText and production tools.
- The 81 non-runtime world-bird paintings remain design references associated
  with the retained species list. A dropped gameplay direction alone is not
  proof that design source artwork should be deleted.
- Current day/night tutorial and preview consumers still exist; their assets
  remain, despite removed day/night rules in the authored level catalog.
- Kept licenses/provenance and accepted source/reference art.

No additional image has sufficient evidence for safe deletion in this round.

## Documentation change

Mark the older Shop and FamilyDialog visual contracts as historical. Link later
contracts that explicitly replaced their layout/material requirements; remove
only the ambiguity that those old hard requirements govern new work. Preserve
historical geometry, behavior constraints and the distinction between source
consistency and user visual acceptance.

## Validation boundary

Documentation-only diff; no runtime source, asset or gameplay changes. Checked
local Markdown link targets and git diff whitespace. No new graphical acceptance
or gameplay test pass is claimed for this documentation change.
