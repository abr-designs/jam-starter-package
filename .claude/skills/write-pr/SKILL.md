---
name: write-pr
description: Drafts a Pull Request body for the jam-starter-package repo following the project PR template. Reads git diff and commit log, infers issue numbers from the branch name, then writes Issue / Description / Tech Notes / Type of Change sections in the established house style. Use when user wants to write, draft, or create a PR description, or invokes /write-pr.
---

# Write PR

## Process

1. **Gather context** (run in parallel):
   - `git log main..HEAD --oneline` — summarize commits
   - `git diff main..HEAD --stat` — files changed
   - `git branch --show-current` — infer issue number(s) from branch name (pattern: `develop/<issue>-slug` or `develop/agents/<slug>`)
   - Read `CHANGELOG.md` — pull entries added on this branch for Description fodder

2. **Ask for any missing info** (AskUserQuestion, one question at a time):
   - Issue number(s) if not inferrable from branch
   - Type of Change if ambiguous (Bug Fix / New Feature / New Sample / Breaking Change)
   - Any known limitations or workarounds worth calling out in Tech Notes

3. **Draft the PR body** following the template and house style (see below).

4. **Present draft** — show the full markdown, then ask:
   - ⭐ "Looks good — copy to clipboard" (Recommended)
   - "Edit something"
   - "Regenerate"

5. **Output** — print the final markdown in a fenced block so the user can copy-paste into GitHub.

---

## Template

```markdown
## Issue
Closes #N

## Description
[High-level summary. What does this PR accomplish? Why does it matter?]

## Tech Notes
[Technical detail: files added/changed, class/method names, any limitations or workarounds.]

## Screenshots
<!-- skipped -->

## Type of Change
- [ ] Bug Fix
- [ ] New Feature
- [ ] New Sample
- [ ] Breaking Change

## Checklist
- [ ] Branch was updated from `develop/`
    - [ ] All conflicts have been resolved
- [ ] `CHANGELOG.md` was updated
- [ ] All Tests Pass
- [ ] Changes do not break
- [ ] Builds Successfully
```

---

## House Style

### Issue
- Single issue: `Closes #N`
- Multiple: bullet list, one `Closes #N` per line

### Description
- Present tense, no filler ("This PR adds…" not "This PR was created to add…")
- For multi-feature PRs: bold sub-header per feature (`**Feature Name:**`), then 2–3 sentences
- Focus on user-facing behaviour and intent, not implementation

### Tech Notes
- Bullet list of specific changes: file names, class names, method names — all in backticks
- Call out limitations and workarounds explicitly (e.g. "Calling `BakeArcLengthTable()` is required after runtime handle edits")
- Omit if there's nothing technically non-obvious

### Type of Change
- Leave checkboxes unchecked (user fills in on GitHub)
- List only the types that apply — drop inapplicable ones from the output

### Formatting rules
- Backticks for: filenames, class names, method names, enum values, inspector field names
- Bold for sub-section headers inside Description
- No trailing punctuation on bullet items unless they form full sentences
