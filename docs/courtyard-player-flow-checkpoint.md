# Courtyard and player-flow checkpoint — 2026-10-31

Status: **verified intermediate work; studio-quality goal remains active**.
Follows runtime commit `292c82b`. No push, release, visibility or licence change.

## Result and manifest

- `CourtyardEncounter` authors an original arrival area, two genuine stone/ore
  salvage banks, wood stores, broken cover walls, approach lanes and a raised
  relay structure into the one canonical chunk. It does not create a Simulation.
  Initial wardens approach through the forward view. Plain WorldZone generation
  and existing headless contracts are unchanged; GameManager opts into the layout.
- MeshBuilder now includes stable block IDs in its existing mesh buffers.
  VoxelMeshRenderer combines the artistic resource palette with the existing
  power tints, and passes block IDs to the actual Unity mesh. Original URP
  `Forgeborne/VoxelSurface` adds readable block edges, procedural surface variation,
  directional shadows/fog and subtle ember emission. It uses normal Unity
  framework includes/passes; no external game asset or source was copied.
- `PlayerFlow` provides a real entry/pause menu, terminal/restart recovery,
  local save/load slot, resource counts and build-cost hints. Pausing is a normal
  single-player menu, not a turn-based build/combat phase. Live encounters still
  advance in real time. Restarting a live encounter requires confirmation;
  quitting an active encounter attempts a save and surfaces failure before discard.
- `LocalSaveStore` writes bounded strict UTF-8, flushes a temporary file before
  same-volume replacement, retains the previous slot as a backup, and bounds
  reads to 2 MiB. Temporary files are cleaned on failure. UI errors retain the
  live world and avoid exposing machine paths. The initial-save message does
  not claim a backup existed before the first slot.
- Main now includes PlayerFlow, the material and the larger visible crosshair.
  Company/product identity is Wizard Productions AI Studio / Forgeborne; the
  native Unity input setting remains Both. No pre-existing slot existed at the
  new tested data path. The QA slot and backup remain local, outside Git.
- The core DLL was rebuilt/published after core changes. No core `.cs` file was
  added under Assets. Native Unity metadata, scene and settings remain versioned.

## Verification and limits

Core build: **0 warnings, 0 errors**; **5118 passed, 0 failed** on .NET 8.0.33.
New checks cover deterministic/idempotent layout, clear arrival/support, available
wall/workstation resources, open spawn/core cells, world-save retention, mesh
block identities, atomic file replacement/backup, size limits, malformed UTF-8,
BOM handling, pending-file cleanup and backup restore through Simulation.Load.

Installed netstandard2.1 core DLL SHA-256:
`d2d5e296b3b6bdd3af2484eea77365a7a06e1244fd71aa39f6cc39ea23e07a27`.
Unity 6000.3.27f2 on GTX 1660 Ti rendered the courtyard with distinct resource
colours, cover, shadows, the visible relay and wardens. The custom shader's
first compile failed on an HLSL reserved identifier; that source error was fixed.
ShaderUtil subsequently returned **0 messages**. Final console error/warning
queries returned **0 entries**.

The live mesh check passed with **1348 vertices / 674 triangles**, matching
MeshFilter/MeshCollider objects, full colour/block-ID channels and the same
Simulation in all five linked adapters. Production in-memory save/load also
passed with a 44,003-character snapshot. These are integration observations,
not a native-build performance or visual-quality acceptance.

Actual Windows Game-view UI actions were observed:

| Player action | Observed result |
|---|---|
| Open Main in Play | Entry menu, Time.timeScale 0, pointer released, no pressure before entry |
| Click Enter | Menu closed, time scale 1, pointer captured |
| Press Escape | Pause menu, time scale 0, pointer released |
| Click Save Slot | Local 44,003-byte JSON exactly matched current canonical save |
| Resume and stand under pressure | Real wardens caused death; SUIT BREACH menu appeared |
| Click Load Slot after death | Valid state restored, paused for an explicit resume; saved/current JSON identical |
| Restart through menu, before confirmation hardening | Fresh 100 HP / 85 shield encounter; all linked input still used the same Simulation; final confirmation tested separately below |
| Press W after restart | Feet Z moved from 16 to 16.05; subsequent Escape paused again |
| Stop/re-enter Play, then click Load Slot | Disk snapshot restored into the fresh session; JSON identical |

Additional live Unity calls exercised the actual PlayerFlow methods: malformed
slot content was rejected without changing the world or Simulation (the QA file
was restored in `finally`); restart confirmation preserved a staged change until
confirmed; the second slot write's backup matched its exact prior bytes. The
confirmation/backup checks were scripted integration tests, not claimed as mouse
actions. No pre-existing player slot was overwritten by this QA setup.

## Still required

Repository inspection was targeted; long historical decision/changelog records
were read selectively. All supplied production-brief attachments were read fully.

This is not the final graphics/gameplay delivery. Precise FPS position, velocity,
look and fractional clock persistence remain open; current legacy saves retain
grid position/ticks only. Export/import, a true overhead view, coherent gathering/
building/weapon/impact feedback, original audio, final animation/environment art,
an encounter that proves the full loop, native-player QA and sustained 1440p
profiling remain required. Rob's fun/quality review is pending.

Section 0: 1 FAIL (full requested experience incomplete), 2 PASS, 3 PASS for scoped
checks, 4 FAIL (whole player/native flow still open), 5 PASS, 6 PASS, 7 FAIL
(representative performance), 8 PASS, 9 PASS for this checkpoint, 10 PASS,
11 PASS with Unity asset GUID variance, 12 PASS (original work/framework use),
13 N/A (single integrator), 14 PASS, 15 PASS.

Self-audit: core checks and the installed DLL agree; material channels were
observed in the real renderer. Actual menu/input/slot/re-entry paths were
exercised, and error/confirmation paths preserved state. A shader compile failure
was fixed at its cause, then rechecked. Precise pose/time persistence and the
remaining production/art/native gates are explicitly open. Nothing was published
and no director approval or fun acceptance is implied.
