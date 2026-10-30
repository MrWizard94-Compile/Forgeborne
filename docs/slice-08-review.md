# Forgeborne — Slice 8 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 929/929.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, and 7. This review closes Slice 8 as a reviewed
whole; it does not claim Slice 8 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 8 close the interaction gap Slice 7 left open — a
player who could look anywhere, yaw toward +X and pitch up
to 89 degrees, and still act in exactly one direction (+Z
for gather and build, Manhattan-nearest for fire, fixed +Z
axes for movement) — with yaw-rotated movement first, then
one shared aim ray for gather, build, and fire, then a
composed demo proving movement and aim together, without
inventing a new movement mode, a weapon rework, a new
doctrine or kind, a power tier, a balance pass, or a
save-format change, and without breaking the gather ->
build -> survive loop, squad price and cap, Dynamo cost
and cadence, loot, skills, core health, or old saves? The
answer headlessly is yes for the systems below, with the
gaps and next candidates listed honestly at the end — and
with one claim split the review must keep split: the core
movement and aim rules are tested, the adapter key / click
feel is written (or unchanged, per Step 2) and
checklist-listed, and neither claim borrows the other's
evidence.

## What Slice 8 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.35.0 scope | `docs/slice-08-scope.md`: Slice 8 scoped as aim-based interaction, in order — yaw-rotated movement, then aim-based gather / build / fire, then demo + review. Ranked Slice 9 candidates recorded, not promised | Decision document only, no system built by it | Tests 862/862 unchanged |
| 0.36.0 yaw-rotated movement | `Simulation.Tick` rotates raw W/S + A/D scalars by `Look` yaw (forward (sin yaw, cos yaw), right (cos yaw, -sin yaw) in XZ, horizontal only); new `FpsController.UpdateWorld` (world +X/+Z, X-then-Z sliding, same vertical code); old `FpsController.Update` delegates to it, so direct-controller callers keep Slice 1 behaviour and there is one movement implementation | Rotation lives in one place, above the controller, so the controller keeps no Look and cannot rotate twice; gather / build / fire deliberately still use fixed +Z cells / nearest-enemy rule in this step, so movement failure and targeting failure stay distinguishable | Tests 889/889; yaw 0 exactly Slice 1, yaw 90 W +X and D -Z (the player's right when facing +X), yaw 180 / -90 / 45 / 360 as expected, pitch never moves the player vertically, diagonal speed preserved not normalised, Swift Feet +5% scales rotated movement, wall-plane sliding (X blocked, Z still moves) |
| 0.37.0 aim-based gather / build / fire | One shared ray, `AimRay.Trace` (new `src/Forgeborne.Core/Aim.cs`): eye at feet + 1.6 m (the adapter's existing camera height), Look forward, 0.05 m face-by-face steps, 10 m reach for all three actions. `GatherAhead` takes the first solid through existing `World.Gather` rules; `BuildAimed` (all three builders) builds in the last air cell before the first solid through existing `World.Build` rules, retiring the front-right / front-left / front-centre offsets (builders now distinct by block type); new `FireAimed` replaces BOTH `FireAtNearestEnemy` and `FireAtCore` (removed, not aliased) — first enemy / core cell within reach, solid blocks the shot, misses still consume ammo. No-target behaviour is refusal with existing results, never a silent +Z fallback | The ray is the single aim rule, so the three actions cannot drift into three aim rules; the adapter gains no new binding, only the renamed fire call plus comments, because E / Q / T / Y and mouse click already call these methods | Tests 908/908; pitch -45 at yaw 0 reaches the old gather cell (16,3,17), level aim at yaw 0 does NOT reproduce the Slice 1 cells (eye ray passes over ground) so the Slice 1 demo was updated and its numbers stand, yaw-90 aim gathers (17,3,16) / builds (17,4,16) / kills a +X enemy, wall blocks fire, core hit by aim, skyward build Floating with no spend, out-of-chunk OutOfBounds, enemy past 10 m unhit with the round spent |
| 0.38.0 demo scenario + review | This review, plus a composed Slice 8 demo scenario: one Simulation in which the player yaws 90, walks +X, gathers, builds, and fires by aim, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel is explicitly out of headless reach and stays in its checklist | Tests 929/929; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **929 passed, 0 failed**
  (was 908; +21 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the
  (18,3,16) gather cell, so no test or demo fix was needed
  in this step).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 1, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,921 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars); Step 3 scenario unchanged (core 42 HP, 3 paid
  squads 90 HP, 10 ore left, 4th picker deploy REFUSED at cap,
  save 44,103 chars); Slice 4 scenario unchanged (HoldGround
  at rally, Advance hunting, save 44,026 chars); Slice 5 Step 2
  scenario unchanged (powered 5 vs kinetic 1 in 5 ticks, stone
  0 -> 6 then 12, save 43,974 chars); Slice 6 Step 4 scenario
  unchanged (pitch clamp at 89, HoldGround Holding, Advance
  Idle after its kill, power 1/1, loaded powered gathered 5,
  save 44,024 chars); Slice 7 Step 2 scenario unchanged
  (Vanguard 90 HP, Recon 40 HP, enemies left 0, save
  44,013 chars). New Step 3 scenario, in full: one
  Simulation, starting at feet (16, 4, 16) with Look yaw 0 /
  pitch 0. Yaw 90, one W tick: feet (16.6, 4, 16) — W walks
  +X, Z unchanged, exactly the Step 1 rule. Pitch -45:
  aimed gather succeeds, gathered 1, cell (18,3,16) now Air.
  With 10 stone funded, aimed build succeeds, built 1,
  cell (18,3,16) now Wall, stone left 9 (10 funded + 1
  gathered - 2 wall). Pitch level: aimed fire x3 kills the
  +X enemy at (22,5,16), killed 1, enemies left 0, ammo
  39/120. Save / load: wall at (18,3,16) survives, enemies
  left 0, player feet restart at (16, 4, 16) — the
  fractional 0.6 m of the pre-save walk is not saved, by
  the documented cell-corner rule — Look restarts at yaw 0
  / pitch 0 because the loaded Simulation is fresh, not
  because a load resets a view (in-place restore keeps the
  current view, per 0.28.0 / 0.14.0), and the session
  counters reset to 0, as in every slice. One loaded W tick
  then walks +Z to (16, 4, 16.6), proving the fresh-view
  rule in the same run instead of leaving it as a comment.
  Save JSON 43,974 chars — the same length as the Slice 5
  scenario's save by coincidence of format, not the same
  save: different wall, different world state, same
  character count, and the review does not treat the
  length as a content fingerprint across scenarios.
- Slice 8 gate, checked against `docs/slice-08-scope.md`:
  every step pushed with passing tests; yaw-rotated
  movement proved at yaw 0 (unchanged) and at yaw 90
  (W walks +X), with the strafe sign (D walks -Z at yaw 90)
  and the diagonal question (speed preserved, not
  normalised) each decided and tested; gather, build, and
  fire each read the core `Look` direction by a stated ray
  (eye + 1.6 m, 0.05 m steps, 10 m reach), with no-target
  behaviour decided per action (gather Air / OutOfBounds,
  build Floating / OutOfBounds, fire consumes the round)
  and no silent fallback to fixed +Z cells; neutral-aim
  compatibility proved as a split (pitch -45 at yaw 0
  reaches the old gather cell, level aim does not, and
  the changed Slice 1 / Slice 2 demo numbers are updated
  openly with reasons); the three builders stay distinct
  by block type once their offsets retire; weapon, squad,
  Dynamo, mill, loot, skill, and save rules unchanged
  (no save-format field added, `Look` still session
  state); the composed demo proves movement and aim
  together through a save/load round-trip; and this
  review records honestly that Slice 8 is still not
  playtested, balanced, or Unity-verified.
- One geometry fact this step pins, so it is not
  re-derived wrong later: the composed gather cell is
  (18,3,16), not the feet-level yaw-90 cell (17,3,16)
  from Step 2. The walk moves the eye 0.6 m east before
  the pitch-down ray is traced, and the descending ray
  strikes the ground one cell further east. The demo
  prints the cell and the tests assert it, so a future
  reader who replays the scenario from the Step 2 cells
  alone will see the discrepancy explained, not hidden.
- One claim split stated plainly, so it is not
  over-claimed later: the demo proves the core rules
  (walk direction, gather cell, build cell, fire hit,
  save/load world state) and the fresh-view rule, but it
  does not prove that a mouse turn feels right, that the
  crosshair sits on the aimed cell, or that E / Q / T / Y
  / click feel right in a scene. Those are adapter claims
  with Windows checklist items (items 13 and 14 in
  `docs/slice-01-playtest-gate.md`), and this review adds
  no new checklist item for the demo itself, because the
  demo exercises no adapter binding the checklist does
  not already cover — the same no-new-item rule the
  Slice 7 review applied, for the same reason.
- No Slice 8 step duplicates logic in a Unity adapter.
  Core owns the rotation, the ray, and the three actions;
  the adapter only feeds mouse deltas and key presses
  into them and renders from the core Look. The rotation
  and the ray cannot be compiled headlessly as adapter
  code — they are core code, tested, and the adapter
  bindings are written and checklist-listed, not
  test-count claims.
- No Slice 8 step changed a saved value's meaning: the
  save format gains no field, and every earlier demo save
  length above is unchanged. The new scenario's save
  length (43,974 chars) is a composition fingerprint for
  this scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (movement and fire are per-tick, with no
  planning pauses), no twin-stick Asteroids loop, no
  copied assets, lore, code, or trademarks. The aim ray
  is an original core rule, not a copied mechanic; the
  voxel placement rule (last air cell before a solid) is
  a genre-standard rule expressed in original code, not
  a copied asset or name.

## Deliberately NOT in Slice 8 (still open, not forgotten)

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
  are deferred, not rejected, and are ranked first for
  Slice 9 scoping below.
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
- A balance pass. Reach (10 m), eye height (1.6 m), and
  step (0.05 m) are first-pass values for determinism
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

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–8.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 8 adds items 13 (mouse turn + W/D feel) and 14
   (aimed gather / build / fire feel) to that plan; neither
   is claimed verified by any test count in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 8 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Aim feel is unproven, not just unverified.** Reach,
   eye height, and the pitch-down gather geometry are
   first-pass and headless-proved only. A Windows playtest
   may find 10 m too short or too long, level aim
   gathering nothing feels wrong, or the (18,3,16)-style
   cell shift after a walk confusing — and that
   observation, not this review, justifies retuning, with
   its own changelog entry.
4. **Mesh tint and mesh refresh remain named gaps.** A
   Dynamo is still not rendered distinctly from stone or
   mills, and no adapter refreshes the voxel mesh on any
   key build, so a wall built by aim may be counted
   before it is visible in the scene. Both were named in
   0.30.0, and neither is closed by a review step.
5. **Two kinds share one marker.** Decided, not forgotten:
   if a Windows playtest shows players mistaking a Recon
   for a Vanguard in the scene, that observation, not
   this review, justifies a distinction, with its own
   checklist item.
6. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 8 verdict (headless)

Slice 8 is **reviewed and closed as tested aim interaction
plus written (or unchanged) adapters — a player who yaws
and walks in the yawed direction, gathers, builds, and
fires by one shared aim ray with no silent +Z fallback,
proved together in one composed demo through a save/load
round-trip**. It is not playtested, not balanced, and not
Unity-verified, and its adapter claims must not be quoted
as if the test count covered them. **Slice 9 is not scoped
by this review** — its scope is the decision for the next
run, chosen from the ranked candidates below, so it is
chosen with Slice 8's composition evidence in hand rather
than guessed now.

## Ranked candidates for Slice 9 scoping (starting point, not promised)

Carried from the 0.35.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional
   consumers and sources), deferred, not rejected.
2. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 8's aim evidence will inform, not promises
   made before that evidence exists.
