# Operating Model

Use the smallest graph that can expose a demonstrated failure mode.

## Node responsibilities

1. **Environment preflight** records the project revision, working state,
   stateful applications, execution backend, and whether the requested mode is
   safe.
2. **Acceptance compiler** converts the approved user outcome into observable
   checks. It uses user-facing language rather than implementation names.
3. **Maker** is the only node allowed to modify shared project files.
4. **Deterministic gates** run existing compilers, tests, builders, linters,
   simulations, and state-preservation checks.
5. **Independent checker** inspects the resulting diff and black-box evidence
   from a fresh context. It reports pass, fail, or insufficient evidence.
6. **Evidence judge** reconciles deterministic and checker results. It never
   upgrades missing evidence to a pass.
7. **Human gate** owns product intent, feel, readability, visual taste, and any
   approval reserved by the project.

## Compact task packet

Pass only:

- objective and explicit non-goals;
- observable acceptance criteria;
- revision and environment manifest;
- changed-file list or diff reference;
- commands or artifacts the checker may inspect;
- protected state and authorization boundaries;
- required output schema.

Do not copy long histories or the maker's reasoning. Link authoritative files
and include only the relevant sections when the checker cannot read them.

## Checker output

Require:

- `verdict`: pass, fail, or insufficient-evidence;
- findings ordered by user impact;
- exact evidence for every finding;
- checks performed and checks omitted;
- environment and revision observed;
- human-review items;
- token usage only when reported by the runtime.

## Repair edge

Return only actionable failed criteria and evidence to the maker. Preserve
passing evidence and do not rerun unaffected gates. Allow one targeted repair
by default. Escalate after a repeated failure, disagreement over product
intent, or any request for broader authority.

## Test the test system

Use isolated mutation fixtures when a validator is important enough to gate
completion. Seed representative failures such as a missing reference, reversed
screen direction, stale generated content, or an omitted startup object. The
validator must reject the mutations without changing the real workspace.

## Metrics

Compare accepted changes using:

- defects caught before human play;
- defects escaping to human play;
- first-pass acceptance;
- repair rounds;
- elapsed time;
- token input/output when available;
- false passes and false failures from mutation fixtures.

Do not claim a quality improvement until representative runs outperform the
project's prior workflow on these measures.
