# Reset saved progress

Provide an Editor-only developer menu action that refuses Play mode, asks for confirmation, and removes only the PlayerPrefs owned by `ProgressStore`, including the notifications profile. Report the number of existing keys deleted, preserve unrelated preferences, and cover the reset logic with an isolated EditMode test.
