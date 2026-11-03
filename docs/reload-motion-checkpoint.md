# Reload motion checkpoint — 0.133.0

Rob's arm/hand correction remains active. This contribution addresses the fast,
straight support-hand return in 0.132; it does not accept the glove anatomy,
character art or full AAA+ presentation. All six game influences remain equal.

## Presentation change

The cosmetic gesture now lasts 2.1 seconds instead of 1.2. Its final return has
336 ms instead of 120 ms. Quintic easing brings velocity and acceleration to
zero at each hold. Original outward wrist arcs separate the magazine-to-slide
and slide-to-foregrip transitions from the straight interpolation. These are
authored paths, not a mesh collision/clearance guarantee. A double elapsed-time
accumulator reduces cadence-dependent pose error.

Normalized timing: magazine reach .02–.18; withdrawal .24–.34; reseat .38–.48;
charging reach .50–.68; pull .70–.76; release .79–.84; return .84–1. Magazine
contact is measured over .18–.50 and charging contact over .68–.84. The smoke
probe's sample windows/counts change with that intentional timing contract;
the 20-micrometre contact tolerance is retained.

Core ammo transfer remains immediate and firing still interrupts the cosmetic
reload. Original source art, FBX/prefab, rest rigs, twenty maps, materials,
shaders, physical catalogs, Core/tests, nine Plugins DLLs and Main are unchanged.
The one Seed-42 GameManager Simulation, sole FPS frame driver and Both legacy
input setting remain verified. No save migration is required.

## Measured and rendered evidence

Evidence is in `evidence/2026-11-02/reload-motion-0133/`. The folder follows the
continuing work session; current receipts are dated November 3 UTC.

| Actual camera-space palm reference, 240 Hz | 0.132 baseline | Selected 0.133 |
| --- | ---: | ---: |
| Presentation duration | 1.2 s | 2.1 s |
| Peak speed | 5.7503 m/s | 2.7284 m/s |
| Peak acceleration | 169.5174 m/s² | 57.6546 m/s² |

The same physical sampling interval is used, rather than comparing steps from
different-duration phase sweeps. Working art budgets are <=3.5 m/s and <=60 m/s²;
they are not universal human biomechanics limits or natural-motion acceptance.
The first 2.0-second candidate passed rig checks but exceeded the acceleration
budget at 63.6896 m/s². Its receipts and failed comparison remain in `first-motion/`.
The chosen duration was adjusted without loosening either budget.

The first post-edit attempt ran an old loaded assembly despite the new bundle
version. The contact check rejected its old paths. Its diagnostic is retained
as `stale-loaded-assembly-kinematics.json`; stopped Play, explicit AssetDatabase
refresh and reflection verified the new duration/double field before rerunning.

The selected actual rig sweep covers 241 phase samples, 77 magazine-contact
samples and 38 charging-contact samples. Maximum palm/hook errors are
2.3051e-6/1.8446e-6 m. Forearm-length error is <=1.073e-6 m; charging travel is
.0350005 m. Actual CPU skin at the same elapsed moments across 30/60/144 Hz
differs by <=2.082e-6 m. Pause, firing interruption and exact imported rest
reset pass. The existing production-shot oracle still measures 2.50138855 m
and preserves canonical hit/ammo/damage and world state.

Six hundred warm presentation updates took 6.267 ms with zero measured managed
allocation. This excludes rendering, skinning and platform overhead; it is not
a representative native performance budget.

The silent clips are real Unity FPS-camera renders: `baseline-clip/reload.mp4`
(73 frames) and `rendered-clip/reload.mp4` (127 frames), 1280×720 at encoded 60 fps.
Every pose crosses at least two normal player frames before capture. Terminal
receipts check the same Simulation, canonical state and restored camera/view/
background flags. Selected original PNG frames were inspected, including reach
and return. A deliberately cancelled zero-frame job also restores these flags;
its report honestly records failure. Scripted clip playback is separate from
native input and realtime performance. These clips make motion reviewable;
the remaining anatomy/material/actor craft is still apparent and unaccepted.

## Windows verification and preservation

Unity 6000.3.27f2 Windows package `20261103T050015Z` succeeded with zero build
errors/warnings: 168,032,102 reported bytes, 25.5968612 seconds, Mono2x/options
None. The stopped Editor console also reports zero errors/warnings. Complete
package inventory and plugin provenance are fingerprinted in the manifest:
194 files, 124 ScriptingAssemblies names, no MCP/Coplay DLL or name. One additional
Burst compiler listing lives under `Forgeborne_BurstDebugInformation_DoNotShip/`;
the complete QA inventory retains it explicitly rather than treating this folder
as a release archive. No prior package path is missing. Editor MCP
access remains working after the supported build filter.

Ten original native Windows screenshots show entry/FPS, fire 42/120→41/120,
legacy R 42/119, magazine and charging poses, return, Tab orbital/FPS, Escape
and an existing-slot load. The revised support-hand return was observed. R
snapshot operations completed 968.257 and 1614.771 ms after a clock started
before the input call; those wall clocks do not establish animation phase or
runtime performance. A first occluded snapshot showed the older player;
activating and re-observing the exact new returned window resolved it before
inputs. Only the verified new-window captures are exported.

Both slot names/bytes remain exact at 2026-11-03 05:04:03Z. QA made verified
private backups and did not write/restore slots or click Quit/autosave. The
exact new PID 35204, executable/log argument and creation time matched at the
receipt; its last observed UI was paused after loading. Earlier sessions were
not closed by QA. Later user changes take precedence. Editor Play is stopped.
The private live log snapshot is 1,970 bytes with zero recorded project-exception
pattern matches and one retained D3D12 info-queue diagnostic, classified in
surface-material-checkpoint.md. This is not shutdown or sustained stability proof.

## Reproduction, rollback and full-game audit

From paused FPS Main, use the pinned Unity MCP Python client and exact connected
instance to execute `tools/unity-reload-kinematics.cs`, `unity-hand-motion-smoke.cs`,
`unity-carbine-pose-smoke.cs`, `unity-weapon-framing-feedback.cs` and
`unity-authored-main-status.cs`. Compare saved actual receipts with
`tools/compare-reload-kinematics.py <baseline> <current> --output <new-json>`.
Use pinned MCP Python for `tools/capture-reload-clip.py --instance <instance>
--output-dir docs/evidence/<new-empty-folder>`. It polls the existing job and
requires a terminal report; timeout does not authorize a duplicate capture.
Do not edit Assets while a capture owns the view. Stop Play before packaging.

Rollback is this isolated contribution; no user-save rollback is needed.
Verified main updates remain authorized; release/licence/visibility and unrelated
publication remain outside scope. Historical 5,629 Core/source tests are retained
evidence; that unchanged suite was not rerun for this presentation-only change.

Full six-influence AAA+ production remains active. Better hand anatomy/tailoring,
actor/environment craft, the complete native gather/build/survive loop and
current-v2 cold recovery, representative budgets, heard mix and director visual/
experience/fun acceptance remain open. Passing checks do not prove fun.

Section 0: **1 FAIL; 2 PASS; 3 PASS** for affected checks with inherited diagnostics
classified; **4 FAIL** for missing full end-to-end/human evidence; **5 PASS;
6 PASS; 7 FAIL** for representative budgets; **8 PASS; 9 FAIL** for incomplete
full-game packaging; **10 PASS; 11 PASS** with measurement limits explicit;
**12 PASS; 13 N/A** (no delegation); **14 PASS; 15 PASS**. Applicable failures
prohibit a completion claim.

Self-audit: a stale loaded assembly was caught rather than accepted by its version.
Physical-time comparison rejected the first revised candidate without relaxing budgets.
Actual rig, skin, render and native observations cover different boundaries explicitly.
Original art and canonical gameplay remain preserved. Full art/fun acceptance is absent.

Frozen Constitution 6.18.0 remains 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
Skill evolution: elapsed-time/callback evidence change 922dcbb remains validated
and **staged, not propagated**, in the same continuing invocation.
