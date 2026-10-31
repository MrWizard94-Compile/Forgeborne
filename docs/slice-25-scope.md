# Forgeborne — Slice 25 Scope

**Status: SCOPED 2026-10-31, under delegation and
under the Slice 24 review's ranked candidates.
No Slice 25 system is built by this document.
Each step below is built one at a time, in this
order, and Slice 25 is closed by its own review
— it is not closed by finishing step 1 alone.**

## Scope question

Slices 15, 18, 20, 21, 22, and 23 decided, in
order, the patrol route itself, its stuck
reading, its rescue, its generalisation, its
removal, and its radius and waypoint count —
but no slice has yet given the loop's SHAPE
its own decision. Slice 23 Step 1 kept radius
3 and waypoint count 5 on audit evidence, and
its review says so plainly: the loop walked
there is the same cross loop at radius 3, not
a circular, climbing, or otherwise reshaped
loop, and a loop-shape change was deliberately
NOT smuggled inside a radius change. The
radius question and the shape question are
therefore still separate, and the shape one
is still open: is the current flat cardinal
loop — rally, then east, south, west, and
north at radius 3, then back to the rally —
the right route shape to keep, or does a
shape-caused failure in it justify a named
change? A shape is not a radius. Widening a
loop and reshaping a loop are different
decisions, with different tests, and this
slice keeps them separate, as Slice 23 did.

Slice 25 is therefore the loop-shape slice,
kept as small as the slices before it:
**decide the loop shape first, then a
composed demo plus its own review** — not a
radius or waypoint-count re-decision, not a
movement-rule change, not a pathfinder, not
an economy change, not an adapter or marker
change, and not a claim of Unity verification
made from a headless run. Deciding the shape
does not pass any playtest-gate item, and no
step in this slice marks one passed from
headless work. If Step 1 keeps the shape, the
slice's value is that the keep is now a
decided answer with its own audit, not a
first-pass value carried by inertia; if Step
1 changes the shape, the slice's value is
that the change is named, proved, and
composed, not smuggled inside another step.

## Inputs this scope is decided from

- The Slice 24 review's ranked candidates, in
  order, and what this scope does with each:
  candidate 1 (any Slice 13 remainder that
  Slice 24 does not itself settle as an
  observation — the vertex-colour material
  observation, the MeshRenderer wiring
  observation, and the marker-occlusion
  observation, plus the dusk and view model
  observations in items 33 and 34, plus the
  tint, count, and patrol observations in
  items 35, 36, and 37, plus items 39, 40, 41,
  42, and 43) remains observations to be made
  when Windows verification runs, not steps
  to be re-scoped blindly — and Slice 25
  changes that standing in no way: no step
  here observes a scene, because Unity 6000
  on Windows is still not ready (Rob
  confirmed 2026-10-31 that he hit a snag
  and then got busy with work), and scoping
  an observation as a headless step would
  manufacture a step that cannot move.
  Candidate 2 (first-pass graphics values
  retuned only on Windows evidence — dusk
  azimuth, fog contrast, view model offset,
  static weapon-agnostic model) is not
  Slice 25 work: no Windows observation has
  yet justified a retune, and a shape step
  must not retune a light, fog, or model
  value to pay for deciding a route.
  Candidate 3 (a general re-rally price on
  Windows playtest evidence, if free
  repositioning proves abusive kiting) and
  candidate 4 (a dismiss refund on Windows
  playtest evidence, if slot-only removal
  proves punishing) are not Slice 25 work
  either: both are economy decisions gated
  on playtest evidence that does not exist
  yet, and a shape step must not reprice
  either action. Candidate 5 (a loop-shape
  decision, if ever wanted, as its own
  decision with its own tests — carried from
  the Slice 23 review's gaps, not to be
  confused with the radius and count values
  Slice 23 kept) IS Slice 25's work, in the
  narrowed form below. It is the only ranked
  candidate that is a headlessly decidable
  step rather than a Windows observation or
  a Windows-evidence-gated economy decision,
  and that is why it is first: the other
  candidates are not demoted, they are
  waiting on evidence this slice cannot
  manufacture headlessly. The "if ever
  wanted" in candidate 5 is answered here,
  not deferred again: the want is not a
  taste preference for a prettier loop, it
  is the standing question Slice 23 left
  open by keeping radius and count while
  explicitly refusing to decide shape at
  the same time. A route whose shape was
  never separately audited is a first-pass
  value carried by inertia, and this slice
  is where that audit happens.
- The Slice 24 review's other gaps, in
  priority order, and what this scope does
  with each: Windows Unity verification
  (Slices 1–24, items 25–27 and 30–43 still
  open) and the director playtest remain
  open, but neither is Slice 25 work in
  itself — the first is blocked on Unity
  6000 on Windows, the second on the
  director — and scoping a slice does not
  close, reorder away, or pretend to answer
  either. The identical-twin re-validation
  limit, the invisible pending dismiss, the
  unparented re-rally pending marker,
  same-frame K+X branch order, and pending
  state not auto-clearing on a target death
  are Slice 24 adapter gaps, carried after
  Slice 25, not patched inside a shape step:
  a shape step moves no adapter branch, no
  marker, and no pending flag, and a twin
  identity field would be a core and
  save-format change smuggled inside a
  route step. The graphics retune, the
  re-rally price, and the dismiss refund are
  candidates 2, 3, and 4 above. The loop
  shape is candidate 5 above, taken here.
- Ground facts this scope is decided from,
  verified this run against the code as it
  stands, not copied from review prose: the
  loop is `PatrolTarget` in
  `src/Forgeborne.Core/Orbital.cs`, derived
  from the rally and `PatrolWaypointIndex`
  on every call, never stored — index 0 is
  the rally itself, 1 is east
  (rally + radius, same Y, same Z), 2 is
  south (same X, same Y, rally + radius in
  Z), 3 is west (rally − radius), 4 is
  north (rally − radius in Z), then back
  to 0. `PatrolLoopRadius` is 3 and
  `PatrolWaypointCount` is 5, both carried
  from Slice 23, not re-decided here. The
  loop is flat: every waypoint sits on the
  rally's own Y plane, so a patrol is a flat
  loop at rally height, not a climb. The
  circuit is 24 steps at radius 3: the two
  cardinal legs (rally to east, north to
  rally) are 3 steps each, and the three
  diagonal legs (east to south, south to
  west, west to north) are 6 Manhattan steps
  each, because a diagonal leg closes a gap
  of 3 in X and 3 in Z, and
  `MoveTowardPosition` steps one voxel per
  tick on the largest-gap axis, ties X then
  Y then Z, so a diagonal leg stair-steps —
  east to south, for example, walks
  (2,0), (2,1), (1,1), (1,2), (0,2), (0,3)
  relative to the rally — rather than
  moving diagonally in one step, which the
  voxel rule never allows. Fire uses
  Manhattan distance (`FindTarget`): rally
  to any waypoint is exactly 3, waypoint to
  an adjacent waypoint is exactly 6, and
  waypoint to the opposite waypoint is
  exactly 6 as well, so from a waypoint no
  kind reaches another waypoint (Recon's
  range 5 is the largest, still below 6),
  while from the rally Vanguard reaches a
  waypoint at exactly its range 3, Recon
  within range, and Heavy beyond its range
  2 — the Slice 23 range relation, carried,
  not re-derived here as a new claim.
  `MoveForPatrol` is the only route walker:
  one step per tick, a distance-reducing
  sidestep on a remaining gap axis when the
  direct step is refused, then a skip of
  that waypoint for this circuit, with
  `PatrolConsecutiveRefusals` climbing and
  `IsPatrolStuck` reading true only at or
  above the waypoint count 5 — so the stuck
  threshold is coupled to the count Slice 23
  kept, and any shape change that changed
  the count would have to answer that
  coupling explicitly, which is one reason
  a shape change is not a free edit. A
  re-rally re-derives the whole loop from
  the new rally through `PatrolTarget`
  (centre moves, radius and count do not),
  and a loaded patrol resets its index to
  0 and marches to its rally first, so the
  save carries doctrine, kind, position,
  and rally, and no shape field exists to
  carry — a shape change that needed a
  saved shape would be a save-format change
  and must say so. The deploy price (10
  EmberOre) and the cap (3 live squads)
  stand, the re-rally stays free, and the
  dismiss stays refund none with one slot
  freed. The next free playtest-gate number
  is 44 (items run to 43). Tests stand at
  4577/4577 before this scope, with the
  Slice 23 demo save at 44,041 chars; a
  scope step is expected to leave both
  standing, for the reason given under
  "Verification" below, and a step that
  moves either must say why.

## The shape question Step 1 must answer

This is the one question no earlier slice
answers, and it must be decided in Step 1
and proved, not assumed — or Step 1 must say
why the shape cannot be decided headlessly
and defer itself, rather than quietly keeping
a shape it has not audited. The question is
not "which shape looks best". It is: does
the current flat cardinal loop cause any
failure that a different named shape would
fix, and if not, is keeping it justified on
evidence rather than inertia? Step 1 must
answer it in this order, with a proof set
for each answer, not a paragraph of taste:

- Shape-caused stuck, separated from
  step-cell-caused stuck. Slice 23 proved
  stuck is a blocked-cell reading, not a
  stone-specific one, and that clearing only
  the staged blocks lets the same squad walk
  on at radius 3 and count 5. Step 1 must
  ask the shape version of that question:
  does any leg of the current shape refuse
  where a differently shaped leg through the
  same ground would not? The diagonal legs
  are the suspect, because they stair-step
  through six cells where a cardinal leg
  steps through three, and a sidestep on a
  diagonal leg can move the squad onto a
  cell the next leg's stair-step would not
  have visited. The audit must stage the
  same block against a diagonal leg and
  against a cardinal leg and say whether
  the shape, or only the blocked cell,
  explains the refusal — including a block
  on a stair-step cell that is not itself a
  waypoint, which Slice 23's waypoint-block
  cases do not cover. If no staged case
  makes shape the walking-versus-sticking
  difference, that is evidence for keep,
  not an absence of evidence, provided the
  audit names the cases it staged.
- Shape-caused fire asymmetry. From the
  ground facts above, waypoint-to-waypoint
  fire is impossible for every kind, and
  rally-to-waypoint fire splits the kinds
  (Vanguard exactly, Recon within, Heavy
  beyond). Step 1 must ask whether that
  split is a property of the radius, of the
  shape, or of both: would a differently
  shaped loop with the same radius and the
  same count move a waypoint closer to the
  rally, or closer to another waypoint, in
  Manhattan distance, and would that move
  change which kind engages what? A shape
  that put a waypoint at Manhattan distance
  2 from the rally would let Heavy engage
  it; a shape that put two waypoints at
  Manhattan distance 5 or less would let
  Recon shoot across the loop. Step 1 must
  compute those distances for the current
  shape and for any shape it proposes, and
  must not claim a fire change it has not
  computed — and must not claim the current
  split is a shape virtue if the same split
  follows from radius alone.
- Shape-caused coverage gaps. The current
  loop visits four cardinal points and the
  rally, and its diagonal legs stair-step
  between them, so the ground the patrol
  actually walks is a diamond perimeter,
  not the full square of side 6 around the
  rally and not the disc of radius 3. Step
  1 must name, cell by cell, what the walked
  perimeter covers and what it leaves
  unwalked — in particular the four corner
  cells of that square (rally ± 3 in both X
  and Z), which no leg visits — and must say
  whether that unwalked ground matters: a
  patrol that never visits a corner cannot
  be said to patrol the square, and a shape
  change that visited the corners (a square
  perimeter, for example) would walk a
  longer circuit at the same radius and
  would change the leg lengths, the circuit
  step count, and the waypoint-to-waypoint
  distances, all of which must be computed,
  not gestured at. A climbing shape must
  answer a further question the flat shape
  never faces: which Y a waypoint sits on
  when the rally's Y plane is not the
  ground, and what a step up or down does
  to the one-voxel-per-tick rule.
- Coupling answers, stated before any
  change, not discovered after it. If Step
  1 keeps the shape, the couplings stand as
  they are: radius 3, count 5, stuck
  threshold 5, circuit 24 steps, range
  relation as above, no save field, loop
  re-derived on every re-rally and on load
  from index 0. If Step 1 changes the shape,
  it must answer, each explicitly: does the
  waypoint count change, and if so what
  happens to the stuck threshold that is
  coupled to it; does the circuit step count
  change, and what does Step 2 compose
  against; does any waypoint move in
  Manhattan distance from the rally or from
  another waypoint, and what does that do
  to the kind-by-kind fire relation; does
  the shape need a saved field, and if so
  what happens to old saves that carry no
  such field; and does the re-rally rule
  (centre moves, shape does not) still hold,
  or does a re-rally now also reshape? A
  change that answers none of these is not
  a decision, it is an edit, and Step 1 must
  not push it as one.
- Alternatives, named, not implied. Step 1
  must name at least the live alternatives
  it weighed, even if it rejects them all:
  keep the flat cardinal cross loop; a
  square perimeter through the four corner
  cells; a circular approximation through
  more waypoints (which would change the
  count and therefore the stuck threshold);
  and a climbing loop (which would break
  the flat-Y invariant). Each rejected
  alternative needs a mechanism, not a
  taste: a square perimeter costs a longer
  circuit for the same radius, a circular
  approximation costs a count change with
  its stuck coupling, and a climbing loop
  costs a Y rule the movement code does not
  have. If Step 1 finds a shape-caused
  failure that none of these fixes, it must
  say so and defer to a later slice rather
  than inventing a fifth shape on the spot
  without a proof set.
- Checklist relation, decided in Step 1,
  not left to Step 2. Items 37 (patrol
  route), 39 (stuck re-rally), and 40
  (general re-rally) already observe the
  loop as kept, and item 37 in particular
  observes the route a shape decision
  touches. Step 1 must state whether a keep
  needs no new item (as Slice 23's keep
  needed none), and whether a change needs
  one — a new item only if Step 1 finds a
  player-visible shape reading with no home
  in items 37, 39, or 40, in which case the
  new item takes the next free number, 44,
  and states its relation to the standing
  items, superseding none of them by
  silence. A keep that adds an item anyway,
  or a change that adds none without saying
  why, is a scope deviation Step 2 must
  call out in its review.

## Steps, in order

- Step 1, the loop-shape decision, decided
  in that step: keep the flat cardinal
  cross loop, or change it to a named
  shape, on the audit above, with the
  coupling answers, the rejected
  alternatives with mechanisms, and the
  checklist relation all recorded in
  DECISIONS.md, with the same decision
  summarised in CHANGELOG. No demo
  scenario belongs to Step 1 alone, per the
  Slice 23 precedent: the composed demo
  belongs to Step 2, where the decided
  shape is staged together with its
  controls. If Step 1 keeps the shape, no
  core file needs to move except a comment
  correction Step 1 justifies explicitly,
  as Slice 23 Step 1 corrected the radius
  comment; if Step 1 changes the shape, the
  core change is confined to the route
  derivation and its tests, with no
  movement, targeting, fire, economy,
  marker, material, light, fog, model, or
  HUD rule moved except what the shape
  change itself requires and proves.
- Step 2, the composed demo plus the
  documentation-only review closing Slice
  25, per this scope and following the
  Slice 16 precedent for claims no
  headless run can prove: one composed
  field staging the decided shape together
  with its controls (a walking patrol on
  the decided loop, a stuck patrol whose
  stuck reading is step-cell-caused and not
  shape-caused, a re-rallied patrol whose
  new loop is the decided shape re-derived
  from the new rally, and the kind-by-kind
  fire relation recomputed for the decided
  shape), with a proof set mirroring the
  demo tick by tick, and a review checking
  this scope step by step, separating
  headless-proved claims from the Windows
  observations that alone can close items
  37, 39, 40, and any new item Step 1 adds.
  Slice 26 is NOT scoped here; it is scoped
  afterwards, chosen from the ranked
  candidates the review carries.

## Verification

- Adapter code is not compiled headlessly,
  so a scope step's proof is a code reading
  and a suite run, recorded here: every
  ground fact above was read in
  `Orbital.cs` this run (waypoint switch,
  radius and count constants, flat-Y
  comment, circuit step count, Manhattan
  fire rule, sidestep and skip rule, stuck
  threshold, re-rally re-derivation, load
  index reset), every candidate disposition
  was checked against the Slice 24 review's
  ranked list, and the standing suite and
  demo were re-run before any edit. A scope
  step that moves a test count or a demo
  save length must say why; this scope
  moves neither, because a document decides
  no route and proves no key.
- Standing suite and demo re-run unchanged
  in this scoping run: 4577/4577 tests,
  Slice 23 demo save 44,041 chars, every
  earlier save unchanged (Slice 22 43,990,
  Slice 21 44,040, Slice 20 44,041, Slice 19
  43,965, Slice 18 44,015, Slice 15 44,016,
  Slice 17 43,974 chars, and all earlier
  saves as in the 0.95.0 entry). No new
  scenario exists, so no new length is
  quoted. This re-check does not compile
  an adapter, run a scene, press a key, or
  observe a marker — those remain Windows
  work, and no step in Slice 25 claims
  otherwise.

## Slice 25 gate (what "done" means)

- Every step pushed with passing tests —
  for this slice, with the suite standing
  at 4577/4577 through the scope unless a
  step justifies a rise with a headless
  claim it names, and never falling; the
  Step 2 review pushed with the suite
  standing, as a documentation-only step.
- The Step 1 shape decision recorded with
  rationale in DECISIONS.md: the
  shape-caused stuck audit, the fire
  asymmetry computation, the coverage
  account, the coupling answers, the
  rejected alternatives with mechanisms,
  and the checklist relation, each stated,
  not implied.
- No radius, count, movement, targeting,
  fire, economy, marker, material, light,
  fog, model, or HUD rule changed in any
  step except a shape-necessity change a
  step justifies explicitly and proves
  against the standing proof sets; no demo
  scenario added except Step 2's composed
  scenario; every earlier demo save length
  unchanged, with Slice 23 quoted at
  44,041 chars.
- No checklist item marked passed from
  headless work: items 37, 39, and 40
  observe the decided shape, not pass it,
  and any new item (44, if Step 1 finds a
  reading with no home) states its relation
  to the standing items. The next free gate
  number after this slice is 44, or 45 if
  item 44 is taken.
- The Step 2 review records the verdict
  honestly, separating headless-proved
  claims from the Windows-checklist
  observations, including that Slice 25
  is still not playtested and not
  Unity-verified. The Slice 1 director
  playtest gate is still pending
  throughout and is not closed by Slice 25.

## Deliberately NOT in Slice 25

- A radius or waypoint-count re-decision.
  Radius 3 and count 5 are carried from
  Slice 23, not re-decided headlessly, and
  a shape step that quietly widens or
  renumbers the loop has smuggled a radius
  or count change inside a shape decision.
- A movement, targeting, or fire rule
  change. MoveTowardPosition's axis choice
  and tie-break, MoveForPatrol's sidestep,
  skip, and stuck threshold, and
  FindTarget's Manhattan range rule stand;
  no pathfinder, no wider sidestep, no
  hold replacing a skip, and no range
  retune is added here, except what a
  named shape change itself requires and
  proves in Step 1.
- A re-rally repricing, a dismiss refund
  of any size, a deploy price or cap
  change, or any other economy change. The
  deploy price (10) and the cap (3) stand,
  the re-rally stays free, and the dismiss
  stays refund none with one slot freed.
- A reserve, bench, or redeploy-from-
  reserve system of any kind. No step in
  this slice adds a stored-squad list, a
  bench cap, or a redeploy price quietly
  afterwards, under any name.
- A general squad-command system, a
  material, shader, marker, light, fog, or
  view model change, a HUD redesign, an
  adapter edit, or a binding implementation.
  The K and X bindings stay implemented as
  Slice 24 left them, and no headless test
  proves a binding.
- A scene file (.unity), an imported asset,
  a texture, or a shader file written by a
  headless run and claimed as a built
  scene. Scene assembly remains Windows
  checklist work.
- A copied asset, model, skyline, HUD
  layout, or trademarked look from any
  other game.
- Windows Unity verification itself, the
  director playtest, or a Slice 26 scope.

## Ranked candidates for Slice 26 scoping (not promised)

These are recorded here so the Slice 25
review starts from a list, not from
scratch, in the order the Slice 24 review
carried them, with the shape decision taken:

1. Any Slice 13 remainder that Slice 25
   does not itself settle as an
   observation (the vertex-colour material
   observation, the MeshRenderer wiring
   observation, and the marker-occlusion
   observation, plus the dusk and view
   model observations in items 33 and 34,
   plus the tint, count, and patrol
   observations in items 35, 36, and 37,
   plus items 39, 40, 41, 42, and 43, plus
   any new item a later step adds) — if
   those observations are still unmade
   when the review is written, they remain
   observations to be made when Windows
   verification runs, not steps to be
   re-scoped blindly, and not promised
   here. Items 39, 40, 41, 42, and 43 leave
   this candidate's "unmade" set only when
   their Windows observations are actually
   made, not when their bindings were
   implemented or their shape was decided.
2. First-pass graphics values retuned only
   on Windows evidence (dusk azimuth, fog
   contrast, view model offset, static
   weapon-agnostic model) — a retuning
   step is justified by an observation,
   not by a headless re-decision, and is
   not promised here.
3. A general re-rally price on Windows
   playtest evidence, if free
   repositioning proves abusive kiting —
   a later economy decision, recorded as
   a gap in Slice 21 Step 1, not set here.
4. A dismiss refund on Windows playtest
   evidence, if slot-only removal proves
   punishing — a later economy decision,
   recorded as a gap in Slice 22 Step 1,
   not set here.
5. The Slice 24 adapter gaps, weighed as
   their own step only on the evidence
   Slice 25 leaves standing — the
   identical-twin re-validation limit, the
   invisible pending dismiss, the
   unparented re-rally pending marker,
   same-frame K+X branch order, and
   pending state not auto-clearing on a
   target death — carried, not promised
   here, and not to be confused with a
   shape, economy, or graphics decision:
   a display or routing step for these
   gaps must say which gap it closes and
   what Windows observation would prove
   it, rather than bundling all five into
   one step.
