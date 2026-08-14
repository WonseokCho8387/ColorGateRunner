# Development Loop

Play

↓

Human Feedback

↓

Design

↓

Codex

↓

Tests

↓

Experiment

↓

Play Again

---

KPI

Completion

Retry

Continue

Fun

Replay

Tap Distribution

Mechanic Usage

---

Latest Completed Cycle

Play

Campaign speed felt too weak during human play.

↓

Analyze

Current speed and spacing are deterministic, progress-based, and coupled
through authored cadence.

↓

Design

Double campaign movement and world distances together while keeping cadence
and Experiment Lab values unchanged.

↓

Codex

Applied 2x normal speed, Booster speed/distance, gate lead, spacing scale, and
Goal distance without introducing a second movement system.

↓

Tests

217 EditMode and 111 PlayMode passed; Scene Builder passed; all campaign
non-spatial metrics and all Experiment report hashes remained unchanged.

↓

Experiment

Portrait mobile speed perception, gate readability, and transition comfort
remain human checks.

↓

Play Again

---

Latest Hidden/Flicker Balance Cycle

Play

Human play found Hidden too late to create enough memory pressure and found
random Flicker color jumps capable of demanding impractical tap bursts.

Analyze

Hidden difficulty grows with hide-to-arrival time, opposite Camouflage's
reveal-to-arrival pressure. Flicker and player input used separate color-order
rules.

Design

Set Hidden hide lead to `0.85s`. Derive Flicker from every active color and
advance it with the player's one-forward-tap palette order.

Codex

Shared one Core next-color rule between player input and Flicker planning,
removed the authored `2/3` count, and updated the serialized Scene default.

Tests

EditMode 288/288 and post-Builder PlayMode 144/144 passed. Campaign three
hashes and Step 10 five hashes remain exact.

Experiment

Human review now owns Hidden memory pressure and whether three- through
six-color Flicker remains followable at `0.50s`.

---

Iteration 4 Completion Loop

Play

Human feedback rejected Clone and approved Echo plus campaign expansion.

Analyze

The first three-color mechanic-stage candidate compounded input complexity;
simulation supported isolating Stages 6-10 with two colors.

Design

One catalog, shared modifiers, ordinary-gate Echo, ETA Camouflage, deterministic
speed curves, and one primary mechanic per new stage.

Codex

Implemented Stage 6-11 without a second generator, judgment path, movement
system, item inventory, or persistence model.

Tests

248 EditMode and 125 PlayMode passed. Stage 1-5 rows and all Step 10 files
match their baselines.

Experiment

Echo comprehension, reveal timing, speed feel, and campaign progression remain
human decisions after automation.

Play Again

Start the next tuning loop only after mobile play notes are recorded. Change
one documented catalog value at a time and rerun the same regression matrix.

---

Current Echo Expansion Cycle

Play

Human feedback rejected the additional Clone Gate concept.

↓

Analyze

Campaign data is hardcoded, experimental modifiers are mode-specific, and the
five-stage UI is not data-driven.

↓

Design

Use one serialized Catalog, pure Core adapters, shared modifiers, one Echo
offer coordinator, ETA Camouflage, deterministic speed curves, and Stage
6–11 stage-local mechanics.

↓

Codex

Implementation proceeds through independently tested Architecture, Modifier,
Echo, ETA, Speed, Campaign, Simulation, and Regression phases.

↓

Tests

Each phase must preserve the immediately preceding passing baseline before
the next begins.

↓

Experiment

Echo comprehension, reveal timing, speed feel, and campaign progression remain
human decisions after automation.

---

Historical Hidden Cycle (originally named Flicker)

Play

Human play approved a standalone Hidden Experiment contract: show target
information first, hide it once near judgment, then require memory while the
ordinary gate remains physically readable.

↓

Analyze

The shared Gate Modifier, deterministic Experiment sequence, effective-speed
ETA, ordinary judgment priority, and six-View pool can support Hidden without
a second gameplay system.

↓

Design

Use validated Inspector settings, deterministic eligible-gate selection,
minimum observation plus ETA, one-way target hide, persistent identity, and
ordinary Player/Echo/Shield/Failure resolution. Disable Booster and defer
modifier combinations and campaign use.

↓

Codex

Extended the existing sequence and pooled View with pure-Core settings and
visibility state. Reused existing materials, symbol rendering, Launcher,
Retry/Replay, and judgment.

↓

Tests

262 EditMode and 134 PlayMode passed. The twice-built scene passed all 134
PlayMode tests again. Campaign and all five Step 10 artifacts reproduced exact
baseline hashes.

↓

Experiment

Observation duration, hide lead, transition clarity, memory interval, retained
gate readability, Echo/Shield comprehension, portrait readability, and
repeat-entry fatigue remain human decisions.

---

Current Hidden / Color-Cycling Flicker Cycle

Play

The current hide-on-approach behavior must remain as a memory mechanic named
Hidden. A separate visible color-cycle mechanic is approved as Flicker.

↓

Analyze

Existing enum values and serialized Launcher fields require explicit
migration. Experiment elapsed Playing time, shared judgment, deterministic
sequence planning, and the fixed View pool already provide the required
boundaries.

↓

Design

Preserve Hidden values and seed results. Give new Flicker new values,
deterministic two-/three-color plans, absolute Gameplay Time calculation,
collision-time judgment, minimum pooled exposure, and a distinct persistent
marker.

↓

Codex

Implementation preserved former numeric values and seed results as Hidden,
gave color-cycling Flicker distinct values and serialized field names, and
reused the deterministic plan, Gameplay Time, shared judgment, and fixed pool.

↓

Tests

Final EditMode is 281/281. PlayMode is 143/143 before and after two successful
Scene Builder runs. All three campaign and five Step 10 hashes remain exact.

↓

Experiment

Human review now owns concept separation, `0.50s` switching speed, three-color
difficulty, boundary comprehension, pulse, symbols, Echo/Shield feedback,
minimum visible cycles, and mobile readability. Automated results do not
decide those qualities.

---

Latest Clone Cycle

Play

Existing Experiment uses one deterministic gate stream, one judgment path,
six pooled gate Views, shared Shield protection, and visibility-only
Camouflage.

↓

Analyze

Clone can extend gate metadata and the current sequence without a second
runtime. Game Director fixed Sources 3/6 and GapSeconds 0.45.

↓

Design

Two same-color, independently judged Clone Gates; Booster disabled;
Camouflage hides and reveals but never exempts judgment.

↓

Codex

Extended gate metadata, the existing deterministic sequence, shared judgment,
and pooled gate presentation for Clone-only Experiment.

↓

Tests

231 EditMode and 117 PlayMode passed; Scene Builder passed; campaign and
Step10 artifact hashes remained unchanged.

↓

Experiment

Clone relationship, 0.45-second spacing, hazard recognition, Shield feedback,
and Camouflage reveal remain human checks.

↓

Play Again

---

Latest Campaign Hidden/Flicker Cycle

Play

Campaign ended at Echo Stage 11 while Hidden and Flicker were available only
as Lab experiments. Stage 6+ already promised consistent Shield/Booster
selection.

↓

Analyze

Campaign lacked Flicker plan metadata, View binding, collision-time color
judgment, and simulator parity. Disabling Booster would avoid the interaction
instead of validating it.

↓

Design

Stage 12 Hidden and Stage 13 Flicker, three colors, isolated modifiers,
shared deterministic planning, absolute Gameplay Time, and unchanged
Shield/Booster behavior.

↓

Codex

Added catalog revision 5, shared Flicker planning data, Campaign runtime/View/
simulation wiring, 13 stage buttons, and deterministic exposure checks at
Booster speed.

↓

Tests

294 EditMode and 147 PlayMode pass. Scene Builder runs twice and post-Builder
PlayMode remains 147/147. The 260-row Campaign matrix preserves every Stage
1-11 row, and Step 10 keeps all five hashes.

↓

Experiment

Human play now owns the Stage 11→12 difficulty jump, Hidden memory demand,
Flicker boundary readability, post-Booster teaching frequency, and portrait
legibility. Automated results do not decide balance quality.

↓

Play Again

---

Latest Campaign Learning-Curve Redistribution Cycle

Play

Sequential Campaign play introduced new mechanics faster than players could
adapt. Convenience-item demonstrations were useful early, but each following
Stage moved immediately to another mechanic.

↓

Analyze

The single Catalog and deterministic runtime already supported longer isolated
mechanic blocks. Hidden and Flicker could leave the current Campaign without
losing implementation coverage because their Experiment paths remain active.

↓

Design

Keep Stages 1-5 exact, isolate provided Shield at 6 and provided Booster at 7,
use clean three-color Stage 8 for ordinary item application, then teach
Camouflage, Fog, Ice and Echo in three-Stage 2/2/3-color blocks. Defer Hidden
21-23 and Flicker 24-26.

↓

Codex

Catalog revision 7 redistributed the 20 stable-ID definitions and regenerated
the Campaign Scene. Product-save identity, mechanic rules, fixed pools and
Experiment contracts were preserved.

↓

Tests

EditMode `367/367` and post-Builder PlayMode `195/195` passed after two Builder
passes. Two 400-row Campaign simulations were byte-identical; Stage 1-5's 100
CSV rows and first 100 JSON results remain exact, and continuity/pool
violations remain zero. Step 10 stayed untouched.

↓

Experiment

Human play now owns whether each introduction/practice/mastery block creates
real learning without repetitive fatigue, and whether the 2/2/3-color mastery
step is readable on portrait devices.

↓

Play Again

---

Latest Consumable Start-Item Cycle

Play

Lobby showed owned Shield and Booster counts, but selecting those items in
PreRun did not spend inventory. Retry also retained selections without a new
ownership consequence.

↓

Analyze

Schema-2 Product save already owned inventory and idempotent transactions. The
existing accepted Start boundary could add consumption without moving item
effects or deterministic gameplay into Product services.

↓

Design

Consume each selected item once in one atomic Start transaction from Stage 8,
re-consume retained selections on Retry, keep Stage 6/7 provided grants free,
and leave PreRun unchanged on shortage or save failure. Missing Product never
creates a free selected-item fallback.

↓

Codex

Added Product-owned `ConsumeStartItems`, an explicit Campaign inventory gateway
and generated PreRun synchronization while preserving schema, Core, Catalog
and mechanic behavior.

↓

Tests

Targeted Product `30/30` and PlayMode `5/5` passed. Campaign Builder passed
twice; full EditMode `372/372` and post-Builder PlayMode `207/207` passed.
Packages, ProjectSettings and the actual Product save remained unchanged.
Tier 2 preserved Campaign and Step 10 hashes without rerunning simulations.

↓

Experiment

Human review owns whether cost, owned quantity, Retry re-consumption and failure
feedback are clear, and whether the free lessons make Stage 8's first inventory
use feel fair.

↓

Play Again

---

Latest Continue Economy and Attempt-Policy Cycle

Play

Failure had a functional resume path but no truthful Coin price, rewarded-ad
choice or visible attempt limit.

↓
Analyze

Core already owned deterministic resume and Retry, while Product already owned
Coins and atomic persistence. An attempt-local policy could coordinate them
without creating another gameplay or economy authority.

↓
Design

Permit three Continues per Attempt; price Coin uses at 300/600/900 by Coin
ordinal; independently allow one successful rewarded ad; preserve unused rights
across mixed source order; reset on Retry; keep failure frozen when authorization
or persistence fails; hide the ad action when no provider exists.

↓
Codex

Added Core Continue counting, atomic idempotent Product spending, attempt-policy
source authorization, truthful provider visibility and deterministic simulation
use metrics while preserving the existing safe-resume invariants.

↓
Tests

Campaign Builder and validation passed twice. EditMode `400/400` and final
post-Builder PlayMode `215/215` passed. Two 400-row Campaign simulations were
byte-identical with Summary
`BF450495BCE1EF312C591EC5BA1B5EA3750F45E60E9966B7236C676DAD12B390`,
JSON `EB355F9FCE121B9157815D2940EC4AA1A277D600310F41049BE312232052CEED`
and CSV `2693BD576A27422FA7A2C0E7226005D11C6624BEA16B99750C24332E55E229A5`.
Step 10 remained unchanged.

↓
Experiment

Human review owns whether escalating cost, the independent ad right, unavailable
states and Retry reset are understandable and feel fair. Automation does not
measure value, frustration or willingness to watch an ad.

↓

Play Again

## Iteration 20 loop learning — monetization baseline before Shop

### Observe

IAP was installed and the desired catalog/UI direction was supplied, but the
project still had a template application ID and no Store or Firebase client.

### Analyze

A complete Shop iteration would combine package baselining, new persistence
models, asynchronous billing, server validation and a large Frontend redesign.
That would hide failures and make purchase safety difficult to verify.

### Decide

Lock the platform identity and package baseline first. Record catalog and
visual intent without pretending unsupported Heart, timed, Continue or Shop
features already exist.

### Validate

The normalized package set and final Android identifier compiled with full
EditMode `400/400` and PlayMode `215/215`. Gameplay simulations were preserved
because no deterministic input changed.

### Next loop

Define the missing reward models and stable product IDs, then build a pure
purchase grant boundary before exposing a real Shop action.

## Iteration 21 loop learning — close reward semantics before Store UI

### Play / Analyze

Approved bundles could not be represented truthfully while Hearts, unlimited
time and Continue inventory were absent from the save.

### Decide

Close the local loop first: stable product definitions, atomic order grants,
Heart start authorization and Ticket-first Continue selection. Keep Store and
Shop actions hidden until external purchase authority exists.

### Validate

Builder twice, EditMode `408/408`, PlayMode `218/218`, and an exact real-save
comparison established the local reward baseline without changing gameplay
simulation inputs.

### Next loop

Connect Unity IAP pending orders to receipt/server validation and this grant
boundary, then confirm Store orders only after the durable grant succeeds.

## Iteration 23 loop learning - close failure-to-next-stage recovery

### Play / Analyze

Twenty-Stage play showed that the recovery loop exposed balances and buttons
without one coherent contract: Continue ended after three uses, countdown
briefly revealed stale mechanics, a last-Heart clear blocked natural forward
motion, and Campaign clear actions converged on Lobby or misleading Replay.

### Decide

Make Continue uncapped at 900/1,900/2,900/4,900-repeat by Coin ordinal, keep ad
and Ticket outside that ordinal, settle the charged Heart on clear, grant
100/200/500 first-clear Coins by difficulty, and route Campaign clear directly
to the next PreRun. Treat the Continue countdown as already-resumed mechanic
state with consumed buffs absent.

### Validate

Campaign Builder passed twice; EditMode passed `418/418`; post-Builder PlayMode
passed `222/222`. Two Campaign simulations were byte-identical with Summary
`698565A5AA723173094082C1E6F2895F9809EBC16B3D2DAE9FF42EC1DB47D532`,
JSON `68400C449988669B9530F224D81C8FC66CC3FDC2B4E355D717C5192816A85903`
and CSV `2C19D4779B75BBCF86D59482E17D5A9C37FED58AD8F523F41F63053A89BA8C2E`.
Step 10 remained unchanged.

### Next loop

Use the now-coherent economy/result flow as the foundation for Lobby layout,
zero-stock start-item purchase and real Store/ad integrations. Evaluate Fog
and Ice presentation in separate gameplay iterations so economy UX does not
mask mechanic readability changes.

## Iteration 24 loop learning — Continue pool exhaustion

- **Play:** Stage 11 stalled after repeated late Continues; no gate remained
  and Goal could not complete the run.
- **Analyze:** Fog was coincidental. Continue removed one object from the
  six-slot presentation pool per use.
- **Design:** consume the failed plan but recycle its object immediately.
- **Validate:** exceed the pool size with late Continues, finish every gate,
  then cross Goal and assert Stage Cleared.
- **Learn:** uncapped product actions must be validated against every bounded
  presentation resource they repeatedly touch.

## Iteration 25 loop learning — Fog as a timed visibility event

- **Play:** nearest-two Fog exposed upcoming spacing and its end boundary.
- **Analyze:** coloring individual gates neutral made Fog legible as a counted
  sequence rather than a temporary obstruction.
- **Design:** keep gate truth intact and place one speed-adaptive curtain
  between player and upcoming content for a fixed hold-and-fade window.
- **Validate:** test one-shot timing, distance, Pause/Continue freeze, Retry
  reset, ordinary judgment and Builder uniqueness separately.
- **Learn:** visibility challenge should obscure perception without rewriting
  the underlying deterministic target state.
