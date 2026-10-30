# Forgeborne — Slice 10 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 10 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 10 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 9 closed the electrical core Slice 8 left open: the
power system now has a two-level ladder (LV and HV), a
bidirectional transformer, cable distribution that carries
a voltage beyond face-adjacency through branches and
through a transformer in the run, and a failure mode —
a mill fed HV is destroyed on its delivery tick, with no
salvage and no cascade. What Slice 9 did not touch is
the width of that system. There is still exactly one
powered consumer and exactly one power source. Every
powered behaviour in the game is a Kinetic Mill running
every tick instead of every 5th tick, and every volt in
the game comes from a Dynamo outputting LV, with HV
existing only as a transformer's flip of that LV. A
player who powers anything else, or who generates power
any other way, has no second machine and no second
source to choose — the ladder, the cable, and the
destruction rule are proved against one load and one
generator, and a rule proved against one of each is a
rule whose generality is still an assumption. Slice 10
is therefore an electrical-widening slice, kept as small
as the slices before it: **one second powered consumer,
then one distinct power source, proved against both
consumers, and closed by its own review** — not a third
consumer, not a second source, not tiers 3-4, not a
third squad type or doctrine.

## Inputs this scope is decided from

- Slice 9 review (`docs/slice-09-review.md`) ranked
  candidates, carried unchanged from the 0.39.0 scope:
  additional powered consumers and power sources first,
  a third squad type and the deferred doctrines second.
  The 0.43.0 decision recorded that the Step 4 evidence
  gives no reason to reorder them, and this scope keeps
  that order — the top candidate becomes Slice 10, the
  rest become the ranked Slice 11 candidates below, not
  promises. Slice 9 removes one reason widening was
  awkward before: a second consumer no longer needs a
  fixed front-left / front-right / front-centre offset,
  because Slice 8 retired those offsets and builders are
  now distinct by block type through `BuildAimed` — so
  a new machine, if Step 1 builds one, can be placed by
  aim like a wall, a mill, a Dynamo, a transformer, or
  a cable, and each step below must use that rule
  rather than inventing a placement rule of its own,
  except where this scope names a placement gap the
  step itself must close (see the workstation note
  under Step 1).
- The Slice 9 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 10 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. What Slice 10 does,
  like Slices 6, 7, 8, and 9 before it, is write only
  adapter code the checklists will one day run (build
  keys for new block types, and any power indicator a
  step adds) and record, step by step, which parts are
  headless-tested and which parts are Windows-checklist
  only. Electrical balance (gap 3) is not a slice: any
  cost, output level, or powered effect the steps choose
  is a first-pass value for determinism and testability,
  exactly as Dynamo's 5 EmberOre and the mill's
  every-tick cadence were. Destruction with no warning
  (gap 4) remains a future adapter step, not Slice 10
  work: a second consumer's failure rule, if it has
  one, must still be decided in its own step (see Step
  1), but no warning is retrofitted here, and no
  warning rides along as an untested extra. Mesh tint
  and mesh refresh (gap 5) remain named gaps from
  0.30.0, not Slice 10 work: a new block type does not
  need a tint or a refresh hook to prove its core rule,
  and bundling either into this slice would make it two
  systems at once. An energised cable feeding no mill
  showing nothing (gap 6) is the one gap Slice 10 must
  reckon with directly, because its display question
  recurs: each step must decide its own display (see
  Steps 1 and 2), rather than leaving a powered
  workstation or a new source invisible and untestable
  in the scene by inheriting silence. The shared-marker
  question (gap 7) and the free-deploy gap (gap 8)
  remain documentation, not enforcement, for the same
  reason as before.
- Current behaviour this scope must reckon with, not
  ignore: `BlockType` in `src/Forgeborne.Core/Voxel.cs`
  holds KineticMill = 7, Dynamo = 8, Transformer = 9,
  and Cable = 10, all appended only so old saves still
  load, and any new block type must be appended the
  same way, never inserted in the middle of the values.
  `WorldZone.IsPowered` is mill-only by construction:
  it returns true exactly when a cell's delivered
  voltage equals `VoltageRules.MillRated` (LV), and
  `WorldZone.IsMismatched` is mill-only the same way —
  a non-mill cell is never mismatched, whatever voltage
  touches it, because cables are voltage-blind carriers
  and transformers convert both levels. Generalising
  power to a second consumer is therefore NOT free:
  neither `IsPowered` nor `IsMismatched` can simply be
  reused for it, and Step 1 must decide that consumer's
  own rating and its own mismatch semantics, with
  tests, rather than inheriting the mill's by accident.
  `WorldZone.DeliveredVoltage` derives voltage every
  tick from placement — direct Dynamo face-adjacency
  beats any converted or cabled voltage, otherwise the
  single agreed voltage from face-adjacent transformer
  outputs and cable components, mixed voltages resolving
  to None — so a second source in Step 2 must state
  where it sits in that precedence, not merely that it
  "also generates". The tick order is fixed and must be
  reckoned with by any new phase: destruction first
  (`RunMismatchedDestruction`, every tick, before
  either automation phase), kinetic automation every
  5th tick, powered automation every tick, each skipping
  mills the other phase owns. A second consumer's phase
  and its place in that order are Step 1's decision,
  and a second source's delivery must be proved
  against that order, not beside it.
- The one existing machine that is already a candidate
  consumer, and the fact that makes it non-trivial:
  the Workstation (BlockType = 5) already has a real,
  distinct effect — if a workstation block is within
  Manhattan distance 3 of the player, the player restores
  shields per tick (`WorkstationRegenPerTick`, base 5,
  raised by the `WorkstationRegenBonus` skill effect),
  capped at 85 — and it already costs 10 Stone + 5
  EmberOre, so it already competes with Dynamos for ore
  and with mills for stone. But it is the only machine
  with no aimed builder and no adapter key: it is built
  in core only through `World.Build` directly, while
  every Slice 5 / Slice 9 machine has a `Build...Ahead`
  method and a key (T mill, Y Dynamo, U transformer, I
  cable). A powered-workstation step therefore cannot
  merely add a power rule to a machine the player
  already places by key, because no such key exists —
  Step 1 must decide the placement path as part of the
  consumer, or choose a new machine instead, and say
  why. Its effect is also player-centred, not
  machine-centred: today any workstation near the player
  regenerates, with no notion of which workstation is
  powered. Step 1 must decide whether powered means a
  powered workstation near the player, a powered
  workstation anywhere, or something else — and what
  the unpowered workstation still does — rather than
  letting "powered" silently mean two different things
  for the mill and the workstation.
- Why this slice is two core steps, not one: a second
  consumer proved against the Dynamo alone would prove
  the ladder generalises to a second load, but would
  leave the source side still singular — and a second
  source proved against the mill alone would prove a
  new generator, but would leave the widening still
  mill-only. Each step therefore creates the condition
  the next step's proof needs: the consumer step gives
  the source step a second load it must feed, so the
  source step cannot pass by feeding the mill alone.
  Proving both in one step would leave no test able to
  say whether the consumer rule or the source rule
  broke.

## Slice 10, in order

### Step 1 — One second powered consumer

Build exactly one second powered consumer, decided in
that step — not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the consumer, and record it:
  whether it is a powered behaviour for the existing
  Workstation, a second powered behaviour for an
  existing machine, or a new machine altogether. The
  scope's starting hypothesis is a powered Workstation,
  because it is the only existing machine with a real
  effect that power could strengthen without inventing
  a new production chain — but it is a hypothesis, not
  a decision: if the step finds the workstation's
  player-centred regen cannot honestly carry a powered
  distinction, or that its missing builder and key make
  it the wrong first consumer, it must say so and build
  the new machine instead, with a recorded reason none
  of the existing machines fits. What the step must NOT
  do is build two consumers, or a consumer plus a
  source, as one step — the second source is Step 2.
- The step must decide the powered behaviour, and
  record it: what changes when the consumer is powered,
  and what it still does when it is not. The scope's
  starting hypothesis is that the unpowered behaviour
  stays exactly as today — a workstation still
  regenerates at its base rate, a mill still runs at
  kinetic cadence — and the powered behaviour is a
  stated strengthening of that same effect, not a
  different effect wearing the same name. If the step
  instead makes power a requirement (no power, no
  effect at all), it must say why a machine the player
  already owns goes dark rather than weaker, and prove
  the old unpowered demo numbers either stand or change
  with a stated reason. A powered behaviour that is
  only "the same effect, but the HUD says powered"
  is not a consumer: the step must prove a behavioural
  difference a test can measure (a rate, a radius, a
  cap, or a second measurable quantity), not a label.
- The step must decide the rating and the mismatch
  rule, and record them: which delivered voltage (or
  voltages) counts as powered for this consumer, and
  what a mismatched voltage does to it. The scope's
  starting hypothesis is that the consumer is rated LV
  like the mill, because one rating rule for both
  consumers is simpler than two — but the step must
  weigh it itself: a consumer rated HV would need a
  transformer in every powered path and could never
  run on a plain Dynamo, which is a different game
  from a mill that can. The mismatch question is open
  and must not be inherited by accident: Slice 9's
  destruction rule is mill-only, and the step must
  decide whether this consumer is destroyed by HV like
  a mill, disabled to its unpowered behaviour, or
  unaffected — with the destruction phase order (before
  either automation phase, so a destroyed consumer
  acts nothing on its destruction tick) either reused
  or explicitly departed from, proved by a test. A
  consumer with no stated mismatch outcome is not
  decided, even if no test happens to feed it HV.
- The step must decide the placement path, and record
  it: if the consumer is the Workstation, whether it
  gains an aimed builder and an adapter key (which key,
  suppressed while the picker is open like T / Y / U /
  I, with a Windows checklist item for key feel), or
  stays `World.Build`-only with a stated reason a key
  is not needed; if it is a new machine, its aimed
  builder and key by the same rule. The step must also
  decide the cost, and record it: a new machine is
  priced in existing gathered resources only, with no
  new currency invented in this step, and its price
  must be weighed against the machines it competes
  with (Dynamo 5 ore, workstation 10 stone + 5 ore,
  mill 10 stone + 5 wood, squad 10 ore) — a consumer
  priced in ore competes with power itself, a consumer
  priced in stone and wood competes with walls and
  mills, and the step must say which competition it
  intends. A powered workstation adds no new build cost
  unless the step decides one, because the machine
  already has a price — but then the step must say
  what the player pays for power (the Dynamo, the
  cable, the transformer, or a new cost), not pretend
  the powered behaviour is free.
- The step must decide the save and display rules, and
  record them: the scope's starting hypothesis is that
  powered state, like mill power today, is derived at
  load from the blocks and their neighbours, so no new
  powered flag needs saving — unless the consumer's
  design creates stored state (for example, a stored
  charge or a selectable mode), in which case that
  state follows the same old-save / malformed-entry
  rules as squads and weapons (skipped, never fatal)
  and the step must say so. The display question is
  open and must be decided, not inherited: `HudState`
  counts powered mills only (`POWER: x/y MILLS
  POWERED`), so the step must decide whether that count
  gains a second line, a second count, or stays
  mill-only — and if it stays mill-only, it must say
  how a powered consumer is visible at all, rather
  than leaving a powered workstation invisible in the
  HUD as an energised cable feeding no mill already is.
  A display that calls an HV-fed consumer "powered"
  when the step's own mismatch rule destroys or
  disables it would be a display lie the step must
  avoid, by stating what its count means, as Step 1 of
  Slice 9 did for the mill.
- The step must decide the phase and interaction rules,
  and record them: where the consumer's powered effect
  runs in the tick order (with destruction, kinetic,
  and powered automation), whether it stacks with or
  replaces the unpowered effect on a powered tick, and
  how it interacts with the skill that already touches
  it (for a workstation, `WorkstationRegenBonus`).
  The step must also decide whether the consumer's
  output, if it produces anything, goes to the player
  inventory like mill output, or elsewhere — and if
  elsewhere, it must justify a per-machine inventory,
  which Slice 9 deliberately did not build, rather
  than smuggling one in as a consumer detail.
- What must NOT change in this step: the ladder and
  conversion rules from Slice 9; cable distribution
  and mixed-voltage resolution; the mill's rating,
  powered cadence, destruction rule, and cost; Dynamo
  cost and output level; squad, loot, skill (except
  for the interaction the step itself decides and
  proves), and core health rules; the gather / build /
  survive loop; and the save format, except for an
  appended block type the step justifies.

Why first, and why alone: a second load is the
smallest rule that makes "powered" a property of
machines in general instead of a property of one
machine. Until a second consumer can be powered, every
question about power is really a question about mills,
and any source Step 2 builds could only ever be proved
against a mill.

### Step 2 — One distinct power source

Build exactly one distinct power source, decided in
that step — not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the source, and record it:
  what it is, what it outputs, and how it differs from
  a Dynamo. The scope's starting hypothesis is the
  blueprint's alternator name, taken as a name only,
  not a design — because the blueprint's tier 2 list
  names "alternators" beside cables and transformers
  without stating what an alternator outputs, costs,
  or needs. If the step finds the alternator name
  cannot honestly carry a distinct source (for
  example, if every alternator design it can state
  collapses to "a Dynamo with a different mesh"), it
  must say so and build the distinct source under
  another original name, with a recorded reason. What
  the step must NOT do is build a second Dynamo under
  a new name: a source that outputs LV at Dynamo cost
  through Dynamo placement is a reskin, not a source,
  and its tests would pass against a Dynamo's rules.
- The step must decide the output level, and record
  it: the scope's starting hypothesis is LV, the
  Dynamo's level, so that the new source can feed both
  consumers directly — but the step must weigh the
  alternative itself: a source outputting HV directly
  would be genuinely distinct (it would need a
  transformer before any LV-rated consumer could use
  it, and would destroy an LV mill fed directly), and
  that distinctness has a cost in complexity the step
  must name. Whichever level it chooses, the step must
  state where the source sits in `DeliveredVoltage`
  precedence: whether a directly adjacent new source
  beats a converted voltage like a Dynamo does, whether
  two sources touching one network resolve like two
  Dynamos (same voltage is one voltage, mixed voltages
  are None), and what happens when a Dynamo and the
  new source touch the same consumer or the same cable
  network. A source whose precedence is left to the
  walk's scan order is undefined, however deterministic
  the walk is.
- The step must decide the source's own requirements,
  and record them: whether it generates always while
  placed like a Dynamo, or only under a stated
  condition (a fuel, a spin-up, a placement rule, a
  terrain rule), and what happens when that condition
  fails. The scope's starting hypothesis is always-on
  like a Dynamo, because a fuel or condition invented
  in the same step as the source would make a failed
  delivery ambiguous (no fuel, or unwired, or
  conflicted?) — but if the step finds always-on makes
  the source a Dynamo reskin, it must state the
  condition, prove its boundary with a test (the cell
  just inside the condition generating, the cell just
  outside it not), and say what a player sees at that
  boundary. Fuel, crafting, and tiers 3-4 are NOT
  assumed by this scope: a fuel rule is a new system,
  and the step must justify it as its own decision
  with its own tests, not inherit it from the
  blueprint's vocabulary.
- The step must decide the placement, cost, and form,
  and record them: whether the source is a new block
  type (appended only, with an aimed builder and an
  adapter key suppressed while the picker is open,
  plus a Windows checklist item) or a variant of an
  existing block (in which case it must say why a
  single block cannot honestly carry the source, and
  prove the variant's save and placement rules with
  tests, as Slice 9 Step 1 required for a Dynamo
  variant). Its price must be weighed against the
  Dynamo's 5 EmberOre and the consumers it feeds: a
  source priced like a Dynamo must justify why a player
  builds it instead of a second Dynamo, and a source
  priced above a Dynamo must say what that premium
  buys (a different voltage, a different condition, a
  different reach) that a second Dynamo does not.
- The step must decide the proof burden, and record
  it: the source must be proved feeding BOTH consumers
  — the mill and the Step 1 consumer — through the
  same delivery paths (face-adjacency and, where the
  step's design allows it, cable), not the mill alone.
  A source step whose tests feed only the mill has
  proved a generator, not a widening, and the review
  must not close it as one. The step must also prove
  the source against the Step 1 mismatch rule: a
  source whose output destroys or disables a consumer
  must be proved doing so, and a source whose output
  is safe for both consumers must be proved safe for
  both, not assumed from the mill's proof.
- The step must decide the save and display rules, and
  record them: the scope's starting hypothesis is that
  source output, like Dynamo output today, is derived
  at load from placement, so no new source flag needs
  saving and an old save's Dynamos are untouched. The
  display question is open: the step must decide
  whether the HUD gains a source count or line, or
  whether the existing powered-consumer counts are
  enough — and if a live source feeding no consumer
  shows nothing, it must say so openly, as Step 2 of
  Slice 9 did for an energised cable feeding no mill,
  rather than leaving a second invisible live network
  beside the first.
- What must NOT change in this step: the ladder and
  conversion rules from Slice 9, except for the
  precedence addition this step itself justifies; cable
  distribution and mixed-voltage resolution, except
  where a new source's precedence changes a network,
  which the step must prove; the mill's and the Step 1
  consumer's ratings, powered behaviours, mismatch
  rules, and costs; Dynamo cost and output level;
  squad, loot, skill, and core health rules; the gather
  / build / survive loop; and the save format, except
  for an appended block type the step justifies.

Why second, and why alone: a second source is the
rule that makes "generation" a choice instead of a
given. Until a second source exists, every power
decision is "build a Dynamo or go without", and no
trade-off — voltage against cost, condition against
convenience, reach against price — can be tested at
all. Proving it after the second consumer exists means
its tests can feed two different loads through two
different delivery paths, rather than staging a source
against the one load that already had a generator.

### Step 3 — Slice 10 demo scenario and review

As with 0.13.0, 0.19.0, 0.23.0, 0.26.0, 0.31.0, 0.34.0,
0.38.0, and 0.43.0: extend the demo with one composed
Slice 10 run, then write `docs/slice-10-review.md` as a
documentation-only step. The demo proves, headlessly,
the second consumer and the second source together,
not separately: one Simulation in which a mill and the
Step 1 consumer both run powered, an unpowered
baseline of each still runs at its unpowered behaviour
beside them, the Step 2 source feeds both consumers
(through the delivery paths the steps proved), and a
mismatched delivery, if the steps' rules create one,
behaves by the rule the step decided — through a
save/load round-trip that keeps the surviving blocks,
keeps any destroyed block gone, and re-derives power
on load without a stored power flag. The demo cannot
prove adapter feel — no headless run can show a new
build key feeling right, a powered effect reading right
in the scene, or a new source looking distinct — so
the review's verdict must separate the two claims in
words: the core consumer and source rules are tested,
the adapter keys and any indicator are written and
checklist-listed, and neither claim is allowed to
borrow the other's evidence. The review closes Slice
10 as tested electrical widening (one second consumer,
one distinct source) plus written adapters — not as
playtested, balanced, or Unity-verified — and names
the next candidates in priority order. Slice 11 is not
scoped by that review; it is scoped afterwards, the
same way this document follows the Slice 9 review's
candidates rather than preceding them.

## Deliberately NOT in Slice 10

- A third powered consumer, or a second source added
  alongside the first as a bonus. One consumer and one
  source are Slice 10; further widening is a Slice
  11-or-later decision with Slice 10's evidence in hand.
- Automation tiers 3-4 (digital / arcane), crafting,
  and a per-machine inventory system beyond the player
  inventory rule Slice 5 decided, unless a Step 1
  consumer justifies a per-machine inventory for its
  own behaviour with its own tests — it is not assumed
  here, and a consumer that needs one must say so in
  its step, not in this scope.
- A fuel system, unless Step 2 justifies one as its own
  decision with its own boundary tests — it is not
  assumed here, and a source that needs fuel must say
  so in its step, not in this scope.
- A per-hop voltage loss, a cable length limit, or a
  drag-to-lay placement mode. None was decided in Slice
  9 Step 2 as its own stated rule with a boundary test,
  so none rides along as an untested extra here.
- A third squad type, or a second kind added alongside
  anything else as a bonus. One new kind was Slice 7;
  the next kind, if any, is a Slice 11-or-later decision
  with Slice 10's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a power behaviour.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 10 wiring stays in the FPS view,
  not a second camera.
- A new movement mode, a weapon rework, or a balance
  pass. Any cost, output level, powered effect, or
  mismatch rule the steps choose is a first-pass value
  for determinism and testability, exactly as Slice 3's
  prices and Slice 5's Dynamo cost were, and must be
  labelled as such, not quoted as final feel.
- A pre-destruction warning, a mesh tint for any new
  block, or a mesh refresh on any key build. All three
  remain named gaps from 0.30.0 / 0.42.0, not fixed by
  a widening step — a warning for the Step 1 consumer,
  if its mismatch rule needs one to be fair, is named
  as adapter work with its own checklist item in that
  step, not as a core rule smuggled into the adapter.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- Windows Unity verification itself. It remains open
  for Slices 1-10 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.
  The checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification
  plan, and Slice 10 steps must add their own checklist
  items to those docs rather than claiming verification.

## Ranked candidates for Slice 11 scoping (not promised)

These are recorded here so the Slice 10 review starts
from a list, not from scratch. Order follows the
0.39.0 / 0.43.0 ranking with the Slice 10 candidate
removed:

1. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 10's electrical evidence will inform, not
   promises made before that evidence exists.
2. Further electrical widening beyond one consumer and
   one source (a third consumer, a third source, or a
   display for live networks feeding no consumer) —
   deferred, not rejected, and only after Slice 10
   proves the second of each.

## Slice 10 gate (what "done" means)

Slice 10 is done when: every step above is pushed with
passing tests; the second consumer's powered behaviour,
rating, mismatch rule, placement path, cost, and
display are each decided and tested, with its unpowered
behaviour unchanged or changed only for a stated reason
with its old numbers accounted for; the distinct
source's output level, precedence, requirements, cost,
and display are each decided and tested, proved feeding
both consumers and not merely a Dynamo reskin; the
ladder, conversion, cable, and destruction rules from
Slice 9 are unchanged except for the precedence and
mismatch additions the steps themselves justify; the
composed demo proves the consumer and the source
together through a save/load round-trip, with any
destroyed block staying destroyed and power re-derived
on load without a stored flag; and `docs/slice-10-review.md`
records the verdict honestly, separating headless-tested
core claims from Windows-checklist adapter claims,
including that Slice 10 is still not playtested,
balanced, or Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is not
closed by Slice 10.
