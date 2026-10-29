# Forgeborne — Orbital view

**Status: HEADLESS SNAPSHOT TESTED. UNITY ADAPTER CODE WRITTEN,
NOT YET COMPILED OR RUN IN UNITY ON WINDOWS.**
Do not claim the Orbital view is verified in Unity until the
checklist below is completed there and recorded in DECISIONS.md.

## What the view is

`OrbitalView.Snapshot(world)` in `src/Forgeborne.Core/Orbital.cs`
returns a copied, read-only list of one `SquadSnapshot` per live
squad, in deploy order. Each snapshot carries: position, rally,
health, living members, damage per tick, at-rally / moving state,
doctrine, and kind. It is a copy, not a live reference: taking a
snapshot, then advancing the world, leaves the old snapshot at
its pre-tick position. That is deliberate — rendering must never
mutate core state, and a frame's display must not change mid-frame
because a tick ran.

`ForgeborneUnity/Assets/Scripts/OrbitalSquadDisplay.cs` is a thin
adapter only: it renders the snapshot as one text line per squad
(`VANGUARD HOLDGROUND - 3 MEMBERS 90 HP 30 DMG - HOLDING AT
(16,5,16) -> RALLY (16,5,16)`), or `ORBITAL: NO SQUADS (press O
to deploy)` when none are deployed. It contains no squad logic.

## Wiring (Unity 6000, Windows)

1. Add `OrbitalSquadDisplay` to a GameObject in the scene (the HUD
   Canvas owner is a natural home; it needs no mesh or collider).
2. Drag the scene's `GameManager` (which owns the `Simulation`)
   into its `Simulation` field — the same Simulation that
   `FpsInputAdapter` and `HudDisplay` already use. Do not create
   a second Simulation for the Orbital view.
3. Add a Text field to the HUD Canvas near the objective readout
   and drag it into `SquadText`.
4. Press Play, then press **O**: a Vanguard squad deploys under
   HoldGround at the player's current position and holds there.
   The display line appears; pressing O again deploys another.

## The provisional O key

O is a provisional command binding, not the final Orbital UI.
It deploys at the player's feet, so the squad holds immediately
and fires at enemies in range 3. There is deliberately no rally
picker yet: picking a distant rally in the FPS view is a later
step. O is reversible — it will be replaced by a doctrine menu /
rally picker, and the binding can change without touching core.

## Deliberately NOT in this step

- No 3D squad markers, unit models, or Orbital camera. The view
  is a text list; a player can see squads exist, where they are,
  and whether they are moving or holding, but not as figures in
  the voxel scene yet.
- No doctrine menu, rally picker, deployment cost, or squad cap.
  O deploys unlimited squads for now, exactly as the headless
  command does — that is a known, documented gap, not a balance
  decision.
- No enemy AI targeting squads (unchanged from 0.9.0/0.10.0).
- No change to save/load: squads already persist (0.10.0), and
  the snapshot reads the same live list the saver reads.

## Unity verification checklist — to be done on Windows, Unity 6000

1. [ ] Project opens with no compile errors in
   `OrbitalSquadDisplay.cs` and the edited `FpsInputAdapter.cs`.
2. [ ] With no squads, the display reads
   `ORBITAL: NO SQUADS (press O to deploy)`.
3. [ ] Press O: one line appears, `3 MEMBERS 90 HP`, `HOLDING`,
   at the player's position.
4. [ ] Walk an enemy into range 3 of the held squad: the squad
   fires (enemy loses 30 HP per tick at full strength) and the
   line's HP/members update as the squad takes adjacent damage.
5. [ ] Press O twice more: three lines, in deploy order.
6. [ ] Save, close, reopen, load: the same squads return (save
   persistence is already headless-tested in 0.10.0; this checks
   the Unity load path shows them — note the Slice 1 limitation
   in `GameManager.LoadFromJson`, which logs a load but does not
   yet replace `Simulation.World` in place; if that blocks this
   step, record it as the next follow-up instead of claiming it).

Record the result of each step, with Unity version and GPU, in
DECISIONS.md.
