# Frontend Flow

## Status

Approved commercial-flow design baseline. Implementation pending.

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
-> Title
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
- Campaign currently contains Stages 1-13.
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

- Title.
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
-> Required consent page or popup
-> Title
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
-> Title
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

# 3. Title Page

## Required components

- Game logo.
- `Tap to Start` or equivalent.
- Account state: Guest, Linked, Offline, or Sync Warning.
- Account-link entry.
- Settings.
- Terms.
- Privacy.
- Support.
- App version.

## Rules

- Empty or nonfunctional service buttons remain hidden.
- Title input must not trigger underlying gameplay input.
- Account linking returns without losing navigation state.
- External account providers are deferred; initial implementation is Guest
  only.

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
- Stage 1-13 authored data remains unchanged during frontend work.
- The layout supports at least 13 Stages without overlap.
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
- Stage-provided items are visually distinct from selected items.
- Other Stages preserve their catalog-authored selection rules.
- Stage Detail does not invent costs or inventory balances.

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
- Continue availability.
- Retry.
- Campaign.
- Home.

Rules:

- Do not expose internal implementation language.
- Flicker may show the collision-time Gate color.
- Hidden remains an ordinary mismatch result with Hidden context.
- Continue preserves the existing one-free-Continue contract.
- Future ad or currency Continue must enter through a service boundary.

# 10. Clear Result

Required information:

- Stage Clear.
- Clear time.
- Best time.
- No-item best when relevant.
- Selected or provided items.
- Rewards only when an economy exists.
- Newly unlocked Stage.
- New mechanic or content unlock.
- Next Stage.
- Retry.
- Campaign.
- Home.

Rules:

- Clear is recorded once.
- Continue clears keep current Best restrictions.
- `Next Stage` resolves through stable Stage IDs.
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

- Title: platform-appropriate exit confirmation.
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

## Frontend Iteration 2 — Frontend shell

- Frontend Scene.
- Page router.
- Title.
- Placeholder Lobby.
- Safe Area.
- Back handling.
- Transition blocker.

## Frontend Iteration 3 — Lobby, Campaign, Stage Detail

- Data-driven Lobby modules.
- Lobby theme slot.
- Campaign page from Stage Catalog.
- Stage Detail.
- Existing item selection.
- Stage 1-13 launch.

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
- Stage 1-13 remain deterministic.
- Retry, Continue, and Goal rules remain unchanged.
- Event modules and Lobby themes are data-driven.
- Empty future-feature slots remain hidden.
- Portrait Safe Area works.
- Back and background flows do not duplicate navigation or gameplay.
- Experiment Lab remains isolated.
- No package, provider SDK, ad, IAP, or network dependency is added without
  separate approval.
