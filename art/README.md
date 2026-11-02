# Original Forgeborne art sources

These are original work-in-progress assets. They do not establish the requested
AAA+ quality or approval of the current world. Rob explicitly rejected the cubic
courtyard. Its replacement is tracked in docs/world-presentation-migration.md.

The current new game uses the retained Cinder Exchange v2 catalog. Its original
physical source is `source/CinderExchangeV2.blend`, authored by
`tools/build_foundry_v2.py` with `tools/fracture_geometry.py`. The same closed
triangles feed the renderer, collision, gather, aiming and construction. V1's
JSON, generator and Blender source remain frozen for existing saves. See
`docs/foundry-v2-checkpoint.md` for exact identities, reproduction, actual Unity
and limited native evidence, and remaining art gaps. Future physical changes
must retain existing catalog identities or implement a tested migration.

Generate v2 from the repository root with the pinned Blender executable:
`--background --python-exit-code 1 --python art/tools/build_foundry_v2.py`.
The explicit Python exit code is required: Blender's default process status can
otherwise report success after an authoring exception. Two final geometry
generations matched SHA-256; Blender container bytes are not reproducible.

Eight original paired 1024-square pigment/HROM recipes are in
`tools/bake_surface_library.py` and `source/FoundrySurfaces.json`. Unity's
IndustrialSurface shader consumes those maps; CastFloor reuses the Basalt pair
with separate relief/roughness settings. No downloaded art was introduced.

## Earlier model sources

`tools/build_visual_assets.py` authors the geometry in Blender 4.5.15 LTS. No
models, textures, character templates or sampled art were imported. The script
creates each `.blend` source, a geometry manifest and its Unity FBX export in one
run. Blender is a developer tool kept outside this repository; it is not shipped.

| Asset | Purpose | Triangles |
|---|---|---:|
| PulseCarbine | Original refined carbine, continuous skinned gloves, moving assemblies and muzzle landmark | 89528 |
| EmberWarden | Original compact hovering enemy shell and sensor | 7024 |
| AetherRelay | Original caged induction chamber/objective | 4820 |
| ForgeSuit | Original 1.79 m pressure suit for the orbital view | 12144 |
| RelayFoundry | Architectural context, service bays, field rings and distant plant | 79308 |

The suit is segmented, not skeletal/animated yet. Wardens still move by canonical
grid steps. These are named quality gaps, not completed character animation.
RelayFoundry is the earlier decorative backdrop and owns no collision. It is
disabled in authored v1/v2 worlds. The current traversable exchange is the
separate shared physical catalog described above.

Current PulseCarbine delegates to `tools/build_carbine_v2.py`: two continuous
gloves, seventeen original joints per hand, at most four normalized influences,
and no parallel collision. New GloveFabric and SleeveFabric pairs are authored separately
by `tools/bake_glove_surface.py`; existing eight pairs are retained byte-exact.
The original OpticGlass shader supplies the transparent combiner. See
`docs/carbine-art-checkpoint.md` for source/import audits, actual GPU/native
evidence, limits and full reproduction commands. The current 89,528-triangle count stays
under its 100,000 budget; this is not proof of AAA+ quality or native performance.
The 0.128 receiver/framing pass is in `docs/first-person-framing-checkpoint.md`.
At that checkpoint, source hand positions, bindings and rigs matched 0.127 exactly, with equivalent
oriented polygon/material sets despite raw trim-face ordering differences.
The preceding 0.129 pass changes only 640 cuff-vertex bindings per hand from palm
to forearm and adds runtime grasp/charging motion. Actual indexed hand geometry,
rest rigs, other weights and all twenty maps are preserved. The regular generator
contains the binding correction; `tools/rebind_carbine_cuffs.py` can migrate an
existing original source without regenerating geometry. See
`docs/hand-motion-checkpoint.md` for source comparison, import, rendering and native evidence.
Current 0.130 replaces swollen palms/uniform finger tubes with tapered wrist/
metacarpal forms, shaped phalanges, rounded tips, blended smaller thumb pads and
dorsal stitching fitted to the actual skin. Stitch weights follow the nearest
skin vertex. Glove geometry/bindings are intentionally regenerated; rest rigs,
landmarks, quantized weapon surfaces and all twenty PNGs are preserved. The
read-only `tools/audit_glove_form.py` checks closed bodies, joint coverage,
normalized skin and stitch fit. See `docs/glove-form-checkpoint.md` for rejected
candidates, source/import/render/native evidence and remaining craft gaps.

## Reproduce and inspect

From the repository root in PowerShell, with Play stopped:

```powershell
& 'C:\Users\Bulkl\.codex\tools\blender-4.5.15\blender-4.5.15-windows-x64\blender.exe' --background --python-exit-code 1 --python art\tools\build_visual_assets.py
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
also run against the new art. These are scripted Editor checks. The current
checkpoint records limited actual native controls and retained-world loading.
The complete native loop, heard mix, sustained performance and Rob's AAA+/fun
gates remain unmet; no screenshot or model count can grant those gates.
