# Hint and sleeping feedback

Scope: issue #102. Keep canonical rules, levels, persistence and entrance presentation unchanged. Own only hint/sleep/booster hooks in GameRoot, hint display in BoardView, sound IDs in shared Audio, SleepBubble and three regression fixtures.

- Exercise repeated actual hints across all 40 authored levels; record CSV and compare real action count with Core BFS optimum plus two. Explain shortest-path reversals with a short reason arrow. Never substitute an unproven greedy move when search times out.
- Sleeping tap: 0.35 seconds, two eased vertical cycles bounded by six design pixels; no body mirroring or scale shake. Pulse existing Zzz from one to 1.5 and back at full alpha. Show a cream rounded 300 by 120 icon-only wake explanation for 1.6 seconds, dismissed on the next tap. All sleepers have 1.6-second rising/fading Zzz loops covering 30 pixels. Respect Instant and lifecycle cancellation.
- Give each booster and undo a distinct effect ID, reuse existing clips, trigger once at successful commit, and silence playback under mute.
- Run naming guard, changed fixtures and full Unity suite once under the shared lock. Commit timed 0.1-second sleeping contact sheet, hint CSV and report. Do not stop the user's open Editor.
