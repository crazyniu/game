# Bird frame sequence production

The built-in imagegen tool draws the animation poses using the approved species stills as identity references. Sheet slicing, uniform export registration and QA layout do not create action frames. Existing approved art is read-only.

Every animation has its generation prompt recorded here. Failed candidates are explicitly marked rejected in the QA inventory and are never exported as runtime frames.

Sunny flap used `sunny_flap_prompt.txt`, followed by `flap_cleanup_prompt.txt` for alpha extraction. The other Sunny actions additionally reference that cleaned flap sheet for consistency. Sky and Berry actions reference their own four approved idle, cheer, fly_up and fly_down stills. The approved popup supplies the inspected style reference.

Runtime target: 320 x 320 RGBA, centered sprite pivot, toe baseline at y=300 for every frame including airborne hop poses. The consumer supplies the hop trajectory. Offline previews run at 10 fps; the hop preview demonstrates base offsets of 0, -3, -18, -30, -16, 0 pixels while using all six different drawn poses. Unity playback is a coordinator validation step.
