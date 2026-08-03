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
- Experiment must use the same Runtime Flow as Campaign.

## Iteration 3 Clone experiment presentation

- Clone keeps the Source Gate silhouette, target color, symbol, and readable
  judgment opening.
- Clone has lower visual intensity than its Source and reads as a translucent
  echo, while its outline and central symbol remain legible.
- The presentation must distinguish Clone from both a decorative afterimage
  and the segmented Shield around the player.
- Clone uses the existing pooled gate geometry, generated materials, and color
  symbols. It adds no shader package, post-processing, full-screen distortion,
  camera effect, or dedicated particle system.
- Under Camouflage, Clone uses the ordinary neutral silhouette and hidden
  symbol, then restores its Source color and symbol at the ordinary reveal
  condition. Camouflage does not imply safety or judgment exemption.
- Clone failure uses the existing failure presentation plus a short,
  distinguishable cause such as `CLONE MISS`.
- Retry/Replay clear Clone reaction and visibility state before the identical
  condition is rebuilt.

## Iteration 4 Echo Modifier presentation

This section supersedes the Clone Gate presentation above.

- An Echo Provider keeps the ordinary gate frame, color opening, and symbol.
  A compact `ECHO` marker sits above the readable judgment information.
- Hidden Camouflage providers use a neutral marker that reveals no target
  color. Marker, gate color, and symbol react together when ETA reveal begins.
- The held Echo is a thin inner player ring or shell colored from the stored
  effective gate color. The existing segmented Shield remains the outer layer.
- Acquisition uses one short transfer/pulse. Consumption brightens and
  collapses the inner shell. Existing primitives, generated materials, and
  bounded animation are reused.
- Shield and Echo must remain distinguishable when simultaneous. No new
  shader package, post-processing, full-screen distortion, or dedicated
  particle system is introduced.
- Stage-local Shield and Booster status may reuse the existing item HUD with
  a concise `PROVIDED BY STAGE` treatment.

Implementation result: Echo uses the thin four-segment inner player shell,
while Shield remains the larger six-segment outer shell. An ordinary provider
gate uses the existing frame and an `ECHO` marker. Camouflage keeps provider
target color and symbol hidden until ETA reveal. Human portrait-mobile review
is still required for simultaneous Echo/Shield separation and marker size.

## Iteration 6 Hidden presentation (originally named Flicker)

- A Hidden gate uses the ordinary pooled gate silhouette, material, target
  color, symbol, and judgment opening.
- A compact neutral `HIDDEN` label is visible before and after target
  information hides without covering the target symbol.
- During the short transition, existing material color blending and text
  alpha are reused. No new shader, particle package, post-processing, camera
  effect, or full-screen distortion is introduced.
- At the end of the transition, the gate frame remains as a neutral
  silhouette and the `HIDDEN` label remains readable, while the target color
  and target symbol are hidden.
- Gate geometry, collider, judgment opening, position, and scale remain
  visible and stable. Hide never makes the whole gate disappear.
- Portrait-mobile human review owns marker size, minimum observation time,
  hide timing, transition clarity, memory interval, and confusion with
  Camouflage.

Implementation result: the existing pooled gate uses its target material and
symbol during observation, then blends to the existing neutral material while
the symbol's rich-text alpha reaches zero. `HIDDEN` remains visible in the
same TextMesh, so no new scene object, material, shader, particle system, or
package was required. Automated checks confirm retained transform, collider,
marker, and pooled object; portrait readability remains a human decision.

## Iteration 7 Hidden and color-cycling Flicker presentation

The Iteration 6 presentation is renamed to Hidden. Its visual behavior is not
redesigned:

- Target color and symbol begin visible.
- The existing short hide transition removes target information.
- Neutral silhouette, judgment opening, transform, collider, and a neutral
  `HIDDEN` marker remain.

The new Flicker remains fully visible and uses a distinct neutral `FLICKER`
marker. Core supplies one authoritative active color; the View applies that
color and its existing matching symbol together.

- Logical color changes immediately at the configured boundary.
- A short brightness or outline pulse may emphasize the boundary without
  mixing two judgment colors, introducing a neutral interval, hiding the
  gate, or changing geometry/collider state.
- The marker persists through every transition and does not imply a fixed
  target color.
- Frame skips synchronize immediately to the current Core phase rather than
  replaying missed transitions.
- Existing generated materials, symbols, and pooled objects are reused. No
  shader package, post-processing, large particle treatment, or full-screen
  effect is added.

Portrait human review must distinguish Camouflage, Hidden, and Flicker; judge
full-active-palette rotation at each supported color count, whether the
one-forward-tap sequence feels followable, collision-boundary readability,
pulse strength, symbol synchronization, Echo/Shield feedback, and 9:16
legibility.

## Campaign Stage 12-13 presentation

- Stage 12 reuses the approved pooled `HIDDEN` presentation without adding a
  new gate mesh, material family, shader, particle system, or screen effect.
- Stage 13 reuses the approved neutral `FLICKER` marker, synchronized color
  and symbol swap, and short transition pulse.
- Campaign binding must reset every Hidden/Flicker View field before a pooled
  gate is reused for an ordinary gate or the other modifier.
- Booster visuals remain the existing player effect. Hidden/Flicker gates do
  not change their marker or geometry to indicate an automatic Booster pass.
- Lobby Stage 12 and 13 entries use the existing catalog-driven button and
  summary layout. No separate mechanic-selection UI is added.
- Portrait human review must confirm that Stage 12's longer memory window and
  Stage 13's three-color rotation remain readable at their authored speed
  curves and while Booster is offered.

## Iteration 12 Account, Settings, and Pause presentation

- Account Choice presents the working Guest action as primary. An unavailable
  provider action is hidden in release presentation and may appear only as a
  truthful diagnostic in development.
- Frontend and Gameplay reuse one compact Settings panel layout for Master,
  Music, SFX, and Vibration, plus Apply and Cancel. A save error remains on the
  panel instead of implying success.
- Gameplay Pause uses a full-viewport, raycast-blocking black Dim at alpha
  `0.85`, drawn above gameplay guidance. The interactive panel stays within
  portrait Safe Area and offers Resume, Restart, Settings, and Lobby.
- Restart and Lobby require confirmation. A load error is readable without
  exposing gameplay interaction underneath it.
- The pause treatment uses existing uGUI primitives and generated assets. It
  adds no blur, post-processing, shader, package, or Scene-wide VFX search.
- Human review owns 9:16 hierarchy, Account Choice trust and clarity, slider
  readability, Dim coverage, rapid Back/tap behavior, and background/foreground
  feel. Music/SFX sliders currently persist but have no audible target.

## Lobby consolidation presentation

- The Frontend Lobby shows one recommended Stage number, title, mechanic, and
  cleared count above a dominant `START STAGE` action. It does not expose a
  Stage picker or duplicate the existing Campaign Lobby.
- The shared Settings panel uses four clear toggles: Notifications, Music,
  SFX, and Vibration. Master Volume is hidden. A visible prototype notice says
  notifications are not sent yet.
- Terms, Privacy, and Support use one visual treatment. In development,
  missing URLs remain visible but disabled with `URL NOT CONFIGURED`; release
  presentation hides unavailable actions.
- The Pause button belongs to `GameplayHudRoot` and sits in the upper-right
  Safe Area below the Stage HUD, clear of Shield state and the gameplay tap
  surface. Its minimum interactive size is 44 by 44 UI units.
- Human review owns portrait hierarchy, wording, toggle comprehension, link
  discoverability, and Pause-button reachability on Android and WebGL.

## Iteration 14 toggle-state correction

- Toggle state uses one right-aligned label per row. The same label changes
  between green `ON` and neutral `OFF`; two state words must never overlap or
  remain together in the hierarchy.
- Notifications, Music, SFX, and Vibration use the same typography, alignment,
  color rule, and immediate state-change response in Frontend and Pause.
