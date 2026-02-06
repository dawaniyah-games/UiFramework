# Test Release Workflow

This file tests the automated release workflow.

When merged to main via PR, it should:

1. Read the version from Packages/com.dawaniyahgames.uiframework/package.json
2. Create git tag vX.Y.Z (if missing)
3. Create GitHub release with auto-generated notes

Notes:
- Version bump is expected to be done manually (in dev) before merging to main.