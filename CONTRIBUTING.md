# Contributing to UI Framework

Thanks for contributing. Before writing code, read:

- [DESIGN.md](DESIGN.md): how the system works and the invariants it relies on.
- [CODING_STANDARDS.md](CODING_STANDARDS.md): the rules every change must follow. PRs that break a **MUST** rule are not merged.

## Branch strategy

```
main      production. Every merge creates a tagged GitHub Release
  ↑ PR only (release PRs from dev)
dev       integration
  ↑ PR only
feature/*, fix/*, docs/*   one branch per change, cut from dev
```

- ❌ Never push directly to `main` or `dev`.
- ✅ Branch from an up-to-date `dev`, open a PR back into `dev`.
- ✅ `main` only receives release PRs from `dev`.

## Day-to-day workflow

```bash
git checkout dev
git pull origin dev
git checkout -b feature/short-description
# ... work ...
git commit -m "feat(editor): short description"
git push -u origin feature/short-description
```

Open a PR **into `dev`** and fill in the checklist below.

External contributors: fork the repo, then follow the same flow, opening the PR against `dawaniyah-games/UiFramework:dev`.

### Commit messages

[Conventional Commits](https://www.conventionalcommits.org/): `type(scope): summary`

| Type | Use for |
| --- | --- |
| `feat` | New behaviour or API |
| `fix` | Bug fix |
| `refactor` | Code change with no behaviour change |
| `docs` | Documentation only |
| `chore` | Version bumps, settings, housekeeping |
| `ci` | Workflow changes |

Common scopes: `runtime`, `core`, `editor`, `tweening`, `sample`, `release`.

## Pull request checklist

- [ ] Follows [CODING_STANDARDS.md](CODING_STANDARDS.md): no IDE errors from `.editorconfig`, and new log messages are tagged (`[ClassName]` in runtime code, `[UiFramework]` in editor tooling).
- [ ] Assembly direction respected (`Tweening ← Core ← Runtime ← Editor`), with no `UnityEditor` usage in runtime assemblies.
- [ ] No serialized field renamed or removed without `[FormerlySerializedAs]`.
- [ ] `.meta` files committed for every new file or folder.
- [ ] EditMode and PlayMode tests pass (Test Runner), and the change was checked in Play Mode with the sample project ([CODING_STANDARDS.md §11](CODING_STANDARDS.md#11-testing--verification)).
- [ ] [DESIGN.md](DESIGN.md) / [README.md](README.md) updated if behaviour, API or editor workflow changed.
- [ ] Entry added under `## [Unreleased]` in [CHANGELOG.md](CHANGELOG.md).

## Release process

1. On `dev`, bump `version` in `Packages/com.dawaniyahgames.uiframework/package.json` following [SemVer](https://semver.org/): patch = fix, minor = feature, major = breaking.
2. In `CHANGELOG.md`, rename `## [Unreleased]` to `## [X.Y.Z] - YYYY-MM-DD` and add a new empty `## [Unreleased]` above it.
3. Update the version line at the top of `DESIGN.md` and the pinned example in `README.md`.
4. Open a PR `dev → main` titled `release: vX.Y.Z - short description`, then merge.
   - `main` is guarded by the **Protect-main** ruleset: 1 approving review and a required status check named "Bump Package Version". That check comes from a workflow that no longer exists, so it never reports. Until the ruleset is updated, repository admins merge release PRs with the bypass (*Merge without waiting for requirements*, or `gh pr merge <n> --merge --admin`).
5. The **Auto release on main** workflow (`.github/workflows/release.yml`) reads the version, creates and pushes tag `vX.Y.Z`, and publishes a GitHub Release with generated notes.
   - It **fails if the tag already exists**, which almost always means step 1 was skipped. Bump the version and merge again.

## Questions

Open an [issue](https://github.com/dawaniyah-games/UiFramework/issues) or a [discussion](https://github.com/dawaniyah-games/UiFramework/discussions).
