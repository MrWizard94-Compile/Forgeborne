# Forgeborne — Slice 13 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1807/1807.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, and 12. This review closes Slice 13 as a reviewed
whole; it does not claim Slice 13 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 13 close the visibility-and-fairness gap Slice 12 left open —
a game in which a player who wires a mill to HV learns it only when
the mill disappears on its delivery tick, a player who wires a
workstation to HV or a pylon to LV learns it only when regen or fire
stops that tick, a player looking at the scene cannot tell a Heavy
from a Vanguard or a Recon because all three kinds share one cyan
cube marker, and no machine is rendered distinctly by power state at
all, with no adapter refreshing the voxel mesh on a key build — with
one warning first, then one squad-marker distinction, then one mesh
tint and refresh, and closed by a composed demo proving the warning,
the query, and the snapshot kinds together, without adding a third
source, a third consumer, Patrol, a fourth kind, a second doctrine,
a balance pass, a new save field, or a delivery / transformer /
cable rule change, and without changing any existing consumer's
rating, cost, effect, or mismatch rule, or any existing source's
output or precedence? The answer headlessly is yes for the systems
below, with the gaps and next candidates listed honestly at the end
— and with one claim split the review must keep split: the core
warning readout and power-state query are tested, the marker, tint,
and refresh lines are written adapters with Windows checklist items,
and neither claim borrows the other's evidence.

## What Slice 13 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.56.0 scope | `docs/slice-13-scope.md`: Slice 13 scoped as adapter visibility and fairness, in order — one warning, then one marker distinction, then one mesh tint and refresh, then demo + review. Ranked Slice 14 candidates recorded, not promised | Decision document only, no system built by it | Tests 1622/1622 unchanged |
| 0.57.0 warning | `HudState` gains split warning counts `WarnedMills` / `WarnedWorkstations` / `WarnedPylons`, derived on every snapshot, never saved: a mill delivered HV warns of destruction on the next tick, a workstation delivered HV warns of disablement, a pylon delivered LV warns of disablement. Split per outcome, not folded into one number, because a destruction and a disablement are not the same loss. Powered, unpowered, and conflicted (None) machines never warn, because they lose nothing on the next tick. Rewiring clears a warning on the next snapshot; destroying a mill removes its warning because the mill is gone. A warning postpones nothing. `HudDisplay` gains an optional fifth Text field `WarningText` with its own line, unwired = no change; playtest-gate checklist item 24 (Windows-only). No delivery, precedence, conflict, rating, cost, or phase rule moves | Readout only: counts re-derive after load, every earlier demo save length stands | Tests 1664/1664; demo unchanged, Slice 12 Step 3 scenario save 43,974 chars |
| 0.58.0 marker distinction | `SquadMarkerDisplay` renders the snapshot Kind it already read but ignored: Vanguard keeps the recognised cyan cube 0.6 units, Recon is a magenta capsule 0.5 wide x 0.8 tall, Heavy is a light-grey cylinder 0.8 wide x 0.7 tall. Shape first, colour second, so the kinds stay apart for a player who cannot tell the colours apart, and no kind colour reads as orange, so no squad marker is mistaken for an orange rally or pending marker. Rally (orange flat cube) and pending (orange sphere) markers unchanged. Pool budget preserved: 3 squad slots, each a parent holding one child primitive, swapped only when a slot is reused by a later deployment of a different kind, never every frame; unknown kind falls back to the Vanguard cube. Playtest-gate checklist item 25 (Windows-only), which supersedes item 20's statement that a Heavy marker is the same cyan cube. No kind shape, cost, cap, movement, targeting, or doctrine rule moves; `OrbitalSquadDisplay` untouched, so text and markers read Kind from the same snapshot | Adapter only: no core file touched, no save-format change | Tests 1664/1664 unchanged, adapter-only step; demo unchanged, Slice 12 Step 3 scenario save 43,974 chars |
| 0.59.0 mesh tint and refresh | `WorldZone.PowerState`, derived from placement and delivered voltage on every call, never saved: Powered for a consumer delivered its rated voltage, Mismatched for a consumer delivered a non-None voltage that is not its rating (exactly the Step 1 warning predicates, so tint and warning agree cell for cell), Energised for a Cable cell in a component carrying a non-None voltage, None for everything else — including sources and transformers, by decision, because a lone source tinted live would claim a delivery that reaches no cell. Vertex-colour channel, not a per-machine material split: `MeshData` gains a `Colors` list parallel to `Vertices`, `MeshQuad` gains a `PowerState` that joins the greedy merge key, so quads never merge across a tint boundary; with no provider every quad is None and merge behaviour is exactly as before. Tints, first-pass values: Powered green (0.25, 0.9, 0.25), never cyan, so it cannot read as a squad marker; Mismatched red (1, 0.15, 0.15), never orange, so it cannot read as a rally marker; Energised yellow (1, 0.85, 0.1), brighter and greener than orange, on cable cells, never as a floating marker; None white, which leaves a material's own colour unchanged. Refresh event set, named, not "always": `VoxelMeshRenderer.RefreshMesh` builds the tinted mesh and assigns `mesh.colors`, and `FpsInputAdapter` refreshes only on a successful gather, a successful build key, a mill destruction on a tick, load, or Start — never every frame, and never on a failed key, a reload, a shot, or a squad deploy. Honest material limit: `mesh.colors` only shows if the chunk material multiplies vertex colours, so the scene must wire a vertex-colour material. Playtest-gate checklist items 26 (tint) and 27 (refresh), Windows-only. No delivery, precedence, conflict, rating, cost, or phase rule moves, and no new save field | Query is core and tested; tint and refresh are adapter-only Unity code, not compiled headlessly, so no test count proves a tint reads or a refresh fires in the scene | Tests 1708/1708; demo unchanged, Slice 12 Step 3 scenario save 43,974 chars |
| 0.60.0 demo scenario + review | This review, plus a composed Slice 13 demo scenario: one Simulation in which the Step 1 warning reads a threatened mill, a threatened workstation, and a threatened pylon, beside the Step 2 snapshot kinds (all three, paid, through the picker) and the Step 3 power-state query reading the same cells the warning reads, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in their checklist items (24 for the warning line, 25 for the markers, 26 for the tint, 27 for the refresh), not in the numbers below | Tests 1807/1807; demo Step 4 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1807 passed, 0 failed**
  (was 1708; +99 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the tick-1
  destruction, the rewiring clear, the rewired-pylon kill, the
  5-in-5 mill gather, the bench-walk regen reading, and the
  loaded-phase repeat, so no test or demo fix was needed in
  this step, except one test-file repair that is not a code
  fix: the composed tests were first appended after the
  runner's top-level `return`, so they did not execute and
  the count still read 1708; moving the summary and return
  to the end of the file made them run, and the count rose
  to 1807 with no failures. That repair is recorded here so
  a future step does not append tests after the return again
  and mistake a stale count for a passing suite).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 1, kinetic gathered 1, squad intact, save 43,921 chars);
  Step 1 scenario unchanged (core 42 HP, weapon 28/120, shields
  100/105, save 44,016 chars); Step 3 scenario unchanged (core
  42 HP, 3 paid squads 90 HP, 10 ore left, 4th picker deploy
  REFUSED at cap, save 44,103 chars); Slice 4 scenario unchanged
  (HoldGround at rally, Advance hunting, save 44,026 chars);
  Slice 5 Step 2 scenario unchanged (powered 5 vs kinetic 1 in
  5 ticks, stone 0 -> 6 then 12, save 43,974 chars); Slice 6 Step 4
  scenario unchanged (pitch clamp at 89, HoldGround Holding,
  Advance Idle after its kill, power 1/1, loaded powered
  gathered 5, save 44,024 chars); Slice 7 Step 2 scenario
  unchanged (Vanguard 90 HP, Recon 40 HP, enemies left 0, save
  44,013 chars); Slice 8 Step 3 scenario unchanged (gathered 1,
  built 1, killed 1, save 43,974 chars); Slice 9 Step 4 scenario
  unchanged (voltages LV/LV/HV/None, HUD 2/4 mills before and
  2/3 after tick 1, powered 10 + kinetic 1 + destroyed 1 in 5
  ticks, stone 11, loaded phase powered 10 + kinetic 1, stone
  22, save 43,975 chars); Slice 10 Step 3 scenario unchanged
  (powered 5 + loaded 5, kinetic 1 + loaded 1, destroyed 1, save
  43,974 chars); Slice 11 Step 3 scenario unchanged (Heavy
  Defend Core at core, Vanguard HoldGround at rally, dead squad
  stays dead, save 44,038 chars); Slice 12 Step 3 scenario
  unchanged (powered 5 + loaded 5, destroyed 0, save 43,974
  chars). New Step 4 scenario, in full: one Simulation, three
  paid squads through the picker (Vanguard, Recon, Heavy, 10
  EmberOre each, ore left 0, all HoldGround), staged on the
  y=20 plane with a mill fed HV directly by an Alternator
  (threatened), a mill fed LV directly by a Dynamo with a
  five-stone pocket (powered), and a live cable run (Dynamo ->
  cable -> cable, free end, energised, feeding nothing), and
  on the y=5 plane with a workstation fed HV directly by an
  Alternator (threatened), a workstation fed LV through an
  Alternator -> transformer step-down (powered), and a pylon
  fed LV directly by a Dynamo (threatened), with one enemy
  staged at (10,5,19), in the threatened pylon's range. The
  player spawns at (16,5,16), already beside the threatened
  workstation, takes 20 damage (shields 85 -> 65), and the
  pre-tick HUD reads 1/2 MILLS POWERED, 1/2 WORKSTATIONS
  POWERED, 0/1 PYLONS POWERED, WARNED 1 MILL, 1 WORKSTATION,
  1 PYLON, POWER 1 LIVE NETWORK, 0 LONE SOURCES, while the
  query reads Mismatched for all three threatened cells,
  Powered for both powered consumers, Energised for the
  cable, and None for a Dynamo and for the transformer, and
  the snapshot carries all three kinds, all HoldGround,
  reading Marching — squads deploy at the player, not at
  their rallies, so Marching, not Holding, is the honest
  word at every point in this run. On tick 1 the threatened
  mill's cell becomes Air (destroyed 1), the powered mill
  gathers 1, shields stay 65 (regen 0 at the disabled bench),
  the enemy steps to (11,5,19) and stays 30 HP (the disabled
  pylon fires nothing), and the warnings read 0 mills (the
  mill is gone), 1 workstation, 1 pylon. The two surviving
  threatened machines are then rewired — the workstation's
  Alternator swapped for a Dynamo, the pylon's Dynamo swapped
  for an Alternator — and the next snapshot reads WARNED
  0/0/0, 2/2 WORKSTATIONS POWERED, 1/1 PYLONS POWERED, with
  both queries reading Powered: warning and tint clear
  together, as Step 3 promised by construction. The player
  walks to (10,5,10) on the pylon lane; on tick 2 the rewired
  pylon fires 10 (enemy 30 -> 20, at (12,5,19)), on tick 3 it
  fires 10 again (20 -> 10, at (12,5,18)), and on tick 4 it
  kills the enemy (10 -> 0, enemy list empty) — a recovery
  kill by the same pylon that fired nothing while threatened,
  with each tick's drop exactly 10, so no squad fired a
  shot in the kill (a squad hit would have added 12, 16, or
  30 to a tick's drop, and none did, because no squad had
  reached its rally to hold and fire). The powered mill
  gathers 5 in 5 ticks (pocket exhausted), stone is 5,
  shields stay 65 on the pylon lane (no bench in range),
  and the player walks back to the rewired bench to read
  effective regen 10 (base 5 + powered 5). Save / load: the
  threatened mill cell stays Air (no resurrection), the
  powered mill, the cable, the rewired workstation, and the
  rewired pylon all load; the HUD re-derives to 1/1 mills,
  2/2 workstations, 1/1 pylons, WARNED 0/0/0, POWER 1 LIVE
  NETWORK, 1 LONE SOURCE — the lone source is the destroyed
  mill's orphaned Alternator, counted only after the tick,
  not a new source — and the queries re-derive to Powered
  for the mill, the workstation, and the pylon, and
  Energised for the cable (queries and counts are derived,
  not saved); session counters reset (powered 0, destroyed
  0); stone stays 5; the player is back at the bench
  (16,5,16) reading regen 10; the snapshot still carries
  all three kinds, still Marching, en route to their
  rallies (Vanguard (13,5,14), Recon (12,5,17), Heavy
  (18,5,13)). In the loaded phase the player takes 20 damage
  again (65 -> 45), the mill pocket is re-seeded, and after
  5 ticks the mill gathers 5 again, nothing is destroyed,
  stone is 10 (5 + 5, no refund on load), shields are capped
  at 85 (45 + 10 per bench tick), and enemies stay 0 — the
  pylon kill stays dead. Save JSON 44,045 chars — a
  composition fingerprint for this scenario only, not a
  balance number.
- Slice 13 gate, checked against `docs/slice-13-scope.md`:
  every step pushed with passing tests (or, for the
  adapter-only parts of Steps 2 and 3, with the test suite
  unchanged and the adapter-only verification stated, never
  inflated into a test claim); the Step 1 warning's coverage
  (all three mismatch outcomes), shape (split per-outcome
  counts, never saved), and adapter line (fifth Text field
  with its own line, plus Windows-only checklist item 24)
  are each decided and tested, with delivery rules unchanged
  and every earlier demo save length unchanged; the Step 2
  marker distinction's shape-or-colour choice (shape first,
  colour second), palette (named against the orange rally /
  pending markers), and untouched markers are each decided
  and checklist-listed (item 25), with no squad rule changed;
  the Step 3 tint set (Powered / Mismatched / Energised /
  None, sources and transformers None by decision), colour
  channel (vertex colours, merge key joined by power state),
  and refresh event set (successful gather, successful
  build key, destruction tick, load, Start — never every
  frame) are each decided, with the power-state query tested
  and no power rule changed and no new save field; the
  composed demo proves the warning, the query, and the
  snapshot kinds together through a save/load round-trip,
  with the warning reading before the destruction /
  disablement tick and clearing on rewiring; and this
  review records honestly that Slice 13 is still not
  playtested, balanced, or Unity-verified. The Slice 1
  director playtest gate is still pending throughout and is
  not closed by Slice 13.
- Three staging facts this step pins, so they are not
  over-claimed later. First, the demo stages its layout
  directly with `TrySet`, exactly as the Step 1-3 tests stage
  theirs, because aim placement of a mill, a workstation, a
  pylon, a cable, a transformer, a Dynamo, and an Alternator
  was each proved in its own step's tests; the demo claims
  composition of core rules, not aim placement of this exact
  layout. Second, the squads are marching, not holding, at
  every snapshot in this run: a picker deploy spawns at the
  player, and five ticks do not carry a squad from (16,5,16)
  to a rally at (5,5,5), (5,5,25), or (25,5,5), so the demo
  proves the snapshot carries all three kinds and doctrines
  together, not that any squad has reached its rally — that
  holding proof stays in the earlier steps' tests, and a
  reader who quotes this demo as a holding proof is
  misquoting it. Third, the orphaned Alternator is a
  consequence, not a bug and not a new source: destroying
  the threatened mill leaves its Alternator adjacent to
  nothing, so the lone-source count, which was 0 before the
  tick, reads 1 after it and after load, while the live
  network count stays 1 throughout. A future step that
  treats a post-destruction lone source as surprising has
  missed this paragraph.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (warning counts
  before the tick, destruction and disablement on the tick,
  warning and query clearing together on rewiring, the
  rewired pylon's recovery kill, mill gather, bench regen,
  save/load re-derivation, and three kinds in one snapshot)
  and the HUD numbers, but it does not prove that the
  WarningText line reads right in the scene, that a Recon
  capsule reads as Recon at FPS distance, that a green or
  red or yellow tint reads on the machine itself, or that
  a mesh refresh fires on a build key in the scene — a
  printed power state does not prove a visible tint, and a
  printed kind does not prove a visible marker. Those are
  adapter claims with Windows checklist items (24 for the
  warning line, 25 for the markers, 26 for the tint, 27 for
  the refresh), and this review adds no new checklist item
  for the demo itself, because the demo exercises no adapter
  binding the checklist does not already cover.
- No Slice 13 step duplicates logic in a Unity adapter.
  Core owns the warning predicates, the power-state query,
  the tint colours as data, and the save bytes; the adapter
  only renders the existing HUD fields and snapshot kinds,
  carries vertex colours into the mesh, and feeds key
  presses into the existing builders. The warning and query
  rules cannot be compiled headlessly as adapter code — they
  are core code, tested, and the adapter lines are written
  and checklist-listed, not test-count claims.
- No Slice 13 step changed a saved value's meaning: the save
  format gains no field, and every earlier demo save length
  above is unchanged. The new scenario's save length
  (44,045 chars) is a composition fingerprint for this
  scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (warning counts derive per snapshot, tint is a
  per-cell readout, refresh is an event set, with no planning
  pauses), no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. The warning, the marker shapes,
  and the tints are original display rules, not copied
  mechanics; the blueprint's electrical vocabulary was taken
  as names only and given tested meanings the blueprint
  never stated.

## Deliberately NOT in Slice 13 (still open, not forgotten)

- A third source, a third consumer, or any other electrical
  widening added alongside the visibility steps as a bonus.
  One visibility question is Slice 13; the deferred third
  source is a Slice 14-or-later decision with this slice's
  evidence in hand.
- A warning that delays, prevents, or softens a loss: no
  grace period, no salvage on destruction, no auto-rewire,
  no destruction refund. The warning is a readout; the loss
  rules from Slices 9 and 10 stand exactly as tested.
- A marker distinction that changes squad rules, or a tint
  that changes power rules. Display steps must not move the
  rules they display.
- Patrol, a fourth squad kind, or a second doctrine. The
  orbital side is closed for this slice; a visibility slice
  that also adds a doctrine would make the visibility work
  untestable in isolation.
- A balance pass. The warning's shape, the marker shapes or
  colours, and the tint set are first-pass values chosen for
  determinism and testability, exactly as Slice 12's pylon
  values were, and must be labelled as such, not quoted as
  final feel.
- An Orbital camera, or a mini-map beyond the Slice 1 placeholder.
  Slice 13 markers remain scene markers in the FPS view, not a
  second camera.
- A new movement mode, a weapon rework, or a free-deploy
  enforcement change. The free `DeploySquad` primitive remains
  a headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- Windows Unity verification itself. It remains open for
  Slices 1–13 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–13.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 13 adds items 24 (warning line), 25 (markers), 26
   (tint, including the vertex-colour material observation),
   and 27 (refresh, including the MeshRenderer wiring) to
   that plan. None of these items is claimed verified by any
   test count in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 13 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Visibility and electrical balance are unproven, not just
   unverified.** The warning's split-count shape, the marker
   shapes and colours, and the tint values are first-pass
   values for determinism and testability. A Windows playtest
   may find a warning line unreadable beside four POWER
   lines, a Recon capsule unreadable at FPS distance, a
   green/red tint unreadable for a player who cannot tell
   those colours apart, or a yellow energised cable
   indistinguishable from an orange rally marker in the
   scene — and that observation, not this review, justifies
   retuning, with its own changelog entry.
4. **The scene proof for the visibility adapters is still
   owed in detail.** Even though the adapter code is written,
   three observations are still open and are not closed by
   this review: whether the scene's chunk material actually
   multiplies vertex colours (item 26), whether
   `FpsInputAdapter.MeshRenderer` is wired to the same
   renderer GameManager links or silently no-ops on refresh
   (item 27), and whether markers read through the scene's
   occlusion at FPS distance (item 25). If any observation
   fails, that failure is a fix step, recorded as a Slice 13
   remainder for Slice 14 scoping, not a silent gap.
5. **The deferred electrical widening is a third source, not a
   decision.** Slice 12 chose a third consumer over a third
   source, and Slice 13 added no source; the third source is
   deferred, not rejected, and a later step must still prove
   a distinctness beyond voltage — a condition, a fuel, a
   cost shape — before it can be claimed, not inherit the
   pylon's proof by analogy.
6. **Patrol is deferred, not designed.** The route question
   it needs — loop, back-and-forth, or waypoints, and where
   the route is stored — is still unanswered, and a later
   doctrine step must answer it with its own tests before
   Patrol can be claimed, not inherit Defend Core's chase rule
   by analogy.
7. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the visibility gaps from
Slice 12 (its gap 4, destruction and disablement with no warning;
its gap 5, mesh tint and mesh refresh; and its gap 6, three kinds
sharing one scene marker). Step 1 added the split warning counts,
Step 2 added the shape-first marker distinction, and Step 3 added
the power-state query, the vertex-colour channel, and the refresh
event set, so a threatened machine is now warned in the HUD, tinted
in the mesh data, and distinct by kind in the scene markers — as
tested core plus written adapters, not yet as scene-verified
proof, which is gap 4's question above, not this one's. They are
recorded here as closed in that sense, not left in the numbered
gaps above, so a later review does not carry them again by
accident.

## Slice 13 verdict (headless)

Slice 13 is **reviewed and closed as tested warning core and
power-state query plus written adapters (one split warning readout
— a mill about to be destroyed, a workstation about to be disabled,
and a pylon about to be disabled, each counted separately, never
saved, clearing on rewiring and vanishing with a destroyed mill —
one shape-first scene-marker distinction — Vanguard cyan cube,
Recon magenta capsule, Heavy light-grey cylinder, pooled, never
orange — and one mesh treatment — a per-cell power-state query,
a vertex-colour channel that never merges across a tint boundary,
and a named refresh event set, never every frame — proved together
in one composed demo through a save/load round-trip, with the
warning reading before the destruction / disablement tick, the
query reading the same power state the warning reads, the snapshot
carrying all three kinds, the rewired machines recovering, and no
new save field)**. It is not playtested, not balanced, and not
Unity-verified, and its adapter claims must not be quoted as if the
test count covered them.
**Slice 14 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 13's composition evidence in
hand rather than guessed now.

## Ranked candidates for Slice 14 scoping (starting point, not promised)

Carried from the 0.56.0 scope, in the same order, because
the Step 4 evidence gives no reason to reorder them:

1. The deferred electrical widening (a third source, now that
   Slice 12 chose a third consumer) — deferred, not rejected,
   and only after Slice 13 proves its visibility steps.
   Its distinctness question is unchanged: a condition, a
   fuel, a cost shape, or another distinctness beyond voltage,
   proved by a test neither existing source can pass.
2. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.
3. Any Slice 13 remainder the Slice 13 review names (the
   vertex-colour material observation, the MeshRenderer
   wiring observation, and the marker-occlusion observation
   in gap 4 above) — recorded at review time, not promised
   here.
