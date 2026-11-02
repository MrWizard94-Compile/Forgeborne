# Foundry v2 — physical art correction

Continuation of Rob's rejected block/faceted presentation. Baseline is verified
main f60c1827d26af9ad06941f8b4ac01dd836adbc81. This is implementation work, not a
new visual approval or removal of any of the six equally important influences.
Constitution 6.18.0 remains frozen at 63d99cdc72a1c18b5aa91b9336d02fd30126407b;
the invocation's single evolution candidate remains staged, not propagated.

1. Retain the exact v1 payload, generator and Blender source. Add an explicit
   v2 profile and bounded embedded catalog resolution. Save profile, content ID
   and digest must agree before live replacement; old profiles retain behavior.
2. Author v2 fractured mineral masses and low-profile ore veins. Closed pieces
   bind to resource owners; gather removes only the selected owner's components.
   Vary the whole formation rather than repeating one flattened sphere per cell.
   Use inclined 3D Voronoi fracture boundaries, not rectangular or upright cell cuts. V2 deposits
   may extend up to .45 m horizontally beyond their owner; v1 and all construction
   templates keep their original cell bounds. Queries use the exact bound meshes.
3. Add a closed vaulted roof and structural supports to make a coherent hall.
   Keep routes, useful cover, real sightlines and the existing gather/build loop.
   Stay within the existing 150,000-triangle / 24 MB content contract.
4. Resolve templates, planned actor steps, aim, building, materials and previews
   using the current profile. Invalidate preview caches across catalog changes.
   One GameManager-owned Simulation and one frame driver remain mandatory.
5. Verify retained v1 bytes, cross-profile/cold/exact recovery, content mismatch
   refusal, gather/build/rays, actual Unity meshes/colliders and visual inspection.
   Repair geometry/material defects before packaging or pushing the checkpoint.

Rollback is a source revert or loading an unchanged retained-profile save, never
editing/deleting a user slot or overwriting an accepted content identity. Do not
implicitly convert saves. No private saves, builds or external assets enter Git.
Rob already authorized verified direct main updates. Full native/performance/
animation/audio/fun and AAA+ acceptance remain independent open gates.

## Contact lock correction before publication

Rob reported complete movement lock near obstacles. A fresh source probe
reproduced all-four-directions zero-distance contact in both retained v1 and v2,
including the vessel collar at (6.0127454, 3.9999995, 16.5) and v2 rock slopes.
Sub-micrometre contact rounding and unscaled triangle SAT projections are the
current lead, not permission to disable physical surfaces or shrink the body.

Add failing production-controller regressions for approach, release, tangent
movement, slope recovery and retained saves. Repair contact classification and
maintain a small bounded separation at resolved contacts, preserving solid
obstruction, grounded/jump, step and sweep behavior. Repeat exact source/Unity
consumer checks, then package the corrected runtime; prior native screenshots
remain evidence of the earlier build, not evidence of this movement correction.
