# Forgeborne — Slice 25 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 5053/5053.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, 20, 21, 22, 23, and 24. This review closes Slice 25
as a reviewed whole; it does not claim Slice 25 is playtested,
Unity-verified, or a visual result.**

## Review question

Does Slice 25 settle the patrol loop's SHAPE — kept separate
from its radius and waypoint count, which Slice 23 already
decided — as its own decision with its own audit tests, then
proved together with its controls in one demo, without a
radius or waypoint-count re-decision, a movement, targeting,
or fire rule change, an economy change, an adapter or marker
change, a material, shader, light, fog, or view model change,
a HUD redesign, a new block, kind, doctrine, source, or
consumer, a general squad-command system, a reserve or bench
system, a balance pass, or a claim of Unity verification made
from a headless run? The answer headlessly is yes for the keep
as decided, with the split this review keeps throughout: the
shape is tested, the scene observation of the shape is still
owed on Windows, and the one staged enclosure in the demo is
a classification proof, not a new system.

## What Slice 25 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.100.0 scope | `docs/slice-25-scope.md`: Slice 25 scoped as the loop-shape slice, in order — the loop-shape decision first, then demo + review. Slice 26 is not scoped there. Ranked Slice 26 candidates recorded, not promised | Decision document only, no system built by it | Tests 4577/4577 unchanged |
| 0.101.0 Step 1 | Loop-shape decision decided: KEEP the flat cardinal cross loop — rally, then east, south, west, and north at radius 3, then back to the rally, all on the rally's Y plane — on a shape-caused stuck audit, a Manhattan fire-asymmetry computation, a coverage account naming the unwalked corner cells, coupling answers, and rejected alternatives with mechanisms. No core file moves; no new playtest-gate item | `src/Forgeborne.Core/Orbital.cs` comment already accurate, so no behaviour change; audit note in `docs/orbital-command.md` | Tests 4722/4722 (was 4577; +145, including the diagonal sidestep versus cardinal skip contrast, the both-blocked one-skip case, the enclosed shape-independence control, the fire split and its square and climbing rejections, and the re-rally re-derivation); demo unchanged, Slice 23 save 44,041 chars |
| 0.102.0 Step 2 | This review, plus a new composed demo scenario staging the decided shape together with its controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 5053/5053 (was 4722; +331 composed-integration assertions mirroring the demo, plus the coverage and fire recomputation); every earlier demo save length unchanged; new Slice 25 scenario save 44,041 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, three paid squads, no enemy, no pocket seeded
beyond the 30 EmberOre the three deploys spend — the demo
proves the kept shape, its stuck classification, its rescue,
and its persistence, and nothing else:

- Phase 1, the second-doctrine control marched away first:
  Heavy HoldGround, rally (10,5,10), deploys at the player
  (16,5,16), ore 30 to 20, and marches (15,16), (15,15),
  (14,15), (14,14), (13,14), (13,13) over six ticks, still
  reading Marching. It is deployed first so it is not
  standing on the player cell when the stuck patrol deploys
  there, and so its later Holding reading is a control, not
  an accident of spawn.
- Phase 2, the walking control: Recon Patrol, rally
  (20,5,20), deploys at the player (16,5,16), ore 20 to 10,
  and marches (17,16), (17,17), (18,17), (18,18), (19,18),
  (19,19), (20,19), (20,20) over eight ticks, reaching its
  rally reading Patrolling, while the HoldGround reaches
  (10,5,10) on tick 6 of this phase and reads Holding from
  then on.
- Phase 3, the kept shape walked in full, with its coverage
  recorded: over 24 ticks the Recon Patrol walks rally
  (20,5,20) to east (23,5,20) in 3 steps, to south (20,5,23)
  in 6 more, to west (17,5,20) in 6 more, to north (20,5,17)
  in 6 more, and back to its rally in 3 more, waypoint index
  back to 0, refusals 0, reading Patrolling throughout, every
  step on Y 5 — the shape Step 1 decided, now walked in
  composition beside a holding squad, at radius 3 and count
  5, with no retune. The walked ground is recorded in the
  demo line, not hidden: a diamond perimeter of 22 distinct
  cells, with the four square corners (17,17), (23,17),
  (17,23), (23,23) never visited. The fire relation is
  recomputed beside the loop from the table and from
  Manhattan distance, not staged as a fire demo: rally to
  any waypoint 3, waypoint to waypoint 6, so Vanguard 3
  engages a waypoint at exactly 3, Recon 5 within 5, Heavy
  2 not beyond 2, and no kind shoots across the loop — a
  radius property, stated as such, not a shape virtue.
- Phase 4, the Step 1 case in one field: Vanguard Patrol,
  rally (12,5,16), deploys at the player (16,5,16), ore 10
  to 0, cap now full at 3 live squads. The route-blocked
  shape is staged after this deploy, as in Slice 18: stone
  at (15,5,16) west, (16,5,17) south, and (16,5,15) north
  of the squad's cell, with the east step cell (17,5,16)
  left open — the exact Step 1 geometry translated to the
  player cell, so the proof is the same shape, not a new
  one. Over five ticks the stuck patrol counts refusals 1
  through 5, its waypoint cycling 1, 2, 3, 4, then back to
  0, reading Patrolling through four refusals and Stuck at
  five, unmoved at (16,5,16), while the HoldGround reads
  Holding at (10,5,10) and the walking Patrol walks its
  loop east to (23,5,20) and turns south to (22,5,21)
  beside it. The classification is stated in the demo
  lines, not left as a shape: every attempted step cell
  in those five ticks is stone, and the east waypoint
  (15,5,16) — the rally plus radius 3 — is the same
  blocked cell counted once, as a blocked cell, not as a
  loop shape.
- Phase 5, the rescue and the cap, unchanged from Slices
  20 and 22: the stuck patrol accepts a free re-rally to
  (22,5,16), waypoint and counter reset to 0, position
  unchanged, ore still 0, cap still 3; a fourth paid
  deploy at the full cap is refused with nothing changed;
  the re-rallied patrol marches east to its new rally in
  six ticks and turns onto the new loop on the next tick,
  east waypoint (25,5,16), the new rally plus radius 3 —
  the same flat cardinal cross shape re-derived from the
  new rally, not a reshaped loop — while the walking Patrol
  steps to (18,5,22) on its own loop beside it.
- Phase 6, persistence: the field saved after the action
  loads with all three squads, the walking patrol keeping
  rally (20,5,20) and the re-rallied patrol keeping its
  new rally (22,5,16) — it does not revert to (12,5,16) —
  all patrol indices reset to 0, ore still 0, healths kept
  (HoldGround 100, walking Recon 40, rescued Vanguard 90);
  on loaded tick 1 the HoldGround stays Holding at
  (10,5,10), the walking patrol steps to (19,5,22) toward
  its rally, and the re-rallied patrol steps back to its
  new rally (22,5,16) at index 0, as the index-reset load
  rule requires.

## Composition rationale, stated plainly

Why this composition, and why not a larger one: the scope
asked for the decided shape together with its controls in
one field — a patrol walking a loop on the decided shape, a
stuck geometry whose reading is step-cell-caused and not
shape-caused, a re-rallied patrol whose new loop is the
decided shape re-derived from the new rally, and the
kind-by-kind fire relation recomputed for the decided shape.
A HoldGround control is included because a shape walked
beside nothing proves less than a shape walked beside a
squad that is not looping; a second stuck geometry is not
included because Step 1 already classified the enclosed and
blocked-new-loop shapes, and re-staging them here would
repeat the audit, not compose it. A diagonal sidestep case
is not re-staged for the same reason: Step 1 already proved
a single diagonal stair-step block sidesteps while a
cardinal block skips, and re-staging it beside a stuck
patrol would repeat that audit, not compose the shape with
its controls. No enemy is staged because a fire claim would
need a target, a range, and a kill, and Step 1 already proved
the range relation from the table and from Manhattan
distance; staging an enemy on a waypoint to "show" the range
would have turned a shape demo into a fire demo and risked
the patrol's path on a kill. The fire relation is therefore
recomputed in the demo lines and in the composed tests from
FindTarget, not claimed from a staged kill. The demo
therefore proves the keep — the same shape, the same stuck
reading, the same rescue — with the audit's classification
visible, and re-claims nothing as a new system.

## Scope check, step by step

- Step 1, the loop-shape decision: the scope asked for a
  shape-caused stuck audit, a fire-asymmetry computation, a
  coverage account, coupling answers, rejected alternatives
  with mechanisms, and a checklist relation, all decided in
  that step. The step's record answers each: no staged case
  makes shape the walking-versus-sticking difference
  (diagonal sidestep, cardinal skip, both-blocked one skip,
  enclosed sticks with no leg shape exercised); fire follows
  from radius and Manhattan distance, not shape alone
  (rally to waypoint 3, waypoint to waypoint 6); coverage is
  named cell by cell (24 steps, 22 distinct cells, diamond
  perimeter, four corners never visited, judged not to
  matter for a patrol route); couplings stand (count 5,
  threshold 5, circuit 24, no save field, re-rally re-derives
  the same shape); square, circular, and climbing
  alternatives are rejected with mechanisms, not taste; and
  no new checklist item, because items 37, 39, and 40 already
  observe the loop as kept. No scope deviation is found in
  Step 1, and this review does not re-decide it.
- Step 2, the composed demo plus review: the scope asked for
  one composed field staging the decided shape with its
  controls, a proof set mirroring the demo tick by tick, and
  a review checking the scope step by step, separating
  headless-proved claims from the Windows observations that
  alone can close items 37, 39, and 40. This step does
  exactly that: the demo stages the walking patrol, the
  step-cell-caused stuck patrol, the re-rallied patrol on
  the re-derived shape, and the recomputed fire relation;
  the composed tests mirror every demo tick (hold march,
  patrol march, 24-step loop with every position, waypoint,
  and Y, plus the coverage set and corner absences, the
  fire recomputation, the stuck-phase refusals and waypoint
  cycle with the walking patrol's loop positions beside
  them, the free re-rally state and economy, the fourth-
  deploy refusal, the six-tick march, the loop turn with
  radius 3 and waypoint count 5 asserted unchanged, the
  post-action save/load and loaded tick 1, and the range
  constants re-asserted from the table); and this review
  keeps the claim split below. No core file moves in Step 2,
  no adapter file moves, no checklist item is added, and
  Slice 26 is NOT scoped here.

## Claim split, honestly kept

Headless-tested, in this slice: the flat cardinal cross loop
walks a 24-step circuit with no refusals in open ground, on
Y 5 throughout, visiting 22 distinct cells and never the
four square corners; every proved stuck geometry traces to
blocked step cells, and the route-blocked shape in
composition sticks for exactly that reason, not for its leg
shape; a re-rally re-derives the same shape from the new
rally, with the new east waypoint at the new rally plus
radius 3; the fire split recomputes as Vanguard exactly,
Recon within, Heavy beyond, and no kind across the loop;
the kept shape persists through a save/load round-trip with
its centres kept and its indices reset; the free stuck
re-rally and the full-cap refusal behave exactly as Slices
20 and 22 decided. Still owed on Windows, not proved here:
whether a loop of this shape routinely fits ordinary ground,
whether waypoints routinely land inside structures a player
did not place, and whether a 24-step diamond circuit feels
like a patrol in play — the reopen condition Step 1 stated,
which no headless run can meet: a Windows observation on a
real map that traces to leg shape rather than to blocked
cells, not a staged enclosure or a player-built wall. The
Slice 1 director playtest gate is still pending throughout
and is not closed by Slice 25.

## Checklist relation

No new playtest-gate item, per the scope and per Step 1: no
player-visible behaviour changes in this slice, so items 37
(patrol route), 39 (stuck re-rally), and 40 (general
re-rally) already observe the loop as kept, superseding none
of them, and the next free gate number remains 44. Items 39,
40, 41, 42, and 43 remain honestly pending their Windows
observations: the K and X bindings are implemented as Slice
24 left them, but no headless test proves a binding, and no
shape decision passes an observation.

## Deliberately NOT in Slice 25

- A radius or waypoint-count re-decision. Radius 3 and count
  5 are carried from Slice 23, not re-decided, and no step
  here widens or renumbers the loop inside a shape decision.
- A movement, targeting, or fire rule change. MoveTowardPosition's
  axis choice and tie-break, MoveForPatrol's sidestep, skip,
  and stuck threshold, and FindTarget's Manhattan range rule
  stand; no pathfinder, no wider sidestep, no hold replacing
  a skip, and no range retune is added here.
- A re-rally repricing, a dismiss refund of any size, a deploy
  price or cap change, or any other economy change. The deploy
  price (10) and the cap (3) stand, the re-rally stays free,
  and the dismiss stays refund none with one slot freed.
- A reserve, bench, or redeploy-from-reserve system of any
  kind. No step in this slice adds a stored-squad list, a
  bench cap, or a redeploy price quietly afterwards, under
  any name.
- A general squad-command system, a material, shader, marker,
  light, fog, or view model change, a HUD redesign, an
  adapter edit, or a binding implementation. The K and X
  bindings stay implemented as Slice 24 left them, and no
  headless test proves a binding.
- A scene file (.unity), an imported asset, a texture, or a
  shader file written by a headless run and claimed as a built
  scene. Scene assembly remains Windows checklist work.
- A copied asset, model, skyline, HUD layout, or trademarked
  look from any other game.
- Windows Unity verification itself, the director playtest,
  or a Slice 26 scope.

## Ranked candidates for Slice 26 scoping (not promised)

These are recorded here so the Slice 26 scoping starts from
a list, not from scratch, in the order the Slice 25 scope
carried them, with the shape decision taken:

1. Any Slice 13 remainder that Slice 25 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and the
   marker-occlusion observation, plus the dusk and view model
   observations in items 33 and 34, plus the tint, count, and
   patrol observations in items 35, 36, and 37, plus items 39,
   40, 41, 42, and 43, plus any new item a later step adds) —
   if those observations are still unmade when the scoping
   is written, they remain observations to be made when
   Windows verification runs, not steps to be re-scoped
   blindly, and not promised here. Items 39, 40, 41, 42, and
   43 leave this candidate's "unmade" set only when their
   Windows observations are actually made, not when their
   bindings were implemented or their shape was decided.
2. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model offset,
   static weapon-agnostic model) — a retuning step is
   justified by an observation, not by a headless
   re-decision, and is not promised here.
3. A general re-rally price on Windows playtest evidence, if
   free repositioning proves abusive kiting — a later economy
   decision, recorded as a gap in Slice 21 Step 1, not set
   here.
4. A dismiss refund on Windows playtest evidence, if
   slot-only removal proves punishing — a later economy
   decision, recorded as a gap in Slice 22 Step 1, not set
   here.
5. The Slice 24 adapter gaps, weighed as their own step only
   on the evidence Slice 25 leaves standing — the
   identical-twin re-validation limit, the invisible pending
   dismiss, the unparented re-rally pending marker,
   same-frame K+X branch order, and pending state not
   auto-clearing on a target death — carried, not promised
   here, and not to be confused with a shape, economy, or
   graphics decision: a display or routing step for these
   gaps must say which gap it closes and what Windows
   observation would prove it, rather than bundling all five
   into one step.

## Slice 25 gate (what "done" means, and whether it is met)

- Every step pushed with passing tests: scope 4577/4577
  unchanged, Step 1 4722/4722, Step 2 5053/5053 — met.
- The Step 1 shape decision recorded with rationale in
  DECISIONS.md, with its checklist relation stated (no new
  item for a keep) — met, and this review repeats that
  relation rather than inventing an item.
- No radius, count, movement, targeting, fire, economy,
  marker, material, light, fog, model, or HUD rule changed
  in any step except what Step 1 justifies explicitly (no
  core change at all in Step 1, and no core change in Step
  2); no material, marker, light, fog, model, or HUD rule
  changed; no economy value changed — met.
- Every earlier demo save length unchanged, with the new
  Slice 25 length quoted as its own entry: Slice 25 save
  44,041 chars; Slice 23 save 44,041 chars, Slice 22 save
  43,990 chars, Slice 21 save 44,040 chars, Slice 20 save
  44,041 chars, Slice 19 save 43,965 chars, Slice 18 save
  44,015 chars, Slice 15 save 44,016 chars, Slice 17 save
  43,974 chars — met. (The Slice 25, Slice 23, and Slice 20
  saves sharing a length is not a shared field and not
  proof by itself: the Slice 25 field at save time carries
  the same three squads, positions, healths, and ore as the
  Slice 23 field by design, because the composition reuses
  that field geometry to stage the decided shape with its
  controls; the new proof in this step is the coverage and
  fire recomputation asserted beside it, not the length.)
- This review records the verdict honestly, separating
  headless-tested shape claims from the Windows-checklist
  shape observation, including that Slice 25 is still not
  playtested and not Unity-verified. The Slice 1 director
  playtest gate is still pending throughout and is not
  closed by Slice 25 — met, as a statement, not as a gate
  passed.

## Constitution note, stated honestly

PROJECT-POINTER.md still records Constitution SHA
6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a note to
re-validate before claiming adoption complete — that full
adoption audit was NOT run in this step, so adoption
completeness is not claimed here.
