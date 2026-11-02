# 0.126 — Retained worlds and more organic physical meshes

Rob called the emerging hall real progress, then asked for more realistic,
organic meshes. This pass keeps the hall's direction and replaces the repeating
fragments with larger sculpted formations, inclined fracture sections and layered
mineral veins. It adds the vaulted roof, suspension stays and a bowed defensive
plate. **The full AAA+ mesh/experience gate remains open.** Weapon, hands, suit,
actors, animation, environment detail and the complete playable loop still need
substantial work. Positive direction is not final-quality acceptance.

## Actual new-game entry and retained saves

GameManager explicitly populates authored profile **3** in its existing Seed-42
Simulation. No second Simulation or frame driver was added. Linked adapters keep
the same owner, Active Input Handler remains Both (2), and Core ships as nine DLLs.
The legacy root mesh/collider is empty in new Main.

Profile 2 remains the exact published v1 world. Its JSON, generator and Blender
source were preserved; SHA-256 remains
`3aaa044009b8db239b903b52bd3d928a59efd678e813a1b2eb36b25333863272`.
Profile 0 keeps legacy behavior; continuous profiles share the contact-lock
correction documented below. The original one-argument Populate and
ConstructionTemplate APIs retain v1 behavior; new consumers select a profile.

Profile 3 names `forgeborne.cinder-exchange.v2`, final geometry SHA-256
`3db547ff7a68ce12e5e51126ca755f38076448c5eb831fde659d333c07e5d6bd`.
Profile, ID and digest must agree before live replacement. Unknown/missing/mixed
identities fail without changing the current Simulation. Templates, material
ledger validation and geometry caches resolve through the selected catalog.
Preview caches also include profile identity; an old flat plate cannot remain
after switching to the bowed v2 plate.

**Freeze this final v2 identity when published.** Future physical changes must
retain versioned content or provide a tested explicit migration. Do not replace
v1/v2 bytes under an existing save digest. Loading an older slot deliberately
loads its original world; Restart Encounter uses the current initial encounter.
No slot is silently converted. Initial draft v2 images/data are labeled initial
evidence, not the final geometry or a separately published catalog.

## Original geometry and materials

`art/tools/build_foundry_v2.py` and `fracture_geometry.py` author the source in
Blender 4.5.15. Source and runtime data come from the same closed components.
There are **369 source pieces / 144,624 triangles** including construction
templates; actual opening Main has **327 pieces / 136,320 triangles / 408,960
expanded vertices**. The final UTF-8/LF payload is 15,987,819 bytes, within the
existing 150,000-triangle / 24 MB content contract. Maximum source vertices per
piece: 1,530. Two consecutive final generations match exact geometry bytes;
Blender/native container-byte reproducibility is not claimed.

Five formations start from 20,480-triangle source surfaces with seeded asymmetry,
bedding and fracture planes. Closed 3D Voronoi sections allow inclined breaks
rather than upright cell-shaped holes. V2 deposits may reach .45 m horizontally
beyond their ledger owner; their vertical bound and all construction templates
remain within one metre. Rendering, MeshCollider, containment, rays and swept
motion use the exact resulting triangles. Gathering removes every component of
the selected owner, credits its resource and reuses unaffected pieces.

Authoring preserves shared cut edges, cleans sub-millimetre Boolean artifacts,
tracks cap faces explicitly and transfers source normals from the compatible
geometric side at sharp edges. Interior fans, constrained triangulation and
oriented-boundary checks prevent zero-area cap strips or omitted shell edges.
Final quantized triangles still pass the unchanged Core area, normal, closure,
orientation, connected-shell, volume and capacity checks. No runtime validator
threshold was loosened. Smooth-skin continuity excludes intentionally flat
internal caps and genuine hard creases; **1,995 smooth joins** are checked.

CastFloor reuses the original Basalt map pair with .12 mm shading relief and a
roughness adjustment. EmberMineral supplies a darker mineral matrix around thin
hot inclusions; branches follow the actual host surface. No downloaded geometry,
photos or texture assets were used. The original eight map pairs and calibrated
colour contract remain. Existing v1 materials keep their previous settings.

## Orbital cutaway and physical consistency

Actual camera inspection caught a regression that source checks did not: the
new roof hid most of the orbital encounter. In perspective orbital mode, only
the six roof shells render shadows-only. Their colliders and canonical Core
geometry remain present. Switching back to FPS restores visible roofs.

Reserved layer 31, ForgeborneOverhead, lets orbital command picking and selection
occlusion ignore that same cutaway shell. FPS picking still includes it; other
walls and cover retain normal occlusion. This is an explicit tactical view,
not another physical world. The actual production picker reaches a visible
floor point beneath a roof while the unfiltered Unity/Core physical ray still
hits the same roof. View changes preserve the exact saved world and Simulation.

## Verification

Release source build: **zero warnings/errors; 5,629 passed, zero failed**. Tests
retain the old cases and exercise both authored catalogs: real resource budget,
gather, machine geometry, exact preview buffers, sweeping, cold/exact recovery,
cross-profile refusal, malformed payloads, bounded natural fractures and preserved
v1 identity. The roof underside matches an independent ellipse oracle, and its
daylight slot is actual empty space.

Unity MCP, actual Main, 6000.3.27f2:

- Exact render/material/collider consumers pass; **300 collider rays** agree with
  Core within **5.73e-6 m**. Fourteen architectural practical lights remain.
- Production gather/build, nine choices / 42 ghost pieces, payment, cancellation,
  power tint invalidation and exact original-state recovery pass.
- Live profile sequence 3 → 2 → 3 reaches distinct actual plate preview buffers:
  1,560 → 1,296 → 1,560 vertices. One Simulation, no slot writes.
- Actual floor-shader GPU A/B: 4,096 pixels on a linear target without
  post-processing; mean RGB-channel difference .15842085 when roughness bias
  changes 0 → -.16. This proves program consumption, not final art or performance.
- Owned/affected shader messages: zero. Unity's own console counts: zero errors,
  zero warnings, three informational logs. MCP's URP inventory misclassification
  remains documented; no installed package or log filter was modified.
- FPS, gather-close-up and orbital camera images were inspected. These are actual
  game cameras without overlay UI; the native JPEGs include actual overlay UI.

Cached refresh .1582 ms and gather refresh 1.6859 ms are scoped scripted timings.
They do not establish cold entry, native FPS, representative 1440p load, extensive
construction, memory limits or a complete performance budget.

## Native package and real controls

Current corrected Windows QA package:
`ForgeborneUnity/Builds/Windows-QA/20261102T140715Z/Forgeborne.exe`.
Unity 6000.3.27f2, Mono2x, BuildOptions.None, product 0.126.0:
**Succeeded, zero errors/warnings**, 159,477,203 reported bytes, 39.250164 seconds.
The package manifest fingerprints its runtime files, not just the unchanged
Unity launcher executable.

Earlier native package 20261102T073159Z was tested before the contact correction.
That actual native UI loaded the existing v1 slot,
confirmed restart into v2, switched Tab to perspective cutaway and back, fired
one mouse shot (42/120 → 41/120), reloaded with legacy R (42/119), and paused with
Escape. Its native-0126 images/report apply to that earlier package. The current
corrected package adds actual native contact movement and Windows import/export
evidence, described below. Native gather/build, full encounter completion,
current v2 cold-slot recovery, heard mix, representative performance and fun
remain unestablished.

The existing slot and backup were copied outside Git before QA and were
byte-exact unchanged at **2026-11-02T07:49:31Z**. Native was left paused for Rob;
Editor Play was stopped. Later user actions may change those files: never restore
an earlier private backup automatically. The current native contact run separately
verified slot/backup preservation at 14:14:46Z and left the existing user slot
loaded and paused in the corrected player. No raw save data enters this repository.
An unrelated foreground window briefly covered the player at first observation;
that image was not persisted, and the unrelated application was not operated.

The 1,764-byte bounded native log had no observed project exception. It retained
the D3D12 info-queue diagnostic 0x80004002, classified in the preceding surface
checkpoint using Unity staff's explanation. No warning suppression, OS graphics
tools installation or security-setting change was made. The driver/stability
matrix is still open. The menu's old Relay Yard subtitle also remains a known
presentation inconsistency; the actual new-game button and world are Cinder
Exchange. This does not change the full visual gate.

## Player contact lock correction

Rob reported total movement lock near objects. Production-controller probes
reproduced all-four-directions time-zero collisions in both retained worlds,
including the vessel collar and new rock slopes. SAT projections in large world
coordinates lost contact precision; contact rounding then made separating motion
look like an initial collision. Unscaled cross-product axes also gave different
effective contact tolerances for different triangle sizes.

Triangle/body sweeps now project relative to the body, scale spatial tolerances
with the SAT axis and allow separating/tangent motion at a ten-micrometre contact.
Approaching sweeps and deeply contained bodies still collide. The controller
keeps a 50-micrometre separation after resolved horizontal, vertical and step
contacts. Player dimensions, solid meshes and content digests are unchanged.

New independent box obstruction/escape tests and actual world approach/release,
retained stuck-pose recovery and clear-space wall sliding pass. The full 5,629
checks retain jump, gravity, ceiling, long-sweep, topology, save and no-drift
coverage. Unity's actual existing controller/Simulation passes four contact
approach/retreat scenarios, each retreating about one metre, plus wall sliding
and retained-pose recovery. The exact original world/pose is restored afterward.

The corrected native build imported a private save at the exact pre-fix collar
pose. Actual W moved X from 6.0127454 to 6.11274576; a new file exported through
the real Windows dialog confirms 0.10000036 m of escape with unchanged Z/yaw.
This is one native contact/input case, not exhaustive traversal. Its fixture had
no enemies so the evidence isolates movement. The real local slot/backup stayed
byte-exact, and Load Slot restored the user's encounter for paused review.
Private fixtures/exports remain outside Git. See movement-contact-checkpoint.md
and native-contact-qa.json for the precise scope and artifacts.

## Commands, evidence and next work

Build/run Core as in authored-foundry-checkpoint.md, using Release and the
isolated .NET 8.0.33 runtime. Publish only the nine DLLs with
`tools/build-unity-core.ps1`. Generate with Blender 4.5.15:
`--background --python-exit-code 1 --python art/tools/build_foundry_v2.py`.

With actual Main stopped/imported, use the pinned Python 3.10
`tools/unity-mcp-client.py`, instance `ForgeborneUnity@9fc4dca83b83a1f1`:
`unity-foundry-v2-prepare.cs` → optional isolated `unity-foundry-floor-gpu-probe.cs`
→ Play → `unity-foundry-cutaway-smoke.cs` → `unity-authored-foundry-smoke.cs` →
`unity-foundry-v2-capture.cs` → construction/power/catalog/main-status probes.
Stop Play before packaging. ForgeborneWindowsBuild.Request is asynchronous;
inspect its exact destination and terminal build-report.json before launch.

Current evidence: `docs/evidence/2026-11-02`, including the final reproduction,
world/preview/cutaway/GPU/native reports and `foundry-0126-manifest.json`.
Files labeled initial preserve the earlier inspected draft. They do not prove
the final dense mesh or its current package.

Rob also requested actual BioShock visual references. Remotely viewed publisher/
platform images and concrete original-art consequences are recorded in
bioshock-visual-study.md; no reference image or third-party game art was imported.
Continue model-specific art: weapon/hand anatomy, suit/actors, deformation and
animation; more convincing authored props/material detail and composition.
Continuous actor poses/hit shapes/navigation and complete native gather → build
→ survive → save/cold recovery remain open. Preserve the full, equally weighted
Minecraft/StarCraft/Diablo/PoE2/Borderlands/BioShock brief. No phase is completed
by this mesh checkpoint; a passing check does not prove fun.

Section 0 for the **full requested game**: **1 FAIL; 2 PASS; 3 PASS** for owned/
affected checks with the upstream diagnostic classified; **4 FAIL** for missing
full end-to-end/human evidence; **5 PASS; 6 PASS; 7 FAIL** for representative
budgets; **8 PASS; 9 FAIL** for incomplete full-game packaging; **10 PASS;
11 PASS** with documented container variance; **12 PASS; 13 N/A** (no delegation);
**14 PASS; 15 PASS**. Applicable failures prohibit a completion claim.

Self-audit: actual FPS/close-up/orbital views were inspected, not inferred from tests.
That inspection found and repaired the roof's orbital visibility/picking regression.
The physical mesh, catalog identity, input ownership and actual material consumers were checked.
Retained v1 loaded in the native player; the original save files stayed untouched at the recorded check.
Native control evidence remains distinct from complete gameplay, audio, performance and fun.
The meshes remain below the final AAA+ target; the next contribution must improve the unresolved product.

Frozen Constitution 6.18.0:
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`, verified in this invocation.
Skill evolution: elapsed-time/callback evidence, isolated `922dcbb`, validated
and **staged, not propagated**. No second invocation/evolution on this continuation.
Rob authorized verified main updates; no release, visibility, licence or separate
Constitution publication is implied.
