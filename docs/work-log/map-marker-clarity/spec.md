# Current map marker clarity

Issue #309: enforce the owner's no-glow direction by removing the extra yellow glow_soft behind the current level. Keep the existing bird as the current-level marker and retain every tile, number, star, lock, click region, navigation action and gameplay/save rule. No replacement art or shared texture change is required.

Scope is map_view.gd's four-line glow decoration and run_map_nodes.gd's corresponding regression assertion. Validate actual before/after rendering, current-level bird motion, all210 pointer regions, card clipping and the standard aggregate tests. No chest, sound, advertisement, payment or user-local catalog/tool work is involved.
