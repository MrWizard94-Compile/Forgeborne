# Forgeborne — Changelog

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
