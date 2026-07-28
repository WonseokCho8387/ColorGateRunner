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
