# Forgeborne — Slice 1 Plan

## Goal

Prove that gather → build → survive is fun in one small 3D voxel
zone, before any other system is added.

Slice 1 is a foundation milestone, not a game release. It ships only
when its definition of done is met and the director approves it.

## Scope (only this)

1. One small voxel zone, PC native, FPS.
2. FPS movement, looking, and interaction in that zone.
3. Gather: at least two resource types, with different rarity / risk
   in the zone, gathered in real time.
4. Build: place at least one structure type and one workstation type
   from gathered resources, with a real cost and a real effect.
5. Survive: at least one enemy type and one pressure event that tests
   what was gathered and built, in real time, with no pauses for
   planning.
6. Canonical state: ECS / SVO (or engine-equivalent deterministic
   state) as source of truth, separated from rendering.
7. Local saves: save, close, reopen, and reproduce the same zone state.
   Export / import a save file. No silent save loss.
8. Tests: automated tests for gather cost, build cost, damage, death,
   and save / load round-trip, plus manual playtest evidence.

## Explicitly NOT in Slice 1

- Passive graph (beyond, at most, a placeholder data structure with
  no gameplay effect — preferably none at all).
- Gem composition, procedural weapon families, or elemental matrix.
- Automation tiers beyond the single workstation in scope above.
- RTS squads, Orbital Tactical command, or doctrine AI.
- Networking, multiplayer, or co-op.
- Neural world models or VM agents.
- Multiple zones, biomes, bosses, or endgame systems.
- Mobile / touch controls. PC native first.

## Definition of done

- Player can, in one continuous session: move in FPS, gather both
  resources, build both structure and workstation, survive the
  pressure event, die or complete the zone, save, reopen, and see the
  same state.
- All tests in scope pass. Edge cases named: gathering with full
  inventory, building with insufficient resources, building on
  invalid terrain, death during building, save during combat.
- Performance measured, not claimed: frame time recorded in the zone
  with hundreds of entities / voxels active. 50,000 entities is NOT
  a Slice 1 gate.
- No placeholders in the Slice 1 loop. A feature that is not complete
  is cut from Slice 1, not shipped as a demo.
- CHANGELOG updated, docs updated, director playtest approval recorded
  in `DECISIONS.md`.

## Playtest gate question

Is moving, gathering, building, and surviving fun WITHOUT extra
systems? If the answer is no, Slice 1 fails — adding loot, graphs,
or automation will not fix it.

## Next slices (after Slice 1 approval only)

- Slice 2: one combat loop + one weapon family + one small skill cluster.
- Slice 3: one automation tier.
- Slice 4: doctrine-based squad AI and Orbital Tactical command.
- Further slices only by director approval, one system at a time.
