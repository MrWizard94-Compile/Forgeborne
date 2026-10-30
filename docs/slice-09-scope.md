# Forgeborne — Slice 9 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 9 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 9 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 8 closed the interaction gap Slice 7 left open: the
player now yaws, walks in the yawed direction, and gathers,
builds, and fires by one shared aim ray, with no silent +Z
fallback. What Slice 8 did not touch is the electrical
system itself, and that system still stops at its first
loop. There is exactly one voltage — a single voltage,
always on while placed, with no tiers, no conversion, and
no failure mode — and exactly one distribution rule: a
mill is powered only when face-adjacent to a Dynamo, with
no cables, no network walk, and no chaining through
another block. There is exactly one powered behaviour: a
powered Kinetic Mill runs every tick instead of every
5th tick, through the same reused gather path into the
same player inventory. Nothing can be over- or
under-powered, because there is no second voltage to
mismatch against, and nothing can be wired at a distance,
because power does not travel. Slice 9 is therefore an
electrical-completion slice, kept as small as the slices
before it: **a voltage ladder with a transformer, cable
distribution that carries a voltage beyond face-adjacency,
and mismatched-voltage destruction, proved against the
mill that already exists** — not a new consumer, not a
new source, not a third squad type or doctrine.

## Inputs this scope is decided from

- Slice 8 review (`docs/slice-08-review.md`) ranked
  candidates, carried unchanged from the 0.35.0 scope:
  the rest of automation tier 2 first, then a third kind
  and the deferred doctrines. The 0.38.0 decision recorded
  that the Step 3 evidence gives no reason to reorder
  them, and this scope keeps that order — the top
  candidate becomes Slice 9, the rest become the ranked
  Slice 10 candidates below, not promises. Slice 8
  removes one reason tier 2 work was awkward before:
  new blocks no longer need a fixed front-left /
  front-right / front-centre offset, because Slice 8
  retired those offsets and builders are now distinct by
  block type through `BuildAimed` — so a transformer or
  a cable can be placed by aim like a wall, a mill, or a
  Dynamo, and each step below must use that rule rather
  than inventing a placement rule of its own.
- The Slice 8 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 9 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. What Slice 9 does,
  like Slices 6, 7, and 8 before it, is write only adapter
  code the checklists will one day run (build keys for
  new block types, and any voltage indicator a step
  adds) and record, step by step, which parts are
  headless-tested and which parts are Windows-checklist
  only. Aim feel (gap 3) is a later observation, not a
  slice: a Windows playtest may retune reach or eye
  height, but no Slice 9 step retunes aim to make room
  for power. Balance (the same later pass, carried from
  every review) is not a slice: any voltage count, cable
  cost, or destruction rule the steps choose is a
  first-pass value for determinism and testability,
  exactly as Dynamo's 5 EmberOre and the mill's
  every-tick cadence were. The mesh tint and mesh
  refresh gaps (gap 4) remain named gaps from 0.30.0,
  not Slice 9 work: a new block type does not need a
  tint or a refresh hook to prove its core rule, and
  bundling either into this slice would make it two
  systems at once. The shared-marker question (gap 5)
  and the free-deploy gap (gap 6) remain documentation,
  not enforcement, for the same reason.
- Current behaviour this scope must reckon with, not
  ignore: `BlockType` in `src/Forgeborne.Core/Voxel.cs`
  holds KineticMill = 7 and Dynamo = 8, both appended
  only so old saves still load, and any new block type
  must be appended the same way, never inserted in the
  middle of the values. `World.IsPowered` returns true
  when at least one of a cell's six face neighbours
  holds a Dynamo — face only, no diagonals, no chaining
  through another mill, no cables — and powered state
  is derived each tick, not stored: remove a Dynamo and
  the mill falls back to kinetic cadence on the next
  tick, and a loaded powered mill gathers on its first
  tick, with no powered flag in the save format. The
  powered behaviour is cadence, not a new excavation:
  `RunPoweredAutomation` runs every tick, `RunKineticAutomation`
  runs every 5th tick and skips powered mills, so a
  powered mill excavates once per tick, not twice on a
  5th tick, and the two rates stay separable in the
  session counters `PoweredBlocksGathered` and
  `KineticBlocksGathered`, neither of which is saved.
  Dynamo cost is 5 EmberOre only, under the same air /
  support rules as other machines, deliberately competing
  with workstations (also 5 EmberOre) and squads (10
  EmberOre = two Dynamos), not with walls or mills for
  Stone and Wood. A Dynamo is solid and not gatherable,
  so no mill ever excavates its own power source, and a
  full inventory consumes nothing, as with kinetic mills.
  `HudState` already derives `PoweredMills` / `TotalMills`
  from `IsPowered` per mill only, and the Unity adapter
  already binds T (mill) and Y (Dynamo) through the
  aimed builders, suppressed while the picker is open.
  Blueprint vocabulary this scope must not mistake for
  a decision: `docs/architecture.md` names tier 2 as
  "Electrical (LV to EV): alternators, insulated cables,
  transformers; mismatched voltage without transformers
  destroys machines." Those are blueprint names and a
  blueprint failure rule, not a Slice 9 design. Each step
  below must choose which, if any, of those names maps
  to a unit it builds, and whether the failure rule
  belongs where the blueprint puts it — with the
  reasoning recorded in DECISIONS.md.
- Why this slice is three core steps, not one: a ladder
  without distribution can only mismatch a mill sitting
  against its own Dynamo, which is a placement error,
  not a wiring system; distribution without a ladder
  can only carry the one voltage that already exists,
  which is a longer face-adjacency, not a network; and
  destruction without either would be a rule that can
  never fire, because a single voltage delivered by
  face-adjacency alone can never mismatch. Each step
  therefore creates the condition the next step needs,
  and proving all three in one step would leave no test
  able to say whether the ladder, the cable, or the
  destruction rule broke.

## Slice 9, in order

### Step 1 — Voltage ladder and transformer

Build the ladder and its conversion, decided in that
step — not in this scope document — with the reasoning
recorded in DECISIONS.md:

- The step must decide the ladder, and record it: how
  many voltage levels there are, what they are called,
  and how they order. The scope's starting hypothesis
  is the blueprint's LV-to-EV vocabulary, with at
  least two levels so that conversion means something —
  a ladder of one level is the Slice 5 loop renamed —
  but the count, the names, and whether intermediate
  levels exist between the ends are decided in the
  step, not here. A hypothesis is not a decision: if
  the step finds two levels suffice and more would be
  unused names, it must say so and build two, not four
  for the sake of the blueprint's initials.
- The step must decide what the existing Dynamo outputs,
  and record it: the scope's starting hypothesis is
  that an existing Dynamo keeps working at one stated
  level — the level the step names as the Dynamo's
  output — so that a world built in Slice 5, with
  Dynamos already placed against mills, does not go
  dark or change behaviour on load for a reason no
  test names. If the step instead re-rates existing
  Dynamos, it must say so openly, prove the old
  powered demo numbers either stand or change with a
  stated reason, and deal with old saves explicitly:
  a Dynamo in an old save carries no voltage field, so
  whatever level it loads at is a load rule the step
  must state, not a value it must guess from a field
  that does not exist.
- The step must decide the transformer, and record it:
  what it is, what it converts, and in which direction.
  The scope's starting hypothesis is a new block type,
  appended only, that converts one stated level to one
  stated neighbouring level when placed face-adjacent
  to a powered cell — because a block is placeable by
  the existing aimed builder, countable in the world,
  and removable like a Dynamo, while a Dynamo mode or
  a per-Dynamo setting would be a second, hidden
  configuration on a block that today has none. If the
  step instead makes conversion a Dynamo variant, a
  setting, or a pair of one-way blocks, it must say why
  a single block cannot honestly carry the conversion,
  and prove the variant's save and placement rules with
  tests. The step must also decide whether a transformer
  converts in one direction only or both, and what
  happens at the ends of the ladder: a step-up with
  nowhere higher to go, and a step-down with nowhere
  lower to go, each need a stated outcome (no conversion,
  refusal, or passthrough), proved by a test, not left
  as an untested edge.
- The step must decide mill voltage acceptance, and
  record it: which delivered voltages power a mill at
  the existing every-tick cadence, and what an
  unmatched voltage does in this step. The scope's
  starting hypothesis is matched voltage powers, and
  converted-to-matched voltage powers — that is what
  the transformer is for — while an unmatched voltage
  simply does not power in this step, so the mill falls
  back to kinetic cadence, exactly as a mill with no
  Dynamo does today. Destruction is NOT decided here:
  it is Step 3's decision, and Step 1's tests must pin
  the interim fallback openly, so that Step 3 changing
  it to destruction is a recorded behaviour change with
  a rationale, not a silent regression or a test quietly
  rewritten. If Step 1 instead decides destruction
  already, it must say why Step 3 then exists at all,
  and Step 3 must be re-scoped before it is built.
- The step must decide the cost, and record it: new
  blocks are priced in existing gathered resources only,
  with no new currency invented in this step. The scope's
  starting hypothesis is EmberOre for the transformer,
  of the same order as Dynamo's 5, because the Slice 5
  pricing rationale prices electrical builds in ore so
  they compete with squads and workstations, not with
  walls and mills for Stone and Wood — but the step
  must weigh that competition itself: a transformer
  priced in ore is a second ore investment on top of
  the Dynamo it converts from, and the step must say
  what that double cost buys the player that a second
  Dynamo does not.
- The step must decide the save and display rules, and
  record them: the scope's starting hypothesis is that
  voltage, like powered state today, is derived at load
  from the blocks and their neighbours, so no new
  powered or voltage flag needs saving — unless the
  step's transformer design creates stored state (for
  example, a selectable direction), in which case that
  state follows the same old-save / malformed-entry
  rules as squads and weapons (skipped, never fatal)
  and the step must say so. The display question is
  open: `HudState` counts powered mills today, and the
  step must decide whether that count splits by voltage,
  gains a voltage line, or stays as it is — a count
  that calls a mismatched mill "powered" when Step 3
  will destroy it would be a display lie the step must
  avoid, even before Step 3 exists, by stating what its
  count means.
- The step must decide the adapter question, and record
  it: the scope's starting hypothesis is one new build
  key for the transformer through the existing aimed
  builder, suppressed while the picker is open like T
  and Y, with a Windows checklist item for key feel,
  not a test-count claim. If the step finds the
  transformer needs no key (adapter reuse, or a
  different placement path), it must say so and record
  which checklist item, if any, covers its placement.
- What must NOT change in this step: kinetic mill
  behaviour in zones without power; the powered
  every-tick cadence at matched voltage; Dynamo cost
  and face-adjacency power; squad, loot, skill, and
  core health; the gather / build / survive loop; the
  save format, except for appended block types the step
  justifies; and any destruction rule — a mill fed the
  wrong voltage in this step falls back, it does not
  break, by the interim rule above.

Why first, and why alone: conversion is the smallest
rule that makes "voltage" a real quantity instead of a
label. Until a transformer can turn one level into
another, there is no second voltage in the world for a
cable to carry or a mismatch rule to punish, and any
ladder written without a conversion proof would be
names in an enum, not a system.

### Step 2 — Cable distribution

Carry a voltage beyond face-adjacency, decided in that
step — not in this scope document — with the reasoning
recorded in DECISIONS.md:

- The step must decide the cable, and record it: what
  it is, what it costs, and what it is not. The scope's
  starting hypothesis is a new block type, appended
  only — an insulated cable in the blueprint's words —
  built under the same air / support rules as other
  machines, priced in existing resources only, solid
  and not gatherable like a Dynamo, so no mill ever
  excavates its own wiring. If the step instead makes
  distribution a property of existing blocks, a radius
  around a Dynamo, or a free placement, it must say why
  a cable block cannot honestly carry distribution, and
  prove the alternative's cost, placement, and removal
  rules with tests. A cable that costs nothing and
  occupies nothing is not distribution, it is a longer
  face-adjacency rule wearing a name.
- The step must decide how voltage travels, and record
  it: the scope's starting hypothesis is a deterministic
  network walk over face-adjacent cable cells from a
  source — Dynamo or transformer output — to a consumer,
  face only, no diagonals, with no loss per hop and no
  length limit in this step, because a loss or limit
  introduced in the same step as the network itself
  would make a failed delivery ambiguous (too far, or
  too lossy, or unconnected?). If the step finds a hop
  limit or a per-hop loss is the honest shape, it must
  state the number, prove the boundary with a test
  (the cell just inside the limit powered, the cell
  just outside it not), and say what a player sees at
  that boundary. Chaining through another mill, wall,
  or workstation is NOT assumed: the step must decide
  which block types conduct, and prove that a
  non-conducting block breaks the network rather than
  passing voltage through silently.
- The step must decide what direct face-adjacency still
  means, and record it: the scope's starting hypothesis
  is that Dynamo-against-mill still powers exactly as
  in Slice 5, and cables extend that rule, not replace
  it — a mill face-adjacent to a Dynamo needs no cable
  to run, and a mill face-adjacent to an energised cable
  runs at the cable's voltage, through the same powered
  phase and the same every-tick cadence. The step must
  prove both in the same run, so that a reader cannot
  mistake a cable step for a Dynamo nerf.
- The step must decide network voltage resolution, and
  record it: what voltage a cable network carries when
  more than one source touches it. The scope's starting
  hypothesis is one voltage per connected network, with
  the step deciding the resolution rule openly — same
  voltage from two sources is one voltage, not two, and
  a network touched by two different voltages must
  resolve to one stated outcome in this step, even if
  that outcome is only an interim rule Step 3 revises
  (for example, the higher voltage wins, the first in
  scan order wins, or the network carries no voltage
  until the conflict is resolved). What the step must
  NOT do is leave a mixed-voltage network undefined
  and let the scan order decide it by accident: the
  scan order is deterministic, but determinism without
  a stated rule is still an accident with a test around
  it. Branching is decided here too: a cable that splits
  to two mills carries the same voltage to both, proved
  by a test, or the step says why a branch does not.
- The step must decide transformer placement in a
  network, and record it: a transformer sits in a cable
  run — source cable on one face, converted cable on
  another — and the step must prove that a mill beyond
  a transformer runs at the converted voltage, not the
  source voltage, while a mill on the source side still
  runs at the source voltage. If the step's transformer
  design from Step 1 cannot sit in a cable run (for
  example, if it only converts face-adjacent mills),
  the step must say so and either extend the transformer
  rule openly, with Step 1's tests updated for the
  stated reason, or record the transformer as
  face-adjacency-only and prove the cable network
  without it — but it must not claim a converted cable
  run it has not built.
- The step must decide the save and display rules, and
  record them: the scope's starting hypothesis is that
  cable networks, like powered state today, are derived
  at load from the blocks themselves, so no network
  field needs saving, and a loaded cable run powers on
  its first tick exactly as a loaded Dynamo pair does.
  The display question is open: the step must decide
  whether `HudState` gains a cable or energised-network
  count, or whether the existing powered-mill count is
  enough — and if a cable network is energised but
  feeds no mill, the step must say whether the display
  shows it at all, rather than leaving an energised
  cable invisible and untestable in the scene.
- The step must decide the adapter question, and record
  it: the scope's starting hypothesis is one new build
  key for cable through the existing aimed builder,
  suppressed while the picker is open like T and Y,
  with a Windows checklist item for key feel, not a
  test-count claim. A cable is placed one cell at a
  time by aim in this step; a drag-to-lay or multi-cell
  placement mode is a new interaction, not a cable rule,
  and is deliberately not in this step.
- What must NOT change in this step: the ladder and
  conversion rules from Step 1; kinetic mill behaviour;
  the powered every-tick cadence at matched voltage;
  Dynamo cost; the interim unmatched-voltage fallback
  from Step 1 (destruction is still Step 3's decision);
  squad, loot, skill, and core health; the gather /
  build / survive loop; and the save format, except for
  the appended cable type.

Why second, and why alone: distribution is the rule
that makes voltage a wiring problem instead of a
placement problem. Until power can travel, every
electrical question is "is the mill touching the
Dynamo?", and no transformer placement, no branch, and
no mismatch can be tested at a distance — which is
where wiring decisions actually live.

### Step 3 — Mismatched-voltage destruction

Punish the wrong voltage, decided in that step — not in
this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the rating, and record it: which
  voltage (or voltages) each consumer accepts, and what
  counts as a mismatch. The scope's starting hypothesis
  is that a Kinetic Mill is rated at one level — the
  level the step names, consistent with Step 1's
  acceptance rule — so that a mill fed a higher voltage
  through a cable or a transformer output, without a
  transformer in the path correcting it, is mismatched,
  while a mill fed its rated voltage, directly or
  converted, is safe. The step must decide undervoltage
  openly: the starting hypothesis is that undervoltage
  falls back to kinetic cadence, not destruction,
  because the blueprint's failure rule punishes a machine
  fed too much, and a machine fed too little is a machine
  not running, not a machine breaking — but if the step
  finds undervoltage must also destroy, it must say why
  a starved mill breaks rather than idles, and prove it
  with a test. A rating that no delivered voltage can
  violate is not a rating, and a rating that every
  delivered voltage violates is not a game.
- The step must decide destruction semantics, and record
  them: what happens, when, and what survives. The
  scope's starting hypothesis is that a mismatched mill
  is destroyed on the tick its mismatched voltage is
  delivered — removed from the world (its cell becomes
  Air) during the powered phase, in deterministic scan
  order, so that a destroyed mill excavates nothing on
  that tick and never again, and its session counters
  stop advancing. The step must decide salvage openly:
  whether destruction refunds any of the mill's build
  cost to the player inventory, refunds nothing, or
  drops something else — a refund is an economy decision,
  not a detail, because a free rebuild after every
  mistake would make destruction a tax the player can
  ignore, while no refund makes a wiring mistake cost a
  whole mill. The step must also decide cascade: whether
  destruction spreads to neighbouring cables, to the
  transformer that fed it, or to the Dynamo that sourced
  it — starting hypothesis: it does not, because a
  failure rule that eats its own source would punish
  the player for building the source correctly, and
  any cascade the step does choose must be proved with
  a test that names exactly which neighbours break and
  which survive.
- The step must decide the transformer's own rating, and
  record it: a transformer that steps voltage up or down
  is itself fed a voltage, and the step must say whether
  a transformer fed a voltage outside its input rating
  is destroyed, passes through unconverted, or refuses
  — and whether a cable itself can be destroyed by
  overvoltage, or whether cables are voltage-blind
  carriers that never break. What the step must NOT do
  is give every block a rating by default: a rating
  belongs on blocks the step proves it on (the mill
  first, the transformer if the step decides it), and
  every other block's behaviour under overvoltage is
  stated, not assumed.
- The step must decide the save and display rules, and
  record them: destruction is world state, so a destroyed
  mill stays destroyed after a save/load round-trip — no
  resurrection, no refund on load that was not given on
  destruction — proved by a test, not asserted. The
  display question is open: the step must decide whether
  destruction is counted (a session counter, a HUD line,
  or nothing), and must reckon with the mesh-refresh gap
  honestly: a destroyed block may be counted before it
  is visible as gone in the scene, so the review must
  not claim the scene shows destruction until a Windows
  checklist item proves it.
- The step must decide the adapter question, and record
  it: the scope's starting hypothesis is no new binding
  at all — destruction is a core rule that fires on a
  tick, not a key press — with a Windows checklist item
  only if the step adds a destruction indicator the
  scene must show. If the step finds destruction needs
  a warning (a HUD flash, a sound, or a marker) before
  it is fair, it must name it as adapter work with its
  own checklist item, not as a core rule smuggled into
  the adapter.
- What must NOT change in this step: the ladder and
  conversion rules from Step 1, except for the
  acceptance change this step itself justifies (the
  interim fallback for overvoltage is replaced by
  destruction, with the changed tests named openly, as
  Slice 8 Step 2 did for its own intended behaviour
  change); the cable distribution rules from Step 2,
  except where a destroyed block's removal changes a
  network, which the step must prove (a network whose
  consumer is destroyed carries on to its other
  branches, or it does not, with a test); kinetic mill
  behaviour; Dynamo cost; squad, loot, skill, and core
  health; the gather / build / survive loop; and the
  save format, except for appended block types already
  justified in Steps 1 and 2.

Why third, and why alone: destruction is the only step
in this slice that removes something the player built,
so it must be the step with the most proof and the least
company. Proving it after the ladder and the network
exist means its tests can feed a mill a real mismatched
voltage through a real cable run and a real transformer
path, rather than staging a mismatch by hand-editing a
cell — and a reader can tell, from the tests alone,
whether the mill broke because the voltage was wrong
or because the wiring was never right.

### Step 4 — Slice 9 demo scenario and review

As with 0.13.0, 0.19.0, 0.23.0, 0.26.0, 0.31.0, 0.34.0,
and 0.38.0: extend the demo with one composed Slice 9
run, then write `docs/slice-09-review.md` as a
documentation-only step. The demo proves, headlessly,
the ladder, the cable, and the destruction rule together,
not separately: one Simulation in which a Dynamo feeds
a mill at its rated voltage through a cable run, a
second mill beyond a transformer runs at the converted
voltage, a third mill fed a mismatched voltage through
a cable run is destroyed with the salvage rule the step
decided, and a kinetic mill with no power at all still
excavates every 5th tick beside them — through a
save/load round-trip that keeps the surviving blocks,
keeps the destroyed mill gone, and re-derives power on
load without a stored voltage flag. The demo cannot
prove adapter feel — no headless run can show a cable
being laid by aim, a voltage line reading right, or a
destruction flash in a scene — so the review's verdict
must separate the two claims in words: the core voltage,
distribution, and destruction rules are tested, the
adapter keys and any indicator are written and
checklist-listed, and neither claim is allowed to borrow
the other's evidence. The review closes Slice 9 as
tested electrical tier 2 core (ladder, transformer,
cable, destruction) plus written adapters — not as
playtested, balanced, or Unity-verified — and names the
next candidates in priority order. Slice 10 is not
scoped by that review; it is scoped afterwards, the
same way this document follows the Slice 8 review's
candidates rather than preceding them.

## Deliberately NOT in Slice 9

- Additional powered consumers or power sources: a
  powered workstation, a second powered behaviour for
  an existing machine, an alternator as a distinct
  source from the Dynamo, or any other consumer or
  source beyond the mill Slice 5 already powers. One
  consumer proves the ladder, the cable, and the
  destruction rule against a known baseline; a second
  consumer in the same slice would need its own
  baseline proved at the same time, and is the first
  ranked Slice 10 candidate below, not Slice 9 scope
  creep.
- Automation tiers 3-4 (digital / arcane), crafting,
  fuel or power beyond the Dynamo source Steps 1-3 use,
  and a per-machine inventory system beyond the player
  inventory rule Slice 5 decided. Tier 1 and powered
  output stay in the player inventory unless a later
  consumer step justifies changing that rule for its
  own behaviour.
- A per-hop voltage loss, a cable length limit, or a
  drag-to-lay placement mode, unless Step 2 decides one
  of them as its own stated rule with its boundary test
  — none of them is assumed by this scope, and none
  rides along as an untested extra.
- A third squad type, or a second kind added alongside
  anything else as a bonus. One new kind was Slice 7;
  the next kind, if any, is a Slice 10-or-later decision
  with Slice 9's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a power behaviour.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 9 wiring stays in the FPS view,
  not a second camera.
- A new movement mode, a weapon rework, or a balance
  pass. Any voltage count, cable cost, transformer cost,
  or salvage rule the steps choose is a first-pass value
  for determinism and testability, exactly as Slice 3's
  prices and Slice 5's Dynamo cost were, and must be
  labelled as such, not quoted as final feel.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- A mesh tint for Dynamo / powered mills / cables / a
  transformer, a mesh refresh on any key build, and a
  distinct Recon marker. All three remain named gaps
  from 0.30.0 / 0.33.0, not fixed by an electrical step.
- Windows Unity verification itself. It remains open
  for Slices 1-9 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.
  The checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification
  plan, and Slice 9 steps must add their own checklist
  items to those docs rather than claiming verification.

## Ranked candidates for Slice 10 scoping (not promised)

These are recorded here so the Slice 9 review starts
from a list, not from scratch. Order follows the
0.35.0 / 0.38.0 ranking with the Slice 9 candidate
removed, and with the tier 2 remainder split honestly:
Slice 9 closes the ladder, the cable, and the
destruction rule, so what remains of tier 2 is its
widening, not its core:

1. Additional powered consumers and power sources
   (a powered workstation or a second powered behaviour,
   an alternator as a distinct source), deferred, not
   rejected.
2. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 9's electrical evidence will inform, not
   promises made before that evidence exists.

## Slice 9 gate (what "done" means)

Slice 9 is done when: every step above is pushed with
passing tests; the voltage ladder and the transformer
conversion are each decided and tested, with existing
Dynamos loading at a stated level and old saves still
loading; cable distribution carries a stated voltage
beyond face-adjacency, face only, through branches and
through a transformer in the run, with mixed-voltage
network resolution decided and tested; mismatched
voltage destroys a mill by the stated semantics, with
salvage and cascade each decided and tested, and
undervoltage decided openly rather than left undefined;
kinetic mill behaviour and matched-voltage every-tick
cadence are unchanged; the composed demo proves the
ladder, the cable, and the destruction rule together
through a save/load round-trip, with the destroyed mill
staying destroyed; and `docs/slice-09-review.md`
records the verdict honestly, separating headless-tested
core claims from Windows-checklist adapter claims,
including that Slice 9 is still not playtested,
balanced, or Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is not
closed by Slice 9.
