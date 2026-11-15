# Tool qualification compatibility prototype

Local only, based on main827141b for #248. Keep catalog IDs and existing per-level records. Persist grandfathered tool eligibility independently of inventory and paid starter gifts. Read schemas1–5 through native existing migrations, then freeze eligibility from the immutable previous gates [2,4,11,5,15,17] plus existing stock/grant evidence. Do not infer old gates from replaced catalog. Migration grants no stock, coins or starter flag. Normal grant remains once-only when invoked.

New profiles use schema6 with empty legacyBoosterQualifications; they use the new catalog gates, not historical gates. Migrated profiles retain their prior qualification even after spending stock. Schema6 serialization uses a validated integer ID set; malformed/future state enters existing recovery and valid backup may recover. Exact level-ID permutation is not part of this task and belongs to later #250.

Only Profile/Progress changed in this prototype. Shipping must also coordinate launchers, which currently write schema5 and could reinterpret fresh progress as legacy. Never ship this isolated prototype alone. The separate exact-access prototype also uses experimental schema6; these are mutually exclusive prototypes, not additive production migrations. Assign final schema transitions before integration. No user-machine files are read or changed.

## Ordered schema combination

1. #248 uses schema6 solely for legacyBoosterQualifications. Legacy schemas1–5 normalize then freeze historic eligibility; new schema6 profiles never run that freeze. Level IDs/records do not move.
2. #249 changes finale definitions/reward behaviour only, retaining schema6 and IDs.
3. #250 must use schema7, retaining the schema6 field and adding authoritative catalogAccess after the final bijection is fixed. A direct schema1–5 to7 load must first execute the exact schema6 normalization/qualification step, then the level permutation. Schema6 to7 must not recompute old eligibility from highest progress or new catalog data. Repeated7 loads do neither migration again.
4. The exact-access prototype currently rejects versions except its temporary6 and uses bamboo-order. It is not combinable as-is. Before reuse, replace that prototype envelope with schema7 and preserve legacyBoosterQualifications; no catalogAccess is written in the #248 prototype.
5. Developer all-level launchers intentionally qualify all six tools without changing inventory or starter flags. Normal preview/capture paths do not boost. Future/invalid schemas bypass boost and preserve files for game recovery. Both launchers must change with each shipping schema; never downgrade7 to6.
