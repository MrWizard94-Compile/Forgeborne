# Forgeborne — Slice 11 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 11 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 11 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 10 closed the electrical-widening gap Slice 9 left open:
the power system now has two powered consumers and two
distinct sources, proved against each other. What Slice 10
did not touch is the width of the orbital side. There are
still exactly two squad kinds and exactly two doctrines.
Every squad a player fields is a Vanguard generalist
(3 members, 30 HP each, 10 damage each, range 3) or a
Recon screen (2 members, 20 HP each, 6 damage each,
range 5), and every squad obeys HoldGround (march one
voxel per tick to a rally, fire only once holding) or
Advance (hunt the nearest living enemy one voxel per
tick, fire whenever in range, rally ignored after
deployment). A player who wants a third tactical choice —
a tougher anchor, a flying scout, a squad that guards
the core instead of a rally, a squad that walks a route
instead of chasing — has no third kind and no third
doctrine to choose. Slice 11 is therefore a
squad-widening slice, kept as small as the slices before
it: **one third squad kind, then one deferred doctrine,
proved across the kinds it actually serves, and closed
by its own review** — not a fourth kind, not a second
doctrine, not a third consumer or source, not a balance
pass.

## Inputs this scope is decided from

- Slice 10 review (`docs/slice-10-review.md`) and scope
  (`docs/slice-10-scope.md`) ranked candidates, carried
  unchanged from the 0.44.0 scope: a third squad type
  and the deferred doctrines first, further electrical
  widening second. The 0.47.0 decision recorded that the
  Step 3 composition evidence gives no reason to
  reorder them, and this scope keeps that order — the
  top candidate becomes Slice 11, the rest become the
  ranked Slice 12 candidates below, not promises.
  Slice 10 removes one reason squad widening was
  awkward before: the picker, the save field, the
  snapshot, and the display already accept a kind and a
  doctrine, so a new kind or doctrine does not need a
  new picker, a new save field, or a new display line
  invented for it — it needs a shape, a behaviour, a
  binding, and the tests that prove them, exactly as
  Slice 7 Step 1 and Slice 4 Step 2 each proved theirs.
- The Slice 10 review's gaps, in priority order, and
  what this scope does with each: Unity verification
  (gap 1) and the director playtest (gap 2) remain
  open, but neither is Slice 11 work in itself — the
  first is blocked on Unity 6000 on Windows, the
  second on the director — and scoping a slice does
  not close, reorder away, or pretend to answer
  either. What Slice 11 does, like Slices 6, 7, 8, 9,
  and 10 before it, is write only adapter code the
  checklists will one day run (a picker key for the
  new kind, a picker key for the new doctrine, and any
  display change a step adds) and record, step by
  step, which parts are headless-tested and which
  parts are Windows-checklist only. Electrical balance
  (gap 3) is not a slice, and squad balance is not
  one either: any shape, price, or cap the steps choose
  is a first-pass value for determinism and
  testability, exactly as Vanguard's numbers and the
  10-ore price were, and must be labelled as such,
  not quoted as final feel. Destruction and
  disablement with no warning (gap 4), mesh tint and
  mesh refresh (gap 5), and a live network feeding no
  consumer showing nothing (gap 6) remain named gaps,
  not Slice 11 work: a squad step must not retrofit
  them, and must not let a new kind or doctrine
  silently depend on them either. The workstation key
  (B) having no playtest-gate checklist item (gap 7)
  is different: it is a gate-doc gap, and the 0.47.0
  decision said the next step that touches the gate
  doc for its own reason closes it. Step 1 below adds
  a checklist item for its own key, so Step 1 touches
  the gate doc for its own reason — Step 1 therefore
  also adds the missing B item, named here so the
  repair is a stated part of Step 1 and not a quiet
  extra. Two kinds sharing one scene marker (gap 8)
  grows with a third kind: Step 1 must state its
  marker decision (shared marker carried, or a
  distinction with its own checklist item), not
  inherit the shared marker by accident. The free
  deploy primitive (gap 9) remains a headless-only
  rule, re-named, not quietly enforced here.

## Slice 11, in order

### Step 1 — Exactly one third squad kind

Add one third kind, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose the kind, and record it: the
  scope's starting pool is the one the code already
  names — heavy, flyer — carried from Slice 7's pool
  with Recon already spent, and the step chooses
  exactly one from it, or another original role only
  if it records why neither of the two fits the gap
  it is closing. A hypothesis is not a decision: the
  pool is a starting point, not a vote, and the step
  must say what tactical choice the new kind adds
  that Vanguard and Recon cannot already make — a
  tougher anchor is not a flying scout, and a flying
  scout is not a tougher anchor, so the choice must
  name the choice, not just the name. What the step
  must NOT do is add two kinds and call the second
  one a bonus, or add a kind that is Vanguard or
  Recon with one number changed and no distinct role
  to play.
- The step must decide the kind's shape, and record
  it: members, HP per member, damage per living
  member, engagement range, and movement — each as a
  stated number, with the test that proves it. The
  scope's starting hypothesis is that the new kind
  keeps the shared one-voxel-per-tick movement rule
  and the shared step refusals (solid and
  enemy-occupied cells), differing in its numbers and
  in what it is for, not in how it walks — because a
  new movement rule in the same step as a new kind
  would make the kind untestable in isolation. If
  the step chooses a flyer-like role, it must
  confront this hypothesis directly: a unit that
  ignores solid cells, or moves in the air, is a
  movement decision with targeting and occupancy
  consequences, and the step must prove those
  consequences with tests, not assert that flight is
  a stat. The same discipline applies to range and
  damage: a kind that out-ranges or out-damages
  Vanguard in every case is not a choice, it is a
  replacement, and the step must say what it gives
  up for what it gains.
- The step must decide how stats are represented,
  and record it: the scope's starting hypothesis is
  a per-kind shape the squad reads from its kind, in
  `SquadShapes.For`, rather than a second set of
  shared constants or a branch in every rule that
  reads Vanguard's or Recon's numbers. Vanguard's
  numbers (3 members, 30 HP each, 10 damage each,
  range 3) and Recon's numbers (2 members, 20 HP
  each, 6 damage each, range 5) must NOT change in
  this step: a third kind that silently rebalances
  either existing kind would break every test and
  demo number that proves the game so far. The new
  kind is appended as an enum value only, so old
  saves still load — the save format already stores
  kind as an int and the loader already rejects
  undefined kinds, so the step must prove the new
  value loads and the old values still load, not
  invent a new save field.
- The step must decide price and cap, and record
  them: the scope's starting hypothesis is that the
  settled economy holds — 10 EmberOre and a cap of
  3 live squads, the same for the new kind as for
  Vanguard and Recon, so the picker's COST 10 /
  MAX 3 line stays true. If the step finds the new
  kind genuinely needs a different price, that is a
  separate economy decision the step must justify on
  its own, not smuggled in as a stat. No new
  doctrine rides along: the priced unit deploys
  under HoldGround and Advance exactly as the
  existing kinds do, and a third doctrine is Step 2
  work, not a Step 1 extra.
- The step must decide the adapter binding and the
  display, and record them: the picker today uses
  Alpha1 / Alpha2 for doctrines and Alpha3 / Alpha4
  for kinds, so a third kind needs a key the step
  names (Alpha5 is the next free number key, but
  the step must confirm it does not collide with
  any other binding while picking) and a
  playtest-gate checklist item, Windows-only. The
  headless display already prints kind, doctrine,
  members, health, damage, range, and activity, so
  no new display line is assumed — but the scene
  marker question from gap 8 must be answered in
  the step, as above. And because Step 1 touches
  the gate doc for its own checklist item, it also
  adds the missing B-key item (workstation build,
  like the T / Y / U / I / N items), closing gap 7
  as a named part of this step.

### Step 2 — Exactly one deferred doctrine

Add one doctrine, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must choose the doctrine, and record it:
  the scope's starting pool is the deferred pair from
  Slice 4 — Defend Core and Patrol — and the step
  chooses exactly one from it, or another original
  doctrine only if it records why neither of the two
  fits the gap it is closing. The scope's starting
  hypothesis is Defend Core first, because the core
  objective already exists (the gate doc places it
  at (28,5,28) with 100 HP) and no squad today
  orders itself to it: HoldGround holds a rally the
  player picks, Advance hunts the nearest enemy, and
  neither answers "guard this thing". Patrol's
  hypothesis is weaker on that question because a
  patrol needs a route definition the core does not
  yet have — a loop, a back-and-forth, waypoints —
  and inventing that route inside a doctrine step
  would make the doctrine untestable in isolation.
  A hypothesis is not a decision: if the step finds
  Defend Core collapses into HoldGround at the core
  cell, or that Patrol's route question is in fact
  already answered by the rally picker, it must say
  so and choose otherwise, with the evidence. The
  doctrine not chosen is deferred, not rejected.
- The step must decide the doctrine's behaviour, and
  record it: a doctrine is a real doctrine, not a
  flag — it needs its own movement and engagement
  behaviour in core, its own tests, and a picker
  selection for it, so Unity play can actually
  choose it once the adapter work for that is done.
  Defend Core, if chosen, must answer: what "core"
  means in core code (the objective core, not a
  rally the player happens to pick near it), where
  the squad stands to defend it, how far it chases
  an attacker before returning, and whether it fires
  only while holding its post (HoldGround's rule)
  or whenever in range (Advance's rule) — and must
  prove the return, not just the chase, because a
  guard that never comes back is Advance with a
  starting point. Patrol, if chosen, must answer:
  what the route is and where it is stored, what
  happens at each endpoint, whether the squad fires
  while moving, and what it does when no enemy is
  in range — and must prove the route repeats or
  reverses, not just that the squad moves once.
  HoldGround and Advance behaviour for zones and
  saves that do not use the new doctrine must be
  unchanged.
- The step must decide which kinds the doctrine
  serves, and record it: the doctrine is picked
  independently of kind, so the step must prove it
  against more than one kind — at minimum the new
  kind from Step 1 and one existing kind — or state
  plainly, with a reason, that it is kind-specific
  and which kinds it refuses. A doctrine proved
  only on Vanguard, in a slice that just added a
  third kind, would leave the Step 1 / Step 2
  pairing unproved. Pricing is settled, not
  reopened: the new doctrine deploys through the
  paid path — 10 EmberOre, cap 3 live squads — with
  no new price invented in this step. The doctrine
  is appended as an enum value only, so old saves
  still load, by the same save rule as Step 1.
- The step must decide the adapter binding, and
  record it: the picker today uses Alpha1 / Alpha2
  for doctrines and Alpha3 / Alpha4 for kinds, and
  Step 1 will have taken the next number key for
  its kind, so the step must name a binding that
  does not collide with any other binding while
  picking — a third doctrine key is a binding
  decision, not a free number — plus a
  playtest-gate checklist item, Windows-only. No
  second kind and no second doctrine ride along.

### Step 3 — Slice 11 demo scenario and review

As with 0.34.0, 0.38.0, 0.43.0, and 0.47.0: extend
the demo with one composed Slice 11 run (the third
kind and the new doctrine in the same Simulation —
not two separate proofs), then write
`docs/slice-11-review.md` as a documentation-only
review. The demo's job is composition: the new kind
under the new doctrine beside an existing kind under
an existing doctrine, so the reader can see the new
pairing is a choice among choices, not a replacement
for them. The review checks the slice against this
scope, states the verdict honestly (tested
third-kind core plus written adapters, not
playtested, balanced, or Unity-verified), separates
headless-tested core claims from Windows-checklist
adapter claims, carries the gaps that remain, and
does NOT scope Slice 12 — Slice 12 is scoped
afterwards, the same way this document follows the
Slice 10 review's candidates rather than preceding
them.

## Deliberately NOT in Slice 11

- A fourth squad type, or a second kind added
  alongside the third as a bonus. One new kind is
  Step 1; the next kind, if any, is a Slice 12-or-
  later decision with this slice's evidence in hand.
- A second doctrine, or a doctrine added as a
  kind's special behaviour. Doctrines are a separate
  widening with their own movement and engagement
  questions, decided in Slice 4 style, not smuggled
  in as a kind's extra.
- Further electrical widening beyond two consumers
  and two sources (a third consumer, a third source,
  or a display for live networks feeding no
  consumer). That is the ranked Slice 12 candidate
  below, not Slice 11 work: a squad slice that also
  rewires power would make the squad untestable in
  isolation.
- A balance pass. The new kind's numbers, the new
  doctrine's ranges, the unchanged price, and the
  unchanged cap are first-pass values chosen for
  determinism and testability, exactly as Slice 3's
  prices and Slice 7's Recon numbers were, and must
  be labelled as such, not quoted as final feel.
- A pre-destruction or pre-disablement warning, a
  mesh tint for any new block or marker, or a mesh
  refresh on any key build. All three remain named
  gaps from 0.30.0 / 0.42.0, not fixed by a squad
  step — a marker distinction for the third kind, if
  Step 1 decides one is needed, is a display
  decision with its own checklist item in that step,
  not a tint or refresh system smuggled in.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 11 markers remain scene markers
  in the FPS view, not a second camera.
- A new movement mode, a weapon rework, or a
  free-deploy enforcement change. The free
  `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- Windows Unity verification itself. It remains open
  for Slices 1–11 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 11 steps must add
  their own checklist items to those docs rather
  than claiming verification.

## Ranked candidates for Slice 12 scoping (not promised)

These are recorded here so the Slice 11 review starts
from a list, not from scratch. Order follows the
0.44.0 / 0.47.0 ranking with the Slice 11 candidate
removed:

1. Further electrical widening beyond two consumers
   and two sources (a third consumer, a third
   source, or a display for live networks feeding no
   consumer) — deferred, not rejected, and only
   after Slice 11 proves the squad widening.
2. Adapter visibility and fairness as one candidate:
   a pre-destruction / pre-disablement warning, a
   mesh tint and mesh refresh, and a scene-marker
   distinction for the squad kinds — gaps named in
   the Slice 9, Slice 10, and Slice 11 reviews, to be
   decided at Slice 12 scoping as one visibility
   question or split apart there, not promised here.

## Slice 11 gate (what "done" means)

Slice 11 is done when: every step above is pushed with
passing tests; the third kind's shape, price, cap,
placement path, binding, and display are each decided
and tested, with Vanguard's and Recon's numbers
unchanged and old saves loading; the deferred
doctrine's movement, engagement, return-or-route,
and binding are each decided and tested, proved
against more than one kind, with HoldGround and
Advance unchanged for zones and saves that do not
use it; the composed demo proves the kind and the
doctrine together through a save/load round-trip,
with any destroyed or dead squad staying dead and
kind and doctrine re-derived on load without a new
save field; the B-key checklist gap from Slice 10
is closed by Step 1's gate-doc touch; and
`docs/slice-11-review.md` records the verdict
honestly, separating headless-tested core claims
from Windows-checklist adapter claims, including
that Slice 11 is still not playtested, balanced, or
Unity-verified. The Slice 1 director playtest gate
is still pending throughout and is not closed by
Slice 11.
