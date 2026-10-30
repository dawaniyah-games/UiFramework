# AGENTS.md

Instructions for AI coding agents (Codex, Copilot, Claude and others) working in this repository: the Unity UPM package `com.dawaniyahgames.uiframework` (`Packages/com.dawaniyahgames.uiframework/`) and its sample host project (`Assets/`).

These files are the source of truth. Read them before editing:

- [CODING_STANDARDS.md](CODING_STANDARDS.md): code style and project rules, including generated templates and the hand-off report (§0).
- [DESIGN.md](DESIGN.md): architecture, runtime lifecycle, invariants (§10), known limitations (§11).
- [CONTRIBUTING.md](CONTRIBUTING.md): branching, commit messages, release steps.

Working rules:

- Unity is not driven from the CLI. Say which Play Mode checks (CODING_STANDARDS §11) should be run instead of claiming a change is verified.
- Don't hand-edit the generated runtime `UiConfig` asset or any `.meta` GUID.
- A behaviour or API change also updates DESIGN.md and adds a `CHANGELOG.md` entry; package changes bump `package.json`.
- Before handing work back, run `bash tools/check-standards.sh` and fix what it reports.

Building UI in a game that uses the package (rather than changing the package) follows the AI build guide in [`Packages/com.dawaniyahgames.uiframework/Documentation~/ai-builder/SKILL.md`](Packages/com.dawaniyahgames.uiframework/Documentation~/ai-builder/SKILL.md).
