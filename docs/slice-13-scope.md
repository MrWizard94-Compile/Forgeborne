# Forgeborne — Slice 13 Scope

**Status: SCOPED 2026-10-30, under delegation. No Slice 13 system is
built by this document. Each step below is built one at a time,
with tests where a step has a headless-testable claim and with
Windows-checklist items where it does not, in this order, and
Slice 13 is closed by its own review — it is not closed by
finishing step 1 alone.**

## Scope question

Slice 12 closed the electrical-widening gap Slice 11 left open:
a live cable network feeding no consumer yet, and a lone source
of either kind, are now counted in the HUD, and HV finally has
a safe direct use in the Shock Pylon. What Slice 12 did not
touch is what the player can see, in the scene, before
something is lost — and what the scene still cannot tell
them apart. A player who wires a mill to HV still learns it
only when the mill disappears on its delivery tick: there is
no warning, and the only headless traces are the HUD totals
and the `DestroyedMills` session counter. A player who wires
a workstation to HV, or a pylon to LV, learns it only when
regen or fire stops that tick. A player looking at the scene
still cannot tell a Heavy from a Vanguard or a Recon: all
three kinds share one cyan cube marker, and kind is read
only from the text line. And no machine is rendered
distinctly by power state at all: a Dynamo, an Alternator,
a transformer, a cable, a powered mill, a powered
workstation, or a powered pylon carries no tint, and no
adapter refreshes the voxel mesh on a key build, so a cable
laid by aim, a pylon placed by aim, or a mill destroyed by
voltage may be counted before it is visible in the scene.
Slice 13 is therefore a visibility-and-fairness slice, kept
as small as the slices before it: **one warning first, then
one squad-marker distinction, then one mesh tint and refresh,
and closed by its own review** — not a balance pass, not a
third source, not Patrol, not a fourth kind.

## Inputs this scope is decided from

- Slice 12 review (`docs/slice-12-review.md`) ranked
  candidates, carried unchanged: adapter visibility and
  fairness first, the deferred electrical widening second,
  Patrol third. The Step 3 composition evidence gives no
  reason to reorder them — the pylon kill, the live counts,
  and the bench regen proved together say nothing about
  whether a player can see a loss coming — so the top
  candidate becomes Slice 13, and the second and third
  become the ranked Slice 14 candidates below, not a
  promise.
- The Slice 12 review's gaps, in priority order, and what
  this scope does with each: Unity verification (gap 1)
  and the director playtest (gap 2) remain open, but
  neither is Slice 13 work in itself — the first is
  blocked on Unity 6000 on Windows, the second on the
  director — and scoping a slice does not close, reorder
  away, or pretend to answer either. What Slice 13 does,
  like Slices 6, 7, 8, 9, 10, 11, and 12 before it, is
  write only adapter code the checklists will one day run
  and record, step by step, which parts are
  headless-tested and which parts are Windows-checklist
  only. Electrical balance (gap 3) is not a slice: any
  warning threshold, tint, or marker shape a step chooses
  is a first-pass value for determinism and testability,
  exactly as the pylon's range 5 and damage 10 were, and
  must be labelled as such, not quoted as final feel.
  Destruction and disablement with no warning (gap 4),
  mesh tint and mesh refresh (gap 5), and three kinds
  sharing one scene marker (gap 6) are Slice 13's gaps,
  taken together as the one visibility question the
  Slice 12 review named — and split apart here, by
  decision, into three ordered steps rather than one
  mega-step, because the three are different kinds of
  work with different verification: a warning has a
  headless-testable core readout, a marker distinction
  is adapter-only on a snapshot that already carries
  kind, and a mesh tint needs a colour channel that
  does not exist yet plus a refresh event set. One step
  covering all three would be untestable as a unit.
  The deferred electrical widening (gap 7, a third
  source) stays deferred with its distinctness question
  intact: a third source must still prove a
  distinctness beyond voltage before it can be claimed,
  and it does not inherit the pylon's proof by analogy.
  Patrol (gap 8) stays deferred with its route question
  intact. The free deploy primitive (gap 9) remains a
  headless-only rule, re-named, not quietly enforced
  here.

## Slice 13, in order

### Step 1 — A pre-destruction / pre-disablement warning

Add one warning readout, decided in that step — not in this
scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide what the warning covers, and
  record it: the scope's starting hypothesis is all three
  mismatch outcomes together, because all three already
  have a mismatch predicate in core — `IsMismatched`
  (mill, destroyed), `IsWorkstationMismatched`
  (workstation, disabled), `IsPylonMismatched` (pylon,
  disabled) — and a warning that covers only the mill
  would leave the two disablements exactly as silent as
  they are today. A hypothesis is not a decision: if
  the step finds that one readout cannot distinguish a
  destruction from a disablement without misleading the
  player, it must say so and split the readout, with
  the evidence. What the step must NOT do is warn only
  after the tick that destroys or disables: a warning
  that appears on the same snapshot as the loss is a
  death notice, not a warning.
- The step must decide the warning's shape, and record
  it: the scope's starting hypothesis is a derived HUD
  readout in the pattern of `PoweredMills` /
  `TotalMills` — derived from placement and delivered
  voltage on every snapshot, never saved, so rewiring
  a machine clears its warning on the next snapshot
  and destroying it removes it — with the questions
  the step must answer, not assume: whether the
  readout is one line or a count per outcome (a mill
  about to be destroyed and a workstation about to be
  disabled are not the same loss, and folding them
  into one number may hide which one the player
  built); whether a conflicted (None) machine warns
  (it is neither powered nor mismatched today, and a
  warning for None would redefine a state the slices
  deliberately left idle); and whether an unpowered
  machine warns (it loses nothing on the next tick,
  so a warning for it would cry wolf every snapshot).
  Delivery, precedence, conflict, rating, cost, and
  phase rules must NOT change in this step: the
  warning is a readout of the mismatch predicates,
  not a delay, a grace period, or a salvage rule —
  a warning that postpones destruction by even one
  tick would be a rule change smuggled in as a
  display step, and every earlier demo save length
  must stand.
- The step must decide the adapter line, and record
  it: `HudDisplay` today renders four POWER lines from
  four existing Text fields (mills, workstations, live
  networks / lone sources, pylons), so a warning needs
  either a fifth Text field with its own line or a
  stated reason to fold into an existing line — and a
  playtest-gate checklist item, Windows-only, for
  whichever it chooses. The headless snapshot already
  carries the numbers, so no new snapshot line is
  assumed beyond the HUD fields themselves; but the
  scene question must be answered, not inherited: a
  HUD warning line is not a mesh tint and not a mesh
  refresh, and the step must not claim the threatened
  machine is visible in the scene because it is
  warned about in the HUD. Gap 5 stays open after this
  step, by decision, and the step's review must say
  so.

### Step 2 — A scene-marker distinction for the squad kinds

Add one marker distinction, decided in that step — not in
this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide what distinguishes the kinds,
  and record it: the scope's starting facts are that
  `SquadSnapshot` already carries `Kind` for every
  live squad, and `SquadMarkerDisplay` already reads
  that snapshot every frame but ignores `Kind`,
  rendering the same cyan cube, 0.6 units, for
  Vanguard, Recon, and Heavy alike — so no core rule
  needs to change for a distinction to exist, and the
  step must not invent a core rule to justify one.
  The questions the step must answer, not assume, are
  whether the distinction is shape, colour, or both
  (a colour-only distinction fails a player who
  cannot tell those colours apart, and a shape-only
  distinction must still read at FPS distance, where
  the existing marker already proves occlusion makes
  markers useless through walls); and whether the
  distinction replaces the shared cyan cube or varies
  it per kind (a replacement risks breaking the one
  marker players already recognise, while a variation
  must stay inside the pooled-marker budget: at most
  3 squad markers, 3 rally markers, and 1 pending
  marker, pooled, not rebuilt per frame).
- The step must decide the palette and the untouched
  markers, and record them: the rally marker is
  already orange, flat, 0.9 x 0.12 x 0.9, and the
  pending marker is already an orange sphere, so any
  kind colour that reads as orange would be mistaken
  for a rally marker — the step must name its kind
  colours or shapes against those two, not in
  isolation — and the rally and pending markers stay
  unchanged unless the step justifies a change to
  them as part of the distinction. No squad rule
  changes in this step: no kind shape, cost, cap,
  movement, targeting, or doctrine rule moves because
  a marker changes, and the text display
  (`OrbitalSquadDisplay`, which already reads kind
  from the same snapshot) must keep agreeing with the
  markers, word for word.
- The step must state its verification honestly, and
  record it: a marker distinction is adapter-only
  Unity code, not compiled headlessly, so no test
  count can prove a colour or a shape reads in the
  scene — the step's verification is the written
  adapter code plus a playtest-gate checklist item,
  Windows-only, and the existing headless tests for
  snapshot `Kind` coverage are cited, not re-counted
  as marker evidence. The step must not claim a test
  proves the distinction, and must not add a demo
  scenario that pretends a printed kind proves a
  visible marker.

### Step 3 — Mesh tint and mesh refresh for power state

Add one mesh treatment, decided in that step — not in
this scope document — with the reasoning recorded in
DECISIONS.md:

- The step must decide what the mesh shows, and
  record it: the scope's starting facts are that
  `MeshData` today carries vertices, normals, uvs,
  and indices, and no colour channel at all; that
  `VoxelMeshRenderer.RefreshMesh` exists but is called
  only at Start and on load, never after a build key
  or a destruction tick; and that power state is
  already derivable per cell from the existing
  delivery rules (`DeliveredVoltage`, the powered /
  mismatched predicates), so the step needs a query,
  not a new rule. The questions the step must answer,
  not assume, are: which cells tint (energised cables
  only, powered machines only, mismatched machines
  only, or all three — each answers a different
  player question, and tinting everything would make
  the tint meaningless); how a tint is carried
  (vertex colours, a per-machine material, or another
  channel the step justifies — each with its cost to
  `MeshBuilder`, which must remain the single chunk
  representation, never a second one); and which
  events refresh the mesh (every build key, every
  destruction tick, every tick, or a dirty set — a
  full 32^3 re-mesh is acceptable per the Slice 1
  note, but refreshing every frame blindly would
  spend that budget for no visible change, and the
  step must name its event set, not inherit "always").
- The step must decide the tint's relationship to the
  warning, and record it: Step 1's HUD warning and
  Step 3's mesh tint are two readouts of the same
  mismatch predicates, so they must agree — a machine
  the HUD warns about must be the machine the mesh
  tints, and a machine the HUD stops warning about
  after rewiring must lose its tint on the next
  refresh — or the step must say why they differ.
  The tint must not collide with the marker palette:
  a mismatched machine tinted orange would read as a
  rally marker, and a powered machine tinted cyan
  would read as a squad marker, so the step names its
  tints against both, not in isolation. No delivery,
  precedence, conflict, rating, cost, or phase rule
  changes in this step, and no new save field: tint
  and refresh are derived and rendered, never saved.
- The step must state its verification honestly, and
  record it: the per-cell power-state query is core
  code and is headless-testable (a cell's tint state
  derives from the same delivery rules the slices
  already test, and the tests must prove it for a
  powered, an unpowered, a mismatched, and a
  conflicted cell alike) — but the tint itself and
  the refresh wiring are adapter-only Unity code, so
  their verification is the written adapter code plus
  playtest-gate checklist items, Windows-only, one
  for the tint and one for the refresh event set, not
  a test count. The step must not claim the query
  tests prove the scene shows the tint.

### Step 4 — Slice 13 demo scenario and review

As with 0.34.0, 0.38.0, 0.43.0, 0.47.0, 0.51.0, and
0.55.0: extend the demo with one composed Slice 13 run
(the Step 1 warning reading a threatened mill, a
threatened workstation, and a threatened pylon in the
same Simulation, beside the Step 2 snapshot kinds and
the Step 3 power-state query — not separate proofs),
then write `docs/slice-13-review.md` as a
documentation-only review. The demo's job is
composition of the headless-testable parts: the warning
must read before the destruction / disablement tick and
clear on rewiring, the query must read the same power
state the warning reads, and the snapshot must carry
all three kinds — so the reader can see the three steps
agree, not merely coexist. The review checks the slice
against this scope, states the verdict honestly (tested
warning core and power-state query plus written
adapters, not playtested, balanced, or Unity-verified),
separates headless-tested core claims from
Windows-checklist adapter claims (marker distinction,
tint, refresh), carries the gaps that remain, and does
NOT scope Slice 14 — Slice 14 is scoped afterwards, the
same way this document follows the Slice 12 review's
candidates rather than preceding them.

## Deliberately NOT in Slice 13

- A third source, a third consumer, or any other
  electrical widening added alongside the visibility
  steps as a bonus. One visibility question is Slice
  13; the deferred third source is a Slice 14-or-later
  decision with this slice's evidence in hand.
- A warning that delays, prevents, or softens a loss:
  no grace period, no salvage on destruction, no
  auto-rewire, no destruction refund. The warning is
  a readout; the loss rules from Slices 9 and 10 stand
  exactly as tested.
- A marker distinction that changes squad rules, or a
  tint that changes power rules. Display steps must
  not move the rules they display.
- Patrol, a fourth squad kind, or a second doctrine.
  The orbital side is closed for this slice; a
  visibility slice that also adds a doctrine would
  make the visibility work untestable in isolation.
- A balance pass. The warning's shape, the marker
  shapes or colours, and the tint set are first-pass
  values chosen for determinism and testability,
  exactly as Slice 12's pylon values were, and must be
  labelled as such, not quoted as final feel.
- An Orbital camera, or a mini-map beyond the Slice
  1 placeholder. Slice 13 markers remain scene
  markers in the FPS view, not a second camera.
- A new movement mode, a weapon rework, or a
  free-deploy enforcement change. The free
  `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- A new block type, a new save field, or a delivery /
  transformer / cable rule change. Every Slice 13
  readout derives from placement and voltage on every
  snapshot, the way the Slice 12 counts do.
- Windows Unity verification itself. It remains open
  for Slices 1–13 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly. The checklists in
  `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`,
  and `docs/powered-presence.md` remain the
  verification plan, and Slice 13 steps must add their
  own checklist items to those docs rather than
  claiming verification.

## Ranked candidates for Slice 14 scoping (not promised)

These are recorded here so the Slice 13 review starts
from a list, not from scratch. Order follows the
0.52.0 / 0.55.0 ranking with the Slice 13 candidate
removed:

1. The deferred electrical widening (a third source,
   now that Slice 12 chose a third consumer) —
   deferred, not rejected, and only after Slice 13
   proves its visibility steps. Its distinctness
   question is unchanged: a condition, a fuel, a cost
   shape, or another distinctness beyond voltage,
   proved by a test neither existing source can pass.
2. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.
3. Any Slice 13 remainder the Slice 13 review names
   (for example a visibility sub-question this slice
   splits off rather than closes) — recorded at
   review time, not promised here.

## Slice 13 gate (what "done" means)

Slice 13 is done when: every step above is pushed
with passing tests (or, for the adapter-only parts of
Steps 2 and 3, with the test suite unchanged and the
adapter-only verification stated, never inflated into
a test claim); the Step 1 warning's coverage, shape,
and adapter line are each decided and tested, with
delivery rules unchanged and every earlier demo save
length unchanged; the Step 2 marker distinction's
shape-or-colour choice, palette, and untouched
markers are each decided and checklist-listed, with
no squad rule changed; the Step 3 tint set, colour
channel, and refresh event set are each decided, with
the power-state query tested and no power rule
changed and no new save field; the composed demo
proves the warning, the query, and the snapshot kinds
together through a save/load round-trip; and
`docs/slice-13-review.md` records the verdict
honestly, separating headless-tested core claims from
Windows-checklist adapter claims, including that Slice
13 is still not playtested, balanced, or
Unity-verified. The Slice 1 director playtest gate is
still pending throughout and is not closed by Slice 13.
