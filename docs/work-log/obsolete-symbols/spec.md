# Obsolete symbol cleanup

Audit the current main after the first artwork cleanup. Remove only unused symbol
images with no direct or dynamic consumer, plus their Godot import descriptors.
Do not remove compatibility resources, accepted references, current animation
families or gameplay functionality. Review remaining obsolete documentation
against current source. No deployment or automatic merge.

Acceptance: repository guards and all 100 stored solutions pass; existing
English/Chinese screen-state previews request none of the removed symbols.
Record the exact removed files, original hashes and validation boundaries.
