# Forgeborne — Changelog

## 0.23.0 — 2026-10-29 — Slice 4 Step 3: demo scenario + review

- Composed Slice 4 demo scenario, plus `docs/slice-04-review.md`
  (documentation only, no new core system, by design). One
  Simulation proves Step 1 and Step 2 together, not
  separately: a HoldGround squad and an Advance squad, both
  deployed through the paid picker (10 EmberOre each, 0 left),
  both weakened to 1 member so their 10-damage fire does not
  kill outright and hide the engagement, player moved far
  away, one enemy east and one west. Tick 1: both enemies
  pursue the squads, both squads march east, nobody hurt.
  Tick 2: Advance fires the east enemy 30 -> 20 while
  HoldGround, still marching, stays silent. Tick 3: the east
  enemy reaches adjacency and both squads take exactly 10
  each (single adjacent damage, not double) while Advance
  fires it 20 -> 10. Tick 4: HoldGround takes a second 10
  while Advance kills its attacker and takes nothing from
  it, and the west enemy has pursued to 14. Save/load after
  tick 4 keeps positions, health, doctrines, rallies, and
  spent ore, and the loaded game cannot deploy more without
  ore. Tick 5 (loaded): HoldGround marches on while Advance
  moves WEST toward the west enemy and fires it — hunting
  from the saved position, not marching to its stored rally.
  Tick 6 (loaded): HoldGround reaches its rally; the west
  enemy retargets to the nearer Advance squad, Advance fires
  it to 10 and takes exactly one adjacent 10, HoldGround at
  its rally unharmed. Save JSON 44,026 chars.
- Review verdict: Slice 4 is **reviewed and closed as tested,
  integrated combat answers** — not playtested, balanced, or
  Unity-verified. Slice 5 is NOT scoped by this review; its
  ranked candidates are carried from the 0.20.0 scope in the
  same order (automation tier 2, then FPS pitch + 3D markers
  including the Advance display gap, then a second squad
  type), with gaps named in priority order in the review.
- Tests: **579 passed, 0 failed** (was 528; +51: paid picker
  deploys for both doctrines, per-tick positions / health /
  damage in the composed run, save/load preservation of
  doctrines / rallies / health / spent ore, loaded-game
  hunting and retargeting). Demo unchanged except the new
  scenario: Slice 1 gathered 1, built 1, killed 2, save
  43,881 chars; Slice 2 save 43,908 chars; Step 1 scenario
  save 44,016 chars; Step 3 scenario save 44,103 chars.
  Next in order: scoping Slice 5 as its own decision step.

## 0.22.0 — 2026-10-29 — Slice 4 Step 2: second doctrine, Advance

- Exactly one second doctrine, chosen in this step from
  Advance / Defend Core / Patrol (rationale in DECISIONS.md):
  Advance. An Advance Vanguard squad hunts the nearest living
  enemy, one voxel per tick, re-derived every tick (Manhattan,
  first in list order on ties), and fires at the nearest enemy
  in range 3 whether or not it is at its rally. The rally the
  picker still requires to deploy is ignored after deployment
  for Advance: with no living enemy the squad stands still,
  and it stands still again once its prey dies — it never
  falls back to marching to its rally, which is what keeps
  Advance from collapsing into "HoldGround at a moving
  rally". Movement is not duplicated: Advance and HoldGround
  share one step rule in TacticalSquad (largest-gap axis,
  X/Y/Z tie-break, solid and enemy-occupied cells refused);
  only the target differs. Adjacent-enemy damage is unchanged
  (one path, squad phase, after squad fire), and HoldGround
  behaviour is unchanged.
- Picker and economy: Advance is selectable in core via
  OrbitalCommand.SelectDoctrine and in the Unity adapter via
  key 2 (1 stays HoldGround); a picker confirm deploys it
  through the Slice 3 paid path — 10 EmberOre, cap 3 live
  squads — with no new price invented. Save/load needs no
  new field: doctrine and rally were already persisted, and
  a loaded Advance squad keeps hunting from its saved
  position. The Unity adapter code is written, NOT compiled
  or run in Unity on Windows; see the new checklist item 8
  in docs/orbital-command.md.
- Also corrected a stale adapter comment that still claimed
  no deployment cost or squad cap existed (settled in 0.18.0).
- Tests: **528 passed, 0 failed** (was 495; +33: Advance
  deploy, no-enemy standstill, moves toward enemy not rally,
  fires while not at rally, move-then-fire entering range,
  nearest-enemy list-order tie, solid-block hold with fire,
  stops after prey dies with no rally fallback, save/load
  hunting, paid picker deploy at the settled price, snapshot
  doctrine). Demo unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 save 43,908 chars;
  Step 1 scenario save 44,016 chars; Step 3 scenario save
  44,103 chars. Next in order: Slice 4 Step 3, demo scenario
  + documentation-only review.
- Defend Core and Patrol are deferred, not rejected; the
  display's MOVING / HOLDING word remains rally-relative, so
  for Advance it is not a hunting state — recorded as a
  presentation gap in DECISIONS.md and docs/orbital-command.md.

## 0.21.0 — 2026-10-29 — Slice 4 Step 1: enemy AI targeting squads

- Enemies now pursue squads, not only the player. Targeting
  rule, decided in this step (rationale in DECISIONS.md):
  nearest live target by Manhattan distance, re-derived
  every tick — the player or any live squad, moving and
  holding squads alike. Ties go to the player, and ties
  between squads go to the first squad in deploy order.
  Dead squads are never targeted. The player remains a
  valid target, chosen whenever nearest or tied and
  re-chosen as soon as a nearer squad dies or moves away,
  so an enemy ignores the player only for the ticks a
  squad is strictly nearer — which is what lets a squad
  intercept or draw pressure off the player, not abandon
  the player wholesale.
- Movement is generalised, not duplicated: `Enemy.
  TickTowardPosition` now holds the single movement rule
  (one voxel per tick, largest-gap axis, ties X then Y
  then Z, no move when adjacent to the target, solid or
  occupied cells refused), and the player-only
  `TickToward` delegates to it. An enemy pursuing one
  target cannot walk through and overlap another target
  on its way there.
- Damage reconciliation, the scope's named risk, is by
  keeping exactly one squad-damage path: the squad phase's
  existing "10 per surviving adjacent enemy, after the
  squad fires". The enemy phase adds no squad damage, so
  a pursued squad is never counted twice for the same
  hit, and a squad that kills its attacker still takes
  nothing from it. Player damage stays proximity-based in
  the enemy phase, as before. Tick order (enemies, then
  squads) is unchanged. No new save field: targets are
  recomputed from saved positions and health, and a
  save/load round-trip still targets correctly.
- One pre-existing test changed for the intended behaviour
  change, not for a bug: the Slice 2 range test's enemy
  would now walk toward the nearer squad into range 3 and
  be killed, so its step is blocked by a solid block to
  keep isolating the range rule; pursuit-into-range is
  covered by the new tests instead.
- Tests: **495 passed, 0 failed** (was 466; +29: nearer-
  squad pursuit, player-nearer, player tie-break, dead
  squad never targeted, moving-squad pursuit over two
  ticks, exactly-once adjacent damage in both the
  surviving and the killing case, retargeting after a
  squad dies, direct movement-generalisation cases, and
  targeting derived after a save/load round-trip).
  Demo unchanged: Slice 1 gathered 1, built 1, killed 2,
  save 43,881 chars; Slice 2 save 43,908 chars; Step 1
  scenario save 44,016 chars; Step 3 scenario save
  44,103 chars. Next in order: Slice 4 Step 2, exactly
  one second doctrine, chosen in that step from Advance /
  Defend Core / Patrol.

## 0.20.0 — 2026-10-29 — Slice 4 scope (documentation only)

- Added `docs/slice-04-scope.md`: Slice 4 is scoped, as its
  own decision step, as **squads under fire** — the combat
  answers Slice 3 left open. In order: (1) enemy AI
  targeting squads, with the targeting rule, the movement
  generalisation, and the reconciliation with the existing
  squad-phase adjacent damage all decided in that step, not
  here; (2) exactly one second doctrine, chosen in that step
  from Advance / Defend Core / Patrol, with Advance as the
  starting hypothesis because it is the moving-into-danger
  doctrine the targeting step exists to answer, deployed
  through the Slice 3 paid path with no new price invented;
  (3) a Slice 4 demo scenario and a documentation-only
  Slice 4 review that closes the slice. Ranked Slice 5
  candidates are recorded for the review to start from, not
  promised.
- Deliberately NOT in Slice 4: automation tier 2, FPS pitch
  + 3D markers, a second squad type, the full 1,500-node
  graph, a balance pass, or any licence / monetisation /
  release change. Windows Unity verification remains open
  for Slices 1-4 alike.
- No code changed in this step, by design: a scope document
  must not smuggle in a system, and the targeting rule and
  the doctrine choice are explicitly left to their own steps
  to decide with evidence. Tests re-run unchanged: **466
  passed, 0 failed**. Demo unchanged: Slice 1 gathered 1,
  built 1, killed 2, save 43,881 chars; Slice 2 save 43,908
  chars; Step 1 scenario save 44,016 chars; Step 3 scenario
  save 44,103 chars. Next in order: Slice 4 Step 1, enemy
  AI targeting squads.

## 0.19.0 — 2026-10-29 — Slice 3 Step 3: demo scenario + review

- Closes Slice 3 with the composition proof its scope asked
  for, and with its own review. No new core system rides
  along with this step, by design: the demo scenario and the
  review prove Steps 1 and 2 together, they do not widen the
  game.
- Demo: a new Slice 3 Step 3 scenario after the Step 1
  scenario — one skilled, loot-equipped Simulation damages
  its core with two shots, deploys 3 squads through the paid
  path, is refused a 4th deploy through the picker at the
  cap (picker stays open, ore untouched), then saves and
  loads into a fresh Simulation. Evidence: core 42 HP,
  Titan Long Pulse Rifle 28/120, effective damage 24,
  shields 100/105, 3 paid squads at 90 HP, 10 ore left,
  before and after the load alike — and the loaded game
  still cannot deploy more (`CanDeployOrbitalSquad` false),
  because the cap survives the round-trip, not just the
  squads. Save JSON 44,103 chars.
- Review: new `docs/slice-03-review.md` checks the Slice 3
  gate against the scope (all steps pushed with passing
  tests, old saves load, core / build / weapon survive a
  round-trip in tests and demo, unaffordable and over-cap
  deploys refuse without charging or half-deploying) and
  records the verdict honestly: Slice 3 is reviewed and
  closed as tested, integrated persistence + economy — not
  playtested, not balanced, not Unity-verified. It carries
  the ranked Slice 4 candidates from the 0.16.0 scope in
  the same order, as the starting point for Slice 4 scoping,
  not as a promise. Slice 4 is NOT scoped by this review.
- Demo otherwise unchanged: Slice 1 gathered 1, built 1,
  killed 2, save 43,881 chars; Slice 2 built 1, killed 2,
  kinetic gathered 1, squad intact 3 members / 90 HP,
  squads saved+loaded 1, save 43,908 chars; Step 1 scenario
  core 42 HP, weapon 28/120, shields 100/105, save
  44,016 chars.
- Tests: **466 passed, 0 failed** (was 437; +29 composed-
  integration tests mirroring the new demo scenario, with
  no test failures during the run). Next in order: scoping
  Slice 4 as its own decision step.

## 0.18.0 — 2026-10-29 — Slice 3 Step 2: squad economy (cost and cap)

- Closes the second Slice 3 debt: Orbital deployment is no
  longer unlimited. One Vanguard squad under HoldGround now
  costs 10 EmberOre from the player inventory, and at most 3
  live squads may be fielded at once. Both are first-pass
  values for determinism and testability, not final balance,
  per the Slice 3 scope's no-balance-pass rule.
- How it is enforced, without a second copy of the rules:
  `SquadEconomy` in `Orbital.cs` holds the two constants
  (`MaxConcurrentSquads = 3`, `EmberOreCost = 10`) and the
  live-count / affordability queries; `WorldZone.
  TryDeployPaidSquad` is the single play deployment path and
  checks rally bounds, doctrine/kind validity, the cap, and
  affordability BEFORE charging, so a refused deploy charges
  nothing and adds no squad, and an unexpected failure after
  charging refunds in full. `OrbitalCommand.Confirm` now
  deploys through that paid path, so a picker confirm the
  player cannot afford, or that exceeds the cap, fails closed
  exactly like a failed Confirm in 0.15.0 — picker stays
  open, no squad, no charge. `Simulation` gains
  `CanDeployOrbitalSquad` and `DeployOrbitalSquadPaid` as the
  headless equivalents of a picker Confirm.
- What deliberately did NOT change: `WorldZone.DeploySquad`
  and `Simulation.DeployOrbitalSquad` remain the free spawn
  primitives for headless tests and the demo only — every
  pre-0.18.0 test that deploys squads still passes unchanged
  on that path, and the distinction is documented in code,
  in tests, and in `docs/orbital-command.md`, rather than
  hidden. Unity play goes through the picker, so play is
  priced; the demo's Slice 2 squad now deploys through the
  paid path (funded with 10 EmberOre first). The Unity
  display's picker prompt now shows the cost and cap; it
  enforces nothing, core does.
- Demo: Slice 1 unchanged (gathered 1, built 1, killed 2,
  save 43,881 chars); Slice 2 unchanged in its reported
  numbers (built 1, killed 2, kinetic gathered 1, squad
  intact 3 members / 90 HP, squads saved+loaded 1, save
  43,908 chars), with the squad now deployed through the
  paid path. Next in order: Slice 3 Step 3, demo scenario
  + review.
- Tests: **437 passed, 0 failed** (was 381; +56). Three
  pre-existing 0.15.0 picker tests were updated, not the
  code, for the intended behaviour change: their confirms
  are now funded with 10 EmberOre, because an unfunded
  picker confirm is exactly what this step refuses — the
  unfunded refusal itself is covered by the new tests.
  Every other pre-existing deploy test passes unchanged
  on the free headless path.

## 0.17.0 — 2026-10-29 — Slice 3 Step 1: save-format completion

- Closes the first Slice 3 debt: a load no longer silently
  resets what the game promises. The save format now carries,
  in addition to everything it carried before: the core
  objective's health, the applied skill build, and the
  equipped weapon — the weapon question the 0.16.0 scope left
  open is decided here, in DECISIONS.md, as YES, it joins the
  format. A damaged core stays damaged across a load (a
  destroyed core stays destroyed); a fresh Simulation loading
  a save restores the build instead of starting at base values
  until someone re-applies it; a loot weapon, including its
  in-progress mag, survives instead of reverting to the Pulse
  Rifle.
- How it is built, without breaking old saves: new fields are
  nullable and declared last in `SaveData` (`CoreHealth`,
  `SkillBuild` as the existing Base64 encoding, and five
  weapon fields — name, damage, mag capacity, current mag,
  reserve — all together or none), and null fields are omitted
  when writing, so a world-only save grows only by the
  CoreHealth field. `SaveSystem.LoadSave` returns the world
  plus the build and weapon, if any; `LoadZone` still returns
  just the world for existing callers. `Simulation.SaveToJson`
  is the full save (world + build + weapon) and
  `Simulation.LoadFromJson` restores all three, replacing the
  current build and weapon when the save carries them and
  keeping them when it does not (an old save). `WorldZone.
  RestoreCoreHealth` clamps to 0..100 and `Weapon.FromSaved`
  restores a live weapon with its current mag, under the
  constructor's clamps. The skill build is decoded before the
  player is restored, so the raised shield cap is in place
  before shields are restored — a skilled save with 100
  shields no longer clamps to the base 85 cap on load.
  Malformed skill strings and partial weapons are skipped,
  never fatal, matching the 0.10.0 squad rule; out-of-range
  core values clamp to 0..100. `GameManager.SaveToJson` now
  uses the full Simulation save instead of the world-only
  save, so Unity saves stop dropping the build and weapon too
  (Unity compile itself is still unverified headlessly).
- Demo: a Slice 3 Step 1 scenario after the Slice 2 scenario —
  a skilled, loot-equipped Simulation fires twice at its core
  from near it (FPS teleported there as the headless stand-in
  for walking), then saves and loads into a fresh Simulation.
  Evidence: core 42 HP, Titan Long Pulse Rifle 28/120,
  effective damage 24, shields 100/105, before and after the
  load alike; full save JSON 44,016 chars.
- Tests: **381 passed, 0 failed** (was 342; +39). One test
  failure during the run was a test-arithmetic error, not a
  code bug: with the weapon restore skipped, the fresh
  Simulation keeps the 10-damage Pulse Rifle, so the restored
  build gives 12, not the loot weapon's 24 — the expectation
  was corrected to 12. Demo otherwise unchanged: Slice 1
  gathered 1, built 1, killed 2, save 43,881 chars (was 43,864;
  the +17 is the CoreHealth field); Slice 2 save 43,908 chars
  (was 43,891; same +17). Next in order: Slice 3 Step 2,
  squad economy (cost and cap).

## 0.16.0 — 2026-10-29 — Slice 3 scope (documentation only)

- Added `docs/slice-03-scope.md`: Slice 3 is scoped, as its
  own decision step, as **economy and persistence** — the two
  debts Slice 2 left behind. In order: (1) save-format
  completion — core objective health and the applied skill
  build persisted in the existing format, old saves still
  loading, equipped weapon decided in that step, not here;
  (2) squad economy — deployment cost and cap decided in
  that step, with resource pricing as the starting hypothesis
  but not a decision, and fail-closed enforcement in the
  picker; (3) a Slice 3 demo scenario and a documentation-only
  Slice 3 review that closes the slice. Ranked Slice 4
  candidates are recorded for the review to start from, not
  promised.
- Deliberately NOT in Slice 3: the full 1,500-node graph,
  automation tiers 2-4, crafting, fuel/power, extra doctrines
  or squad types, enemy AI targeting squads, 3D markers or an
  Orbital camera, a balance pass, FPS pitch, or any licence /
  monetisation / release change. Windows Unity verification
  remains open for Slices 1-3 alike.
- No code changed in this step, by design: a scope document
  must not smuggle in a system, and the economy's pricing
  basis is explicitly left to its own step to decide with
  evidence. Tests re-run unchanged: **342 passed, 0 failed**.
  Demo unchanged: Slice 1 gathered 1, built 1, killed 2, save
  43,864 chars; Slice 2 save 43,891 chars. Next in order:
  Slice 3 Step 1, save-format completion.

## 0.15.0 — 2026-10-29 — Doctrine menu / rally picker replacing provisional O

- Closes Slice 2 review follow-up (3): the provisional O key
  from 0.11.0, which deployed a squad instantly at the player's
  feet, is gone from the Unity adapter. Core now holds the menu
  as `OrbitalCommand` in `src/Forgeborne.Core/Orbital.cs`:
  selected doctrine/kind (HoldGround / Vanguard, the only pair
  so far), `BeginPick` (opens the picker, clearing any stale
  pending rally), `SetRally` (bounds-checked, picking-only),
  `Confirm` (deploys one squad at the pending rally, then
  closes and clears; no pick or no rally deploys nothing and
  a failed deploy keeps the picker open for a re-pick),
  `CancelPick` (no squad, rally dropped), and
  `SelectDoctrine` / `SelectKind` (unknown values rejected,
  selection unchanged). `Simulation` exposes the same flow as
  `BeginOrbitalPick`, `SetOrbitalRally`, `CancelOrbitalPick`,
  and `ConfirmOrbitalDeploy`; the direct `DeployOrbitalSquad`
  path remains for headless use and the demo only.
- Unity adapters stay thin: `FpsInputAdapter` binds O (open /
  cancel picker), crosshair raycast while picking (forward,
  30 m, voxel cell hit), Enter (deploy), Esc (cancel), and 1
  (re-select HoldGround / Vanguard), and suppresses weapon fire
  while picking so a click cannot shoot during rally aiming.
  `OrbitalSquadDisplay` shows a picker prompt line first while
  picking, then the squad list unchanged; with no squads it now
  reads `ORBITAL: NO SQUADS (press O to pick a rally)`.
  `VoxelMeshRenderer` gains a `MeshCollider` sharing the built
  mesh, so the picker's raycast actually hits the chunk.
  All three adapters are still NOT compiled headlessly — see
  `docs/orbital-command.md` for the Windows verification
  checklist, including the honest limit that the camera only
  yaws in Slice 1 (no pitch yet), so the ray runs horizontally
  from eye height and picks a rally by aiming at a face, not by
  looking down at flat ground.
- Deployment cost / cap decided, deferred: none yet, because
  inventory holds blocks, not a currency, so a price invented
  here would be a balance guess with no economy to price
  against. Cost and cap belong to a Slice 3 economy step, not
  to the picker. Repeated picker use still deploys repeatedly
  — a known, documented gap, not a balance decision.
- Updated `docs/orbital-view.md` (provisional-O section now says
  replaced, wiring and checklist steps use the picker) and added
  `docs/orbital-command.md` (menu flow, bindings, checklist).
- Tests: **342 passed, 0 failed** (was 297; +45). Demo unchanged:
  Slice 1 gathered 1, built 1, killed 2, save 43,864 chars;
  Slice 2 save 43,891 chars. With this step, all three Slice 2
  review follow-ups are complete (demo scenario, in-place
  restore, picker) — but Slice 3 is still NOT scoped by this
  step; scoping it is the next decision, not smuggled in here.

## 0.14.0 — 2026-10-29 — GameManager.LoadFromJson in-place restore

- Closed the Slice 1 limitation named in 0.3.0 and repeated in
  the 0.12.0 review: loading a save in Unity previously only
  logged it, because `Simulation.World` was get-only and the FPS
  position was private-set, so no adapter could restore in place.
  Core now supports it, and the Unity adapter is still thin:
  `FpsController.Teleport(x, y, z)` (clears vertical velocity,
  resets grounded), `Simulation.RestoreWorld(loaded)` (swaps the
  world inside the same Simulation object — the Fps object is
  never replaced, so GameManager and MeshRenderer need no
  re-wiring — teleports Fps to the saved player position, resets
  session counters, keeps the equipped weapon since the save
  format carries none, and re-applies an applied skill build to
  the new world so caps, speed, and regen do not silently revert
  to base values), and `Simulation.LoadFromJson(json)` (fail-closed:
  null, empty, or malformed JSON returns false and leaves the
  current world untouched, because the new world is fully parsed
  before any swap). `GameManager.LoadFromJson` now calls it,
  refreshes the mesh on success, and logs a warning on failure.
- Inherited save-format limit, documented not hidden: the core
  objective's health is not in the save format, so a loaded world
  starts with a fresh 100-HP core. Fixing that is a save-format
  step, not smuggled into this restore step.
- Updated `docs/orbital-view.md` step 6: the Unity save/load
  checklist step no longer carries the "only logs a load"
  caveat; the restore itself is now headless-tested, and only
  the Windows Unity verification of it remains open.
- Tests: **297 passed, 0 failed** (was 263; +34). Demo unchanged:
  Slice 1 gathered 1, built 1, killed 2, save 43,864 chars;
  Slice 2 save 43,891 chars. Next in order: (3) a doctrine menu /
  rally picker replacing provisional O. Slice 3 is still NOT scoped.

## 0.13.0 — 2026-10-29 — Slice 2 demo scenario (composition evidence)

- Extended `demo/Forgeborne.Demo` with a Slice 2 scenario after
  the unchanged Slice 1 run: one Simulation composed with every
  Slice 2 system at once — seed-0 loot equipped (Titan Long
  Pulse Rifle, 20 damage, 30 mag), the full Forge Survivor
  cluster applied, a Kinetic Mill built, a Vanguard HoldGround
  squad deployed at the player position, 20 ticks of combined
  play, then a save/load round-trip. This closes the
  composition gap named first by the 0.12.0 review: Slice 2
  systems are now shown working together, not only in isolation.
- Demo evidence this run: effective damage 24, core damage 29,
  shields cap 105, speed 6.3 m/s; mill built and 1 block
  excavated by tick 4; squad intact at 3 members / 90 HP through
  tick 19; 2 enemies killed, player alive; squad survives the
  save/load round-trip (1 squad loaded); Slice 2 save JSON
  43,891 chars. Slice 1 demo unchanged: gathered 1, built 1,
  killed 2, save JSON 43,864 chars.
- Added 9 composed-integration tests: loot name, skilled damage
  20 -> 24 and core damage 20 -> 29, shields cap 85 -> 105, mill
  build, squad deploy, snapshot count, and save/load keeping the
  squad — all through one Simulation, mirroring the demo.
- Tests: **263 passed, 0 failed** (was 254; +9). Next in order:
  (2) the `GameManager.LoadFromJson` in-place restore, then (3)
  a doctrine menu / rally picker replacing provisional O. Slice 3
  is still NOT scoped.

## 0.12.0 — 2026-10-29 — Slice 2 review as a whole (documentation only)

- Added `docs/slice-02-review.md`: a headless review of all
  eight Slice 2 steps (0.4.0 loot generation, 0.5.0 skill
  cluster, 0.6.0 weapon integration, 0.7.0 skill application,
  0.8.0 Kinetic automation, 0.9.0 Orbital prototype, 0.10.0
  squad persistence, 0.11.0 Orbital view), with the integration
  point and test evidence for each, the deliberately-not-done
  list, and gaps in priority order. Verdict: Slice 2 is reviewed
  and closed as tested, integrated core systems — not
  playtested, not balanced, not Unity-verified.
- Review findings, not new systems: the headless demo still
  does not exercise any Slice 2 system (it is unchanged since
  Slice 1, which proves no regression but not composition);
  `GameManager.LoadFromJson` still only logs a load instead
  of restoring `Simulation.World` in place; no Unity
  verification exists yet for Slice 1 or Slice 2; Slice 2
  numbers are first-pass, untuned values. Next in order: (1)
  a Slice 2 demo scenario, (2) the LoadFromJson in-place
  restore, (3) a doctrine menu / rally picker replacing
  provisional O. Slice 3 is NOT scoped by this review.
- Corrected doc debt found by the review: `docs/
  slice-01-playtest-gate.md` still said loot was "NOT
  integrated into Simulation" — true at 0.4.1, false since
  0.6.0. That bullet now says the integration happened in
  0.6.0, so the gate doc no longer contradicts this changelog.
- No code changed in this step, by design: a review must not
  smuggle in a system. Tests re-run unchanged: **254 passed,
  0 failed**. Demo unchanged: gathered 1, built 1, killed 2,
  save JSON 43,864 chars.

## 0.11.0 — 2026-10-29 — Slice 2 Step 8: Orbital view snapshot + Unity adapter

- Added `OrbitalView.Snapshot(world)` in `src/Forgeborne.Core/
  Orbital.cs`: a copied, read-only snapshot of every live squad
  (position, rally, health, living members, damage per tick,
  at-rally / moving state, doctrine, kind), in deploy order.
  It is a copy, not live references, so rendering cannot mutate
  core state and a pre-tick snapshot keeps its pre-tick position.
  Dead squads never appear (Advance already removes them).
- Added Unity adapter `ForgeborneUnity/Assets/Scripts/
  OrbitalSquadDisplay.cs` (NOT compiled headlessly, to be opened
  in Unity 6000 on Windows): renders the snapshot as one text
  line per squad, or `ORBITAL: NO SQUADS (press O to deploy)`
  when empty. No squad logic lives in the adapter.
- Provisional Unity command in `FpsInputAdapter`: **O** deploys
  a Vanguard squad under HoldGround at the player's current
  position, so it holds and fires there. This binding exists so
  the view can actually be verified in Unity; it is reversible
  and will be replaced by a doctrine menu / rally picker later.
- Added `docs/orbital-view.md`: wiring steps, what the snapshot
  carries, the provisional O key, and a Windows Unity verification
  checklist (including the honest note that `GameManager.
  LoadFromJson` still only logs a load and does not yet replace
  `Simulation.World` in place, so the save-then-view step may
  surface that as the next follow-up).
- Still deliberately NOT in this step: 3D squad markers or an
  Orbital camera, a doctrine menu or rally picker, deployment
  cost or a squad cap, and enemy AI targeting squads. With the
  view in place, Slice 2 can now be reviewed as a whole before
  Slice 3 is scoped.
- Tests: **254 passed, 0 failed** (was 235; +19). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,864 chars.

## 0.10.0 — 2026-10-29 — Slice 2 Step 7: Orbital squad save / load persistence

- Squads are no longer session-only: `SaveData` gains a `Squads`
  list, one entry per squad — position, rally, health, doctrine,
  kind — in the same int-array style as the existing enemy entries,
  and `SaveSystem.LoadZone` restores them. A wounded squad keeps
  its member count (30 HP = 1 member still firing) and keeps
  moving toward its rally on the first tick after load.
- `TacticalSquad` gains a load constructor with restored health,
  clamped to 0..90, so a corrupt save cannot create a super-squad.
  The loader skips malformed entries instead of failing the whole
  load: wrong length, out-of-bounds position or rally, unknown
  doctrine or kind, and dead (0 HP) squads, which are not
  resurrected. Old saves without the `Squads` field still load,
  with no squads, as before.
- Still deliberately NOT in this step: deployment cost or squad
  cap, additional doctrines or squad types, enemy AI targeting
  squads, and an Orbital camera / Unity adapter. The next
  candidate is the Orbital view / adapter note.
- Tests: **235 passed, 0 failed** (was 224; +11). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,864 chars
  (was 43,852; the +12 is the empty `Squads` array in the save).

## 0.9.0 — 2026-10-29 — Slice 2 Step 6: Orbital Tactical doctrine prototype (one doctrine, one squad type)

- Added `src/Forgeborne.Core/Orbital.cs`: `TacticalDoctrine.
  HoldGround` (only doctrine in this step) and `SquadKind.
  Vanguard` (only squad type in this step), as `TacticalSquad`:
  3 members, 30 HP each (90 total), 10 damage per living member
  per engagement tick, engagement range 3 (Manhattan).
- HoldGround behaviour, real-time, one world tick at a time, no
  individual micromanagement and no turn-like pauses: the squad
  moves one voxel per tick toward its rally point (largest-gap
  axis first, ties X then Y then Z, as with Enemy movement),
  refusing steps out of bounds, into solid blocks, or into an
  enemy-occupied cell; it does NOT fire while moving. Once
  holding the rally it fires once per tick at the nearest enemy
  in range, then takes 10 damage per enemy still adjacent to it
  — so a squad that kills its adjacent attacker that tick is not
  hit by it, and firepower steps down 30 -> 20 -> 10 as members
  fall. Dead squads are removed by `WorldZone.Advance`.
- `WorldZone.DeploySquad(rally, ...)` spawns a squad at the
  player's position; out-of-bounds rallies and unknown doctrines
  are rejected (null, no squad added). `Simulation.
  DeployOrbitalSquad(...)` exposes the same command to the
  headless loop. Squads are session-only in this prototype:
  they are NOT yet saved by SaveSystem, and there is no Orbital
  camera / Unity adapter yet — those are the next follow-ups,
  not claimed here. No deployment resource cost yet, also a
  follow-up, not a balance decision.
- Tests: **224 passed, 0 failed** (was 180; +44). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars
  (mesher 27 ms this run, timing varies).

## 0.8.0 — 2026-10-29 — Slice 2 Step 5: automation tier 1 (Kinetic only)

- Added `BlockType.KineticMill` (value 7, appended only — earlier
  values unchanged, so old saves still load) and `World.Build`
  support for it: 10 Stone + 5 Wood, same air-target and support
  rules as walls and workstations. A mill is solid and is itself
  not gatherable.
- Kinetic automation, in `WorldZone.Advance` every 5th tick: each
  mill excavates at most one adjacent gatherable block, in fixed
  neighbour order, into the player's inventory, by reusing
  `World.Gather` — so bedrock, walls, workstations, and other
  mills are never excavated, and a full inventory consumes
  nothing. Mills are scanned in chunk-index order, so two mills
  reaching for the same block resolve deterministically (first
  in scan order wins; exactly 1 block, not 2).
- `Simulation.BuildKineticMillAhead` (front-left, mirroring
  `BuildWallAhead`'s front-right) and `WorldZone.
  KineticBlocksGathered` (session counter, not saved) expose the
  system to the headless loop. No separate machine inventory yet:
  tier 1 output goes to the player's inventory, to be revisited
  when digital storage (tier 3) arrives. Tiers 2–4 (electrical,
  digital, arcane) are NOT in this step.
- Tests: **180 passed, 0 failed** (was 154; +26). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars
  (mesher timing varies run to run; 86 ms this run).

## 0.7.0 — 2026-10-29 — Slice 2 Step 4: Forge Survivor skill effects applied to gameplay

- `Simulation.ApplySkills(SkillBuild)` (also a third
  `Simulation(seed, equipped?, skills?)` argument) replaces any
  previous build — it never stacks — and applies the 0.5.0
  cluster for real: DamagePct scales weapon damage, CoreDamagePct
  adds on top for the core only, ShieldBonus raises the shield
  cap (85 -> 105 for the full cluster), MoveSpeedPct scales FPS
  speed (6 -> 6.3 m/s with Swift Feet), WorkstationRegenBonus
  adds to workstation regen (5 -> 10 per tick), and GatherPct
  accrues a fractional bonus unit (Scavenger +20% = one bonus
  unit per 5 successful gathers; failed gathers do not advance
  it). Null is rejected and leaves the build unchanged; an empty
  build respecs back to base values.
- Supporting core changes, no duplicate systems: `Player`
  gains `MaxShields` + `SetMaxShields` (which clamps current
  shields if a respec lowers the cap), `FpsController` gains a
  `SpeedMultiplier` / `EffectiveSpeed`, and `WorldZone` gains
  `WorkstationRegenPerTick`. Save/load is unchanged: a loaded
  world still starts at the base 85 cap until skills are applied.
- Damage rounding is away from zero, so a small bonus is visible:
  a 10-damage weapon with +5% deals 11, not 10. Discriminating
  test weapon is again seed-0 loot (20 damage): full cluster
  gives 24 enemy damage and 29 core damage.
- Tests: **154 passed, 0 failed** (was 128). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars.

## 0.6.0 — 2026-10-29 — Slice 2 Step 3: GeneratedWeapon integrated into Simulation

- `Weapon.FromGenerated(GeneratedWeapon)`: a generated weapon's
  name, damage, and mag size now become a live weapon — no
  duplicate damage / mag logic, and Simulation fires that Weapon,
  so generated stats are the stats that actually hit.
- `Simulation(seed, equipped?)`: pass a GeneratedWeapon to start
  equipped with it; no argument still gives the Pulse Rifle
  (42 mag / 120 reserve, 10 damage). `Simulation.Equip(...)`
  swaps to a generated weapon mid-run (fresh full mag, 120
  reserve); null is rejected and leaves the weapon unchanged.
- HUD weapon name now follows the equipped weapon
  (upper-cased, as before for the Pulse Rifle), instead of a
  hard-coded "PULSE RIFLE".
- Tests use a deliberately discriminating loot seed (seed 0:
  Titan Long Pulse Rifle, 20 damage, 30 mag) so the integration
  cannot pass by accident on the Pulse Rifle's 10 / 42 stats.
- Tests: **128 passed, 0 failed** (was 111). Demo unchanged:
  still gathered 1, built 1, killed 2, save JSON 43,852 chars.

## 0.5.0 — 2026-10-29 — Slice 2 Step 2: first passive skill cluster

- Added `SkillGraph.ForgeSurvivorCluster`: 6 nodes only, NOT the
  full 1,500-node graph. Travel: Sturdy Hands (+5% damage), Iron
  Skin (+10 shields), Swift Feet (+5% move speed). Notable:
  Forge Heart (+15% damage, +10 shields, requires S1+S2),
  Scavenger (+20% gather, requires S3). Keystone: Corebreaker
  (+25% core damage, +5 workstation regen, requires N1+N2).
- `SkillBuild`: prereq-gated allocation, duplicate/unknown
  rejected, summed effects, Base64 encode/decode round-trip.
  Effects are data only in this step — applying them to
  Player/Weapon/Simulation is the next Slice 2 step, so the
  graph can be tested before it changes gameplay.
- Tests: **111 passed, 0 failed** (was 100).

## 0.4.1 — 2026-10-29 — Workstation real effect + Slice 1 playtest gate doc

- Workstation now has a real, distinct effect: within Manhattan
  distance 3, it restores 5 shields per tick, capped at 85. A wall
  only blocks movement; a workstation repairs shields, so building
  one and defending it is a real decision. Tested: near = 65 -> 70
  shields, far = no restore.
- Added `docs/slice-01-playtest-gate.md`: headless evidence table,
  demo numbers, Unity verification checklist for Windows, and an
  honest pending director playtest section. Slice 1 is NOT claimed
  approved until Rob plays it in Unity and answers the gate
  question.
- Tests: **100 passed, 0 failed** (was 96).

## 0.4.0 — 2026-10-29 — Slice 2 Step 1: procedural loot

- Added `LootGenerator`: deterministic procedural weapons from
  Body / Barrel / Stock / Grip / Sight / Capacitor (element),
  rarity Common to Legendary, affixes by rarity, level-scaled
  damage. Same seed + level always gives the same weapon.
  Enemy drop: 30% chance, deterministic by seed + enemy id.
- Note on continuity: work pauses between user messages because
  this agent only runs on a user message or a scheduled job; it
  does not run continuously in the background on its own. The
  silence rule (no messages unless absolutely needed) is about
  messages, not about work — but without a scheduled job or a new
  user message, there is no new turn to do work in. See DECISIONS.
- Tests: **96 passed, 0 failed** (was 89).

## 0.3.0 — 2026-10-29 — Slice 1 Step 4: Unity adapters + headless demo

- Added Unity adapter scripts (NOT compiled headlessly, to be opened
  in Unity 6000 on Windows): `VoxelMeshRenderer` (assigns MeshBuilder
  buffers to a Unity Mesh, no duplicate logic), `FpsInputAdapter`
  (WASD, mouse yaw, Space jump, click fire, E gather, Q build, R
  reload), `HudDisplay` (health, shields, objective, ammo, in the
  reference HUD layout), `GameManager` (one Simulation, spawn wave,
  save to JSON). Load-from-JSON in Unity is documented as not yet
  a finished UI flow; headless load is verified in Core tests.
- Added `Simulation.FireAtCore` (10m range, 10 damage, consumes ammo
  even out of range) and a mesher benchmark test.
- Added `demo/Forgeborne.Demo`: headless 60-tick run — gather,
  build, fight, objective, mesher timing, save length. Verified run:
  gathered 1, built 1, killed 2, player died to enemy pressure
  (standing still after tick 20), mesher 198 quads / 792 vertices
  in 74 ms on the second run, save JSON 43,852 chars. Player death
  in the demo is pressure working, not a bug — movement and building
  are the counterplay, and the demo deliberately stops moving.
- Tests: **89 passed, 0 failed** (was 86). Mesher benchmark:
  13 ms for a generated chunk in tests (< 500 ms gate).

## 0.2.2 — 2026-10-29 — Simulation loop + silence rule

- Added `Simulation`: headless playable loop (FPS controller,
  world, weapon, objective) — move, gather, build, fire, kill,
  objective complete — tested headlessly as the build-around for
  no playable Unity build in this environment.
- Fixed a test-sequence support bug (not core logic): building a
  wall on a just-gathered cell correctly returned Floating, so
  `BuildWallAhead` now targets front-right, supported by ground.
- Communication rule recorded: no messages unless absolutely
  needed, until done, or in direct response to Rob. Progress is
  recorded here, not in chat.
- Tests: **86 passed, 0 failed** (was 78).

## 0.2.1 — 2026-10-29 — Build-around principle + MeshBuilder

- Working principle recorded (Rob): "I can't because…" is not a
  reason. Build around it; if you can't, build around the reason
  why, then build around that. See `DECISIONS.md`.
- Added `MeshBuilder`: converts greedy quads to Unity-ready mesh
  buffers (vertices, normals, UVs, indices) in pure C#. This builds
  around Unity / GPU not being available headlessly: mesh data is
  verified here, Unity adapter later only assigns the buffers.
- Tests: **78 passed, 0 failed** (was 70). Single voxel: 24 vertices,
  36 indices, unit axis normals, all indices in range. Generated
  world: 748 vertices, 1122 indices for 187 quads.

## 0.2.0 — 2026-10-29 — Slice 1 Steps 2-3: mesher, FPS, weapon, HUD

- Added `Mesher.GreedyMesh`: standard greedy rectangle merging.
  Verified: single voxel 6 quads, 2x2x2 cube 6 quads, flat 32x32
  slab 6 quads, generated world 187 quads vs 2608 naive faces.
  Empty chunk 0 quads. Quads carry axis, slice, bounds, type, and
  facing, with 4 corners.
- Added `FpsController`: speed 6, jump 8, gravity -20, AABB
  0.3 half-width x 1.8 height, axis-separated movement with wall
  sliding, grounded detection. Fixed a real tunnelling bug found
  by tests: a large fall step (4.4 -> 2.8) previously snapped to
  Y=3 instead of ground top Y=4; landing now snaps to the highest
  solid block at or below the previous feet position.
- Added `Weapon` (Pulse Rifle, 42 mag / 120 reserve, damage 10,
  fire / reload) and `HudState` (health, shields, DESTROY CORE
  objective distance and percent, ammo, enemy count, 32x32
  mini-map solid data). Core objective added to `WorldZone`
  (core at 28,5,28, health 100, DamageCore).
- Fixed a test arithmetic error (not code): reload reserve is
  120 - 42 = 78, not 79.
- Tests: **70 passed, 0 failed** (was 42). Still no Unity scene,
  no GPU rendering, no Orbital mode — those remain later steps,
  not claimed here.

## 0.1.0 — 2026-10-29 — Slice 1 Step 1: deterministic voxel core

- Added `src/Forgeborne.Core`: 32^3 chunk, deterministic seeded
  generation (bedrock, stone, ~4% EmberOre, sparse wood), gather
  (stone / ore / wood, bedrock ungatherable, inventory-full does not
  consume the block), build (wall 2 stone, workstation 10 stone +
  5 ore, support rule, no floating builds), player (100 HP, 85
  shields, shields-first damage, death at 0), enemies (move toward
  player, attack adjacent for 10, death removes), waves, and JSON
  save / load (chunk Base64, player, inventory, enemies with health,
  seed, tick).
- Added `tests/Forgeborne.Core.Tests`: headless console test runner,
  no external packages. Verified 2026-10-29 with .NET 8 SDK
  (8.0.427, installed locally in the dev environment, not in the
  repo): **42 passed, 0 failed.**
- Added `ForgeborneUnity/` skeleton: Packages manifest (Entities,
  URP, Input System), ProjectVersion placeholder, and an integration
  note. NOT compiled in the Linux headless environment; to be opened
  in Unity 6000 on Windows. Canonical state remains
  `Forgeborne.Core.WorldZone`.
- Known limits of this step: no Unity scene, no FPS input, no voxel
  meshing / rendering, no HUD, no Orbital mode. Those are Slice 1
  Steps 2-4, not claimed here.

## 0.0.2 — 2026-10-29 — Delegation, engine, art direction

- Director delegation recorded: Rob authorised Ember to make all
  future project decisions, with months acceptable, no rushing.
  Decisions are logged in `DECISIONS.md` for review / revocation.
- Engine locked for Slice 1: Unity 6000 (Unity 6) + Entities / DOTS,
  URP, C#. Rationale in `DECISIONS.md` and `README.md`.
- Art direction added (`docs/art-direction.md`) from the
  director-provided reference image. Reference is style only, not a
  shipped asset. All in-game art must be original.
- Licence: no open-source licence added at foundation stage.
  Repository is public for transparency; contents remain WPAI
  property, all rights reserved, until a release licence is chosen.

## 0.0.1 — 2026-10-29 — Foundation

- Repository created manually by Rob at
  https://github.com/MrWizard94-Compile/Forgeborne.git (public, main).
  API creation attempt by Ember failed with 403; see `DECISIONS.md`.
- Initial foundation commit: README, WPAI Constitution project pointer,
  MANIFEST, DECISIONS, CHANGELOG, architecture summary, and Slice 1 plan.
- No playable game, no engine choice, and no performance claim in this
  version. Foundation only.
