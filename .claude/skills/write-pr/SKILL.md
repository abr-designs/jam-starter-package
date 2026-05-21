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

4. **Verify draft against actual code** — before presenting, read every changed file identified in step 1 and check each concrete claim in the draft (method names, lifecycle hooks, field names, behaviour descriptions) against the current source. Correct any discrepancy silently. Supporting docs (plan files, CHANGELOG) may be older than the code — treat the code as the source of truth.

5. **Present draft** — show the full markdown, then ask:
   - ⭐ "Looks good — copy to clipboard" (Recommended)
   - "Edit something"
   - "Regenerate"

6. **Output** — print the final markdown in a fenced block so the user can copy-paste into GitHub.

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
- Single issue: `- Closes #N` (bullet format)
- Multiple: one `- Closes #N` bullet per issue

### Description
- Explain what the feature does, how it's used, and why it's useful — user-facing value only
- **Do NOT** summarise which files changed or what methods were added — that belongs in Tech Notes
- Present tense, no filler ("This PR adds…" not "This PR was created to add…")
- For simple PRs: 1–2 sentences is enough
- For complex/multi-part features: use `###` sub-headers per concept, with bullets under each
  - See PR #104 (SimplePathFollow) as the reference for a well-structured feature Description
- "The goal of this PR is to…" is a valid opening for refactors where intent needs stating

### Tech Notes
- **Optional** — only include when there's a genuine limitation, workaround, or non-obvious implementation detail
- Put critical must-know warnings in a `> [!IMPORTANT]` callout **at the top**, before sub-headers
- Put non-obvious tips (especially with links to docs) in `> [!TIP]` callouts inline under the relevant sub-header
- When multiple files are changed, organise by file using `###` sub-headers (e.g. `### \`SimplePath.cs\``)
- Use a `### Misc` sub-section for cross-cutting changes that don't belong to a single file
- Bullet list of specific changes; use nested sub-bullets for related details
- Use `&` instead of `and` in bullet text

### Tests (sub-header of Tech Notes)
When tests are added or changed, include a `### Tests` sub-header inside Tech Notes:
- Assembly header format: `**EditMode Tests** for \`FileName.cs\`` / `**PlayMode Tests** for \`FileName.cs\``
  - Not `**EditMode — \`FileName.cs\`**`
- Infrastructure notes (test helpers, reflection setup) go as an inline bullet — not a preamble paragraph before the list
- Group related test cases into one bullet — do not split one-bullet-per-test-method
- Lead each bullet with "Checks that" or "Ensures that"
- State the behaviour being verified — omit internal details (return values, numeric thresholds, assertion mechanics)
- Fewer, broader bullets over one-bullet-per-test-case

### Type of Change
- Leave checkboxes unchecked (user fills in on GitHub)
- Always keep the full list — do not remove inapplicable types

### Formatting rules
- Backticks for: filenames (include `.cs` extension), class names, method names, enum values, inspector field names
- `###` sub-headers for sections within Description or Tech Notes
- No trailing punctuation on bullet items unless they form full sentences
