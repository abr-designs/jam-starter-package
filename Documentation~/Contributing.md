---
title: Contributing
---
# Contributing
We welcome contributions from the community.
## Reporting Bugs
Please report bugs by [opening an issue](https://github.com/abr-designs/jam-starter-package/issues/new).

## Documentation
- All docs **must** live in the `/Documentation~/` folder.
- All docs **must** include the [FrontMatter yaml title tag](https://docs.github.com/en/contributing/writing-for-github-docs/using-yaml-frontmatter#title)
```yaml
---
title: My Title
---
```
- All docs will be written in [Markdown](https://www.markdownguide.org/getting-started/).

### Images
If you wish to contribute to the documentation, ensure that all images are as small as possible.
- All images should be in the `Images` folder.
  - If the new image fits within a folder, it should be placed within that folder.
- File size should be less than **`1 MB`**
- `GIF` I would recommend using [ScreenToGif](https://www.screentogif.com/)
  - My recommended settings are located below.

![Screen To Gif Settings](Images/screen-to-gif-settings.png)

## Pull Requests
All new pull requests must target the active development branch. The provided template helps to ensure that all required
information is provided, so please make sure to fill out all fields.
- At least one person must review the pull request before it can be merged.
- Ensure that you link the issue within the pull request

## AI Agents
This repo is set up to work with AI coding agents (Claude Code, Cursor, OpenAI Codex, Gemini CLI, and others).

### How it works
- **`AGENTS.md`** at the repo root is the single source of truth. It holds all repo guidance for agents.
- Tool-specific files point back to it, so there is only ever one file to edit:
  - `CLAUDE.md` imports it with a one-line `@AGENTS.md`.
  - `.gemini/settings.json` sets `contextFileName` to `AGENTS.md`.
  - Cursor, Codex, Factory, Jules, Zed, and Amp read `AGENTS.md` natively, so they need no extra file.
- **Edit `AGENTS.md` only.** Do not copy guidance into the pointer files; that reintroduces drift.

### Unity note
`AGENTS.md` and `CLAUDE.md` sit at the package root, so Unity imports them as `TextAsset`s and they ship committed `.meta` files (the same as `README.md`). The `.gemini/` folder is hidden from Unity by its leading dot. Do not generate `.meta` files for any `.`-prefixed or `~`-suffixed path.

### Included skills
Skills live in `.claude/skills/` and run as slash commands in Claude Code:
- **`/new-sample`**: scaffolds a new Sample end to end (the `package.json` entry, `Samples~` directory, `Documentation~` markdown, and README entry).
- **`/unity-tests`**: writes EditMode or PlayMode tests for game logic, focused on the script logic rather than MonoBehaviour lifecycle.
- **`/write-pr`**: drafts a pull request body from the branch diff and commits, following the repo PR template.

## Features & Samples
WIP