# Forgeborne — Slice 14 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1988/1988.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, and 13. This review closes Slice 14 as a reviewed
whole; it does not claim Slice 14 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 14 close the deferred electrical widening Slice 12
left open — a game with exactly two power sources, both
always-on while placed, feeding three consumers on a
two-level ladder — with exactly one third source, proved
distinct from both existing sources by something other than
voltage, and closed by a composed demo proving that source
feeding a consumer at its voltage beside an existing source
feeding a consumer at its voltage, with a mismatch or idle
case beside them, through a save/load round-trip, without
a second source, a ladder redesign, a delivery /
transformer / cable rule change, a new HUD count, a new
save field, a balance pass, Patrol, a fourth kind, or a
second doctrine, and without changing any existing
consumer's rating, cost, effect, or mismatch rule, or any
existing source's output or precedence? The answer
headlessly is yes for the systems below, with the gaps and
next candidates listed honestly at the end — and with two
findings this review refuses to smooth over: a single
cable cell touching a Conduit reads None in PowerState
while its network is live, and a Conduit standing directly
beside a mill counts as neither a live network nor a lone
source. Both are carried as remainders, not fixed inside
this review, and neither is a delivery failure: the
networks in question deliver exactly as Step 1 proved.

## What Slice 14 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.61.0 scope | `docs/slice-14-scope.md`: Slice 14 scoped as the deferred electrical widening, in order — one third source, decided in its step, then demo + review. Ranked Slice 15 candidates recorded, not promised | Decision document only, no system built by it | Tests 1807/1807 unchanged |
| 0.62.0 third source | `BlockType.Conduit` = 13, appended only, output HV (`VoltageRules.ConduitOutput`), cost 5 Stone + 5 EmberOre, `Simulation.BuildConduitAhead`, adapter key M (suppressed while the picker is open), playtest-gate checklist item 28 (Windows-only). Distinctness is delivery shape, not voltage and not price: a Conduit delivers nothing face-adjacent — a mill, workstation, pylon, or transformer beside one reads None — and delivers HV only through a Cable network touching it. Safe LV use runs through a transformer step-down fed by that cable, exactly as the Alternator's does. Generalisation with precedence and conflict unchanged: `DeliveredVoltage`, `TransformerOutput`, and `CableNetworkVoltage` know three sources, but the direct set stays two — the Conduit is absent from both direct branches by decision. No per-source state, so no save question beyond chunk bytes: a Conduit cable chain re-derives HV on load, old saves load unchanged | Core only, plus a thin adapter key: no duplicate logic in Unity code, no new HUD count, no new save field | Tests 1894/1894; demo unchanged, Slice 13 scenario save 44,045 chars |
| 0.63.0 demo scenario + review | This review, plus a composed Slice 14 demo scenario: one Simulation in which the Conduit feeds a mill to destruction through a two-cable run, a Dynamo feeds a mill to powered gathering directly, a Conduit beside a mill feeds nothing and the mill falls back to kinetic cadence, a Conduit feeds a pylon to a three-tick kill through a single cable, and a Conduit feeds a workstation at LV through a cable -> transformer step-down beside the player spawn, through a save/load round-trip. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in checklist item 28, not in the numbers below | Tests 1988/1988; demo Step 2 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1988 passed, 0 failed**
  (was 1894; +94 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its second execution, after two staging
  corrections described below, and the composed tests
  passed on their first full run, so no test or demo fix
  was needed after the staging was corrected).
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
  (save 44,045 chars). New Step 2 scenario, in full: one
  Simulation, staged on the y=20 plane with a mill fed HV
  through a Conduit -> cable -> cable run (threatened), a
  mill fed LV directly by a Dynamo with a five-stone pocket
  (powered), and a mill standing directly beside a Conduit
  with no cable (unfed, kinetic) with its own five-stone
  pocket, and on the y=5 plane with a Shock Pylon fed HV
  through a single Conduit -> cable run (powered) with one
  enemy staged at (10,5,17) in its range, and a workstation
  fed LV through a Conduit -> cable -> transformer
  step-down beside the player spawn (16,5,16) (powered).
  The player takes 20 damage (shields 85 -> 65) and the
  pre-tick HUD reads 1/3 MILLS POWERED, 1/1 WORKSTATIONS
  POWERED, 1/1 PYLONS POWERED, WARNED 1 MILL, 0
  WORKSTATIONS, 0 PYLONS, POWER 0 LIVE NETWORKS, 0 LONE
  SOURCES, while the voltages read HV for the Conduit
  cable mill, LV for the Dynamo mill, None for the
  direct-Conduit mill, HV for the pylon, and LV for the
  step-down workstation, and the power states read
  Mismatched for the threatened mill, Energised for both
  cables of the two-cable run, Powered for the Dynamo mill,
  None for the direct-Conduit mill, Powered for the pylon,
  None for its single cable, Powered for the step-down
  workstation, Energised for its cable (via its
  transformer output, not via the Conduit directly), and
  None for the transformer and for every Conduit cell, by
  decision. Effective bench regen reads 10 at spawn (base
  5 + powered 5). On tick 1 the threatened mill's cell
  becomes Air (destroyed 1), the Dynamo mill gathers 1,
  the direct-Conduit mill gathers 0 (kinetic cadence, not
  yet its 5th tick), shields rise 65 -> 75 (regen 10 at
  the powered bench), the enemy steps to (11,5,17) and
  drops 30 -> 20 HP (the Conduit-fed pylon fired 10, its
  single cable still reading None), warnings clear to
  0/0/0, and power reads 1 LIVE NETWORK, 0 LONE SOURCES
  — the destroyed mill's cable run now feeds nothing, so
  it counts live, while the direct Conduit still counts
  neither. On tick 2 the enemy steps to (12,5,17) and
  drops 20 -> 10 HP; on tick 3 the pylon kills it
  (10 -> 0, enemy list empty) — a three-tick kill at
  exactly 10 damage per tick, so no other damage source
  fired. On ticks 4 and 5 the Dynamo mill gathers to 5
  (pocket exhausted) and the direct-Conduit mill gathers
  its 1 kinetic stone on tick 5, so after 5 ticks powered
  gathered 5, kinetic gathered 1, destroyed 1, stone 6,
  shields capped at 85, enemies 0. Save / load: the
  destroyed mill cell stays Air (no resurrection), the
  Dynamo mill, the direct-Conduit mill, the pylon, the
  step-down workstation, and all four Conduits load; the
  HUD re-derives to 1/2 mills, 1/1 workstations, 1/1
  pylons, WARNED 0/0/0, POWER 1 LIVE NETWORK, 0 LONE
  SOURCES; the states re-derive to Powered for the Dynamo
  mill, None for the direct-Conduit mill, Powered for the
  pylon, None for the single cable (the finding
  re-derives after load — it is not a stale tint), and
  Powered for the workstation; session counters reset
  (powered 0, kinetic 0, destroyed 0); stone stays 6; the
  player is back at the bench (16,5,16) reading regen 10.
  In the loaded phase the player takes 20 damage again,
  both pockets are re-seeded, and after 5 ticks the mill
  gathers 5 again, the direct-Conduit mill gathers 1
  again, nothing is destroyed, stone is 12 (6 + 5 + 1, no
  refund on load), shields are capped at 85, and enemies
  stay 0 — the pylon kill stays dead. Save JSON 43,974
  chars — a composition fingerprint for this scenario
  only, not a balance number.
- Slice 14 gate, checked against `docs/slice-14-scope.md`:
  every step pushed with passing tests; the Step 1
  source's distinctness (delivery shape, proved by a test
  neither existing source can pass — a direct Conduit mill
  reads None where a direct Dynamo or Alternator feeds),
  voltage (HV, with the full mismatch proof set), no-state
  / no-save-field answer, generalisation (three sources,
  direct set two, precedence and conflict unchanged),
  block / cost / adapter line (BlockType 13, 5 Stone + 5
  EmberOre, M key, checklist item 28), and readout
  agreement (warned counts, PowerState, live-network and
  lone-source counts, with the two findings below stated,
  not hidden) are each decided and tested, with no
  existing block's delivery, rating, cost, or phase rule
  moved and every earlier demo save length unchanged; the
  composed demo proves the third source feeding a consumer
  at its voltage beside an existing source feeding a
  consumer at its voltage, with a mismatch case (the
  destroyed cable mill) and an idle case (the
  direct-Conduit kinetic mill) beside them, through a
  save/load round-trip, with the warning reading before
  the destruction tick and clearing with it; and this
  review records honestly that Slice 14 is still not
  playtested, balanced, or Unity-verified. The Slice 1
  director playtest gate is still pending throughout and
  is not closed by Slice 14.
- Two staging corrections this step pins, so they are not
  over-claimed later. First, the demo's first execution
  staged the pylon enemy at (10,5,19), as Slice 13 had,
  and the pylon fired twice (30 -> 20 -> 10) before the
  enemy walked to Manhattan distance 6 and out of range,
  surviving at 10 HP — the demo as first staged proved a
  two-hit escape, not a kill, so the enemy was re-staged
  to (10,5,17), where it dies on tick 3 as recorded above.
  A reader who quotes this demo as a range-5 boundary
  proof is misquoting it: the kill happens at shrinking
  range (the enemy moves east each tick), and the
  stationary range-5 boundary rests on the Slice 12 Step
  2 tests, as in the Slice 12 and Slice 13 demo notes.
  Second, the demo's first execution also exposed the
  lone-count finding below: the HUD read 0 LONE SOURCES
  where the draft text expected 1, and the draft text was
  corrected, not the count — the count follows the Slice
  12 definition, and the definition, not the demo, is
  what the review carries as a question.
- Two staging facts this step pins, so they are not
  over-claimed later. First, the demo stages its layout
  directly with `TrySet`, exactly as the Step 1 tests stage
  theirs, because aim placement of a Conduit was proved
  in Step 1's tests; the demo claims composition of core
  rules, not aim placement of this exact layout, and it
  contains no squads, so it proves electrical composition
  only, not orbital composition — the orbital proof stays
  in the earlier slices' demos and tests. Second, the
  player never walks in this demo: the step-down bench
  sits beside the spawn by staging, so the regen reading
  is a spawn-bench reading, not a bench-walk measurement
  as in Slices 10 and 13 — a reader who quotes it as a
  walk procedure is misquoting it.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (cable-only HV
  delivery destroying a mill, direct adjacency delivering
  nothing, transformer step-down from a Conduit cable,
  the pylon kill, mill gather rates, bench regen,
  save/load re-derivation, and the two count findings
  below) and the HUD numbers, but it does not prove that
  pressing M builds a Conduit in the scene, that a
  single-cable cell reads untinted in the scene, or that
  a two-cable run reads yellow on both cells in the scene
  — a printed power state does not prove a visible tint.
  Those are adapter claims with a Windows checklist item
  (28 for the Conduit key, which already asks the scene
  observer to record the single-cable untinted cell and
  the two-cable yellow run honestly), and this review
  adds no new checklist item for the demo itself, because
  the demo exercises no adapter binding the checklist does
  not already cover.
- No Slice 14 step duplicates logic in a Unity adapter.
  Core owns the source, the delivery rules, the power-state
  query, and the save bytes; the adapter only feeds the M
  key press into the existing builder. The delivery rules
  cannot be compiled headlessly as adapter code — they are
  core code, tested, and the adapter line is written and
  checklist-listed, not a test-count claim.
- No Slice 14 step changed a saved value's meaning: the save
  format gains no field, BlockType values through ShockPylon
  = 12 are unchanged with Conduit appended as 13, and every
  earlier demo save length above is unchanged. The new
  scenario's save length (43,974 chars) is a composition
  fingerprint for this scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (delivery, destruction, and gather are per-tick
  phases with no planning pauses), no twin-stick Asteroids
  loop, no copied assets, lore, code, or trademarks. The
  Conduit is an original delivery rule, not a copied
  mechanic; the blueprint's electrical vocabulary was taken
  as names only and given tested meanings the blueprint
  never stated.

## Deliberately NOT in Slice 14 (still open, not forgotten)

- A second third source, a third consumer, or any other
  electrical widening added alongside the Conduit as a
  bonus. One source is Slice 14; a source family would
  have made the distinctness proof untestable in
  isolation.
- A third voltage level, or any ladder redesign. The ladder
  stays LV / HV / None, and the Conduit duplicates the
  Alternator's level with a different delivery shape.
- A condition, a fuel, or a pulsed output for the Conduit.
  Those hypotheses were rejected in Step 1 with reasons,
  not left undecided, and this review does not revive
  them silently.
- A fix to either finding below smuggled into a demo step.
  A demo step must not move the rules it demonstrates; a
  query or count redesign is a decision step of its own,
  with its own tests, for Slice 15 scoping to weigh.
- A balance pass. The Conduit's cost (5 Stone + 5
  EmberOre) and HV output are first-pass values chosen for
  determinism and testability, exactly as the pylon's range
  5 and damage 10 were, and must be labelled as such, not
  quoted as final feel.
- Patrol, a fourth squad kind, or a second doctrine. The
  orbital side is closed for this slice; an electrical
  slice that also adds a doctrine would make the source
  proof untestable in isolation.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 14 markers remain scene markers in
  the FPS view, not a second camera.
- A new movement mode, a weapon rework, or a free-deploy
  enforcement change. The free `DeploySquad` primitive
  remains a headless-only rule, re-named in the Slice 3
  review's gaps, not quietly fixed here.
- Windows Unity verification itself. It remains open for
  Slices 1–14 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–14.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 14 adds item 28 (Conduit key, including the
   single-cable scene observation and the two-cable yellow
   run) to that plan. None of these items is claimed
   verified by any test count in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 14 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Electrical balance is unproven, not just unverified.**
   The Conduit's cost and HV output are first-pass values
   for determinism and testability. A Windows playtest may
   find the Conduit too cheap to scatter, or too dear
   against the Alternator's stone-only price — and that
   observation, not this review, justifies retuning, with
   its own changelog entry. Both directions are open until
   played, and neither is decided here.
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
   13 remainder for Slice 15 scoping, not a silent gap.
5. **The single-cable tint finding is still open.**
   Carried from Step 1 and re-proved in composition here:
   a single cable cell touching a Conduit reads None in
   PowerState (untinted) while its network is live — its
   pylon fires, its mill is destroyed — because
   `DeliveredVoltage` for a cell counts only its
   neighbours and a Conduit is not a direct source, while
   a two-cable Conduit run reads Energised on both cells.
   The question for Slice 15 scoping is whether the
   power-state query for a cable cell should also count
   its own component's voltage, which would change tint
   behaviour for every existing source's cables as well
   and must therefore be decided as its own step, with
   its own tests, not patched inside a demo. Until then,
   checklist item 28 asks the scene observer to record
   the untinted cell honestly.
6. **The direct-Conduit count invisibility is still open.**
   Found by this demo, not by Step 1: a Conduit standing
   directly beside a mill counts as neither a live network
   nor a lone source, because the Slice 12 lone rule
   disqualifies any source beside a consumer, whatever it
   delivers — so a player who places a Conduit beside a
   mill sees POWER 0 LIVE, 0 LONE, while the mill reads
   None and gathers kinetically. That is the Slice 12
   definition applied consistently, not a miscount, and
   this review does not call it a bug: the count answers
   "placed sources with no wiring", where wiring includes
   consumer adjacency. But it is a genuine visibility
   question for Slice 15 scoping — whether the counts
   should distinguish a source beside a consumer that
   delivers nothing from one that delivers, or whether
   the mill's None state and kinetic cadence are
   sufficient readout — and it must be decided with its
   own tests, not by redefining a count inside a review.
7. **Patrol is deferred, not designed.** The route question
   it needs — loop, back-and-forth, or waypoints, and where
   the route is stored — is still unanswered, and a later
   doctrine step must answer it with its own tests before
   Patrol can be claimed, not inherit Defend Core's chase
   rule by analogy.
8. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the deferred
electrical widening from Slice 12 (its gap 5 in the Slice
13 review's numbering, carried as Slice 14's work by the
0.61.0 scope). Step 1 added the third source with a
distinctness beyond voltage, and Step 2 proved it in
composition, so a third source is no longer deferred — it
is built, tested, and reviewed as the Conduit. It is
recorded here as closed in that sense, not left in the
numbered gaps above, so a later review does not carry it
again by accident. The single-cable tint finding and the
direct-Conduit count question above are new remainders
about how the Conduit reads, not a reopening of whether
a third source exists.

## Slice 14 verdict (headless)

Slice 14 is **reviewed and closed as a tested third power
source plus a composed demo (one cable-only HV source —
a Conduit beside a consumer delivers nothing, a Conduit
touching a cable delivers HV, safe LV use costs a
transformer step-down, with the full mismatch proof set
for HV — proved together in one composed demo through a
save/load round-trip, with a cable-fed mill destroyed on
tick 1, a Dynamo mill gathering 5 in 5 ticks beside it, a
direct-Conduit mill gathering 1 kinetic stone in 5 ticks
beside that, a Conduit-fed pylon killing an enemy in
three 10-damage ticks, a Conduit step-down bench reading
regen 10 from spawn, warnings reading before the
destruction tick and clearing with it, and no new save
field)**. It is not playtested, not balanced, and not
Unity-verified, and its adapter claims must not be quoted
as if the test count covered them.
**Slice 15 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 14's composition evidence in
hand rather than guessed now.

## Ranked candidates for Slice 15 scoping (starting point, not promised)

Carried from the 0.61.0 scope, in the same order, because
the Step 2 evidence gives no reason to reorder the top two,
with the new remainders ranked after them as the review's
own findings, not as a promise:

1. Patrol, still deferred with its route question
   intact — a loop, a back-and-forth, waypoints,
   and where the route is stored — not designed by
   this scope.
2. Any Slice 14 remainder the Slice 14 review names (the
   single-cable tint query question in gap 5, and the
   direct-Conduit count question in gap 6) — recorded at
   review time, not promised here. Either would be a
   display-query decision step with its own tests, not a
   patch inside another step.
3. Any Slice 13 remainder the Slice 13 review names (the
   vertex-colour material observation, the MeshRenderer
   wiring observation, and the marker-occlusion observation
   in gap 4 above) — recorded at review time, not promised
   here.
