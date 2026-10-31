# Cutie Sort gameplay reference (research only)

Compiled 2026-10-31. Analysis target: **Cutie Sort 1.111 / versionCode 1111**, package `sort.casual.color.cat.puzzle.match`, Unity container declared version `2021.3.56f2`.

This is a textual interpretation of static code and native instructions for that version, **not Perch Sort rules, requirements, or implementation authorization**. [RULES](../spec/RULES.md) still governs this project's rules; current specifications determine requirement status, effective rules, and implementation boundaries, unchanged by this reference.

Evidence has three categories:

- **approved**: Locatable code or native instructions support the described local behavior; key selection, capacity, matching, progress, and deadlock conditions were additionally checked against ARM 64 instructions within fixed ranges. Local confirmation does not mean the complete call chain was recovered.
- **Partly understood**: Data structures, some calls, or local operations were recovered, but complete conditions, callbacks, or upstream events have gaps.
- **Unconfirmed**: Only declarations, names, damaged method bodies, or undecoded configuration exist; do not fill in rules from them.

These conclusions do not rely on phone-play observations. Decompiled output contains many unresolved loads, incorrect conversions, and placeholders; producing `.cs` files does not equal complete source recovery. Ghidra exception paths may also extend into neighboring functions, so interpretation is bounded by each original method's fixed address/byte range; long pseudocode tails cannot count as extra rules.

## 1. Main gameplay and layouts

The ordinary-mode flow can be summarized as:

**Take a matching cat group from the selectable end → check destination → assign empty seats and move → check full-layer match → enter order or waiting state → update completed-group count → check level completion.**

Code declares four layout types; do not assume identical details:

| Layout | Code name | Readable scope this round |
| --- | --- | --- |
| Ordinary double column | `NormalDoubleColumn` | Key paths for ordinary selection, destination conditions, matching, and progress checked |
| Staggered long nest | `LongNestStaggered` | Layout existence confirmed; all direction/operation differences not fully recovered |
| Sorting platforms | `CatOrganize` | Separate selection/reception, platform targets, and per-cat counting paths |
| Vertical long nest | `VerticalLongNest` | Layout existence confirmed; full interaction details not recovered |

Data is not a single-layer color-block list: levels contain nests, nests contain layers, and layers contain configured cat data. Runtime cats track `Id`, `SitIdx`, `layer`, owning nest, attributes, and action state. **Matching is determined by cat ID, not direct comparison of displayed color values.**

## 2. Selecting cats

### 2.1 Ordinary mode: start at the highest seat index

**approved:**

1. `GetSelectableCat` traverses non-null cats in the current layer, chooses the greatest `SitIdx`, then checks operability.
2. `GetSelectableGroup` sorts non-null cats in descending `SitIdx`, using comparator `b.SitIdx.CompareTo(a.SitIdx)`.
3. Starting at the first sorted cat, collect a continuous prefix of matching-ID operable cats; stop at another ID or an inoperable cat.

Thus players cannot bypass an endpoint blocker to extract an arbitrary middle cat. “Continuous” here means a prefix of the sorted list; the ordinary group-selection loop does not independently require adjacent seat indices to differ by1.

Highest seat index is not universally screen-left/right/top. Ordinary horizontal placement reverses local X progression for `Column == 1`; final screen direction also depends on layout and nest Transform.

### 2.2 Individual cat operability gate

`GetCanOperate(bool)` is confirmed to check both state and type:

| Item | Passing condition |
| --- | --- |
| Default action state | `Idle(0)` or `Pick(5)` |
| Specific optional parameter enabled | Additionally permits `Put(7)`; default parameter false |
| Cat attribute | `Normal(0)` or `BlindBox(1)` |

`Moving`, `Matching`, and `PendingMatch` do not pass this gate. Neither do `Question`, `Fixed`, or `Frozen` while retaining those attributes; this does not mean they can never become ordinary cats or enter other handling.

**Unconfirmed:** When each input entry permits quick reselection during `Put`, and the full priority among taps, dragging, and tutorial restrictions.

### 2.3 CatOrganize differences

`GetOrganizeSelectableGroup` likewise requires matching IDs and operability, but additionally checks **contiguity of transformed seat indices**; its boolean direction parameter advances the expected index by `+1` or `-1`.

**Unconfirmed:** Complete mapping of that direction parameter across all platforms, layouts, and actual screen directions. Do not apply this strict seat check to ordinary group selection.

## 3. Moving cats

### 3.1 Capacity and empty seats

**approved:** Current-layer capacity comes from configured `_nestData.LayerDataArray[CurLayer].CatDataArray.Count`, distinct from current runtime occupancy. It falls back to3 only when relevant data is missing.

Therefore neither “all nests have capacity3” nor “every shelf has six cats” is a universal code rule in this version. Six-cat shelves in historical videos establish only those clips' configurations.

`GetAvailableEmptySitIdxs(requested)` collects occupied indices in the current layer, then enumerates empty seats from `0` to `capacity-1`; the returned count exceeds neither `requested` nor actual empty seats.

### 3.2 Ordinary destination conditions

Checked ordinary-destination predicate conditions include:

- Source and destination are different nests.
- Destination is not retired, has operation type `Normal`, and passes `CanPutIn()`.
- If the destination has an endpoint cat, its ID must be compatible with incoming cats; an empty destination does not require an existing matching cat.
- Empty seats must satisfy the supplied `requested` count.

`CanPutIn` is only a partial gate: ordinary attributes pass; `Video` and `VideoTempHolder` additionally require relevant special elements to exist with inactive GameObjects. It does not cover all capacity, ID, tutorial, and controller restrictions.

Sorting mode's `CanAcceptOrganizeGroup` separately checks input ID greater than 0, non-retiring state, not `CantOperate`, valid current layer, and other conditions; candidate count must be below capacity. An empty candidate accepts any positive ID; otherwise its first cat's ID must match. This function receives no moving-group count and cannot alone prove the entire group fits.

### 3.3 Whole-group and partial movement: boundaries remain

The executor is confirmed to use:

`processedCount = min(selectedCatCount, assignedDestinationSeatCount)`

However, the destination-highlighting candidate path explicitly supplies **the full selected-group count** to the capacity precheck. Other real input paths' `requested` arguments and combined conditions are not fully resolved.

Only execution-layer count clipping is confirmed; **do not state that all input entries permit partial moves when space is insufficient**. Which entries allow it and which cats remain after clipping require more evidence.

### 3.4 Data ownership and animation

The recovered `ChangeToNextCatNest` path removes cats from the source collection, updates owning nest/layer, adds them to the destination, writes `SitIdx`, then enters `Moving` and movement animation. `ChangeNestResort` also reparents and refreshes sorting.

These are recovered local sequences; **complete transaction boundaries, animation callback order, cancellation/interruption, quick reselection, and exception rollback remain incompletely recovered**. Damaged tween parameters or missing callbacks are not executable implementation guidance.

## 4. Matching

Confirmed checks in `CheckCatMatch` include:

1. Current nest is neither `TempHolder` nor `VideoTempHolder`.
2. Current-layer runtime cat count equals configured capacity for that layer.
3. Cat objects are valid, IDs nonnegative, and all IDs match.
4. No cats are in `Moving`, `Put`, or `PendingMatch` states.
5. No `Question` cat still satisfies `Conditions >= 1`; Conditions here reads cat configuration `param1`.

Sorting mode also requires positive platform `OrganizeData.TargetCatId` matching the group's ID. Success makes the platform inoperable and reports the layer's cat count to `OnOrganizePlatformCollected`.

These gates are confirmed, but layout, special-element, and scheduling helper conditions are not all recovered. The list is not a complete set of sufficient conditions for all modes.

### 4.1 Ordinary full groups: order or waiting

| Check result | Ordinary branch behavior |
| --- | --- |
| `HasShownOrder(id)` true | Copy the current layer cat list and call `TriggerOrderMatch(id, cats, nest)` |
| false | Call `SleepAni()` for this layer's cats; it sets state `PendingMatch(6)` to wait in place |

`HasShownOrder` does more than search an order list for the same ID: helpers also involve `ShowPosIndex`, position occupancy, and video locks. The occupancy helper's complete meaning is not expanded; false cannot always mean no order of that species exists.

**Both ordinary success paths immediately perform `Percent += 1`, report the match, and call `CheckComplete`, without waiting for order-flight delivery.** Waiting `PendingMatch` then prevents the same layer from repeatedly passing this matching path.

### 4.2 Matching does not always uniformly expose the next layer

The order path includes `MarkRetiring`, `RemoveMatchedCats`, and later removal from the nest list, disappearance, recycling, and relayout.

`RemoveMatchedCats` itself only removes cats from current-layer and tray lists; it neither directly performs `CurLayer++` nor calls `UpdateLayer` inside that method.

A separate `UpdateLayer(true, ...)` can advance `CurLayer` if its initial completion check does not end the flow. One cleanup coroutine does call it, but existing evidence does not completely connect that coroutine to every ordinary order/waiting branch.

**Unconfirmed:** Which layouts/matching branches change layers, which retire the entire nest, and how callback order coordinates them. Do not claim every match automatically exposes the next layer.

## 5. Level completion

`Percent` is a raw integer progress count and `TotalPercent` its target, not inherently a0～100 percentage.

Initialization traverses all configured nests/layers, counts cat entries with `CatID != -1`, and remembers the configured cat-list length of the first nonempty layer as reference capacity.

| Layout | `TotalPercent` initialization | Progress after success |
| --- | --- | --- |
| Ordinary and other non-`CatOrganize` layouts | Valid cat-entry total divided by reference capacity using integer division;0 if capacity is invalid | Each ordinary successful match adds 1, counting groups |
| `CatOrganize` | Total valid cat entries | Add collected cat count, capped at the target, counting cats |

Core `CheckComplete` condition:

`TotalPercent > 0 && Percent >= TotalPercent`

Within accepted states, the method enters `Completed`; an already completed state returns true. Accepted checking states include `Initialized`, `InProgress`, and `Paused`, but this does not prove players can act while paused; invocation timing belongs to another chain.

**This check does not additionally require all nests empty or all order animations delivered.** Historical wording that results occur only after every target is physically collected cannot remain this version's source-code rule. The success interface, animation cleanup, and complete invocation timing remain incompletely recovered.

## 6. Special cats and nests

| Mechanism | Confirmed code behavior | Still unclear |
| --- | --- | --- |
| Blind-box cat `BlindBox` | On `CatMatch` with event parameter 1, clear conditions and call opening; movement events have separate presentation changes | Upstream meaning of parameter 1; cannot state any single elimination reveals it |
| Question cat `Question` | On `CatLayerChange`, if its layer equals its nest's CurLayer, clear conditions, open, and become `Normal` | Complete timing of first display and animation events |
| Fixed/frozen cats `Fixed/Frozen` | Current type fails ordinary operability gate | Independent implementation/release conditions not fully recovered; enum names cannot supply rules |
| Covering box `Box` | On `CatMatch` with remaining conditions>0, decrement 1; zero enters opening animation, with handling to restore nest operability | Whether matching broadcasts to every box, initial counts, and complete animation event chain |
| Specified-cat box `Find` | Also requires event parameter equal configured cat ID; decrement dedicated remaining count 1, open at zero, with handling restoring `Normal` operation | Exact calculation of initial count from configuration; cannot infer cats or groups required |
| Magic nest `Magic` | `LayerChange` decrements Conditions 1; hide element when conditions≤0 | All layer-change senders; cannot convert to one decrement per move |
| Video nest `Video` | Unlock event only after success callback, level not completed, and corresponding unlock precheck passed | All ordinary Video branches, configuration conditions, and costs |
| Video temporary nest `VideoTempHolder` | Unlocking does change it to `TempHolder` and restores `Normal` operation | Actual level capacities, initial counts, and complete activation conditions |
| Conveyor/movable nest `Conveyor/Movable` | Conveyor has dedicated layer numbers/tray branches; movable nests have configuration/branch declarations | Automatic triggers, directions, distance, frequency, and complete movement rules |

Handlers prove code exists for these mechanisms, not that every level enables them. Resources/remote parameters for initial values, appearance levels, and switches were not recovered.

## 7. Items and Undo

Names below describe code effects, without claiming store display names:

| Internal item ID | Confirmed effect | Cannot infer |
| --- | --- | --- |
| `1003` | Undo using history, involving return to original nest, cancellation of pending matching, seat/layer restoration, and some progress rollback | Unlimited Undo, arbitrary depth, guaranteed reversal of recycled order nests, complete atomic rollback |
| `1004` | Select one nest and execute `SortOneCatNest`, including question-cat reveal, seat adjustment, and history | Whole-board random shuffling or exact sorting priorities |
| `1005` | Find an existing unlockable nest returned by `GetUnlockableVideoCatNest()` and send Video unlock event | Creating empty nests anywhere or adding a fixed capacity |

The item system combines remaining free uses with actual inventory into effective quantity. Exact consumption order, prices, unlock levels, and all effect parameters were not recovered.

Undo eligibility is first restricted by ongoing Undo/sorting/sorting-item use and an internal busy count, then delegates further checks. History includes cats, source/destination nests, source/destination seats/layers, and seat changes/collection information produced by sorting.

**Unconfirmed:** Undo depth, reversibility of submitted orders/recycled nests, complete frozen/hidden-state restoration, failed-animation handling, and when item consumption counts as successful. Objects and boolean expressions in long restoration methods are damaged, so they are not a complete Undo algorithm.

## 8. Deadlocks, rescue, and failure

Verified `CheckDeadlock` order:

1. Continue only in `Initialized` / `InProgress`; skip other states.
2. Skip sorting mode if target invalid/already reached; skip other layouts without orders or with an internal count>0. The count's full meaning is unknown.
3. An incompletely explained prerequisite predicate must also pass.
4. In non-sorting mode, if no meaningful move exists excluding temporary storage, first try `SpecialDeadlockGuideControl.TryStartFromTopDeadlock`; return if rescue starts successfully.
5. Check `HasMeaningfulMove()` again; return if a move remains.
6. Send a message if no meaningful move exists. **This method does not directly enter `Failed`.**

Special deadlock guidance has waiting, target selection, and placement into temporary nests, plus load-version information preventing duplicate starts. Helpers selecting exact cats, rows, and temporary nests remain incompletely recovered.

Full occupancy is not immediate failure, and this action scan is not a mathematical solver proving the whole board unsolvable. The complete chain from deadlock messages to failure dialogs, revival, and subsequent actions remains unknown.

## 9. Unconfirmed items

1. Whether every tap/drag-and-drop entry permits partial group movement, and which cats clipping selects.
2. Physical endpoints, input priorities, quick reselection, and complete animation callback order across layouts.
3. All initial conditions, event-broadcast scope, and release/layer-change/whole-nest-exit chains for special cats/nests.
4. Complete scheduling of order-position occupancy, queue replacement, and locks.
5. Undo depth, special-state restoration, consumption timing, and failure handling.
6. Meaningless-move filtering, rescue target selection, and complete deadlock-message→final failure/revival flow.
7. Actual level layouts, every capacity configuration, appearance levels, item prices, and remote switches. Level JSON was not decoded; rule code cannot fabricate it.
8. A complete usable solver and optimality. Code `G/H/F` and some state structures do not equal a recovered runnable A* or proof that all levels are solvable.

## 10. Reference boundaries for Perch Sort

Useful references concern how the game handles endpoint grouping, separation of state/actions, full matches versus actual delivery, waiting caused by target queues, and event-conditioned space opening. Which experiences to adopt still needs independent design/validation.

Project specifications and adoption records decide capacity, partial movement, hidden information, extra branches, Undo, and other rules; do not rewrite project choices as verified competitor facts. This reference does not restore withdrawn/quarantined rules. It changes no project runtime rules, levels, scenes, or assets and does not authorize copying original code, art, text, or levels.

## Appendix A: input identity and evidence records

Analysis inputs were one main APK and four configuration splits. Identity records remain here; reading the text does not require deleted reverse-engineered code.

| APK | Bytes | SHA-256 |
| --- | ---: | --- |
| `Cutie Sort.apk` | 157583466 | `2bfb6cc78e4b012c93e057745a94158439e3c588e55947d68b96a38edd4384af` |
| `split_config.arm64_v8a.apk` | 24151501 | `2356285995297d38195e08109961c497d48eb19d18b0628b3cff121eae07a390` |
| `split_config.en.apk` | 78233 | `296a6cce34f215a98e414a4d48172a9233f2fce0897ba3e1747ce376ac25ea34` |
| `split_config.xxhdpi.apk` | 207037 | `df599d7a4a4f2970215ce78bfdd62e14cf38cdebc6e80b82dea733d98398077d` |
| `split_config.zh.apk` | 90521 | `004226b24b8120ff518ff66197630703201302923e3db709ce9fa39290df1c67` |

Tool record: REA `6.7.0`, JADX provider `0.7.1`, Cpp 2IL `2022.1.0-development.1743+b5ad444b82267cb1e4b88b8b373c008105bdea52`, ILSpy `9.1.0.7988`, Il2CppDumper `6.7.46`, Ghidra `12.1.4`.

Recovery attempted 578 top-level game types;576 produced C#, two UI classes failed whole-class recovery, and per-member output additionally produced 285 items with 17 failures. Recorded 9045 native game-method ranges and generated pseudocode for nine key methods. **These are export coverage statistics, not semantic accuracy rates.** No level JSON was recovered.

Some method locators used at the time follow. RVAs belong to that version's ARM 64 `libil2cpp.so`, not file line numbers or directly runnable addresses:

| Method/purpose | RVA |
| --- | --- |
| `CatNest.GetSelectableCat` | `0xa002ec` |
| `CatNest.GetSelectableGroup` / descending comparator | `0xa069f0` / `0xa0929c` |
| `CatInstance.GetCanOperate` | `0xbe5808` |
| `CatNest.LayerCapacity` getter | `0x9fff50` |
| `CatNest.GetAvailableEmptySitIdxs` | `0xa0528c` |
| Ordinary destination predicate / target-highlighting entry | `0xb4b788` / `0xb5a280` |
| Movement executor `XdV...` | `0xb47938` |
| `CatNest.CanAcceptOrganizeGroup` / `GetOrganizeSelectableGroup` | `0xa00834` / `0xa01160` |
| `CatNest.CheckCatMatch` | `0x9fc6e8` |
| `CatInstance.SleepAni` | `0xbe0168` |
| `CatNest.RemoveMatchedCats` / `UpdateLayer` | `0xa015ac` / `0xa07e04` |
| `LevelControl.TotalPercent` getter / target initialization | `0xb44168` / `0xb540bc` |
| `LevelControl.OnOrganizePlatformCollected` | `0xb5d09c` |
| `LevelControl.CheckComplete` | `0xb57388` |
| `LevelControl.CanUseRevocation` / `CheckDeadlock` | `0xb4993c` / `0xb4f038` |

As requested by the user, reverse-engineered code and app-specific intermediate artifacts were cleaned after textual documentation and checks; original exported APKs remain in a local Git-ignored directory. Rechecking original branches requires analyzing the same-hash inputs above again. This record does not claim to retain complete reproducible development source code.

## Appendix B: relationship to earlier material

Earlier [sorting research](../SORTING_RESEARCH.md) also cited videos and public descriptions. Those retain their own evidence identities: one clip's six-cat capacity cannot generalize to all levels in this version, and visible order-delivery sequence cannot replace the progress counting checked here.

Public sources for further tracing only: [official store](https://play.google.com/store/apps/details?id=sort.casual.color.cat.puzzle.match&hl=en), [historical video 1](https://www.youtube.com/watch?v=fZHsuXpTwEE), [historical video 2](https://www.youtube.com/watch?v=onptn300img). They do not replace this round's static interpretation or imply level-by-level device acceptance this round.
