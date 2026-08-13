# Product Systems

## Status

Approved commercial-systems design baseline. Local Profile/Settings foundation
and schema-3 Campaign/Economy/Lobby, Heart, timed entitlement, Continue Ticket
and commerce-reward foundations are implemented. Unity IAP 5 is installed as
an approved package baseline; runtime purchasing and external validation
providers remain pending.

This document defines local-first service boundaries and data ownership for
the commercial shell. Iteration 21 authorizes local idempotent reward grants
but not a live purchase flow or receipt authority. Login, cloud save,
advertising provider, analytics SDK and remote configuration remain excluded.

### Editor Developer Console

- `Window > Color Gate Runner > Developer Console` is the single Editor-only
  authority for development progression and wallet cheats.
- Campaign reset clears stable-ID records/unlocks and derived Lobby milestone
  presentation while preserving Economy. Economy reset clears wallet,
  inventory, Hearts, timed entitlement and commerce ledger while preserving
  Campaign. Both operations are explicit and independently confirmed.
- Exact Economy edits use the Product session's clone-save-publish boundary;
  invalid values and write failure publish nothing. In Play Mode the active
  AppRoot is mandatory. In Edit Mode an explicit mutation initializes and
  writes the production-format local save; simply opening or reloading the
  window does not write.
- `PLAY STAGE` bypasses only the unlock check for one non-persistent launch. It
  does not grant clears, alter Highest Unlocked, or create Stage records.

## Purpose

Prepare the project for account, persistence, progression, economy, event
content, settings, and telemetry without coupling external providers to
gameplay or creating duplicate sources of truth.

The initial product implementation is local and offline-capable.

## Documentation ownership

This document owns:

- Persistent `AppRoot` service boundaries.
- Profile, account, save, progression, economy, content, settings, popup, and
  analytics contracts.
- Local-first implementations.
- Save schema, versioning, migration, backup, and recovery.
- Future provider integration boundaries.
- Product-system acceptance.

This document does not own:

- Scene and page composition: `FRONTEND_FLOW.md`.
- Gameplay and Stage rules: `GAME_DESIGN.md`.
- Visual direction: `ART_DIRECTION.md`.
- Historical decisions: `DECISIONS.md`.
- Current implementation: `CURRENT_STATUS.md`.
- Detailed test inventory: `TEST_PLAN.md`.

# 1. Product-system principles

- Local play works without an account provider or network.
- Guest profile is the default first-run state.
- External services are adapters, not gameplay dependencies.
- Stable IDs identify Stages, currencies, content modules, rewards, and
  transactions.
- Save writes are atomic or recoverable.
- Save schema is versioned and explicitly migrated.
- Gameplay Core remains pure and does not reference Unity services.
- Product services do not own gate judgment, Stage simulation, or movement.
- UI pages do not become data sources.
- A future server may validate provider data, but local interfaces remain
  independently testable.
- Empty future systems remain hidden.
- Avoid singleton-heavy architecture. One composition root owns lifetimes;
  consumers use interfaces or explicit references.
- Product services are not polled every frame.
- No provider package is added without separate approval and documented need.

# 2. AppRoot

`AppRoot` is the persistent composition root created from Boot.

Responsibilities:

- Construct production local services.
- Define initialization order.
- Expose service interfaces to Scene entry points.
- Survive normal Scene changes.
- Dispose subscriptions and temporary provider connections.
- Reject duplicate AppRoot creation.
- Support test replacements.

Recommended service graph:

```text
AppRoot
├─ SceneFlowService
├─ ProfileService
├─ AccountService
├─ SaveService
├─ StageProgressService
├─ EconomyService
├─ ContentService
├─ AudioService
├─ SettingsService
├─ LocalizationService
├─ PopupService
├─ AnalyticsService
└─ ClockService
```

Not every service must exist in the first Iteration. Active pages must never
receive an unexplained null service.

# 3. Initialization order

Recommended Boot sequence:

1. Create AppRoot.
2. Load project defaults.
3. Initialize platform-safe local paths.
4. Load Settings.
5. Load or create Profile.
6. Validate and migrate Save.
7. Initialize Stage Progress.
8. Initialize Economy.
9. Initialize local Content definitions.
10. Initialize Account local state.
11. Initialize Audio and Localization.
12. Start the local Analytics session.
13. Enter Title or the configured frontend destination.

Optional external providers initialize only after local data is safe.

A failed optional provider must not invalidate successful local
initialization.

# 4. Profile and account

## 4.1 Local profile

The local profile owns:

- Stable local Profile ID.
- Created time.
- Last played time.
- Display name.
- Guest / linked state.
- Account-choice onboarding completion.
- Tutorial acknowledgement.
- Required consent versions when implemented.
- Save revision.
- Optional provider references after integration.

Profile identity is separate from Unity device identifiers.

`AccountChoiceCompleted` is Profile/Account onboarding state, not a user
setting. Schema-1 saves written before this field existed interpret it as
`false`, so those users see Account Choice once. Repairing or resetting only
the Settings section must not change this value.

## 4.2 Account states

Supported model states:

- Guest.
- Link Available.
- Linking.
- Linked.
- Offline.
- Sync Pending.
- Conflict.
- Recoverable Error.
- Blocking Error.

The initial shell implements Guest and local error states only.

## 4.3 Account service contract

Conceptual interface:

```text
IAccountService
- CurrentState
- CurrentProfileId
- CreateGuestProfile()
- LinkAccount(provider)
- SignOut()
- RequestSync()
- ResolveConflict(choice)
```

Initial implementation:

```text
LocalAccountService
```

Deferred adapters may include Google, Apple, or another platform provider.
The frontend must not assume Google is the only possible provider.

## 4.4 Guest-first rules

- First run creates a local Guest profile automatically.
- Guest can complete campaign content.
- Guest progression is saved locally.
- Linking preserves or explicitly reconciles local progress.
- Failed linking returns to Guest state.
- Sign-out cannot silently destroy local data.
- Account deletion requires an explicit policy before release.

## 4.5 Future cloud conflict

A future conflict payload compares:

- Local and cloud profile revisions.
- Highest unlocked Stage.
- Stage completion records.
- Best times.
- Wallet transactions.
- Last modified time.
- Data version.

The UI presents Local, Cloud, or Merge only when the service can safely
support that operation. Arbitrary automatic merge is forbidden.

# 5. Save system

## 5.1 Save root

Recommended structure:

```text
SaveRoot
- SchemaVersion
- SaveRevision
- Profile
- Settings
- StageProgress
- Economy
- TutorialState
- ContentState
- LocalAnalyticsState
- LastWriteUtc
```

Simulation artifacts and development reports are not player save data.

## 5.2 Save service contract

```text
ISaveService
- Load()
- Save(snapshot)
- SaveSection(section)
- CreateBackup()
- RestoreBackup()
- Validate()
- Migrate()
- ResetLocalProgress()
```

Initial implementation may use one versioned JSON file or another
project-approved local format.

`PlayerPrefs` must not become the main structured save. It may remain for tiny
platform settings or legacy adapters while the versioned save becomes
authoritative.

## 5.3 Atomic-write policy

Recommended write process:

1. Serialize to a temporary file.
2. Flush and validate the temporary file.
3. Move the previous primary file to backup.
4. Atomically replace the primary file where supported.
5. Keep the latest known-good backup.
6. Record save revision and timestamp.

A failed write preserves the last valid save.

## 5.4 Save versioning

- Every save has an integer `SchemaVersion`.
- Each migration is explicit and one-way.
- Migrations are tested from every supported old version.
- Unknown future versions do not load as empty data.
- Corrupt optional sections fall back only when their contract permits it.
- Stage progress never uses display order as identity.
- Existing Stage IDs remain stable.

## 5.5 Save triggers

Save after:

- First Guest creation.
- Settings change.
- Stage clear.
- Stage unlock.
- Best-time update.
- Continue result when persistent data changes.
- Reward transaction.
- Account-link or sync-state change.
- Tutorial acknowledgement.
- App pause or quit when data is dirty.

Do not save every frame or every gate.

# 6. Stage progression

## 6.1 Ownership

- The Stage Catalog owns authored Stage definitions.
- `StageProgressService` owns persistent campaign progression.
- The Stage runtime owns the current attempt.
- UI only displays service state.

## 6.2 Stage record

Recommended fields:

```text
StageProgressRecord
- StageId
- Unlocked
- Cleared
- ClearCount
- BestTime
- BestNoItemTime
- LastClearTime
- FirstClearRewardClaimed
- TutorialSeen
```

Only approved game rules are shown to the player.

## 6.3 Progression service contract

```text
IStageProgressService
- IsUnlocked(stageId)
- GetRecord(stageId)
- GetRecommendedStageId()
- RecordStageResult(payload)
- GetNextStageId(stageId)
- ResetProgress()
```

Rules:

- Stable Stage IDs are authoritative.
- Catalog order determines normal next-Stage progression.
- A clear is recorded exactly once per result.
- Continue clears preserve existing Best restrictions.
- Stage 1–20 definitions and deterministic plans are not duplicated in save.
- Unknown saved Stage IDs are not remapped by array index.

# 7. Economy

## 7.1 Current scope

Schema-2 Product Save owns Coins plus Shield and Booster inventory. Approved
Stage and Lobby rewards grant them, Lobby displays them, and Stage 8+ may
consume owned start items. Coin spending, prices, energy, ads, and purchases
remain unavailable until their rules are separately approved.

## 7.2 Economy service contract

```text
IEconomyService
- GetBalance(currencyId)
- CanSpend(cost)
- Grant(transaction)
- Spend(transaction)
- GetLedger()
```

## 7.3 Transaction model

Every idempotent reward transaction uses:

- Stable Transaction ID.
- Currency ID.
- Amount.
- Reason.
- Source content or Stage ID.
- Timestamp.
- Save revision.
- Idempotency key.

Duplicate reward processing must not grant twice.

An Attempt-start item spend is intentionally repeatable rather than
idempotent: each accepted `START`, including a Retry, consumes one of every
selected item. Both optional decrements are validated on a cloned Product
snapshot and saved once; insufficient inventory or save failure publishes
nothing. An itemless Start is a successful no-op and performs no save.

## 7.4 Wallet rules

- Balances cannot silently become negative.
- Unknown currency IDs are rejected.
- UI does not directly edit balances.
- Stage Result proposes rewards; Economy applies validated transactions.
- Development cheats remain development-only.
- Stage 8+ presentation reads Product inventory and never owns a shadow
  balance. Zero-count items cannot be selected.
- Stage 6/7 provided items are attempt-local and never spend inventory.
- A missing Product session cannot grant selectable items for free. Itemless
  and Stage-provided Attempts remain available.
- Premium currency and real-money purchase are deferred.

# 8. Continue service boundary

The current game authorizes Continue through an attempt-local policy and a
Product economy boundary:

```text
AttemptContinuePolicy
- GetCoinOffer(coreContinueCount, coinBalance)
- ConfirmCoinContinue(coreContinueCount, chargedAmount)
- CanRequestRewardedAd(coreContinueCount, service)
- ApplyRewardedAdResult(coreContinueCount, result)

LocalProductSession
- SpendContinueCoins(transactionId, amount)
```

- Coin prices are `300`, `600`, and `900` by successful Coin-Continue ordinal.
- The maximum is three total Continues per attempt across Coin and rewarded-ad
  sources. Core owns the authoritative Continue count; Product policy consumes
  that count as input rather than creating a second gameplay counter.
- One completed rewarded ad is allowed per attempt. Coin-first preserves the
  ad right. Retry creates a fresh policy state.
- Failed, cancelled, unavailable, or stale ad results do not mutate policy or
  gameplay. The Failure Result remains frozen while an asynchronous request is
  pending.
- Coin spend clones, validates, saves, and only then publishes. Its stable
  attempt transaction ID makes duplicate input and replay idempotent. Invalid
  requests, insufficient funds, and write failure publish no partial state.
- The default release ad service is truthfully unavailable. Presentation hides
  the action when no real provider is configured; it never simulates success.

Deferred sources and integrations:

- Actual rewarded-advertising SDK and provider adapter.
- Premium currency.
- Daily free allowance.

Gameplay Continue state remains owned by the current Stage attempt.

# 9. Content and Lobby modules

## 9.1 Content service scope

`ContentService` supplies local definitions for:

- Lobby modules.
- Lobby themes.
- Tutorials.
- Feature visibility.
- Announcement placeholders.
- Chapter presentation.

Initial implementation loads local ScriptableObjects or project-owned data.
Remote configuration is deferred.

## 9.2 Lobby module definition

Recommended fields:

```text
LobbyModuleDefinition
- ModuleId
- Priority
- Enabled
- DevelopmentOnly
- StartUtc
- EndUtc
- Badge
- VisualReference
- Destination
- VisibilityRule
```

Rules:

- Stable IDs.
- Deterministic priority ordering.
- No empty release placeholder.
- Invalid time ranges disable the module and report validation.
- Local clock is not trusted for competitive or paid entitlement.

## 9.3 Lobby theme definition

```text
LobbyThemeDefinition
- ThemeId
- Background
- Midground
- CharacterPlacement
- ForegroundEffect
- AmbientEffect
- MusicId
- LightingPreset
- StartUtc
- EndUtc
- UnlockCondition
```

Theme changes presentation only. It must not change Stage balance,
progression, or rewards.

# 10. Settings and audio

## 10.1 Settings service

Recommended settings:

- Master volume.
- Music volume.
- SFX volume.
- Vibration.
- Language.
- Accessibility preferences.
- Reduced effects when implemented.
- Settings-specific first-run flags only; account onboarding is Profile state.
- Title-skip preference when approved.

Conceptual interface:

```text
ISettingsService
- Current
- Apply(change)
- ResetDefaults()
- Save()
```

The implemented settings panel persists Master, Music, SFX, and Vibration.
Master is currently applied through `AudioListener.volume`. Music and SFX are
stored for future content-specific audio targets and do not yet change a
runtime mixer or source. A disabled Vibration setting prevents the existing
Booster haptic request from being issued at all.

## 10.2 Audio service

Full audio and BPM integration remain deferred.

The boundary may own:

- Music volume.
- SFX volume.
- UI sound requests.
- Scene or page music requests.
- Pause and resume.

It must not introduce music-synchronized gate planning without a separate
gameplay contract.

# 11. Localization

Conceptual interface:

```text
ILocalizationService
- CurrentLocale
- Get(key)
- SetLocale(locale)
- LocaleChanged
```

Initial implementation may use one local language table.

Text keys are presentation-owned. Core enums are not player-facing strings.

# 12. Popup service

Responsibilities:

- Queue blocking modals.
- Show nonblocking toasts.
- Prevent duplicate modal instances.
- Cancel stale requests after navigation.
- Return explicit user results.
- Respect modal priority.

Conceptual interface:

```text
IPopupService
- ShowModal(request)
- ShowToast(request)
- Close(id)
- Clear(scope)
```

A popup cannot become an alternate gameplay-state owner.

# 13. Analytics

## 13.1 Initial scope

No external analytics SDK is introduced.

Initial implementation:

```text
LocalAnalyticsService
```

It records development-safe local events and can be disabled.

## 13.2 Event contract

Initial event names:

- `app_start`
- `boot_complete`
- `title_enter`
- `lobby_enter`
- `campaign_enter`
- `stage_detail_enter`
- `stage_start`
- `stage_fail`
- `stage_continue`
- `stage_retry`
- `stage_complete`
- `stage_quit`
- `settings_change`
- `account_link_start`
- `account_link_result`

Recommended common fields:

- Anonymous local session ID.
- App version.
- Build version.
- Stage ID when relevant.
- Launch source.
- Selected items.
- Provided items.
- Attempt result.
- Continue used.
- Duration.
- Mechanic identifiers when relevant.

Rules:

- No advertising ID.
- No device fingerprint.
- No personal account token.
- No event upload in the local implementation.
- Analytics cannot affect gameplay or navigation.
- Tests cannot require network access.

# 14. Clock and time

Conceptual interface:

```text
IClockService
- UtcNow
- MonotonicNow
```

Uses:

- Save timestamps.
- Module preview windows.
- Local analytics.
- Future event schedules.

Gameplay timing does not use this clock. Flicker and Stage timing continue
using their approved gameplay-time sources.

# 15. Service errors

Use typed errors.

Recommended categories:

- Initialization.
- Save Read.
- Save Write.
- Save Migration.
- Account.
- Sync.
- Economy.
- Content.
- Network.
- Unknown.

Each error defines:

- Recoverable or blocking.
- Player-facing message key.
- Retry support.
- Offline fallback support.
- Development diagnostic detail.

Provider tokens or personal information never appear in logs.

# 16. Data ownership matrix

| Data | Authoritative owner |
|---|---|
| Stage definition | Stage Catalog |
| Current attempt | Stage runtime / Core session |
| Gate judgment | Core gameplay |
| Campaign unlock | StageProgressService |
| Best times | StageProgressService / Save |
| Local profile | ProfileService |
| Account-choice onboarding | ProfileService |
| Account-link state | AccountService |
| Currency balance | EconomyService |
| Lobby modules | ContentService |
| Lobby theme | ContentService |
| Settings | SettingsService |
| Page state | Frontend router |
| Popup state | PopupService |
| Local event log | AnalyticsService |

No row may have two production owners.

# 17. Testing contract

## EditMode / pure tests

- AppRoot graph contains one instance of each enabled service.
- Initialization order is deterministic.
- Guest creation is deterministic when identifiers are injected.
- Save roundtrip preserves supported fields.
- Atomic-write failure preserves the prior valid save.
- Migration covers every supported schema version.
- Unknown future schema fails safely.
- Stable Stage IDs survive save roundtrip.
- Stage clear records exactly once.
- Continue clear applies existing Best restrictions.
- Reward transaction is idempotent.
- Negative wallet balance is rejected.
- Lobby modules sort by priority and obey visibility.
- Invalid module ranges are rejected.
- Popup queue preserves priority and clears stale requests.
- Local analytics contains no provider token or personal field.
- Provider absence does not block initialization.

## PlayMode / integration tests

- Boot creates exactly one AppRoot.
- Re-entering Boot does not duplicate services.
- Fresh install creates a Guest and reaches Title.
- Existing save reaches Title with progress intact.
- Corrupt primary save restores the valid backup.
- Frontend reads Stage progress without owning it.
- Stage result records, saves, unlocks, and returns correctly.
- App pause saves dirty data and Gameplay opens Pause.
- Account failure leaves local play available.
- Empty economy and content systems hide their UI.
- Reset progress requires confirmation and does not alter project assets.

## Manual acceptance

- App restart preserves Stage progress.
- Offline launch reaches Gameplay.
- Save failure messaging is understandable.
- Future account linking does not threaten local progress.
- Currency appears only when real rules exist.
- Event modules disappear cleanly when inactive.
- Lobby themes do not obscure navigation.
- Background and foreground transitions do not duplicate popups or saves.

# 18. Implementation roadmap

## Product Iteration 1 — Local foundation

- AppRoot.
- ProfileService.
- LocalAccountService.
- SaveService.
- SettingsService.
- ClockService.
- Boot initialization.

Implementation status: Completed in Iteration 9.

- One persistent AppRoot composes Clock, Profile, Local Account, Settings,
  Local Save, and the ordered initialization pipeline without service-level
  singletons or a mutable locator.
- The Product assembly is Unity-independent. Unity adapters own persistent
  path, JSON, platform replacement policy, and Scene presentation.
- Schema 1 originally persisted Profile and Settings plus revision/write
  metadata. Iteration 15 migrates it one-way to schema 2. Guest identity
  remains stable across reloads and does not use a device ID.
- Primary/temp/backup validation, corrupt-primary recovery, schema-0 migration,
  future-schema blocking, native atomic preference, and WebGL recoverable-only
  replacement are implemented and tested.
- Stage PlayerPrefs were the progression authority through Iteration 14.
  Iteration 15 imports them once into schema 2 and retains them only as an
  untouched rollback source.
- Frontend Iteration 2 now consumes the initialized AppRoot graph only at its
  Scene composition boundary and converts Profile, Account, and Settings into
  an immutable display Context. It creates no service, writes no Product or
  Stage data, and blocks direct Campaign entry when AppRoot is absent.
- Iteration 12 adds a thin `LocalProductSession` coordinator. It clones the
  current Product snapshot, asks the existing Save service to persist the
  candidate, and publishes Profile/Settings state through one rebind path only
  after success. Failed writes leave all public state unchanged, so the
  session is not a second save authority.
- `AccountChoiceCompleted` is an optional schema-1 Profile field. The schema
  version does not increase, older files read it as `false`, and Settings
  validation or repair is independent from onboarding state.
- Frontend and Gameplay share the same Settings mutation path. Master Volume
  and Booster vibration have runtime consumers; Music/SFX remain persistence-
  only until real audio content exists.
- Schema 1 Settings also persist `NotificationEnabled` plus
  `LastNonZeroMusicVolume` and `LastNonZeroSfxVolume`. Missing fields use
  `false`, `1.0`, and `1.0` without a schema bump. Settings repair remains
  independent from Profile onboarding.
- Music/SFX toggles write zero when disabled and restore their last non-zero
  value when enabled. Master is preserved and remains the only value applied
  to `AudioListener.volume`; it is not exposed by the consolidated Settings
  UI.
- Frontend still consumes a read-only campaign view. Campaign mutations now
  pass through the Product-backed Stage store and transactional Product
  session rather than PlayerPrefs.
- AppRoot owns a non-persistent one-shot Campaign launch request containing a
  stable Stage ID. It is neither serialized nor exposed as a global mutable
  locator and is consumed by Campaign before Lobby activation.
- External product destinations are Presentation configuration, not Product
  state. One `ProductLinkConfiguration` accepts absolute HTTPS only, and an
  injectable URL opener keeps tests and unavailable prototype links truthful.

## Product Iteration 2 — Progression integration

Implementation status: Completed in Iteration 15.

- Schema 2 owns stable-ID Campaign records, Highest Unlocked Stage ID,
  Economy balances/inventory, applied transaction IDs, and Lobby milestone
  applied/presented counters.
- Production Boot imports legacy Campaign PlayerPrefs once and leaves those
  keys untouched for rollback safety. After successful import, Campaign reads
  and writes Product save only.
- A Stage clear writes its record, Highest Unlocked ID, first-clear reward,
  and eligible Lobby milestone in one clone-save-publish transaction. Save
  failure leaves every published section unchanged.
- Transaction IDs make migration and repeated result ingestion idempotent.
  First clear grants 100 Coins; even Stages through 36 grant one milestone
  with the approved starter coin and Shield/Booster reward policy.
- Frontend read models and Campaign store adapters share the Product service;
  display order is resolved through the Stage Catalog only at the Unity edge.
- At Iteration 15 completion, external ads, IAP, Hearts, Continue pricing,
  item purchasing, and real Lobby art/theme content were still deferred.

### Iteration 17 Campaign-content integration

- Stable IDs, Product progression ownership and reward transaction IDs do not
  change when Stage content is reauthored.
- Catalog revision 7 keeps 20 contiguous stable IDs. The active Campaign is
  Stages 1–5 foundation, Stage 6/7 provided-item teaching, Stage 8 clean
  three-color application, then Camouflage 9–11, Fog 12–14, Ice 15–17 and
  Echo 18–20 intro/practice/mastery blocks. Hidden 21–23, Flicker 24–26 and
  Stages 27–36 remain future Catalog content rather than saved placeholders.
- At Iteration 17 completion, Stage 6/7 provided items remained attempt-local
  grants and never consumed
  Product inventory. Manual selection is locked for Stages 1–7 and enabled
  from Stage 8; owned-item consumption was still deferred at that point.
- Rebalancing an existing stable Stage does not migrate, clear or synthesize
  its saved record. Highest Unlocked and all schema-2 balances remain intact.
- Iteration 17 validation preserved the actual schema-2 revision-23 save,
  Guest ID, 13 records, 2,600 Coins and 4/4 item balances exactly.

### Iteration 18 owned start-item consumption

- `LocalProductSession` validates and atomically consumes one owned Shield
  and/or Booster for each accepted Start. The existing clone-save-publish
  boundary prevents partial decrement or runtime activation on save failure.
- Retry creates another spend; Back creates none; duplicate Start input after
  Countdown begins cannot create another spend. Refreshed zero stock clears a
  retained selection.
- Product-unavailable Campaign entry reports zero selectable stock and never
  restores the former free-item behavior. Stage 6/7 provided items and
  itemless Starts require no Product mutation.
- Schema 2, reward transaction IDs, Campaign records, Continue, and Core item
  effects are unchanged. Coin prices, item purchase, Shop, ads, and IAP remain
  deferred.
- Iteration 18 validation passed EditMode `372/372`, PlayMode `207/207`, and
  two consecutive Campaign Builder runs. Campaign and Step 10 deterministic
  artifacts retained their approved baselines.

### Iteration 19 Continue economy policy

- Failure Continue now uses persisted Coins at `300`, `600`, and `900` by
  successful Coin ordinal, with at most three total Continues per attempt.
- `SpendContinueCoins` uses the schema-2 applied-transaction ledger for atomic,
  idempotent clone-save-publish behavior. Insufficient balance, invalid input,
  and save failure do not publish a deduction or resume gameplay.
- One successful rewarded-ad completion is permitted per attempt; Coin-first
  preserves it, Retry resets it, and non-success results consume nothing.
- The advertising boundary is injectable and defaults to unavailable. No SDK,
  development auto-success fake, IAP, Shop, Heart, or schema migration was
  introduced.
- Validation passed Frontend/Campaign Builder twice plus validation, EditMode
  `400/400`, PlayMode `215/215`, and two byte-identical 400-row Campaign
  simulations. Step 10 artifacts remained unchanged.

### Iteration 20 monetization platform baseline and approved catalog intent

- Unity IAP `5.4.2` is installed for standard Google Play billing. The
  Android application ID is `com.wscho.colorgaterunner`; Firebase and Google
  Play registrations must use that exact case-sensitive identifier.
- Installation alone is not a purchase implementation. No Store connection,
  catalog fetch, order confirmation, receipt validation, reward grant or
  localized price UI exists yet.
- The approved commercial catalog intent contains six Coin amounts:
  `1,000`, `5,000`, `10,000`, `25,000`, `50,000`, and `100,000`.
- The approved bundle reward intent is:
  - Starter: Shield 1, Booster 1, Continue 1, unlimited Hearts for 30 minutes.
  - Small: Shield 2, Booster 2, Coins 500, unlimited Hearts for 1 hour.
  - Medium: Shield 4, Booster 4, Coins 1,000, unlimited Hearts for 3 hours.
  - Large: Shield 10, Booster 10, Coins 5,000, unlimited Hearts for 6 hours.
  - Extra Large: Shield 13, Booster 13, Continue 3, Coins 10,000, unlimited
    Hearts for 12 hours.
- These rewards are approved product design, not implemented inventory.
  Hearts, timed unlimited Heart entitlement and owned Continue inventory do
  not exist in schema 2. Their clocks, offline behavior, expiry, attempt use,
  reset and migration rules must be approved before a bundle can grant them.
- Coin packs are naturally consumable. The Starter bundle's one-time versus
  repeatable purchase rule and the remaining bundle product types are still
  unresolved; store product IDs and real prices are also not yet assigned.
- The next runtime purchase contract must save an idempotent order grant
  before confirming the pending store order. A failed save remains
  unconfirmed for safe redelivery. UI prices come from store metadata and are
  never hardcoded from reference screenshots.
- Firebase phase one is limited to Functions plus App Check. Firebase Core,
  Functions and App Check SDK/configuration are not installed in this
  iteration; Analytics and Crashlytics remain deferred.

## Product Iteration 3 — Frontend support

- SceneFlowService.
- PopupService.
- ContentService with local Lobby modules and themes.
- Account and Settings page read models.

## Product Iteration 4 — Empty economy and rewards pipeline

- EconomyService.
- Transaction ledger.
- Result reward payload.
- Hidden UI when no currency is active.

## Product Iteration 5 — Local analytics

- LocalAnalyticsService.
- Event schema.
- Development viewer or file output.
- Disabled-by-default release behavior until privacy decisions are complete.

## Deferred provider Iteration

Requires separate approval:

- Google or platform account provider.
- Cloud save.
- Remote configuration.
- Network content.
- Advertising.
- IAP.
- External analytics.
- Online ranking.

# 19. Definition of done

The local product foundation is complete when:

- Fresh install creates a Guest profile.
- Boot initializes one AppRoot.
- Save data is versioned and recoverable.
- Stage 1–20 progress persists by stable ID.
- Existing Continue and Best rules persist correctly.
- Offline play reaches Gameplay.
- Frontend pages consume services instead of `PlayerPrefs` transport.
- Unavailable future economy and event actions remain hidden.
- Local Lobby modules and themes are data-driven.
- App pause and quit save dirty data.
- No unapproved external package or provider is added. The approved Unity IAP
  package may remain installed while unavailable runtime purchase actions stay
  hidden until their separate contract is implemented.
- Core gameplay remains deterministic and UnityEngine-free.
- Existing EditMode, PlayMode, campaign simulation, and Step 10 baselines
  remain unchanged by product-only work.
### Iteration 21 local commerce model

- Schema 3 extends Economy with Heart count/recharge clock, last observed UTC,
  timed unlimited-Heart expiry, Continue Ticket count and Starter-granted
  state. Schema 2 migrates to 5 Hearts without changing existing wallet,
  inventory, identity or Campaign data.
- Product owns clone-save-publish authorization for Stage-start Heart/item
  spending, Continue Ticket spending and commerce reward grants. Order IDs use
  the existing transaction ledger so redelivery is idempotent; a save failure
  must leave the order unconfirmed when the real IAP adapter is added.
- `CommerceProductCatalog` owns the approved six Coin packs and five bundle
  rewards. It deliberately contains no localized prices; those must come from
  Google Play metadata.
- This model does not initialize Unity IAP, confirm purchases, validate
  receipts or contact Firebase. Those boundaries remain the next provider
  iteration after this local model.
