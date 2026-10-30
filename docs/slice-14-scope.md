# Forgeborne — Slice 14 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 14 system is
built by this document. Each step below is built one at a time,
with tests where a step has a headless-testable claim and with
Windows-checklist items where it does not, in this order, and
Slice 14 is closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 13 closed the visibility-and-fairness gap Slice 12 left open:
a threatened machine is now warned in the HUD, tinted in the mesh
data, and distinct by squad kind in the scene markers, as tested
core plus written adapters. What Slice 13 deliberately did not
touch is the electrical side itself: the game still has exactly
two power sources, both always-on while placed — a Dynamo
delivering LV for 5 EmberOre, and an Alternator delivering HV
for 10 Stone — feeding exactly three consumers on a two-level
ladder, LV / HV / None: a mill and a workstation rated LV, and
a Shock Pylon rated HV. Slice 12 chose a third consumer over a
third source, and Slice 13 added no source at all, so the third
source has now been deferred twice, not rejected twice. Slice 14
is therefore the deferred electrical widening, kept as small as
the slices before it: **one third source first, then its own
demo and review** — not Patrol, not a fourth kind, not a second
doctrine, not a ladder redesign.

## Inputs this scope is decided from

- Slice 13 review (`docs/slice-13-review.md`) ranked
  candidates, carried unchanged: the deferred third source
  first, Patrol second, and any Slice 13 remainder third.
  The Step 4 composition evidence gives no reason to
  reorder them — a warned mill, a rewired pylon kill, and
  three squad kinds in one snapshot say nothing about
  whether a third source can be distinct from the two that
  already exist — so the top candidate becomes Slice 14,
  and the second and third become the ranked Slice 15
  candidates below, not a promise.
- The Slice 13 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 14 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. What Slice 14 does,
  like Slices 6 through 13 before it, is build only core
  code the headless tests can prove plus adapter code
  the checklists will one day run, and record, step by
  step, which parts are headless-tested and which parts
  are Windows-checklist only. Electrical balance (gap 3)
  is not a slice: any cost, condition, or output shape
  a step chooses is a first-pass value for determinism
  and testability, exactly as the pylon's range 5 and
  damage 10 were, and must be labelled as such, not
  quoted as final feel. The scene proof for the
  visibility adapters (gap 4: whether the chunk material
  multiplies vertex colours, whether
  `FpsInputAdapter.MeshRenderer` is wired to the same
  renderer, and whether markers read through occlusion)
  stays exactly where Slice 13 left it: three Windows
  observations inside checklist items 25, 26, and 27, not
  a headless step — a headless run cannot close an
  observation about a scene it cannot run, so gap 4 is
  carried as a Slice 15 candidate, not smuggled into
  Slice 14 as a pretend step. The deferred third source
  (gap 5) is Slice 14's work, taken here as one source,
  not as a source family. Patrol (gap 6) stays deferred
  with its route question intact. The free deploy
  primitive (gap 7) remains a headless-only rule,
  re-named, not quietly enforced here.

## Slice 14, in order

### Step 1 — One third power source

Add one source, decided in that step — not in this scope
document — with the reasoning recorded in DECISIONS.md:

- The step must decide the source's distinctness beyond
  voltage, and record it: the scope's starting facts are
  that the ladder has only two levels, so a third source
  cannot be distinct by voltage alone — it must deliver
  LV or HV, duplicating one existing source's level, and
  its distinctness must come from somewhere else. The
  starting hypotheses, none of them a decision, are: a
  condition (it outputs only while some stated condition
  holds, and idles otherwise); a fuel (it consumes a
  stated resource per tick or per delivery, and stops
  when the fuel runs out); a cost shape (a price no
  existing source charges, in a combination no existing
  source uses); a delivery shape (it feeds only directly,
  or only through cable, or only through a transformer,
  where the existing sources feed all three ways); or a
  pulsed / intermittent output (it delivers on some
  ticks and not others, by a stated rule). A hypothesis
  is not a decision: if the step finds that one of these
  alone is a reskin — a cost-only difference at the same
  voltage with the same always-on behaviour would be
  exactly the reskin Slice 10 rejected for an LV source
  at Dynamo cost — it must say so and combine or
  replace it, with the evidence. What the step must NOT
  do is claim distinctness by voltage: there is no
  third level on this ladder, and inventing one would
  be a ladder redesign, not a single source step, so a
  third voltage level is out of scope for Step 1.
- The step must decide the source's voltage, and record
  it: LV or HV, with the consequences stated, not
  assumed. An LV third source feeds a mill and a
  workstation safely by direct feed and disables a pylon
  by direct feed; an HV third source destroys a mill by
  direct feed, disables a workstation by direct feed,
  and feeds a pylon safely by direct feed. Whichever it
  chooses, the step must carry the mismatch proofs for
  that voltage — the consumers it destroys or disables
  directly are part of the source's proof, not an
  accident to be discovered by the demo — and must
  prove safe use through a transformer step-down or
  step-up exactly as the Alternator step did, where
  safe use needs one.
- The step must decide what happens when the source is
  not outputting, and record it: if the distinctness is
  a condition or a fuel, the step must say, precisely,
  what a consumer beside it reads while the condition
  fails or the fuel is out — None (unpowered, with
  kinetic fallback for a mill and idle for a
  workstation or a pylon), or a held voltage — and
  whether the condition or the fuel is derived from
  placement, from inventory, from the tick count, or
  from a per-source state. If it is per-source state,
  the step must also decide the save question: whether
  that state is saved, re-derived on load, or reset on
  load, and prove old saves still load either way.
  An always-on third source has no such question, and
  must say so instead of leaving it unasked.
- The step must decide the generalisation, and record
  it: `DeliveredVoltage`, `TransformerOutput`, and
  `CableNetworkVoltage` today branch on Dynamo and
  Alternator by name, and the Slice 10 step generalised
  them from "direct Dynamo" to "direct source" once
  already. Step 1 generalises them again, to three
  sources, without changing precedence or conflict:
  two distinct direct sources touching one cell, one
  transformer, or one cable network still resolve to
  None, not to a silent winner, whichever two they are
  — and a third source beside a Dynamo, beside an
  Alternator, and beside both must each be proved, not
  just the pair the step happens to stage first. No
  delivery, precedence, conflict, rating, cost, or
  phase rule for any existing block moves because a
  third source arrives: the new source joins the rules,
  it does not rewrite them.
- The step must decide the block, the cost, and the
  adapter line, and record them: a new `BlockType`
  appended only (13, so earlier values are unchanged
  and old saves still load), a cost stated against the
  existing prices (Dynamo 5 EmberOre, Alternator 10
  Stone, transformer 5 EmberOre, cable 1 EmberOre per
  cell, pylon 5 Stone + 10 EmberOre), a `Simulation`
  builder by aim like every other block since Slice 8
  (no placement rule of its own, no revived offset),
  and an adapter key confirmed free at step time
  (builder keys in use include T, Y, U, I, B, N, and P,
  suppressed while the picker is open, as the
  Alternator's N key is) plus a playtest-gate checklist
  item, Windows-only, for that key. No new HUD source
  count is assumed: by the Dynamo and Alternator
  precedent, the existing powered counts plus the Slice
  12 live-network / lone-source counts are the display,
  and the step must prove the third source is counted
  as live or lone by those counts, or say why a new
  count is needed — a lone third source showing nothing
  would repeat, silently, the observation Slice 12
  recorded for a lone Alternator before it counted one.
- The step must state its verification honestly, and
  record it: a source is core code and is
  headless-testable, so the tests must prove direct
  feed at its voltage (including the mismatch outcomes
  for that voltage), step-down / step-up through a
  transformer, cable distribution, the conflict cases
  above, the condition or fuel behaviour across ticks
  (if any), save / load re-derivation, and agreement
  with the Slice 13 readouts — the warning predicates
  and the `PowerState` query must read the new source's
  cells correctly, because a source the warning cannot
  see would be a visibility regression, not a widening.
  Every earlier demo save length must stand.

### Step 2 — Slice 14 demo scenario and review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0, 0.55.0,
and 0.60.0: extend the demo with one composed Slice 14
run (the third source feeding a consumer at its voltage
beside an existing source feeding a consumer at its
voltage, with a mismatch or idle case beside them so
the demo proves a distinction, not a label — not
separate proofs), then write `docs/slice-14-review.md`
as a documentation-only review. The demo's job is
composition of the headless-testable parts: the new
source must deliver, convert, or idle exactly as Step 1
proved it, through a save / load round-trip, beside
the sources and consumers the reader already trusts.
The review checks the slice against this scope, states
the verdict honestly (tested third source plus written
adapter, not playtested, balanced, or Unity-verified),
separates headless-tested core claims from
Windows-checklist adapter claims (the builder key),
carries the gaps that remain, and does NOT scope
Slice 15 — Slice 15 is scoped afterwards, the same way
this document follows the Slice 13 review's candidates
rather than preceding them.

## Deliberately NOT in Slice 14

- Patrol, a fourth squad kind, or a second doctrine.
  The orbital side is closed for this slice; an
  electrical slice that also adds a doctrine would make
  the source work untestable in isolation.
- A third consumer, a fourth source, or any other
  electrical widening added alongside the third source
  as a bonus. One source is Slice 14; a source family
  would make the distinctness proof untestable as a
  unit, because a failing test could not say which
  source failed it.
- A third voltage level, or any ladder redesign. The
  ladder stays LV / HV / None in this slice; a new
  level would re-rate every consumer and re-prove
  every transformer, cable, and conflict case, which
  is a slice of its own, not a step inside this one.
- A warning, tint, marker, or HUD-count change, except
  where Step 1 must prove the existing readouts already
  cover the new source. Display steps must not move
  the rules they display, and a source step must not
  move the displays it is proved against — if the
  existing counts do not cover a lone third source,
  that is a finding Step 1 records and the review
  carries, not a display redesign smuggled into a
  source step.
- A delivery, transformer, or cable rule change beyond
  the source generalisation above. No loss, no length
  limit, no new conversion rule rides along with the
  third source.
- A balance pass. The third source's cost, condition,
  fuel, or output shape is a first-pass value chosen
  for determinism and testability, exactly as the
  Alternator's stone price and the pylon's range 5 and
  damage 10 were, and must be labelled as such, not
  quoted as final feel.
- A fuel, inventory, or economy system of its own. If
  Step 1 chooses a fuelled distinctness, the fuel is
  one stated resource consumed by one stated rule for
  this source — not a general fuel economy, not a
  second inventory, and not a reason to re-price
  existing blocks.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 14 adds a block, not a camera.
- A new movement mode, a weapon rework, or a
  free-deploy enforcement change. The free
  `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- A new save field, unless Step 1's distinctness
  justifies one and proves old saves still load. A
  source that can be placed and derived needs no field,
  the way the Dynamo and the Alternator need none.
- Windows Unity verification itself. It remains open
  for Slices 1–14 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 14 steps must add their
  own checklist items to those docs rather than
  claiming verification.

## Ranked candidates for Slice 15 scoping (not promised)

These are recorded here so the Slice 14 review starts
from a list, not from scratch. Order follows the
0.56.0 / 0.60.0 ranking with the Slice 14 candidate
removed:

1. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.
2. Any Slice 13 remainder the Slice 13 review names
   (the vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in its gap 4) — three
   Windows observations inside checklist items 25, 26,
   and 27, recorded here as observations to be made
   when Windows verification runs, not as a headless
   slice step, and not promised here.
3. Any Slice 14 remainder the Slice 14 review names
   (for example a source sub-question this slice splits
   off rather than closes) — recorded at review time,
   not promised here.

## Slice 14 gate (what "done" means)

Slice 14 is done when: every step above is pushed
with passing tests (or, for the adapter-only part of
Step 1, with the test suite unchanged for that part
and the adapter-only verification stated, never
inflated into a test claim); the Step 1 source's
distinctness beyond voltage, voltage, non-output
behaviour, generalisation, block, cost, and adapter
line are each decided and tested, with no existing
delivery, precedence, conflict, rating, cost, or phase
rule changed, no existing consumer re-rated, the
ladder still LV / HV / None, and every earlier demo
save length unchanged; the composed demo proves the
third source beside the existing sources through a
save / load round-trip; and `docs/slice-14-review.md`
records the verdict honestly, separating
headless-tested core claims from Windows-checklist
adapter claims, including that Slice 14 is still not
playtested, balanced, or Unity-verified. The Slice 1
director playtest gate is still pending throughout
and is not closed by Slice 14.
