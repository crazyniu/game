# T17 — Lightweight CI and automatic merging

- Protect `main` with an active ruleset requiring PRs, the `CI` check, and resolved review conversations; block deletion and force pushes.
- Enable squash auto-merge for non-draft PRs from repository collaborators on same-repository branches. External PRs require a maintainer to enable auto-merge.
- Run existing pure C# Core tests only when Core or its CI harness changes. Check changed JSON/asmdef syntax and shell/Python syntax; validate workflow changes with actionlint.
- Avoid duplicate push builds, platform matrices, Unity downloads, screenshots, and exhaustive solver runs. Cancel superseded PR checks.
- Keep Unity compilation, UI/scene validation, and EditMode/PlayMode acceptance in the existing Mac workflow; the lightweight check does not cover those.
