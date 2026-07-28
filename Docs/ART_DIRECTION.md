# Art Direction

## Visual goal

Minimal high-contrast mobile graybox presentation that makes stage choice,
selected items, gate judgment, progress, and Goal state readable immediately.

## Camera

- Portrait 9:16
- Position `(0, 8, -10)`, rotation `(20, 0, 0)`
- Fixed rotation throughout all states
- Normal FOV 60; Booster-only FOV 70
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

- Stage Select contains five large portrait-safe entries with OPEN, LOCKED, or
  CLEARED plus best-time summary.
- The development unlock-all control exists for testing but is hidden by
  default.
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

- A visible shell surrounds the player only while the selected Shield remains.
- No collectible or runtime pickup is present in the stage scene.

### Booster

- Booster alone enables stronger speed lines, player trail, and 70-degree FOV.
- Normal play returns to restrained effects and 60-degree FOV immediately
  after its deterministic distance.
- No screen-space distortion, post-processing, or new external effect asset is
  used.

### Goal and results

- A visible neutral Goal appears after the final gate.
- Clear and failure use separate full-screen panels.
- Clear prioritizes STAGE CLEAR, time, items, and best time.
- Failure prioritizes STAGE FAILED, progress, items, and Retry.
- Buttons remain above the gameplay tap surface.

## Constraints

- Unity primitives and generated materials only
- No external assets, post-processing, audio, tween package, or runtime
  allocation-heavy effects
- Maintain safe-area readability on a small portrait phone
