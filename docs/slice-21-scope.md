# Forgeborne — Slice 21 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 20 review's ranked candidates.
No Slice 21 system is built by this document.
Each step below is built one at a time, with
tests, in this order, and Slice 21 is closed by
its own review — it is not closed by finishing
step 1 alone.**

## Scope question

Slice 20 decided one player action for one
state: a stuck Patrol gets a FREE re-rally,
stuck Patrol only, by squad list index, with
its rally changed, its waypoint index and
refusal counter reset to 0, its position,
health, doctrine, and kind untouched, no ore
charged, no refund, no cap slot freed, and
no teleport. Every other live squad refuses
that action with nothing changed: a moving
Patrol, a HoldGround, an Advance, and a
Defend Core squad all refuse it, proved in
Step 1 and staged in Step 2. What Slice 20
deliberately did not touch is the second
half of the first-ranked remainder the Slice
20 review carries, from the Slice 20 scope's
ranked candidates — since Slice 20 scopes
its action to stuck Patrols only, a general
re-rally for other doctrines, and for
non-stuck squads, as its own decision.

Slice 21 is therefore the general re-rally
slice, kept as small as the slices before
it: **one action generalised, decided as its
own step with its own tests, then its own
review** — not a recall, not a general
squad-command system, not a radius retune,
not a movement, targeting, or fire redesign,
not a HUD redesign, and not a claim of Unity
verification made from a headless run.

## Inputs this scope is decided from

- The Slice 20 review's ranked candidates, in
  order, and what this scope does with each:
  the patrol route's remaining questions
  (candidate 1) are Slice 21's work, in the
  narrowed form below: the general re-rally
  first, with the radius and waypoint values
  carried, not re-decided, for the reason
  given under "Radius and waypoints" below,
  and with recall carried after Slice 21, for
  the reason given under "Why re-rally
  before recall" below. The Slice 13
  remainder observations (candidate 2: the
  vertex-colour material observation, the
  MeshRenderer wiring observation, and the
  marker-occlusion observation, plus the
  dusk and view model observations in items
  33 and 34, plus the tint, count, patrol,
  and re-rally observations in items 35, 36,
  37, and 39) remain observations to be made
  when Windows verification runs, not steps
  to be re-scoped blindly — if an
  observation fails, that failure is a fix
  step with its own decision, not a reason
  to re-scope the proof layer. First-pass
  graphics values (candidate 3: dusk
  azimuth, fog contrast, view model offset,
  the static weapon-agnostic model) are
  retuned only on Windows evidence, not
  re-decided headlessly here: no Slice 21
  step moves a light, fog, or model value to
  make a re-rallied squad read better.
- The Slice 20 review's other gaps, in
  priority order, and what this scope does
  with each: Windows Unity verification
  (gap 1) and the director playtest (gap 2)
  remain open, but neither is Slice 21 work
  in itself — the first is blocked on Unity
  6000 on Windows, the second on the
  director — and scoping a slice does not
  close, reorder away, or pretend to answer
  either. First-pass graphics values (gap 3)
  are candidate 3 above. The Slice 13
  scene-proof observations (gap 4) are
  candidate 2 above. The patrol route's
  remaining questions (gap 5) are Slice 21's
  work in the narrowed form below. The
  closed stuck-patrol remainder is not
  reopened: Slice 21 must not move the Slice
  20 decision for a stuck Patrol except
  through the explicit interaction question
  below — whether a general price, if Step 1
  decides one, applies to the stuck rescue
  too, or whether the stuck rescue stays
  free as a special case — and a step that
  reprices the rescue silently, by merging
  the two actions without saying so, has
  confused generalisation with revision.
- Why this question is a generalisation
  question and not a new-action question,
  stated before the step decides anything:
  the mechanism already exists and is
  proved — `TacticalSquad.ReRally` changes
  the rally field, resets patrol progress,
  and leaves everything else untouched, and
  the save array already carries the rally,
  so a re-rallied squad loads with its new
  rally. What does not exist is a decision
  about who else may use that mechanism,
  at what price, and with what observable
  effect per doctrine. The step must keep
  that boundary visible: if it finds its
  honest answer requires a movement move
  (for example, making Advance or Defend
  Core consult a rally they currently
  ignore, or a pathfinder, or a hold
  instead of a march), it must say so and
  defer that move, not smuggle it in as a
  generalisation fix.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands: `WorldZone.TryReRallyStuckPatrol`
  gates on live, stuck, Patrol, in-bounds,
  and different-from-old-rally, in that
  order, and refuses everything else
  fail-closed with nothing changed; its
  gate is the only live-squad rally change
  in core, and `Simulation` carries only a
  thin passthrough for it, with no logic of
  its own. `OrbitalCommand.SetRally`
  updates only a pending deploy rally while
  picking, and fails closed outside picking;
  the picker (O to open, Return to confirm
  through the paid path, Escape to cancel,
  Alpha1–Alpha7 for doctrine and kind while
  picking) deploys squads and nothing else.
  The economy is fixed around the question:
  10 EmberOre per squad, cap 3 concurrent
  live squads, deploy refused at the cap,
  no refund on death or otherwise, so
  deployment is a sunk cost, and a re-rally
  in Slice 20 charges nothing and frees no
  slot. The next free playtest-gate
  checklist number is 40 (items run to 39,
  the stuck-patrol re-rally observation).

## The rally is not the same thing for every doctrine

This is the fact that makes Slice 21 a
decision instead of a gate removal, and
Step 1 must answer it doctrine by
doctrine, not once for all squads:

- HoldGround: the rally IS the destination.
  `MoveTowardRally` walks one voxel per
  tick toward it, and the squad reads
  Marching until it arrives and Holding
  once there, firing only once holding.
  A re-rally for a HoldGround squad is
  therefore a new destination the squad
  will walk to, with a fire consequence the
  existing rule already decides: a Holding
  squad re-rallied elsewhere marches again
  and does not fire while marching, exactly
  as a marching squad does not fire. Step 1
  must state that consequence, not change
  the fire rule to avoid it.
- Patrol: the rally is the loop's centre.
  `PatrolTarget` re-derives the rally and
  then east, south, west, and north at
  radius 3 from it, on every call, and the
  waypoint index is live route progress
  that resets to 0 on load. A re-rally for
  a non-stuck Patrol therefore moves a loop
  mid-circuit, and Step 1 must decide
  whether the index and the refusal counter
  reset to 0 as in the stuck case — the
  clean reading, since the loop is new —
  or whether either carries over, and what
  activity the snapshot reads immediately
  after. A counter that is already above 0
  but below 5 is the case that separates
  the two answers, and Step 1 must prove
  that case, not only the counter-at-0
  case.
- Advance: the rally is NOT consulted by
  movement. An Advance squad walks toward
  the nearest living enemy, re-derived
  every call, and with no living enemy it
  does not move at all — it does not fall
  back to its rally, because a rally
  fallback would make Advance a HoldGround
  squad whenever the battlefield is empty.
  A re-rally for an Advance squad therefore
  changes a stored field with, on the code
  as it stands, no movement effect at all.
  Step 1 must say whether that is a valid
  action (a field change whose marker, if
  any, moves but whose squad does not), a
  refusal (the action is refused
  fail-closed for a doctrine whose rally
  does nothing), or a scope limit (the
  general action covers only the doctrines
  whose rally moves them) — and it must
  prove the Advance case it claims,
  including an Advance squad with a living
  enemy, whose chase must not change
  because its rally changed.
- Defend Core: the rally is NOT consulted
  by movement either. A Defend Core squad
  steps toward the nearest core attacker
  within radius 8 of the core, or toward
  the core itself when there is no
  attacker, and its rally is a deployment
  formality the picker still requires, not
  a place the squad ever goes. The same
  three answers are open as for Advance,
  and Step 1 must prove the Defend Core
  case it claims, including a guard with a
  core attacker in radius, whose chase and
  return must not change because its rally
  changed.

A scope that said "all doctrines, same
effect" without this section would be
wrong on two of the four doctrines. Step 1
may still decide all four, or two, or any
other scope it justifies — but it must
justify the scope per doctrine, with the
inert-rally doctrines answered explicitly,
not by omission.

## Why re-rally before recall

Reversing the order would not break either
decision, stated honestly: recall-first is
a coherent slice. It is chosen against here
because the two remainders are not the same
size. A general re-rally generalises a
proved mechanism (the Slice 20 gate, the
internal `ReRally`, the saved rally field,
the walk-to-new-rally proof) to new
scopes, with the open questions being
scope, price, and per-doctrine effect.
A recall has no mechanism at all: no
removal API for a live squad, no
confirmation design, and an open refund
question (none, full 10, or partial) whose
answer interacts with the cap (a refunded
recall that also frees a slot is two gifts,
not one, as Slice 20 recorded). Recall is
therefore the larger slice, and it is
carried after Slice 21, not patched in
beside a generalisation. Step 1 must not
add a recall or dismiss API quietly
afterwards, under any name.

## Radius and waypoints, carried not re-decided

`PatrolLoopRadius` is 3 and
`PatrolWaypointCount` is 5, a flat loop at
rally height: the rally, then 3 east, south,
west, and north of it, in that order. Both
are first-pass values from Slice 15 Step 1,
not balance claims. Slice 21 does NOT
retune either value headlessly, and the
reason is now stronger than in Slice 20:
Slice 20 Step 1 looked for a stuck case
whose cause was the radius itself and found
none — its stuck cases were blocked cells,
not a loop that could not fit where it was
placed — so there is still no evidence
behind a headless retune. A general
re-rally moves a loop's centre, never its
radius or its waypoint count, for every
doctrine it covers, and a Step 1 finding
that the radius caused a block is recorded
as a gap for a later radius decision, not
acted on by widening. A Windows
observation that a loop does not fit its
ground is the other evidence route, and it
is likewise recorded, not acted on
headlessly here.

## Slice 21, in order

### Step 1 — General re-rally decision

Decide what re-rally, if any, the player
has for live squads beyond a stuck Patrol,
decided in that step — not in this scope
document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the scope, and
  record it per doctrine: does the action
  apply to any live squad of any doctrine,
  to HoldGround and Patrol only, to any
  live Patrol (stuck or not) plus
  HoldGround, or to some other scope it
  justifies? A scope that covers Advance
  or Defend Core must answer the
  inert-rally question above for each; a
  scope that excludes them must say why
  exclusion, rather than a field-only
  change, is the honest answer for a
  doctrine whose rally does nothing. For
  a non-stuck Patrol, the step must say
  whether "general" includes a patrol
  mid-loop with a counter above 0, and what
  happens to that counter. If it scopes
  wider than stuck Patrols, it must prove
  the wider scope's cases too, not only
  the stuck one it inherits.
- The step must decide the interaction
  with the Slice 20 decision, and record
  it: a stuck Patrol is inside any scope
  that includes Patrol, so the step must
  say whether a stuck Patrol's re-rally
  stays free (the Slice 20 answer, kept as
  a special case inside the general action)
  or takes the general price, if any, with
  the reason why. A general price applied
  silently to the stuck rescue would revise
  Slice 20 without saying so, and a free
  general action justified only by "the
  rescue was free" would borrow a rescue
  rationale for a repositioning: a healthy
  squad's re-rally is a redeployment-like
  move, not a rescue from a trap, and the
  step must weigh the price on that basis,
  not by copying Slice 20's answer. The
  Slice 20 proof set (route-blocked march,
  enclosed re-stick, refusals, paid path)
  must still pass unchanged for the stuck
  case, whatever the general answer is,
  or the step must say explicitly what it
  changed and why.
- The step must decide the targeting and
  selection rule, and record it: whether
  the core API targets a squad by its
  order in the squad list (the Slice 20
  rule, snapshot order), by position, by
  rally, or by some other identity it
  defines, and what happens on a miss — no
  squad there, a dead squad, two squads
  sharing a cell or a rally — fail-closed
  with nothing changed, or a defined
  fallback, and why. If it reuses the
  Slice 20 index rule, it must say so and
  prove that the general gate and the
  stuck gate cannot disagree about which
  squad an index names. If it defines a
  new rule, it must say what happens to
  the stuck path: merged into one API, or
  kept separate with the stuck gate
  standing beside it.
- The step must decide the state changes,
  and record them per doctrine: for a
  re-rally, the new rally, the waypoint
  index after the change (Patrol only),
  and the refusal counter after the change
  (Patrol only, including the counter-
  above-0 case). HoldGround, Advance, and
  Defend Core have no index or counter, so
  the step must say what, if anything,
  changes for them besides the rally field
  itself — and, for Advance and Defend
  Core, whether a rally-field-only change
  is claimed as an effect at all. Position,
  health, doctrine, and kind are untouched
  unless the step justifies a change and
  proves it; a re-rally is not a teleport,
  not a heal, and not a redeploy, and the
  step must say so if it finds itself
  tempted otherwise. The snapshot word
  immediately after the action must be
  stated per doctrine from
  `OrbitalView.Snapshot` code, not assumed:
  a HoldGround squad away from its new
  rally, a HoldGround squad re-rallied to
  the cell it already stands on, a Patrol
  on its new loop, an Advance squad with
  and without a living enemy, and a Defend
  Core squad with and without a core
  attacker are different readings, and the
  step must name each it claims.
- The step must decide the economy
  relation, and record it: does a general
  re-rally cost any EmberOre, and if so,
  how much, justified, not assumed? The
  sunk-cost rule — no refund on death or
  otherwise — is the starting position, not
  a decision: a general action is not a
  refund, and a price, if any, must say
  what happens when the player cannot pay:
  fail-closed with the old rally kept, or
  allowed with a debt, and why. A priced
  re-rally must also say whether it frees
  or takes a cap slot: it does neither on
  the Slice 20 reading (the squad never
  left the field), and any departure from
  that is an economy change to justify and
  prove. The price (10) and the cap (3)
  otherwise stand: no Slice 21 step raises
  the cap to make room for a re-rallied
  squad, and no step reprices a deploy to
  pay for a re-rally.
- The step must decide the proof set, and
  record it: a HoldGround squad marching
  to a new rally and then Holding there,
  with its fire suspended while marching
  under the existing rule and resuming
  once holding, proved, not asserted; a
  Holding squad re-rallied elsewhere
  marching again; a non-stuck Patrol
  re-rallied mid-loop, marching to its new
  rally and turning onto its new loop with
  radius 3 and five waypoints unchanged;
  a Patrol with a counter above 0
  re-rallied, with its counter reading as
  decided; for each inert-rally doctrine
  the step covers, a squad whose movement,
  chase, and fire do not change when its
  rally changes, with and without an enemy
  (Advance) or a core attacker (Defend
  Core); for each doctrine the step
  refuses, a refusal with nothing changed;
  all three kinds proved, or a stated
  reason why kind does not matter here
  (kinds differ in health, damage, and
  range, not in route progress, and the
  step must say whether that is enough to
  prove one kind or all three, including
  where range changes a fire proof); a
  save/load round-trip (a generally
  re-rallied squad loads with its new
  rally and marches from index 0 for
  Patrol, a stuck-before-save patrol still
  loads Patrolling at index 0 under the
  existing rule, which the step must not
  break); and the paid deploy path
  unchanged (price 10, cap 3, picker
  Confirm still the only play path, a
  fourth deploy still refused at a full cap
  after a re-rally, a re-rallied squad not
  charged a second deploy price unless the
  step decides a re-rally price and proves
  it). In every case: refusals for a dead
  squad, out-of-range and negative indices
  (if index targeting is kept), an
  out-of-bounds rally, and the same rally
  again, each with nothing changed; and a
  solid rally cell accepted with the squad
  holding short of it, if Step 1 carries
  the Slice 20 solid-rally answer, or a
  stated reason why it does not.
- The step must decide the movement
  boundary, and record it: no movement rule
  moves in this step, under any hypothesis.
  `MoveTowardRally`, `MoveForPatrol`'s
  sidestep, skip, and Stuck threshold,
  Advance's nearest-enemy chase, and
  Defend Core's chase-or-return stand; no
  pathfinder is added; no sidestep widens;
  no hold replaces a skip; no rally
  fallback is added for Advance, and no
  rally consultation is added for Defend
  Core, unless the step defers itself and
  says so. A re-rally that moved a squad
  by teleporting it to the new rally would
  not be a re-rally at all. A HoldGround
  squad re-rallied to a rally it cannot
  reach must hold short under the existing
  step rule, proved, not assumed, and a
  Patrol re-rallied to a loop it cannot
  walk must read Stuck again after a full
  circuit on the new loop, as Slice 20
  proved for the stuck case.
- The step must decide the checklist item,
  and record it: a new checklist item (from
  40) observing the decided general action
  on Windows, and it must say whether that
  item supersedes, extends, or leaves
  standing item 39's stuck-patrol
  observation, item 37's patrol-route
  observation, and item 29's and
  orbital-command item 13's patrol
  observations. If the general action
  reuses the proposed K binding, the step
  must say whether item 39's observation
  already covers it or a new item is still
  needed for the non-stuck and
  other-doctrine cases; if it proposes a
  different binding, the new item observes
  that binding. The step must state its
  verification honestly: like Slice 20's
  step, this step has a headless-testable
  claim — squad states before and after an
  action, for staged layouts — so it adds
  tests for every case in the proof set
  above, and the test suite must rise, not
  stand, with no failures. No demo scenario
  is required of this step alone; the
  composed demo belongs to Step 2, which
  stages the decided action together with
  its controls.
- The step must decide the display
  relation, and record it: no HUD line is
  added, moved, or reworded in this step,
  and no new snapshot word is added unless
  the step justifies one and proves it.
  The existing squad markers render the
  snapshot; a re-rallied squad's marker
  follows its new rally under the existing
  marker rule, which the step must verify
  per doctrine rather than assume — in
  particular, it must say whether Advance
  and Defend Core squads render a rally
  marker at all, since their rally moves
  nothing, and what a player is supposed to
  read from a marker that moves while its
  squad does not. A general action that
  needed a new HUD line to be understood
  would be a HUD decision smuggled inside
  an action step, and the step must say so
  rather than add one.
- The step must decide the adapter binding
  proposal, and record it as a Windows
  checklist item, not as a headless proof:
  the proposed K binding for the stuck
  re-rally (K while not picking, crosshair
  on the stuck marker, picker raycast rally
  reuse, Return to confirm, Escape to
  cancel) is a comment in
  `FpsInputAdapter.cs` only, not
  implemented, and the taken keys are
  WASD / Space / E / Q / R / O / Return /
  Esc / Alpha1-7 / T / Y / U / I / B / N /
  P / M / mouse click. A general binding
  must be checked against those keys and
  against the K proposal: reuse K with a
  wider eligibility rule, or a different
  key, with the reason why, and with the
  fail-closed cases stated (doing nothing
  while picking, nothing when the raycast
  hits no eligible marker, nothing when
  the target squad refuses the action).
  No binding is claimed as working in a
  scene from a core test.

### Step 2 — Slice 21 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, 0.66.0, 0.76.0,
0.80.0, 0.83.0, and 0.86.0 — and unlike
Slice 16's review, which was
documentation-only because no graphics
claim is headless-provable — this review
IS accompanied by a new composed demo
scenario, because the Step 1 action is
headless-testable and a demo that stages
it together with its controls proves
something a step's isolated tests do not:
that the decided action, the untouched
movement rules, and the untouched economy
agree in one field. The demo scenario,
decided in that step, must stage in one
Simulation, at minimum: a HoldGround
squad re-rallied to a new destination and
marching there; a non-stuck Patrol
re-rallied mid-loop, marching to its new
rally and turning onto its new loop; for
each inert-rally doctrine Step 1 covers, a
squad whose rally changes while its
movement, chase, and fire do not; for each
doctrine Step 1 refuses, a refusal with
nothing changed; the stuck Patrol case,
proved as Step 1 decides its interaction
(free special case, or general price, with
the difference visible in ore); and a
save/load round-trip proving the action's
effect persists as decided (a new rally
loads, a stuck-before-save patrol still
loads Patrolling at index 0) — with the
paid deploy path exercised beside it, so a
reader can see the cap and price did not
move to accommodate a repositioning.
Composed-integration tests mirroring the
demo must be added, and the test suite
must rise with no failures. The review
writes `docs/slice-21-review.md`, checks
the slice against this scope step by step,
states the verdict honestly (one general
re-rally action decided with tests, not
playtested, not Unity-verified), separates
headless-tested action claims from the
Windows-checklist action observation,
carries the gaps that remain, and does NOT
scope Slice 22 — Slice 22 is scoped
afterwards, the same way this document
follows the Slice 20 review rather than
preceding it. Every earlier demo save
length must stand; the new scenario's save
length is quoted as the new entry, not as
a change to an old one.

## Deliberately NOT in Slice 21

- A recall or dismiss API of any kind, for
  any doctrine. Step 1 rejected no recall
  here — it carries recall, with its refund
  and confirmation questions, as the first
  Slice 22 candidate — but no step in this
  slice adds one quietly afterwards, under
  any name, and a general re-rally must not
  be presented as a recall substitute: a
  re-rallied squad is still on the field,
  still counts toward the cap, and still
  costs nothing to keep.
- A general squad-command system.
  Selecting, ordering, or commanding squads
  beyond the one decided re-rally action —
  including a general recall, a dismiss, a
  follow order, or a formation command — is
  a larger slice, carried after Slice 21,
  not patched in beside a generalisation.
- A radius, waypoint-count, or loop-shape
  change. The loop stays 3 and 5, flat at
  rally height, unless a later decision on
  recorded evidence changes it — see
  "Radius and waypoints" above.
- A movement, targeting, or fire rule
  change. `MoveTowardRally`,
  `MoveForPatrol`'s sidestep, skip, and
  Stuck threshold, Advance's chase, and
  Defend Core's chase-or-return stand; a
  re-rallied squad still fires under its
  doctrine's existing rule, and no
  pathfinder, no hold, no wider sidestep,
  and no rally fallback for Advance or
  rally consultation for Defend Core is
  added here.
- A delivery, precedence, conflict, rating,
  cost, or phase rule change beyond the
  economy question Step 1 must answer
  explicitly (a general re-rally price, or
  free, with justification, and its
  interaction with the stuck rescue's free
  price). The deploy price (10) and the cap
  (3) otherwise stand, and no power,
  consumer, or save-format rule moves: a
  re-rally reuses the saved rally field,
  and no new save field is added unless
  Step 1 justifies one and proves old saves
  still load.
- A new block, kind, doctrine, source, or
  consumer, a balance pass, or a price or
  cap change disguised as a repositioning.
  Slice 21 is one generalised action and
  its review, and the core stands exactly
  as Slice 20 left it except for the action
  itself.
- A material, shader, marker, light, fog,
  or view model change, or a HUD redesign.
  The Slice 16 surface renders whatever
  Slice 21 decides; a step that re-palettes
  a marker or adds a HUD line to make its
  answer look right has confused the
  surface with the action.
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–39,
  and the new item Step 1 adds, are Windows
  observations, recorded in DECISIONS.md
  when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset,
  a texture, or a shader file written by a
  headless run and claimed as a built scene.
  Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout,
  or trademarked look from any other game.
- Windows Unity verification itself. It
  remains open for Slices 1–21 alike and is
  blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly.
- A Slice 22 scope. Slice 22 is scoped after
  the Slice 21 review, not inside it.

## Ranked candidates for Slice 22 scoping (not promised)

These are recorded here so the Slice 21
review starts from a list, not from scratch.
Order follows the Slice 20 review's gaps
with the general re-rally taken:

1. The patrol route's remaining questions —
   a recall or dismiss as its own decision,
   with its refund (none, full 10, or
   partial) and confirmation questions
   answered, and its cap effect proved, or
   a radius or waypoint first-pass value
   revisited on evidence (a Step 1 finding
   that the radius caused a block, or a
   Windows observation that a loop does not
   fit its ground) — each with its own
   tests, not promised here.
2. Any Slice 13 remainder that Slice 21 does
   not itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in the Slice
   20 review's gap 4, plus the dusk and view
   model observations in items 33 and 34,
   plus the tint, count, patrol, and
   re-rally observations in items 35, 36,
   37, and 39, plus the new item Step 1
   adds) — if those observations are still
   unmade when the review is written, they
   remain observations to be made when
   Windows verification runs, not steps to
   be re-scoped blindly, and not promised
   here.
3. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning step
   is justified by an observation, not by a
   headless re-decision, and is not promised
   here.

## Slice 21 gate (what "done" means)

- Every step pushed with passing tests (for
  Step 1, with the suite risen by the proof-
  set tests, never standing still on an
  action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1
  action decision recorded with rationale in
  DECISIONS.md, with its own Windows
  checklist item (item 40, or the number
  Step 1 decides if it finds 40 already
  taken, with the relation to items 39, 37,
  29, and orbital-command item 13 stated).
- No movement, targeting, fire, delivery,
  precedence, conflict, rating, phase, or
  save-format rule changed in any step
  except the action itself and the economy
  answer Step 1 justifies explicitly (a
  general re-rally price, or free, and its
  interaction with the stuck rescue's free
  price); no material, marker, light, fog,
  model, or HUD rule changed in any step
  except the snapshot word immediately after
  the action, if Step 1 justifies a change
  there.
- Every earlier demo save length unchanged,
  with the new Slice 21 length quoted as its
  own entry; a re-rally that reuses the saved
  rally field changes no save length by
  itself, and the review must say which
  per-doctrine effects, if any, change a
  save, rather than quoting the suite as
  unchanged.
- This review records the verdict honestly,
  separating headless-tested action claims
  from the Windows-checklist action
  observation, including that Slice 21 is
  still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending throughout
  and is not closed by Slice 21.
