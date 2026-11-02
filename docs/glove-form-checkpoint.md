# Organic glove form — 0.130.0

This continues Rob's arm/hand correction and the full six-influence AAA+ game.
Baseline main is 633bc57abab5ff344b3cb7594a3951337acc762c. Work branch:
codex/organic-glove-form. The previous turn published verified 0.129 progress;
neither that checkpoint nor this contribution accepts the game's art or fun.

The original glove generator now uses a tapered wrist/metacarpal fan instead of
a swollen ellipsoid palm. Sleeves end at the wrist; smaller thumb/heel volumes,
shaped phalanges and rounded, tapered fingertips replace uniform finger tubes.
The thumb surface is shorter while its original rest bones remain intact.
Original dorsal stitch dashes are projected onto the actual glove and inherit
the nearest skin vertex's normalized weights. Cuff bands retain their corrected
forearm binding. No downloaded geometry, anatomy templates or game art is used.

Both glove surfaces and their skin bindings are intentionally regenerated.
The seventeen-joint rest rig per hand, rig object matrices, muzzle/signed-axis/
palm landmarks compare exactly with 0.129. Frame, magazine and charging-handle
oriented polygon/material-role multisets agree at the recorded 1e-7 m position
quantum. That scoped comparison does not claim byte-identical FBX containers or
unchanged glove geometry/bindings. All twenty original surface PNGs are exact.

Source and actual Unity import agree on **89,528 triangles**, up 1,304 from
0.129, with **45,172 source vertices**, below the 100,000 view-model limit.
The right/left skins have 28,364/28,604 triangles and 14,264/14,408 source vertices;
actual import splits these to 14,590/14,734 vertices. All source components have
zero nonmanifold edges and zero degenerate faces. Each largest connected glove/
sleeve body contains 12,002 vertices, both fabric roles and all seventeen joints.
Every source/import skin vertex has normalized own-hand weights, at most four
influences. Import maximum weight-sum error is 1.1920929e-7. All 738 imported
split cuff vertices per hand bind to their forearm at unit weight.

The left/right source seams contain 540/396 vertices. Maximum nearest-body gaps
are 0.826/0.800 mm, and nearest-skin weight L1 error is zero. These are source
fit/binding measurements, not proof of natural glove folds or animated contact.
The read-only audit is art/tools/audit_glove_form.py. Its initial arbitrary
stitch-count threshold was replaced by the relevant nonempty/all-vertices fit
contract; the connected-body contract checks fabric roles and all joint coverage.
The final report was rerun, without changing production geometry for those audit
corrections.

A second generation in isolated private output reproduced the reviewed indexed
glove positions, weights, rest rigs and oriented topology/material-role sets,
the geometry manifest and source fit/quantized weapon audit exactly. Raw trim
face-list order and Blender/FBX container hashes differed. The reviewed source
and exported FBX were not modified. See `source-reproduction.json`; this does
not claim a second native run or final anatomy acceptance.

Evidence is in `docs/evidence/2026-11-02/glove-form-0130/`. `first-look/` retains
the first wrist-step candidate; `wrist-study/` retains the next wrist blend.
`pre-thenar/` contains the passing technical reports, nine rendered frames and
native build **20261102T223352Z** for the subsequently rejected isolated right
thumb-pad appearance. That is not the final package. The pad was moved/shrunk
into the palm, then the source/import/pose/render/native checks were repeated.
`thumb-study/` is the corrected first rest inspection. The final nine real
frame-boundary captures are in `rendered/`, with separate callbacks crossing
more than one normal frame, actual skinned renderers and unchanged canonical
state. Temporary update/view/background flags are restored. Rest-landmark
viewport metadata is diagnostic; visible surface contact requires image review.

The final actual rig sweep covers 241 phases, 104 magazine-contact samples and
34 charging-contact samples. Palm/hook reference errors stay below 0.00293/
0.00283 mm; forearm-length error stays below 0.00105 mm. Other-hand baked-skin
difference is below 0.00265 mm. Same-elapsed-time actual skin at 30/60/144 Hz
differs by at most 0.01449 mm. Pause, shot interruption, exact rest reset and
orbital hide/return pass. The maximum sampled palm step is 28.815 mm at 240
intervals; numerical contact/cadence checks do not prove natural motion.
Six hundred warmed presentation updates took 6.2753 ms and allocated zero bytes
in Editor, excluding rendering/skinning/platform overhead. This is not a native
performance budget. The independent production shot/muzzle/hit probe agrees at
2.50138855 m. Reload remains a 1.2-second cosmetic gesture with immediate Core
ammo transfer and shot interruption; no timed gameplay reload is claimed.

Main still owns one GameManager Seed-42 Simulation and the sole FPS frame driver.
Both input remains 2. Runtime C#, Main, shaders/materials, package pins, Core
source/tests, all nine Plugins DLLs and retained physical v1/v2 catalogs are
unchanged. The 5,629 source checks remain historical evidence for unchanged Core
inputs; this checkpoint does not claim a fresh source-suite run.

Final Windows package **20261102T225546Z** succeeded in 11.8260868 seconds with
zero build errors/warnings and 167,898,870 reported bytes. Post-build Editor MCP
still works; console has zero errors/warnings. The unchanged supported build
filter excludes the unused MCP helper; no MCP/Coplay DLL or name remains among
the player's 124 scripting assemblies. The self-excluding manifest records all
**193 package files**. Against 0.129 the only path absent is the optional
`Forgeborne_BurstDebugInformation_DoNotShip/Data/Plugins/x86_64/lib_burst_generated.txt`.
No file was copied/deleted to make counts match; this QA folder is not a release
archive. Installed/packaged identities and hashes are recorded for all nine
plugins, including Unity's four runtime facade replacements.

Actual native UI entered FPS, inspected the corrected thumb blend, fired
42/120 → 41/120 and used legacy R to reload to 42/119. Magazine reach, charging
glove/stitches/cuffs and the returned foregrip were observed. Tab switched to
orbital/back, Escape paused and the existing slot loaded. Its HUD showed 20/119,
stone 3, wood 6, a squad and the core 10 m away. Ten original 1920×1080 JPEGs
are retained. Reload snapshot callback durations are separate observations:
their clock begins before the input API and does not establish gameplay phase
timing or performance.

Both current save files were byte-exact at **23:04:02Z**. No QA save write or
backup restoration occurred. At the dated receipt the exact final player process
was present; its last observed UI was paused after loading. Editor Play is
stopped. Later user changes take precedence. Earlier 0.128/0.129 players were
not closed. Only the launch-owned rejected 223352Z test process was stopped,
after verifying exact package/log argument/creation time, paused initial HUD
and unchanged slots; Quit/autosave was bypassed. Its cleanup receipt records
unchanged saves and absence of that exact process.

The final package uses a separate Unity player `-logFile` argument. Its bounded
1,764-byte live log snapshot has zero observed project-exception patterns and
one inherited D3D12 info-queue diagnostic, classified in
surface-material-checkpoint.md. This is not shutdown or sustained stability
evidence. The full native loop, current-v2 cold recovery, representative budgets,
heard mix and director visual/experience/fun acceptance remain open. Grip
silhouette, glove tailoring/creases, continuous motion, actor and environment
craft still need work; AAA+ remains unmet.

## Reproduction and rollback

From the repository root, use pinned Blender 4.5.15 LTS with
`--background --python-exit-code 1 --python art/tools/build_carbine_v2.py`.
For the read-only source fit audit, load `art/source/PulseCarbine.blend`, then use
`--python art/tools/audit_glove_form.py -- --output <new-report-path>
--require-fitted-stitches`. Run `art/tools/audit_carbine.py` for source totals/
degeneracy and `art/tools/audit_hand_identity.py` for exact rest attributes.
Blender/FBX container bytes, trim ordering and checkout line endings can vary;
retain oriented surface roles/winding, rest rigs/landmarks, normalized skin,
fit/import/pose consumers and original PNG bytes as the explicit contracts.

With stopped/imported actual Main and the pinned Python MCP client connected
to ForgeborneUnity@9fc4dca83b83a1f1, set the intended PlayerSettings.bundleVersion
first, then run `tools/unity-hand-motion-prepare.cs`. Preparation now preserves
the caller's version and validates every imported skin weight. Enter Play at
the paused menu; run `unity-hand-motion-smoke.cs`, `unity-carbine-pose-smoke.cs`,
`unity-weapon-framing-feedback.cs` and `unity-authored-main-status.cs`.
Use `tools/capture-hand-motion.py --instance <instance> --output-dir
docs/evidence/<new-empty-folder>` for separate actual rendered callbacks.
Stop Play before the supported Windows build, then exercise the real controls.
Rollback is this isolated contribution. No save migration is needed. Never
restore older private backups over later user saves. Verified main updates remain
authorized; release/licence/visibility and unrelated publication are outside scope.

## Full-game gate and self-audit

All six influences remain equally important. The full AAA+ game goal is active.
Section 0: **1 FAIL; 2 PASS; 3 PASS** for affected checks, with inherited
diagnostics classified; **4 FAIL** for missing full end-to-end/human evidence;
**5 PASS; 6 PASS; 7 FAIL** for representative budgets; **8 PASS; 9 FAIL** for
incomplete full-game packaging; **10 PASS; 11 PASS** with variance documented;
**12 PASS; 13 N/A** (no delegation); **14 PASS; 15 PASS**. Applicable failures
prohibit a completion claim.

Self-audit: a technically passing thumb candidate was rejected after native inspection.
The source was corrected, then its actual import, poses, render and player were checked again.
Glove surfaces/bindings changed intentionally; rig and weapon preservation claims are scoped.
Save preservation, controls, numerical contact and visible craft remain separate evidence.
This is an unfinished-game contribution, with no AAA+ or director fun approval.

Frozen Constitution 6.18.0 remains 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
The single elapsed-time/callback evolution 922dcbb is validated and **staged,
not propagated**. This continuation starts no new invocation or evolution.
