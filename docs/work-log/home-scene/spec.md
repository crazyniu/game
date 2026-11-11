# Home scene repair contract

The user approved repairing Home first in the conversation "梳理页面 UI 问题与改进方案" and asked
to continue that work in the current chat. The accepted direction restores the island's scene depth,
the relationship between birds and title, and the original dimensional button artwork. Keep the
single-line Bird Sort title and all current functional entries. Deliver the edited canonical view
for the user to try; do not extend the repair to other pages.

## Ownership and boundaries

- Coordinator: `PerchSort/game/home_view.gd`, the Home sections of `docs/design/home-and-aviary.md`
  and `docs/design/island-style.md`, and this execution contract/report.
- Independent reviewer: read-only inspection of the Home source and its integration contracts.
  No file ownership and no additional workers.
- Preserve unrelated uncommitted work, shared UI helpers, coin purchase routing, audio service,
  rules, economy, levels, saves and assets. No artwork generation is needed.

## Shared contract

Use the retained layered island scenery and six existing animated birds, the current Bird Sort
wordmark and compact FeatherBar, and the original complete painted Home control textures. Place
daily challenge and check-in/draw on the right and four equal destinations at the bottom. Keep
complete localized Play faces and change the texture in place on language changes. Preserve
navigation signals, reward badges, the existing idle scheduler and canonical fake-data previews.
The measured presentation contract lives only in `docs/design/home-and-aviary.md`.

The user's explicit testing instruction overrides repository and skill verification checklists:
do not write or run tests, naming checks, lint, builds, runtime capture, UI automation or temporary
validation programs. Source review is not runtime acceptance. No commit, merge or release requested.
