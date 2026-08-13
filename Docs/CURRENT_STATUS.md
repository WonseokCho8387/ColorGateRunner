# Current Status

## Authoritative Baseline

This section is the single current baseline for routine Codex iterations.
Historical counts and hashes later in this file remain evidence for their own
iterations but do not override this section.

### Repository

- Implementation base HEAD: `bdc824d`
- Base commit: `feat: add local commerce rewards and hearts`
- Authoritative completion HEAD: the commit named
  `feat: add editor developer console`; its exact hash is
  recorded in the Iteration 22 final report because a commit cannot contain
  its own content-derived hash.
- Branch at completion: `main`
- Expected working tree: clean
- Push status at completion: not pushed

### Current product state

- Current completed iteration: **Iteration 22 — Editor Developer Console**.
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
- First Stage clear grants 100 Coins once. Every even first-cleared Stage up
  to Stage 36 applies one automatic Lobby milestone and an idempotent reward.
  The starter reward policy grants 200 Coins, or 300 at every third milestone,
  plus alternating Shield/Booster inventory.
- Frontend Lobby displays Coins, inventory, current theme, visible upgrades,
  the next clear target, and a one-time reward summary. Frontend-origin result
  actions return there; direct/development entry keeps Campaign Lobby fallback.
- Campaign contains 20 stable-ID Stages. Stages 1–5 remain the color/rhythm
  foundation; Stage 6 provides Shield and Stage 7 provides Booster while
  selection stays locked; Stage 8 is the first clean three-color Stage with
  selectable items. Stages 9–20 teach Camouflage, Fog, Ice and Echo in
  three-Stage intro/practice/mastery blocks using 2/2/3 colors. Hidden and
  Flicker remain available in Experiment Lab but are deferred from Campaign
  until the planned Stages 21–23 and 24–26.
- From Stage 8 onward, selecting Shield or Booster consumes one owned unit of
  each selected item in one atomic Product save when `START` succeeds, before
  Countdown. Zero stock disables that selection. Insufficient current stock
  or save failure keeps PreRun open with truthful status and publishes no
  partial spend. Retry is a new attempt and consumes again; Back before Start
  and duplicate Start do not consume. Stage 6/7 provided training items remain
  free and do not consume inventory. Without a ready Product session,
  selectable items are unavailable while no-item and provided-item starts
  remain valid.
- A Stage attempt allows at most three Continues. Coin Continues cost 300,
  then 600, then 900 Coins by successful Coin-use ordinal. One completed
  rewarded-ad Continue is available per attempt when a real provider reports
  availability; Coin use does not consume that right and ad-first does not
  raise the first Coin price. Coin spending is atomic and idempotent before
  Core resumes. Insufficient funds, save failure, failed/cancelled ads and
  stale callbacks keep the failure state frozen. Retry creates a fresh policy.
  The current release provider is unavailable, so no ad action is displayed
  or simulated as successful.
- A normal Stage start consumes one Heart atomically with selected start items.
  Hearts cap at 5 and recover one every 30 minutes, including offline;
  backwards clock movement cannot accelerate recovery. Unlimited Hearts
  suppress consumption until UTC expiry and purchased durations stack.
- The local catalog owns six Coin packs and five bundles. Grants are atomic
  and order-ID idempotent; Starter is locally account-limited. Continue
  Tickets are offered before real ads and Coins and still count toward the
  three-Continue cap. Store, receipt, Shop and Firebase work remains deferred.

### Automated validation

- EditMode: `414/414`
- PlayMode: `218/218`
- Post-Builder PlayMode: `218/218`
- Campaign Builder: 2 consecutive successful runs
- Stage Catalog Builder: revision 7 Resource contains 20 valid stages
- Missing Script / Missing Reference / duplicate generated object failures:
  none

### Build Settings

1. `Assets/Scenes/Boot.unity`
2. `Assets/Scenes/Frontend.unity`
3. `Assets/Scenes/SampleScene.unity`

All three entries are expected to be enabled and unique.

### Developer save snapshot

- Schema 3, revision 30, Guest ID
  `2dfe4f6ffa914e7a95e2fd30de5b6307`, highest Stage 14, 13 records,
  1,500 Coins, 3 Shields, 3 Boosters, 0 Continue Tickets and 5 Hearts.
- SHA-256:
  `77AF3ED2CFBB13591E2119FD77F94C68A5BFC488CF4DB62C2FDCC52C8B443B50`.
- Full validation did not mutate this file. Developer Console mutations occur
  only after an explicit Apply, Reset, or Unlock action.

### Campaign simulation baseline

- Rows: `400`
- Summary SHA-256:
  `BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`
- JSON SHA-256:
  `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`
- CSV SHA-256:
  `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`
- Two complete runs are byte-identical. Continue-use metrics are deterministic
  and capped at three. Continuity and pool-violation counters are zero.
- Step 10 was not rerun because its Experiment contracts and inputs did not
  change; the Step 10 baseline below remains authoritative.

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
  Record keys 1–20 when present and must be restored exactly after tests.
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
  `1D7BCAB0E815F5C9BE0779CDF8E88ECB98ADC7EA5BA4C800B5F5402E611FEEE2`
- `Packages/packages-lock.json` SHA-256:
  `BF37ABC71E898CAE0498B49D4AACB9075A47D62E2D14A27C355895B2E8CEFB84`
- `ProjectSettings/ProjectSettings.asset` SHA-256:
  `BFF9843B9363FF1C20B113108D623026E777311CAEF6372B07C92690049717BC`
- Approved package delta: Unity IAP `5.4.2`, Unity Services Core `1.18.0`,
  and `Assets/Resources/BillingMode.json` with Google Play. The incidental
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
- Build-menu and portrait-template EditMode coverage passes 5/5 in the focused
  suite. The full EditMode suite passes 283/283.
- An isolated Android invocation reached the real Player build after script
  and shader compilation, but its copied-Library backend stopped making
  progress before producing an APK. Actual APK installation remains a manual
  acceptance check from the original editor.
- A separate isolated WebGL build completed successfully and produced a
  124,340,289-byte player. Its generated page contains the expected `540 x 960`
  canvas macros and exact `9:16` frame rules, and the validation project's
  prior `960 x 600` default-template settings were restored afterward.
  Browser play, input, and host-page resizing remain human acceptance checks.

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
