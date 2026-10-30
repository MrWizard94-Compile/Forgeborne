# Forgeborne — Slice 9 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1135/1135.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, and 8. This review closes Slice 9 as a reviewed
whole; it does not claim Slice 9 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 9 close the electrical gap Slice 8 left open — a
power system with exactly one voltage, always on while
placed, and exactly one distribution rule (a mill is
powered only when face-adjacent to a Dynamo), with no
conversion, no wiring at a distance, and no failure mode —
with a voltage ladder and a transformer first, then cable
distribution, then mismatched-voltage destruction, then a
composed demo proving all three together, without adding
a new consumer, a new source, a third voltage level, a
per-hop loss, a cable length limit, a drag-to-lay mode, a
third squad type, a new doctrine, a balance pass, or a
save-format field, and without breaking kinetic mill
behaviour, matched-voltage every-tick cadence, Dynamo
cost, squad price and cap, loot, skills, core health, or
old saves? The answer headlessly is yes for the systems
below, with the gaps and next candidates listed honestly
at the end — and with one claim split the review must keep
split: the core voltage, distribution, and destruction
rules are tested, the adapter keys and any indicator are
written and checklist-listed, and neither claim borrows
the other's evidence.

## What Slice 9 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.39.0 scope | `docs/slice-09-scope.md`: Slice 9 scoped as electrical completion, in order — voltage ladder and transformer, then cable distribution, then mismatched-voltage destruction, then demo + review. Ranked Slice 10 candidates recorded, not promised | Decision document only, no system built by it | Tests 929/929 unchanged |
| 0.40.0 voltage ladder and transformer | New `src/Forgeborne.Core/Voltage.cs`: exactly two levels, LV and HV, because two suffice and more would be unused names. Dynamo outputs LV, so existing Dynamos keep working; a mill is powered when its delivered voltage equals its rating, LV. New `BlockType.Transformer = 9`, appended only, 5 EmberOre, one bidirectional block: LV in gives HV out, HV in gives LV out, so a round-trip Dynamo -> transformer -> transformer -> mill returns to LV. `WorldZone.DeliveredVoltage` (Dynamo face-adjacency gives LV, otherwise first face-adjacent transformer output; direct Dynamo beats converted) and `TransformerOutput` (Dynamo input beats transformer input, visited set so a Dynamo-less ring reports None). Unmatched voltage in this step falls back to kinetic cadence, pinned openly as the interim rule Step 3 replaces. `Simulation.BuildTransformerAhead` through the aimed builder; adapter U key, suppressed while the picker is open; `HudState` power count restated as rated-voltage only; playtest-gate checklist item 15 | Voltage is derived every tick from placement, never stored, so no save-format field is added and an old save's Dynamo loads at LV by the DynamoOutput rule; the interim fallback is a named behaviour, not a silent default, so Step 3 can change it as a recorded behaviour change | Tests 975/975; Dynamo-against-mill still LV / powered exactly as before; transformer -> mill delivers HV and the mill falls back to kinetic (1 in 5 ticks); two transformers return to LV (5 in 5); direct Dynamo beats converted HV for mill and transformer; unpowered transformer and Dynamo-less ring deliver None; removing the Dynamo drops converted voltage on the next tick; save/load re-derives HV and LV with no new flag; HUD counts 2 mills / 1 powered for one LV + one HV mill |
| 0.41.0 cable distribution | New `BlockType.Cable = 10`, appended only, 1 EmberOre per cell, solid and not gatherable, same air / support / check-before-spend rules. `DeliveredVoltage` widened: the single agreed voltage from face-adjacent transformer outputs and cable components, direct Dynamo still beating any converted or cabled voltage. `CableComponent` is face-adjacent Cable cells only (walls, workstations, mills break the network); `CableNetworkVoltage` carries one voltage with no loss and no length limit. A transformer in a run is a boundary between two cable components, computed with the fed component excluded as its input and the feeding component excluded as its source. Mixed-voltage networks and mills touching two voltages resolve to None, not to scan order or higher-wins, and removing one source resolves the network to the remaining voltage on the next check. `Simulation.BuildCableAhead` through the aimed builder, one cell per press, no drag-to-lay; adapter I key, next to U, suppressed while the picker is open; playtest-gate checklist item 16 | Cables extend the Slice 5 rule, not replace it: Dynamo-against-mill still powers exactly as before, and a mill face-adjacent to an energised cable runs at the cable's voltage through the same powered phase and cadence; cable networks, like powered state, are derived at load, so no network field is saved | Tests 1023/1023; Dynamo -> cable -> cable -> mill gathers 5 in 5 ticks at powered cadence and never excavates its cable; a ten-cable run still carries LV; one network branches to two mills, 5 each in 5 ticks; a wall between cables leaves the far mill at kinetic 1 in 5 ticks; diagonal Dynamo-to-cable delivers nothing; source-side mill LV / powered 5 in 5 while beyond-transformer mill HV / kinetic 1 in 5 in the same run; mixed LV+HV cable network delivers None, then LV after the transformer source is removed; mill touching LV cable + HV transformer delivers None; cable runs survive save/load and re-derive LV and HV with no new flag; HUD counts 2 mills / 1 powered for one LV + one HV cable mill |
| 0.42.0 mismatched-voltage destruction | `WorldZone.IsMismatched` (delivered voltage neither None nor rated LV — on this ladder, exactly HV) plus `RunMismatchedDestruction`, every tick, chunk-index scan order, before either automation phase, so a destroyed mill excavates nothing on its destruction tick even on a 5th tick. Destroyed cell becomes Air; `DestroyedMills` session counter, not saved. Salvage none, cascade none: cable, transformer, Dynamo, and neighbouring mills all survive, each judged on its own voltage. Transformers convert both levels, so HV input does not destroy them; cables are voltage-blind carriers that never break. No new adapter binding, no new HUD line — the HUD power line already drops because a destroyed mill leaves TotalMills; playtest-gate checklist item 17 covers a destroyed mill's disappearance in the scene, Windows-only | Destruction is world state in the chunk bytes, not a flag: a destroyed mill stays destroyed across save/load, while only the session counter resets on load, like the gather counters. This replaces, as a recorded behaviour change, the Step 1 / Step 2 interim HV kinetic fallback: the two tests that pinned that fallback are rewritten here and named in the changelog, and delivery, precedence, and save/load assertions around them stand unchanged | Tests 1099/1099; direct Dynamo -> transformer -> mill HV destroyed tick 1, 0 kinetic / powered, pocket stone untouched, transformer intact; cable-run HV mill destroyed tick 1 with inventory unchanged (no stone / wood / ore refund) and cable / transformer / Dynamo intact; 5th-tick ordering via RestoreTick(4) — destroyed, 0 kinetic; LV mill beside an HV mill gathers 5 in 5 ticks while the HV mill is gone; branch west LV mill gathers 5 in 5 after east HV mill destroyed, all cables / transformer / Dynamo survive; conflicted (None) and unpowered mills survive 5 ticks at kinetic 1, never destroyed; direct-Dynamo precedence mill receives LV, survives, gathers its 4 stone neighbours at powered cadence; round-trip HV-fed transformer survives and its mill gathers 5 in 5; save/load keeps the destroyed mill Air with no refund, surviving LV mill still powered, destruction counter resets to 0; HUD 2 mills / 1 powered before, 1 mill / 1 powered after |
| 0.43.0 demo scenario + review | This review, plus a composed Slice 9 demo scenario: one Simulation in which an LV cable mill, a round-trip LV mill, an HV cable mill destroyed on tick 1, and a kinetic mill all run together, through a save/load round-trip and a loaded 5-tick phase. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in their checklist items (15, 16, 17) | Tests 1135/1135; demo Step 4 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1135 passed, 0 failed**
  (was 1099; +36 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the
  round-trip LV delivery and the tick-1 destruction, so no
  test or demo fix was needed in this step).
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
  44,013 chars); Slice 8 Step 3 scenario unchanged (gathered 1,
  built 1, killed 1, save 43,974 chars). New Step 4 scenario,
  in full: one Simulation, four mills on the y=20 plane in
  rows z=10, 14, 18, and 22, each with a five-stone pocket
  (its five non-infrastructure neighbours). Voltages before
  any tick: cable mill LV, round-trip mill LV, HV cable mill
  HV, kinetic mill None; HUD 2/4 MILLS POWERED — only the two
  LV mills count, so the HV mill is never displayed as powered
  in its one tick of life. Tick 1: the HV mill's cell becomes
  Air, destroyed 1, powered gathered 2 (one from each LV mill),
  kinetic gathered 0, stone 2, the HV pocket stone at (9,20,18)
  still Stone, and its cable, transformer, and Dynamo all still
  standing; HUD 2/3 MILLS POWERED, the total dropping with the
  destroyed mill and no new HUD line needed. After 5 ticks:
  powered gathered 10 (5 + 5), kinetic gathered 1, destroyed
  still 1, stone 11 (5 + 5 + 1, no refund for the destroyed
  mill). Save / load: cable mill, round-trip mill, and kinetic
  mill survive, the HV mill's cell stays Air (no resurrection,
  no refund on load), power still 2/3, voltages re-derived as
  LV and LV with no stored voltage flag, session counters reset
  to powered 0 / kinetic 0 / destroyed 0, stone still 11. The
  loaded world then re-seeds the two powered pockets (labelled
  as fresh seeding, not leftovers, as in the Slice 5 and Slice 6
  demos) and runs 5 more ticks: powered gathered 10 again
  (5 + 5), kinetic gathered 1 again, destroyed 0 (nothing new
  destroyed, because no HV mill remains), stone 22 (11 + 11).
  Save JSON 43,975 chars — a composition fingerprint for this
  scenario only, not a balance number.
- Slice 9 gate, checked against `docs/slice-09-scope.md`:
  every step pushed with passing tests; the voltage ladder
  (exactly two levels, LV and HV) and the transformer
  conversion (one bidirectional block, round-trip proved)
  are each decided and tested, with existing Dynamos loading
  at LV and old saves still loading (no save-format field
  added in any Slice 9 step — Transformer = 9 and Cable = 10
  are appended block values in the chunk bytes, like every
  block); cable distribution carries a stated voltage beyond
  face-adjacency, face only, through branches and through a
  transformer in the run, with mixed-voltage network
  resolution decided (None) and tested; mismatched voltage
  destroys a mill by the stated semantics (tick of delivery,
  destruction phase before either automation phase, chunk-index
  scan order), with salvage (none) and cascade (none) each
  decided and tested, and undervoltage decided openly (None
  on a two-level ladder, so a starved or conflicted mill idles
  at kinetic cadence and is never destroyed) rather than left
  undefined; kinetic mill behaviour and matched-voltage
  every-tick cadence are unchanged; the composed demo proves
  the ladder, the cable, and the destruction rule together
  through a save/load round-trip, with the destroyed mill
  staying destroyed; and this review records honestly that
  Slice 9 is still not playtested, balanced, or Unity-verified.
- One scope-wording correction this step makes openly, so it
  is not re-derived wrong later: the scope's Step 4 said the
  second mill "runs at the converted voltage". After Step 3,
  a mill running on converted HV cannot survive — HV destroys
  it on its delivery tick — so that wording cannot be taken
  literally in the composed demo. This step proves converted
  voltage two honest ways instead: row B survives on the
  converted voltage converted back to LV (Dynamo -> transformer
  -> transformer -> mill), and row C is destroyed by the
  converted voltage itself (Dynamo -> transformer -> cable ->
  mill, HV). A reader who replays the scope's sentence alone
  will see the discrepancy explained here, not hidden, and
  the tests assert both rows, so neither reading can drift.
- One staging fact this step pins, so it is not over-claimed
  later: the demo stages its four-row layout directly with
  TrySet, exactly as the Step 1-3 tests stage theirs, rather
  than placing each block by aim. Aim placement of a
  transformer (Step 1 test, `BuildTransformerAhead`) and of a
  cable (Step 2 test, `BuildCableAhead`) were each proved in
  their own steps, and walking four aim placements in this
  run would have proved walking, not wiring. The demo
  therefore proves the composition of the core rules, not
  aim placement of this exact layout, and no review sentence
  here claims otherwise.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (delivered voltages,
  destruction tick and survivors, powered vs kinetic rates,
  save/load world state and re-derived power) and the HUD
  counts, but it does not prove that the U or I key feels
  right, that a voltage line reads right in the scene, or
  that a destroyed mill visibly disappears in Unity. Those
  are adapter claims with Windows checklist items (15 for U,
  16 for I, 17 for destruction disappearance, in
  `docs/slice-01-playtest-gate.md`), and this review adds no
  new checklist item for the demo itself, because the demo
  exercises no adapter binding the checklist does not already
  cover — the same no-new-item rule the Slice 7 and Slice 8
  reviews applied, for the same reason.
- No Slice 9 step duplicates logic in a Unity adapter.
  Core owns the ladder, the conversion, the network walk,
  the conflict resolution, and the destruction rule; the
  adapter only feeds U / I key presses into the existing
  aimed builders and renders the existing HUD power count.
  The ladder, network, and destruction rules cannot be
  compiled headlessly as adapter code — they are core code,
  tested, and the adapter bindings are written and
  checklist-listed, not test-count claims.
- No Slice 9 step changed a saved value's meaning: the save
  format gains no field, and every earlier demo save length
  above is unchanged. The new scenario's save length
  (43,975 chars) is a composition fingerprint for this
  scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (destruction, powered, and kinetic phases are
  per-tick rules, with no planning pauses), no twin-stick
  Asteroids loop, no copied assets, lore, code, or
  trademarks. The LV / HV ladder, the bidirectional
  transformer, the cable network, and the destruction rule
  are original core rules, not copied mechanics; the
  blueprint's "LV to EV" vocabulary was treated as a
  hypothesis, not adopted — Slice 9 builds two levels, not
  four, and says why.

## Deliberately NOT in Slice 9 (still open, not forgotten)

- Additional powered consumers or power sources: a
  powered workstation, a second powered behaviour for
  an existing machine, an alternator as a distinct
  source from the Dynamo, or any other consumer or
  source beyond the mill Slice 5 already powers. One
  consumer proves the ladder, the cable, and the
  destruction rule against a known baseline; a second
  consumer is the first ranked Slice 10 candidate below,
  not Slice 9 scope creep.
- Automation tiers 3-4 (digital / arcane), crafting,
  fuel or power beyond the Dynamo source Steps 1-3 use,
  and a per-machine inventory system beyond the player
  inventory rule Slice 5 decided. Tier 1 and powered
  output stay in the player inventory unless a later
  consumer step justifies changing that rule for its
  own behaviour.
- A per-hop voltage loss, a cable length limit, or a
  drag-to-lay placement mode. None was decided in Step 2
  as its own stated rule with a boundary test, so none
  rides along as an untested extra.
- A third squad type, or a second kind added alongside
  anything else as a bonus. One new kind was Slice 7;
  the next kind, if any, is a Slice 10-or-later decision
  with Slice 9's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a power behaviour.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 9 wiring stays in the FPS view,
  not a second camera.
- A new movement mode, a weapon rework, or a balance
  pass. Any voltage count, cable cost, transformer cost,
  or salvage rule the steps chose is a first-pass value
  for determinism and testability, exactly as Slice 3's
  prices and Slice 5's Dynamo cost were, and must be
  labelled as such, not quoted as final feel.
- A pre-destruction warning (a HUD flash, a sound, or a
  marker). Step 3 named fairness signalling as its own
  future adapter step with its own checklist item, not
  as a core rule, and this review does not retrofit it.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- A mesh tint for Dynamo / powered mills / cables / a
  transformer, a mesh refresh on any key build, and a
  distinct Recon marker. All three remain named gaps
  from 0.30.0 / 0.33.0, not fixed by an electrical step.
- Windows Unity verification itself. It remains open
  for Slices 1-9 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–9.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 9 adds items 15 (U key, transformer), 16 (I key,
   cable), and 17 (destroyed mill's disappearance) to that
   plan; none is claimed verified by any test count in
   this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 9 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Electrical balance is unproven, not just unverified.**
   Transformer cost (5 ore), cable cost (1 ore per cell),
   the two-level ladder, and no salvage are first-pass
   values for determinism and testability. A Windows
   playtest may find a cable run too cheap to matter, a
   transformer too dear to bother converting, or no
   salvage too punishing for a wiring mistake — and that
   observation, not this review, justifies retuning, with
   its own changelog entry.
4. **Destruction has no warning, by decision, not by
   oversight.** Step 3 recorded that a warning flash,
   sound, or marker would be a second feature — fairness
   signalling — and this review keeps it as its own future
   adapter step. Until then, a player's first HV mill
   simply disappears on its delivery tick, and the only
   headless trace is the HUD total dropping and the
   `DestroyedMills` session counter.
5. **Mesh tint and mesh refresh remain named gaps.** A
   Dynamo, transformer, cable, or powered mill is still
   not rendered distinctly, and no adapter refreshes the
   voxel mesh on any key build, so a cable laid by aim or
   a mill destroyed by voltage may be counted before it
   is visible in the scene. Both were named in 0.30.0,
   and neither is closed by a review step.
6. **An energised cable feeding no mill shows nothing.**
   Carried from Step 2: the HUD has no cable or
   energised-network count, so a live cable network with
   no mill on it is invisible in the HUD as well as in
   the scene. A later step that adds a consumer or an
   indicator must decide this display question for its
   own behaviour, not inherit silence by accident.
7. **Two kinds share one marker.** Decided, not forgotten:
   if a Windows playtest shows players mistaking a Recon
   for a Vanguard in the scene, that observation, not
   this review, justifies a distinction, with its own
   checklist item.
8. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 9 verdict (headless)

Slice 9 is **reviewed and closed as tested electrical tier 2
core — a two-level voltage ladder with a bidirectional
transformer, cable distribution that carries a voltage
beyond face-adjacency through branches and through a
transformer in the run, and mismatched-voltage destruction
with no salvage and no cascade, proved together in one
composed demo through a save/load round-trip — plus written
adapters (U / I keys, rated-voltage HUD count)**. It is not
playtested, not balanced, and not Unity-verified, and its
adapter claims must not be quoted as if the test count
covered them. **Slice 10 is not scoped by this review** — its
scope is the decision for the next run, chosen from the
ranked candidates below, so it is chosen with Slice 9's
composition evidence in hand rather than guessed now.

## Ranked candidates for Slice 10 scoping (starting point, not promised)

Carried from the 0.39.0 scope, in the same order, because
the Step 4 evidence gives no reason to reorder them:

1. Additional powered consumers and power sources
   (a powered workstation or a second powered behaviour,
   an alternator as a distinct source), deferred, not
   rejected.
2. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 9's electrical evidence will inform, not promises
   made before that evidence exists.
