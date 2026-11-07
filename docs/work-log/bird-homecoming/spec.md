# Bird homecoming

User request: birds must enter through the front opening of the hanging house, rather than disappear behind its body.

- Keep the existing flock order, 0.12-second stagger and 0.6-second flight duration after takeoff.
- Render the approach in front of the house. Aim the bird's body centre at the equipped house's existing doorway, accounting for the feet-based sprite origin.
- Use 0.45 seconds for approach and 0.15 seconds for entry. Fit the bird inside the opening before moving it into the doorway mask; continue the painted wing frames while it recedes.
- Show the resident and completion feedback after the final bird has entered.
- Preserve unrelated workspace edits, art, level rules and saved data. Testing and runtime acceptance belong to the user; do not run tests, builds or captures.
