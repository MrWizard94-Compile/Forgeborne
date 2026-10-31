# Forgeborne — Slice 23 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 4577/4577.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, 17, 18, 19, 20, 21, and 22. This review closes Slice 23 as a reviewed
whole; it does not claim Slice 23 is playtested, Unity-verified,
or a visual result.**

## Review question

Does Slice 23 settle the patrol route's first-pass values — a
loop radius of 3 and a loop of 5 waypoints, carried forward
unchallenged through Slices 15, 18, 20, 21, and 22 — as its own
decision with its own audit tests, then proved together with
its controls in one demo, without a movement, targeting, fire,
delivery, precedence, conflict, rating, cost, or phase rule
change beyond what Step 1 justifies explicitly, a price or cap
change, a radius or waypoint change smuggled inside the demo,
a material, shader, marker, light, fog, or view model change,
a HUD redesign, a new block, kind, doctrine, source, or
consumer, a general squad-command system, a reserve or bench
system, a balance pass, or a claim of Unity verification made
from a headless run? The answer headlessly is yes for the keep
as decided, with the split this review keeps throughout: the
values are tested, the scene observation of the values is
still owed on Windows, and the one staged enclosure in the
demo is a classification proof, not a new system.

## What Slice 23 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.93.0 scope | `docs/slice-23-scope.md`: Slice 23 scoped as the patrol-route first-pass-values slice, in order — the radius and waypoint decision first, then demo + review. Slice 24 is not scoped there. Ranked Slice 24 candidates recorded, not promised | Decision document only, no system built by it | Tests 4109/4109 unchanged |
| 0.94.0 Step 1 | Radius and waypoint decision decided: KEEP radius 3 and waypoint count 5 as one pair — no change, no split, no per-kind radius. The radius comment's range claim corrected to the table: Vanguard 3 equals the radius, Recon 5 sits above it, Heavy 2 below it. Circuit cost stated: 24 steps at radius 3, plus 8 per added unit. Stuck threshold stays coupled to the kept count (5 refusals = a full circuit = Stuck). Windows reopen condition stated | `Orbital.cs` radius comment corrected only, no behaviour; audit note in `docs/orbital-command.md` | Tests 4287/4287 (was 4109; +178, including the range relation, the 24-step circuit, the shared loop across kinds, the tick-by-tick classification of every proved stuck geometry, the removal proofs, the waypoint-caused controls, and the radius-only attempts that fail to stick); no new playtest-gate item; demo unchanged, Slice 22 save 43,990 chars, Slice 21 save 44,040 chars, Slice 20 save 44,041 chars, Slice 19 save 43,965 chars, Slice 18 save 44,015 chars, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.95.0 Step 2 | This review, plus a new composed demo scenario staging the decided values together with their controls in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 4577/4577 (was 4287; +290 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 23 scenario save 44,041 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 2 demo, in numbers

One Simulation, three paid squads, no enemy, no pocket seeded
beyond the 30 EmberOre the three deploys spend — the demo
proves the kept loop, its stuck classification, its rescue,
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
- Phase 3, the kept loop walked in full: over 24 ticks the
  Recon Patrol walks rally (20,5,20) to east (23,5,20) in 3
  steps, to south (20,5,23) in 6 more, to west (17,5,20) in
  6 more, to north (20,5,17) in 6 more, and back to its
  rally in 3 more, waypoint index back to 0, refusals 0,
  reading Patrolling throughout — the circuit Step 1
  audited, now walked in composition beside a holding
  squad, at radius 3 and count 5, with no retune. The range
  relation is printed beside the deploy from the table, not
  from the corrected comment: Vanguard 3, Recon 5, Heavy 2.
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
  east waypoint (25,5,16), the new rally plus radius 3,
  while the walking Patrol steps to (18,5,22) on its own
  loop beside it.
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
asked for the decided values together with their controls in
one field — a patrol walking a loop at the decided radius
and count, a stuck geometry from the audit beside it, and a
save/load round-trip preserving the loop's centre. A
HoldGround control is included because a loop walked beside
nothing proves less than a loop walked beside a squad that
is not looping; a second stuck geometry is not included
because Step 1 already classified the enclosed and
blocked-new-loop shapes, and re-staging them here would
repeat the audit, not compose it. No enemy is staged because
a fire claim would need a target, a range, and a kill, and
Step 1 already proved the range relation from the table;
staging an enemy on the east waypoint to "show" the range
would have turned a loop demo into a fire demo and risked
the patrol's path on a kill. The demo therefore proves the
keep — the same loop, the same stuck reading, the same
rescue — with the audit's classification visible, and
re-claims nothing as a new system.

## Claim split, honestly kept

Headless-tested, in this slice: radius 3 and count 5 walk a
24-step circuit with no refusals in open ground; every
proved stuck geometry traces to blocked step cells, and
clearing those cells lets the same squad walk the same loop
at the same values; a bad waypoint is a skip, never a stick;
the kept loop persists through a save/load round-trip with
its centre kept and its indices reset; the free stuck
re-rally and the full-cap refusal behave exactly as Slices
20 and 22 decided. Still owed on Windows, not proved here:
whether a loop at radius 3 routinely fits ordinary ground,
whether waypoints routinely land inside structures a player
did not place, and whether a 24-step circuit feels like a
patrol in play — the reopen condition Step 1 stated, which
no headless run can meet. The Slice 1 director playtest gate
is still pending throughout and is not closed by Slice 23.

## Checklist relation

No new playtest-gate item, per the scope and per Step 1: no
player-visible behaviour changes in this slice, so items 37
(patrol route), 39 (stuck re-rally), and 40 (general
re-rally) already observe the loop as kept, superseding none
of them, and the next free gate number remains 42. Items 39,
40, and 41 remain honestly pending their bindings: the K
and X proposals are comments in `FpsInputAdapter.cs` only,
not implemented, so those observations cannot even be
attempted until a later adapter step implements them.

## Deliberately NOT in Slice 23

- A movement, targeting, or fire rule change. MoveTowardRally,
  MoveForPatrol's sidestep, skip, and stuck threshold, Advance's
  chase, and Defend Core's chase-or-return stand; no
  pathfinder, no wider sidestep, no hold replacing a skip, no
  rally fallback for Advance, and no rally consultation for
  Defend Core is added here.
- A loop-shape change smuggled inside a radius change: the
  loop walked here is the same cross loop at radius 3, not a
  circular, climbing, or otherwise reshaped loop.
- A re-rally repricing, a dismiss refund of any size, a deploy
  price or cap change, or any other economy change. The deploy
  price (10) and the cap (3) stand, the re-rally stays free,
  and the dismiss stays refund none with one slot freed.
- A reserve, bench, or redeploy-from-reserve system of any
  kind. No step in this slice adds a stored-squad list, a bench
  cap, or a redeploy price quietly afterwards, under any name.
- A general squad-command system, a material, shader, marker,
  light, fog, or view model change, or a HUD redesign.
- A new block, kind, doctrine, source, or consumer, a balance
  pass, or a price or cap change disguised as a loop change.
- An adapter edit or a binding implementation. The K and X
  proposals stay comments in `FpsInputAdapter.cs` through this
  slice; the play path for re-rally and dismiss is unchanged,
  and no headless test proves a binding.
- A scene file (.unity), an imported asset, a texture, or a
  shader file written by a headless run and claimed as a built
  scene. Scene assembly remains Windows checklist work.
- A copied asset, model, skyline, HUD layout, or trademarked
  look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–23 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- A Slice 24 scope. Slice 24 is scoped after this review, not
  inside it.

## Ranked candidates for Slice 24 scoping (not promised)

These are recorded here so the Slice 24 scoping starts from
a list, not from scratch, in the order the Slice 23 scope
carried them:

1. Any Slice 13 remainder that Slice 23 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and the
   marker-occlusion observation, plus the dusk and view model
   observations in items 33 and 34, plus the tint, count,
   patrol, re-rally, and dismiss observations in items 35,
   36, 37, 39, 40, and 41, plus any new item a later step
   adds) — if those observations are still unmade when the
   scoping is written, they remain observations to be made
   when Windows verification runs, not steps to be re-scoped
   blindly, and not promised here.
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
5. The adapter bindings implementation (K for re-rally, X for
   dismiss, both currently comments in `FpsInputAdapter.cs`
   only) as its own adapter step, following the Slice 16
   pattern, so that items 39, 40, and 41 become observable on
   Windows at all — a step whose verification is a Windows
   checklist observation, never a headless test count, and
   not promised here.

## Slice 23 gate (what "done" means, and whether it is met)

- Every step pushed with passing tests: scope 4109/4109
  unchanged, Step 1 4287/4287, Step 2 4577/4577 — met.
- The Step 1 radius and waypoint decision recorded with
  rationale in DECISIONS.md, with its checklist relation
  stated (no new item for a keep) — met, and this review
  repeats that relation rather than inventing an item.
- No movement, targeting, fire, delivery, precedence,
  conflict, rating, phase, or save-format rule changed in
  any step except what Step 1 justifies explicitly (a radius
  comment correction, no value change); no material, marker,
  light, fog, model, or HUD rule changed; no economy value
  changed — met.
- Every earlier demo save length unchanged, with the new
  Slice 23 length quoted as its own entry: Slice 23 save
  44,041 chars; Slice 22 save 43,990 chars, Slice 21 save
  44,040 chars, Slice 20 save 44,041 chars, Slice 19 save
  43,965 chars, Slice 18 save 44,015 chars, Slice 15 save
  44,016 chars, Slice 17 save 43,974 chars — met. (The
  Slice 23 and Slice 20 saves sharing a length is a
  coincidence of field size, not a shared field: the two
  scenarios differ in squads, rallies, and stone, and the
  lengths are quoted per scenario, not compared as proof.)
- This review records the verdict honestly, separating
  headless-tested loop claims from the Windows-checklist
  loop observation, including that Slice 23 is still not
  playtested and not Unity-verified. The Slice 1 director
  playtest gate is still pending throughout and is not
  closed by Slice 23 — met, as a statement, not as a gate
  passed.

## Constitution note, stated honestly

PROJECT-POINTER.md still records Constitution SHA
6dcb4bc03a356eb226156ee19c2b3cc1b32e12d7 with a note to
re-validate before claiming adoption complete — that full
adoption audit was NOT run in this step, so adoption
completeness is not claimed here.
