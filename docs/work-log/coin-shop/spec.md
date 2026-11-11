# Coin shop implementation

Status: authorized static UI shell; user runtime acceptance pending.

## Request

Add a paid-coin purchase page with fixed package data, entered from the homepage coin plus, in the game's current visual style. Follow the measured [screen contract](../../design/coin-shop.md) and [island style](../../design/island-style.md).

## Shared execution contract

- Implement the canonical `PerchSort/game/coin_shop_view.gd` view and its fake-data `preview()` states.
- Reuse shared shop surfaces and existing clean assets. Produce only the missing independent feather-coin package and painted lettering assets.
- Add a shared counter entry and a dedicated top modal layer in the existing navigation owner. Preserve the screen and open popup underneath.
- Show six fixed packages and clearly labelled simulation feedback. No payment integration, economy change, saved coin grant, or profile migration is in scope.
- Keep the existing game's rules, levels, persisted field names, and enum orders unchanged.

## Ownership

The coordinator owns runtime implementation, navigation/counter integration, production art, final asset paths, and the implementation report. The documentation worker owns only `docs/design/coin-shop.md` and this task specification, with no worker spawning or runtime edits.

## Delivery and verification boundary

Deliver source and assets for the user to try. The user's explicit instruction assigns tests and acceptance to the user; do not proactively run tests, naming guards, lint, builds, captures, UI automation, or temporary verification programs. No release or deployment is authorized.

Report actual changed paths and user entry points. Mark runtime appearance, interaction, motion, audio, and device behavior unassessed until the user reports acceptance; static design decisions are not evidence of runtime approval.
