# ADR 0003: Cross-Tool Skill Exposure

**Date:** 2026-06-30
**Status:** Accepted

## Context

[ADR 0002](0002-agentic-config-single-source.md) made `AGENTS.md` the single source of truth for agent *guidance* and deferred the question of whether skills should get cross-tool support. The repo ships skills in `.claude/skills/` (`new-sample`, `unity-tests`, `write-pr`). Each is a markdown workflow with YAML frontmatter.

Claude Code has a skill loader: it reads each skill's frontmatter `description` into the system prompt and lazy-loads the body when a trigger matches, exposing the skill as a slash command. Other agents (Cursor, Codex, Gemini CLI, Zed, Amp, and so on) have no skill-loading runtime. They only read context files such as `AGENTS.md`.

So the skill bodies are already plain markdown any agent can follow. The blocker is not format, it is **discovery and invocation**: a non-Claude agent has no way to know a skill exists or that it should follow it.

## Decision

Add a `## Skills` section to `AGENTS.md` containing a catalog table (`Skill | When | Follow`), one row per skill, pointing at its `.claude/skills/<name>/SKILL.md`. Because every supported agent already loads `AGENTS.md`, the catalog is in context every session with no extra trigger mechanism.

The section also carries a translation note: skill bodies may name Claude Code tools (e.g. `AskUserQuestion`); an agent lacking the named tool performs the plain-language equivalent. Skill bodies are **not** edited, so they stay tuned for Claude while remaining followable elsewhere.

For Claude the catalog is redundant with its loader; it exists to serve every other agent.

### Accepted trade-off: bounded drift

The `When` column duplicates intent that also lives in each skill's frontmatter `description`. This is the content drift ADR 0002 was built to prevent. We accept it here, bounded, because:

- The catalog gives non-Claude agents one-read discovery without a directory scan, which is more reliable for weaker agents.
- At three skills the duplication is one line each, and the `When` column is intentionally terser than the frontmatter triggers.
- The frontmatter remains the authoritative trigger source; the table is a pointer.

Drift is governed by a documented rule in `Documentation~/Contributing.md` ("add or rename a skill, update the table"), not by CI. A generated table or an existence-check guard would close the gap but adds tooling disproportionate to the current scale.

## Alternatives Considered

### A. Discovery instruction instead of a catalog
`AGENTS.md` tells agents to scan `.claude/skills/*/SKILL.md` frontmatter and follow any match. Zero drift and self-maintaining. Rejected: relies on weaker agents performing a directory scan every session; the catalog trades a small, bounded duplication for more reliable discovery.

### B. Generate the catalog in CI
A script reads each frontmatter and writes a marked region of `AGENTS.md`. Zero drift with a rich table. Rejected at this scale: reintroduces the build step ADR 0002 rejected and needs edit-region markers in a hand-edited file.

### C. Generate per-tool command files
Emit `.gemini/commands`, Cursor rules, and so on from each `SKILL.md`. True native invocation per tool. Rejected: a build step plus a drift window per tool, the exact pattern ADR 0002 alternative A rejected.

### D. Neutralize the skill bodies
Rewrite Claude tool references out of every body so all agents read identical prose. Rejected: more churn, and the translation note achieves cross-tool readability while letting bodies stay tuned for Claude.

## Consequences

### Positive
- Non-Claude agents discover and follow skills from the same `AGENTS.md` they already read.
- Skill bodies are untouched, so Claude's experience is unchanged.
- No build step, no per-tool files.

### Negative
- The `When` column can drift from frontmatter; contained only by a documented rule, not automation.
- Adding a skill requires a manual `AGENTS.md` table edit.

### Neutral
- Copilot and Windsurf remain unsupported, consistent with ADR 0002, since they cannot read an external `AGENTS.md`.

## Follow-Up Decisions Deferred

- Promote drift governance to a CI guard or generated table if the skill count grows past hand-maintenance.
