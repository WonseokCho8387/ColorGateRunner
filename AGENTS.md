# Color Gate Runner — Agent Instructions

## Mission

Build a small portrait mobile hyper-casual game that is easy to test,
reproduce, and automate.

Before changing code, read:

1. `Docs/GAME_DESIGN.md`
2. `Docs/ART_DIRECTION.md`
3. `Docs/TEST_PLAN.md`
4. `Docs/DECISIONS.md`

## Working rules

- Use Unity C#.
- Keep gameplay logic separate from presentation.
- Prefer plain C# logic that can be tested without loading a scene.
- Do not add packages without explaining the need.
- Do not change ProjectSettings unless required.
- Do not edit generated files.
- Do not introduce advertisements, analytics, IAP, networking, or login.
- Avoid singleton-heavy architecture.
- Use serialized private fields rather than public mutable fields.
- All random gameplay must accept an explicit seed.
- Do not use `FindObjectOfType` during normal gameplay.
- Avoid per-frame allocations.
- Do not use LINQ in Update or other per-frame gameplay paths.

## Required workflow

For every gameplay change:

1. Explain the intended change.
2. Identify affected files.
3. Implement the smallest working change.
4. Add or update tests.
5. Run EditMode tests.
6. Run PlayMode tests when scene behavior changes.
7. Report test results and remaining risks.

For every gameplay-affecting change:

- Run and compare baseline and final deterministic/stochastic simulations.
- Run every documented player profile and start-item combination.
- Adjust only values explicitly listed as balance-tunable in
  `Docs/TEST_PLAN.md`.
- Never claim fun, excitement, fairness, satisfaction, or motivation from
  automated results. Detailed simulation requirements live in
  `Docs/TEST_PLAN.md`.

## Definition of done

A task is complete only when:

- Unity compiles without errors.
- Relevant automated tests pass.
- Existing tests continue to pass.
- No Missing Script or Missing Reference is introduced.
- The behavior matches `Docs/GAME_DESIGN.md`.
- User-facing changes are summarized.
