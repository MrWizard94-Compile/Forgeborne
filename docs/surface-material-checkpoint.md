# 0.125 — Original surfaces and a current native package

The actual Cinder Exchange now uses eight original paired surface recipes,
calibrated material colours and fourteen architectural practical lights. A
Windows QA package was built and launched. This is continued implementation of
Rob's no-block presentation correction, **not AAA+ acceptance**. Inspection of
the actual native view still fails the target: repeated faceted deposits, sparse
architecture, unfinished weapon/actors, animation, composition and experience
remain substantial work. Material detail does not resolve those problems.

All six influences remain equally important. The full requested game is active;
neither this checkpoint nor its passing checks complete the vertical slice.

## Original source and live consumers

`art/tools/bake_surface_library.py` uses Blender 4.5.15 and its bundled NumPy
1.26.4. Deterministic periodic fields author Graphite, Steel, Copper, Ceramic,
Rubber, Basalt, Paint and Timber. No photos, downloaded textures or external art
were used. `art/source/FoundrySurfaces.json` records recipes, seeds, scale, channel
contract, file hashes and independent expected GPU sample values.

Each material has a 1024-square colour PNG containing sRGB pigment and a matching
raw-linear HROM PNG: R height, G roughness, B occlusion, A metalness. Paint loss
changes pigment, substrate metalness, roughness and relief together. Packed-alpha
storage preserves the other channels even when metalness is zero. Height affects
shading only; it does not displace the published physical surface.

`ForgeborneSurfaceAssets` enforces colour/data import separation, BC7 Standalone
compression, repeated trilinear mipmaps, anisotropy 8 and disabled CPU copies.
`IndustrialSurface.shader` samples both maps with matching triplanar weights,
keeps pattern normals in the same local/world space as pattern positions, and
passes material channels into URP lighting. Emissive/glass roles retain their
existing untextured branch. No new package dependency was introduced.

A temporary isolated URP Unlit GPU probe reproduced the colour calibration
problem: input .5 on a Color property through SetColor, SetVector and a material
property block produced linear .214041144 on a linear ARGBFloat target without
post-processing. The textured roles now use white property tint and calibrated
colour textures. Untextured colours/emission and powered-machine tints use the
intended linear value encoded for the Color property. This is measured in this
Unity version, rather than inferred from a visually brighter screenshot. Unity's
[linear rendering documentation](https://docs.unity.com/en-us/engine/6000.3/manual/materials-and-shaders/graphics-color/color-spaces/linear-color-space/space)
describes the colour-space boundary; the saved GPU probe is the local oracle.

Eight warm point practicals remain. Six cool spot practicals now attach only to
the named bay luminaires, not to every cyan machine/actor part. Sky fill is .36;
the local reflection capture is 256 square. Native build-generated URP shader
prefilter/runtime-settings serialization is retained alongside its build report.
No lighting/performance improvement is claimed from operation counts alone.

## Geometry, ownership and user data

Published geometry `forgeborne.cinder-exchange.v1` remains byte-exact SHA-256
`3aaa044009b8db239b903b52bd3d928a59efd678e813a1b2eb36b25333863272`.
The Core DLL remains `713df788ff14b370e985c84f38d2885eb2adb2426640b77761ff8070fca5d5b6`.
Rendering/contact/aim identity and old save contracts are unchanged. Future
physical art changes must retain versioned content or provide a tested explicit
migration; do not overwrite v1 under an existing save identity.

Actual Main still has one Seed-42 GameManager and one shared Simulation. The
legacy root mesh/collider is empty; FpsInputAdapter remains the sole positive
frame driver. Active Input Handler remains Both (2). No Core source enters Assets.

Before native QA, the two existing slot/backup files were copied to a private
backup outside Git. Neither was loaded or deliberately saved by this test. Both
were byte-exact unchanged at **2026-11-02T04:47:17Z**. This is a timestamped check,
not a guarantee about later user actions. Never restore that backup automatically
over a subsequent user save. Export was cancelled and no export acceptance is
claimed. The native player was left paused for Rob; Editor Play was then stopped.

## Verification and package

- Release source build: zero warnings/errors; **5,535 passed, zero failed**.
  Core code/DLL bytes did not change in this material pass.
- Actual GPU import consumer: 48 colour/data samples, maximum linear-channel
  error .0181056857 against a .035 BC7/quantization allowance. All sixteen maps,
  alpha preservation, import separation, mipmaps/repeat and disabled CPU copies
  passed. Final summed texture runtime allocation is **35,360,512 bytes**; an
  earlier Editor lifecycle observation was 45,453,056 bytes. This variation is
  not attributed to an optimization. Neither value is a whole-game memory,
  native-player budget or VRAM-only measurement.
- Actual paused Main: 272 active pieces, 66,512 triangles; exact render/material/
  collider buffers, 300 collider rays (maximum difference 7.63e-6 m), fourteen
  practicals, production gather/build, exact save recovery and shared ownership
  pass. Power changes reach the material consumer without rebuilding its mesh;
  removing power restores the original tint. Owned/affected shader messages: zero.
- Native Windows Mono2x, Unity 6000.3.27f2, product 0.125.0, BuildOptions.None:
  **Succeeded, zero errors/warnings**, 143,485,971 reported bytes, 124.6712 seconds.
  Output: `ForgeborneUnity/Builds/Windows-QA/20261102T043456Z/Forgeborne.exe`.
  Executable SHA-256:
  `d91dbfa5c7d67cdf2c521d83112df029079f358519a1282d531baad4e1cd80c9`.
- Packaged Core and four other plugin assemblies match installed bytes. The
  packaged System.Buffers, System.Memory, System.Numerics.Vectors and
  System.Threading.Tasks.Extensions differ from the NuGet copies and match this
  Unity installation's unityjit-win32 framework/facade files byte for byte.
  `windows-0125-assembly-provenance.json` records identities, hashes and matches.
  No blanket claim that every Plugins DLL is copied unchanged into the package.
- Actual native UI: entry button starts FPS, Tab enters perspective orbital,
  Escape pauses. The saved JPEGs contain the actual player and overlay UI.
  Native movement/fire/gather/build, cold recovery, export, representative frame
  time, heard mix and fun were **not established** in this run.

The export check selected a stale file-picker control and received a filename
validation error; no rename or export occurred. Subsequent user input cancelled
the dialog. The application kept the encounter. No further automated native UI
input was sent. The file picker initially uses process working directory, which
is a usability issue still to fix through the existing transfer flow.
At a later process check the player had ended; its log ends with physics/input
shutdown. The cause/control used for termination was not observed. The earlier
paused state is timestamped evidence, not a claim that the player is still open.

The MCP console reader labels the URP asset inventory message as a Warning.
The installed URP source emits `Debug.Log`, and Unity's own GetCountsByType reports
**0 errors, 0 warnings, 3 logs**, with raw mode 8406016 for that entry. Both reports
are preserved; no installed package or diagnostic filter was modified.

The native D3D12 log contains `failed to query info queue interface (0x80004002)`.
This line was retained, not suppressed. [Unity staff's explanation](https://discussions.unity.com/t/unity-6-3-windows-player-crashes-on-alt-tab-when-discord-is-running-d3d12-case-in-129682/1702606?page=2)
identifies it as unavailable extra validation logging; it does not by itself
establish a game crash. No project exception/crash was observed in this bounded
run. A broader driver/performance/stability matrix remains open.

## Reproduction and review evidence

With Editor Play stopped, bake through Blender 4.5.15:
`blender --background --python art/tools/bake_surface_library.py`.
Then use the pinned Python 3.10 `tools/unity-mcp-client.py`, selecting
`ForgeborneUnity@9fc4dca83b83a1f1`: run `unity-surface-prepare.cs`,
`unity-surface-map-smoke.cs`, `unity-material-color-probe.cs`; enter Main Play and
run `unity-authored-foundry-smoke.cs`, `unity-authored-power-smoke.cs`,
`unity-authored-main-status.cs`, and `unity-surface-capture.cs`. These probes
restore camera/canonical state and never write a user slot. Stop Play afterward.
Native packaging uses `ForgeborneWindowsBuild.Request`, whose asynchronous job
must reach a terminal report before launching that exact output.

Evidence is under `docs/evidence/2026-11-01`: `surface-0125-manifest.json`,
`unity-surface-0125-*`, `unity-authored-surface-0125-world.json`,
`windows-0125-build-report.json`, versioned camera PNGs and `native-0125-*.jpg`.
The manifest separates source bytes, installed Core and package fingerprints.
The folder retains this invocation's starting date; native packaging and final
verification occurred on November 2.
Recipe seeds/versions and exact current PNG hashes are recorded; two independent
final rebakes and byte-identical Blender/native containers are not claimed.
The final Editor-only filter-mode import guard and evidence tools/docs followed
packaging; runtime source, shaders, materials and textures did not change afterward.
Earlier 0.124 images/manifests remain historical evidence and were preserved.

## Open gates and self-audit

Next art work needs convincing authored forms, less repetitive deposits, richer
human-scale architecture, finished actors/weapon/animation and stronger scene
composition. Physical changes require the versioned-content boundary above.
Continuous actors/hit shapes/navigation, full native loop/save recovery,
representative 1440p performance, heard audio and all six influences' depth remain
required. The full AAA+ visual/experience/fun gate is **FAIL**.

Section 0 against the full requested game: **1 FAIL; 2 PASS; 3 PASS** for owned/
affected checks with the upstream diagnostic classified above; **4 FAIL** for
missing full end-to-end/human evidence; **5 PASS; 6 PASS; 7 FAIL** for unmeasured
representative budgets; **8 PASS; 9 FAIL** for incomplete full-game package;
**10 PASS; 11 PASS** with documented reproducibility limits; **12 PASS** for
original art/unchanged engine dependencies; **13 N/A**, no delegation;
**14 PASS; 15 PASS**. Applicable failures prohibit a completion claim.

Self-audit: actual native images were inspected and still fail the visual target.
GPU values, actual material consumers and canonical recovery were verified.
Source QA, successful packaging and limited native input evidence are separate.
The cancelled export is recorded as unverified, with original slots preserved.
No shader/log suppression, asset copying or geometric save-identity churn occurred.
The next contribution must improve the unresolved product, not inflate test counts.

Frozen Constitution 6.18.0 remains
`63d99cdc72a1c18b5aa91b9336d02fd30126407b`, byte-exact verified in this invocation.
Skill evolution: elapsed-time/callback evidence, isolated commit `922dcbb`,
validated and **staged, not propagated**. This continuation adds no second
evolution and makes no retroactive law claim. Rob's main-update authority applies;
no release, visibility, licence or separate Constitution publication is implied.
