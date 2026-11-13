# Daily draw and coin purchase report

The component's detailed implementation and actual-runtime review are recorded
in [`docs/design/draw-purchase.md`](../../design/draw-purchase.md). Its evidence is
in [this folder](evidence/): 14 original Chinese/English runtime states,
comparison sheets, 24 real draw-animation frames represented by a contact sheet
and manifest, focused/aggregate logs, and native pointer interaction records.

The reviewed component has 48 hash-verified RGBA assets and 179 passing focused
assertions. The component aggregate suite passed all 100 levels. Runtime
captures are original 456 x 812, not full-resolution device captures; Dummy
audio was used and known shutdown diagnostics are disclosed in the detailed
report. These results do not by themselves verify the final combined tree.

The integration owner reapplied the stable patch without changing this
component's source, verified the draw section alongside the chest section,
rechecked all 48 hashes and reran the 179 focused assertions successfully.
Follow [`approved-ui-integration/report.md`](../approved-ui-integration/report.md)
for the final integrated suite, GUI checks and publication status. No component
commit, push, merge or deployment was performed in this local phase.
