# Shared continuous surface checkpoint — 0.123.0

**Full game, world art and AAA+ acceptance remain FAIL.** This is infrastructure
needed to replace the rejected block presentation. Saved Main still starts under
the legacy profile; the new profile was exercised in reversible Play fixtures.
The current rectangular layout and softened placeholder forms are not approved.
All six influences remain equally important. Nothing was pushed or published.

## Implemented contract

`VoxelChunk.SurfaceProfile` explicitly selects LegacyBlocks (0) or
ContinuousTerrainV1 (1). Missing save metadata preserves legacy rules. Unknown
profiles fail before the live world is swapped. New-profile full saves restore
exact JSON, motion/look ownership and clocks in the existing Simulation.

Profile 1 reconstructs a continuous scalar material field on a 0.5 m contour
lattice. It is not an exact signed-distance field or a final art asset. A fixed
six-tetrahedron split generates immutable triangle snapshots. Reversed edge
interpolation is canonicalized to identical bytes. Ambiguous exact-zero nodes
resolve toward air by 1/256 density; genuine local axis planes retain exact unit
heights. This repaired non-manifold diagonal/contact cases without sinking the
ordinary floor. The isolated deposit extremum consequently moves inward by
1/258 m; the analytic new-profile expectations explicitly account for that policy.
No legacy expectation was changed.

Interior tests evaluate the retained immutable scalar samples using that exact
tetrahedral split, not a separate approximate field or a fragile parity ray.
Swept body collision and aimed actions query the same delivered triangles via
regional BVHs. The existing 0.3 m half-width / 1.8 m body, speed, jump and gravity
remain. The continuous controller adds contact sweeps, 0.35 m steps and a 50-degree
walkable-normal limit. Default legacy movement is unchanged.

Regions span 4 m, with 729 fixed slots for this chunk's contour domain. Edits
replace affected immutable regions and reuse the rest. Unity owns pooled region
mesh/collider views of those exact buffers. Unchanged regions containing power
consumers/cables refresh their colors when remote sources change. The current
single chunk requires an identity world transform; a mismatched transform is
rejected rather than silently separating rendering from Core queries.

Shot outcomes now carry an optional resolved surface distance. Production shot
and gather feedback use it under the continuous profile. Legacy feedback retains
its existing path. No additional Simulation or positive frame driver was added;
Core remains DLL-only under Assets/Plugins and input remains Both.

## Verification

Release build: **0 warnings/errors, 5486 passed, 0 failed**, using the genuine
isolated .NET 8.0.33 runtime. This includes the unchanged prior checks and the
dynamic no-Core-source-copy guard. New checks cover all 255 nonempty 2×2×2
occupancy arrangements (closed edges and connected vertex fans), region seams,
300 rays against an independent plane/edge oracle, 500 containment points against
delivered-mesh crossings, cached snapshot immutability, zero warm ray/sweep
allocation, fast motion, floor/jump/ceiling contact, aimed actions and save recovery.

Commands from the repository root:

```powershell
dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj -c Release --no-restore --nologo
& 'C:/Users/Bulkl/.codex/tools/dotnet-runtime-8.0.33/dotnet.exe' tests/Forgeborne.Core.Tests/bin/Release/net8.0/Forgeborne.Core.Tests.dll
./tools/build-unity-core.ps1
```

Nine DLLs were published locally. Current installed Core SHA-256:
`ac9c2c6dee9282f5902a0ff065c1b432d181f852613ca357a5a211865d647bb0`.

Actual paused Unity Main passed `unity-continuous-surface-smoke.cs`: 24,604
triangles / 73,812 vertices in 162 active regions; every actual render/collider
buffer matched Core. All 200 actual MeshCollider rays agreed, maximum distance
difference **7.15e-7 m**. Production gather removed the visible/physical deposit;
surface shot feedback, shared floor movement, exact save/identity recovery and
the bad-transform refusal passed. Scoped console was zero.

`unity-continuous-regional-smoke.cs` verified boundary edits and power-tint updates
in a geometrically unchanged region fed by a remote dynamo. Tight-loop stress
samples reached **28.50 ms**; this is a named remaining hitch, not discarded data.

`unity-continuous-cadenced-edits.cs` then spaced 16 edits and their rollbacks by
0.1 wall seconds and separate player frames, after warmup. The actual callback
completed in **3.6109 s / 811 player frames**, restored exact state/background
configuration, and unregistered itself. Edit samples ranged **1.345–17.219 ms**,
with up to 8 regions rebuilt and zero GC collections during the timed edits.
Maximum observed unscaled frame time was **40.388 ms**. This paused background
Editor test is not native frame-rate, focused input, representative gameplay,
hundreds-of-entities performance or quality acceptance.

The final isolated gather sample was **1.112 ms / one region**, while the full
initial fixture build took **217.790 ms**. Earlier full-build prototypes measured
529.223 ms; their old geometry/counts differ, so that is historical diagnosis,
not a controlled final performance comparison. See the preserved JSON artifacts.

Existing production feedback, orbital handoff, construction safety and guide
checks passed again on the restored legacy Main, preserving exact state and one
Simulation. The original art consumer also passed before the final topology
repair; its art/profile assets did not change. No slot file was written. Play is
stopped and no current native surface package has been built.

## Evidence and remaining work

Current JSONs: evidence/2026-11-01/*-continuous.json and
continuous-cadenced-edits-profile1-final.json. Earlier cadence/monolithic files
are diagnostic history from earlier implementations. Actual FPS/orbital camera
PNGs continuous-*-diagnostic.png inspect the old layout with the new surface;
they exclude overlay UI and restore exact state. They visibly fail the desired
authored-world result and are not a visual milestone or a native review build.

Next implement authored gameplay geometry/content binding and re-author the
playable place with believable architecture and real resource deposits. Preserve
one physical/visual source, stable content identity and save validation before
world swap. The material field is useful for editable natural terrain; it must
not constrain architecture/models to softened cells. Static mesh shells require
their own closed-component containment contract, rather than the uniform-field
region assumption in the current compound. This is not implemented yet.

Prospective construction geometry validation, precise authored actor shapes and
continuous actor motion/animation are still missing. The new profile is not Main's
default until the integration gaps are handled. World boundaries, material/art
quality, audio/feel, native input/save recovery and sustained performance remain
open. Full scope is in design-pillars.md and the director objective.

Section 0 for the full request: 1 FAIL; 2 PASS; 3 PASS (scoped owned checks);
4 FAIL (complete new world/native behavior missing); 5 PASS; 6 PASS; 7 FAIL;
8 PASS; 9 FAIL (current package absent); 10 PASS; 11 PASS (deterministic material
field/profile and explicit source evidence); 12 PASS (original implementation);
13 N/A (no delegation); 14 PASS; 15 PASS (audit below).

The shared geometric source and scripted consumer paths have direct evidence.
Topology defects were reproduced and repaired without weakening old tests.
Partial speed improvements do not pass the full performance requirement.
The old layout remains visibly wrong despite the new contour.
Native/art/actor/construction gaps prohibit completion of the requested game.

Frozen Constitution 6.18.0: `63d99cdc72a1c18b5aa91b9336d02fd30126407b`.
Skill evolution: elapsed-time/callback evidence change `922dcbb` remains validated,
**staged, not propagated**; no second evolution is created on this continuation.

Mathematical background: [triangle-box separating-axis derivation](https://fileadmin.cs.lth.se/cs/Personal/Tomas_Akenine-Moller/pubs/tribox.pdf).
The implementation is original; no reference implementation was copied.
