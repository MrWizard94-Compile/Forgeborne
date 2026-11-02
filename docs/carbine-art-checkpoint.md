# Carbine and glove checkpoint — 0.127.0

The original carbine now has continuous anatomical gloves, an original deformation
rig, a seated tapered magazine/grip, a bored muzzle, open shroud, curved trigger
guard, service fittings and transparent reflex glass. This addresses Rob's request
for more realistic, organic meshes. It is a verified contribution to the full
six-influence game, **not an accepted AAA+ art or gameplay milestone**.

Baseline: `1f40dac4f61dae18f1cf8a96c81508167d7b6d5f`; implementation branch:
`codex/carbine-glove-refinement`. Rob's existing “just update main” authority covers
integration of this verified checkpoint. No release, licence or visibility change.
Reverting this contribution restores the earlier presentation without a save
migration. Do not restore private backups over later user saves.
Rob questioned the initial native arm/hand appearance before publication. That
candidate is preserved under `rejected-155646Z/`, clearly separate from the revised
evidence. The support forearm was approximately .50 m and read as a black tube.
The revision reduces it to .2893 m (right .2625 m), adds oval cloth volumes/folds
and separate sleeves, slims the palms, varies finger lengths and fixes wrist/grip
placement. Technical rig passes did not validate the rejected appearance. The
revised candidate still requires Rob's visual review.

## Original assets and runtime behavior

`art/tools/build_carbine_v2.py` authors the Blender/FBX source. The final carbine
contains **87,884 triangles / 44,194 source vertices**, below the declared 100,000
triangle view-model budget. The prior asset had 21,602 triangles; the increase is
not evidence of artistic quality. The gloves are continuous closed remeshed
surfaces before rigid trim, with opposed thumbs, varied finger lengths, slimmer palms
and connected forearms. There are seventeen original joints per hand and at most
four normalized influences per vertex. Unity uses a shared 34-bone palette for
each skin; validation proves each hand actually uses only its own seventeen
joints. Both actual skinned consumers are retained; there are no art colliders.

Source inspection and Unity import agree on all 87,884 triangles. Zero-area bevel
faces found during authoring were corrected before export. Side-view inspection
also caught an assembly-pivot error; the generator now preserves and verifies
child world matrices when placing the magazine/charging-handle pivots. The final
inspection views show those assemblies seated. Signed axes, metres and the
original muzzle landmark `(0, .016, .665)` remain checked.

`WeaponViewModel` owns a presentation-only 0.8-second reload: the support hand
leaves the handguard, turns toward and draws the magazine, then returns. Forearm
length is preserved. Finger flex accompanies release and firing. Exact authored
local transforms are restored by reset; paused time is stable and orbital hides
the view. The manual pose driver is the sole animation owner for this prefab. A named
authored palm contact drives magazine grip alignment. A subtle view light affects
only layer 30 and switches off in orbital; it does not illuminate world objects.
Core's reload/ammo/damage/input rules remain unchanged; a shot can interrupt the
gesture. This is still a candidate animation, not final tactile weapon feel.

The new original GloveFabric and SleeveFabric pairs add leather grain and woven
cloth through the existing pigment/HROM pipeline. Two consecutive four-map bakes
match; all sixteen older surface
maps remain byte-exact. Colour is sRGB; height/roughness/occlusion/metal data is
linear. Imports retain BC7, mipmaps, repeat/trilinear, anisotropy eight and no CPU
copy. Original `Forgeborne/OpticGlass` adds a transparent optical coating to the
combiner. The menu now names the actual authored Cinder Exchange instead of the
old Relay Yard. Main changes only its saved view offset; version is 0.127.0.

## Verification and its limits

Evidence is in `docs/evidence/2026-11-02/carbine-0127/`. The manifest fingerprints
the current sources, assets, reports and whole native package, excluding itself.

- Actual imported Main: one Seed-42 GameManager, all linked adapters share its
  Simulation, sole FPS frame driver, retained v2 digest, zero shader diagnostics.
  Project setting remains **Both (2)**. Nine DLLs remain in Plugins; no Core source
  was copied into Assets. Core source/tests/DLLs and physical v1/v2 content are
  unchanged from 0.126. The preceding 5,629 passing source checks are historical
  evidence for those unchanged inputs, not a fresh 0.127 source-suite run.
- The production CPU skin probe passes six reload ages, trigger deformation,
  cross-hand isolation, pause, exact reset, forearm length, orbital hide/restore
  and unchanged canonical JSON/Simulation. Trigger deformation is .0208493378 m;
  maximum measured forearm-length error is 3.874302e-7 m.
- A synchronous camera render did not update GPU skinning in the same callback.
  CPU evidence is therefore separate from actual frame-boundary captures. Six
  final reload images crossed 513–554 normal player-loop frames before capture;
  actual skinned renderers deform with canonical state unchanged. Temporary QA
  background execution/view flags were restored. Side views explicitly use
  diagnostic framing; the FPS rest/native images show normal gameplay framing.
- Twelve actual compressed glove/sleeve GPU samples have maximum linear-channel error
  .009589225. The four imported texture objects report 11,363,264 runtime bytes in
  Editor; this is not whole-game/native memory. A 256-pixel optical GPU A/B gives
  clear mean 1.0, production coating .9725029 and high absorption .3647537.
- Six hundred warmed presentation updates took 5.9683 ms with zero allocated
  bytes in the scoped Editor probe. This excludes rendering, skinning and platform
  overhead; representative native frame-time/memory acceptance remains open.
- Windows package **20261102T163104Z** succeeded with **zero errors/warnings**,
  167,847,075 reported build bytes, Unity 6000.3.27f2 / Mono2x. Editor console has
  zero errors/warnings after the build. Launcher hash alone is not package identity.
  Five plugin assemblies are byte-exact originals; four framework replacements
  match the exact pinned Unity runtime assemblies and metadata.
- Actual native UI: correct entry caption, enter FPS, mouse shot 42/120 → 41/120,
  legacy R reload 42/119 with visible gesture, Tab orbital without view model,
  Tab return to FPS, Escape pause, then Load Slot restored the user's encounter
  while paused. Eight observed 1920×1080 JPEGs are preserved. Both slot and backup
  were byte-exact unchanged at **2026-11-02T16:34:55Z**. The current player was
  left paused for Rob; Editor Play is stopped. Later user changes take precedence.

The bounded current native log was 1,764 bytes with no observed project exception.
It retains the known D3D12 info-queue diagnostic `0x80004002`, classified in
surface-material-checkpoint.md. No logging suppression, installed-package patch,
OS graphics-tool installation or security change was made. This short run cannot
establish driver stability, sustained performance, heard mix or fun. Native
movement/contact evidence belongs to the unchanged 0.126 Core checkpoint; this
run does not independently repeat it.

## Reproduction

Use Blender 4.5.15 LTS with stopped/imported Unity:

```powershell
& 'C:\Users\Bulkl\.codex\tools\blender-4.5.15\blender-4.5.15-windows-x64\blender.exe' --background --python-exit-code 1 --python art/tools/build_carbine_v2.py
& 'C:\Users\Bulkl\.codex\tools\blender-4.5.15\blender-4.5.15-windows-x64\blender.exe' --background --python-exit-code 1 --python art/tools/bake_glove_surface.py
& 'C:\Users\Bulkl\.codex\tools\blender-4.5.15\blender-4.5.15-windows-x64\blender.exe' --background art/source/PulseCarbine.blend --python-exit-code 1 --python art/tools/audit_carbine.py
```

Blender/FBX container bytes can vary; geometry, bounds, bindings, material roles
and actual import consumers are the reproducibility contract. PNG bake bytes are
separately reproducible. Raw workspace text hashes may vary with checkout line
endings. Do not regenerate retained physical catalogs during this workflow.

Use the pinned Python 3.10 MCP environment and `tools/unity-mcp-client.py`, with
explicit connected instance `ForgeborneUnity@9fc4dca83b83a1f1`. After refresh has
finished, execute `unity-carbine-prepare.cs`; run stopped GPU glove/optic probes,
then Play, pause actual Main, and execute `unity-carbine-pose-smoke.cs`.
`tools/capture-carbine-poses.py --instance ForgeborneUnity@9fc4dca83b83a1f1`
records reload frames; add `--inspection` for the two diagnostic side views.
`unity-carbine-camera.cs` records normal FPS rest framing. The capture helper
restores presentation and runtime flags in `finally`. Stop Play before requesting
`ForgeborneWindowsBuild.Request`; inspect its exact terminal report before launch.
Native controls must be tested through the real player UI, preserving local saves.

## Remaining product work and audit

The [direct BioShock visual study](bioshock-visual-study.md) identifies connected
architectural volumes, construction depth, purposeful service fittings, local
wear and composed light as the next environment needs. The broad planes,
repeated props, sparse storytelling, character anatomy/motion and weapon finish
still fall short. More triangles or texture noise cannot pass that gate. Continue
the full equally weighted Minecraft / StarCraft / Diablo / PoE2 / Borderlands /
BioShock brief. Complete native gather → build → survive → save/cold recovery,
representative budgets, heard audio and Rob's art/feel/fun review remain open.

Section 0 for the **full requested game**: **1 FAIL; 2 PASS; 3 PASS** for owned
and affected checks with the upstream diagnostic classified; **4 FAIL** for
missing full end-to-end/human evidence; **5 PASS; 6 PASS; 7 FAIL** for unproven
representative budgets; **8 PASS; 9 FAIL** for incomplete full-game packaging;
**10 PASS; 11 PASS** with documented container variance; **12 PASS; 13 N/A**
(no delegation); **14 PASS; 15 PASS**. Applicable failures prohibit completion.

Self-audit: actual FPS, side, reload and native frames were inspected.
Inspection corrected floating assemblies/stale GPU captures; Rob's review exposed the poor arm proportions.
Source/import counts, real skin weights, GPU materials and reset/state isolation were checked.
Native input evidence and unchanged slots are separated from full gameplay acceptance.
The result remains below the director's AAA+ target; no visual or fun approval is inferred.

Frozen Constitution 6.18.0: `63d99cdc72a1c18b5aa91b9336d02fd30126407b`.
The single elapsed-time/callback evolution remains validated `922dcbb`,
**staged, not propagated**. This continuation does not start a second invocation.
