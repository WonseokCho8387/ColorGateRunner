# Frontend Flow

## Status

Approved commercial-flow design baseline. The Account Onboarding, consolidated
Lobby, Settings, Gameplay Pause, and automatic Lobby progression foundation
are implemented. The first Color Courtyard Lobby art slice is implemented.
Campaign Page, Stage Detail, final result presentation, additional Lobby themes
and final device polish remain pending.

This document defines the target player-facing Scene, page, overlay, and
navigation structure. It does not change gameplay, balance, Stage data,
Continue rules, or Experiment Lab behavior by itself.

## Purpose

Build a commercial-ready shell around the existing deterministic campaign
without turning every page into a separate Unity Scene or coupling UI to
gameplay state.

```text
App Start
-> Boot
-> Account Choice (first run only)
-> Lobby
-> Campaign
-> Stage Detail
-> Gameplay
-> Failure / Clear
-> Retry / Next / Home
```

## Documentation ownership

This document owns:

- Unity Scene boundaries for player-facing flow.
- Frontend pages and Gameplay overlays.
- Navigation and Back-button rules.
- Page-level UI composition.
- First-run, returning-run, offline, and error flows.
- Frontend payloads passed between pages and Gameplay.

This document does not own:

- Gate, item, Continue, stage, or judgment rules: `GAME_DESIGN.md`.
- Visual style details: `ART_DIRECTION.md`.
- Account, save, economy, event, analytics, and service internals:
  `PRODUCT_SYSTEMS.md`.
- Historical decisions: `DECISIONS.md`.
- Current implementation state: `CURRENT_STATUS.md`.
- Detailed tests: `TEST_PLAN.md`.

## Current-project constraints

The commercial shell must preserve these contracts:

- The campaign uses stable Stage IDs and one Inspector-authored Stage Catalog.
- Campaign currently contains Stages 1–20.
- Runtime, simulation, and tests share the same Core rules.
- Core remains free of UnityEngine references.
- Experiment Lab remains isolated from campaign progress and release
  navigation.
- Retry, Continue, Goal, Shield, Booster, Echo, Camouflage, Fog, Ice, Hidden,
  and Flicker retain their current gameplay contracts.
- Player-facing gameplay remains portrait 9:16 and Safe Area compliant.
- Frontend-only work must not change deterministic seeds or simulation output.

## Navigation direction

The commercial target introduces separate `Lobby`, `Campaign`, and
`Stage Detail` pages.

This supersedes the old player-facing single-current-stage Lobby only after
the new frontend is completely implemented and validated. Until then, the
existing navigation remains authoritative.

The hidden development Stage picker and Experiment Lab remain development
tools and must not appear in normal release navigation.

# 1. Unity Scene structure

Use a small number of Scenes and page-based navigation inside them.

## 1.1 Boot Scene

Responsibilities:

- Create or locate the persistent `AppRoot`.
- Load settings and local save data.
- Validate and migrate save-data version.
- Create a local Guest profile when no profile exists.
- Resolve account state without blocking offline play.
- Select the initial destination.
- Show initialization errors and retry actions.

Visible elements:

- Game logo.
- Loading indicator.
- App version and build version.
- Blocking initialization-error panel.
- Retry action.

Boot must not contain campaign or gameplay logic.

## 1.2 Frontend Scene

Pages:

- Account Choice.
- Lobby.
- Campaign.
- Stage Detail.
- Account.
- Settings.
- Help / Mechanic Guide when implemented.

Shared frontend UI:

- Safe Area root.
- Header.
- Loading overlay.
- Popup layer.
- Toast layer.
- Transition blocker.
- Development-only navigation root.

Only one primary frontend page is interactive at a time.

## 1.3 Gameplay Scene

Gameplay overlays:

- Gameplay HUD.
- Countdown.
- First-mechanic tutorial.
- Pause.
- Continue.
- Failure Result.
- Clear Result.
- Loading / transition blocker.

Failure and Clear remain overlays in Gameplay rather than separate Scenes.
Retry must reuse the current Stage restart path.

## 1.4 Experiment Lab

Experiment Lab remains development-only.

Its panel also exposes `SPLINE TRACK LAB`. That route hides all normal flow
roots and opens a separate non-persistent curved-track HUD with Restart and
Exit. Exit always restores the Campaign straight track and returns to Lobby;
it never writes Campaign progress or Economy state.

- It does not read or write campaign progression.
- It does not award currency.
- It does not expose account or event UI.
- It is excluded from normal release navigation.
- It keeps the existing deterministic Experiment flow.

# 2. App-entry flows

## 2.1 First run

```text
Boot
-> Create local Guest profile
-> Account Choice
-> Save Guest choice completion
-> Lobby
-> Campaign or recommended first Stage
```

Rules:

- Google or platform account linking is not required to start playing.
- The player must be able to reach gameplay offline.
- Account linking is presented as progress protection.
- Failed linking must not delete or block the local profile.
- Required legal consent and optional marketing consent remain separate.

## 2.2 Returning run

```text
Boot
-> Load local profile and settings
-> Resolve optional account state
-> Lobby
```

## 2.3 Offline run

Offline mode supports:

- Campaign progression.
- Local settings.
- Local Stage results.
- Retry, Continue, Next Stage, and Home.
- Deferred synchronization after a provider is added.

## 2.4 Initialization failure

Recoverable actions may include:

- Retry.
- Continue offline.
- Restore the latest valid local backup.
- Open support information.

# 3. Account Choice Page

## Required components

- Game title and version.
- Working `Start as Guest` action.
- Provider-backed account actions only when a real provider adapter reports
  availability.

## Rules

- Empty or nonfunctional service buttons remain hidden.
- Account Choice input must not trigger underlying gameplay input.
- The Guest action reaches Lobby only after Product Save success.
- Save failure remains on Account Choice and displays a truthful error.
- External account providers are deferred; the current implementation is Guest
  only and never presents a fake Linked state.
- Existing schema-1 users without the optional completion field intentionally
  see Account Choice once.

# 4. Lobby Page

## Purpose

Act as the changing home surface and route the player toward the next useful
action.

## Layout zones

### Header

- Profile icon.
- Display name or Guest label.
- Account / sync state.
- Data-driven currency display list.
- Notification entry when implemented.
- Settings.

### Main visual area

- Current Lobby theme.
- Player character or representative visual.
- Campaign progress summary.
- Recommended next Stage.
- Primary `Continue` or `Play` action.

### Dynamic module area

- Event or feature modules.
- New-stage notice.
- Update notice.
- Future attendance, challenge, shop, or seasonal modules.

### Navigation

Initial tabs:

- Home.
- Campaign.

Reserved tabs remain hidden until functional.

## Dynamic-module contract

A Lobby module may define:

- Stable `ModuleId`.
- Priority.
- Visibility condition.
- Optional start and end time.
- Badge state.
- Icon or visual reference.
- Navigation destination.
- Development-only flag.

If no module is active, the layout collapses without empty icons.

## Lobby-theme contract

Theme layers:

- Background.
- Midground.
- Character placement.
- Foreground effect.
- Ambient effect.
- Music reference when audio exists.
- Lighting or palette preset.

Changing theme must not require a new Lobby Scene.

# 5. Campaign Page

## Required components

- Chapter or campaign title.
- Campaign progress.
- Stage list or map.
- Current Stage focus.
- Locked, unlocked, and completed states.
- Primary mechanic icon.
- Best result summary when available.
- Back to Lobby.
- Header currency display where appropriate.

## Stage-node states

- Locked.
- Unlocked.
- Current.
- Completed.
- Mastered, reserved for a future rating rule.
- Coming Soon.

`Mastered` stays hidden until its rule exists.

## Rules

- Nodes use stable Stage IDs, not list indices.
- Stage order comes from the Stage Catalog.
- The current 20-Stage Catalog order and stable IDs remain unchanged during
  frontend-only work.
- The layout supports the current 20 Stages and the planned 36-Stage campaign
  without overlap.
- Selecting an unlocked Stage opens Stage Detail.
- Locked nodes explain their unlock requirement.

# 6. Stage Detail Page

## Required components

- Stage number and title.
- Primary mechanic.
- Stage objective.
- Difficulty indicator only after a real rule exists.
- Reward preview only after a real economy exists.
- Existing Shield and Booster selection.
- Stage-provided mechanic treatment.
- New-mechanic tutorial entry.
- Start.
- Back.

## Existing item rules

- Stages 1-5 keep Shield and Booster locked.
- Stage 6 provides Shield and Stage 7 provides Booster while manual selection
  remains locked. Provided items are visually distinct from selected items.
- Stage 8 is the first Stage that enables ordinary Shield and Booster
  selection. Later Stages preserve their catalog-authored selection rules.
- Stage 8+ displays the Product-owned Shield and Booster counts. A zero-count
  item is off and cannot be selected.
- Pressing `START` consumes one of every selected item in one Product Save
  mutation before item effects or Countdown begin. Save failure remains in
  PreRun and shows a truthful error without consuming or activating an item.
- Retry is a new Attempt and consumes retained selections again. A selection
  is turned off when its refreshed count reaches zero. Back before Start does
  not consume, and repeated Start input for one Attempt cannot consume twice.
- Stage 6/7 provided items remain free and attempt-local. They do not consume
  Product inventory. Missing Product services expose zero selectable stock;
  itemless runs and Stage-provided items remain playable.
- Stage Detail does not invent Coin prices or purchase actions. Those require
  a separately approved economy contract.

Example:

```text
SHIELD
PROVIDED BY STAGE
1 CHARGE
```

## First-mechanic guidance

The guide must describe current rules:

- Camouflage: hidden, then revealed before judgment.
- Hidden: shown, then hidden before judgment.
- Flicker: cycles colors and judges the collision-time color.
- Echo: stores one gate color and resolves before Shield.

# 7. Gameplay HUD

## Information hierarchy

Preserve the order in `ART_DIRECTION.md`:

1. Upcoming gate.
2. Player current color.
3. Next tap color.
4. Stage progress.
5. Relevant item or mechanic status.
6. Decorative information.

Normal HUD and particles must not cover the central recognition corridor.

## Required areas

### Top panel

- Stage identifier.
- Progress.
- Pause.
- Conditional Shield, Booster, and Echo status.
- Other temporary status only while relevant.

### Color guidance

- Current color.
- Next color.
- Active palette order.
- Existing color-symbol mapping.

### Center feedback

A prioritized queue for:

- Countdown.
- New mechanic.
- Echo acquired or used.
- Shield used.
- Booster activated.
- Stage clear.
- Stage failed.

Critical messages replace lower-priority messages instead of stacking.

### Input area

- Existing one-thumb input.
- Safe Area compliance.
- UI actions cannot leak into gameplay input.
- Feedback cannot hide the player or upcoming gate.

# 8. Pause Overlay

Actions:

- Resume.
- Restart.
- Settings.
- Mechanic Help.
- Leave Stage.

Rules:

- Pause is an overlay.
- Gameplay time and Flicker phase stop under the approved pause policy.
- Leaving requires confirmation.
- App backgrounding or focus loss requests Pause.
- Resume cannot duplicate countdowns or listeners.
- Restart reuses the existing Retry / PreRun contract.

# 9. Failure Result

Required information:

- Failure title.
- Player-readable reason.
- Reached progress.
- Failed Gate color.
- Player color.
- Mechanic context when useful.
- Current Coin and Heart status.
- Continue availability.
- Retry.
- Campaign.
- Home.

Rules:

- Do not expose internal implementation language.
- Flicker may show the collision-time Gate color.
- Hidden remains an ordinary mismatch result with Hidden context.
- Continue offers the next Coin price by successful Coin-Continue ordinal:
  `900`, `1900`, `2900`, then `4900` Coins for every later Coin Continue.
- One successful rewarded-ad Continue is available per attempt when a real
  provider reports availability. Taking a Coin Continue first preserves that
  rewarded-ad opportunity. Ticket and rewarded-ad Continues do not advance
  the Coin-price ordinal.
- Continue has no total count cap. Owned Tickets, the one rewarded-ad right,
  and available Coins are the only source-specific limits. Retry creates a
  fresh attempt and resets the Coin ordinal and rewarded-ad right.
- While payment or an ad request is pending, the Failure Result remains frozen
  and cannot submit another Continue. Failed, cancelled, unavailable, or stale
  requests resume the same Failure Result without consuming a Continue.
- Release builds hide the rewarded-ad action when no provider is configured.
  The current build has no provider, so the action is intentionally hidden.
- Insufficient Coins open a truthful local notice. Because Shop navigation is
  not implemented, the notice must not present a working Shop action or imply
  that a purchase occurred.

# 10. Clear Result

Required information:

- Stage Clear.
- Clear time.
- Best time.
- No-item best when relevant.
- Selected or provided items.
- Base clear reward and milestone reward as separate entries when earned.
- Newly unlocked Stage.
- New mechanic or content unlock.
- Next Stage.
- Campaign.
- Home.

Rules:

- Clear is recorded once.
- A finite Heart consumed by this successful attempt is refunded atomically
  with the Clear mutation. Unlimited-Heart attempts have nothing to refund;
  failure, Retry, and leaving Gameplay do not refund the failed attempt.
- First Clear grants the difficulty base reward once: `100` Coins for Normal,
  `200` for Hard, and `500` for Very Hard. Even-Stage milestone rewards remain
  separate from the base reward and are also first-Clear only.
- Continue clears keep current Best restrictions.
- Campaign Clear hides Replay. Experiment Lab may retain its diagnostic Replay.
- `Next Stage` resolves through stable Stage IDs and opens the unlocked next
  Stage's PreRun directly. The final Stage, or an unavailable/unpersisted next
  Stage, falls back to Lobby instead of starting an invalid destination.
- Empty reward or rating sections remain hidden.

# 11. Account Page

Initial scope:

- Show local Guest profile.
- Explain progress protection.
- Keep real providers deferred.
- Show sync state only after a provider exists.

Future states:

- Not linked.
- Linking.
- Linked.
- Offline.
- Sync pending.
- Conflict.
- Error.

Provider integration requires separate approval, package review, privacy
review, and tests.

# 12. Settings Page

Initial settings:

- Master volume.
- Music volume.
- SFX volume.
- Vibration.
- Language placeholder or supported language.
- Color / symbol accessibility information.
- Reset local progress behind confirmation.
- Account entry.
- Terms, privacy, support, version.

Settings are available from Frontend and Pause without losing the active
Stage.

# 13. Common overlays

Supported categories:

- Loading.
- Confirmation.
- Error.
- Reward.
- Unlock.
- Tutorial.
- Account.
- Cloud conflict.
- Update required.
- Maintenance.
- Toast.

Only one blocking modal is active at a time.

# 14. Navigation and Back rules

- Account Choice: platform-appropriate exit confirmation.
- Lobby: exit confirmation.
- Campaign: return to Lobby.
- Stage Detail: return to Campaign and preserve Stage focus.
- Account / Settings: return to the opening page.
- Gameplay: open Pause.
- Pause: Resume.
- Failure / Clear: require explicit Campaign or Home.
- Blocking modal: close only when cancellable.

Navigation safety:

- A transition blocker prevents double navigation.
- Pages cannot register duplicate listeners.
- Returning from Gameplay restores the intended focus.
- `PlayerPrefs` is not used as temporary page transport.

# 15. Frontend payloads

## FrontendNavigationContext

- Destination page.
- Source page.
- Focus Stage ID.
- Popup or tutorial.
- Return destination.
- Transition reason.

## StageLaunchContext

- Stable Stage ID.
- Launch source.
- Selected Shield.
- Selected Booster.
- Stage-provided mechanics.
- Retry flag.
- Seed or replay context.
- Tutorial state.

## StageResultPayload

- Stable Stage ID.
- Clear or failure.
- Passed progress.
- Failure reason.
- Failure Gate color.
- Player color.
- Continue used.
- Clear time.
- Selected and provided items.
- Gameplay statistics.
- Reward transactions.
- Newly unlocked Stage ID.

These are transport contracts. Gameplay state remains owned by the Stage
session and services.

# 16. Implementation roadmap

## Frontend Iteration 1 — Boot and AppRoot

- Boot Scene.
- AppRoot lifecycle.
- Local Guest profile.
- Local save and migration shell.
- Initial destination.
- Loading and fatal-error presentation.

Implementation status: Completed in Iteration 9.

- `Assets/Scenes/Boot.unity` is the first active Build Settings Scene.
- The Builder serializes the first active non-Boot Scene path; successful
  initialization currently enters the existing Campaign/Experiment Scene.
- Boot provides portrait Safe Area, game title, Loading, version, blocking
  local-error text, and Retry. It contains no Campaign or gameplay logic.
- Retry reuses the existing AppRoot and service graph. Re-entering Boot rejects
  a duplicate AppRoot and does not add an EventSystem.
- Title, Frontend Scene, page router, and the later Lobby/Campaign/Stage Detail
  shell remain deferred to Frontend Iteration 2+.

## Frontend Iteration 2 — Frontend shell

- Frontend Scene.
- Page router.
- Title.
- Placeholder Lobby.
- Safe Area.
- Back handling.
- Transition blocker.

Implementation status: Completed in Iteration 10.

- `Assets/Scenes/Frontend.unity` contains Title and placeholder Lobby as its
  only primary Pages. One Router owns Page, Modal, and Transition state.
- Boot enters Frontend, and Frontend uses a separately serialized active
  Campaign path. Build Settings are Boot, Frontend, Campaign.
- The shell reads an immutable Guest/Account/Settings display Context from the
  existing AppRoot. Missing AppRoot produces blocking `BOOT REQUIRED`; no
  alternate service graph is created.
- Placeholder Lobby deliberately does not read Stage progression and shows
  only `CONTINUE CAMPAIGN`. Empty future slots and legal actions are hidden.
- Existing Campaign Lobby and navigation remain authoritative after entry.

## Implemented UX slice — Account Choice, Settings, and Gameplay Pause

- Iteration 12 replaces the implemented Title Page with first-run Account
  Choice. `AccountChoiceCompleted=false` selects Account Choice; `true` selects
  Lobby before the first rendered frame.
- Frontend still contains no AppRoot. Direct Scene execution shows blocking
  `BOOT REQUIRED` and cannot save a choice or enter Campaign.
- Lobby no longer routes back to Title. Its Account action truthfully reports
  the deferred provider state, while system Back opens Exit Confirmation.
- Frontend and Gameplay instantiate the same `SettingsPanelController`
  contract. Apply persists all four values; Cancel closes without saving draft
  slider values. Save failure remains open with an error.
- Gameplay Pause is available in Countdown, Playing, and Shield Recovery.
  Back opens Pause; Back from the base Pause panel resumes; Back from Settings
  or a confirmation closes that layer. Focus loss requests Pause, and focus
  gain never resumes automatically.
- The full-screen Pause Dim covers the viewport, blocks raycasts, uses alpha
  0.85, and is drawn above gameplay guidance. The Pause panel remains inside
  Safe Area.
- Resume preserves the same Attempt. Restart confirmation delegates to the
  existing Retry-to-PreRun contract. Lobby confirmation loads the
  Builder-serialized Frontend Scene and records no attempt result.
- Scene-load failure leaves the Attempt paused and offers a recoverable close;
  transition input and repeated requests are blocked.

## Implemented UX slice - Consolidated Lobby and Settings access

- The Frontend Lobby now reads the existing Campaign store through a
  read-only `IStageProgressReader`. It displays the recommended stable Stage
  ID as number, title, mechanic, and cleared count without exposing any
  progression write operation.
- `START STAGE` queues one non-persistent launch request in the existing
  AppRoot, loads the Builder-serialized Campaign Scene, and consumes the
  request before Campaign Lobby activation. The destination opens the existing
  PreRun and item-selection flow with the same Stage seed and gate plan.
- A failed or cancelled Scene load removes the pending request. Missing or
  invalid context falls back to the existing Campaign Lobby; direct
  SampleScene, development, and Experiment Lab entry remain unchanged.
- Frontend and Pause instantiate the same Settings panel and controller for
  Notifications, Music, SFX, Vibration, Terms, Privacy, and Support. Pages do
  not activate peers or load Scenes directly; their existing router and Pause
  coordinator remain the only state owners.
- Music and SFX toggles persist zero when OFF and restore the last non-zero
  value when ON. Master remains persisted and applied but hidden. Notification
  is preference-only and clearly says delivery is not implemented.
- One `ProductLinkConfiguration` supplies HTTPS-only destinations. Empty
  prototype values disable the development buttons and open no browser.
- The Pause button is generated under `GameplayHudRoot` in the upper-right
  Safe Area with a minimum 44 by 44 hit region, outside Stage/Shield HUD and
  above the gameplay tap surface.

## Implemented UX slice — Automatic Lobby progression foundation

- Lobby reads schema-2 Product progression rather than PlayerPrefs and shows
  Coins, Shield/Booster inventory, theme identity, unlocked visual steps, next
  automatic upgrade target, and a pending reward summary.
- Every two first-cleared Stages advances one milestone. There are 18 stable
  milestones across the planned 36-Stage campaign, grouped as six visible
  upgrades in each of three themes.
- Milestones apply automatically; the player does not spend or place an
  upgrade. Opening Lobby acknowledges presentation only and cannot grant the
  reward again.
- Frontend-origin Clear and Failure home actions load Frontend Lobby. Direct
  SampleScene and development paths still return to Campaign Lobby.
- Theme color blocks and upgrade panels are structural placeholders for human
  flow validation, not final art or proof of visual quality.

## Implemented UX slice — Iteration 17 Campaign learning curve

- The Campaign contains 20 contiguous stable-ID Stages at Catalog revision 7.
- Stages 1–5 are the color/rhythm foundation. Stage 6 provides Shield and
  Stage 7 provides Booster with selection locked; Stage 8 is a clean
  three-color application Stage and the first selectable-item Stage.
- Camouflage 9–11, Fog 12–14, Ice 15–17, and Echo 18–20 use contiguous
  intro/practice/mastery blocks. Their active-color progression is `2/2/3`.
- Hidden 21–23 and Flicker 24–26 are planned Campaign blocks. Their existing
  Experiment Lab implementations remain available, but Campaign presentation
  must not imply that those deferred Stages are playable. Stages 27–36 remain
  later content.

## Implemented UX slice — Iteration 18 owned start items

- PreRun replaces the obsolete free/unlimited claim with owned counts and a
  one-item-per-selected-type Start contract.
- Product inventory validation and the two optional decrements are atomic.
  Only a successful durable save permits `StageSession.SelectItems` and
  Countdown. No schema migration or Core item-effect change is introduced.
- Retry, Back, duplicate Start, provided Stage 6/7 items, zero stock, stale
  stock, save failure, and missing-Product behavior follow the item rules
  above. Coin purchase, Continue economy, Shop, ads, and IAP remain deferred.

## Implemented UX slice — Iteration 19 Continue economy (historical)

The original prices and three-Continue cap below are superseded by Iteration
23. This section is retained as implementation history.

- Failure Result exposes Coin Continue at `300`, `600`, and `900` Coins by
  successful Coin-Continue ordinal, with a maximum of three total Continues
  per attempt.
- A successful rewarded-ad completion authorizes one Continue per attempt.
  Coin-first does not consume that right; failed, cancelled, unavailable, and
  stale callbacks do not mutate the attempt or Product balance.
- Coin spend is persisted before gameplay resumes. Save or balance failure
  leaves the Failure Result frozen and truthful, and duplicate input cannot
  create a second spend.
- Retry creates a new attempt with the first Coin price and a fresh ad right.
  Production has no simulated-success ad path: without a configured provider,
  the ad action is hidden. A real advertising SDK remains deferred.

## Frontend Iteration 3 — Lobby, Campaign, Stage Detail

Implementation status: Partially completed through Iteration 15.

- Consolidated recommended-Stage Lobby, read-only Campaign progress, and
  direct existing-PreRun launch are implemented.
- Automatic Lobby progression and result-to-Frontend return are implemented.
  Full Campaign page, Stage Detail, final theme art, and data-driven expansion
  modules remain deferred.

- Data-driven Lobby modules.
- Lobby theme slot.
- Campaign page from Stage Catalog.
- Stage Detail.
- Existing item selection.
- Stage 1–20 launch.

## Frontend Iteration 4 — Gameplay shell

- HUD integration.
- Pause.
- Gameplay entry context.
- Background pause.
- Settings access.

## Frontend Iteration 5 — Results

- Failure and Clear.
- Continue.
- Retry.
- Next Stage.
- Campaign.
- Home.
- Progress save.

## Frontend Iteration 6 — Expansion slots

- Account page with local service.
- Currency header with local wallet.
- Event-module data.
- Theme switching.
- Popup queue.
- Local analytics log.

# 17. Acceptance path

```text
Fresh install
-> Guest profile created
-> Title
-> Lobby
-> Campaign
-> Stage 1 Detail
-> Gameplay
-> Failure
-> Retry
-> Clear
-> Stage 2 unlocked
-> Next Stage
-> App close
-> App restart
-> Stage progress restored
```

Additional acceptance:

- Offline play works.
- Login failure cannot block local play.
- Stage progress uses stable IDs.
- The current Stage 1–20 Catalog remains deterministic.
- Retry resets the attempt-local Continue economy, while Goal rules remain
  unchanged.
- Event modules and Lobby themes are data-driven.
- Empty future-feature slots remain hidden.
- Portrait Safe Area works.
- Back and background flows do not duplicate navigation or gameplay.
- Experiment Lab remains isolated.
- No package, provider SDK, ad, IAP, or network dependency is added without
  separate approval. Iteration 20 approves the Unity IAP package baseline but
  does not make a Shop or purchase action available.

## Iteration 14 — PreRun Back and toggle-state hotfix

- Campaign records a Frontend entry origin only when the AppRoot-owned launch
  context is successfully consumed. It is Scene-local and is never inferred
  from Scene names or active UI.
- PreRun Back from that origin reuses the Builder-serialized Frontend path and
  existing Scene transition loader. The legacy Campaign Lobby never activates
  during the return. Load failure stays in PreRun, releases the input block,
  and permits retry.
- Without a launch context, direct SampleScene, development, and Experiment
  navigation retain the existing Campaign Lobby and Lab paths.
- Each shared Settings toggle owns one state label that switches between
  `ON` and `OFF`; separate overlapping state objects are forbidden.

## Iteration 20 — approved Lobby and Shop information architecture reference

The supplied reference screens define information hierarchy and navigation,
not reusable art assets or an exact visual copy.

- Persistent top resource header: profile access, Coin balance and purchase
  entry, Heart state and purchase entry, then Settings.
- Persistent bottom navigation: Shop, competitive/leaderboard slot, Home,
  Journey/progression slot and Collection slot. Only implemented destinations
  may be actionable; unavailable modules remain hidden or truthfully disabled.
- Home keeps one dominant central Lobby scene, a compact progression/reward
  rail above it and one primary Stage action near the lower center.
- Shop is a portrait vertical scroll page with a persistent Coin header,
  visually separated special-offer and bundle sections, explicit reward
  contents and one localized store-price action per product card.
- Journey uses a vertical milestone path with current position, claimed state
  and next rewards. Collection uses a category grid with progress, reward
  previews and a clear category state.
- The immediate Shop foundation may implement Home and Shop navigation only.
  Leaderboard, Journey and Collection require separate product iterations and
  must not be presented as working placeholders.
## Iteration 21 failure offer hierarchy

- Campaign failure actions are ordered by owned Continue Ticket, real
  rewarded ad availability, then Coin Continue. A zero Ticket balance hides
  the Ticket action; an unavailable rewarded provider remains hidden.
- All authorization/save failures keep the failed run frozen and leave Retry
  and Lobby available. No placeholder can simulate a successful purchase or
  advertisement.
- Lobby and Shop composition from the Iteration 20 reference remains deferred;
  this iteration changes only the failure-offer hierarchy needed by the newly
  modeled Continue Ticket reward.

## Implemented UX slice — Iteration 23 result-loop closure

- Failure Result keeps Coin and Heart status visible and uses the uncapped
  `900 / 1900 / 2900 / 4900+` Coin Continue schedule. Ticket and the single
  rewarded-ad right remain independent sources; the absent ad provider keeps
  the ad action hidden.
- Insufficient Coins show a truthful local notice. Shop navigation remains
  unavailable and is never presented as a successful or working action.
- Before the first Continue-countdown frame, retained gate modifier visuals
  and safe colors match resumed gameplay, while consumed Shield, Booster,
  Echo, local grants, and their presentation are already inactive.
- Clear refunds the finite Heart spent by that successful attempt. It presents
  the difficulty base reward separately from any even-Stage milestone reward.
- Campaign Clear hides Replay. `Next Stage` opens the unlocked next Stage's
  PreRun directly; final-Stage and invalid/unpersisted destinations return to
  Lobby.
- Final Lobby composition, Fog curtain redesign, and authored Ice approach are
  deferred to their dedicated visual/gameplay iterations.

## Implemented UX slice — Iteration 25 timed Campaign Fog

- Campaign Stages 12–14 use one timed world-space curtain instead of per-gate
  nearest-two neutralization. It triggers when the first Fog gate is next,
  follows at 1.5 seconds of travel distance, holds for 1.5 seconds and fades
  over 0.5 seconds.
- Pause and Continue countdown freeze it; Continue preserves/repositions it
  and Retry resets it. This changes no Frontend navigation or result contract.
- Final Lobby composition and authored Ice approach remain deferred.

## Implemented UX slice — Iteration 29 Lobby visual hierarchy

- Lobby has three stable zones: Profile / Coins / Hearts / Settings at the
  top, current theme and automatic upgrade progress in the center, and a
  compact current-Stage card with one primary `PLAY` action at the bottom.
- The duplicate visible `LOBBY` title and second account label are removed.
  Shield and Booster counts remain secondary chips rather than competing
  headers. A milestone summary appears only while an unpresented reward exists.
- The Stage card displays Stage number, title, primary mechanic, authored
  difficulty and cleared count. `PLAY` keeps the existing stable-ID launch.
- Heart state refreshes while Lobby is visible. It shows `5/5` when full, a
  next-heart countdown while recharging, or remaining unlimited time. The UI
  reads Product state and does not own recharge or persistence rules.
- Shop, IAP product cards and bottom navigation destinations remain separate
  iterations.

## Implemented UX slice — Iteration 30 Theme 1 visual slice

- `LobbyThemeVisualCatalog` supplies three theme definitions without adding a
  Scene. Theme 1 assigns original Background, Midground reactor and Foreground
  energy artwork; Themes 2 and 3 retain palette-only fallbacks.
- Frontend Builder owns the Sprite import settings, catalog asset, serialized
  layer references and six energy-node positions. Rebuilding the Scene is
  idempotent and creates no duplicate visual root.
- `LobbyProgressionPanel` selects the theme from the existing automatic
  milestone count and activates the same six local milestone slots. It does not
  write Product state or introduce a manual Lobby action.
- Decorative layers are non-interactive and remain behind persistent resources
  and the Stage card. Existing `PLAY`, Back, Settings and stable-ID launch paths
  are unchanged.

## Implemented UX slice — Iteration 26 Fog visibility curve

- Campaign Fog enters over `0.5 seconds`, stays fully opaque for the authored
  Stage 12 / 13 / 14 duration of `5 / 6 / 7 seconds`, then exits over
  `0.5 seconds`.
- The values come from Stage Catalog revision 9, so future Fog stages can tune
  visibility pressure without changing navigation or hard-coding Stage IDs in
  the controller.
- Final Lobby composition and authored Ice approach remain deferred.
