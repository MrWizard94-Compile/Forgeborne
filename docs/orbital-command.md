# Forgeborne — Orbital doctrine menu / rally picker

**Status: CORE HEADLESS-TESTED. UNITY ADAPTER CODE WRITTEN,
NOT YET COMPILED OR RUN IN UNITY ON WINDOWS.**
Do not claim the picker is verified in Unity until the
checklist below is completed there and recorded in DECISIONS.md.
This step replaces the provisional O key from 0.11.0, which
deployed a squad instantly at the player's feet. That binding
is gone: in play, a deploy is now always a picked rally.

## What the picker is

`OrbitalCommand` in `src/Forgeborne.Core/Orbital.cs` is the
menu state, in pure C# core — no rendering, no second copy of
squad rules. It holds the selected doctrine (HoldGround) and
squad kind (Vanguard), the only pair that exists so far, and
a pending rally while picking. Flow:

1. `BeginPick()` opens the picker and clears any stale pending
   rally, so Enter cannot deploy yesterday's point.
2. `SetRally(x, y, z)` updates the pending rally while picking.
   Only bounds are checked, matching `WorldZone.DeploySquad`:
   a rally inside a solid block is accepted and the squad then
   holds short of it, so a bad rally is visible, not silent.
   SetRally outside picking, or out of bounds, fails closed.
3. `Confirm(world)` deploys one squad with the selected
   doctrine/kind at the pending rally, then closes the picker
   and clears the rally. Confirm with no pick or no rally
   deploys nothing; a failed deploy keeps the picker open for
   a re-pick instead of losing the menu.
4. `CancelPick()` abandons the pick: no squad, rally dropped.
5. `SelectDoctrine` / `SelectKind` reject unknown values and
   leave the selection unchanged.

`Simulation` exposes the same operations as
`BeginOrbitalPick`, `SetOrbitalRally`, `CancelOrbitalPick`,
and `ConfirmOrbitalDeploy`. The direct `DeployOrbitalSquad`
path remains for headless use and the demo; the Unity adapter
no longer uses it.

## Unity bindings (thin adapters only)

`ForgeborneUnity/Assets/Scripts/FpsInputAdapter.cs`:

- **O** — open the picker, or cancel it if already picking.
- **While picking** — a centre-screen raycast from the camera
  (forward, 30 m) updates the pending rally to the voxel cell
  hit. `VoxelMeshRenderer` now carries a `MeshCollider` sharing
  the built mesh, so the raycast actually hits the chunk; without
  it the ray would pass through the rendered voxels and never
  set a rally. The camera only yaws in Slice 1 (no pitch yet),
  so the ray runs horizontally from eye height and hits walls
  and vertical faces — a rally on flat ground ahead is picked
  by aiming at the face beyond it, not by looking down. Pitch
  aiming is a later FPS step, not claimed here.
- **Enter** — deploy the selected squad at the pending rally.
- **Esc** — cancel the pick, no squad.
- **1** — re-select HoldGround / Vanguard (the only pair).
- While picking, the mouse does not fire: Enter is the confirm,
  so a click cannot shoot while the player is aiming a rally.

`OrbitalSquadDisplay.cs` shows a picker prompt line first while
picking (`ORBITAL: PICKING RALLY - VANGUARD HOLDGROUND ->
(x,y,z) - ENTER TO DEPLOY, ESC TO CANCEL`, or `AIM AT A TARGET
CELL` when no rally is set yet), then the squad list unchanged.
With no squads and no pick in progress it reads
`ORBITAL: NO SQUADS (press O to pick a rally)`.

## Deployment cost / cap — decided, deferred

This step decides cost/cap explicitly: **none yet, and that is
a decision, not an oversight left open.** Inventory holds
blocks, not a currency, so any squad price invented now would
be a balance guess with no economy to price against, and a cap
with no economy behind it would be an arbitrary limit. Cost and
cap belong to a Slice 3 economy step, where they are designed
with the resource system instead of bolted onto the picker.
Pressing through the picker repeatedly still deploys repeatedly,
exactly as the headless command does — a known, documented gap.

## Deliberately NOT in this step

- No extra doctrines or squad types (still HoldGround only,
  Vanguard only); the selection API exists so adding them does
  not reshape the picker.
- No 3D squad markers, rally marker in the scene, or Orbital
  camera — the rally is shown as text coordinates only.
- No deployment cost or squad cap (see above).
- No enemy AI targeting squads (unchanged from 0.9.0/0.10.0).
- No Slice 3 scope: with the picker done, Slice 2's three
  review follow-ups (demo scenario, in-place restore, picker)
  are all complete, but Slice 3 is scoped by a later step, not
  by this one.

## Unity verification checklist — to be done on Windows, Unity 6000

1. [ ] Project opens with no compile errors in the edited
   `FpsInputAdapter.cs`, `OrbitalSquadDisplay.cs`, and
   `VoxelMeshRenderer.cs` (including its new MeshCollider).
2. [ ] With no squads, the display reads
   `ORBITAL: NO SQUADS (press O to pick a rally)`.
3. [ ] Press O: the picking prompt appears; Esc cancels it
   with no squad deployed.
4. [ ] Press O, aim the crosshair at a voxel face, Enter: one
   squad deploys, marches to the rally, and the prompt clears.
5. [ ] Press O, aim nowhere hittable (open sky), Enter: no
   squad deploys and the picker stays open.
6. [ ] While picking, a mouse click does not fire the weapon.
7. [ ] Save, close, reopen, load: a picked squad returns
   (persistence is headless-tested in 0.10.0, restore in
   0.14.0; this step checks the Unity path shows it).

Record the result of each step, with Unity version and GPU, in
DECISIONS.md.
