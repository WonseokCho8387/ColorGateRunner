# Color Gate Runner — Agent Instructions

## 1. Purpose

Build a portrait mobile hyper-casual game that is deterministic, testable,
reproducible, and safe to evolve through small approved iterations.

This file contains the **common Codex workflow**. Iteration prompts should only
describe the task-specific delta, decisions, exclusions, and commit message.
Do not repeat the common baseline, regression suite, Git policy, or report
format in every prompt.

## 2. Source of Truth

Read the following before proposing or implementing changes:

1. `Docs/PROJECT_CHARTER.md` — project mission and hard boundaries
2. `Docs/CURRENT_STATUS.md` — latest implementation state and authoritative baseline
3. `Docs/GAME_DESIGN.md` — gameplay rules
4. `Docs/ART_DIRECTION.md` — visual and portrait/mobile rules
5. `Docs/TEST_PLAN.md` — standard regression suite and feature acceptance history
6. `Docs/DECISIONS.md` — approved architectural and product decisions
7. `Docs/FRONTEND_FLOW.md` — Scene, Page, Overlay, and navigation contracts
8. `Docs/PRODUCT_SYSTEMS.md` — AppRoot, Profile, Save, Settings, and service boundaries
9. `Docs/LOOP.md` — core iteration/gameplay loop when applicable
10. `Docs/ITERATIONS.md` — prior iteration learning and deferred work

Authority by topic:

- Current test counts, hashes, Build Settings, and approved state:
  `CURRENT_STATUS.md`
- What must be tested and when: `TEST_PLAN.md`
- Current design contract: the relevant domain document
- Historical rationale: `DECISIONS.md` and `ITERATIONS.md`
- Task-specific scope: the latest user-approved iteration prompt

If these sources conflict, do not guess. Report the conflict and stop before
modifying files.

## 3. Standard Iteration Protocol

### Phase A — Inspect and propose

1. Run `Tools/QualityGraph/Invoke-QualityPreflight.ps1 -Mode inspect` and
   record the observed Editor, project-copy, backend, revision, and dirty state.
2. Read all required documents.
3. Run the baseline checks defined in `CURRENT_STATUS.md` and
   `TEST_PLAN.md`.
4. Inspect the actual code, Scene, Builder, persistence, and test boundaries
   relevant to the request.
5. Identify the smallest safe implementation.
6. Report:
   - baseline result;
   - current architecture;
   - proposed design and ownership;
   - expected new and modified files;
   - risks and unresolved decisions;
   - explicit non-goals;
   - feature-specific tests.
7. Stop and wait for user approval.

Do not modify code, Scene assets, ProjectSettings, packages, or documentation in
Phase A unless the user explicitly requests a documentation-only iteration.

### Phase B — Implement after approval

1. Reconfirm that the working tree and Quality Graph environment state still
   match the approved Phase A state.
2. Implement the smallest approved change.
3. Add or update feature-specific tests.
4. Run the applicable Standard Regression Suite from `TEST_PLAN.md`.
5. Compare final results with the Authoritative Baseline in
   `CURRENT_STATUS.md`.
6. Update documentation only after implementation and validation agree.
7. Perform the Git safety procedure in this file.
8. Commit only when all approved completion conditions pass.
9. Do not push.

## 4. Baseline Policy

- `CURRENT_STATUS.md` is the sole authoritative location for the latest:
  - approved HEAD;
  - EditMode and PlayMode counts;
  - Builder expectations;
  - Build Settings order;
  - Campaign and Step 10 artifact hashes;
  - persistence snapshots;
  - Package and ProjectSettings baselines.
- Do not copy those changing values into routine iteration prompts.
- If the current repository does not match the documented baseline, stop and
  report the exact difference before implementation.
- Do not silently redefine a changed baseline.
- `APP_UI_EDITOR_ONLY` and `SENTIS_ANALYTICS_ENABLED` are package-managed
  WebGL scripting defines. Their environment-dependent presence, absence,
  combination, or ordering is an allowed volatile difference, but they must
  never be staged in a feature commit.
- Any other scripting define or ProjectSettings semantic change remains a
  baseline failure, including resolution, WebGL template, Input Actions,
  preloaded assets, PlayerSettings, Graphics, Quality, or Android settings.
- Tests must not destroy or rewrite the user's Editor Campaign PlayerPrefs,
  Product save, Guest identity, or other local development data.
- Campaign-affecting PlayMode fixtures must use the documented snapshot and
  exact-restore policy.

## 5. Standard Validation Policy

Apply `Docs/TEST_PLAN.md` according to the change type.

Always:

- compile successfully;
- run relevant targeted tests;
- run the full required EditMode and PlayMode suites;
- run `git diff --check`;
- check Missing Script and Missing Reference;
- verify that unrelated packages and ProjectSettings did not change.

When a Scene or Builder changes:

- run each affected Builder twice;
- run post-Builder PlayMode tests;
- verify unique generated roots, required references, EventSystem count, and
  Build Settings order.

When gameplay, deterministic content, stage data, timing, balance, generation,
or judgment changes:

- run the documented Campaign and Step 10 simulations;
- compare every required artifact hash;
- run all applicable profiles and start-item combinations;
- change only values explicitly approved as balance-tunable.

When Product save, Profile, Settings, or account state changes:

- verify save migration, validation, recovery, roundtrip, Guest ID stability,
  and failure reporting according to `PRODUCT_SYSTEMS.md` and `TEST_PLAN.md`.

Automated evidence must never be described as proof of fun, fairness,
readability, satisfaction, motivation, or visual quality. Report those as human
review items.

## 5.1 Quality Graph and Environment Contract

The versioned generic Skill source is
`Tools/AgentSkills/quality-graph`; the Color Gate Runner adapter is
`Tools/QualityGraph/quality-graph.json`. Use the adapter before any Unity
command whose correctness depends on the Editor state.

Operating modes:

- `inspect`: read-only investigation; the interactive Editor may be open.
- `source-edit`: ordinary C# and external tooling changes may proceed while
  Unity is open. Scene, Package, and ProjectSettings paths require Unity closed.
- `batch-validate`: the primary Editor and its project lock must be absent.
- `visual-qc`: use a graphics-capable Editor or supplied Player artifact;
  Headless output is not visual evidence.
- `finalize`: save intended assets, exit Play Mode, close Unity, and verify the
  final disk and Git state.

Quality Graph routing preserves the existing iteration protocol:

- Routine documentation or isolated pure-code work uses one maker and the
  deterministic tier selected by `TEST_PLAN.md`.
- Integrated behavior, shared controllers, persistence, generation, or
  cross-feature work adds one fresh-context checker after deterministic gates.
- Scene, shader, material, camera, UI, animation, device, or release-facing
  work adds black-box or rendered-artifact QC and retains a human gate.

Only the maker may modify shared project files. A checker receives the approved
acceptance criteria, diff and evidence, but not the maker's reasoning or full
conversation. It reports evidence and does not silently repair. Default to one
checker, one targeted repair, and no parallel writers. A second failed check,
conflicting evidence, or missing product authority returns to the user.

Every final report identifies the project copy, Unity state, graphics backend,
revision and checks actually observed. Record token usage only when the runtime
reports it; never estimate it. Headless validation never certifies rendered
pixels, visual quality, readability, feel or device behavior.

## 6. Architecture and Coding Rules

- Use Unity C#.
- Keep gameplay rules separate from presentation.
- Prefer plain C# that can be tested without loading a Scene.
- Keep `ColorGateRunner.Product` Unity-independent.
- Avoid service-specific singletons and mutable global service locators.
- Use serialized private fields instead of public mutable fields.
- All random gameplay must accept an explicit seed.
- Do not use `FindObjectOfType` in normal runtime flow.
- Avoid per-frame allocations and LINQ in `Update`, `FixedUpdate`, or other
  per-frame gameplay paths.
- Reuse existing state machines, Retry paths, deterministic generators,
  service boundaries, and Builders instead of creating parallel systems.
- Runtime Scene destinations must use serialized references or approved
  adapters; do not hardcode Scene names or Build indices.
- Do not hand-edit Builder-owned generated structures when the Builder is the
  authority. Change the Builder and regenerate them.
- Do not add packages or change ProjectSettings unless explicitly approved in
  the iteration scope.
- Do not introduce ads, analytics, IAP, networking, external login, cloud,
  economy, or remote configuration unless explicitly approved.

## 7. Scope Discipline

- Implement only the approved iteration scope.
- Do not solve deferred architecture while fixing a local issue.
- Do not migrate persistence, rename keys, reset data, rebalance stages, or
  redesign adjacent UI unless explicitly approved.
- Preserve existing behavior outside the requested change.
- When the requested behavior depends on an unresolved product decision,
  present the alternatives in Phase A rather than selecting one silently.
- A placeholder must not pretend that an unavailable function succeeded.
  Hide unavailable release actions or display a truthful unavailable state.

## 8. Documentation Rules

Treat `Docs` as the project source of truth and avoid duplication.

After successful implementation:

- `CURRENT_STATUS.md` — update the Authoritative Baseline and latest result;
- `ITERATIONS.md` — record Play, Analyze, Design, Implementation, Validation,
  Learning, Deferred, and Human Review;
- `DECISIONS.md` — add only decisions that were actually approved and made;
- `TEST_PLAN.md` — add feature-specific acceptance and final evidence;
- domain documents — update only when their contract or implementation status
  changed.

Do not:

- pre-mark an implementation as complete before validation;
- copy the same contract into several documents;
- overwrite historical iteration evidence;
- create a new document when an existing authority already owns the topic.

## 9. Git Safety

Before work and before commit:

```text
git status --short
git diff --check
```

Before staging:

```text
git diff --stat
git diff
```

Stage only explicit files from the approved iteration:

```text
git add -- <explicit file list>
```

Then inspect:

```text
git diff --cached --check
git diff --cached --stat
git diff --cached
```

Forbidden:

```text
git add .
git add -A
git commit -am
git commit --no-verify
```

Never stage or commit unrelated changes, `Library`, `Temp`, `Logs`, `obj`, test
builds, local save files, PlayerPrefs dumps, generated simulation output that
replaces an approved baseline, or unapproved Package/ProjectSettings changes.

If an incorrect file is staged:

```text
git restore --staged -- <file>
```

After commit:

```text
git status --short
git show --stat --oneline --decorate HEAD
```

Do not push. Do not bypass hooks or validation failures. Report the failure and
current staged state.

## 10. Standard Final Report

Use this concise structure:

1. **Baseline** — HEAD, working tree, authoritative tests/hashes/snapshots
2. **Analysis / Architecture** — important ownership and design decisions
3. **Implementation** — changed files and responsibilities
4. **Validation** — targeted tests, full suites, Builders, simulations, data safety
5. **Documentation** — files updated and why
6. **Human Review** — checks automation cannot decide
7. **Deferred / Risks** — intentionally excluded work and remaining risk
8. **Git** — commit hash/message, clean state, push status

Do not repeat every historical test name when the full suite passed. Report the
new tests and summarize the standard suite by reference to `TEST_PLAN.md`.

## 11. Compact Iteration Prompt Contract

A normal future prompt only needs:

```text
Color Gate Runner의 [Iteration name]을 진행한다.

AGENTS.md의 Standard Iteration Protocol을 따른다.
CURRENT_STATUS.md의 Authoritative Baseline을 사용한다.
TEST_PLAN.md의 Standard Regression Suite를 적용한다.

## Goal
- task-specific outcomes

## Required decisions
- task-specific behavior not already defined in Docs

## Scope
- approved implementation boundaries

## Excluded
- explicit non-goals that are easy to confuse with this task

## Phase A focus
- unknowns that must be inspected before implementation

Phase A 보고 후 중단하고 승인받는다.
승인 후 검증 완료 시 문서를 갱신하고 지정된 메시지로 커밋한다.
Push하지 않는다.
```

Prompts should not repeat common Git commands, full regression lists, current
test counts, artifact hashes, standard report headings, or general coding rules
already owned by this file and the Docs.

## 12. Definition of Done

A task is complete only when:

- the approved behavior is implemented;
- Unity compiles without errors;
- targeted and standard regression tests pass;
- relevant deterministic artifacts match or an approved new baseline is
  documented;
- no Missing Script, Missing Reference, duplicate generated root, or data-loss
  regression is introduced;
- Package and ProjectSettings changes are within the approved scope;
- documentation matches the implementation;
- human review items are clearly separated from automated claims;
- the approved files are committed, the working tree is clean, and nothing is
  pushed.

## 13. AI Collaboration

Optimize the development process as well as the code. Propose workflow,
testability, documentation, or architecture improvements when they reduce
future risk or repeated work, but keep those proposals separate from the
approved implementation scope.
