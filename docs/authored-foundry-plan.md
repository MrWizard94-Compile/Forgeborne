# Authored playable foundry migration

Continuation of the director's no-block presentation correction. Baseline is
the director-merged 9e6ef62 on main (PR #1). Work continues on
codex/authored-foundry during implementation. Rob subsequently directed "just
update main": commit/push verified checkpoints on main, with no extra PR gate.
Constitution 6.18.0 remains frozen at 63d99cdc72a1c18b5aa91b9336d02fd30126407b.

Historical implementation plan: 0.124 completed the default-entry/shared-geometry
checkpoint; 0.125 adds original surface materials and limited native QA evidence.
See surface-material-checkpoint.md. The visual target remains failed, not accepted.

The next implementation binds original closed mesh pieces to the existing
material ledger. Static architecture, gatherable deposits and constructed
machines use those same triangles for rendering, rays and swept movement.
Each closed component has a validated outward orientation and a material role.
Containment is the union of components, including overlapping pieces; it must
not use the contour field's uniform-region lookup. No additional Simulation or
time driver is permitted.

Implement in this order: validated immutable content contract and embedded
original geometry; saved profile/content digest and rejection before world
swap; source tests for independent shape oracles, edits and recovery; Unity
consumer/material/collider/lifecycle checks; actual camera inspection and repair.
Authored geometry must precede the default-scene switch. Actor motion/hit shapes,
native controls, heard audio, representative performance and the complete art
review remain explicit acceptance dependencies, not implied by this checkpoint.

The original design is a damaged industrial exchange hall with human-scale
service bays, curved structural ribs, salvage, fractured mineral seams and
visible machinery. Minecraft supplies the gameplay loop, never the silhouette.
All six influences and the full AAA+ objective remain in scope.

After the first optional-only consumer proof, Rob's "Still seeing blocks bro"
corrected the implementation priority. Main now must enter the authored world,
with its existing GameManager/adapters. Static collision must also stop planned
enemy/squad steps and construction. Old saves remain exact compatibility paths.

Keep old saves under profiles 0/1. New content is identified by exact SHA-256;
unknown/missing/mismatched content fails without altering live state or slots.
Do not silently convert a save. Recovery is a targeted forward correction or
loading an unchanged old save; no history rewrite or save deletion. Actual
Main entry, construction ghost/materials, geometry, recovery and console must
be checked before committing the switch. This does not pass the AAA+ art gate.
