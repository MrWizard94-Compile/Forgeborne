# Forgeborne

AAA+ target, PC-native, FPS-RTS ARPG hybrid.

**Gather. Build. Survive.**

Forgeborne merges, in original form only, the strengths of modded Minecraft
(gather / craft / build / forge), StarCraft (real-time macro and escalating
pressure), Diablo and Path of Exile 2 (procedural loot, builds, and a deep
passive graph), BioShock (immersive atmosphere and systemic powers), and
Borderlands (procedural weapon variety and personality).
All six influences are equally important. Rob clarified that modded Minecraft
supplies the gather/build/survive/combat concept. Forgeborne requires believable
original 3D environments; the blocky world aesthetic is explicitly rejected.

No assets, lore, characters, items, music, code, or trademarks from those
games are copied. Inspiration is by mechanics and feel only. All world,
names, story, art, code, and audio in this repository are original.

## Status

**Pre-production. Director rejected current presentation; AAA+ target unmet.**

Rob's 2026-11-01 review rejected the sparse primitive presentation. Original
environment/weapon/actor art, atmospheric lighting, animation and a cohesive
playable encounter now take priority over feature expansion. Technical checks
are scoped evidence; visual, gameplay and fun acceptance remain closed.
Main now enters the rebuilt Cinder Exchange through saved surface profile 3,
with retained profile 2 for the original authored world.
Authored architecture, deposits and constructed machines share their exact
triangles with collision and aiming. Model-shaped construction ghosts replace
the cube outline for new games. Old saves retain their previous rules. See
[the current organic meshes/native evidence and remaining quality gaps](docs/foundry-v2-checkpoint.md)
and [the world migration](docs/world-presentation-migration.md).
The preceding 0.127 adds original continuous skinned gloves, refined carbine
assemblies, transparent optic and reload presentation. Source/import, actual GPU
poses/materials and native firing/reload/orbital/pause/load checks are recorded in
[the carbine checkpoint](docs/carbine-art-checkpoint.md). AAA+ remains unmet.
The preceding 0.128 slims the housing and refits its assemblies, with a saved holding
angle that exposes the firing hand. Actual render, shot and native evidence is in
[the first-person craft checkpoint](docs/first-person-framing-checkpoint.md).
The preceding 0.129 connects the support-hand reload to the magazine/charging slide
and corrects sleeve cuff binding. It remains cosmetic, with further art and
motion work required. See [the hand-motion checkpoint](docs/hand-motion-checkpoint.md).
The preceding 0.130 replaces swollen palms/uniform finger tubes with shaped original
gloves and fitted, skin-weighted stitches. A native thumb candidate was rejected
and corrected. Actual import, poses, rendering and affected Windows controls pass
their scoped checks; finer tailoring and continuous motion remain open. See
[the glove-form checkpoint](docs/glove-form-checkpoint.md).
The preceding 0.131 corrects the support elbow's camera-local placement and removes
detached-looking round knuckle trim. Forearm length and source grip landmarks
are preserved; actual rendered/native poses and affected controls were checked.
Further anatomy, tailoring and motion remain open. See
[the arm-fit checkpoint](docs/arm-fit-checkpoint.md).
The preceding 0.132 adds woven glove-back panels, fitted seam boundaries, shaped fingers
and shallow flexion folds while preserving the rest rigs. A rendered candidate
with stray fabric patches was rejected and repaired. Source/import, actual poses/
rendering and affected native controls are recorded in
[the glove-construction checkpoint](docs/glove-construction-checkpoint.md).
The preceding 0.133 slows the support-hand reload return, adds eased outward transition
arcs and preserves original art/gameplay. Actual rig, skin, real Unity clips and
affected native controls are checked; anatomy and final motion quality remain
unaccepted. See [the reload-motion checkpoint](docs/reload-motion-checkpoint.md).
Current 0.134 corrects the support palm/wrist layout, refits thumb and finger
forms, and closes the charging grasp. The sewn panel now follows the palm,
preventing thumb rotation from pulling its corners into spikes. Source/imported
skin, actual Unity renders and native evidence are recorded in
[the glove-anatomy checkpoint](docs/glove-anatomy-checkpoint.md). AAA+ remains unmet.

`ForgeborneUnity/Assets/Scenes/Main.unity` now opens in Unity 6000.3.27f2.
It wires the FPS, chunk, HUD and squad adapters to GameManager's single
Seed 42 Simulation, with a URP renderer, dusk sky, original weapon/drone/core
views, the authored exchange hall, resource materials, pointer capture,
entry/pause/restart and local slot save/load, with a fixed real-time world cadence. Unity MCP was used
to create and inspect the scene on Windows. Exact interactive pose/time saves are
verified. Windows export/import passes earlier actual Editor UI checks. Earlier
native packages passed; 0.126 has a successful historical Windows QA package and
actual v1 load/v2 restart, Tab cutaway/FPS, firing/reload and Escape checks.
The corrected build also proves native escape from a formerly stuck contact
through actual W input and Windows import/export. Full native loop/current-v2 cold-save
recovery and performance acceptance remain open. Studio-quality encounter/art/
audio remain active work. A perspective overhead command view now has live
component/render evidence and a native Tab-switch check; its full play gate remains open.
Resolved combat/gather/build feedback and eleven original synthesized audio
candidates now pass scripted Unity integration checks; heard mix review is open.
Placement rejects suit/unit/relay overlap before spending resources, with
production adapter checks and exact refusal-state preservation.
See [the production tracker](docs/production-status.md),
[construction safety evidence](docs/construction-safety-checkpoint.md),
[action feedback evidence](docs/action-feedback-checkpoint.md),
[Windows transfer evidence](docs/windows-save-transfer-checkpoint.md),
[orbital camera evidence](docs/orbital-camera-checkpoint.md),
[current persistence evidence](docs/runtime-persistence-checkpoint.md),
[player-flow evidence](docs/courtyard-player-flow-checkpoint.md), and
[integration setup](docs/unity-main-integration.md).
A passing check does not prove usefulness or fun.

## Locked decisions (director: Rob, 2026-10-29)

- **Platform:** PC native. Web-first is superseded.
- **Perspective:** First-Person Tactile Mode for gather / build / combat,
  Orbital Overhead Tactical Mode for RTS command. Third-person is superseded.
- **Multiplayer:** Single-player first. Networking is deferred. Deterministic
  ECS runs locally first; client-server sections of the architecture are
  future reference, not Day 1 work.
- **Core pillars, in order:** resource gathering, building, surviving.
- **Hard limits:** 2D is strictly off limits. No turn-based mechanics.
  No twin-stick Asteroids loop. Not a reskin of Ember Forge.
- **Quality gate:** Studio quality and production grade are requirements,
  not suggestions, for every milestone. See `MANIFEST.md`.

## Architecture (summary)

See `docs/architecture.md` for the full summary. In brief:

- Voxel world: dual-density blocks + signed distance fields, chunks of
  32^3 or 64^3, GPU greedy meshing with indirect draws, chunk merging.
  Targets from the blueprint: < 1.5 MB per 64^3 chunk, < 400 total draw
  calls, 60+ FPS at 1440p on standard PC hardware (to be proven, not assumed).
- Navigation: Sparse Voxel Octree encoding free space, not solids, for true
  3D pathfinding. ECS parallel A* with spatial hashing and hierarchical
  path abstraction. Blueprint claim: 50,000+ entities — a scaling target,
  not a Slice 1 requirement.
- Progression: 1,500+ node passive graph, gem / support-gem composition,
  procedural weapons from Body / Barrel / Stock / Grip / Sight / Capacitor,
  intransitive elemental matrix interacting with the voxel world.
- Automation: four tiers — Kinetic / Mechanical, Electrical (LV to EV),
  Digital (ME-style), Arcane / Source.
- Command: doctrine-based squads (formation, line-of-sight, volumetric
  scanning, perching) via StateTree / EQS-style evaluation, not individual
  micromanagement. Commander moderation controls against griefing.
- Canonical state: ECS / SVO is the source of truth. Any neural or
  procedural prediction is suggestion only and must be validated before
  it affects gameplay, saves, or progression.

The Microsoft MUSE / WHAM neural layer from a separate research draft is
**not** part of this canonical blueprint. See `DECISIONS.md`.

## Slice 1 — what comes next

See `docs/slice-01-plan.md`. Slice 1 is: one small voxel zone, FPS
gather / build, canonical ECS / SVO state, local saves with export /
import, and tests. It must prove that gather → build → survive is fun
before any other system is added. Performance is proven with hundreds of
entities first, not tens of thousands.

## Repository layout

- `PROJECT-POINTER.md` — WPAI Constitution adoption pointer (not a copy
  of constitutional law).
- `MANIFEST.md` — mission, scope, non-goals, quality gate.
- `DECISIONS.md` — dated director decisions, with superseded proposals
  kept visible, not erased.
- `CHANGELOG.md` — what changed, by version.
- `docs/architecture.md` — architecture summary and targets.
- `docs/slice-01-plan.md` — Slice 1 definition of done.

## Building

**Engine, locked 2026-10-29 under director delegation (see
`DECISIONS.md`): Unity 6000 (Unity 6) + Entities / DOTS, Universal
Render Pipeline (URP), C#.**

Rationale in brief: DOTS / Entities is the blueprint's ECS foundation
and the proven route to tens of thousands of entities; C# is the
fastest iteration language for a one-person, AI-assisted team; URP,
not HDRP, is chosen for performance on mid-range hardware (including
the director's GTX 1660 Ti-class dev machine) at the 60 FPS / 1440p
target. Unreal's Mass / StateTree / EQS concepts are adopted as
design patterns (doctrine-based squads, environment queries), not as
a requirement to use Unreal Engine. If Slice 1 proves Unity DOTS
voxel meshing insufficient, the engine decision is re-opened before
Slice 2 — it is locked for Slice 1, not irreversible for the whole
game.

Art direction is recorded in `docs/art-direction.md`, from the
director-provided reference image. That image is a style reference
only; it is not a game asset and must not be shipped as one.

## Wizard Productions AI Studio

Forgeborne is a Wizard Productions AI Studio (WPAI) project. Human-directed,
AI-assisted, with AI use disclosed on releases, per WPAI philosophy and
the WPAI Constitution (see `PROJECT-POINTER.md`).
