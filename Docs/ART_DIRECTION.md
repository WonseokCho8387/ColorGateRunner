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
- Clear prioritizes STAGE CLEAR, the difficulty base Coin reward, a separate
  milestone reward when earned, time, items, and best time.
- Failure prioritizes STAGE FAILED, progress, current Coin/Heart status,
  eligible Continue actions, and Retry.
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
- **Superseded only in timing by Iteration 41:** Continue keeps the exact
  failure scene visible and frozen behind its transition overlay. Camera
  position/rotation, tracks, and unaffected gate
  transforms remain at the failure location.
- Retained modifier visuals and safe colors must already match the resumed
  state on the first countdown frame. Consumed Shield, Booster, Echo, local
  grants, and their HUD/world effects must already be absent; they do not
  remain visible during countdown and disappear only at `GO`.
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
- Experiment Ice uses the Cyan generated material on the fixed track pool for
  its historical comparison. Campaign Ice uses the preplaced runway contract
  below and never recolors the whole normal track.
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

## Iteration 15 automatic Lobby progression presentation

- Frontend Lobby reserves a non-interactive full-screen theme layer behind the
  existing profile, Stage summary and primary Play action.
- Wallet and Shield/Booster inventory remain compact header information. Theme
  name, next clear target and pending reward summary must not compete with the
  primary Play action.
- Six visible upgrade blocks represent the current theme's automatic growth.
  Three palette states establish the planned three-theme structure, but these
  generated blocks are implementation placeholders rather than final art.
- No upgrade placement interaction, camera navigation, shader package or new
  Scene is introduced. Theme changes occur inside the existing Frontend Lobby.
- Human review owns whether the newly unlocked step is noticeable, reward text
  is understandable, the hierarchy survives Safe Area, and the placeholder
  does not misrepresent launch-quality visual completion.

## Iteration 17 campaign learning presentation

- Stage 6 and 7 PreRun cards show `PROVIDED` for the named training item even
  though manual selection is locked. Provided state has visual precedence over
  the generic locked state.
- Stage 8 is the first screen where both Shield and Booster cards become
  selectable. Its clean three-color run establishes item feedback without a
  competing gate modifier.
- Intro and practice Stages use two active color tiles; mastery restores the
  third tile. Camouflage, Fog, Ice and Echo retain their existing symbols,
  materials and feedback rather than gaining tier-specific variants.
- Hidden/Flicker Campaign presentation is absent from Stages 1–20 but remains
  available in the development-only Experiment Lab.

## Iteration 18 owned start-item presentation

- Stage 8+ Shield and Booster cards show their current owned count with the
  existing `ON`/`OFF` selection state. This historical zero-count-disabled
  behavior is superseded by Iteration 39's `BUY 900` modal; Stage 6/7
  `PROVIDED` and Stage 1–5 `LOCKED` labels retain precedence over inventory
  copy.
- PreRun explains that one owned unit is used when starting. Insufficient
  current inventory reports `NOT ENOUGH START ITEMS`; persistence failure
  reports `ITEM SAVE FAILED`. Both statuses remain inside PreRun and must not
  imply that Countdown or a free grant succeeded.
- The status line remains separated from both item cards and the Start action
  within the portrait Safe Area. This iteration adds no new item art, purchase
  affordance, shop surface, animation or gameplay HUD treatment.

## Iteration 19 Continue economy presentation (historical)

The three-Continue cap and original prices below are superseded by Iteration
23. This section is retained as presentation history.

- Failure Result replaces the ambiguous free `CONTINUE` button with a Coin
  action showing its current exact price and a separate `WATCH AD TO CONTINUE`
  action only when the provider is available. Retry and Lobby remain visible.
- While an ad request is pending, Continue actions are locked and the frozen
  failure scene remains visible. Insufficient Coins, save failure, ad failure
  and cancellation use truthful status text without implying a resume.
- After three successful Continues, both Continue sources disappear. Retry
  begins a fresh attempt at the first Coin price with a fresh ad right.
- Clear Result retains its existing `CONTINUE` navigation action; its separate
  name and hierarchy must not read as a Coin or ad offer.
- This iteration adds no ad creative, provider branding, Coin animation,
  purchase surface or final result-screen art. Human review owns price
  comprehension, hierarchy and portrait readability.

## Iteration 20 Lobby and Shop visual reference direction

- The approved reference direction is a bright, high-saturation, rounded
  mobile-game shell with layered depth, large icon-led actions, strong color
  grouping and readable outlined type. It is a hierarchy reference, not
  permission to copy characters, logos, icons, layouts pixel-for-pixel or
  other proprietary artwork.
- Home composition uses a dominant themed Lobby illustration/environment,
  compact resource chrome at the top and a persistent icon navigation bar at
  the bottom. The main Stage action remains the strongest interactive target.
- Shop cards use clear section bands, large reward imagery, quantity labels
  and one high-contrast localized-price button. The Coin balance remains
  visible while scrolling.
- Journey and Collection references establish future vertical progression and
  category-grid patterns. They are not current implementation scope.
- All future generated art must use Color Gate Runner's neon sci-fi identity,
  original characters/symbols and the portrait Safe Area. Reference branding
  and copyrighted character likenesses must not enter project assets.

## Iteration 21 Heart and Continue Ticket presentation

- Heart status belongs in the persistent top resource chrome when the Lobby
  redesign is implemented. It must distinguish `5/5`, recharge countdown and
  timed unlimited state without relying on color alone.
- Failure Result orders eligible Continue actions as owned Ticket, real ad,
  then Coin. The Ticket action shows the owned count and never masquerades as
  a free or paid offer.
- This iteration adds the functional graybox Ticket action only. Final Heart,
  Ticket, Shop-card art and animation remain part of the original neon sci-fi
  visual pass and must not copy the supplied reference assets.

## Iteration 23 result-loop presentation

- Failure Result exposes a compact wallet row for current Coins and Hearts.
  Eligible actions remain ordered Ticket, real rewarded ad, then Coin, with
  the uncapped Coin schedule `900 / 1900 / 2900 / 4900+` clearly labeled.
- The current build has no rewarded-ad provider, so the ad action is hidden.
  Insufficient Coins open a truthful local modal; because Shop is not yet a
  working destination, the modal provides no fake Shop or purchase action.
- On Continue countdown, modifier visuals and safe colors are already in their
  resumed state. Consumed Shield, Booster, Echo, local grants, and associated
  HUD/world effects are already off from the first countdown frame.
- Clear Result shows the difficulty base Coin reward and any milestone reward
  as separate rows rather than one ambiguous total. Campaign hides Replay;
  `Next Stage` is the primary action and enters the next unlocked PreRun
  directly, with Lobby used for final-Stage or unavailable destinations.
- Heart refund on Clear is represented by the refreshed Heart value rather
  than an unapproved reward animation.
- Final Lobby layout, the distance-based Fog curtain treatment, and authored
  Ice floor approach/readability remain deferred. This slice does not claim
  final visual polish for those surfaces.

## Iteration 25 timed Fog curtain presentation

- Campaign Fog is one dark blue-gray translucent world-space curtain spanning
  the playable road and vertical view, rather than gray materials applied to
  individual distant gates.
- It follows the player at 1.5 seconds of effective travel distance, holds at
  full opacity for 1.5 seconds and fades smoothly over 0.5 seconds.
- Gates retain their authored colors behind the curtain. The curtain is hidden
  outside its timed window and must not duplicate across Builder runs.
- Pause and Continue countdown freeze the visible Alpha. Continue respawn
  repositions the same curtain without flashing or restarting it.
- The generated treatment is still a graybox visibility proof. Human review
  owns final texture, volumetric depth, softness, color and device readability.

## Iteration 26 Fog entrance and duration presentation

- The Campaign curtain now eases into view over `0.5 seconds` rather than
  appearing at full opacity on one frame.
- Full-opacity time follows the learning block: `5 seconds` for intro,
  `6 seconds` for practice and `7 seconds` for mastery. Every stage then fades
  out over `0.5 seconds`.
- Human portrait review must judge whether the longer Stage 14 obstruction is
  challenging without making the authored gate colors feel arbitrarily hidden.

## Iteration 27 Ice runway presentation

- Campaign Ice is visible on the road before the player reaches it. Cyan floor
  panels span deterministic gate-approach intervals so entry reads as physical
  terrain rather than a whole-floor material pop.
- The Scene contains exactly 50 prebuilt panels, matching the maximum current
  authored gate count. Only panels belonging to Ice plans are active; no
  runtime object creation or pool growth is allowed.
- Panels sit slightly above and inside the normal seven-unit track, overlap at
  seams, and leave the recycled track material unchanged. Continue preserves
  their world positions; Retry hides and deterministically rebuilds them.
- This remains a graybox Cyan treatment. Human portrait review owns final
  texture, edge glow, depth, transition readability and whether `2.0` speed
  feels forceful without becoming visually unfair.

## Iteration 28 Ice input readability

- Human play accepted the distance-based Fog curtain and preplaced Ice runway
  directions. Their visual timing, geometry and materials are unchanged here.
- Campaign Ice now presents a predominantly one-tap color rhythm so its `2.0`
  movement pressure does not rely on repeated rapid tapping. The single rare
  Stage 17 double-tap remains a gameplay cue, not a new visual effect.
- Human review must confirm the one-tap cycle remains readable against Cyan
  terrain and that the rare double-tap does not look like an input error.

## Iteration 29 Lobby hierarchy

- The graybox Lobby now follows the approved mobile hierarchy rather than a
  centered text list: compact resource controls at the top, a dominant themed
  development area, and a bottom Stage action card.
- Theme name and upgrade rail are the central focal point. The one-time reward
  banner may temporarily outrank the rail but disappears after presentation.
- Profile, Coin, Heart and Settings share one dark top bar. Shield and Booster
  inventory is visually secondary. The bottom card owns the only large green
  `PLAY` action.
- This iteration establishes layout and information weight only. Final
  illustration, icons, materials, typography, animation and responsive device
  polish remain human-reviewed visual production work.

## Iteration 30 Color Courtyard visual slice

- Theme 1 is an original futuristic elevated courtyard at night. Deep navy is
  the base; Cyan/Electric Blue lead the energy language, with Green and Coral
  used as restrained color-system accents.
- The Lobby environment is composed as Background, Midground reactor and
  Foreground ambient energy. The top 18 percent and lower Stage-action region
  remain dark and low-detail so persistent UI keeps priority.
- The central reactor has a faceted color-energy crystal and six surrounding
  attachment nodes. Product Lobby milestones activate those nodes one at a
  time; no tap or manual construction interaction is added.
- Ambient motion is limited to a subtle reactor scale pulse and a low-amplitude
  energy-frame alpha pulse using unscaled time. Decorative Images never receive
  raycasts.
- Theme 2 and Theme 3 remain palette fallbacks. Their final illustrations,
  typography/icon replacement and device-responsive polish require separate
  visual iterations.
- The supplied Royal Match screens informed only mobile hierarchy. No Royal
  Match character, building, prop, iconography or branded composition is used.

## Iteration 31 Neon gameplay visual slice

- Theme 1 gameplay uses original Blender-authored production assets rather
  than primitive-only presentation: one spherical cyber vehicle, a modular
  three-part gate, a neon track segment, distant city silhouettes and a Goal
  portal. The editable `.blend`, exported FBX files and UV/PBR map families
  remain together under the Theme 1 gameplay art folder.
- The runner reads as a compact ball-shaped cyber vehicle with a distinct
  color shell. Gates keep three separately colorable judgment parts and the
  fixed six-slot runtime pool; visual replacement must not change collision,
  authored order or color judgment.
- A successful gate crossing emits one deterministic `0.3s` break treatment
  from a fixed six-view pool. The fragments are presentation only and never
  create runtime physics objects or affect judgment.
- Track edge rails, lane pulses, city highlights and the Goal portal use
  emissive Cyan / Electric Blue accents over dark alloy. The existing URP
  profile enables restrained low-cost Bloom with four iterations; readability
  and mobile performance take priority over a large glow radius.
- The Lobby ambient layer must contain genuine transparent pixels. A baked
  checkerboard or opaque transparency preview is invalid even if it resembles
  transparency in an image viewer.
- Human portrait review owns final scale, silhouette readability, gate-break
  timing, emissive balance, Bloom intensity and performance on target devices.

## Iteration 32 Neon UI skin

- Frontend and Campaign share one original dark-alloy UI language: deep navy
  translucent interiors, restrained Cyan/Electric Blue edge light, Green for
  primary actions, Coral for destructive actions and Gold for currency.
- Panels, modals, item cards, buttons and resource chips use genuine-alpha
  9-slice sprites. Corners and edge nodes must not distort across portrait
  aspect ratios; decorative circuitry stays subordinate to text.
- Coin, Heart, Shield, Booster, Settings, Play, Back, Pause, Retry and Continue
  use original geometric neon icons. Icon Images never block input and remain
  paired with existing text until final localization/accessibility review.
- Button states are visibly distinct through hover, press, selection and
  disabled tints. Disabled actions must read unavailable without looking like
  a successful or purchasable state.
- Existing hierarchy, wording, navigation and runtime values remain the source
  of truth. Human review owns final typography, icon/text spacing and device-
  specific scale.

## Iteration 33 Theme 1 artwork axis correction

- Every imported Theme 1 FBX instance begins at local position zero, identity
  rotation and unit scale beneath its gameplay-owned presentation parent. FBX
  importer rotation is never allowed to become the authored Scene pose.
- The gameplay track reads as a horizontal road: approximately `7.12` units
  wide on X, no more than `3` units high on Y and at least `39.5` units long
  on forward Z. A tall Y-axis wall is a Builder validation failure.
- Runner, Gate, Track, Goal portal and City backdrop share this normalization
  contract so their relative Blender composition is preserved consistently.
- Human portrait review still owns camera composition, road depth, gate/runner
  contact readability and whether the corrected horizon feels comfortable on
  the target device.

## Iteration 35 Protection field and Warp Booster

- This section supersedes the Iteration 4 cube-segment Shield/Echo shells and
  simultaneous separation requirement. A normal Shield attempt cannot generate
  Echo Providers, so the two fields never need to communicate simultaneous
  layers in normal play.
- Shield and held Echo use the same original transparent sphere language:
  procedural hex cells, Fresnel edge, subtle surface motion and emissive output
  that reacts to the existing restrained URP Bloom. No tutorial asset or new
  render package is imported.
- Normal Shield is Cyan/Electric Blue. Echo uses only its stored effective
  runner color; brightness and alpha may animate, but no secondary hue,
  rainbow edge or color gradient may contaminate recognition.
- Consumption collapses the surface immediately and emits one 0.35-second
  outward particle pulse in the exact same field hue. Shield and Echo each own
  one fixed Builder-created emitter; no runtime effect object is created.
- Warp Booster is camera-local and centered. Two additive stretched-particle
  layers use Cyan and Gold, emit from a radius-five circle toward the camera,
  travel at 35 units/second for 1.5 seconds and fade at both lifetime ends.
  Their combined emitted capacity is capped at 160 and the center remains
  readable through a hollow radial origin rather than an opaque overlay.
- Existing Booster distance, FOV, warning, camera blend, pause and state timing
  remain unchanged. Both collapse emitters participate in the existing attempt
  pause contract. Human device review owns pulse intensity, density, streak
  length, Bloom, field transparency and mobile frame cost.

## Iteration 36 Runner readability and Bloom

- The chase-camera view must communicate a spherical cyber vehicle before
  close inspection. Its dark ball-car hull is framed by a wide rear bumper,
  side fins and twin exhaust housings; twin emissive exhaust rings, chevrons
  and one rear light bar establish a readable rear face.
- The runner's current gameplay color remains recognizable on a restrained
  beveled rear panel. It must not tint the whole hull or make the vehicle merge
  into the Track.
- Red, Blue, Green, Yellow, Purple, Cyan and Neutral gameplay materials use HDR
  emission. Track dark alloy remains near-black and non-emissive so Bloom
  belongs to semantic color and energy accents instead of washing out the road.
- The existing URP Bloom stays at four iterations and uses threshold `0.8`,
  intensity `0.85` and scatter `0.58`. Human target-device review owns final
  apparent glow, silhouette separation and performance.
- Track curves, Spline packages, camera banking, multiplayer lanes and AI
  racers are separate iterations and do not alter this visual contract.

### Runner glass color follow-up

- The main spherical hull follows the current runner color with a smooth,
  coated-glass appearance. It remains opaque enough for a stable mobile
  silhouette, highly smooth and strictly non-emissive.
- The rear panel, exhaust rings, chevrons and light bar remain the HDR accents.
  Bloom must not spread across the entire sphere.

## Iteration 37 Spline Track Lab

- The first curved-track slice is a broad horizontal S-curve designed for a
  portrait chase camera. It changes yaw without banking or rolling the horizon.
- The road is generated from the same Spline that drives gameplay presentation:
  near-black alloy center, narrow Cyan emissive edges, seven-unit width and no
  decorative geometry that implies a second path.
- Runner, gates and Goal align to the local forward tangent. The camera follows
  the same tangent with a fixed chase offset and world-up orientation.
- Human review owns curve anticipation, motion comfort, edge Bloom strength and
  whether upcoming gates remain readable through both bend directions.

## Iteration 38 World-space protection and Warp correction

- This section supersedes Iteration 35's camera-local, centered Circle Warp
  paragraph. The Booster effect belongs to the runner's 3D travel direction,
  never to a fixed screen coordinate or Canvas overlay.
- One non-emitting controller follows the Player. Its Cyan and Gold layers emit
  from broad forward box volumes around the track and simulate in World Space,
  so streaks keep depth and move backward past the runner/camera as speed cues.
- The corridor stays visually open around the next gate. Streaks use velocity
  alignment and two-ended fades rather than an opaque central starburst. Total
  emitted capacity remains 160.
- The shared Shield/Echo sphere retains its procedural hex cells, Fresnel rim,
  exact semantic hue and collapse pulse. Its custom URP shader must compile
  without an error fallback on the active render pipeline; a pink surface is a
  defect, not an accepted style.
- Human portrait/device review owns apparent 3D depth, forward alignment,
  Shield transparency, Echo hue, Bloom balance and mobile performance.

## Iteration 39 Shield centering, Warp coverage and chase framing

- Shield and Echo field surfaces keep the same transparent procedural-hex
  material and semantic color contract, but their shared root and collapse
  pulse center `0.85` units above the Player origin so the sphere wraps the
  authored vehicle rather than the track floor.
- Cyan and Gold Warp boxes extend farther forward and higher into the portrait
  frame. Both prewarm so activation reads as an immediate 3D corridor, not a
  sparse lower-screen tail; World Space simulation, stretch alignment and the
  total 160-particle cap remain mandatory.
- Normal chase framing moves closer to `(0, 7, -8.2)` / `18°`; Booster blends
  to `(0, 5, -6.5)` / `13°`. The next gate and semantic runner color must stay
  readable, and the effect must leave an open central travel lane.
- The PreRun purchase modal reuses the shared Theme 1 panel, primary and
  secondary surfaces. Text hierarchy is item/price first, current balance or
  shortage second; it does not introduce Store art or imply a real-money sale.
- Human portrait/device review owns field fit and transparency, Warp density in
  the upper frame, camera comfort, gate occlusion, Bloom balance and mobile
  frame cost.

## Iteration 40 Campaign Spline and continuous city

- The active Campaign road now uses the same Spline pose for its generated
  near-black center, Cyan edges, runner, camera, gates, Goal and mechanic
  presentation. Curvature must read as one continuous roller-coaster-like route
  without introducing banking or horizon roll.
- Early Stages use restrained horizontal turns. Later Stages broaden turns and
  add gentle climbs/descents, while the next gate remains visible within the
  portrait chase framing. Loops, inversions and abrupt crests remain excluded.
- Theme 1 city silhouettes and their glow partners form one fixed 24-slot pool.
  They appear on both sides at deterministic varied distances and offsets,
  never enter the seven-unit road corridor, recycle behind the runner and remain
  present through the Goal to sustain parallax and speed perception.
- The generated road and pooled city must not allocate presentation objects per
  frame. Human device review owns curve anticipation, building pop, city
  density, horizon comfort, gate readability and mobile performance.

## Future curved-track visual guardrails

- The completed Campaign Spline uses broad anticipation, restrained elevation
  and a stable horizon before any banking or roller-coaster roll is considered.
- A future three-to-five-lane Race view derives parallel road ribbons from one
  center Spline. Lane separation, opponent silhouette and result readability
  must be proven in portrait before additional effects or collision are added.

## Iteration 41 Spline camera inertia and quick Continue

- The Campaign chase camera follows route rotation with a restrained `0.2s`
  half-life. Lag is capped at `12°` yaw and `8°` pitch, while roll is rebuilt
  as zero so the runner side/top becomes briefly visible without tilting the
  portrait horizon.
- Stage start, Retry and Continue snap the camera basis to the current route.
  Pause and failure freeze camera feedback; Booster continues to blend only
  its existing local chase offset, pitch and FOV on top of the same basis.
- Initial entry keeps `3, 2, 1, GO`. Continue shows centered `READY` for
  `0.5s`; gameplay resumes as `GO` appears, and GO remains a non-blocking
  `0.25s` flash over the live HUD.
- Human device review owns motion comfort, curve strength, crest visibility,
  GO legibility and whether the shorter recovery preserves continuity.

## Iteration 42 Mechanic surface readability

- The Campaign road center is rough, near-black asphalt with original tiling
  Base Color, Normal and Metallic/Smoothness maps. It is non-emissive; only the
  narrow Cyan route edges own Bloom.
- Ice is a smooth, non-emissive blue surface laid directly over the same
  sampled Spline ribbon. It reads by gloss, frost and crack detail rather than
  Bloom and may not cut corners on horizontal or vertical curves.
- Fog uses six overlapping low-poly bank sections authored in Blender and
  distributed along a speed-adaptive Spline band. It should read as spatial
  cloud volume rather than one flat curtain; volumetric rendering is excluded.
- An Echo Provider fills the gate opening with the shared animated hex pattern
  in the gate's exact hue. The membrane has no collider and disappears on
  crossing as the same hue appears on the runner shell.
- Human portrait/device review owns asphalt/ice contrast, Fog density and edge
  artifacts, Echo legibility, overdraw, Bloom balance and frame cost.

## Iteration 43 Storm Fog, steering and color strip

- Fog should arrive closer and read as weather, not a translucent prop: ten
  offset low-poly banks sit low over the road while soft capped wisps fill the
  near view, slanted rain crosses the full portrait frame and a scoped color
  adjustment darkens only the active Fog interval. The road, gate and runner
  silhouettes must remain readable; volumetric fog and lightning are excluded.
- Stronger camera inertia may expose more of the runner side/top, but the
  horizon remains upright. The runner visual leans and yaws into the upcoming
  curve while its gameplay root stays aligned to the road.
- The color-order UI belongs at the bottom center, outside the curve-recognition
  corridor. Current is largest, next remains strong on the right, previous is
  quieter on the left, and intermediate colors reduce in scale/alpha without
  becoming a second dominant HUD row.
- Human portrait/device review owns storm density, screen darkness, rain motion,
  curve anticipation, steering feel, color-cycle intuition, overdraw and frame
  time on the target device.

## Iteration 44 Campaign Hidden readability

- Stages 21–23 reuse the existing pooled Hidden presentation: the target begins
  readable, its color and symbol disappear at the approved timing, and the
  neutral `HIDDEN` marker plus ordinary gate silhouette remain visible through
  judgment. No new art, shader, particle layer or screen overlay is added.
- The bottom color-order strip remains visible as a sequence aid but must not
  reveal a hidden gate's target. Human portrait review owns memory-window
  clarity, Stage 20-to-21 onboarding, three-color mastery load and whether the
  persistent marker is legible against Storm Fog and curved-road backgrounds.

## Iteration 45 Gate emblems and text-free Hidden

This section supersedes the Step 9C Unicode shape mapping and Iteration 44's
visible `HIDDEN` marker for current runtime presentation.

- Red, Blue, Green, Yellow, Purple and Cyan each use one distinct transparent
  Theme 1 image emblem. The silhouettes are abstract, non-elemental neon-metal
  marks with a bright center and dark machined edge so they remain recognizable
  on a gate without competing with its colored frame.
- The same sprite is reused in the bottom color-order HUD. Current remains the
  largest center authority, next remains strong on the right and previous is
  quieter on the left. No Unicode circle, square, triangle, star, diamond or
  hexagon is used as the normal gate/HUD identifier.
- Gate emblems occupy the readable opening as a sprite layer integrated with
  the existing frame. Echo and Flicker may retain their concise mechanic marker
  above it; an ordinary or Hidden gate adds no explanatory text.
- Hidden starts with the full emblem, then fades it to transparent while
  reducing its scale to `0.72` over the authored transition. The gate frame
  simultaneously becomes neutral and finishes on the same neutral material
  used by a hidden Camouflage gate, rather than retaining a color-emissive
  material with only its base tint overridden. Geometry, opening, collider and
  judgment pose remain stable, so the user experiences erased target
  information rather than a disappearing obstacle or a tutorial label.
- Elemental fire/ice/vine identities, per-color particles, new gate models and
  stage-theme coupling remain deferred. Human portrait/device review owns
  emblem distinction, integration with the gate frame, Hidden memory pressure,
  Bloom balance and recognition during curves and Storm Fog.

## Iteration 45B Hidden neon power loss

- Hidden should feel like a neon gate losing power, not a UI element fading.
  Within the existing short erase duration, color and emission drop sharply,
  recover twice at weaker strength, flicker once more and then stay off.
- The emblem shares the exact light-strength envelope so it never contradicts
  the frame. Its silhouette still shrinks smoothly toward `0.72`, preserving a
  single directional motion underneath the electrical flicker.
- Recovery never flashes white or exceeds the assigned gate color. The final
  silhouette remains the non-target neutral gate. Audio, particles and
  full-screen effects remain excluded.

## Iteration 46 Flicker neon wipe

- A Flicker transition reads as the next neon color filling the gate opening
  from left to right, like a short gauge. The existing frame and emblem retain
  the current color while a collider-free inner wipe reveals the next color.
- The wipe uses the existing protection-field shader/material family and a
  fixed pooled quad. It grows only in width, never allocates during play and
  disappears outside the `0.12s` transition.
- Flicker shows no label or mechanic marker. Motion and the two simultaneous
  neon colors must communicate the state without tutorial copy.
- Human portrait review owns wipe direction/readability under Bloom, color
  distinction at maximum speed, and whether the fixed quad feels integrated
  with the gate rather than like a detached UI overlay.

## Iteration 47 Flicker frame trace and emblem transform

This section supersedes Iteration 46's inner-quad wipe presentation.

- The moving next color belongs to the gate's U-shaped neon channel, not the
  opening. It rises along the left pillar, crosses the top and descends the
  right pillar with rounded joins and caps. The path is additive/HDR,
  collider-free and pooled.
- The center emblem uses two coincident sprite layers and a complementary
  procedural-noise dissolve. A narrow next-color emissive boundary makes the
  replacement read as one continuous transformation; there is no intentional
  fully blank beat between symbols.
- No explanatory text, particles, audio, elemental identity or full-screen
  effect is added. Human portrait/mobile/WebGL review owns line alignment,
  line width, Bloom strength, aliasing and `0.12s` readability.

## Iteration 48 Mesh-bound Flicker dissolve

This section supersedes Iteration 47's separate additive frame trace.

- Flicker color replacement remains attached to the authored neon frame. The
  existing `LeftNeon`, `TopNeon` and `RightNeon` meshes dissolve from old HDR
  color to new HDR color using their own local bounds in the approved left-up,
  top-right and right-down order.
- Do not add a screen-aligned line, opening fill or duplicate model for this
  transition. The mesh stays opaque while its emissive color is replaced, with
  only a restrained procedural edge reacting to Bloom.
- Magenta Unity error-material output is never an acceptable fallback. Both
  Flicker shaders require graphics-platform compiler-message validation.
