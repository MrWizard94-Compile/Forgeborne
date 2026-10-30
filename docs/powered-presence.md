# Forgeborne — Powered Unity Presence (Slice 6 Step 3)

**Status: ADAPTER CODE WRITTEN 2026-10-30, NOT compiled or run
in Unity on Windows.** The core power-indicator numbers are
headless-tested (709 tests); the keys, the HUD line, and the
mesh behaviour below are Windows-checklist only, exactly as
Steps 1–2 of this slice. Do not claim any of it verified in
Unity until the checklist at the end is completed there and
recorded in DECISIONS.md.

## What this step adds, and what it does not

Slice 5 proved a powered loop headlessly and touched no
adapter, so a loop that works had never been built by hand
or seen in the game itself. This step gives it a hand in
Unity: two build keys and one HUD indicator line. It adds
no core build rule, no power rule, and no save field — the
builders (`Simulation.BuildKineticMillAhead`,
`Simulation.BuildDynamoAhead`, with their front-left and
front-centre cells, costs, and support rules) and the power
rule (`WorldZone.IsPowered`, face-adjacency only) already
existed and are unchanged. Dynamo cost stays 5 EmberOre,
distribution stays face-adjacency with no cables or
chaining, powered cadence stays every tick replacing (not
stacking with) the kinetic cadence, and kinetic mill
behaviour, squad rules, loot, skills, core health, and the
save format are all unchanged.

## Build keys (FpsInputAdapter)

| Key | Action | Core builder | Cost |
|-----|--------|--------------|------|
| T | Build Kinetic Mill, front-left at feet level | `BuildKineticMillAhead` | 10 Stone + 5 Wood |
| Y | Build Dynamo, front-centre at feet level | `BuildDynamoAhead` | 5 EmberOre |

T and Y were chosen because they are free — existing keys
are WASD / Space / E (gather) / Q (wall) / R (reload) / O
(picker) / Return / Esc / 1 / 2 / mouse click — and because
they sit together as the build pair: a mill built with T
and a Dynamo built with Y, with no move between them, are
face-adjacent (front-left vs front-centre), which is
exactly what makes the mill powered. Build the pair in
either order; adjacency is symmetric.

Both keys are suppressed while the rally picker is open,
like mouse fire. The picker is modal for squad deployment,
and a Dynamo build while picking would spend 5 EmberOre —
the resource a pending Confirm is about to charge 10 of —
so an untimed Dynamo key could make the deploy the player
is aiming fail for no visible reason. The mill is
suppressed with it so the pair behaves as one modal rule.
E / Q / R keep their pre-existing unsuppressed behaviour
in this step: they spend no EmberOre, so they cannot break
a pending Confirm's affordability, and changing them would
be a second modal change bundled into a powered-presence
step. A failed key build spends nothing, per the existing
core rule (check before spending), and a failed build
shows no new message in this step — the HUD inventory
numbers are the feedback, as they are for Q today.

## Power indicator (HudDisplay, from HudState)

Wire the new `PowerText` field in the Inspector to a HUD
line. It renders one line from `Simulation.Hud()` every
frame:

```
POWER: {powered}/{total} MILLS POWERED
```

`PoweredMills` / `TotalMills` are derived in core
(`HudState.FromWorld`): every Kinetic Mill cell in the
32^3 chunk is counted, and each is asked
`WorldZone.IsPowered`, so the line reads 0/0 with no mills,
0/1 for a lone mill, 1/1 for a powered pair, and 1/2 for
two mills of which one is powered. A Dynamo on its own, or
a wall next to a Dynamo, counts as neither — only mills
are consumers in this loop. The counts are derived, not
saved: removing a Dynamo drops the count on the next
snapshot, and a loaded game counts its surviving pair
again, because the Dynamo block itself survives as a chunk
byte. The adapter holds no power rule of its own; if the
line and the mill's behaviour ever disagree, the core
count is canonical and the adapter is wrong.

## Mesher check: no distinct Dynamo / mill tint in this step

Checked, and recorded, as the scope required: the mesher
does NOT already render a Dynamo block distinctly from
stone or mills. `MeshBuilder` assigns every quad the same
unit UVs (0..1 across the quad) with no per-block colour,
texture atlas, or material split, and `VoxelMeshRenderer`
assigns the resulting buffers to a single Unity Mesh, so a
Dynamo and a mill render as ordinary voxel blocks. A
distinct tint would therefore need per-block UV or
material work plus Unity material wiring — not a small
addition to a keys-and-indicator step, and not one that
can be proved headlessly. So no tint is added here, and
none is claimed: the text indicator above is the presence
proof, per the scope ("a text indicator that exists is
worth more than a tint that is promised"). A distinct
Dynamo / mill mesh tint is carried as a named gap for a
later presentation step, not quietly dropped.

## Mesh refresh gap (pre-existing, named not fixed)

No adapter refreshes the voxel mesh on a key build today:
`GameManager` refreshes the mesh on scene start and on
load only, and the E gather / Q wall keys have always
built core state without a refresh hook. T / Y follow the
same path — they change core state immediately, and the
HUD power line (which updates every frame from `Hud()`)
shows the result even if the mesh has not yet re-rendered.
Whether a given Unity scene re-meshes per frame, on
build, or only on load is therefore a checklist question
(checklist item 6), not an assumption: if Windows testing
shows key builds are invisible in the mesh until load, a
refresh-on-build hook is the follow-up, and it should be
added once for all builders (E / Q / T / Y), not wired
T/Y-only in a way that makes the new keys behave
differently from the old ones.

## Unity verification checklist — to be done on Windows, Unity 6000

1. [ ] Project opens with no compile errors in the edited
   `FpsInputAdapter.cs` and `HudDisplay.cs`, with
   `PowerText` wired to a HUD line.
2. [ ] With 10+ Stone and 5+ Wood, press T: a mill builds
   front-left, the HUD power line reads 0/1, and the
   inventory drops by exactly 10 Stone and 5 Wood.
3. [ ] With the mill standing and 5+ EmberOre, press Y
   with no move between the builds: a Dynamo builds
   front-centre, the HUD power line reads 1/1, and the
   inventory drops by exactly 5 EmberOre.
4. [ ] With the powered pair standing and a stone pocket
   seeded beside the mill, watch 5 ticks: the mill
   excavates about one block per tick (powered cadence),
   not one per 5 ticks — the behaviour Slice 5 proved
   headlessly, now seen by hand.
5. [ ] While the picker is open (press O), press T and Y:
   neither builds, no resources are spent, and the
   pending pick is unaffected; Esc then Enter behave as
   before. A Dynamo key during picking must not be able
   to starve a pending Confirm of its 10 EmberOre.
6. [ ] Mesh visibility: confirm whether the T / Y builds
   appear in the rendered chunk immediately, on the next
   refresh, or only after save/load — and record which.
   If they are invisible until load, record the
   refresh-on-build follow-up (see the gap above) rather
   than claiming the builds render.
7. [ ] Mesh tint observation: confirm a Dynamo block and
   a mill block render as ordinary voxel blocks (no
   distinct tint), as this step states — the tint gap is
   confirmed present, not accidentally fixed or claimed.

Record the result of each step, with Unity version and
GPU, in DECISIONS.md.
