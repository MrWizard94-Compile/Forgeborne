# Forgeborne — Slice 12 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1622/1622.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, and 11. This review closes Slice 12 as a reviewed
whole; it does not claim Slice 12 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 12 close the electrical-widening gap Slice 11 left open —
a game in which a live cable network feeding no consumer yet, and a
lone source of either kind, are invisible in the HUD as well as in
the scene, and in which HV has no safe direct use because both
powered consumers are rated LV (a mill fed HV is destroyed, a
workstation fed HV is disabled, and safe HV use always costs a
transformer step-down) — with one live-network display first, then
exactly one further electrical widening decided in its step, then a
composed demo proving the display and the widening together, without
adding a second consumer or a second source, a warning / tint /
refresh system, a squad-marker distinction, Patrol, a fourth kind, a
second doctrine, a balance pass, a new save field, or a delivery /
transformer / cable rule change, and without changing either
existing consumer's rating, cost, effect, or mismatch rule, or either
existing source's output or precedence? The answer headlessly is yes
for the systems below, with the gaps and next candidates listed
honestly at the end — and with one claim split the review must keep
split: the core display and widening rules are tested, the adapter
lines and keys are written and checklist-listed, and neither claim
borrows the other's evidence.

## What Slice 12 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.52.0 scope | `docs/slice-12-scope.md`: Slice 12 scoped as electrical widening, in order — one live-network display, then exactly one further widening, then demo + review. Ranked Slice 13 candidates recorded, not promised | Decision document only, no system built by it | Tests 1485/1485 unchanged |
| 0.53.0 live-network and lone-source display | `WorldZone.LivePowerCounts`, derived on every snapshot, never saved: LiveNetworks counts live electrical structures no consumer count already shows, LoneSources counts lone Dynamo / Alternator cells. Live means carrying a voltage that is not None, never merely placed. The count is of networks, not cells (a ten-cable LV run is one network); a network feeding a consumer is not counted again, so one delivery is never two live things; LV and HV count alike, with no voltage split, and the count is explicitly not a safety indicator. The counting path reuses the existing `CableComponent` / `CableNetworkVoltage` walk through a new public method, not a re-walk in the HUD; a standalone transformer group (connected transformers with no cable neighbour, one network per group) covers live outputs feeding nothing. Stated approximations, not hidden: "feeds" is direct adjacency or one transformer hop, so a cable, transformer, transformer, mill chain still reads as a live network; a transformer touching a cable is never standalone, so a live output into a conflicted component shows nothing. `HudState` gains both fields, derived in `FromWorld`; `HudDisplay` gains an optional third Text field `LivePowerText` with its own line ("POWER: n LIVE NETWORKS, m LONE SOURCES"), unwired = no change; playtest-gate checklist item 22 (Windows-only), which supersedes item 18's statement that a lone Alternator shows nothing. No delivery, precedence, conflict, rating, cost, or phase rule moves, and no save-format change | Display only: counts re-derive after load, every earlier demo save length stands | Tests 1519/1519; demo unchanged, Slice 11 Step 3 scenario save 44,038 chars |
| 0.54.0 third consumer | Shock Pylon, appended as `BlockType = 12` only, rated HV (`VoltageRules.PylonRated`, its own constant, not borrowed from any other consumer) — the first consumer that actually wants HV, so the Alternator gains a safe direct use and HV stops being a voltage that only ever punishes LV consumers. A powered pylon fires once per tick at the nearest living enemy within Manhattan range 5, for 10 damage, through the shared `AttackEnemy` path, in a new pylon phase last in the tick; two pylons on one enemy stack to 20 in a tick, and ties resolve to the first enemy in list order. Its effect is combat only: it gathers nothing and regens nothing, which is what makes it a third consumer rather than a re-rated mill or workstation. Mismatch follows the workstation, not the mill: a pylon delivered LV is disabled for that tick, not destroyed, and fires again as soon as it is rewired to HV; no voltage (None) idles, and a conflicted (None) pylon is neither powered nor mismatched. `RunMismatchedDestruction` is untouched: it checks `IsMismatched`, which is mill-only by construction, so no pylon is ever destroyed by a wiring mistake. Cost 5 Stone + 10 EmberOre, a new combination — half a workstation's stone frame, and exactly a squad's deploy price in ore, so a pylon competes with a squad for ore. Display: `HudState` gains PoweredPylons / TotalPylons, `HudDisplay` gains an optional `PylonPowerText` line ("POWER: x/y PYLONS POWERED"), unwired = no change. `IsConsumer` now includes the pylon, so the Step 1 anti-double-count rule extends to it: a network feeding a pylon is shown by the pylon count, not counted again as a live network, and a lone pylon counts as no live network and no lone source, because it is a consumer, not a source. `Simulation.BuildShockPylonAhead` places by aim; adapter key P, suppressed while the rally picker is open; playtest-gate checklist item 23 (Windows-only). No per-block mesh colour exists for any machine, so the pylon shares the generic scene rendering of the other machines — a stated rendering fact, not a pylon-specific gap | Consumer stored as a chunk byte, power re-derived after load, so no new save field: a loaded pylon fires on its first tick | Tests 1575/1575; demo unchanged, Slice 11 Step 3 scenario save 44,038 chars |
| 0.55.0 demo scenario + review | This review, plus a composed Slice 12 demo scenario: one Simulation in which a live network feeding no consumer and a lone source read only through the Step 1 counts, beside an HV pylon firing, an LV mill gathering, and a powered workstation regenerating, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in their checklist items (22 for the live line, 23 for the P key), not in the numbers below | Tests 1622/1622; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1622 passed, 0 failed**
  (was 1575; +47 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the
  3-tick pylon kill, the 5-in-5 mill gather, the bench-walk
  regen readings, and the loaded-phase repeat, so no test or
  demo fix was needed in this step).
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
  stays dead, save 44,038 chars). New Step 3 scenario, in full:
  one Simulation, on the y=20 plane a live network (Dynamo at
  (14,20,10) -> cable (13,20,10) -> cable (12,20,10), free end),
  an LV mill at (10,20,14) fed directly by a Dynamo at
  (11,20,14) with a five-stone pocket, and a lone Alternator at
  (10,20,18); on the y=5 plane a Shock Pylon at (17,5,16) fed HV
  directly by an Alternator at (18,5,16), and a workstation at
  (17,5,28) fed LV through an Alternator at (19,5,28) ->
  transformer at (18,5,28). Before any tick the voltages read
  mill LV, pylon HV, workstation LV, and the HUD reads 1/1 MILLS
  POWERED, 1/1 WORKSTATIONS POWERED, 1/1 PYLONS POWERED, and
  POWER: 1 LIVE NETWORKS, 1 LONE SOURCES — the live network and
  the lone source appear in no older count, and the pylon's own
  Alternator is not counted again as either, because the pylon
  count already shows that delivery. The player takes 20 damage
  (shields 85 -> 65) and parks at (10,5,10), far from the bench,
  so no regen confounds the pylon ticks; an enemy is staged at
  (17,5,21), 5 from the pylon. On tick 1 the enemy steps to
  (17,5,20) and the pylon fires 10 (30 -> 20 HP), on tick 2 it
  steps to (17,5,19) and the pylon fires again (20 -> 10 HP),
  and on tick 3 it steps to (17,5,18) and the pylon kills it
  (10 -> 0, enemy list empty) — a 3-tick kill at shrinking
  range, not a stationary range-5 kill, for the reason the
  staging facts below state. Shields stay 65 through all three
  ticks: no bench is in range while parked. The player then
  walks to (16,5,28) beside the powered bench and reads
  effective regen 10 (base 5 + powered 5). After 5 ticks total
  the LV mill has gathered 5 (pocket exhausted), no mill is
  destroyed, stone is 5, and shields are capped at 85 (65 + 10
  per bench tick). Save / load: the live cable, the lone
  Alternator, the mill, the pylon, and the workstation all
  load; the HUD re-derives to the same 1/1, 1/1, 1/1, and
  POWER: 1 LIVE NETWORKS, 1 LONE SOURCES (counts are derived,
  not saved); session counters reset (powered 0, destroyed 0);
  stone stays 5; the player is back at the bench (16,5,28)
  reading regen 10. In the loaded phase the player takes 20
  damage again (85 -> 65), the mill pocket is re-seeded, and
  after 5 ticks the mill gathers 5 again, nothing is destroyed,
  stone is 10 (5 + 5, no refund on load), shields are capped at
  85 again, and enemies stay 0 — the pylon kill stays dead.
  Save JSON 43,974 chars — a composition fingerprint for this
  scenario only, not a balance number.
- Slice 12 gate, checked against `docs/slice-12-scope.md`:
  every step pushed with passing tests; the Step 1 display's
  definition of live (carries a voltage that is not None; a lone
  source is live because it is already generating), its count
  shape (two counts — networks, not cells, for cables and
  transformer groups; per-cell for lone sources; no voltage
  split; a fed network never counted twice), and its adapter
  line (optional third Text field with its own line, plus
  Windows-only checklist item 22) are each decided and tested,
  with delivery rules unchanged and every earlier demo save
  length unchanged; the Step 2 widening's rating (HV, its own
  constant), effect (10 damage per tick at range 5, nearest
  living enemy, last phase in the tick), mismatch rule
  (disabled like a workstation, not destroyed like a mill),
  cost (5 Stone + 10 EmberOre), placement path (aimed builder,
  P key, picker-suppressed), binding, and display (powered /
  total pylon count, extended anti-double-count rule) are each
  decided and tested, with existing consumers' and sources'
  rules unchanged and old saves loading; the composed demo
  proves the display and the widening together through a
  save/load round-trip, with a live network reading live where
  the old counts read nothing and no new save field; and this
  review records honestly that Slice 12 is still not playtested,
  balanced, or Unity-verified. The Slice 1 director playtest
  gate is still pending throughout and is not closed by Slice 12.
- Three staging facts this step pins, so they are not
  over-claimed later. First, the demo stages its layout directly
  with `TrySet`, exactly as the Step 1-2 tests stage theirs,
  because aim placement of a cable, a mill, a pylon, a
  workstation, a transformer, a Dynamo, and an Alternator was
  each proved in its own step's tests; the demo claims
  composition of core rules, not aim placement of this exact
  layout. Second, the parked player and the bench walk are a
  measurement procedure, not a play pattern: workstation regen
  is player-centred (Manhattan distance 3) while pylon fire is
  machine-centred, so one player position cannot prove both at
  once, and the demo states the walk instead of pretending
  otherwise — the same procedure Slice 10's demo used for its
  three benches. Third, the pylon's first shot in the demo is
  at range 4, not range 5: the enemy is staged at range 5, but
  enemies move before the pylon phase, so by the time the pylon
  fires the enemy has stepped one cell closer. The demo therefore
  proves a 3-tick kill at shrinking range (5 staged, then 4, 3,
  2 at firing time), and the stationary range-5 boundary itself
  — fires at 5, silent at 6 — is proved in Step 2's tests, not
  re-proved by this demo, which must not be quoted as proving
  it. Likewise the demo does not re-prove the one-transformer-hop
  feeds approximation or the deeper-chain case: those are Step 1
  decisions, carried here as stated approximations, not demo
  claims.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (live counts, lone
  source, pylon fire and kill, mill gather, bench regen,
  save/load re-derivation) and the HUD numbers, but it does not
  prove that the LivePowerText or PylonPowerText line reads
  right in the scene, that the P key feels right, or that a live
  cable looks different from a dead one in the scene — a live
  network is counted in the HUD, not made visible in the scene,
  and no mesh tint or mesh refresh exists for any machine. Those
  are adapter claims with Windows checklist items (22 for the
  live line, 23 for the P key), and this review adds no new
  checklist item for the demo itself, because the demo exercises
  no adapter binding the checklist does not already cover.
- No Slice 12 step duplicates logic in a Unity adapter.
  Core owns the live-count walk, the pylon rating, fire rule,
  targeting, and phase, and the save bytes; the adapter only
  renders the existing HUD fields and feeds the P key press into
  the existing aimed builder. The display and fire rules cannot
  be compiled headlessly as adapter code — they are core code,
  tested, and the adapter lines are written and checklist-listed,
  not test-count claims.
- No Slice 12 step changed a saved value's meaning: the save
  format gains no field, and every earlier demo save length
  above is unchanged. The new scenario's save length
  (43,974 chars) is a composition fingerprint for this
  scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (counts derive per snapshot, pylon fire is a
  per-tick rule, with no planning pauses), no twin-stick
  Asteroids loop, no copied assets, lore, code, or trademarks.
  The Shock Pylon and the live counts are original core rules,
  not copied mechanics; the blueprint's electrical vocabulary was
  taken as names only and given tested meanings the blueprint
  never stated.

## Deliberately NOT in Slice 12 (still open, not forgotten)

- A second consumer or a second source added alongside the
  Step 2 widening as a bonus. One widening is Step 2; the next,
  if any, is a Slice 13-or-later decision with this slice's
  evidence in hand — the deferred third source is now the
  ranked Slice 13 electrical candidate, behind adapter
  visibility and fairness.
- A pre-destruction or pre-disablement warning, a mesh tint,
  or a mesh refresh on any key build. All three remain named
  gaps from 0.30.0 / 0.42.0, not fixed by a display step: Step 1
  counts live networks in the HUD, it does not warn before a
  mill is destroyed or a pylon is disabled, tint an energised
  cable, or refresh a mesh.
- A scene-marker distinction for the squad kinds. That is the
  Slice 13 visibility candidate, not a Slice 12 extra: Step 1
  displays power, and a marker step that rides along would make
  the display untestable as a display.
- Patrol, a fourth squad kind, or a second doctrine. The orbital
  side is closed for this slice; an electrical slice that also
  adds a doctrine would make the electrical widening untestable
  in isolation.
- A balance pass. The pylon's range 5, damage 10 per tick, and
  cost 5 Stone + 10 EmberOre are first-pass values chosen for
  determinism and testability, exactly as Slice 9's transformer
  cost and Slice 10's Alternator cost were, and must be labelled
  as such, not quoted as final feel.
- An Orbital camera, or a mini-map beyond the Slice 1 placeholder.
  Slice 12 markers remain scene markers in the FPS view, not a
  second camera.
- A new movement mode, a weapon rework, or a free-deploy
  enforcement change. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps, not
  quietly fixed here.
- Windows Unity verification itself. It remains open for
  Slices 1–12 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–12.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 12 adds items 22 (live line) and 23 (P key) to that
   plan. None of these items is claimed verified by any test
   count in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 12 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Electrical balance is unproven, not just unverified.**
   The pylon's range 5 and damage 10 per tick, its cost of
   5 Stone + 10 EmberOre against a squad's 10 ore, and the
   live counts' lack of a voltage split are first-pass values
   for determinism and testability. A Windows playtest may find
   a pylon too strong beside a squad, too weak to hold a lane
   alone, its ore price wrong against a mobile squad, or an HV
   live network indistinguishable from a safe LV one in the
   HUD — and that observation, not this review, justifies
   retuning, with its own changelog entry.
4. **Destruction and disablement have no warning, by decision,
   not by oversight.** Carried from Slices 9, 10, and 12: a
   player's first HV mill simply disappears on its delivery
   tick, a player's first HV workstation simply stops
   regenerating that tick, and a player's first LV pylon simply
   stops firing that tick, and the only headless traces are the
   HUD totals and the `DestroyedMills` session counter.
5. **Mesh tint and mesh refresh remain named gaps.** A
   Dynamo, Alternator, transformer, cable, powered mill,
   powered workstation, or powered pylon is still not rendered
   distinctly, and no adapter refreshes the voxel mesh on any
   key build, so a cable laid by aim, a pylon placed by aim, or
   a mill destroyed by voltage may be counted before it is
   visible in the scene. Both were named in 0.30.0, and
   neither is closed by a review step.
6. **Three kinds still share one scene marker.** Decided in
   Slice 11 Step 1, not forgotten, and carried as a Slice 13
   visibility question together with gaps 4 and 5: if a Windows
   playtest shows players mistaking a Heavy for a Vanguard or
   a Recon in the scene, that observation, not this review,
   justifies a distinction, with its own checklist item —
   decided at Slice 13 scoping as one visibility question, not
   promised here.
7. **The deferred electrical widening is a third source, not a
   decision.** Step 2 chose a third consumer over a third
   source for the scope's reason (HV had no safe direct use);
   the third source is deferred, not rejected, and a later step
   must still prove a distinctness beyond voltage — a condition,
   a fuel, a cost shape — before it can be claimed, not inherit
   the pylon's proof by analogy.
8. **Patrol is deferred, not designed.** The route question
   it needs — loop, back-and-forth, or waypoints, and where
   the route is stored — is still unanswered, and a later
   doctrine step must answer it with its own tests before
   Patrol can be claimed, not inherit Defend Core's chase rule
   by analogy.
9. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the live network /
lone source invisibility gap from Slice 11 (its gap 6). Step 1
added the live-network and lone-source counts, and Step 2's
pylon extended their anti-double-count rule without breaking
it, so a live cable network with no consumer on it, and a lone
Dynamo or Alternator, are now counted in the HUD — not yet
visible in the scene, which is gap 5's question, not this one's.
It is recorded here as closed, not left in the numbered gaps
above, so a later review does not carry it again by accident.

## Slice 12 verdict (headless)

Slice 12 is **reviewed and closed as tested electrical widening
(one live-network and lone-source display — live means carrying a
voltage that is not None, counted as networks not cells, with a fed
network never counted twice, and a lone source counted per cell —
and one third consumer — the Shock Pylon, an HV-rated defensive
emitter firing 10 per tick at range 5, disabled on LV rather than
destroyed, costing 5 Stone + 10 EmberOre, proved against every
delivery path both ways — proved together in one composed demo
through a save/load round-trip, with a live network reading live
where the old counts read nothing, a lone source reading lone, and
no new save field — plus written adapters (LivePowerText and
PylonPowerText lines, P key, gate items 22 and 23))**. It is not
playtested, not balanced, and not Unity-verified, and its adapter
claims must not be quoted as if the test count covered them.
**Slice 13 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 12's composition evidence in
hand rather than guessed now.

## Ranked candidates for Slice 13 scoping (starting point, not promised)

Carried from the 0.52.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. Adapter visibility and fairness as one candidate:
   a pre-destruction / pre-disablement warning, a mesh tint
   and mesh refresh, and a scene-marker distinction for the
   squad kinds — gaps named in the Slice 9, Slice 10, Slice 11,
   and Slice 12 reviews, to be decided at Slice 13 scoping as one
   visibility question or split apart there, not promised here.
2. The deferred electrical widening from Step 2's pool (a third
   source, now that Step 2 chose a third consumer) — deferred,
   not rejected, and only after Slice 12 proves its widening.
3. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.
