# Forgeborne — Slice 20 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 19 review's ranked candidates.
No Slice 20 system is built by this document.
Each step below is built one at a time, with
tests, in this order, and Slice 20 is closed by
its own review — it is not closed by finishing
step 1 alone.**

## Scope question

Slice 19 closed the last Slice 17 display-query
remainder: a Conduit beside a dead standalone
transformer counts as lone, folded into the
existing POWER line, with live, lone, and
dead-cable controls proved together in one
demo. What Slice 19 deliberately did not touch
is the first-ranked remainder the Slice 19
review carries, from the Slice 18 review's
gap 6 — the patrol route's open questions:

- A patrol whose loop is permanently blocked
  still cycles its waypoint index forever,
  reading Stuck, with no recall, no re-rally,
  and no player action defined for it. Stuck
  is a display word only: the squad still
  fires in passing, still retries every
  waypoint on every circuit, and still reads
  Stuck after a full circuit of refusals.
  Nothing the player can do, short of waiting
  for the block to clear or for the squad to
  die, changes that state.
- The radius (3) and the five-waypoint loop
  are still first-pass values, not balance
  claims, and no step has yet said whether
  the radius is ever the cause of a block.

Slice 20 is therefore the stuck-patrol slice,
kept as small as the slices before it: **one
player-action decision for a stuck patrol,
decided as its own step with its own tests,
then its own review** — not a radius retune,
not a general squad-command system, not a
refund or economy redesign, not a HUD
redesign, and not a claim of Unity
verification made from a headless run.

## Inputs this scope is decided from

- The Slice 19 review's ranked candidates, in
  order, and what this scope does with each:
  the patrol route's open questions
  (candidate 1) are Slice 20's work, in the
  narrowed form below: the stuck-patrol
  action first, with radius and waypoint
  values carried, not re-decided, for the
  reason given under "Radius and waypoints"
  below. The Slice 13 remainder observations
  (candidate 2: items 25–27 and 30–38, still
  unmade as observations) remain observations
  to be made when Windows verification runs,
  not steps to be re-scoped blindly — if an
  observation fails, that failure is a fix
  step with its own decision, not a reason
  to re-scope the proof layer. First-pass
  graphics values (candidate 3: dusk
  azimuth, fog contrast, view model offset,
  the static weapon-agnostic model) are
  retuned only on Windows evidence, not
  re-decided headlessly here: no Slice 20
  step moves a light, fog, or model value to
  make a stuck patrol read better.
- The Slice 19 review's other gaps, in
  priority order, and what this scope does
  with each: Windows Unity verification
  (gap 1) and the director playtest (gap 2)
  remain open, but neither is Slice 20 work
  in itself — the first is blocked on Unity
  6000 on Windows, the second on the
  director — and scoping a slice does not
  close, reorder away, or pretend to answer
  either. First-pass graphics values (gap 3)
  are candidate 3 above. The Slice 13
  scene-proof observations (gap 4) are
  candidate 2 above. The patrol route's
  stated limits (gap 5) are Slice 20's work
  in the narrowed form below. The closed
  transformer-adjacency remainder is not
  reopened: Slice 20 must not move the
  Slice 19 count rule, and a stuck-patrol
  step that re-decides a count has confused
  its slice with the last one.
- Why this question is a player-action
  question and not a movement question,
  stated before the step decides anything:
  movement is what MoveForPatrol already
  decides — sidestep on a refused step, then
  skip, then Stuck after a full circuit of
  refusals — and no Slice 20 step is allowed
  to change that movement rule as a side
  effect of adding an action. A player
  action is what the player can do about a
  squad that movement has already declared
  stuck. The step must keep that boundary
  visible: if it finds its honest answer
  requires a movement move (for example, a
  pathfinder, a wider sidestep, or a hold
  instead of a skip), it must say so and
  defer that move, not smuggle it in as an
  action fix.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands: `TacticalSquad.IsPatrolStuck` is
  true only for a Patrol squad whose
  `PatrolConsecutiveRefusals` has reached
  `PatrolWaypointCount` (5); the counter
  resets to 0 on any successful Patrol step,
  direct or sidestep, and starts at 0 on
  load, so a loaded patrol is never stuck
  on arrival, whatever it was before the
  save. `PatrolWaypointIndex` is live route
  progress, not a saved field: it starts at
  0 on deploy and resets to 0 on load, and
  `PatrolTarget` is re-derived from the
  rally and the index on every call, so a
  changed rally re-derives the whole loop
  without any new save field — the save
  array already carries doctrine, kind,
  position, and rally. `OrbitalView.Snapshot`
  reads Stuck from the same live counter
  movement uses, and a stuck patrol still
  fires in passing exactly as a moving
  patrol does. There is no recall, dismiss,
  or re-rally API for a live squad anywhere
  in core: `OrbitalCommand.SetRally` updates
  only a pending deploy rally while picking,
  and fails closed outside picking; the
  picker (O to open, Return to confirm
  through the paid path, Escape to cancel,
  Alpha1–Alpha7 for doctrine and kind while
  picking) deploys squads and nothing else.
  The economy is fixed around the action
  question: 10 EmberOre per squad, cap 3
  concurrent live squads, deploy refused at
  the cap with no oldest-squad recall —
  because recall would silently destroy a
  paid investment and its position — and no
  refund exists, on death or otherwise, so
  deployment is a sunk cost. Dead squads do
  not count toward the cap, so a replacement
  can deploy after a squad dies. The next
  free playtest-gate checklist number is 39
  (items run to 38, the dead-transformer
  count observation).

## Radius and waypoints, carried not re-decided

`PatrolLoopRadius` is 3 and
`PatrolWaypointCount` is 5, a flat loop at
rally height: the rally, then 3 east, south,
west, and north of it, in that order. Both
are first-pass values from Slice 15 Step 1,
not balance claims: 3 is larger than every
kind's engagement range except Recon's 5,
and small enough that a full circuit is 24
steps or fewer. Slice 20 does NOT retune
either value headlessly. A wider loop might
unstick some patrols and strand others, and
a narrower loop might hide a block without
answering what the player does when one
remains — neither is a player action, and
neither can be justified without Windows
evidence or a stuck case whose cause is
proved to be the radius itself. Step 1 must
therefore do two things about radius: first,
it must not widen, narrow, or reshape the
loop as a side effect of its action (a
re-rally moves the loop's centre; it does
not change its radius or its waypoint
count); second, if its proof finds a stuck
case whose cause is the radius rather than
the block — a loop that cannot fit where it
was placed, with no blocked cell in the way
— it must record that finding as a gap for
a later radius decision, not quietly widen
the radius to make the proof pass. The
Slice 19 review's gap 5 says exactly this,
and this scope repeats it so the step
cannot mistake a radius finding for a
radius licence.

## Slice 20, in order

### Step 1 — Stuck-patrol player action

Decide what action, if any, the player has
for a stuck patrol, decided in that step —
not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the action question
  the Slice 18 review left open, and record
  it: for a patrol reading Stuck, does the
  player get a recall (the squad leaves the
  field), a re-rally (the squad takes a new
  rally and a new loop), both as separate
  actions, or neither — Stuck stays a display
  word and the player's only remedies stay
  waiting for the block to clear, for the
  squad to die, or for a replacement after
  death? The step must weigh at least those
  hypotheses, none of them adopted here, and
  it must weigh them against the economy
  facts above, not in the abstract: a
  player-initiated recall is not the silent
  oldest-squad recall the cap rule rejected
  — the player chooses it, knowing the cost
  — but it still destroys a paid investment
  and its position, so the step must say
  whether that destruction needs a refund,
  a confirmation, or neither, and why. A
  re-rally preserves the investment and its
  position history, but it moves a squad the
  player paid to hold a route, so the step
  must say whether that move is free, priced,
  or priced only for a stuck squad, and why
  a price, if any, does not turn a rescue
  into a second deploy. Keeping neither is a
  decision, not a default: the step must say
  why a squad that reads Stuck for circuit
  after circuit, firing in passing all the
  while, needs no player action at all — or
  why it does, with the player's route task
  in mind: a patrol is placed to walk a route,
  and a route that cannot be walked is a
  placement the player cannot currently
  correct.
- The step must decide the scope of the
  action, and record it: does it apply to a
  stuck Patrol only, to any live Patrol, or
  to squads of any doctrine? A general
  dismiss or a general re-rally is a larger
  decision than a stuck-patrol rescue — it
  touches HoldGround, Advance, and Defend
  Core squads that are not stuck and have no
  route failure to correct — and the step
  must justify whichever scope it chooses,
  not assume the motivating case generalises.
  If it scopes to stuck Patrols only, it must
  say what happens when the action is
  attempted on a squad that is not stuck:
  refused fail-closed with nothing changed,
  or allowed with the same effect, and why.
  If it scopes wider, it must prove the wider
  scope's cases too, not only the stuck one.
- The step must decide the targeting and
  selection rule, and record it: how does the
  core know which squad the action is for?
  The picker selects doctrine and kind for a
  deploy that does not yet exist; it selects
  no live squad, and no live-squad selection
  exists in core. The step must decide
  whether the core API targets a squad by
  its position, by its rally, by its order in
  the squad list, or by some other identity
  it defines, and it must say what happens
  on a miss — no squad there, a dead squad,
  two squads sharing a cell — fail-closed
  with nothing changed, or a defined
  fallback, and why. The step must also say
  what the adapter binding is proposed to be,
  as a Windows checklist item, not as a
  headless proof: the picking keys Alpha1–
  Alpha7 are taken while picking, O toggles
  the picker, Return and Escape act while
  picking, and mouse fire acts when not
  picking, so a new binding must be checked
  against those keys and must not be claimed
  as working in a scene from a core test.
- The step must decide the state changes, and
  record them: for a re-rally, the new rally,
  the waypoint index after the change, and
  the refusal counter after the change. The
  index and the counter are live progress,
  not saved fields, so the step must say
  whether both reset to 0 on a re-rally —
  the clean reading, since the loop is
  re-derived from the new rally — or whether
  either carries over, and what activity the
  snapshot reads immediately after: still
  Stuck until the first successful step on
  the new loop, or Patrolling at once, and
  why that reading is honest either way. For
  a recall, the step must say whether the
  squad is removed at once like a dead squad
  (freeing a cap slot on the next deploy
  check, with no corpse, no marker, and no
  snapshot entry), and whether any counter,
  inventory, or save value moves with it.
  For neither, the step must say what, if
  anything, changes in core at all — a
  decision that adds no API must still be
  recorded as a decision, with its proof
  being that a stuck patrol keeps cycling,
  firing, and reading Stuck exactly as
  Slice 18 proved.
- The step must decide the economy relation,
  and record it: does a recall refund any
  EmberOre (none, full 10, or partial, with
  the amount justified, not assumed), and
  does a re-rally cost any EmberOre? The
  sunk-cost rule — no refund on death or
  otherwise — is the starting position, not
  a decision: a refund is an economy change
  and must be justified as one, with its
  effect on the cap proved (a refunded
  recall that also frees a slot is two
  gifts, not one, and the step must say so
  if it grants both). A priced re-rally must
  say what happens when the player cannot
  pay: fail-closed with the old rally kept,
  or allowed with a debt, and why. The price
  (10) and the cap (3) otherwise stand: no
  Slice 20 step raises the cap to make room
  for a rescued squad, and no step reprices
  a deploy to pay for a rescue.
- The step must decide the proof set, and
  record it: an enclosed patrol reading
  Stuck (the Slice 18 Step 1 proof, reused
  as the starting state, not re-proved as if
  new), then the decided action applied,
  then the observable effect proved — for a
  re-rally, the squad marches toward the new
  rally, its loop waypoints re-derive from
  the new rally, its counter and index read
  as decided, and its activity reads as
  decided, with fire in passing unchanged
  unless the step justifies a fire change;
  for a recall, the squad is gone from the
  snapshot, the cap count drops by one, a
  new paid deploy succeeds in the freed
  slot, and no other squad moves, fires, or
  changes activity because of it; for
  neither, the stuck patrol keeps cycling
  and firing and reading Stuck for a further
  full circuit, proving the decision is a
  reading, not an omission. In every case:
  a non-stuck patrol control (the action
  refused, or allowed, exactly as decided);
  a second doctrine control (a HoldGround
  squad untouched by a patrol-scoped action,
  or affected exactly as a wider scope
  decides); all three kinds proved, or a
  stated reason why kind does not matter
  here (kinds differ in health, damage, and
  range, not in route progress, and the step
  must say whether that is enough to prove
  one kind or all three); a save/load
  round-trip (a re-rallied patrol loads with
  its new rally and marches from index 0, a
  recalled squad stays gone after load, a
  stuck-before-save patrol still loads
  Patrolling at index 0 under the existing
  rule, which the step must not break); and
  the paid deploy path unchanged (price 10,
  cap 3, picker Confirm still the only play
  path, a recalled squad not redeployable
  free, a re-rallied squad not charged a
  second deploy price unless the step
  decides a re-rally price and proves it).
- The step must decide the movement
  boundary, and record it: no movement rule
  moves in this step, under any hypothesis.
  MoveForPatrol's sidestep, skip, and Stuck
  threshold stand; HoldGround, Advance, and
  Defend Core movement stand; no pathfinder
  is added; no sidestep widens; no hold
  replaces a skip. A re-rally that moved a
  squad by teleporting it to the new rally
  would not be a re-rally at all, and the
  step must say so if it finds itself
  tempted: the squad walks to the new rally
  under the existing step rule, or the step
  says why it does not. A stuck squad that
  is re-rallied to a rally it still cannot
  reach must read Stuck again after a full
  circuit on the new loop, proved, not
  assumed — a rescue that cannot fail again
  has hidden the block, not answered it.
- The step must decide the checklist item,
  and record it: a new checklist item (from
  39) observing the decided action on
  Windows — an enclosed patrol reading
  Stuck, the action applied by the player,
  and the observable effect read aloud
  (gone, or marching to the new rally) —
  and it must say whether that item
  supersedes, extends, or leaves standing
  item 37's patrol-route observation and
  item 29's and orbital-command item 13's
  patrol observations. The step must state
  its verification honestly: like Slice 18's
  steps, this step has a headless-testable
  claim — a squad state before and after an
  action, for staged layouts — so it adds
  tests for every case in the proof set
  above, and the test suite must rise, not
  stand, with no failures. No demo scenario
  is required of this step alone; the
  composed demo belongs to Step 2, which
  stages the decided action together with
  its controls.
- The step must decide the display relation,
  and record it: the snapshot activity word
  for a stuck patrol stays Stuck unless the
  step justifies a new word, and no HUD line
  is added, moved, or reworded in this step.
  A re-rally or recall that needed a new HUD
  line to be understood would be a HUD
  decision smuggled inside an action step,
  and the step must say so rather than add
  one. The existing squad markers render the
  snapshot; a recalled squad needs no marker
  because it needs no snapshot entry, and a
  re-rallied squad's marker follows its new
  rally under the existing marker rule, which
  the step must not change to make its
  answer look right.

### Step 2 — Slice 20 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, 0.66.0, 0.76.0,
0.80.0, and 0.83.0 — and unlike Slice 16's
review, which was documentation-only because
no graphics claim is headless-provable —
this review IS accompanied by a new composed
demo scenario, because the Step 1 action is
headless-testable and a demo that stages it
together with its controls proves something
a step's isolated tests do not: that the
decided action, the untouched movement rule,
and the untouched economy agree in one
field. The demo scenario, decided in that
step, must stage in one Simulation, at
minimum: an enclosed patrol reading Stuck
(the Step 1 case, with the action applied
and the effect read before and after); a
moving patrol control that is not stuck
(the action refused, or allowed, exactly as
Step 1 decides); a second-doctrine control
(a HoldGround squad untouched, or affected
exactly as a wider scope decides); and a
save/load round-trip proving the action's
effect persists as decided (a new rally
loads, a recalled squad stays gone, a
stuck-before-save patrol still loads
Patrolling at index 0) — with the paid
deploy path exercised beside it, so a reader
can see the cap and price did not move to
accommodate the rescue. Composed-integration
tests mirroring the demo must be added, and
the test suite must rise with no failures.
The review writes `docs/slice-20-review.md`,
checks the slice against this scope step by
step, states the verdict honestly (one
stuck-patrol action decided with tests, not
playtested, not Unity-verified), separates
headless-tested action claims from the
Windows-checklist action observation,
carries the gaps that remain, and does NOT
scope Slice 21 — Slice 21 is scoped
afterwards, the same way this document
follows the Slice 19 review rather than
preceding it. Every earlier demo save length
must stand; the new scenario's save length
is quoted as the new entry, not as a change
to an old one.

## Deliberately NOT in Slice 20

- A radius, waypoint-count, or loop-shape
  change. The loop stays 3 and 5, flat at
  rally height, unless a later decision on
  recorded evidence changes it — see
  "Radius and waypoints" above.
- A movement, targeting, or fire rule change.
  MoveForPatrol's sidestep, skip, and Stuck
  threshold stand; a stuck patrol still fires
  in passing; no pathfinder, no hold, no
  wider sidestep is added here.
- A delivery, precedence, conflict, rating,
  cost, or phase rule change beyond the
  economy question Step 1 must answer
  explicitly (recall refund, re-rally price,
  or neither, with justification). The deploy
  price (10) and the cap (3) otherwise stand,
  and no power, consumer, or save-format rule
  moves: a re-rally reuses the saved rally
  field, a recall removes a squad the way
  death already removes one, and no new save
  field is added unless Step 1 justifies one
  and proves old saves still load.
- A new block, kind, doctrine, source, or
  consumer, a balance pass, or a price or
  cap change disguised as a rescue. Slice 20
  is one stuck-patrol action and its review,
  and the core stands exactly as Slice 19
  left it except for the action itself.
- A material, shader, marker, light, fog, or
  view model change, or a HUD redesign. The
  Slice 16 surface renders whatever Slice 20
  decides; a step that re-palettes a marker
  or adds a HUD line to make its answer look
  right has confused the surface with the
  action.
- A general squad-command system. Selecting,
  ordering, or commanding squads beyond the
  one decided stuck-patrol action is a larger
  slice, carried after Slice 20, not patched
  in beside a rescue.
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–38,
  and the new item Step 1 adds, are Windows
  observations, recorded in DECISIONS.md when
  observed, not closed by a document, a
  script, or a test count.
- A scene file (`.unity`), an imported asset,
  a texture, or a shader file written by a
  headless run and claimed as a built scene.
  Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout,
  or trademarked look from any other game.
- Windows Unity verification itself. It
  remains open for Slices 1–20 alike and is
  blocked on Unity 6000 on Windows, not on
  anything this slice can do headlessly.
- A Slice 21 scope. Slice 21 is scoped after
  the Slice 20 review, not inside it.

## Ranked candidates for Slice 21 scoping (not promised)

These are recorded here so the Slice 20
review starts from a list, not from scratch.
Order follows the Slice 19 review's gaps
with the stuck-patrol action taken:

1. The patrol route's remaining questions —
   a radius or waypoint first-pass value
   revisited on evidence (a Step 1 finding
   that the radius caused a block, or a
   Windows observation that a loop does not
   fit its ground), or, if Slice 20 scopes
   its action to stuck Patrols only, a
   general recall or re-rally for other
   doctrines as its own decision — each with
   its own tests, not promised here.
2. Any Slice 13 remainder that Slice 20 does
   not itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in the Slice
   19 review's gap 4, plus the dusk and view
   model observations in items 33 and 34,
   plus the tint, count, and patrol
   observations in items 35, 36, and 37,
   plus the new item Step 1 adds in item 39)
   — if those observations are still unmade
   when the review is written, they remain
   observations to be made when Windows
   verification runs, not steps to be
   re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning step
   is justified by an observation, not by a
   headless re-decision, and is not promised
   here.

## Slice 20 gate (what "done" means)

- Every step pushed with passing tests (for
  Step 1, with the suite risen by the proof-
  set tests, never standing still on an
  action step, and for Step 2, with the suite
  risen by the composed tests); the Step 1
  action decision recorded with rationale in
  DECISIONS.md, with its own Windows
  checklist item (item 39, or the number
  Step 1 decides if it finds 39 already
  taken, with the relation to items 37, 29,
  and orbital-command item 13 stated).
- No movement, targeting, fire, delivery,
  precedence, conflict, rating, phase, or
  save-format rule changed in any step
  except the action itself and the economy
  answer Step 1 justifies explicitly (recall
  refund, re-rally price, or neither); no
  material, marker, light, fog, model, or HUD
  rule changed in any step except the
  snapshot activity reading Step 1 justifies
  for the moment after the action.
- Every earlier demo save length unchanged,
  with the new Slice 20 length quoted as its
  own entry; a re-rally that reuses the saved
  rally field changes no save length by
  itself, and a recall that removes a squad
  changes a save only as death already does
  — the review must say which, rather than
  quoting the suite as unchanged.
- This review records the verdict honestly,
  separating headless-tested action claims
  from the Windows-checklist action
  observation, including that Slice 20 is
  still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending throughout
  and is not closed by Slice 20.
