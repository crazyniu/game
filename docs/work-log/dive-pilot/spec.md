# Dive pilot — spec

Step two of [gameplay depth](../gameplay-depth/spec.md) (sections 3.2, 3.3, 3.5 and 4.2): the first talent, tried on
three lab levels with existing art. The rule is in section 3.5 of that page.

- A level lists talents as `"talents": {"<species>": "dive"}`; the 100 catalog levels list none.
- Lab levels live in `PerchSort/levels/lab.json` and open only with the launcher argument `--lab=<1..3>`. They
  are played with a copy of the profile: no save, no progress, no drops.
- A Dive bird wears a painted badge. Selecting a Dive run lights each branch it can only reach by diving.
- Lab level 1 opens with the hand on its first move.
