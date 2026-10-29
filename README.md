# Forgeborne

Studio-grade, PC-native, FPS-RTS voxel ARPG hybrid.

**Gather. Build. Survive.**

Forgeborne merges, in original form only, the strengths of modded Minecraft
(gather / craft / build / forge), StarCraft (real-time macro and escalating
pressure), Diablo and Path of Exile (procedural loot, builds, and a deep
passive graph), BioShock (immersive atmosphere and systemic powers), and
Borderlands (procedural weapon variety and personality).

No assets, lore, characters, items, music, code, or trademarks from those
games are copied. Inspiration is by mechanics and feel only. All world,
names, story, art, code, and audio in this repository are original.

## Status

**Foundation only. No playable game yet.**

This commit establishes the repository structure, the WPAI Constitution
pointer, the locked director decisions, the architecture summary, and the
Slice 1 plan. It does not claim a working game, a chosen engine, or any
performance result. A passing check does not prove usefulness or fun.

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
