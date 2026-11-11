# Modal materials

## Direction and ownership

The approved page repair restores deliberate warm wood, painted lettering and dimensional controls.
Cream is the quiet inner reading surface of a frame, not a stack of decorative cards. Reuse the
existing clean `kit/panel` and `island_name_plank` artwork without extracting any reference pixels.
This document owns the shared `Modal` frame plus Win and Settings presentation. Home is unchanged.

## Shared frame

The 960 px panel width and existing content coordinates, group gaps and action metadata remain.
Draw the existing wooden panel with fixed 130 px corner slices at source scale, giving the wood
and hibiscus enough weight. Keep the full surface under the removable `Panel` node. Draw the
760 px wooden name plank across the top edge, centred 24 px below the panel top; its native
proportions produce a height of about 220 px. The central 56% carries the gold title and optional
painted level number. All title art stays under the removable `TitleGroup` node.

The plank replaces the tall rope assembly for this modal role. The title reservation, dim layer,
Close node, signatures and open/close lifetimes remain. Shop, draw and chest can continue removing
the base panel and title as before. No shared UI helper or locale catalog changes are required.

## Win

| Element | Placement in 1080 x 1920 design coordinates |
| --- | --- |
| Panel | Existing content-height calculation, 960 px wide |
| Title | Shared 760 px wooden plank at panel top + 24 |
| Hero bird | Existing 240 px BirdView; feet at the title plank top |
| Side birds | Existing 150 px BirdViews beside the title, feet at panel top + 30 |
| Stars | Existing 270 px group, 190 / 240 / 190 px stars |
| Statistics | One 120 px row when coins exist; move icon and count beside a 380 x 120 gold reward bar, separated by 64 px |
| Zero reward | Centred move count in the existing 100 px row |
| Booster drops | Existing group and count badges, no extra card |
| Actions | Existing 480 x 150 green Next above 384 x 120 Replay and Home |

Keep the three live birds, stars, counts, drop animations, rotating rays and confetti. The next-action
availability and every callback remain unchanged. Reward data is presented without a second backing
card. The primary-to-secondary size ratio remains 1.25.

## Settings — restored and frozen

The user had already approved Settings and explicitly rejected its inclusion in this redesign.
The complete Settings block in `popups.gd` is restored to the source captured before this round;
that block also matches the tracked pre-round implementation. Do not redesign Settings again
without explicit user authority.

Use the original `Simple.card`, baked `Simple.Words`, round close control and original row/action
geometry: width 840, padding 48, title height 128, four 132-high rows, 72 px icons, 54 px row words,
and 340 × 116 action pills. Menu Settings shows Help; board Settings shows Restart/Home above Help.
Music, Sound, Vibration and Language callbacks and in-place language updates remain unchanged.
The generic Modal panel is hidden, so the altered shared wooden frame is not visible in Settings.

A focused native runtime capture of `popups:settings_sound_on` was inspected after restoration;
see `docs/work-log/settings-restoration/report.md`. No other modal was rolled back by this request.

## User review boundary

Canonical preview contracts remain unchanged. User review covers Win with three, two and one star,
zero rewards, milestone drops and disabled Next; Settings from menu and board in English and Chinese,
switches, language changes and all existing actions. Runtime appearance, fit, input, animation, audio
and device behavior are NOT ASSESSED in this source-only delivery. No runtime baseline is replaced.
