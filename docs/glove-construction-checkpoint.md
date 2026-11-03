# Glove construction — 0.132.0

Continue the full six-influence AAA+ game and Rob's arm/hand correction. Baseline
main is 58bf95283eff3ee60fbf971d55b5584fc8a35d78; work branch is
codex/glove-construction. The previous turn made verified, published 0.131
progress. This contribution does not accept the hand art or the full game.

Original glove surfaces now use narrower palms, rounded-square finger sections,
interdigital saddle volumes and shallow folds on the inside of bending joints.
The folds are authored rest geometry following the existing skin, not dynamic
cloth simulation. A woven dorsal panel and leather palm/fingers reuse the existing
SleeveFabric and GloveFabric materials/maps. No texture bytes, shader code or
material settings change; no downloaded geometry, anatomy template or game art
is imported. The panel cuts and fitted stitch dashes share one original outline.
An exposed-palm ray selects the seed for a connected cloth region, preventing
unwanted fabric islands on occluded fingers with the same projected coordinates.

Both seventeen-joint rest rigs, their object matrices and source muzzle/signed
axes/palm landmarks are exact against 0.131. Frame, magazine and charging-handle
oriented surface/material-role sets agree at the recorded 1e-7 m position quantum.
The corrected support elbow, grip paths, saved view offset/angles and 75-degree
FPS FOV remain intact. Glove geometry, bindings and role assignments are
intentionally regenerated; their identity against 0.131 is not claimed.

Source and actual import agree on **93,342 triangles / 47,073 source vertices**,
up 8,038 triangles and 4,029 vertices from 0.131. The complete view stays below
its 100,000-triangle limit; that limit does not establish native performance.
Left/right skins have 30,896/29,886 triangles and 15,546/15,027 source vertices,
split to 15,644/15,125 on actual import. Largest connected bodies contain
14,204/13,769 vertices. Every source component is closed/manifold with zero
degenerate faces. Both bodies contain both fabric roles and all seventeen joints.
Source/import weights are normalized, own-hand and at most four influences;
import maximum weight-sum error is 1.1920929e-7. All 738 imported cuff vertices
per hand remain unit-bound to their forearm.

The source audit independently finds one connected dorsal fabric region on each
hand: 422/160 left/right faces, areas .00217881/.000873192 square metres and
112/80 leather-boundary edges. Left/right source stitches contain 540/456 vertices.
Maximum stitch distance to the actual material boundary stays below .800 mm,
as does distance to the body; nearest-skin weight L1 error is zero. The new
`--require-dorsal-panels` flag checks connected construction and seam-to-boundary
fit separately from normalized skin. Those measurements do not prove natural
anatomy, dynamic cloth or final visible craft.

The first higher-resolution source attempt found a three-vertex disconnected
sliver near the left thumb pad before the panel cuts. Palm/thenar overlap was
revised without weakening the connected-body guard. The narrowed body then
exposed an old seam return outside the glove, so the shared cut/stitch outline
was refitted. Finally a cut quad with three collinear corners produced a
6.78775213e-13 square-metre triangle. Explicit beauty triangulation selects its
diagonal before export; the original degeneracy threshold remains enforced.
All three source failures stopped before asset export.

The first rendered candidate passed geometry/skin checks but showed unwanted
fabric patches on fingertips. Its source audit found six dorsal fabric regions
on the left. That candidate is retained in `first-look/`, including the matching
import and source-form reports. The seeded connected-panel recipe replaced that
assignment; the independent one-panel and seam-boundary checks now pass. The
final rest, magazine draw and charging images in `rendered/` were inspected.
Each actual skinned capture crosses separate callbacks and more than one normal
frame, preserves canonical state and restores temporary view/update/background
flags. Rest-landmark viewport values remain projection diagnostics, not visible
contact evidence. Candidate history is recorded separately.

A second isolated generation reproduces indexed positions, weights, rest rigs,
oriented topology/material roles, the geometry manifest and source fit/panel/
quantized weapon audit. Raw face order also matches in this pair. Blender/FBX
container hashes differ; reviewed source/FBX were unmodified. Pinned Blender
4.5.15 LTS and sorted cut input support these geometry/consumer contracts.

The affected actual rig sweep passes 241 phases, 104 magazine-contact and 34
charging-contact samples. Maximum palm/hook reference errors are .00271/.00323 mm;
forearm-length error stays below .000924 mm. Other-hand baked-skin difference is
below .00265 mm. Same-elapsed-time actual skin at 30/60/144 Hz differs by at most
.01362 mm. Pause, shot interruption, exact imported reset and orbital hide/return
pass. Maximum sampled palm step remains 28.815 mm at 240 intervals; numerical
reference agreement does not prove natural motion or visible surface contact.
Six hundred warmed presentation updates took 6.3696 ms and allocated zero bytes
in Editor, excluding rendering/skinning/platform overhead. No representative
native budget is claimed. The production shot/muzzle/hit probe still agrees with
an independent box-ray oracle at 2.50138855 m. Trigger-finger skin displacement
is 21.035 mm. Reload remains the unchanged 1.2-second cosmetic gesture, with
immediate Core ammo transfer and shot interruption; no timed gameplay reload.

Runtime C#, Main, shaders/materials, package pins, Core source/tests, nine Plugins
DLLs, physical v1/v2 catalogs and all twenty original surface PNGs are unchanged.
Main retains one GameManager Seed-42 Simulation, linked adapters and the sole
FPS frame driver. Both input remains 2. The 5,629 source checks are historical
evidence for unchanged Core inputs, not a fresh source-suite run here. The
regenerated prefab only gained trailing whitespace; prior semantic content was
checked before normalization. No Core C# was copied into Assets.

Windows QA package **20261103T041055Z** succeeded in 10.6731719 seconds, zero build
errors/warnings, 168,031,590 reported bytes. Actual post-build Editor MCP still
works; console has zero errors/warnings. The unchanged supported build filter
excludes the unused MCP helper. No MCP/Coplay DLL or name remains among 124
scripting assemblies. The manifest records all **193 package files** and fresh
installed/packaged identities/hashes for nine plugins, including four Unity
runtime facade replacements. Path inventory matches 0.131. This QA folder is
not a release archive.

Actual native UI entered FPS, fired 42/120 → 41/120, then used legacy R to reach
42/119. Magazine reach, charging hand with woven panel/leather fingers and return
to grip were observed. The first/second snapshot callback wall times were
280.419/1,011.704 ms, with the later screenshot scheduled within the same reload.
Their clock starts before the input API, so they do not measure authoritative
animation phase or performance. Tab orbital/FPS, Escape pause and existing-slot
load were observed. Loaded HUD: 20/119, stone 3, wood 6, squad visible, core 10 m
away. Ten original native JPEGs have verified 1920×1080 dimensions.

Both current save files remained byte-exact at **2026-11-03 04:21:51Z**. No QA
save write or backup restoration occurred. At that dated receipt the exact new
process/package/separate log argument and creation time were verified; its last
observed UI was paused after loading. Earlier native sessions were not closed;
Editor Play is stopped. Later user changes take precedence. Do not use Quit/
autosave or restore older private backups over later saves. The separate
1,764-byte live log snapshot has zero matches for its recorded project-exception
pattern and one retained D3D12 info-queue diagnostic classified in
surface-material-checkpoint.md. This is not shutdown or sustained stability
evidence. Full native loop/current-v2 cold recovery, heard mix, representative
budgets and director visual/experience/fun gates remain open.

## Reproduction and rollback

Use pinned Blender 4.5.15 LTS with `--background --python-exit-code 1 --python
art/tools/build_carbine_v2.py`. Load the resulting original source and run
`audit_carbine.py`, `audit_hand_identity.py` and `audit_glove_form.py -- --output
<new-path> --require-fitted-stitches --require-dorsal-panels`. The new panel flag
is opt-in so older sources can still be inspected without pretending they had
the new construction. Compare exact rest rigs/landmarks and recorded oriented
weapon roles/positions; container bytes and checkout line endings may vary.

With stopped/imported actual Main and the pinned Python MCP client connected to
ForgeborneUnity@9fc4dca83b83a1f1, set the intended version before running
`tools/unity-hand-motion-prepare.cs`. Enter paused Play and run
`unity-hand-motion-smoke.cs`, `unity-carbine-pose-smoke.cs`,
`unity-weapon-framing-feedback.cs` and `unity-authored-main-status.cs`.
Run `tools/capture-hand-motion.py` through the pinned MCP Python interpreter,
explicit instance and a new empty evidence folder, with `--phases 0 .44 .84`.
Stop Play before the supported Windows build, then exercise actual controls.

Rollback is this isolated contribution; no save migration is needed. Verified
main updates remain authorized. Release/licence/visibility and unrelated
publication remain outside scope. All six influences remain equally important.

## Full-game gate and self-audit

The full AAA+ goal stays active. Further hand anatomy/tailoring, continuous arm/
actor animation, environment craft, the full native gather/build/survive loop and
current-v2 cold recovery, representative budgets, heard mix and director
experience/fun acceptance remain incomplete. A passing check does not prove fun.

Section 0: **1 FAIL; 2 PASS; 3 PASS** for affected checks, with inherited
diagnostics classified; **4 FAIL** for missing full end-to-end/human evidence;
**5 PASS; 6 PASS; 7 FAIL** for representative budgets; **8 PASS; 9 FAIL** for
incomplete full-game packaging; **10 PASS; 11 PASS** with variance documented;
**12 PASS; 13 N/A** (no delegation); **14 PASS; 15 PASS**. Applicable failures
prohibit a completion claim.

Self-audit: source connectivity/triangulation guards stopped malformed candidates before export.
Rendered fingertip patches failed the visual construction gate despite valid geometry/skin.
The panel recipe was repaired and checked independently against actual material boundaries.
Rest rigs and gameplay remain preserved while new glove surfaces/bindings are explicit.
This is an unfinished-game contribution with no AAA+ or director fun approval.

Frozen Constitution 6.18.0 remains 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
The single elapsed-time/callback evolution 922dcbb is validated and **staged,
not propagated**. This continuation starts no new invocation or evolution.
