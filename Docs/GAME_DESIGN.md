# Game Design

## One-line pitch

Choose free start items, cycle the runner color, and clear a short authored
stage by matching every gate before crossing the Goal.

## Target

- Portrait mobile
- One-thumb input
- Approximately 25–40 seconds for a no-item prototype-stage clear
- Deterministic, immediately retryable runs

## Main flow (Step 9A)

1. Lobby shows the lowest unlocked uncleared stage and current visual tier.
2. Play opens PreRun Item Selection for that single current stage.
3. Shield and Booster are independent free toggles only when the selected
   stage allows them. Stages 1-5 keep both items locked.
4. Start enters one `3, 2, 1, GO` countdown.
5. Items activate after `GO`, then the runner moves automatically.
6. Taps cycle through the colors currently allowed by the stage.
7. A matching gate advances progress. A mismatch fails unless Shield absorbs
   it or Booster is active.
8. The final gate reveals a Goal. Crossing it clears the stage.
9. Clear waits for a finish animation, then Continue returns to the updated
   Lobby. Failure waits for its animation, then offers one Continue, Retry,
   and Lobby. Retry returns to item selection with toggles retained.

## Authoritative flow states

- StageSelect
- PreRunSelection
- Countdown
- Playing
- ShieldRecovery
- StageFinishing
- StageCleared
- Failed

Lobby is controller-level navigation outside a live StageSession. The
development stage picker is hidden from the normal flow.

## Continue

- One free Continue is available per attempt.
- It keeps stage, passed-gate progress, current color, and remaining Shield
  state.
- It never restores Shield or Booster and never opens item selection.
- It uses `3, 2, 1, GO`, one second of recovery protection, then two safe
  gates: current color and at most one tap.
- A continued clear unlocks the next stage and increments clear count, but
  cannot update Best or no-item Best.

UI visibility follows these states and does not own gameplay state.

## Five prototype stages

| Stage | Title | Gates | Colors | Speed | Cadence | Final |
|---|---|---:|---|---|---|---:|
| 1 | Two-Color Basics | 24 | Red, Blue | 7 → 10 | 1.55 → 1.25 s | 4 |
| 2 | Rhythm Contrast | 28 | Red, Blue | 8 → 11 | 1.45 → 1.10 s | 5 |
| 3 | Exception Color | 30 | Red, Blue | 8.5 → 11.5 | 1.40 → 1.00 s | 6 |
| 4 | Green Introduction | 32 | Red, Blue, Green | 8 → 12 | 1.45 → 1.00 s | 6 |
| 5 | Three-Color Challenge | 36 | Red, Blue, Green | 9 → 13.5 | 1.25 → 0.85 s | 8 |

Stage 4 keeps its first eight gates Red/Blue, then presents a deterministic
Green onboarding gate. Stages 1–3 never introduce Green. Stage 5 uses all
three colors and the strongest supported pattern mix.

The input cycle uses the stage's active colors in authored forward order.
Stage 4 activates Red, Blue, and Green from its initial countdown even though
Green first appears at gate index 8. Runtime input, HUD guidance, and
simulation required-tap calculation use the same modular forward-cycle rule.

## Start items

### Shield

- Free and unlimited at selection time.
- Activates only after `GO`.
- Absorbs one mismatching gate and is then removed.
- The absorbed gate advances stage progress but does not count as a correct
  judgment.
- Recovery lasts exactly one second before normal vulnerability resumes.

### Booster

- Free and unlimited at selection time.
- Activates only after `GO`.
- Uses stage-specific high speed from 22 through 26 units per second.
- Lasts a deterministic stage-specific distance.
- Gate crossings advance progress while accuracy judgments are bypassed.
- Shield is never consumed during Booster.
- At Booster end, speed eases for 0.35 seconds to the normal speed calculated
  from current stage progress.

## Gate and Goal rules

- Gate colors, patterns, spacing, and stage definitions use explicit seeds.
- No runtime gate instantiation occurs; six gate views recycle.
- No more than four consecutive gates share a color.
- Spacing derives from stage progress, speed, and authored cadence.
- The final gate enters StageFinishing and reveals the Goal ten units ahead.
- No failure may occur after StageFinishing begins.
- Goal crossing records exactly one clear.

## Persistence

Each stage stores cleared state, best time, best no-item time, and clear count.
The profile also stores highest unlocked stage. Better means lower time.
Booster or Shield clears may improve overall best but never overwrite the
no-item best. Corrupt values fall back to safe empty records.

## Track and camera

- Six pre-created 40-unit straight track segments recycle behind the player.
- Normal camera rotation and 60-degree FOV remain fixed.
- Booster alone temporarily uses 74-degree FOV, pooled speed lines, and trail.
- Step 9A Booster presentation adds a distance meter, final-20%
  warning, launch pulse/shake/haptic request, and two safe exit gates.
- No post-processing or normal-speed camera escalation is used.

## Legacy and deferred

The previous deterministic Endless implementation remains reusable code but is
not exposed by the main scene. Optional Endless mode, direct color buttons,
consumable economies, extra mechanics/colors, audio or BPM placement, ads,
analytics, networking, and online ranking are deferred.

## Step 9C player-facing presentation

Presentation mirrors the existing controller and Core states without owning
stage progress, color, item, Continue, or gate rules.

- Lobby, PreRun, gameplay HUD, Countdown, clear, failure, and development
  navigation have separate generated roots. Exactly one primary root is
  visible; Countdown overlays the gameplay HUD.
- The Lobby exposes only the current stage path. Its environment tier is
  derived from persisted clears and is not selectable.
- Gameplay color guidance is a two- or three-tile read-only cycle. The active
  stage definition and Stage 4 introduction progress determine which colors
  are available. Tap input and cycle behavior are unchanged.
- The top HUD owns stage progress plus relevant Shield and Booster status.
  Booster status is absent before activation and after its deterministic
  distance completes.
- UI, gate symbols, and edge-only Booster presentation improve recognition
  without changing stage balance, cadence, speed, item power, Continue,
  simulation, or sequence behavior.

## Simulation and telemetry

Core simulation uses the same stage/session/gate/item/Continue rules as play.
Perfect, Expert, Average, Novice, and Stress are configurable provisional
profiles. Reports estimate mechanical duration, input pressure, failures, and
item/Continue effects only. Local development telemetry may later calibrate
profiles; it is disabled by default, contains no identifiers, and is never
uploaded.

## Step 9A.1 continuity and historical movement overrides

This section supersedes only the conflicting Step 9A transition and movement
values above. All unrelated stage, item, input, persistence, and result rules
remain active.

- Continue overlays `3, 2, 1, GO` on the frozen failure scene. It preserves
  elapsed time, progress, color, sequence cursor, camera, tracks, and the live
  six-gate pool. The failed gate is resolved exactly once at GO; Shield and
  Booster are not restored.
- Booster exit never rebuilds or repositions the gate stream. The next two
  already-positioned unresolved gates receive temporary color-only safety:
  current color, then current or the next cycle color. The third gate returns
  to its original authored plan.
- Starting/maximum speeds for Stages 1–5 are respectively `14/20`, `16/22`,
  `17/23`, `16/24`, and `18/27`. Booster speeds are `44`, `46`, `48`, `50`,
  and `52`, with distances `160`, `180`, `200`, `210`, and `230`.
- The initial gate lead is 24 units and the Goal is 20 units after the final
  gate. Authored spacing already equals speed multiplied by cadence, so
  doubling speed doubles spatial scale while preserving the existing
  time-based decision cadence and approximate Booster duration.
- Simulation and local development telemetry preserve planned pattern/color
  metadata separately from temporary effective colors and report continuity
  metrics. Mechanical continuity does not establish perceptual smoothness,
  comfort, fairness, or fun.

## Iteration 2 active campaign speed-scale override

This section supersedes only the Step 9A.1 campaign movement and world-distance
values above. Continue, Booster continuity, cadence, input, persistence, and
result rules remain unchanged.

- Normal campaign speed is linear in passed-gate progress:
  `speed = start + (maximum - start) * progress`.
- Starting/maximum speeds for Stages 1–5 are respectively `28/40`, `32/44`,
  `34/46`, `32/48`, and `36/54` units per second.
- Fixed Booster speeds are `88`, `92`, `96`, `100`, and `104`, with travel
  distances `320`, `360`, `400`, `420`, and `460` units.
- Authored gate cadence remains `1.55→1.25`, `1.45→1.10`, `1.40→1.00`,
  `1.45→1.00`, and `1.25→0.85` seconds. Gate spacing remains speed multiplied
  by cadence, so spatial spacing doubles with speed.
- Campaign initial gate lead is 48 units and Goal distance is 40 units.
- Experiment Lab retains its separate movement values and 24-unit initial
  lead.
- The scale change preserves time-based decision cadence, approximate stage
  duration, and approximate Booster duration by construction. Human playtest
  must determine whether the new motion reads as faster and remains
  comfortable.

## Step 10 development experiment lab

The normal Lobby and Stages 1–5 remain unchanged. Editor and development
builds contain a separately controlled experiment catalog for three through
six colors crossed with None, Camouflage, Fog, and Ice.

- Tap-to-cycle remains the input. Experimental order is Red, Blue, Green,
  Yellow, Purple, Cyan, truncated to the selected capacity.
- The current-first vertical HUD stack shows every active color in forward tap
  order. Logical state changes immediately; a 0.12-second interrupt-and-retarget
  transition moves the fixed tiles without queue buildup.
- Camouflage keeps its geometry visible and neutral, then reveals color and
  symbol when exactly one unpassed gate remains before it.
- Fog keeps the nearest two unpassed gates fully readable. Farther experiment
  gates remain neutral silhouettes; plans and transforms do not change.
- Ice gates keep color judgment active. The experiment uses a 1.45 speed
  multiplier, 1.30 spacing multiplier, 0.30-second entry, and 0.35-second
  exit model across gates 12–19.
- Experiment sessions and reports never load or save normal stage progress.
  Launcher item toggles use the shared experiment item rules for manual tests;
  items remain outside the controlled first pass and run only for shortlisted
  conditions.
- The first-pass matrix contains 64,016 deterministic model runs. Its risk
  labels and shortlist are provisional mechanical evidence, not claims about
  fun, fairness, comfort, or replay motivation.

Flicker, Clone, combined mechanics, reverse cycling, swipe-back input, and
permanent four-through-six-color progression remain deferred.

## Iteration 3 Clone-only experiment contract

Clone is a development-only Experiment capability. It is not part of campaign
Stages 1–5 and does not introduce a new input.

- A Clone Gate is an additional real judgment gate placed immediately after
  one explicit Source Gate. Source and Clone are judged independently.
- Clone uses the Source Gate's target color. Passing the Source does not pass
  its Clone, and passing a Clone contributes to Experiment completion.
- Gate roles are `Standard`, `Source`, and `Clone`. Every generated judgment
  gate has a stable generated identity. A Clone records its Source identity;
  Standard and Source gates have no Source identity.
- The first Clone-only condition uses `SourceIndices [3, 6]`, where indices
  count non-Clone judgment gates from one. Exactly two Clones are generated.
  Source selection is explicit rather than random and is unchanged by seed.
- A definition with fewer than six non-Clone judgment gates is invalid. It
  does not substitute another Source.
- Clone gap is calculated as
  `Experiment base movement speed * 0.45 seconds`. It is not recalculated for
  Booster. Later gate positions shift forward in sequence, so Source and Clone
  never overlap and no following gap is shortened.
- A Clone cannot be a Goal, a Goal cannot be a Source, and a Clone cannot
  create another Clone. One Source owns at most one Clone.
- Clone-only Launcher runs disable Booster and use normal Experiment speed.
- A Clone mismatch uses the existing failure flow. Active Shield consumes its
  one shared protection, passes that Clone, and keeps the Experiment running.
  No Clone-specific Shield rule is introduced.
- Camouflage remains a visibility mechanic. Clone hides its color and symbol
  under the same condition as an ordinary Camouflage gate, reveals at the same
  distance, then uses normal judgment. Camouflage never exempts Clone from
  judgment and never suppresses failure.
- Retry and Replay preserve the definition, seed, Source relationships, Clone
  positions, and colors while resetting all judgment and visibility state.
- Clone failure is identified separately from an ordinary gate failure in
  runtime result information.

Clone color variation, multiple Clones per Source, Clone chains, probabilistic
Source selection, Flicker, Clone with Booster balancing, Clone with Fog or Ice
balancing, campaign use, Pattern/Section metadata, and a new Pattern Generator
remain deferred.

## Iteration 4 approved Echo and campaign-expansion contract

This section supersedes the Iteration 3 Clone-only gameplay contract. The
historical Clone record remains for traceability, but production and
development play no longer use an additional Clone Gate, Source/Clone
relationship, `0.45s` insertion gap, or `CLONE MISS`.

- Campaign stage authoring uses one `StageCatalogAsset`. A Unity adapter
  converts it into immutable pure-Core definitions consumed by runtime,
  simulation, and tests through `IStageCatalog`.
- `StageSpeedProfile` evaluates progress from 0–1 using one deterministic Core
  curve. `MaxSpeed` caps stage base speed before Booster or Ice modifiers.
- Gate modifiers share one Core representation for None, Camouflage, Fog,
  Ice, and Echo Provider. Flicker and a generic combination engine remain
  deferred.
- Echo Provider is a modifier on an ordinary gate and never increases gate
  count. An exact player-color match stores the gate's effective color in one
  non-stacking player Echo.
- Judgment priority is Player match, Echo match, Shield, then failure. Echo
  and Shield cannot be consumed by the same judgment.
- Echo cannot be acquired from Echo-assisted, Shielded, Continue-protected, or
  automatic success. Runtime offer state prevents a second visible provider
  while Echo is held or another offer is active.
- Camouflage reveal is based on deterministic ETA rather than gate count. A
  reveal never reverses, and visibility never exempts judgment.
- Stages 1-5 do not allow start-item selection. Stage 6 grants one stage-local
  Shield at start. Stage 7 grants one stage-local Booster at start. A provided
  item uses the same post-countdown activation timing as the equivalent
  player-selected item, cannot be selected twice, and does not consume or
  persist inventory state.
- Stages 8–11 focus respectively on Camouflage, Fog, Ice, and Echo. Existing
  flow, Continue, Retry, Goal, and stable stage IDs remain authoritative.
- Stage 6–11 color count, speed, curve, cadence, and mechanic density are
  selected from documented simulation candidates. Automated results cannot
  establish fun, readability, comfort, fairness, or satisfaction.

## Iteration Notes

Stage unlock policy Mechanics are introduced once.
After that they become optional.

---

Tap Budget
Maximum = 4
4 Tap Peak only <1%
Difficulty comes from Rhythm not randomness.

## Iteration 4 final campaign stage catalog

Base speed is evaluated as
`Start + (Max - Start) * Curve(normalized gate progress)`. Curve points below
are normalized `(progress, speed fraction)` values sampled once into pure Core.
Booster replaces base movement speed. Ice multiplies non-Booster movement by
`1.45` and authored spacing by `1.30`.

| Stage | Colors | Base speed curve | Primary mechanic |
|---:|---:|---|---|
| 1 | 2 | Linear `28 -> 40` | Two-color basics |
| 2 | 2 | Linear `32 -> 44` | Rhythm contrast |
| 3 | 2 | Linear `34 -> 46` | Exception color |
| 4 | 3 | Linear `32 -> 48` | Green introduction |
| 5 | 3 | Linear `36 -> 54` | Three-color challenge |
| 6 | 2 | `38 -> 56`; `(0,.0) (.35,.22) (.70,.62) (1,1)` | Local Shield at start |
| 7 | 2 | `40 -> 58`; `(0,.0) (.30,.18) (.65,.55) (1,1)` | Local Booster at start |
| 8 | 2 | `40 -> 60`; `(0,.0) (.40,.25) (.75,.70) (1,1)` | Camouflage |
| 9 | 2 | Linear `42 -> 62` | Fog |
| 10 | 2 | `44 -> 64`; `(0,.0) (.50,.35) (.80,.75) (1,1)` | Ice |
| 11 | 3 | `44 -> 66`; `(0,.0) (.45,.30) (.75,.68) (1,1)` | Echo |

Stages 6-10 intentionally return to two colors so human evaluation can focus
on one new mechanic. Stage 11 restores three colors. The final simulation
improved Average/None first-clear rates for Stages 8-10 from
`21.2/20.0/18.5%` to `31.2/30.5/27.9%`; these figures are mechanical evidence,
not a claim of fun, fairness, or comfort.
