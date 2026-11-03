# First-person arm fit — 0.131.0

Continue the full six-influence AAA+ game and Rob's arm/hand correction. Baseline
main is 1400c67d2c8253740a745a41828b3409c81d5f01; work branch is
codex/first-person-arm-fit. The previous goal turn made verified, published 0.130
progress. The current art, complete experience and fun remain unaccepted.

The support elbow's source x=-.195 was nearly cancelled by the actual camera-local
weapon offset x=.19. Actual paused Main measured its rest viewport x=.492679536,
almost on the centre line, explaining the sleeve rising from the middle of the
screen. The original source now moves that elbow left and forward, retaining its
forearm length. Actual rest viewport x is .404091865: a 170.1-pixel leftward
change at 1920-pixel width. The actual wrist changes by less than 0.002 mm;
source wrist/palm/finger endpoints and grip landmarks are exact. These projections
are diagnostics; the separate rendered and native images were also inspected.

Four detached-looking round knuckle trim pieces per hand are removed. Knuckle
form remains in the continuous original glove body, with the fitted stitching
from 0.130. No downloaded art, anatomy template or copied game geometry is used.
Source and actual import now have **85,304 triangles / 43,044 source vertices**,
down 4,224 triangles and 2,128 vertices. Left/right skins have 26,492/26,252
triangles and 13,344/13,200 source vertices, split to 13,442/13,298 on import.

The left sleeve/body/bindings are regenerated for the new forearm direction.
Only LeftForearm's authored endpoints change; the right rest rig is exact.
All other left endpoint coordinates and parent names are exact, but moving their
float32 parent changes descendant rest-matrix arithmetic. The initial exact-matrix
comparison correctly failed that assumption. The report now explicitly records
maximum descendant matrix-element difference 5.96046448e-7 and translation
difference 2.98023224e-8 m, checked against 1e-6 bounds; it does not claim bitwise
matrix identity. Source forearm-length difference is 1.57987795e-8 m. Actual
imported forearms measure .289326727 m left and .262506664 m right.

The right connected body's indexed positions/weights/oriented material-role
surfaces are exact against 0.130. Both rigs still have seventeen joints. Rig
object matrices, muzzle/signed axes/palm landmarks and frame/magazine/charging
oriented surface roles at the recorded 1e-7 m quantum are preserved. Both largest
connected glove/sleeve bodies contain 12,002 vertices, both fabric roles and all
seventeen joints. All source components are closed/manifold with zero degenerate
faces. Source/import weights are normalized, own-hand and at most four influences;
import maximum sum error is 1.1920929e-7. Each hand's 738 imported cuff vertices
remain unit-bound to its forearm. Left/right source stitch gaps stay below
.813/.800 mm, with zero nearest-skin weight L1 error.

A second generation in isolated private output reproduces indexed glove
positions, weights, rest rigs, oriented topology/material-role sets, the geometry
manifest and source fit/quantized weapon audit. Raw face order also matches in
this pair; Blender/FBX container hashes differ. Reviewed source/FBX are unmodified.
Container identity is recorded separately from those geometry/consumer contracts.

Evidence is in `docs/evidence/2026-11-02/arm-fit-0131/`. `baseline/` records real
0.130 rest/charging renders. The selected `first-fit/` records 0.131 rest,
magazine draw and charging. Each actual skinned capture crosses separate callbacks
and more than one normal frame, preserves canonical state and restores temporary
view/update/background flags. Landmark viewport metadata does not prove visible
contact. Baseline/current projection, source comparison, import and isolated
reproduction reports retain their separate scopes.

The actual rig sweep passes 241 phases, 104 magazine-contact and 34 charging-contact
samples. Maximum palm/hook reference errors are .00271/.00323 mm; forearm-length
error is below .000924 mm. Other-hand baked-skin difference is below .00265 mm.
Same-elapsed-time actual skin at 30/60/144 Hz differs by at most .01358 mm. Pause,
shot interruption, exact imported reset and orbital hide/return pass. Maximum
sampled palm step remains 28.815 mm at 240 intervals; these measurements do not
prove natural continuous animation or visible surface contact. Six hundred warmed
presentation updates took 8.009 ms and allocated zero bytes in Editor, excluding
rendering/skinning/platform overhead. No representative native budget is claimed.
The independent production shot/muzzle/hit probe still agrees at 2.50138855 m.

The unchanged runtime gesture is 1.2 seconds and cosmetic: Core ammo transfer is
immediate and firing can interrupt. Runtime C#, Main, shaders/materials, package
pins, Core source/tests, all nine Plugins DLLs, physical v1/v2 catalogs and all
twenty original surface maps are unchanged. Main retains one GameManager Seed-42
Simulation, linked adapters and the sole FPS frame driver. Both input remains 2.
The 5,629 source checks remain historical evidence for unchanged Core inputs;
this checkpoint does not claim a fresh source-suite run. The regenerated prefab
only gained trailing whitespace; normalization was checked against its prior
semantic content before removing that noise.

Windows QA package **20261103T031237Z** succeeded in 11.4451031 seconds with zero
build errors/warnings and 167,677,142 reported bytes. Actual post-build Editor MCP
still works; console has zero errors/warnings. The unchanged supported assembly
filter excludes the unused MCP helper; no MCP/Coplay DLL or name remains among
124 player scripting assemblies. The manifest inventories all **193 package
files**, plus installed/packaged identities and hashes for all nine plugins.
Unity's four runtime facade replacements match this Editor's runtime bytes.
This QA folder is not a release archive.

Actual native UI entered FPS and showed the revised sleeve. Mouse fire changed
42/120 → 41/120; legacy R reached 42/119, with visible magazine reach/draw and
return to grip. Both first reload snapshots were too early to show charging;
they are labelled as magazine observations. A second mouse shot gave 41/119,
then R reached 42/118 and the charging hand/sleeve was captured. Callback wall
times, including a delayed second observation, are recorded separately and begin
before the input API. They do not measure authoritative animation phase or speed.
Tab orbital/FPS, Escape pause and existing-slot load also passed their observed
checks. Loaded HUD: 20/119, stone 3, wood 6, squad visible, core 10 m away.
Thirteen original native 1920×1080 JPEG observations are retained.

Both current user save files remained byte-exact at **2026-11-03 03:23:11Z**.
No QA save write or backup restoration occurred. At that dated receipt, the exact
new process/package/separate log argument and launch creation time were verified;
its last observed UI was paused after loading. Earlier native sessions were not
closed; Editor Play is stopped. These are dated observations, and later user
changes take precedence. Do not use Quit/autosave or restore older private backups
over later saves. The private receipt collector's initial time comparison parsed
an already typed JSON DateTime a second time, losing its zone. Comparing explicit
DateTimeOffsets repaired the collector; actual creation-time difference is zero.

The separate 1,764-byte live native log snapshot has zero matches for its recorded
project-exception pattern and one inherited `d3d12: failed to query info queue
interface` diagnostic, classified in surface-material-checkpoint.md. The matching
pattern includes the actual spaced wording. The diagnostic was retained, not
suppressed. This is not shutdown, sustained stability, a full native loop or
current-v2 cold-recovery evidence. No heard mix, native budgets or director art/
experience/fun approval is established.

## Reproduction and rollback

Use pinned Blender 4.5.15 LTS with `--background --python-exit-code 1 --python
art/tools/build_carbine_v2.py`. Load the resulting original source and run
`audit_carbine.py`, `audit_hand_identity.py` and `audit_glove_form.py -- --output
<new-path> --require-fitted-stitches`. Compare intended rest endpoint/parent
changes explicitly, including recorded descendant matrix variance. Weapon
oriented roles preserve winding at the recorded quantum; container hashes and
checkout line endings may vary.

With stopped/imported actual Main and the pinned Python MCP client connected to
ForgeborneUnity@9fc4dca83b83a1f1, set the intended version before running
`tools/unity-hand-motion-prepare.cs`. Enter paused Play and run
`unity-hand-motion-smoke.cs`, `unity-carbine-pose-smoke.cs`,
`unity-weapon-framing-feedback.cs` and `unity-authored-main-status.cs`.
`unity-arm-fit-projection.cs` measures actual rest-joint camera projections; it
resets cosmetic feedback to rest and checks canonical state/ownership preservation.
Run `tools/capture-hand-motion.py` with the **pinned MCP Python interpreter**,
explicit instance and a new empty evidence folder; `--phases 0 .44 .84` selects
the affected views. An initial launch through general Python lacked the pinned
MCP dependency and failed before pose/canonical mutation; using the pinned
interpreter repaired that tooling invocation. Stop Play before the supported
Windows build and exercise actual controls separately.

Rollback is this isolated contribution; no save migration is needed. Verified
main updates remain authorized. Release/licence/visibility and unrelated
publication remain outside scope.

## Full-game gate and self-audit

All six influences remain equally important. The full AAA+ goal remains active.
Finger anatomy, glove tailoring/creases, continuous arm/actor motion, environment
craft, full native gather/build/survive and current-v2 cold recovery, representative
budgets, heard mix and director experience/fun gates remain open.

Section 0: **1 FAIL; 2 PASS; 3 PASS** for affected checks, with inherited
diagnostics classified; **4 FAIL** for missing full end-to-end/human evidence;
**5 PASS; 6 PASS; 7 FAIL** for representative budgets; **8 PASS; 9 FAIL** for
incomplete full-game packaging; **10 PASS; 11 PASS** with variance documented;
**12 PASS; 13 N/A** (no delegation); **14 PASS; 15 PASS**. Applicable failures
prohibit a completion claim.

Self-audit: actual camera projection exposed a source fit problem beyond texture detail.
The source elbow and trim changed; runtime gameplay and retained content did not.
An exact matrix assumption failed, and the measured float32 variance is explicit.
Native early snapshots were labelled honestly and a later gesture was observed separately.
This remains an unfinished-game contribution with no AAA+ or director fun approval.

Frozen Constitution 6.18.0 remains 63d99cdc72a1c18b5aa91b9336d02fd30126407b.
The single elapsed-time/callback evolution 922dcbb is validated and **staged,
not propagated**. This continuation starts no new invocation or evolution.
