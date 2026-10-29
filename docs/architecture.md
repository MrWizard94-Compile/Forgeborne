# Forgeborne — Architecture Summary

Source: "Technical and Systems Architecture for a Studio-Grade
FPS-RTS Voxel ARPG Hybrid", Version 3, blueprint for Ember (muse.ai)
to build. This file is a summary, not a copy of the full blueprint.
Targets below are blueprint targets to be proven by measurement;
they are not results.

## Voxel world and rendering

- Chunks: 32^3 or 64^3 voxels.
- Storage: bit-packed 16-bit block IDs + 8-bit signed distance field
  (SDF) for smooth terrain. Voxels more than 2 units from an isosurface
  rounded to binary solid / air for compression.
- Meshing: GPU greedy meshing + indirect draw commands
  (glMultiDrawElementsIndirect / D3D12 / Vulkan indirect buffers).
  Adjacent coplanar faces merged into quads.
- Worst-case quad bound: Q_max = L^3 · 6 · (1 − x) · x, peaking at
  occupancy x = 0.5.
- Range analysis: min / max density per chunk bounds evaluated before
  vertex generation; empty or fully enclosed volumes skip meshing.
- Chunk merging: adjacent sub-chunks into large clusters (e.g.
  512×512 regions) to reduce draw calls.
- Targets: < 1.5 MB per 64^3 chunk, < 400 total draw calls, zero CPU
  stall from meshing, 60+ FPS at 1440p on standard PC hardware.

## Navigation

- Do not use static 2.5D Recast meshes: dynamic voxel deformation
  invalidates them, and they do not serve burrowing or aerial units.
- Use a Sparse Voxel Octree (SVO) encoding free traversable air and
  tunnel space, for true 3D pathfinding.
- Scale: 0.5 cm resolution up to streamed open-world environments.
- Engine: parallel DOTS / ECS, SIMD-accelerated A* over spatial
  hashing, hierarchical path abstraction (HPA).
- Streaming: distance-based navigation invokers, flat memory budget.
- Blueprint claim: 50,000+ active entities at 60 FPS. This is a
  scaling target, not a Slice 1 requirement. Prove hundreds first.

## Progression

- Passive graph: 1,500+ nodes — Attribute Travel (+5 Strength /
  Dexterity / Intelligence), Small Passives (3–8%), Notable Passives,
  Keystones (rule-rewriting, e.g. shield → ammo, damage → mana).
- Data: JSON schema with coordinates, clusters, in / out adjacency.
  Validation by graph traversal for path continuity.
- Serialisation: Base64 bit-packed; 120 allocated nodes < 100 bytes.
- Loadouts: separate passive configurations per weapon set; swapping
  weapons transitions active branches.
- Active skills: firearm in primary hand, tactical / elemental ability
  in off-hand. Support gems modify orthogonally: Greater Multiple
  Projectiles, Chain / Fork, Lifetap, Elemental Conversion.

## Automation (four tiers, one at a time)

1. Kinetic / Mechanical: steam, waterwheels, cranks; shafts, gears,
   belts, gearboxes; crushing and excavation.
2. Electrical (LV to EV): alternators, insulated cables, transformers;
   mismatched voltage without transformers destroys machines.
3. Digital (ME Network): digitised spatial storage cells, glass fibre
   data channels, automated craft-on-demand across assembly lines.
4. Arcane / Source: Sourcelinks, Archwood, essence, relays, jars;
   converters transmute Source into electrical or rotational power.

## Command (FPS-RTS)

- Modes: First-Person Tactile ↔ Orbital Overhead Tactical.
- No individual micromanagement. Squads follow doctrines via
  StateTree-style evaluation + 3D Environment Query System (EQS):
  formation slots, line-of-sight engagement using voxel cover,
  volumetric scanning by recon drones, perching / landing for flyers.
- Focus: macro-economy, spatial expansion, army composition, not APM.
- Anti-griefing: commander controls — vehicle access lockouts,
  remote ejection.

## Weapons and elemental system

- Procedural assembly: Body, Barrel, Stock, Grip, Sight, Elemental
  Capacitor — fire rate, recoil, magazine, elemental channel.
- Intransitive matrix:
  - Incendiary: strong vs unarmoured flesh; ignites gas / timber;
    weak vs kinetic shields.
  - Shock: strong vs shields / electronics; conducts through water;
    weak vs heavy armour.
  - Corrosive: strong vs plated armour / mech hulls; melts soil,
    structural collapse; weak shield suppression.
  - Cryo: strong vs high-mobility light units; freezes water into
    bridges; weak vs high-mass structures.
- Systemic powers: Telekinesis (throw voxels / ordnance),
  Electro-Bolt (wiring / water, generator locks), Incinerate (fuel,
  wooden supports, ceiling collapse).
- Narrative: audio logs, factory broadcasts, environmental details
  embedded in voxel ruins — without interrupting combat.

## Networking (deferred)

- Blueprint dual layer (future reference only, not Slice 1):
  client-server prediction + rollback for FPS action; deterministic
  rollback for mass ECS RTS (send inputs / doctrines, simulate
  locally); voxel delta compression as spherical payloads
  (Position, Radius, Material Output ID).
- Single-player first: deterministic ECS runs locally. Do not build
  networking until the director un-defers it.

## Memory budgets (blueprint targets, 60+ FPS at 1440p)

- Voxel Engine: Async Worker Pool — 3.0 GB VRAM / 2.0 GB RAM.
- Mass ECS AI: Job System (all threads) — 1.5 GB RAM.
- Passive Skill Graph: Main / Worker — 200 MB RAM.
- Factory / AE2 Nets: Fixed Interval Worker — 1.0 GB RAM.
- Rendering / Audio: Render / RHI Thread — 4.0 GB VRAM.

## What is not canonical

- Microsoft MUSE / WHAM neural frame prediction, MaaG neural
  consistency, and VM-based agent runtimes are NOT in this blueprint
  version. They are separate research reference. Canonical state is
  ECS / SVO.
