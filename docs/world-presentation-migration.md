# World presentation correction

Director decision, 2026-11-01: modded Minecraft supplies the gather/build/survive/
combat concept. Forgeborne's world must use believable original architecture,
sculpted surfaces and detailed models. The blocky presentation is rejected.
All six influences remain equally important; the AAA+ target remains open.

Current state: 0.124 switched actual Main to shared original authored profile 2;
0.125 adds paired surface materials and a current native QA package. Historical
infrastructure paragraphs below describe earlier states, not today's default.
See [current evidence and remaining visual failures](surface-material-checkpoint.md).

## Verified cause and boundary

The current `VoxelChunk` stores one-metre material cells. `MeshBuilder` emits
exposed square faces, `FpsController` collides with whole occupied cells, and
`AimRay` targets whole cells. `CourtyardEncounter` builds rectangular resource
banks and masonry from those cells. Better shaders cannot remove that shared
geometric assumption. The architecture blueprint already described smooth
surfaces/SDF, but the current runtime never implemented that part.

Replacing the renderer alone would leave invisible corners blocking the suit,
shots hitting air, and gathering selecting surfaces that are not drawn. The
replacement must give rendering, motion, aim, gathering and building the same
surface. The GameManager still owns one Simulation; presentation cannot create
another world, independently mutate resources or advance time.

## Implementation order

Infrastructure follow-up 0.123: the optional continuous material-field profile,
immutable regional triangles/field samples, shared swept movement/aim, saved
profile validation and actual Unity buffer/collider checks are implemented.
See continuous-surface-checkpoint.md for exact evidence and failures. Main still
defaults to legacy. The camera diagnostic confirms that smoothing the old layout
does not produce the required world. Authored gameplay geometry/content binding,
the actual re-authored environment, prospective construction/actor contracts and
native/performance acceptance are the next work; they are not replaced by this
infrastructure checkpoint.

1. Preserve the existing mechanical checkpoint and usable original art separately
   on the local working branch. Do not package the rejected courtyard as a visual
   milestone. Keep existing source tests and old save fixtures unchanged.
2. Implement a deterministic shared continuous surface in Core, starting with the
   terrain/material field. Mesh the field and query that same field for movement,
   contact and aiming. Keep the existing binary API as an explicit compatibility
   path, not the new game's art specification. Arbitrary scene art must not
   silently become a second physics authority.
3. Version the surface contract in saves. Load old binary saves under their known
   rules, validate a new surface profile before swapping the world, and preserve
   exact new-save cold recovery. No implicit conversion or destructive slot write.
4. Re-author the first playable place around continuous ground, fractured mineral
   deposits, believable salvage and constructed machinery. Use authored curved
   industrial forms, human scale, useful cover and routes. The floor must not be
   a visible grid and resources must not look like painted cubes.
5. Give player-built objects actual original structure/machine models and honest
   placement previews. Keep shared price/support/occupancy validation, real-time
   pressure and persistent edits. A model's visible body and interactions need a
   documented surface rather than a hidden full-cell hitbox.
6. Verify the complete encounter in both actual 3D views: gather from the visible
   seam, walk/jump along curved edges, shoot exposed targets and cover, build and
   remove structures, command squads, save/quit/cold load. Add animation, material
   fracture/debris and audio as part of the same result. Then profile a native
   1440p workload and submit actual game evidence for Rob's review.

## Risk, evidence and rollback

This is a geometry/interaction migration, not a palette patch. Risk is highest at
surface boundaries, seams between chunks, fast motion, steep slopes, ceilings,
aim occlusion, partial destruction, construction support and save compatibility.
Independent geometric fixtures must measure visible/contact/trace agreement,
including corners where the old occupied cube would have produced a false hit.
Native observations and performance remain required after source checks pass.

Rob merged PR #1, then explicitly directed "just update main". Verified main
checkpoints are authorized; visibility, licence and release changes are not. Keep
the old renderer/legacy save path available in Git and record the precise commit
before changing the geometry contract. Revert a failed candidate by a targeted
forward change on the branch; never reset unrelated work or rewrite local saves.
Existing user slots are preserved. The new contract must be proven before it
becomes Main's default; no automatic profile conversion on loading a saved game.

Current acceptance: **FAIL**. Original weapon/actor/suit models and renderer
consumer repairs are reusable work, but the live courtyard still has the rejected
cell geometry. No new visual, native, performance or fun approval is implied.
