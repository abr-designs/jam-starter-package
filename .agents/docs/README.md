# Working Docs Index

File map for the repo's working/internal docs. Use this page to find what features are **planned**, **implemented**, or **decided**, and where each record lives.

## Layout

| Folder | Holds | Tracked |
|---|---|---|
| `adr/` | Architecture Decision Records: one accepted decision each (context, alternatives, consequences). | Yes |
| `plans/` | Feature plans in the arch-plan template: architecture diagrams, components, acceptance criteria, and a Status field. | Yes |
| `reviews/` | Generated code-review output (HTML + comment JSON) from the review skill. Local only, gitignored, not part of the record. | No |

## Architecture Decision Records (`adr/`)

Files named `NNNN-slug.md`. Status comes from each file's `Status:` line.

| ID | File | Status | Decision |
|---|---|---|---|
| 0001 | `adr/0001-dual-backend-tweening.md` | Accepted | Dual-backend tweening: keep the sync `TweenTo` / `TweenToCoroutine` engine, add an optional UniTask `TweenToAsync` engine in a gated asmdef, share curve math via `TweenMath`. Caller picks the backend at the call site. |
| 0002 | `adr/0002-agentic-config-single-source.md` | Accepted | `AGENTS.md` at the repo root is the single source of AI-agent guidance; `CLAUDE.md`, `.gemini`, and other tools point at it rather than duplicating content. |
| 0003 | `adr/0003-cross-tool-skill-exposure.md` | Accepted | A `## Skills` catalog table in `AGENTS.md` exposes the `.claude/skills/` workflows to non-Claude agents that have no skill loader. |

## Feature Plans (`plans/`)

Files named `NNNN-slug.md`. Status comes from each plan's Status field (Proposed / In Progress / Implemented).

| ID | File | Status | Feature |
|---|---|---|---|
| 0001 | `plans/0001-simple-path.md` | Implemented | Extract a reusable `SimplePath` base class out of `SimplePathFollow`, splitting path structure and sampling from motion. |
| 0002 | `plans/0002-async-tweening.md` | Implemented | UniTask-powered async tween backend (`TweenToAsync` family) beside the sync engine, gated behind the `UNITASK` define. Realises ADR 0001. |

## Conventions

- **Numbering:** zero-padded `NNNN`, monotonic within each folder, never reused.
- **Status source of truth:** a plan's own Status field, an ADR's own `Status:` line. The tables above mirror them.
- **Keep this current:** when you add, renumber, or change the status of an ADR or plan, update the matching row here.
