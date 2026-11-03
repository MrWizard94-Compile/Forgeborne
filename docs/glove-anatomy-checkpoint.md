# Glove anatomy checkpoint — 0.134.0

This is progress toward Rob's arm/hand correction and the full six-influence
AAA+ game. Current anatomy, character art and fun remain unaccepted. The
contribution is on `codex/glove-thumb-phalanges`, based on
70410c044614c6d5023833a00b04a4a1a9c94239. The affected source, actual Unity,
Windows build and native controls pass their scoped checks. Rob explicitly
authorized resuming QA after the physical Escape stop; the interrupted partial
observations remain separate from the completed native evidence.

## Shape and pose correction

Rendered inspection exposed a structural problem: the support palm was built
along the knuckle span, producing a long curved silhouette during reload. The
new palm is broad and flatter, with its wrist beneath the knuckles. Its wrist,
palm, elbow and thumb rest layout change intentionally; the original forearm
length and grip landmark remain protected. The thumb is refitted forward along
the foregrip instead of standing beside the barrel as a thin loop.

Both original gloves now use finger sections aligned to their bend planes,
flatter leather backs, rounded contact pads and more distinct shaft/tip taper.
Tapered thenar sweeps replace the rounded thumb-base lumps. The woven support
panel and stitches are refitted. During charging, the index retains its existing
hook; the other three fingers use 40/40/25-degree curls. The thumb adds a
-65-degree fold after opposition. These are original authored forms and poses,
not imported anatomy, animation or game assets.

The 2.1-second cosmetic sequence, immediate Core ammo transfer, firing
interruption, quintic timing, transition arcs, saved view offset (.19, -.10, .55),
yaw -12 and 75-degree FOV remain. Aiming/damage and the physical world are unchanged.

## Panel skinning repair

Pose comparison exposed another defect: otherwise normalized weights allowed
thumb rotation to pull panel corners into spikes. The sewn back now follows the
palm. A full hold extends 4 mm beyond the panel and blends through surrounding
leather to the existing binding at 12 mm. This is rigid panel ownership in the
current rig, not dynamic cloth simulation.

`audit_glove_form.py --require-palm-panel-binding` checks the actual source panel
vertices/material boundary. `tools/unity-glove-panel-smoke.cs` independently
selects imported cloth above the wrist in world metres, checks its actual bone
weights, and compares CPU-baked vertices in palm space over six reload moments.

| Actual imported consumer | Before repair | Selected repair |
| --- | ---: | ---: |
| Maximum non-palm weight, left panel | .9958594 | 0 |
| Maximum left panel movement relative to palm | 16.75498 mm | .00289142 mm |
| Maximum right panel movement relative to palm | — | .00251128 mm |

The selected check covers 315 left and 123 right panel vertices. The left thumb
still articulates through 60.35536 degrees of local rotation. The original
20-micrometre panel-stability threshold remains; the negative and positive
consumer receipts are retained. Source positions, topology, material roles and
rest rig are exact between the thumb-refit and binding-repair sources; only
bindings change in that final source step.

## Source and actual Unity evidence

Evidence lives in `evidence/2026-11-03/glove-anatomy-0134/`. The selected source and
import contain **92,520 triangles / 46,654 source vertices**, below the unchanged
100,000-triangle view budget. Both primary glove bodies are closed with no
nonmanifold edges; all 17 joints per hand remain active, with at most four
normalized influences. Imported maximum weight-sum error is 1.1920929e-7. Both
738-vertex split cuffs remain bound to their own forearm.

The source changes LeftForearm, LeftPalm and both three-joint thumb chains.
The twelve other left joint heads/tails/parents remain exact; their descendant
rest-matrix elements vary by at most 8.34465027e-7 after the parent correction.
The fourteen right non-thumb joints remain exact. Source left forearm length is
.2893267909 m versus .2893267799 m before. Rig-object matrices and named grip/
muzzle/axis landmarks remain exact. Frame/magazine/slide oriented surfaces and
roles remain equivalent at the recorded 1e-7 m quantum. All twenty original
maps, material/shader settings, physical catalogs, Core/tests, nine Plugins DLLs,
package pins and Main remain unchanged.

A second isolated pinned-Blender 4.5.15 generation reproduces indexed positions,
weights, rest data and oriented material topology, including raw face order in
that pair. Blender/FBX container bytes may differ. The reviewed current files
remain unchanged by the reproduction. Hashes are recorded separately.

The actual paused Main checks pass: 241 phase samples, 77 magazine contacts and
38 charging contacts; maximum palm/hook errors 4.011e-6/2.501e-6 m; 35.0005 mm
slide travel; <=2.177e-6 m skin difference at equal elapsed moments across
30/60/144 Hz; pause, shot interruption and exact imported rest reset. The actual
production-shot oracle remains 2.50138855 m and preserves canonical hit/ammo/
damage behavior. One GameManager Seed-42 Simulation and sole FPS frame driver
remain verified; legacy input stays Both and Core remains DLL-only.

At fixed 240 Hz sampling, peak camera-space palm-reference speed is 2.731633 m/s
and acceleration 56.045036 m/s², within the existing working 3.5/60 animation
budgets. The comparison retains the original 0.132 1.2-second reference; this
checkpoint preserves 0.133's 2.1-second timing. These are working art limits,
not universal biomechanics or natural-motion acceptance.

Six hundred warm presentation updates took 6.8779 ms with zero measured managed
allocation, excluding rendering, skinning and platform overhead. This is not
representative native performance evidence.

The selected silent `rendered-clip/reload.mp4` contains 127 real Unity camera
renders at encoded 60 fps, 1280×720. Each pose crosses normal player frames before
capture. Terminal receipts verify the same Simulation, canonical state and
restored camera/view/background flags. Rest and charging frames were inspected.
Scripted playback does not prove native input, realtime performance, final art
or fun. Detailed hand/cloth/actor craft remains open.

## Rejected candidates and native status

The first thumb/finger candidate passed source/import checks but failed rendered
palm anatomy review (`rejected-first/`). A panel cut initially produced an
8.6002e-13 m² triangle on a far curled finger; cuts are now limited to the outer
palm, retaining the 1e-12 m² guard. New panel outlines also failed projection/
boundary-fit checks before being refitted, without changing the audit tolerance.

Native package 20261103T054528Z exposed the upright thumb and was rejected.
Its exact owned QA PID 57992 was stopped without Quit/autosave, with slot hashes
unchanged before and after; earlier sessions were untouched. That package and
its screenshot/stop receipt remain separate in `rejected-native-thumb/`.
`thumb-refit/` and `thumb-poses/` retain the geometry/pose comparison that exposed
the panel-binding defect. An early diagnostic probe compile typo was corrected;
it is distinct from the passing project build/console checks.

Selected Windows package **20261103T124500Z** succeeds on Unity 6000.3.27f2:
zero build errors/warnings, 167,995,318 reported bytes, 41.0242032 seconds,
Mono2x/options None. The stopped Editor console reports zero errors/warnings.
Its 194-file QA inventory includes a Burst compiler listing under DoNotShip;
124 ScriptingAssemblies names contain no MCP/Coplay entry, and no such managed
DLL is shipped. Nine installed/packaged plugin identities and the four expected
Unity facade replacements are verified. Editor MCP still works after build.
This local QA package is not a release archive.

The selected native player entered FPS and firing changed 42/120 to 41/120.
The first reload observation was interrupted by the physical Escape stop and
remains recorded as unknown in `native-partial/`. After Rob explicitly directed
"Resume QA player testing", QA resumed only this exact player. R then restored
42/119; the magazine reach, charging grasp, stable panel and return to the
foregrip were observed. Tab hid the view for perspective orbital command and
restored FPS; Escape paused. The existing slot loaded while remaining paused,
showing 20/119 ammo, stone 3, wood 6, a squad and core distance 10 m. Ten original
1920×1080 native JPEGs and the separate reload observation receipt are retained.

Both current save names and hashes matched their fresh before-launch copies at
2026-11-03 13:22:04Z. No Save, Quit/autosave or older-save restore was performed.
Exact owned PID 74560, package path, separate log argument and creation time
matched at receipt; the player was last observed paused after loading. Earlier
sessions were not closed by this final QA. The bounded 1,764-byte log snapshot
has zero matches for the recorded project-exception pattern and one inherited
D3D12 info-queue diagnostic, retained/classified as in the surface checkpoint.
This is dated affected-controls evidence, not sustained stability or fun approval.

## Reproduction and full-game gate

With Play stopped, use pinned Blender 4.5.15 and `--python-exit-code 1` with
`art/tools/build_carbine_v2.py`. Audit the resulting original source with
`audit_carbine.py` and `audit_glove_form.py -- --output <new-path>
--require-fitted-stitches --require-dorsal-panels --require-palm-panel-binding`.
Use an isolated output directory when checking reproducibility.

Prepare/import the original carbine through actual Unity MCP, then enter paused
FPS Main. Run `unity-glove-panel-smoke.cs`, `unity-hand-motion-smoke.cs`,
`unity-carbine-pose-smoke.cs`, `unity-reload-kinematics.cs`,
`unity-weapon-framing-feedback.cs` and `unity-authored-main-status.cs`.
Use pinned MCP Python with `capture-reload-clip.py`, the exact connected instance
and a new empty evidence folder. Poll the existing capture handle; do not edit
Assets while it owns the view. Stop Play before the supported Windows build.

Rollback is the isolated contribution; no save migration is required. Verified
main updates remain authorized; the affected native verification is complete.
Release/licence/visibility and unrelated publication remain outside scope.
Historical 5,629 Core/source tests remain retained evidence; that unchanged suite
was not rerun for this hand/skin presentation change.

The full six-influence AAA+ goal remains active. Character/environment craft,
hand anatomy/materials and continuous actor animation, the complete native
gather/build/survive loop and current-v2 cold recovery, representative budgets,
heard mix and director experience/fun acceptance remain open. Passing checks do
not prove fun.

Section 0: **1 FAIL; 2 PASS; 3 PASS** for scoped checks with inherited diagnostics
classified; **4 FAIL** for full end-to-end/human evidence; affected native checks pass;
**5 PASS; 6 PASS; 7 FAIL** for representative budgets; **8 PASS; 9 FAIL** for full
game packaging; **10 PASS; 11 PASS** with matrix variance explicit; **12 PASS;
13 N/A** (no delegation); **14 PASS; 15 PASS**. Applicable failures prohibit a
completion claim.

Self-audit: source validity did not accept the first rendered anatomy candidate.
Native inspection rejected the upright thumb despite a passing build.
The panel probe rejected normalized but semantically wrong bindings.
Its before/after receipts test the actual imported deformation.
The Escape interruption was respected; explicit user authorization preceded
resumed native input. The partial and completed receipts remain distinguishable.

Frozen Constitution 6.18.0: 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
Skill evolution: elapsed-time/callback evidence change 922dcbb remains validated
and **staged, not propagated**, in this continuing invocation.
