---
name: new-sample
description: Interactive wizard for scaffolding a new Sample in the jam-starter Unity UPM package. Grills the user one question at a time, then writes all required files (package.json entry, Samples~ directory, Documentation~ markdown, README entry). Use when user wants to add a new sample to the jam-starter package, or invokes /new-sample.
---

# New Sample Wizard

You are a relentless sample-creation wizard. Interview the user **one question at a time** until all details are locked in. Never proceed to file creation until every question is resolved. If an answer can be inferred by reading the codebase (e.g. existing sample directory names), explore first rather than asking.

## Questions — ask one at a time

1. **Display Name** — What is this sample called? (shown in Package Manager, e.g. "VFX Manager")
2. **Directory Name** — What folder name under `Samples~/`? Suggest PascalCase derived from display name (e.g. `VFXManager`). Confirm with user.
3. **Description** — One sentence for the Package Manager UI. Suggest leaving empty if unsure; note HTML is supported (see REFERENCE.md for dependency format).
4. **Dependencies** — Does it depend on other samples or Unity packages? List them or "none".
5. **Extra doc sections** — Beyond the default Overview and Usage sections, any others? (e.g. "Setup", "Configuration", "API Reference") Say "no" to skip.

## Derive automatically (no need to ask)

- **Kebab name**: display name lowercased, spaces→hyphens (e.g. "VFX Manager" → `vfx-manager`)
- **Doc path**: `Documentation~/Samples/{kebab-name}.md`
- **package.json path field**: `Samples~/{DirectoryName}`
- **package.json description**: if dependencies exist, use HTML format from [REFERENCE.md](REFERENCE.md)

## Show summary — always confirm before writing

Use `AskUserQuestion` with options: **"Create it"** / **"Change something"** / **"Cancel"**. Present this table first:

```
Display Name  : {name}
Directory     : Samples~/{dir}/
Doc file      : Documentation~/Samples/{kebab}.md
README entry  : - ### [{name}](Documentation~/Samples/{kebab}.md)
package.json  : { "displayName": "...", "description": "...", "path": "Samples~/..." }
Extra sections: {sections or "none"}
```

## File operations (only after "Create it")

Execute in order — see [REFERENCE.md](REFERENCE.md) for exact formats:

1. **`package.json`** — Read file, append new object to `"samples"` array, write back.
2. **`Samples~/{DirectoryName}/`** — Create directory with a `.gitkeep` placeholder file.
3. **`Documentation~/Samples/{kebab-name}.md`** — Write doc file using template in REFERENCE.md.
4. **`README.md`** — Read file, locate the last `- ###` line under `## Samples`, insert new entry after it.

After writing, confirm what was created and remind the user to add real assets to the `Samples~/` directory.
