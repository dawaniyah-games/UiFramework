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
bash tools/check-standards.sh
git commit -m "Describe the outcome in sentence case"
git push -u origin feature/short-description
```

Open a PR **into `dev`** and fill in the checklist below.

External contributors: fork the repo, then follow the same flow, opening the PR against `dawaniyah-games/UiFramework:dev`.

### Commit messages

Imperative, sentence case, no prefix, no trailing period, about 72 characters or less, describing the outcome. Examples from the DenDon history:

```
Handle suspended and deleted accounts from any backend or Photon call
Animate match chat bubbles and tidy player panels
Unify popup backdrops and localize club editor messages
```

Group commits by intent (runtime fix / editor tooling / tests / version bump), not by file.

## Pull request checklist

- [ ] Follows [CODING_STANDARDS.md](CODING_STANDARDS.md): `bash tools/check-standards.sh` passes (the **Coding standards** check runs it on the PR), and there are no IDE errors from `.editorconfig`.
- [ ] Assembly direction respected (`Tweening ← Core ← Runtime ← Editor`), with no `UnityEditor` usage in runtime assemblies, and nothing game-specific in the package.
- [ ] Public API and package serialized fields unchanged, or the breaking change is approved and the PR has a consumer impact note with DenDon migration steps.
- [ ] `.meta` files committed for every new file or folder.
- [ ] EditMode and PlayMode tests pass (Test Runner), and the change was checked in Play Mode with the sample project ([CODING_STANDARDS.md §11](CODING_STANDARDS.md#11-testing--verification)).
- [ ] [DESIGN.md](DESIGN.md) / [README.md](README.md) updated if behaviour, API or editor workflow changed.
- [ ] Entry added under `## [Unreleased]` in [CHANGELOG.md](CHANGELOG.md), and the file copied into the package (`cp CHANGELOG.md Packages/com.dawaniyahgames.uiframework/CHANGELOG.md`).

## Release process

1. On `dev`, bump `version` in `Packages/com.dawaniyahgames.uiframework/package.json` following [SemVer](https://semver.org/): patch = fix, minor = feature, major = breaking.
2. In `CHANGELOG.md`, rename `## [Unreleased]` to `## [X.Y.Z] - YYYY-MM-DD` and add a new empty `## [Unreleased]` above it.
3. Copy `CHANGELOG.md` into the package: `cp CHANGELOG.md Packages/com.dawaniyahgames.uiframework/CHANGELOG.md`. The root file is the source of truth; the package copy is what the Asset Store and Package Manager show. `bash tools/check-standards.sh` fails when the package's `CHANGELOG.md` or `LICENSE.md` differs from the root file.
4. Update the version line at the top of `DESIGN.md` and the pinned example in `README.md`.
5. Open a PR `dev → main` titled `Release vX.Y.Z: short description`, then merge.
   - `main` is guarded by the **Protect-main** ruleset: 1 approving review and a required status check named "Bump Package Version". That check comes from a workflow that no longer exists, so it never reports. Until the ruleset is updated, repository admins merge release PRs with the bypass (*Merge without waiting for requirements*, or `gh pr merge <n> --merge --admin`).
   - Keep `dev` after the merge: ignore the *Delete branch* button. The **Protect-dev** ruleset blocks deleting or force-pushing `dev`. Deleting it would also retarget every open PR into `dev` to `main`.
6. The **Auto release on main** workflow (`.github/workflows/release.yml`) reads the version, creates and pushes tag `vX.Y.Z`, and publishes a GitHub Release with generated notes.
   - It **fails if the tag already exists**, which almost always means step 1 was skipped. Bump the version and merge again.

## Questions

Open an [issue](https://github.com/dawaniyah-games/UiFramework/issues) or a [discussion](https://github.com/dawaniyah-games/UiFramework/discussions).
