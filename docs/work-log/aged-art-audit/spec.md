# Aged artwork audit

Limit image removal to unused assets first introduced into Git more than 24 hours
before this audit. Git introduction is a verifiable age proxy, not the original
file creation time. Confirm direct and dynamic consumers, retained design/source
uses and current replacements before deleting; do not deduplicate by hash alone.

Keep pending #229/#230 changes separate. Update only directly superseded current
constraints; preserve historical facts and licenses. Validate repository guards,
all stored levels and relevant screen construction. No automatic merge/deploy.
