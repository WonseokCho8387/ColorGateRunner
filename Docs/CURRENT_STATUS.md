# Current Status

## Authoritative Baseline

This section is the single current baseline for routine Codex iterations.
Historical counts and hashes later in this file remain evidence for their own
iterations but do not override this section.

### Repository

- Implementation base HEAD: `db8d21219a05885d8020a3333731c04f26810c70`
- Base commit: `feat: add hidden stages and refresh test builds`
- Authoritative completion HEAD: the commit named
  `feat: improve hidden readability and color emblems`; its exact hash is
  recorded in the Iteration 45 final report because a commit cannot contain
  its own content-derived hash.
- Branch at completion: `main`
- Expected working tree: clean
- Push status at completion: not pushed

### Current product state

Iteration 23 supersedes the active Iteration 22 gameplay/economy baseline
below where their contracts differ.

- Current completed iteration: **Iteration 45 — Hidden Feel and Color
  Emblems**.
- The development Experiment panel now exposes `SPLINE TRACK LAB`. It runs a
  separate horizontal S-curve with distance-to-Spline runner, camera, gate and
  Goal poses plus a generated dark-alloy/Cyan road mesh. Restart and Exit remain
  Lab-local and non-persistent.
- All 23 active Campaign Stages use one scalar-distance Spline route rather
  than the legacy recycled straight-track placement. The same evaluated pose
  owns runner, chase camera, pooled gates, Goal, Fog curtain and preplaced Ice
  runway. Stages 1–7 introduce restrained horizontal curvature, Stages 8–13
  broaden it and begin gentle elevation, and Stages 14–23 increase both within
  a stable world-up/no-roll portrait camera contract.
- The Campaign chase camera follows the current Spline rotation with a `0.3s`
  half-life and bounded `24°` yaw / `16°` pitch lag. It never banks or rolls,
  snaps at Stage start, Retry and Continue, and freezes with Pause or failure.
- The runner root, Rigidbody and judgment pose remain exact Spline consumers.
  A presentation-only visual child anticipates the upcoming tangent with a
  bounded `10°` yaw and `6°` lean so the vehicle visibly steers through turns
  without creating a second movement authority.
- Campaign road presentation uses original rough, non-emissive asphalt maps
  while retaining the Cyan emissive edge. Ice uses a fixed 50-slot pool of
  runtime Spline-sampled ribbon meshes with original smooth, non-emissive ice
  maps, so active intervals follow the same horizontal and vertical route pose.
- Campaign Fog presents ten fixed Blender-authored low-poly banks across a
  closer speed-adaptive 18–32 unit Spline band. A fixed 24-wisp / 96-rain
  camera-following particle budget and a Fog-scoped darker color-adjustment
  volume share the authored `0.5s` fade-in, Stage-owned `5 / 6 / 7s` hold and
  `0.5s` fade-out lifecycle. No volumetric rendering is introduced.
- Every pooled gate owns one inactive, collider-free Echo membrane using the
  shared protection-field hex shader. Only a visible unresolved Echo Provider
  exposes it in its assigned color; crossing hides it and transfers that exact
  hue to the player Echo shell.
- Campaign track presentation is one generated seven-unit two-submesh road for
  the full attempt. The legacy six-segment straight pool remains available only
  to existing Experiment paths and is inactive during Campaign play.
- The 24 imported Theme 1 city body/glow pairs form a deterministic fixed pool.
  They follow the Spline outside a 7.5-unit minimum lateral clearance, recycle
  behind the runner without allocation and continue through the Goal buffer.
- Unity Editor exposes `Tools > Color Gate Runner > Developer Console`.
  It can open any authored Stage as a non-persistent cheat launch, unlock
  Campaign through a selected Stage, reset Campaign separately from Economy,
  and set/reset Coins, Shields, Boosters, Continue Tickets, Hearts and timed
  unlimited Hearts. Edit Mode writes the same local Product save before Play;
  Play Mode uses the active AppRoot session and refreshes visible Product UI.
  The tool is Editor-only and is absent from player builds.
- Unity IAP `5.4.2` and its Unity Services Core `1.18.0` dependency are
  installed. Android billing mode is Google Play and the Android application
  ID is `com.wscho.colorgaterunner`.
- The package baseline does not yet initialize a store, fetch products,
  process orders, validate receipts, grant paid rewards, or display a Shop.
  Firebase exists only as a console project; no Firebase Unity SDK or mobile
  configuration file is present in the repository.
- Runtime flow: `Boot(0) -> Frontend(1) -> SampleScene/Campaign(2)`.
- Frontend contains first-run Account Choice and the consolidated Campaign
  Lobby. It does not contain an AppRoot and reuses the Boot-created persistent
  AppRoot.
- `LocalProfileData.AccountChoiceCompleted` owns onboarding completion.
  Returning users enter Lobby directly; schema-1 saves without the field
  intentionally see Account Choice once.
- Google integration remains absent, its release action is hidden, and no
  linked state is simulated.
- Frontend and Gameplay Pause share Notifications, Music, SFX, Vibration, and
  configured-link Settings. Master Volume remains hidden and applies through
  `AudioListener.volume`; Music and SFX persist without a current content-
  specific target. Vibration gates the existing Booster haptic call.
- Frontend reads Campaign progression through the schema-2 Product save and
  queues one non-persistent stable-ID launch request. Production entry opens
  the existing PreRun/item selection without briefly showing Campaign Lobby;
  direct/development/fallback entry retains that Lobby.
- Campaign records the non-persistent Frontend entry origin only after it
  consumes that launch request. PreRun Back returns that path directly to the
  Frontend Lobby; direct/development entry still returns to Campaign Lobby.
- Every Settings toggle uses one `StateLabel`, showing exactly one of `ON` or
  `OFF`. Frontend and Pause share the same immediate synchronization path.
- Gameplay Pause freezes attempt time, movement, input, judgment, recycling,
  mechanic timing, camera feedback, and registered attempt VFX. Resume,
  existing Retry, Settings, and confirmed Frontend Lobby return are available.
- Product save schema 3 owns Profile, Settings, stable-ID Campaign records,
  Economy inventory, Hearts, timed unlimited Hearts, Continue Tickets,
  transaction IDs, Starter entitlement and Lobby milestone presentation.
- On the first production launch after upgrade, legacy device-wide Campaign
  PlayerPrefs are imported once. Legacy keys remain for rollback safety, but
  Product save becomes the runtime write authority.
- First clear grants `100 / 200 / 500` Coins for Normal / Hard / Very Hard
  difficulty. Stages 11, 14, 17 and 23 are Hard; Stage 20 is Very Hard; the other
  authored Stages are Normal. Every even first-cleared Stage up
  to Stage 36 applies one automatic Lobby milestone and an idempotent reward.
  The starter reward policy grants 200 Coins, or 300 at every third milestone,
  plus alternating Shield/Booster inventory.
- Frontend Lobby uses three clear zones: persistent Profile / Coin / Heart /
  Settings controls at the top, the current Lobby theme and upgrade progress
  as the dominant center, and one compact Stage card with one primary `PLAY`
  action at the bottom. Shield and Booster stock remain compact secondary
  chips. Duplicate `LOBBY` and account labels are removed; milestone rewards
  appear only as a pending banner. Hearts show `5/5`, next-recharge time, or
  timed-unlimited remaining time from Product state. Frontend-origin result
  actions return there; direct/development entry keeps Campaign Lobby fallback.
- Theme 1, `COLOR COURTYARD`, now uses original portrait artwork in three
  Builder-owned layers: a dark futuristic courtyard background, a central
  color-energy reactor and a transparent ambient energy frame. Six automatic
  Lobby milestones light six surrounding energy nodes. The presentation uses
  only a restrained unscaled-time pulse, never blocks UI input and does not
  change the top wallet, Stage card or `PLAY` navigation. Theme 2 and 3 retain
  palette fallbacks until their own art slices are approved.
- Theme 1 Campaign presentation now uses Blender-authored FBX art for the
  spherical cyber vehicle, modular Left / Right / Top gate, neon track, city
  backdrop and Goal portal. Editable `.blend` source and Runner / Gate /
  Environment UV/PBR map families are checked in. A fixed six-view,
  five-fragment-per-view deterministic pool shows a `0.3s` gate break without
  physics allocation or gameplay authority. The existing URP profile enables
  restrained four-iteration Bloom and the Builder enables camera
  post-processing. Gate/Track pool sizes, collision, judgment, Stage data and
  balance are unchanged.
- The Theme 1 runner now reads from the chase camera as a spherical cyber
  vehicle with a smooth non-emissive glass-like hull that follows the current
  runner color, plus an HDR current-color rear panel, wide bumper, side fins,
  twin exhausts, twin chevrons and a rear light bar. Generated gameplay colors
  use HDR emission while the glass hull and Track dark alloy remain
  non-emissive. The
  existing four-iteration Bloom profile uses threshold `0.8`, intensity
  `0.85` and scatter `0.58`; no render package or ProjectSettings changed.
- The gameplay color-order HUD is a bottom-center horizontal strip. Current
  color is centered, next is emphasized to the right and previous is reduced
  to the left; two-color stages use a balanced current/next pair and additional
  colors reuse the same fixed slots without covering left-hand curves.
- Gates and the bottom color-order HUD now share six distinct transparent,
  non-elemental Theme 1 image emblems. The former Unicode shape glyphs are no
  longer used by normal runtime presentation. Echo and Flicker retain their
  concise mechanic markers, while Hidden has no explanatory marker or text.
- Frontend and Campaign now share an original Theme 1 UI skin: seven genuine-
  alpha 9-slice panel/button/chip sprites and ten semantic resource/action
  icons. Builder-created panels retain their existing hierarchy and text;
  buttons gain primary, secondary and danger surfaces plus distinct hover,
  press, selected and disabled states. Coin/Heart, Play, Settings, Back,
  Pause, Shield, Booster, Continue and Retry cues are non-blocking Images.
  Navigation, Product state, economy values and gameplay authority are
  unchanged.
- Theme 1 FBX instances now discard the importer-supplied `X -90°` root
  rotation and use normalized local position, rotation and scale. The Track
  therefore remains approximately 7 units wide, below 3 units high and 40
  units long along gameplay Z instead of standing 40 units vertically. The
  same correction keeps the runner, gates, Goal portal and city in their
  authored Unity-axis orientation. Collision, Track recycling and Stage
  coordinates remain owned by their existing non-art parents.
- Each pooled 40-unit Track segment now owns matching start/end anchors at
  local Z `-20 / +20`. Placement aligns the start anchor, rather than the
  imported-art parent pivot, to the requested world start. Reset and recycling
  therefore keep all six fixed segments end-to-start continuous beyond the
  authored Stage distance without pool growth, overlap or a disappearing road.
- Shield and Echo share one original transparent spherical protection-field
  presentation with a procedural hex grid, Fresnel rim, restrained motion and
  Bloom-reactive emission. Normal Shield is fixed Cyan/Electric Blue; held Echo
  uses only its exact stored runner hue through a property block. Consumption
  emits one short outward pulse in that same exact field hue before the surface
  disappears. The URP shader uses an explicit shader-model target, stable
  branch-free hex-cell math and instancing/stereo setup; EditMode and the
  Campaign Builder reject compiler errors instead of allowing a pink error
  fallback. An attempt
  that starts with a selected or stage-provided normal Shield generates no Echo
  Provider gates for that entire attempt in Campaign or Experiment, including
  after Shield consumption, Continue or Retry with the retained selection.
- Booster presentation is a runner-aligned 3D Warp corridor. Its non-emitting
  controller follows the Player, while Cyan and Gold stretched particles emit
  through enlarged forward box volumes, including the upper portrait frame,
  and remain in World Space so depth and parallax read along the travel
  direction instead of sticking to screen center. Both layers prewarm. The
  two-ended lifetime fade and hard 160-particle emitted capacity remain. The
  normal chase camera is closer at `(0, 7, -8.2)` / `18°`; Booster blends to
  `(0, 5, -6.5)` / `13°` while preserving its existing FOV contract. It reuses
  the existing
  Booster state, distance, FOV, warning and pause ownership and allocates no
  runtime emitter.
- Campaign contains 23 stable-ID Stages. Stages 1–5 remain the color/rhythm
  foundation; Stage 6 provides Shield and Stage 7 provides Booster while
  selection stays locked; Stage 8 is the first clean three-color Stage with
  selectable items. Stages 9–20 teach Camouflage, Fog, Ice and Echo in
  three-Stage intro/practice/mastery blocks using 2/2/3 colors. Stages 21–23
  now teach Hidden recall with the same 2/2/3-color block shape and isolated
  Hidden modifier. Flicker remains available in Experiment Lab and is deferred
  from Campaign until planned Stages 24–26.
- Hidden Stages 21–23 now place `7 / 9 / 11` deterministic occurrences across
  the full run. Their `1.30 / 1.15 / 1.00s` hide leads and shared `0.18s`
  emblem fade/contract transition make the intro readable while increasing
  repetition and later-run memory pressure. Judgment and simulation authority
  remain unchanged.
- Existing saves that cleared Stage 20 and still store Stage 20 as their
  highest unlocked Stage resolve Stage 21 as effectively unlocked after the
  catalog expansion. Loading does not rewrite the Product save or legacy
  PlayerPrefs; a later ordinary progression write persists the new stable ID.
- From Stage 8 onward, selecting Shield or Booster consumes one owned unit of
  each selected item in one atomic Product save when `START` succeeds, before
  Countdown. At zero stock the item button offers one Shield or Booster for
  `900` Coins through a blocking confirmation modal. A successful purchase is
  atomic, transaction-ID idempotent, grants exactly one item and selects it;
  `START` then consumes it through the existing attempt authorization. Low
  balance shows the exact shortage, while save failure spends and grants
  nothing. Insufficient current stock or start-save failure keeps PreRun open
  with truthful status and publishes no partial spend. Retry is a new attempt
  and consumes again; Back before Start and duplicate Start do not consume.
  Stage 6/7 provided training items remain free and do not consume inventory.
  Without a ready Product session, selectable items and quick buy are
  unavailable while no-item and provided-item starts remain valid.
- Coin Continues cost `900`, `1,900`, `2,900`, then `4,900` Coins for every
  later successful Coin use in the same attempt. There is no attempt-wide
  Continue-count cap; a finite Stage and available authorization sources
  remain the practical bounds. One completed
  rewarded-ad Continue is available per attempt when a real provider reports
  availability; Coin use does not consume that right and ad-first does not
  raise the first Coin price. Coin spending is atomic and idempotent before
  Core resumes. Insufficient funds, save failure, failed/cancelled ads and
  stale callbacks keep the failure state frozen. Retry creates a fresh policy.
  The current release provider is unavailable, so the ad action is
  intentionally hidden and never simulated as successful. Failure UI shows
  the current Coin and Heart wallet. Insufficient Coin selection opens a
  truthful popup while keeping the failed run frozen; the popup does not
  pretend the still-deferred Shop exists.
- A normal Stage start consumes one Heart atomically with selected start items.
  Hearts cap at 5 and recover one every 30 minutes, including offline;
  backwards clock movement cannot accelerate recovery. Unlimited Hearts
  suppress consumption until UTC expiry and purchased durations stack. A
  stored Heart actually consumed for an attempt is refunded atomically when
  that attempt clears, so a successful run has zero net Heart cost while
  failure, quit, or restart retains the spend.
- Initial Stage start retains `3, 2, 1, GO`. Continue instead shows `READY` for
  `0.5s`, resumes gameplay when `GO` appears and removes the non-blocking `GO`
  flash after `0.25s`. Active gate modifiers are already rendered and consumed
  Shield/Booster state is already absent before READY. Campaign Clear hides Replay,
  separates first-clear and Lobby milestone rewards, and opens the unlocked
  next Stage directly in PreRun. Final or still-locked next Stages hide that
  action.
- Campaign Stages 12–14 no longer neutralize all but two Fog gates. When the
  first Fog gate becomes the next judgment, one world-space storm bank appears
  at 0.9 seconds of current travel distance ahead of the player. It fades from
  alpha 0 to 1 over 0.5 seconds, stays fully opaque for an authored 5 / 6 / 7
  seconds across the intro / practice / mastery curve, then fades to 0 over
  0.5 seconds. It cannot retrigger in that attempt. Pause and Continue
  countdown freeze it; Continue preserves its state and Retry resets it.
  Experiment Lab retains its historical nearest-two Fog comparison contract.
- Campaign Stages 15–17 preplace every authored Ice approach through one fixed
  50-panel runway pool before Countdown. Ice panels span the deterministic
  approach interval into each Ice gate, while the six recycled normal track
  segments keep their normal material. Campaign Ice multiplies non-Booster
  speed by an authored `2.0`; Booster remains authoritative while active.
  Continue preserves the same runway and Retry rebuilds it deterministically.
  Experiment Lab intentionally retains its historical whole-track `1.45`
  comparison behavior.
- Authored Ice gates now prefer one forward tap. Stage 15 and 16 require one
  tap on every Ice gate; Stage 17 requires one tap on 14 of 15 Ice gates and
  two taps on one deterministic gate (`6.67%`). Ice gates never require zero
  or more than two authored taps. Retry reproduces the same quota and colors.
- The local catalog owns six Coin packs and five bundles. Grants are atomic
  and order-ID idempotent; Starter is locally account-limited. Continue
  Tickets are offered before real ads and Coins. Store connection, receipt
  validation, Shop UI, Firebase integration and a real rewarded-ad provider
  remain deferred.

### Automated validation

- EditMode: `450/450`
- PlayMode: `249/249`
- Post-Builder PlayMode: `249/249`
- Frontend Scene Builder: two consecutive passes completed; Boot and Frontend
  generated references, UI skin, unique roots, EventSystem and Build Settings
  passed
- Campaign Builder: two consecutive Iteration 45 passes completed; the active
  Campaign Spline path, generated full-route road, deterministic 24-slot city
  pool, ten-bank Fog band, capped weather particles and scoped tone volume,
  50-slot Spline Ice mesh pool and six Echo membranes passed alongside the
  Spline Lab, bounded camera/runner steering, adaptive color strip, quick
  Continue, quick-buy/VFX contracts, fixed gate/legacy track pools and Build Settings
- Stage Catalog Builder: revision 12 Resource contains 23 valid stages
- Missing Script / Missing Reference / duplicate generated object failures:
  none

### Build Settings

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/Frontend.unity`
3. `Assets/Scenes/SampleScene.unity`

All three entries are expected to be enabled and unique.

### Developer save snapshot

- Schema 3, revision 501, highest unlocked Stage ID `stage-01` with no records,
  0 Coins, 0 Shields, 0 Boosters and 4 Hearts.
- SHA-256:
  `29FBE57FD5BE028EAEEEF75EC6DD5B2DD3208DBCB533AD33C9905EC373CC9D20`.
- This user-authored reset/attempt state predates Iteration 43 validation.
  Its `LastWriteUtc` remained `2026-08-27T01:45:18.7882931Z`; automated
  validation did not restore or mutate this file. Developer Console
  mutations still occur only after an explicit Apply, Reset, or Unlock action.

### Campaign simulation baseline

- Rows: `460`
- Summary SHA-256:
  `4E3B9ACE68ABDFF9540F9A26D7085C664C1658DAC133D73AE7E87BE3A5A1D07B`
- JSON SHA-256:
  `3C3233C5CA3DA50B1272ADE1FE451D718CAA4E148598C81220881530B942A68D`
- CSV SHA-256:
  `9CFB8DE0CFC54483E518F91290F998FEF57CBC5E0722CC4A0B2BDC744EF43611`
- Two complete runs are byte-identical. Continue-use metrics are deterministic
  and bounded by each finite Stage. Continuity and pool-violation counters are
  zero.
- The 60 new rows cover Stages 21–23 across every standard profile and start-
  item combination. Existing Stage 1–20 rows retain their prior behavior.
- Step 10 was rerun twice. Its 80-row, 64,016-run artifacts remain byte-exact
  to the baseline below.

### Step 10 simulation baseline

- Report rows: `80`
- Simulation runs: `64,016`
- CSV SHA-256:
  `04FB1F0395EED309A78B78DCF89882A33DE75220022DCE235FE143FDC0D75C04`
- JSON SHA-256:
  `5FD03691B938388B8AE772D9D3F935303539A871111B548188FDB7CDF52D0C00`
- Summary SHA-256:
  `611CCFF9AD680BBBD9FEF903AA149E1075E87C34FE8CB91DC33BB47286943A27`
- Comparison SHA-256:
  `00B5B102FE9FD9684E46E10B73C006F493C6FB80A2114C30DC5691AC0BBDA37B`
- Shortlist SHA-256:
  `068334D359126233F454AD031E617047DDF48D8C30C095683DBE8FF866F0CCE6`

### Persistence safety baseline

- Campaign PlayerPrefs rollback snapshot contains Highest Unlocked plus Stage
  Record keys 1–23 when present and must be restored exactly after tests.
- Actual Editor snapshot at Iteration 12 completion:
  - existing Campaign entries: `12`
  - canonical SHA-256:
    `B57BDC93138A3E1C376272C33FB042274E821B8BB9C0EC12A33B9480010C73CD`
- The Editor Product save is schema 2, revision 23, profile/Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, with Highest Stage 13, 13 Stage
  records, 2,600 Coins, 4 Shields and 4 Boosters. Its SHA-256 is
  `39FB23837EE260E8F1B3CF7E1EF7EBE6389DC689A59CC3E68B6164516E180A18`
  and `LastWriteUtc` is `2026-08-11T03:21:03.1535325Z`. The hash and values
  were identical after Iteration 18 validation. Iteration 18 retained schema
  2 and did not migrate or rewrite the actual Product save.
- Values deleted before Iteration 11 are unknown and must not be guessed.

### Package and ProjectSettings baseline

- `Packages/manifest.json` SHA-256:
  `5E44864DB10C6A0A47806035C801D2271F6F48B0BF8BF6F699B88C71D6758C96`
- `Packages/packages-lock.json` SHA-256:
  `FF4B3AC486B926719707FF2447580CEB1CE33D7C0851BABA229A3CD73C48CBBE`
- `ProjectSettings/ProjectSettings.asset` SHA-256:
  `BFF9843B9363FF1C20B113108D623026E777311CAEF6372B07C92690049717BC`
- Approved package delta: Unity IAP `5.4.2`, Unity Services Core `1.18.0`,
  Unity Splines `2.9.0` with its Settings Manager dependency, and
  `Assets/Resources/BillingMode.json` with Google Play. The incidental
  Navigation, Rider, Visual Studio and Visual Scripting upgrades made during
  installation were removed and remain at their prior approved versions.
- Approved ProjectSettings delta: Android application ID
  `com.wscho.colorgaterunner`. Company name and non-Android identifiers are
  unchanged so the existing Editor save location is preserved.
- `ProjectSettings/ProjectSettings.asset` must retain the approved portrait,
  custom WebGL template, Input Actions preload, and all other semantic
  baselines.
- Package-managed WebGL defines `APP_UI_EDITOR_ONLY` and
  `SENTIS_ANALYTICS_ENABLED` may vary by environment in presence, combination,
  or order. They are never staged in a feature commit. Any other scripting
  define or ProjectSettings semantic difference is a baseline failure.
- Routine iterations must not redefine these values. Any intended Package or
  ProjectSettings change requires explicit approval and a new documented
  baseline.

## Latest Iteration Result and History

## Iteration 45 validation result

- Starting HEAD `db8d212` was clean. Completion commit name:
  `feat: improve hidden readability and color emblems`.
- Catalog revision 12 keeps 23 stable Stages and redistributes Hidden across
  the whole of Stages 21–23 with `7 / 9 / 11` occurrences. Stage-local leads
  are `1.30 / 1.15 / 1.00s` and the shared transition is `0.18s`; ordinary
  target-color judgment, seeds, gate counts, speed, cadence, rewards and save
  contracts remain unchanged.
- Six original transparent Theme 1 emblem sprites replace Unicode shapes on
  pooled gates and the bottom color-order HUD. Hidden now communicates only by
  fading/contracting that emblem and neutralizing the frame; no `HIDDEN` text
  or instructional label remains. Elemental gate theming stays deferred.
- Android Gradle alignment now implements its post-generation callback on
  every Editor start target, rather than compiling the callback only while
  Android is already active. The real combined Android-plus-WebGL command
  therefore succeeds when launched from WebGL.
- Focused Hidden EditMode passed `10/10`; focused Hidden PlayMode passed `7/7`
  plus the live Campaign approach case `1/1`. WebGL-start build-tool EditMode
  passed `7/7`. Campaign Builder completed consecutive passes; final full
  EditMode passed `450/450` and final post-Builder PlayMode passed `249/249`.
- Two 460-row Campaign simulations retained Summary
  `4E3B9ACE68ABDFF9540F9A26D7085C664C1658DAC133D73AE7E87BE3A5A1D07B`,
  JSON `3C3233C5CA3DA50B1272ADE1FE451D718CAA4E148598C81220881530B942A68D`
  and CSV `9CFB8DE0CFC54483E518F91290F998FEF57CBC5E0722CC4A0B2BDC744EF43611`.
  Two Step 10 runs retained all five authoritative hashes.
- The real combined Development build produced a fresh 68,774,305-byte APK
  and a 155,566,553-byte WebGL folder with index, data, framework, loader and
  WASM. Its canvas remains `540 x 960` and exact `9:16`.
- Package files, tracked ProjectSettings and the developer Product save retain
  their approved baselines. Installation, touch/browser behavior, emblem
  distinction and Hidden feel remain human review.

## Iteration 44 validation result

- Starting HEAD `0318ba1` was clean. Completion commit name:
  `feat: add hidden stages and refresh test builds`.
- Catalog revision 11 adds stable Stages 21–23 as Hidden intro, practice and
  mastery with 2 / 2 / 3 active colors. Stage 23 is Hard. Hidden uses the
  existing deterministic modifier, pooled presentation and ordinary judgment;
  no new runtime authority or save schema was introduced.
- Catalog-expansion repair makes Stage 21 effectively available to saves that
  cleared Stage 20 without mutating them on load. Focused runtime coverage
  verifies Campaign Hidden hides only its target while retaining the `HIDDEN`
  marker and ordinary judgment.
- Test Build commands now refresh Stage Catalog, Campaign, Frontend and Boot
  immediately before packaging and delete the previous platform output first.
  Android additionally aligns Kotlin stdlib dependencies in generated Gradle
  projects, including upgrading a cached legacy insertion safely.
- Campaign Builder completed two consecutive passes. Full EditMode passed
  `449/449`; final PlayMode passed `248/248`. Two Campaign simulations produced
  460 byte-identical rows with the authoritative hashes above; two Step 10
  simulations retained all five 80-row / 64,016-run hashes.
- A real menu-driven Android Development build produced a 64,957,676-byte APK.
  A real menu-driven WebGL Development build produced a 156,006,568-byte folder
  with `index.html`, data, framework, loader and WASM; its canvas is `540 x 960`
  with an exact `9:16` frame. Installation, browser interaction and device
  performance remain human acceptance work.
- Package files and tracked ProjectSettings have no diff. Build output and
  Unity-generated temporary changes remain untracked/ignored and unstaged.

## Iteration 43 validation result

- Starting HEAD `f507692` was clean after the allowed package-managed define
  churn and unrelated Editor material refreshes were restored. Completion
  commit name: `feat: refine fog weather and curve readability`.
- Fog now combines ten closer fixed banks, bounded world-space wisps and rain,
  and a scoped darker tone under the existing one-shot lifecycle. Camera lag
  expands to `24° / 16°` with a `0.3s` half-life; runner visual steering remains
  independent of the exact Spline root. The color sequence is bottom-centered
  and horizontal with current / next / previous emphasis.
- The affected Graybox PlayMode group passed `156/156`. Campaign Builder
  completed two consecutive passes. Full EditMode passed `444/444`; final
  post-Builder PlayMode passed `246/246`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package files are byte-exact and ProjectSettings is
  at the tracked semantic baseline with no staged change. The developer save
  remained byte- and timestamp-exact during validation.
- Human portrait/device play still owns Fog/rain density, dark-tone comfort,
  camera lag strength, runner steering feel, bottom-strip intuition, overdraw
  and target-mobile performance.

## Iteration 42 validation result

- Starting HEAD `ec50719` was clean. Completion commit name:
  `feat: polish campaign mechanic surfaces`.
- Campaign road and Ice now use separate original mapped non-emissive
  materials; each active fixed Ice slot samples its deterministic interval from
  the Campaign Spline. Fog uses six fixed Blender banks across an adaptive path
  band, and each gate owns one shared-shader Echo membrane that transfers its
  exact assigned color before hiding.
- The affected Graybox PlayMode group passed `154/154`. Campaign Builder
  completed two consecutive passes. Full EditMode passed `444/444`; final
  post-Builder PlayMode passed `244/244`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package and ProjectSettings hashes remained exact;
  isolated validation did not mutate the original Editor Product save.
- Human portrait/device play still owns Fog density/seams, surface contrast,
  Echo transfer readability, overdraw, Bloom balance and mobile performance.

## Iteration 41 validation result

- Starting HEAD `f1cc9d1` was clean. Completion commit name:
  `feat: add spline camera inertia and quick continue`.
- Campaign camera rotation follows the Spline with a `0.2s` half-life, bounded
  yaw/pitch lag and a hard no-roll reconstruction. Stage start, Retry and
  Continue snap the follow basis; Pause and failure freeze it.
- Continue now uses a frozen `READY` presentation for `0.5s`, begins gameplay
  when `GO` appears and removes the non-blocking GO flash after `0.25s`.
  Initial Stage start remains the existing three-second countdown.
- Focused quick-Continue PlayMode passed `17/17`; focused curved-camera and
  Booster-return tests passed `1/1` each. Campaign Builder completed two
  consecutive passes. Full EditMode passed `444/444`; final post-Builder
  PlayMode passed `244/244`.
- Campaign and Step 10 simulations ran twice and retained every authoritative
  artifact hash exactly. Package and ProjectSettings hashes remained exact;
  isolated validation did not mutate the original Editor Product save.
- Human portrait/device play still owns the perceived lag strength, motion
  comfort, gate visibility on crests and the READY/GO pacing.

## Iteration 40 validation result

- Starting HEAD `db7443336a4533aa4bca72f20bea1bd8848af93f` was clean.
  Completion commit name: `feat: convert campaign to spline routes and city
  pool`.
- All active Campaign Stages now map scalar progress to one Spline pose. Runner,
  camera, pooled gates, Goal, Fog, Ice and Continue share that authority; the
  generated full-route road replaces the legacy straight pool during Campaign.
- The 24 existing Theme 1 body/glow pairs now form one deterministic fixed city
  pool with at least 7.5 units of lateral road clearance and coverage through
  the Goal buffer.
- Focused Spline/city PlayMode passed `1/1`; full EditMode passed `444/444` and
  full post-Builder PlayMode passed `242/242`. Campaign Builder completed two
  consecutive passes with valid references, unique roots, EventSystem and Build
  Settings.
- Two Campaign simulations retained all three approved 400-row hashes. Two
  Step 10 simulations retained all five approved 80-row / 64,016-run hashes.
  Package manifest, package lock and `ProjectSettings.asset` hashes remain
  exact; isolated validation did not mutate the original Editor Product save.
- Human portrait/device review owns curve anticipation, gate readability on
  slopes, Fog/Ice visibility, Continue comfort, city density/pop, stable
  horizon, Bloom and mobile performance. Banking, loops, inversions, Race Mode,
  Stages 21–36 and new city art remain deferred.

## Iteration 39 validation result

- Starting HEAD `56c2c11` was clean. Completion commit name:
  `feat: add start-item quick buy and refine vfx`.
- A zero-stock selectable Shield or Booster now opens one blocking PreRun
  confirmation for a fixed `900` Coins. Product grants one item and charges
  once through an atomic, idempotent ledger transaction; success auto-selects
  the item, insufficient balance reports the shortage, and save failure
  publishes no spend or grant.
- The D3D11-reserved protection-shader identifier was replaced and the field
  roots were centered `0.85` units above the Player origin. Booster Cyan/Gold
  world-space boxes were enlarged and raised, both prewarm, and normal/Booster
  chase framing moved closer while keeping Booster rules, timing, FOV, pause
  behavior and the 160-particle cap unchanged.
- Focused Product EditMode passed `62/62`; focused Campaign PlayMode passed
  `152/152`. Campaign Builder completed two consecutive passes. Full EditMode
  passed `444/444`; full post-Builder PlayMode passed `241/241`. The protection
  shader also passed its focused compiler test `1/1` on an actual D3D11 AMD
  Radeon Vega 8 device with no shader error.
- Package and ProjectSettings hashes remain exact. Validation used an isolated
  project copy and did not mutate the original Editor Product save. Campaign
  and Step 10 simulations were omitted under Tier 2 because Core rules, Stage
  data, deterministic generation, timing, judgment, balance and Experiment
  inputs are unchanged.
- Human portrait/device review owns the modal hierarchy and wording, Shield
  centering/transparency, Warp coverage above and beside the runner, closer
  camera comfort, gate visibility, Bloom and mobile frame cost.
- This Iteration 39 roadmap note is historical and is superseded by Iteration
  40's whole-Campaign Spline conversion. Multi-lane Ghost AI Race Mode remains
  a later roadmap item and is not implemented.

## Iteration 38 validation result

- Starting HEAD `047a8fa` was clean. Completion commit name:
  `fix: restore world-space protection effects`.
- The protection-field material reference was intact, but human play exposed a
  pink shader fallback that name-only validation could not detect. The shader
  now uses URP-compatible branch-free hex math, explicit shader target and
  instancing/stereo setup; Builder and EditMode inspect compiler messages.
- The former camera-local radius-five Warp was the direct cause of the flat
  screen-center presentation. Its existing controller now follows the Player,
  while two Cyan/Gold forward box volumes simulate in World Space and stream
  backward along the runner direction. Booster rules, FOV, warning, pause,
  timing and the 160-particle cap are unchanged.
- Focused protection shader EditMode passed `1/1`. Campaign Builder completed
  two consecutive passes. Full EditMode passed `440/440`; full post-Builder
  PlayMode passed `237/237`.
- Package and ProjectSettings hashes remain exact. Validation used an isolated
  project copy and did not touch the original Editor Product save. Campaign and
  Step 10 simulations were omitted because no Core rule, Stage data,
  deterministic generation, timing, judgment, balance or Experiment input
  changed.
- Human portrait/device review owns final Shield transparency/hex readability,
  absence of pink fallback, Warp corridor depth, center readability, Bloom and
  mobile frame cost.

## Iteration 37 validation result

- Starting HEAD `ff3cc01` was clean. Completion commit name:
  `feat: add spline track lab`.
- Unity Splines `2.9.0` supplies one Builder-authored horizontal S-curve. A
  shared distance-to-path view evaluates runner, yaw-only chase camera, pooled
  gates and Goal poses; the Builder generates a dark road with Cyan edge
  submesh from the same curve.
- The Experiment panel exposes a development-only Lab entry with its own HUD,
  Restart and Exit. Lab failure/clear state is non-persistent. Exit restores
  the straight Campaign track and resets pooled gate positions and rotations.
- Focused EditMode passed `1/1`, focused PlayMode passed `2/2`, Campaign Builder
  passed twice, full EditMode passed `439/439`, and full post-Builder PlayMode
  passed `237/237`.
- ProjectSettings and the actual developer save were not changed. Campaign and
  Step 10 simulations were omitted because Core rules, authored Stage data,
  deterministic generation, timing, judgment, balance and Experiment inputs
  are unchanged.
- Human portrait review owns S-curve readability, camera comfort, road-edge
  Bloom and the transition back to the straight Campaign.

## Iteration 36 follow-up validation result

- Starting HEAD `91f17e8` was clean. Completion commit name:
  `fix: tint runner glass shell`.
- The whole spherical hull now follows the authoritative runner/failure color
  through one cached material property block while remaining non-emissive and
  highly smooth. The rear panel retains the existing HDR semantic material.
- Campaign Builder completed two consecutive passes. Focused EditMode passed
  `3/3`, focused PlayMode passed `1/1`, full EditMode passed `438/438`, and
  full post-Builder PlayMode passed `235/235`.
- Package and ProjectSettings hashes remain exact. Campaign and Step 10
  simulations were omitted because no gameplay or deterministic input changed.

## Iteration 36 validation result

- Starting HEAD `004e0ec` was clean. Completion commit name:
  `feat: improve runner readability and bloom`.
- Blender source and the Runner FBX now provide a dark ball-car body with a
  clearly authored rear silhouette: bumper, side fins, twin thrusters and
  glows, twin chevrons, rear light bar and a restrained current-color panel.
- Builder now regenerates the semantic gameplay colors with HDR emission,
  keeps dark alloy non-emissive and rejects missing/reversed rear parts,
  invalid material contrast or a weakened Bloom profile.
- Campaign Builder completed two consecutive passes. Focused EditMode passed
  `3/3`, focused PlayMode passed `1/1`, full EditMode passed `438/438`, and
  full post-Builder PlayMode passed `235/235`. Missing Script, Missing
  Reference, unique roots, EventSystem, fixed pools and Build Settings passed.
- Campaign and Step 10 simulations were not rerun because this iteration does
  not change Stage data, deterministic generation, timing, judgment, balance,
  collision or Experiment inputs. Package and ProjectSettings hashes remain
  exact to the approved baseline.
- Human portrait/device review still owns final rear-shape recognition,
  player/Track separation, Bloom strength and mobile performance.

## Iteration 35 validation result

- Starting HEAD `c4462e5` was clean. Completion commit name:
  `feat: upgrade protection and booster effects`.
- Campaign and Experiment now pass an immutable attempt-level normal-Shield
  exclusion into the existing deterministic Echo coordinator. Selected and
  stage-provided Shield suppress every Echo Provider; Echo itself does not.
- Builder replaced the six/four cube Shield/Echo shells with two instances of
  one transparent procedural-hex field and replaced side box speed lines with
  centered Cyan/Gold Warp layers capped at 160 emitted particles. Shield and
  Echo consumption now trigger fixed, pause-owned 0.35-second outward pulses
  using the exact consumed field hue.
- Campaign Builder completed two consecutive passes. Focused Core and
  PlayMode coverage, including both consumption pulses, passed; full EditMode
  passed `436/436`, and full final post-Builder PlayMode passed `235/235`.
- Campaign simulation ran twice with 400 byte-identical rows and the new hashes
  above. Only 24 Shield-bearing rows in Echo Stages 18–20 changed; all
  no-Shield rows are exact. Step 10 ran twice and retained every approved hash.
- Package and meaningful ProjectSettings hashes remain exact. Isolated
  validation did not use or mutate the developer Product save. The already-open
  original Editor had independently advanced that save from revision 231 to
  revision 237 before final validation; the revision 237 snapshot above was
  preserved rather than restored.
- Human portrait/device review owns field transparency, hex scale, Echo hue
  recognition, Warp density, central readability, Bloom balance and mobile
  frame cost.

## Iteration 34 validation result

- Starting HEAD `04826df` was clean. Completion commit name:
  `fix: preserve recycled track continuity`.
- Human play exposed the road disappearing after sustained movement. The
  imported mesh is 40 units long, but generated segment anchors still spanned
  only one unit; recycled segments therefore accumulated instead of appending.
- Builder now authors `-20 / +20` anchors and rejects a non-40-unit or
  discontinuous pool. Runtime placement aligns each start anchor to the prior
  segment's end anchor.
- Campaign Builder completed two consecutive passes. The new PlayMode
  regression recycles the fixed six-segment pool through 1.2 km and checks
  every span and seam. Full EditMode passed `434/434`; full post-Builder
  PlayMode passed `233/233`.
- Campaign and Step 10 simulations were not rerun because Stage data,
  deterministic gate generation, mechanic timing, judgment, balance and
  Experiment inputs are unchanged. Package and meaningful ProjectSettings
  hashes remain exact; isolated validation did not use the developer save.
- Human portrait play must confirm the road remains continuous through a full
  Stage and during prolonged development runs.

## Iteration 33 validation result

- Starting HEAD `12011cc` was clean. Completion commit name:
  `fix: normalize theme one artwork axes`.
- The generated Scene exposed the common imported-FBX root quaternion
  `(-0.7071068, 0, 0, 0.7071067)`, equivalent to `X -90°`. This rotated the
  authored 40-unit Z Track into the vertical Y axis. The Theme 1 instantiation
  boundary now explicitly normalizes all five imported artwork roots.
- Campaign Builder completed two consecutive passes and now rejects any
  non-normalized Theme 1 root or Track bounds that are not approximately
  7 units wide, below 3 units high and 40 units long on Z.
- Full EditMode passed `434/434`; final post-Builder PlayMode passed `232/232`.
  Existing Track recycling, Ice, Booster, Continue and fixed-pool coverage
  remained active.
- Campaign and Step 10 simulations were not rerun because collision, Stage
  data, generation, timing, balance, judgment and Experiment inputs did not
  change. Package and meaningful ProjectSettings hashes remain exact.
- Isolated validation did not use the developer save. The open original Editor
  independently advanced the live play state to revision 231; it was preserved
  and recorded above.
- Human portrait play must confirm the corrected road plane, runner/gate/Goal
  silhouettes and camera composition.

## Iteration 32 validation result

- Starting HEAD `6a542a9` was clean. Completion commit name:
  `feat: apply neon ui skin`.
- Added a reproducible procedural source for seven transparent 9-slice
  surfaces and ten neon action/resource icons. One shared Editor skin owner
  imports and applies them to both generated Frontend and Campaign UI without
  replacing live labels, navigation events or Product state.
- Frontend and Campaign Builders each completed two passes. Full EditMode
  passed `434/434`; final post-Builder PlayMode passed `231/231`. The new
  coverage checks real alpha, sprite borders, button state surfaces and
  non-raycast icons in both Scenes.
- Campaign and Step 10 simulations were not rerun because no Stage data,
  deterministic generation, timing, balance, judgment or Experiment input
  changed. Their approved hashes remain authoritative.
- Validation ran only in the isolated project copy. The developer save stayed
  at revision 229 and the package and meaningful ProjectSettings hashes match
  the authoritative baseline.
- Human portrait/device review owns icon scale, text/icon balance, 9-slice
  corner quality, disabled-state readability and final typography.

## Iteration 31 validation result

- Starting HEAD `3086233` was clean. Completion commit `6a542a9`:
  `feat: add neon gameplay visual slice`.
- Added reproducible Blender source/export tooling, one editable Theme 1
  `.blend`, five FBX models, fifteen UV/PBR maps, genuine-alpha Lobby ambient
  art, imported Campaign presentation and fixed pooled gate-break VFX.
- Campaign Builder completed two final passes. Full EditMode passed `433/433`
  and final post-Builder PlayMode passed `229/229`. Missing Script, required
  reference, duplicate-root and imported-prefab-parenting checks passed.
- Campaign and Step 10 simulations were not rerun because no Stage data,
  deterministic generation, timing, balance, judgment or Experiment input
  changed. Their approved hashes remain authoritative.
- Product validation ran only in the isolated project copy and did not touch
  the developer save. The open original Editor independently advanced the
  Heart clock to revision 229; that live state was preserved and is recorded
  above rather than restored. Package and meaningful ProjectSettings hashes
  match the authoritative baseline.
- Human portrait/device review still owns model scale, runner/gate silhouette,
  break readability, emissive balance, Bloom intensity and mobile performance.

## Iteration 30 validation result

- Starting HEAD `7284cb2` was clean. Completion commit name:
  `feat: add color courtyard visual slice`.
- Added three original Theme 1 raster assets and one data-driven Lobby visual
  catalog. Frontend Builder imports them as non-mipmapped UI Sprites, assigns
  the Background / Midground / Foreground layers and preserves palette-only
  fallbacks for Themes 2 and 3.
- The six existing automatic milestone visuals are now compact energy nodes
  around the reactor. Their activation still derives only from Product Lobby
  progression; all wallet, Stage card and launch behavior is unchanged.
- Frontend Scene Builder completed two passes. Focused Builder EditMode passed
  `9/9`, focused Frontend PlayMode passed `19/19`; full EditMode passed
  `431/431`; full post-Builder PlayMode passed `228/228`.
- Campaign and Step 10 simulations were not rerun because no gameplay, Stage
  data, timing, generation, judgment or Experiment input changed. Their
  existing approved hashes remain authoritative.
- Product save and backup remained byte-, timestamp- and SHA-exact. Package
  hashes and the meaningful ProjectSettings hash match the baseline. No
  Missing Script marker or required-reference failure was found.
- Human portrait review still owns visual scale, crop, contrast, pulse
  restraint and milestone-node readability on target devices.

## Iteration 29 validation result

- Starting HEAD `3e9b389` was clean. Completion commit name:
  `feat: simplify lobby hierarchy`.
- Frontend Lobby now uses a persistent top resource bar, one dominant theme /
  upgrade center and one compact bottom Stage card with a single primary
  `PLAY` action. Duplicate visible Lobby/account labels were removed.
- Product Heart snapshots expose the next recharge UTC without changing save
  schema or persistence. Lobby Heart text refreshes once per second and shows
  full count, next-recharge countdown or unlimited remaining time.
- The current Stage card includes authored difficulty. Shield/Booster stock is
  secondary, and milestone reward copy appears only while a reward is pending.
- Frontend Scene Builder completed two consecutive passes. Focused Lobby
  EditMode passed `8/8`, focused Product EditMode passed `58/58`, focused
  Frontend PlayMode passed `18/18`; full EditMode passed `430/430`; full
  post-Builder PlayMode passed `227/227`.
- Campaign and Step 10 simulations were not rerun because no Campaign Scene,
  gameplay, deterministic generation, timing, judgment or Experiment input
  changed. Their Iteration 28 and Step 10 hashes remain authoritative.
- Product save and backup remained byte-, timestamp- and SHA-exact. Package
  and meaningful ProjectSettings hashes remain the authoritative baseline.

## Iteration 28 validation result

- Starting HEAD `eddf720` was clean. Completion commit name:
  `feat: rebalance ice tap rhythm`.
- The Campaign Stage sequence now applies an Ice-only tap budget. Stage 15 / 16
  use one tap on all 12 / 14 Ice gates. Stage 17 uses one tap on 14 gates and
  two taps on one deterministic gate, keeping the double-tap rate at `6.67%`.
- Non-Ice stages, Ice speed/spacing/runway presentation, Continue, stable Stage
  IDs and Experiment Lab remain unchanged. Retry reproduces the same colors.
- Focused EditMode passed `13/13`; focused PlayMode passed `1/1`; full EditMode
  passed `429/429`; full PlayMode passed `227/227`. No Builder was required.
- Two complete 400-row Campaign simulations were byte-identical. Summary /
  JSON / CSV SHA-256 are
  `D3287E93FE0F022ABDF504EC3304076724BFBB3357A890DEFB0545E072CD9DB1`,
  `43C61E0CCB28B10211C6E201631EE2938D0160F72DA3A334AE0A459331276500`, and
  `4E1E87C9C1D5B27D63122744889DE066A3D22F2A93DF86CB6062DAC2165F5B99`.
  All continuity and pool-violation counters are zero. Step 10 was not rerun
  because Experiment behavior and inputs are unchanged.
- The real Product save and backup remained byte-, timestamp- and SHA-exact.

## Iteration 27 validation result

- Starting HEAD `4c3ce13` was clean. Completion commit name:
  `feat: add preplaced ice runway`.
- Stage Catalog revision 10 authors a `2.0` Campaign Ice speed multiplier for
  Stages 15–17. The Stage IDs, gate count, sequence, seed, spacing modifier,
  judgment and Experiment Lab definitions remain unchanged.
- Campaign Scene owns one fixed `IceRunway` with exactly 50 prebuilt panels.
  It activates all deterministic Ice approach intervals before Countdown,
  never grows at runtime, preserves the layout through Continue and clears it
  on Retry. The normal six-segment track no longer changes material for
  Campaign Ice.
- The Fog View was moved into a matching Unity script asset after a clean
  domain reload exposed its former file/class-name reference instability.
  Fog state, timing and behavior are unchanged.
- Campaign Builder completed two consecutive runs. Focused Ice tests passed
  EditMode `8/8` and PlayMode `4/4`; full EditMode passed `425/425`; final
  post-Builder PlayMode passed `226/226`. Missing references, duplicate roots,
  fixed pool and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical. Summary /
  JSON / CSV SHA-256 are
  `86D55FB085FE51135BFA5E0F5D17D0242166C1F9DBD3EE03F58485A33BFC27B8`,
  `3CE081477E7A431939D4C8AB6D0140FFBB6C22D6A6850E7551499E4C28BC28C2`, and
  `9D6E7903B31D59C3307DBE49A1E1FCF705A299FE37571299F17FAD63DF464B95`.
  All displacement, gap, duplicate-index, cursor-reset and full-pool-reset
  counters are zero. Step 10 was not rerun because Experiment behavior and
  inputs retain the historical `1.45` Ice comparison.
- The real Product save remained byte-, length-, timestamp- and SHA-identical
  to the captured pre-test snapshot.

## Iteration 26 validation result

- Starting HEAD `74a9953` was clean. Completion commit name:
  `feat: add fog visibility curve`.
- Fog curtain timing is authored in Stage Catalog revision 9. Stages 12 / 13 /
  14 use a 0.5-second fade-in, 5 / 6 / 7 seconds at full opacity, and a
  0.5-second fade-out respectively.
- Campaign judgment, gate plans, speed, spacing and stable Stage IDs are
  unchanged. Experiment Lab retains its historical nearest-two benchmark.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `423/423`; post-Builder PlayMode passed `225/225`. Missing references,
  duplicate generated objects, fixed pools and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical and retained
  Summary / JSON / CSV hashes
  `698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`,
  `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`, and
  `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`.
  Step 10 was not rerun because Experiment inputs and behavior are unchanged.
- The real Product save and backup remained byte- and timestamp-identical to
  their pre-test snapshots.

## Iteration 25 validation result

- Starting HEAD `a8f7892` was clean. Completion commit name:
  `feat: add timed fog curtain`.
- Campaign Fog now uses a single speed-adaptive world-space curtain instead
  of per-gate nearest-two neutralization. The curtain holds for 1.5 seconds,
  fades for 0.5 seconds, preserves its state through Pause and Continue
  countdown, and resets on Retry.
- Campaign judgment, authored gate plans, speed, spacing and stable Stage IDs
  are unchanged. Experiment Lab keeps the historical nearest-two benchmark.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `421/421`; final post-Builder PlayMode passed `225/225`. Missing references,
  duplicate generated Fog curtain, fixed pools and Build Settings checks passed.
- Campaign and Step 10 simulations were not rerun because no Core, Catalog,
  generator, judgment, seed, balance or simulation input changed; Iteration 23
  hashes remain authoritative.
- The real Product save and backup remained byte- and timestamp-identical to
  their pre-test snapshots.

## Iteration 24 validation result

- Starting HEAD `61147cd` was clean. Completion commit name:
  `fix: recycle failed gates after continue`.
- Continue now recycles the failed gate's fixed-pool slot immediately instead
  of permanently deactivating it. Repeated Continues therefore keep supplying
  later gates and preserve Goal completion even after more Continues than the
  six visible gate slots.
- The clean respawn path no longer writes velocity to a kinematic Rigidbody.
- The Stage 11 regression deliberately Continues more than six times from the
  late Camouflage sequence, resolves all 46 gates, crosses Goal and reaches
  Stage Cleared.
- Focused Stage 11 PlayMode passed `1/1`; full EditMode passed `418/418`; full
  PlayMode passed `223/223`. No Scene, Builder, Core, Catalog, deterministic
  content or Product-save contract changed, so Builder, Campaign and Step 10
  artifacts retain the Iteration 23 baseline.
- The real Product save and backup remained byte-identical to their pre-test
  snapshots.

## Iteration 23 validation result

- Starting HEAD `7858803` was clean and matched the approved Iteration 22
  baseline. Completion commit name: `feat: refine continue and clear economy
  flow`.
- Continue pricing is `900 / 1,900 / 2,900 / 4,900-repeat` with no attempt
  cap. Resume Countdown applies modifiers immediately and presents consumed
  buffs as already gone.
- Failure shows Coin/Heart resources and a truthful insufficient-Coin popup.
  Campaign Clear hides Replay, displays first-clear and milestone rewards
  separately, refunds a consumed stored Heart atomically, and opens an
  unlocked next Stage directly in PreRun.
- Normal / Hard / Very Hard first-clear rewards are `100 / 200 / 500` Coins.
  Stages 11, 14 and 17 are Hard; Stage 20 is Very Hard.
- Campaign Builder completed two consecutive runs. Full EditMode passed
  `418/418`; final post-Builder PlayMode passed `222/222`. Missing Script,
  Missing Reference, duplicate generated root and Build Settings checks passed.
- Two complete 400-row Campaign simulations were byte-identical with the
  authoritative hashes above. Step 10 was not rerun because Experiment inputs
  and contracts were unchanged.
- The real schema-3 Product save remained byte-identical at SHA-256
  `A81C6DA765C17DF44C131650EE73316D0F8B052B41938DC1AA82B18A350C8E18`.
- A real rewarded-ad provider, Shop navigation, Fog/Ice presentation redesign
  and final Lobby layout remain separate iterations and human review items.

## Iteration 22 validation result

- Full EditMode passed `414/414`; full PlayMode passed `218/218`.
- New isolated Product coverage verifies exact Economy edits, invalid-input
  non-publication, Campaign-only reset and Economy-only reset. Editor coverage
  verifies the approved menu path and existing Build Settings order.
- No Scene, Builder-owned hierarchy, gameplay rule, Stage data or deterministic
  input changed. Builders, Campaign simulation and Step 10 were therefore not
  rerun; their prior approved baselines remain authoritative.
- The actual developer save remained schema 3 revision 30 with SHA-256
  `77AF3ED2CFBB13591E2119FD77F94C68A5BFC488CF4DB62C2FDCC52C8B443B50`.

## Iteration 21 validation result

- Campaign Builder completed two consecutive passes and regenerated the
  Ticket-first failure UI with all required references.
- Full EditMode passed `408/408`; full post-Builder PlayMode passed `218/218`.
- The actual Editor Product save remained byte-, timestamp- and hash-exact at
  schema 2 revision 29 with SHA-256
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`.
  Schema-3 migration was validated only with isolated saves.
- Campaign and Step 10 simulations were not rerun because Core gameplay,
  Stage data, generation, timing and deterministic inputs are unchanged; the
  previously approved hashes remain authoritative.

Version

Iteration 20

(Monetization Platform Baseline)

---

- Unity IAP `5.4.2` is the approved standard Google Play billing package and
  Android now uses `com.wscho.colorgaterunner`.
- No Store, product catalog, runtime purchase service, receipt grant, Firebase
  SDK, or Shop UI is claimed by this baseline.
- Full EditMode passed `400/400` and full PlayMode passed `215/215` after the
  final application-ID change. No Builder or deterministic simulation was
  required because no Scene, gameplay rule, content, or simulation input
  changed; all prior Campaign and Step 10 hashes remain authoritative.
- The actual Editor Product save remained schema 2 at revision 29 with Guest
  ID `2dfe4f6ffa914e7a95e2fd30de5b6307`, Highest Stage 14, 13 Stage records,
  1,500 Coins, 3 Shields and 3 Boosters. Its SHA-256 is
  `9850956C9219B35ABD6DC5049BE59FA48309A3A17CF72B7CA158986EDA85153E`;
  its last-write time predates both final validation suites.

Version

Iteration 19

(Continue Economy Policy)

---

- Campaign failure now offers authorized Continue sources rather than the
  legacy free button. Coin prices are `300 / 600 / 900` by successful Coin
  ordinal, with one completed rewarded-ad right and a total cap of three
  Continues per attempt. Retry resets the attempt-local policy.
- Coin spend is an atomic, idempotent schema-2 Product transaction before Core
  resume. Insufficient funds, save failure, failed/cancelled/unavailable ads,
  duplicate callbacks and stale callbacks preserve the frozen failure state.
  An unavailable release provider is hidden and never grants a fake success.
- The Campaign Builder command built twice and validated successfully. Full
  EditMode passed `400/400`; final post-Builder PlayMode passed `215/215`.
- Two 400-row Campaign simulations were byte-identical with Summary, JSON and
  CSV hashes `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`,
  `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`,
  and `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`.
  Step 10 remained unchanged.

Version

Iteration 18

(Consumable Owned Start Items)

---

- Stage 8+ selectable Shield and Booster now consume one owned unit per
  selected item in one atomic successful save before Countdown. Starting with
  no selected item performs no inventory write.
- Zero stock disables the corresponding toggle. Stale insufficient inventory
  and save failure remain in PreRun with truthful status and no partial
  published decrement. Retry consumes again for the new attempt; Back before
  Start consumes nothing; duplicate Start consumes once.
- Stage 6/7 provided items remain free. Missing Product context cannot grant a
  selectable item for free, while no-item and provided-item starts remain
  available. Product save schema remains 2 with no migration.
- Targeted Product tests passed `30/30`; targeted PlayMode tests passed `5/5`.
  The Campaign Builder command built twice and validated the generated Scene.
  Full EditMode passed `372/372` and full post-Builder PlayMode passed
  `207/207`.
- Campaign and Step 10 simulations were omitted under Tier 2 because Core,
  stage data and deterministic simulation inputs did not change. All
  Iteration 17 artifact hashes remain authoritative.

Version

Iteration 17

(Campaign Learning Curve Rebalance)

---

- Catalog revision 7 preserves stable IDs and Stage 1–5 exact simulation
  results while replacing the Stage 6–20 learning order.
- Stage 6/7 provided-item labels take precedence over locked selection;
  Stage 8 is the first selectable-item application Stage.
- Camouflage, Fog, Ice and Echo now use contiguous intro/practice/mastery
  blocks. Hidden and Flicker Campaign bindings are deferred to Stages 21+;
  their Experiment implementations and tests remain.
- EditMode `367/367`, post-Builder PlayMode `195/195`, and two Campaign
  Builder runs passed. The 400-row Campaign report reproduced byte-identically
  twice with zero continuity or pool violations.
- Save-failure PlayMode coverage now arms the exact next write, verifies the
  user-visible failure state, and no longer expects a log that the product
  contract never emits.

Version

Iteration 16

(Campaign Act 2: Stage 14–20)

---

- Catalog revision 6 adds stable IDs `stage-14` through `stage-20`, all using
  the existing Red/Blue/Green cycle and existing Shield/Booster selection.
- Stage 14 is a shorter unmodified recovery stage. Stages 15–20 revisit one
  existing primary mechanic each: Camouflage, Fog, Ice, Echo, Hidden and
  Flicker. No stage enables mixed modifiers or a new tutorial grant.
- Campaign Builder generated 20 Stage buttons in two consecutive passes. A
  two-percent normalized gap between current-stage and title text fixes the
  overlap exposed by portrait reference-resolution validation.
- Targeted EditMode `3/3`, full EditMode `368/368`, and final post-Builder
  PlayMode `186/186` passed. No Missing Script, Missing Reference, duplicate
  root, EventSystem or Build Settings regression was reported.
- Campaign simulation now contains 400 rows. Its prior Stage 1–13 CSV rows and
  JSON result prefix are byte-exact, while the approved full hashes are owned
  by the Authoritative Baseline above. Step 10 was not rerun because shared
  mechanic rules, Experiment inputs and player profiles did not change.
- Mechanical Average/No Item first-clear rates are `23.6%` at Stage 14,
  `15.9%`, `15.8%`, `12.9%`, `24.8%`, `13.4%`, and `12.6%` through Stage 20.
  Stage 18 deliberately forms an Echo relief beat. These figures do not prove
  fun, readability, fairness or launch balance.

---

Version

Iteration 15

(Automatic Lobby Progression Foundation)

---

- Schema 2 adds stable-ID Campaign records, Economy wallet/inventory and
  idempotent transaction IDs, plus applied/presented Lobby milestone state.
- Production Boot imports legacy Campaign PlayerPrefs once without deleting
  them. Stage clear then persists record, unlock and first-clear rewards in one
  Product save transaction; failed writes publish no partial state.
- Frontend Lobby shows wallet, Shield/Booster inventory, theme progress, next
  automatic upgrade target, and pending reward summary. Every two first clears
  advances one of 18 milestones across the planned 36-Stage campaign.
- Frontend-origin Clear/Failure home actions return to Frontend Lobby so the
  earned upgrade is visible. Direct and development entry keeps the legacy
  Campaign Lobby route.
- Product-focused EditMode `25/25`, Frontend PlayMode `16/16`, full EditMode
  `365/365`, and post-Builder PlayMode `186/186` passed. Frontend Builder passed
  twice with no missing reference, duplicate root or Build Settings failure.
- Campaign and Step 10 simulations were not rerun under TEST_PLAN Tier 2:
  Stage definitions, deterministic generation, timing, judgment and balance
  inputs did not change. Their approved hashes remain authoritative.

---

Version

Iteration 14

(PreRun Back Navigation / Settings Toggle Presentation Hotfix)

---

- A successfully consumed Frontend Campaign launch records a Scene-local,
  non-persistent entry origin. PreRun Back reuses the serialized Frontend path
  and existing Scene loader without activating the legacy Campaign Lobby.
- Missing launch context, direct SampleScene, development, and Experiment
  paths retain their existing Campaign Lobby behavior.
- Return input is blocked while loading. Failure stays in PreRun, reports the
  error, restores the Back action, and permits one clean retry.
- The shared Settings Builder now creates one `StateLabel` per toggle. The
  controller updates its text and color on open, click, save, re-entry, and
  save reload; legacy overlapping `OnLabel` and `OffLabel` objects are absent.
- EditMode `361/361` and post-Builder PlayMode `184/184` passed. Frontend and
  Boot were rebuilt twice through the Frontend Builder, and Campaign Builder's
  internal two-pass build succeeded.
- Campaign remains 260 rows and Step 10 remains 80 rows / 64,016 runs with all
  eight approved hashes unchanged. Packages and non-volatile ProjectSettings
  semantics are unchanged.

---

Version

Iteration 13

(Lobby Consolidation / Gameplay Settings Entry / Unified Settings Access)

---

- Frontend now recommends the current Campaign Stage from the existing
  device-wide PlayerPrefs store through a read-only adapter. The AppRoot-owned
  one-shot stable-ID request enters the existing PreRun/item-selection flow
  without displaying Campaign Lobby in production.
- Direct Campaign, development, Experiment Lab, and missing-context entry keep
  the existing Campaign Lobby as a safe fallback.
- Frontend and Pause share Notifications, Music, SFX, Vibration, and external-
  link Settings. Master stays hidden/preserved; Music/SFX restore their last
  non-zero values; notification delivery and content-specific audio remain
  unimplemented and are presented truthfully.
- Pause remains owned by the existing coordinator. Its generated HUD button
  is inside Safe Area, at least 44 by 44, clear of Stage/Shield HUD, and does
  not leak clicks to gameplay.
- Full EditMode `361/361` and post-Builder PlayMode `180/180` passed. Frontend,
  Boot, and Campaign Builders each passed twice with no Missing Script,
  Missing Reference, duplicate root, or EventSystem regression.
- Campaign remained 260 rows and Step 10 remained 80 rows / 64,016 runs with
  all eight approved hashes unchanged. Package hashes are unchanged.
- WebGL `APP_UI_EDITOR_ONLY` and `SENTIS_ANALYTICS_ENABLED` are recorded as
  package-managed volatile defines and excluded from the feature commit; no
  other ProjectSettings semantic change is allowed.

---

Version

Iteration 12

(Account Onboarding / Auto Lobby / Settings / Gameplay Pause UX implemented)

---

Account Onboarding, Settings, and Gameplay Pause Result

- `AccountChoiceCompleted` is Profile onboarding state, not a Settings value.
  Missing schema-1 fields decode as `false`; Settings repair cannot reset it.
- A thin `LocalProductSession` clones the current save, delegates persistence
  to the existing Save service, and publishes/rebinds state only after write
  success. Failed account or Settings writes leave public state unchanged.
- Account Choice offers a working local Guest path. Lobby has no Back-to-Title
  route; system Back requests exit confirmation. The unavailable Google action
  is hidden in release UI.
- One `SettingsPanelController` contract is used in Frontend and Gameplay.
  Apply persists Master/Music/SFX/Vibration; Music and SFX await real content
  targets, while Master and Booster vibration are applied now.
- `GameplayPauseCoordinator` is the sole Pause/modal/transition owner. Pause
  uses a full-screen raycast-blocking 0.85 alpha Dim and only pauses the two
  Builder-registered attempt ParticleSystems.
- Campaign-to-Frontend return uses a Builder-serialized active Scene path.
  Load failure stays paused with a recoverable error instead of recording a
  Stage result.
- Full EditMode `352/352`, post-Builder PlayMode `179/179`, all three Builders
  twice, Campaign 260-row hashes, and Step 10 80-row/64,016-run hashes passed.
  Packages and ProjectSettings are unchanged.

---

Version

Iteration 11

(Campaign Progress Restore Hotfix: PlayMode PlayerPrefs isolation and
restart/re-entry regression coverage implemented)

---

Campaign Progress Restore Hotfix Result

- Campaign progress remains device-wide Unity `PlayerPrefs` owned only by
  `PlayerPrefsStageProgressStore`. It is not linked to the Product Guest and
  is not copied, reset, migrated, or written into `product-save.json`.
- Investigation found that two PlayMode fixtures deleted the real Editor
  `HighestUnlocked` and Stage 6 Record keys during cleanup without preserving
  their previous values. Windows Editor project copies share this PlayerPrefs
  namespace when Company and Product names match.
- All Campaign-affecting PlayMode fixtures now capture Highest Unlocked plus
  every Stage 1-13 Record before setup work and restore exact key existence,
  integer/string values, and `PlayerPrefs.Save()` in exception-safe cleanup.
- Regression coverage now proves Stage 11 first entry and re-entry, Lobby /
  PreRun / Gameplay stable-ID parity, fresh Stage 1 clear followed by Stage 2
  after controller recreation, new Guest preservation of existing Campaign
  progress, and missing/corrupt Highest fallback without Record deletion.
- Values deleted before this investigation cannot be reconstructed from
  evidence. The missing prior Highest Unlocked and Stage 6 Record are not
  guessed or automatically repaired.
- Profile-scoped Campaign progress remains deferred to the separately approved
  `StageProgressService` Iteration. A future testability improvement may inject
  an in-memory Campaign store, but this Hotfix does not alter runtime storage.

Iteration 11 Validation

- Snapshot Utility tests: `3/3`; Campaign restore regression tests: `4/4`.
- EditMode: `335/335`; PlayMode: `167/167`; post-Builder PlayMode: `167/167`.
- Frontend Builder, Boot Builder, and Campaign Builder each succeeded twice.
  Builder and PlayMode validation found no duplicate generated objects,
  Missing Script, Missing Reference, or required-reference failure.
- The actual Editor Campaign PlayerPrefs snapshot retained 11 existing entries
  and SHA-256
  `2A8AF71352FCCF9D80C8BDCB9BDC89FC3F61A7B356633C3423A847F576D0B6C9`
  before and after the test runs. The Product save file hash also remained
  unchanged, preserving the current Guest ID.
- Campaign simulation remains 260 rows with all three approved hashes. Step 10
  remains 80 rows / 64,016 runs with all five approved hashes.
- `ProjectSettings.asset`, Package manifest/lock, Scene assets, runtime
  Campaign code, Stage Catalog, balance, and Product Save Schema are unchanged.

---

Commercial Flow Foundation Frontend Iteration 2 Result

- `Frontend.unity` is Build Index 1 between Boot and the existing Campaign
  `SampleScene`. Boot serializes Frontend as its destination; Frontend
  separately serializes the existing active Campaign path. Runtime does not
  hardcode the `SampleScene` name.
- One `FrontendPageRouter` owns the Title/Lobby Page, blocking Modal, and
  Transition state. Page Views do not activate one another or load Scenes.
- Title shows the initialized Guest display name, truthful `GUEST` state,
  version, account-unavailable notice, and read-only Settings summary. Empty
  Terms/Privacy/Support actions are hidden.
- Placeholder Lobby shows `CONTINUE CAMPAIGN` without reading or estimating
  Stage progress. Currency, Event, Notification, and Lobby-theme slots remain
  inactive and consume no layout space.
- Frontend contains no AppRoot. It captures an immutable display Context from
  the Boot-initialized AppRoot. Direct Frontend execution without AppRoot
  displays blocking `BOOT REQUIRED` and disables Campaign entry.
- Lobby Back returns to Title, Title Back opens Exit Confirmation, a
  cancellable Modal consumes Back, and Transition state ignores Back and
  blocks duplicate input.
- Campaign entry loads the existing Scene once and retains its current Lobby,
  PreRun, Gameplay, result, progress, and return contracts. No Campaign-to-
  Frontend route was added.
- The approved portrait/Input baseline was first restored in isolated commit
  `c191826`; this Iteration does not modify `ProjectSettings.asset` again.

Iteration 10 Validation

- EditMode: `335/335` passed. PlayMode: `160/160` passed. The same counts pass
  after all final Builder runs.
- Frontend Builder succeeded twice, Boot Builder succeeded twice, and the
  Campaign Builder completed its two-build command successfully. Required
  references, Safe Area, one EventSystem, one primary Page, unique generated
  roots, and no Missing Script were validated.
- Stage 1-13 Campaign simulation remains 260 rows and preserves Summary/JSON/
  CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and preserves all five approved hashes. Package
  manifest/lock hashes remain unchanged. The only ProjectSettings change in
  this Iteration is adding Frontend to `EditorBuildSettings.asset`.
- Human review remains required for 9:16 small-screen readability, Boot-to-
  Title and Lobby-to-Campaign transition feel, Android Back behavior, UI input
  leakage on devices, and the intentional new-Lobby/old-Lobby UX handoff.

Deferred after Iteration 10

- Final Lobby, Campaign Page, Stage Detail, Gameplay/Results shells,
  StageProgressService integration, Campaign-to-Frontend navigation, external
  account/cloud, economy, events, analytics, ads, IAP, and final visual art.

---

Commercial Flow Foundation Iteration 1 Result

- `Boot.unity` is Build Index 0 and initializes one persistent `AppRoot`
  before loading the first active non-Boot Build Settings Scene. The serialized
  destination is `Assets/Scenes/SampleScene.unity`; runtime contains no
  `SampleScene` name hardcode.
- `AppRoot` owns one explicit service graph: Clock, Profile, Local Account,
  Settings, Local Save, and the ordered initialization pipeline. Individual
  services are not singletons and no mutable service locator was added.
- The new `ColorGateRunner.Product` assembly has no Unity reference.
  `Application.persistentDataPath`, `JsonUtility`, Scene loading, and Boot UI
  stay in the Unity Presentation adapter.
- A fresh local product save creates exactly one Guest with an injected-capable
  UUID and UTC clock. Reload preserves the Profile ID and updates Last Played.
  Device identifiers are neither read nor stored.
- Product save schema 1 contains Profile, Settings, SaveRevision, and
  LastWriteUtc. Candidate revision and timestamp are applied before temporary
  serialization and validation. Primary/temp/backup, corrupt-primary recovery,
  supported schema-0 migration, and future-version blocking are implemented.
- Desktop/native uses atomic replace when supported. WebGL explicitly selects
  backup-based recoverable replacement and never reports an atomic result.
- `PlayerPrefsStageProgressStore` remains the sole Stage progression owner.
  Product save has no Stage section, does not read/write progression keys, and
  performs no copy, migration, reset, or new Stage result write.
- Boot shows Loading, version, a blocking local-error panel, and Retry. Retry
  reuses the existing service graph; returning to Boot rejects duplicate
  `AppRoot` creation.

Iteration 9 Validation

- EditMode: `313/313` passed.
- PlayMode: `152/152` passed; the same `152/152` passed after Boot Builder and
  again after two Campaign Scene Builder runs.
- Boot Builder succeeded twice. Campaign Scene Builder succeeded twice. Both
  validate required references, Safe Area, one EventSystem, and no Missing
  Script; PlayMode validates the existing Campaign Scene after regeneration.
- Stage 1-13 Campaign simulation produced 260 rows and reproduced Summary,
  JSON, and CSV hashes `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 reproduced 80 rows, 64,016 runs, and all five Iteration 8 hashes.
- Package manifest and lock hashes remain unchanged. The only approved
  ProjectSettings diff is the Boot-first `EditorBuildSettings` Scene list.
- Human review remains required for Boot presentation, transition feel,
  understandable save-error text, restart persistence on Android/WebGL, and
  preservation of real device Stage progress.

Deferred after Iteration 9

- Title, Frontend/Lobby redesign, Campaign Page, Stage Detail, Gameplay shell,
  Results redesign, StageProgressService integration/migration, external
  login/cloud, economy, events, analytics, ads, and IAP.

Previous Iteration 5 Result

- `StageCatalog.asset` is the single production source for all 11 campaign
  stages. Runtime, simulation, and tests consume its pure-Core adapter through
  `IStageCatalog`.
- Clone gameplay has been removed. Echo is a modifier on an ordinary gate and
  does not add gates, positions, collision paths, or a second generator.
- Camouflage reveal uses effective-speed ETA, transitions once, never hides
  again, and always keeps ordinary judgment active.
- Stages 1-5 lock both start items in catalog data and the PreRun UI.
- Stage 6 provides one local Shield at start; Stage 7 provides one local
  Booster at start. Both activate after the same countdown boundary as an
  equivalent selected item, and the picker prevents duplicate selection.
- Player movement crossing an unresolved gate plane reuses the existing
  one-shot `TryResolveCrossing()` path. This prevents high-speed Booster
  movement from tunneling past the thin physics Trigger without adding a
  second judgment system or increasing the six-gate pool.
- Stages 8, 9, 10, and 11 teach Camouflage, Fog, Ice, and Echo respectively.
  Stages 6-10 use two active colors to isolate the new mechanic; Stage 11
  returns to three colors for the finale.
- Stage speed Start/Max/Curve, cadence, active color count, mechanic,
  Echo/Camouflage values, seed, and local grant are Inspector-authored.
- The stage picker is catalog-driven and contains 11 stable-ID buttons in a
  two-column portrait layout.

Iteration 5 Validation

- EditMode: 249 / 249 passed.
- PlayMode: 128 / 128 passed.
- Stage 7 PlayMode starts Booster at `GO`, crosses all 40 planned gates at
  runtime speed, retains the fixed pool, and reaches Goal/StageCleared.
- The requested Stage 6-11 simulation subset contains 120 rows and 96,024
  modeled runs: six stages, five profiles, four item inputs, Perfect once and
  every stochastic condition 1,000 times.
- Stages 6 and 8-11 have zero changed rows versus the preceding campaign
  baseline. All 20 Stage 7 rows change only under the approved start-Booster
  contract.
- Stage 7 Average/None changes mechanically from first-clear
  `47.6% -> 47.0%`, Continue-clear `84.8% -> 95.1%`, median clear time
  `33.203s -> 31.356s`, and average Booster bypasses `9.780 -> 15.774`.
  These are not claims about fun or comfort.
- All 24 Stage 6-11 Perfect/item rows clear, and all 120 rows report zero
  Booster/Continue displacement, gate-index gap/duplicate, cursor reset, and
  full-pool reset violations.
- A fresh Step 10 run reproduced all five baseline hashes exactly.
- The full refreshed Stage 1-11 matrix is stored in the existing Mechanic
  Campaign artifacts.
- Core keeps `noEngineReferences: true`; no package or ProjectSettings feature
  change was introduced.

Stage Progression Hotfix

- Fixed a stale stable-ID reference in `ShowLobby()`. The lobby previously
  displayed the newly unlocked stage number but `PlayFromLobby()` could still
  start the prior stage ID.
- The displayed `StageDefinition.StageId` is now synchronized whenever lobby
  progression selects a stage.
- PlayMode explicitly verifies displayed Stage 2 starts Stage 2 and displayed
  Stage 3 starts Stage 3.

Test Build Tooling

- `Tools > Color Gate Runner > Test Builds` provides Android APK, WebGL,
  combined Android-plus-WebGL, and output-folder commands.
- Both platforms use the enabled `EditorBuildSettings` scenes and Development
  mode. Outputs stay under ignored `Builds/Test`; Android always produces an
  APK and restores the previous App Bundle setting afterward.
- Every platform command refreshes AssetDatabase, Stage Catalog, Campaign,
  Frontend and Boot before packaging, then removes that platform's previous
  output. A failed invocation therefore cannot leave an older APK or WebGL
  folder looking current.
- WebGL test builds temporarily use a `540 x 960` logical canvas and the
  project-owned `ColorGateRunnerPortrait` template. Its browser container
  preserves an exact `9:16` aspect ratio with neutral letterboxing instead of
  stretching to a square or landscape host.
- The build path restores the prior WebGL width, height, and template settings
  after success or failure, so Android and persistent ProjectSettings remain
  unchanged.
- Missing scenes, missing platform support, platform-switch failure, and
  unsuccessful `BuildReport` results fail explicitly instead of reporting a
  false success.
- Generated Android Gradle projects force Kotlin stdlib `1.8.22` and exclude
  the legacy split jdk7/jdk8 artifacts that otherwise duplicate classes with
  current Google dependencies. The postprocessor is idempotent and repairs a
  cached pre-`plugins` insertion without deleting the whole build cache. Its
  callback remains registered when the Editor starts on WebGL.
- Build-menu, portrait-template and Gradle-upgrade EditMode coverage passes
  `7/7`; the full current EditMode suite passes `450/450`.
- The current combined menu-driven Android build completed and produced a
  68,774,305-byte APK. Installation and target-device behavior remain manual.
- The same command produced a 155,566,553-byte WebGL output folder. Its
  generated page contains a `540 x 960` canvas and exact
  `9:16` frame rules. Browser play, input, resizing and host-server behavior
  remain human acceptance checks.

Authoritative Iteration 6 Result (now Hidden)

- The memory mechanic then named Flicker, now named Hidden, is a flag on the
  shared ordinary-gate `GateModifier`. It adds no gate, generator, collision
  path, judgment path, or pool object.
- Its settings, now `HiddenSettings`, validate the approved
  Inspector-authored fields. The
  Launcher converts its serialized values into the pure-Core settings and
  disables Booster for Hidden-only runs.
- The existing deterministic Experiment sequence selects stable Hidden gate
  IDs without changing color/spacing PRNG output, gate count, or Step 10.
- A pure-Core visibility state requires both minimum readable time and shared
  effective-speed ETA, starts one transition, and never reveals target
  information again before judgment.
- The pooled View begins with target color/symbol plus a persistent `HIDDEN`
  identity. Hide blends to the existing neutral silhouette and removes only
  target symbol information; geometry, judgment opening, transform, collider,
  and marker remain.
- Player, held Echo, Shield, and ordinary failure reuse the existing priority.
  Retry/Replay reproduce selection and reset visibility state.
- Campaign Stages 1-11 and the 16-condition Step 10 matrix are unchanged.

The initial Hidden settings are Inspector-authored human-play candidates:
eligible progress `0.15-0.85`, chance `0.35`, cooldown `2`, visible duration
`1.00s`, hide lead `0.65s`, transition `0.12s`, maximum `4`, and guaranteed
first occurrence. Booster is disabled for this standalone condition.

Iteration 6 Validation

- Original open Editor: scripts compiled without C# errors.
- EditMode: 262/262 passed.
- PlayMode: 134/134 passed.
- Scene Builder: the command's two consecutive builds succeeded; the rebuilt
  isolated scene then passed PlayMode 134/134 again.
- Campaign simulation: 220 rows and 176,044 modeled runs. Summary, JSON, and
  CSV hashes are byte-identical to the preceding baseline.
- Step 10: all five CSV/JSON/summary/comparison/shortlist hashes are
  byte-identical to the approved baseline.
- Packages and ProjectSettings have no intended feature change. Automated
  results do not establish Hidden readability, comfort, comprehension,
  fairness, satisfaction, motivation, or fun.

Iteration 7 Baseline

- Clean worktree and `git diff --check`.
- EditMode 262/262 and PlayMode 134/134.
- Echo and former-Flicker targeted PlayMode cases 12/12.
- Campaign Summary/JSON/CSV and all five Step 10 hashes match Iteration 6.
- Package manifest/lock and ProjectSettings diffs are empty.

Iteration 7 Approved Contract

- Existing `Flicker = 16` gate modifier and mechanic value `5` become Hidden
  with identical selection, hide, judgment, View, and Retry/Replay behavior.
- New Flicker uses new enum values and cycles two or three unique active
  colors. The collision-time color from
  `ExperimentSession.ElapsedPlayingSeconds` is authoritative.
- Gate plans fix cycle colors, interval, deterministic phase, pulse, base
  color, Gate ID, and seed-derived data. View and judgment share the same Core
  active-color calculation.
- Minimum visible cycles are checked from existing deterministic speed,
  spacing, and six-View exposure. Booster is disabled.
- Player → Echo → Shield → Failure remains unchanged.
- Campaign, music/BPM/DSP, Hidden/Flicker or other modifier combinations,
  speed-based interval adjustment, new colors/input, and Stage 6-11 work are
  out of scope.

Iteration 7 Result

- The old modifier and mechanic numeric values `16` and `5` now deserialize
  as Hidden. New Flicker uses values `32` and `6`.
- Every former Launcher field has explicit `FormerlySerializedAs` migration
  metadata. New Flicker fields use a distinct `colorCycleFlicker` serialized
  prefix, so no old field name can bind to the new mechanic.
- Hidden preserves the former seed-selected gate IDs, ETA-based one-way hide,
  neutral silhouette, ordinary judgment, held Echo/Shield behavior, and
  Retry/Replay reset.
- Human-play feedback raised the Hidden default hide lead from `0.65s` to
  `0.85s`. The `1.00s` minimum observation and `0.12s` transition remain.
  Level tuning treats shorter Camouflage reveal time as harder but longer
  Hidden memory time as harder.
- Flicker remains an ordinary gate modifier. Its plan fixes the base color,
  every active-palette color in player tap order, interval, deterministic
  phase offset, pulse, Gate ID, and selection seed without changing gate
  count. The authored `2/3` cycle-count setting is removed.
- `FlickerCycleCalculator` is the single pure-Core phase path used by the
  View, collision-time judgment, simulation, and tests. An exact interval
  boundary uses the new phase.
- Experiment Gameplay Time advances only in Playing, freezes in Countdown,
  Failed, and StageCleared, and resets on Retry/Replay.
- Collision priority remains Player, held Echo, Booster if already present in
  a constructed session, Shield, then ordinary failure. The Launcher disables
  Booster for both Hidden-only and Flicker-only runs.
- The pooled View shows `HIDDEN` for the memory mechanic and `FLICKER` for the
  color cycle. Flicker color and symbol derive from the same Core color, with
  only a short brightness pulse above the immediate logical change.

Iteration 7 Validation

- EditMode: 281/281 passed.
- PlayMode: 143/143 passed before Scene Builder and 143/143 passed again after
  two consecutive successful Scene Builder runs.
- Related passed cases: Echo 27/27, Shield 29/29, Camouflage 9/9, Hidden
  25/25, Flicker 27/27, and Experiment Runtime 5/5.
- The full Stage 1-11 campaign regression wrote 220 matrix rows, including
  the requested Stage 1-5 coverage; Summary, JSON, and CSV hashes are
  byte-identical to the Iteration 6 baseline.
- Step 10 wrote 80 rows and 64,016 runs; all five
  CSV/JSON/summary/comparison/shortlist hashes are byte-identical.
- The rebuilt scene has no Missing MonoBehaviour, required-reference, pool,
  duplicate-root, or Lab return/re-entry failure in the final PlayMode suite.
- Package manifest/lock and ProjectSettings have no intended change.
- Automated validation does not establish switching comfort, concept
  comprehension, boundary readability, fairness, satisfaction, or fun.

Hidden/Flicker Human-Feedback Revision Validation

- Baseline EditMode `283/283` and PlayMode `143/143` passed.
- Final EditMode `288/288` and PlayMode `144/144` passed. Hidden/Flicker
  focused coverage is EditMode `14/14` and `20/20`, PlayMode `6/6` and
  `10/10`.
- Scene Builder completed twice. The regenerated Launcher serializes Hidden
  `0.85s` and contains no Flicker cycle-count field; post-Builder PlayMode is
  `144/144`.
- Stage 1-11 Campaign simulation wrote 220 rows and retained all three hashes.
  Step 10 wrote 80 rows and 64,016 runs and retained all five hashes.
- Package manifest/lock and ProjectSettings have no intended change.

Authoritative Iteration 8 Result

- Campaign now contains 13 stable catalog stages. Stage 12 `HIDDEN MEMORY`
  has 50 gates and Stage 13 `FLICKER FLOW` has 52; both use Red, Blue, Green
  and retain selectable Shield and Booster.
- Stage 12 uses the approved Hidden `0.85s` lead and deterministically selects
  Gate IDs `8, 19, 22, 26`. Stage 13 deterministically selects Flicker Gate
  IDs `8, 14, 19, 22`.
- `FlickerGatePlan` and `DeterministicModifierPlanner` are shared by
  Experiment and Campaign. Runtime, View, tests, and simulation use the same
  absolute Gameplay Time phase calculation.
- Campaign Flicker collision resolves the active color at
  `StageSession.ElapsedPlayingSeconds`. Booster remains the existing
  start-at-`GO` auto-pass and receives no Hidden/Flicker exception.
- Flicker exposure is accepted only when six pooled-view spacings provide at
  least three `0.50s` cycles at the authored Booster speed. Item choice does
  not alter targets, cycles, or phase.
- Catalog revision 5 and the rebuilt Scene contain 13 buttons. Scene Builder
  completed twice and post-Builder PlayMode passed.
- Final EditMode is `294/294`; PlayMode is `147/147` before and after the
  second Builder run.
- Stage 1-13 simulation wrote 260 rows. Its first 220 Stage 1-11 rows are
  exactly equal to the previous JSON results. Perfect clears Stage 12 and 13
  under all four item combinations.
- New 13-stage artifact hashes are Summary
  `A06F565542393722D7F0D7FE740D9446B8B49C6C553C7BEB6B053D42024A951D`,
  JSON
  `CED8EF947680C75C7851BAD7CF9B681D165A84C05577A3E735E444D882EEAA36`,
  and CSV
  `1BCE424F8FF478CB112A9EFC0A126BD050D5D955A51352640E5F490553810AAA`.
- Step 10 remains 80 rows and 64,016 runs with all five hashes unchanged.
  Package manifest/lock retain their prior hashes and ProjectSettings has no
  diff.
- Average/None first-attempt clear is `26.9%` at Stage 11, `16.2%` at Stage
  12, and `17.6%` at Stage 13. This mechanical drop is not auto-tuned and
  requires human difficulty review.

Next Iteration

Perform portrait-device human play of Stage 12 and 13. Record Hidden memory
difficulty, Flicker collision-boundary readability, whether post-Booster
Flicker encounters are sufficient, and whether the Stage 11 -> 12 difficulty
step is too large. Do not mix modifiers or auto-tune values from simulation
alone.

---

Completed / Historical Milestones

Stage 1~5

Experiment Lab

Continue

Retry

Goal

Booster

Shield

Camouflage

Fog

Ice

Experiment Countdown

Experiment Failed / Completed Results

Experiment Retry / Replay

Stage 4 Runtime / Simulation Color-Cycle Parity

Campaign Movement / Spatial Scale 2x

Superseded Clone-only Experiment

Superseded Clone Source / Relationship Metadata

Superseded Clone Failure Cause

Superseded Clone Shield / Camouflage Integration

---

Current Decisions

Maximum Tap = 4

Mechanics unlock by Stage

Experiment uses deterministic stages

Experiment uses one explicit StageFlowState

Experiment Retry / Replay preserves condition and seed

Experiment never reads or writes campaign progress

Campaign speed and authored world distances scale together

Campaign cadence and approximate stage duration remain unchanged

Experiment Lab movement values remain unchanged

Echo uses ordinary gates and never changes gate count

Echo Camouflage uses ordinary ETA hide, reveal, and judgment

Echo offers are deterministic and non-stacking

---

Iteration 4 Delivered

Architecture Foundation

Echo Modifier

Camouflage ETA Reveal

Stage Speed Profiles

Campaign Stages 6–11

---

Known Issues

Iteration 3 Clone Gate concept was rejected by human feedback and is
superseded by the approved Echo Modifier contract below.

Echo frequency, Echo/Shield readability, Camouflage reveal lead, and Stage
6–11 pacing require human mobile play.

Shield visual

Campaign 2x speed feel and comfort require human mobile playtest
