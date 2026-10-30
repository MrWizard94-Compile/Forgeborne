# Forgeborne — Slice 6 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 6 system is
built by this document. Each step below is built one at a time,
with tests, in this order, and Slice 6 is closed by its own
review — it is not closed by finishing step 1 alone.**

## Scope question

Slice 5 closed the building gap Slice 4 left open: a Dynamo,
one face-adjacency rule, and one powered behaviour now make a
machine do what an unpowered machine does not. What Slice 5
did not touch is the player's own view of any of it. The
presentation gap is now the exposed one, in three parts that
all limit the same Unity play session. First, the FPS player
cannot look up or down: the adapter yaws only, the camera
rotates `Euler(0, yaw, 0)`, and the picker raycast therefore
runs horizontally from eye height, so a rally on flat ground
ahead is picked by aiming at the face beyond it, not by
looking down. Second, squads and rallies exist in Unity only
as text lines: there is no squad marker, no rally marker, and
no pending-rally marker in the 3D scene, and the one display
word that describes a squad's state (`MOVING` / `HOLDING`) is
rally-relative, so an Advance squad reads wrong — hunting
while standing adjacent to an enemy, or holding at a rally it
is ignoring. Third, powered automation has no Unity presence
at all: Slice 5 touched no adapter, so there is no mill or
Dynamo build key and no power indicator, and a loop proved
headlessly has never been built by hand or seen in the game
itself. Slice 6 is therefore a presentation slice: **FPS pitch
aiming, 3D squad / rally markers, and powered Unity
presence** — not a new core system, not a new unit, not a
new power tier.

## Inputs this scope is decided from

- Slice 5 review (`docs/slice-05-review.md`) ranked candidates,
  carried unchanged from the 0.24.0 scope: FPS pitch aiming +
  3D squad / rally markers first (now including the Advance
  display gap and the new note that powered automation has
  no Unity presence at all yet), then a second squad type.
  The 0.26.0 decision recorded that the Step 2 evidence gives
  no reason to reorder them, and this scope keeps that order
  — the top candidate becomes Slice 6, the second becomes
  the first ranked Slice 7 candidate below, not a promise.
  The powered note is folded into Slice 6 as its own step
  (step 3), rather than left as a footnote on the markers
  step, because a build key and an indicator are a different
  small job from markers, and folding them into the markers
  step would make that step do two presentation systems at
  once.
- The Slice 5 review's gaps, in priority order, and what this
  scope does with each: Unity verification (gap 1) and the
  director playtest (gap 2) remain open, but neither is
  Slice 6 work in itself — the first is blocked on Unity
  6000 on Windows, the second on the director — and scoping
  a slice does not close, reorder away, or pretend to answer
  either. What Slice 6 does is shrink the first gap's
  checklist honestly: it writes the adapter code the
  checklist will one day run, and it records, step by step,
  which parts are headless-tested and which parts are
  Windows-checklist only. Balance (gap 3) is a later pass,
  not a slice. The free-deploy gap (gap 5) remains
  documentation, not enforcement.
- Current behaviour this scope must reckon with, not ignore:
  `FpsInputAdapter` stores yaw only (`_yaw`, mouse X times
  the existing sensitivity), rotates the camera with yaw
  alone, and feeds world-aligned movement into
  `FpsController` — movement rotation by yaw is already
  flagged in that adapter as a later Slice 1 step, not done.
  `Simulation.GatherAhead` gathers the ground block at
  front-centre (+Z, fixed, not yaw-relative),
  `BuildWallAhead` builds front-right, `BuildKineticMillAhead`
  front-left, and `BuildDynamoAhead` front-centre, all at
  feet level under the same air / support rules. `FireAtNearestEnemy`
  fires at the nearest enemy by Manhattan distance, with no
  aim in the rule at all. The picker raycast is a
  centre-screen ray from the camera, forward, 30 m, against
  the voxel mesh's MeshCollider. `OrbitalView.Snapshot`
  copies position, rally, health, members, damage, doctrine,
  and kind per live squad, with `AtRally` / `IsMoving` as its
  only state words. `OrbitalSquadDisplay` renders that
  snapshot as text lines, plus the picker prompt, and states
  in its own header comment that scene markers are a later
  step, not claimed. No adapter key builds a mill or a
  Dynamo (keys today are E gather, Q wall, R reload, O
  picker, Enter / Esc / 1 / 2 while picking). `WorldZone.IsPowered`
  already answers, in core, whether a cell is powered, and
  `HudState` carries no power field. Those two facts are why
  step 3 is small: the core question is already answered,
  and only the key and the indicator are missing.
- A honesty constraint this scope states up front, because
  Slice 6 differs from Slices 3, 4, and 5: its main value is
  adapter code that cannot be compiled or run headlessly.
  "Tests passing" in Slice 6 therefore covers the core
  pieces each step adds (a Look model, a snapshot activity
  word, a power-indicator derivation), and the adapter
  pieces are covered by Windows verification checklists, not
  by the test count. A Slice 6 step that claimed its adapter
  code as tested because the core tests passed would be
  mislabelling, and the Slice 6 review must separate the two
  claims in its verdict, as every adapter doc since 0.11.0
  has done.

## Slice 6, in order

### Step 1 — FPS pitch aiming

Add vertical aim, decided in that step — not in this scope
document — with the reasoning recorded in DECISIONS.md:

- The step must decide where pitch lives, and record it:
  the scope's starting hypothesis is a minimal, testable
  core Look (pitch, clamped, plus a direction vector derived
  from yaw and pitch) that the adapter feeds with mouse
  deltas and renders the camera from, rather than an
  adapter-only pitch variable no headless test can touch.
  The adapter's existing yaw storage is the precedent for
  the adapter-only shape, and the step must say why that
  shape is or is not enough for pitch — a direction vector
  is what the picker raycast, and any future aim rule,
  actually consumes, so a pitch that exists only as a camera
  angle is a rendering fact, not an aim fact. A hypothesis
  is not a decision: if the step finds a core Look would
  duplicate the adapter's yaw or force movement to rotate
  with it, it must say so and choose otherwise, with the
  evidence.
- The step must decide what pitch changes first, and record
  it: the scope's starting hypothesis is camera plus picker
  only. The picker raycast already follows the camera's
  forward, so a pitched camera immediately lets a rally be
  picked by looking down at flat ground, which is the
  concrete picker failure pitch exists to fix. Gather, build,
  and fire do NOT switch to aim in this step under the
  hypothesis: `GatherAhead`, the three builders, and
  `FireAtNearestEnemy` keep their Slice 1 rules, because
  switching all three at once would change the proven
  gather -> build -> survive loop in the same step that
  introduces pitch, and no test could then say which change
  broke it. Aim-based gather / build / fire are named
  follow-ups for a later step, not smuggled into this one —
  unless the step proves a switch is trivially the same
  forward-cell rule for one of them, in which case it must
  prove that one, not assert all three.
- The step must decide the clamp and the sensitivity, and
  record them: the scope's starting hypothesis is a clamp
  that prevents looking past vertical (so the camera cannot
  flip), reusing the adapter's existing mouse sensitivity
  rather than inventing a second sensitivity value, with no
  invert option claimed unless the step adds one. The clamp
  value is a first-pass value for testability, not a feel
  claim — feel is a playtest question, and no headless test
  can answer it.
- The step must decide whether yaw-rotated movement rides
  along, and record it: the scope's starting hypothesis is
  no. Movement rotation by yaw is already a flagged later
  Slice 1 step, and pitch is vertical aim, not movement;
  bundling a movement rewrite into the pitch step would make
  Slice 6 step 1 a small FPS rewrite, not a small step. If
  the step finds pitch without yaw movement is incoherent
  in a way the tests can show, it must prove that, not
  assert it — and the Slice 6 review carries the movement
  question forward either way, not quietly.
- The step must decide the save question, and record it:
  the scope's starting hypothesis is that Look is session
  input state, like the picker selection, and is not saved —
  a loaded game starts looking wherever the adapter starts
  it, and no save field, old-save rule, or malformed-entry
  rule is created for it. If the step saves Look, it must
  say why a view direction belongs in a save at all.
- What must NOT change in this step: weapon damage, range,
  and mag rules; the fire targeting rule; gather and build
  cells; squad movement, fire, targeting, price, and cap;
  Dynamo cost, distribution, and cadence; and the save
  format, unless the save decision above creates a field.
  Adapters, if touched, only feed mouse deltas and render
  the camera from the core Look — no duplicate aim math.

Why first: every other Slice 6 step is aimed, literally,
through the camera. Markers are placed and read through the
player's view, and a powered build is placed by aiming at a
cell; a picker that can only raycast horizontally limits
both, and a camera that cannot pitch is the oldest FPS gap
in the repo (named as later work in `docs/orbital-command.md`
since 0.15.0). Pitch first, markers second, so the markers
are designed for a camera that can look down at them.

### Step 2 — 3D squad / rally markers, and the Advance display fix

Put squads and rallies in the scene, decided in that step —
not in this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the snapshot extension, and record
  it: the scope's starting hypothesis is an activity word
  derived in core, not in the adapter, replacing the
  rally-relative `MOVING` / `HOLDING` word for display.
  HoldGround marching to its rally reads as marching, at
  its rally as holding; Advance with a living enemy reads
  as hunting whether it is moving toward that enemy or
  standing adjacent and firing; Advance with no living
  enemy reads as idle, standing still and ignoring its
  rally, which is the documented Advance behaviour, not a
  bug to hide. The hunting target cell is re-derived each
  tick by the same nearest-enemy rule Advance already uses
  for movement and fire, so movement, fire, and display can
  never disagree about the prey — and, like targeting, it
  is derived, not persisted, so no save field is created
  for it. A hypothesis is not a decision: if the step finds
  the activity word belongs on the squad itself rather than
  on the snapshot, or that a target cell in the snapshot
  would leak a second targeting rule, it must say so and
  choose otherwise, with the evidence.
- The step must decide the marker set, and record it: the
  scope's starting hypothesis is one marker per live squad,
  one marker per squad rally, and one pending-rally marker
  while the picker is open — no more, so the scene does not
  gain a marker layer the HUD discipline in
  `docs/art-direction.md` would call clutter. Marker form
  is decided in the step, under two constraints that are
  decisions of this scope, not hypotheses: markers are
  original primitives built in the adapter (no imported
  assets, no copied art), and their colours follow the art
  direction (neon cyan as the primary energy / HUD colour
  for squads, orange as the secondary accent for rallies
  and objectives). Whether markers show through walls is
  decided in the step, with the scope's starting hypothesis
  that they do not — world objects occlude honestly, and a
  through-wall marker would be a wallhack claim, not a
  readability aid — unless the step proves occlusion makes
  the markers useless at FPS distance.
- The step must update the text display to use the new
  activity word, and record it: the rally-relative word is
  replaced, not kept alongside as a second word that can
  contradict the first. Existing display tests, if any,
  are updated for the intended behaviour change, as the
  0.21.0 range test was, and the change is labelled as such
  in the step's changelog entry.
- What must NOT change in this step: squad movement, fire,
  targeting, doctrine, and kind rules; squad price and cap;
  the picker flow and its keys; Dynamo and mill behaviour;
  and the save format, unless the snapshot decision above
  creates a field it must then justify. Adapters render
  the snapshot only — no second copy of squad rules, and no
  marker that moves a squad.

Why second: markers are the display half of the picker, and
the picker is the system Slice 6 exists to make usable in
Unity. A rally picked by text coordinates alone is the
presentation gap the orbital docs have named since 0.11.0,
and the Advance word is the one display lie still in the
game — a hunting squad described by a rally it ignores.

### Step 3 — Powered Unity presence

Give the Slice 5 loop a hand in Unity, decided in that step —
not in this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide the build keys, and record them:
  the scope's starting hypothesis is one key for a Kinetic
  Mill and one key for a Dynamo, alongside the existing E /
  Q / R / O bindings, chosen with no collision and with a
  stated rule for what happens while the picker is open
  (the picker already suppresses mouse fire; the step must
  say whether build keys are suppressed while picking too,
  and why). The builders already exist in core
  (`BuildKineticMillAhead`, `BuildDynamoAhead`, with their
  front-left and front-centre cells, costs, and support
  rules), so this step adds no core build rule — it wires
  keys to builders that are already proved.
- The step must decide the power indicator, and record it:
  the scope's starting hypothesis is a HUD / display line
  derived from `WorldZone.IsPowered` — for example, a count
  of powered mills, or a powered mark on the mill lines —
  not a new core rule and not a new mesh system. The step
  must check, and record, whether the mesher already renders
  a Dynamo block distinctly from stone and mills; if it
  does not, a distinct mesh tint is a decision for this step
  only if the step proves it small, and otherwise it is
  carried as a named gap, not claimed. A text indicator that
  exists is worth more than a tint that is promised.
- What must NOT change in this step: Dynamo cost (5
  EmberOre), face-adjacency distribution, powered cadence
  (every tick, replacing not stacking with the kinetic
  cadence), kinetic mill behaviour, squad rules, loot,
  skills, core health, and the save format. No voltage
  ladder, transformer, second source, second consumer,
  crafting, or fuel rides along — this step makes the one
  proved loop visible and buildable by hand, it does not
  extend it.

Why third, not first: a Dynamo key before pitch and markers
would let a player build power while still unable to aim
down at a build cell or see a squad that defends it. The
order is the same argument as Slice 5's: presentation follows
the loop it presents, and within presentation, aim first,
then the things aim is for.

### Step 4 — Slice 6 demo scenario and review

As with 0.13.0, 0.19.0, 0.23.0, and 0.26.0: extend the demo
with one composed Slice 6 run, then write
`docs/slice-06-review.md` as a documentation-only step. The
demo proves, headlessly, the core pieces of steps 1–3: a
clamped Look and its direction vector; a HoldGround squad
reading marching then holding, and an Advance squad reading
hunting then idle as its prey lives and dies; and a powered
mill count derived from placement through a save/load
round-trip, with the Dynamo and mill surviving it. The demo
cannot prove adapter rendering — no headless run can show a
marker in a scene or a key press building a Dynamo — so the
review's verdict must separate the two claims in words: the
core presentation pieces are tested, the adapter pieces are
written and checklist-listed, and neither claim is allowed
to borrow the other's evidence. The review closes Slice 6 as
tested presentation core plus written adapters — not as
playtested, balanced, or Unity-verified — and names the next
candidates in priority order. Slice 7 is not scoped by that
review; it is scoped afterwards, the same way this document
follows the Slice 5 review's candidates rather than
preceding them.

## Deliberately NOT in Slice 6

- A second squad type, priced under the Slice 3 economy.
  First ranked Slice 7 candidate; pricing a second unit
  before the picker that deploys it can be aimed and read
  in the scene would repeat the 0.15.0 mistake of widening
  combat while the existing system's presentation is
  unfinished.
- Defend Core and Patrol doctrines, an Orbital camera, or a
  mini-map beyond the Slice 1 placeholder. The orbital docs
  name the camera as later work; Slice 6 markers are scene
  markers in the FPS view, not a second camera.
- The rest of automation tier 2: a voltage ladder (LV to
  EV), transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Those are
  deferred, not rejected, and are not ranked above the
  second squad type by this scope — the Slice 6 review may
  reorder on the evidence of step 3, but this scope does
  not pre-order them first.
- Aim-based gather / build / fire, and yaw-rotated movement.
  Both are named follow-ups that step 1 must carry forward
  explicitly, not work it smuggles in: switching the proven
  Slice 1 interaction cells or the movement axes in the same
  step as pitch would make pitch untestable in isolation.
  They are ranked as a Slice 7 candidate below, behind the
  second squad type, because they change how the player
  touches the world, not just how the player looks at it.
- A balance pass. Pitch clamp, marker colours, and the
  Dynamo key are first-pass values chosen for determinism
  and testability, exactly as Slice 3's prices and Slice 4's
  combat numbers were, and must be labelled as such, not
  quoted as final feel.
- Enforcement of the free-deploy rule beyond its existing
  documentation. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- FPS pitch beyond this scope's step 1, licence,
  monetisation, release status, or any change to repo
  visibility. Those need their own decision steps (and, for
  licence / monetisation / release, Rob).
- Windows Unity verification itself. It remains open for
  Slices 1–6 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly. The
  checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, and `docs/orbital-command.md`
  remain the verification plan, and Slice 6 steps must add
  their own checklist items to those docs rather than
  claiming verification.

## Ranked candidates for Slice 7 scoping (not promised)

These are recorded here so the Slice 6 review starts from a
list, not from scratch. Order follows the 0.24.0 / 0.26.0
ranking with the Slice 6 candidate removed, plus the two
follow-ups this scope names explicitly:

1. A second squad type, priced under the Slice 3 economy.
2. Aim-based gather / build / fire, and yaw-rotated
   movement — the interaction follow-ups step 1 carries
   forward, as one candidate because they are one question
   (does the player's aim, not a fixed +Z cell, decide what
   the player touches?), not two unrelated systems.
3. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional consumers
   and sources), deferred, not rejected.

## Slice 6 gate (what "done" means)

Slice 6 is done when: every step above is pushed with passing
tests; pitch is clamped and its direction vector is proved
in core tests, and the picker raycast follows the pitched
camera in the adapter code; squads and rallies have scene
markers rendered from the snapshot only, and an Advance
squad reads hunting or idle by its prey, never by a rally it
ignores; a mill and a Dynamo are buildable by key in the
adapter code, with a power indicator derived from the
existing `IsPowered` rule; the composed demo proves the
core presentation pieces through a save/load round-trip;
old saves still load; squad price, cap, and behaviour,
Dynamo cost, distribution, and cadence, loot, skills, and
core health are unchanged; and `docs/slice-06-review.md`
records the verdict honestly, separating headless-tested
core claims from Windows-checklist adapter claims, including
that Slice 6 is still not playtested, balanced, or
Unity-verified. The Slice 1 director playtest gate is still
pending throughout and is not closed by Slice 6.
