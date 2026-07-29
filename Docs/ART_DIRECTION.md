# Art Direction

## Visual goal

Minimal high-contrast mobile graybox presentation that makes stage choice,
selected items, gate judgment, progress, and Goal state readable immediately.

## Camera

- Portrait 9:16
- Position `(0, 8, -10)`, rotation `(20, 0, 0)`
- Fixed rotation throughout all states
- Normal FOV 60; Booster-only FOV 74
- Player and upcoming gates remain readable without follow smoothing

## Palette

- Red `#E63946`
- Blue `#2D7FF9`
- Green `#22C55E`
- Neutral `#D9D9D9`
- Failure `#6B7280`
- Dark translucent panels separate navigation and results from gameplay

Stage 4 explicitly onboards Green. Stages 1–3 use only Red and Blue. Gameplay
colors are reserved for the runner, gates, item accents, and progress fill.

## Stage-select and item presentation

Step 9A replaces player-facing Stage Select with a single-current-stage Lobby.
The five-entry picker remains hidden for development.

- Lobby tiers add generated palette/banner/floor accents after Stages 2, 4,
  and 5.

- The hidden development Stage Select retains five entries and unlock-all
  controls for test setup, but is not part of player-facing navigation.
- PreRun shows the selected stage and two large toggle cards for Shield and
  Booster, followed by Start and Back.
- The toggles communicate free selection rather than an inventory balance.

## Gameplay feedback

### Correct gate

- Gate response and the reusable particle burst begin in the judgment frame.
- The pooled gate resets all reaction scale/material state when recycled.

### Incorrect gate

- Movement stops in the judgment frame.
- The gate and player use the neutral failure material.
- A brief fixed-position camera shake preserves camera rotation.
- Failure result appears promptly with progress and selected items.

### Shield

- Six thin segmented arcs surround the player while leaving the central
  gameplay material unobscured.
- Selected Shield is visible during countdown; protection remains Core-owned.
- No collectible or runtime pickup is present in the stage scene.

### Booster

- Booster alone enables stronger speed lines, player trail, launch
  shake/pulse, and 74-degree FOV.
- A draining bar and final-20% warning communicate the exit.
- Normal play returns to restrained effects and 60-degree FOV immediately
  after its deterministic distance.
- No screen-space distortion, post-processing, or new external effect asset is
  used.

### Goal and results

- A visible neutral Goal appears after the final gate.
- Clear and failure use separate full-screen panels.
- Clear prioritizes STAGE CLEAR, time, items, and best time.
- Failure prioritizes STAGE FAILED, progress, items, and Retry.
- Clear waits 1.2 seconds and uses bright positive effects; failure waits 1.0
  second and uses a darker hierarchy. The character animation remains visible
  before either panel.
- Buttons remain above the gameplay tap surface.

## Constraints

- Unity primitives and generated materials only
- No external assets, post-processing, audio, tween package, or runtime
  allocation-heavy effects
- Maintain safe-area readability on a small portrait phone

## Step 9A.1 transition presentation

- Booster effects ease out while the world and existing gates continue
  moving. There is no world freeze, gate jump, pool rebuild, or spawn gap.
- Continue keeps the exact failure scene visible and frozen behind the
  `3, 2, 1, GO` overlay. Camera position/rotation, tracks, and unaffected gate
  transforms remain at the failure location.
- Only the failed gate's harmful judgment is retired. Continue must read as
  the same run resuming, never as a new scene or distant safe-section spawn.
- The doubled movement scale does not add camera rotation, follow smoothing,
  post-processing, or new effect assets.

## Information hierarchy

The Step 9C player-facing implementation uses this strict order:

1. Upcoming gate.
2. Player current color.
3. Next tap color.
4. Stage progress.
5. Relevant item status.
6. Decorative information.

The central recognition corridor is the middle 36% of the screen from the
lower player-reading region through the upcoming-gate region. Normal gameplay
UI and particles must not enter it. The compact color HUD sits at the upper
left, while stage progress and conditional item status remain inside one top
panel.

## Forbidden player-facing presentation

- Gameplay HUD visible in the Lobby.
- Overlapping labels.
- Unexplained horizontal bars.
- Debug tier controls.
- Full color-order sentences over gameplay.
- Centered UI covering gates.
- Permanent item status text.
- Normal gameplay particles obscuring the recognition corridor.

`BASE`, `UP 1`, `UP 2`, and `UP 3` are not player-facing controls or labels.
Lobby progression changes one non-interactive environment treatment at a
time. The hidden stage picker belongs only under `DevelopmentDebugRoot`.

## Graybox quality rule

A graybox may use primitive assets, but it must still provide:

- Clear hierarchy.
- Correct spacing.
- No overlap.
- Immediate function recognition.
- Consistent margins.
- Readable contrast.
- Mobile Safe Area compliance.

These rules are an implementation contract. The idempotent Scene Builder,
structural checks, resolution tests, and visual evidence must enforce them;
they are not advisory.

## Step 9C mobile UI contract

- The seven generated roots are `LobbyRoot`, `PreRunRoot`,
  `GameplayHudRoot`, `CountdownRoot`, `ClearResultRoot`,
  `FailedResultRoot`, and `DevelopmentDebugRoot`. Countdown is the only
  normal overlay that accompanies the gameplay HUD.
- Lobby contains a title, separate stage number/title/description, one
  dominant Play button, compact progress, and exactly one active
  non-interactive environment tier.
- Color instructions use read-only tiles. Red uses a circle, Blue a square,
  and Green a triangle. The current tile is largest, the next tile is marked,
  and unavailable colors are hidden. Gates repeat the same symbol on their
  visible top crossbar.
- Shield is a small icon only while selected or active. Booster uses a
  labeled `BOOST` bar only while active; it is inside the top panel and warns
  during its final 20 percent.
- Normal play has no speed-line emission. Booster uses pooled stretched
  emitters at the left and right camera edges. The former central player
  trail is disabled. Retry, Continue, failure, clear, and Booster exit clear
  all pooled effects.

## Step 10 experiment presentation

- Lobby tier backgrounds are subtle full-screen palette treatments behind
  content. The former tall side accent blocks are removed.
- The active color stack is upper-left and outside the central recognition
  corridor. Its top tile is largest, the next tile is medium and marked
  `NEXT`, and later tiles compact progressively so all six fit.
- Experimental accessibility mapping appends Yellow/star `#F4C430`,
  Purple/diamond `#9B5DE5`, and Cyan/hexagon `#00B8D9`.
- Normal camera starts at `(0, 8, -10)`, rotation `(20, 0, 0)`, FOV 60.
  Booster blends in 0.22 seconds toward player-relative offset
  `(0, 5.4, -7.4)`, rotation `(14, 0, 0)`, FOV 78, then returns exactly in
  0.35 seconds. Position shake is layered on the blended pose and never
  accumulates.
- Camouflage and distant Fog gates use the existing neutral silhouette
  material and hide the symbol. Reveal restores both without moving the gate.
- Ice uses the Cyan generated material on the fixed track pool during the
  development preview. Entry and exit never create or rebuild track objects.
- The Reset Progress confirmation and experiment launcher are development
  controls. They are absent from the normal release flow.
