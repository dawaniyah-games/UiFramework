# UI Framework - Final Workflow Test

This file verifies the complete automated release workflow is functioning correctly.

## Expected Behavior

When this commit is merged from `dev` to `main` via Pull Request:

1. ✅ GitHub Actions workflow triggers automatically
2. ✅ Reads version from `Packages/com.dawaniyahgames.uiframework/package.json`
3. ✅ Creates git tag `vX.Y.Z` (if missing)
4. ✅ Creates GitHub Release (auto-generated notes)
6. ✅ Release appears at releases page

## Test Date

2025-12-05

## Package Info

- Name: com.dawaniyahgames.uiframework
- Repository: https://github.com/dawaniyah-games/UiFramework
- Installation: Via UPM Git URL
