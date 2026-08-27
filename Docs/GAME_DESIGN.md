# Game Design

## One-line pitch

Choose owned start items, cycle the runner color, and clear a short authored
stage by matching every gate before crossing the Goal.

## Target

- Portrait mobile
- One-thumb input
- Approximately 25–40 seconds for a no-item prototype-stage clear
- Deterministic, immediately retryable runs

## Main flow (Step 9A)

1. Lobby shows the lowest unlocked uncleared stage and current visual tier.
2. Play opens PreRun Item Selection for that single current stage.
3. Shield and Booster are independent owned-item toggles only when the selected
   stage allows them and inventory is available. Stages 1–7 keep selection
   locked; Stage 6 provides one free Shield, Stage 7 provides one free Booster,
   and Stage 8 unlocks both consumable toggles.
4. Start enters one `3, 2, 1, GO` countdown.
5. Items activate after `GO`, then the runner moves automatically.
6. Taps cycle through the colors currently allowed by the stage.
7. A matching gate advances progress. A mismatch fails unless Shield absorbs
   it or Booster is active.
8. The final gate reveals a Goal. Crossing it clears the stage.
9. Clear waits for a finish animation, shows the first-clear base reward and
   any separate Lobby milestone reward, then offers `NEXT STAGE` directly to
   the next PreRun selection or `LOBBY`. Campaign Replay is not offered.
   Failure waits for its animation, then offers eligible Ticket, rewarded-ad,
   and Coin Continue sources, Retry, and Lobby. Retry returns to item selection
   with toggles retained.

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

- Continues have no per-attempt count limit. A finite Stage still ends after
  its remaining authored gates are cleared.
- Successful Coin Continues cost `900`, `1,900`, `2,900`, then `4,900` Coins
  by successful Coin-Continue ordinal. Every later Coin Continue in the same
  attempt remains `4,900` Coins.
- One completed rewarded-ad Continue may be used per attempt when a real
  provider is available. Coin use preserves the ad right; ad-first preserves
  the first `900`-Coin price. Failed, cancelled or unavailable ads do not use
  the right.
- Coin authorization is saved atomically before gameplay resumes. Insufficient
  funds or save failure keeps the failure scene frozen and spends nothing.
- Retry is a new attempt and resets Continue count, Coin-price ordinal and ad
  right. Duplicate or stale ad callbacks cannot resume or charge another
  attempt.
- It keeps stage, passed-gate progress, current color, and remaining Shield
  state.
- It never restores Shield or Booster and never opens item selection.
- It shows `READY` for `0.5s` while the restored world remains frozen, resumes
  gameplay when `GO` appears, and removes the non-blocking GO flash after
  `0.25s`. One second of recovery protection and the next two safe gates remain
  unchanged: current color and at most one tap.
- A continued clear unlocks the next stage and increments clear count, but
  cannot update Best or no-item Best.
- The clear-result `CONTINUE` action is navigation and is not a paid/ad
  failure Continue.

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

Campaign selection is locked through Stage 7. Provided training items at
Stages 6 and 7 activate after `GO` without inventory consumption or a
duplicate selection. Stage 8 is the first clean application Stage where both
selection toggles are available. A successful `START` atomically consumes one
owned unit of each selected item before Countdown. At zero stock, an allowed
item offers one Shield or Booster for `900` Coins. Purchase success grants and
auto-selects exactly one item; insufficient Coins or save failure leaves the
player in PreRun with no partial spend or grant. Insufficient current inventory
at `START` or its save failure follows the same atomic failure rule. Retry is a
new attempt and consumes selected items again; Back before Start and duplicate
Start do not. Without a ready Product session, selectable items and quick buy
remain unavailable, while a no-item start and the Stage 6/7 provided-item
starts remain valid.

### Shield

- Costs one owned Shield when selected for a successful Stage 8+ Start.
- Activates only after `GO`.
- Absorbs one mismatching gate and is then removed.
- The absorbed gate advances stage progress but does not count as a correct
  judgment.
- Recovery lasts exactly one second before normal vulnerability resumes.

### Booster

- Costs one owned Booster when selected for a successful Stage 8+ Start.
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

- Campaign progress is one scalar distance evaluated on a Builder-owned Spline.
  Runner, camera, pooled gates, Goal, Fog curtain and Ice runway all consume
  that same distance-to-pose contract; world Z is not Campaign authority.
- The Campaign road is generated for the full route at attempt setup. It is
  seven units wide and has a 20-unit rear extension, so it cannot expose a gap
  before the runner or disappear before the Goal.
- The route uses broad horizontal turns and gentle elevation only. The camera
  retains world-up with no banking, roll, loop or inversion. Its path rotation
  follows with a `0.2s` half-life, capped at `12°` yaw and `8°` pitch lag, then
  snaps on Stage start, Retry and Continue and freezes during Pause/failure.
  Normal chase uses
  the existing `(0, 7, -8.2)` / `18°` local path-relative pose and fixed
  60-degree FOV.
- Booster blends closer/lower to `(0, 5, -6.5)` with `13°` pitch, temporarily
  uses 74-degree FOV, world-space Warp particles and the existing trail.
- Step 9A Booster presentation adds a distance meter, final-20%
  warning, launch pulse/shake/haptic request, and two safe exit gates.
- The legacy six-segment 40-unit straight pool remains an Experiment-only
  compatibility path and is inactive during Campaign play.

## Legacy and deferred

The previous deterministic Endless implementation remains reusable code but is
not exposed by the main scene. Optional Endless mode, direct color buttons,
Shop/bundle purchasing beyond the one-item Coin quick buy, extra mechanics or
colors, audio or BPM placement, ads, analytics, networking, and online ranking
are deferred.

## Campaign Spline and Race roadmap

- Iteration 37 already completed the isolated horizontal `SPLINE TRACK LAB` and
  remains a development-only comparison entry.
- Iteration 40 makes Spline distance the sole active Campaign spatial contract
  for all 20 authored Stages. Existing stage distances, speeds, gate plans,
  judgment, Continue and economy remain unchanged; only their world poses are
  sampled from the route.
- Future Race Mode may derive three default and at most five parallel routes
  from one center Spline. Initial opponents are non-colliding Ghost AI with
  independent scalar progress and result UI. Physics interference, economy or
  rank rewards and online play are not part of that first race slice.

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

- **Superseded only in timing by Iteration 41:** Continue formerly overlaid
  `3, 2, 1, GO` on the frozen failure scene. It still preserves
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
  and Shield cannot be consumed by the same judgment. A consumed Shield or
  Echo emits one short presentation-only pulse in its own field color; this
  does not extend protection or alter judgment timing.
- Echo cannot be acquired from Echo-assisted, Shielded, Continue-protected, or
  automatic success. Runtime offer state prevents a second visible provider
  while Echo is held or another offer is active.
- Iteration 35 adds attempt-level normal-Shield exclusion. If an attempt begins
  with selected Shield or a stage-provided Shield grant, no Echo Provider is
  generated for that entire attempt in Campaign or Experiment. Consuming the
  Shield or using Continue does not reopen offers; Retry with the retained
  Shield selection remains excluded. Starting without normal Shield preserves
  the existing deterministic Echo slots, and a held Echo is not a normal
  Shield for this rule.
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

## Iteration 6 Hidden-only Experiment contract (originally named Flicker)

Hidden is a modifier on an ordinary Experiment judgment gate. It is not a
separate gate type and does not alter the target color, transform, sequence,
completion requirement, or judgment rules.

- A Hidden gate begins fully readable with its target color and symbol.
- The target remains readable for at least `RevealDurationSeconds`.
- Hide begins once, and only when both the minimum readable time has elapsed
  and the shared effective-speed ETA is at or below `HideLeadTimeSeconds`.
- Camouflage and Hidden use ETA in opposite difficulty directions. A shorter
  Camouflage reveal-to-arrival window is harder, while a larger Hidden hide
  lead creates a longer memory interval and is harder. Level authors must
  evaluate these as separate tuning axes rather than applying one shared
  "shorter is harder" rule.
- The hide transition removes only target color and symbol information over
  `TransitionSeconds`. A neutral gate silhouette, the judgment opening, and a
  persistent `HIDDEN` marker remain visible.
- Hidden target information never reappears before judgment, even if speed or
  ETA later changes.
- Judgment priority remains Player color, Echo color, Shield, then failure.
  Hidden introduces no automatic pass, failure exemption, or custom failure
  flow.
- Retry and Replay preserve seed, settings, gate order, target colors, and
  selected Hidden gates while resetting observation, transition, hidden,
  visual-alpha, and judgment state.
- The Hidden-only Launcher condition disables Booster. A previously held
  Echo or active Shield may still use the existing shared judgment path.
- Hidden does not combine with Camouflage, Fog, Ice, Echo Provider, or another
  new modifier in this iteration. Campaign Stages 1-11 and the existing
  16-condition Step 10 matrix remain unchanged.

The initial Inspector-authored defaults are: eligible progress `0.15-0.85`,
occurrence chance `0.35`, minimum cooldown `2` gates, readable duration
`1.00s`, hide lead `0.85s`, transition `0.12s`, maximum `4`, and guaranteed
first occurrence enabled. These values are human-play candidates, not final
balance.

## Iteration 7 Hidden migration and color-cycling Flicker contract

The Iteration 6 mechanic is renamed to **Hidden** without changing its
behavior. Hidden begins readable, hides color and symbol after the existing
minimum-observation plus ETA condition, preserves the neutral gate silhouette,
and continues to judge against the color shown before hiding.

The new **Flicker** is a separate ordinary-gate modifier. It never hides the
gate. Instead, it cycles through every currently active palette color and its
matching symbol in the same forward order used by player tap-to-cycle input.
The color at the exact collision Gameplay Time is the authoritative judgment
color.

- Camouflage is hidden first, reveals near arrival, then judges.
- Hidden is visible first, hides near arrival, then judges the memorized
  pre-hide color.
- Flicker remains visible and repeatedly changes color; collision-time color
  judges.
- Hidden retains the former Flicker enum values and deterministic selected
  gate IDs. New Flicker receives new enum values, so existing serialized data
  cannot silently change mechanics.
- Flicker plans fix the base color, ordered `CycleColors`, derived cycle count,
  switch interval, deterministic phase offset, transition pulse, gate ID, and
  seed-derived selection data before play.
- The first cycle color is the existing effective base color. Every remaining
  active color follows exactly once in player input order, wrapping at the end
  of the active palette. Each logical Flicker switch is therefore one forward
  player-color input from the previously displayed color.
- Flicker has no authored two-/three-color count setting. Its plan cycle count
  is derived from the active palette captured during deterministic planning.
  Experiment runs use the selected Experiment color count; any future Campaign
  use must capture only colors active at that gate.
- Active color uses the shared pure Core calculation
  `floor((GameplayTime + PhaseOffset) / SwitchInterval) % CycleColorCount`.
  An exact switch boundary uses the new color.
- `ExperimentSession.ElapsedPlayingSeconds` is the only Gameplay Time source.
  Countdown, Failed, and StageCleared do not advance it; Retry and Replay
  reset it.
- Judgment order remains Player → held Echo → Shield → Failure. Player and
  Echo compare with the collision-time Flicker color, and Echo and Shield
  cannot both be consumed.
- Only gates whose deterministic expected pooled-view exposure is at least
  `SwitchIntervalSeconds * MinimumCyclesVisible` may receive Flicker.
- Flicker-only runs disable Booster. Goal application, Campaign application,
  music/BPM/DSP synchronization, speed-adjusted switch intervals, and
  same-gate modifier combinations remain deferred.

Initial Inspector-authored Flicker values are: enabled, progress `0.15-0.85`,
chance `0.30`, cooldown `2`, maximum `4`, guaranteed first occurrence,
`0.50s` switch interval, `0.10s` pulse, three minimum visible cycles, and
deterministic randomized phase offset. Cycle colors are derived from the full
active palette rather than authored. These are human-play candidates rather
than final balance.

## Iteration 8 Campaign Hidden and Flicker stages

Status: Implemented / Human Review Pending.

Campaign expands without changing Stages 1-11:

| Stage | Title | Gates | Colors | Base speed | Cadence | Primary mechanic |
|---:|---|---:|---|---|---|---|
| 12 | Hidden Memory | 50 | Red, Blue, Green | `46 -> 68` | `1.05 -> 0.78s` | Hidden |
| 13 | Flicker Flow | 52 | Red, Blue, Green | `46 -> 68` | `1.08 -> 0.80s` | Flicker |

- Each stage introduces only its named gate modifier. Hidden and Flicker do
  not mix with Camouflage, Fog, Ice, Echo Provider, or each other.
- Stage 12 uses the approved Hidden `0.85s` hide lead, `1.00s` minimum
  readable time, and `0.12s` transition.
- Stage 13 cycles all three active colors in the same forward order as player
  input. Its initial switch interval remains `0.50s`.
- Both stages retain the existing Stage 6+ Shield and Booster selection
  policy. Booster starts at `GO`, uses ordinary Booster auto-pass behavior,
  and creates no Hidden/Flicker exception.
- Flicker target selection must satisfy minimum visible cycles at the fastest
  effective arrival speed, including selectable Booster. A selected Booster
  may bypass early Flicker gates, so deterministic planning must leave enough
  later Flicker occurrences to teach and evaluate the mechanic.
- Hidden uses ordinary static target-color judgment. Flicker uses the
  collision-time Campaign Gameplay Time color. Judgment priority remains
  Player, held Echo when supported, Booster, Shield, then Failure.
- Goal gates never receive Hidden or Flicker. Gate count and the fixed View
  pool do not change because either mechanic is active.
- Retry reproduces modifier targets, cycles, offsets, and gate order while
  resetting Gameplay Time and all per-View visibility/pulse state.
- These values are human-play candidates, not a claim of final balance.

## Iteration 16 Campaign Act 2 mastery curve

Status: Implemented / Human Review Pending.

Campaign expands from 13 to 20 Stages without adding a new color, input rule,
item grant, modifier type, judgment path, or persistence authority.

| Stage | Title | Gates | Base speed | Cadence | Primary mechanic |
|---:|---|---:|---|---|---|
| 14 | Tricolor Reset | 48 | `46 -> 66` | `1.10 -> 0.84s` | None |
| 15 | Camouflage Mastery | 52 | `48 -> 68` | `1.08 -> 0.82s` | Camouflage |
| 16 | Fog Forecast | 54 | `48 -> 68` | `1.08 -> 0.82s` | Fog |
| 17 | Ice Sprint | 52 | `50 -> 70` | `1.04 -> 0.80s` | Ice |
| 18 | Echo Decisions | 56 | `50 -> 70` | `1.06 -> 0.80s` | Echo |
| 19 | Hidden Recall | 56 | `50 -> 70` | `1.08 -> 0.82s` | Hidden |
| 20 | Flicker Finale | 60 | `52 -> 72` | `1.04 -> 0.78s` | Flicker |

- All seven Stages use Red, Blue and Green from the start. Existing tap-budget,
  maximum same-color run, deterministic seed and Retry contracts remain.
- Stage 14 is intentionally shorter and mechanically clean after Stage 13. It
  restores reaction margin before the mastery climb rather than making every
  numeric value monotonically harder.
- Stages 15–20 each enable exactly one previously introduced modifier. Echo
  Provider remains the only runtime-added provider role and does not combine
  with another modifier in this batch.
- Stage 17 is a shorter pressure spike. Stage 18 uses Echo as an intentional
  relief and resource-timing beat before Hidden and Flicker close the act.
- Stage 6+ Shield and Booster selection remains available. No local Shield or
  Booster tutorial grant is repeated, and inventory consumption is unchanged.
- Stage 20 is an act finale, not the campaign endpoint. Clear progression,
  automatic even-Stage Lobby milestones and stable-ID save records continue
  through the already planned 36-Stage campaign.
- Mechanical simulation values are balance evidence only. Human play must
  judge visual readability, fatigue, rhythm variety and whether the returning
  mechanics feel like mastery rather than repetition.

## Iteration 17 campaign learning-curve contract

This contract supersedes the Campaign placement described by Iterations 4,
8, and 16 without changing those historical records, stable Stage IDs, or
Experiment mechanics.

| Stage | Learning role | Colors | Gates | Speed | Cadence | Mechanic |
|---:|---|---:|---:|---|---|---|
| 1–5 | Color and rhythm foundation | authored | authored | unchanged | unchanged | None |
| 6 | Provided Shield training; selection locked | 2 | 38 | `38 -> 56` | `1.22 -> 0.82s` | Shield grant |
| 7 | Provided Booster training; selection locked | 2 | 40 | `40 -> 58` | `1.18 -> 0.80s` | Booster grant |
| 8 | Clean item application | 3 | 40 | `38 -> 56` | `1.22 -> 0.84s` | None |
| 9–11 | Camouflage intro / practice / mastery | 2 / 2 / 3 | 38 / 42 / 46 | `38->54` / `40->58` / `42->62` | `1.24->.90` / `1.18->.84` / `1.12->.80s` | Camouflage |
| 12–14 | Fog intro / practice / mastery | 2 / 2 / 3 | 40 / 44 / 48 | `40->56` / `42->60` / `44->64` | `1.22->.88` / `1.16->.82` / `1.10->.80s` | Fog |
| 15–17 | Ice intro / practice / mastery | 2 / 2 / 3 | 42 / 46 / 50 | `40->56` / `42->60` / `44->64` | `1.20->.88` / `1.14->.82` / `1.08->.78s` | Ice |
| 18–20 | Echo intro / practice / mastery | 2 / 2 / 3 | 42 / 46 / 50 | `42->58` / `44->62` / `46->66` | `1.20->.86` / `1.14->.80` / `1.08->.78s` | Echo |

- Every learning block uses one modifier only. Intro and practice retain two
  colors; mastery restores three colors.
- Hidden is reserved for planned Stages 21–23 and Flicker for Stages 24–26.
  Both remain implemented and testable in Experiment Lab.
- All Stage 8–20 entries allow Shield and Booster selection. The Iteration 18
  contract below supersedes the deferred-consumption statement from this
  historical learning-curve iteration.

## Iteration 18 consumable start-item contract

- On Stage 8+, a successful `START` consumes exactly one owned Shield and/or
  Booster for each selected toggle in one atomic Product save before
  Countdown. Selecting both never exposes a partial decrement.
- Zero stock disables only that item's selection. A stale insufficient-stock
  result or save failure keeps PreRun active, reports the truthful failure and
  starts no attempt.
- Retry returns to selection for a new attempt and consumes retained selected
  items again. Back before Start consumes nothing. Repeated Start after the
  first accepted call cannot consume twice.
- Stage 6/7 provided training items remain stage-local, free and non-consuming.
  A missing Product context disables selectable inventory but does not block
  no-item or provided-item starts.
- Product save remains schema 2; no migration or new persistent field is
  introduced.

## Iteration 12 Gameplay Pause contract

- Pause is available during Countdown, Playing, and Shield Recovery. It does
  not advance attempt Gameplay Time or alter the deterministic gate plan.
- While paused, player movement and color input, gate recycling, judgment,
  mechanic timing/presentation, camera feedback, and registered attempt VFX
  are frozen. Global `Time.timeScale` is not used.
- A gate cannot resolve merely because its trigger or manual judgment was
  already queued in the same frame; the shared Pause coordinator is checked
  immediately before result mutation.
- Resume continues the same Attempt. Restart confirms before using the
  existing Retry-to-PreRun path. Lobby confirmation leaves without recording
  clear or failure.
- Focus loss requests Pause where the state permits it. Focus gain never
  resumes automatically.
- Pause changes no Campaign balance, gate timing, modifier rules, item rules,
  Stage progress format, or deterministic simulation result.
## Iteration 21 commerce reward and Heart contract

- A normal Stage start consumes one Heart in the same atomic Product save as
  selected Shield/Booster inventory. A failed authorization/save never starts
  Countdown and publishes no partial spend. Retry/replay is a new Stage start
  and consumes again.
- Heart capacity is 5. One missing Heart recharges every 30 minutes from an
  explicit UTC anchor, including offline time. A backwards local-clock change
  cannot accelerate recharge. Unlimited Hearts suppress consumption until
  their UTC expiry and purchased durations extend from the later of now or the
  existing expiry.
- Continue Tickets are owned inventory. On failure they are offered before a
  real rewarded-ad offer, then the existing Coin offer. Ticket use is an
  atomic, idempotent Product transaction and still counts toward Core's total
  three-Continue attempt cap.
- The exact approved catalog IDs and rewards are owned by
  `CommerceProductCatalog`. All entries are Google Play consumables; Starter
  reward grant is account-limited locally. Actual store connection, receipt
  verification, Shop UI and Firebase remain separate work.

## Iteration 23 Continue, difficulty, reward and Heart contract

This active contract supersedes the three-Continue cap and Coin prices from
Iterations 19 and 21 without deleting their historical records.

- Continue has no per-attempt count cap. Coin prices are `900`, `1,900`,
  `2,900`, and `4,900`; the fourth and every later Coin Continue remain
  `4,900`. Ticket and rewarded-ad Continues do not advance the Coin-price
  ordinal. Retry starts a new attempt at `900` and restores the one-ad right.
- The release rewarded-ad provider is still absent. Its action remains hidden
  and cannot simulate success until a real provider is integrated.
- Campaign difficulty is authored Stage data: Stages 11, 14, and 17 are
  `Hard`; Stage 20 is `VeryHard`; all other current Stages are `Normal`.
- First-clear base rewards are `100` Coins for Normal, `200` for Hard, and
  `500` for VeryHard. They are idempotent and separate from the existing
  even-Stage Lobby milestone reward. Result presentation separates both
  reward sources instead of displaying a combined unexplained amount.
- A normal Start still atomically spends one stored Heart with selected start
  items. If that same attempt clears, the Product transaction atomically
  records progress and rewards and refunds exactly that consumed Heart.
  Unlimited-Heart and provided/free starts create no refund entitlement.
  Failed save publication cannot partially grant progress, reward, or refund.
- Lobby visual redesign remains deferred. Campaign Fog and Ice are superseded
  by the Iteration 25–27 contracts below; Experiment Lab keeps its historical
  diagnostic comparisons.

## Iteration 25 Campaign Timed Fog Curtain

This section supersedes only the Campaign Fog presentation contract above.
Experiment Lab retains its historical nearest-two visibility comparison.

- In Campaign Stages 12–14, the first Fog-modified gate becoming the next
  judgment triggers one Fog curtain per attempt.
- The curtain follows the player at `current effective speed * 1.5 seconds`
  ahead, keeping the obstruction at roughly one to two gate intervals as
  speed changes.
- Alpha is `1` for `1.5 seconds`, then linearly fades from `1` to `0` over
  `0.5 seconds`. It cannot retrigger until a new Retry/attempt resets it.
- Pause and Continue countdown do not advance Fog time. Continue preserves
  the current Alpha and repositions the curtain relative to the clean respawn;
  Retry resets it.
- Fog changes only visibility presentation. Gate color, judgment, sequence,
  spacing, speed, Goal, item and Continue rules are unchanged.

## Iteration 26 Fog Visibility Curve

This section supersedes only the Iteration 25 Campaign curtain alpha and
duration values. Its one-trigger, positioning and lifecycle contracts remain.

- Every Campaign curtain fades from alpha `0` to `1` over `0.5 seconds` and
  fades back from `1` to `0` over its final `0.5 seconds`.
- Full-opacity duration is authored per Stage instead of hard-coded in the
  view: Stage 12 intro uses `5 seconds`, Stage 13 practice uses `6 seconds`,
  and Stage 14 mastery uses `7 seconds`.
- Future Fog stages must author the three phase durations through the Stage
  Catalog. Runtime code must not infer them from display number or difficulty
  label.

## Iteration 27 Preplaced Ice Runway

This section supersedes only Campaign Ice speed and floor presentation.
Experiment Lab retains its historical whole-track `1.45` comparison.

- Stage Catalog revision 10 authors `IceRunwaySettings.SpeedMultiplier = 2.0`
  for Campaign Stages 15–17. Ice still multiplies non-Booster movement only;
  Booster remains authoritative while active.
- One fixed 50-panel presentation pool covers the maximum current authored
  Stage gate count. Before Countdown, each panel mapped to an Ice gate is
  positioned across that gate's deterministic approach interval. Non-Ice and
  unused panels remain inactive.
- Campaign Ice never swaps the material of the recycled six-segment normal
  track and never instantiates or grows runway content during an attempt.
- Continue preserves the preplaced runway. Retry clears and reconstructs the
  same deterministic layout. Gate color judgment, spacing multiplier, seed,
  sequence, items, Goal and Continue rules are unchanged.

## Iteration 28 Ice Tap Rhythm

This section supersedes only the authored color rhythm inside Campaign Ice
gates. Iteration 27 speed and runway presentation remain authoritative.

- Every Ice gate requires either one or two forward taps from the preceding
  authored target. Zero-tap and three-or-more-tap Ice gates are forbidden.
- Stage 15 and 16 require one tap on every Ice gate. Stage 17 uses one tap on
  14 of 15 Ice gates and one deterministic two-tap gate (`6.67%`).
- Future Ice sequences must keep `double taps / Ice gates < 10%`. The quota and
  selected gate are derived deterministically from the Stage seed, and Retry
  reproduces the same sequence.
- Continue safe-color recovery remains separate from this authored no-miss
  rhythm. Non-Ice stages and Experiment Lab retain their existing generators.

## Iteration 42 mechanic presentation mapping

- Fog timing, trigger count and Stage-owned `5 / 6 / 7` second hold values are
  unchanged. Six presentation banks occupy one speed-adaptive Spline band and
  never alter gate truth, spacing, judgment or target visibility state.
- Each Ice approach remains a prebuilt member of the fixed 50-slot pool and
  keeps the existing `2.0` speed multiplier and tap rhythm. Its mesh samples
  the Campaign Spline between the same deterministic interval boundaries.
- Echo eligibility and acquisition rules are unchanged. The gate membrane is
  presentation-only and appears only while that Echo Provider is legally
  visible and unresolved.

## Iteration 43 storm Fog and curve feedback

- Campaign Fog retains its one-trigger and Stage-authored timing authority, but
  follows at `current effective speed * 0.9 seconds` and occupies a closer
  18–32 unit band. Ten fixed banks, capped wisps, rain and darker tone are one
  presentation lifecycle; none may change gate truth, judgment or speed.
- Camera follow uses a `0.3s` half-life with `24°` yaw and `16°` pitch caps.
  The player root stays on the exact scalar-distance Spline pose while only its
  visual child anticipates the next tangent with bounded steering and lean.
- The gameplay color sequence is a bottom-center horizontal cycle. Current is
  the center authority, next reads to the right and previous to the left. Two-
  color stages show current/next as a balanced pair; larger palettes retain
  fixed presentation slots and the same deterministic color order.

## Iteration 44 Hidden Campaign block

This section supersedes only the Iteration 17 deferral of Hidden Stages 21–23.
Flicker remains deferred to Stages 24–26.

| Stage | Learning role | Colors | Gates | Speed | Cadence | Difficulty |
|---:|---|---:|---:|---|---|---|
| 21 | Hidden Intro | 2 | 42 | `42 -> 58` | `1.20 -> 0.86s` | Normal |
| 22 | Hidden Practice | 2 | 46 | `44 -> 62` | `1.14 -> 0.80s` | Normal |
| 23 | Hidden Mastery | 3 | 50 | `46 -> 66` | `1.08 -> 0.78s` | Hard |

- Every ordinary eligible gate uses only the existing Hidden modifier and its
  approved `0.85s` hide-lead contract. Hidden does not combine with another
  modifier, apply to Goal, add gates, or change ordinary color judgment.
- Intro and practice retain two active colors; mastery restores three. The
  authored seeds are `210021`, `220022`, and `230023`, and Retry reproduces
  the same sequence and Hidden selections.
- Stage 21 uses Steady/Release, Stage 22 uses ThreeColorFlow/Compression/
  Release, and Stage 23 uses ThreeColorFlow/Syncopation/Release. Existing
  start-item selection, Hearts, rewards, Continue and even-Stage milestone
  rules apply without a Hidden exception.
- Clearing Stage 20 on an older 20-Stage catalog makes Stage 21 effectively
  available when revision 11 loads. This compatibility repair does not write
  or synthesize a save record during load.
