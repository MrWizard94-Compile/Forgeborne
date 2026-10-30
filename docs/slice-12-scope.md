# Forgeborne — Slice 12 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 12 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 12 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 11 closed the squad-widening gap Slice 10 left open:
the orbital side now has three squad kinds (Vanguard,
Recon, Heavy) and three doctrines (HoldGround, Advance,
Defend Core), proved together in one composed demo.
What Slice 11 did not touch is the width of the power
side, and what the power side still cannot show. The
power system now has two powered consumers (Kinetic
Mill and Workstation, both rated LV) and two distinct
sources (Dynamo at LV, Alternator at HV), proved
against each other through transformers and cables.
But a player who lays a live cable network that feeds
no consumer yet — or places a lone Alternator, or a
lone Dynamo, before any consumer is wired to it —
sees nothing for it: the HUD counts only consumers
(`PoweredMills` / `TotalMills`, `PoweredWorkstations` /
`TotalWorkstations`, rendered as two POWER lines by
`HudDisplay`), and a live network with no consumer
on it, and a lone source of either kind, are
invisible in the HUD as well as in the scene. And a
player who wants a third electrical choice — a
consumer that actually wants HV, so the Alternator
has a safe direct use, or a third source with a
distinctness a Dynamo reskin would not have — has
neither. Slice 12 is therefore an electrical-widening
slice, kept as small as the slices before it: **one
live-network display first, then one further
electrical widening decided in its step, and closed
by its own review** — not a balance pass, not a
warning / tint / refresh system, not a squad-kind
marker distinction, not Patrol, not a fourth kind.

## Inputs this scope is decided from

- Slice 11 review (`docs/slice-11-review.md`) and scope
  (`docs/slice-11-scope.md`) ranked candidates, carried
  unchanged: further electrical widening first,
  adapter visibility and fairness second. The Step 3
  composition evidence gives no reason to reorder
  them — the Heavy / Defend Core pairing proved
  squads compose, and says nothing about power
  width — so the top candidate becomes Slice 12,
  and the second becomes the ranked Slice 13
  candidate below, not a promise. Slice 11's
  candidate list named the electrical widening as
  "a third consumer, a third source, or a display
  for live networks feeding no consumer", and this
  scope orders that list instead of treating it as
  a menu: display first, then one widening, because
  a display step is the smallest step that closes a
  named gap on its own, and because a third
  consumer or source built before the display
  exists would be proved only through the consumer
  counts it already has, while a display built
  first gives the widening step a second, honest
  readout for its own proof.
- The Slice 11 review's gaps, in priority order, and
  what this scope does with each: Unity verification
  (gap 1) and the director playtest (gap 2) remain
  open, but neither is Slice 12 work in itself — the
  first is blocked on Unity 6000 on Windows, the
  second on the director — and scoping a slice does
  not close, reorder away, or pretend to answer
  either. What Slice 12 does, like Slices 6, 7, 8,
  9, 10, and 11 before it, is write only adapter
  code the checklists will one day run (a HUD line
  for the new display, and any binding a step adds)
  and record, step by step, which parts are
  headless-tested and which parts are
  Windows-checklist only. Squad balance (gap 3) is
  not a slice, and electrical balance is not one
  either: any rating, cost, or effect a step
  chooses is a first-pass value for determinism
  and testability, exactly as the mill's LV rating
  and the Alternator's stone-only cost were, and
  must be labelled as such, not quoted as final
  feel. Destruction and disablement with no warning
  (gap 4) and mesh tint and mesh refresh (gap 5)
  remain named gaps, not Slice 12 work: Step 1 is a
  HUD count, not a warning, not a tint, and not a
  mesh refresh, and the step must say so plainly
  rather than letting a new HUD line be quoted
  later as if it warned anyone. A live network
  feeding no consumer showing nothing (gap 6) is
  Step 1's gap, closed by Step 1, not carried past
  it. Three kinds sharing one scene marker (gap 7)
  remains a display question for Slice 13 scoping
  to decide as one visibility question together
  with gaps 4 and 5, not a Slice 12 extra: Step 1
  displays power, not squads, and must not smuggle
  a marker distinction in as a display step.
  Patrol deferred (gap 8) stays deferred with its
  route question intact. The free deploy primitive
  (gap 9) remains a headless-only rule, re-named,
  not quietly enforced here.

## Slice 12, in order

### Step 1 — A display for live networks and lone sources

Add one HUD display, decided in that step — not in
this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide what counts as live, and
  record it: the scope's starting hypothesis is
  that a network is live when it carries a voltage
  that is not None — a cable component whose
  `CableNetworkVoltage` is LV or HV, a transformer
  output that is not None, and a source cell
  (Dynamo LV, Alternator HV) on its own — because
  "live" must mean "carrying power", not merely
  "placed". A cable component with no source, and
  a mixed-voltage component that resolves to None,
  are therefore not live under this hypothesis,
  however many cable cells they contain; a lone
  source is live, because it is already generating,
  even with no cable and no consumer. A hypothesis
  is not a decision: if the step finds that
  counting a lone source as a network of one
  confuses the count, or that a transformer with
  no input should count differently from a cable
  with no source, it must say so and count
  otherwise, with the evidence. What the step must
  NOT do is count placed blocks regardless of
  voltage — a cable count is not a live-network
  count — or count a consumer's delivery twice by
  counting both the network and the mill it feeds
  as if they were two live things.
- The step must decide the count's shape, and
  record it: the scope's starting hypothesis is
  derived HUD counts in `HudState.FromWorld`, in
  the pattern of `PoweredMills` / `TotalMills` —
  derived from placement on every snapshot, never
  saved, so removing a source drops the count on
  the next snapshot — with one number per thing
  counted, not a voltage split smuggled in as two
  numbers unless the step justifies the split.
  The questions the step must answer, not assume,
  are: whether networks and sources are one count
  or two (a lone source and a cabled network are
  not the same shape of thing, and folding them
  into one number may hide which one the player
  built); whether the count is of networks, of
  energised cells, or of sources (a ten-cable LV
  run is one network but ten cells, and the step
  must say which number it prints, because each
  answers a different player question); and
  whether an HV network and an LV network count
  alike or apart (the existing consumer counts
  answer "rated voltage or not", and a live
  count that hides the voltage would let an HV
  network look identical to a safe LV one).
  The step must reuse the existing network walk,
  not duplicate it: `CableComponent` and
  `CableNetworkVoltage` are private to `WorldZone`
  today, so the step must expose a counting path
  through them rather than re-walking cables in
  the HUD, or record why a new walk is genuinely
  needed. Delivery rules must NOT change in this
  step: no voltage, precedence, conflict, rating,
  cost, or phase rule moves because a count is
  added, and every existing demo save length must
  stand, because a display step that changes a
  save has changed the game, not the display.
- The step must decide the adapter line, and
  record it: `HudDisplay` today renders two POWER
  lines from two existing Text fields, so a new
  display needs either a third Text field with its
  own line or a stated reason to fold the new
  count into an existing line — and a
  playtest-gate checklist item, Windows-only, for
  whichever it chooses. The headless snapshot
  already carries the numbers, so no new snapshot
  line is assumed beyond the HUD fields themselves;
  but the scene question must be answered, not
  inherited: a HUD line is not a mesh tint and not
  a mesh refresh, and the step must not claim the
  network is visible in the scene because it is
  counted in the HUD. Gap 5 stays open after this
  step, by decision, and the step's review must
  say so.

### Step 2 — Exactly one further electrical widening

Add one widening, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose the widening, and record
  it: the scope's starting pool is the one the
  Slice 11 review named — a third consumer or a
  third source — and the step chooses exactly one
  from it, or another original electrical widening
  only if it records why neither fits the gap it
  is closing. The scope's starting hypothesis is a
  third consumer first, because both existing
  consumers are rated LV, so HV today has no safe
  direct use: a mill fed HV is destroyed, a
  workstation fed HV is disabled, and safe HV use
  always costs a transformer step-down. A
  consumer rated HV would therefore prove itself
  the way the Alternator proved itself — by a
  test a Dynamo-fed, LV-rated consumer cannot pass
  (direct Alternator feed, safe) — while a third
  source on a two-level ladder would need a
  distinctness beyond its voltage (a condition, a
  fuel, a cost shape) that the step would have to
  invent and justify as a second system. A
  hypothesis is not a decision: if the step finds
  an HV-rated consumer collapses into a mill with
  a different rating constant, or that a third
  source's distinctness is in fact already proved
  by cost or placement alone, it must say so and
  choose otherwise, with the evidence. The
  widening not chosen is deferred, not rejected.
- The step must decide the widening's rules, and
  record them: if a third consumer, its rating,
  its powered effect and phase, its mismatch rule
  (destroyed like a mill, disabled like a
  workstation, or something else with a reason),
  its cost and placement path (aimed builder and
  adapter key, confirmed non-colliding while
  picking), and its display (the existing
  consumer-count pattern, extended, or a stated
  reason not to). If a third source, its output
  level or condition, its DeliveredVoltage
  precedence against Dynamo and Alternator, its
  cost, and the proof that it is not a reskin of
  either. In either case: appended as an enum
  value only, so old saves still load; no
  existing consumer's rating, cost, effect, or
  mismatch rule changes; no existing source's
  output or precedence changes; no delivery,
  transformer, or cable rule changes except the
  ones the widening itself justifies and tests.
  No second consumer and no second source ride
  along, and no balance pass rides along either.
- The step must decide the adapter binding and the
  display, and record them: a new block needs a
  builder key the step names and confirms free
  (the builder keys today run T / Y / U / I / N /
  B by block, so the step must check the actual
  adapter, not the scope's memory of it), plus a
  playtest-gate checklist item, Windows-only, and
  a HUD-count decision in Step 1's terms — a new
  consumer that Step 1's display cannot count, or
  a new source that Step 1's display cannot show
  live, must say so rather than leaving Step 1's
  counts silently wrong for the new block.

### Step 3 — Slice 12 demo scenario and review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, and 0.51.0:
extend the demo with one composed Slice 12 run
(the Step 1 display reading a live network with no
consumer, a lone source, and the Step 2 widening
in the same Simulation — not three separate
proofs), then write `docs/slice-12-review.md` as a
documentation-only review. The demo's job is
composition: the display must read live where the
old counts read nothing, and the widening must run
beside an existing consumer and an existing source
so the reader can see it is a choice among
choices, not a replacement for them. The review
checks the slice against this scope, states the
verdict honestly (tested display and widening
core plus written adapters, not playtested,
balanced, or Unity-verified), separates
headless-tested core claims from Windows-checklist
adapter claims, carries the gaps that remain, and
does NOT scope Slice 13 — Slice 13 is scoped
afterwards, the same way this document follows
the Slice 11 review's candidates rather than
preceding them.

## Deliberately NOT in Slice 12

- A second consumer or a second source added
  alongside the Step 2 widening as a bonus. One
  widening is Step 2; the next, if any, is a Slice
  13-or-later decision with this slice's evidence
  in hand.
- A pre-destruction or pre-disablement warning, a
  mesh tint, or a mesh refresh on any key build.
  All three remain named gaps from 0.30.0 /
  0.42.0, not fixed by a display step: Step 1
  counts live networks in the HUD, it does not
  warn before a mill is destroyed, tint an
  energised cable, or refresh a mesh.
- A scene-marker distinction for the squad kinds.
  That is the Slice 13 visibility candidate, not a
  Slice 12 extra: Step 1 displays power, and a
  marker step that rides along would make the
  display untestable as a display.
- Patrol, a fourth squad kind, or a second
  doctrine. The orbital side is closed for this
  slice; an electrical slice that also adds a
  doctrine would make the electrical widening
  untestable in isolation.
- A balance pass. The Step 2 widening's rating,
  cost, and effect are first-pass values chosen
  for determinism and testability, exactly as
  Slice 9's transformer cost and Slice 10's
  Alternator cost were, and must be labelled as
  such, not quoted as final feel.
- An Orbital camera, or a mini-map beyond the Slice
  1 placeholder. Slice 12 markers remain scene
  markers in the FPS view, not a second camera.
- A new movement mode, a weapon rework, or a
  free-deploy enforcement change. The free
  `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- Windows Unity verification itself. It remains open
  for Slices 1–12 alike and is blocked on Unity
  6000 on Windows, not on anything this slice can
  do headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 12 steps must add
  their own checklist items to those docs rather
  than claiming verification.

## Ranked candidates for Slice 13 scoping (not promised)

These are recorded here so the Slice 12 review starts
from a list, not from scratch. Order follows the
0.48.0 / 0.51.0 ranking with the Slice 12 candidate
removed:

1. Adapter visibility and fairness as one
   candidate: a pre-destruction / pre-disablement
   warning, a mesh tint and mesh refresh, and a
   scene-marker distinction for the squad kinds —
   gaps named in the Slice 9, Slice 10, and Slice
   11 reviews, to be decided at Slice 13 scoping
   as one visibility question or split apart
   there, not promised here.
2. The deferred electrical widening from Step 2's
   pool (a third consumer if Step 2 chose a third
   source, or a third source if Step 2 chose a
   third consumer) — deferred, not rejected, and
   only after Slice 12 proves its widening.
3. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.

## Slice 12 gate (what "done" means)

Slice 12 is done when: every step above is pushed
with passing tests; the Step 1 display's definition
of live, its count shape, and its adapter line are
each decided and tested, with delivery rules
unchanged and every earlier demo save length
unchanged; the Step 2 widening's rating or output,
effect or condition, mismatch or precedence rule,
cost, placement path, binding, and display are each
decided and tested, with existing consumers' and
sources' rules unchanged and old saves loading;
the composed demo proves the display and the
widening together through a save/load round-trip,
with a live network reading live where the old
counts read nothing and no new save field; and
`docs/slice-12-review.md` records the verdict
honestly, separating headless-tested core claims
from Windows-checklist adapter claims, including
that Slice 12 is still not playtested, balanced,
or Unity-verified. The Slice 1 director playtest
gate is still pending throughout and is not closed
by Slice 12.
