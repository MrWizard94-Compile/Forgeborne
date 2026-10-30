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
doctrine, kind, and — since Slice 6 Step 2 (0.29.0) — a display
activity word and, for a hunting squad, its prey cell. It is a
copy, not a live reference: taking a snapshot, then advancing
the world, leaves the old snapshot at its pre-tick position and
its pre-tick activity and prey. That is deliberate — rendering
must never mutate core state, and a frame's display must not
change mid-frame because a tick ran.

## The activity word (Slice 6 Step 2, 0.29.0)

The old display word was rally-relative (`MOVING` /
`HOLDING`), and it lied about Advance: a hunting squad read
`MOVING` while standing adjacent to its prey firing, or
`HOLDING` at a rally it was ignoring. The word is now
`SquadActivity`, derived in core, not in the adapter:

- HoldGround: `MARCHING` while away from its rally,
  `HOLDING` at it.
- Advance: `HUNTING` whenever a living enemy exists —
  moving toward it or standing adjacent firing — with
  `HuntingTargetX/Y/Z` set to the nearest living enemy
  (Manhattan, first in list order on ties, no range limit,
  re-derived on every snapshot from the same rule movement
  and fire already use, so the three can never disagree
  about the prey); `IDLE` when no living enemy exists,
  standing still and ignoring its rally, which is the
  documented Advance behaviour, not a bug to hide.

Activity and prey are derived, not saved: no save field
exists for either, and a loaded Advance squad reads
`HUNTING` at the same prey again because the world it loads
still contains that prey. `AtRally` / `IsMoving` remain on
the snapshot as position facts, but they are no longer the
display word, and no second word is kept alongside that
could contradict the first.

## 3D markers (Slice 6 Step 2, 0.29.0)

`ForgeborneUnity/Assets/Scripts/SquadMarkerDisplay.cs` is a
thin adapter only: it renders the same snapshot as scene
markers, one per live squad, one per squad rally, and one
pending-rally marker while the picker is open — no more, so
the scene gains no marker layer the HUD discipline in
`docs/art-direction.md` would call clutter. Forms are
original primitives built in the adapter, colours follow the
art direction (neon cyan cubes for squads, orange flat cubes
for rallies, an orange sphere for a pending rally so a pick
in progress cannot be mistaken for a deployed rally), and
markers are pooled (at most 3 + 3 + 1) with their colliders
removed so they never block the picker raycast or movement.
Markers do NOT show through walls, by decision: they are
ordinary opaque primitives, so the voxel mesh occludes them
honestly by depth. If Windows testing proves that makes
them useless at FPS distance, that is a later step's
evidence to revisit, not a silent shader change.

Marker distinction (Slice 13 Step 2, 0.58.0): the shared cyan
cube above is superseded — Vanguard keeps the cyan cube 0.6
units, Recon renders as a magenta capsule 0.5 wide x 0.8 tall,
and Heavy as a light-grey cylinder 0.8 wide x 0.7 tall, so
kind is read from shape first and colour second, never from
colour alone. Rally and pending markers are unchanged, and
the pooled budget is unchanged (at most 3 squad slots, each
holding one child primitive for its current kind, swapped
only when a slot is reused by a different kind). No squad
rule changes with the markers, and the text display still
reads kind from the same snapshot. This distinction is
adapter-only Unity code: no headless test proves it reads
in the scene, and the Windows checklist for it is
playtest-gate item 25.

`ForgeborneUnity/Assets/Scripts/OrbitalSquadDisplay.cs` is a thin
adapter only: it renders the snapshot as one text line per squad
(`VANGUARD HOLDGROUND - 3 MEMBERS 90 HP 30 DMG - HOLDING AT
(16,5,16) -> RALLY (16,5,16)`, or for a hunting squad
`VANGUARD ADVANCE - 3 MEMBERS 90 HP 30 DMG - HUNTING PREY
(20,5,16) AT (16,5,16) -> RALLY (24,5,16)`), or
`ORBITAL: NO SQUADS (press O to pick a rally)` when none are
deployed, plus a picker prompt line while the picker is open
(see `docs/orbital-command.md`). It contains no squad logic:
the activity word is rendered verbatim from the snapshot.

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
5. Add `SquadMarkerDisplay` to a GameObject in the scene
   (any empty object; it needs no mesh or collider of its
   own), drag the scene's `GameManager` Simulation into its
   `Simulation` field — the same Simulation, not a second
   one — and drag an empty GameObject into `MarkerRoot`
   (or leave it empty and markers parent to the scene root).
   Markers appear from the same snapshot the text lines
   render; no extra wiring per squad is needed.

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

- No unit models or Orbital camera. Markers (0.29.0) are
  scene primitives, not figures: a player can see squads
  and rallies in the scene, but squads are not rendered as
  characters, and there is no second camera.
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
7. [ ] Markers, added 0.29.0 (adapter code written, NOT yet
   compiled or run in Unity on Windows; the core activity
   word and prey derivation are headless-tested, the adapter
   rendering is NOT): deploy a HoldGround squad — a cyan
   cube appears at the squad and an orange flat marker at
   its rally, and the text line reads MARCHING then HOLDING
   as it arrives; deploy an Advance squad with a living
   enemy — the text reads HUNTING with the prey cell, and
   it keeps reading HUNTING while the squad stands adjacent
   firing (the old word would have read MOVING or HOLDING
   by rally); kill the last enemy — the Advance line reads
   IDLE and the squad stands still. Markers move with
   their squads, the pending-rally sphere appears only
   while picking with a rally set, and markers disappear
   when their squad dies or is removed.
8. [ ] Marker occlusion, added 0.29.0: walk behind a wall so
   a squad marker is on the far side — the marker is hidden
   by the wall (no through-wall rendering), and reappears
   when line of sight returns. If markers prove unreadable
   at FPS distance because of this, record that as evidence
   for a later step; do not silently change the material.
   Note, added 0.69.0 (Slice 16 Step 2): this item stands
   as the baseline occlusion observation. Playtest-gate
   item 32 is the re-observation under the Step 1 chunk
   material as occluder, plus combined tint-and-marker
   readability at FPS distance bands; it does not
   supersede this item. Re-observation under the Step 3
   dusk light is still owed by Step 3's own item.
9. [ ] Marker hygiene, added 0.29.0: markers never block
   the picker (aim through a marker cell and the rally
   still picks the voxel behind it) and never block player
   movement (walk through a marker cell unimpeded), and
   no marker remains after its squad is gone or the
   picker is cancelled.

Record the result of each step, with Unity version and GPU, in
DECISIONS.md.
