# Post-merge regression acceptance

Validate issues #254 and #272 on main `2bcc7969f1181d91dd368c6d6f12d6508973b9af`, after both dialog and test-sweep repairs landed. Use the repository's Godot 4.7.2 standard build. No gameplay, interface, art, economy or save implementation changes.

Repair only test harness gaps exposed by the new merged test inventory: distinguish successful key=value summaries from failures, and provide the explicit isolation marker to the recovery test in its already-dedicated user-data directory. Add missing malformed-primary/no-backup/restart recovery coverage. Update superseded dialog geometry assertions to match the approved #277 frame and exterior actions; replace the draw-close fixed delay with bounded lifecycle waiting and separate elapsed-time diagnostics. Retain behavior assertions. Never touch a personal save. Run the full standalone sweep, repeat the four #254 regressions three times, repeat recovery UI and exercise Exit separately, and rerun the unchanged aggregate suite.

Physical-device, audible audio, store integration and human trial acceptance remain separate; do not infer them from software-rendered cloud results.
