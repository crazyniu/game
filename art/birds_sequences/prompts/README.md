# Bird frame sequence production

The built-in imagegen tool draws the animation poses using the approved species stills as identity references. Sheet slicing, uniform export registration and QA layout do not create action frames. Existing approved art is read-only.

Every animation has its generation prompt recorded here. Failed candidates are explicitly marked rejected in the QA inventory and are never exported as runtime frames.

Runtime target: 320 x 320 RGBA, centered sprite pivot, grounded toe baseline at y=300. Hop airborne poses intentionally lift above the shared ground plane. Offline previews run at 10 fps; Unity playback is a coordinator validation step.
