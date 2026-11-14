# Viewport-local inventory test input

Repair #254's zero-stock pointer assertion without changing game behavior. The test obtains the button center from the logical 1080x1920 viewport, so inject motion and button events into that viewport with local coordinates, following existing aviary/map tests. Preserve real pointer routing, the one-more request assertion, cancellation/profile checks and screenshots. Do not replace clicks with signal emission or call the tool handler to satisfy the failing assertion.

Scope: `PerchSort/tests/run_tool_inventory_gui.gd` and this work log only. No product, audio, chest, advertisement/payment integration, save or catalog changes. This is a focused follow-up, not completion of the full #254 sweep.
