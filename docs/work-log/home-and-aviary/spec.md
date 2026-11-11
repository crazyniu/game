# Home and aviary presentation alignment

## Request and shared contract

Apply the current shop style to Home and the third homepage navigation entry (Aviary). Follow `docs/design/home-and-aviary.md` and `docs/design/island-style.md`; current shop source supersedes stale prose where it differs. Scope is presentation, independently generated assets and existing preview states. Rules, levels, profile schema, navigation signals and purchase/equip rules are unchanged.

## Ownership

- Coordinator: shared design, `home_view.gd`, new standalone home artwork, integration and delivery report.
- Aviary worker: `aviary_view.gd` only, starting from the current uncommitted source supplied by the coordinator, in an isolated worktree. Do not edit shared helpers, shop, locale packs or other ongoing work. No nested workers.

No tests, builds, lint, captures or UI automation: the user's explicit testing instruction supersedes repository verification defaults. Deliver the canonical clickable screens for the user's review, with existing production data interfaces and fake-data previews preserved. No release or deployment.

## Homepage correction from the supplied phone reference

The user reported the missing title, rejected the oversized mismatched birdhouse, named the game Bird Sort,
and requested vertically stacked check-in and Play actions. The phone image is a proportion/layout reference,
not artwork to copy. This correction supersedes the earlier Home title, house and event arrangement.

- Coordinator owns `home_view.gd`, the Home section in `docs/design/home-and-aviary.md`, and native generated
  title, shore background, compact open perch, and two painted check-in word sprites. Preserve all other work.
- Independent reviewer owns only the correction section in `report.md`; read-only source/asset inspection,
  no tests, runtime, validation programs, captures or nested workers.
- Reuse ShopSkin surfaces, current birds/idle sequences, navigation signals and fake profiles. Check-in opens
  the existing Lucky Draw/check-in flow; the daily puzzle remains a smaller separate icon entry.
- Title is exactly Bird Sort. The main composition uses a smaller open perch on a sandy island shore with
  four live birds, a compact title above, check-in above Play, and the current four bottom destinations.
- Import production images so Godot can load the title and other new assets. This is asset installation only;
  no runtime acceptance, automated testing, export, release, profile mutation or rule changes are authorized.
