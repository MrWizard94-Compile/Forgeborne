# Forgeborne production status

The full director brief is mapped in [design-pillars.md](design-pillars.md),
including the explicit Path of Exile 2 reference in the latest objective.
The 2026-11-01 objective states that all six influences are equally important.
Modded Minecraft supplies the gather/build/survive/combat concept; Rob has
explicitly rejected the blocky aesthetic. Believable original 3D world art is
required. This correction supersedes historical voxel-style art requirements.

Director: Rob. Integrator: Codex. Current phase: **pre-production / vertical slice**.
Director gate 2026-11-01: current presentation **REJECTED**; finished-game target
**AAA+**. Feature expansion is paused in favor of cohesive original art,
atmosphere, animation/audio and the actual playable encounter. See DECISIONS.md
and the current art-direction gate. No visual, experience or fun approval.
PC-native 3D FPS + orbital command, single-player first, original game code/art,
Unity 6000.3.27f2 + URP. One GameManager-owned Simulation. Legacy input requires
Both. Core is distributed only as DLLs in Assets/Plugins. Rob merged PR #1 at
9e6ef62 and then directed "just update main": verified checkpoints may now be
pushed to main directly. Release, licence, visibility and unrelated repository
changes still require his direction. This is publication authority, not visual
or fun acceptance.

Current 0.133 lengthens the cosmetic reload to 2.1 seconds, with quintic easing,
outward transition arcs and robust elapsed-time accumulation. Same-time-interval
actual palm speed falls from 5.7503 to 2.7284 m/s and acceleration from 169.5174
to 57.6546 m/s². Actual contact/skin/cadence/reset/shot checks, real Unity
clips and affected native controls pass their scoped checks. Art, Core/DLLs,
catalogs/Main and gameplay rules remain unchanged. Windows package 20261103T050015Z
and console have zero errors/warnings; saves exact at 2026-11-03 05:04:03Z. Exact
process present at receipt, last observed paused after loading; Editor stopped.
Earlier sessions not closed by QA. Anatomy and final motion quality remain
unaccepted. See [current reload-motion evidence](reload-motion-checkpoint.md).

The preceding 0.132 adds original glove construction: narrower palms, shaped fingers,
webbing/flexion detail and one sewn woven dorsal panel per leather glove. An
initial rendered fabric-island candidate was rejected and repaired. Rest rigs,
weapon surfaces and maps are preserved; glove geometry/bindings/roles change.
Source/import/regeneration/pose/shot/render checks and affected native controls
pass their scoped checks. Windows package 20261103T041055Z and console have zero
errors/warnings; saves exact at 2026-11-03 04:21:51Z. Exact new process present,
last observed paused after loading, Editor stopped. Earlier sessions not closed.
Further anatomy/tailoring/motion and full-game gates remain open. See
[current glove-construction evidence](glove-construction-checkpoint.md).

The preceding 0.131 corrects support-arm camera fit and removes detached-looking knuckle
trim. Actual elbow projection moves left while source wrist/grip and forearm
length are preserved. Rest-matrix float variance is recorded. Source/import,
isolated regeneration, pose/shot/render and affected native controls pass their
scoped checks. Windows package 20261103T031237Z and console have zero errors/
warnings; saves exact at 2026-11-03 03:23:11Z. Exact new player present and last
observed paused after loading; earlier sessions not closed, Editor stopped.
Further anatomy/tailoring/motion and full-game gates remain open. See
[preceding arm-fit evidence](arm-fit-checkpoint.md).

The preceding 0.130 replaces swollen palms and uniform fingers with original tapered
glove forms, shaped phalanges, blended thumb pads and fitted skin-weighted stitches.
Rest rigs/landmarks and quantized weapon surfaces remain preserved; new glove
surfaces/bindings are intentional. Actual import/pose/shot/render checks pass.
Native inspection rejected a thumb candidate, corrected before the final build.
Windows package 20261102T225546Z and console have zero errors/warnings; affected
native controls/reload/slot load observed. Saves exact at 23:04:02Z, exact process
present and last observed paused; Editor stopped. Earlier sessions were not closed.
Further tailoring/animation and full-game gates remain open. See
[preceding glove-form evidence](glove-form-checkpoint.md).

The preceding 0.129 connects the support-hand reload to the magazine and charging slide,
with finger curl/thumb opposition and a 1.2-second cosmetic gesture. Rendered
inspection rejected an open pose and corrected sleeve cuffs bound to the palm.
Actual source/import binding, rig/contact/cadence/reset/shot checks pass. Windows
package 20261102T214317Z and console pass with zero errors/warnings; native reload
poses, firing, orbital/FPS, pause and existing-slot load observed. Saves exact at
21:50:16Z; new player present and last observed paused, earlier native session
not closed, Editor stopped. No final art, timed gameplay reload or fun approval.
See [preceding hand-motion evidence](hand-motion-checkpoint.md).

The preceding 0.128 refines the original receiver proportions/assemblies and first-person
holding angle, preserving the corrected hand surfaces, bindings and rest rigs.
Source/import, actual skin/reset/state and production shot/muzzle/hit checks pass.
Windows package 20261102T210226Z succeeded with zero build/console errors/warnings;
native firing/reload, orbital/FPS, pause and slot load observed. Slots exact at
21:13:44Z, same process present, last observed UI paused. These are dated checks;
later user changes take precedence. A supported build filter excludes unused
MCP helper assemblies from the player; Editor MCP access remains verified.
Earlier unfiltered native evidence is retained separately. Revised art remains unaccepted. See
[current first-person craft evidence](first-person-framing-checkpoint.md).

The preceding 0.127 adds original continuous anatomical gloves, a 34-joint rig, refined
carbine assembly, transparent optic and reload presentation. Actual imported
skins, CPU poses/reset/state isolation, frame-boundary GPU captures/materials
and native firing/reload/orbital/pause/existing-slot load pass their scoped checks.
That Windows QA build is 20261102T163104Z, zero build/console errors/warnings;
user slots unchanged at 16:34:55Z, player left paused. See
[current carbine evidence and full audit](carbine-art-checkpoint.md).

The preceding 0.126 checkpoint adds dense organic formations, 3D fractures, roof/stays,
bowed construction, retained v1/v2 worlds and an orbital cutaway with matching
command picking. Current native QA build succeeded 0 warnings/errors; v1 load,
v2 restart, Tab/FPS/cutaway, firing/reload and pause observed. Rob calls the
emerging direction real progress and still requires realistic, organic meshes.
Player contact locks are corrected and verified in source, actual Main and one
native stuck-pose/W/import/export case. Full native loop/current-v2 cold recovery, representative performance,
heard audio and fun remain open; AAA+ is not accepted.
See [retained world/contact evidence and acceptance audit](foundry-v2-checkpoint.md).

The director's studio-production reference was read in full on 2026-10-31.
Adopt its discipline ownership, design contracts, dependency gates, short evidence
handoffs and engine/runtime verification. Treat its claimed timelines, guaranteed
hallucination elimination and coordination complexity as unverified assertions.
Do not equate an agent count, a schema or a green test suite with studio quality.

| Phase | Current gate | Owner/evidence |
|---|---|---|
| Concept / feasibility | Direction locked; complete feasibility/performance case still open | Rob directs; README, MANIFEST, DECISIONS and architecture targets |
| Pre-production / vertical slice | ACTIVE; final-quality gather → build → survive encounter not accepted | Codex integrates design, gameplay, art, audio and QA; Rob judges quality/fun |
| Full production | UNSTARTED; requires accepted vertical slice | No asset-volume expansion before the loop works |
| Validation / platform readiness | UNSTARTED as a product gate | PC build, regression, save recovery, hardware/profile matrix; console certification is outside current PC scope |
| Launch | UNSTARTED; director controls distribution/release | No storefront deployment or public marketing action implied |
| Post-launch | UNSTARTED | Maintenance planning follows an accepted release scope |

## Dependency order for the first reviewable vertical slice

1. **Integrated foundation — partial checkpoint.** Saved Main, shared ownership,
   rendered URP, input/HUD and MCP connection observed; see
   [the exact evidence](unity-main-integration.md). This is not a gameplay milestone.
2. **Real-time gameplay contract — verified checkpoint, feel review open.**
   Fixed movement/world clocks passed equal-time core tests and live Main
   observation; see [runtime-pacing-checkpoint.md](runtime-pacing-checkpoint.md).
   The candidate two-tick/second pace is still subject to encounter playtesting.
3. **Readable encounter.** Visible enemies and objective from the same canonical
   world, pointer capture/release, legible gathering/building targets and feedback,
   clear player death/restart, continuous real-time pressure and an attainable win.
   [Construction safety](construction-safety-checkpoint.md) now rejects occupied
   placement before spending, with source and production adapter evidence.
4. **A cohesive original place.** Resource/material distinction, purposeful cover
   and sightlines, industrial dusk palette, weapon/impact feedback, lighting and
   original audio. Check all assets at FPS and orbital distances; do not add
   decorative systems to hide missing interactions.
   Resolved action feedback and eleven original synthesis candidates now have
   [scoped live component evidence](action-feedback-checkpoint.md); heard mix and
   final presentation remain open.
5. **Persistent player flow.** Save/load/export/import through the actual UI,
   safe malformed-save handling, scene restart, retained inventory/builds/enemies,
   correct adapter identity after load and a recoverable failure path.
6. **Orbital integration.** Actual 3D overhead command view plus return to FPS,
   coherent selection/rally/deploy/cancel feedback and existing doctrine rules.
7. **Runtime hardening and review build.** Native Windows build, repeated sessions,
   intended device/input paths, 1440p frame-time and memory measurements on the
   GTX 1660 Ti, then director playtest. A passing technical gate cannot pass fun.

Each checkpoint needs a coherent commit, changed-path manifest, runtime evidence,
known gaps and rollback. Work from the current branch; preserve unrelated local
files and do not reset shared history. The saved integration scene/commit is the
rollback point before changing runtime cadence or encounter behavior.

## Completion boundary

The studio-quality goal stays active until the intended experience is reviewable
and the required technical and director gates are met. Do not relabel this tracker,
the inherited 26 source-only slices or the initial Main scene as a completed game.
