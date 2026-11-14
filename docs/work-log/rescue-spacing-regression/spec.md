# Compact rescue spacing regression

Follow-up to #254. The guide-family test predates the approved compact rescue card. `docs/work-log/stuck-rescue-clarity/spec.md` specifies 32 px between its frame and action; `StuckOffer.ACTION_GAP` implements 32 px. Keep that product design unchanged. Other guide-family dialogs retain the existing minimum 40 px test.

Give the test helper an explicit minimum-gap argument for this one caller and assert the rescue gap is exactly 32 px. No production geometry, callbacks, saves, artwork, ads or payments change.
