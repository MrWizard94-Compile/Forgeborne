# Original Forgeborne art sources

These are original work-in-progress assets. They do not establish the requested
AAA+ quality or approval of the current world. Rob explicitly rejected the cubic
courtyard. Its replacement is tracked in docs/world-presentation-migration.md.

`tools/build_visual_assets.py` authors the geometry in Blender 4.5.15 LTS. No
models, textures, character templates or sampled art were imported. The script
creates each `.blend` source, a geometry manifest and its Unity FBX export in one
run. Blender is a developer tool kept outside this repository; it is not shipped.

| Asset | Purpose | Triangles |
|---|---|---:|
| PulseCarbine | Original receiver/barrel/induction assemblies, gauntlets and muzzle landmark | 21602 |
| EmberWarden | Original compact hovering enemy shell and sensor | 7024 |
| AetherRelay | Original caged induction chamber/objective | 4820 |
| ForgeSuit | Original 1.79 m pressure suit for the orbital view | 12144 |
| RelayFoundry | Architectural context, service bays, field rings and distant plant | 79308 |

The suit is segmented, not skeletal/animated yet. Wardens still move by canonical
grid steps. These are named quality gaps, not completed character animation.
The foundry currently stands outside the playable yard and owns no collision.
Do not treat the backdrop as a completed traversable environment.

## Reproduce and inspect

From the repository root in PowerShell, with Play stopped:

```powershell
& 'C:\Users\Bulkl\.codex\tools\blender-4.5.15\blender-4.5.15-windows-x64\blender.exe' --background --python art\tools\build_visual_assets.py
```

The pinned official portable archive was SHA-256 verified:
`52dd02fd74877fa08e74b67d7854cf32468a232c02aaa19b6844c91285574f8d`.
Source: https://download.blender.org/release/Blender4.5/ . Tool provenance is kept
alongside that local tool installation. `.blend1`/`.blend2` recovery copies remain
ignored and preserved, not distributed as source duplicates.

Let Unity finish importing. Run `Forgeborne > Art > Prepare Authored Models`.
That operation maps named material roles to the original IndustrialSurface shader,
then saves prefabs. Unity materials are the runtime color/roughness authority;
Blender's material previews are authoring aids. `Apply Foundry Atmosphere` requires
saved Main, stopped Play and completed imports. It persists the sky/grade/layers,
assigns actual URP post-processing resources and enables restrained contact shading.

Import validation checks signed X/Y/Z markers, metres, muzzle position, materials,
mesh bounds and absence of art colliders. `AuthoredSkin` preserves the FBX import
rest transform below a neutral game-space wrapper. Never overwrite that transform
to move a model; animate the wrapper or a named assembly.

FBX/.blend container bytes can vary between regenerations because of Blender
metadata/IDs. Reproducibility is checked at geometry/consumer level (counts,
bounds, material roles, unit scale, signed markers and muzzle); current file hashes
are recorded separately in docs/evidence/2026-11-01/authored-0122-assets.json.
Those are raw current workspace hashes; Git's checkout line-ending conversion can
also change text-file bytes. Regenerate the manifest after a new import/checkpoint.

`tools/unity-art-consumer-smoke.cs` runs against paused actual Main and verifies
resource consumers, the authored suit handoff, unchanged Simulation/state and a
rendered exposure response. Existing feedback/orbital/construction-guide checks
also run against the new art. These are scripted Editor checks. Native art,
heard mix, sustained performance, world geometry and Rob's visual/fun gates remain
unmet; no screenshot or model count can grant those gates.
