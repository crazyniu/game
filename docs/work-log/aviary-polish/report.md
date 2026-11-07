# Aviary list polish report

## Subsequent approval

The user approved this list and authorized extending its treatment to the remaining screens. See [UI polish](../ui-polish/report.md) for that follow-up. The first-page delivery below records the original review boundary.

## Delivered for user review

Only the aviary list was restyled in the canonical `PerchSort/game/aviary_view.gd`:

- Complete English title/name textures replace the assembled-letter labels on this page. Small names use plain dark ink without outlines; the title is forest green.
- A cream header separates the close button, title and collection counter. The counter has consistent numeral cells and a dedicated icon.
- Cards use softer shadows and separate bird, ability-badge and name regions. Three complete rows fit the viewport at rest; scrolling retains the existing interaction model.
- The unknown silhouette no longer repeats an Unknown caption. Existing live birds and their animations remain in place.
- Clean close/feather SVG assets are scoped to the list. The shared UI kit and other screens are unchanged.

The user authorized this bounded UI implementation after requesting a proposal. Core/meta, rules, level data, save fields, economy and bird-animation code were not changed. Existing launcher/window changes and concurrent bird-model work were preserved.

## Multilingual foundation, English content only

`game/ui_text.gd` resolves a full locale, then its base language, then the English default declared in `art/locale/catalog.json`. Labels are requested through stable IDs such as `aviary.title` and `bird.sky.name`; the visible English wording is not a resource identifier. Only the aviary list has been migrated so far.

The editable source is `tools/kit/locales/en.json`. The offline generator creates whole phrases, supports explicit line breaks, and emits uniform numeral cells plus common baseline/cap metadata. No font is rendered at runtime. Only the English pack is shipped, so there is no language-switch control.

To add a locale, author its source catalog with the same IDs and an appropriate source font, generate the pack, then register its manifest in `art/locale/catalog.json`. The list refreshes on Godot's translation-change notification and retains its scroll position. Other screens can migrate after the user approves this page.

Font source: [Nunito from Google Fonts](https://github.com/google/fonts/tree/main/ofl/nunito), with its OFL license retained under `tools/kit/fonts/`. Godot locale integration follows [TranslationServer](https://docs.godotengine.org/en/stable/classes/class_translationserver.html) and [Node translation notifications](https://docs.godotengine.org/en/stable/classes/class_node.html#class-node-constant-notification-translation-changed).

## Production and handoff

Asset generation:

```sh
DYLD_LIBRARY_PATH=/opt/homebrew/opt/fribidi/lib uv run --no-project --isolated --with pillow tools/kit/build_localized_labels.py en
```

This produced 11 complete label textures and 12 numeral/symbol cells. Two initial production attempts could not locate FriBiDi; the final command used the already-installed library. No system package was installed.

User preview:

```sh
./run.sh --preview=aviary_view:list
```

The requested native page is open in the 936 x 1664 window. It displays the revised header and three complete rows of English bird cards. The preview uses its existing separate example profile, does not connect saving, plays menu music and quits when its close button is pressed. A single native-window observation confirmed the requested page was displayed; no screenshot baseline was created or accepted.

No test suite, lint, build, automated interaction, timed animation check or additional state-capture run was performed. Multilingual packs beyond English have not been authored or assessed. Visual acceptance belongs to the user; implementation stops at this page pending feedback.
