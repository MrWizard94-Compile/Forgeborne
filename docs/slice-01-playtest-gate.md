# Forgeborne — Slice 1 Playtest Gate

**Status: HEADLESS GATE MET, DIRECTOR PLAYTEST STILL PENDING.**
Do not claim Slice 1 is approved until the director playtest
section below is marked pass by Rob. This document is evidence,
not approval.

## Gate question

Is moving, gathering, building, and surviving fun WITHOUT extra
systems? If the answer is no, Slice 1 fails — adding loot, graphs,
or automation will not fix it.

## Headless evidence (verified 2026-10-29, tests 100/100)

| Slice 1 requirement | Headless evidence | Status |
|---|---|---|
| One small voxel zone, FPS | 32^3 chunk, FPS controller (speed 6, jump 8, gravity -20, AABB collision, wall sliding) | Met headlessly |
| Gather 2+ resources, rarity/risk | Stone (common), EmberOre (~4%, rarer), Wood (sparse surface) — 1 block = 1 unit, full inventory does not consume block, bedrock ungatherable | Met, tested |
| Build structure + workstation, real cost/effect | Wall = 2 Stone, blocks enemy movement. Workstation = 10 Stone + 5 EmberOre, restores 5 shields/tick within Manhattan distance 3, capped at 85. Wall and workstation effects are distinct and tested. | Met, tested (workstation effect added 0.4.1) |
| Survive 1 enemy type + pressure event, real time | Enemy: 30 HP, 10 dmg, moves 1 voxel/tick toward player, attacks if adjacent. Wave spawn at zone edge. Headless demo: player died standing still after tick 20 — pressure is real, movement/building are the counterplay. | Met headlessly |
| Canonical state separated from rendering | Forgeborne.Core is pure C#, no UnityEngine. Unity scripts are thin adapters only. | Met |
| Local saves, reopen, reproduce | JSON save/load round-trip tested, including enemies and health. Save JSON 43,852 chars in demo. Export/import is the JSON file itself. | Met headlessly |
| Tests: gather cost, build cost, damage, death, save/load | 100/100 tests, including full inventory, insufficient resources, floating build, invalid terrain, death, save during combat (save is a snapshot of zone state, taken at any tick) | Met |
| Performance measured | Mesher: generated chunk 187 quads (vs 2,608 naive faces) in 13 ms in tests, 198 quads in 33–74 ms in demo runs. Hundreds of voxels, not 50,000 entities — that is NOT a Slice 1 gate. | Met for Slice 1 scope |

## Headless demo (2026-10-29, 60 ticks, seed 42)

Gathered 1, built 1, killed 2, player died to enemy pressure
(standing still), mesher 198 quads / 792 vertices, save 43,852 chars.
Run it: `dotnet run --project demo/Forgeborne.Demo`

## Unity verification checklist — to be done on Windows, Unity 6000

This is the part that CANNOT be verified headlessly in this
workspace, and it is not claimed as done. Do these steps in
Unity 6000 on Windows, in `ForgeborneUnity/`:

1. [ ] Open project, no compile errors in VoxelMeshRenderer,
   FpsInputAdapter, HudDisplay, GameManager.
2. [ ] Create a scene: GameManager (Seed 42), VoxelMeshRenderer
   (linked to GameManager), FPS camera with FpsInputAdapter,
   Canvas with HudDisplay Text fields wired.
3. [ ] Press Play: voxel chunk renders, no pink/missing material
   on the chunk mesh, FPS camera moves with WASD + mouse.
4. [ ] Press E near ground: gather succeeds, inventory increases.
5. [ ] Press Q with 2+ Stone: wall builds in front-right, blocks
   enemy movement.
6. [ ] Build workstation (10 Stone + 5 EmberOre), stand near it:
   shields restore 5/tick, capped at 85.
7. [ ] Survive a wave: enemies approach, attack, can be shot
   (click, 3 shots = 1 kill at 10 dmg vs 30 HP).
8. [ ] Objective: core at (28,5,28), 100 HP, DamageCore reduces it,
   HUD shows distance and % complete.
9. [ ] Save to JSON, close scene, reopen, load: same zone state.
10. [ ] Frame time: record FPS with the chunk + wave active.
    Target for Slice 1: playable on GTX 1660 Ti-class hardware
    at 1440p, 60 FPS is the blueprint target, not a Slice 1 fail
    threshold if the loop is otherwise playable — record the
    actual number, do not claim it.
11. [ ] FPS pitch, added 0.28.0 (adapter code written, NOT yet
    compiled or run in Unity on Windows; core Look math is
    headless-tested): mouse up looks up, mouse down looks
    down, the camera never flips past vertical, and looking
    down at flat ground lets the orbital picker pick that
    ground cell — full detail in checklist item 9 of
    `docs/orbital-command.md`. Do not claim this verified
    in Unity until that item is completed there and recorded
    in DECISIONS.md.
12. [ ] Powered Unity presence, added 0.30.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core power-indicator counts are headless-tested):
    press T to build a mill and Y to build a Dynamo, the
    HUD power line reads 0/1 then 1/1 for the pair, both
    keys are suppressed while the rally picker is open,
    and mesh visibility / tint are recorded as observed —
    full detail in the checklist in
    `docs/powered-presence.md`. Do not claim this verified
    in Unity until that checklist is completed there and
    recorded in DECISIONS.md.
13. [ ] Yaw-rotated movement, added 0.36.0 (adapter logic
    unchanged — the adapter still passes raw WASD scalars
    and Simulation.Tick rotates them in core, headless-
    tested): turn with the mouse to face +X, then W walks
    +X and D walks -Z; looking up or down does not lift
    or dive the player; a diagonal still feels faster
    than a straight walk, as in Slice 1. Do not claim
    this verified in Unity until it is observed there
    and recorded in DECISIONS.md.
14. [ ] Aim-based gather / build / fire, added 0.37.0
    (adapter bindings unchanged — E / Q / T / Y and mouse
    click call the same methods, which now read the core
    Look ray, headless-tested): look down at the ground
    and press E to gather the ground block under the
    crosshair; look at the ground and press Q / T / Y to
    build a wall / mill / Dynamo in the cell before the
    aimed surface; click with the crosshair on an enemy
    within 10 m to hit it, and on empty sky to waste the
    round; a wall between crosshair and enemy blocks the
    shot. Do not claim this verified in Unity until it
    is observed there and recorded in DECISIONS.md.
15. [ ] Transformer key, added 0.40.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core ladder, conversion, and rated-voltage rules
    are headless-tested): press U with 5 EmberOre to build
    a Transformer in the cell before the aimed surface,
    like T and Y; U is suppressed while the rally picker
    is open, like T and Y; the HUD power line still counts
    only mills receiving rated LV, so a mill fed HV
    through a transformer reads as unpowered (kinetic
    cadence) until Step 3. Do not claim this verified in
    Unity until it is observed there and recorded in
    DECISIONS.md.

Record the result of each step, with the Unity version and GPU,
in DECISIONS.md, and only then mark the director playtest
section as pass or fail.

## Director playtest — PENDING

- [ ] Rob played Slice 1 in Unity on Windows.
- [ ] Rob answered the gate question (fun without extra systems?).
- [ ] Result recorded in DECISIONS.md, with date and verdict.

## Known gaps, honestly stated

- No Unity scene file is shipped yet — scene creation is step 2
  above, to be done in the Unity editor, not hand-written YAML
  that could break the project.
- Mini-map in HudState is 32x32 solid data, not a rendered
  mini-map in Unity yet — placeholder in Slice 1 HUD.
- Slice 2 loot (LootGenerator) was integrated into Simulation
  in 0.6.0 (see `docs/slice-02-review.md`); that integration
  does not change the Slice 1 gate, which is about the loop
  without extra systems.
