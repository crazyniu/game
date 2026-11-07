# Sleep and hint guidance

The user reports missing day/night information and hints that appear to request moving a sleeping bird on level 37. They clarified that the missing control means the upper-left sun/moon counter.

- Preserve authored levels and rules: level 37 has `dayLength: 0` and explicitly sleeping birds. Do not introduce a day/night countdown or change sleep flags for this level. Timed levels retain their existing phase counter.
- A transfer hint points only to the next required click: the awake source first, then the free landing seat after selection. Keep guidance through source selection and rejected taps. Clear it after a committed action or selecting a different source.
- A swivel hint points at the branch's rotation knot, which must have its own click target.
- Sleep feedback uses the matching-species face and wake-song artwork for authored sleepers; the moon is reserved for nocturnal daytime sleep. Do not add explanatory text or new artwork.
- Testing, builds, captures and runtime acceptance belong to the user.
