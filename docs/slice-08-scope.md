# Forgeborne — Slice 8 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 8 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 8 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 7 closed the combat-shape gap Slice 6 left open: the
player now has two squad types to choose, priced under the
Slice 3 economy, deployed through a picker that can itself
be aimed by pitch. What Slice 7 did not touch is the
player's own hands. The player can look anywhere — yaw
toward +X, pitch up to 89 degrees — and still acts in
exactly one direction: +Z. Gather takes the ground block
directly in front at fixed +Z. Walls, mills, and Dynamos
build at fixed front-right, front-left, and front-centre
cells, all at fixed +Z. Fire hits the Manhattan-nearest
enemy wherever it stands, whether or not the player is
looking at it. Movement is world-aligned too: W always
walks +Z, D always walks +X, no matter which way the
player faces. So today the player looks one way, walks
another, and touches a third — the FPS promise, that aim
is the player's hand in the world, is not yet kept. Slice 8
is therefore an interaction slice, kept as small as the
slices before it: **the player's aim (the core `Look`
yaw and pitch) decides what the player touches and which
way the player walks** — yaw-rotated movement first,
aim-based gather / build / fire second, closed by a
composed demo and review — not a new movement mode, not
a weapon rework, not a third kind or doctrine.

## Inputs this scope is decided from

- Slice 7 review (`docs/slice-07-review.md`) ranked
  candidates, carried unchanged from the 0.32.0 scope:
  aim-based gather / build / fire plus yaw-rotated
  movement first, then the rest of automation tier 2,
  then a third kind and the deferred doctrines. The
  0.34.0 decision recorded that the Step 2 evidence
  gives no reason to reorder them, and this scope keeps
  that order — the top candidate becomes Slice 8, the
  rest become the ranked Slice 9 candidates below, not
  promises. Slice 7 removes the reason aim work was
  deferred in the first place: aiming a new interaction
  rule at a world with only one squad shape would have
  proved it against Vanguard alone, and the range-5
  Recon now gives the aim step a second shape to prove
  its fire rule against, not just the anchor.
- The Slice 7 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 8 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. What Slice 8 does,
  like Slices 6 and 7 before it, is write only adapter
  code the checklists will one day run (none is
  expected here — see Step 2) and record, step by step,
  which parts are headless-tested and which parts are
  Windows-checklist only. Balance (gap 3) is a later
  pass, not a slice: any reach, range, or rate the aim
  steps choose is a first-pass value for determinism
  and testability, exactly as Vanguard's and Recon's
  numbers were. The shared-marker question (gap 4) and
  the mesh tint / mesh refresh gaps (gap 5) remain named
  gaps from 0.30.0 / 0.33.0, not Slice 8 work: aim does
  not need a distinct marker or a mesh refresh hook, and
  bundling either into this slice would make it two
  systems at once. The free-deploy gap (gap 6) remains
  documentation, not enforcement.
- Current behaviour this scope must reckon with, not
  ignore: `Look` in `src/Forgeborne.Core/Look.cs` holds
  yaw (0 faces +Z, increasing toward +X, unbounded) and
  pitch (clamped [-89, 89], positive up), with a unit
  `Forward()` vector from the two angles, and is session
  input state — not saved, and an in-place RestoreWorld
  keeps the current view rather than snapping to +Z
  (0.28.0). The rally picker already consumes that
  vector: the adapter raycasts from the camera along
  `transform.forward` while picking (0.15.0), and a
  pitched camera already aims the rally at the ground
  (0.28.0) — so aim infrastructure exists in core and
  in the adapter, and Slice 8 extends it to the player's
  own actions rather than inventing a second aim system.
  Against that, the player's own actions ignore `Look`
  entirely today: `Simulation.GatherAhead` gathers
  (floor X, floor Y - 1, floor Z + 1); `BuildWallAhead`
  builds at (+1 X, +1 Z) at feet level, `BuildKineticMillAhead`
  at (-1 X, +1 Z), `BuildDynamoAhead` at (0, +1 Z), all
  through `World.Build`'s air and ground-support rules;
  `FireAtNearestEnemy` picks the Manhattan-nearest enemy
  to the FPS position and hits it regardless of aim,
  consuming ammo even when no enemy exists (a shot fired
  with nothing hit still returns true); `FireAtCore`
  damages the core within a 10-metre Euclidean bubble,
  also aim-blind. Movement is equally fixed:
  `FpsController.Update` moves forward onto +Z and
  strafe onto +X, axes resolved separately so sliding
  along a wall works, at base speed 6 m/s with gravity
  -20 and jump velocity 8, and `Simulation.Tick` passes
  the adapter's raw W/S/A/D scalars straight through —
  the adapter scales mouse deltas by its single
  `MouseSensitivity` and passes movement scalars raw.
  Skill `MoveSpeedPct` multiplies speed (0.7.0), so any
  rotation must rotate direction, not scale speed. Those
  facts are why Slice 8 is two steps, not one: movement
  rotation changes the player's body, aim targeting
  changes the player's hands, and proving both in one
  step would leave no test able to say which rule broke.

## Slice 8, in order

### Step 1 — Yaw-rotated movement

Rotate FPS movement by the core `Look` yaw, decided in
that step — not in this scope document — with the
reasoning recorded in DECISIONS.md:

- The step must decide where the rotation lives, and
  record it: the scope's starting hypothesis is core,
  in `Simulation.Tick` rotating the two scalars by
  `Look.Yaw` before they reach `FpsController.Update`,
  with the adapter still passing raw W/S/A/D — because
  the Look-in-core precedent (0.28.0) exists precisely
  so that a direction rule is testable headlessly, and
  an adapter-side rotation would be a second, untestable
  movement rule beside the core one. If the step puts
  the rotation inside `FpsController` instead, it must
  say why, and prove the same yaw-0 behaviour.
- The step must decide the rotation itself, and record
  it: yaw only, horizontal only. Pitch must NOT move
  the player vertically — looking up must not make W
  climb, and looking down must not make W dive — so
  the forward used for movement is the horizontal
  projection of `Look.Forward`, normalised, not the
  full 3D vector. At yaw 0 the result must be exactly
  today's behaviour (W walks +Z, D walks +X), proved
  by the existing movement tests passing unchanged; at
  yaw 90, W must walk +X, proved by a new test. The
  strafe sign at yaw 90 is a decision the step must
  state and prove, not a fact to assume: facing +X, D
  walks to one side of that facing and A to the other,
  and the step's test must pin which side is which, so
  a later reader is not left to guess from a sign.
- The step must decide the diagonal question, and
  record it: today forward and strafe are applied as
  separate axis moves at full speed each, so a diagonal
  (W + D together) is faster than a straight move. The
  step must say openly whether rotation preserves that
  quirk or normalises diagonal speed to the base speed
  — preserving it keeps Slice 1 feel, normalising it
  changes feel — and must prove whichever it chooses
  with a test. Silently changing diagonal speed inside
  a rotation step would be a balance change disguised
  as a direction fix.
- The step must decide what movement does NOT change,
  and record it: base speed 6, gravity -20, jump
  velocity 8, the AABB (half-width 0.3, height 1.8),
  axis-separated sliding, grounded logic, and the skill
  speed multiplier all stay exactly as they are — the
  rotation changes direction only. Squad movement,
  enemy movement, and enemy targeting are untouched:
  a yaw-rotated player is still the same target at the
  same position, and `ChooseTarget` still picks the
  nearest live target.
- What must NOT change in this step, stated plainly so
  the temporary asymmetry is not mistaken for a bug:
  gather, build, and fire keep their fixed +Z cells
  and their nearest-enemy rule until Step 2. A player
  at yaw 90 who walks +X and then gathers will still
  gather the +Z cell for exactly one step, because
  Step 1 proves movement in isolation — bundling aim
  targeting into the same step would make a movement
  failure and a targeting failure indistinguishable.
  The demo numbers for Slice 1 stay unchanged in this
  step, since no interaction cell moves.

Why first, and why alone: movement is the smaller rule
— one rotation of two scalars — and its tests depend on
nothing but yaw, while the aim step's tests will want
to position and face the player freely. Proving the
body first means the aim step can walk a yawed player
to a firing position with the real movement rule,
instead of teleporting around a world-aligned walk it
is about to replace. Body before hands, for the same
reason Slice 6 proved pitch before markers: each rule
gets one step in which it is the only thing that moves.

### Step 2 — Aim-based gather / build / fire

Replace the fixed +Z interaction cells and the
nearest-enemy fire rule with rules that read the core
`Look` direction, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the ray, and record it: its
  origin, its traversal, and its reach. The scope's
  starting hypothesis is a voxel-stepped ray along
  `Look.Forward` from the player's eye — but eye height
  is not a core constant today (the FPS position is
  feet position), so the step must define the origin
  openly (feet plus a stated eye height, or feet with
  the reason stated) rather than borrowing an adapter
  camera height headlessly. Traversal must be a stated
  method — stepping cell by cell along the ray, or a
  stated sampling rule — not "the ray hits the block"
  asserted without a walk. Reach must be a stated
  number, in metres or cells, and the step must say
  whether one reach covers all three actions or each
  action has its own; a reach chosen for testability
  is a first-pass value, labelled as such, not a feel
  claim. What the step must NOT do is leave reach
  unbounded: an unbounded gather ray would let the
  player strip a distant hillside through a key press,
  and an unbounded fire ray would silently replace
  the weapon's range with infinity.
- The step must decide gather targeting, and record
  it: what cell the ray gathers, and what it refuses.
  The scope's starting hypothesis is the first
  gatherable block along the ray, through the existing
  `World.Gather` rules (bedrock, walls, workstations,
  and mills are never gathered today, and the skill
  gather bonus still accrues only on successful
  gathers) — but the step must confront pitch directly:
  looking down at the ground gathers ground, looking
  level gathers what is ahead, and looking up must be
  decided, not discovered (a ceiling block gathered,
  or a refusal, each with its test). If the step
  instead gathers the ground block under the aimed
  point, it must say so and prove the level-aim case
  that rule would otherwise leave empty.
- The step must decide build targeting, and record
  it: what cell a build lands in, and how the three
  builders stay distinct. The scope's starting
  hypothesis is that the front-left / front-right /
  front-centre offsets retire in this step: the aimed
  cell (or the first air cell face-adjacent to the
  aimed solid, decided in the step) is the build cell,
  and the three builders are distinguished by block
  type, not by offset — because keeping three offsets
  under an aim rule would mean the aim decides one
  cell and the offset silently moves it to another.
  `World.Build`'s air and ground-support rules are
  reused, not rewritten, and the Dynamo-mill
  face-adjacency power rule (0.25.0) is unchanged: a
  powered pair is still a mill face-adjacent to a
  Dynamo, however the two blocks were placed. The step
  must prove a build that would collide with an
  existing block fails by the existing rule, not by a
  new aim exception.
- The step must decide fire targeting, and record it:
  what the shot hits, and what a miss costs. The
  scope's starting hypothesis is the first enemy whose
  cell or position the aim ray reaches within reach
  (and, for the core, the core itself when the ray
  reaches it within its rule) — replacing both the
  Manhattan-nearest rule and the 10-metre bubble with
  one aim rule, or two stated rules if the step finds
  one rule cannot honestly cover both targets. The
  step must decide whether weapon range, damage, and
  magazine behaviour change at all (starting hypothesis:
  they do not — aim changes what is hit, not how hard
  the weapon hits), and whether a shot that hits
  nothing still consumes ammo (starting hypothesis:
  yes, as today — a fired shot is a fired shot, and
  making misses free would be an economy change, not
  an aim fix). Squad fire rules are untouched: squads
  still fire by their own range and doctrine rules,
  and aim-based player fire must not change squad
  damage, range, or targeting.
- The step must decide the no-target behaviour, and
  record it: the scope's starting hypothesis is refusal
  with the existing failure results, never a silent
  fallback to the fixed +Z cells. A gather with nothing
  aimed returns the existing failure, a build with no
  aimed cell fails by the existing build rule, and a
  shot with nothing aimed consumes ammo and hits
  nothing, as today — but none of them may quietly do
  the old +Z action instead, because a fallback would
  hide every aim failure and reintroduce, as a hidden
  default, the exact bug this slice closes. If the step
  overturns this hypothesis for any action, it must do
  so openly, with the rationale and the test that
  proves the fallback cannot mask a miss.
- The step must decide neutral-aim compatibility, and
  record it: at yaw 0, pitch 0, the step must prove
  what changes against the Slice 1 cells and say so
  openly. If aim at neutral reproduces the old gather
  cell, the old build cells, and the old fire target,
  the existing demo numbers stand and the step proves
  it; if it does not — because an eye-height ray at
  level pitch strikes a different cell than the old
  feet-level +Z rule — the step must update the demo
  numbers openly and say which cells moved and why,
  not silently keep numbers the new rule no longer
  produces. Either outcome is honest; only a silent
  mismatch is not.
- The step must decide the adapter question, and record
  it: the scope's starting hypothesis is no adapter
  logic change at all. The E / Q / T / Y keys already
  call the same builder methods, and mouse click
  already calls the same fire method — if aim lives in
  core behind those methods, the adapter needs no new
  binding, and the step's evidence is the core tests
  plus a Windows checklist item for key / click feel,
  not a new adapter claim. If the step finds the
  adapter does need a change (a crosshair, a held-key
  behaviour, or anything else), it must name it, write
  it as adapter-only code, and checklist it — not test-
  count it.
- What must NOT change in this step: weapon damage,
  magazine, and reload rules; squad shapes, doctrines,
  price, and cap; Dynamo cost, distribution, and
  cadence; kinetic mill behaviour; loot, skills, and
  core health; enemy AI and targeting; the gather /
  build / survive loop itself; and the save format,
  which gains no field — `Look` stays session state,
  and a loaded game keeps the current view per 0.28.0
  rather than restoring a saved aim. No new movement
  mode (sprint, crouch, or flight), no vertical
  movement by pitch, no Orbital camera, and no balance
  pass ride along.

Why second, and why whole: gather, build, and fire are
three faces of one question — does the player's aim,
not a fixed +Z cell, decide what the player touches? —
and splitting them across slices would leave the game
in a state where the player aims to gather but not to
build, or aims to build but fires blind. One step
proves the ray once and applies it three times, with
a test per action, so no action can borrow another's
evidence. Movement came first so that step can face
the player with the real rule; aim comes second so
that movement was proved before the hands changed.

### Step 3 — Slice 8 demo scenario and review

As with 0.13.0, 0.19.0, 0.23.0, 0.26.0, 0.31.0, and
0.34.0: extend the demo with one composed Slice 8 run,
then write `docs/slice-08-review.md` as a
documentation-only step. The demo proves, headlessly,
yaw-rotated movement and aim-based interaction together,
not separately: one Simulation in which the player yaws,
walks in the yawed direction, aims, gathers, builds, and
fires by aim, through a save/load round-trip that keeps
the world, the builds, and the kills intact while the
view rule (loaded game keeps the current view, fresh
game starts at yaw 0) is stated, not smuggled. The demo
cannot prove adapter feel — no headless run can show a
mouse turn, a crosshair, or a key press in a scene — so
the review's verdict must separate the two claims in
words: the core movement and aim rules are tested, the
adapter key / click feel is written (or unchanged, per
Step 2) and checklist-listed, and neither claim is
allowed to borrow the other's evidence. The review
closes Slice 8 as tested aim interaction plus written
(or unchanged) adapters — not as playtested, balanced,
or Unity-verified — and names the next candidates in
priority order. Slice 9 is not scoped by that review;
it is scoped afterwards, the same way this document
follows the Slice 7 review's candidates rather than
preceding them.

## Deliberately NOT in Slice 8

- A third squad type, or a second kind added alongside
  Recon as a bonus. One new kind was Slice 7; the next
  kind, if any, is a Slice 9-or-later decision with
  Slice 8's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as an aim behaviour.
- The rest of automation tier 2: a voltage ladder (LV to
  EV), transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Those
  are deferred, not rejected, and are ranked behind the
  aim candidate's successor list below — the Slice 8
  evidence may reorder them, but this scope does not
  pre-order them first.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 8 interaction stays in the FPS
  view, not a second camera.
- A new movement mode: sprint, crouch, flight, climb,
  or vertical movement by pitch. Step 1 rotates the
  existing walk; it does not add a way to move.
- A weapon rework: damage, magazine, reload, or range
  changes, a hitscan-versus-projectile decision beyond
  what the aim step's ray already is, or a balance pass.
  Aim changes what is hit, not how hard the weapon hits.
- A balance pass. Any reach, eye height, or rate the aim
  steps choose is a first-pass value for determinism
  and testability, exactly as Slice 3's prices and Slice
  4's combat numbers were, and must be labelled as such,
  not quoted as final feel.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- A mesh tint for Dynamo / powered mills, a mesh
  refresh on any key build, and a distinct Recon marker.
  All three remain named gaps from 0.30.0 / 0.33.0, not
  fixed by an interaction step.
- Windows Unity verification itself. It remains open
  for Slices 1–8 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.
  The checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification
  plan, and Slice 8 steps must add their own checklist
  items to those docs rather than claiming verification.

## Ranked candidates for Slice 9 scoping (not promised)

These are recorded here so the Slice 8 review starts
from a list, not from scratch. Order follows the
0.32.0 / 0.34.0 ranking with the Slice 8 candidate
removed:

1. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional
   consumers and sources), deferred, not rejected.
2. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 8's aim evidence will inform, not promises
   made before that evidence exists.

## Slice 8 gate (what "done" means)

Slice 8 is done when: every step above is pushed with
passing tests; yaw-rotated movement is proved at yaw 0
(unchanged) and at yaw 90 (W walks +X), with the strafe
sign and the diagonal question each decided and tested;
gather, build, and fire each read the core `Look`
direction by a stated ray with a stated reach, with
no-target behaviour decided per action and no silent
fallback to fixed +Z cells; neutral-aim compatibility
against the Slice 1 cells is proved or the changed demo
numbers are updated openly; the three builders stay
distinct by block type once their offsets retire, or
the step proves otherwise; weapon, squad, Dynamo,
mill, loot, skill, and save rules are unchanged; the
composed demo proves movement and aim together through
a save/load round-trip; and `docs/slice-08-review.md`
records the verdict honestly, separating headless-tested
core claims from Windows-checklist adapter claims,
including that Slice 8 is still not playtested,
balanced, or Unity-verified. The Slice 1 director
playtest gate is still pending throughout and is not
closed by Slice 8.
