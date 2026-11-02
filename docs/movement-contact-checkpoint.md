# Player contact recovery — 0.126.0

Rob reported that getting close to an object completely stopped movement. The
source probe reproduced the lock in both authored profiles. A vessel collar
left the feet at (6.0127454, 3.9999995, 16.5); all four horizontal sweeps returned
zero travel. V2 additionally locked on two inclined rock contacts. This was a
real movement defect, not an accepted trade-off for detailed geometry.

`TerrainGeometry.TriangleSweep` now uses body-relative projections and scales
spatial tolerance with the SAT axis. A contact within ten micrometres may move
away or tangent; motion into the solid still collides. `FpsController` resolves
contact with a 50-micrometre separation, including steps and vertical landing.
The 0.3 m half-width, 1.8 m height, geometry, input ownership and save identities
are unchanged. Legacy-block movement is untouched. Continuous profiles share
the corrected contact behavior.

Verification:

- Release build 0 warnings/errors; **5,629 checks pass, zero fail**. New checks
  use real approach/release, the exact old stuck pose, validated free-space wall
  sliding and an independently specified closed box. Approaching/high-speed
  obstruction and deeply contained refusal remain covered. Existing movement,
  jumping, ceiling, landing, topology, save and package/no-drift checks pass.
- `tools/unity-movement-contact-smoke.cs` uses actual Main's existing controller
  and Simulation, both retained profiles, four approach/release cases (roughly
  one metre of retreat each), tangent movement and retained-pose recovery.
  `finally` restores the original snapshot exactly; no slot writes.
- Main's mesh/ray/gather/build/power/catalog/cutaway consumers were rerun after
  publishing the corrected Core DLL. Console: 0 errors, 0 warnings, 3 logs.
- Corrected Windows build **20261102T140715Z** succeeded, 0 warnings/errors,
  Unity 6000.3.27f2 / Mono2x / None, version 0.126.0. Core SHA-256:
  `855e6ea07332f59d0b14cd3519baf2d4a09c8c72a5b1e0475bd295300bd67211`.
- The exact native executable imported a private valid v2 save at the old stuck
  collar pose, resumed, accepted a real legacy W key and paused. Its actual
  Windows export moved X **6.0127454 → 6.11274576**, Z 16.5 and yaw 90 unchanged.
  The native before/after images and export-success image are in current evidence.
  This fixture had no enemies; it proves one native contact escape, not a battle.
- Existing slot and backup file set/bytes were unchanged at
  **2026-11-02T14:14:46Z**. Load Slot then left the user's existing encounter
  paused in the corrected build. No files were automatically restored. Private
  raw fixtures, exports and slot copies are outside Git.

Current evidence is under `docs/evidence/2026-11-02`: unity-movement-contact.json,
native-contact-qa.json, native-contact-*.jpg, windows-0126-contact-build-report.json,
windows-0126-contact-assembly-provenance.json and foundry-0126-manifest.json.
The earlier 073159Z build and native-0126 images predate this correction. The
launcher executable is not a unique build identity; inspect the full manifest.

Run the Release test project and pinned .NET 8.0.33 test DLL; publish with
tools/build-unity-core.ps1. Refresh stopped Main with Unity MCP, Play and wait for
GameManager/PlayerFlow initialization, run the contact probe, then affected world/
construction/power/catalog/cutaway checks. Stop Play and request a fresh Windows
QA build. Native fixture/import/export evidence is separate from scripted Unity.

Full AAA+ quality, all-obstacle traversal, complete native gather/build/survive/
save loop, representative performance, heard mix and fun remain open. The full
Section 0 audit and this invocation's staged evolution are in
foundry-v2-checkpoint.md. No completion of the full game is claimed.
