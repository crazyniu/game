> **T30 current-engine scope (2026-11-01):** [GODOT_MIGRATION](../GODOT_MIGRATION.md) owns active implementation, cloud-first testing and gaps. Unity/C#/LitMotion/PlayerPrefs/Canvas/asmdef commands, APIs, dimensions and test results retained below are **legacy evidence/reference, not active Godot instructions or pass claims**. Engine-independent rule/content/asset-approval requirements remain; current engine mappings are in the migration matrix. Other feature expansion is stopped during migration. Native save version1 is isolated from Unity save6 and design-only economy save7.

Current product execution: native Godot framework migration only. Existing game-rule/content/visual design decisions remain reference contracts; other feature expansion is stopped by explicit user instruction. The native runtime milestone is fixed bird-only L01 with Undo/Restart/persistence/presentation, not all40 levels or implemented economy/help skills. Godot save version1 does not implement or overwrite Unity save6/design-only economy save7. User authorization permits migration submission/merge after required review/CI, not deployment. Refer to GODOT_MIGRATION for actual implemented/tested state.

# Bird Home — Economy, Shop and Help Skills

Status: CURRENT DESIGN CONTRACT / RUNTIME DESIGN_ONLY, adopted under the user’s 2026-11-01 request to define the shop, economy and help skills. Integration baseline: PR #7 head 4c2df6de4a3c98a6e5be38aafbec4242c3033877, t21-first-level-slice. Source research began at b9c4521; owning documents were reread at the integration baseline. All amounts, limits and upgrade strengths below are PROJECT INITIAL TUNING VALUES, NOT MEASURED COMPETITOR VALUES. New user instructions authorize designing coin purchases, skill purchases/upgrades and voluntary reward doubling; they do not authorize an ad SDK, real-money purchases, deployment or replacing accepted art.

## 1. Product decision

Terminology: **help skills (帮助技能)** means the three-slot assistance collection below. Undo/Restart are basic controls, not equipped skills. This file owns economy/catalog/skill policy; RULES incorporates §4 as its helpPolicyVersion2 normative extension. Existing base puzzle rules remain in [RULES](RULES.md). Runtime is not upgraded by this document. Prices are coins only, not real-money/IAP quotations.

Keep the authored sorting puzzle, not an RPG: finish a level, earn coins, choose a same-color bird appearance or a useful help item. Own several help skills but equip exactly three. Undo and Restart are separate, always free controls and do not occupy skill slots. No lives, energy, subscriptions, battle pass, random loot, equipment stats, multiplayer economy or daily-task system.

The four help skills are Hint, Extra Branch, Rewind and Shuffle. Default loadout is Hint / Extra Branch / Rewind. The first three are owned at installation; Shuffle is a permanent unlock. Paid consumable charges and permanent skill levels are separate values. A purchased bird is a cosmetic variant of an existing color category, never a new puzzle color, stronger bird or additional token.

All fixed levels must be solvable without using any skill or coins. The existing L01 N4/K2/P3 layout remains S1=AAA, S2=BBBA, S3=B; its two-transfer solution needs no help. Core completion, retirement and token conservation are unchanged. This document never creates a second extra branch, changes branch capacity, changes colors, reveals hidden eggs or changes a fixed initial layout. Shuffle is an explicit optional current-board transformation, never a change to authored level data.

## 2. Evidence and explicit adaptations

The following summaries are sufficient to understand the adopted reference without visiting links. Sources are first-party descriptions, not installation/playtest evidence.

| Source | Documented reference | Adoption and deliberate difference |
| --- | --- | --- |
| Azura, Bird Sort – Color Puzzle, Google Play, HOW TO PLAY/GAME FEATURES: https://play.google.com/store/apps/details?id=com.birdsort.color.sortpuzzle&hl=en | Color sorting, undo/restart/extra branch, completed-level coins for unlockable items/backgrounds. | Adopt completion coins and cosmetic purchases. Exact rewards, prices, replay policy and three slots are our decisions, not Azura values. |
| mobirix, Bird Sort Quest, Features: https://play.google.com/store/apps/details?id=com.mobirix.ldbis&hl=en | Bird, branch and background costumes; undo/extra branch; eggs reveal when the bird in front moves. | Same-color skins and standard branch assistance; preserve egg concealment. Do not adopt daily quests, competitions or the reader reviews' claimed item prices. |
| Simple Games, Water Sort, Moves/Undo/Hint: https://pixapps.ai/simple-games/games/water-sort/en/ | Unlimited free undo and hints; a hint is the first move of a proven solution; failed search is not proof of impossibility. | Keep free undo and verified-path semantics. New limited free grants plus paid hint inventory explicitly supersede our old unlimited Hint policy; this commercial adaptation is user-directed, not attributed to Simple Games. |
| Happy Life Games, Bird Sort strategy guide, FAQ: https://happylifegames.com/bird-sort/ | Its hint may rewind to a solvable point when the current board cannot be solved; it says this to the player. | Rewind uses explicit preview/confirmation and existing Undo snapshots. We do NOT adopt this source's different rule that a completed branch becomes free again. Our completed branches still retire. |
| Rovio Dream Blast support, How do Advertisements Work: https://roviosupport.helpshift.com/hc/en/4-angry-birds-dream-blast/faq/3064-how-do-advertisements-work-in-dream-blast/ | Optional rewarded videos can grant coins/boosters; completion required; offers are not always available. | Optional first-clear bonus only, no forced ads. Exact 2x comes from the user; callback/dedup rules are engineering requirements. This is adjacent puzzle monetization, not a same-genre movement source. |
| Rovio Hatchlings support, What is Yarn: https://roviosupport.helpshift.com/hc/en/7-angry-birds-hatchlings/faq/2450-what-is-yarn-and-how-can-i-get-it/ | Search-indexed official support states yarn upgrades outfits from the collection menu. Direct page fetch failed during this review. | Supplementary only: does NOT establish upgradable sorting skills or our tier effects. No exact first-party sorting-skill upgrade table was verified. Permanent tiers below are explicit user-directed presentation/convenience adaptations of existing hint/undo mechanics. |

FALCON Bird Sort 2 official HOW TO PLAY (https://play.google.com/store/apps/details?id=com.globalplay.birdsort2.color.puzzle&hl=en-US), checked directly, explicitly describes back-step Undo, an extra branch and shuffle changing all bird orders. Gaming Laboratory Magic Water official HOW TO PLAY (https://play.google.com/store/apps/details?id=com.magic.relaxing.water.sort.colorgame.puzzle&hl=en) also lists Undo, Shuffle and an extra tube. Neither publishes exact shuffle distribution, solvability safety, prices or upgrades. Our bounded verified-candidate policy and no-egg limit are explicit project adaptations, not competitor facts. Rewind is bulk existing Undo, not a new solver feature. Upgrade strengths remain initial project tuning, adopted as design rather than measured competitor behavior.

## 3. Coins and reward timing

Currency: one non-purchasable soft currency, `coins`, nonnegative integer. Installation balance is zero. No real-money conversion.

| Event | Reward | Conditions |
| --- | --- | --- |
| First committed clear of a campaign level | 50 | Once per campaignId + levelId; assisted and unassisted clears equal; no speed/star/perfect-run bonus. |
| Voluntary completed reward ad | Additional 50 | Once for that first-clear reward, total 100. Original 50 is already credited before any ad. |
| Replay clear | 10 | A previously completed level may pay once per reward day, with at most 5 replay rewards (50 coins) across all levels that day. |
| Restart, Undo-Win loop, scene reload, canceled/failed run | 0 | Never creates a new reward identity. |
| Replay ad | Not offered | No doubling of replay rewards; keep the choice simple. |

A first clear is not also a replay reward. Replaying that same level later on its first-clear day can qualify once; this is deliberate, not repeated unlimited farming. Finishing after Undoing a Won board cannot qualify as replay: a replay requires a new runId created by explicit Replay/Start after the completed run is closed. Rewards are outside Undo history.

Replay rewards provide a small renewable, no-ad source for consumables after the fixed 40 levels. This is a counter, not daily tasks: no check-in, streak, timer banner or obligation. Reward-day key is UTC YYYY-MM-DD. Persist the greatest observed valid day; backward clock movement never resets the counter. A forward day creates a fresh day bucket; local offline clock manipulation cannot be securely prevented. We accept that limitation for a local soft-currency prototype, do not claim secure anti-cheat, and do not add networking/account infrastructure just to enforce it. Future verified-time/cloud rules require a separate design. Offline play, first rewards and local replay rewards work normally; ads do not.

### Reward result UI

On committed win, show `+50` and current wallet; main Continue is always available. Optional ad button clearly says `Watch ad · +50 (100 total)`, not an ambiguous second 100. The offer persists with the latest unclosed result; Continue/Map dismisses its offer (not its earned base reward). An already-started ad's verified late completion remains payable even after the screen closes. No reward animation may precede successful wallet persistence. Replays show `+10` or `Replay reward claimed today`/`Today's replay rewards collected`, with Continue unchanged. Never sell a hint in the win modal.

## 4. Four-skill contract

### 4.1 Inventory and limits

Permanent ownership, permanent skill level and purchased charge inventory are different fields. Each new attempt starts with one free Hint use, one free Rewind use and the R07 free Extra Branch allowance. Restart resets those free uses; this is deliberate generosity, not an exploit to punish. Purchased inventory is persistent and does not regenerate or refund on Restart. Free uses are consumed first. Undo never restores a consumed help use. Shuffle unlock includes one purchased charge; it has no per-attempt free use.

At most three PURCHASED-inventory activations total may be committed per attempt, across Hint/Rewind/Shuffle. This cap excludes free uses and Extra Branch. Shuffle additionally has a maximum of one use per attempt. Restart begins a new attempt and restores these attempt limits, but never refunds spent inventory. A clear UI separates `Free this run: 1`, `Owned: 3` and `Lv2`; zero free allowance does not mean zero owned charges. There is no lifetime-per-level paid-use cap.

| Skill | Acquisition and refill | Exact effect | Permanent tiers |
| --- | --- | --- | --- |
| Hint | Default owned Lv1; 1 free use/attempt; refill 1 inventory charge for10 coins | After a verified route and confirmation, highlight legal next transfer. No auto-move. | Lv1 up to1 step; Lv2 up to2 numbered steps. Upgrade120 coins. MaxLv2. |
| Extra Branch | Default owned, free, no stock | Existing R07 one empty N-seat branch. No second branch, no capacity upgrade and no purchase button. | Fixed, no upgrade. |
| Rewind | Default owned Lv1; 1 free use/attempt; refill1 for20 coins | Preview and confirm undoing the most recent min(tierLimit, historyLength) actions in one atomic operation. Normal free single-step Undo remains available independently. | Lv1 up to3 actions; Lv2 up to6 actions. Upgrade120 coins. MaxLv2. |
| Shuffle | Unlock100 coins including1 charge; refill1 for20 coins | At most once/attempt; rearrange current remaining visible birds using one verified solvable candidate. Details below. | Fixed, no upgrade. Do not fake an upgrade for every card. |

Hint gives information, Extra Branch gives space, Rewind corrects recent decisions and Shuffle gives a different arrangement. Only Hint uses the existing current-board route search directly; Rewind is a bounded sequence of existing Undo snapshots. No new autonomous strategy engine is needed.

### 4.2 Exact execution and safety

Hint uses the current exact stateKey/actionSequence/viewGeneration and a verified winning path. Unknown/canceled/stale results consume nothing. A two-step preview stops before a later move depending on an egg revealed by the first move; it must not show that future egg color or a simulated newly revealed board. Show `Up to2 steps; stops at hidden birds`. A shorter preview reaching Won is valid; actual count is visible before confirmation. If the player deviates, undoes, restarts, adds space or changes views, discard remaining preview. Already delivered valid information is not refunded.

Rewind uses no solver. With no history it is unavailable and costs nothing. Otherwise show exact number of actions and the target visible snapshot; Cancel is free. Confirm restores the selected existing snapshot and pops exactly that many records, including any AddBranch record. Preserve token IDs, eggs, Home set, retired branches and original move count. Do not normalize an empty branch into Retired. Bulk rewind is not pushed as a new forward history entry. Inventory/free allowances and transaction receipts remain outside board Undo. Rewind makes no promise the older state is solvable; it is convenient multi-Undo, not a purchased guaranteed rescue.

Shuffle is allowed only while stable, not Won, with no remaining eggs anywhere on the board, and at least two different species remaining. Its scope is current Available-branch occupied slots only. Preserve every token ID, species and form, each branch's occupied-slot count and capacity, branch IDs, all retired branches, Home set and extraBranchAdded. Permute token positions; never create/remove/recolor birds or repopulate a retired branch. Reject unchanged candidates and candidates containing an already completed full same-color branch, avoiding free automatic Home events or instant win on Shuffle itself. It changes current order, never the level's original arrays.

Prefer authored and prevalidated candidate permutation plans stored per level, keyed by board-shape signature (ordered Available branch IDs and occupied counts). Each plan is an explicit permutation of the flattened occupied-slot indices in branch/slot order; every index occurs exactly once. Store at most8 plans per supported signature, with a fixed authored selection order and catalog revision. Apply those plans to the current visible tokens; no runtime RNG, random new level, seed reroll or clock-based candidate generation. Missing signature means Shuffle is unavailable with no charge. Each authored plan has a base test witness, but that witness does not certify every later token arrangement: for each current candidate, run the EXISTING Transfer-only verification solver under the same aggregate budget used for one Hint request. Select the first candidate for which a full legal path to Won is verified. If the budget ends or none succeeds, report `No safe arrangement found` and consume nothing; never infer impossibility. Cached result is bound to the state. This may be unavailable on some boards; preview before confirmation shows the actual new arrangement. No infinite search, hidden automatic purchases or guarantee that shuffle is easier/optimal. Candidate count8 is engineering tuning, not a game reward tier.

On confirm, atomic commit includes board permutation, one charge decrement, purchased-use count, shuffledThisAttempt=true and a before snapshot. A Shuffle action adds0 Transfer moves, and is undoable as an explicitly new action type; undoing it restores the complete prior board but NOT the consumed charge or attempt usage. Mark the run's result as skill-assisted once any Shuffle commits, even if subsequently undone; keep it separate from the old extra-branch-only best-score record. Restart clears this attempt's assistance mark. No-helper witnesses do not automatically certify Shuffle states; every committed candidate has its own verified continuation. RULES/DATA_SAVE adopt this design together; runtime and new score certificates remain unimplemented/unverified.

Extra Branch keeps exact R07 semantics. Its Undo rewinds the current path and restores branch availability, not a second paid branch. Restart restores initial branches. Paid skills never add space. No free/paid hint or shuffle failure is allowed to consume inventory.

### 4.3 Equipment

Exactly three distinct owned skill IDs, ordered by HUD slot. Equip outside an active run, on the map/shop. A run freezes its loadout; changes affect the next run, never hot-swap to evade activation limits. To equip a fourth, choose the replacement slot and confirm. Cancel leaves all slots unchanged. No empty-slot purchase, fourth-slot paywall or duplicate skill stacks. Free Undo/Restart remain independently accessible. A default skill not equipped cannot be activated from the skill HUD in that run; this is explicit loadout choice, not lost ownership. Extra Branch remains accessible in NoMoves as the legacy free fallback even if unequipped, to retain that entitlement. Hint and Rewind use their explicit free-per-attempt then inventory policy; no hidden unlimited fallback path exists.

## 5. Skins and the only extra coin sink

Per-color skin selection is independent. Equip one skin for each already-defined puzzle color; all birds of that color in a board use that skin consistently. A species/color ID never changes when buying/equipping. A skin's apparent biological variety is presentation only. Future colors retain a free default; shop never unlocks a gameplay color or blocks a required color.

Initial catalog per color: Default free + Variant A 80 coins + Variant B 160 coins. At most eight color families in the current content. Do not promise a production asset is ready: these are semantic catalog entries; only QA-approved derived assets can ship. Blue examples: default Blue Roundbird (the current first blue type); Blue Crest (small blue crest, same blue belly and category silhouette cue); Blue Speckle (blue wing accents and small light speckles, no category-color substitution). Other colors follow the same family model, not a random mixed-color skin pack.

Main body and wing remain recognizably in the assigned color family. Variant details cannot mask the face, silhouette/category cue, branch edge or neighboring bird; no sparkling effect or accessory confuses selectability. At 64px and in grayscale each category must remain distinguishable by its established secondary cue; same-color variants must still read as one category. Unknown eggs keep the same neutral shell regardless of equipped skin. Validate idle/selection/flight/home frames, pivots and hitboxes across every skin; cosmetics must not change collider, token position or performance class. Always preserve original art.

Recommend exactly one additional sink: a 150-coin Forest Sprig decorative set applied to approved noninteractive scenery sockets. It cannot alter branch shape, branch position, capacity, receiving-nest position, hitbox, foreground visibility or world unlocks. Do not implement nest relocation as a purchasable upgrade. No weather bonuses, rent, repair charges, pay-to-skip levels, rerolls, coins-for-world-access or extra reward multipliers.

## 6. Shop operations and readable states

Two primary tabs: Birds and Skills. Optional Forest Sprig appears in a small Decoration subsection under Birds, not another large economy screen. Skin color filters show a swatch plus accessible color name. Each skin card shows preview, price or Owned/Equipped; selection opens details before purchase. Skill card shows permanent level, owned charges, equipped status, one refill button and a separate Upgrade button with exact before/after effect. Unlock is shown only for a not-owned skill. Free Extra Branch never shows Buy. Inventory `3` and level `Lv2` must be visually distinct.

Operations:

1. Buy skin: preview -> confirm exact item/coin price -> atomic debit + ownership -> Owned -> optional separate Equip. Purchase does not silently replace a current skin.
2. Equip skin: owned item only, no cost; save selection, effective on next level/run (shop previews may change immediately). Active run retains its frozen appearance mapping for clarity.
3. Unlock Shuffle: confirm 100 -> atomic debit + ownership Lv1 + one charge. Equipping is separate.
4. Refill: owned consumable skill, buy one specified charge at its displayed cost; debit and +1 in one commit. No bundles or hidden quantity defaults.
5. Upgrade: only Hint or Rewind Lv1 to Lv2, confirm effect and price; debit and tier change together; no automatic refill or loadout swap. At max show Max level, no clickable charge control disguised as Upgrade.
6. Equip skill: choose owned skill and target of three slots -> atomic replace; duplicate equip rejected without spending.

Low balance: Buy disabled, show `Need 30 more coins`; provide Back and Play, never a forced advertisement/paywall. Ads are only on eligible results, not a nagging low-wallet shortcut. Empty inventory: show `0` and shop shortcut in a paused state, never auto-purchase on a help tap. If stock exists but run cap reached, show `Help limit reached for this run` plus free Undo/Restart, not Buy. Insufficient level history, no eligible route, no ad fill and network offline each have distinct neutral states. Busy controls disable repeated taps but keep a visible pending indicator. Failed saves never show Owned, Equipped, New level or a changed balance as committed.

## 7. Local data and transactions

Extend the future full save contract through a separate future economy-capable save contract (save7 / economyVersion1, DESIGN_ONLY); do not silently add unknown fields to the strict T22 save6 adapter or claim full production save6 is implemented. Static content schema4/rules4 payloads and level hashes do not change just for catalog skins or currency. Assistance semantics and certificate identity do require a reviewed helpPolicyVersion2. RULES owns behavior; DATA_SAVE owns actual serialization.

| Field | Meaning / invariant |
| --- | --- |
| economyVersion, catalogVersion, helpPolicyVersion | Explicit versions; unknown versions preserved read-only. |
| wallet.coins | Integer >=0; checked arithmetic; no float prices. |
| wallet.sequence, transactionJournal | Monotonic committed sequence; durable idempotency keys, immutable command/result hashes. |
| firstClearRewardByLevel | campaign/level key -> base amount, base transaction, offer state, optional ad attempt and bonus transaction. Level hash changes do not create a second first-clear reward. |
| replayRewards | greatestObservedUtcDay, paidLevelIdsForDay, paidCount<=5; each eligible run reward keyed once. |
| freeHelpState | Hint/Rewind freeUsed booleans per attempt; Restart resets; Undo does not. |
| ownedSkinIds, equippedSkinByColor | Defaults always owned; equipped IDs owned and valid for the category. |
| skills[id] | unlocked, level, charges; Extra Branch has no charge count; unknown IDs rejected/preserved, not sold again. |
| loadout[3] | Distinct owned IDs; default Hint/ExtraBranch/Rewind. |
| activeRun | runId, frozen loadout/skin mapping, purchasedHelpUsesCommitted<=3, freeHelpState, shuffledThisAttempt, skillAssisted, board snapshot reference. |
| pendingAdAttempts | Attempt ID bound to one first-clear reward; status and validated completion evidence; no ad personalization secrets. |
| migrationReceipts | Source version/hash, adapter revision, imported entitlements and one-time grant keys. |

One serialized save gate owns board, progress and wallet mutations, preventing a stale shop save from overwriting a win or vice versa. Each operation validates latest ownership/balance/catalog version and expected sequence, builds one immutable candidate, persists atomically with backup/readback, then updates UI. Reuse DATA_SAVE's NotCommitted/Committed/Uncertain distinction. If replacement is uncertain, read back the same operation ID/candidate hash; retry the same candidate, not a new debit.

Win commits completion, unlock/progress, reward receipt and base +50 together. Replay result and +10 similarly commit together. Skill consumption commits charge decrement, run use count and (for Rewind/Shuffle) target board/history together. For informational skills it commits the verified preview receipt before showing it; a crash after commit may reopen that still-valid preview without spending again. A view change can invalidate preview content, but must not replay the debit.

Purchases and upgrades are exactly-once, serial commands. Two taps with the same request ID return the recorded result. A second different purchase request for an owned skin or max tier is rejected, never charged. Refill requests require deliberate quantity confirmation; button taps while a request is pending reuse the same request ID. A catalog price change invalidates the old quote and asks for confirmation of the new price; never silently charge more.

On confirmed uncommitted failure, balance and inventory remain unchanged. On uncertain outcome, lock spending and retain evidence pending readback. Do not 'refund' an uncertain operation, which could duplicate currency. If a committed purchase was invalid because a catalog asset was withdrawn, one compensating transaction restores exactly the paid coins and records the revoked entitlement; preserve upgrade history and reset equipment only to a valid free default. This is technical correction, not an unrestricted buy/use/refund exploit. Ordinary successful cosmetic purchases are final in the prototype; cancellation before commit costs nothing.

Local offline progress is device-local, not an account. Clearing app data or using another device does not restore a wallet; no cloud-save promise. Retain last compatible backup; corrupt/unknown saves are preserved, not automatically reset and granted coins. No real-money entitlement restoration is claimed.

## 8. Ad state machine (mock contract, not SDK integration)

Offer Eligible -> Requested -> Showing -> CompletionVerified -> BonusCommitted. Alternate branches: unavailable/failed/canceled -> Eligible while result remains open; dismiss -> Closed. One outstanding attempt per reward; input double-taps cannot create two SDK requests. Only an explicit provider reward-completion event accepted by the future adapter qualifies, not a close callback, elapsed timer, background event or view animation.

Base +50 is irrevocably earned before ad launch. A verified completion creates an idempotent `bonus:<campaign>:<level>` +50 transaction. Duplicate callback, late callback, retry and process recovery all find that key and cannot pay twice. Ad cancellation/failure leaves base intact and gives no bonus. Network loss before verified completion is not proof of completion; retain pending state and provider evidence for reconciliation, show bonus pending if outcome is uncertain. Do not fabricate a reward when verification is impossible. Closing the ad screen never deletes a recorded valid completion. Success followed by a contradictory failure still remains success; failure/close is not allowed to revoke a committed bonus.

No fill/offline: clearly unavailable, Continue remains enabled. An SDK is not installed in this design task. Test through a deterministic fake adapter with success/cancel/fail/duplicate/late/unknown callbacks. Live SDK/privacy/consent and platform requirements are separate authorized integration gates. This document is not legal or platform-policy approval.

## 9. Ten-level no-ad simulation

Assumptions: 50 coins per first clear, no replay income, no ad, one free Hint and Rewind per attempt; no refill purchases. These free uses do not alter coin totals. Prices are initial tuning, not a promise of measured playtime.

| Level first clear | Cumulative coins earned | Planned purchase after result | Spend | Balance |
| --- | ---: | --- | ---: | ---: |
| 1 | 50 | None | 0 | 50 |
| 2 | 100 | Blue Crest skin | 80 | 20 |
| 3 | 150 | None | 0 | 70 |
| 4 | 200 | Hint Lv2 | 120 | 0 |
| 5 | 250 | None | 0 | 50 |
| 6 | 300 | Shuffle unlock +1 charge | 100 | 0 |
| 7 | 350 | None | 0 | 50 |
| 8 | 400 | None | 0 | 100 |
| 9 | 450 | Rewind Lv2 | 120 | 30 |
| 10 | 500 | Two Shuffle refills | 40 | 40 |

Alternative priorities are real: save for skin B (160) by L04; Shuffle unlock first by L02; Rewind upgrade (120) by L03. Two 10-coin Hint refills can be funded by two eligible replay wins, without ads. A 20-coin Rewind/Shuffle refill takes two eligible replays. Daily replay maximum is 50, independent of ads. These are completion counts, not fabricated minutes: actual time-to-purchase requires playtest measurement.

All 40 first clears earn 2,000 coins without ads, or up to 4,000 if every first-clear bonus is completed. At eight color families, both paid skins per family total 1,920; all permanent skill unlock/upgrades total 340; decoration 150, so complete nonconsumable collection costs 2,410. A no-ad collector who buys no refills needs another 410, or 41 eligible replay clears over at least 9 reward days. This is OPTIONAL collection, never level access. A player spending on consumables will collect later. Do not advertise 'all cosmetics by level40'; do not increase board difficulty to sell charges. Review these collection numbers in testing and lower prices before adding chores or new currency.

## 10. Migration and superseded policy

This is a conscious replacement of old free-unlimited Hint policy, not coexistence with an invisible second rule. Undo/Restart and one free Extra Branch per run remain unchanged. The current design now adopts free-per-attempt plus charge-driven Hint. Existing running builds retain their implemented behavior until an authorized, tested migration; this documentation does not change them.

No evidence was found of released users or historical paid entitlements; do not invent either. Preserve test saves. For recognized older saves: make a read-only backup, migrate completed set under existing campaign identity rules, preserve settings and valid skins/verified ownership, and write one receipt. Grant 50 coins for each already-completed level of the SAME recognized campaign exactly once, marking those first-clear rewards claimed with no retroactive ad offer. Initialize free-per-attempt Hint and Rewind allowances without inventing retroactive consumable gifts. Never map old rules2 campaign results into new campaign completions merely because numbers match. Unknown/experimental formats remain isolated until an explicit adapter exists.

If evidence later shows an actual permanent unlimited-Hint entitlement was granted, preserve it with an explicit legacyFreeHint entitlement (no charge or paid help-use cap for that helper); do not confiscate it or silently turn it into a few charges. Do not grant this flag to all unverified local test saves. Historical bought items require verified receipt mapping, not guesses from field names. Legacy entitlement status must be visible in the shop so two valid cohorts have explicit policies, not contradictory hidden rules.

## 11. Acceptance cases

| ID | Required result |
| --- | --- |
| EC01 | Win from 0 ->50; repeated win callback/reload/Undo-Win yields no extra base reward. New legitimate replay can earn only its eligible 10. |
| EC02 | First-clear ad success ->100 total; canceled/failed/no-fill ->50; duplicated/late success never exceeds100 for that level. |
| EC03 | Replay same level twice in one reward day yields10 then0; five distinct eligible rewards yield50; sixth0. Backward clock does not reset; local forward-clock limitation disclosed. |
| EC04 | Purchase skin from80 ->0 and Owned atomically; repeated request no second debit. Equip never changes species/level hash/hitbox. |
| EC05 | Buy Shuffle at100 ->0, unlocked Lv1 and exactly1 charge; duplicate unlock cannot add free charges. |
| EC06 | Upgrade Hint charges120 once, Lv2, unchanged charge count. Lv2 repeated request rejects free of cost. |
| EC07 | Start/Restart restores exactly one free Hint/Rewind use; Resume preserves remaining free uses; Undo never restores free uses or inventory. |
| EC08 | Unknown/stale/canceled route never consumes; confirmed valid use consumes its free allowance first, otherwise1 inventory charge and1 purchased-use count exactly once. Third purchased activation allowed, fourth blocked without spend. |
| EC09 | Rewind restores exactly the previewed last3/6-action snapshot and consumes its free use or charge once atomically. Paid ledger cannot be undone. |
| EC10 | Hint preview stops at hidden-information boundary; Shuffle rejects egg states, preserves invariants, verifies candidate before debit, and its Undo never refunds stock. |
| EC11 | One free branch per run, same N; no shop purchase creates second branch. Undo/Restart remain unlimited at0 coins/0 inventory. |
| EC12 | Zero coins/zero stock/no ads: every authored base board still has a valid no-helper witness. Evidence must distinguish independent replay, actual Core tests and Unity runtime. |
| EC13 | Pending purchase/ad/skill save + background/crash resumes old or complete new transaction, never lost coins without entitlement; Uncertain locks writes and verifies before retry. |
| EC14 | Skill loadout has three distinct owned IDs; cancel replacement leaves unchanged; active run loadout frozen; Undo/Restart are outside those slots. |
| EC15 | Every skin passes64px/grayscale/category cue/egg-concealment/flight-pivot/in-context checks before integration. Text concepts are not accepted cutouts. |
| EC16 | Full40 non-helper replays remain valid; L01 stays two transfers. Cosmetics, purchases and rewards do not rewrite level content or claim old v2 certificates validate new policy. |
| EC17 | Offline no-ad economy completes rewards, purchases and migration; same-device persistence verified, no false cross-device restore promise. |
| EC18 | Migration once-only credit uses recognized campaign identity, preserves backup, no unknown save overwrite or automatic revocation of evidenced permanent rights. |

## 12. Implementation and verification status

This is the current researched design and engineering contract, not a runtime delivery. The documentation package changes no SDK, code, app build, accepted asset or remote environment. No Unity/runtime tests were run. Numbers were arithmetically checked; purchase timing, solve fairness under help previews and retention/collection pacing remain playtest gates. Nest position and final visual layouts are owned by the parallel design work, not by this economy file.
