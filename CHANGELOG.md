# Forgeborne — Changelog

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
