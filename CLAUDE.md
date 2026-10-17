# CLAUDE.md

Unity UPM package `com.dawaniyahgames.uiframework` (`Packages/com.dawaniyahgames.uiframework/`) with a sample host project in `Assets/`.

Follow these. They are the single source of truth:

- @CODING_STANDARDS.md: code style and project rules. All code, including generated templates, must comply.
- @DESIGN.md: architecture, runtime lifecycle, invariants (§10), known limitations (§11).
- @CONTRIBUTING.md: branching, commit messages, release steps.

`.github/copilot-instructions.md` is a condensed version of the same rules for other assistants. Keep it in sync when the rules change.

Working notes:
- Unity is not driven from the CLI here. Say which Play Mode checks (CODING_STANDARDS §11) the user should run, rather than claiming a change is verified.
- The generated runtime `UiConfig` asset and `.meta` GUIDs are not edited by hand.
- A behaviour or API change also updates DESIGN.md and adds a `CHANGELOG.md` `[Unreleased]` entry.
