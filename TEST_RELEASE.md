# Test Release Workflow

This file tests the automated release workflow.

When merged to main via PR, it should:

1. Bump version from 1.0.0 to 1.0.1
2. Update package.json
3. Update CHANGELOG.md
4. Create git tag v1.0.1
5. Create GitHub release with auto-generated notes
