# First-person carbine craft — 0.128.0

This continues the full six-influence AAA+ game and Rob's arm/hand feedback.
Baseline is `586f8f79133a804bfdadd70346727e66ad047cfa`; work branch is
`codex/first-person-weapon-framing`. The previous corrected gloves remain a
candidate, with no director visual or fun approval.

The carbine housing was oversized for its grip and barrel. Moving it closer
without changing its angle made that problem worse. Those offset-only images
are retained separately. Angled views then exposed the shape defect directly.
The original receiver now tapers through slimmer cross sections; its panels,
charging slide, selector and optic mounts are refitted. Actual magazine and
pistol-grip sockets connect the assemblies to the narrower body. No downloaded
models, textures or third-party game art were used.

Main now saves offset `(.19, -.10, .55)` and view angles `(0, -12, 0)` at the
unchanged 75-degree FPS field of view. Recoil/reload compose with that rest
orientation; reset and orbital return restore it. This is a view-only holding
angle. Core aim, ammo, damage, movement, collision, saves and physical catalogs
remain unchanged. The muzzle landmark is still `(0, .016, .665)` in model space.

The actual source and import both contain **88,224 triangles**, within the
existing 100,000 view-model limit; 340 additional triangles form the refitted
socket/bridge geometry. This count does not prove visual quality. The corrected
glove vertex positions, skin weights and seventeen-joint rest rigs are exact
against 0.127. Blender reordered some trim polygons; winding-preserving indexed
polygon/material multisets are exact. The audit retains the failed raw ordering
comparison and distinguishes that variance from actual geometry changes. It does
not normalize reversed faces away. Both actual skin consumers and all four map
weights remain validated by preparation.

Evidence is in `docs/evidence/2026-11-02/framing-0128/`. `offset-only/` and
`pre-refinement/` apply to the old housing. Current `candidates/` compare placements
on the revised source, including selected rest, reload and peak recoil. The real
skinned renderer crosses normal player/render frames before each capture. The
temporary held-pose/update/background flags and original view properties are
restored afterward; no baked replacement mesh or new Simulation is used. Rest
palm projections are diagnostics and do not prove visible hand pixels. Actual
images were separately inspected.

`framing-0128-manifest.json` records changed source/art/tools/docs/evidence and
all 194 files of the final Windows package, with itself excluded. It retains
the distinct source, scripted Editor, native-control and director-review scopes.

The affected production action probe fires through the real input adapter into
the existing Simulation. An independent box-ray oracle agrees at 2.50138855 m;
the actual line renderer starts at the authored muzzle and ends at the canonical
hit. Damage/ammo, unchanged world time, rest reset and exact in-place state
recovery pass. CPU skin checks retain pause, cross-hand isolation, trigger flex,
forearm length and orbital hide/return coverage. Six hundred warmed presentation
updates took 6.7753 ms with zero allocated bytes in Editor. This excludes GPU
skinning/rendering/platform costs and does not establish native performance.

Actual Main retains one Seed-42 GameManager/Simulation and the sole FPS frame
driver; owned shader diagnostics are zero. Both input remains 2. The nine Plugins
DLLs, Core source/tests, v1/v2 catalogs and all twenty existing surface PNGs are
unchanged. The preceding 5,629 passing source checks are historical evidence for
unchanged Core inputs, not a fresh source-suite run in this checkpoint.

Final Windows package **20261102T210226Z** succeeded with zero build/console errors
or warnings, 167,829,430 reported bytes, Unity 6000.3.27f2 / Mono2x. Actual native
UI entered FPS, fired 42/120 → 41/120, reloaded via legacy R to 42/119 with the
mid-gesture visible, switched Tab to orbital and back to the selected framing,
paused with Escape, and loaded the current user slot while paused. Eight observed
1920×1080 JPEGs are preserved. The loaded HUD shows 20/119, stone 3, wood 6, a
squad and the core 10 m away. Slot and backup were exact at **21:13:44Z**; the
same player process was present and the last observed UI was paused. Editor Play
is stopped. This is a dated observation, not a claim that the app remains open.
Later user save changes take precedence; no raw saves are committed or restored.
The bounded 1,764-byte live-log snapshot has no observed project exception and
retains the inherited D3D12 info-queue diagnostic described in
surface-material-checkpoint.md. No complete
native gather/build/survive loop, current-v2 cold recovery, representative budget,
heard-mix, stability or director fun acceptance is established by this art pass.

Packaging inspection rejected the earlier **20261102T201106Z** package because
it included an unused `MCPForUnity.Runtime.dll` helper from the pinned authoring
package. That package's native receipts are retained under `pre-filter-player/`;
they do not describe the final package. No game source references to that helper
were found. The new Editor-only `ForgebornePlayerAssemblyFilter` uses Unity's
supported [IFilterBuildAssemblies callback](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Build.IFilterBuildAssemblies.OnFilterAssemblies.html),
also verified in the installed 6000.3.27f2 API XML. The normal build excludes the
helper before player packaging and fails its postcondition if MCP DLLs remain.
The final Managed directory and all 124 `ScriptingAssemblies.json` names contain
no MCP assemblies. A real Editor MCP query after this build still succeeds.
Neither the installed package nor packaged metadata was patched or suppressed.

## Reproduction and rollback

With stopped/imported Unity, generate the original asset using pinned Blender
4.5.15 LTS with `--background --python-exit-code 1 --python art/tools/build_carbine_v2.py`.
Audit the loaded `art/source/PulseCarbine.blend` with `art/tools/audit_carbine.py`;
an optional `-- --output <new-report-path>` avoids overwriting earlier evidence.
`audit_hand_identity.py` fingerprints indexed positions, oriented surfaces,
material roles, weights and rest rigs; detailed comparison files stay private.
Blender/FBX container bytes and raw polygon ordering can vary. Geometry/import
consumers, authored material contracts and documented semantic invariants are
the reproducibility boundary. Checkout text bytes may vary with line endings.

Use the pinned Python 3.10 MCP environment and explicit connected instance
`ForgeborneUnity@9fc4dca83b83a1f1`. Run `unity-weapon-framing-prepare.cs`, then Play
and pause actual Main. Run `capture-weapon-framing.py --instance <instance>`;
`--reload` compares reload, `--shot --selected-only` captures the selected peak
shot pose. The helper restores view properties and runtime flags in `finally`.
Use `unity-carbine-pose-smoke.cs`, `unity-weapon-framing-feedback.cs` and
`unity-authored-main-status.cs` for affected consumers, saving into a new evidence
folder. Stop Play before packaging and inspect the exact terminal build report.
Test native controls through the real player UI while preserving current saves.

Rollback is this isolated contribution; no save migration is needed. Never
restore earlier private backups over later user changes. Rob's verified main
update authority persists; no release/licence/visibility change is included.
The Editor had closed normally before this run; the same pinned project was
reopened and its existing local MCP session reconnected. No package update,
client configuration action, authentication or security change was performed.

## Full-game gate and self-audit

The weapon/hand sculpt and motion still need further craft and Rob's review.
Environment construction/history, actors, continuous motion, full native loop,
performance, heard audio and complete depth across all six equally important
influences remain open. AAA+ remains unmet; a passing check does not prove fun.

Section 0 for the full requested game: **1 FAIL; 2 PASS; 3 PASS** for owned and
affected checks with the inherited diagnostic classified; **4 FAIL** for missing
full end-to-end/human evidence; **5 PASS; 6 PASS; 7 FAIL** for representative
budgets; **8 PASS; 9 FAIL** for incomplete full-game packaging; **10 PASS;
11 PASS** with variance documented; **12 PASS; 13 N/A** (no delegation);
**14 PASS; 15 PASS**. Applicable failures prohibit a completion claim.

Self-audit: initial offset-only changes were rejected after inspecting real images.
The holding-angle study exposed the housing defect; actual geometry was corrected.
Hand preservation was checked by indexed/oriented data, not inferred from counts.
Real skin, action, reset and state consumers were checked separately from visual review.
The result remains a contribution to an unfinished game, with no AAA+ or fun approval.

Frozen Constitution 6.18.0 remains `63d99cdc72a1c18b5aa91b9336d02fd30126407b`.
The single elapsed-time/callback evolution `922dcbb` is validated and **staged,
not propagated**. This continuation does not start another invocation/evolution.
