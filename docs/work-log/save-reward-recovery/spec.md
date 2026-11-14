# Reward date and save recovery

Scope: fix confirmed draw date rollback and preserve damaged saves. No gameplay, payout, UI, level or schema changes.

- A normal draw day may advance but never move backwards; existing remaining chances resume when the device date catches up. Explicit unlimited preview remains supported.
- Complete and verify a temporary save before replacing the primary; retain the previous valid primary as a backup. Recover a missing or truncated primary from the backup.
- If neither stored file is parseable, preserve both and block automatic writes to that path. Do not claim recovery of data that never had a backup.
- Regression checks use only generated files in a unique OS temporary directory.
