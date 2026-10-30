# Forgeborne — Slice 7 Review

**Status: REVIEWED HEADLESSLY 2026-10-30, tests 862/862.
Director playtest and Windows Unity verification still pending,
as with Slices 1, 2, 3, 4, 5, and 6. This review closes Slice 7 as a reviewed
whole; it does not claim Slice 7 is playtested, balanced, or
finished in every direction it could grow.**

## Review question

Does Slice 7 close the combat-shape gap Slice 6 left open — a
game in which every squad was a Vanguard, with no second unit
to choose and no price to weigh against it — with exactly one
second squad type, priced under the Slice 3 economy, deployed
through the existing paid picker, proved together with
Vanguard, and closed by a composed demo, without inventing a
third kind, a new doctrine, a voltage tier, a balance pass, or
a Vanguard rebalance, and without breaking the gather ->
build -> survive loop, squad price and cap, Dynamo cost and
cadence, loot, skills, core health, or old saves? The answer
headlessly is yes for the systems below, with the gaps and
next candidates listed honestly at the end — and with one
claim split the review must keep split: the core kind rules
are tested, the adapter kind binding is written and
checklist-listed, and neither claim borrows the other's
evidence.

## What Slice 7 actually contains, step by step

| Step | What was built | Integration point | Headless proof |
|---|---|---|---|
| 0.32.0 scope | `docs/slice-07-scope.md`: Slice 7 scoped as exactly one second squad type, in order — kind decided in Step 1, then demo + review. Ranked Slice 8 candidates recorded, not promised | Decision document only, no system built by it | Tests 746/746 unchanged |
| 0.33.0 exactly one second squad type, Recon | `SquadShape` / `SquadShapes.For` in `src/Forgeborne.Core/Orbital.cs`: Vanguard unchanged (3 members, 30 HP each, 10 damage each, range 3) and Recon (2 members, 20 HP each, 6 damage each, range 5), read from the squad's kind by movement, fire, snapshot, and load alike; old Vanguard constants removed, every rule that read them now reads the shape. Price 10 EmberOre for both kinds (`SquadEconomy.CostForKind`), cap 3 live squads in total, not per kind. Picker doctrine and kind made independent choices: adapter keys 1 / 2 select doctrine only, keys 3 / 4 select kind only (before this step 1 and 2 also forced Vanguard). Both kinds work under both doctrines, with no restriction and no refusal rule invented. Save: no new field (kind was already index 8 of the saved squad array), load clamp now per-kind (Vanguard 0..90, Recon 0..40). Targeting stays kind-blind. Markers unchanged: same cyan squad cube for both kinds, kind readable from the text line | The shape table is the single source for every rule, so movement, fire, snapshot, and load cannot disagree about what a kind is; the paid path, the picker, and the prompt all stay true at one price and one total cap | Tests 810/810; Recon fires at range 5 and is silent at range 6, Vanguard silent at range 4 in the same geometry; Recon marches one voxel per tick like Vanguard; Advance Recon hunts and fires; mixed kinds count toward one total cap, a 4th deploy is refused and charges nothing; a corrupt 999-HP Recon loads at 40 and a Vanguard at 90; a mixed save round-trips both kinds with their own health; an enemy pursues a nearer Recon squad with no kind priority |
| 0.34.0 demo scenario + review | This review, plus a composed Slice 7 demo scenario: one Simulation in which Vanguard and Recon deploy through the paid picker, fight in parallel lanes, and survive a save/load round-trip together. No new core system rides along with this step | Demo and composed-integration tests mirror each other; adapter rendering is explicitly out of headless reach and stays in its checklist | Tests 862/862; demo Step 2 scenario below |

## What the review verified this run

- Full test suite re-run before writing: **862 passed, 0 failed**
  (was 810; +52 composed-integration tests for this step,
  with no test failures during the run — one test-authoring
  error was caught on re-read before the first run, not by
  a failure: a lane assertion first read the Recon's X
  where its Z was meant, and was corrected to Z = 20
  before running).
- Headless demo re-run: Slice 1 unchanged (gathered 1, built 1,
  killed 2, save 43,881 chars); Slice 2 unchanged (built 1,
  killed 2, kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars); Step 1 scenario
  unchanged (core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars); Step 3 scenario unchanged (core 42 HP, 3 paid
  squads 90 HP, 10 ore left, 4th picker deploy REFUSED at cap,
  save 44,103 chars); Slice 4 scenario unchanged (HoldGround
  at rally, Advance hunting, save 44,026 chars); Slice 5 Step 2
  scenario unchanged (powered 5 vs kinetic 1 in 5 ticks, stone
  0 -> 6 then 12, save 43,974 chars); Slice 6 Step 4 scenario
  unchanged (pitch clamp at 89, HoldGround Holding, Advance
  Idle after its kill, power 1/1, loaded powered gathered 5,
  save 44,024 chars). New Step 2 scenario, in full: one
  Simulation, 20 EmberOre funded, a Vanguard HoldGround squad
  deployed through the paid picker at rally (20,5,16) reading
  3 members, 90 HP, 30 damage, range 3, and — after the player
  walks to the second lane, because a squad spawns at the
  player's feet — a Recon Advance squad deployed through the
  paid picker at rally (24,5,20) reading 2 members, 40 HP,
  12 damage, range 5, price 10 / 10 EmberOre each, ore left 0.
  A third picker deploy is REFUSED unaffordable, the picker
  stays open, and nothing is charged. The player is then
  parked far away at (28,5,4), and one enemy per lane is placed
  at (27,5,16) and (27,5,20), so every enemy step pursues a
  squad. Tick 1: both squads step east (17,5,z) and both
  enemies step west (26,5,z). Tick 2: the same, to 18 and 25,
  with the Recon still out of range. Tick 3: the Vanguard is
  still marching at (19,5,16) and does not fire, while the
  Recon, now at (19,5,20), fires its lane enemy at exactly
  range 5 for 12 damage (30 -> 18) — a distance Vanguard's
  range 3 cannot reach, in the same run as the Vanguard's
  silence. Tick 4: the Vanguard reaches its rally (20,5,16)
  and kills its lane enemy at exactly range 3 for 30 damage,
  while the Recon fires its lane enemy down to 6 HP.
  Tick 5: the Vanguard holds, and the Recon closes to
  (21,5,20) and kills its lane enemy, so both squads finish
  intact at full health (90 and 40) with no enemies left.
  Save / load: both squads survive with kind, doctrine,
  position, and health intact (Vanguard HoldGround at
  (20,5,16), 90 HP; Recon Advance at (21,5,20), 40 HP),
  ore still 0, and a further unfunded deploy still refused.
  One loaded tick with no enemies leaves the Vanguard
  Holding and the Recon Idle, by derivation. An old save
  with its Squads field stripped loads with 0 squads and no
  crash, by the same stripping the tests use. Save JSON
  44,013 chars.
- Slice 7 gate, checked against `docs/slice-07-scope.md`:
  every step pushed with passing tests; exactly one second
  squad type exists, with its shape (2 x 20 HP, 6 damage,
  range 5, shared one-voxel movement), price (10 EmberOre,
  same as Vanguard), cap (3 live squads in total, not per
  kind), picker binding (kind keys 3 / 4, independent of
  doctrine keys 1 / 2), doctrine interaction (both kinds
  under both doctrines, no restriction), save (no new field,
  per-kind load clamp), and targeting (kind-blind) each
  decided in Step 1 and recorded in DECISIONS.md; Vanguard's
  stats, behaviour, and demo numbers are unchanged, and
  every pre-existing Vanguard test passes unchanged; both
  kinds deploy through the paid picker (the free primitive
  remaining headless-only by rule, not by enforcement);
  old Vanguard-only saves still load, and a mixed-kind save
  round-trips both kinds with their own health intact; the
  composed demo proves both kinds together through a
  save/load round-trip; and this review records honestly
  that Slice 7 is still not playtested, balanced, or
  Unity-verified.
- One scope reading stated plainly, so it is not
  over-claimed later: the demo exercises both doctrines
  across the two kinds (Vanguard HoldGround, Recon Advance),
  not all four doctrine-kind combinations in one run.
  The other two combinations (Vanguard Advance, Recon
  HoldGround) are proved in the Step 1 tests, not in the
  demo, and the review does not pretend otherwise. The
  scope's wording — "both doctrines exercised across the
  two kinds or the restriction proved" — is satisfied by
  the cross coverage plus the Step 1 combination tests,
  and a future demo that wants all four combinations in
  one run would need a third squad slot it does not have
  at cap 3, or a second run.
- A demo-design fact this step proved the hard way, and
  the tests now pin: a squad spawns at the player's feet,
  so two lanes need a player move between deploys. The
  first draft of this scenario deployed both squads from
  one spot, stacking them on z=16, and the Recon then
  hunted the Vanguard-lane enemy instead of its own — the
  output showed it, the scenario was fixed by walking the
  player to z=20 between deploys, and the test asserts
  the Recon's spawn lane (Z = 20) so the mistake cannot
  silently return. This is a scenario-scripting fact, not
  a core rule change: deployment itself is unchanged.
- No Slice 7 step duplicates logic in a Unity adapter. Core
  owns the kind shape, the price, the cap, the picker state
  machine, and the snapshot; the adapter only binds keys
  3 / 4 to the existing `SelectKind` and renders the kind
  word the snapshot already carries. The kind keys
  themselves cannot be compiled headlessly — they are
  written, checklist item 10 in `docs/orbital-command.md`,
  not test-count claims.
- No Slice 7 step changed a saved value's meaning: the save
  format gains no field, and the only load change is the
  clamp becoming per-kind, which corrects a corrupt-save
  case (a Recon claiming Vanguard health) rather than
  redefining any legitimate save. Every earlier demo save
  length above is unchanged, and the new scenario's save
  length (44,013 chars) is a composition fingerprint, not
  a balance number.
- Hard limits held: 3D real-time only, no turn-based mechanics
  (squads act once per world tick, with no planning
  pauses), no twin-stick Asteroids loop, no copied assets,
  lore, code, or trademarks. Recon is an original role — a
  fragile long-range screen — not a renamed unit from
  another game; the free deploy primitive is headless-only
  by rule, not by enforcement, and is untouched by this
  slice.

## Deliberately NOT in Slice 7 (still open, not forgotten)

- A third squad type, or a second kind added alongside
  Recon as a bonus. One new kind was the slice; the next
  kind, if any, is a Slice 8-or-later decision with this
  slice's evidence in hand.
- A new doctrine (Defend Core, Patrol, or any other).
  Doctrines are a separate widening with their own
  targeting and movement questions, decided in Slice 4
  style, not smuggled in as a kind's special behaviour.
- Aim-based gather / build / fire, and yaw-rotated
  movement. Both are the first ranked Slice 8 candidate
  below, not Slice 7 work: switching the proven Slice 1
  interaction cells or the movement axes in the same
  step as a new kind would have made the kind untestable
  in isolation, exactly as pitch discipline required in
  Slice 6 step 1.
- The rest of automation tier 2: a voltage ladder (LV to
  EV), transformers, mismatched-voltage destruction, and
  additional powered consumers or power sources. Those
  are deferred, not rejected, and are ranked behind the
  aim candidate by the scope — the Slice 7 evidence gives
  no reason to reorder them first.
- An Orbital camera, or a mini-map beyond the Slice 1
  placeholder. Slice 7 markers remain scene markers in
  the FPS view, not a second camera.
- A balance pass. Recon's numbers, its shared price, and
  the unchanged cap are first-pass values chosen for
  determinism and testability, exactly as Slice 3's prices
  and Slice 4's combat numbers were, and must be
  labelled as such, not quoted as final feel. The demo's
  range-5-versus-range-3 contrast proves the shapes
  differ; it does not prove they feel right together.
- A distinct scene marker for Recon. The same cyan squad
  cube for both kinds, with kind readable from the text
  line, was decided in Step 1 on the Slice 6 discipline of
  one marker per squad and no extra layer, and is kept
  here: two kinds sharing a marker is a real readability
  question, but it is not yet a play-observed confusion,
  and this review adds no marker on a guess.
- Enforcement of the free-deploy rule beyond its existing
  documentation. The free `DeploySquad` primitive remains a
  headless-only rule, re-named in the Slice 3 review's gaps,
  not quietly fixed here.
- A mesh tint for Dynamo / powered mills, and a mesh
  refresh on any key build. Both remain named gaps from
  0.30.0, not fixed by a squad-type step.
- Windows Unity verification itself. It remains open for
  Slices 1–7 alike and is blocked on Unity 6000 on Windows,
  not on anything this slice can do headlessly. The
  checklists in `docs/slice-01-playtest-gate.md`,
  `docs/orbital-view.md`, `docs/orbital-command.md`, and
  `docs/powered-presence.md` remain the verification plan,
  and this review adds no new checklist item because its
  adapter claims are already covered by checklist item 10
  in `docs/orbital-command.md` (kind keys 3 / 4,
  doctrine-kind independence, prompt kind word, Recon text
  line, same cyan marker).

## Gaps this review names, in priority order

1. **No Unity verification at all yet** — for Slices 1–7.
   Every adapter is explicitly uncompiled headlessly and
   needs Unity 6000 on Windows. Slice 7's exposure is
   narrow but real: the kind keys 3 / 4, the prompt kind
   word, and the Recon text line are written, not run,
   and the shared cyan marker has never been seen with
   two kinds in a scene. Until checklist item 10 runs,
   Recon is a tested core kind with a written binding,
   not a seen unit.
2. **No director playtest yet.** The Slice 1 playtest gate
   (`docs/slice-01-playtest-gate.md`) is still pending
   throughout Slice 7 and is not closed by it: no one has
   played gather -> build -> survive in Unity and answered
   whether it is fun, and Slice 7 adds no playtest evidence
   of its own beyond headless composition — a range-5
   screen versus a range-3 anchor is a feel question, and
   feel is not proved by a damage number.
3. **No balance pass.** Recon's shape, its shared price,
   and the unchanged cap are first-pass. A fragile screen
   that fires earlier but dies faster may be the right
   trade, or may be too fragile to field, or too cheap at
   Vanguard's price — the demo proves the trade exists,
   not that it is tuned.
4. **Two kinds share one marker.** Decided, not forgotten:
   if a Windows playtest shows players mistaking a Recon
   for a Vanguard in the scene, that observation, not
   this review, justifies a distinction, with its own
   checklist item.
5. **Mesh tint and mesh refresh remain named gaps.**
   A Dynamo is still not rendered distinctly from stone or
   mills, and no adapter refreshes the voxel mesh on any
   key build, so a mill or Dynamo built by key may be
   counted by the HUD line before it is visible in the scene.
   Both were named in 0.30.0, and neither is closed by a
   review step.
6. **The free deploy primitive remains a documented gap.**
   It is headless-only by rule, not by enforcement: nothing
   in code stops a future play path from calling it. If a
   future step finds a play path reaching it, that is a bug
   in that path, per the 0.18.0 decision — but the review
   notes it again here so it is not forgotten as slices grow.

## Slice 7 verdict (headless)

Slice 7 is **reviewed and closed as tested second-kind core
plus written adapters — a Recon screen that fires at range 5
where Vanguard cannot, priced at Vanguard's 10 EmberOre
under a shared cap of 3, deployable in either doctrine
through an independent kind selection, saving and loading
with its own health intact, and composed with Vanguard in
one demo run through a save/load round-trip**. It is not
playtested, not balanced, and not Unity-verified, and its
adapter claims must not be quoted as if the test count
covered them. **Slice 8 is not scoped by this review** — its
scope is the decision for the next run, chosen from the
ranked candidates below, so it is chosen with Slice 7's
composition evidence in hand rather than guessed now.

## Ranked candidates for Slice 8 scoping (starting point, not promised)

Carried from the 0.32.0 scope, in the same order, because
the Step 2 evidence gives no reason to reorder them:

1. Aim-based gather / build / fire, and yaw-rotated
   movement — the interaction follow-ups Slice 6 step 1
   carries forward, as one candidate because they are one
   question (does the player's aim, not a fixed +Z cell,
   decide what the player touches?), not two unrelated
   systems.
2. The rest of automation tier 2 (voltage ladder,
   transformers, mismatch destruction, additional consumers
   and sources), deferred, not rejected.
3. A third squad type, and the deferred doctrines
   (Defend Core, Patrol) — widening candidates that
   Slice 7's second-kind evidence will inform, not
   promises made before that evidence exists.
