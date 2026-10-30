# Forgeborne — Slice 10 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1300/1300.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, and 9. This review closes Slice 10 as a reviewed
whole; it does not claim Slice 10 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 10 close the electrical-widening gap Slice 9 left open — a
power system with exactly one powered consumer (the Kinetic Mill) and
exactly one power source (the Dynamo outputting LV, with HV existing
only as a transformer's flip of that LV), so that every powered
behaviour was one machine running every tick and every volt came
from one generator — with one second powered consumer first, then
one distinct power source proved against both consumers, then a
composed demo proving both together, without adding a third
consumer, a second source, automation tiers 3-4, a fuel system, a
per-hop loss, a cable length limit, drag-to-lay, a third squad
type, a new doctrine, a balance pass, or a save-format field, and
without breaking the ladder, conversion, cable, or destruction
rules from Slice 9, kinetic mill behaviour, matched-voltage
every-tick cadence, Dynamo cost, squad price and cap, loot,
skills, core health, or old saves? The answer headlessly is yes
for the systems below, with the gaps and next candidates listed
honestly at the end — and with one claim split the review must
keep split: the core consumer and source rules are tested, the
adapter keys and any indicator are written and checklist-listed,
and neither claim borrows the other's evidence.

## What Slice 10 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.44.0 scope | `docs/slice-10-scope.md`: Slice 10 scoped as electrical widening, in order — one second powered consumer, then one distinct power source, then demo + review. Ranked Slice 11 candidates recorded, not promised | Decision document only, no system built by it | Tests 1135/1135 unchanged |
| 0.45.0 second powered consumer | The existing Workstation block, not a new block type, rated LV by its own constant (`VoltageRules.WorkstationRated`), with its own `IsWorkstationPowered` / `IsWorkstationMismatched` — neither `IsPowered` nor `IsMismatched` reused, because both are mill-only by construction. Powered behaviour is a stated strengthening of the existing effect, not a new effect: a powered workstation near the player regenerates base + 5 (additive with the skill bonus already in `WorkstationRegenPerTick`), in the existing workstation phase, and produces nothing, so no per-machine inventory is built. Mismatch is decided, not inherited: an HV-fed workstation is disabled for that tick (regen 0), not destroyed, and works again when rewired to LV or left unpowered — a repair bench tripping offline is a recoverable wiring mistake, where destroying it would double-punish a support structure and remove the counterplay a player needs to survive the mistake. Unpowered behaviour is unchanged (base regen). New `Simulation.BuildWorkstationAhead` (10 Stone + 5 EmberOre, the workstation's existing price) plus a B adapter key, suppressed while the picker is open; `HudState` gains a powered/total workstation count, with HV counting as NOT powered, so the count never calls a disabled bench powered | Power is derived every tick from placement, never stored, so no save-format field is added; the workstation phase runs where it always ran (after the enemy phase, before the squad phase), and the powered bonus is a player effect, not an excavation, so it does not touch the powered/kinetic automation phases | Tests 1157/1157; LV-fed workstation powered at its own rating, unpowered workstation unpowered and not mismatched, HV-fed workstation mismatched, surviving its tick, restoring nothing, then powered again after rewiring with no rebuild; powered regen 10 per tick (base 5 + 5), powered + skill 15 per tick; aimed workstation build through `BuildWorkstationAhead`; HUD counts 1/1 powered for LV, 1 total / 0 powered for HV |
| 0.46.0 distinct power source | New `BlockType.Alternator = 11`, appended only, a native HV generator (`VoltageRules.AlternatorOutput`), not a Dynamo reskin: an LV-outputting second source at Dynamo cost would have been proved by the same tests a Dynamo passes, while HV output is proved by tests a Dynamo cannot pass (direct feed destroys a mill and disables a workstation). Always-on while placed, like a Dynamo: no fuel or condition is invented, so a failed delivery stays unambiguous (unwired, conflicted, or mismatched, never "out of fuel"). Cost 10 Stone, no EmberOre — the mirror image of the Dynamo's 5 ore, so the sources compete for different resources and the Alternator can be staged before the player has ore; safe use still costs a transformer's 5 ore, so the safe-use ore total ties a Dynamo's and the Alternator is not a cheaper Dynamo. Precedence generalised, not appended: "direct source" now means Dynamo or Alternator together, so a direct source still beats any converted or cabled voltage whichever source it is, while two distinct direct sources touching one cell, one transformer, or one cable network resolve to None, not to a silent winner. Cable networks carry Alternator HV with the same mixed-network rule. New `Simulation.BuildAlternatorAhead` plus an N adapter key, suppressed while the picker is open, and playtest-gate checklist item 18, Windows-only | Source output, like Dynamo output, is derived at load from placement, so no save-format field is added and old saves load unchanged; Alternator step-down chains re-derive LV on load. No change to the ladder, conversion, cable loss or length rules, either consumer's rating, powered behaviour, mismatch rule, or cost, or Dynamo cost or output | Tests 1239/1239; direct Alternator mill destroyed tick 1 (face-adjacent and through cable), pocket untouched, Alternator surviving; face-adjacent step-down mill LV, 5 in 5 ticks; face-adjacent step-down workstation LV, regen 10/tick; direct HV workstation mismatched, regen 0, survives; cable step-down mill LV, 5 in 5; cable HV mill destroyed tick 1; cable step-down workstation LV, regen 10/tick; Dynamo+Alternator mill / transformer / cable-network conflicts all resolve to None with kinetic fallback and survival; direct Alternator beats converted LV (mill destroyed); aimed Alternator build; save/load chunk equal with both Alternator chains re-deriving LV; HUD 1/1 mills and 1/1 workstations for Alternator-fed consumers, lone Alternator counting as neither |
| 0.47.0 demo scenario + review | This review, plus a composed Slice 10 demo scenario: one Simulation in which a cable step-down Alternator mill and a step-down Alternator workstation both run powered, a kinetic mill and an unpowered workstation still run at their unpowered behaviours beside them, a direct-HV mill is destroyed on tick 1 and a direct-HV workstation is disabled but survives, through a save/load round-trip and a loaded 5-tick phase. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in their checklist items (17 for destruction disappearance, 18 for the N key; the B key has no checklist item yet, the gap Step 2 observed and carried here) | Tests 1300/1300; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1300 passed, 0 failed**
  (was 1239; +61 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the cable
  step-down delivery, the tick-1 destruction, the three
  bench regen readings, and the loaded phase, so no test
  or demo fix was needed in this step).
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
  22, save 43,975 chars). New Step 3 scenario, in full: one
  Simulation, three mills on the y=20 plane and three
  workstations on the y=5 plane beside the player's spawn.
  Voltages before any tick: cable step-down mill LV,
  direct-HV mill HV, kinetic mill None, step-down workstation
  LV, unpowered workstation None, HV workstation HV; HUD 1/3
  MILLS POWERED and 1/3 WORKSTATIONS POWERED — only the two
  LV consumers count, so the HV mill is never displayed as
  powered in its one tick of life and the HV workstation is
  never displayed as powered while disabled. Tick 1: the HV
  mill's cell becomes Air, destroyed 1, powered gathered 1,
  kinetic gathered 0, stone 1, the HV pocket stone at
  (9,20,14) still Stone, its Alternator still standing, the
  HV workstation still standing, and shields 65 -> 75 (the
  powered bench's base 5 + powered 5, after a 20-damage
  opening); HUD 1/2 MILLS POWERED (the total drops with the
  destroyed mill) and 1/3 WORKSTATIONS POWERED (the disabled
  bench stays in the total, not the powered count). The
  player then walks between the benches, with no tick
  passing, and the regen readings are 5 at the unpowered
  bench (base, no source), 0 at the HV bench (disabled —
  not destroyed, not boosted), and 10 again at the powered
  bench, proving the powered reading is the bench, not the
  walk. After 5 ticks: powered gathered 5 (pocket
  exhausted), kinetic gathered 1, destroyed still 1, stone
  6 (5 + 1, no refund for the destroyed mill), shields
  capped at 85. Save / load: cable step-down mill, kinetic
  mill, and all three workstations survive, the HV mill's
  cell stays Air (no resurrection, no refund on load),
  power still 1/2 mills and 1/3 workstations, voltages
  re-derived as mill LV, powered workstation LV, and HV
  workstation HV with no stored voltage flag, session
  counters reset to powered 0 / kinetic 0 / destroyed 0,
  stone still 6, and the player back at the powered bench
  (16,5,16) with effective regen 10. The loaded world then
  takes 20 damage, re-seeds the powered mill's pocket
  (labelled as fresh seeding, not leftovers, as in the
  Slice 9 demo), and runs 5 more ticks: powered gathered
  5 again, kinetic gathered 1 again, destroyed 0 (nothing
  new destroyed, because no HV mill remains), stone 12
  (6 + 6), shields 65 -> 85 capped. Save JSON 43,974 chars
  — a composition fingerprint for this scenario only, not
  a balance number, and the same length as the Slice 5 and
  Slice 8 saves by coincidence of format, not the same save.
- Slice 10 gate, checked against `docs/slice-10-scope.md`:
  every step pushed with passing tests; the second
  consumer's powered behaviour (base + 5 regen), rating
  (LV by its own constant), mismatch rule (disable, not
  destroy), placement path (aimed builder + B key), cost
  (the workstation's existing 10 Stone + 5 EmberOre), and
  display (powered/total workstation count, HV counting
  as NOT powered) are each decided and tested, with its
  unpowered behaviour unchanged (base regen, proved in
  this demo beside the powered bench, not just in Step 1
  tests); the distinct source's output level (native HV),
  precedence (direct-source rule generalised, distinct-source
  conflicts resolving to None), requirements (always-on,
  no fuel or condition), cost (10 Stone, no ore), and
  display (no new source count, by the Dynamo precedent)
  are each decided and tested, proved feeding both
  consumers through face-adjacency and through cable, and
  not merely a Dynamo reskin (its HV proofs are tests a
  Dynamo cannot pass); the ladder, conversion, cable, and
  destruction rules from Slice 9 are unchanged except for
  the precedence generalisation Step 2 itself justified and
  the workstation mismatch rule Step 1 itself justified, both
  of which leave the mill's rules untouched; the composed
  demo proves the consumer and the source together through
  a save/load round-trip, with the destroyed block staying
  destroyed and power re-derived on load without a stored
  flag; and this review records honestly that Slice 10 is
  still not playtested, balanced, or Unity-verified.
- One staging fact this step pins, so it is not over-claimed
  later: workstation regen is player-centred, so the demo
  cannot prove all three benches at one player position —
  a powered bench near the player masks an unpowered bench
  near the same player, because `EffectiveWorkstationRegen`
  returns the powered rate whenever any near bench is
  powered. The demo therefore walks the player between
  benches (MoveTo, each returning True, into Air cells
  above the y=4 tree layer) and reads each bench's regen
  at its own position, with the powered bench read again
  after the walk to prove the reading follows the bench.
  That walk is a measurement procedure, not a gameplay
  claim: it proves each bench's rule at its position, and
  the tests assert the same readings, so no review
  sentence here claims three benches were proved
  simultaneously at one position. The mill rows, by
  contrast, are simultaneous: machine-centred rates let
  the powered, destroyed, and kinetic mills run in the
  same ticks, and the demo proves them that way.
- A second staging fact, carried from Slice 9: the demo
  stages its layout directly with TrySet, exactly as the
  Step 1-2 tests stage theirs, rather than placing each
  block by aim. Aim placement of a mill, a workstation,
  a transformer, a cable, and an Alternator was each
  proved in its own step's tests, and walking this layout
  in by aim would have proved walking, not wiring. The
  demo therefore proves the composition of the core rules,
  not aim placement of this exact layout, and no review
  sentence here claims otherwise. The bench walk above is
  the one exception, and it is a player-position walk, not
  a block-placement walk.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (delivered voltages,
  destruction tick and survivors, disablement survival,
  powered vs kinetic rates, powered vs base vs disabled
  regen, save/load world state and re-derived power) and
  the HUD counts, but it does not prove that the B or N key
  feels right, that a powered workstation reads right in
  the scene, or that a destroyed mill visibly disappears
  in Unity. Those are adapter claims with Windows checklist
  items (17 for destruction disappearance, 18 for the N key),
  and this review adds no new checklist item for the demo
  itself, because the demo exercises no adapter binding the
  checklist does not already cover — except for the B key,
  which has no checklist item at all, and that gap is named
  below rather than silently closed by this review, for the
  same reason Step 2 recorded it: a gate-doc repair belongs
  to a step that touches the gate doc for its own reason.
- No Slice 10 step duplicates logic in a Unity adapter.
  Core owns the ratings, the mismatch rules, the regen
  phase, the source output, the precedence walk, and the
  destruction rule; the adapter only feeds B / N key
  presses into the existing aimed builders and renders
  the existing HUD counts. The consumer and source rules
  cannot be compiled headlessly as adapter code — they are
  core code, tested, and the adapter bindings are written
  and checklist-listed, not test-count claims.
- No Slice 10 step changed a saved value's meaning: the save
  format gains no field, and every earlier demo save length
  above is unchanged. The new scenario's save length
  (43,974 chars) is a composition fingerprint for this
  scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (destruction, disablement, powered, kinetic,
  and regen phases are per-tick rules, with no planning
  pauses), no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. The LV-rated workstation with
  a disable-on-HV rule and the native-HV Alternator are
  original core rules, not copied mechanics; the blueprint's
  "alternator" name was taken as a name only and given a
  tested meaning the blueprint never stated.

## Deliberately NOT in Slice 10 (still open, not forgotten)

- A third powered consumer, or a second source added
  alongside the first as a bonus. One consumer and one
  source are Slice 10; further widening is a Slice
  11-or-later decision with Slice 10's evidence in hand.
- Automation tiers 3-4 (digital / arcane), crafting,
  and a per-machine inventory system beyond the player
  inventory rule Slice 5 decided. The workstation's powered
  effect produces nothing, so it needed no inventory, and
  none was built.
- A fuel system for the Alternator, unless a later step
  justifies one as its own decision with its own boundary
  tests — it is not assumed here, and an always-on source
  must not later gain a fuel rule by silent edit.
- A per-hop voltage loss, a cable length limit, or a
  drag-to-lay placement mode. None was decided in Slice 9
  Step 2 or Slice 10 Step 2 as its own stated rule with a
  boundary test, so none rides along as an untested extra.
- A third squad type, or a second kind added alongside
  anything else as a bonus. One new kind was Slice 7;
  the next kind, if any, is a Slice 11-or-later decision
  with Slice 10's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a power behaviour.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 10 wiring stays in the FPS view,
  not a second camera.
- A new movement mode, a weapon rework, or a balance
  pass. Any cost, output level, powered effect, or
  mismatch rule the steps chose is a first-pass value
  for determinism and testability, exactly as Slice 3's
  prices and Slice 5's Dynamo cost were, and must be
  labelled as such, not quoted as final feel.
- A pre-destruction warning (a HUD flash, a sound, or a
  marker), and likewise no pre-disablement warning for a
  workstation about to trip offline on HV. Both remain
  future adapter steps with their own checklist items, not
  core rules, and this review does not retrofit either.
- Enforcement of the free-deploy rule beyond its
  existing documentation. The free `DeploySquad`
  primitive remains a headless-only rule, re-named in
  the Slice 3 review's gaps, not quietly fixed here.
- A mesh tint for Dynamo / Alternator / powered mills /
  cables / a transformer, a mesh refresh on any key build,
  and a distinct Recon marker. All three remain named gaps
  from 0.30.0 / 0.33.0, not fixed by an electrical step.
- Windows Unity verification itself. It remains open
  for Slices 1-10 alike and is blocked on Unity 6000 on
  Windows, not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–10.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 10 adds item 18 (N key, Alternator) to that plan;
   the B key (workstation) still has no item, named as gap
   7 below, so the plan itself is incomplete for Slice 10
   until that item is written. None of these items is
   claimed verified by any test count in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 10 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Electrical balance is unproven, not just unverified.**
   Alternator cost (10 Stone, no ore), transformer cost
   (5 ore), cable cost (1 ore per cell), the powered
   workstation bonus (+5 regen), and no salvage are
   first-pass values for determinism and testability. A
   Windows playtest may find an Alternator too dear once
   its transformer is counted, a powered bench too strong
   or too weak to matter beside a mill, or no salvage too
   punishing for a wiring mistake — and that observation,
   not this review, justifies retuning, with its own
   changelog entry.
4. **Destruction and disablement have no warning, by decision,
   not by oversight.** Step 3 of Slice 9 recorded that a
   warning flash, sound, or marker would be a second
   feature — fairness signalling — and Slice 10 Step 1
   decided disablement the same way, without a warning.
   Until a later adapter step adds one, a player's first
   HV mill simply disappears on its delivery tick, and a
   player's first HV workstation simply stops regenerating
   that tick, and the only headless traces are the HUD
   totals and the `DestroyedMills` session counter.
5. **Mesh tint and mesh refresh remain named gaps.** A
   Dynamo, Alternator, transformer, cable, or powered
   mill is still not rendered distinctly, and no adapter
   refreshes the voxel mesh on any key build, so a cable
   laid by aim, an Alternator placed by aim, or a mill
   destroyed by voltage may be counted before it is
   visible in the scene. Both were named in 0.30.0,
   and neither is closed by a review step.
6. **A live network feeding no consumer shows nothing.**
   Carried from Slice 9 Step 2 and Slice 10 Step 2: the HUD
   has no cable, energised-network, or source count, so a
   live cable network with no consumer on it, and a lone
   Alternator (like a lone Dynamo before it), are
   invisible in the HUD as well as in the scene. A later
   step that adds a consumer or an indicator must decide
   this display question for its own behaviour, not inherit
   silence by accident — and this review keeps it as a
   ranked Slice 11 candidate, not a Slice 10 omission to
   be quietly patched.
7. **The workstation key (B) has no playtest-gate checklist
   item.** Observed in Slice 10 Step 2, carried here, not
   fixed here: every other builder key (T, Y, U, I, N) has
   an item, and B does not, so a Windows checker following
   the gate doc would never be asked to press B or to
   observe a powered workstation in the scene. This is a
   documentation gap, not a core gap, and closing it
   belongs to the next step that touches the gate doc for
   its own reason — but it is named here so the gate doc
   is not quoted as complete for Slice 10 while it lacks B.
8. **Two kinds share one marker.** Decided, not forgotten:
   if a Windows playtest shows players mistaking a Recon
   for a Vanguard in the scene, that observation, not
   this review, justifies a distinction, with its own
   checklist item.
9. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 10 verdict (headless)

Slice 10 is **reviewed and closed as tested electrical widening
(one second powered consumer — the LV-rated Workstation with a
disable-on-HV rule and a base + 5 powered regen — and one distinct
power source — the native-HV Alternator at a stone cost, proved
feeding both consumers face-adjacent and through cable, with
direct-source precedence generalised and distinct-source conflicts
resolving to None), proved together in one composed demo through
a save/load round-trip — plus written adapters (B / N keys,
powered-mill and powered-workstation HUD counts)**. It is not
playtested, not balanced, and not Unity-verified, and its adapter
claims must not be quoted as if the test count covered them.
**Slice 11 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 10's composition evidence in
hand rather than guessed now.

## Ranked candidates for Slice 11 scoping (starting point, not promised)

Carried from the 0.44.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 10's electrical evidence will inform, not promises
   made before that evidence exists.
2. Further electrical widening beyond one consumer and
   one source (a third consumer, a third source, or a
   display for live networks feeding no consumer) —
   deferred, not rejected, and only after Slice 10
   proves the second of each.
