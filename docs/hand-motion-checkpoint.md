# Support-hand reload and cuff binding — 0.129.0

This continues Rob's arm/hand correction and the full six-influence AAA+ game.
Baseline main is d8ca84081d4ba92122c5447a2e1df74ef8d73f4c. Work branch:
codex/support-hand-reload-contact. The earlier native player was present during
this work; its saves and session were not closed or restored by these probes.
The previous goal turn made verified progress by publishing 0.128, not merely
restating status. The current full-game goal remains active and unaccepted.

The old gesture moved the charging slide while the support hand was returning
to the foregrip. The new original 1.2-second sequence releases the foregrip,
grasps the capacitor magazine, withdraws/reseats it, reaches/hooks/pulls the
charging slide, releases it and returns to grip. The hand follows the actual
moving assembly references. Fingers curl independently of the palm solve;
thumb opposition and an angled charging grasp replace the initial open pose.
The solver uses the already-curled actual distal index joint, so curling cannot
silently detach that reference from the slide.

This remains cosmetic presentation: Core still transfers ammo immediately and
shooting can interrupt the gesture. A timed gameplay reload, complete continuous
animation, final anatomy/material craft and director fun approval are not claimed.
The prior offset, holding angle, FOV and all Core aim/damage rules remain intact.

Rendered inspection also exposed cuff welts following the palm instead of the
sleeve. The source asset now binds exactly 640 Rubber-only cuff vertices per
hand to that hand's forearm, at weight 1. All indexed hand positions, polygon
arrays, material roles, finger/palm/sleeve geometry and rest rigs are exact
against 0.128; all other skin weights are exact. The regular original generator
has the same correction. The targeted Blender migration preserves existing
geometry rather than regenerating it. A second application in isolated private
output changed zero bindings and retained exact indexed source attributes.

The actual Unity import contains 738 cuff vertices per hand because import
splits some vertices. Every Rubber-role triangle vertex reaches the correct
forearm with weight 1. Source/import still have 88,224 triangles, 44,368 source
vertices, two actual skins and 34 unique joints; no degenerate source faces.
This is an explicit binding change, not a claim that bindings stayed unchanged.

Evidence is in `docs/evidence/2026-11-02/hand-motion-0129/`. The first open-hand
candidate and its technically passing CPU report remain under
`rejected-open-charge/`. Two intermediate grasp studies are in `pose-study/`.
The final `rendered/` samples cross separate actual player/render callbacks and
use the real skinned renderer at 1920×1080; they restore temporary view/update/
background flags and leave canonical state untouched. Palm projections in their
metadata are static rest-landmark diagnostics, not proof of visible contact.
Actual images were inspected separately, including the cuff correction.
`hand-motion-0129-manifest.json` records current source/art/tools/docs/evidence
and the complete final Windows package, with itself excluded.

The actual rig sweep covers 241 phases, 104 magazine-contact samples and 34
charging-contact samples. Maximum reference errors are 0.00293 mm for the palm
and 0.00283 mm for the index hook; forearm-length error is 0.00105 mm. The other
hand's baked-skin difference stays below 0.00265 mm. These numerical references
do not prove visible surface contact or natural motion. Same-elapsed-time actual
skin comparisons at 30/60/144 Hz differ by at most 0.01446 mm. Pause, shot
interruption, exact imported bone/assembly reset and orbital hide/return pass.
Six hundred warmed presentation updates, with a full-reload interval of 168
updates at 120 Hz, took 6.4403 ms and allocated zero bytes in Editor. This omits
rendering/skinning/platform costs and does not establish native budgets.

The production shot/muzzle/hit probe still agrees with an independent box-ray
oracle at 2.50138855 m. Main retains one Seed-42 GameManager/Simulation and the
sole FPS frame driver. Both input remains 2. Core source/tests, all nine Plugins
DLLs, package pins, retained v1/v2 physical catalogs and all twenty surface PNGs
are unchanged. The 5,629 source checks are historical evidence for unchanged
Core inputs, not a new source-suite run in this checkpoint.

Windows package **20261102T214317Z** succeeded in 23.1812146 seconds, with zero
build/console errors or warnings and 167,829,942 reported bytes. The supported
build filter excludes the unused MCP helper; no MCP DLL or scripting-assembly
entry remains among the final 124 assembly names. Editor MCP still works after
the build. The manifest records all 194 package files, not just the launcher.

Actual native UI entered FPS, fired 42/120 → 41/120, reloaded through legacy R to
42/119, showed the magazine reach and later charging-hand/cuff pose, returned to
the foregrip, switched Tab to orbital/back, paused with Escape and loaded the
existing slot. Ten original 1920×1080 JPEG observations are retained. Snapshot
callback durations are recorded separately; their clock begins before the input
API and does not establish gameplay phase timing or performance. The loaded HUD
shows 20/119, stone 3, wood 6, a squad and the core 10 m away.

Both current user save files remained byte-exact at **21:50:16Z**. The exact
new player process was present and its last observed UI was paused. Editor Play
is stopped. These are dated observations; later user changes take precedence.
Neither player was quit and no private backup was restored. The new package uses
Unity's documented [`-logFile` player argument](https://docs.unity.com/en-us/engine/6000.3/manual/unity-editor/command-line-arguments/player)
to keep its QA log separate from the earlier live session. Its 1,764-byte live
snapshot has zero observed project-exception patterns and retains the inherited
D3D12 info-queue diagnostic classified in surface-material-checkpoint.md. This
does not establish a full native loop, cold recovery, stability, heard mix or
representative performance. No visual or fun approval was received.

## Reproduction and rollback

Use pinned Blender 4.5.15 LTS with `--python-exit-code 1`. The regular original
`art/tools/build_carbine_v2.py` generator now binds cuffs correctly. To migrate an
existing original source without regenerating its geometry, load
`art/source/PulseCarbine.blend` and run `art/tools/rebind_carbine_cuffs.py`.
The targeted operation is idempotent. Use `audit_hand_identity.py` and
`audit_carbine.py -- --output <new-path>` for source evidence. Container bytes
and checkout line endings may vary. Full regeneration can reorder trim faces;
compare indexed oriented polygon/material multisets without reversing winding.
The geometry/binding/import contracts and original PNG bytes are the relevant
reproducibility boundaries.

With actual stopped/imported Main and the pinned Python MCP client connected to
ForgeborneUnity@9fc4dca83b83a1f1, run `unity-hand-motion-prepare.cs`. Enter Play
at the paused menu; run `unity-hand-motion-smoke.cs`, `unity-carbine-pose-smoke.cs`,
`unity-weapon-framing-feedback.cs` and `unity-authored-main-status.cs`.
Run `capture-hand-motion.py --instance <instance> --output-dir
docs/evidence/<new-empty-folder>`; `--phases` can select study moments. The helper
uses existing production framing probes in separate callbacks and preserves
earlier evidence by requiring an empty output folder. Stop Play before packaging.

Rollback is this isolated contribution; no save migration is needed. Never
restore old private backups over later user saves. Rob's verified main-update
authority persists; no release/licence/visibility or unrelated publication.

## Full-game gate and self-audit

The full six influences remain equally important. Actor/environment craft,
believable anatomy and continuous motion, the full native gather/build/survive
loop, current-v2 cold recovery, representative budgets, heard mix and director
experience/fun acceptance are incomplete. AAA+ remains unmet.

Section 0 for the full requested game: **1 FAIL; 2 PASS; 3 PASS** for owned and
affected checks, with inherited diagnostics classified; **4 FAIL** for missing
full end-to-end/human evidence; **5 PASS; 6 PASS; 7 FAIL** for representative
budgets; **8 PASS; 9 FAIL** for incomplete full-game packaging; **10 PASS;
11 PASS** with variance documented; **12 PASS; 13 N/A** (no delegation);
**14 PASS; 15 PASS**. Applicable failures prohibit a completion claim.

Self-audit: technically passing contact checks did not pass the open-pose visual gate.
The charging grasp was revised only after inspecting actual skinned images.
Cuff binding was repaired at its original source and checked on the real import.
Canonical ownership, affected actions, pause/reset and rendering remain separate evidence scopes.
This is a contribution to an unfinished game, with no AAA+ or director fun approval.

Frozen Constitution 6.18.0 remains 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
The single elapsed-time/callback evolution 922dcbb is validated and **staged,
not propagated**. This continuation starts no new invocation or evolution.
