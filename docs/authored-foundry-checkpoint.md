# 0.124 — Main enters Cinder Exchange

Rob rejected the optional-only change because Main still showed the old block
courtyard. Main now saves `UseAuthoredFoundry: 1`, `UseCourtyardEncounter: 0` and
starts profile 2 through its existing GameManager. The entry button says
**ENTER CINDER EXCHANGE**. This corrects the actual entry, not just a diagnostic.

**Full AAA+ acceptance remains FAIL.** Actual FPS/orbital captures were inspected.
The room has original curved structure, pressure vessels, arched doors, exchanger,
fractured deposits and machine models, but still lacks convincing authored
surface detail, finished lighting/composition, animation and experience quality.
It is an integration checkpoint, not an accepted vertical slice or finished game.
All six equally important influences remain as specified in design-pillars.md.

## Shipped source and actual consumer

- `art/tools/build_foundry_environment.py` generates original Blender source and
  the same closed mesh pieces embedded in Core. No imported art/templates.
- 314 source pieces / 74,728 triangles include the nine construction templates.
  Actual opening world: 272 pieces / 66,512 triangles / 199,536 expanded vertices.
  Eight practical lights attach to the actual authored luminaires.
- Each piece validates bounded coordinates/indices, outward normals, paired
  directed edges, positive volume, connected closed topology and ledger binding.
  Arbitrary geometry, paths and URLs cannot enter through saves.
- Rendering, MeshCollider, Core rays, containment and swept motion use the same
  triangles. Authored compound containment uses union semantics; it does not
  index the contour field's fixed region lattice. Unchanged pieces are reused.
- Deposits bind to the existing resource cells. Gathering removes their actual
  pieces and credits the existing inventory. Stock is 25 stone, 11 ember, 6 timber:
  enough for workstation + mill + dynamo + wall without debug grants. Availability
  is a source prerequisite check, not proof of a fun or balanced gathering route.
- All nine constructed types have original shared geometry. Construction reserves
  a one-metre service envelope against static architecture before spending. Planned
  enemy/squad steps also sweep the authored surfaces, rather than walking through
  an empty ledger cell occupied by a vessel. Actor movement remains grid-stepped.
- ConstructionGuide now renders the exact template as a transparent model ghost,
  with no collider. The cube outline remains only in legacy profile compatibility.
  Placement, prices, input routing and cancellation remain in existing production
  paths. Power changes reach industrial material property blocks without rebuilding
  the unchanged workstation; removing the source restores its unpowered tint.
- The carbine pressure casing/readout, gauntlets and forearms now use curved
  volumes. Only PulseCarbine was regenerated; the selective export command avoids
  churning unrelated original models. Metres, signed axes and muzzle checks pass.

## Save contract and reproducibility

Profiles 0/1 retain their previous behavior. Profile 2 saves require content ID
`forgeborne.cinder-exchange.v1` and exact geometry SHA-256
`3aaa044009b8db239b903b52bd3d928a59efd678e813a1b2eb36b25333863272`.
Missing/changed IDs, digests, floor bindings or unsupported invisible resources
fail before live replacement. New saves recover exact pose, removals, machines,
inventory and owner identities. Old user slots were not written or converted.
Loading an old prototype save deliberately restores its old geometric rules;
Restart/New Game uses the new initial encounter.

Two consecutive final Blender generations produced identical geometry bytes.
`.gitattributes` forces LF for embedded geometry JSON so a Windows checkout cannot
change its save identity. Blender/FBX container-byte reproducibility is not claimed.
The new source and runtime byte manifest is authored-0124-manifest.json.

**Freeze published v1 geometry.** Future geometry edits must retain this contract
under its saved identity and introduce versioned content/profile resolution or an
explicit tested migration. Do not overwrite v1 and make existing saves unreadable.
Material and nonphysical skin changes still need visual/consumer verification.

## Verification

Release build: 0 warnings/errors. **5,535 source checks pass**, including all old
tests, independent geometric oracles, closed unions, movement/boundary refusal,
real resource budget, exact preview/placement meshes, nine construction choices,
malformed payloads and cold/exact recovery. Nine DLLs were published; no Core
source was copied to Assets. Installed/build Core hashes agree (see manifest).

Unity MCP, actual Main entry, Unity 6000.3.27f2:

- One Seed-42 GameManager; every linked adapter references its one Simulation;
  FpsInputAdapter remains the sole positive driver. Both input remains value 2.
- Exact mesh/material/collider buffers and 300 independent MeshCollider ray
  comparisons pass; maximum distance difference **7.63e-6 m**.
- Actual gather removal, construction, unchanged-piece reuse, power material
  changes, exact save recovery and original-state restoration pass.
- Actual nine-choice ghost check covers 42 template pieces, transparent material,
  no cube outline/collider, read-only preview, primary-action payment and pause
  cancellation. No keyboard or human-play claim.
- Final Main status reports the legacy root mesh/collider empty, new entry text,
  three enemies, paused menu, shader messages 0. Scoped runtime console 0.
- Warm cached refresh and gather timings are recorded in the final Main probe.
  These do not measure cold entry, rendered/native FPS or representative load.

Commands: run `dotnet build tests/Forgeborne.Core.Tests/Forgeborne.Core.Tests.csproj
-c Release --no-restore --nologo`, then the Release test DLL with the isolated
8.0.33 runtime; publish with `tools/build-unity-core.ps1`. Generate geometry with
Blender 4.5.15 `--background --python art/tools/build_foundry_environment.py`.
Regenerate the gun with `--python art/tools/build_visual_assets.py -- PulseCarbine`.
Use tools/unity-mcp-client.py and the pinned local Python 3.10 MCP environment:
stopped/imported Editor → unity-authored-foundry-prepare.cs → Play →
unity-authored-foundry-smoke.cs → unity-authored-construction-smoke.cs →
unity-authored-power-smoke.cs → unity-authored-main-status.cs. Preparation saves
Main; the probes restore full canonical JSON in place and never write a slot.

Evidence files are under docs/evidence/2026-11-01. `*-main-smoke`, preparation,
construction, power and main-status/console reports describe final v1. The earlier
unity-authored-foundry-smoke.json is a superseded optional prototype, not final
v1 timing evidence. The two diagnostic PNGs are actual cameras, without overlay
UI, and remain unapproved. Existing legacy feedback/orbital/field checks passed
before the default switch; those reports are scoped compatibility evidence.

## Open gates and full-goal audit

No current native art package or native input/audio acceptance. Current full-game
limits include precise continuous actor poses/hit shapes/navigation/animation,
representative 1440p performance, world escape after extensive construction,
finished resources/fracture/audio/weapon feel, and full six-influence depth/content.
The massive skill tree, loot/build depth, arsenal and orbital progression are not
made complete by this environment change. Source checks do not prove fun.

Section 0 against the **full requested game**: 1 FAIL; 2 PASS (local prerequisites);
3 PASS (owned/affected checks); 4 FAIL (full end-to-end/native/human evidence open);
5 PASS; 6 PASS; 7 FAIL (representative budget open); 8 PASS; 9 FAIL (full game/package
incomplete); 10 PASS; 11 PASS with documented container variance; 12 PASS (original
authored content, engine dependencies unchanged); 13 N/A (no delegation); 14 PASS;
15 PASS. Applicable failures keep the goal active and block a completion claim.

Self-audit: the previous optional-only result missed the actual Main entry.
This checkpoint fixes that entry and verifies its real consumers and recovery.
The data layer remains internal; it does not prescribe cubic art. Independent
Core/Unity queries agree, and the old tests are retained. Visual inspection still
fails the director's AAA+ target; native, heard and human acceptance remain open.

Frozen Constitution 6.18.0: 63d99cdc72a1c18b5aa91b9336d02fd30126407b, byte-exact
revalidated. Skill evolution: elapsed-time/callback evidence, isolated 922dcbb,
validated and **staged, not propagated**. No second evolution on this continuation.
Rob merged PR #1 and authorized direct main updates; no visibility/licence,
release or unrelated Constitution publication was performed.
