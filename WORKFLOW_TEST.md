# UI Framework: Release Workflow Check

Use this after merging a release PR into `main` to confirm the automated release worked.

## Expected result

1. The **Auto release on main** run for the merge commit succeeds (`gh run list --workflow release.yml`).
2. The tag `vX.Y.Z` exists and matches `Packages/com.dawaniyahgames.uiframework/package.json` (`git ls-remote --tags origin`).
3. The GitHub Release `vX.Y.Z` is marked **Latest** (`gh release view vX.Y.Z`).
4. A project can install it with the Git URL ending in `#vX.Y.Z`.

## Last verified

- Version: v1.1.0
- Date: 2026-10-17

## Package info

- Name: `com.dawaniyahgames.uiframework`
- Repository: https://github.com/dawaniyah-games/UiFramework
- Installation: UPM Git URL (`?path=Packages/com.dawaniyahgames.uiframework#vX.Y.Z`)
