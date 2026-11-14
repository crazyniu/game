# Local verification
- Recharge confirmations reuse PurchaseDialog.open_shop; all six pack definitions and transaction callbacks are untouched.
- Credits reuse JungleDialog with the existing gold painted title and scrolling document texture. No font or artwork replacement.
- Godot 4.7.2 graphical captures: credits and recharge in Chinese/English, under build/screens/. Confirmed complete title, readable contents and exterior centered price button. These are real renderer preview states, not real-money purchase tests.
- run_menu_navigation: 107 checks passed. run_purchase_dialogs: 118 checks passed; now asserts the jungle surface and exterior action for recharge as well as item purchases.
- run_aux_dialogs checks both credits locales, existing painted titles, footer clearance and scroll limits.
- Existing dormant chapters() is unchanged: only preview/test callers, outdated ten-level ranges and missing Replay painted-word mapping. Normal map navigation never calls it.
- document() has only a credits preview/test caller; terms/contact textures and production entries do not exist. No legal content invented or removed. Only the existing credits document is verified.
- Already approved bird information/aviary, chest and draw pages are unchanged. No commit, push, payment, merge or deployment performed.
