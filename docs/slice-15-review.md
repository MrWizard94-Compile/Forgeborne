# Forgeborne — Slice 15 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 2130/2130.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, and 14. This review closes Slice 15 as a reviewed
whole; it does not claim Slice 15 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 15 close the deferred doctrine Slice 4 and Slice 11
left open — a game with exactly three doctrines, none of
which walks a route, gaining exactly one fourth doctrine
that walks a route derived from the rally alone, proved
distinct from HoldGround (which holds its rally), from
Advance (which hunts its prey), and from Defend Core
(which guards the core), and closed by a composed demo
proving that doctrine walking its route and engaging on
it beside an existing doctrine doing its own job,
through a save/load round-trip, without a second
doctrine, a fourth kind, a price or cap change, a
movement rule change, a pathfinder, a balance pass, a
save field, a marker change, an electrical widening, or
a display-query redesign, and without changing any
existing doctrine's movement, engagement, or snapshot
behaviour, or any existing kind's price, range, or
damage? The answer headlessly is yes for the systems
below, with the gaps and next candidates listed honestly
at the end — and with the route's stated limits carried,
not smoothed over: a refused step skips its waypoint
for that circuit, and a fully enclosed patrol cycles
its index without moving.

## What Slice 15 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.64.0 scope | `docs/slice-15-scope.md`: Slice 15 scoped as the deferred doctrine, in order — one fourth doctrine, decided in its step, then demo + review. Ranked Slice 16 candidates recorded, not promised | Decision document only, no system built by it | Tests 1988/1988 unchanged |
| 0.65.0 fourth doctrine | `TacticalDoctrine.Patrol` = 3, appended only, walking a closed loop derived from the rally alone: rally, then 3 east, 3 south, 3 west, 3 north, repeat (PatrolLoopRadius 3, PatrolWaypointCount 5, flat at rally height). Route derived on every tick, never stored, never saved; live waypoint index starts 0 on deploy and resets 0 on load. At a waypoint the squad turns and continues in the same tick (no dwell); a refused step skips that waypoint for this circuit, no pathfinder. Fire whenever in range, shooting in passing without chasing — Advance's opposite on engagement. All three kinds patrol, price 10 EmberOre and cap 3 unchanged, new activity Patrolling = 5 (its own word, not Marching or Hunting, no hunting target), adapter Alpha7, playtest-gate checklist item 29 and orbital-command checklist item 13 (both Windows-only) | Core only, plus a thin adapter key: no duplicate logic in Unity code, no new save field, no new HUD count, no new marker shape | Tests 2069/2069; demo unchanged, Slice 14 scenario save 43,974 chars |
| 0.66.0 demo scenario + review | This review, plus a composed Slice 15 demo scenario: one Simulation in which a Recon Patrol walks its rally loop and fires in passing at an enemy that meets it on the route, beside a Heavy HoldGround marching to its own rally and holding there, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in checklist items 29 and 13, not in the numbers below | Tests 2130/2130; demo Step 2 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **2130 passed, 0 failed**
  (was 2069; +61 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, and the composed
  tests passed on their first full run, so no test or demo
  fix was needed in this step).
- Headless demo re-run: Slice 1 unchanged (save 43,881
  chars); Slice 2 unchanged (save 43,921 chars); Step 1
  scenario unchanged (save 44,016 chars); Step 3 scenario
  unchanged (save 44,103 chars); Slice 4 scenario unchanged
  (save 44,026 chars); Slice 5 Step 2 scenario unchanged
  (save 43,974 chars); Slice 6 Step 4 scenario unchanged
  (save 44,024 chars); Slice 7 Step 2 scenario unchanged
  (save 44,013 chars); Slice 8 Step 3 scenario unchanged
  (save 43,974 chars); Slice 9 Step 4 scenario unchanged
  (save 43,975 chars); Slice 10 Step 3 scenario unchanged
  (save 43,974 chars); Slice 11 Step 3 scenario unchanged
  (save 44,038 chars); Slice 12 Step 3 scenario unchanged
  (save 43,974 chars); Slice 13 Step 4 scenario unchanged
  (save 44,045 chars); Slice 14 Step 2 scenario unchanged
  (save 43,974 chars). New Step 2 scenario, in full: one
  Simulation, a Recon Patrol deployed paid through the
  picker to rally (20,5,20) and a Heavy HoldGround
  deployed paid through the picker to rally (10,5,10),
  both deploying at the player (16,5,16), 20 EmberOre
  spent and 0 left, with one enemy staged at (26,5,20)
  east of the patrol loop. The patrol reads Patrolling
  and the Heavy reads Marching before any tick. The
  patrol marches to its rally in 8 ticks — (17,16),
  (17,17), (18,17), (18,18), (19,18), (19,19), (20,19),
  (20,20) — one voxel per tick through the shared step
  rule, alternating axes as the gaps shrink, not a
  diagonal and not a straight line, and the Heavy
  marches to its rally in 12 ticks, reading Holding
  from tick 12 onward at (10,5,10). The enemy walks
  west one cell per tick toward the nearest squad or
  the player — (25,20), (24,20), (23,20), (22,20),
  (21,20) — and the patrol fires in passing while
  still walking its route: on tick 5, at (19,18), the
  enemy at (21,20) drops 30 -> 18 HP (one Recon shot
  of 12, in range 5); on tick 6, at (19,19), the enemy
  at (20,20) — standing on the rally cell itself —
  drops 18 -> 6 HP; on tick 7, at (20,19), the patrol
  fires the third shot and the enemy dies, and the
  patrol is still on route, stepping toward its rally
  rather than onto the enemy's cell or after a prey.
  The snapshot through the encounter reads Patrolling
  with no hunting target, even while firing, because
  the patrol chases nothing. From its rally the patrol
  walks the loop with no dwell at any waypoint: east
  waypoint (23,20) on tick 11, turning west toward
  the south waypoint on tick 12 (the turn and the
  next step happen in the same tick); south waypoint
  (20,23) on tick 17; west waypoint (17,20) on tick
  23; north waypoint (20,17) on tick 29; and back to
  its rally (20,20) on tick 32 — a full 24-tick
  circuit from rally to rally, so the route repeats,
  it does not just walk once. The Heavy holds its
  rally throughout, at full 100 HP, having taken no
  damage on its march or its hold. Save / load: the
  patrol loads at its rally (20,20) with its waypoint
  index reset to 0, the Heavy loads at (10,10) still
  reading Holding, and the kill stays dead. The reset
  is proved by discrimination, not by assertion: on
  loaded tick 1 the patrol turns east to (21,20),
  waypoint 1, where a persisted waypoint 4 would
  have stepped north toward (20,17) instead; it
  reaches the east waypoint again on loaded tick 3
  and is still walking the loop, reading Patrolling,
  on loaded tick 8 at (20,22). Save JSON 44,016 chars
  — a composition fingerprint for this scenario only,
  not a balance number, and the same length as the
  Step 1 scenario's save by coincidence of format,
  not the same save.
- Slice 15 gate, checked against `docs/slice-15-scope.md`:
  every step pushed with passing tests; the Step 1
  doctrine's route (rally loop, radius 3, five
  waypoints, in order), route storage and save answer
  (derived, never stored, never saved, index live
  only, reset on load), endpoint behaviour (turn and
  continue, no dwell), movement reuse and blocked-step
  answer (shared one-voxel rule, skip on refusal, no
  pathfinder), engagement and return-to-route rules
  (fire whenever in range, shoot in passing, no chase
  so no return is owed), kinds served (all three,
  proved in Step 1), price and cap (10 EmberOre, cap
  3, unchanged), display activity (Patrolling, its
  own word, no hunting target), and adapter line
  (Alpha7, gate item 29, orbital-command item 13)
  are each decided and tested, with no existing
  doctrine's movement, engagement, or snapshot
  behaviour changed, no existing kind re-priced or
  re-ranged, the ladder still LV / HV / None, and
  every earlier demo save length unchanged; the
  composed demo proves the new doctrine walking its
  route and engaging on it beside an existing
  doctrine through a save/load round-trip, with the
  engagement proved over three ticks rather than a
  single killing shot, and with the loop proved to
  repeat (rally on tick 8 and again on tick 32); and
  this review records honestly that Slice 15 is still
  not playtested, balanced, or Unity-verified. The
  Slice 1 director playtest gate is still pending
  throughout and is not closed by Slice 15.
- Two staging facts this step pins, so they are not
  over-claimed later. First, the demo stages only the
  enemy directly: both squads deploy through the
  picker, as in Slice 13, and no electrical layout is
  staged, because this demo claims orbital
  composition, not wiring — a reader who quotes it
  as a power proof is misquoting it. Second, the
  Heavy's role in the demo is a holding contrast, not
  a fire proof: it marches, holds, and reads Holding,
  but no enemy ever comes within its range 2 while
  it holds, so the demo does not prove Heavy fire —
  that rule rests on the earlier slices' tests, and
  the demo's silence on it is a scope limit, not a
  gap in the doctrine. Likewise the patrol's blocked
  step is not exercised in composition here: the
  skip-on-block rule rests on Step 1's tests, and a
  demo that staged a wall on the loop would prove
  the same rule a second time, not a new one.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (loop shape and
  repeat, no-dwell turns, three-tick passing fire
  without chase, Patrolling with no hunting target,
  paid deploy through the picker, Heavy march and
  hold, save/load index reset proved by the eastward
  turn) and the snapshot words, but it does not prove
  that pressing Alpha7 selects Patrol in the scene,
  that a Recon marker reads as a capsule at FPS
  distance, or that any marker reads through
  occlusion — a printed snapshot line does not prove
  a visible marker. Those are adapter claims with
  Windows checklist items (29 for the Patrol key,
  13 in the orbital-command checklist for the same
  key, 25 for the markers), and this review adds no
  new checklist item for the demo itself, because
  the demo exercises no adapter binding the checklist
  does not already cover.
- No Slice 15 step duplicates logic in a Unity adapter.
  Core owns the doctrine, the route derivation, the
  fire rule, the activity word, and the save bytes;
  the adapter only feeds the Alpha7 key press into
  the existing picker. The route cannot be compiled
  headlessly as adapter code — it is core code,
  tested, and the adapter line is written and
  checklist-listed, not a test-count claim.
- No Slice 15 step changed a saved value's meaning: the save
  format gains no field, doctrine values through Defend
  Core = 2 are unchanged with Patrol appended as 3, and
  every earlier demo save length above is unchanged. The
  new scenario's save length (44,016 chars) is a
  composition fingerprint for this scenario only, not
  a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (movement, fire, and the enemy's approach
  are per-tick phases with no planning pauses), no
  twin-stick Asteroids loop, no copied assets, lore,
  code, or trademarks. Patrol is an original route
  rule, not a copied mechanic; the blueprint's
  doctrine vocabulary was taken as names only and
  given tested meanings the blueprint never stated.

## Deliberately NOT in Slice 15 (still open, not forgotten)

- A second doctrine, a fifth squad kind, or any other
  orbital widening added alongside Patrol as a bonus.
  One doctrine is Slice 15; a doctrine family would
  have made the route proof untestable in isolation.
- A display-query redesign of the single-cable tint or
  the direct-Conduit count. Either is a candidate
  with its own tests, not a patch inside a doctrine
  step: a doctrine step must not move the displays
  it is proved against, and a count or tint step
  must not move the doctrine it displays.
- A scene-proof observation (vertex-colour material,
  MeshRenderer wiring, marker occlusion) presented as
  a headless step. Those are Windows observations
  inside checklist items 25, 26, and 27, carried as
  candidates, not closed by a document — and, per the
  director queue decision recorded below, they are
  now addressed by the graphics slice that follows
  Slice 15, not by a Slice 16 display step first.
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
- A balance pass. The route radius 3, the flat loop,
  and the fire rule are first-pass values chosen for
  determinism and testability, exactly as Defend
  Core's chase radius 8 was, and must be labelled as
  such, not quoted as final feel. In particular, a
  Recon patrol's range 5 reaches across its own
  radius-3 loop, so it can shoot across the loop
  where a Vanguard or Heavy shoots what the loop
  brings close — a first-pass asymmetry, not a
  tuned tradeoff, and a playtest may retune it.
- A new movement mode, a weapon rework, a camera, a
  mini-map, or a free-deploy enforcement change. The
  free `DeploySquad` primitive remains a headless-only
  rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- A new save field. Step 1's route decision
  justifies none, and the demo's index reset proves
  the no-field answer in composition: a saved patrol
  loses circuit position on load, and that loss is
  the stated cost of the choice, not a bug.
- Windows Unity verification itself. It remains open
  for Slices 1–15 alike and is blocked on Unity 6000
  on Windows, not on anything this slice can do
  headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–15.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 15 adds item 29 (Patrol key, Windows-only) and
   orbital-command item 13 to that plan. None of these
   items is claimed verified by any test count in this
   review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 15 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Doctrine balance is unproven, not just unverified.**
   The loop radius 3 and the fire-in-passing rule are
   first-pass values for determinism and testability. A
   Windows playtest may find a radius-3 loop too tight
   to feel like a patrol, or a Recon patrol too strong
   across its own loop — and that observation, not this
   review, justifies retuning, with its own changelog
   entry. Both directions are open until played, and
   neither is decided here.
4. **The scene proof for the visibility adapters is still
   owed in detail.** Even though the adapter code is written,
   three observations are still open and are not closed by
   this review: whether the scene's chunk material actually
   multiplies vertex colours (item 26), whether
   `FpsInputAdapter.MeshRenderer` is wired to the same
   renderer GameManager links or silently no-ops on refresh
   (item 27), and whether markers read through the scene's
   occlusion at FPS distance (item 25). If any observation
   fails, that failure is a fix step, recorded as a Slice
   13 remainder — and, per the queue decision below, the
   graphics slice that follows Slice 15 is where that
   proof layer is built, not a later display step.
5. **The single-cable tint finding is still open.**
   Carried from Slice 14: a single cable cell touching a
   Conduit reads None in PowerState (untinted) while its
   network is live — its pylon fires, its mill is destroyed
   — because `DeliveredVoltage` for a cell counts only its
   neighbours and a Conduit is not a direct source, while
   a two-cable Conduit run reads Energised on both cells.
   The question is whether the power-state query for a
   cable cell should also count its own component's voltage,
   which would change tint behaviour for every existing
   source's cables as well and must therefore be decided
   as its own step, with its own tests, not patched inside
   a demo. Until then, checklist item 28 asks the scene
   observer to record the untinted cell honestly.
6. **The direct-Conduit count invisibility is still open.**
   Carried from Slice 14: a Conduit standing directly
   beside a mill counts as neither a live network nor a
   lone source, because the Slice 12 lone rule
   disqualifies any source beside a consumer, whatever it
   delivers — so a player who places a Conduit beside a
   mill sees POWER 0 LIVE, 0 LONE, while the mill reads
   None and gathers kinetically. That is the Slice 12
   definition applied consistently, not a miscount, and
   this review does not call it a bug: the count answers
   "placed sources with no wiring", where wiring includes
   consumer adjacency. But it is a genuine visibility
   question — whether the counts should distinguish a
   source beside a consumer that delivers nothing from one
   that delivers, or whether the mill's None state and
   kinetic cadence are sufficient readout — and it must be
   decided with its own tests, not by redefining a count
   inside a review.
7. **The patrol route's stated limits are still open.**
   Carried from Step 1, not found by this demo: a refused
   step skips its waypoint for that circuit, with no
   detour, and a squad whose every waypoint is blocked
   cycles its index without moving — stationary, like
   HoldGround holding short, while still reading
   Patrolling. A later step may add a detour, a hold,
   or a stuck readout, but only as its own decision
   with its own tests, not as a silent change to the
   route this review closes.
8. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the deferred
doctrine from Slice 4 and Slice 11 (its gap 7 in the
Slice 14 review's numbering, carried as Slice 15's work
by the 0.64.0 scope). Step 1 added the fourth doctrine
with a route definition, endpoint and blocked-step
answers, an engagement rule, kinds, price, display
word, and adapter line, and Step 2 proved it in
composition, so Patrol is no longer deferred — it is
built, tested, and reviewed as a doctrine. It is
recorded here as closed in that sense, not left in the
numbered gaps above, so a later review does not carry
it again by accident. The route limits in gap 7 above
are remainders about how Patrol behaves at a wall, not
a reopening of whether a fourth doctrine exists.

## Slice 15 verdict (headless)

Slice 15 is **reviewed and closed as a tested fourth doctrine
plus a composed demo (one rally-derived loop — rally, 3
east, 3 south, 3 west, 3 north, repeat, flat at rally
height, no dwell at waypoints, skip on a refused step,
fire whenever in range without chasing, Patrolling as
its own activity word with no hunting target, all three
kinds, 10 EmberOre, cap 3, Alpha7 key — proved together
in one composed demo through a save/load round-trip,
with a Recon patrol firing three 12-damage passing shots
over ticks 5–7 while still walking its route, a Heavy
HoldGround marching to its rally in 12 ticks and
holding there at full health, a full 24-tick circuit
from rally on tick 8 back to rally on tick 32, and a
save/load index reset proved by the loaded patrol
turning east on its first tick where a persisted
waypoint would have stepped north)**. It is not
playtested, not balanced, and not Unity-verified, and
its adapter claims must not be quoted as if the test
count covered them.
**Slice 16 is not scoped by this review** — but see the
queue decision below, which the director made before
this review and which this review records, as directed.

## Director queue decision recorded here (2026-10-30, Rob, in chat, 09:27 EDT)

**Graphics is the next slice after Slice 15 closes.**
This decision supersedes, for ordering only, the
previously ranked Slice 16 candidates (the Slice 14
remainders in gaps 5 and 6 above, and the Slice 13
scene-proof remainders in gap 4 above): those remainders
are carried after graphics, unless a later director
decision reorders them again. Graphics, as directed, is
the scene-proof layer: vertex-colour URP material and
MeshRenderer wiring, marker occlusion, lighting / sky /
fog to `docs/art-direction.md`, and a first FPS weapon
view model script — adapter-only where Unity is
concerned, with Windows checklist items, following the
Slice 13 pattern. It is recorded here and in
DECISIONS.md so it is visible in the repo, not only in
a schedule body. This review does not scope the graphics
slice itself: its own scope document is the next run's
decision step, the same way this document follows the
Slice 14 review's candidates rather than preceding them.

## Ranked candidates carried after graphics (starting point, not promised)

Carried from the 0.64.0 scope, in the same order, with
the queue decision above applied to their ordering —
graphics first, then these, unless the director reorders
them again:

1. Any Slice 14 remainder this review names (the
   single-cable tint query question in gap 5, and the
   direct-Conduit count question in gap 6) — recorded at
   review time, not promised here. Either would be a
   display-query decision step with its own tests, not a
   patch inside another step.
2. Any Slice 13 remainder this review names (the
   vertex-colour material observation, the MeshRenderer
   wiring observation, and the marker-occlusion
   observation in gap 4 above) — recorded at review time,
   not promised here, and partly addressed by the
   graphics slice's proof layer before these are taken
   as steps of their own.
