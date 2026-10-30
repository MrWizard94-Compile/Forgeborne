# Forgeborne — Slice 15 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 15 system is
built by this document. Each step below is built one at a time,
with tests where a step has a headless-testable claim and with
Windows-checklist items where it does not, in this order, and
Slice 15 is closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 14 closed the electrical widening Slice 12 left open: a
game with exactly two power sources now has exactly three, the
third proved distinct by its delivery shape, and the composed
demo proves it beside the sources already trusted. What Slice 14
deliberately did not touch is the orbital side, and what the
orbital side still owes is the last deferred doctrine. Patrol
was deferred in Slice 4, when Advance was chosen from Advance /
Defend Core / Patrol, and deferred again in Slice 11, when
Defend Core was chosen from Defend Core / Patrol because the
core objective already exists and no squad ordered itself to
it, while Patrol still needed a route definition the core does
not yet have. That route question — loop, back-and-forth, or
waypoints, and where the route is stored — is still
unanswered, and no later slice has answered it by accident:
HoldGround holds a rally the player picks, Advance hunts the
nearest enemy, Defend Core guards the core, and none of them
walks a route. Slice 15 is therefore the deferred doctrine
slice, kept as small as the slices before it: **one fourth
doctrine first, then its own demo and review** — not a
display-query redesign, not a scene-proof observation, not a
fifth kind, not a second doctrine, not an electrical
widening.

## Inputs this scope is decided from

- Slice 14 review (`docs/slice-14-review.md`) ranked
  candidates, carried unchanged: Patrol first, any Slice 14
  remainder second, any Slice 13 remainder third. The Step 2
  composition evidence gives no reason to reorder them — a
  Conduit cable mill destroyed on tick 1, a Dynamo mill
  gathering 5 in 5 ticks, and a Conduit-fed pylon kill say
  nothing about whether a squad can walk a route — so the
  top candidate becomes Slice 15, and the second and third
  become the ranked Slice 16 candidates below, not a
  promise.
- The Slice 14 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 15 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. Electrical balance
  (gap 3) is not a slice: any route length, chase rule,
  or fire rule a step chooses is a first-pass value for
  determinism and testability, exactly as Defend Core's
  chase radius 8 was, and must be labelled as such, not
  quoted as final feel. The scene proof for the
  visibility adapters (gap 4: whether the chunk material
  multiplies vertex colours, whether
  `FpsInputAdapter.MeshRenderer` is wired to the same
  renderer, and whether markers read through occlusion)
  stays exactly where Slice 13 left it: three Windows
  observations inside checklist items 25, 26, and 27, not
  a headless step — a headless run cannot close an
  observation about a scene it cannot run, so gap 4 is
  carried as a Slice 16 candidate, not smuggled into
  Slice 15 as a pretend step. The single-cable tint
  finding (gap 5: a cable cell whose only neighbours are
  a consumer and a Conduit reads None in PowerState
  while its network is live) and the direct-Conduit
  count question (gap 6: a Conduit beside a mill counts
  as neither a live network nor a lone source) are
  display-query decisions, each with its own tests,
  because the first would change tint behaviour for
  every existing source's cables and the second would
  redefine a count the Slice 12 and Slice 14 demos were
  proved against — so both are carried as Slice 16
  candidates, not patched inside a doctrine step, and
  checklist item 28 already asks the scene observer to
  record the untinted cell honestly in the meantime.
  Patrol (gap 7) is Slice 15's work, taken here as one
  doctrine, not as a doctrine family. The free deploy
  primitive (gap 8) remains a headless-only rule,
  re-named, not quietly enforced here.
- Why Patrol first, and not the remainders first, stated
  as a decision rather than a ranking habit: the
  remainders are questions about how existing rules
  read, and deciding either of them inside a doctrine
  step would move the display the doctrine is proved
  against, making a failing test unable to say whether
  the doctrine or the display failed it. Patrol, by
  contrast, is a question about what a squad does, and
  it is the only doctrine question left — doctrines
  today are HoldGround, Advance, and Defend Core, and
  no fourth behaviour exists to confuse it with. A
  doctrine slice that also redesigned a count would
  make both untestable in isolation, so the order is
  doctrine first, display-query decisions after, with
  the remainders losing nothing by waiting one slice:
  their checklist lines already record them honestly.

## Slice 15, in order

### Step 1 — Exactly one fourth doctrine

Add one doctrine, decided in that step — not in this scope
document — with the reasoning recorded in DECISIONS.md:

- The step must choose the doctrine, and record it:
  the scope's starting pool is the single deferred
  doctrine, Patrol, and the step chooses Patrol, or
  another original doctrine only if it records why
  Patrol still cannot be defined after this scope has
  named its route question. A hypothesis is not a
  decision: if the step finds that a route cannot be
  defined without a second picker point, a new save
  field, or a movement rule of its own, it must say
  so and design that cost openly, with the evidence,
  rather than collapsing Patrol into HoldGround at a
  moving rally or Advance with a starting point. The
  doctrine not chosen, if any, is deferred, not
  rejected.
- The step must decide the route, and record it: a
  patrol without a route is a label, not a doctrine.
  The starting hypotheses, none of them a decision,
  are: a loop through a closed set of waypoints, a
  back-and-forth between two endpoints, or a waypoint
  list walked in order and then repeated. The step
  must answer, precisely: what the route is, where it
  is stored, and what happens at each endpoint — turn
  and continue, reverse direction, dwell a tick, or
  stop — and must prove the route repeats or
  reverses, not just that the squad moves once. One
  rally point is not a route: the picker today
  requires exactly one rally to deploy, and a squad
  already stores that rally, its spawn, its doctrine,
  and its kind, so the step must say whether the
  route derives from rally and spawn alone, from a
  second picker point, from a saved waypoint list, or
  from some other stated source — and, if the route
  needs a new save field, must decide the save
  question (saved, re-derived on load, or reset on
  load) and prove old saves still load either way,
  with the loader's existing rejection of undefined
  doctrines unchanged. An appended enum value needs
  no field; a waypoint list does, and must say so
  instead of leaving it unasked.
- The step must decide movement, and record it:
  movement is one voxel per tick through the shared
  step rule, with the same axis choice, tie-break,
  and solid / enemy-occupied refusals as HoldGround,
  Advance, and Defend Core — a patrol step that
  duplicates that rule, or steps through a solid
  block or an occupied cell to stay on route, is a
  movement change, not a doctrine. The step must
  also decide what a blocked route step does: hold
  that tick and retry, skip to the next waypoint, or
  reverse — and must prove the blocked case, because
  a route that only works on open ground is a demo
  route, not a doctrine. A patrol that cannot reach
  a waypoint must not march forever at a wall while
  claiming to patrol.
- The step must decide engagement, and record it:
  whether the squad fires only while holding a
  waypoint (HoldGround's rule), whenever an enemy is
  in its kind's range (Advance's and Defend Core's
  rule), or by a third stated rule — and must prove
  the firing-while-moving case either way, because
  that case is what separates a patrol from a march.
  It must also decide what the squad does with an
  enemy in range: chase it, shoot it in passing
  without chasing (Defend Core's answer for a
  non-attacker), or ignore it until the route brings
  it back — and must prove the return to route after
  any chase, not just the chase, because a patrol
  that chases once and never returns is Advance with
  a starting point. With no enemy in range, the
  squad keeps walking its route: standing still with
  no enemy would collapse Patrol into HoldGround at
  whatever waypoint it stopped on.
- The step must decide which kinds the doctrine
  serves, and record it: the doctrine is picked
  independently of kind, so the step must prove it
  against more than one kind — at minimum two kinds,
  following the Defend Core precedent, which proved
  all three — or state plainly, with a reason, that
  it is kind-specific and which kinds it refuses. A
  doctrine proved only on Vanguard would leave the
  kind / doctrine pairing unproved. Pricing is
  settled, not reopened: the new doctrine deploys
  through the paid path — 10 EmberOre, cap 3 live
  squads — with no new price invented in this step.
  The doctrine is appended as an enum value only
  (`TacticalDoctrine = 3`, so earlier values are
  unchanged and old saves still load), unless the
  route decision above justifies a new save field
  and proves old saves still load.
- The step must decide the display, and record it:
  the snapshot activity for a patrolling squad — the
  existing activities are Marching, Holding, Hunting,
  Idle, and Guarding, derived in core so the text
  display and the 3D markers read the same word —
  must either reuse one of those words with a stated
  meaning (a patrolling squad reading Marching, for
  example, must be distinguished from a HoldGround
  squad marching to its rally, or the reuse must be
  defended as the same truth) or gain its own word,
  with the movement / display agreement proved the
  way Defend Core's Guarding / Hunting / Marching
  agreement was. The marker distinction from Slice 13
  Step 2 is by kind, not by doctrine, so no new
  marker shape is assumed: if the step wants a
  doctrine marker, it must say so and prove it is
  not a kind marker in disguise.
- The step must decide the adapter binding, and
  record it: the picker today uses Alpha1 for
  HoldGround, Alpha2 for Advance, Alpha3 for
  Vanguard, Alpha4 for Recon, Alpha5 for Heavy, and
  Alpha6 for Defend Core, plus Return / Esc / O, and
  no builder key fires while picking — so the next
  doctrine key is confirmed free at step time (Alpha7
  is the starting hypothesis, not a decision) plus a
  playtest-gate checklist item and an
  orbital-command checklist item, both Windows-only,
  for that key. No new HUD doctrine count is assumed:
  the snapshot line already reads doctrine, kind,
  and activity, and the step must prove the new
  doctrine reads there, or say why a new line is
  needed.
- The step must state its verification honestly, and
  record it: a doctrine is core code and is
  headless-testable, so the tests must prove the
  route shape (repeat or reverse, endpoint
  behaviour, blocked step), the engagement rule
  (firing while moving, chase or pass-through, return
  to route), the idle rule (keeps walking with no
  enemy), the kinds served, the paid deploy path,
  save / load (route state as decided above), and
  agreement with the snapshot activity — and that
  HoldGround, Advance, and Defend Core behaviour for
  zones and saves that do not use the new doctrine
  is unchanged, proved by a contrast test the way
  Defend Core's one-tick contrast proved it. Every
  earlier demo save length must stand.

### Step 2 — Slice 15 demo scenario and review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0, 0.55.0,
0.60.0, and 0.63.0: extend the demo with one composed
Slice 15 run (the new doctrine walking its route beside
an existing doctrine doing its own job, with an enemy
encounter on the route so the demo proves engagement,
not just walking — not separate proofs), then write
`docs/slice-15-review.md` as a documentation-only
review. The demo's job is composition of the
headless-testable parts: the patrol must walk, engage,
and return to route exactly as Step 1 proved it,
through a save / load round-trip, beside the doctrines
the reader already trusts. The review checks the slice
against this scope, states the verdict honestly (tested
fourth doctrine plus written adapter, not playtested,
balanced, or Unity-verified), separates
headless-tested core claims from Windows-checklist
adapter claims (the picker key), carries the gaps that
remain, and does NOT scope Slice 16 — Slice 16 is
scoped afterwards, the same way this document follows
the Slice 14 review's candidates rather than preceding
them.

## Deliberately NOT in Slice 15

- A second doctrine, a fifth squad kind, or any other
  orbital widening added alongside Patrol as a bonus.
  One doctrine is Slice 15; a doctrine family would
  make the route proof untestable as a unit, because
  a failing test could not say which doctrine failed
  it.
- A display-query redesign of the single-cable tint or
  the direct-Conduit count. Either is a Slice 16
  candidate with its own tests, not a patch inside a
  doctrine step: a doctrine step must not move the
  displays it is proved against, and a count or tint
  step must not move the doctrine it displays.
- A scene-proof observation (vertex-colour material,
  MeshRenderer wiring, marker occlusion) presented as
  a headless step. Those are Windows observations
  inside checklist items 25, 26, and 27, carried as
  Slice 16 candidates, not closed by a document.
- An electrical widening of any kind: no fourth
  source, no fourth consumer, no ladder redesign, no
  delivery, transformer, or cable rule change. The
  ladder stays LV / HV / None, and no existing
  block's rating, cost, or phase rule moves because
  a doctrine arrives.
- A movement rule change. The shared one-voxel-per-
  tick step rule, its axis choice and tie-break, and
  its solid / enemy-occupied refusals are reused, not
  rewritten, and a patrol route does not gain a
  speed, a fly mode, or a pathfinder of its own.
- A balance pass. The route length, any chase radius,
  and the fire rule are first-pass values chosen for
  determinism and testability, exactly as Defend
  Core's chase radius 8 was, and must be labelled as
  such, not quoted as final feel.
- A new movement mode, a weapon rework, a camera, a
  mini-map, or a free-deploy enforcement change. The
  free `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- A new save field, unless Step 1's route decision
  justifies one and proves old saves still load. A
  doctrine whose route derives from rally and spawn
  needs no field, the way Advance and Defend Core
  need none.
- Windows Unity verification itself. It remains open
  for Slices 1–15 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 15 steps must add their
  own checklist items to those docs rather than
  claiming verification.

## Ranked candidates for Slice 16 scoping (not promised)

These are recorded here so the Slice 15 review starts
from a list, not from scratch. Order follows the
0.63.0 ranking with the Slice 15 candidate removed,
and the Slice 14 remainders now first because Patrol
is taken:

1. Any Slice 14 remainder the Slice 14 review names
   (the single-cable tint query question in its gap 5,
   and the direct-Conduit count question in its gap 6)
   — each a display-query decision step with its own
   tests, not a patch inside another step, and not
   promised here.
2. Any Slice 13 remainder the Slice 13 review names
   (the vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in its gap 4) — three
   Windows observations inside checklist items 25, 26,
   and 27, recorded here as observations to be made
   when Windows verification runs, not as a headless
   slice step, and not promised here.

## Slice 15 gate (what "done" means)

Slice 15 is done when: every step above is pushed
with passing tests (or, for the adapter-only part of
Step 1, with the test suite unchanged for that part
and the adapter-only verification stated, never
inflated into a test claim); the Step 1 doctrine's
route, route storage and save answer, endpoint
behaviour, movement reuse and blocked-step answer,
engagement and return-to-route rules, kinds served,
price and cap, display activity, and adapter line
are each decided and tested, with no existing
doctrine's movement, engagement, or snapshot behaviour
changed, no existing kind re-priced or re-ranged, the
ladder still LV / HV / None, and every earlier demo
save length unchanged; the composed demo proves the
new doctrine walking its route and engaging on it
beside an existing doctrine through a save / load
round-trip; and `docs/slice-15-review.md` records the
verdict honestly, separating headless-tested core
claims from Windows-checklist adapter claims,
including that Slice 15 is still not playtested,
balanced, or Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is not
closed by Slice 15.
