# Carbine and glove refinement

Director correction, 2026-11-02: Rob questioned the arm/hand appearance during
native review. Hold publication of the initial 0.127 candidate. Correct the
overlong support forearm, tubular sleeve silhouette, bulky palms, finger lengths,
material readability and reload grip alignment, then replace final evidence only
after inspecting the actual revised consumers. Preserve the rejected candidate's
native evidence separately; successful rig checks did not pass its visual gate.

Continuation of the full six-influence AAA+ objective and Rob's request for
realistic, organic meshes and direct BioShock visual study. Baseline main:
1f40dac4f61dae18f1cf8a96c81508167d7b6d5f. Work in
codex/carbine-glove-refinement; integrate a verified contribution into main under
Rob's existing direct-update authority. No release/licence/visibility changes.

1. Replace the current primitive hand assemblies with continuous original glove
   surfaces, properly placed curled fingers, opposed thumbs and connected tapered
   forearms. Add an original deformation rig with bounded normalized skin weights.
2. Refine the carbine's silhouette, grip, trigger clearance, receiver, open barrel
   shroud, service lines, latches, recesses and actual fasteners. Keep the existing
   muzzle/axis/metre contract. No copied BioShock shapes, signs or assets.
3. Author original glove material detail through the established paired-map
   pipeline, preserving existing surface map bytes. Keep new view geometry under
   100,000 triangles and four bone influences per vertex. Measure actual import
   counts, materials, bone consumers and frame/resource costs where practical.
4. Preserve GameManager's one Seed-42 Simulation, sole input driver, Both legacy
   input, ammo/aim/damage/collision and retained v1/v2 physical content. The change
   is presentation; no new save geometry catalog is needed for view-only art.
5. Integrate the rig into recoil/reload presentation, retaining exact rest reset,
   paused state and orbital visibility. Inspect actual FPS, close-ups and motion
   samples, plus current native packaging/controls. Tests/counts cannot grant the
   director's visual or fun acceptance.

Rollback: revert this isolated contribution. Preserve prior assets in Git history,
existing save slots and earlier evidence. Do not restore private save backups over
user changes. Freeze no new physical-world identity in this visual-only pass.

Full game remains active. Constitution 6.18.0 is still frozen at
63d99cdc72a1c18b5aa91b9336d02fd30126407b for this continuing invocation. Its single
elapsed-time/callback evolution remains validated 922dcbb, staged, not propagated.
