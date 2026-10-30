# Forgeborne — Slice 11 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 1485/1485.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, 6, 7, 8, 9, and 10. This review closes Slice 11 as a reviewed
whole; it does not claim Slice 11 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 11 close the squad-widening gap Slice 10 left open —
a game with exactly two squad kinds and exactly two doctrines,
so that every squad a player fields is a Vanguard generalist
(3 members, 30 HP each, 10 damage each, range 3) or a Recon
screen (2 members, 20 HP each, 6 damage each, range 5), and
every squad obeys HoldGround (march one voxel per tick to a
rally, fire only once holding) or Advance (hunt the nearest
living enemy one voxel per tick, fire whenever in range, rally
ignored after deployment) — with one third squad kind first,
then one deferred doctrine proved across the kinds it actually
serves, then a composed demo proving the new kind under the
new doctrine beside an existing kind under an existing doctrine,
without adding a fourth kind, a second doctrine, a balance pass,
a new save field, or a new movement rule, and without changing
Vanguard's or Recon's numbers, HoldGround's or Advance's
behaviour, the 10-EmberOre price, or the 3-live-squad cap? The
answer headlessly is yes for the systems below, with the gaps
and next candidates listed honestly at the end — and with one
claim split the review must keep split: the core kind and
doctrine rules are tested, the adapter keys and any marker are
written and checklist-listed, and neither claim borrows the
other's evidence.

## What Slice 11 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.48.0 scope | `docs/slice-11-scope.md`: Slice 11 scoped as squad widening, in order — one third squad kind, then one deferred doctrine, then demo + review. Ranked Slice 12 candidates recorded, not promised | Decision document only, no system built by it | Tests 1300/1300 unchanged |
| 0.49.0 third squad kind | Heavy, appended as `SquadKind = 2` only, shape 2 members, 50 HP each, 8 damage each, engagement range 2 (full health 100, full damage 16 per tick), read from `SquadShapes.For` like the other kinds. It is the anchor the other kinds cannot be: a 95-damage hit kills a Vanguard (90 HP) outright and leaves a Heavy at 5 HP with 1 member, but a Heavy is silent at range 3 where a Vanguard fires for 30, and deals barely half a Vanguard's damage where it does fire, so it is a trade, not a replacement. Flyer was not chosen: it would need a movement rule with targeting and occupancy consequences, which would make the kind untestable in isolation — the scope's own warning, carried from Slice 7. Movement stays the shared one-voxel-per-tick rule for all three kinds. Price stays 10 EmberOre and the cap stays 3 live squads in total, not per kind. Adapter binding Alpha5 selects Heavy while picking, after Alpha3 Vanguard and Alpha4 Recon, with playtest-gate checklist item 20 and orbital-command checklist item 11, both Windows-only. Scene marker, decided in this step: Heavy keeps the shared cyan cube marker, stated openly in the adapter, not inherited by accident — a per-kind marker distinction is a display system for Slice 12 scoping to decide as one visibility question. Gate-doc gap closed, as assigned by the scope: because Step 1 touches the gate doc for its own checklist item, it also adds the missing B-key item (item 19), closing Slice 10 review gap 7 as a named part of this step | Kind stored as an int, loader already rejects undefined kinds, so no new save field: a 999-HP Heavy loads clamped to 100, a 999-HP Vanguard still loads at 90, and a mixed Vanguard + Recon + Heavy save round-trips all three kinds with their own health and member counts | Tests 1370/1370; demo unchanged, Slice 10 Step 3 scenario save 43,974 chars |
| 0.50.0 third doctrine | Defend Core, appended as `TacticalDoctrine = 2` only, chosen from the scope's deferred pair (Defend Core / Patrol) over Patrol, because the core objective already exists (28,5,28, 100 HP) and no squad today orders itself to it, while Patrol still needs a route definition the core does not yet have. Patrol is deferred, not rejected. Behaviour: the guard post is the core cell itself, not the rally the picker still requires to deploy — a Defend Core squad never marches to its rally. It marches one voxel per tick toward the core through the shared step rule (same axis choice, tie-break, and solid / enemy-occupied refusals, so a blocked post is held short of, not stepped through). A living enemy within 8 Manhattan of the core is an attacker the guard chases, one voxel per tick, through a single decided move per tick (a blocked chase step does not fall through to the core in the same tick); an enemy 9 or more from the core is ignored, however near it stands to the guard. With no attacker the guard returns to the core. Engagement: Defend Core fires whenever an enemy is in its kind's engagement range, Advance's fire rule, not HoldGround's hold-only rule — a chasing or returning guard shoots what it passes in range, but does not chase that target, because movement stays core-bound. The doctrine is picked independently of kind and is proved against all three kinds — Vanguard, Heavy, and Recon — not just the new one. Price stays 10 EmberOre and the cap stays 3 live squads, through the paid picker path. Display: a Defend Core squad reads Guarding at its post with no attacker, Hunting while chasing a core attacker (with that attacker's cell), and Marching on the return march. Adapter binding Alpha6 selects Defend Core while picking, after Alpha1 HoldGround and Alpha2 Advance, with playtest-gate checklist item 21 and orbital-command checklist item 12, both Windows-only | HoldGround and Advance behaviour for zones and saves that do not use Defend Core is unchanged, proved by a one-tick contrast test: same rally, no enemies — HoldGround walks west to its rally, Advance stands still, Defend Core walks east to the core. Doctrine stored as an int, loader already rejects undefined doctrines, so no new save field | Tests 1447/1447; demo unchanged, Slice 10 Step 3 scenario save 43,974 chars |
| 0.51.0 demo scenario + review | This review, plus a composed Slice 11 demo scenario: one Simulation in which a Heavy Defend Core squad and a Vanguard HoldGround squad both deploy through the paid picker, the Vanguard kills its lane enemy at its rally, the Heavy marches to the core and guards it, a staged core attacker is killed at the post, a weakened third squad dies in one tick, and the game saves and loads with kind and doctrine preserved and the dead squad still dead. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter feel and scene visibility are explicitly out of headless reach and stay in their checklist items (20 for the Heavy key, 21 for the Defend Core key, 11 and 12 in orbital-command), not in the numbers below | Tests 1485/1485; demo Step 3 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **1485 passed, 0 failed**
  (was 1447; +38 composed-integration tests for this step,
  with no test failures during the run — the demo scenario
  ran correctly on its first execution, including the 24-tick
  core march, the tick-4 lane kill, the two-tick attacker kill,
  and the dead squad's absence after load, so no test or demo
  fix was needed in this step).
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
  43,974 chars). New Step 3 scenario, in full: one Simulation,
  30 EmberOre, a Heavy Defend Core squad deployed through the
  paid picker with a dummy rally (20,5,20) and a Vanguard
  HoldGround squad deployed through the same picker to a real
  rally (20,5,8), each costing 10 EmberOre, leaving 10 ore.
  The player parks at (4,5,4) so every enemy step is a squad
  pursuit, and a lane enemy starts at (27,5,8). On tick 4 the
  Vanguard reaches its rally and reads Holding, the lane enemy
  is gone (killed at exactly range 3, the Slice 7 pattern),
  and the Heavy is at (18,5,18) reading Marching. On tick 8
  the Heavy is at (20,5,20) — its dummy rally cell, on its
  core-ward path — and still reads Marching: it passes
  through the rally without holding there, which is the
  doctrine-not-rally proof in its strongest form, not a
  coincidence to hide. On tick 24 the Heavy is at (28,5,28)
  reading Guarding, the Vanguard still Holding at its rally,
  no enemies left, core 100 HP. A core attacker is then
  staged at (26,5,28), 2 from the core: the guard reads
  Hunting with that cell as its target. On tick 25 the
  attacker steps to (27,5,28), the guard stays at its post
  (the step onto the attacker's cell is refused), fires 16
  at range 2, the attacker drops to 14 HP, and the guard
  takes one 10-damage adjacent hit, 100 -> 90 HP. On tick 26
  the guard fires again, the attacker dies, the enemy list
  is empty, and the guard reads Guarding again — chase,
  kill, and return-to-post, with the guard never leaving
  its post to hunt anything farther from the core. A third
  picker squad is then deployed at the player's feet
  (10,5,10) for a further 10 ore, weakened to 10 HP, with an
  enemy adjacent at (11,5,10): it fires once for 10 (the
  enemy drops to 20 HP) and dies to the adjacent hit in the
  same tick, leaving 2 squads, not 3. Save / load: the Heavy
  (Defend Core, at the core, 90 HP) and the Vanguard
  (HoldGround, at its rally, 90 HP) survive with kind and
  doctrine re-derived from the saved ints, the dead squad
  stays dead (2 squads loaded, not 3), the surviving enemy
  keeps its 20 HP, ore is 10, and the cap is freed by the
  death (`CanDeployOrbitalSquad` true). One loaded tick
  proves both squads still read Guarding / Holding, with no
  resurrection and no new save field. Save JSON 44,038 chars
  — a composition fingerprint for this scenario only, not
  a balance number.
- Slice 11 gate, checked against `docs/slice-11-scope.md`:
  every step pushed with passing tests; the third kind's
  shape (2 x 50 HP, 8 damage, range 2), price (10 EmberOre),
  cap (3 live squads in total), placement path (paid picker,
  Alpha5 key), binding, and display (kind, doctrine, members,
  health, damage, range, activity already printed) are each
  decided and tested, with Vanguard's and Recon's numbers
  unchanged and old saves loading; the deferred doctrine's
  movement (core-ward march, chase within 8, return),
  engagement (fire whenever in range), return-or-route
  (return proved, not just chase), and binding (Alpha6 key)
  are each decided and tested, proved against more than one
  kind — all three — with HoldGround and Advance unchanged
  for zones and saves that do not use it; the composed demo
  proves the kind and the doctrine together through a
  save/load round-trip, with a dead squad staying dead and
  kind and doctrine re-derived on load without a new save
  field; the B-key checklist gap from Slice 10 is closed by
  Step 1's gate-doc touch (item 19, alongside item 20); and
  this review records honestly that Slice 11 is still not
  playtested, balanced, or Unity-verified. The Slice 1
  director playtest gate is still pending throughout and is
  not closed by Slice 11.
- Three staging facts this step pins, so they are not
  over-claimed later. First, the dummy-rally coincidence
  above: the Heavy's path to the core happens to cross its
  dummy rally cell, because the rally (20,5,20) lies on the
  diagonal from the spawn (16,5,16) to the core (28,5,28).
  The demo does not avoid that cell and does not claim the
  guard never visits it — it claims, and the tick-8 snapshot
  proves, that the guard does not hold there: it reads
  Marching at (20,5,20) and Guarding only at (28,5,28).
  Second, the core attacker is staged only after the guard
  is at its post, so the chase, kill, and return are not
  confounded by a march still in progress — and the harder
  radius question, an attacker walking away from the core
  until it leaves the 8-radius and the guard stops chasing,
  is proved in Step 2's tests (the 8 / 9 boundary), not
  re-proved by this demo, which must not be quoted as
  proving it. Third, the doomed squad is a staged death,
  not a balance claim: weakening a squad to 10 HP by a
  direct damage call is a demo and test device for proving
  removal and non-resurrection, and no review sentence here
  claims a 10-HP squad is a fair fight or a likely death in
  play. The demo stages its enemies directly with
  `Enemies.Add`, exactly as the Slice 4 and Slice 7 demos
  stage theirs, rather than spawning them by wave — wave
  spawning was proved in Slice 1, and walking a wave into
  these exact cells would prove walking, not squads.
- One claim split stated plainly, so it is not over-claimed
  later: the demo proves the core rules (march, hold, guard,
  chase, fire, kill, return, removal, save/load kind and
  doctrine) and the snapshot words, but it does not prove
  that the Alpha5 or Alpha6 key feels right, that a Heavy
  reads right in the scene, or that a Defend Core squad is
  distinguishable from a Vanguard in the scene — the three
  kinds still share one marker, by Step 1's stated decision.
  Those are adapter claims with Windows checklist items (20
  for the Heavy key, 21 for the Defend Core key, 11 and 12
  in orbital-command), and this review adds no new checklist
  item for the demo itself, because the demo exercises no
  adapter binding the checklist does not already cover.
- No Slice 11 step duplicates logic in a Unity adapter.
  Core owns the kind shapes, the doctrine movement and
  engagement rules, the chase radius, the snapshot words,
  and the save ints; the adapter only feeds Alpha5 / Alpha6
  key presses into the existing picker and renders the
  existing snapshot lines. The kind and doctrine rules cannot
  be compiled headlessly as adapter code — they are core
  code, tested, and the adapter bindings are written and
  checklist-listed, not test-count claims.
- No Slice 11 step changed a saved value's meaning: the save
  format gains no field, and every earlier demo save length
  above is unchanged. The new scenario's save length
  (44,038 chars) is a composition fingerprint for this
  scenario only, not a balance number.
- Hard limits held: 3D real-time only, no turn-based
  mechanics (march, chase, fire, and removal are per-tick
  rules, with no planning pauses), no twin-stick Asteroids
  loop, no copied assets, lore, code, or trademarks. Heavy
  and Defend Core are original core rules, not copied
  mechanics; the blueprint's squad vocabulary was taken as
  names only and given tested meanings the blueprint never
  stated.

## Deliberately NOT in Slice 11 (still open, not forgotten)

- A fourth squad type, or a second kind added alongside
  the third as a bonus. One new kind is Slice 11; the next
  kind, if any, is a Slice 12-or-later decision with this
  slice's evidence in hand.
- A second doctrine, or a doctrine added as a kind's special
  behaviour. Patrol remains the deferred doctrine, not a
  rejected one: it still needs a route definition — a loop,
  a back-and-forth, waypoints — that the core does not yet
  have, and inventing that route inside this review would
  make it an untested extra, not a decision.
- Further electrical widening beyond two consumers and two
  sources (a third consumer, a third source, or a display
  for live networks feeding no consumer). That is the ranked
  Slice 12 candidate below, not Slice 11 work: a squad slice
  that also rewires power would make the squad untestable
  in isolation.
- A balance pass. The Heavy's numbers, Defend Core's 8-cell
  chase radius, the unchanged price, and the unchanged cap
  are first-pass values chosen for determinism and
  testability, exactly as Slice 3's prices and Slice 7's
  Recon numbers were, and must be labelled as such, not
  quoted as final feel.
- A pre-destruction or pre-disablement warning, a mesh tint
  for any new block or marker, or a mesh refresh on any key
  build. All three remain named gaps from 0.30.0 / 0.42.0,
  not fixed by a squad step — a marker distinction for the
  third kind, decided against for now in Step 1, is a display
  decision with its own Slice 12 question, not a tint or
  refresh system smuggled in.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 11 markers remain scene markers in the
  FPS view, not a second camera.
- A new movement mode, a weapon rework, or a free-deploy
  enforcement change. The free `DeploySquad` primitive
  remains a headless-only rule, re-named in the Slice 3
  review's gaps, not quietly fixed here.
- Windows Unity verification itself. It remains open for
  Slices 1–11 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly.

## Gaps this review names, in priority order

1. **Windows Unity verification, Slices 1–11.** The
   checklists in `docs/slice-01-playtest-gate.md`,
   `docs/orbital-view.md`, `docs/orbital-command.md`, and
   `docs/powered-presence.md` remain the verification plan.
   Slice 11 adds items 20 (Heavy key) and 21 (Defend Core
   key) to that plan, and items 11 and 12 to orbital-command.
   None of these items is claimed verified by any test count
   in this review.
2. **The Slice 1 director playtest gate is still pending.**
   Rob has not yet played Slice 1 in Unity on Windows or
   answered the gate question (fun without extra systems?).
   Slice 11 does not close that gate, and its demo numbers
   must not be quoted as a playtest result.
3. **Squad balance is unproven, not just unverified.**
   Heavy's shape (2 x 50 HP, 8 damage, range 2), Defend
   Core's chase radius (8), the unchanged price (10
   EmberOre), and the unchanged cap (3 live squads) are
   first-pass values for determinism and testability. A
   Windows playtest may find a Heavy too durable beside a
   Vanguard, too silent at range 3 to matter, the 8-cell
   perimeter too wide or too narrow for the core it guards,
   or the shared price wrong for a 100-HP anchor — and that
   observation, not this review, justifies retuning, with
   its own changelog entry.
4. **Destruction and disablement have no warning, by decision,
   not by oversight.** Carried from Slices 9 and 10: a
   player's first HV mill simply disappears on its delivery
   tick, and a player's first HV workstation simply stops
   regenerating that tick, and the only headless traces are
   the HUD totals and the `DestroyedMills` session counter.
5. **Mesh tint and mesh refresh remain named gaps.** A
   Dynamo, Alternator, transformer, cable, or powered mill
   is still not rendered distinctly, and no adapter
   refreshes the voxel mesh on any key build, so a cable
   laid by aim, an Alternator placed by aim, or a mill
   destroyed by voltage may be counted before it is
   visible in the scene. Both were named in 0.30.0, and
   neither is closed by a review step.
6. **A live network feeding no consumer shows nothing.**
   Carried from Slice 9 Step 2 and Slice 10 Step 2: the HUD
   has no cable, energised-network, or source count, so a
   live cable network with no consumer on it, and a lone
   Alternator (like a lone Dynamo before it), are
   invisible in the HUD as well as in the scene. This stays
   a ranked Slice 12 candidate, not a Slice 11 omission to
   be quietly patched.
7. **Three kinds now share one scene marker.** Decided in
   Step 1, not forgotten, and grown by this slice: if a
   Windows playtest shows players mistaking a Heavy for a
   Vanguard or a Recon in the scene, that observation, not
   this review, justifies a distinction, with its own
   checklist item — decided at Slice 12 scoping as one
   visibility question together with gaps 4 and 5, not
   promised here.
8. **Patrol is deferred, not designed.** The route question
   it needs — loop, back-and-forth, or waypoints, and where
   the route is stored — is still unanswered, and a later
   doctrine step must answer it with its own tests before
   Patrol can be claimed, not inherit Defend Core's chase
   rule by analogy.
9. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

**Closed by this slice, not carried:** the workstation key
(B) checklist gap from Slice 10. Step 1 added gate-doc item
19 for the B key alongside its own item 20, as the 0.48.0
scope assigned, so the gate doc is no longer incomplete on
that key. It is recorded here as closed, not left in the
numbered gaps above, so a later review does not carry it
again by accident.

## Slice 11 verdict (headless)

Slice 11 is **reviewed and closed as tested squad widening
(one third squad kind — Heavy, a close-range anchor at
2 x 50 HP, 8 damage, range 2, proved as a trade against
Vanguard and Recon, not a replacement — and one deferred
doctrine — Defend Core, a core guard with an 8-cell chase
perimeter, fire-whenever-in-range, and a proved return,
proved against all three kinds — proved together in one
composed demo through a save/load round-trip, with a dead
squad staying dead — plus written adapters (Alpha5 / Alpha6
keys, shared-marker decision stated, gate items 19, 20, 21
and orbital-command items 11, 12))**. It is not playtested,
not balanced, and not Unity-verified, and its adapter claims
must not be quoted as if the test count covered them.
**Slice 12 is not scoped by this review** — its scope is the
decision for the next run, chosen from the ranked candidates
below, so it is chosen with Slice 11's composition evidence in
hand rather than guessed now.

## Ranked candidates for Slice 12 scoping (starting point, not promised)

Carried from the 0.48.0 scope, in the same order, because
the Step 3 evidence gives no reason to reorder them:

1. Further electrical widening beyond two consumers and
   two sources (a third consumer, a third source, or a
   display for live networks feeding no consumer) —
   deferred, not rejected, and only after Slice 11 proves
   the squad widening.
2. Adapter visibility and fairness as one candidate:
   a pre-destruction / pre-disablement warning, a mesh tint
   and mesh refresh, and a scene-marker distinction for the
   squad kinds — gaps named in the Slice 9, Slice 10, and
   Slice 11 reviews, to be decided at Slice 12 scoping as one
   visibility question or split apart there, not promised here.
