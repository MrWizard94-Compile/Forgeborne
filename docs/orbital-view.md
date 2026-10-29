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
to pick a rally)` when none are deployed, plus a picker prompt
line while the picker is open (see `docs/orbital-command.md`).
It contains no squad logic.

## Wiring (Unity 6000, Windows)

1. Add `OrbitalSquadDisplay` to a GameObject in the scene (the HUD
   Canvas owner is a natural home; it needs no mesh or collider).
2. Drag the scene's `GameManager` (which owns the `Simulation`)
   into its `Simulation` field — the same Simulation that
   `FpsInputAdapter` and `HudDisplay` already use. Do not create
   a second Simulation for the Orbital view.
3. Add a Text field to the HUD Canvas near the objective readout
   and drag it into `SquadText`.
4. Press Play, then use the picker (see
   `docs/orbital-command.md`): press **O**, aim the crosshair
   at a rally cell, press **Enter**. A Vanguard squad deploys
   under HoldGround, marches to the rally, and holds there.
   The display line appears; repeating the picker deploys
   another squad.

## The provisional O key — replaced in 0.15.0

O was a provisional command binding, not the final Orbital UI:
it deployed instantly at the player's feet, so the squad held
immediately and fired at enemies in range 3, with no rally
choice at all. That binding is gone. O now opens the doctrine
menu / rally picker (`docs/orbital-command.md`): aim, Enter to
deploy at the picked rally, Esc to cancel. The old instant
deploy survives only as the headless `DeployOrbitalSquad`
path, used by tests and the demo — not by the Unity adapter.

## Deliberately NOT in this step

- No 3D squad markers, unit models, or Orbital camera. The view
  is a text list; a player can see squads exist, where they are,
  and whether they are moving or holding, but not as figures in
  the voxel scene yet.
- No deployment cost or squad cap (decided in 0.15.0, deferred
  to a Slice 3 economy step — see `docs/orbital-command.md`).
  The picker deploys unlimited squads for now, exactly as the
  headless command does — that is a known, documented gap, not
  a balance decision.
- No enemy AI targeting squads (unchanged from 0.9.0/0.10.0).
- No change to save/load: squads already persist (0.10.0), and
  the snapshot reads the same live list the saver reads.

## Unity verification checklist — to be done on Windows, Unity 6000

1. [ ] Project opens with no compile errors in
   `OrbitalSquadDisplay.cs` and the edited `FpsInputAdapter.cs`.
2. [ ] With no squads, the display reads
   `ORBITAL: NO SQUADS (press O to pick a rally)`.
3. [ ] Press O, aim at a rally cell, Enter: one line appears,
   `3 MEMBERS 90 HP`, `MOVING` then `HOLDING` at the rally.
4. [ ] Walk an enemy into range 3 of the held squad: the squad
   fires (enemy loses 30 HP per tick at full strength) and the
   line's HP/members update as the squad takes adjacent damage.
5. [ ] Repeat the picker twice more: three lines, in deploy
   order.
6. [ ] Save, close, reopen, load: the same squads return (save
   persistence is already headless-tested in 0.10.0, and the
   in-place restore is headless-tested in 0.14.0 via
   `Simulation.LoadFromJson` + `GameManager.LoadFromJson`; this
   step checks the Unity load path shows them, including a mesh
   refresh from the restored chunk).

Record the result of each step, with Unity version and GPU, in
DECISIONS.md.
