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
3. Shield and Booster are independent free toggles.
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

## Simulation and telemetry

Core simulation uses the same stage/session/gate/item/Continue rules as play.
Perfect, Expert, Average, Novice, and Stress are configurable provisional
profiles. Reports estimate mechanical duration, input pressure, failures, and
item/Continue effects only. Local development telemetry may later calibrate
profiles; it is disabled by default, contains no identifiers, and is never
uploaded.
