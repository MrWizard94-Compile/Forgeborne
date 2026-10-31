# Forgeborne — Slice 18 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the spent director queue decision of
2026-10-30 09:27 EDT. No Slice 18 system is built
by this document. Each step below is built one at
a time, with tests, in this order, and Slice 18 is
closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 17 settled what two display queries say about
a wiring. What it deliberately did not touch is
what two orbital remainders do in play. Both were
carried honestly rather than patched inside a
query step, and both are about a squad or a deploy
that looks correct while behaving incorrectly:

- A Patrol squad whose waypoint step is refused
  skips that waypoint for the circuit, and a
  squad whose every waypoint is blocked cycles
  its waypoint index without moving — while
  still reading Patrolling, exactly as a moving
  patrol reads. The route rule is stated, not
  hidden, but no detour, hold, or stuck readout
  exists, so a player watching an enclosed patrol
  sees a patrolling squad that never patrols.
- The free deploy primitive,
  `WorldZone.DeploySquad` (and through it
  `Simulation.DeployOrbitalSquad`), charges
  nothing and ignores the squad cap. It is
  headless-only by rule, not by enforcement:
  nothing in code stops a future play path from
  calling it, while the play path today — the
  picker through `OrbitalCommand.Confirm` to
  `TryDeployPaidSquad` — is priced and capped.

Slice 18 is therefore the orbital-remainder slice,
kept as small as the slices before it: **the two
carried orbital remainders, each decided as its
own step with its own tests, then its own
review** — not a new doctrine, kind, block, or
source, not a balance pass, not a pathfinder, not
a HUD redesign, and not a claim of Unity
verification made from a headless run.

## Inputs this scope is decided from

- The Slice 17 review's ranked candidates, in the
  same order, with the Slice 17 work taken:
  patrol route limits and the free deploy
  primitive first, Slice 13 remainders as
  observations second, first-pass graphics
  retuning on Windows evidence third. This scope
  obeys that order; it does not re-rank it, and
  it does not reorder the two remainders among
  themselves without a reason — the reason for
  the order below is stated, not assumed.
- The Slice 17 review's gaps, in priority order,
  and what this scope does with each: Windows
  Unity verification (gap 1) and the director
  playtest (gap 2) remain open, but neither is
  Slice 18 work in itself — the first is blocked
  on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close,
  reorder away, or pretend to answer either.
  First-pass graphics values (gap 3: dusk
  azimuth, fog contrast arithmetic, view model
  offset, the static weapon-agnostic model) are
  unproven, but they are retuned only on Windows
  evidence, not re-decided headlessly here: no
  Slice 18 step moves a light, fog, or model
  value to make a patrol or a deploy read
  better. The Slice 13 scene-proof observations
  (gap 4: items 25–27 and 30–36, still unmade as
  observations) remain observations to be made
  when Windows verification runs, not steps to
  be re-scoped blindly — if an observation
  fails, that failure is a fix step with its own
  decision, not a reason to re-scope the proof
  layer. The transformer-adjacency invisibility
  (gap 5: a Conduit beside a transformer alone
  still reads 0 LIVE, 0 LONE) is carried, not
  forgotten and not smuggled into Slice 18: it
  is a display question of the Slice 17 kind,
  but the review ranked the orbital remainders
  ahead of it, and Slice 18 already carries two
  decisions plus a review. It is the first
  candidate for Slice 19 scoping, alongside the
  gap-4 observations and gap-3 retuning — not
  promised here, and not decided here. The
  patrol route's stated limits (gap 6) and the
  free deploy primitive (gap 7) are Slice 18's
  work, in that order, for the reason below.
- Why the patrol step comes before the deploy
  step, stated as a decision rather than a
  ranking habit: the patrol question changes
  what a player sees a squad do on the field —
  move, hold, or stand still while reading
  Patrolling — and it is the one whose answer
  changes the composed demo's marching proof.
  The deploy question changes what code paths
  are allowed, and today no play path uses the
  free path at all, so its answer changes no
  player's field behaviour until a future play
  path is written. Reversing the order would not
  break either step, and the scope records that
  honestly: patrol-first is chosen because a
  visible behaviour outranks a boundary rule
  with no current violator, not because the
  deploy step depends on the patrol step. The
  deploy step's cases must still be stated
  independently of the patrol answer: a patrol
  deployed through the paid path must behave
  exactly as a patrol deployed any other way
  behaves, or the deploy step must say why the
  deploy path changes a doctrine's movement —
  it should not, and the step must prove it
  does not.
- Why these two questions are orbital questions
  and not economy or movement-system questions,
  stated before either step decides anything:
  the squad economy (10 EmberOre, cap 3 live
  squads, no refund) and the shared movement
  rule (one voxel per tick on the largest-gap
  axis, ties X, then Y, then Z, refused out of
  bounds, into a solid block, or onto a living
  enemy's cell) are both settled, and no Slice
  18 step is allowed to move them silently. A
  patrol answer that changed the shared rule
  would move HoldGround, Advance, and Defend
  Core with it, and a deploy answer that changed
  the price or the cap would be a balance pass
  wearing a boundary's name. If a step finds
  its honest answer requires such a move, it
  must say so and defer that move, not smuggle
  it in as a route or boundary fix.
- Ground facts this scope is decided from,
  verified this run against the code as it stands:
  `TacticalSquad.PatrolLoopRadius` is 3 and
  `PatrolWaypointCount` is 5 (rally, then east,
  south, west, north, flat at rally height);
  `PatrolTarget` is derived from the rally and
  `PatrolWaypointIndex` on every call, never
  stored; `PatrolWaypointIndex` is live progress
  starting at 0 on deploy and resetting to 0 on
  load, so a loaded patrol marches to its rally
  first; `MoveForPatrol` advances past a waypoint
  the squad already stands on in the same tick
  (turn and continue, no dwell), steps through
  the shared `MoveTowardPosition` rule, and on a
  refused step advances the index without moving
  (skip for this circuit, tried again next
  circuit) — so a fully blocked patrol cycles
  its index without moving, which the Slice 15
  comments call "still not a hold" and "waiting
  on no single waypoint"; Patrol fires whenever
  in range through `FindTarget`, never chases,
  and its snapshot activity is Patrolling
  (appended 5) whether moving or cycling; the
  other doctrines' blocked behaviour is their
  own (HoldGround holds short of its rally,
  Advance stops adjacent to its prey, Defend
  Core returns to its post), and none of them
  shares Patrol's skip rule. On deploy:
  `WorldZone.DeploySquad` charges nothing,
  ignores the cap, rejects only an out-of-bounds
  rally or an undefined doctrine/kind, and is
  documented as "for headless tests and the demo
  only"; `TryDeployPaidSquad` checks bounds,
  validity, cap, and affordability before
  charging, charges only after every check
  passes, and refunds in full on an unexpected
  post-charge failure; `SquadEconomy` prices
  every kind at 10 EmberOre and caps live squads
  at 3; the picker (`OrbitalCommand.Confirm`)
  deploys through the paid path, and the Unity
  adapter (`FpsInputAdapter`, Enter while
  picking) calls `ConfirmOrbitalDeploy`, never
  the free primitive — verified this run: no
  adapter file references `DeploySquad` or the
  free `DeployOrbitalSquad` at all. The free
  primitive's cost of removal is also a ground
  fact, not a guess: the test file calls
  `DeploySquad` on over a hundred lines, and
  the demo calls the free
  `Simulation.DeployOrbitalSquad` path, so a
  hard removal or rename is a migration across
  the whole test and demo suite, not a one-line
  tightening — and the step must weigh that
  cost in the open. The next free playtest-gate
  checklist number is 37 (items run to 36, the
  direct-Conduit count).

## Slice 18, in order

### Step 1 — Patrol route limits

Decide what a patrol does when its route is
blocked, decided in that step — not in this scope
document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the question the Slice 15
  and Slice 16 reviews left open, and record it:
  when a patrol's step toward its waypoint is
  refused, should it detour, hold, keep skipping,
  or read as stuck — and, separately, what a
  fully blocked patrol (every waypoint refused)
  should do and should read as? The step must
  weigh at least four hypotheses, none of them
  adopted here: a one-step sidestep (try a
  second axis when the largest-gap axis is
  refused, still one voxel per tick, still no
  pathfinder); a hold (stay and retry the same
  waypoint each tick, like HoldGround holding
  short, until the block clears); keep the skip
  rule and add a stuck readout (the movement
  stands, but a cycling-without-moving patrol
  stops reading as an ordinary Patrolling
  patrol); and keep the skip rule unchanged with
  a stated reason (a patrol is a route, not a
  guarantee, and a blocked route's cost is a
  missed circuit, not a broken squad). Keeping
  the rule is a decision, not a default: the
  step must say why a player watching an
  enclosed patrol is not misled, or why they
  are, with the squad's field behaviour in mind
  — a patrol that never moves while reading
  Patrolling is the orbital equivalent of the
  single-cable misread Slice 17 fixed, a live
  claim about a dead route.
- The step must decide the scope of the change,
  and record it: a sidestep in the shared
  `MoveTowardPosition` rule would move every
  doctrine, not only Patrol, so the step must
  say whether its mechanism lives in
  `MoveForPatrol` alone or in the shared rule,
  and prove the other doctrines unchanged in
  either case — HoldGround holding short,
  Advance stopping adjacent to its prey, Defend
  Core returning to its post, and each doctrine's
  existing blocked-step tests still passing with
  their old readings. A full pathfinder (search
  over several cells, a planned route, a route
  cache) is a new system, not a detour: if the
  step wants one, it must justify it as a new
  system with its own cost (per-tick work,
  saved or live route state, interaction with
  the one-voxel-per-tick rule), not present it
  as a small sidestep. A stuck readout, if one
  is added, must say whether it is a new
  snapshot activity, a new snapshot field, or a
  derived reading from existing fields — and if
  it is state, whether it is live progress like
  `PatrolWaypointIndex` (reset on load) or saved
  state, with the load behaviour proved, not
  assumed. No price, cap, radius, waypoint
  count, fire rule, or engagement range moves in
  this step: radius 3 and the five-waypoint loop
  are first-pass values, and if the step finds
  the radius is the cause of a block, that
  finding is recorded as a gap, not fixed by
  quietly widening the loop.
- The step must decide the proof set, and record
  it: a single blocked waypoint (solid block on
  the direct axis — the step must prove the next
  tick's behaviour, and the next circuit's retry,
  under its chosen rule); a fully enclosed
  patrol (every waypoint refused — the step must
  prove it does not move, does not crash, cycles
  or holds exactly as decided, and reads as
  decided, over several ticks, not one); a step
  refused by a living enemy's cell rather than a
  solid block (the block can move away, so hold
  and skip differ observably here — the step
  must prove which one its rule does when the
  enemy leaves); a blocked waypoint that later
  clears (enemy moves, or the patrol's own
  circuit brings it back — the step must prove
  resumption, not only refusal); fire in passing
  while blocked, holding, or stuck (a patrol
  that cannot move must still fire exactly as
  the decided rule says — the step must prove
  firing does not stop, start, or chase under
  the new rule); all three kinds, or a stated
  reason why one kind's proof covers the others
  (movement is kind-independent in code, but
  Recon's range 5 reaches across its own loop
  while Vanguard and Heavy do not, so a
  fire-while-blocked proof for one kind does not
  automatically cover the others); and save/load
  (a blocked or stuck patrol saved and loaded
  must behave as the step's load rule says —
  waypoint index reset to 0 is the existing rule,
  and any stuck state must follow the rule the
  step chose for it, proved by a loaded patrol
  that marches to its rally first). If the step
  keeps the skip rule, it must prove the same
  set reads as it does today, and must say what
  the player is expected to read instead of a
  movement change for an enclosed patrol — the
  marker, the text line, or the absence of a
  circuit — rather than leaving the misread
  unnamed.
- The step must decide the checklist relation,
  and record it: a new checklist item (37)
  observing the decided behaviour on Windows —
  an enclosed patrol, a singly blocked
  waypoint, and a patrol whose block clears,
  against the dark voxel scene, with the
  marker and the text line read for each — and
  it must say whether that item supersedes,
  extends, or leaves standing item 29's Patrol
  observation and orbital-command item 13.
  Item 29 stands as the pre-decision
  observation, and the new item is the
  post-decision observation, or the step must
  say why they are the same observation. The
  step must state its verification honestly: a
  patrol's movement and activity are
  headless-testable claims, so it adds tests
  for every case in the proof set above, and
  the test suite must rise, not stand, with no
  failures. No demo scenario is required of this
  step alone; the composed demo belongs to Step
  3, which stages the decided patrol behaviour
  together with the decided deploy boundary.
  No material, shader, marker, light, or model
  value moves in this step, except a marker or
  text change the stuck readout itself requires
  — and if it requires one, the step must weigh
  it against HUD discipline and the marker
  distinction Slice 13 built, not add a shape
  or colour as an afterthought.

### Step 2 — Free deploy primitive enforcement

Decide what, in code, stops a play path from
using the free deploy primitive, decided in that
step — not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide the question the Slice 15
  and Slice 16 reviews left open, and record it:
  is the free primitive's headless-only status
  enforced, and if so, by what mechanism — or
  does it stay a documented rule, and if so,
  why is a rule enough? The step must weigh at
  least four hypotheses, none of them adopted
  here: hard removal or privatisation of
  `DeploySquad` and the free
  `Simulation.DeployOrbitalSquad`, migrating
  every test and demo call site to the paid path
  (or to a test-only seam the step designs);
  renaming the primitive so its headless-only
  status is in its name (for example a
  test-deploy name), with the old name removed,
  so a future play-path call reads as wrong at
  the call site; a compiler-level marking (such
  as Obsolete) that warns on any new call
  without breaking the existing test suite; and
  keeping the rule-only status, with the play
  path audited and the audit recorded (picker
  through paid deploy, adapter through Confirm,
  no adapter call to the free path), on the
  grounds that no play path uses it today and
  the migration cost of hard enforcement buys
  no current player protection. Keeping the
  rule is a decision, not a default: the step
  must say what would have to change for the
  rule to stop being enough — a second play
  path, a Unity script that deploys directly, a
  demo that becomes a play path — and must
  name that trigger, so a later reader knows
  when to reopen the question.
- The step must decide the migration cost in the
  open, and record it: the test file calls
  `DeploySquad` on over a hundred lines and the
  demo calls the free Simulation path, so hard
  removal is not a one-line tightening. The step
  must count the call sites it would move (or
  state the count it found, with the counting
  method), say which demo scenarios would need
  re-seeding with ore to deploy through the paid
  path, and say whether any earlier demo save
  length moves as a consequence — a moved save
  length must be quoted in the review as a new
  entry with its rationale, never silently
  absorbed as an unchanged old entry, and the
  step must say whether that churn is
  acceptable for a boundary step or whether
  the churn alone rejects hard removal. A
  renaming or marking mechanism must say the
  same for its own churn: a rename touches the
  same call sites, while a marking touches
  (in principle) none, and the step must not
  present a rename as cheaper than it is.
  The step must also say what happens to
  `Simulation.DeployOrbitalSquad` separately
  from `WorldZone.DeploySquad`: they are two
  entry points to one primitive, and enforcing
  one while leaving the other open enforces
  nothing — or the step must say why one of
  them is not a play-path risk (for example
  because Simulation's free path is itself the
  headless loop's entry point, and the Unity
  adapter never touches Simulation's deploy
  methods directly — verified this run for the
  adapter files, but the step must re-verify
  it against the code as it stands then).
- The step must decide the proof set, and record
  it: the play-path audit, re-verified (picker
  Confirm to `TryDeployPaidSquad`, adapter
  Enter to `ConfirmOrbitalDeploy`, no adapter
  file calling the free primitive — an audit
  claim, not a test-count claim, because
  adapter code is not compiled headlessly); the
  paid path unchanged (cap 3, price 10 for every
  kind, refused deploy charges nothing and
  deploys nothing, post-charge failure refunds
  in full — proved for all four doctrines and
  all three kinds through the paid path, not
  assumed from earlier slices, because this step
  is the one that touches the deploy surface);
  the forbidden path under the chosen mechanism
  (a test that calls the free path and proves
  it fails, warns, or is unreachable exactly as
  the mechanism promises — or, if the mechanism
  is rule-only, a test that proves the play path
  still charges and caps, and a recorded audit
  in place of a forbidden-path test, with the
  step saying plainly that no test can prove a
  convention); and the paid-path patrol
  (a Patrol squad deployed through the paid
  path behaves exactly as Step 1 decided — same
  loop, same blocked behaviour, same activity —
  so the deploy step does not silently change a
  doctrine's movement). If the mechanism adds a
  player-observable behaviour (for example a
  picker refusal message or a new HUD word for
  a refused deploy), the step must weigh it
  against HUD discipline, as Slice 17's count
  step did, and must take a checklist item only
  if there is something in the scene to observe
  — a code boundary with no scene change takes
  no checklist item, and the next free number
  then remains 38 after Step 1's item 37. The
  step must state its verification honestly:
  like Step 1, it has a headless-testable claim
  — the paid path's behaviour and the chosen
  mechanism's effect — so it adds tests for
  every case in the proof set above, and the
  test suite must rise, not stand, with no
  failures, unless the step's decided mechanism
  is rule-only and it records, with reasons,
  which of its claims are audit claims rather
  than test claims. No price, cap, refund,
  doctrine, kind, or movement rule moves in this
  step, under any hypothesis: enforcement is
  about which path may be taken, not about what
  the path costs or what the squad does.

### Step 3 — Slice 18 demo + review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0,
0.55.0, 0.60.0, 0.63.0, 0.66.0, and 0.76.0 — and
unlike Slice 16's review, which was
documentation-only because no graphics claim is
headless-provable — this review IS accompanied
by a new composed demo scenario, because both
Slice 18 decisions are headless-testable and a
demo that stages them together proves something
a step's isolated tests do not: that a patrol
deployed through the decided deploy boundary
behaves as Step 1 decided on a route that is
actually blocked. The demo scenario, decided in
that step, must stage in one Simulation, at
minimum: a patrol deployed through the paid path
(the picker or the paid headless path, with the
ore cost visible in the demo's numbers — not
through the free primitive, whatever Step 2
decided about it, or the demo must say why it
still uses the free path and what that costs
the composed claim); a route with at least one
blocked waypoint, so the decided detour, hold,
skip, or stuck behaviour is exercised, not
merely asserted; a second patrol or a second
phase proving the fully blocked case if the
demo's first patrol does not already prove it
(a singly blocked route does not prove an
enclosed one); and a save/load round-trip
proving the decided load behaviour (waypoint
index reset to 0, and any stuck state following
Step 1's load rule). Composed-integration tests
mirroring the demo must be added, and the test
suite must rise with no failures. The review
writes `docs/slice-18-review.md`, checks the
slice against this scope step by step, states
the verdict honestly (two orbital remainders
decided with tests, not playtested, not
Unity-verified), separates headless-tested
movement and deploy claims from
Windows-checklist patrol observations, carries
the gaps that remain, and does NOT scope Slice
19 — Slice 19 is scoped afterwards, the same way
this document follows the Slice 17 review rather
than preceding it. Every earlier demo save
length must stand, except any length Step 2
explicitly re-baselined with rationale in
DECISIONS.md — a silent length change fails the
gate, and the review must quote any re-baselined
length as a new entry, not as a change to an old
one.

## Deliberately NOT in Slice 18

- A delivery, precedence, conflict, rating, cost,
  or phase rule change, a price or cap change
  (10 EmberOre, cap 3, no refund), a radius or
  waypoint-count change (radius 3, five
  waypoints), a fire, targeting, or engagement
  rule change, or a save-format change beyond
  what a stuck readout, if added, explicitly
  requires and justifies. If a step finds its
  honest answer needs one of these, that need
  is recorded and deferred, not made inside a
  route or boundary step.
- A new doctrine, kind, block, source, or
  consumer, a movement rule change for any
  doctrine other than the one Step 1 explicitly
  decides, a pathfinder presented as a sidestep,
  or a balance pass. Slice 18 is two orbital
  remainders and their review, and the core
  stands exactly as Slice 17 left it except for
  the two decisions themselves.
- A material, shader, marker, light, fog, or view
  model change, except a marker or text change
  Step 1's stuck readout itself requires and
  justifies against the Slice 13 marker
  distinction and HUD discipline. A step that
  re-palettes a marker or retunes dusk to make
  a patrol read better has confused the surface
  with the route.
- A HUD redesign. The deploy step decides one
  boundary question, and the patrol step one
  readout question, and nothing else: no new HUD
  colour, no new HUD layout, no quest list, no
  second POWER line smuggled in as a
  "clarification".
- A scene-proof observation presented as a
  headless result. Items 25–27 and 30–36, and
  the new item Step 1 adds, are Windows
  observations, recorded in DECISIONS.md when
  observed, not closed by a document, a script,
  or a test count.
- A scene file (`.unity`), an imported asset, a
  texture, or a shader file written by a headless
  run and claimed as a built scene. Scene
  assembly remains Windows checklist work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains
  open for Slices 1–18 alike and is blocked on
  Unity 6000 on Windows, not on anything this
  slice can do headlessly.
- The transformer-adjacency invisibility, or any
  other display-query remainder, as a bonus
  inside a route or boundary step. Each is its
  own decision with its own tests, carried
  after Slice 18, not patched in beside a patrol.
- A Slice 19 scope. Slice 19 is scoped after the
  Slice 18 review, not inside it.

## Ranked candidates for Slice 19 scoping (not promised)

These are recorded here so the Slice 18 review
starts from a list, not from scratch. Order
follows the Slice 17 review's gaps with the two
orbital remainders taken:

1. The transformer-adjacency invisibility,
   carried from the Slice 17 review's gap 5 — a
   Conduit beside a transformer alone reading
   0 LIVE, 0 LONE, decided with its own proof
   set (including the dead-cable case, so the
   decision is consistent), not by inheriting
   Step 2 of Slice 17's sentence.
2. Any Slice 13 remainder that Slice 18 does not
   itself settle as an observation (the
   vertex-colour material observation, the
   MeshRenderer wiring observation, and the
   marker-occlusion observation in gap 4, plus
   the dusk and view model observations in items
   33 and 34, plus the tint and count
   observations in items 35 and 36, plus the
   patrol observation Step 1 adds) — if those
   observations are still unmade when the review
   is written, they remain observations to be
   made when Windows verification runs, not steps
   to be re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on
   Windows evidence (dusk azimuth, fog contrast,
   view model offset, static weapon-agnostic
   model) — a retuning step is justified by an
   observation, not by a headless re-decision,
   and is not promised here.

## Slice 18 gate (what "done" means)

Slice 18 is done when: every step above is pushed
with passing tests (for Steps 1 and 2, with the
test suite risen by the proof-set tests, never
standing still on a decision step — except a
rule-only deploy decision, which must record
which of its claims are audit claims and why no
test can prove them — and never inflated into a
claim about a visible scene); the Step 1 patrol
decision and the Step 2 deploy decision are each
recorded with rationale, Step 1 with its own
Windows checklist item from 37, Step 2 with one
only if it adds a scene-observable behaviour,
with no price, cap, radius, waypoint, fire,
targeting, delivery, precedence, conflict,
rating, cost, or phase rule changed except as a
step explicitly justifies and records, no
material, marker, light, fog, model, or HUD rule
changed except a stuck readout's own justified
change, and every earlier demo save length
unchanged except a length Step 2 explicitly
re-baselines with rationale; and
`docs/slice-18-review.md` records the verdict
honestly, separating headless-tested movement
and deploy claims from Windows-checklist patrol
observations, including that Slice 18 is still
not playtested and not Unity-verified. The Slice
1 director playtest gate is still pending
throughout and is not closed by Slice 18.
