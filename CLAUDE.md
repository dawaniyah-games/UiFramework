# CLAUDE.md

Unity UPM package `com.dawaniyahgames.uiframework` (`Packages/com.dawaniyahgames.uiframework/`) with a sample host project in `Assets/`.

Follow these. They are the single source of truth:

- @CODING_STANDARDS.md: code style and project rules. All code, including generated templates, must comply.
- @DESIGN.md: architecture, runtime lifecycle, invariants (§10), known limitations (§11).
- @CONTRIBUTING.md: branching, commit messages, release steps.

`.github/copilot-instructions.md` and `AGENTS.md` are condensed versions of the same rules for other assistants. Keep them in sync when the rules change. The guide for building UI *with* the package (for consumers' assistants) is `Packages/com.dawaniyahgames.uiframework/Documentation~/ai-builder/`.

Working notes:
- Unity is not driven from the CLI here. Say which Play Mode checks (CODING_STANDARDS §11) the user should run, rather than claiming a change is verified.
- The generated runtime `UiConfig` asset and `.meta` GUIDs are not edited by hand.
- A behaviour or API change also updates DESIGN.md and adds a `CHANGELOG.md` `[Unreleased]` entry.
- Before handing work back, run `bash tools/check-standards.sh` (CI runs it on every PR) and fix what it reports. Then give the hand-off report from CODING_STANDARDS §0: what changed, files, what was and wasn't verified, consumer impact for DenDon, and the package version.
