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
16. [ ] Cable key, added 0.41.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core cable, network, branch, transformer-in-run,
    and conflict rules are headless-tested): press I with
    1 EmberOre to build one Cable cell in the cell before
    the aimed surface, like T / Y / U, one cell per press
    with no drag-to-lay; I is suppressed while the rally
    picker is open, like T / Y / U; a mill fed LV through
    a cable run reads as powered on the HUD power line,
    and a mill on a broken or conflicted cable run reads
    as unpowered. Do not claim this verified in Unity
    until it is observed there and recorded
    in DECISIONS.md.
17. [ ] Mismatched-voltage destruction, added 0.42.0 (no
    adapter code — destruction is a core rule on a tick,
    headless-tested, not a key press): feed a mill HV
    through a transformer or a cable run, and observe
    that the mill disappears from the scene on that tick,
    that its cable, transformer, and Dynamo remain, that
    the HUD power line's mill total drops by one, and
    that a mill fed LV beside it keeps working; note
    explicitly whether the disappearance is visible on
    the destruction tick or only after a mesh refresh,
    because the mesh-refresh gap from 0.30.0 may make a
    counted destruction precede its visible disappearance.
    Do not claim this verified in Unity until it is
    observed there and recorded in DECISIONS.md.
18. [ ] Alternator key, added 0.46.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core HV output, stone price, precedence, and
    step-down rules are headless-tested): press N with
    10 Stone to build an Alternator in the cell before
    the aimed surface, like T / Y / U / I / B; N is
    suppressed while the rally picker is open, like the
    other builders; a mill or workstation fed LV through
    an Alternator plus a transformer reads as powered on
    the existing HUD counts, a mill fed HV directly from
    an Alternator disappears on its tick (item 17), a
    workstation fed HV directly reads as NOT powered,
    and a lone Alternator feeding no consumer shows
    nothing on the HUD, by decision. Do not claim this
    verified in Unity until it is observed there and
    recorded in DECISIONS.md.
19. [ ] Workstation key, added 0.45.0, checklist item
    added 0.49.0 closing Slice 10 review gap 7 (adapter
    code written, NOT yet compiled or run in Unity on
    Windows; the core workstation build, cost, and regen
    rules are headless-tested): press B with 10 Stone
    and 5 EmberOre to build a workstation in the cell
    before the aimed surface, like T / Y / U / I / N; B
    is suppressed while the rally picker is open, like
    the other builders; stand near the built workstation
    and shields restore 5/tick, capped at 85. Do not
    claim this verified in Unity until it is observed
    there and recorded in DECISIONS.md.
20. [ ] Heavy kind key, added 0.49.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core Heavy shape, price, cap, and save rules are
    headless-tested): press O, press 5 — the prompt shows
    HEAVY with the current doctrine; press 1 — the prompt
    shows HEAVY HOLDGROUND, proving doctrine changed
    without changing kind; Enter deploys the shown
    combination for 10 EmberOre; a deployed Heavy squad's
    text line reads its kind, members, health, and damage,
    and its scene marker is the same cyan cube as a
    Vanguard's or Recon's, by the stated Slice 11 Step 1
    marker decision. Do not claim this verified in Unity
    until it is observed there and recorded in
    DECISIONS.md.
21. [ ] Defend Core doctrine key, added 0.50.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core Defend Core movement, chase radius, fire,
    return, and save rules are headless-tested): press O,
    press 6 — the prompt shows DEFENDCORE with the current
    kind; press 3 — the prompt shows VANGUARD DEFENDCORE,
    proving kind changed without changing doctrine; Enter
    deploys the shown combination for 10 EmberOre; a
    deployed Defend Core squad's text line reads its kind,
    doctrine, members, health, damage, range, and activity
    (Guarding at the core, Hunting a core attacker,
    Marching on the return). Do not claim this verified
    in Unity until it is observed there and recorded in
    DECISIONS.md.

22. [ ] Live-power display, added 0.53.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core live-network and lone-source counts are
    headless-tested): wire LivePowerText in the Inspector
    to a HUD line; lay a live cable network feeding no
    consumer and observe the line read 1 LIVE NETWORK, 0
    LONE SOURCES; place a lone Dynamo or Alternator and
    observe the lone-source count rise with no network
    counted; wire the network to a mill and observe the
    live-network count drop to 0 while the mill power
    line reads powered, proving one delivery is not
    counted twice. Note explicitly that the line is a
    count, not a scene tint and not a warning: an HV
    network counted here is live, not safe. This item
    supersedes item 18's statement that a lone Alternator
    feeding no consumer shows nothing on the HUD — that
    was the decided behaviour before this display existed.
    Do not claim this verified in Unity until it is

23. [ ] Shock Pylon key, added 0.54.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core HV rating, fire phase, mismatch-disable, cost,
    and save rules are headless-tested): press P with 5
    Stone and 10 EmberOre to build a pylon in the cell
    before the aimed surface, like T / Y / U / I / B / N;
    P is suppressed while the rally picker is open, like
    the other builders; wire PylonPowerText in the
    Inspector to a HUD line and observe it read 0/1 for
    a lone pylon and 1/1 once an Alternator feeds it HV
    directly, and 0/1 while a Dynamo feeds it LV; place
    an enemy within 5 cells of a powered pylon and
    observe it lose 10 HP per tick, and an enemy 6 cells
    away lose nothing. Do not claim this verified in
    Unity until it is observed there and recorded in
    DECISIONS.md.
    observed there and recorded in DECISIONS.md.

24. [ ] Pre-destruction / pre-disablement warning, added
    0.57.0 (adapter code written, NOT yet compiled or run
    in Unity on Windows; the core warned counts are
    headless-tested): wire WarningText in the Inspector to
    a HUD line and observe it stay empty with no mismatched
    machine; wire a workstation to HV and observe the line
    read 0 MILLS WILL BE DESTROYED, 1 WORKSTATIONS WILL BE
    DISABLED, 0 PYLONS WILL BE DISABLED, persisting until
    the workstation is rewired to LV, when the line clears;
    wire a pylon to LV and observe the same persistence and
    clearing on rewiring to HV. For a mill, observe honestly
    and record what actually happened: the headless proof is
    a snapshot taken between placement and tick, but in
    Unity a tick runs every frame, so a mill wired to HV is
    expected to show its warning for at most one frame
    before destruction — record whether that flash was
    seen, and do NOT claim the mill warning is usable in
    live play until tick pacing is decided; that pacing
    question is carried, not answered, by this step, and a
    warning must never be quoted as a grace period. Note
    explicitly that the line is a HUD readout, not a scene
    tint and not a mesh refresh: a warned machine is not
    thereby visible in the scene. Do not claim this
    verified in Unity until it is observed there and
    recorded in DECISIONS.md.

25. [ ] Squad-kind scene markers, added 0.58.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    NO headless test proves a colour or a shape reads in
    the scene — the cited headless evidence is only that
    OrbitalView.Snapshot carries Kind for every live
    squad, proved by the existing deploy, picker, save,
    and view snapshot tests): deploy one squad of each
    kind (the cap is 3 live squads, so all three fit at
    once) and observe three distinct markers — a cyan
    cube 0.6 units for Vanguard, a magenta capsule 0.5
    wide x 0.8 tall for Recon, a light-grey cylinder 0.8
    wide x 0.7 tall for Heavy — each at its squad's cell
    centre, and each squad's text line reading the same
    kind as its marker, word for word; dismiss one squad
    and deploy a different kind in its place and observe
    that slot's marker change shape and colour, proving
    slot reuse rather than a stale marker; open the
    picker and observe the rally marker still an orange
    flat cube and the pending marker still an orange
    sphere, unchanged; walk behind a wall so a marker is
    on the far side and observe it hidden, proving
    occlusion is still honest. This item supersedes item
    20's statement that a Heavy squad's scene marker is
    the same cyan cube as a Vanguard's or Recon's — that
    was the decided behaviour before this distinction
    existed. Do not claim this verified in Unity until it
    is observed there and recorded in DECISIONS.md.

26. [ ] Mesh tint for power state, added 0.59.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core power-state query, colour channel, merge
    separation, and tint/warning agreement are
    headless-tested): wire a vertex-colour material to the
    chunk mesh — record explicitly which material the scene
    uses and whether it multiplies vertex colours, because
    with a material that ignores vertex colours the mesh
    renders untinted and that is a material finding, not a
    tint-code finding. With a vertex-colour material, build
    a powered mill (Dynamo beside it) and observe its faces
    green; build a mismatched workstation (Alternator beside
    it) and observe its faces red, and the HUD warning line
    (item 24) reading the same workstation at the same time;
    lay a Dynamo-fed cable and observe its faces yellow;
    leave a mill unpowered and a mill conflicted (Dynamo and
    Alternator both touching it) and observe both untinted,
    white, exactly as ordinary stone. Rewire the mismatched
    workstation to LV and observe the red tint clear on the
    next refresh, together with the warning line clearing.
    Observe explicitly that no tint reads as a marker: green
    is not the cyan squad cube, red is not the orange rally
    flat cube or pending sphere, and yellow sits on ground
    cable cells, never as a floating marker. Note honestly
    that a converting transformer and a lone source render
    untinted, by decision — if either reads as dead in play,
    record that as evidence to revisit, not as a bug in the
    query. This item supersedes item 7 of
    `docs/powered-presence.md` (a Dynamo and a mill render
    as ordinary blocks with no distinct tint) — that was
    the decided behaviour before this tint existed. Do not
    claim this verified in Unity until it is observed there
    and recorded in DECISIONS.md.

27. [ ] Mesh refresh event set, added 0.59.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core query is headless-tested, the refresh wiring
    is not): wire FpsInputAdapter.MeshRenderer to the same
    VoxelMeshRenderer GameManager links — record explicitly
    that the wiring is done, because an unwired field makes
    every refresh call a no-op and a stale mesh is then a
    wiring failure, not a refresh-code failure. Press T, Y,
    U, I, B, N, and P in turn and observe each built block
    appear in the scene at once, with no save/load and no
    scene reload; press a builder key with insufficient
    resources and observe no refresh is needed, because
    nothing changed. Press E to gather a block and observe
    it disappear at once. Then answer the item-17 question
    this step was built to settle: feed a mill HV and
    observe whether the mill disappears from the scene on
    its destruction tick, now that a destruction refreshes
    the mesh — record whether the disappearance is on the
    tick, and if it is not, record what actually happened
    rather than claiming the hook worked. Note explicitly
    that no refresh runs every frame: the mesh refreshes
    on a successful key, a gather, a destruction, a load,
    or Start, and on nothing else. Do not claim this
    verified in Unity until it is observed there and
    recorded in DECISIONS.md.

28. [ ] Conduit key, added 0.62.0 (adapter code written,
    NOT yet compiled or run in Unity on Windows; the core
    cable-only HV delivery, cost, mismatch proofs, and
    save/load re-derivation are headless-tested): press M
    with 5 Stone and 5 EmberOre to build a Conduit in the
    cell before the aimed surface, like T / Y / U / I / B /
    N / P; M is suppressed while the rally picker is open,
    like the other builders. Place a Conduit beside a mill
    and observe the mill keep running at kinetic cadence
    with no warning and no tint change, proving direct
    adjacency delivers nothing; lay one cable from the
    Conduit to the mill and observe the mill destroyed on
    its next tick and the HUD warning line reading the
    mill before destruction, proving cable delivery carries
    HV. Observe honestly the single-cable tint finding
    carried from the headless step: a single cable cell
    touching a Conduit is expected to read untinted in the
    scene while its network is live, because the headless
    PowerState query reads None for it — record whether
    that untinted cell is what is actually seen, and do
    NOT claim the tint agrees with the network until a
    two-cable Conduit run is also observed with both cable
    cells tinted yellow. Do not claim this verified in
    Unity until it is observed there and recorded in
    DECISIONS.md.

29. [ ] Patrol key, added 0.65.0 (adapter code written,
    NOT yet compiled or run in Unity on Windows; the core
    rally loop, skip-on-block, fire-in-passing, and
    save/load reset are headless-tested): press O, press
    7 — the prompt shows PATROL with the current kind;
    press 3 — the prompt shows VANGUARD PATROL, proving
    kind changed without changing doctrine; Enter deploys
    the shown combination for 10 EmberOre; the deployed
    squad marches to the picked rally, then keeps walking
    east, south, west, and north around it instead of
    holding there, and its text line reads Patrolling
    throughout, never Hunting, even while it shoots an
    enemy in range in passing. Observe honestly that a
    blocked waypoint is skipped, not waited on: put a
    wall on the squad's east step and watch it turn
    south on the next circuit leg rather than stand at
    the wall. Do not claim this verified in Unity until
    it is observed there and recorded in DECISIONS.md.

30. [ ] Chunk material assignment, added 0.68.0 (adapter code
    written, NOT yet compiled or run in Unity on Windows;
    the core colour channel is headless-tested, the
    material assignment is not): press Play and record
    explicitly which material the chunk MeshRenderer
    carries — an Inspector-wired ChunkMaterial, or the
    runtime fallback, and in the fallback case which
    shader was found (Universal Render Pipeline/Lit
    expected, Standard fallback, or neither, with the
    warning logged). Observe that ordinary stone, ore,
    and wood render exactly as before, with no pink /
    missing material on the chunk mesh, and that a
    deliberately Inspector-wired material is NOT
    replaced by the runtime fallback at Play. This item
    does NOT settle whether tints show — that is item
    26, observed beside it, not duplicated by it: a
    material can be assigned correctly and still
    ignore vertex colours, and that is a material
    finding to record, not a tint-code finding. If
    neither shader is found, record the warning and
    the Inspector material the scene fell back to.
    Do not claim this verified in Unity until it is
    observed there and recorded in DECISIONS.md.

31. [ ] MeshRenderer wiring null-fill, added 0.68.0 (adapter
    code written, NOT yet compiled or run in Unity on
    Windows; the core refresh event set is
    headless-tested, the wiring is not): first, with
    FpsInputAdapter.MeshRenderer left unwired in the
    Inspector but GameManager.MeshRenderer and
    GameManager.FpsAdapter wired, press Play and
    observe that a successful build, gather, and mill
    destruction each appear in the scene on their
    tick, proving the Start null-fill linked the
    adapter to the same renderer GameManager links,
    with no warning logged. Second, with
    FpsInputAdapter.MeshRenderer wired to a second,
    deliberate renderer, press Play and observe that
    the adapter refreshes that renderer, not the
    GameManager one, proving a non-null field is
    never overwritten. Third, with neither the
    adapter field nor GameManager.MeshRenderer wired,
    press Play and observe the wiring warning logged,
    proving an unwired scene announces itself rather
    than silently rendering a stale first mesh. This
    item does NOT close item 27 — item 27 remains the
    observation that each builder key, gather, and
    destruction appears on its tick in a normally
    wired scene; this item observes the fill and its
    non-overwrite rule, not the refresh event set
    itself. Do not claim this verified in Unity until
    it is observed there and recorded in DECISIONS.md.

32. [ ] Marker occlusion under the Step 1 material, added
    0.69.0 (adapter code unchanged except a comment
    reaffirming the decision, NOT yet compiled or run in
    Unity on Windows; no headless test proves a marker
    hidden or visible in the scene — the cited headless
    evidence is only that markers are ordinary opaque
    primitives with their own URP Lit / Standard
    materials, separate from the Step 1 chunk material):
    build one combined scene and observe it whole, not
    one marker at a time — one squad of each kind (cyan
    cube Vanguard, magenta capsule Recon, light-grey
    cylinder Heavy), each rally's orange flat cube, the
    orange pending sphere while picking, a Powered mill
    with green faces, a Mismatched workstation with red
    faces, and a yellow Energised cable run, all with
    the Step 1 chunk material as the occluding mesh.
    First, walk behind a wall so each kind marker in
    turn, then a rally marker, then the pending marker,
    is on the far side, and observe each hidden while
    occluded and reappearing when line of sight returns,
    proving honest depth occlusion stands under the new
    material. Second, at three FPS distance bands —
    near (2-3 cells), mid (about 10 cells), and far
    (bench-to-core scale, about 24 cells Manhattan) —
    observe each marker still reading as itself against
    dark voxel and the scene's current sky, and record
    the band at which any marker stops reading, if any
    does. Third, in that same combined view, observe
    the separations together: green Powered faces are
    not the cyan squad cube, red Mismatched faces are
    not the orange rally cube or pending sphere, yellow
    cable sits on ground cells and never as a floating
    marker, and the light-grey Heavy cylinder is not an
    untinted white voxel — that last separation is by
    shape and centre position, not colour alone, and
    must be observed as such. This item does NOT
    observe the Step 3 dusk light, which is not yet
    built: Step 3's own checklist item re-observes this
    same combined scene under that light, and until
    then no light claim is made here. This item does
    NOT supersede item 25 (kind distinction and slot
    reuse) or item 8 of `docs/orbital-view.md`
    (baseline occlusion): it extends the occlusion
    question to the Step 1 material as occluder plus
    combined tint-and-marker readability, and those
    items stand alongside it. If honest occlusion
    proves markers unreadable at FPS distance, record
    that as evidence for a later step's stated aid
    (size, brightness, or outline change that still
    hides behind a wall); do NOT silently change a
    marker shader or render queue in this observation.
    Do not claim this verified in Unity until it is
    observed there and recorded in DECISIONS.md.

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
