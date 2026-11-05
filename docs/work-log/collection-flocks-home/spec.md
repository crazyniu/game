# Collection homecoming persistence

Verify the missing normal-win integration on main and connect the existing Aviary counter to the existing committed win boundary. Count Core HomedSpecies once per rewarded winning attempt, including replay wins. Partial/abandoned attempts and undo/redo of a committed win must not duplicate counts. Preserve gameplay, reward policy, persistence keys and schema.

Owned paths: GameRoot.cs, PopupFlow.cs, HomecomingProgressTests.cs and its meta, and this work-log directory. No naming-task files or primary-checkout changes.

Validation: naming guard/self-test, serialized full Mac scene build and EditMode/PlayMode suites, stable-state screenshots. Integration tests cover real level input, PlayerPrefs readback, scene reload, partial restart, completed restart, and winning-move undo/redo. Existing test setup backs up and restores game PlayerPrefs.
