# Contributing to UI Framework

Thank you for your interest in contributing! Please follow this workflow to maintain code quality and release consistency.

## Development Workflow

### For Repository Owner (dawaniyah-games)

1. **Create a feature branch from `dev`**

   ```bash
   git checkout dev
   git pull origin dev
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes and commit**

   ```bash
   git add .
   git commit -m "feat: describe your feature"
   git push origin feature/your-feature-name
   ```

3. **Create a Pull Request**

   - Go to: https://github.com/dawaniyah-games/UiFramework/pulls
   - Click "New Pull Request"
   - Base: `dev` ← Compare: `feature/your-feature-name`
   - Fill in description
   - Create PR

4. **Review and Merge to `dev`**

   - Review your own changes
   - Click "Merge pull request"
   - Delete feature branch

5. **Release to `main` (when ready)**
   - Create PR: `dev` → `main`
   - Title: `release: v1.0.X - Brief description`
   - Description: Summarize changes
   - Merge the PR
   - ✨ **Automatic release workflow triggers**:
     - Version bumps (patch)
     - GitHub Release created
     - Tag pushed
     - Package.json and CHANGELOG updated

### For External Contributors

1. **Fork the repository**
2. **Create a feature branch from `dev`**
3. **Make changes and commit**
4. **Push to your fork**
5. **Create PR to `dawaniyah-games/UiFramework:dev`**
6. **Wait for review and merge**

## Branch Strategy

```
main (production)
  ↑ ← PR merges only (with releases)
  |
dev (development)
  ↑ ← Feature PRs merge here
  |
feature/* (feature branches)
```

## Important Rules

- ❌ **NEVER push directly to `main`** - Only via PR
- ❌ **NEVER push directly to `dev`** - Use feature branches + PR
- ✅ **Always work in feature branches** - Off `dev`
- ✅ **Always create PRs** - For code review
- ✅ **Keep `main` release-ready** - Only merge from `dev`

## Release Process

1. Test thoroughly on `dev`
2. Create PR: `dev` → `main`
3. Merge PR
4. Automated workflow creates:
   - ✅ Version bump
   - ✅ Git tag (v1.0.X)
   - ✅ GitHub Release
   - ✅ Updated CHANGELOG.md
   - ✅ Updated package.json

## Code Standards

See [`.editorconfig`](.editorconfig) for C# coding standards. All code must adhere to:

- No `var` keyword - explicit types only
- No target-typed `new()`
- Braces always required
- `camelCase` for private fields (no underscore prefix)
- Using directives outside namespace

## Questions?

See [Copilot Instructions](.github/copilot-instructions.md) for detailed framework documentation.
