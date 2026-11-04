# T48 static QA index

These are static inspection composites, not Unity screenshots or completed animation. Source-screen placement comparisons do not replace the measured coordinates in `art/approved_ui/layout.json`. Approved originals remain unchanged.

| Group | Main review evidence | Detailed records |
|---|---|---|
| HUD: 44 sprites | [64 px color/grayscale](hud_64.png), [individual approved-screen overlays](hud_overlays/) | [generated prompts](../generation_manifest.json), [atlas extraction](../extracted/provenance.json) |
| Panels / Shop / Aviary: 16 sprites | [contact sheet](../panels/qa/contact_sheet.jpg), [approved-screen overlay sheet](../panels/qa/overlays_sheet.jpg), [ribbon slices](../panels/qa/ribbon_slices_overlay.jpg) | [metadata](../panels/delivery.json), [report](../panels/report.md) |
| Houses: 16 sprites | [all roofs and doors](../houses/qa/assembled_sheet.jpg), [beams](../houses/qa/beams_sheet.jpg), [overlays](../houses/qa/overlays_sheet.jpg) | [anchors](../houses/delivery.json), [report](../houses/report.md) |
| Map: 11 sprites | [40 measured node positions](../map/qa/all_paths.jpg), [join checks](../map/qa/final_rgba_all_joins.jpg), [sprite overlays](../map/qa/all_sprites_over_approved.jpg), [64 px recognition](../map/qa/sprites_64px_color_gray.png) | [paths](../map/world_paths.json), [report](../map/report.md) |
| Effects: 20 sprites | [light/dark alpha sheet](../effects/qa/effects_contact.jpg), [approved-screen overlays](../effects/qa/overlay_contact.jpg) | [metadata](../effects/delivery.json), [report](../effects/report.md) |
| Daily gift: 4 sprites | [closed](../gift/qa/closed_dark.jpg), [open](../gift/qa/open_dark.jpg), [approved-screen overlay](../gift/qa/overlay_closed.jpg) | [anchors and layers](../gift/delivery.json), [report](../gift/report.md) |
| Gates: 9 sprites | [parts and fallbacks](../gates/qa/gates_contact.jpg), [three worlds with light](../gates/qa/world_light_composites.jpg), [approved-screen overlay](../gates/qa/gate_open_overlay.jpg) | [hinges and layers](../gates/delivery.json), [report](../gates/report.md) |

House multi-part screen overlays sometimes compress their group vertically; use the uncompressed house/beam sheets for alignment and preserve aspect ratio in runtime. Map backgrounds need the documented 2840 px stride and topmost fade handling. Glows require runtime material/intensity review. Regenerated parts are approved-reference approximations, not exact screen cutouts.
