# Selected bird hover report

Updated `PerchSort/game/bird_view.gd` so selection starts a continuous loop of the existing six flight-flap frames at 18 fps. The loop remains active during the 0.18-second return to the perch, then the bird resumes its perched idle. Idle actions and neighbor glances do not interrupt the wingbeats.

Repeated selection changes cancel the previous lift/return tween. Transfers cancel that tween, carry the lifted offset into their starting position and retain wingbeats through the flight delay and crouch before the normal flight sequence takes over. Instant mode does not start the hover loop; freezing stops it.

Existing flight art is reused without generating or editing images. As with existing travel flights, these are base-species frames; outfit-specific flying artwork is not present. Rules, levels, saves and unrelated local changes are untouched.

No tests, lint, build, game launch, UI automation or captures were run, as instructed. Runtime appearance and animation acceptance remain with the user.
