# T53D Map and world gate (defects D10, D11)
Read `tasks/T53_ui_fidelity_contract.md` first. You own `MapView.cs`, `MapModel.cs`, `MapMotion.cs`, `GateView.cs`, `LeafTransition.cs`.
Goal: `map.jpg` matches `ui7_map.png`: stone/wood nodes with correct sizes, star trios on completed nodes, glowing current node with the chick and sparkles,
locked stones with padlocks, the vine-arch gate at the end of the world segment (locked/unlocked states), bottom bar discs. Scroll viewport must start
at the player's current node. Verify with fixtures at: new game, level 4 current, world end (gate visible). Do not change progress/unlock logic.
