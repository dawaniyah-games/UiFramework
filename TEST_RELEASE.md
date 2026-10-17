# Release Workflow

How a release is published. The full checklist is in [CONTRIBUTING.md → Release process](CONTRIBUTING.md#release-process).

When a pull request is merged into `main`, the **Auto release on main** workflow (`.github/workflows/release.yml`):

1. Reads `version` from `Packages/com.dawaniyahgames.uiframework/package.json`.
2. Creates and pushes the tag `vX.Y.Z`. If that tag already exists, the workflow **fails**. This usually means the version wasn't bumped before merging.
3. Creates the GitHub Release `vX.Y.Z` with auto-generated notes and marks it as latest.

It can also be started by hand (*Actions → Auto release on main → Run workflow*). There, `allow_existing_tag: true` skips the tag and release instead of failing.

Notes:
- Bump the version on `dev` (in `package.json`, `CHANGELOG.md`, `DESIGN.md` and `README.md`) before opening the `dev → main` PR.
- `release-notes.md` holds the hand-written notes for the latest release.
