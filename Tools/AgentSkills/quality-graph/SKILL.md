---
name: quality-graph
description: Design and run risk-routed maker/checker development workflows with explicit environment state, independent QC, evidence gates, and bounded token use. Use when a user asks for loop engineering, graph engineering, independent AI verification, QC orchestration, or a reusable cross-project quality workflow. Do not invoke for an ordinary coding task that needs only the project's existing tests.
---

# Quality Graph

Build on the project's existing workflow instead of replacing it. Treat each
existing implementation loop as one node and add only the independent checks
that address demonstrated risk.

## Start with the project contract

1. Read the repository instructions and its current source-of-truth documents.
2. Locate an existing quality-graph configuration or adapter before inventing
   commands.
3. Run an environment preflight before any tool that can conflict with an open
   editor, build process, database, server, or other stateful application.
4. If the repository requires a proposal and approval phase, finish that phase
   before changing files.

For a repository without an adapter, read
[references/project-adapter.md](references/project-adapter.md) and propose the
smallest useful one. For Unity work, also read
[references/unity.md](references/unity.md).

## Route by risk

- **Routine:** one maker plus deterministic project checks.
- **Integrated:** one maker, deterministic checks, then one fresh-context
  checker for behavior or cross-boundary risk.
- **Perceptual or release-facing:** add black-box or rendered-artifact QC and a
  human decision gate.

Do not spawn multiple agents merely because the graph supports them. Use a
checker only when independence can catch a real class of failure. Keep one
writer for shared files; checkers report evidence and do not silently repair.

## Preserve checker independence

Give a checker the approved acceptance contract, changed artifacts, executable
evidence, and relevant project rules. Do not give it the maker's private
reasoning, intended answer, suspected defect, or full conversation history.
Prefer a fresh context. A different model can reduce correlated blind spots,
but is optional and never substitutes for different evidence.

## Require evidence

A pass must identify the exact environment, revision, checks, and artifacts it
observed. Separate deterministic, behavioral, visual, and human evidence. A
headless pass never certifies rendered pixels, device behavior, feel,
readability, or visual quality.

Read [references/operating-model.md](references/operating-model.md) when
designing or executing the graph, including task packets, evidence merging,
repair routing, metrics, and stopping conditions.

## Bound cost and retries

Default to one maker and at most one checker, run sequentially. Give each node
only its task packet and artifact references. Prefer deterministic scripts
before model judgment. Retry one targeted repair after a checker failure; on a
second failure, conflicting evidence, or missing authority, stop and escalate
to the user. Record token usage when the runtime exposes it, but never guess.
