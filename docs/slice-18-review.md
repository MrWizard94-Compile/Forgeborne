# Forgeborne — Slice 18 Review

**Status: REVIEWED HEADLESSLY 2026-10-31, tests 2690/2690.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
16, and 17. This review closes Slice 18 as a reviewed whole; it
does not claim Slice 18 is playtested, Unity-verified, or a
visual result.**

## Review question

Does Slice 18 settle the two orbital remainders Slice 17 left
honestly carried and Slice 16 carried before it — what a
Patrol squad does when its route is blocked, and what in
code stops a play path from using the free deploy primitive —
each as its own decision with its own tests, then proved
together in one demo, without a price, cap, radius, waypoint,
fire, targeting, delivery, precedence, conflict, rating, cost,
or phase rule change, a material, shader, marker, light, fog,
or view model change, a HUD redesign, a new doctrine, kind,
block, source, or consumer, a pathfinder, a balance pass, or
a claim of Unity verification made from a headless run? The
answer headlessly is yes for the two remainders as decided,
with the split this review keeps throughout: the route and
boundary behaviours are tested, the scene observation of
the route behaviour is still owed on Windows, and the
boundary behaviour has no scene observation to owe, because
a compile error is not a scene behaviour.

## What Slice 18 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.77.0 scope | `docs/slice-18-scope.md`: Slice 18 scoped as the orbital-remainder slice, in order — patrol route limits first, free deploy primitive enforcement second, then demo + review. Ranked Slice 19 candidates recorded, not promised | Decision document only, no system built by it | Tests 2301/2301 unchanged |
| 0.78.0 Step 1 | Patrol route limits decided: a refused patrol step first tries a distance-reducing sidestep in `MoveForPatrol` only, and only if every gap axis is refused does it skip that waypoint for the circuit; a patrol that refuses, with no sidestep move, on every waypoint of a full circuit reads Stuck, not Patrolling | `MoveForPatrol` sidestep plus skip plus live counter `PatrolConsecutiveRefusals`, new `SquadActivity.Stuck` appended as 6, derived in `OrbitalView.Snapshot`; shared `MoveTowardPosition` untouched, so no other doctrine gains a sidestep | Tests 2433/2433 (was 2301; +132, including single-axis skip, diagonal sidestep for all three kinds, both-axes skip, enemy-blocked skip and sidestep, next-circuit retry and block-clears resumption, fully enclosed threshold and unstuck, fire while stuck for all three kinds, save/load); new playtest-gate item 37; demo Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.79.0 Step 2 | Free deploy primitive enforcement decided: the public free entry points are removed, and headless tests deploy through an internal seam the test assembly alone can reach | `WorldZone.DeploySquad` and `Simulation.DeployOrbitalSquad` deleted; `WorldZone.SpawnSquad` internal, called by the paid path (same assembly) and by the test assembly through `InternalsVisibleTo("Forgeborne.Core.Tests")` in new `src/Forgeborne.Core/AssemblyInfo.cs`; paid path, price, and cap unchanged | Tests 2556/2556 (was 2433; +123, including the paid matrix for all four doctrines by all three kinds, cap through the paid path, reflection proofs that the old public names are absent and the seam is internal, picker Patrol, paid-path patrol identity, and a forbidden-path proof outside the suite); no new checklist item; demo unchanged, Slice 15 save 44,016 chars, Slice 17 save 43,974 chars |
| 0.80.0 Step 3 | This review, plus a new composed demo scenario staging both decided remainders together in one Simulation (see below) | Review document plus CHANGELOG and DECISIONS entries; no core file, no adapter file, no material, marker, light, fog, or view model value, no checklist item added | Tests 2690/2690 (was 2556; +134 composed-integration assertions mirroring the demo); every earlier demo save length unchanged; new Slice 18 scenario save 44,015 chars, quoted as its own entry below, not as a change to any earlier save |

## The Step 3 demo, in numbers

One Simulation, two paid patrols, no enemy, no free deploy
path anywhere in it:

- Recon Patrol, rally (20,5,20), paid through the picker,
  10 EmberOre, ore left 10. It deploys at the player
  (16,5,16) and marches to its rally. A stone block at
  (18,5,17) blocks a diagonal march step, and a stone block
  at (21,5,20) blocks the loop leg east.
- Vanguard Patrol, rally (16,5,16) — the player cell itself
  — paid through the picker, 10 EmberOre, ore left 0. It is
  deployed only after the first patrol has marched away,
  and its enclosure is staged only after it deploys: stone
  at (17,5,16), (15,5,16), (16,5,17), and (16,5,15). Staging
  the enclosure first would trap the first patrol at its
  own spawn and prove nothing about either route, so the
  order is part of the composition, not an accident of it.

Phase 1, ticks 1–10, the first patrol alone:

- Tick 1: (17,5,16). Tick 2: (17,5,17). Tick 3: the
  sidestep — the direct east step to (18,5,17) is solid,
  so the patrol steps south to (17,5,18) instead, keeping
  waypoint 0, counting no refusal, and reducing its
  Manhattan distance to the rally from 6 to 5. Tick 4:
  (18,5,18). Tick 5: (19,5,18). Tick 6: (19,5,19). Tick 7:
  (20,5,19). Tick 8: (20,5,20), the rally, waypoint 0.
- Tick 9: the skip — standing on its rally, the patrol
  turns to the east waypoint (23,5,20), its step east to
  (21,5,20) is solid, there is no second gap axis, so it
  skips to the south waypoint, waypoint 2, unmoved,
  refusal counter 1, still reading Patrolling. Tick 10:
  (20,5,21), walking south, counter reset to 0. The
  sidestep and the skip are therefore both exercised in
  one patrol's first ten ticks, and the counter reset is
  exercised by the very next move — not asserted in
  separate worlds.

Enclosed phase, ticks 1–5 with both patrols in the field:

- The walking patrol continues its loop south and west:
  (20,5,22), (20,5,23) — the south waypoint — (19,5,23),
  (19,5,22), (18,5,22), still reading Patrolling throughout,
  beside the enclosed patrol. A walking patrol and a stuck
  patrol in the same field, reading different words, is
  the composition's point: Stuck is a property of one
  patrol's route, not of the field.
- The enclosed patrol, unmoved at (16,5,16) throughout:
  tick 1 east blocked, waypoint 2, refusals 1, Patrolling;
  tick 2 south blocked, waypoint 3, refusals 2; tick 3
  west blocked, waypoint 4, refusals 3; tick 4 north
  blocked, waypoint 0, refusals 4, still Patrolling —
  four refusals are not stuck, and the demo prints that
  threshold honesty rather than jumping to the word;
  tick 5 east blocked again, waypoint 2, refusals 5,
  reading Stuck, unmoved. Five is the waypoint count
  itself, not a tuned number: one full circuit tried and
  refused in turn.

Save/load round-trip, under Step 1's load rule:

- Both patrols load with waypoint index 0 and refusal
  counter 0, so both read Patrolling on arrival — the
  enclosed patrol is not stuck on arrival, whatever it was
  before the save, because stuck is live progress, not
  saved state. The walking patrol loads at (18,5,22) and
  the enclosed patrol at (16,5,16).
- Loaded ticks 1–5: the walking patrol marches to its
  rally first — (19,5,22), (19,5,21), (20,5,21),
  (20,5,20) — and on loaded tick 5, standing on its rally,
  turns east and skips the blocked leg again (waypoint 2,
  refusals 1, Patrolling), because the stone at (21,5,20)
  survives in the save. The enclosed patrol, in the same
  five ticks, cycles east, south, west, north, east again
  — waypoints 2, 3, 4, 0, 2, refusals 1 through 5 — and
  reads Stuck again on loaded tick 5, still unmoved at
  (16,5,16). Load therefore re-proves both decisions in
  one phase: the index reset (the walking patrol marches
  to its rally, it does not resume its loop from
  (18,5,22)) and the counter reset (the enclosed patrol
  must earn Stuck again, it does not arrive with it).
- The new scenario's save is 44,015 chars. That length is
  one char shorter than the Slice 15 scenario's 44,016 —
  a different save, quoted here as the Slice 18 entry,
  not as a change to the Slice 15 entry, and not as a
  claim about why one char differs: the earlier lengths
  below were read from the same demo run and are all
  unchanged, so no save rule moved in this step.

## What the review verified this run

- Full test suite re-run before writing: **2690 passed, 0
  failed** (was 2556; +134, no failures on the final runs).
  The Step 3 assertions are composed-integration tests
  mirroring the demo layout above, inserted before the test
  runner's summary so they execute — the Slice 13 Step 4
  lesson, that tests appended after the runner's return do
  not run, is observed here, and the count rise from 2556
  to 2690 is the check that they did.
- Headless demo re-run, save lengths as a no-core-change
  check for the earlier scenarios and as the new entry for
  Slice 18: Slice 1 unchanged (save 43,881 chars); Slice 2
  unchanged (save 43,921 chars); Step 1 scenario unchanged
  (save 44,016 chars); Step 3 scenario unchanged (save
  44,103 chars); Slice 4 scenario unchanged (save 44,026
  chars); Slice 5 Step 2 scenario unchanged (save 43,974
  chars); Slice 6 Step 4 scenario unchanged (save 44,024
  chars); Slice 7 Step 2 scenario unchanged (save 44,013
  chars); Slice 8 Step 3 scenario unchanged (save 43,974
  chars); Slice 9 Step 4 scenario unchanged (save 43,975
  chars); Slice 10 Step 3 scenario unchanged (save 43,974
  chars); Slice 11 Step 3 scenario unchanged (save 44,038
  chars); Slice 12 Step 3 scenario unchanged (save 43,974
  chars); Slice 13 Step 4 scenario unchanged (save 44,045
  chars); Slice 14 Step 2 scenario unchanged (save 43,974
  chars); Slice 15 Step 2 scenario unchanged (save 44,016
  chars); Slice 17 Step 3 scenario unchanged (save 43,974
  chars); Slice 18 Step 3 scenario new (save 44,015 chars).
  A changed earlier save length would have meant a save
  rule moved; none did. Both Slice 18 decisions add no
  save field — the refusal counter and the waypoint index
  are live progress, reset on load by construction — so
  the unchanged earlier lengths are consistent with the
  decisions, not just coincident with them.
- Slice 18 gate, checked against `docs/slice-18-scope.md`:
  every step pushed with passing tests (for Steps 1 and 2,
  with the suite risen by the proof-set tests, never
  standing still on a decision step, and for Step 3, with
  the suite risen by the composed tests); the Step 1
  patrol decision and the Step 2 deploy decision are each
  recorded with rationale in DECISIONS.md, Step 1 with
  its own Windows checklist item (item 37) and Step 2
  with none, exactly as the scope allows for a boundary
  with no scene change; no price, cap, radius, waypoint,
  fire, targeting, delivery, precedence, conflict, rating,
  cost, or phase rule changed in any step; no material,
  marker, light, fog, model, or HUD rule changed in any
  step; every earlier demo save length unchanged, with
  the new Slice 18 length quoted as its own entry; and
  this review records the verdict honestly, separating
  headless-tested route and boundary claims from the
  Windows-checklist patrol observation, including that
  Slice 18 is still not playtested and not
  Unity-verified. The Slice 1 director playtest gate is
  still pending throughout and is not closed by Slice 18.
- Checklist relations, stated so a later reader does not
  miscount them: item 37 is the post-decision patrol
  observation (an enclosed patrol, a singly blocked
  waypoint, and a patrol whose block clears, against the
  dark voxel scene, with the marker and the text line
  read for each). Item 37 extends item 29's Patrol
  observation and orbital-command item 13, neither
  superseded — item 29 stands as the pre-decision
  observation, item 37 as its post-decision counterpart,
  and this review adds no new item, because the review
  exercises no behaviour the checklist does not already
  cover. The next free playtest-gate number remains 38.
  Step 2's boundary takes no item, for the reason stated
  above: a compile error is not a scene behaviour, and
  inventing an item to watch a build fail would be
  theatre, not verification.
- One claim split stated plainly, so it is not over-claimed
  later: the headless evidence for Slice 18 is movement
  positions, refusal counters, and snapshot words for
  staged routes, plus a demo that prints those values for
  two patrols in one field and a save/load round-trip
  that re-derives them, plus a compile-time boundary
  proved by reflection inside the suite and by a
  throwaway non-friend build outside it (Step 2). None
  of that proves a stuck patrol reads Stuck in the scene,
  a marker still distinguishes a stuck Vanguard from a
  walking Recon at FPS distance, or a player understands
  the word while watching a route fail. That is a Windows
  observation (item 37), recorded in DECISIONS.md when
  observed, not closed by a test count. The boundary, by
  contrast, is not an observation at all: it is proved
  where it lives, at compile time, and no Windows item
  can add to it.
- No Slice 18 step duplicates logic in a Unity adapter.
  The sidestep, skip, and stuck reading live in core
  `MoveForPatrol` and `OrbitalView.Snapshot`, the deploy
  boundary lives in core `WorldZone` and its friend
  declaration, and no adapter file was edited in this
  slice except Step 2's audit comment in
  `FpsInputAdapter.cs` — the Slice 16 surface renders
  whatever activity word the snapshot returns, including
  Stuck, because the text line already renders
  `Activity.ToString()`, with no second route rule in
  either adapter that could drift from the core rule.
- No Slice 18 step changed a saved value's meaning: no
  save field was added, no block, kind, doctrine, source,
  or consumer value moved, and every earlier demo save
  length above is unchanged. The two superseded readings
  in this slice's history — the enclosed patrol reading
  Patrolling while cycling without moving (Slice 15's
  stated limit, now Stuck after a full circuit) and the
  free primitive being callable by any assembly (now
  removed, with the seam internal) — were changed with
  history kept in DECISIONS.md, not deleted silently.
- Hard limits held: 3D real-time only, no turn-based
  mechanics, no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Slice 18 adds no block, kind,
  doctrine, source, or consumer, so there is nothing new
  in it that could copy one.

## Deliberately NOT in Slice 18 (still open, not forgotten)

- A delivery, precedence, conflict, rating, cost, or phase
  rule change. Route behaviour is what a patrol does on a
  tick, and no Slice 18 step moves it for any doctrine
  but Patrol, and for Patrol only in the refused-step case
  Step 1 decided: the sidestep lives in `MoveForPatrol`
  alone, proved by HoldGround, Advance, and Defend Core
  contrasts that do not sidestep.
- A price or cap change (10 EmberOre, cap 3, no refund on
  death), a radius or waypoint-count change (radius 3,
  five waypoints), a fire, targeting, or engagement rule
  change, or a save-format change. A stuck patrol fires
  exactly as a moving patrol fires, and a paid patrol
  moves, sidesteps, skips, and reads Stuck exactly as a
  seam-deployed patrol does — the deploy step changes
  which path may be taken, not what the path costs or
  what the squad does.
- A material, shader, marker, light, fog, or view model
  change, except none at all: Step 1's stuck readout
  needed no marker or text change, because the text line
  already renders the activity word, and Step 2's boundary
  needed no scene change at all. A step that re-paletted
  a marker or retuned dusk to make a patrol read better
  would have confused the surface with the route.
- A HUD redesign. The patrol step decides one readout
  question (a new activity word, appended, rendered by an
  existing line) and the deploy step decides one boundary
  question, and nothing else: no new HUD colour, no new
  HUD layout, no quest list, no second POWER line.
- A pathfinder presented as a sidestep. A sidestep is one
  voxel on a gap axis, not a planned route, a route cache,
  or a search over several cells, and no Slice 18 step
  adds one.
- A scene-proof observation presented as a headless result.
  Items 25–27 and 30–37 are Windows observations, recorded
  in DECISIONS.md when observed, not closed by a document,
  a script, or a test count.
- A scene file (`.unity`), an imported asset, a texture, or
  a shader file written by a headless run and claimed as a
  built scene. Scene assembly remains Windows checklist
  work.
- A copied asset, model, skyline, HUD layout, or
  trademarked look from any other game.
- Windows Unity verification itself. It remains open for
  Slices 1–18 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.
- The transformer-adjacency invisibility, or any other
  display-query remainder, as a bonus inside a route or
  boundary step. Each is its own decision with its own
  tests, carried after Slice 18, not patched in beside a
  patrol.
- A Slice 19 scope. Slice 19 is scoped after the Slice 18
  review, not inside it.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–18.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 18 adds item 37 to that plan, alongside the
   still-open items 25, 26, 27, and 30–36. None of these
   items is claimed verified by any test count in this
   review, and scene assembly is still owed in Unity on
   Windows, not in this repo.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 18 does not close that gate, and its test count
   must not be quoted as a playtest result.
3. **First-pass graphics values are unproven, not just
   unverified.** Carried from Slice 16: the dusk sun's
   azimuth 235, intensity 1.1, and warm colour; the fog
   density 0.008 and its about-83%-contrast arithmetic at
   the far band; the view model offset 0.22, -0.22, 0.60;
   and the static, weapon-agnostic model itself are
   first-pass values for readability and testability. A
   Windows observation may find the dusk too warm for the
   cyan/green separation, the fog too thick at the far
   band, or the model too large in frame — and that
   observation, not this review, justifies retuning, with
   its own changelog entry. Slice 18 adds a reason this
   matters: the Stuck word must read in the text line
   under that dusk light, and only item 37's observation
   can say whether a stuck patrol is distinguishable at
   FPS distance from a walking one, or whether the word
   alone carries it.
4. **The Slice 13 scene-proof observations are still unmade,
   as observations.** Carried from Slice 16: whether the
   chunk material actually multiplies vertex colours (item
   26), whether the adapter refreshes the same renderer
   GameManager links (item 27, with item 31 observing the
   null-fill beside it), whether markers read through
   occlusion (item 25, with item 32 extending it to the
   Step 1 material), and now whether the dusk scene reads
   (item 33), the view model sits correctly in frame (item
   34), the decided tint reads (item 35), the decided
   count reads (item 36), and the decided patrol route
   reads (item 37), are all still to be observed on
   Windows. If any observation fails, that failure is a fix
   step, recorded as its own decision — not a reason to
   re-scope the proof layer blindly, and not closed by this
   review.
5. **The transformer-adjacency invisibility is still open.**
   Carried from Slice 17, stated there as a deliberate
   asymmetry, not an oversight: a Conduit beside a
   transformer alone still reads 0 LIVE, 0 LONE, even
   though the transformer outputs None with no other input
   and the Conduit feeds nothing — because a transformer
   is wiring, not a consumer endpoint, and folding it into
   lone would also have to cover the dead-cable case to be
   consistent. If a later step decides that second
   invisibility too, it decides it with its own proof set,
   not by inheriting Slice 17 Step 2's sentence.
6. **The patrol route's stated limits, as decided, still
   leave open questions a later step may take.** Step 1
   decided the refused-step rule and the stuck readout,
   but a patrol whose loop is permanently blocked still
   cycles its index forever, reading Stuck, with no
   recall, no re-rally, and no player action defined for
   it — those are new decisions, not gaps in Step 1's
   proof, and none is smuggled into this review as a
   promise. The radius (3) and the five-waypoint loop are
   still first-pass values, and if a later step finds the
   radius is the cause of a block, that finding is a gap,
   not a quiet widening.

**Closed by this slice, not carried:** the two orbital
remainders in the Slice 17 review's gaps 6 and 7 — the
patrol route's stated limits and the free deploy primitive
— are now decided, with tests, with a Windows checklist
item for the route behaviour and a compile-time boundary
for the deploy behaviour, and proved together in one demo,
so the orbital-remainder question Slice 17 left open is
closed in that sense, not left in the numbered gaps above.
The observation in gaps 1 and 4 above is a remainder about
whether the decided route reads correctly in a built
scene, not a reopening of whether the decision exists.

## Slice 18 verdict (headless)

Slice 18 is **reviewed and closed as two orbital remainders
decided with tests and proved together in one demo (a
paid Recon Patrol sidesteps a diagonal march block, skips
a single-axis loop block, and resets its refusal counter
on moving again, while a paid Vanguard Patrol deployed
later reads Patrolling through four consecutive refusals
and Stuck, unmoved, after the fifth, beside it — and both
patrols, saved and loaded, reset index and counter to 0,
so the enclosed patrol reads Patrolling on arrival and
earns Stuck again only after a fresh full circuit, while
the walking patrol marches to its rally first and skips
the blocked east leg again; and the free deploy primitive
is gone, with the paid path unchanged, price 10, cap 3)**.
It is not playtested, not Unity-verified, and not a visual
result, and its route claims must not be quoted as if the
test count covered the scene reading in item 37.
**Slice 19 is not scoped by this review** — it is scoped
afterwards, the same way this document follows the Slice 17
review rather than preceding it.

## Ranked candidates carried after Slice 18 (starting point, not promised)

Carried from the 0.77.0 scope, in the same order, with the
Slice 18 work taken:

1. The transformer-adjacency invisibility, carried from
   the Slice 17 review's gap 5 — a Conduit beside a
   transformer alone reading 0 LIVE, 0 LONE, decided with
   its own proof set (including the dead-cable case, so
   the decision is consistent), not by inheriting Slice 17
   Step 2's sentence.
2. Any Slice 13 remainder that Slice 18 does not itself
   settle as an observation (the vertex-colour material
   observation, the MeshRenderer wiring observation, and
   the marker-occlusion observation in gap 4, plus the
   dusk and view model observations in items 33 and 34,
   plus the tint and count observations in items 35 and
   36, plus the patrol observation Step 1 adds in item
   37) — if those observations are still unmade when the
   next slice is scoped, they remain observations to be
   made when Windows verification runs, not steps to be
   re-scoped blindly, and not promised here.
3. First-pass graphics values retuned only on Windows
   evidence (dusk azimuth, fog contrast, view model
   offset, static weapon-agnostic model) — a retuning step
   is justified by an observation, not by a headless
   re-decision, and is not promised here.
